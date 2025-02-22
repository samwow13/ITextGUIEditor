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
        private FlowLayoutPanel _flowPanel;
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
            _flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            Controls.Add(_flowPanel);
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
                
                _flowPanel.Controls.Clear();
                AddJsonElement(doc.RootElement, "");
            }
            else
            {
                _currentDocumentId = null;
                _checkStates.Clear();
                _flowPanel.Controls.Clear();
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
            if (value.ValueKind == JsonValueKind.Object || value.ValueKind == JsonValueKind.Array)
            {
                // Add a header for objects and arrays
                var header = new Label
                {
                    Text = $"{new string(' ', indent * 4)}{name}:",
                    AutoSize = true,
                    Font = new Font(Font.FontFamily, Font.Size, FontStyle.Bold),
                    Margin = new Padding(3)
                };
                _flowPanel.Controls.Add(header);
                AddJsonElement(value, path, indent + 1);
            }
            else
            {
                AddLeafElement($"{name}: {value}", path, indent);
            }
        }

        private void AddLeafElement(string text, string path, int indent)
        {
            var panel = new Panel
            {
                AutoSize = true,
                Margin = new Padding(3),
                Padding = new Padding(indent * 20, 0, 0, 0)
            };

            var checkbox = new CheckBox
            {
                AutoSize = true,
                Text = text,
                Tag = path,
                Checked = _checkStates.ContainsKey(path) && _checkStates[path]
            };

            checkbox.CheckedChanged += (s, e) =>
            {
                var cb = (CheckBox)s;
                _checkStates[cb.Tag.ToString()] = cb.Checked;
                SaveProgress();
            };

            panel.Controls.Add(checkbox);
            _flowPanel.Controls.Add(panel);
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
