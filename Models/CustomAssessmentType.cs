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
            string projectRoot = Path.GetFullPath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..")
            );
            ConfigFilePath = Path.Combine(
                projectRoot,
                "PersistentDataJSON",
                "customAssessmentTypes.json"
            );
            LoadCustomTypes();
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
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                _customTypes =
                    JsonSerializer.Deserialize<List<CustomAssessmentType>>(jsonContent, options)
                    ?? new List<CustomAssessmentType>();
            }
            catch (Exception)
            {
                // If there's an error loading, start with an empty list
                _customTypes = new List<CustomAssessmentType>();
            }
        }

    }
}
