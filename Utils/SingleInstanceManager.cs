using System;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

namespace iTextDesignerWithGUI.Utils
{
    /// <summary>
    /// Manages application instance uniqueness to prevent multiple copies running at the same time
    /// </summary>
    public class SingleInstanceManager : IDisposable
    {
        private Mutex _mutex;
        private bool _isFirstInstance;
        private bool _isDisposed;
        private static readonly string MutexName = "iTextDesignerWithGUI_SingleInstanceMutex";
        
        /// <summary>
        /// Creates a new instance of the SingleInstanceManager
        /// </summary>
        public SingleInstanceManager()
        {
            try
            {
                // Try to create a named mutex
                _mutex = new Mutex(true, MutexName, out _isFirstInstance);
                Debug.WriteLine($"SingleInstanceManager: Is first instance: {_isFirstInstance}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SingleInstanceManager: {ex.Message}");
                // Default to allowing this instance if we encounter an error
                _isFirstInstance = true;
            }
        }
        
        /// <summary>
        /// Checks if this is the first/only instance of the application
        /// </summary>
        /// <returns>True if this is the first instance, false otherwise</returns>
        public bool IsFirstInstance()
        {
            return _isFirstInstance;
        }
        
        /// <summary>
        /// Tries to activate the first instance of the application if another instance is already running
        /// </summary>
        /// <returns>True if another instance was found and activated, false otherwise</returns>
        public bool TryActivateFirstInstance()
        {
            if (_isFirstInstance)
                return false;
                
            try
            {
                // Get all processes with the same name as the current process
                Process currentProcess = Process.GetCurrentProcess();
                Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);
                
                foreach (Process process in processes)
                {
                    // Skip the current process
                    if (process.Id == currentProcess.Id)
                        continue;
                        
                    // Try to activate the window of the other process
                    NativeMethods.SetForegroundWindow(process.MainWindowHandle);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error trying to activate first instance: {ex.Message}");
            }
            
            return false;
        }
        
        /// <summary>
        /// Releases resources used by the SingleInstanceManager
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Releases resources used by the SingleInstanceManager
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Release managed resources
                    if (_mutex != null)
                    {
                        if (_isFirstInstance)
                        {
                            try
                            {
                                _mutex.ReleaseMutex();
                            }
                            catch (ApplicationException)
                            {
                                // Ignore - mutex might not be owned by this thread
                            }
                        }
                        
                        _mutex.Dispose();
                        _mutex = null;
                    }
                }
                
                _isDisposed = true;
            }
        }
        
        /// <summary>
        /// Finalizer for SingleInstanceManager
        /// </summary>
        ~SingleInstanceManager()
        {
            Dispose(false);
        }
        
        /// <summary>
        /// Native methods for window manipulation
        /// </summary>
        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetForegroundWindow(IntPtr hWnd);
        }
    }
}
