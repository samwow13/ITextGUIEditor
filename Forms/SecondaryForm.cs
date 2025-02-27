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

            // JSON View Tab
            _jsonViewTab = new TabPage("JSON View");
            _jsonChecklistControl = new JsonChecklistControl
            {
                Dock = DockStyle.Fill
            };
            _jsonViewTab.Controls.Add(_jsonChecklistControl);

            // Add tabs to tab control
            _tabControl.TabPages.Add(_jsonViewTab);

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
    }
}
