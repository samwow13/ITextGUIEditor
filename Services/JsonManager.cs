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
                if (_assessmentTypeWrapper.IsBuiltIn && _assessmentTypeWrapper.BuiltInType.HasValue)
                {
                    // Get the type name, which is the string representation of the enum
                    string typeName = _assessmentTypeWrapper.TypeName ?? _assessmentTypeWrapper.BuiltInType.Value.ToString();
                    
                    // Use a string-based switch for better compatibility with changes to the enum
                    switch (typeName)
                    {//ADD FORMS HERE
                        case AssessmentTypeConstants.OralCare:
                            return JsonSerializer.Deserialize<List<OralCareDataInstance>>(jsonContent, options).Cast<object>().ToList();
                        case AssessmentTypeConstants.RegisteredNurseTaskAndDelegation:
                            return JsonSerializer.Deserialize<List<RegisteredNurseTaskDelegDataInstance>>(jsonContent, options).Cast<object>().ToList();
                        case AssessmentTypeConstants.TestRazorDataInstance:
                            return JsonSerializer.Deserialize<List<TestRazorDataInstance>>(jsonContent, options).Cast<object>().ToList();
                        case AssessmentTypeConstants.Tester:
                            // Try to find the appropriate data instance type for Tester
                            var testerInstanceType = Assembly.GetExecutingAssembly()
                                .GetTypes()
                                .FirstOrDefault(t => t.Name.Contains("testerInstance") && !t.IsInterface && !t.IsAbstract);
                                
                            if (testerInstanceType != null)
                            {
                                // Use generic method to deserialize to the correct type
                                var genericMethod = typeof(JsonSerializer).GetMethod("Deserialize", new[] { typeof(string), typeof(JsonSerializerOptions) })
                                    .MakeGenericMethod(typeof(List<>).MakeGenericType(testerInstanceType));
                                
                                var result = genericMethod.Invoke(null, new object[] { jsonContent, options });
                                // Need to cast to list of objects for compatibility
                                return ((IEnumerable<object>)result).Cast<object>().ToList();
                            }
                            throw new ArgumentException($"Could not find appropriate data instance type for Tester");
                        default:
                            throw new ArgumentException($"Unsupported built-in assessment type: {typeName}");
                    }
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
