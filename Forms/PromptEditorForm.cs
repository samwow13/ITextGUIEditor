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
        private TextBox _promptNameTextBox;
        private TextBox _promptContentTextBox;
        private Button _saveButton;
        private Button _cancelButton;
        private Button _moveUpButton;
        private Button _moveDownButton;
        private Label _editModeIndicatorLabel;
        private ComboBox _promptComboBox;

        // Mode of operation
        private bool _isEditMode = false;
        private bool _isDuplicateMode = false;
        private string _originalCategoryName = string.Empty;
        private string _originalPromptName = string.Empty;

        // For returning edited prompt info to the caller
        public string EditedCategoryName { get; private set; }
        public string EditedPromptName { get; private set; }

        /// <summary>
        /// Constructor for creating a new prompt
        /// </summary>
        public PromptEditorForm()
        {
            var projectDirService = new ProjectDirectoryService();
            _promptBuilderJsonPath = projectDirService.GetFilePath(
                "PersistentDataJSON/promptBuilder.json"
            );

            InitializeComponent();
            LoadPromptData();

            this.Text = "Create New Prompt";
            _isEditMode = false;
            _editModeIndicatorLabel.Visible = false;
            _moveUpButton.Visible = false;
            _moveDownButton.Visible = false;

            // Select the first category if available
            if (_categoryComboBox.Items.Count > 0)
            {
                _categoryComboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Constructor for editing an existing prompt
        /// </summary>
        /// <param name="categoryName">Category of the prompt to edit</param>
        /// <param name="promptName">Name of the prompt to edit</param>
        public PromptEditorForm(string categoryName, string promptName)
            : this(categoryName, promptName, false) { }

        /// <summary>
        /// Constructor for editing or duplicating an existing prompt
        /// </summary>
        /// <param name="categoryName">Category of the prompt to edit</param>
        /// <param name="promptName">Name of the prompt to edit</param>
        /// <param name="isDuplicate">True if duplicating, false if editing</param>
        public PromptEditorForm(string categoryName, string promptName, bool isDuplicate)
        {
            var projectDirService = new ProjectDirectoryService();
            _promptBuilderJsonPath = projectDirService.GetFilePath(
                "PersistentDataJSON/promptBuilder.json"
            );

            InitializeComponent();
            LoadPromptData();

            _isDuplicateMode = isDuplicate;
            _isEditMode = !isDuplicate; // If duplicating, it's not technically editing
            _originalCategoryName = categoryName;
            _originalPromptName = promptName;

            // Set the title based on mode
            this.Text = isDuplicate ? "Duplicate Prompt" : "Edit Prompt";

            // Show edit mode indicator and reordering buttons (only for edit mode, not duplicate)
            _editModeIndicatorLabel.Visible = true;
            _editModeIndicatorLabel.Text = isDuplicate
                ? $"Duplicating: {promptName}"
                : $"Editing: {promptName}";
            _editModeIndicatorLabel.ForeColor = isDuplicate ? Color.DarkGreen : Color.DarkBlue;

            _moveUpButton.Visible = !isDuplicate; // Only show reordering in edit mode
            _moveDownButton.Visible = !isDuplicate; // Only show reordering in edit mode

            if (!isDuplicate)
            {
                _moveUpButton.Click += MovePromptUp_Click;
                _moveDownButton.Click += MovePromptDown_Click;
            }

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
            this.Size = new Size(750, 680); // Increase form height to add more space
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create labels
            Label categoryLabel = new Label
            {
                Text = "Category:",
                Location = new Point(20, 20),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            };

            Label promptLabel = new Label
            {
                Text = "Select Existing Prompt:",
                Location = new Point(380, 20),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Visible = _isEditMode || _isDuplicateMode, // Only show in edit or duplicate mode
            };

            Label promptNameLabel = new Label
            {
                Text = "Prompt Name:",
                Location = new Point(20, 70),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            };

            Label promptContentLabel = new Label
            {
                Text = "Prompt Content:",
                Location = new Point(20, 120),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            };

            // Create fields
            _categoryComboBox = new ComboBox
            {
                Location = new Point(20, 40),
                Size = new Size(350, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            };
            _categoryComboBox.SelectedIndexChanged += CategoryComboBox_SelectedIndexChanged;

            _promptComboBox = new ComboBox
            {
                Location = new Point(380, 40),
                Size = new Size(350, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Visible = _isEditMode || _isDuplicateMode, // Only show in edit or duplicate mode
            };
            _promptComboBox.SelectedIndexChanged += PromptComboBox_SelectedIndexChanged;

            _promptNameTextBox = new TextBox
            {
                Location = new Point(20, 90),
                Size = new Size(710, 24),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            };

            _promptContentTextBox = new TextBox
            {
                Location = new Point(20, 140),
                Size = new Size(710, 400),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            };

            // Create buttons
            _saveButton = new Button
            {
                Text = "Save",
                Location = new Point(560, 570), // Move buttons down to add more space
                Size = new Size(80, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            };
            _saveButton.Click += SaveButton_Click;

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(650, 570), // Move buttons down to add more space
                Size = new Size(80, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            };
            _cancelButton.Click += (s, e) => this.Close();

            _moveUpButton = new Button
            {
                Text = "Move Up",
                Location = new Point(560, 10), // Moved to a more visible position
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            };

            _moveDownButton = new Button
            {
                Text = "Move Down",
                Location = new Point(650, 10), // Moved to a more visible position
                Size = new Size(90, 25),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            };

            _editModeIndicatorLabel = new Label
            {
                Text = "Edit Mode",
                Location = new Point(20, 590), // Move indicator down to match buttons
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Visible = false,
            };

            // Add controls to form
            this.Controls.Add(categoryLabel);
            this.Controls.Add(_categoryComboBox);
            this.Controls.Add(promptLabel);
            this.Controls.Add(_promptComboBox);
            this.Controls.Add(promptNameLabel);
            this.Controls.Add(_promptNameTextBox);
            this.Controls.Add(promptContentLabel);
            this.Controls.Add(_promptContentTextBox);
            this.Controls.Add(_saveButton);
            this.Controls.Add(_cancelButton);
            this.Controls.Add(_moveUpButton);
            this.Controls.Add(_moveDownButton);
            this.Controls.Add(_editModeIndicatorLabel);
        }

        /// <summary>
        /// Loads prompt data from the JSON file
        /// </summary>
        private void LoadPromptData()
        {
            try
            {
                // Read and deserialize the JSON file
                string jsonContent = File.ReadAllText(_promptBuilderJsonPath);
                _promptData = JsonSerializer.Deserialize<PromptBuilderJson>(jsonContent);

                // Clear the ComboBox
                _categoryComboBox.Items.Clear();
                _promptComboBox.Items.Clear();

                // Populate the category ComboBox
                if (_promptData != null && _promptData.Categories != null)
                {
                    foreach (var category in _promptData.Categories)
                    {
                        _categoryComboBox.Items.Add(category.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading prompt data: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
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
                    MessageBox.Show(
                        "Prompt name cannot be empty.",
                        "Validation Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                if (_categoryComboBox.SelectedItem == null)
                {
                    MessageBox.Show(
                        "Please select a category.",
                        "Validation Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // Get the selected category
                string categoryName = _categoryComboBox.SelectedItem as string;

                // Get the category object
                var category = _promptData.Categories.FirstOrDefault(c => c.Name == categoryName);
                if (category == null)
                {
                    MessageBox.Show(
                        $"Category '{categoryName}' not found.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                // Create or update the prompt
                string promptName = _promptNameTextBox.Text.Trim();
                string promptContent = _promptContentTextBox.Text;

                // When duplicating, make sure to append " (Copy)" if name hasn't changed
                if (_isDuplicateMode && promptName == _originalPromptName)
                {
                    promptName += " (Copy)";
                    _promptNameTextBox.Text = promptName;
                }

                // Check for name conflict within the target category
                if (
                    (_isEditMode || _isDuplicateMode)
                    && categoryName == _originalCategoryName
                    && promptName != _originalPromptName
                    && category.Prompts.Any(p => p.Name == promptName)
                )
                {
                    var result = MessageBox.Show(
                        $"A prompt named '{promptName}' already exists in this category. Would you like to overwrite it?",
                        "Prompt Already Exists",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result != DialogResult.Yes)
                    {
                        return;
                    }
                }

                if (_isEditMode && !_isDuplicateMode)
                {
                    // If editing an existing prompt, remove the original one first
                    var originalCategory = _promptData.Categories.FirstOrDefault(c =>
                        c.Name == _originalCategoryName
                    );
                    if (originalCategory != null)
                    {
                        originalCategory.Prompts.RemoveAll(p => p.Name == _originalPromptName);

                        // If the category is now empty and not the target category, remove it
                        if (
                            originalCategory.Prompts.Count == 0
                            && originalCategory.Name != categoryName
                        )
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
                    category.Prompts.Add(
                        new PromptItem { Name = promptName, Prompt = promptContent }
                    );
                }

                // Save the JSON file
                string jsonContent = JsonSerializer.Serialize(
                    _promptData,
                    new JsonSerializerOptions { WriteIndented = true }
                );
                File.WriteAllText(_promptBuilderJsonPath, jsonContent);

                // Show a success message including the mode (create, edit, duplicate)
                string modeText = _isEditMode
                    ? "edited"
                    : (_isDuplicateMode ? "duplicated" : "created");
                MessageBox.Show(
                    $"Prompt {modeText} successfully.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                // Store edited prompt info
                EditedCategoryName = categoryName;
                EditedPromptName = promptName;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving prompt: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Move the current prompt up in the category order
        /// </summary>
        private void MovePromptUp_Click(object sender, EventArgs e)
        {
            if (_promptComboBox.SelectedIndex > 0)
            {
                try
                {
                    // Get the current prompt and category
                    string categoryName = _categoryComboBox.SelectedItem.ToString();
                    string promptName = _promptComboBox.SelectedItem.ToString();

                    // Load the JSON data
                    string jsonContent = File.ReadAllText(_promptBuilderJsonPath);
                    var promptData = JsonSerializer.Deserialize<PromptBuilderJson>(jsonContent);

                    // Get the current category
                    var category = promptData.Categories.FirstOrDefault(c =>
                        c.Name == categoryName
                    );
                    if (category == null)
                        return;

                    // Find the prompt position
                    int currentIndex = category.Prompts.FindIndex(p => p.Name == promptName);
                    if (currentIndex <= 0)
                        return; // Already at the top

                    // Swap with the previous prompt
                    var prompt = category.Prompts[currentIndex];
                    category.Prompts.RemoveAt(currentIndex);
                    category.Prompts.Insert(currentIndex - 1, prompt);

                    // Save the JSON
                    string updatedJson = JsonSerializer.Serialize(
                        promptData,
                        new JsonSerializerOptions { WriteIndented = true }
                    );
                    File.WriteAllText(_promptBuilderJsonPath, updatedJson);

                    // Update the UI
                    LoadPromptData();

                    // Reselect the current items
                    _categoryComboBox.SelectedItem = categoryName;
                    _promptComboBox.SelectedIndex = currentIndex - 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error moving prompt: {ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        /// <summary>
        /// Move the current prompt down in the category order
        /// </summary>
        private void MovePromptDown_Click(object sender, EventArgs e)
        {
            if (
                _promptComboBox.SelectedIndex >= 0
                && _promptComboBox.SelectedIndex < _promptComboBox.Items.Count - 1
            )
            {
                try
                {
                    // Get the current prompt and category
                    string categoryName = _categoryComboBox.SelectedItem.ToString();
                    string promptName = _promptComboBox.SelectedItem.ToString();

                    // Load the JSON data
                    string jsonContent = File.ReadAllText(_promptBuilderJsonPath);
                    var promptData = JsonSerializer.Deserialize<PromptBuilderJson>(jsonContent);

                    // Get the current category
                    var category = promptData.Categories.FirstOrDefault(c =>
                        c.Name == categoryName
                    );
                    if (category == null)
                        return;

                    // Find the prompt position
                    int currentIndex = category.Prompts.FindIndex(p => p.Name == promptName);
                    if (currentIndex < 0 || currentIndex >= category.Prompts.Count - 1)
                        return; // Already at the bottom

                    // Swap with the next prompt
                    var prompt = category.Prompts[currentIndex];
                    category.Prompts.RemoveAt(currentIndex);
                    category.Prompts.Insert(currentIndex + 1, prompt);

                    // Save the JSON
                    string updatedJson = JsonSerializer.Serialize(
                        promptData,
                        new JsonSerializerOptions { WriteIndented = true }
                    );
                    File.WriteAllText(_promptBuilderJsonPath, updatedJson);

                    // Update the UI
                    LoadPromptData();

                    // Reselect the current items
                    _categoryComboBox.SelectedItem = categoryName;
                    _promptComboBox.SelectedIndex = currentIndex + 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error moving prompt: {ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }
    }
}
