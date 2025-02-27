using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using iTextDesignerWithGUI.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using iTextDesignerWithGUI.Forms;
using iTextDesignerWithGUI.Services;

namespace iTextDesignerWithGUI.Forms
{
    /// <summary>
    /// Form for adding a new custom assessment type
    /// </summary>
    public class AddCustomAssessmentTypeForm : Form
    {
        private TextBox displayNameTextBox;
        private ComboBox projectNameComboBox;
        private Label statusLabel;

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
            this.Size = new System.Drawing.Size(500, 470); 
            this.Padding = new Padding(20);
            this.BackColor = Color.White;

            // Create main container
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 8,
                ColumnCount = 1,
                Padding = new Padding(20),
                BackColor = Color.White
            };

            // Add rows with proper sizing
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 

            // Title
            var titleLabel = new Label
            {
                Text = "Add Custom Assessment Type",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainContainer.Controls.Add(titleLabel, 0, 0);

            // Project name section
            var projectNameLabel = new Label
            {
                Text = "Project Name:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            mainContainer.Controls.Add(projectNameLabel, 0, 1);

            projectNameComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Margin = new Padding(0, 0, 0, 20),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(248, 249, 250),
                Height = 30
            };
            projectNameComboBox.SelectedIndexChanged += ProjectNameComboBox_SelectedIndexChanged;
            mainContainer.Controls.Add(projectNameComboBox, 0, 2);

            // Display name section
            var displayNameLabel = new Label
            {
                Text = "PDF Name:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            mainContainer.Controls.Add(displayNameLabel, 0, 3);

            // Add helper text for naming convention
            var helperLabel = new Label
            {
                Text = "Use PascalCase naming (e.g., DelegationAssessment)",
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(108, 117, 125),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            mainContainer.Controls.Add(helperLabel, 0, 4);

            var displayNameContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            displayNameContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75));
            displayNameContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            displayNameTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Height = 32,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 249, 250)
            };
            displayNameContainer.Controls.Add(displayNameTextBox, 0, 0);

            var generateFilesButton = new Button
            {
                Text = "Generate",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Margin = new Padding(10, 0, 0, 0),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 32,
                Cursor = Cursors.Hand
            };
            generateFilesButton.FlatAppearance.BorderSize = 0;
            generateFilesButton.Click += GenerateFilesButton_Click;
            displayNameContainer.Controls.Add(generateFilesButton, 1, 0);

            mainContainer.Controls.Add(displayNameContainer, 0, 5);

            // Status label for feedback
            statusLabel = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(108, 117, 125),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20),
                Visible = false
            };
            mainContainer.Controls.Add(statusLabel, 0, 6);

            // Template manager link - styled as a more appropriate navigation element
            var templateManagerLink = new LinkLabel
            {
                Text = "Manage Templates",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                LinkColor = Color.FromArgb(23, 162, 184), // Info color
                ActiveLinkColor = Color.FromArgb(0, 123, 255),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand
            };
            templateManagerLink.LinkClicked += (sender, e) => DeleteTemplateButton_Click(sender, e);
            mainContainer.Controls.Add(templateManagerLink, 0, 7);

            // Button panel
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 60,
                Margin = new Padding(0),
                AutoSize = true,
                Padding = new Padding(20, 15, 20, 15),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            // Create Go Back button
            var goBackButton = new Button
            {
                Text = "Go Back",
                DialogResult = DialogResult.Cancel,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Padding = new Padding(15, 8, 15, 8),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 40),
                Margin = new Padding(0),
                Cursor = Cursors.Hand
            };
            goBackButton.FlatAppearance.BorderSize = 0;

            buttonPanel.Controls.Add(goBackButton);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(mainContainer);
            this.CancelButton = goBackButton;
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
                            UpdateStatus($"Project '{selectedProject}' selected", isSuccess: true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating project selection: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GenerateFilesButton_Click(object sender, EventArgs e)
        {
            // Validate input
            if (projectNameComboBox.SelectedIndex < 0)
            {
                UpdateStatus("Please select a project", isSuccess: false);
                MessageBox.Show("Please select a project.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(displayNameTextBox.Text))
            {
                UpdateStatus("Please enter a PDF name", isSuccess: false);
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
                            // User confirmed, proceed with PDF generation
                            // The confirmation modal will handle the file generation process
                            
                            // Note: No need for an additional MessageBox here as it would create
                            // an extra dialog that the user has to dismiss after file generation
                            UpdateStatus($"Generation process initiated for '{pdfName}'", isSuccess: true);
                        }
                    }
                }
                else
                {
                    UpdateStatus($"Could not find project '{projectName}' in configuration", isSuccess: false);
                    MessageBox.Show($"Could not find project '{projectName}' in configuration.", 
                                   "Project Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("Error preparing PDF generation", isSuccess: false);
                MessageBox.Show($"Error preparing PDF generation: {ex.Message}", 
                               "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            // Get the selected project name
            string projectName = projectNameComboBox.SelectedItem.ToString();

            // Create and save the custom assessment type
            var customType = new CustomAssessmentType(
                displayNameTextBox.Text.Trim(),
                "",
                ""
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

        /// <summary>
        /// Handler for the Delete Template button click event
        /// </summary>
        private void DeleteTemplateButton_Click(object sender, EventArgs e)
        {
            // Open the Delete Template form
            using (var deleteTemplateForm = new DeleteTemplateForm())
            {
                deleteTemplateForm.ShowDialog();
            }
        }

        /// <summary>
        /// Updates the status label with a message and sets its color based on success/failure
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="isSuccess">Whether the status is a success or failure message</param>
        private void UpdateStatus(string message, bool isSuccess)
        {
            statusLabel.Text = message;
            statusLabel.ForeColor = isSuccess ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69);
            statusLabel.Visible = true;
        }
    }
}
