using System;
using System.Drawing;
using System.Windows.Forms;
using iTextDesignerWithGUI.Models;
using System.Collections.Generic;
using System.Linq;
using iTextDesignerWithGUI.Services;

namespace iTextDesignerWithGUI.Forms
{
    public class AssessmentTypeSelector : Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public AssessmentTypeWrapper SelectedTypeWrapper { get; private set; }
        public string SelectedBuiltInType => SelectedTypeWrapper?.IsBuiltIn == true ? SelectedTypeWrapper.BuiltInType : null;
        public bool WasCancelled { get; private set; } = true;

        // UI Components
        private ComboBox projectComboBox;
        private ComboBox assessmentComboBox;
        private Label projectLabel;
        private Label assessmentLabel;

        public AssessmentTypeSelector()
        {
            try
            {
                InitializeComponent();
                InitializeForm();
                
                // Add a slight delay to ensure the UI is fully initialized before populating assessment types
                // This helps prevent race conditions in certain environments (like running from a build)
                System.Windows.Forms.Timer initTimer = new System.Windows.Forms.Timer();
                initTimer.Interval = 100; // 100ms delay
                initTimer.Tick += (s, e) => 
                {
                    try
                    {
                        initTimer.Stop();
                        initTimer.Dispose();
                        
                        // Now perform initial load of the assessment types
                        if (projectComboBox?.SelectedItem is ProjectDirectoryDefinition selectedProject && selectedProject != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Initial load after constructor for project: {selectedProject.Name}");
                            PopulateAssessmentTypes(selectedProject.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in delayed initialization: {ex}");
                    }
                };
                initTimer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing AssessmentTypeSelector: {ex}");
                MessageBox.Show($"Error initializing form: {ex.Message}", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        }

        private void InitializeForm()
        {
            // Form settings
            this.Text = "Select Assessment Type";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new System.Drawing.Size(400, 300); // Increased height to ensure buttons are visible
            this.Padding = new Padding(20);
            this.BackColor = Color.White;

            // Create main container
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4, // 4 rows now: project label, project dropdown, assessment label, assessment selection and buttons
                ColumnCount = 1,
                Padding = new Padding(10),
            };
            
            // Configure row styles for better layout
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Project label
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Project dropdown
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Assessment label
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Assessment selection and buttons

            // Add project selection label
            projectLabel = new Label
            {
                Text = "Please select a project:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };
            mainContainer.Controls.Add(projectLabel, 0, 0);

            // Create project combobox container
            var projectComboBoxContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                AutoSize = true,
                Margin = new Padding(0, 3, 0, 20)
            };
            projectComboBoxContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Add project combobox
            projectComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Height = 25
            };
            projectComboBox.SelectedIndexChanged += ProjectComboBox_SelectedIndexChanged;

            // Load projects from pdfCreationData.json
            LoadProjects();

            projectComboBoxContainer.Controls.Add(projectComboBox, 0, 0);
            mainContainer.Controls.Add(projectComboBoxContainer, 0, 1);

            // Add assessment type label
            assessmentLabel = new Label
            {
                Text = "Please select the type of assessment:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };
            mainContainer.Controls.Add(assessmentLabel, 0, 2);

            // Selection container for the assessment dropdown and add button
            var assessmentSelectionContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 2,
                Margin = new Padding(0, 0, 0, 5) // Reduced bottom margin for less spacing
            };
            
            // First column for the dropdown (takes most of the space)
            assessmentSelectionContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 85f));
            
            // Second column for the add button (fixed width)
            assessmentSelectionContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40f));
            
            // Configure row styles for better spacing
            assessmentSelectionContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // First row - dropdown
            assessmentSelectionContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Second row - buttons

            // Add assessment combobox
            assessmentComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Height = 25
            };
            assessmentSelectionContainer.Controls.Add(assessmentComboBox, 0, 0);

            // Add new template button
            var addTemplateButton = new Button
            {
                Text = "+",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Size = new Size(30, 25),
                Margin = new Padding(5, 2, 0, 0), // Changed from 3 to 0 to move it up by 3 pixels
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0),
                UseCompatibleTextRendering = true
            };
            addTemplateButton.Click += AddTemplateButton_Click;
            addTemplateButton.FlatAppearance.BorderSize = 0;
            assessmentSelectionContainer.Controls.Add(addTemplateButton, 1, 0);

            // Buttons panel - now in its own row with fixed height
            var buttonsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
                Margin = new Padding(0, 10, 0, 0), // Added 10px top margin for spacing between dropdown and buttons
                Height = 45 // Taller buttons
            };
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            // Button base style
            var buttonSize = new Size(120, 40); // Larger buttons
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
                Height = buttonSize.Height,
                Enabled = false // Initially disabled since no assessment is selected yet
            };
            okButton.Click += (s, e) =>
            {
                try
                {
                    // We don't need to check if an assessment is selected here anymore
                    // because the button will be disabled when nothing is selected
                    SelectedTypeWrapper = (AssessmentTypeWrapper)assessmentComboBox.SelectedItem;
                    WasCancelled = false;
                    this.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error selecting assessment type: {ex}");
                    MessageBox.Show($"Error selecting assessment type: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

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
            cancelButton.Click += (s, e) => this.Close();

            buttonsPanel.Controls.Add(okButton, 0, 0);
            buttonsPanel.Controls.Add(cancelButton, 1, 0);
            assessmentSelectionContainer.Controls.Add(buttonsPanel, 0, 1);

            mainContainer.Controls.Add(assessmentSelectionContainer, 0, 3);

            this.Controls.Add(mainContainer);
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            UpdateOkButtonAppearance(okButton);
            
            // Add the assessmentComboBox event handler after okButton is created
            assessmentComboBox.SelectedIndexChanged += (s, e) => {
                okButton.Enabled = assessmentComboBox.SelectedItem != null;
                UpdateOkButtonAppearance(okButton);
            };
        }

        /// <summary>
        /// Loads projects from pdfCreationData.json into the project combobox
        /// </summary>
        private void LoadProjects()
        {
            try
            {
                // Get projects from the loader
                var projectDirectories = ProjectDirectoryLoader.Instance.LoadProjectDirectories();
                
                if (projectDirectories != null && projectDirectories.Count > 0)
                {
                    // Add projects to the dropdown
                    projectComboBox.BeginUpdate();
                    projectComboBox.Items.Clear();
                    
                    foreach (var project in projectDirectories)
                    {
                        if (project != null && !string.IsNullOrEmpty(project.Name))
                        {
                            projectComboBox.Items.Add(project);
                        }
                    }
                    
                    projectComboBox.DisplayMember = "Name";
                    projectComboBox.EndUpdate();
                    
                    // Select first project without triggering event (will be handled in constructor)
                    if (projectComboBox.Items.Count > 0)
                    {
                        projectComboBox.SelectedIndex = 0;
                    }
                }
                else
                {
                    // If no projects were found, disable the assessment selector
                    assessmentComboBox.Enabled = false;
                    MessageBox.Show("No projects found in the configuration. Please check your pdfCreationData.json file.", 
                        "No Projects Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading projects: {ex.Message}");
                MessageBox.Show($"Error loading projects: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Loads assessment types filtered by the selected project
        /// </summary>
        /// <param name="projectName">Name of the project to filter by, or null for all projects</param>
        private void LoadAssessmentTypes(string projectName)
        {
            // This method is called by the event handler, so we'll ensure assessment types are populated
            PopulateAssessmentTypes(projectName);
        }
        
        /// <summary>
        /// Populates the assessment type dropdown with filtered types
        /// </summary>
        /// <param name="projectName">The project name to filter by</param>
        private void PopulateAssessmentTypes(string projectName)
        {
            try
            {
                // Safety check for null or disposed controls
                if (assessmentComboBox == null || assessmentComboBox.IsDisposed)
                {
                    System.Diagnostics.Debug.WriteLine("Cannot populate assessment types: ComboBox is null or disposed");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"Populating assessment types for project: '{projectName}'");
                
                // Clear the current items
                assessmentComboBox.BeginUpdate();
                assessmentComboBox.Items.Clear();
                
                // Get all assessment types from the JSON file and filter by project
                var allTypes = AssessmentTypeWrapper.DiscoverAssessmentTypes() ?? new List<AssessmentTypeWrapper>();
                System.Diagnostics.Debug.WriteLine($"Found {allTypes.Count} total assessment types");
                
                // Apply project filter
                var filteredTypes = !string.IsNullOrEmpty(projectName) 
                    ? ProjectAssessmentFilters.FilterByProject(allTypes, projectName) 
                    : allTypes;
                    
                System.Diagnostics.Debug.WriteLine($"Filtered to {filteredTypes?.Count ?? 0} types for project '{projectName}'");
                
                // Ensure we have a valid list
                if (filteredTypes == null)
                {
                    filteredTypes = new List<AssessmentTypeWrapper>();
                }
                
                // Track display names to avoid duplicates
                var displayNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                
                // Add filtered types to the dropdown
                foreach (var wrapper in filteredTypes)
                {
                    if (wrapper == null || string.IsNullOrEmpty(wrapper.DisplayName))
                    {
                        continue;
                    }
                    
                    // Only add if we don't already have an item with this display name
                    if (!displayNames.Contains(wrapper.DisplayName))
                    {
                        assessmentComboBox.Items.Add(wrapper);
                        displayNames.Add(wrapper.DisplayName);
                        System.Diagnostics.Debug.WriteLine($"Added assessment type: {wrapper.DisplayName}");
                    }
                }
                
                assessmentComboBox.DisplayMember = "DisplayName";
                assessmentComboBox.EndUpdate();
                
                // Select the first item if available
                if (assessmentComboBox.Items.Count > 0)
                {
                    assessmentComboBox.SelectedIndex = 0;
                    var selected = assessmentComboBox.SelectedItem as AssessmentTypeWrapper;
                    System.Diagnostics.Debug.WriteLine($"Selected first assessment: {selected?.DisplayName ?? "null"}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No assessment types found for project '{projectName}'");
                    // Make sure the OK button is disabled when there are no items
                    Button okButton = this.AcceptButton as Button;
                    if (okButton != null)
                    {
                        okButton.Enabled = false;
                        UpdateOkButtonAppearance(okButton);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error populating assessment types: {ex}");
                MessageBox.Show($"Error loading assessment types: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Event handler for when the selected project changes
        /// </summary>
        private void ProjectComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (projectComboBox.SelectedItem == null)
                {
                    LoadAssessmentTypes(null);
                    return;
                }
                
                var selectedProject = projectComboBox.SelectedItem as ProjectDirectoryDefinition;
                string projectName = selectedProject?.Name;
                
                // Reload assessment types filtered by the selected project
                LoadAssessmentTypes(projectName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in project selection change: {ex.Message}");
                MessageBox.Show($"Error in project selection: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void AddTemplateButton_Click(object sender, EventArgs e)
        {
            using (var addCustomTypeForm = new AddCustomAssessmentTypeForm())
            {
                if (addCustomTypeForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh the form to show the new custom type
                    this.Close();
                    var newSelector = new AssessmentTypeSelector();
                    newSelector.ShowDialog();
                }
            }
        }

        /// <summary>
        /// Updates the appearance of the OK button based on its enabled state
        /// </summary>
        private void UpdateOkButtonAppearance(Button button)
        {
            if (button.Enabled)
            {
                // Enabled appearance
                button.BackColor = Color.FromArgb(0, 120, 212);
                button.ForeColor = Color.White;
            }
            else
            {
                // Disabled appearance - grey
                button.BackColor = Color.LightGray;
                button.ForeColor = Color.DarkGray;
            }
        }
    }
}
