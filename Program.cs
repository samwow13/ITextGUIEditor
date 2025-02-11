using System;
using System.Windows.Forms;
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
                Application.Run(new MainForm(selector.SelectedType));
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ShowSelectorAndRunMainForm();
        }
    }
}
