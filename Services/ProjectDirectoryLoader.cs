using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace iTextDesignerWithGUI.Services
{
    /// <summary>
    /// Service for loading project directories from the pdfCreationData.json file
    /// </summary>
    public class ProjectDirectoryLoader
    {
        private readonly string _jsonFilePath;
        private static ProjectDirectoryLoader _instance;
        private static readonly object _lock = new object();
        private List<ProjectDirectoryDefinition> _cachedDirectories;

        /// <summary>
        /// Gets the singleton instance of the ProjectDirectoryLoader
        /// </summary>
        public static ProjectDirectoryLoader Instance
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
                            
                            // Create the path to the pdfCreationData.json file
                            string jsonFilePath = Path.Combine(
                                directoryService.GetDirectory("PersistentDataJSON"),
                                "pdfCreationData.json");
                                
                            _instance = new ProjectDirectoryLoader(jsonFilePath);
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ProjectDirectoryLoader class
        /// </summary>
        /// <param name="jsonFilePath">Path to the JSON file containing project directory definitions</param>
        public ProjectDirectoryLoader(string jsonFilePath)
        {
            _jsonFilePath = jsonFilePath ?? throw new ArgumentNullException(nameof(jsonFilePath));
        }

        /// <summary>
        /// Loads project directory definitions from the JSON file
        /// </summary>
        /// <param name="forceRefresh">If true, forces a refresh of the cached data</param>
        /// <returns>List of project directory definitions</returns>
        public List<ProjectDirectoryDefinition> LoadProjectDirectories(bool forceRefresh = false)
        {
            if (_cachedDirectories != null && !forceRefresh)
            {
                return _cachedDirectories;
            }

            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Project directories JSON file not found at: {_jsonFilePath}");
                    return new List<ProjectDirectoryDefinition>();
                }

                string jsonContent = File.ReadAllText(_jsonFilePath);
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    System.Diagnostics.Debug.WriteLine("Project directories JSON file is empty");
                    return new List<ProjectDirectoryDefinition>();
                }

                System.Diagnostics.Debug.WriteLine($"Loading project directories from: {_jsonFilePath}");
                System.Diagnostics.Debug.WriteLine($"JSON Content: {jsonContent}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                var result = JsonSerializer.Deserialize<ProjectDirectoryContainer>(jsonContent, options);
                
                if (result == null)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to deserialize project directories JSON (null result)");
                    return new List<ProjectDirectoryDefinition>();
                }
                
                if (result.ProjectDirectories == null)
                {
                    System.Diagnostics.Debug.WriteLine("ProjectDirectories list is null in deserialized JSON");
                    return new List<ProjectDirectoryDefinition>();
                }

                _cachedDirectories = result.ProjectDirectories;
                System.Diagnostics.Debug.WriteLine($"Successfully loaded {_cachedDirectories.Count} project directories");
                return _cachedDirectories;
            }
            catch (JsonException jsonEx)
            {
                System.Diagnostics.Debug.WriteLine($"JSON error loading project directories: {jsonEx.Message}");
                return new List<ProjectDirectoryDefinition>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading project directories from JSON: {ex.Message}");
                return new List<ProjectDirectoryDefinition>();
            }
        }

        /// <summary>
        /// Gets a project directory definition by name
        /// </summary>
        /// <param name="projectName">Name of the project directory</param>
        /// <returns>Project directory definition or null if not found</returns>
        public ProjectDirectoryDefinition GetProjectDirectoryByName(string projectName)
        {
            var directories = LoadProjectDirectories();
            return directories.Find(d => string.Equals(d.Name, projectName, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Container for project directory definitions in the JSON file
    /// </summary>
    public class ProjectDirectoryContainer
    {
        /// <summary>
        /// List of project directory definitions
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("projectDirectories")]
        public List<ProjectDirectoryDefinition> ProjectDirectories { get; set; } = new List<ProjectDirectoryDefinition>();
    }

    /// <summary>
    /// Definition of a project directory from the JSON file
    /// </summary>
    public class ProjectDirectoryDefinition
    {
        /// <summary>
        /// Name of the project
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Path to the project directory
        /// </summary>
        public string Path { get; set; }
    }
}
