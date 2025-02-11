using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using OralCareReference.Models;

namespace OralCareReference.Services
{
    /// <summary>
    /// Manages JSON file operations for oral care reference data
    /// </summary>
    public class JsonManager
    {
        private readonly string _jsonFilePath;

        public JsonManager(string jsonFilePath)
        {
            _jsonFilePath = jsonFilePath;
        }

        /// <summary>
        /// Loads reference data items from the JSON file
        /// </summary>
        /// <returns>List of reference data items</returns>
        public async Task<List<OralCareDataInstance>> LoadReferenceDataAsync()
        {
            try
            {
                string jsonContent = await File.ReadAllTextAsync(_jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<List<OralCareDataInstance>>(jsonContent, options);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading reference data: {ex.Message}", ex);
            }
        }
    }
}
