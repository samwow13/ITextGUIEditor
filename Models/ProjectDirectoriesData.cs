using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Model representing the project directories data structure from pdfCreationData.json
    /// </summary>
    public class ProjectDirectoriesData
    {
        /// <summary>
        /// List of project directories
        /// </summary>
        [JsonPropertyName("projectDirectories")]
        public List<ProjectDirectory> ProjectDirectories { get; set; } = new List<ProjectDirectory>();
    }

    /// <summary>
    /// Model representing a single project directory entry
    /// </summary>
    public class ProjectDirectory
    {
        /// <summary>
        /// The name of the project
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// The relative path to the project directory
        /// </summary>
        [JsonPropertyName("path")]
        public string Path { get; set; }
    }
}
