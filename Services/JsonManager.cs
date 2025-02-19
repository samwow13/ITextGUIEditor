using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly AssessmentType _assessmentType;

        public JsonManager(string jsonFilePath, AssessmentType assessmentType)
        {
            _jsonFilePath = jsonFilePath;
            _assessmentType = assessmentType;
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

                return _assessmentType switch
                {//ADD FORMS HERE
                    AssessmentType.OralCare => JsonSerializer.Deserialize<List<OralCareDataInstance>>(jsonContent, options).Cast<object>().ToList(),
                    AssessmentType.RegisteredNurseTaskAndDelegation => JsonSerializer.Deserialize<List<RegisteredNurseTaskDelegDataInstance>>(jsonContent, options).Cast<object>().ToList(),
                    AssessmentType.TestRazorDataInstance => JsonSerializer.Deserialize<List<TestRazorDataInstance>>(jsonContent, options).Cast<object>().ToList(),
                    _ => throw new ArgumentException($"Unsupported assessment type: {_assessmentType}")
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading reference data: {ex.Message}", ex);
            }
        }
    }
}
