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
            // Form settings
            this.Text = "Select Assessment Type";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new System.Drawing.Size(400, 225);
            this.Padding = new Padding(20);
            this.BackColor = Color.White;

            // Create main container
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(10),
            };
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Add descriptive label
            var label = new Label
            {
                Text = "Please select the type of assessment:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };
            mainContainer.Controls.Add(label, 0, 0);

            // Create and configure ComboBox
            var comboBox = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Margin = new Padding(0, 0, 0, 20),
                Height = 30
            };

            // Add assessment types
            foreach (AssessmentType type in Enum.GetValues(typeof(AssessmentType)))
            {
                comboBox.Items.Add(type.ToString().SplitCamelCase());
            }
            comboBox.SelectedIndex = 0;
            mainContainer.Controls.Add(comboBox, 0, 1);

            // Create button panel
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 40,
                Margin = new Padding(0),
                AutoSize = true,
                Padding = new Padding(0)
            };

            // Button base style
            var buttonSize = new Size(100, 35);
            var buttonMargin = new Padding(5, 0, 5, 0);
            var buttonFont = new Font("Segoe UI", 9F, FontStyle.Regular);
            var buttonPadding = new Padding(10, 5, 10, 5);

            // Create and style buttons
            var okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Font = buttonFont,
                Padding = buttonPadding,
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = buttonSize,
                Margin = buttonMargin,
                Height = buttonSize.Height
            };
            okButton.FlatAppearance.BorderSize = 0;
            okButton.Click += (s, e) =>
            {
                SelectedType = (AssessmentType)Enum.Parse(typeof(AssessmentType), comboBox.SelectedItem.ToString().Replace(" ", ""));
                WasCancelled = false;
                this.Close();
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Font = buttonFont,
                Padding = buttonPadding,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Size = buttonSize,
                Margin = buttonMargin,
                Height = buttonSize.Height
            };
            cancelButton.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 212);
            cancelButton.FlatAppearance.BorderSize = 1;
            cancelButton.Click += (s, e) => this.Close();

            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(cancelButton);
            mainContainer.Controls.Add(buttonPanel, 0, 2);

            this.Controls.Add(mainContainer);
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }
    }
}
