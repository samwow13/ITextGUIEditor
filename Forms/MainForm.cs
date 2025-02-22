using System;
using System.Collections.Generic;
using System.Windows.Forms;
using iTextDesignerWithGUI.Models;
using iTextDesignerWithGUI.Models.TestRazorDataModels;
using iTextDesignerWithGUI.Services;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace iTextDesignerWithGUI.Forms
{
    // Static class for string extensions
    public static class StringExtensions
    {
        // Helper method to add spaces between camel case words
        public static string SplitCamelCase(this string str)
        {
            return System.Text.RegularExpressions.Regex.Replace(
                str,
                "([A-Z])",
                " $1",
                System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }
    }

    public partial class MainForm : Form
    {
        // Add Win32 API declarations
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const int EDGE_WINDOW_OFFSET = 20; // Pixels to offset Edge window from MainForm

        private readonly IAssessment _assessment;
        private readonly JsonManager _jsonManager;
        private readonly PdfGeneratorService _pdfGenerator;
        private readonly TemplateWatcherService _templateWatcher;
        private List<object> _referenceData;
        private string _currentPdfPath;
        private AssessmentType _currentAssessmentType;
        private Label _statusLabel;
        private const string RegistryPath = @"Software\ITextGUIDesigner";
        private const string WindowPosXKey = "WindowPosX";
        private const string WindowPosYKey = "WindowPosY";
        private const string LastSelectedRowKey = "LastSelectedRow";
        private static bool _isReloading = false; // Add static flag to prevent multiple reloads

        private int? _lastSelectedRow;
        private Process _currentEdgeProcess;

        public MainForm(AssessmentType assessmentType = AssessmentType.OralCare)
        {
            InitializeComponent();
            _currentAssessmentType = assessmentType;
            _assessment = CreateAssessment(assessmentType);
            _jsonManager = new JsonManager(_assessment.JsonDataPath, assessmentType);
            _pdfGenerator = new PdfGeneratorService();
            
            // Initialize template watcher
            var templatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\Templates");
            _templateWatcher = new TemplateWatcherService(
                Path.GetFullPath(templatesPath), 
                () => ReloadTemplates_Click(this, EventArgs.Empty),
                this);
            _templateWatcher.StartWatching();
            
            // Update the window title to show the selected assessment type
            this.Text = $"iText Designer - {assessmentType.ToString().SplitCamelCase()}";
            
            // Load the saved window position
            LoadWindowPosition();
            
            InitializeAsync();
        }

        private void LoadWindowPosition()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key != null)
                    {
                        var x = (int?)key.GetValue(WindowPosXKey);
                        var y = (int?)key.GetValue(WindowPosYKey);

                        if (x.HasValue && y.HasValue)
                        {
                            // Ensure the window will be visible on the screen
                            var screen = Screen.FromPoint(new Point(x.Value, y.Value));
                            if (screen != null)
                            {
                                this.StartPosition = FormStartPosition.Manual;
                                this.Location = new Point(
                                    Math.Max(screen.WorkingArea.X, Math.Min(x.Value, screen.WorkingArea.Right - this.Width)),
                                    Math.Max(screen.WorkingArea.Y, Math.Min(y.Value, screen.WorkingArea.Bottom - this.Height))
                                );
                                return;
                            }
                        }
                    }
                }
                
                // If no saved position or invalid, center on screen
                this.StartPosition = FormStartPosition.CenterScreen;
            }
            catch
            {
                // If anything goes wrong, fall back to center screen
                this.StartPosition = FormStartPosition.CenterScreen;
            }
        }

        private void SaveWindowPosition()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    if (key != null)
                    {
                        key.SetValue(WindowPosXKey, this.Location.X, RegistryValueKind.DWord);
                        key.SetValue(WindowPosYKey, this.Location.Y, RegistryValueKind.DWord);
                    }
                }
            }
            catch
            {
                // Ignore any errors saving position
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Save the window position before closing
            SaveWindowPosition();

            if (!string.IsNullOrEmpty(_currentPdfPath) && File.Exists(_currentPdfPath))
            {
                try
                {
                    File.Delete(_currentPdfPath);
                }
                catch
                {
                    // Ignore deletion errors on closing
                }
            }
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _templateWatcher?.Dispose();
            }
            base.Dispose(disposing);
        }

        private IAssessment CreateAssessment(AssessmentType type)
        {
            return type switch
            {//ADD FORMS HERE
                AssessmentType.OralCare => new OralCareAssessment(),
                AssessmentType.RegisteredNurseTaskAndDelegation => new RegisteredNurseTaskDelegAssessment(),
                AssessmentType.TestRazorDataInstance => new TestRazorDataAssessment(),      
                _ => throw new ArgumentException($"Unsupported assessment type: {type}")
            };
        }

        private async void InitializeAsync()
        {
            try
            {
                var data = await _jsonManager.LoadReferenceDataAsync();
                _referenceData = data;
                PopulateDataGrid();
                InitializeDataGridView();

                // Add Back to Selection button
                Button backButton = new Button
                {
                    Text = "Back to Selection",
                    Dock = DockStyle.None,  // Changed from Bottom to None for side-by-side layout
                    Height = 40,
                    Width = 150,
                    Margin = new Padding(10),
                    Padding = new Padding(5),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = System.Drawing.Color.FromArgb(0, 123, 255),  // Bootstrap primary blue
                    ForeColor = System.Drawing.Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular),
                    Cursor = Cursors.Hand
                };
                backButton.Click += BackToSelection_Click;

                // Add Reload Templates button
                Button reloadButton = new Button
                {
                    Text = "Reload Templates",
                    Dock = DockStyle.None,
                    Height = 40,
                    Width = 150,
                    Margin = new Padding(10),
                    Padding = new Padding(5),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = System.Drawing.Color.FromArgb(40, 167, 69),  // Bootstrap success green
                    ForeColor = System.Drawing.Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular),
                    Cursor = Cursors.Hand
                };
                reloadButton.Click += ReloadTemplates_Click;

                // Create a panel for the buttons
                TableLayoutPanel buttonPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Bottom,
                    Height = 80, // Increased height to accommodate status label
                    Padding = new Padding(10),
                    ColumnCount = 3,
                    RowCount = 2 // Added another row for the status label
                };

                // Configure columns: Left spacing (auto) | Back button | Reload button
                buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Left spacing
                buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Back button
                buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Reload button

                // Configure rows
                buttonPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Status label row
                buttonPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Buttons row

                // Create status label
                _statusLabel = new Label
                {
                    Text = "",
                    AutoSize = true,
                    TextAlign = ContentAlignment.MiddleRight,
                    Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold),
                    ForeColor = System.Drawing.Color.FromArgb(40, 167, 69), // Bootstrap success green
                    Visible = false
                };

                // Add controls to the panel
                buttonPanel.Controls.Add(_statusLabel, 2, 0); // Status label in first row, right column
                buttonPanel.Controls.Add(backButton, 1, 1); // Back button in second row
                buttonPanel.Controls.Add(reloadButton, 2, 1); // Reload button in second row
                
                this.Controls.Add(buttonPanel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateDataGrid()
        {
            var data = new List<object>();
            
            foreach (var item in _referenceData)
            {
                string name;
                //ADD FORMS HERE
                if (item is OralCareDataInstance oralCare)
                {
                    name = oralCare.ChildInfo?.ChildName;
                }
                else if (item is RegisteredNurseTaskDelegDataInstance nurseTask)
                {
                    name = nurseTask.ChildInfo?.Name;
                }
                else if (item is TestRazorDataInstance testRazor)
                {
                    name = testRazor.User?.Name;
                }
                else
                {
                    continue;
                }

                data.Add(new { Name = name });
            }
            
            dataGridView.DataSource = data;
        }

        private void InitializeDataGridView()
        {
            // Clear any existing columns first
            dataGridView.Columns.Clear();
            
            // Basic grid settings
            this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView.MultiSelect = false;
            this.dataGridView.ReadOnly = true;
            this.dataGridView.AutoGenerateColumns = false;
            this.dataGridView.RowHeadersVisible = false;  // Remove the empty first column
            this.dataGridView.AllowUserToAddRows = false; // Remove empty row at bottom
            this.dataGridView.BorderStyle = BorderStyle.None;
            this.dataGridView.BackgroundColor = System.Drawing.SystemColors.Window;

            // Add Name column
            var nameColumn = new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                DataPropertyName = "Name",
                FillWeight = 70
            };
            dataGridView.Columns.Add(nameColumn);

            // Add Generate PDF button column
            var generatePdfColumn = new DataGridViewButtonColumn
            {
                HeaderText = "Actions",
                Text = "Generate PDF",
                Name = "GeneratePdf",
                UseColumnTextForButtonValue = true,
                FillWeight = 30,
                MinimumWidth = 120
            };
            dataGridView.Columns.Add(generatePdfColumn);

            dataGridView.CellContentClick += dataGridView_CellContentClick;
        }

        private async void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView.Columns["GeneratePdf"].Index)
            {
                _lastSelectedRow = e.RowIndex;
                
                // Save the selected row index to registry
                try
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                    {
                        if (key != null)
                        {
                            key.SetValue(LastSelectedRowKey, e.RowIndex, RegistryValueKind.DWord);
                        }
                    }
                }
                catch
                {
                    // Ignore errors saving row index
                }

                var item = _referenceData[e.RowIndex];
                try
                {
                    //ADD FORMS HERE
                    string name;
                    if (item is OralCareDataInstance oralCare)
                    {
                        name = oralCare.ChildInfo?.ChildName;
                    }
                    else if (item is RegisteredNurseTaskDelegDataInstance nurseTask)
                    {
                        name = nurseTask.ChildInfo?.Name;
                    }
                    else if (item is TestRazorDataInstance testRazor)
                    {
                        name = testRazor.User?.Name;
                    }
                    else
                    {
                        throw new InvalidOperationException("Unknown data type");
                    }

                    Debug.WriteLine($"Generating PDF for row {e.RowIndex}");
                    Debug.WriteLine($"Data for PDF: Name={name}");

                    var pdfBytes = _pdfGenerator.GeneratePdf(item, _assessment.TemplateFileName);
                    Debug.WriteLine($"PDF generated, size: {pdfBytes.Length} bytes");

                    // Use a meaningful filename
                    var fileName = $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                        .Replace(" ", "_")
                        .Replace("/", "_")
                        .Replace("\\", "_");
                    
                    _currentPdfPath = Path.Combine(Path.GetTempPath(), fileName);
                    File.WriteAllBytes(_currentPdfPath, pdfBytes);
                    Debug.WriteLine($"PDF saved to: {_currentPdfPath}");

                    // Start Edge and store the process
                    var startInfo = new ProcessStartInfo(_currentPdfPath)
                    {
                        UseShellExecute = true
                    };
                    _currentEdgeProcess = Process.Start(startInfo);

                    // Wait briefly for Edge window to open
                    await Task.Delay(1000);

                    // Find the Edge window by looking for the PDF filename in the title
                    var edgeWindow = FindWindow(null, fileName);
                    if (edgeWindow != IntPtr.Zero)
                    {
                        // Position Edge window to the right of the MainForm
                        var mainFormRight = this.Location.X + this.Width;
                        var mainFormTop = this.Location.Y;
                        SetWindowPos(edgeWindow, IntPtr.Zero, mainFormRight + EDGE_WINDOW_OFFSET, mainFormTop, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
                    }

                    // Show the blank form below and update it with the data
                    var blankForm = new SecondaryForm(this);
                    blankForm.Show();
                    blankForm.UpdateData(item); // Pass the data object to the form
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error generating PDF: {ex}");
                    MessageBox.Show($"Error generating PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private System.Windows.Forms.DataGridView dataGridView;

        private void InitializeComponent()
        {
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.SuspendLayout();
            
            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 250);
            this.Controls.Add(this.dataGridView);
            this.Name = "MainForm";
            this.ResumeLayout(false);
        }

        private void OnTemplateChanged(AssessmentType type)
        {
            var newAssessment = CreateAssessment(type);
            _referenceData = null;
            dataGridView.DataSource = null;
            InitializeAsync();
        }

        private void BackToSelection_Click(object sender, EventArgs e)
        {
            try
            {
                // Create a list to store forms to close to avoid modification during enumeration
                var formsToClose = new List<Form>();
                
                // Find all SecondaryForm instances
                foreach (Form form in Application.OpenForms)
                {
                    if (form is SecondaryForm)
                    {
                        formsToClose.Add(form);
                    }
                }
                
                // Close the forms
                foreach (var form in formsToClose)
                {
                    if (!form.IsDisposed && form.Visible)
                    {
                        form.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error closing SecondaryForm: {ex}");
                // Continue with form selection even if closing secondary form fails
            }

            var selector = new AssessmentTypeSelector();
            var result = selector.ShowDialog();
            
            if (result == DialogResult.OK && !selector.WasCancelled)
            {
                // Hide this form while showing the new one
                this.Hide();
                
                // Create and show the new form
                var newForm = new MainForm(selector.SelectedType);
                newForm.FormClosed += (s, args) => this.Close(); // Close this form when new form closes
                newForm.Show();
            }
        }

        private async void ReloadTemplates_Click(object sender, EventArgs e)
        {
            // Prevent multiple simultaneous reloads
            if (_isReloading) return;
            
            try
            {
                _isReloading = true;
                
                // Stop watching while we reload
                _templateWatcher?.StopWatching();
                
                // Save current window position before doing anything else
                SaveWindowPosition();

                // Show the building message
                _statusLabel.Text = "Building application...";
                _statusLabel.ForeColor = System.Drawing.Color.FromArgb(255, 193, 7); // Bootstrap warning yellow
                _statusLabel.Visible = true;

                // Get the project directory path by going up from the bin directory
                var binDir = AppDomain.CurrentDomain.BaseDirectory;
                var projectDir = Path.GetFullPath(Path.Combine(binDir, "..\\..\\.."));
                var projectPath = Directory.GetFiles(projectDir, "*.csproj", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault();
                
                if (string.IsNullOrEmpty(projectPath))
                {
                    throw new FileNotFoundException($"Could not find .csproj file in the project directory: {projectDir}");
                }

                // Try to kill any existing processes of our app (except the current one)
                var currentProcess = Process.GetCurrentProcess();
                var processName = currentProcess.ProcessName;
                var attempts = 0;
                const int maxAttempts = 3;
                
                while (attempts < maxAttempts)
                {
                    var existingProcesses = Process.GetProcessesByName(processName)
                        .Where(p => p.Id != currentProcess.Id)
                        .ToList();
                        
                    if (!existingProcesses.Any())
                        break;
                        
                    foreach (var existingProcess in existingProcesses)
                    {
                        try
                        {
                            // Check if process has exited
                            if (existingProcess.HasExited)
                                continue;
                                
                            // Try to close the process gracefully first
                            if (!existingProcess.CloseMainWindow())
                            {
                                // If graceful close fails, try to kill
                                existingProcess.Kill();
                            }
                            await Task.Delay(500); // Give it some time to shut down
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Failed to close process {existingProcess.Id}: {ex.Message}");
                            // Continue with other processes
                        }
                    }
                    
                    attempts++;
                    if (attempts < maxAttempts)
                        await Task.Delay(1000); // Wait before next attempt
                }
                
                // Check if we still have running processes
                var remainingProcesses = Process.GetProcessesByName(processName)
                    .Where(p => p.Id != currentProcess.Id)
                    .ToList();
                    
                if (remainingProcesses.Any())
                {
                    throw new InvalidOperationException("Unable to close all instances of the application. Please close them manually and try again.");
                }

                // Instead of killing Edge, we'll keep track of the old PDF file
                var oldPdfPath = _currentPdfPath;

                // Create process to run dotnet build
                var buildProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"build \"{projectPath}\"", // Specify the project file path
                        WorkingDirectory = Path.GetDirectoryName(projectPath), // Use project directory as working directory
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                var output = new System.Text.StringBuilder();
                var error = new System.Text.StringBuilder();

                // Capture output and error streams
                buildProcess.OutputDataReceived += (s, args) => { if (args.Data != null) output.AppendLine(args.Data); };
                buildProcess.ErrorDataReceived += (s, args) => { if (args.Data != null) error.AppendLine(args.Data); };

                // Start the build process
                buildProcess.Start();
                buildProcess.BeginOutputReadLine();
                buildProcess.BeginErrorReadLine();
                await buildProcess.WaitForExitAsync();

                // Check if build was successful
                if (buildProcess.ExitCode != 0)
                {
                    _statusLabel.Text = "Build failed!";
                    _statusLabel.ForeColor = System.Drawing.Color.FromArgb(220, 53, 69); // Bootstrap danger red
                    
                    // Show the build output in a message box
                    var errorMessage = "Build failed with the following output:\n\n";
                    if (output.Length > 0) errorMessage += "Output:\n" + output.ToString() + "\n";
                    if (error.Length > 0) errorMessage += "Errors:\n" + error.ToString();
                    
                    MessageBox.Show(errorMessage, "Build Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    await Task.Delay(700); // Show error for 2 seconds
                    _statusLabel.Visible = false;
                    return;
                }

                // Show reloading message
                _statusLabel.Text = "Build successful! Reloading...";
                _statusLabel.ForeColor = System.Drawing.Color.FromArgb(40, 167, 69); // Bootstrap success green
                await Task.Delay(700);

                // Hide this form while showing the new one
                this.Hide();
                
                // Create and show the new form with the same assessment type
                var newForm = new MainForm(_currentAssessmentType);
                newForm.FormClosed += (s, args) => this.Close(); // Close this form when new form closes
                newForm.Show();

                // Allow reloading again after a delay to ensure the new form is fully loaded
                await Task.Delay(2000);
                _isReloading = false;

                // Regenerate the last PDF if we have a saved row index
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                    {
                        if (key != null)
                        {
                            var lastRowIndex = key.GetValue(LastSelectedRowKey) as int?;
                            if (lastRowIndex.HasValue)
                            {
                                await Task.Delay(1000); // Give the form a moment to load and data to populate
                                
                                // Simulate clicking the Generate PDF button for the last selected row
                                if (newForm.dataGridView.Rows.Count > lastRowIndex.Value)
                                {
                                    var cellEventArgs = new DataGridViewCellEventArgs(
                                        newForm.dataGridView.Columns["GeneratePdf"].Index,
                                        lastRowIndex.Value
                                    );
                                    newForm.dataGridView_CellContentClick(newForm.dataGridView, cellEventArgs);

                                    // Delete the old PDF file after a delay to ensure the new one is opened
                                    await Task.Delay(2000);
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(oldPdfPath) && File.Exists(oldPdfPath))
                                        {
                                            File.Delete(oldPdfPath);
                                        }
                                    }
                                    catch
                                    {
                                        // Ignore errors deleting old PDF
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore errors trying to regenerate PDF
                }
            }
            catch (Exception ex)
            {
                _statusLabel.Text = "Error during reload!";
                _statusLabel.ForeColor = System.Drawing.Color.FromArgb(220, 53, 69); // Bootstrap danger red
                _statusLabel.Visible = true;
                MessageBox.Show($"Error during reload: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                await Task.Delay(2000);
                _statusLabel.Visible = false;
            }
        }
    }
}
