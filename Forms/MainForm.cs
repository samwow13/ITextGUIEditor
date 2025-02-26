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
        private AssessmentTypeWrapper _currentTypeWrapper;
        private Label _statusLabel;
        private const string RegistryPath = @"Software\ITextGUIDesigner";
        private const string WindowPosXKey = "WindowPosX";
        private const string WindowPosYKey = "WindowPosY";
        private const string LastSelectedRowKey = "LastSelectedRow";
        private const string AutoSavingEnabledKey = "AutoSavingEnabled";
        private const string CloseEdgeOnChangeKey = "CloseEdgeOnChange";
        private static bool _isReloading = false; // Add static flag to prevent multiple reloads
        
        // Constant for Task.Delay duration in milliseconds
        private const int TASK_DELAY_MS = 1;

        private int? _lastSelectedRow;
        private Process _currentEdgeProcess;
        private bool _closeEdgeOnChange = false;
        private SecondaryForm _secondaryForm;

        public MainForm(AssessmentTypeWrapper typeWrapper)
        {
            InitializeComponent();
            _currentTypeWrapper = typeWrapper;
            
            if (typeWrapper.IsBuiltIn && typeWrapper.BuiltInType.HasValue)
            {
                // No need to store the enum value separately, it's in the wrapper
            }
            else
            {
                // For custom types, no need for a default enum value
            }
            
            _assessment = typeWrapper.GetAssessment();
            _jsonManager = new JsonManager(_assessment.JsonDataPath, _currentTypeWrapper);
            _pdfGenerator = new PdfGeneratorService();
            
            // Initialize secondary form
            _secondaryForm = new SecondaryForm(this);
            
            // Initialize template watcher
            var templatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\Templates");
            _templateWatcher = new TemplateWatcherService(
                Path.GetFullPath(templatesPath), 
                () => ReloadTemplates_Click(this, EventArgs.Empty),
                this);
            
            _closeEdgeOnChange = LoadCloseEdgePreference();
            
            // Update the window title to show the selected assessment type
            this.Text = $"iText Designer - {_assessment.DisplayName}";
            
            // Load the saved window position
            LoadWindowPosition();
            
            // Initialize the status label
            InitializeStatusLabel();
            
            // Initialize the form asynchronously
            InitializeAsync();
        }

        /// <summary>
        /// Constructor that takes an AssessmentType enum value and converts it to an AssessmentTypeWrapper
        /// </summary>
        /// <param name="assessmentType">The built-in assessment type</param>
        public MainForm(AssessmentType assessmentType = AssessmentType.OralCare) 
            : this(AssessmentTypeWrapper.FromBuiltIn(assessmentType))
        {
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

        private bool LoadAutoSavingPreference()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key != null)
                    {
                        var value = key.GetValue(AutoSavingEnabledKey);
                        if (value != null)
                        {
                            return Convert.ToBoolean(value);
                        }
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Error loading auto-saving preference");
            }
            return false; // Default to disabled if not found or error
        }

        private void SaveAutoSavingPreference(bool enabled)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    if (key != null)
                    {
                        key.SetValue(AutoSavingEnabledKey, enabled ? 1 : 0, RegistryValueKind.DWord);
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Error saving auto-saving preference");
            }
        }

        private bool LoadCloseEdgePreference()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key != null)
                    {
                        var value = key.GetValue(CloseEdgeOnChangeKey);
                        if (value != null)
                        {
                            return Convert.ToBoolean(value);
                        }
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Error loading close-edge preference");
            }
            return false; // Default to disabled if not found or error
        }

        private void SaveCloseEdgePreference(bool enabled)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    if (key != null)
                    {
                        key.SetValue(CloseEdgeOnChangeKey, enabled ? 1 : 0, RegistryValueKind.DWord);
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Error saving close-edge preference");
            }
        }

        protected void OnFormClosing(FormClosingEventArgs e)
        {
            try
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
            }
            finally
            {
                base.OnFormClosing(e);
            }
        }

        protected void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    _templateWatcher?.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void OnTemplateChanged(AssessmentTypeWrapper typeWrapper)
        {
            _currentTypeWrapper = typeWrapper;
            var newAssessment = typeWrapper.GetAssessment();
            _referenceData = null;
            dataGridView.DataSource = null;
            InitializeAsync();
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

                // Add Automatic Saving checkbox
                CheckBox autoSaveCheckbox = new CheckBox
                {
                    Text = "Automatic Saving",
                    AutoSize = true,
                    Margin = new Padding(10),
                    Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular),
                    Cursor = Cursors.Hand,
                    Checked = LoadAutoSavingPreference()  // Load saved preference
                };

                // Add Close Edge on change checkbox
                CheckBox closeEdgeCheckbox = new CheckBox
                {
                    Text = "Close Edge on change",
                    AutoSize = true,
                    Margin = new Padding(10),
                    Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular),
                    Cursor = Cursors.Hand,
                    Checked = LoadCloseEdgePreference()
                };

                closeEdgeCheckbox.CheckedChanged += (sender, e) =>
                {
                    _closeEdgeOnChange = closeEdgeCheckbox.Checked;
                    SaveCloseEdgePreference(closeEdgeCheckbox.Checked);
                };

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
                    Cursor = Cursors.Hand,
                    Enabled = !LoadAutoSavingPreference()  // Enable if auto-saving is disabled
                };

                // Add event handler for checkbox state change
                autoSaveCheckbox.CheckedChanged += (sender, e) =>
                {
                    reloadButton.Enabled = !autoSaveCheckbox.Checked;
                    // Update button appearance when disabled
                    reloadButton.BackColor = autoSaveCheckbox.Checked ? 
                        System.Drawing.Color.FromArgb(108, 117, 125) : // Bootstrap gray for disabled
                        System.Drawing.Color.FromArgb(40, 167, 69);    // Bootstrap success green for enabled
                    
                    // Control the template watcher service
                    if (autoSaveCheckbox.Checked)
                    {
                        _templateWatcher.StartWatching();
                        Debug.WriteLine("Template watcher service started");
                    }
                    else
                    {
                        _templateWatcher.StopWatching();
                        Debug.WriteLine("Template watcher service stopped");
                    }

                    // Save the preference
                    SaveAutoSavingPreference(autoSaveCheckbox.Checked);
                };

                // Initialize template watcher based on saved preference
                if (autoSaveCheckbox.Checked)
                {
                    _templateWatcher.StartWatching();
                }

                reloadButton.Click += ReloadTemplates_Click;

                // Create a panel for the buttons
                TableLayoutPanel buttonPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Bottom,
                    Height = 120,
                    Padding = new Padding(10),
                    ColumnCount = 3,
                    RowCount = 3
                };

                // Configure columns: Left spacing (auto) | Back button | Reload button 
                buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Left spacing
                buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Back button
                buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Reload button

                // Configure rows: Checkboxes | Status Label | Buttons
                buttonPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Checkbox row
                buttonPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Status label row
                buttonPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Buttons row

                // Create status label
                _statusLabel = new Label
                {
                    Text = "",
                    AutoSize = true,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold),
                    ForeColor = System.Drawing.Color.FromArgb(40, 167, 69), // Bootstrap success green
                    Visible = false,
                    Dock = DockStyle.Fill
                };

                // Add controls to the panel
                buttonPanel.Controls.Add(closeEdgeCheckbox, 1, 0); // Close Edge checkbox
                buttonPanel.Controls.Add(autoSaveCheckbox, 2, 0); // Auto Save checkbox
                buttonPanel.Controls.Add(_statusLabel, 2, 1); // Status label centered in middle row
                buttonPanel.Controls.Add(backButton, 1, 2); // Back button in bottom row
                buttonPanel.Controls.Add(reloadButton, 2, 2); // Reload button in bottom row

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
            
            // Create numbered entries for each item in the reference data
            for (int i = 0; i < _referenceData.Count; i++)
            {
                data.Add(new { Entry = $"Entry {i + 1}" });
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

            // Add Entry column
            var entryColumn = new DataGridViewTextBoxColumn
            {
                HeaderText = "Entry",
                DataPropertyName = "Entry",
                FillWeight = 70
            };
            dataGridView.Columns.Add(entryColumn);

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
                    // Use a simple "Entry X" naming scheme instead of extracting names from different data types
                    string name = $"Entry {e.RowIndex + 1}";
                    
                    // Show the secondary form with the current item's data
                    _secondaryForm.UpdateData(item);
                    _secondaryForm.Show();

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

                    // Position Edge window relative to MainForm
                    if (_currentEdgeProcess != null)
                    {
                        await Task.Delay(500); // Give Edge time to open
                        var edgeWindow = FindWindow(null, Path.GetFileName(_currentPdfPath) + " - Microsoft Edge");
                        if (edgeWindow != IntPtr.Zero)
                        {
                            var x = this.Location.X + this.Width + EDGE_WINDOW_OFFSET;
                            var y = this.Location.Y;
                            SetWindowPos(edgeWindow, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error generating PDF: {ex.Message}");
                    Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                    // Format the error message to be more user-friendly
                    var errorMessage = new System.Text.StringBuilder();
                    errorMessage.AppendLine("PDF Generation Error:");
                    errorMessage.AppendLine(ex.Message);
                    
                    if (ex.InnerException != null)
                    {
                        errorMessage.AppendLine("\nDetailed Error:");
                        errorMessage.AppendLine(ex.InnerException.Message);
                    }

                    // Show the custom error form
                    using (var errorForm = new CustomErrorForm(errorMessage.ToString()))
                    {
                        errorForm.ShowDialog(this);
                    }
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

        private void BackToSelection_Click(object sender, EventArgs e)
        {
            // Create and show the AssessmentTypeSelector form
            var selector = new AssessmentTypeSelector();
            this.Hide(); // Hide this form instead of closing it immediately
            
            // Show the selector as a dialog
            if (selector.ShowDialog() == DialogResult.OK && !selector.WasCancelled)
            {
                // If user selected an assessment type, create a new MainForm with it
                var newForm = new MainForm(selector.SelectedTypeWrapper);
                newForm.FormClosed += (s, args) => this.Close(); // Close this form when the new one is closed
                newForm.Show();
            }
            else
            {
                // If user cancelled, just show this form again
                this.Show();
            }
        }

        private async void ReloadTemplates_Click(object sender, EventArgs e)
        {
            if (_isReloading) return;
            _isReloading = true;

            try
            {
                // Stop watching while we reload
                _templateWatcher?.StopWatching();

                // Close Edge windows if the option is enabled - do this in parallel
                var closeEdgeTask = Task.Run(() => CloseEdgeWindows());

                // Save current window position before doing anything else - do this in parallel
                var saveWindowTask = Task.Run(() => SaveWindowPosition());

                // Show the building message
                _statusLabel.Text = "Building application...";
                _statusLabel.ForeColor = System.Drawing.Color.FromArgb(255, 193, 7);
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

                // Wait for parallel tasks to complete
                await Task.WhenAll(closeEdgeTask, saveWindowTask);

                // Instead of killing Edge, we'll keep track of the old PDF file
                var oldPdfPath = _currentPdfPath;

                // Create process to run dotnet build with incremental build enabled
                var buildProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"build \"{projectPath}\" --no-restore --no-dependencies",
                        WorkingDirectory = Path.GetDirectoryName(projectPath),
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
                    _statusLabel.ForeColor = System.Drawing.Color.FromArgb(220, 53, 69);

                    // Format the error message to combine both output and error streams
                    var errorMessage = new System.Text.StringBuilder();
                    if (output.Length > 0) 
                    {
                        errorMessage.AppendLine("Build Output:");
                        errorMessage.AppendLine(output.ToString());
                    }
                    if (error.Length > 0)
                    {
                        if (output.Length > 0) errorMessage.AppendLine("\n");
                        errorMessage.AppendLine("Compilation Errors:");
                        errorMessage.AppendLine(error.ToString());
                    }

                    // Show the custom error form
                    using (var errorForm = new CustomErrorForm(errorMessage.ToString()))
                    {
                        errorForm.ShowDialog(this);
                    }

                    await Task.Delay(TASK_DELAY_MS);
                    _statusLabel.Visible = false;
                    return;
                }

                // Show reloading message
                _statusLabel.Text = "Build successful! Reloading...";
                _statusLabel.ForeColor = System.Drawing.Color.FromArgb(40, 167, 69);

                // Hide this form while showing the new one
                this.Hide();
                
                // Create and show the new form with the same assessment type
                var newForm = new MainForm(_currentTypeWrapper);
                newForm.FormClosed += (s, args) => this.Close();
                newForm.Show();

                // Allow reloading again immediately
                _isReloading = false;

                // Delete old PDF file in parallel if it exists
                if (!string.IsNullOrEmpty(oldPdfPath) && File.Exists(oldPdfPath))
                {
                    _ = Task.Run(() =>
                    {
                        try
                        {
                            File.Delete(oldPdfPath);
                        }
                        catch
                        {
                            // Ignore errors deleting old PDF
                        }
                    });
                }

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
                                // Wait for the DataGridView to be populated
                                int retryCount = 0;
                                while (newForm.dataGridView.Rows.Count == 0 && retryCount < 50)
                                {
                                    await Task.Delay(10); // Short delay between checks
                                    retryCount++;
                                }

                                if (newForm.dataGridView.Rows.Count > lastRowIndex.Value)
                                {
                                    // Give the form a moment to fully initialize
                                    await Task.Delay(100);
                                    
                                    var cellEventArgs = new DataGridViewCellEventArgs(
                                        newForm.dataGridView.Columns["GeneratePdf"].Index,
                                        lastRowIndex.Value
                                    );
                                    newForm.dataGridView_CellContentClick(newForm.dataGridView, cellEventArgs);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error regenerating PDF: {ex.Message}");
                    // Ignore errors trying to regenerate PDF
                }
            }
            catch (Exception ex)
            {
                using (var errorForm = new CustomErrorForm(ex.Message))
                {
                    errorForm.ShowDialog(this);
                }
                _statusLabel.Text = "Error during reload!";
                _statusLabel.ForeColor = System.Drawing.Color.FromArgb(220, 53, 69);
                _statusLabel.Visible = true;
                await Task.Delay(TASK_DELAY_MS);
                _statusLabel.Visible = false;
            }
            finally
            {
                _isReloading = false;
            }
        }

        private void CloseEdgeWindows()
        {
            if (!_closeEdgeOnChange) return;

            try
            {
                var edgeProcesses = Process.GetProcessesByName("msedge");
                foreach (var process in edgeProcesses)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.CloseMainWindow();
                            // Give it a moment to close gracefully
                            if (!process.WaitForExit(1000))
                            {
                                process.Kill();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error closing Edge process {process.Id}: {ex.Message}");
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CloseEdgeWindows: {ex.Message}");
            }
        }

        private void InitializeStatusLabel()
        {
            _statusLabel = new Label
            {
                Dock = DockStyle.Bottom,
                AutoSize = false,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                BorderStyle = BorderStyle.Fixed3D,
                Text = "Ready"
            };
            this.Controls.Add(_statusLabel);
        }

        /// <summary>
        /// Closes the Edge browser process if the setting is enabled
        /// </summary>
        private void CloseEdgeIfNeeded()
        {
            if (_closeEdgeOnChange && _currentEdgeProcess != null && !_currentEdgeProcess.HasExited)
            {
                try
                {
                    _currentEdgeProcess.Kill();
                    _currentEdgeProcess = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error closing Edge: {ex.Message}");
                }
            }
        }
    }
}
