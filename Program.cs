using System;
using System.Windows.Forms;
using System.Diagnostics;
using iTextDesignerWithGUI.Forms;
using iTextDesignerWithGUI.Models;

namespace iTextDesignerWithGUI
{
    static class Program
    {
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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ShowSelectorAndRunMainForm();
        }
    }
}
