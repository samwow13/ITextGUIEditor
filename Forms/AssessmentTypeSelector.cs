using System;
using System.Drawing;
using System.Windows.Forms;
using iTextDesignerWithGUI.Models;

namespace iTextDesignerWithGUI.Forms
{
    public partial class AssessmentTypeSelector : Form
    {
        public AssessmentType SelectedType { get; private set; }
        public bool WasCancelled { get; private set; } = true;

        public AssessmentTypeSelector()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Select Assessment Type";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new System.Drawing.Size(300, 150);

            var comboBox = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(10)
            };

            // Add assessment types
            foreach (AssessmentType type in Enum.GetValues(typeof(AssessmentType)))
            {
                comboBox.Items.Add(type);
            }
            comboBox.SelectedIndex = 0;

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 40,
                Padding = new Padding(5)
            };

            var okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK
            };
            okButton.Click += (s, e) =>
            {
                SelectedType = (AssessmentType)comboBox.SelectedItem;
                WasCancelled = false;
                this.Close();
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel
            };
            cancelButton.Click += (s, e) => this.Close();

            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Controls.Add(okButton);

            this.Controls.Add(buttonPanel);
            this.Controls.Add(comboBox);
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }
    }
}
