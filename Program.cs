using System;
using System.Windows.Forms;
using OralCareReference.Forms;

namespace OralCareReference
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var selector = new AssessmentTypeSelector();
            selector.ShowDialog();

            if (!selector.WasCancelled)
            {
                Application.Run(new MainForm(selector.SelectedType));
            }
        }
    }
}
