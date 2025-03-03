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
        // Classes to represent the prompt structure from the JSON file
        private class PromptCategory
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            
            [JsonPropertyName("prompts")]
            public List<PromptItem> Prompts { get; set; }
        }
        
        private class PromptItem
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; }
        }
        
        private class PromptBuilderJson
        {
            [JsonPropertyName("categories")]
            public List<PromptCategory> Categories { get; set; }
        }
        
        private readonly MainForm _parentForm;
        private TabControl _tabControl;
        private TabPage _jsonViewTab;
        private TabPage _powerToolsTab;  // New tab for Power Tools
        private Label _instructionLabel;  // Label for general instructions
        private ToolTip _toolTip;  // ToolTip for displaying hover information
        private JsonChecklistControl _jsonChecklistControl;
        private object _currentData;
        
        // Checkbox controls for the context builder
        private CheckBox _cssCheckBox;
        private CheckBox _templateCheckBox;
        private CheckBox _modelCheckBox;
        private CheckBox _jsonCheckBox;
        
        // Prompt Builder UI components
        private ComboBox _promptCategoryComboBox;
        private ListBox _promptListBox;
        private string _selectedPrompt = "";
        private Label _promptBuilderLabel;
        private TextBox _promptPreviewTextBox;
        
        // Registry keys for saving preferences
        private const string RegistryPath = @"Software\ITextGUIDesigner\SecondaryForm";
        private const string CssCheckboxKey = "CssCheckboxEnabled";
        private const string TemplateCheckboxKey = "TemplateCheckboxEnabled";
        private const string ModelCheckboxKey = "ModelCheckboxEnabled";
        private const string JsonCheckboxKey = "JsonCheckboxEnabled";
        private const string SelectedTabKey = "SelectedTabIndex";
        private const string SelectedPromptCategoryKey = "SelectedPromptCategory";
        private const string SelectedPromptKey = "SelectedPrompt";

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
            
            // Create a heading for the LLM Context Builder section
            Label llmContextBuilderLabel = new Label
            {
                Text = "LLM Context Builder",
                Location = new Point(20, 20),
                Size = new Size(300, 24),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.DarkSlateBlue
            };
            
            // Description label for the new section
            Label descriptionLabel = new Label
            {
                Text = "Select items to include in the context for Large Language Models:",
                Location = new Point(20, 50),
                Size = new Size(450, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            
            // Create checkboxes for selecting content to copy
            _cssCheckBox = new CheckBox
            {
                Text = "Global Styles CSS",
                Location = new Point(30, 80),
                Size = new Size(200, 24),
                Checked = LoadCheckboxState(CssCheckboxKey, true) // Load saved state with default true
            };
            _toolTip.SetToolTip(_cssCheckBox, "Include globalStyles.css in the context");
            _cssCheckBox.CheckedChanged += (s, e) => SaveCheckboxState(CssCheckboxKey, _cssCheckBox.Checked);
            
            _templateCheckBox = new CheckBox
            {
                Text = "Current CSHTML Template",
                Location = new Point(30, 110),
                Size = new Size(200, 24),
                Checked = LoadCheckboxState(TemplateCheckboxKey, true) // Load saved state with default true
            };
            _toolTip.SetToolTip(_templateCheckBox, "Include the current CSHTML template in the context");
            _templateCheckBox.CheckedChanged += (s, e) => SaveCheckboxState(TemplateCheckboxKey, _templateCheckBox.Checked);
            
            _modelCheckBox = new CheckBox
            {
                Text = "Current Model Instance",
                Location = new Point(30, 140),
                Size = new Size(200, 24),
                Checked = LoadCheckboxState(ModelCheckboxKey, true) // Load saved state with default true
            };
            _toolTip.SetToolTip(_modelCheckBox, "Include the current model instance file in the context");
            _modelCheckBox.CheckedChanged += (s, e) => SaveCheckboxState(ModelCheckboxKey, _modelCheckBox.Checked);
            
            _jsonCheckBox = new CheckBox
            {
                Text = "Current JSON Data",
                Location = new Point(30, 170),
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
                Location = new Point(30, 210),
                BackColor = Color.FromArgb(230, 240, 255),
                ForeColor = Color.DarkBlue,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            copyToClipboardButton.Click += (s, e) => BuildAndCopyContext(_cssCheckBox.Checked, _templateCheckBox.Checked, _modelCheckBox.Checked, _jsonCheckBox.Checked);
            _toolTip.SetToolTip(copyToClipboardButton, "Build and copy the selected context items to the clipboard for use with LLMs");
            
            // Create a divider for the Prompt Builder section
            Panel promptDividerPanel = new Panel
            {
                Location = new Point(20, 270),
                Size = new Size(powerToolsPanel.Width - 40, 2),
                BackColor = Color.LightGray,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            
            // Create heading for the Prompt Builder section
            _promptBuilderLabel = new Label
            {
                Text = "Create a prompt",
                Location = new Point(20, 285),
                Size = new Size(300, 24),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.DarkSlateBlue
            };
            
            // Create the category ComboBox
            _promptCategoryComboBox = new ComboBox
            {
                Location = new Point(20, 315),
                Size = new Size(280, 24),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _promptCategoryComboBox.SelectedIndexChanged += PromptCategory_SelectedIndexChanged;
            
            // Create the prompt ListBox
            _promptListBox = new ListBox
            {
                Location = new Point(20, 345),
                Size = new Size(280, 100),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ScrollAlwaysVisible = true
            };
            _promptListBox.SelectedIndexChanged += PromptListBox_SelectedIndexChanged;
            
            // Add a label for the prompt preview
            Label promptPreviewLabel = new Label
            {
                Text = "Prompt Preview:",
                Location = new Point(20, 455),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            
            // Create a text box to preview the selected prompt
            _promptPreviewTextBox = new TextBox
            {
                Location = new Point(20, 480),
                Size = new Size(280, 80),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Create button to copy prompt to clipboard
            Button copyPromptButton = new Button
            {
                Text = "Copy Prompt to Clipboard",
                Size = new Size(200, 30),
                Location = new Point(20, 570),
                BackColor = Color.FromArgb(230, 240, 255),
                ForeColor = Color.DarkBlue,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            copyPromptButton.Click += (s, e) => 
            {
                if (!string.IsNullOrEmpty(_selectedPrompt))
                {
                    Clipboard.SetText(_selectedPrompt);
                    MessageBox.Show("Prompt copied to clipboard", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No prompt is selected", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            
            // Load prompts from the JSON file
            LoadPromptsFromJson();
            
            // Add controls to the panel
            powerToolsPanel.Controls.Add(llmContextBuilderLabel);
            powerToolsPanel.Controls.Add(descriptionLabel);
            powerToolsPanel.Controls.Add(_cssCheckBox);
            powerToolsPanel.Controls.Add(_templateCheckBox);
            powerToolsPanel.Controls.Add(_modelCheckBox);
            powerToolsPanel.Controls.Add(_jsonCheckBox);
            powerToolsPanel.Controls.Add(copyToClipboardButton);
            powerToolsPanel.Controls.Add(promptDividerPanel);
            powerToolsPanel.Controls.Add(_promptBuilderLabel);
            powerToolsPanel.Controls.Add(_promptCategoryComboBox);
            powerToolsPanel.Controls.Add(_promptListBox);
            powerToolsPanel.Controls.Add(promptPreviewLabel);
            powerToolsPanel.Controls.Add(_promptPreviewTextBox);
            powerToolsPanel.Controls.Add(copyPromptButton);
            
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
            Size = new Size(800, 630);
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
        /// Builds context from selected files and copies to clipboard based on user's checkbox selections
        /// </summary>
        private void BuildAndCopyContext(bool includeCss, bool includeTemplate, bool includeModel, bool includeJsonData)
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
                if (includeJsonData && File.Exists(jsonDataPath))
                {
                    jsonDataContent = File.ReadAllText(jsonDataPath);
                }
                else if (includeJsonData)
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
                
                // Add the selected prompt if one exists
                if (!string.IsNullOrEmpty(_selectedPrompt))
                {
                    string categoryName = _promptCategoryComboBox.SelectedItem?.ToString() ?? "Unknown Category";
                    string promptName = _promptListBox.SelectedItem?.ToString() ?? "Unknown Prompt";
                    
                    clipboardContent.AppendLine("## SELECTED PROMPT");
                    clipboardContent.AppendLine($"Category: {categoryName}");
                    clipboardContent.AppendLine($"Prompt: {promptName}");
                    clipboardContent.AppendLine("```");
                    clipboardContent.AppendLine(_selectedPrompt);
                    clipboardContent.AppendLine("```");
                    clipboardContent.AppendLine();
                }
                
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
                
                if (includeJsonData && File.Exists(jsonDataPath))
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
        private bool LoadCheckboxState(string keyName, bool defaultValue = true)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key != null)
                    {
                        object value = key.GetValue(keyName);
                        if (value != null)
                        {
                            return Convert.ToBoolean(value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading checkbox state: {ex.Message}");
            }
            
            return defaultValue;
        }
        
        /// <summary>
        /// Loads prompts from the promptBuilder.json file and populates the UI components
        /// </summary>
        private void LoadPromptsFromJson()
        {
            try
            {
                // Use ProjectDirectoryService to find the JSON file
                var projectDirService = new ProjectDirectoryService();
                string promptBuilderPath = projectDirService.GetFilePath("PersistentDataJSON/promptBuilder.json");
                
                if (!File.Exists(promptBuilderPath))
                {
                    MessageBox.Show("Prompt builder JSON file not found at: " + promptBuilderPath, 
                        "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Read and deserialize the JSON file
                string jsonContent = File.ReadAllText(promptBuilderPath);
                var promptBuilder = JsonSerializer.Deserialize<PromptBuilderJson>(jsonContent);
                
                if (promptBuilder == null || promptBuilder.Categories == null || promptBuilder.Categories.Count == 0)
                {
                    MessageBox.Show("No prompt categories found in the promptBuilder.json file.", 
                        "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Clear existing items
                _promptCategoryComboBox.Items.Clear();
                _promptListBox.Items.Clear();
                
                // Add categories to the ComboBox
                foreach (var category in promptBuilder.Categories)
                {
                    _promptCategoryComboBox.Items.Add(category.Name);
                }
                
                // Load the previously selected category, if any
                string savedCategory = LoadSelectedCategory();
                if (!string.IsNullOrEmpty(savedCategory) && _promptCategoryComboBox.Items.Contains(savedCategory))
                {
                    _promptCategoryComboBox.SelectedItem = savedCategory;
                }
                else if (_promptCategoryComboBox.Items.Count > 0)
                {
                    // Default to the first item
                    _promptCategoryComboBox.SelectedIndex = 0;
                }
                
                // Now the SelectedIndexChanged event will populate the ListBox
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading prompts: {ex.Message}", 
                    "Prompt Builder Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        /// <summary>
        /// Event handler for when a new category is selected in the ComboBox
        /// </summary>
        private void PromptCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Clear the existing prompts
                _promptListBox.Items.Clear();
                _selectedPrompt = "";
                
                if (_promptCategoryComboBox.SelectedItem == null)
                    return;
                    
                string selectedCategory = _promptCategoryComboBox.SelectedItem.ToString();
                
                // Save the selected category
                SaveSelectedCategory(selectedCategory);
                
                // Load the JSON file again to get the prompts for this category
                var projectDirService = new ProjectDirectoryService();
                string promptBuilderPath = projectDirService.GetFilePath("PersistentDataJSON/promptBuilder.json");
                string jsonContent = File.ReadAllText(promptBuilderPath);
                var promptBuilder = JsonSerializer.Deserialize<PromptBuilderJson>(jsonContent);
                
                if (promptBuilder == null || promptBuilder.Categories == null)
                    return;
                    
                // Find the selected category
                var category = promptBuilder.Categories.FirstOrDefault(c => c.Name == selectedCategory);
                if (category == null || category.Prompts == null)
                    return;
                    
                // Add the prompts to the ListBox
                foreach (var prompt in category.Prompts)
                {
                    _promptListBox.Items.Add(prompt.Name);
                }
                
                // Load the previously selected prompt for this category, if any
                string savedPrompt = LoadSelectedPrompt();
                if (!string.IsNullOrEmpty(savedPrompt) && _promptListBox.Items.Contains(savedPrompt))
                {
                    _promptListBox.SelectedItem = savedPrompt;
                }
                else if (_promptListBox.Items.Count > 0)
                {
                    // Default to the first item
                    _promptListBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading prompts for category: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Event handler for when a new prompt is selected in the ListBox
        /// </summary>
        private void PromptListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (_promptListBox.SelectedItem == null || _promptCategoryComboBox.SelectedItem == null)
                    return;
                    
                string selectedCategory = _promptCategoryComboBox.SelectedItem.ToString();
                string selectedPromptName = _promptListBox.SelectedItem.ToString();
                
                // Save the selected prompt
                SaveSelectedPrompt(selectedPromptName);
                
                // Get the prompt text from the JSON file
                var projectDirService = new ProjectDirectoryService();
                string promptBuilderPath = projectDirService.GetFilePath("PersistentDataJSON/promptBuilder.json");
                string jsonContent = File.ReadAllText(promptBuilderPath);
                var promptBuilder = JsonSerializer.Deserialize<PromptBuilderJson>(jsonContent);
                
                if (promptBuilder == null || promptBuilder.Categories == null)
                    return;
                    
                var category = promptBuilder.Categories.FirstOrDefault(c => c.Name == selectedCategory);
                if (category == null || category.Prompts == null)
                    return;
                    
                var prompt = category.Prompts.FirstOrDefault(p => p.Name == selectedPromptName);
                if (prompt == null)
                    return;
                    
                // Store the selected prompt text
                _selectedPrompt = prompt.Prompt;
                
                // Update the preview text box
                _promptPreviewTextBox.Text = _selectedPrompt;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting selected prompt: {ex.Message}");
                _selectedPrompt = "";
                _promptPreviewTextBox.Text = "";
            }
        }
        
        /// <summary>
        /// Save the selected prompt category to the registry
        /// </summary>
        private void SaveSelectedCategory(string category)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    key.SetValue(SelectedPromptCategoryKey, category);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving selected category: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Load the selected prompt category from the registry
        /// </summary>
        private string LoadSelectedCategory()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key != null)
                    {
                        return key.GetValue(SelectedPromptCategoryKey, "").ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading selected category: {ex.Message}");
            }
            
            return "";
        }
        
        /// <summary>
        /// Save the selected prompt to the registry
        /// </summary>
        private void SaveSelectedPrompt(string prompt)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    key.SetValue(SelectedPromptKey, prompt);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving selected prompt: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Load the selected prompt from the registry
        /// </summary>
        private string LoadSelectedPrompt()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key != null)
                    {
                        return key.GetValue(SelectedPromptKey, "").ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading selected prompt: {ex.Message}");
            }
            
            return "";
        }
    }
}
