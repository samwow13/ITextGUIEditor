using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Represents the progress state for a specific JSON document
    /// </summary>
    public class ChecklistProgress
    {
        public string DocumentId { get; set; }
        public string DocumentType { get; set; }
        public Dictionary<string, bool> CheckStates { get; set; }
        public DateTime LastModified { get; set; }

        public ChecklistProgress()
        {
            CheckStates = new Dictionary<string, bool>();
            LastModified = DateTime.Now;
        }
    }

    /// <summary>
    /// Manages the storage and retrieval of checklist progress data
    /// </summary>
    public class ChecklistProgressManager
    {
        private readonly string _storageDirectory;
        private const string FILE_EXTENSION = ".progress.json";

        public ChecklistProgressManager(string storageDirectory)
        {
            _storageDirectory = storageDirectory;
            if (!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
            }
        }

        /// <summary>
        /// Generates a unique document ID based on the document type and content
        /// </summary>
        public string GenerateDocumentId(string documentType, object data)
        {
            var jsonString = JsonSerializer.Serialize(data);
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
            var hash = sha.ComputeHash(bytes);
            return $"{documentType}_{BitConverter.ToString(hash).Replace("-", "").Substring(0, 16)}";
        }

        /// <summary>
        /// Saves the progress for a specific document
        /// </summary>
        public void SaveProgress(ChecklistProgress progress)
        {
            var filePath = Path.Combine(_storageDirectory, progress.DocumentId + FILE_EXTENSION);
            var jsonString = JsonSerializer.Serialize(progress, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(filePath, jsonString);
        }

        /// <summary>
        /// Loads the progress for a specific document
        /// </summary>
        public ChecklistProgress LoadProgress(string documentId)
        {
            var filePath = Path.Combine(_storageDirectory, documentId + FILE_EXTENSION);
            if (!File.Exists(filePath))
            {
                return null;
            }

            var jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<ChecklistProgress>(jsonString);
        }

        /// <summary>
        /// Gets all saved progress files
        /// </summary>
        public List<ChecklistProgress> GetAllProgress()
        {
            var result = new List<ChecklistProgress>();
            var files = Directory.GetFiles(_storageDirectory, $"*{FILE_EXTENSION}");
            
            foreach (var file in files)
            {
                try
                {
                    var jsonString = File.ReadAllText(file);
                    var progress = JsonSerializer.Deserialize<ChecklistProgress>(jsonString);
                    result.Add(progress);
                }
                catch (Exception)
                {
                    // Skip invalid files
                    continue;
                }
            }

            return result;
        }
    }
}
