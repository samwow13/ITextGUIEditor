using System;
using System.IO;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.Generic;

namespace iTextDesignerWithGUI.Services
{
    /// <summary>
    /// Service for removing assessment templates from the system
    /// </summary>
    public class TemplateRemovalService
    {
        private readonly string _projectRoot;
        private readonly string _assessmentTypesJsonPath;
        private readonly ProjectDirectoryService _directoryService;

        /// <summary>
        /// Constructor for TemplateRemovalService
        /// </summary>
        public TemplateRemovalService()
        {
            _projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\.."));
            _assessmentTypesJsonPath = Path.Combine(_projectRoot, "PersistentDataJSON", "assessmentTypes.json");
            _directoryService = new ProjectDirectoryService();

            Debug.WriteLine($"TemplateRemovalService initialized with path: {_assessmentTypesJsonPath}");
        }

        /// <summary>
        /// Removes a template from the assessmentTypes.json file and its associated files
        /// </summary>
        /// <param name="templateName">The name of the template to remove</param>
        /// <returns>True if the removal was successful, false otherwise</returns>
        public bool RemoveTemplate(string templateName)
        {
            try
            {
                Debug.WriteLine($"Attempting to remove template '{templateName}' from {_assessmentTypesJsonPath}");

                // Check if the file exists
                if (!File.Exists(_assessmentTypesJsonPath))
                {
                    Debug.WriteLine($"Error: Assessment types JSON file not found at {_assessmentTypesJsonPath}");
                    return false;
                }

                // Read the existing JSON file
                string jsonContent = File.ReadAllText(_assessmentTypesJsonPath);

                // Store file paths to remove
                string templateType = "";
                string cshtmlTemplatePath = "";
                string jsonDataPath = "";
                string modelsDirectory = "";
                bool templateFound = false;

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
                            
                            // Copy all existing assessment types except the one to be removed
                            if (document.RootElement.TryGetProperty("assessmentTypes", out JsonElement assessmentTypes))
                            {
                                foreach (JsonElement assessmentType in assessmentTypes.EnumerateArray())
                                {
                                    // Check if this is the assessment type to remove
                                    if (assessmentType.TryGetProperty("name", out JsonElement nameElement) &&
                                        string.Equals(nameElement.GetString(), templateName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        templateFound = true;
                                        
                                        // Capture the file paths before skipping this element
                                        if (assessmentType.TryGetProperty("cshtmlTemplateDirectory", out JsonElement cshtmlTemplateElement))
                                        {
                                            cshtmlTemplatePath = cshtmlTemplateElement.GetString();
                                            
                                            // Extract the template type from the path (e.g., "Templates/HealthAndWellness/...")
                                            string[] pathParts = cshtmlTemplatePath.Split('/');
                                            if (pathParts.Length > 1)
                                            {
                                                templateType = pathParts[1]; // Second part is the template type
                                            }
                                        }
                                        
                                        if (assessmentType.TryGetProperty("jsonDataLocationDirectory", out JsonElement jsonDataElement))
                                        {
                                            jsonDataPath = jsonDataElement.GetString();
                                        }
                                        
                                        if (assessmentType.TryGetProperty("assessmentTypeDirectory", out JsonElement assessmentTypeElement))
                                        {
                                            string assessmentTypePath = assessmentTypeElement.GetString();
                                            string[] pathParts = assessmentTypePath.Split('/');
                                            if (pathParts.Length > 2)
                                            {
                                                // Extract the models directory from "Models/TemplateType/NameModels/..."
                                                modelsDirectory = $"Models/{pathParts[1]}/{pathParts[2]}";
                                            }
                                        }
                                        
                                        Debug.WriteLine($"Found template '{templateName}' to remove");
                                        // Skip this element as we want to remove it
                                        continue;
                                    }
                                    
                                    // Copy the existing assessment type as-is
                                    assessmentType.WriteTo(writer);
                                }
                            }
                            
                            // End the assessmentTypes array
                            writer.WriteEndArray();
                            
                            // End the root object
                            writer.WriteEndObject();
                            
                            // If the template wasn't found, return false
                            if (!templateFound)
                            {
                                Debug.WriteLine($"Template '{templateName}' not found in assessment types JSON");
                                return false;
                            }
                        }
                        
                        // Get the JSON as a string
                        ms.Position = 0;
                        using (StreamReader reader = new StreamReader(ms))
                        {
                            string updatedJsonContent = reader.ReadToEnd();
                            
                            // Write the updated JSON back to the file
                            File.WriteAllText(_assessmentTypesJsonPath, updatedJsonContent);
                        }
                    }
                }
                
                // Now remove the associated files
                if (templateFound)
                {
                    DeleteTemplateFiles(templateName, templateType, cshtmlTemplatePath, jsonDataPath, modelsDirectory);
                }
                
                Debug.WriteLine($"Successfully removed template '{templateName}' from assessment types JSON");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing template from JSON file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete the files and directories associated with a template
        /// </summary>
        /// <param name="templateName">Name of the template</param>
        /// <param name="templateType">Type of the template (e.g., "HealthAndWellness")</param>
        /// <param name="cshtmlTemplatePath">Path to the cshtml template file</param>
        /// <param name="jsonDataPath">Path to the JSON data file</param>
        /// <param name="modelsDirectory">Path to the models directory</param>
        private void DeleteTemplateFiles(string templateName, string templateType, string cshtmlTemplatePath, string jsonDataPath, string modelsDirectory)
        {
            List<string> deletionResults = new List<string>();
            
            try
            {
                // 1. Delete the cshtml template file
                if (!string.IsNullOrEmpty(cshtmlTemplatePath))
                {
                    string fullTemplatePath = Path.Combine(_projectRoot, cshtmlTemplatePath.Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(fullTemplatePath))
                    {
                        File.Delete(fullTemplatePath);
                        deletionResults.Add($"Deleted cshtml template: {cshtmlTemplatePath}");
                    }
                    else
                    {
                        deletionResults.Add($"Cshtml template not found: {cshtmlTemplatePath}");
                    }
                }
                
                // 2. Delete the JSON data file
                if (!string.IsNullOrEmpty(jsonDataPath))
                {
                    string fullJsonPath = Path.Combine(_projectRoot, jsonDataPath.Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(fullJsonPath))
                    {
                        File.Delete(fullJsonPath);
                        deletionResults.Add($"Deleted JSON data file: {jsonDataPath}");
                    }
                    else
                    {
                        deletionResults.Add($"JSON data file not found: {jsonDataPath}");
                    }
                }
                
                // 3. Delete the models directory (contains all model files)
                if (!string.IsNullOrEmpty(modelsDirectory))
                {
                    string fullModelsPath = Path.Combine(_projectRoot, modelsDirectory.Replace('/', Path.DirectorySeparatorChar));
                    if (Directory.Exists(fullModelsPath))
                    {
                        // Use recursive delete to remove the directory and all its contents
                        Directory.Delete(fullModelsPath, true);
                        deletionResults.Add($"Deleted models directory: {modelsDirectory}");
                    }
                    else
                    {
                        deletionResults.Add($"Models directory not found: {modelsDirectory}");
                    }
                }
                
                // Log all deletion results
                foreach (string result in deletionResults)
                {
                    Debug.WriteLine(result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting template files: {ex.Message}");
                
                // Log the deletion results up to the error
                foreach (string result in deletionResults)
                {
                    Debug.WriteLine(result);
                }
            }
        }
    }
}
