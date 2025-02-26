using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Represents a custom assessment type that can be added at runtime
    /// </summary>
    public class CustomAssessmentType
    {
        /// <summary>
        /// Gets or sets the unique identifier for this custom assessment type
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the display name for this custom assessment type
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the template file name for PDF generation
        /// </summary>
        public string TemplateFileName { get; set; }

        /// <summary>
        /// Gets or sets the JSON data file path for this assessment type
        /// </summary>
        public string JsonDataPath { get; set; }

        /// <summary>
        /// Creates a new instance of the CustomAssessmentType class
        /// </summary>
        public CustomAssessmentType()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Creates a new instance of the CustomAssessmentType class with the specified properties
        /// </summary>
        public CustomAssessmentType(string displayName, string templateFileName, string jsonDataPath)
        {
            Id = Guid.NewGuid().ToString();
            DisplayName = displayName;
            TemplateFileName = templateFileName;
            JsonDataPath = jsonDataPath;
        }
    }

    /// <summary>
    /// Manages custom assessment types that can be added at runtime
    /// </summary>
    public static class CustomAssessmentTypeManager
    {
        private static readonly string ConfigFilePath;
        private static List<CustomAssessmentType> _customTypes;

        /// <summary>
        /// Static constructor to initialize the configuration file path
        /// </summary>
        static CustomAssessmentTypeManager()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\.."));
            ConfigFilePath = Path.Combine(projectRoot, "PersistentDataJSON", "customAssessmentTypes.json");
            LoadCustomTypes();
        }

        /// <summary>
        /// Gets all custom assessment types
        /// </summary>
        public static IEnumerable<CustomAssessmentType> GetAllCustomTypes()
        {
            return _customTypes.ToList(); // Return a copy to prevent external modification
        }

        /// <summary>
        /// Adds a new custom assessment type
        /// </summary>
        public static bool AddCustomType(CustomAssessmentType customType)
        {
            if (customType == null || string.IsNullOrWhiteSpace(customType.DisplayName))
                return false;

            // Check for duplicates
            if (_customTypes.Any(t => t.DisplayName.Equals(customType.DisplayName, StringComparison.OrdinalIgnoreCase)))
                return false;

            _customTypes.Add(customType);
            SaveCustomTypes();
            return true;
        }

        /// <summary>
        /// Removes a custom assessment type by its ID
        /// </summary>
        public static bool RemoveCustomType(string id)
        {
            var typeToRemove = _customTypes.FirstOrDefault(t => t.Id == id);
            if (typeToRemove == null)
                return false;

            _customTypes.Remove(typeToRemove);
            SaveCustomTypes();
            return true;
        }

        /// <summary>
        /// Gets a custom assessment type by its ID
        /// </summary>
        public static CustomAssessmentType GetCustomTypeById(string id)
        {
            return _customTypes.FirstOrDefault(t => t.Id == id);
        }

        /// <summary>
        /// Loads custom assessment types from the configuration file
        /// </summary>
        private static void LoadCustomTypes()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                {
                    _customTypes = new List<CustomAssessmentType>();
                    return;
                }

                string jsonContent = File.ReadAllText(ConfigFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                _customTypes = JsonSerializer.Deserialize<List<CustomAssessmentType>>(jsonContent, options) ?? new List<CustomAssessmentType>();
            }
            catch (Exception)
            {
                // If there's an error loading, start with an empty list
                _customTypes = new List<CustomAssessmentType>();
            }
        }

        /// <summary>
        /// Saves custom assessment types to the configuration file
        /// </summary>
        private static void SaveCustomTypes()
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(ConfigFilePath);
                if (!Directory.Exists(directory) && directory != null)
                {
                    Directory.CreateDirectory(directory);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonContent = JsonSerializer.Serialize(_customTypes, options);
                File.WriteAllText(ConfigFilePath, jsonContent);
            }
            catch (Exception)
            {
                // Handle exception (could log or show message)
            }
        }
    }
}
