using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using iTextDesignerWithGUI.Utilities;
using System.IO;

namespace iTextDesignerWithGUI.Forms
{
    /// <summary>
    /// Form that displays a progress bar while generating files
    /// </summary>
    public class FileGenerationProgressForm : Form
    {
        private ProgressBar progressBar;
        private Label statusLabel;
        private Label titleLabel;
        private Button closeButton;
        private System.Windows.Forms.Timer progressTimer;
        private System.Windows.Forms.Timer pulseTimer;
        private float progressValue = 0;
        private Label percentLabel;
        private Panel glowPanel;
        private int pulseValue = 0;
        private bool increasingPulse = true;
        private TableLayoutPanel stepsPanel;
        private Label[] stepLabels;
        private PictureBox iconPictureBox;

        /// <summary>
        /// Constructor for the file generation progress form
        /// </summary>
        public FileGenerationProgressForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize the form components
        /// </summary>
        private void InitializeComponent()
        {
            // Form settings
            this.Text = "Generating Files";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(700, 500); // Increase height to accommodate all elements
            this.Padding = new Padding(20);
            this.BackColor = Color.FromArgb(240, 240, 250);
            this.ControlBox = false;

            // Main content panel
            var contentPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 7,
                Padding = new Padding(25, 20, 25, 25) // Adjusted padding
            };

