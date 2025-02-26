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
                
                // Construct the target file path
                string targetFilePath = Path.Combine(targetDirectory, $"{fileName}.json");
                
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
            // Create a sample data model with example data
            var sampleData = new List<object>
            {
                new
                {
                    model = new
                    {
                        id = 1,
                        name = fileName,
                        description = $"Sample data for {fileName}",
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
                }
            };
            
            // Serialize the object to JSON with proper formatting
            return JsonSerializer.Serialize(sampleData, new JsonSerializerOptions 
            { 
                WriteIndented = true
            });
        }
    }
}
