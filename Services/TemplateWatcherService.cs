using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace iTextDesignerWithGUI.Services
{
    /// <summary>
    /// Service that watches for changes in the Templates directory and triggers reload functionality
    /// </summary>
    public class TemplateWatcherService : IDisposable
    {
        private readonly List<FileSystemWatcher> _watchers;
        private readonly Action _onTemplateChanged;
        private readonly Control _uiControl;
        private readonly ProjectDirectoryService _directoryService;
        private readonly System.Windows.Forms.Timer _cooldownTimer;
        private bool _isDisposed;
        private const int COOLDOWN_PERIOD = 5000; // 5 second cooldown
        private bool _isInCooldown;
        private const int PRE_PROCESS_DELAY = 1000; // 1 second delay before processing changes

        /// <summary>
        /// Initializes a new instance of the TemplateWatcherService class.
        /// </summary>
        /// <param name="onTemplateChanged">Action to execute when a template file changes</param>
        /// <param name="uiControl">Control to use for invoking UI operations</param>
        public TemplateWatcherService(Action onTemplateChanged, Control uiControl)
        {
            _onTemplateChanged = onTemplateChanged ?? throw new ArgumentNullException(nameof(onTemplateChanged));
            _uiControl = uiControl ?? throw new ArgumentNullException(nameof(uiControl));
            _directoryService = new ProjectDirectoryService();
            _watchers = new List<FileSystemWatcher>();
            _isInCooldown = false;

            string templatesPath = _directoryService.GetDirectory("Templates");
            Debug.WriteLine($"Initializing TemplateWatcherService for path: {templatesPath}");

            // Initialize the cooldown timer
            _cooldownTimer = new System.Windows.Forms.Timer();
            _cooldownTimer.Interval = COOLDOWN_PERIOD;
            _cooldownTimer.Enabled = false;
            _cooldownTimer.Tick += OnCooldownComplete;

            // Create watchers for different file types
            string[] fileTypes = new[] { "*.html", "*.cshtml", "*.css" };
            foreach (var fileType in fileTypes)
            {
                var watcher = new FileSystemWatcher
                {
                    Path = templatesPath,
                    NotifyFilter = NotifyFilters.LastWrite 
                        | NotifyFilters.FileName 
                        | NotifyFilters.DirectoryName 
                        | NotifyFilters.Size 
                        | NotifyFilters.LastAccess
                        | NotifyFilters.CreationTime
                        | NotifyFilters.Attributes,
                    Filter = fileType,
                    EnableRaisingEvents = false, // Start disabled
                    IncludeSubdirectories = true // Enable monitoring of subdirectories
                };

                // Attach event handlers
                watcher.Changed += OnTemplateFileChanged;
                watcher.Created += OnTemplateFileChanged;
                watcher.Deleted += OnTemplateFileChanged;
                watcher.Renamed += OnTemplateFileRenamed;
                watcher.Error += OnWatcherError;

                _watchers.Add(watcher);
            }
        }

        /// <summary>
        /// Initializes a new instance of the TemplateWatcherService class.
        /// </summary>
        /// <param name="templatesPath">Path to the Templates directory</param>
        /// <param name="onTemplateChanged">Action to execute when a template file changes</param>
        /// <param name="uiControl">Control to use for invoking UI operations</param>
        public TemplateWatcherService(string templatesPath, Action onTemplateChanged, Control uiControl)
        {
            if (string.IsNullOrEmpty(templatesPath))
                throw new ArgumentNullException(nameof(templatesPath));

            _onTemplateChanged = onTemplateChanged ?? throw new ArgumentNullException(nameof(onTemplateChanged));
            _uiControl = uiControl ?? throw new ArgumentNullException(nameof(uiControl));
            _watchers = new List<FileSystemWatcher>();
            _isInCooldown = false;

            Debug.WriteLine($"Initializing TemplateWatcherService for path: {templatesPath}");

            // Initialize the cooldown timer
            _cooldownTimer = new System.Windows.Forms.Timer();
            _cooldownTimer.Interval = COOLDOWN_PERIOD;
            _cooldownTimer.Enabled = false;
            _cooldownTimer.Tick += OnCooldownComplete;

            // Create watchers for different file types
            string[] fileTypes = new[] { "*.html", "*.cshtml", "*.css" };
            foreach (var fileType in fileTypes)
            {
                var watcher = new FileSystemWatcher
                {
                    Path = templatesPath,
                    NotifyFilter = NotifyFilters.LastWrite 
                        | NotifyFilters.FileName 
                        | NotifyFilters.DirectoryName 
                        | NotifyFilters.Size 
                        | NotifyFilters.LastAccess
                        | NotifyFilters.CreationTime
                        | NotifyFilters.Attributes,
                    Filter = fileType,
                    EnableRaisingEvents = false, // Start disabled
                    IncludeSubdirectories = true // Enable monitoring of subdirectories
                };

                // Attach event handlers
                watcher.Changed += OnTemplateFileChanged;
                watcher.Created += OnTemplateFileChanged;
                watcher.Deleted += OnTemplateFileChanged;
                watcher.Renamed += OnTemplateFileRenamed;
                watcher.Error += OnWatcherError;

                _watchers.Add(watcher);
            }
        }

        public void StartWatching()
        {
            Debug.WriteLine("Starting template watchers");
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = true;
            }
        }

        public void StopWatching()
        {
            Debug.WriteLine("Stopping template watchers");
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
            }
        }

        private async void OnTemplateFileChanged(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine($"File change detected: {e.ChangeType} - {e.FullPath}");
            
            if (_isInCooldown)
            {
                Debug.WriteLine("Change ignored - in cooldown period");
                return;
            }

            // Add a short delay to allow file operations to complete
            await Task.Delay(PRE_PROCESS_DELAY);
            
            if (_uiControl.InvokeRequired)
            {
                _uiControl.BeginInvoke(new Action(() => HandleFileChange()));
            }
            else
            {
                HandleFileChange();
            }
        }

        private void HandleFileChange()
        {
            // Trigger the change immediately
            _onTemplateChanged?.Invoke();

            // Enter cooldown period
            _isInCooldown = true;
            _cooldownTimer.Start();
        }

        private void OnTemplateFileRenamed(object sender, RenamedEventArgs e)
        {
            Debug.WriteLine($"File renamed: {e.OldFullPath} -> {e.FullPath}");
            OnTemplateFileChanged(sender, e);
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine($"Watcher error: {e.GetException()}");
            MessageBox.Show($"Error watching templates: {e.GetException().Message}", "Template Watcher Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void OnCooldownComplete(object sender, EventArgs e)
        {
            _cooldownTimer.Stop();
            _isInCooldown = false;
            Debug.WriteLine("Cooldown period complete - resuming file watching");
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            Debug.WriteLine("Disposing TemplateWatcherService");
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            _cooldownTimer.Dispose();
            _isDisposed = true;
        }
    }
}
