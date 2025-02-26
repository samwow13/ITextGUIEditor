using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using iTextDesignerWithGUI.Models;

namespace iTextDesignerWithGUI.Forms
{
    /// <summary>
    /// Form for adding a new custom assessment type
    /// </summary>
    public class AddCustomAssessmentTypeForm : Form
    {
        private TextBox displayNameTextBox;
        private TextBox templateFileTextBox;
        private TextBox jsonDataPathTextBox;
        private Button browseTemplateButton;
        private Button browseJsonButton;

        public AddCustomAssessmentTypeForm()
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Form settings
            this.Text = "Add Custom Assessment Type";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new System.Drawing.Size(500, 350);
            this.Padding = new Padding(20);
            this.BackColor = Color.White;

            // Create main container
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 7,
                ColumnCount = 1,
                Padding = new Padding(10),
            };

            // Add rows
            for (int i = 0; i < 7; i++)
            {
                mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            // Display name section
            var displayNameLabel = new Label
            {
                Text = "Display Name:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            mainContainer.Controls.Add(displayNameLabel, 0, 0);

            displayNameTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Margin = new Padding(0, 0, 0, 15),
                Height = 25
            };
            mainContainer.Controls.Add(displayNameTextBox, 0, 1);

            // Template file section
            var templateFileLabel = new Label
            {
                Text = "Template File:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            mainContainer.Controls.Add(templateFileLabel, 0, 2);

            var templateFileContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 15)
            };
            templateFileContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));
            templateFileContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            templateFileTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Height = 25
            };
            templateFileContainer.Controls.Add(templateFileTextBox, 0, 0);

            browseTemplateButton = new Button
            {
                Text = "Browse...",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Margin = new Padding(5, 0, 0, 0)
            };
            browseTemplateButton.Click += BrowseTemplateButton_Click;
            templateFileContainer.Controls.Add(browseTemplateButton, 1, 0);

            mainContainer.Controls.Add(templateFileContainer, 0, 3);

            // JSON data path section
            var jsonDataPathLabel = new Label
            {
                Text = "JSON Data Path:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            mainContainer.Controls.Add(jsonDataPathLabel, 0, 4);

            var jsonDataPathContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 15)
            };
            jsonDataPathContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));
            jsonDataPathContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            jsonDataPathTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Height = 25
            };
            jsonDataPathContainer.Controls.Add(jsonDataPathTextBox, 0, 0);

            browseJsonButton = new Button
            {
                Text = "Browse...",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Margin = new Padding(5, 0, 0, 0)
            };
            browseJsonButton.Click += BrowseJsonButton_Click;
            jsonDataPathContainer.Controls.Add(browseJsonButton, 1, 0);

            mainContainer.Controls.Add(jsonDataPathContainer, 0, 5);

            // Button panel
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 40,
                Margin = new Padding(0, 10, 0, 0),
                AutoSize = true,
                Padding = new Padding(0)
            };

            // Button styles
            var buttonSize = new Size(100, 35);
            var buttonMargin = new Padding(5, 0, 5, 0);
            var buttonFont = new Font("Segoe UI", 9F, FontStyle.Regular);
            var buttonPadding = new Padding(10, 5, 10, 5);

            // Create buttons
            var saveButton = new Button
            {
                Text = "Save",
                DialogResult = DialogResult.OK,
                Font = buttonFont,
                Padding = buttonPadding,
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = buttonSize,
                Margin = buttonMargin,
                Height = buttonSize.Height
            };
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.Click += SaveButton_Click;

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Font = buttonFont,
                Padding = buttonPadding,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Size = buttonSize,
                Margin = buttonMargin,
                Height = buttonSize.Height
            };
            cancelButton.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 212);
            cancelButton.FlatAppearance.BorderSize = 1;

            buttonPanel.Controls.Add(saveButton);
            buttonPanel.Controls.Add(cancelButton);
            mainContainer.Controls.Add(buttonPanel, 0, 6);

            this.Controls.Add(mainContainer);
            this.AcceptButton = saveButton;
            this.CancelButton = cancelButton;
        }

        private void BrowseTemplateButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*";
                openFileDialog.Title = "Select Template File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    templateFileTextBox.Text = openFileDialog.FileName;
                }
            }
        }

        private void BrowseJsonButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                openFileDialog.Title = "Select JSON Data File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    jsonDataPathTextBox.Text = openFileDialog.FileName;
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(displayNameTextBox.Text))
            {
                MessageBox.Show("Please enter a display name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            if (string.IsNullOrWhiteSpace(templateFileTextBox.Text))
            {
                MessageBox.Show("Please select a template file.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            if (string.IsNullOrWhiteSpace(jsonDataPathTextBox.Text))
            {
                MessageBox.Show("Please select a JSON data file.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            // Create and save the custom assessment type
            var customType = new CustomAssessmentType(
                displayNameTextBox.Text.Trim(),
                templateFileTextBox.Text.Trim(),
                jsonDataPathTextBox.Text.Trim()
            );

            if (CustomAssessmentTypeManager.AddCustomType(customType))
            {
                MessageBox.Show($"Custom assessment type '{customType.DisplayName}' has been added successfully.", 
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Failed to add custom assessment type. A type with this name may already exist.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }
    }
}
