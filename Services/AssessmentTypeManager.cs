using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iTextDesignerWithGUI.Models;

namespace iTextDesignerWithGUI.Services
{
    /// <summary>
    /// Manages assessment types in the application
    /// </summary>
    public class AssessmentTypeManager
    {
        private readonly AssessmentTypeLoader _loader;

        /// <summary>
        /// Initializes a new instance of the AssessmentTypeManager class
        /// </summary>
        public AssessmentTypeManager()
        {
            _loader = AssessmentTypeLoader.Instance;
        }

        /// <summary>
        /// Gets all assessment types
        /// </summary>
        /// <returns>List of assessment type definitions</returns>
        public async Task<List<AssessmentTypeDefinition>> GetAllAssessmentTypesAsync()
        {
            return await _loader.LoadAssessmentTypesAsync();
        }

        /// <summary>
        /// Adds a new assessment type
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

            return await _loader.AddAssessmentTypeAsync(typeName, displayName);
        }

        /// <summary>
        /// Removes an assessment type
        /// </summary>
        /// <param name="typeName">Name of the assessment type to remove</param>
        /// <returns>True if the operation was successful</returns>
        public async Task<bool> RemoveAssessmentTypeAsync(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentException("Type name cannot be null or empty", nameof(typeName));
            }

            return await _loader.RemoveAssessmentTypeAsync(typeName);
        }

        /// <summary>
        /// Gets an assessment type by name
        /// </summary>
        /// <param name="typeName">Name of the assessment type</param>
        /// <returns>Assessment type definition or null if not found</returns>
        public async Task<AssessmentTypeDefinition> GetAssessmentTypeByNameAsync(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentException("Type name cannot be null or empty", nameof(typeName));
            }

            var types = await _loader.LoadAssessmentTypesAsync();
            return types.Find(t => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Checks if an assessment type exists
        /// </summary>
        /// <param name="typeName">Name of the assessment type</param>
        /// <returns>True if the assessment type exists</returns>
        public async Task<bool> AssessmentTypeExistsAsync(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return false;
            }

            var types = await _loader.LoadAssessmentTypesAsync();
            return types.Exists(t => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
