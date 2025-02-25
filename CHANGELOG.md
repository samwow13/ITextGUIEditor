# Changelog

## [2025-02-25]
- Updated dataGridView_CellContentClick method to use simple "Entry X" naming scheme
- Simplified PDF filename generation to match the entry naming in PopulateDataGrid
- Removed type-specific name extraction logic from CellContentClick handler

## [2025-02-24]
- Added "+" button to AssessmentTypeSelector form for creating new templates
- Created new PDFTemplateForm for capturing template names
- Enhanced UI layout in AssessmentTypeSelector to accommodate new button
- Maintained consistent styling across forms using Windows Forms controls

## Changes Log

### 2024-02-24
- Added project directory dropdown to PDFTemplateForm
- Created JSON structure for project directories in pdfCreationData.json
- Fixed path resolution for loading JSON configuration
- Improved form layout and spacing in PDFTemplateForm
- Added error handling for JSON configuration loading
