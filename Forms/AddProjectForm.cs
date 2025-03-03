using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using iTextDesignerWithGUI.Models;

namespace iTextDesignerWithGUI.Forms
{
    /// <summary>
    /// Form for adding a new project directory to pdfCreationData.json
    /// </summary>
    public class AddProjectForm : Form
    {
        private TextBox projectNameTextBox;
        private Label pathPreviewLabel;
        private Label statusLabel;

        public AddProjectForm()
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Form settings
            this.Text = "Add Project";
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
                RowCount = 6,
                ColumnCount = 1,
                Padding = new Padding(20),
                BackColor = Color.White
            };

            // Add rows with proper sizing
            for (int i = 0; i < 6; i++)
            {
                mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            // Title
            var titleLabel = new Label
            {
                Text = "Add New Project",
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

            // Add helper text for naming convention
            var helperLabel = new Label
            {
                Text = "Use PascalCase naming (e.g., NewProjectName)",
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(108, 117, 125),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            mainContainer.Controls.Add(helperLabel, 0, 2);

            // Project name text box
            projectNameTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Height = 32,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 249, 250),
                Margin = new Padding(0, 0, 0, 20)
            };
            projectNameTextBox.TextChanged += ProjectNameTextBox_TextChanged;
            mainContainer.Controls.Add(projectNameTextBox, 0, 3);

            // Path preview label
            var pathPreviewTitleLabel = new Label
            {
                Text = "Project Path Preview:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            mainContainer.Controls.Add(pathPreviewTitleLabel, 0, 4);

            pathPreviewLabel = new Label
            {
                Text = "Templates/",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20),
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 249, 250)
            };
            mainContainer.Controls.Add(pathPreviewLabel, 0, 5);

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
            mainContainer.Controls.Add(statusLabel, 0, 5);

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

            // Create Save button
            var saveButton = new Button
            {
                Text = "Save",
                DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Padding = new Padding(15, 8, 15, 8),
                BackColor = Color.FromArgb(40, 167, 69), // Success color
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 40),
                Margin = new Padding(0, 0, 10, 0),
                Cursor = Cursors.Hand
            };
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.Click += SaveButton_Click;
            buttonPanel.Controls.Add(saveButton);

            // Create Cancel button
            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Padding = new Padding(15, 8, 15, 8),
                BackColor = Color.FromArgb(108, 117, 125), // Secondary color
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 40),
                Margin = new Padding(10, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            buttonPanel.Controls.Add(cancelButton);

            this.Controls.Add(buttonPanel);
            this.Controls.Add(mainContainer);
            this.AcceptButton = saveButton;
            this.CancelButton = cancelButton;
        }

        /// <summary>
        /// Updates the path preview label as the user types the project name
        /// </summary>
        private void ProjectNameTextBox_TextChanged(object sender, EventArgs e)
        {
            // Update the path preview label with the entered project name
            string projectName = projectNameTextBox.Text.Trim();
            pathPreviewLabel.Text = $"Templates/{projectName}";
        }

        /// <summary>
        /// Saves the new project to pdfCreationData.json
        /// </summary>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Validate input
            string projectName = projectNameTextBox.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(projectName))
            {
                UpdateStatus("Please enter a project name", isSuccess: false);
                MessageBox.Show("Please enter a project name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            try
            {
                // Get the path to pdfCreationData.json
                string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\.."));
                string jsonPath = Path.Combine(projectRoot, "PersistentDataJSON", "pdfCreationData.json");

                if (!File.Exists(jsonPath))
                {
                    UpdateStatus("Could not find the project directories configuration file", isSuccess: false);
                    MessageBox.Show("Could not find the project directories configuration file.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None;
                    return;
                }

                // Read the existing JSON data
                string jsonContent = File.ReadAllText(jsonPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                
                var data = JsonSerializer.Deserialize<ProjectDirectoriesData>(jsonContent, options);
                
                if (data?.ProjectDirectories == null)
                {
                    data = new ProjectDirectoriesData
                    {
                        ProjectDirectories = new List<ProjectDirectory>()
                    };
                }

                // Check if a project with the same name already exists
                if (data.ProjectDirectories.Any(p => p.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase)))
                {
                    UpdateStatus($"A project with the name '{projectName}' already exists", isSuccess: false);
                    MessageBox.Show($"A project with the name '{projectName}' already exists.", "Duplicate Project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }

                // Add the new project
                string projectPath = $"Templates/{projectName}";
                data.ProjectDirectories.Add(new ProjectDirectory
                {
                    Name = projectName,
                    Path = projectPath
                });

                // Create the project directory if it doesn't exist
                string fullProjectPath = Path.Combine(projectRoot, projectPath);
                if (!Directory.Exists(fullProjectPath))
                {
                    Directory.CreateDirectory(fullProjectPath);
                }

                // Save the updated JSON back to the file
                string updatedJson = JsonSerializer.Serialize(data, options);
                File.WriteAllText(jsonPath, updatedJson);

                UpdateStatus($"Project '{projectName}' has been added successfully", isSuccess: true);
                MessageBox.Show($"Project '{projectName}' has been added successfully. The directory has been created at '{projectPath}'.", 
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                UpdateStatus("Error adding project", isSuccess: false);
                MessageBox.Show($"Error adding project: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
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
