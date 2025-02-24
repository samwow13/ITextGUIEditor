using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;

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
        private readonly System.Windows.Forms.Timer _cooldownTimer;
        private bool _isDisposed;
        private const int COOLDOWN_PERIOD = 2000; // 2 second cooldown
        private bool _isInCooldown;

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

        private void OnTemplateFileChanged(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine($"File change detected: {e.ChangeType} - {e.FullPath}");
            
            if (_isInCooldown)
            {
                Debug.WriteLine("Change ignored - in cooldown period");
                return;
            }

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
