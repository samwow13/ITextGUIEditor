using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace iTextDesignerWithGUI.Services
{
    /// <summary>
    /// Service to generate HTML files based on user input
    /// </summary>
    public class HtmlGenerationService
    {
        private readonly string _projectRootPath;

        /// <summary>
        /// Constructor for HtmlGenerationService
        /// </summary>
        /// <param name="projectRootPath">Root path of the project. If null, it will be determined automatically</param>
        public HtmlGenerationService(string? projectRootPath = null)
        {
            // If root path is not provided, determine it from the executing assembly location
            _projectRootPath = projectRootPath ?? DetermineProjectRootPath();
            Debug.WriteLine($"HtmlGenerationService initialized with root path: {_projectRootPath}");
        }

        /// <summary>
        /// Determines the project root path automatically
        /// </summary>
        /// <returns>Root path of the project</returns>
        private string DetermineProjectRootPath()
        {
            try
            {
                // Navigate from the bin directory up to the actual project root
                // This matches the approach used in MainForm for TemplateWatcherService
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                
                // Go up three directories: bin/Debug/net6.0-windows -> project root
                string projectPath = Path.Combine(baseDirectory, "..\\..\\..\\");
                string fullPath = Path.GetFullPath(projectPath);
                
                Debug.WriteLine($"Determined project root path: {fullPath}");
                
                // Validate that we found the correct directory by checking for Templates
                if (!Directory.Exists(Path.Combine(fullPath, "Templates")))
                {
                    Debug.WriteLine("Warning: Templates directory not found in determined project path");
                }
                
                return fullPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error determining project root path: {ex.Message}");
                // Fallback to the current directory if we encounter an error
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        /// <summary>
        /// Generates a basic HTML file with the specified name in the given template directory
        /// </summary>
        /// <param name="fileName">Name of the file to generate (without extension)</param>
        /// <param name="templateType">Type of template (e.g., "HealthAndWellness")</param>
        /// <returns>True if generation was successful, false otherwise</returns>
        public bool GenerateHtmlFile(string fileName, string templateType)
        {
            try
            {
                // Construct the target directory path
                string targetDirectory = Path.Combine(_projectRootPath, "Templates", templateType);
                
                // Ensure the directory exists
                if (!Directory.Exists(targetDirectory))
                {
                    Debug.WriteLine($"Creating directory: {targetDirectory}");
                    Directory.CreateDirectory(targetDirectory);
                }
                
                // Construct the target file path
                string targetFilePath = Path.Combine(targetDirectory, $"{fileName}.html");
                
                Debug.WriteLine($"Generating HTML file at: {targetFilePath}");
                
                // Generate a basic HTML file
                string htmlContent = GenerateBasicHtmlContent(fileName);
                
                // Write the HTML content to the file
                File.WriteAllText(targetFilePath, htmlContent);
                
                Debug.WriteLine($"Successfully generated HTML file: {targetFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generating HTML file: {ex.Message}");
                MessageBox.Show($"Error generating HTML file: {ex.Message}", "Generation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Generates basic HTML content for a new file
        /// </summary>
        /// <param name="fileName">Name of the file (used as the title)</param>
        /// <returns>String containing HTML content</returns>
        private string GenerateBasicHtmlContent(string fileName)
        {
            return $@"<!DOCTYPE html>
<html lang=""en"">
  <head>
    <meta charset=""UTF-8"">
    <title>{fileName}</title>
    <link href=""globalStyles.css"" rel=""stylesheet"">
  </head>
  <body>
    <div class=""container"">
      <!-- Header with Title -->
      <header>
        <div class=""title"">
          {fileName}
        </div>
      </header>

      <!-- Content goes here -->
      <div class=""content"">
        <p>This is a template file for {fileName}.</p>
      </div>
    </div>
  </body>
</html>";
        }
    }
}
