using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using iTextDesignerWithGUI.Models;
using iTextDesignerWithGUI.Models.TestRazorDataModels;

namespace iTextDesignerWithGUI.Services
{
    /// <summary>
    /// Manages JSON file operations for reference data
    /// </summary>
    public class JsonManager
    {
        private readonly string _jsonFilePath;
        private readonly AssessmentTypeWrapper _assessmentTypeWrapper;

        /// <summary>
        /// Initializes a new instance of the JsonManager class
        /// </summary>
        /// <param name="jsonFilePath">Path to the JSON file containing reference data</param>
        /// <param name="assessmentTypeWrapper">The assessment type wrapper</param>
        public JsonManager(string jsonFilePath, AssessmentTypeWrapper assessmentTypeWrapper)
        {
            _jsonFilePath = jsonFilePath;
            _assessmentTypeWrapper = assessmentTypeWrapper ?? throw new ArgumentNullException(nameof(assessmentTypeWrapper));
        }

        /// <summary>
        /// Loads reference data items from the JSON file based on assessment type
        /// </summary>
        /// <returns>List of reference data items</returns>
        public async Task<List<object>> LoadReferenceDataAsync()
        {
            try
            {
                string jsonContent = await File.ReadAllTextAsync(_jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Handle both built-in and custom assessment types
                if (_assessmentTypeWrapper.IsBuiltIn && !string.IsNullOrEmpty(_assessmentTypeWrapper.BuiltInType))
                {
                    // Get the type name, which is the string representation of the enum
                    string typeName = _assessmentTypeWrapper.TypeName ?? _assessmentTypeWrapper.BuiltInType;
                    
                    // Try to get the model data type from the assessment type wrapper
                    Type modelDataType = null;
                    
                    if (_assessmentTypeWrapper.JsonDefinition != null)
                    {
                        // Try to find the data instance type based on the path in the JSON definition
                        string dataInstancePath = _assessmentTypeWrapper.JsonDefinition.AssessmentDataInstanceDirectory;
                        if (!string.IsNullOrEmpty(dataInstancePath))
                        {
                            // Extract the class name from the path
                            string className = System.IO.Path.GetFileNameWithoutExtension(dataInstancePath);
                            
                            // Try to find the type by name
                            modelDataType = Assembly.GetExecutingAssembly()
                                .GetTypes()
                                .FirstOrDefault(t => t.Name.Equals(className, StringComparison.OrdinalIgnoreCase));
                        }
                    }
                    
                    // If we couldn't find the type from the JSON definition, try to infer it
                    if (modelDataType == null)
                    {
                        // Get the assessment type name
                        string typeNameLocal = _assessmentTypeWrapper.TypeName;
                        
                        // Try to find a matching data instance type
                        modelDataType = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .FirstOrDefault(t => t.Name.Equals(typeNameLocal + "DataInstance", StringComparison.OrdinalIgnoreCase));
                    }
                    
                    // If we found a valid model data type, use it for deserialization
                    if (modelDataType != null)
                    {
                        // Use generic method to deserialize to the correct type
                        var listType = typeof(List<>).MakeGenericType(modelDataType);
                        var deserializeMethod = typeof(JsonSerializer).GetMethod("Deserialize", new[] { typeof(string), typeof(JsonSerializerOptions) });
                        var genericMethod = deserializeMethod.MakeGenericMethod(listType);
                        
                        // Deserialize to the specific list type
                        var typedList = genericMethod.Invoke(null, new object[] { jsonContent, options });
                        
                        // Convert to List<object>
                        var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(typeof(object));
                        var castedList = castMethod.Invoke(null, new[] { typedList });
                        
                        // Convert to List<object>
                        var toListMethod = typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(typeof(object));
                        return (List<object>)toListMethod.Invoke(null, new[] { castedList });
                    }
                    
                    // Fallback to deserializing as JsonElement list if we couldn't determine the type
                    var elements = JsonSerializer.Deserialize<List<JsonElement>>(jsonContent, options);
                    return elements.Cast<object>().ToList();
                }
                else if (!string.IsNullOrEmpty(_assessmentTypeWrapper.CustomTypeId))
                {
                    // For custom types, we'll use a generic approach to deserialize the JSON
                    // This assumes custom types use a common data structure or can be handled generically
                    // You may need to adjust this based on your specific requirements
                    return JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(jsonContent, options)
                        .Cast<object>()
                        .ToList();
                }

                throw new InvalidOperationException("Invalid assessment type wrapper state");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading reference data: {ex.Message}", ex);
            }
        }
    }
}
