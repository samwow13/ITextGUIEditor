using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.Generic;
using iTextDesignerWithGUI.Models;
using iTextDesignerWithGUI.Models.TestRazorDataModels;
using iTextDesignerWithGUI.Controls;
using System.IO;
using System.Text;
using System.Linq;
using iTextDesignerWithGUI.Services;
using System.Diagnostics;

namespace iTextDesignerWithGUI.Forms
{
    /// <summary>
    /// A form that appears below the main form with JSON and C# Model views
    /// </summary>
    public class SecondaryForm : Form
    {
        private readonly MainForm _parentForm;
        private TabControl _tabControl;
        private TabPage _jsonViewTab;
        private TabPage _powerToolsTab;  // New tab for Power Tools
        private Button _initialFormButton;  // Button for Initial Form
        private Label _instructionLabel;  // Label for general instructions
        private ToolTip _toolTip;  // ToolTip for displaying hover information
        private JsonChecklistControl _jsonChecklistControl;
        private object _currentData;

        public SecondaryForm(MainForm parentForm)
        {
            _parentForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm));
            InitializeComponent();
            PositionFormBelowParent();
        }

        public void UpdateData(object data)
        {
            // Clear previous data first
            if (_currentData != null)
            {
                _currentData = null;
                GC.Collect(); // Optional: Request garbage collection
            }

            _currentData = data;
            if (data != null)
            {
                // Create a clean representation of the data based on its type
                object displayData = data;
                string documentType = data.GetType().Name;

                // Use reflection to create a dynamic object with the properties from the data object
                try
                {
                    // Get the properties of the data object
                    var properties = data.GetType().GetProperties();

                    // Create a dictionary to hold the property values
                    var propertyValues = new Dictionary<string, object>();

                    // Add each property to the dictionary
                    foreach (var prop in properties)
                    {
                        propertyValues[prop.Name] = prop.GetValue(data);
                    }

                    // Add the type information
                    propertyValues["Type"] = documentType.Replace("DataInstance", " Assessment");

                    // Create a dynamic object from the dictionary
                    displayData = propertyValues;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating display data: {ex.Message}");
                    // Fall back to using the original data object
                }

                _jsonChecklistControl.UpdateData(displayData, documentType);
            }
            else
            {
                _jsonChecklistControl.UpdateData(null, null);
            }
        }

        private void InitializeComponent()
        {
            // Form properties
            Text = "JSON Viewer";
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            ShowInTaskbar = false;

            // Initialize tab control
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Initialize tooltip
            _toolTip = new ToolTip();
            _toolTip.AutoPopDelay = 10000;  // Show the tooltip for 10 seconds
            _toolTip.InitialDelay = 500;    // Wait half a second before showing the tooltip
            _toolTip.ReshowDelay = 200;     // Delay before showing the tooltip again if moved to another control

            // JSON View Tab
            _jsonViewTab = new TabPage("JSON View");
            _jsonChecklistControl = new JsonChecklistControl
            {
                Dock = DockStyle.Fill
            };
            _jsonViewTab.Controls.Add(_jsonChecklistControl);

            // Power Tools Tab
            _powerToolsTab = new TabPage("Power Tools");
            
            // Create a panel to organize controls in the Power Tools tab
            Panel powerToolsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            
            // Create the instruction label
            _instructionLabel = new Label
            {
                Text = "Hover over buttons for detailed instructions",
                Location = new Point(20, 20),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.DarkSlateGray
            };
            
            // Create the Initial Form button
            _initialFormButton = new Button
            {
                Text = "Initial Form Copy",
                Size = new Size(160, 40),  // Wider to accommodate the longer text
                Location = new Point(20, 50),  // Moved down to accommodate the label
                BackColor = SystemColors.Control,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            _initialFormButton.Click += InitialFormButton_Click;
            
            // Set the tooltip for the Initial Form button
            string tooltipText = "This copies the associated Model, globalCSS, and current state of the HTML file" + Environment.NewLine +
                                 "into your clipboard, along with a prompt to seed your Data model into the template." + Environment.NewLine +
                                 "Copy this into the premiere ChatGPT model";
            _toolTip.SetToolTip(_initialFormButton, tooltipText);
            
            // Add controls to the panel
            powerToolsPanel.Controls.Add(_instructionLabel);
            powerToolsPanel.Controls.Add(_initialFormButton);
            
            // Add panel to the Power Tools tab
            _powerToolsTab.Controls.Add(powerToolsPanel);

            // Add tabs to tab control
            _tabControl.TabPages.Add(_jsonViewTab);
            _tabControl.TabPages.Add(_powerToolsTab);  // Add the Power Tools tab

            // Add tab control to form
            Controls.Add(_tabControl);

            // Set form size
            Size = new Size(800, 400);
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

        /// <summary>
        /// Handles the click event for the Initial Form Copy button
        /// Gathers model, template, and CSS files and copies them to clipboard
        /// </summary>
        private void InitialFormButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentData == null)
                {
                    MessageBox.Show("No assessment data is currently loaded.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Use ProjectDirectoryService to find root directory
                var projectDirService = new ProjectDirectoryService();
                string rootPath = projectDirService.GetRootDirectory();

                // Get the current assessment type from the data object
                string currentAssessmentType = _currentData.GetType().Name.Replace("Instance", "");

                // Load the assessment types from JSON
                string assessmentTypesPath = projectDirService.GetFilePath("PersistentDataJSON/assessmentTypes.json");
                string assessmentTypesJson = File.ReadAllText(assessmentTypesPath);
                var assessmentTypesDoc = JsonDocument.Parse(assessmentTypesJson);
                var assessmentTypes = assessmentTypesDoc.RootElement.GetProperty("assessmentTypes");

                // Find the matching assessment type entry
                JsonElement? matchingAssessment = null;
                foreach (var assessment in assessmentTypes.EnumerateArray())
                {
                    string name = assessment.GetProperty("name").GetString();
                    if (name == currentAssessmentType)
                    {
                        matchingAssessment = assessment;
                        break;
                    }
                }

                if (matchingAssessment == null)
                {
                    MessageBox.Show($"Could not find assessment type '{currentAssessmentType}' in assessmentTypes.json", 
                        "Assessment Type Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Get paths from the matching assessment
                string modelPath = projectDirService.GetFilePath(
                    matchingAssessment.Value.GetProperty("assessmentDataInstanceDirectory").GetString());
                
                string templatePath = projectDirService.GetFilePath(
                    matchingAssessment.Value.GetProperty("cshtmlTemplateDirectory").GetString());
                
                // Get the reference JSON data path
                string jsonDataPath = projectDirService.GetFilePath(
                    matchingAssessment.Value.GetProperty("jsonDataLocationDirectory").GetString());

                // Global CSS is always in the same location
                string cssPath = projectDirService.GetFilePath("Templates/globalStyles.css");

                // Check if all required files exist
                if (!File.Exists(modelPath))
                {
                    MessageBox.Show($"Model file not found: {modelPath}", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                if (!File.Exists(templatePath))
                {
                    MessageBox.Show($"Template file not found: {templatePath}", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                if (!File.Exists(cssPath))
                {
                    MessageBox.Show($"CSS file not found: {cssPath}", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Check if JSON reference data exists (don't fail if it doesn't)
                string jsonDataContent = "// No reference JSON data found";
                if (File.Exists(jsonDataPath))
                {
                    jsonDataContent = File.ReadAllText(jsonDataPath);
                }
                else
                {
                    Debug.WriteLine($"Warning: JSON reference data file not found: {jsonDataPath}");
                }

                // Read the content of all files
                string modelContent = File.ReadAllText(modelPath);
                string templateContent = File.ReadAllText(templatePath);
                string cssContent = File.ReadAllText(cssPath);

                // Create the clipboard content
                StringBuilder clipboardContent = new StringBuilder();
                
                clipboardContent.AppendLine("// MODEL FILE: " + Path.GetFileName(modelPath));
                clipboardContent.AppendLine("```csharp");
                clipboardContent.AppendLine(modelContent);
                clipboardContent.AppendLine("```");
                clipboardContent.AppendLine();
                
                clipboardContent.AppendLine("// GLOBAL CSS: globalStyles.css");
                clipboardContent.AppendLine("```css");
                clipboardContent.AppendLine(cssContent);
                clipboardContent.AppendLine("```");
                clipboardContent.AppendLine();
                
                clipboardContent.AppendLine("// TEMPLATE: " + Path.GetFileName(templatePath));
                clipboardContent.AppendLine("```html");
                clipboardContent.AppendLine(templateContent);
                clipboardContent.AppendLine("```");
                clipboardContent.AppendLine();
                
                clipboardContent.AppendLine("// REFERENCE JSON DATA: " + Path.GetFileName(jsonDataPath));
                clipboardContent.AppendLine("```json");
                clipboardContent.AppendLine(jsonDataContent);
                clipboardContent.AppendLine("```");
                clipboardContent.AppendLine();
                
                clipboardContent.AppendLine("Please use the model above and the globalStyles.css referenced above to generate a cshtml document (the service injects via itext in C#). The model is an instance of the data found in the json file. Do not generate Bootstrap styles and only include a link to <link href=\"globalStyles.css\" rel=\"stylesheet\">     ");

                // Copy to clipboard
                Clipboard.SetText(clipboardContent.ToString());
                
                MessageBox.Show("Files copied to clipboard successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}\n\n{ex.StackTrace}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
