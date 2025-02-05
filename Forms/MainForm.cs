using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OralCareReference.Models;
using OralCareReference.Services;
using System.IO;
using System.Diagnostics;

namespace OralCareReference.Forms
{
    public partial class MainForm : Form
    {
        private readonly JsonManager _jsonManager;
        private List<ReferenceDataItem> _referenceData;
        private readonly PdfGeneratorService _pdfGenerator;
        private readonly IAssessment _assessment;

        public MainForm(AssessmentType assessmentType)
        {
            InitializeComponent();
            _assessment = CreateAssessment(assessmentType);
            _jsonManager = new JsonManager(_assessment.JsonDataPath);
            _pdfGenerator = new PdfGeneratorService();
            this.Text = _assessment.DisplayName;
            InitializeAsync();
        }

        private IAssessment CreateAssessment(AssessmentType type)
        {
            return type switch
            {
                AssessmentType.OralCare => new OralCareAssessment(),
                _ => throw new ArgumentException($"Unsupported assessment type: {type}")
            };
        }

        private async void InitializeAsync()
        {
            try
            {
                _referenceData = await _jsonManager.LoadReferenceDataAsync();
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
            dataGridView.DataSource = _referenceData.Select(item => new
            {
                ChildName = item.ChildInfo.ChildName,
                CaseNumber = item.ChildInfo.CaseNumber,
                AssessmentDate = item.ChildInfo.AssessmentDate
            }).ToList();
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

            // Add columns
            var childNameColumn = new DataGridViewTextBoxColumn
            {
                HeaderText = "Child Name",
                DataPropertyName = "ChildName",
                FillWeight = 35
            };
            dataGridView.Columns.Add(childNameColumn);

            var caseNumberColumn = new DataGridViewTextBoxColumn
            {
                HeaderText = "Case Number",
                DataPropertyName = "CaseNumber",
                FillWeight = 25
            };
            dataGridView.Columns.Add(caseNumberColumn);

            var assessmentDateColumn = new DataGridViewTextBoxColumn
            {
                HeaderText = "Assessment Date",
                DataPropertyName = "AssessmentDate",
                FillWeight = 25
            };
            dataGridView.Columns.Add(assessmentDateColumn);

            // Add Generate PDF button column with increased width
            var generatePdfColumn = new DataGridViewButtonColumn
            {
                HeaderText = "Actions",
                Text = "Generate PDF",
                Name = "GeneratePdf",
                UseColumnTextForButtonValue = true,
                FillWeight = 15,
                MinimumWidth = 100  // Ensure minimum width for the button
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
                    Debug.WriteLine($"Generating PDF for row {e.RowIndex}");
                    Debug.WriteLine($"Data for PDF: ChildName={item.ChildInfo?.ChildName}, AssessmentDate={item.ChildInfo.AssessmentDate}");

                    var pdfBytes = _pdfGenerator.GeneratePdf(item, _assessment.TemplateFileName);
                    Debug.WriteLine($"PDF generated, size: {pdfBytes.Length} bytes");

                    var tempPath = Path.GetTempFileName() + ".pdf";
                    File.WriteAllBytes(tempPath, pdfBytes);
                    Debug.WriteLine($"PDF saved to: {tempPath}");
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tempPath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error generating PDF: {ex}");
                    var errorMessage = ex.InnerException != null 
                        ? $"Error generating PDF: {ex.Message}\nDetails: {ex.InnerException.Message}"
                        : $"Error generating PDF: {ex.Message}";
                    
                    MessageBox.Show(errorMessage + "\n\nStack Trace:\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}
