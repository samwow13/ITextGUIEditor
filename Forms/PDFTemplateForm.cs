using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace iTextDesignerWithGUI.Forms
{
    public class PDFTemplateForm : Form
    {
        public string TemplateName { get; private set; }
        public string ProjectDirectory { get; private set; }
        public bool WasCancelled { get; private set; } = true;
        private ComboBox projectDirectoryComboBox;

        public PDFTemplateForm()
        {
            InitializeForm();
            LoadProjectDirectories();
        }

        private void LoadProjectDirectories()
        {
            try
            {
                string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\.."));
                string jsonPath = Path.Combine(projectRoot, "PersistentDataJSON", "pdfCreationData.json");

                if (!File.Exists(jsonPath))
                {
                    MessageBox.Show("Could not find the project directories configuration file.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string jsonContent = File.ReadAllText(jsonPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var data = JsonSerializer.Deserialize<ProjectDirectoriesData>(jsonContent, options);
                
                if (data?.ProjectDirectories != null && data.ProjectDirectories.Any())
                {
                    projectDirectoryComboBox.Items.Clear();
                    projectDirectoryComboBox.Items.AddRange(data.ProjectDirectories.Select(d => d.Name).ToArray());
                    projectDirectoryComboBox.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No project directories found in the configuration file.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading project directories: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeForm()
        {
            // Form settings
            this.Text = "New PDF Template";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new System.Drawing.Size(400, 300);
            this.Padding = new Padding(20);
            this.BackColor = Color.White;

            // Create main container
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 1,
                Padding = new Padding(10),
            };
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  

            // Project directory label
            var directoryLabel = new Label
            {
                Text = "Select project directory:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };
            mainContainer.Controls.Add(directoryLabel, 0, 0);

            // Project directory dropdown
            projectDirectoryComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Margin = new Padding(0, 0, 0, 20),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Height = 25
            };
            mainContainer.Controls.Add(projectDirectoryComboBox, 0, 1);

            // Template name label
            var templateLabel = new Label
            {
                Text = "Enter template name:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };
            mainContainer.Controls.Add(templateLabel, 0, 2);

            // Template name input
            var textBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Margin = new Padding(0, 0, 0, 20),
                Height = 25
            };
            mainContainer.Controls.Add(textBox, 0, 3);

            // Create button panel
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 40,
                Margin = new Padding(0, 10, 0, 0),  
                AutoSize = true,
                Padding = new Padding(0)
            };

            // Button base style
            var buttonSize = new Size(100, 35);
            var buttonMargin = new Padding(5, 0, 5, 0);
            var buttonFont = new Font("Segoe UI", 9F, FontStyle.Regular);
            var buttonPadding = new Padding(10, 5, 10, 5);

            // Create and style buttons
            var okButton = new Button
            {
                Text = "OK",
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
            okButton.FlatAppearance.BorderSize = 0;

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Font = buttonFont,
                Padding = buttonPadding,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Size = buttonSize,
                Margin = buttonMargin,
                Height = buttonSize.Height
            };
            cancelButton.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 212);
            cancelButton.FlatAppearance.BorderSize = 1;

            // Add buttons to panel
            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(cancelButton);

            // Add button panel to container
            mainContainer.Controls.Add(buttonPanel, 0, 4);

            // Add container to form
            this.Controls.Add(mainContainer);

            // Wire up events
            okButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    MessageBox.Show("Please enter a template name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }

                if (projectDirectoryComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select a project directory.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }

                TemplateName = textBox.Text;
                ProjectDirectory = projectDirectoryComboBox.SelectedItem.ToString();
                WasCancelled = false;
            };

            cancelButton.Click += (s, e) => WasCancelled = true;

            // Set accept button
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }
    }

    // Class to deserialize JSON data
    public class ProjectDirectory
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class ProjectDirectoriesData
    {
        public List<ProjectDirectory> ProjectDirectories { get; set; }
    }
}
