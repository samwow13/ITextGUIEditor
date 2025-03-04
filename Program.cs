using System;
using System.Windows.Forms;
using System.Diagnostics;
using iTextDesignerWithGUI.Forms;
using iTextDesignerWithGUI.Models;
using iTextDesignerWithGUI.Utils;

namespace iTextDesignerWithGUI
{
    static class Program
    {
        // Static reference to the singleton manager to keep it alive for the duration of the app
        private static SingleInstanceManager _singleInstanceManager;
        
        public static void ShowSelectorAndRunMainForm()
        {
            var selector = new AssessmentTypeSelector();
            if (selector.ShowDialog() == DialogResult.OK && !selector.WasCancelled)
            {
                Application.Run(new MainForm(selector.SelectedTypeWrapper));
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Trace.Listeners.Add(new ConsoleTraceListener());
            Trace.Listeners.Add(new TextWriterTraceListener("debug.log"));
            Trace.AutoFlush = true;
            
            // Initialize the single instance manager
            _singleInstanceManager = new SingleInstanceManager();
            
            // Only allow one instance of the application to run
            if (!_singleInstanceManager.IsFirstInstance())
            {
                // If this is not the first instance, try to activate the existing instance
                if (_singleInstanceManager.TryActivateFirstInstance())
                {
                    Debug.WriteLine("Another instance is already running. Activating it.");
                    return; // Exit this instance
                }
            }
            
            // Continue with normal application startup
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += (sender, e) =>
            {
                Debug.WriteLine($"Unhandled thread exception: {e.Exception.Message}");
                Debug.WriteLine($"Stack trace: {e.Exception.StackTrace}");
            };
            
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Debug.WriteLine($"Unhandled AppDomain exception: {e.ExceptionObject}");
            };
            
            try
            {
                ShowSelectorAndRunMainForm();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fatal error in application: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"A fatal error has occurred: {ex.Message}\n\nPlease check the debug.log file for details.",
                    "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Clean up the instance manager
                _singleInstanceManager?.Dispose();
            }
        }
    }
}
