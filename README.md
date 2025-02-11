# iText Designer with GUI

This application allows users to generate multiple types of PDFs using iText7 for .NET, with support for multiple data sets and templates.

## Adding a New Form Type

To add a new form type to the application, follow these steps:

1. Create the Data Models
   - Create a new directory under `Models` for your form-specific models
   - Create model classes for your assessment and data instances
   - Implement the necessary interfaces (IAssessment)

2. Create the HTML Template
   - Add your template file to the `Templates` folder
   - Use HTML with appropriate placeholders for dynamic data
   - Follow the existing template patterns for consistency

3. Add Reference Data
   - Create a JSON file in `ReferenceDataJsons` folder
   - Follow the existing JSON structure pattern
   - Ensure the data matches your model structure

4. Update the Application
   - Add your new form type to the `AssessmentType` enum in `IAssessment.cs`
   - Add your assessment type to the `CreateAssessment` method in `MainForm.cs`

## Project Structure

- `Forms/` - Contains the GUI forms
- `Models/` - Contains data models for each form type
- `Services/` - Contains core services like PDF generation
- `Templates/` - Contains HTML templates for each form type
- `ReferenceDataJsons/` - Contains JSON data files for each form type

## Best Practices

1. Follow Object-Oriented Design
   - Create separate classes for different responsibilities
   - Use inheritance and interfaces appropriately

2. Maintain Clean Code
   - Keep methods focused and concise
   - Use meaningful names for classes and variables
   - Add appropriate comments and documentation

3. Follow Existing Patterns
   - Study existing implementations as examples
   - Maintain consistency with the current codebase
   - Reuse existing components when possible

## Changelog

Check the `ChangeLog.txt` file for a history of changes and updates to the application.
