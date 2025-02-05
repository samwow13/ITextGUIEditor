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

        public MainForm()
        {
            InitializeComponent();
            string jsonPath = "OralCareReferenceData.json";
            _jsonManager = new JsonManager(jsonPath);
            _pdfGenerator = new PdfGeneratorService();
            InitializeAsync();
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
                try
                {
                    Debug.WriteLine($"Generating PDF for row {e.RowIndex}");
                    var selectedItem = _referenceData[e.RowIndex];
                    Debug.WriteLine($"Data for PDF: ChildName={selectedItem.ChildInfo?.ChildName}, AssessmentDate={selectedItem.ChildInfo.AssessmentDate}");

                    var pdfBytes = _pdfGenerator.GeneratePdf(selectedItem);
                    Debug.WriteLine($"PDF generated, size: {pdfBytes.Length} bytes");

                    var tempPath = Path.GetTempFileName() + ".pdf";
                    File.WriteAllBytes(tempPath, pdfBytes);
                    Debug.WriteLine($"PDF saved to: {tempPath}");
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tempPath) { UseShellExecute = true });
                    MessageBox.Show("PDF generated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            this.Text = "Oral Care Reference Data Viewer";
            this.ResumeLayout(false);
        }
    }
}
