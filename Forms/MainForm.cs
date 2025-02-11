using System;
using System.Collections.Generic;
using System.Windows.Forms;
using iTextDesignerWithGUI.Models;
using iTextDesignerWithGUI.Services;
using System.IO;
using System.Diagnostics;

namespace iTextDesignerWithGUI.Forms
{
    public partial class MainForm : Form
    {
        private readonly IAssessment _assessment;
        private readonly JsonManager _jsonManager;
        private readonly PdfGeneratorService _pdfGenerator;
        private List<object> _referenceData;
        private string _currentPdfPath;

        public MainForm(AssessmentType assessmentType = AssessmentType.OralCare)
        {
            InitializeComponent();
            _assessment = CreateAssessment(assessmentType);
            _jsonManager = new JsonManager(_assessment.JsonDataPath, assessmentType);
            _pdfGenerator = new PdfGeneratorService();
            
            // Set the form to appear in center screen
            this.StartPosition = FormStartPosition.CenterScreen;
            
            InitializeAsync();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
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

        private void OnTemplateChanged(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine($"Template change detected: {e.FullPath}, ChangeType: {e.ChangeType}");
            
            // Ensure we're on the UI thread
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnTemplateChanged(sender, e)));
                return;
            }

            try
            {
                // Only regenerate if we have a current PDF
                if (!string.IsNullOrEmpty(_currentPdfPath))
                {
                    Debug.WriteLine($"Current PDF path: {_currentPdfPath}");
                    Debug.WriteLine($"Template changed: {e.FullPath}. Regenerating PDF...");
                    
                    // Wait a brief moment to ensure the template file is not locked
                    System.Threading.Thread.Sleep(500);
                    
                    var pdfBytes = _pdfGenerator.RegeneratePdf();
                    if (pdfBytes != null)
                    {
                        // Save to the same location
                        File.WriteAllBytes(_currentPdfPath, pdfBytes);
                        Debug.WriteLine($"PDF regenerated and saved to: {_currentPdfPath}");
                    }
                    else
                    {
                        Debug.WriteLine("RegeneratePdf() returned null - no previous data available");
                    }
                }
                else
                {
                    Debug.WriteLine("No current PDF path set - skipping regeneration");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error regenerating PDF: {ex}");
                MessageBox.Show($"Error regenerating PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private IAssessment CreateAssessment(AssessmentType type)
        {
            return type switch
            {
                AssessmentType.OralCare => new OralCareAssessment(),
                AssessmentType.RegisteredNurseTaskAndDelegation => new RegisteredNurseTaskDelegAssessment(),
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
                if (item is OralCareDataInstance oralCare)
                {
                    name = oralCare.ChildInfo?.ChildName;
                }
                else if (item is RegisteredNurseTaskDelegDataInstance nurseTask)
                {
                    name = nurseTask.ChildInfo?.Name;
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

            // Add Name column
            var nameColumn = new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                DataPropertyName = "Name",
                FillWeight = 85
            };
            dataGridView.Columns.Add(nameColumn);

            // Add Generate PDF button column
            var generatePdfColumn = new DataGridViewButtonColumn
            {
                HeaderText = "Actions",
                Text = "Generate PDF",
                Name = "GeneratePdf",
                UseColumnTextForButtonValue = true,
                FillWeight = 15,
                MinimumWidth = 100
            };
            dataGridView.Columns.Add(generatePdfColumn);

            dataGridView.CellContentClick += dataGridView_CellContentClick;
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView.Columns["GeneratePdf"].Index)
            {
                var item = _referenceData[e.RowIndex];
                try
                {
                    string name;
                    if (item is OralCareDataInstance oralCare)
                    {
                        name = oralCare.ChildInfo?.ChildName;
                    }
                    else if (item is RegisteredNurseTaskDelegDataInstance nurseTask)
                    {
                        name = nurseTask.ChildInfo?.Name;
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
                    Process.Start(new ProcessStartInfo(_currentPdfPath) { UseShellExecute = true });
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
            this.ClientSize = new System.Drawing.Size(800, 450);
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
    }
}
