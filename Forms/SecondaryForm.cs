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
using Microsoft.Win32;

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
        
        // Checkbox controls for the context builder
        private CheckBox _cssCheckBox;
        private CheckBox _templateCheckBox;
        private CheckBox _modelCheckBox;
        private CheckBox _jsonCheckBox;
        
        // Registry keys for saving preferences
        private const string RegistryPath = @"Software\ITextGUIDesigner\SecondaryForm";
        private const string CssCheckboxKey = "CssCheckboxEnabled";
        private const string TemplateCheckboxKey = "TemplateCheckboxEnabled";
        private const string ModelCheckboxKey = "ModelCheckboxEnabled";
        private const string JsonCheckboxKey = "JsonCheckboxEnabled";
        private const string SelectedTabKey = "SelectedTabIndex";

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
                Padding = new Padding(10),
                AutoScroll = true // Add scrolling for many controls
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
            
            // Create a visual divider between top and bottom sections
            Panel dividerPanel = new Panel
            {
                Location = new Point(20, 110),
                Size = new Size(powerToolsPanel.Width - 40, 2),
                BackColor = Color.LightGray,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            
            // Create a heading for the LLM Context Builder section
            Label llmContextBuilderLabel = new Label
            {
                Text = "LLM Context Builder",
                Location = new Point(20, 130),
                Size = new Size(300, 24),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.DarkSlateBlue
            };
            
            // Description label for the new section
            Label descriptionLabel = new Label
            {
                Text = "Select items to include in the context for Large Language Models:",
                Location = new Point(20, 160),
                Size = new Size(450, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            
            // Create checkboxes for selecting content to copy
            _cssCheckBox = new CheckBox
            {
                Text = "Global Styles CSS",
                Location = new Point(30, 190),
                Size = new Size(200, 24),
                Checked = LoadCheckboxState(CssCheckboxKey, true) // Load saved state with default true
            };
            _toolTip.SetToolTip(_cssCheckBox, "Include globalStyles.css in the context");
            _cssCheckBox.CheckedChanged += (s, e) => SaveCheckboxState(CssCheckboxKey, _cssCheckBox.Checked);
            
            _templateCheckBox = new CheckBox
            {
                Text = "Current CSHTML Template",
                Location = new Point(30, 220),
                Size = new Size(200, 24),
                Checked = LoadCheckboxState(TemplateCheckboxKey, true) // Load saved state with default true
            };
            _toolTip.SetToolTip(_templateCheckBox, "Include the current CSHTML template in the context");
            _templateCheckBox.CheckedChanged += (s, e) => SaveCheckboxState(TemplateCheckboxKey, _templateCheckBox.Checked);
            
            _modelCheckBox = new CheckBox
            {
                Text = "Current Model Instance",
                Location = new Point(30, 250),
                Size = new Size(200, 24),
                Checked = LoadCheckboxState(ModelCheckboxKey, true) // Load saved state with default true
            };
            _toolTip.SetToolTip(_modelCheckBox, "Include the current model instance file in the context");
            _modelCheckBox.CheckedChanged += (s, e) => SaveCheckboxState(ModelCheckboxKey, _modelCheckBox.Checked);
            
            _jsonCheckBox = new CheckBox
            {
                Text = "Current JSON Data",
                Location = new Point(30, 280),
                Size = new Size(200, 24),
                Checked = LoadCheckboxState(JsonCheckboxKey, true) // Load saved state with default true
            };
            _toolTip.SetToolTip(_jsonCheckBox, "Include the current JSON data file in the context");
            _jsonCheckBox.CheckedChanged += (s, e) => SaveCheckboxState(JsonCheckboxKey, _jsonCheckBox.Checked);
            
            // Create button to copy selected content to clipboard
            Button copyToClipboardButton = new Button
            {
                Text = "Build & Copy Context",
                Size = new Size(200, 40),
                Location = new Point(20, 320),
                BackColor = Color.FromArgb(230, 240, 255),
                ForeColor = Color.DarkBlue,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            copyToClipboardButton.Click += (s, e) => BuildAndCopyContext(_cssCheckBox.Checked, _templateCheckBox.Checked, _modelCheckBox.Checked, _jsonCheckBox.Checked);
            _toolTip.SetToolTip(copyToClipboardButton, "Build and copy the selected context items to the clipboard for use with LLMs");
            
            // Add controls to the panel
            powerToolsPanel.Controls.Add(_instructionLabel);
            powerToolsPanel.Controls.Add(_initialFormButton);
            powerToolsPanel.Controls.Add(dividerPanel);
            powerToolsPanel.Controls.Add(llmContextBuilderLabel);
            powerToolsPanel.Controls.Add(descriptionLabel);
            powerToolsPanel.Controls.Add(_cssCheckBox);
            powerToolsPanel.Controls.Add(_templateCheckBox);
            powerToolsPanel.Controls.Add(_modelCheckBox);
            powerToolsPanel.Controls.Add(_jsonCheckBox);
            powerToolsPanel.Controls.Add(copyToClipboardButton);
            
            // Add panel to the Power Tools tab
            _powerToolsTab.Controls.Add(powerToolsPanel);

            // Add tabs to tab control
            _tabControl.TabPages.Add(_jsonViewTab);
            _tabControl.TabPages.Add(_powerToolsTab);  // Add the Power Tools tab
            
            // Set the selected tab based on saved preference
            _tabControl.SelectedIndex = LoadSelectedTabIndex();
            
            // Add event handler to save the selected tab when it changes
            _tabControl.SelectedIndexChanged += (s, e) => SaveSelectedTabIndex(_tabControl.SelectedIndex);

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
                
                // Update the divider width when the form resizes
                if (_powerToolsTab.Controls.Count > 0)
                {
                    Panel powerToolsPanel = (Panel)_powerToolsTab.Controls[0];
                    if (powerToolsPanel.Controls.Count > 3)
                    {
                        Panel dividerPanel = (Panel)powerToolsPanel.Controls[3];
                        dividerPanel.Width = powerToolsPanel.Width - 40;
                    }
                }
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
                
                clipboardContent.AppendLine("Please generate a complete .cshtml template using the model structure above and reference the globalStyles.css. Follow these guidelines for Razor compatibility:");
                clipboardContent.AppendLine("1. Begin with the appropriate model directive that exactly matches the model class name and namespace");
                clipboardContent.AppendLine("2. ALWAYS include these required namespace imports:");
                clipboardContent.AppendLine("   @using System");
                clipboardContent.AppendLine("   @using System.Linq");
                clipboardContent.AppendLine("   @using System.Collections.Generic");
                clipboardContent.AppendLine("3. Access properties using the correct path hierarchy (e.g., if properties are nested in Model.SomeProperty.ChildProperty)");
                clipboardContent.AppendLine("4. When using .NET framework types (Convert, DateTime, etc.), use their fully qualified names (System.Convert, System.DateTime)");
                clipboardContent.AppendLine("5. For collection operations (Any(), First(), etc.), ensure they are properly accessed on collection properties");
                clipboardContent.AppendLine("6. Avoid variable names that conflict with Razor keywords (like 'section', 'model', 'page')");
                clipboardContent.AppendLine("7. Only include this CSS reference: <link href=\"globalStyles.css\" rel=\"stylesheet\">");
                clipboardContent.AppendLine("8. Do not include Bootstrap or other external CSS frameworks");
                clipboardContent.AppendLine("9. The template will be processed by RazorLight engine with iText for PDF generation");
                clipboardContent.AppendLine("10. When working with JSON data types in Razor, use safe parsing instead of direct conversion:");
                clipboardContent.AppendLine("    - Use bool.TryParse() instead of Convert.ToBoolean()");
                clipboardContent.AppendLine("    - Use DateTime.TryParse() instead of DateTime.Parse()");
                clipboardContent.AppendLine("    - Use decimal.TryParse() or double.TryParse() for numeric values");
                clipboardContent.AppendLine("    - Always include null checks and fallback values");
                clipboardContent.AppendLine("    - When using nullable properties with the null conditional operator (?.) in conditions:");
                clipboardContent.AppendLine("      CORRECT: @(Model.Model?.IsActive == true ? \"Yes\" : \"No\")");
                clipboardContent.AppendLine("      INCORRECT: @(Model.Model?.IsActive ? \"Yes\" : \"No\") // This will cause a nullable bool error");
                clipboardContent.AppendLine("11. When writing conditional logic blocks in Razor:");
                clipboardContent.AppendLine("    - Avoid nesting @{ } code blocks - this causes parsing errors");
                clipboardContent.AppendLine("    - Declare variables in a single @{ } block at the beginning of complex conditional sections");
                clipboardContent.AppendLine("    - Use separate @if statements outside the @{ } block for rendering HTML");
                clipboardContent.AppendLine("12. Remember that the model structure determines how properties are accessed:");
                clipboardContent.AppendLine("    - If your JSON has { \"model\": { \"name\": \"...\" } }, use @Model.Model.Name");
                clipboardContent.AppendLine("    - If your JSON has { \"name\": \"...\" }, use @Model.Name");
                clipboardContent.AppendLine("13. When using TryParse methods with conditional (ternary) operators:");
                clipboardContent.AppendLine("    - Place the closing parenthesis of the TryParse method before the question mark");
                clipboardContent.AppendLine("    - CORRECT: @(DateTime.TryParse(value, out DateTime result) ? result.ToString(\"format\") : \"fallback\")");
                clipboardContent.AppendLine("    - INCORRECT: @(DateTime.TryParse(value, out DateTime result ? result.ToString(\"format\") : \"fallback\"))");
                clipboardContent.AppendLine("14. Be extra careful with balancing parentheses in Razor expressions, especially with nested conditions");

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

        /// <summary>
        /// Builds context from selected files and copies to clipboard based on user's checkbox selections
        /// </summary>
        private void BuildAndCopyContext(bool includeCss, bool includeTemplate, bool includeModel, bool includeJson)
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

                // Check if required files exist based on user selections
                bool filesExist = true;
                
                if (includeModel && !File.Exists(modelPath))
                {
                    MessageBox.Show($"Model file not found: {modelPath}", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    filesExist = false;
                }
                
                if (includeTemplate && !File.Exists(templatePath))
                {
                    MessageBox.Show($"Template file not found: {templatePath}", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    filesExist = false;
                }
                
                if (includeCss && !File.Exists(cssPath))
                {
                    MessageBox.Show($"CSS file not found: {cssPath}", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    filesExist = false;
                }

                if (!filesExist)
                {
                    return;
                }

                // Check if JSON reference data exists (don't fail if it doesn't)
                string jsonDataContent = "// No reference JSON data found";
                if (includeJson && File.Exists(jsonDataPath))
                {
                    jsonDataContent = File.ReadAllText(jsonDataPath);
                }
                else if (includeJson)
                {
                    Debug.WriteLine($"Warning: JSON reference data file not found: {jsonDataPath}");
                }

                // Read the content of all selected files
                string modelContent = includeModel ? File.ReadAllText(modelPath) : "";
                string templateContent = includeTemplate ? File.ReadAllText(templatePath) : "";
                string cssContent = includeCss ? File.ReadAllText(cssPath) : "";

                // Create the clipboard content
                StringBuilder clipboardContent = new StringBuilder();
                
                // Add a header to explain the context
                clipboardContent.AppendLine("# LLM Context Builder Output");
                clipboardContent.AppendLine("The following files are provided as context for working with this assessment template:");
                clipboardContent.AppendLine();
                
                if (includeModel)
                {
                    clipboardContent.AppendLine("## MODEL FILE: " + Path.GetFileName(modelPath));
                    clipboardContent.AppendLine("```csharp");
                    clipboardContent.AppendLine(modelContent);
                    clipboardContent.AppendLine("```");
                    clipboardContent.AppendLine();
                }
                
                if (includeCss)
                {
                    clipboardContent.AppendLine("## GLOBAL CSS: globalStyles.css");
                    clipboardContent.AppendLine("```css");
                    clipboardContent.AppendLine(cssContent);
                    clipboardContent.AppendLine("```");
                    clipboardContent.AppendLine();
                }
                
                if (includeTemplate)
                {
                    clipboardContent.AppendLine("## TEMPLATE: " + Path.GetFileName(templatePath));
                    clipboardContent.AppendLine("```html");
                    clipboardContent.AppendLine(templateContent);
                    clipboardContent.AppendLine("```");
                    clipboardContent.AppendLine();
                }
                
                if (includeJson && File.Exists(jsonDataPath))
                {
                    clipboardContent.AppendLine("## REFERENCE JSON DATA: " + Path.GetFileName(jsonDataPath));
                    clipboardContent.AppendLine("```json");
                    clipboardContent.AppendLine(jsonDataContent);
                    clipboardContent.AppendLine("```");
                    clipboardContent.AppendLine();
                }

                // Copy to clipboard
                Clipboard.SetText(clipboardContent.ToString());
                
                MessageBox.Show("Selected files copied to clipboard successfully!", "Context Builder Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}\n\n{ex.StackTrace}", 
                    "Context Builder Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads the saved state of a checkbox from the registry
        /// </summary>
        /// <param name="keyName">Registry key name</param>
        /// <param name="defaultValue">Default value if the registry key doesn't exist</param>
        /// <returns>The saved checkbox state or the default value</returns>
        private bool LoadCheckboxState(string keyName, bool defaultValue)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key != null)
                    {
                        var value = key.GetValue(keyName);
                        if (value != null)
                        {
                            return Convert.ToBoolean(value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading checkbox state for {keyName}: {ex.Message}");
            }
            return defaultValue; // Return default value if not found or error
        }

        /// <summary>
        /// Saves the state of a checkbox to the registry
        /// </summary>
        /// <param name="keyName">Registry key name</param>
        /// <param name="isChecked">Current state of the checkbox</param>
        private void SaveCheckboxState(string keyName, bool isChecked)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    if (key != null)
                    {
                        key.SetValue(keyName, isChecked ? 1 : 0, RegistryValueKind.DWord);
                        Debug.WriteLine($"Saved {keyName} state: {isChecked}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving checkbox state for {keyName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the saved selected tab index from the registry
        /// </summary>
        /// <returns>The saved tab index or 0 if not found</returns>
        private int LoadSelectedTabIndex()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key != null)
                    {
                        var value = key.GetValue(SelectedTabKey);
                        if (value != null)
                        {
                            int tabIndex = Convert.ToInt32(value);
                            // Ensure the index is valid (in case the number of tabs changes in the future)
                            if (tabIndex >= 0 && tabIndex < _tabControl.TabCount)
                            {
                                return tabIndex;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading selected tab index: {ex.Message}");
            }
            return 0; // Default to first tab if not found or error
        }

        /// <summary>
        /// Saves the selected tab index to the registry
        /// </summary>
        /// <param name="tabIndex">The index of the currently selected tab</param>
        private void SaveSelectedTabIndex(int tabIndex)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    if (key != null)
                    {
                        key.SetValue(SelectedTabKey, tabIndex, RegistryValueKind.DWord);
                        Debug.WriteLine($"Saved selected tab index: {tabIndex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving selected tab index: {ex.Message}");
            }
        }
    }
}
