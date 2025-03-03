using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;
using iTextDesignerWithGUI.Services;

namespace iTextDesignerWithGUI.Forms
{
    /// <summary>
    /// A form for creating new prompts and editing existing prompts
    /// </summary>
    public class PromptEditorForm : Form
    {
        // Classes to represent the prompt structure from the JSON file
        public class PromptCategory
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            
            [JsonPropertyName("prompts")]
            public List<PromptItem> Prompts { get; set; } = new List<PromptItem>();
        }
        
        public class PromptItem
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; }
        }
        
        public class PromptBuilderJson
        {
            [JsonPropertyName("categories")]
            public List<PromptCategory> Categories { get; set; } = new List<PromptCategory>();
        }
        
        private readonly string _promptBuilderJsonPath;
        private PromptBuilderJson _promptData;
        
        // UI Components
        private ComboBox _categoryComboBox;
        private TextBox _newCategoryTextBox;
        private ComboBox _promptComboBox;
        private TextBox _promptNameTextBox;
        private TextBox _promptContentTextBox;
        private Button _saveButton;
        private Button _cancelButton;
        
        // Mode of operation
        private bool _isEditMode = false;
        private string _originalCategoryName = string.Empty;
        private string _originalPromptName = string.Empty;
        
        /// <summary>
        /// Constructor for creating a new prompt
        /// </summary>
        public PromptEditorForm()
        {
            var projectDirService = new ProjectDirectoryService();
            _promptBuilderJsonPath = projectDirService.GetFilePath("PersistentDataJSON/promptBuilder.json");
            
            InitializeComponent();
            LoadPromptData();
            
            this.Text = "Create New Prompt";
            _isEditMode = false;
        }
        
        /// <summary>
        /// Constructor for editing an existing prompt
        /// </summary>
        /// <param name="categoryName">Category of the prompt to edit</param>
        /// <param name="promptName">Name of the prompt to edit</param>
        public PromptEditorForm(string categoryName, string promptName)
        {
            var projectDirService = new ProjectDirectoryService();
            _promptBuilderJsonPath = projectDirService.GetFilePath("PersistentDataJSON/promptBuilder.json");
            
            InitializeComponent();
            LoadPromptData();
            
            this.Text = "Edit Prompt";
            _isEditMode = true;
            _originalCategoryName = categoryName;
            _originalPromptName = promptName;
            
            // Select the category and prompt
            int categoryIndex = _categoryComboBox.Items.IndexOf(categoryName);
            if (categoryIndex >= 0)
            {
                _categoryComboBox.SelectedIndex = categoryIndex;
                _promptComboBox.Visible = true;
                
                // Add prompts from this category to the combobox
                _promptComboBox.Items.Clear();
                var category = _promptData.Categories.FirstOrDefault(c => c.Name == categoryName);
                if (category != null)
                {
                    foreach (var prompt in category.Prompts)
                    {
                        _promptComboBox.Items.Add(prompt.Name);
                    }
                    
                    // Select the prompt to edit
                    int promptIndex = _promptComboBox.Items.IndexOf(promptName);
                    if (promptIndex >= 0)
                    {
                        _promptComboBox.SelectedIndex = promptIndex;
                        
                        // Set the prompt name and content
                        var prompt = category.Prompts.FirstOrDefault(p => p.Name == promptName);
                        if (prompt != null)
                        {
                            _promptNameTextBox.Text = prompt.Name;
                            _promptContentTextBox.Text = prompt.Prompt;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Initializes the form components
        /// </summary>
        private void InitializeComponent()
        {
            // Form properties
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(600, 650);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Create labels
            Label categoryLabel = new Label
            {
                Text = "Category:",
                Location = new Point(20, 20),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            
            Label newCategoryLabel = new Label
            {
                Text = "or Create New Category:",
                Location = new Point(20, 70),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            
            Label promptNameLabel = new Label
            {
                Text = "Prompt Name:",
                Location = new Point(20, 120),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            
            Label promptContentLabel = new Label
            {
                Text = "Prompt Content:",
                Location = new Point(20, 170),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            
            // Create fields
            _categoryComboBox = new ComboBox
            {
                Location = new Point(20, 40),
                Size = new Size(350, 24),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _categoryComboBox.SelectedIndexChanged += CategoryComboBox_SelectedIndexChanged;
            
            _newCategoryTextBox = new TextBox
            {
                Location = new Point(20, 90),
                Size = new Size(350, 24),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            
            _promptComboBox = new ComboBox
            {
                Location = new Point(380, 40),
                Size = new Size(180, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Visible = false  // Initially hidden until a category is selected when in edit mode
            };
            _promptComboBox.SelectedIndexChanged += PromptComboBox_SelectedIndexChanged;
            
            _promptNameTextBox = new TextBox
            {
                Location = new Point(20, 140),
                Size = new Size(350, 24),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            
            _promptContentTextBox = new TextBox
            {
                Location = new Point(20, 190),
                Size = new Size(540, 360),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            
            // Create buttons
            _saveButton = new Button
            {
                Text = "Save",
                Location = new Point(380, 570),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(230, 240, 255),
                ForeColor = Color.DarkBlue,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _saveButton.Click += SaveButton_Click;
            
            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(490, 570),
                Size = new Size(80, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            _cancelButton.Click += (s, e) => this.Close();
            
            // Add controls to form
            this.Controls.Add(categoryLabel);
            this.Controls.Add(_categoryComboBox);
            this.Controls.Add(newCategoryLabel);
            this.Controls.Add(_newCategoryTextBox);
            this.Controls.Add(_promptComboBox);
            this.Controls.Add(promptNameLabel);
            this.Controls.Add(_promptNameTextBox);
            this.Controls.Add(promptContentLabel);
            this.Controls.Add(_promptContentTextBox);
            this.Controls.Add(_saveButton);
            this.Controls.Add(_cancelButton);
        }
        
        /// <summary>
        /// Loads prompt data from the JSON file
        /// </summary>
        private void LoadPromptData()
        {
            try
            {
                // Read the JSON file
                string jsonContent = File.ReadAllText(_promptBuilderJsonPath);
                _promptData = JsonSerializer.Deserialize<PromptBuilderJson>(jsonContent);
                
                // Populate the category dropdown
                _categoryComboBox.Items.Clear();
                foreach (var category in _promptData.Categories)
                {
                    _categoryComboBox.Items.Add(category.Name);
                }
                
                // Select the first item if not in edit mode
                if (!_isEditMode && _categoryComboBox.Items.Count > 0)
                {
                    _categoryComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading prompt data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Event handler for when a category is selected
        /// </summary>
        private void CategoryComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // When in edit mode and a category is selected, show the prompt dropdown
            if (_isEditMode)
            {
                _promptComboBox.Visible = true;
                _promptComboBox.Items.Clear();
                
                var categoryName = _categoryComboBox.SelectedItem as string;
                var category = _promptData.Categories.FirstOrDefault(c => c.Name == categoryName);
                
                if (category != null)
                {
                    foreach (var prompt in category.Prompts)
                    {
                        _promptComboBox.Items.Add(prompt.Name);
                    }
                    
                    // If the original prompt is in this category, select it
                    if (category.Name == _originalCategoryName)
                    {
                        var index = _promptComboBox.Items.IndexOf(_originalPromptName);
                        if (index >= 0)
                        {
                            _promptComboBox.SelectedIndex = index;
                        }
                        else if (_promptComboBox.Items.Count > 0)
                        {
                            _promptComboBox.SelectedIndex = 0;
                        }
                    }
                    else if (_promptComboBox.Items.Count > 0)
                    {
                        _promptComboBox.SelectedIndex = 0;
                    }
                }
            }
        }
        
        /// <summary>
        /// Event handler for when a prompt is selected
        /// </summary>
        private void PromptComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isEditMode && _promptComboBox.SelectedItem != null)
            {
                var categoryName = _categoryComboBox.SelectedItem as string;
                var promptName = _promptComboBox.SelectedItem as string;
                
                var category = _promptData.Categories.FirstOrDefault(c => c.Name == categoryName);
                if (category != null)
                {
                    var prompt = category.Prompts.FirstOrDefault(p => p.Name == promptName);
                    if (prompt != null)
                    {
                        _promptNameTextBox.Text = prompt.Name;
                        _promptContentTextBox.Text = prompt.Prompt;
                    }
                }
            }
        }
        
        /// <summary>
        /// Event handler for the Save button
        /// </summary>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(_promptNameTextBox.Text))
                {
                    MessageBox.Show("Prompt name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Determine which category to use
                string categoryName;
                if (!string.IsNullOrWhiteSpace(_newCategoryTextBox.Text))
                {
                    // Use the new category
                    categoryName = _newCategoryTextBox.Text.Trim();
                    
                    // Check if the category already exists
                    var existingCategory = _promptData.Categories.FirstOrDefault(c => c.Name == categoryName);
                    if (existingCategory == null)
                    {
                        // Create a new category
                        _promptData.Categories.Add(new PromptCategory
                        {
                            Name = categoryName,
                            Prompts = new List<PromptItem>()
                        });
                    }
                }
                else if (_categoryComboBox.SelectedItem != null)
                {
                    // Use the selected category
                    categoryName = _categoryComboBox.SelectedItem as string;
                }
                else
                {
                    MessageBox.Show("Please select a category or create a new one.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Get the category object
                var category = _promptData.Categories.FirstOrDefault(c => c.Name == categoryName);
                if (category == null)
                {
                    MessageBox.Show($"Category '{categoryName}' not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // Create or update the prompt
                string promptName = _promptNameTextBox.Text.Trim();
                string promptContent = _promptContentTextBox.Text;
                
                if (_isEditMode)
                {
                    // If editing an existing prompt, remove the original one first
                    var originalCategory = _promptData.Categories.FirstOrDefault(c => c.Name == _originalCategoryName);
                    if (originalCategory != null)
                    {
                        originalCategory.Prompts.RemoveAll(p => p.Name == _originalPromptName);
                        
                        // If the category is now empty and not the target category, remove it
                        if (originalCategory.Prompts.Count == 0 && originalCategory.Name != categoryName)
                        {
                            _promptData.Categories.Remove(originalCategory);
                        }
                    }
                }
                
                // Check if a prompt with the same name already exists in the target category
                var existingPrompt = category.Prompts.FirstOrDefault(p => p.Name == promptName);
                if (existingPrompt != null)
                {
                    // Update the existing prompt
                    existingPrompt.Prompt = promptContent;
                }
                else
                {
                    // Add a new prompt
                    category.Prompts.Add(new PromptItem
                    {
                        Name = promptName,
                        Prompt = promptContent
                    });
                }
                
                // Save the JSON file
                string jsonContent = JsonSerializer.Serialize(_promptData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_promptBuilderJsonPath, jsonContent);
                
                MessageBox.Show("Prompt saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving prompt: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
