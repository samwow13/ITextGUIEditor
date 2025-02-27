using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace iTextDesignerWithGUI.Utilities
{
    /// <summary>
    /// Extension methods for Windows Forms controls
    /// </summary>
    public static class ControlExtensions
    {
        /// <summary>
        /// Sets the style of a ProgressBar control
        /// </summary>
        /// <param name="progressBar">The progress bar to modify</param>
        /// <param name="style">The style to set</param>
        public static void SetStyle(this ProgressBar progressBar, ProgressBarStyle style)
        {
            if (progressBar == null) throw new ArgumentNullException(nameof(progressBar));
            
            // Send the WM_STYLECHANGED message to set the progress bar style
            progressBar.Style = style;
            
            // Force the progress bar to redraw
            progressBar.Invalidate();
        }
    }
}
