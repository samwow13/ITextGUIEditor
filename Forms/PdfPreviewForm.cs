using System;
using System.Drawing;
using System.Windows.Forms;

namespace iTextDesignerWithGUI.Forms
{
    /// <summary>
    /// Form for displaying PDF preview positioned below the main form
    /// </summary>
    public class PdfPreviewForm : Form
    {
        private readonly MainForm _parentForm;
        private readonly string _pdfPath;
        private readonly WebBrowser _webBrowser;

        public PdfPreviewForm(MainForm parentForm, string pdfPath)
        {
            _parentForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm));
            _pdfPath = pdfPath ?? throw new ArgumentNullException(nameof(pdfPath));
            
            // Initialize the web browser
            _webBrowser = new WebBrowser
            {
                Dock = DockStyle.Fill,
                ScriptErrorsSuppressed = true
            };

            InitializeComponent();
            PositionFormBelowParent();
            LoadPdfInBrowser();
        }

        private void InitializeComponent()
        {
            // Basic form settings
            this.Text = "PDF Preview";
            this.StartPosition = FormStartPosition.Manual;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.ShowInTaskbar = false;

            // Add browser to form
            this.Controls.Add(_webBrowser);

            // Handle form closing to clean up resources
            this.FormClosing += (s, e) => {
                _webBrowser.Dispose();
            };
        }

        private void PositionFormBelowParent()
        {
            // Match parent form width
            this.Width = _parentForm.Width;
            
            // Make height 2.5 times taller than parent
            this.Height = (int)(_parentForm.Height * 2.5);
            
            // Position directly below parent form
            Point parentLocation = _parentForm.Location;
            this.Location = new Point(
                parentLocation.X,
                parentLocation.Y + _parentForm.Height
            );

            // Handle parent form moving
            _parentForm.LocationChanged += (s, e) => {
                this.Location = new Point(
                    _parentForm.Location.X,
                    _parentForm.Location.Y + _parentForm.Height
                );
            };

            // Handle parent form resizing
            _parentForm.SizeChanged += (s, e) => {
                this.Width = _parentForm.Width;
            };
        }

        private void LoadPdfInBrowser()
        {
            try
            {
                // Navigate to the PDF file
                _webBrowser.Navigate(_pdfPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading PDF: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
