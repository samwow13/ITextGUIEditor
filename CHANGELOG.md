# Changelog

## [2025-02-22]
- Added new `PdfPreviewForm.cs` to handle PDF preview functionality
  - Form is positioned below MainForm
  - Matches MainForm width and is 2.5x taller
  - Automatically repositions when MainForm moves or resizes
  - Uses WebBrowser control to display PDF content
- Modified `MainForm.cs` to use PdfPreviewForm instead of launching Edge browser
  - Removed direct Edge browser launch
  - Added integration with new PdfPreviewForm
- Fixed readonly field initialization in PdfPreviewForm
  - Moved WebBrowser initialization to constructor
  - Resolved CS0191 compilation error
## [2025-02-22]
- Added new `SecondaryForm.cs` to create a window below MainForm
  - Form is positioned below MainForm
  - Matches MainForm width and is 2.5x taller
  - Automatically repositions when MainForm moves or resizes
  - Added TabControl with two tabs:
    - JSONView tab with syntax highlighting for viewing PDF data
      - Uses RichTextBox with custom syntax highlighting
      - Read-only view with horizontal and vertical scrollbars
      - Custom color scheme for JSON elements:
        - Keys in red
        - String values in dark green
        - Numbers in purple
        - Booleans in brown
        - Null values in gray
        - Braces and commas in blue
      - Monospace font (Consolas) for better readability
      - Added support for all assessment types:
        - Oral Care Assessment
        - Registered Nurse Task Delegation
        - Test Razor Assessment
      - Cleanly formats data based on assessment type
    - C# Model View tab (placeholder for future implementation)
- Modified `MainForm.cs` to show SecondaryForm when generating PDFs
  - Maintains original PDF opening behavior in Edge
  - Added integration with SecondaryForm to display JSON data
## [2025-02-22]
- Added new `JsonChecklistControl.cs` to display JSON data with checkboxes
  - Custom control that converts JSON data into an interactive checklist
  - Supports nested objects and arrays
  - Maintains checkbox state for each item
  - Indentation for better visual hierarchy
  - Automatic scrolling for long content
- Modified `SecondaryForm.cs` to use JsonChecklistControl
  - Replaced RichTextBox with new JsonChecklistControl
  - Removed JSON syntax highlighting (no longer needed)
  - Maintained support for all assessment types
  - Improved UI with better spacing and layout
## [2025-02-22]
- Added persistent storage for JSON checklist progress
  - Created `ChecklistProgress.cs` model to handle progress data
  - Added `ChecklistProgressManager` to manage storage in TempStorage folder
  - Progress is saved per document type (OralCareAssessment, NurseTaskDelegation, TestRazorAssessment)
  - Each document's progress is uniquely identified and stored separately
  - Progress is automatically loaded when viewing a document
  - Changes are saved immediately when checkboxes are toggled
