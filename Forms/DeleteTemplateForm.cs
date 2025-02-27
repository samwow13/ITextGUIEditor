using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using iTextDesignerWithGUI.Models;
using iTextDesignerWithGUI.Services;

namespace iTextDesignerWithGUI.Forms
{
    /// <summary>
    /// Form for deleting existing assessment templates
    /// </summary>
    public class DeleteTemplateForm : Form
    {
        private ComboBox templateComboBox;
        private Label statusLabel;
        private Button deleteButton;
        private Button cancelButton;
        private readonly TemplateRemovalService _removalService;

        /// <summary>
        /// Constructor for DeleteTemplateForm
        /// </summary>
        public DeleteTemplateForm()
        {
            // Initialize the template removal service
            _removalService = new TemplateRemovalService();
            
            // Initialize the form
            InitializeForm();
            
            // Load the available templates
            LoadAvailableTemplates();
        }

        /// <summary>
        /// Initializes the form UI components
        /// </summary>
        private void InitializeForm()
        {
            // Form settings
            this.Text = "Delete Template";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(500, 300);
            this.Padding = new Padding(20);
            this.BackColor = Color.White;

            // Create main container
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(10),
            };

            // Add rows
            for (int i = 0; i < 4; i++)
            {
                mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            // Template selection section
            var templateLabel = new Label
            {
                Text = "Select Template to Delete:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            mainContainer.Controls.Add(templateLabel, 0, 0);

            templateComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Margin = new Padding(0, 0, 0, 15),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            templateComboBox.SelectedIndexChanged += TemplateComboBox_SelectedIndexChanged;
            mainContainer.Controls.Add(templateComboBox, 0, 1);

            // Status label
            statusLabel = new Label
            {
                Text = "Please select a template to delete.",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 15),
                ForeColor = Color.Gray
            };
            mainContainer.Controls.Add(statusLabel, 0, 2);

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
            var buttonSize = new Size(120, 35);
            var buttonMargin = new Padding(5, 0, 5, 0);
            var buttonFont = new Font("Segoe UI", 9F, FontStyle.Regular);
            var buttonPadding = new Padding(10, 5, 10, 5);

            // Delete button
            deleteButton = new Button
            {
                Text = "Delete Template",
                Font = buttonFont,
                Padding = buttonPadding,
                BackColor = Color.Crimson,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = buttonSize,
                Margin = buttonMargin,
                Enabled = false
            };
            deleteButton.FlatAppearance.BorderSize = 0;
            deleteButton.Click += DeleteButton_Click;
            buttonPanel.Controls.Add(deleteButton);

            // Cancel button
            cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Font = buttonFont,
                Padding = buttonPadding,
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = buttonSize,
                Margin = buttonMargin
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            buttonPanel.Controls.Add(cancelButton);

            mainContainer.Controls.Add(buttonPanel, 0, 3);

            this.Controls.Add(mainContainer);
            this.CancelButton = cancelButton;
        }

