# iText Designer with GUI

This application allows users to generate multiple types of PDFs using iText7 for .NET, with support for multiple data sets and templates. Helping the user quickly update 
a html & CSS template and view the generated PDF.

## Adding a New Form Type

To add a new form type to the application, follow these steps:

1. Add Reference Data (find out the requirements for the PDF to generate, then build a json model for it.)
   - Create a JSON file in `ReferenceDataJsons` folder
   - Follow the existing JSON structure pattern

2. Create the Data Models
   - Create a new directory under `Models` for your form-specific models
   - Create a model class for your assessment (model for the assessment, you can copy other similar model layouts)
   - Create a model class for your reference data (model for the json you created in step 1)
   - Implement the necessary interfaces (IAssessment)

3. Update GUI Components and JSON Manager
   - In `Forms/AssessmentTypeSelector.cs`:
     - Add your new form type to the `AssessmentType` enum in the Models folder
     - Update the type selector dropdown to include the new assessment type
     - Ensure proper handling in the selection changed event
   
   - In `Services/JsonManager.cs`:
     - Add support for your new assessment type in the JSON loading logic
     - Implement proper deserialization for your reference data model
     - Ensure proper file path handling for your JSON data file
     - Add appropriate error handling for file operations
   
   - In `Forms/MainForm.cs`:
     - Add UI controls for your new assessment type's data entry
     - Set up data grid view columns if your form type requires tabular data
     - Implement template handling for your new assessment type
     - Add proper data validation and error handling
     - Ensure the "Back to Selection" functionality works with your new form type

4. Update PdfGeneratorService
   - In `Services/PdfGeneratorService.cs`:
     - Implement dynamic JSON data loading for your assessment type
     - Add methods to process and validate the model data
     - Create functions to map the model data to template placeholders
     - Implement type-specific placeholder replacement methods
     - Add proper image resource handling and base URI configuration (for adding images to the template)
     - Ensure proper error handling for data processing
     - Add support for template regeneration if needed

5. Create and Configure HTML Template
   - Add your template file to the `Templates` folder
   - Begin with a basic HTML structure and simple placeholders
   - Link to the shared `globalStyles.css` for consistent styling
   - Test the template with sample data to verify functionality
   - Gradually incorporate the full model data from step 4
   - Add CSS styling following Bootstrap conventions
   - Implement dynamic data binding for complex data structures
   - Ensure proper handling of checkboxes and form elements
   - Test image resource paths and rendering

## Project Structure

- `Forms/` - Contains the GUI forms
- `Models/` - Contains data models for each form type
- `Services/` - Contains core services like PDF generation
- `Templates/` - Contains HTML templates for each form type
- `ReferenceDataJsons/` - Contains JSON data files for each form type
