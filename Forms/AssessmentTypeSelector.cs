using System;
using System.Drawing;
using System.Windows.Forms;
using iTextDesignerWithGUI.Models;
using System.Collections.Generic;
using System.Linq;
using iTextDesignerWithGUI.Services;

namespace iTextDesignerWithGUI.Forms
{
    public partial class AssessmentTypeSelector : Form
    {
        public AssessmentTypeWrapper SelectedTypeWrapper { get; private set; }
        public AssessmentType? SelectedBuiltInType => SelectedTypeWrapper?.IsBuiltIn == true ? SelectedTypeWrapper.BuiltInType : null;
        public bool WasCancelled { get; private set; } = true;

        public AssessmentTypeSelector()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Form settings
            this.Text = "Select Assessment Type";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new System.Drawing.Size(400, 225);
            this.Padding = new Padding(20);
            this.BackColor = Color.White;

            // Create main container
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(10),
            };
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Add descriptive label
            var label = new Label
            {
                Text = "Please select the type of assessment:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };
            mainContainer.Controls.Add(label, 0, 0);

            // Create combobox container
            var comboBoxContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                Margin = new Padding(0, 3, 0, 20)
            };
            comboBoxContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            comboBoxContainer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            // Add combobox
            var comboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Height = 25
            };

            // Track display names to avoid duplicates
            var displayNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            // Load assessment types from the JSON file first
            var jsonLoader = AssessmentTypeJsonLoader.Instance;
            var jsonTypes = jsonLoader.LoadAssessmentTypes(forceRefresh: true);
            
            // Create wrappers for each JSON type
            foreach (var jsonType in jsonTypes)
            {
                try
                {
                    var wrapper = AssessmentTypeWrapper.FromJsonDefinition(jsonType);
                    
                    // Only add if we don't already have an item with this display name
                    if (!displayNames.Contains(wrapper.DisplayName))
                    {
                        comboBox.Items.Add(wrapper);
                        displayNames.Add(wrapper.DisplayName);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating wrapper for JSON type {jsonType.Name}: {ex.Message}");
                }
            }

            // Add built-in assessment types (only if they're not already in the JSON)
            foreach (AssessmentType type in Enum.GetValues(typeof(AssessmentType)))
            {
                // Skip "Tester" since we'll get it from discovery instead
                if (type.ToString() != AssessmentTypeConstants.Tester)
                {
                    var wrapper = AssessmentTypeWrapper.FromBuiltIn(type);
                    
                    // Only add if we don't already have an item with this display name
                    if (!displayNames.Contains(wrapper.DisplayName))
                    {
                        comboBox.Items.Add(wrapper);
                        displayNames.Add(wrapper.DisplayName);
                    }
                }
            }

            // Add dynamically discovered assessment types
            var discoveredTypes = AssessmentTypeWrapper.DiscoverAssessmentTypes();
            
            foreach (var discoveredWrapper in discoveredTypes)
            {
                // Only add if we don't already have an item with this display name
                if (!displayNames.Contains(discoveredWrapper.DisplayName))
                {
                    comboBox.Items.Add(discoveredWrapper);
                    displayNames.Add(discoveredWrapper.DisplayName);
                }
            }

            // Add custom assessment types
            foreach (var customType in CustomAssessmentTypeManager.GetAllCustomTypes())
            {
                var wrapper = AssessmentTypeWrapper.FromCustom(customType);
                
                // Only add if we don't already have an item with this display name
                if (!displayNames.Contains(wrapper.DisplayName))
                {
                    comboBox.Items.Add(wrapper);
                    displayNames.Add(wrapper.DisplayName);
                }
            }

            comboBox.DisplayMember = "DisplayName";
            comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
            comboBoxContainer.Controls.Add(comboBox, 0, 0);

            // Add new template button
            var addTemplateButton = new Button
            {
                Text = "+",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Size = new Size(30, 25),
                Margin = new Padding(5, 3, 0, 0),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0),
                UseCompatibleTextRendering = true
            };
            addTemplateButton.Click += AddTemplateButton_Click;
            addTemplateButton.FlatAppearance.BorderSize = 0;
            comboBoxContainer.Controls.Add(addTemplateButton, 1, 0);

            mainContainer.Controls.Add(comboBoxContainer, 0, 1);

            // Create button panel
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 40,
                Margin = new Padding(0),
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
            okButton.Click += (s, e) =>
            {
                SelectedTypeWrapper = (AssessmentTypeWrapper)comboBox.SelectedItem;
                WasCancelled = false;
                this.Close();
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

            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(cancelButton);
            mainContainer.Controls.Add(buttonPanel, 0, 2);

            this.Controls.Add(mainContainer);
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
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
    }
}
