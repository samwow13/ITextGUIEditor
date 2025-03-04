using System.Diagnostics;

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
        private bool _isInitialized = false; // Flag to track if the service has been properly initialized
        private bool _isWatching = false; // Flag to track if watching is currently enabled

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
            _isDisposed = false;  // Explicitly initialize this field
            _isWatching = false;  // Initialize watching state to false

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
                    EnableRaisingEvents = false, // Always start disabled, will be controlled by StartWatching/StopWatching
                    IncludeSubdirectories = true // Enable monitoring of subdirectories
                };

                // Event handlers will be attached in StartWatching method
                _watchers.Add(watcher);
            }
            
            // Mark as initialized with watchers disabled by default
            _isInitialized = true;
            Debug.WriteLine("TemplateWatcherService initialized with watchers disabled by default");
        }


        /// <summary>
        /// Starts watching for template file changes
        /// </summary>
        public void StartWatching()
        {
            if (!_isInitialized)
            {
                Debug.WriteLine("Warning: Attempting to start template watchers before initialization");
                return;
            }
            
            if (_isWatching)
            {
                Debug.WriteLine("Template watchers already running, ignoring StartWatching call");
                return;
            }
            
            Debug.WriteLine("Starting template watchers");
            
            foreach (var watcher in _watchers)
            {
                // Attach event handlers
                watcher.Changed += OnTemplateFileChanged;
                watcher.Created += OnTemplateFileChanged;
                watcher.Deleted += OnTemplateFileChanged;
                watcher.Renamed += OnTemplateFileRenamed;
                watcher.Error += OnWatcherError;
                
                // Enable events after attaching handlers
                watcher.EnableRaisingEvents = true;
            }
            
            _isWatching = true;
            Debug.WriteLine("Template watchers started successfully");
        }

        /// <summary>
        /// Stops watching for template file changes
        /// </summary>
        public void StopWatching()
        {
            if (!_isInitialized)
            {
                Debug.WriteLine("Warning: Attempting to stop template watchers before initialization");
                return;
            }
            
            if (!_isWatching)
            {
                Debug.WriteLine("Template watchers already stopped, ignoring StopWatching call");
                return;
            }
            
            Debug.WriteLine("Stopping template watchers");
            
            foreach (var watcher in _watchers)
            {
                // First disable events
                watcher.EnableRaisingEvents = false;
                
                // Then detach event handlers
                watcher.Changed -= OnTemplateFileChanged;
                watcher.Created -= OnTemplateFileChanged;
                watcher.Deleted -= OnTemplateFileChanged;
                watcher.Renamed -= OnTemplateFileRenamed;
                watcher.Error -= OnWatcherError;
            }
            
            _isWatching = false;
            Debug.WriteLine("Template watchers stopped successfully");
        }

        /// <summary>
        /// Checks if the template watchers are currently enabled
        /// </summary>
        /// <returns>True if any watchers are enabled, false otherwise</returns>
        public bool IsWatching()
        {
            return _isWatching;
        }

        private async void OnTemplateFileChanged(object sender, FileSystemEventArgs e)
        {
            // First check if watching is enabled
            if (!_isWatching || !_watchers.Any(w => w.EnableRaisingEvents))
            {
                Debug.WriteLine($"File change detected but watching is disabled, ignoring: {e.ChangeType} - {e.FullPath}");
                return;
            }
            
            Debug.WriteLine($"File change detected: {e.ChangeType} - {e.FullPath}");
            
            if (_isInCooldown)
            {
                Debug.WriteLine("Change ignored - in cooldown period");
                return;
            }

            // Add a short delay to allow file operations to complete
            await Task.Delay(PRE_PROCESS_DELAY);
            
            // Double-check that watching is still enabled before proceeding
            if (!_isWatching || !_watchers.Any(w => w.EnableRaisingEvents))
            {
                Debug.WriteLine("Watching was disabled during delay, ignoring change");
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
            // Extra safety check - don't process if watching is disabled
            if (!_isWatching)
            {
                Debug.WriteLine("HandleFileChange called while watching is disabled, ignoring");
                return;
            }
            
            // Check if there are any open CustomErrorForms before proceeding
            var activeErrorForms = Application.OpenForms.OfType<Forms.CustomErrorForm>().ToList();
            if (activeErrorForms.Any())
            {
                Debug.WriteLine("Template change detected but error form(s) are open. Bringing to front instead of reloading.");
                // Bring the error form to front to remind the user to close it first
                foreach (var errorForm in activeErrorForms)
                {
                    errorForm.BringToFront();
                    errorForm.Flash(); // If we add a Flash method to the CustomErrorForm
                }
                return;
            }

            // Trigger the change immediately if no error forms are open
            _onTemplateChanged?.Invoke();

            // Enter cooldown period
            _isInCooldown = true;
            _cooldownTimer.Start();
        }

        private void OnTemplateFileRenamed(object sender, RenamedEventArgs e)
        {
            // First check if watching is enabled
            if (!_isWatching || !_watchers.Any(w => w.EnableRaisingEvents))
            {
                Debug.WriteLine($"File rename detected but watching is disabled, ignoring: {e.OldFullPath} -> {e.FullPath}");
                return;
            }
            
            Debug.WriteLine($"File renamed: {e.OldFullPath} -> {e.FullPath}");
            OnTemplateFileChanged(sender, e);
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            // Only show error if we're actually watching
            if (!_isWatching)
            {
                Debug.WriteLine($"Watcher error occurred while disabled, ignoring: {e.GetException()}");
                return;
            }
            
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
            
            // Make sure to stop watching and detach all event handlers
            StopWatching();
            
            foreach (var watcher in _watchers)
            {
                watcher.Dispose();
            }
            _cooldownTimer.Dispose();
            _isDisposed = true;
        }
    }
}
