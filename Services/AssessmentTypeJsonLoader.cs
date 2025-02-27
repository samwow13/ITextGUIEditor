using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace iTextDesignerWithGUI.Services
{
    /// <summary>
    /// Service for loading assessment types directly from the assessmentTypes.json file
    /// </summary>
    public class AssessmentTypeJsonLoader
    {
        private readonly string _jsonFilePath;
        private static AssessmentTypeJsonLoader _instance;
        private static readonly object _lock = new object();
        private List<AssessmentTypeJsonDefinition> _cachedTypes;

        /// <summary>
        /// Gets the singleton instance of the AssessmentTypeJsonLoader
        /// </summary>
        public static AssessmentTypeJsonLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            // Use ProjectDirectoryService to get the project root directory
                            var directoryService = new ProjectDirectoryService();
                            
                            // Create the path to the assessmentTypes.json file
                            string jsonFilePath = Path.Combine(
                                directoryService.GetDirectory("PersistentDataJSON"),
                                "assessmentTypes.json");
                                
                            _instance = new AssessmentTypeJsonLoader(jsonFilePath);
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the AssessmentTypeJsonLoader class
        /// </summary>
        /// <param name="jsonFilePath">Path to the JSON file containing assessment type definitions</param>
        public AssessmentTypeJsonLoader(string jsonFilePath)
        {
            _jsonFilePath = jsonFilePath ?? throw new ArgumentNullException(nameof(jsonFilePath));
        }

        /// <summary>
        /// Loads assessment type definitions from the JSON file
        /// </summary>
        /// <param name="forceRefresh">If true, forces a refresh of the cached data</param>
        /// <returns>List of assessment type definitions</returns>
        public List<AssessmentTypeJsonDefinition> LoadAssessmentTypes(bool forceRefresh = false)
        {
            if (_cachedTypes != null && !forceRefresh)
            {
                return _cachedTypes;
            }

            try
            {
                string jsonContent = File.ReadAllText(_jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<AssessmentTypeJsonContainer>(jsonContent, options);
                _cachedTypes = result?.AssessmentTypes ?? new List<AssessmentTypeJsonDefinition>();
                return _cachedTypes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading assessment types from JSON: {ex.Message}");
                return new List<AssessmentTypeJsonDefinition>();
            }
        }

        /// <summary>
        /// Gets an assessment type definition by name
        /// </summary>
        /// <param name="typeName">Name of the assessment type</param>
        /// <returns>Assessment type definition or null if not found</returns>
        public AssessmentTypeJsonDefinition GetAssessmentTypeByName(string typeName)
        {
            var types = LoadAssessmentTypes();
            return types.Find(t => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Container for assessment type definitions in the JSON file
    /// </summary>
    public class AssessmentTypeJsonContainer
    {
        /// <summary>
        /// List of assessment type definitions
        /// </summary>
        public List<AssessmentTypeJsonDefinition> AssessmentTypes { get; set; } = new List<AssessmentTypeJsonDefinition>();
    }

    /// <summary>
    /// Definition of an assessment type from the JSON file with all properties
    /// </summary>
    public class AssessmentTypeJsonDefinition
    {
        /// <summary>
        /// Name of the assessment type (used as the constant value)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Display name for the assessment type
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Path to the assessment type class file
        /// </summary>
        public string AssessmentTypeDirectory { get; set; }

        /// <summary>
        /// Path to the assessment data instance class file
        /// </summary>
        public string AssessmentDataInstanceDirectory { get; set; }

        /// <summary>
        /// Path to the CSHTML template file
        /// </summary>
        public string CshtmlTemplateDirectory { get; set; }

        /// <summary>
        /// Path to the JSON data file for this assessment type
        /// </summary>
        public string JsonDataLocationDirectory { get; set; }
    }
}
