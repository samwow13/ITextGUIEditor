using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.Json;
using System.Collections.Generic;

namespace iTextDesignerWithGUI.Services
{
    /// <summary>
    /// Service to generate Razor (cshtml) files based on user input
    /// </summary>
    public class CshtmlGenerationService
    {
        private readonly string _projectRootPath;

        /// <summary>
        /// Constructor for CshtmlGenerationService
        /// </summary>
        /// <param name="projectRootPath">Root path of the project. If null, it will be determined automatically</param>
        public CshtmlGenerationService(string? projectRootPath = null)
        {
            // If root path is not provided, determine it from the executing assembly location
            _projectRootPath = projectRootPath ?? DetermineProjectRootPath();
            Debug.WriteLine($"CshtmlGenerationService initialized with root path: {_projectRootPath}");
        }

        /// <summary>
        /// Determines the project root path automatically
        /// </summary>
        /// <returns>Root path of the project</returns>
        private string DetermineProjectRootPath()
        {
            try
            {
                // Navigate from the bin directory up to the actual project root
                // This matches the approach used in MainForm for TemplateWatcherService
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                
                // Go up three directories: bin/Debug/net6.0-windows -> project root
                string projectPath = Path.Combine(baseDirectory, "..\\..\\..\\");
                string fullPath = Path.GetFullPath(projectPath);
                
                Debug.WriteLine($"Determined project root path: {fullPath}");
                
                // Validate that we found the correct directory by checking for Templates
                if (!Directory.Exists(Path.Combine(fullPath, "Templates")))
                {
                    Debug.WriteLine("Warning: Templates directory not found in determined project path");
                }
                
                return fullPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error determining project root path: {ex.Message}");
                // Fallback to the current directory if we encounter an error
                return AppDomain.CurrentDomain.BaseDirectory;
            }
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
                // Construct the target directory path
                string targetDirectory = Path.Combine(_projectRootPath, "Templates", templateType);
                
                // Ensure the directory exists
                if (!Directory.Exists(targetDirectory))
                {
                    Debug.WriteLine($"Creating directory: {targetDirectory}");
                    Directory.CreateDirectory(targetDirectory);
                }
                
                // Append "Template" to the file name
                string templateFileName = $"{fileName}Template";
                
                // Construct the target file path
                string targetFilePath = Path.Combine(targetDirectory, $"{templateFileName}.cshtml");
                
                Debug.WriteLine($"Generating Razor (cshtml) file at: {targetFilePath}");
                
                // Generate a basic Razor content
                string cshtmlContent = GenerateBasicCshtmlContent(fileName);
                
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
                string targetDirectory = Path.Combine(_projectRootPath, "Models", templateType, $"{fileName}Models");
                
                // Ensure the directory exists
                if (!Directory.Exists(targetDirectory))
                {
                    Debug.WriteLine($"Creating models directory: {targetDirectory}");
                    Directory.CreateDirectory(targetDirectory);
                }
                
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
                string targetDirectory = Path.Combine(_projectRootPath, "ReferenceDataJsons", templateType);
                
                // Ensure the directory exists
                if (!Directory.Exists(targetDirectory))
                {
                    Debug.WriteLine($"Creating directory: {targetDirectory}");
                    Directory.CreateDirectory(targetDirectory);
                }
                
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
        /// <returns>String containing Razor (cshtml) content</returns>
        private string GenerateBasicCshtmlContent(string fileName)
        {
            return $@"@page
@model {fileName}Model
@{{
    ViewData[""Title""] = ""{fileName}"";
}}

<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>@ViewData[""Title""]</title>
    <link href=""~/css/globalStyles.css"" rel=""stylesheet"">
</head>
<body>
    <div class=""container"">
        <!-- Header with Title -->
        <header>
            <div class=""title"">
                @ViewData[""Title""]
            </div>
        </header>

        <!-- Content goes here -->
        <div class=""content"">
            <p>This is a template file for @ViewData[""Title""].</p>
            
            @* Razor code example *@
            <div>
                @for (int i = 0; i < 3; i++)
                {{
                    <div>Item @i</div>
                }}
            </div>
        </div>
    </div>
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
                string assessmentTypesJsonPath = Path.Combine(_projectRootPath, "PersistentDataJSON", "assessmentTypes.json");
                
                Debug.WriteLine($"Updating assessment types JSON file at: {assessmentTypesJsonPath}");
                
                // Read the existing JSON file
                string jsonContent = File.ReadAllText(assessmentTypesJsonPath);
                
                // Define a local class to match the JSON structure
                var assessmentTypesJson = JsonSerializer.Deserialize<AssessmentTypesJson>(jsonContent);
                
                // Check if the JSON was deserialized properly
                if (assessmentTypesJson == null)
                {
                    Debug.WriteLine("Error: Failed to deserialize assessmentTypes.json");
                    return false;
                }
                
                // Initialize the AssessmentTypes list if it's null
                assessmentTypesJson.AssessmentTypes ??= new List<AssessmentTypeJson>();
                
                // Check if the assessment type already exists
                if (assessmentTypesJson.AssessmentTypes.Exists(t => string.Equals(t.Name, fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    Debug.WriteLine($"Assessment type '{fileName}' already exists in the JSON file");
                    return true; // Consider it a success if it already exists
                }
                
                // Create a new assessment type definition
                var newAssessmentType = new AssessmentTypeJson
                {
                    Name = fileName,
                    DisplayName = fileName,
                    JsonDataPath = $"{fileName}.json",
                    TemplateFileName = $"{fileName}.cshtml",
                    ModelDataInstance = $"{fileName}Data",
                    ModelAssessmentType = $"{fileName}Assessment"
                };
                
                // Add the new assessment type to the list
                assessmentTypesJson.AssessmentTypes.Add(newAssessmentType);
                
                // Serialize the updated list back to JSON
                var writeOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                string updatedJsonContent = JsonSerializer.Serialize(assessmentTypesJson, writeOptions);
                
                // Write the updated JSON content back to the file
                File.WriteAllText(assessmentTypesJsonPath, updatedJsonContent);
                
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
            public string Name { get; set; } = string.Empty;
            
            /// <summary>
            /// Display name for the assessment type
            /// </summary>
            public string DisplayName { get; set; } = string.Empty;
            
            /// <summary>
            /// Path to the JSON data file for this assessment type
            /// </summary>
            public string JsonDataPath { get; set; } = string.Empty;
            
            /// <summary>
            /// Path to the template file for this assessment type
            /// </summary>
            public string TemplateFileName { get; set; } = string.Empty;
            
            /// <summary>
            /// Name of the data instance model class
            /// </summary>
            public string ModelDataInstance { get; set; } = string.Empty;
            
            /// <summary>
            /// Name of the assessment type model class
            /// </summary>
            public string ModelAssessmentType { get; set; } = string.Empty;
        }
    }
}
