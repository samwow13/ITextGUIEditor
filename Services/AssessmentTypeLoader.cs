using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace iTextDesignerWithGUI.Services
{
    /// <summary>
    /// Loads assessment type definitions from a JSON file
    /// </summary>
    public class AssessmentTypeLoader
    {
        private readonly string _jsonFilePath;
        private static AssessmentTypeLoader _instance;
        private static readonly object _lock = new object();
        private List<AssessmentTypeDefinition> _cachedTypes;

        /// <summary>
        /// Gets the singleton instance of the AssessmentTypeLoader
        /// </summary>
        public static AssessmentTypeLoader Instance
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
                                
                            _instance = new AssessmentTypeLoader(jsonFilePath);
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the AssessmentTypeLoader class
        /// </summary>
        /// <param name="jsonFilePath">Path to the JSON file containing assessment type definitions</param>
        public AssessmentTypeLoader(string jsonFilePath)
        {
            _jsonFilePath = jsonFilePath ?? throw new ArgumentNullException(nameof(jsonFilePath));
        }

        /// <summary>
        /// Loads assessment type definitions from the JSON file
        /// </summary>
        /// <returns>List of assessment type definitions</returns>
        public async Task<List<AssessmentTypeDefinition>> LoadAssessmentTypesAsync()
        {
            if (_cachedTypes != null)
            {
                return _cachedTypes;
            }

            try
            {
                string jsonContent = await File.ReadAllTextAsync(_jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<AssessmentTypeContainer>(jsonContent, options);
                _cachedTypes = result?.AssessmentTypes ?? new List<AssessmentTypeDefinition>();
                return _cachedTypes;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading assessment types: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Loads assessment type definitions synchronously from the JSON file
        /// </summary>
        /// <returns>List of assessment type definitions</returns>
        public List<AssessmentTypeDefinition> LoadAssessmentTypes()
        {
            if (_cachedTypes != null)
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

                var result = JsonSerializer.Deserialize<AssessmentTypeContainer>(jsonContent, options);
                _cachedTypes = result?.AssessmentTypes ?? new List<AssessmentTypeDefinition>();
                return _cachedTypes;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading assessment types: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Adds a new assessment type to the JSON file
        /// </summary>
        /// <param name="typeName">Name of the assessment type</param>
        /// <param name="displayName">Display name for the assessment type</param>
        /// <returns>True if the operation was successful</returns>
        public async Task<bool> AddAssessmentTypeAsync(string typeName, string displayName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentException("Type name cannot be null or empty", nameof(typeName));
            }

            try
            {
                var types = await LoadAssessmentTypesAsync();
                
                // Check if type already exists
                if (types.Exists(t => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase)))
                {
                    return false; // Type already exists
                }

                // Add new type
                types.Add(new AssessmentTypeDefinition
                {
                    Name = typeName,
                    DisplayName = displayName ?? typeName
                });

                // Save back to file
                var container = new AssessmentTypeContainer
                {
                    AssessmentTypes = types
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonContent = JsonSerializer.Serialize(container, options);
                await File.WriteAllTextAsync(_jsonFilePath, jsonContent);
                
                // Update cache
                _cachedTypes = types;
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Removes an assessment type from the JSON file
        /// </summary>
        /// <param name="typeName">Name of the assessment type to remove</param>
        /// <returns>True if the operation was successful</returns>
        public async Task<bool> RemoveAssessmentTypeAsync(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentException("Type name cannot be null or empty", nameof(typeName));
            }

            try
            {
                var types = await LoadAssessmentTypesAsync();
                
                // Find and remove the type
                int initialCount = types.Count;
                types.RemoveAll(t => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase));
                
                if (types.Count == initialCount)
                {
                    return false; // Type not found
                }

                // Save back to file
                var container = new AssessmentTypeContainer
                {
                    AssessmentTypes = types
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonContent = JsonSerializer.Serialize(container, options);
                await File.WriteAllTextAsync(_jsonFilePath, jsonContent);
                
                // Update cache
                _cachedTypes = types;
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Container for assessment type definitions in the JSON file
    /// </summary>
    public class AssessmentTypeContainer
    {
        /// <summary>
        /// List of assessment type definitions
        /// </summary>
        public List<AssessmentTypeDefinition> AssessmentTypes { get; set; } = new List<AssessmentTypeDefinition>();
    }

    /// <summary>
    /// Definition of an assessment type
    /// </summary>
    public class AssessmentTypeDefinition
    {
        /// <summary>
        /// Name of the assessment type (used as the constant value)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Display name for the assessment type
        /// </summary>
        public string DisplayName { get; set; }
    }
}