        /// <summary>
        /// Loads available templates from the assessmentTypes.json file
        /// </summary>
        private void LoadAvailableTemplates()
        {
            try
            {
                string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\.."));
                string assessmentJsonPath = Path.Combine(projectRoot, "PersistentDataJSON", "assessmentTypes.json");

                if (!File.Exists(assessmentJsonPath))
                {
                    MessageBox.Show("Could not find the assessment types configuration file.", 
                        "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string jsonContent = File.ReadAllText(assessmentJsonPath);
                using (JsonDocument document = JsonDocument.Parse(jsonContent))
                {
                    if (document.RootElement.TryGetProperty("assessmentTypes", out JsonElement assessmentTypes))
                    {
                        // Create a list to hold the display items
                        var displayItems = new List<TemplateDisplayItem>();
                        
                        foreach (JsonElement assessmentType in assessmentTypes.EnumerateArray())
                        {
                            if (assessmentType.TryGetProperty("name", out JsonElement nameElement) &&
                                assessmentType.TryGetProperty("displayName", out JsonElement displayNameElement))
                            {
                                string name = nameElement.GetString();
                                string displayName = displayNameElement.GetString();
                                
                                // Get additional path information for display
                                string templatePath = "";
                                string jsonPath = "";
                                string modelsPath = "";
                                
                                if (assessmentType.TryGetProperty("cshtmlTemplateDirectory", out JsonElement cshtmlTemplateElement))
                                {
                                    templatePath = cshtmlTemplateElement.GetString();
                                }
                                
                                if (assessmentType.TryGetProperty("jsonDataLocationDirectory", out JsonElement jsonDataElement))
                                {
                                    jsonPath = jsonDataElement.GetString();
                                }
                                
                                if (assessmentType.TryGetProperty("assessmentTypeDirectory", out JsonElement assessmentTypeElement))
                                {
                                    string assessmentTypePath = assessmentTypeElement.GetString();
                                    string[] pathParts = assessmentTypePath.Split('/');
                                    if (pathParts.Length > 2)
                                    {
                                        modelsPath = $"Models/{pathParts[1]}/{pathParts[2]}";
                                    }
                                }
                                
                                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(displayName))
                                {
                                    displayItems.Add(new TemplateDisplayItem
                                    {
                                        Name = name,
                                        DisplayName = displayName,
                                        TemplatePath = templatePath,
                                        JsonDataPath = jsonPath,
                                        ModelsPath = modelsPath
                                    });
                                }
                            }
                        }
                        
                        // Sort display items by name for easier navigation
                        displayItems = displayItems.OrderBy(item => item.Name).ToList();
                        
                        // Set the data source
                        templateComboBox.DataSource = displayItems;
                        templateComboBox.DisplayMember = "DisplayName";
                        templateComboBox.ValueMember = "Name";
                        
                        if (templateComboBox.Items.Count > 0)
                        {
                            templateComboBox.SelectedIndex = 0;
                        }
                        else
                        {
                            statusLabel.Text = "No templates available to delete.";
                            deleteButton.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading templates: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handler for template combo box selection changed event
        /// </summary>
        private void TemplateComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (templateComboBox.SelectedIndex >= 0)
            {
                var selectedItem = (TemplateDisplayItem)templateComboBox.SelectedItem;
                statusLabel.Text = $"Selected template: {selectedItem.DisplayName} ({selectedItem.Name})";
                deleteButton.Enabled = true;
            }
            else
            {
                statusLabel.Text = "Please select a template to delete.";
                deleteButton.Enabled = false;
            }
        }

        /// <summary>
        /// Handler for delete button click event
        /// </summary>
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (templateComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a template to delete.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedItem = (TemplateDisplayItem)templateComboBox.SelectedItem;
            
            // Prepare details of what will be deleted
            string details = "The following will be deleted:\n\n";
            
            if (!string.IsNullOrEmpty(selectedItem.TemplatePath))
            {
                details += $"• Template file: {selectedItem.TemplatePath}\n";
            }
            
            if (!string.IsNullOrEmpty(selectedItem.JsonDataPath))
            {
                details += $"• JSON data file: {selectedItem.JsonDataPath}\n";
            }
            
            if (!string.IsNullOrEmpty(selectedItem.ModelsPath))
            {
                details += $"• Models directory: {selectedItem.ModelsPath} (including all files)\n";
            }
            
            details += $"• Entry in assessmentTypes.json for '{selectedItem.Name}'";
            
            // Confirm deletion
            DialogResult result = MessageBox.Show(
                $"Are you sure you want to delete the template '{selectedItem.DisplayName}'?\n\n" +
                details,
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            
            if (result == DialogResult.Yes)
            {
                // Show a progress form during deletion
                using (var progressForm = new ProgressForm("Deleting Template", $"Deleting template '{selectedItem.DisplayName}'..."))
                {
                    progressForm.Show();
                    progressForm.SetProgress(30, "Removing from configuration...");
                    
                    // Perform the deletion
                    bool success = _removalService.RemoveTemplate(selectedItem.Name);
                    
                    progressForm.SetProgress(100, "Complete");
                    progressForm.Close();
                    
                    if (success)
                    {
                        MessageBox.Show($"Successfully removed template '{selectedItem.DisplayName}' and its associated files.",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Refresh the template list
                        LoadAvailableTemplates();
                    }
                    else
                    {
                        MessageBox.Show($"Failed to remove template '{selectedItem.DisplayName}'.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Class to represent a template item in the combo box
    /// </summary>
    public class TemplateDisplayItem
    {
        /// <summary>
        /// Internal name of the template
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Display name of the template
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Path to the template file
        /// </summary>
        public string TemplatePath { get; set; }
        
        /// <summary>
        /// Path to the JSON data file
        /// </summary>
        public string JsonDataPath { get; set; }
        
        /// <summary>
        /// Path to the models directory
        /// </summary>
        public string ModelsPath { get; set; }
    }
    
    /// <summary>
    /// Simple progress form to show deletion progress
    /// </summary>
    public class ProgressForm : Form
    {
        private Label statusLabel;
        private ProgressBar progressBar;
        
        public ProgressForm(string title, string initialStatus)
        {
            this.Text = title;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(400, 120);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false;
            this.BackColor = Color.White;
            
            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(20)
            };
            
            statusLabel = new Label
            {
                Text = initialStatus,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                AutoSize = true
            };
            container.Controls.Add(statusLabel, 0, 0);
            
            progressBar = new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Dock = DockStyle.Fill,
                Height = 25
            };
            container.Controls.Add(progressBar, 0, 1);
            
            this.Controls.Add(container);
        }
        
        public void SetProgress(int percentage, string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetProgress(percentage, status)));
                return;
            }
            
            progressBar.Value = percentage;
            statusLabel.Text = status;
            Application.DoEvents();
        }
    }
}
