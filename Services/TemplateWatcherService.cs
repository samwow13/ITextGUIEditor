using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace iTextDesignerWithGUI.Services
{
    /// <summary>
    /// Service that watches for changes in the Templates directory and triggers reload functionality
    /// </summary>
    public class TemplateWatcherService : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly Action _onTemplateChanged;
        private readonly Control _uiControl;
        private readonly System.Windows.Forms.Timer _debounceTimer;
        private bool _isDisposed;
        private const int DEBOUNCE_INTERVAL = 500; // Half second debounce

        public TemplateWatcherService(string templatesPath, Action onTemplateChanged, Control uiControl)
        {
            if (string.IsNullOrEmpty(templatesPath))
                throw new ArgumentNullException(nameof(templatesPath));

            _onTemplateChanged = onTemplateChanged ?? throw new ArgumentNullException(nameof(onTemplateChanged));
            _uiControl = uiControl ?? throw new ArgumentNullException(nameof(uiControl));

            Debug.WriteLine($"Initializing TemplateWatcherService for path: {templatesPath}");

            // Initialize the debounce timer
            _debounceTimer = new System.Windows.Forms.Timer();
            _debounceTimer.Interval = DEBOUNCE_INTERVAL;
            _debounceTimer.Enabled = false; // Start disabled
            _debounceTimer.Tick += OnDebounceTimerTick;

            // Initialize the FileSystemWatcher with more comprehensive notify filters
            _watcher = new FileSystemWatcher
            {
                Path = templatesPath,
                NotifyFilter = NotifyFilters.LastWrite 
                    | NotifyFilters.FileName 
                    | NotifyFilters.DirectoryName 
                    | NotifyFilters.Size 
                    | NotifyFilters.LastAccess
                    | NotifyFilters.CreationTime
                    | NotifyFilters.Attributes,
                Filter = "*.html", // Watch only HTML files
                EnableRaisingEvents = false // Start disabled
            };

            // Attach event handlers
            _watcher.Changed += OnTemplateFileChanged;
            _watcher.Created += OnTemplateFileChanged;
            _watcher.Deleted += OnTemplateFileChanged;
            _watcher.Renamed += OnTemplateFileRenamed;

            // Enable error handling
            _watcher.Error += OnWatcherError;
        }

        public void StartWatching()
        {
            Debug.WriteLine("Starting template watcher");
            _watcher.EnableRaisingEvents = true;
        }

        public void StopWatching()
        {
            Debug.WriteLine("Stopping template watcher");
            _watcher.EnableRaisingEvents = false;
        }

        private void OnTemplateFileChanged(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine($"File change detected: {e.ChangeType} - {e.FullPath}");
            
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
            Debug.WriteLine("Handling file change - Resetting timer");
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void OnTemplateFileRenamed(object sender, RenamedEventArgs e)
        {
            Debug.WriteLine($"File renamed: {e.OldFullPath} -> {e.FullPath}");
            OnTemplateFileChanged(sender, e);
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine($"FileSystemWatcher error: {e.GetException()}");
        }

        private void OnDebounceTimerTick(object sender, EventArgs e)
        {
            if (_isDisposed) return;

            Debug.WriteLine("Timer elapsed - Triggering template reload");
            _debounceTimer.Stop();
            _onTemplateChanged.Invoke();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Debug.WriteLine("Disposing TemplateWatcherService");
                    StopWatching();
                    _watcher.Changed -= OnTemplateFileChanged;
                    _watcher.Created -= OnTemplateFileChanged;
                    _watcher.Deleted -= OnTemplateFileChanged;
                    _watcher.Renamed -= OnTemplateFileRenamed;
                    _watcher.Error -= OnWatcherError;
                    _watcher.Dispose();
                    _debounceTimer.Tick -= OnDebounceTimerTick;
                    _debounceTimer.Dispose();
                }
                _isDisposed = true;
            }
        }
    }
}