            // Configure row styles
            contentPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 18)); // Icon
            contentPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 12)); // Title
            contentPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 15)); // Progress bar
            contentPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 8));  // Percentage
            contentPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10)); // Status
            contentPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 22)); // Steps
            contentPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 15)); // Close button

            // Icon panel
            var iconPanel = new Panel { Dock = DockStyle.Fill };
            
            iconPictureBox = new PictureBox
            {
                Size = new Size(64, 64),
                SizeMode = PictureBoxSizeMode.Zoom,
                Anchor = AnchorStyles.None,
                Image = CreateDocumentIcon(64, 64)
            };
            
            iconPictureBox.Location = new Point(
                (iconPanel.Width - iconPictureBox.Width) / 2,
                (iconPanel.Height - iconPictureBox.Height) / 2
            );
            
            iconPanel.Controls.Add(iconPictureBox);
            contentPanel.Controls.Add(iconPanel, 0, 0);

            // Title label
            titleLabel = new Label
            {
                Text = "Generating Your Files",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                AutoSize = false,
                ForeColor = Color.FromArgb(50, 50, 100)
            };
            contentPanel.Controls.Add(titleLabel, 0, 1);

            // Glow panel (background for progress bar)
            glowPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(30, 10, 30, 5),
                BackColor = Color.FromArgb(235, 235, 245),
                BorderStyle = BorderStyle.None
            };
            
            glowPanel.Paint += (sender, e) => 
            {
                // Draw a subtle gradient in the background
                using (var brush = new LinearGradientBrush(
                    glowPanel.ClientRectangle,
                    Color.FromArgb(230, 240, 255),
                    Color.FromArgb(220, 225, 240),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, glowPanel.ClientRectangle);
                }
                
                // Draw a slight border
                using (var pen = new Pen(Color.FromArgb(200, 200, 230), 1))
                {
                    var rect = glowPanel.ClientRectangle;
                    rect.Width -= 1;
                    rect.Height -= 1;
                    e.Graphics.DrawRoundedRectangle(pen, rect, 6);
                }
            };

            // Progress bar
            progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Continuous,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Height = 30,
                Dock = DockStyle.Fill,
                Margin = new Padding(5)
            };
            
            // Custom drawing for the progress bar
            progressBar.SetStyle(ProgressBarStyle.Continuous);
            progressBar.BackColor = Color.FromArgb(235, 235, 245);
            
            glowPanel.Controls.Add(progressBar);
            contentPanel.Controls.Add(glowPanel, 0, 2);

            // Percentage label
            percentLabel = new Label
            {
                Text = "0%",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                AutoSize = false,
                ForeColor = Color.FromArgb(60, 60, 120)
            };
            contentPanel.Controls.Add(percentLabel, 0, 3);

            // Status label
            statusLabel = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                AutoSize = false,
                ForeColor = Color.FromArgb(60, 60, 120)
            };
            contentPanel.Controls.Add(statusLabel, 0, 4);

            // Steps panel
            stepsPanel = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 4,
                Dock = DockStyle.Fill,
                Margin = new Padding(40, 5, 40, 5) // Add some vertical margin
            };
            
            for (int i = 0; i < 4; i++)
            {
                stepsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            }
            
            // Step labels
            stepLabels = new Label[4];
            string[] stepTexts = new string[]
            {
                "✓ Preparing environment...",
                "⟳ Generating CSHTML template files...",
                "⟳ Creating model classes and data.cs files...",
                "⟳ Updating configuration and data.json files..."
            };
            
            for (int i = 0; i < 4; i++)
            {
                stepLabels[i] = new Label
                {
                    Text = stepTexts[i],
                    Font = new Font("Segoe UI", 11F, i == 0 ? FontStyle.Bold : FontStyle.Regular),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill,
                    AutoSize = false,
                    ForeColor = i == 0 ? Color.FromArgb(0, 120, 215) : Color.Gray
                };
                stepsPanel.Controls.Add(stepLabels[i], 0, i);
            }
            
            contentPanel.Controls.Add(stepsPanel, 0, 5);

            // Close button (hidden initially)
            closeButton = new Button
            {
                Text = "Close",
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                Padding = new Padding(15, 8, 15, 8),
                Size = new Size(180, 45), // Increased width for the text
                Anchor = AnchorStyles.None,
                FlatStyle = FlatStyle.Flat,
                Visible = false,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatAppearance = {
                    BorderColor = Color.FromArgb(0, 100, 200),
                    BorderSize = 1
                }
            };
            
            closeButton.Click += CloseButton_Click;
            
            // Create a panel to center the close button
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            buttonPanel.Controls.Add(closeButton);
            
            // Center the close button in the panel
            closeButton.Location = new Point(
                (buttonPanel.Width - closeButton.Width) / 2,
                (buttonPanel.Height - closeButton.Height) / 2
            );
            
            contentPanel.Controls.Add(buttonPanel, 0, 6);

            // Add the panel to the form
            this.Controls.Add(contentPanel);

            // Initialize the timers
            progressTimer = new System.Windows.Forms.Timer();
            progressTimer.Interval = 120; // Doubled from 60 to slow down progress
            progressTimer.Tick += ProgressTimer_Tick;
            
            pulseTimer = new System.Windows.Forms.Timer();
            pulseTimer.Interval = 50; // Fast for smooth pulsing
            pulseTimer.Tick += PulseTimer_Tick;

            // Handle resize event to recenter elements
            this.Resize += (sender, e) => 
            {
                closeButton.Location = new Point(
                    (buttonPanel.Width - closeButton.Width) / 2,
                    (buttonPanel.Height - closeButton.Height) / 2
                );
                
                iconPictureBox.Location = new Point(
                    (iconPanel.Width - iconPictureBox.Width) / 2,
                    (iconPanel.Height - iconPictureBox.Height) / 2
                );
            };
        }

        /// <summary>
        /// Creates a document icon for display on the form
        /// </summary>
        private Image CreateDocumentIcon(int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                
                // Create document shape
                var docRect = new Rectangle(width/6, height/10, width*2/3, height*4/5);
                var docPath = new System.Drawing.Drawing2D.GraphicsPath();
                
                // Document shape with folded corner
                docPath.AddLine(docRect.Left, docRect.Top, docRect.Right - width/5, docRect.Top);
                docPath.AddLine(docRect.Right, docRect.Top + height/5, docRect.Right, docRect.Bottom);
                docPath.AddLine(docRect.Right, docRect.Bottom, docRect.Left, docRect.Bottom);
                docPath.AddLine(docRect.Left, docRect.Bottom, docRect.Left, docRect.Top);
                
                // Fill the document
                using (var docBrush = new LinearGradientBrush(
                    docRect, Color.White, Color.FromArgb(240, 240, 250), LinearGradientMode.Vertical))
                {
                    g.FillPath(docBrush, docPath);
                }
                
                // Draw document outline
                using (var docPen = new Pen(Color.FromArgb(100, 110, 150), 2))
                {
                    g.DrawPath(docPen, docPath);
                }
                
                // Draw folded corner
                var cornerPath = new System.Drawing.Drawing2D.GraphicsPath();
                cornerPath.AddLine(docRect.Right - width/5, docRect.Top, 
                                  docRect.Right - width/5, docRect.Top + height/5);
                cornerPath.AddLine(docRect.Right - width/5, docRect.Top + height/5,
                                  docRect.Right, docRect.Top + height/5);
                
                using (var cornerPen = new Pen(Color.FromArgb(100, 110, 150), 2))
                {
                    g.DrawPath(cornerPen, cornerPath);
                }
                
                // Draw lines for text
                using (var linePen = new Pen(Color.FromArgb(180, 190, 210), 1.5f))
                {
                    int lineY = docRect.Top + height/3;
                    int lineSpacing = height/9;
                    int lineLength = width/2;
                    int lineX = docRect.Left + width/10;
                    
                    for (int i = 0; i < 3; i++)
                    {
                        g.DrawLine(linePen, lineX, lineY + i * lineSpacing, 
                                  lineX + lineLength, lineY + i * lineSpacing);
                    }
                }
            }
            return bitmap;
        }
        
        /// <summary>
        /// Pulse effect for the glow panel
        /// </summary>
        private void PulseTimer_Tick(object sender, EventArgs e)
        {
            if (increasingPulse)
            {
                pulseValue += 2;
                if (pulseValue >= 30) increasingPulse = false;
            }
            else
            {
                pulseValue -= 2;
                if (pulseValue <= 0) increasingPulse = true;
            }
            
            // Force the glowPanel to redraw with new alpha value
            glowPanel.Invalidate();
            
            // Rotate the icon slightly for a subtle animation
            iconPictureBox.Image = RotateImage(CreateDocumentIcon(64, 64), pulseValue / 3.0f);
        }

        /// <summary>
        /// Rotates an image by the specified angle
        /// </summary>
        private Image RotateImage(Image image, float angle)
        {
            Bitmap rotatedBmp = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(rotatedBmp))
            {
                g.TranslateTransform(image.Width / 2f, image.Height / 2f);
                g.RotateTransform(angle);
                g.TranslateTransform(-image.Width / 2f, -image.Height / 2f);
                g.DrawImage(image, new Point(0, 0));
            }
            return rotatedBmp;
        }

        /// <summary>
        /// Starts the generation process animation
        /// </summary>
        public void StartProgress(Action generationAction)
        {
            // Start the timers
            progressTimer.Start();
            pulseTimer.Start();
            
            // Run the actual generation in a background task
            Task.Run(() =>
            {
                try
                {
                    // Execute the file generation
                    generationAction?.Invoke();
                    
                    // When complete, make sure progress is 100%
                    this.Invoke((MethodInvoker)delegate
                    {
                        progressValue = 100;
                        progressBar.Value = (int)progressValue;
                        percentLabel.Text = $"{(int)progressValue}%";
                        statusLabel.Text = "Files generated successfully. Please restart the application.";
                        progressTimer.Stop();
                        pulseTimer.Stop();
                        UpdateStepLabels(4); // Mark all steps complete
                        ShowCloseButton();
                    });
                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        progressValue = 100;
                        progressBar.Value = (int)progressValue;
                        percentLabel.Text = "Error";
                        statusLabel.Text = $"Error: {ex.Message}";
                        progressTimer.Stop();
                        pulseTimer.Stop();
                        ShowCloseButton();
                    });
                }
            });
        }

        /// <summary>
        /// Update the progress bar on each timer tick
        /// </summary>
        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            // Update progress
            if (progressValue < 100)
            {
                // Increment progress to complete in about 12-14 seconds
                progressValue += 0.5f;
                if (progressValue > 100) progressValue = 100;
                
                progressBar.Value = (int)progressValue;
                percentLabel.Text = $"{(int)progressValue}%";
                
                // Update status text and step labels based on progress
                if (progressValue < 25.0f)
                {
                    statusLabel.Text = "Preparing environment...";
                    UpdateStepLabels(1);
                }
                else if (progressValue < 50.0f)
                {
                    statusLabel.Text = "Generating CSHTML template files...";
                    UpdateStepLabels(2);
                }
                else if (progressValue < 75.0f)
                {
                    statusLabel.Text = "Creating model classes and data.cs files...";
                    UpdateStepLabels(3);
                }
                else if (progressValue < 100.0f)
                {
                    statusLabel.Text = "Updating configuration and data.json files...";
                }
            }
            else
            {
                progressTimer.Stop();
            }
        }

        /// <summary>
        /// Updates the step labels to show the current progress
        /// </summary>
        private void UpdateStepLabels(int completedSteps)
        {
            for (int i = 0; i < stepLabels.Length; i++)
            {
                if (i < completedSteps)
                {
                    // Completed step
                    stepLabels[i].Text = $"✓ {stepLabels[i].Text.Substring(1).Trim()}";
                    stepLabels[i].Font = new Font(stepLabels[i].Font, FontStyle.Bold);
                    stepLabels[i].ForeColor = Color.FromArgb(0, 120, 215);
                }
                else if (i == completedSteps)
                {
                    // Current step
                    stepLabels[i].Text = $"⟳ {stepLabels[i].Text.Substring(1).Trim()}";
                    stepLabels[i].Font = new Font(stepLabels[i].Font, FontStyle.Bold);
                    stepLabels[i].ForeColor = Color.FromArgb(50, 50, 120);
                }
                else
                {
                    // Pending step
                    stepLabels[i].Text = $"⟳ {stepLabels[i].Text.Substring(1).Trim()}";
                    stepLabels[i].Font = new Font(stepLabels[i].Font, FontStyle.Regular);
                    stepLabels[i].ForeColor = Color.Gray;
                }
            }
        }

        /// <summary>
        /// Show the close button once processing is complete
        /// </summary>
        private void ShowCloseButton()
        {
            closeButton.Visible = true;
            closeButton.Text = "Close and Restart";
            closeButton.Size = new Size(180, 45); // Ensure size is updated even after creation
            
            // Add a slight delay to ensure the size change takes effect
            this.BeginInvoke(new Action(() => {
                closeButton.Location = new Point(
                    (closeButton.Parent.Width - closeButton.Width) / 2,
                    (closeButton.Parent.Height - closeButton.Height) / 2
                );
            }));
        }

        /// <summary>
        /// Close button click handler
        /// </summary>
        private void CloseButton_Click(object sender, EventArgs e)
        {
            try 
            {
                // Get the current project directory
                string projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\.."));
                
                // Save the current window position
                var mainForm = Application.OpenForms[0]; // Get the main form (index 0)
                string positionFile = Path.Combine(Path.GetTempPath(), "AppPosition.txt");
                File.WriteAllText(positionFile, $"{mainForm.Location.X},{mainForm.Location.Y},{mainForm.Width},{mainForm.Height}");
                
                // Create a batch file that will rebuild and restart the application
                string batchFilePath = Path.Combine(Path.GetTempPath(), "RestartApplication.bat");
                
                // Write commands to the batch file
                // Wait 1 second for the current process to close, then rebuild and run the application silently
                string batchContent = 
                    "@echo off\r\n" +
                    "timeout /t 1 /nobreak >nul\r\n" +
                    $"cd /d \"{projectDirectory}\"\r\n" +
                    "dotnet build >nul 2>&1\r\n" +
                    "if %ERRORLEVEL% == 0 (\r\n" +
                    "    start /b \"\" dotnet run --no-build\r\n" +
                    ") else (\r\n" +
                    "    echo Build failed during restart! >buildError.log\r\n" +
                    ")\r\n";
                
                File.WriteAllText(batchFilePath, batchContent);
                
                // Start the batch file in a new process
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c \"{batchFilePath}\"";
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                
                // Exit the current application
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restarting application: {ex.Message}", "Restart Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // If we can't restart, just exit
                Application.Exit();
            }
        }
    }
}
