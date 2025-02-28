using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace iTextDesignerWithGUI.Services
{
    /// <summary>
    /// Service to generate Razor (cshtml) files based on user input
    /// </summary>
    public class CshtmlGenerationService
    {
        private readonly ProjectDirectoryService _directoryService;

        /// <summary>
        /// Constructor for CshtmlGenerationService
        /// </summary>
        /// <param name="directoryService">Service for managing project directories</param>
        public CshtmlGenerationService(ProjectDirectoryService directoryService)
        {
            _directoryService = directoryService ?? throw new ArgumentNullException(nameof(directoryService));
            Debug.WriteLine($"CshtmlGenerationService initialized");
        }

        /// <summary>
        /// Generates a Razor (cshtml) file with the specified name in the given template directory
        /// </summary>
        /// <param name="fileName">Name of the file to generate (without extension)</param>
        /// <param name="templateType">Type of template (e.g., "HealthAndWellness")</param>
        /// <returns>True if generation was successful, false otherwise</returns>
        public bool GenerateCshtmlFile(string fileName, string templateType)
        {
            try
            {
                // Construct the target directory path using the directory service
                string targetDirectory = _directoryService.EnsureDirectoryExists(Path.Combine("Templates", templateType));
                
                // Append "Template" to the file name
                string templateFileName = $"{fileName}Template";
                
                // Construct the target file path
                string targetFilePath = Path.Combine(targetDirectory, $"{templateFileName}.cshtml");
                
                Debug.WriteLine($"Generating Razor (cshtml) file at: {targetFilePath}");
                
                // Generate a basic Razor content
                string cshtmlContent = GenerateBasicCshtmlContent(fileName, templateType);
                
                // Write the Razor content to the file
                File.WriteAllText(targetFilePath, cshtmlContent);
                
                Debug.WriteLine($"Successfully generated Razor (cshtml) file: {targetFilePath}");
                
                // Generate the corresponding JSON file
                bool jsonGenerated = GenerateJsonReferenceFile(fileName, templateType);
                if (!jsonGenerated)
                {
                    Debug.WriteLine("Warning: Generated cshtml file, but failed to generate JSON reference file");
                }
                
                // Generate the corresponding model files
                bool modelsGenerated = GenerateModelFiles(fileName, templateType);
                if (!modelsGenerated)
                {
                    Debug.WriteLine("Warning: Generated cshtml and JSON files, but failed to generate model files");
                }
                
                // Add the new assessment type to the assessmentTypes.json file
                bool assessmentTypeAdded = AddAssessmentTypeToJson(fileName, templateType);
                if (!assessmentTypeAdded)
                {
                    Debug.WriteLine("Warning: Generated files successfully, but failed to update assessmentTypes.json");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generating Razor (cshtml) file: {ex.Message}");
                MessageBox.Show($"Error generating Razor (cshtml) file: {ex.Message}", "Generation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Generates model files for the assessment including an Assessment class and a DataInstance class
        /// </summary>
        /// <param name="fileName">Name of the file (without extension)</param>
        /// <param name="templateType">Type of template (e.g., "HealthAndWellness")</param>
        /// <returns>True if generation was successful, false otherwise</returns>
        public bool GenerateModelFiles(string fileName, string templateType)
        {
            try
            {
                // Construct the target directory path
                string targetDirectory = _directoryService.EnsureDirectoryExists(Path.Combine("Models", templateType, $"{fileName}Models"));
                
                // Generate the assessment model file
                bool assessmentGenerated = GenerateAssessmentModelFile(fileName, templateType, targetDirectory);
                
                // Generate the data instance model file
                bool dataInstanceGenerated = GenerateDataInstanceModelFile(fileName, templateType, targetDirectory);
                
                return assessmentGenerated && dataInstanceGenerated;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generating model files: {ex.Message}");
                MessageBox.Show($"Error generating model files: {ex.Message}", "Generation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Generates the assessment model file
        /// </summary>
        /// <param name="fileName">Name of the file (without extension)</param>
        /// <param name="templateType">Type of template (e.g., "HealthAndWellness")</param>
        /// <param name="targetDirectory">Target directory to save the model file</param>
        /// <returns>True if generation was successful, false otherwise</returns>
        private bool GenerateAssessmentModelFile(string fileName, string templateType, string targetDirectory)
        {
            try
            {
                // Construct the target file path
                string targetFilePath = Path.Combine(targetDirectory, $"{fileName}Assessment.cs");
                
                Debug.WriteLine($"Generating Assessment model file at: {targetFilePath}");
                
                // Generate the assessment model content
                string assessmentContent = $@"namespace iTextDesignerWithGUI.Models
{{
    /// <summary>
    /// Implementation of IAssessment for {fileName} assessments
    /// </summary>
    public class {fileName}Assessment : IAssessment
    {{
        public string TemplateFileName => ""{templateType}/{fileName}Template.cshtml"";
        public string JsonDataPath => ""ReferenceDataJsons/{templateType}/{fileName}Data.json"";
        public string DisplayName => ""{fileName}"";
    }}
}}
";
                
                // Write the assessment model content to the file
                File.WriteAllText(targetFilePath, assessmentContent);
                
                Debug.WriteLine($"Successfully generated Assessment model file: {targetFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generating Assessment model file: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Generates the data instance model file based on the JSON structure
        /// </summary>
        /// <param name="fileName">Name of the file (without extension)</param>
        /// <param name="templateType">Type of template (e.g., "HealthAndWellness")</param>
        /// <param name="targetDirectory">Target directory to save the model file</param>
        /// <returns>True if generation was successful, false otherwise</returns>
        private bool GenerateDataInstanceModelFile(string fileName, string templateType, string targetDirectory)
        {
            try
            {
                // Construct the target file path
                string targetFilePath = Path.Combine(targetDirectory, $"{fileName}Instance.cs");
                
                Debug.WriteLine($"Generating Data Instance model file at: {targetFilePath}");
                
                // Generate the data instance model content based on our JSON structure
                string dataInstanceContent = $@"using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace iTextDesignerWithGUI.Models.{fileName}Models
{{
    public class {fileName}Instance
    {{
        [JsonProperty(""model"")]
        public ModelData Model {{ get; set; }}

        [JsonProperty(""properties"")]
        public Properties Properties {{ get; set; }}
    }}

    public class ModelData
    {{
        [JsonProperty(""id"")]
        public int Id {{ get; set; }}
        
        [JsonProperty(""name"")]
        public string Name {{ get; set; }}
        
        [JsonProperty(""description"")]
        public string Description {{ get; set; }}
        
        [JsonProperty(""created_at"")]
        public string CreatedAt {{ get; set; }}
        
        [JsonProperty(""is_active"")]
        public bool IsActive {{ get; set; }}
    }}

    public class Properties
    {{
        [JsonProperty(""sections"")]
        public List<Section> Sections {{ get; set; }}
    }}

    public class Section
    {{
        [JsonProperty(""title"")]
        public string Title {{ get; set; }}
        
        [JsonProperty(""fields"")]
        public List<Field> Fields {{ get; set; }}
    }}

    public class Field
    {{
        [JsonProperty(""name"")]
        public string Name {{ get; set; }}
        
        [JsonProperty(""type"")]
        public string Type {{ get; set; }}
        
        [JsonProperty(""value"")]
        public object Value {{ get; set; }}
    }}
}}
";
                
                // Write the data instance model content to the file
                File.WriteAllText(targetFilePath, dataInstanceContent);
                
                Debug.WriteLine($"Successfully generated Data Instance model file: {targetFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generating Data Instance model file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generates a JSON reference data file with a basic data model structure
        /// </summary>
        /// <param name="fileName">Name of the file (without extension)</param>
        /// <param name="templateType">Type of template (e.g., "HealthAndWellness")</param>
        /// <returns>True if generation was successful, false otherwise</returns>
        public bool GenerateJsonReferenceFile(string fileName, string templateType)
        {
            try
            {
                // Construct the target directory path
                string targetDirectory = _directoryService.EnsureDirectoryExists(Path.Combine("ReferenceDataJsons", templateType));
                
                // Append "Data" to the file name
                string jsonFileName = $"{fileName}Data";
                
                // Construct the target file path
                string targetFilePath = Path.Combine(targetDirectory, $"{jsonFileName}.json");
                
                Debug.WriteLine($"Generating JSON reference file at: {targetFilePath}");
                
                // Generate a basic JSON content
                string jsonContent = GenerateBasicJsonContent(fileName);
                
                // Write the JSON content to the file
                File.WriteAllText(targetFilePath, jsonContent);
                
                Debug.WriteLine($"Successfully generated JSON reference file: {targetFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generating JSON reference file: {ex.Message}");
                MessageBox.Show($"Error generating JSON reference file: {ex.Message}", "Generation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Generates basic Razor (cshtml) content for a new file
        /// </summary>
        /// <param name="fileName">Name of the file (used as the title)</param>
        /// <param name="templateType">Type of template (e.g., "HealthAndWellness")</param>
        /// <returns>String containing Razor (cshtml) content</returns>
        private string GenerateBasicCshtmlContent(string fileName, string templateType)
        {
            // Get root directory for instructions
            string rootDirectory = _directoryService.GetRootDirectory();
            
            // Generate specific folder paths based on the fileName and templateType
            string referenceDataJsonFolder = Path.Combine(rootDirectory, "ReferenceDataJsons", templateType);
            string referenceDataJsonFile = Path.Combine(referenceDataJsonFolder, $"{fileName}Data.json");
            
            string modelsFolder = Path.Combine(rootDirectory, "Models", templateType, $"{fileName}Models");
            string modelInstanceFile = Path.Combine(modelsFolder, $"{fileName}Instance.cs");
            
            string templatesFolder = Path.Combine(rootDirectory, "Templates", templateType);
            string templateFile = Path.Combine(templatesFolder, $"{fileName}Template.cshtml");

            return $@"@model iTextDesignerWithGUI.Models.{fileName}Models.{fileName}Instance

<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{fileName}</title>
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css"" rel=""stylesheet"">
    <link href=""globalStyles.css"" rel=""stylesheet"">
</head>
<body>
    <div class=""container"">
        <!-- Header with Title -->
        <header class=""my-4"">
            <h1 class=""text-center"">{fileName} Setup Instructions</h1>
        </header>

        <!-- Setup Instructions -->
        <div class=""content"">
            <div class=""card mb-4"">
                <div class=""card-header bg-primary text-white"">
                    <h2 class=""h4 mb-0"">Step 1: Fixing the JSON</h2>
                </div>
                <div class=""card-body"">
                    <p>Close this application and find the ReferenceDataJsons folder for your assessment type:</p>
                    
                    <p>Your JSON file will be located at:</p>
                    <code class=""d-block bg-light p-2 mb-3"">{referenceDataJsonFile}</code>
                    
                    <p>Create and add your JSON here. You need to have an array of your initial JSON structure with at least 3 entries to work with the GUI.</p>
                </div>
            </div>

            <div class=""card mb-4"">
                <div class=""card-header bg-primary text-white"">
                    <h2 class=""h4 mb-0"">Step 2: Fixing the C# Model</h2>
                </div>
                <div class=""card-body"">
                    <p>Next, go to the Models Folder for your assessment type:</p>
                    
                    <p>Your model instance file will be located at:</p>
                    <code class=""d-block bg-light p-2 mb-3"">{modelInstanceFile}</code>
                    
                    <p>Using ChatGPT, you can quickly create a model by copying in one of your JSON entries and asking for a C# model to be built.</p>
                </div>
            </div>

            <div class=""card mb-4"">
                <div class=""card-header bg-primary text-white"">
                    <h2 class=""h4 mb-0"">Step 3: Getting access to the data</h2>
                </div>
                <div class=""card-body"">
                    <p>Find the HTML template that was generated at:</p>
                    <code class=""d-block bg-light p-2 mb-3"">{templateFile}</code>
                    
                    <p>Then, simply remove the old models references like <code>@@@@Model.User.Name</code>, while keeping the @@@@model declaration on the 1st line of code. This line of code passes your entire data set into the template.</p>
                </div>
            </div>

            <div class=""card mb-4"">
                <div class=""card-header bg-primary text-white"">
                    <h2 class=""h4 mb-0"">Step 4: Referencing globalStyles.css</h2>
                </div>
                <div class=""card-body"">
                    <p>Refer to other templates in the project to find out how to reference the globalStyles.css sheet. You can see how it's done in this example:</p>
                    <code class=""d-block bg-light p-2 mb-3"">&lt;link href=""globalStyles.css"" rel=""stylesheet""&gt;</code>
                </div>
            </div>
        </div>
    </div>

    <script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js""></script>
</body>
</html>";
        }
        
        /// <summary>
        /// Generates basic JSON content for a new file
        /// </summary>
        /// <param name="fileName">Name of the file (used to structure the model)</param>
        /// <returns>String containing JSON content</returns>
        private string GenerateBasicJsonContent(string fileName)
        {
            // Create a sample data model with example data (3 data sets)
            var sampleData = new List<object>
            {
                // Example data set 1
                new
                {
                    model = new
                    {
                        id = 1,
                        name = $"{fileName} Sample 1",
                        description = $"Primary sample data for {fileName}",
                        created_at = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        is_active = true
                    },
                    properties = new
                    {
                        sections = new List<object>
                        {
                            new
                            {
                                title = "Section 1",
                                fields = new List<object>
                                {
                                    new
                                    {
                                        name = "Field1",
                                        type = "text",
                                        value = "Sample value 1"
                                    },
                                    new
                                    {
                                        name = "Field2",
                                        type = "number",
                                        value = 42
                                    }
                                }
                            },
                            new
                            {
                                title = "Section 2",
                                fields = new List<object>
                                {
                                    new
                                    {
                                        name = "Field3",
                                        type = "checkbox",
                                        value = true
                                    },
                                    new
                                    {
                                        name = "Field4",
                                        type = "date",
                                        value = DateTime.Now.ToString("yyyy-MM-dd")
                                    }
                                }
                            }
                        }
                    }
                },
                
                // Example data set 2
                new
                {
                    model = new
                    {
                        id = 2,
                        name = $"{fileName} Sample 2",
                        description = $"Secondary sample data for {fileName}",
                        created_at = DateTime.Now.AddDays(-7).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        is_active = true
                    },
                    properties = new
                    {
                        sections = new List<object>
                        {
                            new
                            {
                                title = "Personal Information",
                                fields = new List<object>
                                {
                                    new
                                    {
                                        name = "FullName",
                                        type = "text",
                                        value = "Jane Smith"
                                    },
                                    new
                                    {
                                        name = "Age",
                                        type = "number",
                                        value = 35
                                    },
                                    new
                                    {
                                        name = "Email",
                                        type = "email",
                                        value = "jane.smith@example.com"
                                    }
                                }
                            },
                            new
                            {
                                title = "Preferences",
                                fields = new List<object>
                                {
                                    new
                                    {
                                        name = "ReceiveNotifications",
                                        type = "checkbox",
                                        value = false
                                    },
                                    new
                                    {
                                        name = "Theme",
                                        type = "select",
                                        value = "Dark"
                                    }
                                }
                            }
                        }
                    }
                },
                
                // Example data set 3
                new
                {
                    model = new
                    {
                        id = 3,
                        name = $"{fileName} Sample 3",
                        description = $"Tertiary sample data for {fileName}",
                        created_at = DateTime.Now.AddDays(-14).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        is_active = false
                    },
                    properties = new
                    {
                        sections = new List<object>
                        {
                            new
                            {
                                title = "Product Details",
                                fields = new List<object>
                                {
                                    new
                                    {
                                        name = "ProductName",
                                        type = "text",
                                        value = "Super Widget Pro"
                                    },
                                    new
                                    {
                                        name = "SKU",
                                        type = "text",
                                        value = "WDG-1234-PRO"
                                    },
                                    new
                                    {
                                        name = "Price",
                                        type = "currency",
                                        value = 199.99
                                    },
                                    new
                                    {
                                        name = "InStock",
                                        type = "checkbox",
                                        value = true
                                    }
                                }
                            },
                            new
                            {
                                title = "Shipping Information",
                                fields = new List<object>
                                {
                                    new
                                    {
                                        name = "Weight",
                                        type = "number",
                                        value = 2.5
                                    },
                                    new
                                    {
                                        name = "Dimensions",
                                        type = "text",
                                        value = "10 x 8 x 3 inches"
                                    },
                                    new
                                    {
                                        name = "ShippingMethod",
                                        type = "select",
                                        value = "Express"
                                    }
                                }
                            }
                        }
                    }
                }
            };
            
            // Serialize the object to JSON with proper formatting
            return JsonSerializer.Serialize(sampleData, new JsonSerializerOptions 
            { 
                WriteIndented = true
            });
        }
        
        /// <summary>
        /// Adds a new assessment type to the assessmentTypes.json file
        /// </summary>
        /// <param name="fileName">Name of the file (without extension)</param>
        /// <param name="templateType">Type of template (e.g., "HealthAndWellness")</param>
        /// <returns>True if the operation was successful, false otherwise</returns>
        private bool AddAssessmentTypeToJson(string fileName, string templateType)
        {
            try
            {
                // Construct the path to the assessmentTypes.json file
                string assessmentTypesJsonPath = Path.Combine(_directoryService.GetDirectory("PersistentDataJSON"), "assessmentTypes.json");
                
                Debug.WriteLine($"Updating assessment types JSON file at: {assessmentTypesJsonPath}");
                
                // Read the existing JSON file
                string jsonContent = File.ReadAllText(assessmentTypesJsonPath);
                
                // Parse the JSON document to preserve the exact structure
                using (JsonDocument document = JsonDocument.Parse(jsonContent))
                {
                    // Create a new JSON document with the same structure
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (Utf8JsonWriter writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true }))
                        {
                            writer.WriteStartObject();
                            
                            // Start writing the assessmentTypes array
                            writer.WritePropertyName("assessmentTypes");
                            writer.WriteStartArray();
                            
                            // Copy all existing assessment types
                            bool assessmentTypeExists = false;
                            if (document.RootElement.TryGetProperty("assessmentTypes", out JsonElement assessmentTypes))
                            {
                                foreach (JsonElement assessmentType in assessmentTypes.EnumerateArray())
                                {
                                    // Check if this assessment type already exists
                                    if (assessmentType.TryGetProperty("name", out JsonElement nameElement) &&
                                        string.Equals(nameElement.GetString(), fileName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        assessmentTypeExists = true;
                                        // Skip this element as we'll add an updated version
                                        continue;
                                    }
                                    
                                    // Copy the existing assessment type as-is
                                    assessmentType.WriteTo(writer);
                                }
                            }
                            
                            // If the assessment type doesn't exist, add it
                            if (!assessmentTypeExists)
                            {
                                // Create paths for the new assessment type
                                string assessmentTypeDirectory = $"Models/{templateType}/{fileName}Models/{fileName}Assessment.cs";
                                
                                // Use consistent naming convention: [Name]Instance.cs instead of [Name]DataInstance.cs
                                string assessmentDataInstanceDirectory = $"Models/{templateType}/{fileName}Models/{fileName}Instance.cs";
                                
                                string cshtmlTemplateDirectory = $"Templates/{templateType}/{fileName}Template.cshtml";
                                string jsonDataLocationDirectory = $"ReferenceDataJsons/{templateType}/{fileName}Data.json";
                                
                                // Write the new assessment type
                                writer.WriteStartObject();
                                writer.WriteString("name", fileName);
                                writer.WriteString("displayName", fileName);
                                writer.WriteString("assessmentTypeDirectory", assessmentTypeDirectory);
                                writer.WriteString("assessmentDataInstanceDirectory", assessmentDataInstanceDirectory);
                                writer.WriteString("cshtmlTemplateDirectory", cshtmlTemplateDirectory);
                                writer.WriteString("jsonDataLocationDirectory", jsonDataLocationDirectory);
                                writer.WriteEndObject();
                            }
                            
                            // End the assessmentTypes array
                            writer.WriteEndArray();
                            
                            // End the root object
                            writer.WriteEndObject();
                        }
                        
                        // Get the JSON as a string
                        ms.Position = 0;
                        using (StreamReader reader = new StreamReader(ms))
                        {
                            string updatedJsonContent = reader.ReadToEnd();
                            
                            // Write the updated JSON back to the file
                            File.WriteAllText(assessmentTypesJsonPath, updatedJsonContent);
                        }
                    }
                }
                
                Debug.WriteLine($"Successfully added assessment type '{fileName}' to the JSON file");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding assessment type to JSON file: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Private class to match the structure of the assessmentTypes.json file
        /// </summary>
        private class AssessmentTypesJson
        {
            /// <summary>
            /// List of assessment type definitions
            /// </summary>
            [JsonPropertyName("assessmentTypes")]
            public List<AssessmentTypeJson> AssessmentTypes { get; set; } = new List<AssessmentTypeJson>();
        }
        
        /// <summary>
        /// Private class to match the structure of an assessment type in the JSON file
        /// </summary>
        private class AssessmentTypeJson
        {
            /// <summary>
            /// Name of the assessment type
            /// </summary>
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
            
            /// <summary>
            /// Display name for the assessment type
            /// </summary>
            [JsonPropertyName("displayName")]
            public string DisplayName { get; set; } = string.Empty;
            
            /// <summary>
            /// Path to the assessment type model class
            /// </summary>
            [JsonPropertyName("assessmentTypeDirectory")]
            public string AssessmentTypeDirectory { get; set; } = string.Empty;
            
            /// <summary>
            /// Path to the data instance model class
            /// </summary>
            [JsonPropertyName("assessmentDataInstanceDirectory")]
            public string AssessmentDataInstanceDirectory { get; set; } = string.Empty;
            
            /// <summary>
            /// Path to the template file for this assessment type
            /// </summary>
            [JsonPropertyName("cshtmlTemplateDirectory")]
            public string CshtmlTemplateDirectory { get; set; } = string.Empty;
            
            /// <summary>
            /// Path to the JSON data file for this assessment type
            /// </summary>
            [JsonPropertyName("jsonDataLocationDirectory")]
            public string JsonDataLocationDirectory { get; set; } = string.Empty;
        }
    }
}
