using System;
using System.Drawing;
using System.Windows.Forms;

namespace iTextDesignerWithGUI.Forms
{
    public class CustomErrorForm : Form
    {
        private readonly string _errorMessage;

        public CustomErrorForm(string errorMessage)
        {
            _errorMessage = errorMessage;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Set form properties
            this.Text = "🦄 Oopsie! Something went wrong! 🌈";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ColorTranslator.FromHtml("#FFE5F9"); // Light pink background

            // Create a panel for gradient background
            var gradientPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            gradientPanel.Paint += (s, e) =>
            {
                var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    gradientPanel.ClientRectangle,
                    ColorTranslator.FromHtml("#FFE5F9"), // Light pink
                    ColorTranslator.FromHtml("#E5F9FF"), // Light blue
                    45F);
                e.Graphics.FillRectangle(brush, gradientPanel.ClientRectangle);
            };
            this.Controls.Add(gradientPanel);

            // Create error icon label (using emoji)
            var iconLabel = new Label
            {
                Text = "🦄",
                Font = new Font("Segoe UI Emoji", 48),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            iconLabel.Location = new Point((this.Width - iconLabel.PreferredWidth) / 2, 20);
            gradientPanel.Controls.Add(iconLabel);

            // Create title label
            var titleLabel = new Label
            {
                Text = "Oops! We found a small hiccup!",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#FF69B4"), // Hot pink
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            titleLabel.Location = new Point((this.Width - titleLabel.PreferredWidth) / 2, 100);
            gradientPanel.Controls.Add(titleLabel);

            // Create error message textbox with scroll
            var errorTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Text = FormatErrorMessage(_errorMessage),
                Font = new Font("Consolas", 11),
                BackColor = Color.White,
                ForeColor = ColorTranslator.FromHtml("#4B0082"), // Indigo
                Location = new Point(20, 150),
                Size = new Size(this.Width - 60, 150)
            };
            gradientPanel.Controls.Add(errorTextBox);

            // Create OK button with rainbow gradient
            var okButton = new Button
            {
                Text = "Got it! 🌈",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorTranslator.FromHtml("#FF69B4"),
                ForeColor = Color.White
            };
            okButton.Location = new Point((this.Width - okButton.Width) / 2, 320);
            okButton.Click += (s, e) => this.Close();
            okButton.Paint += (s, e) =>
            {
                var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    okButton.ClientRectangle,
                    ColorTranslator.FromHtml("#FF69B4"),
                    ColorTranslator.FromHtml("#4B0082"),
                    0F);
                e.Graphics.FillRectangle(brush, okButton.ClientRectangle);
            };
            gradientPanel.Controls.Add(okButton);
        }

        private string FormatErrorMessage(string message)
        {
            // Add some fun formatting to the error message
            var formattedMessage = message.Replace("[", "\n[")  // Add newlines before file paths
                                        .Replace(") error", ")\n❌ Error")  // Add newlines and emoji before errors
                                        .Replace(") warning", ")\n⚠️ Warning");  // Add newlines and emoji before warnings

            return $"🎨 Here's what happened:\n\n{formattedMessage}\n\n✨ Don't worry! Just fix the syntax and try again! 🌟";
        }
    }
}
