using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.IO;
using iTextDesignerWithGUI.Forms;

namespace iTextDesignerWithGUI.Forms
{
    public class CustomErrorForm : Form
    {
        private readonly string _errorMessage;
        private readonly string _templatePath; // Added field to store template path
        private System.Windows.Forms.Timer _animationTimer;
        private float _unicornX = -50;
        private float _catX;
        private float _unicornY = 50;
        private float _catY = 50;
        private bool _battleStarted = false;
        private int _animationFrame = 0;
        private readonly Random _random = new Random();
        private Panel _animationPanel;
        private readonly string[] _battleSounds = new[] { "✨ Sparkle!", "💫 Whoosh!", "⚡ Zap!", "🌈 Rainbow!", "😺 Meow!", "🦄 Neigh!" };
        private string _currentBattleSound = "";
        private TextBox _errorTextBox; // Added field to access the error text box

        public CustomErrorForm(string errorMessage) : this(errorMessage, string.Empty)
        {
            // Calls the main constructor with an empty template path
        }

        /// <summary>
        /// Creates a new error form with information about the template that caused the error
        /// </summary>
        /// <param name="errorMessage">The error message to display</param>
        /// <param name="templatePath">Path to the template file that caused the error</param>
        public CustomErrorForm(string errorMessage, string templatePath)
        {
            _errorMessage = errorMessage;
            _templatePath = templatePath;
            InitializeComponents();
            SetupAnimation();
        }

        private void SetupAnimation()
        {
            _catX = this.Width + 50; // Start cat from right side
            _animationTimer = new System.Windows.Forms.Timer();
            _animationTimer.Interval = 50; // 50ms for smooth animation
            _animationTimer.Tick += AnimationTimer_Tick;
            _animationTimer.Start();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (!_battleStarted)
            {
                // Move characters to center
                _unicornX += 5;
                _catX -= 5;
                
                if (_unicornX >= 100 && _catX <= this.Width - 150)
                {
                    _battleStarted = true;
                }
            }
            else
            {
                // Battle animation
                _animationFrame++;
                
                // Every 10 frames, generate a new battle sound
                if (_animationFrame % 10 == 0)
                {
                    _currentBattleSound = _battleSounds[_random.Next(_battleSounds.Length)];
                }

                // Bounce characters
                _unicornY = 50 + (float)(Math.Sin(_animationFrame * 0.2) * 10);
                _catY = 50 + (float)(Math.Cos(_animationFrame * 0.2) * 10);
                
                // Random horizontal movement during battle
                _unicornX += (float)(_random.NextDouble() * 4 - 2);
                _catX += (float)(_random.NextDouble() * 4 - 2);
                
                // Keep within bounds
                _unicornX = Math.Max(50, Math.Min(_unicornX, this.Width - 200));
                _catX = Math.Max(_unicornX + 50, Math.Min(_catX, this.Width - 100));
            }

            _animationPanel.Invalidate();
        }

        private void InitializeComponents()
        {
            // Set form properties
            this.Text = "🦄 vs 😺 Error Battle! 🌈";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ColorTranslator.FromHtml("#FFE5F9");

            // Create main table layout
            var mainTableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(10),
                BackColor = Color.Transparent
            };

