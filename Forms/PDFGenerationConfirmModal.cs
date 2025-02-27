using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using iTextDesignerWithGUI.Services;

namespace iTextDesignerWithGUI.Forms
{
    /// <summary>
    /// Modal dialog to confirm PDF generation with template information
    /// </summary>
    public class PDFGenerationConfirmModal : Form
    {
        private Label pdfNameLabel;
        private Label templateDirLabel;
        private Button confirmButton;
        private Button cancelButton;

        public string PDFName { get; private set; }
        public string TemplatePath { get; private set; }
        
        /// <summary>
        /// Constructor for the PDF generation confirmation modal
        /// </summary>
        /// <param name="pdfName">The name of the PDF to be generated</param>
        /// <param name="templatePath">The path to the template directory</param>
        public PDFGenerationConfirmModal(string pdfName, string templatePath)
        {
            PDFName = pdfName;
            TemplatePath = templatePath;
            InitializeComponent();
            
            // Add event handler for confirm button
            confirmButton.Click += ConfirmButton_Click;
        }

        private void InitializeComponent()
        {
            // Form settings
            this.Text = "Confirm PDF Generation";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(600, 320);
            this.Padding = new Padding(20);
            this.BackColor = Color.White;

            // Button panel
            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                ColumnCount = 2,
                RowCount = 1,
                Height = 60,
                Margin = new Padding(0),
                Padding = new Padding(10, 10, 10, 10),
                BackColor = SystemColors.Control
            };
            
            // Configure button panel columns
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // Button styles
            var buttonSize = new Size(120, 40);
            var buttonFont = new Font("Segoe UI", 10F, FontStyle.Regular);
            var buttonPadding = new Padding(10, 5, 10, 5);

            // Cancel button (left side)
            cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Font = buttonFont,
                Padding = buttonPadding,
                FlatStyle = FlatStyle.Standard,
                Size = buttonSize,
                Anchor = AnchorStyles.Right,
                Height = buttonSize.Height
            };
            
            // Generate button (right side)
            confirmButton = new Button
            {
                Text = "Generate Files",
                DialogResult = DialogResult.OK,
                Font = buttonFont,
                Padding = buttonPadding,
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Standard,
                Size = buttonSize,
                Anchor = AnchorStyles.Left,
                Height = buttonSize.Height
            };

            // Add buttons to panel with proper alignment
            var cancelContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            cancelContainer.Controls.Add(cancelButton);
            
            var confirmContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            confirmContainer.Controls.Add(confirmButton);
            
            buttonPanel.Controls.Add(cancelContainer, 0, 0);
            buttonPanel.Controls.Add(confirmContainer, 1, 0);

            // Create a content panel for everything except buttons
            var contentPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(20),
            };
            
            // Add rows
            for (int i = 0; i < 3; i++)
            {
                contentPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            
            // Icon and Message Panel (first row)
            var messagePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
                Margin = new Padding(0, 0, 0, 15)
            };
            
            // Configure columns for the message panel
            messagePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
            messagePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            
            // Add warning icon
            var iconLabel = new Label
            {
                Image = SystemIcons.Question.ToBitmap(),
                Size = new Size(40, 40),
                Margin = new Padding(0, 0, 10, 0)
            };
            messagePanel.Controls.Add(iconLabel, 0, 0);
            
            // Header label with confirmation message
            var headerLabel = new Label
            {
                Text = "Please verify your spelling before clicking generate files",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0),
                Dock = DockStyle.Fill
            };
            messagePanel.Controls.Add(headerLabel, 1, 0);
            
            contentPanel.Controls.Add(messagePanel, 0, 0);

            // Divider
            var divider = new Panel
            {
                Height = 1,
                Dock = DockStyle.Fill,
                BackColor = Color.LightGray,
                Margin = new Padding(0, 0, 0, 15)
            };
            contentPanel.Controls.Add(divider, 0, 1);

            // Information panel
            var infoPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 2,
                Margin = new Padding(0, 0, 0, 15)
            };

            // Configure info panel columns
            infoPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            infoPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

            // PDF Name Row
            var pdfNameHeaderLabel = new Label
            {
                Text = "PDF Name:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 5)
            };
            infoPanel.Controls.Add(pdfNameHeaderLabel, 0, 0);

            pdfNameLabel = new Label
            {
                Text = PDFName,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 5)
            };
            infoPanel.Controls.Add(pdfNameLabel, 1, 0);

            // Template Directory Row
            var templateDirHeaderLabel = new Label
            {
                Text = "Template Dir:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 5)
            };
            infoPanel.Controls.Add(templateDirHeaderLabel, 0, 1);

            templateDirLabel = new Label
            {
                Text = TemplatePath,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 5),
                MaximumSize = new Size(320, 0),
                AutoEllipsis = false
            };
            infoPanel.Controls.Add(templateDirLabel, 1, 1);

            contentPanel.Controls.Add(infoPanel, 0, 2);
            
            // Add both panels to the form
            this.Controls.Add(contentPanel);
            this.Controls.Add(buttonPanel);
            
            this.AcceptButton = confirmButton;
            this.CancelButton = cancelButton;
        }
        
        /// <summary>
        /// Event handler for the confirm button click
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The event arguments</param>
        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Extract the template type from the template path
                // Template path has format like "Templates\HealthAndWellness"
                string templateType = Path.GetFileName(TemplatePath);
                
                // Create the project directory service and CSHTML generation service
                var directoryService = new ProjectDirectoryService();
                var cshtmlService = new CshtmlGenerationService(directoryService);
                
                // Generate the CSHTML file
                bool success = cshtmlService.GenerateCshtmlFile(PDFName, templateType);
                
                if (success)
                {
                    MessageBox.Show($"Successfully generated CSHTML file: {PDFName}.cshtml", 
                        "File Generation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during file generation: {ex.Message}", 
                    "Generation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
