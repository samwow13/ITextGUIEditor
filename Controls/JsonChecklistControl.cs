using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text.Json;
using System.Collections.Generic;
using iTextDesignerWithGUI.Models;

namespace iTextDesignerWithGUI.Controls
{
    /// <summary>
    /// A custom control that displays JSON data as a checklist with persistent storage
    /// </summary>
    public class JsonChecklistControl : Panel
    {
        private Dictionary<string, bool> _checkStates = new Dictionary<string, bool>();
        private TableLayoutPanel _mainTable;
        private ChecklistProgressManager _progressManager;
        private string _currentDocumentId;
        private string _currentDocumentType;
        private object _currentData;

        public JsonChecklistControl()
        {
            InitializeComponent();
            _progressManager = new ChecklistProgressManager(
                System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, 
                    "TempStorage"
                )
            );
        }

        private void InitializeComponent()
        {
            _mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                ColumnCount = 2,
                RowCount = 0,
                AutoSize = true,
                GrowStyle = TableLayoutPanelGrowStyle.AddRows,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = Color.White
            };

            // Set column styles
            _mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30F));  // Checkbox column
            _mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // Content column

            Controls.Add(_mainTable);
        }

        /// <summary>
        /// Updates the control with new JSON data and loads any saved progress
        /// </summary>
        public void UpdateData(object data, string documentType)
        {
            _currentData = data;
            _currentDocumentType = documentType;

            if (data != null)
            {
                // Generate a unique ID for this document
                _currentDocumentId = _progressManager.GenerateDocumentId(documentType, data);

                // Load any saved progress
                var progress = _progressManager.LoadProgress(_currentDocumentId);
                if (progress != null)
                {
                    _checkStates = progress.CheckStates;
                }
                else
                {
                    _checkStates.Clear();
                }

                // Convert data to JSON and parse it
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(data, options);
                using JsonDocument doc = JsonDocument.Parse(json);
                
                _mainTable.Controls.Clear();
                _mainTable.RowCount = 0;
                AddJsonElement(doc.RootElement, "");
            }
            else
            {
                _currentDocumentId = null;
                _checkStates.Clear();
                _mainTable.Controls.Clear();
                _mainTable.RowCount = 0;
            }
        }

        private void AddJsonElement(JsonElement element, string path, int indent = 0)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        string newPath = string.IsNullOrEmpty(path) ? property.Name : $"{path}.{property.Name}";
                        AddPropertyElement(property.Name, property.Value, newPath, indent);
                    }
                    break;

                case JsonValueKind.Array:
                    int index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        string newPath = $"{path}[{index}]";
                        if (item.ValueKind == JsonValueKind.Object || item.ValueKind == JsonValueKind.Array)
                        {
                            AddJsonElement(item, newPath, indent + 1);
                        }
                        else
                        {
                            AddLeafElement(item.ToString(), newPath, indent + 1);
                        }
                        index++;
                    }
                    break;

                default:
                    AddLeafElement(element.ToString(), path, indent);
                    break;
            }
        }

        private void AddPropertyElement(string name, JsonElement value, string path, int indent)
        {
            if (value.ValueKind == JsonValueKind.Object)
            {
                // Add the object header row
                AddContentRow($"\"{name}\": {{", path, indent, true);

                // Add the object's properties
                AddJsonElement(value, path, indent + 1);

                // Add closing brace
                AddContentRow("}", null, indent, false);
            }
            else if (value.ValueKind == JsonValueKind.Array)
            {
                // Add the array header row
                AddContentRow($"\"{name}\": [", path, indent, true);

                // Add the array items
                AddJsonElement(value, path, indent + 1);

                // Add closing bracket
                AddContentRow("]", null, indent, false);
            }
            else
            {
                // For primitive values, show them inline
                var content = value.ValueKind == JsonValueKind.String
                    ? $"\"{name}\": \"{value}\""
                    : $"\"{name}\": {value}";

                AddContentRow(content, path, indent, true, value.ValueKind);
            }
        }

        private void AddLeafElement(string text, string path, int indent)
        {
            AddContentRow(text, path, indent, true);
        }

        private void AddContentRow(string content, string path, int indent, bool addCheckbox, JsonValueKind? valueKind = null)
        {
            // Create new row
            _mainTable.RowCount++;
            int rowIndex = _mainTable.RowCount - 1;

            // Add checkbox if needed
            if (addCheckbox)
            {
                var checkbox = new CheckBox
                {
                    AutoSize = true,
                    Margin = new Padding(5, 3, 0, 0),
                    Tag = path,
                    Checked = _checkStates.ContainsKey(path) && _checkStates[path]
                };

                checkbox.CheckedChanged += (s, e) =>
                {
                    var cb = (CheckBox)s;
                    _checkStates[cb.Tag.ToString()] = cb.Checked;
                    SaveProgress();
                };

                _mainTable.Controls.Add(checkbox, 0, rowIndex);
            }
            else
            {
                // Add empty panel for spacing
                var emptyPanel = new Panel
                {
                    Height = 20,
                    Margin = new Padding(0)
                };
                _mainTable.Controls.Add(emptyPanel, 0, rowIndex);
            }

            // Add content
            var contentPanel = new Panel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(indent * 4, 0, 0, 0),
                Margin = new Padding(0),
                BackColor = Color.White
            };

            var label = new Label
            {
                Text = content,
                AutoSize = true,
                Font = new Font("Consolas", 9),
                ForeColor = valueKind.HasValue ? GetJsonValueColor(valueKind.Value) : Color.Black,
                Margin = new Padding(0),
                UseMnemonic = false
            };

            contentPanel.Controls.Add(label);
            _mainTable.Controls.Add(contentPanel, 1, rowIndex);
        }

        private Color GetJsonValueColor(JsonValueKind valueKind)
        {
            return valueKind switch
            {
                JsonValueKind.String => Color.FromArgb(34, 162, 201),    // Light blue for strings
                JsonValueKind.Number => Color.FromArgb(247, 140, 108),   // Orange for numbers
                JsonValueKind.True or JsonValueKind.False => Color.FromArgb(174, 129, 255),  // Purple for booleans
                JsonValueKind.Null => Color.FromArgb(239, 83, 80),       // Red for null
                _ => Color.FromArgb(152, 195, 121)                       // Green for property names
            };
        }

        private void SaveProgress()
        {
            if (_currentDocumentId != null)
            {
                var progress = new ChecklistProgress
                {
                    DocumentId = _currentDocumentId,
                    DocumentType = _currentDocumentType,
                    CheckStates = new Dictionary<string, bool>(_checkStates),
                    LastModified = DateTime.Now
                };
                _progressManager.SaveProgress(progress);
            }
        }

        /// <summary>
        /// Gets a dictionary of all items and their checked states
        /// </summary>
        public Dictionary<string, bool> GetCheckStates()
        {
            return new Dictionary<string, bool>(_checkStates);
        }
    }
}