            // Set row styles
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150)); // Animation panel
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Error message
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // Buttons

            this.Controls.Add(mainTableLayout);

            // Create animation panel
            _animationPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            _animationPanel.Paint += AnimationPanel_Paint;
            mainTableLayout.Controls.Add(_animationPanel, 0, 0);

            // Create gradient panel for error message
            var gradientPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.Transparent
            };
            gradientPanel.Paint += (s, e) =>
            {
                var brush = new LinearGradientBrush(
                    gradientPanel.ClientRectangle,
                    ColorTranslator.FromHtml("#FFE5F9"),
                    ColorTranslator.FromHtml("#E5F9FF"),
                    45F);
                e.Graphics.FillRectangle(brush, gradientPanel.ClientRectangle);
            };
            mainTableLayout.Controls.Add(gradientPanel, 0, 1);

            // Create error message textbox with scroll
            _errorTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Text = FormatErrorMessage(_errorMessage),
                Font = new Font("Consolas", 12, FontStyle.Regular),
                BackColor = Color.White,
                ForeColor = ColorTranslator.FromHtml("#4B0082"),
                Dock = DockStyle.Fill,
                WordWrap = true
            };
            gradientPanel.Controls.Add(_errorTextBox);

            // Create button panel
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            mainTableLayout.Controls.Add(buttonPanel, 0, 2);

            // Create OK button with rainbow gradient
            var okButton = new Button
            {
                Text = "Got it! 🌈",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(150, 45),
                FlatStyle = FlatStyle.Standard,
                BackColor = ColorTranslator.FromHtml("#FF69B4"),
                ForeColor = Color.White
            };
            
            // Create Copy button with galaxy gradient
            var copyButton = new Button
            {
                Text = "Copy to Clipboard 📋",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(200, 45),
                FlatStyle = FlatStyle.Standard,
                BackColor = ColorTranslator.FromHtml("#4B0082"),
                ForeColor = Color.White
            };
            
            // Position the buttons side by side
            okButton.Location = new Point((buttonPanel.Width / 2) - okButton.Width - 10, (buttonPanel.Height - okButton.Height) / 2);
            copyButton.Location = new Point((buttonPanel.Width / 2) + 10, (buttonPanel.Height - copyButton.Height) / 2);
            
            okButton.Click += (s, e) => 
            {
                _animationTimer.Stop();
                
                // Reset the reload state in MainForm to ensure error handling is reset
                MainForm.ResetReloadState();
                
                this.Close();
            };
            
            copyButton.Click += (s, e) => CopyErrorDetailsToClipboard();

            // Custom paint for OK button gradient
            okButton.Paint += (s, e) =>
            {
                var buttonRect = new Rectangle(0, 0, okButton.Width, okButton.Height);
                using (var brush = new LinearGradientBrush(
                    buttonRect,
                    ColorTranslator.FromHtml("#FF69B4"),
                    ColorTranslator.FromHtml("#4B0082"),
                    0F))
                {
                    e.Graphics.FillRectangle(brush, buttonRect);
                }

                // Draw text with shadow for better visibility
                var textSize = e.Graphics.MeasureString(okButton.Text, okButton.Font);
                var textX = (okButton.Width - textSize.Width) / 2;
                var textY = (okButton.Height - textSize.Height) / 2;
                
                // Draw shadow
                using (var shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                {
                    e.Graphics.DrawString(okButton.Text, okButton.Font, shadowBrush, textX + 1, textY + 1);
                }
                // Draw text
                e.Graphics.DrawString(okButton.Text, okButton.Font, Brushes.White, textX, textY);
            };
            
            // Custom paint for Copy button gradient
            copyButton.Paint += (s, e) =>
            {
                var buttonRect = new Rectangle(0, 0, copyButton.Width, copyButton.Height);
                using (var brush = new LinearGradientBrush(
                    buttonRect,
                    ColorTranslator.FromHtml("#4B0082"),
                    ColorTranslator.FromHtml("#00BFFF"),
                    0F))
                {
                    e.Graphics.FillRectangle(brush, buttonRect);
                }

                // Draw text with shadow for better visibility
                var textSize = e.Graphics.MeasureString(copyButton.Text, copyButton.Font);
                var textX = (copyButton.Width - textSize.Width) / 2;
                var textY = (copyButton.Height - textSize.Height) / 2;
                
                // Draw shadow
                using (var shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                {
                    e.Graphics.DrawString(copyButton.Text, copyButton.Font, shadowBrush, textX + 1, textY + 1);
                }
                // Draw text
                e.Graphics.DrawString(copyButton.Text, copyButton.Font, Brushes.White, textX, textY);
            };

            // Add buttons to panel
            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(copyButton);
            
            // Center the buttons in their panel
            buttonPanel.Resize += (s, e) =>
            {
                okButton.Location = new Point((buttonPanel.Width / 2) - okButton.Width - 10, (buttonPanel.Height - okButton.Height) / 2);
                copyButton.Location = new Point((buttonPanel.Width / 2) + 10, (buttonPanel.Height - copyButton.Height) / 2);
            };

            this.FormClosing += (s, e) =>
            {
                _animationTimer?.Stop();
            };
        }

        private void AnimationPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Draw battle sound effect
            if (_battleStarted && !string.IsNullOrEmpty(_currentBattleSound))
            {
                using (var font = new Font("Segoe UI", 14, FontStyle.Bold))
                {
                    var x = (_unicornX + _catX) / 2;
                    var y = Math.Min(_unicornY, _catY) - 30;
                    e.Graphics.DrawString(_currentBattleSound, font, Brushes.HotPink, x, y);
                }
            }

            // Draw unicorn
            DrawUnicorn(e.Graphics, _unicornX, _unicornY);
            
            // Draw cat
            DrawCat(e.Graphics, _catX, _catY);

            // Draw sparkles during battle
            if (_battleStarted)
            {
                DrawSparkles(e.Graphics);
            }
        }

        private void DrawSparkles(Graphics g)
        {
            for (int i = 0; i < 5; i++)
            {
                var x = _random.Next((int)_unicornX, (int)_catX);
                var y = _random.Next(20, 100);
                var size = _random.Next(5, 15);
                using (var brush = new SolidBrush(Color.FromArgb(_random.Next(128, 255), 255, 182, 193)))
                {
                    g.FillEllipse(brush, x, y, size, size);
                }
            }
        }

        private void DrawUnicorn(Graphics g, float x, float y)
        {
            // Draw unicorn body (simplified cute version)
            using (var brush = new SolidBrush(Color.White))
            {
                g.FillEllipse(brush, x, y, 60, 40); // Body
            }
            using (var brush = new SolidBrush(ColorTranslator.FromHtml("#FF69B4")))
            {
                g.FillPolygon(brush, new PointF[] {
                    new PointF(x + 50, y + 5),
                    new PointF(x + 60, y - 15),
                    new PointF(x + 70, y + 5)
                }); // Horn
            }
        }

        private void DrawCat(Graphics g, float x, float y)
        {
            // Draw cat (simplified cute version)
            using (var brush = new SolidBrush(Color.Gray))
            {
                g.FillEllipse(brush, x, y, 50, 40); // Body
                
                // Ears (triangles)
                g.FillPolygon(brush, new PointF[] {
                    new PointF(x + 10, y),
                    new PointF(x + 20, y - 15),
                    new PointF(x + 30, y)
                });
                g.FillPolygon(brush, new PointF[] {
                    new PointF(x + 30, y),
                    new PointF(x + 40, y - 15),
                    new PointF(x + 50, y)
                });
            }
        }

        /// <summary>
        /// Formats the error message with additional information about the template path
        /// </summary>
        private string FormatErrorMessage(string errorMessage)
        {
            StringBuilder formattedMessage = new StringBuilder();
            
            // Add template information if it exists
            if (!string.IsNullOrEmpty(_templatePath))
            {
                formattedMessage.AppendLine($"🔍 Template: {_templatePath}");
                formattedMessage.AppendLine();
            }
            
            // Format error text to be more readable
            var formattedErrorText = errorMessage.Replace("[", "\n[")
                                      .Replace(") error", ")\n❌ Error")
                                      .Replace(") warning", ")\n⚠️ Warning");
            
            formattedMessage.AppendLine("📝 Error Details:");
            formattedMessage.AppendLine(formattedErrorText);
            
            formattedMessage.AppendLine("\n✨ Don't worry! The unicorn and kitty are working on it! 🌟");
            
            return formattedMessage.ToString();
        }

        private void CopyErrorDetailsToClipboard()
        {
            try
            {
                // Create the clipboard content
                StringBuilder clipboardContent = new StringBuilder();
                
                // Add a header
                clipboardContent.AppendLine("# Error Details");
                clipboardContent.AppendLine("The following error occurred in the application:");
                clipboardContent.AppendLine();
                
                // Add specific LLM instructions
                clipboardContent.AppendLine("## INSTRUCTIONS FOR LLM");
                clipboardContent.AppendLine("Always ensure to re-write the entire cshtml file attached, only fixing the error found in the error message. If you don't think that's possible, then, and only then let the user know other changes might be necessary elsewhere in the code base");
                clipboardContent.AppendLine();
                
                // Add the error message
                clipboardContent.AppendLine("## ERROR MESSAGE");
                clipboardContent.AppendLine("```");
                clipboardContent.AppendLine(_errorMessage);
                clipboardContent.AppendLine("```");
                clipboardContent.AppendLine();
                
                // Add the template file content if available
                if (!string.IsNullOrEmpty(_templatePath) && File.Exists(_templatePath))
                {
                    try
                    {
                        string templateContent = File.ReadAllText(_templatePath);
                        string fileName = Path.GetFileName(_templatePath);
                        
                        clipboardContent.AppendLine($"## TEMPLATE FILE: {fileName}");
                        clipboardContent.AppendLine("```html");
                        clipboardContent.AppendLine(templateContent);
                        clipboardContent.AppendLine("```");
                        clipboardContent.AppendLine();
                    }
                    catch (Exception ex)
                    {
                        // If we can't read the template file, add an error message
                        clipboardContent.AppendLine("## TEMPLATE FILE");
                        clipboardContent.AppendLine($"Could not read template file: {_templatePath}");
                        clipboardContent.AppendLine($"Error: {ex.Message}");
                        clipboardContent.AppendLine();
                    }
                }
                
                // Copy to clipboard
                Clipboard.SetText(clipboardContent.ToString());
                
                MessageBox.Show("Error details and template file copied to clipboard successfully!", "Copy Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while copying: {ex.Message}", 
                    "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Flashes the form to get the user's attention
        /// </summary>
        public void Flash()
        {
            // Store the original background color
            var originalBackColor = this.BackColor;
            
            // Create a timer for the flashing effect
            var flashTimer = new System.Windows.Forms.Timer();
            flashTimer.Interval = 100; // 100ms intervals
            
            int flashCount = 0;
            const int MaxFlashes = 5;
            
            // Toggle between original color and attention color
            flashTimer.Tick += (s, e) => 
            {
                flashCount++;
                if (flashCount > MaxFlashes * 2)
                {
                    // Stop after the specified number of flashes
                    flashTimer.Stop();
                    flashTimer.Dispose();
                    this.BackColor = originalBackColor;
                    return;
                }
                
                // Toggle the background color
                this.BackColor = flashCount % 2 == 0 
                    ? originalBackColor 
                    : Color.FromArgb(255, 255, 200); // Light yellow flash
            };
            
            // Start the flash timer
            flashTimer.Start();
        }
    }
}
