using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using iTextDesignerWithGUI.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

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
        private ComboBox projectNameComboBox;

        public AddCustomAssessmentTypeForm()
        {
            InitializeForm();
            LoadProjectDirectories();
        }

        private void InitializeForm()
        {
            // Form settings
            this.Text = "Add Custom Assessment Type";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new System.Drawing.Size(500, 400);
            this.Padding = new Padding(20);
            this.BackColor = Color.White;

            // Create main container
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 8,
                ColumnCount = 1,
                Padding = new Padding(10),
            };

            // Add rows
            for (int i = 0; i < 8; i++)
            {
                mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            // Project name section
            var projectNameLabel = new Label
            {
                Text = "Project Name:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            mainContainer.Controls.Add(projectNameLabel, 0, 0);

            projectNameComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Margin = new Padding(0, 0, 0, 15),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            projectNameComboBox.SelectedIndexChanged += ProjectNameComboBox_SelectedIndexChanged;
            mainContainer.Controls.Add(projectNameComboBox, 0, 1);

            // Display name section
            var displayNameLabel = new Label
            {
                Text = "PDF Name:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            mainContainer.Controls.Add(displayNameLabel, 0, 2);

            var displayNameContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 15)
            };
            displayNameContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));
            displayNameContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            displayNameTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Height = 25
            };
            displayNameContainer.Controls.Add(displayNameTextBox, 0, 0);

            var generateFilesButton = new Button
            {
                Text = "Generate Files",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Margin = new Padding(5, 0, 0, 0),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White
            };
            generateFilesButton.Click += GenerateFilesButton_Click;
            displayNameContainer.Controls.Add(generateFilesButton, 1, 0);

            mainContainer.Controls.Add(displayNameContainer, 0, 3);

            // Template file section
            var templateFileLabel = new Label
            {
                Text = "Template File:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            mainContainer.Controls.Add(templateFileLabel, 0, 4);

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

            mainContainer.Controls.Add(templateFileContainer, 0, 5);

            // JSON data path section
            var jsonDataPathLabel = new Label
            {
                Text = "JSON Data Path:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            mainContainer.Controls.Add(jsonDataPathLabel, 0, 6);

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

            mainContainer.Controls.Add(jsonDataPathContainer, 0, 7);

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
            mainContainer.Controls.Add(buttonPanel, 0, 8);

            this.Controls.Add(mainContainer);
            this.AcceptButton = saveButton;
            this.CancelButton = cancelButton;
        }

        // Load project directories from pdfCreationData.json
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
                    projectNameComboBox.Items.Clear();
                    projectNameComboBox.Items.AddRange(data.ProjectDirectories.Select(d => d.Name).ToArray());
                    if (projectNameComboBox.Items.Count > 0)
                    {
                        projectNameComboBox.SelectedIndex = 0;
                    }
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

        private void ProjectNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (projectNameComboBox.SelectedIndex >= 0)
            {
                // Update the template path based on the selected project
                string selectedProject = projectNameComboBox.SelectedItem.ToString();
                
                try
                {
                    // Get the project path from JSON
                    string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\.."));
                    string jsonPath = Path.Combine(projectRoot, "PersistentDataJSON", "pdfCreationData.json");
                    string jsonContent = File.ReadAllText(jsonPath);
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var data = JsonSerializer.Deserialize<ProjectDirectoriesData>(jsonContent, options);
                    
                    // Find the selected project
                    var project = data?.ProjectDirectories?.FirstOrDefault(p => p.Name == selectedProject);
                    
                    if (project != null)
                    {
                        // Update the template path with the project path
                        string fullPath = Path.Combine(projectRoot, project.Path);
                        if (Directory.Exists(fullPath))
                        {
                            // Just update the UI to show the project selection was successful
                            // We'll let the user select the specific template file
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating project selection: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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
            if (projectNameComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a project.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }
            
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

            // Get the selected project name
            string projectName = projectNameComboBox.SelectedItem.ToString();

            // Create and save the custom assessment type
            var customType = new CustomAssessmentType(
                displayNameTextBox.Text.Trim(),
                templateFileTextBox.Text.Trim(),
                jsonDataPathTextBox.Text.Trim()
            );
            
            // You could store the project name with the custom type if needed
            // For now, just log it
            Console.WriteLine($"Creating assessment type for project: {projectName}");

            if (CustomAssessmentTypeManager.AddCustomType(customType))
            {
                MessageBox.Show($"Custom assessment type '{customType.DisplayName}' has been added successfully for project '{projectName}'.", 
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Failed to add custom assessment type. A type with this name may already exist.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }

        private void GenerateFilesButton_Click(object sender, EventArgs e)
        {
            // Validate input
            if (projectNameComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a project.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(displayNameTextBox.Text))
            {
                MessageBox.Show("Please enter a PDF name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Get the selected project name and PDF name
            string projectName = projectNameComboBox.SelectedItem.ToString();
            string pdfName = displayNameTextBox.Text.Trim();
            
            try
            {
                // Get the project path from JSON
                string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\.."));
                string jsonPath = Path.Combine(projectRoot, "PersistentDataJSON", "pdfCreationData.json");
                string jsonContent = File.ReadAllText(jsonPath);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var data = JsonSerializer.Deserialize<ProjectDirectoriesData>(jsonContent, options);
                
                // Find the selected project
                var project = data?.ProjectDirectories?.FirstOrDefault(p => p.Name == projectName);
                
                if (project != null)
                {
                    // Get the template directory path from the project configuration
                    // Using the relative path directly from the JSON file
                    string templatePath = project.Path;
                    
                    // Show the confirmation modal with the PDF name and template directory
                    using (var confirmModal = new PDFGenerationConfirmModal(pdfName, templatePath))
                    {
                        if (confirmModal.ShowDialog() == DialogResult.OK)
                        {
                            // User confirmed, you can proceed with PDF generation here
                            MessageBox.Show($"PDF generation confirmed for '{pdfName}' using template at '{templatePath}'.",
                                          "Generation Confirmed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            // Future implementation: Add code to generate the PDF
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"Could not find project '{projectName}' in configuration.", 
                                   "Project Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error preparing PDF generation: {ex.Message}", 
                               "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
