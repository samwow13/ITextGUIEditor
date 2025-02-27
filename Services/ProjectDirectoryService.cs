using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Linq;

namespace iTextDesignerWithGUI.Services
{
    /// <summary>
    /// Service for finding and managing project directories.
    /// Provides standardized access to project directories without relying on bin folder paths.
    /// </summary>
    public class ProjectDirectoryService
    {
        private readonly string _rootDirectory;
        private readonly string _executableDirectory;
        
        /// <summary>
        /// Initializes a new instance of the ProjectDirectoryService.
        /// Automatically detects the project root directory.
        /// </summary>
        public ProjectDirectoryService()
        {
            _executableDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _rootDirectory = FindProjectRootDirectory();
            Debug.WriteLine($"Project root directory initialized: {_rootDirectory}");
            Debug.WriteLine($"Executable directory initialized: {_executableDirectory}");
        }

        /// <summary>
        /// Gets the absolute path to the project's root directory.
        /// </summary>
        /// <returns>The absolute path to the project root directory.</returns>
        public string GetRootDirectory()
        {
            return _rootDirectory;
        }

        /// <summary>
        /// Gets the absolute path to the executable's directory (bin folder).
        /// </summary>
        /// <returns>The absolute path to the executable directory.</returns>
        public string GetExecutablePath()
        {
            return _executableDirectory;
        }

        /// <summary>
        /// Gets the absolute path to a specific directory within the project.
        /// </summary>
        /// <param name="directoryName">The name or relative path of the directory within the project.</param>
        /// <returns>The absolute path to the specified directory.</returns>
        public string GetDirectory(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
            {
                return _rootDirectory;
            }

            string fullPath = Path.Combine(_rootDirectory, directoryName);
            
            // Ensure the directory exists
            if (!Directory.Exists(fullPath))
            {
                Debug.WriteLine($"Warning: Directory does not exist: {fullPath}");
            }

            return fullPath;
        }

        /// <summary>
        /// Creates a directory if it doesn't exist and returns its absolute path.
        /// </summary>
        /// <param name="directoryName">The name or relative path of the directory to create.</param>
        /// <returns>The absolute path to the directory.</returns>
        public string EnsureDirectoryExists(string directoryName)
        {
            string fullPath = GetDirectory(directoryName);
            
            if (!Directory.Exists(fullPath))
            {
                Debug.WriteLine($"Creating directory: {fullPath}");
                Directory.CreateDirectory(fullPath);
            }
            
            return fullPath;
        }

        /// <summary>
        /// Gets the absolute path to a file within the project.
        /// </summary>
        /// <param name="relativePath">The relative path to the file from the project root.</param>
        /// <returns>The absolute path to the file.</returns>
        public string GetFilePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentNullException(nameof(relativePath), "File path cannot be null or empty");
            }

            return Path.Combine(_rootDirectory, relativePath);
        }

        /// <summary>
        /// Checks if a path is within the project directory.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>True if the path is within the project directory, false otherwise.</returns>
        public bool IsPathWithinProject(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            string fullPath = Path.GetFullPath(path);
            return fullPath.StartsWith(_rootDirectory, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Finds the root directory of the project by navigating up from the executing assembly location
        /// and looking for key project indicators.
        /// </summary>
        /// <returns>The absolute path to the project root directory.</returns>
        private string FindProjectRootDirectory()
        {
            // Start with the directory of the executing assembly
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Debug.WriteLine($"Starting directory search from: {currentDirectory}");

            // Navigate up until we find the project root
            while (!string.IsNullOrEmpty(currentDirectory))
            {
                // If we're in a bin directory, move up to its parent
                if (Path.GetFileName(currentDirectory).Equals("bin", StringComparison.OrdinalIgnoreCase))
                {
                    var parent = Directory.GetParent(currentDirectory);
                    if (parent != null)
                    {
                        currentDirectory = parent.FullName;
                        Debug.WriteLine($"Moving up from bin directory to: {currentDirectory}");
                    }
                    else
                    {
                        throw new DirectoryNotFoundException("Could not navigate up from bin directory");
                    }
                }

                // Check for key project indicators
                if (IsLikelyProjectRoot(currentDirectory))
                {
                    Debug.WriteLine($"Found project root at: {currentDirectory}");
                    return currentDirectory;
                }

                // Move up one directory
                var parentDir = Directory.GetParent(currentDirectory);
                if (parentDir == null)
                {
                    // We've reached the root of the drive
                    throw new DirectoryNotFoundException("Could not find project root directory");
                }

                currentDirectory = parentDir.FullName;
            }

            throw new DirectoryNotFoundException("Could not find project root directory");
        }

        /// <summary>
        /// Checks if a directory is likely to be the project root by looking for key indicators.
        /// </summary>
        /// <param name="directory">The directory to check.</param>
        /// <returns>True if the directory is likely the project root, false otherwise.</returns>
        private bool IsLikelyProjectRoot(string directory)
        {
            // Check for presence of key project directories
            bool hasServicesDir = Directory.Exists(Path.Combine(directory, "Services"));
            bool hasFormsDir = Directory.Exists(Path.Combine(directory, "Forms"));
            bool hasModelsDir = Directory.Exists(Path.Combine(directory, "Models"));
            bool hasTemplatesDir = Directory.Exists(Path.Combine(directory, "Templates"));
            
            // Check for presence of .csproj file
            bool hasCsprojFile = Directory.GetFiles(directory, "*.csproj").Any();
            
            // A directory is likely the project root if it has most of these indicators
            int score = 0;
            if (hasServicesDir) score++;
            if (hasFormsDir) score++;
            if (hasModelsDir) score++;
            if (hasTemplatesDir) score++;
            if (hasCsprojFile) score++;
            
            // Consider it the project root if it has at least 3 indicators
            return score >= 3;
        }
    }
}
