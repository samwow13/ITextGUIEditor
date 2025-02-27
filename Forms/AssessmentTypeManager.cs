using System;
using System.Collections.Generic;
using System.Windows.Forms;
using iTextDesignerWithGUI.Models;
using iTextDesignerWithGUI.Services;

namespace iTextDesignerWithGUI.Forms
{
    /// <summary>
    /// Form for managing assessment types
    /// </summary>
    public partial class AssessmentTypeManagerForm : Form
    {
        private readonly AssessmentTypeManager _manager;
        private List<AssessmentTypeDefinition> _assessmentTypes;

        /// <summary>
        /// Initializes a new instance of the AssessmentTypeManagerForm class
        /// </summary>
        public AssessmentTypeManagerForm()
        {
            InitializeComponent();
            _manager = new AssessmentTypeManager();
        }

        /// <summary>
        /// Handles the form load event
        /// </summary>
        private async void AssessmentTypeManagerForm_Load(object sender, EventArgs e)
        {
            await LoadAssessmentTypesAsync();
        }

        /// <summary>
        /// Loads assessment types from the JSON file
        /// </summary>
        private async System.Threading.Tasks.Task LoadAssessmentTypesAsync()
        {
            try
            {
                _assessmentTypes = await _manager.GetAllAssessmentTypesAsync();
                listBoxTypes.Items.Clear();

                foreach (var type in _assessmentTypes)
                {
                    listBoxTypes.Items.Add($"{type.Name} ({type.DisplayName})");
                }

                // Enable/disable buttons based on selection
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading assessment types: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Updates the enabled state of buttons based on the current selection
        /// </summary>
        private void UpdateButtonStates()
        {
            bool hasSelection = listBoxTypes.SelectedIndex >= 0;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
        }

        /// <summary>
        /// Handles the add button click event
        /// </summary>
        private async void btnAdd_Click(object sender, EventArgs e)
        {
            using (var dialog = new AssessmentTypeEditorForm())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        bool success = await _manager.AddAssessmentTypeAsync(dialog.TypeName, dialog.DisplayName);
                        if (success)
                        {
                            await LoadAssessmentTypesAsync();
                        }
                        else
                        {
                            MessageBox.Show("Failed to add assessment type. It may already exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error adding assessment type: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the edit button click event
        /// </summary>
        private async void btnEdit_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBoxTypes.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < _assessmentTypes.Count)
            {
                var selectedType = _assessmentTypes[selectedIndex];
                
                using (var dialog = new AssessmentTypeEditorForm(selectedType.Name, selectedType.DisplayName))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            // Remove the old type
                            await _manager.RemoveAssessmentTypeAsync(selectedType.Name);
                            
                            // Add the new type
                            bool success = await _manager.AddAssessmentTypeAsync(dialog.TypeName, dialog.DisplayName);
                            if (success)
                            {
                                await LoadAssessmentTypesAsync();
                            }
                            else
                            {
                                MessageBox.Show("Failed to update assessment type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error updating assessment type: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the delete button click event
        /// </summary>
        private async void btnDelete_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBoxTypes.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < _assessmentTypes.Count)
            {
                var selectedType = _assessmentTypes[selectedIndex];
                
                if (MessageBox.Show($"Are you sure you want to delete the assessment type '{selectedType.Name}'?", 
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        bool success = await _manager.RemoveAssessmentTypeAsync(selectedType.Name);
                        if (success)
                        {
                            await LoadAssessmentTypesAsync();
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete assessment type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting assessment type: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the selection changed event
        /// </summary>
        private void listBoxTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        /// <summary>
        /// Handles the close button click event
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Required designer variable
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor
        /// </summary>
        private void InitializeComponent()
        {
            this.listBoxTypes = new System.Windows.Forms.ListBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // listBoxTypes
            // 
            this.listBoxTypes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxTypes.FormattingEnabled = true;
            this.listBoxTypes.Location = new System.Drawing.Point(12, 29);
            this.listBoxTypes.Name = "listBoxTypes";
            this.listBoxTypes.Size = new System.Drawing.Size(360, 212);
            this.listBoxTypes.TabIndex = 0;
            this.listBoxTypes.SelectedIndexChanged += new System.EventHandler(this.listBoxTypes_SelectedIndexChanged);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAdd.Location = new System.Drawing.Point(12, 247);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEdit.Enabled = false;
            this.btnEdit.Location = new System.Drawing.Point(93, 247);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(75, 23);
            this.btnEdit.TabIndex = 2;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDelete.Enabled = false;
            this.btnDelete.Location = new System.Drawing.Point(174, 247);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(297, 247);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Assessment Types:";
            // 
            // AssessmentTypeManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 282);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.listBoxTypes);
            this.MinimumSize = new System.Drawing.Size(400, 320);
            this.Name = "AssessmentTypeManagerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Assessment Type Manager";
            this.Load += new System.EventHandler(this.AssessmentTypeManagerForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxTypes;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label1;
    }
}
