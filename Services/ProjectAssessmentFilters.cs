using System;
using System.Collections.Generic;
using System.Linq;
using iTextDesignerWithGUI.Models;

namespace iTextDesignerWithGUI.Services
{
    /// <summary>
    /// Service for filtering assessment types by project
    /// </summary>
    public class ProjectAssessmentFilters
    {
        /// <summary>
        /// Filters the assessment types by project name
        /// </summary>
        /// <param name="assessmentTypes">List of assessment types to filter</param>
        /// <param name="projectName">Project name to filter by</param>
        /// <returns>Filtered list of assessment types</returns>
        public static List<AssessmentTypeWrapper> FilterByProject(
            List<AssessmentTypeWrapper> assessmentTypes, 
            string projectName)
        {
            if (assessmentTypes == null)
            {
                return new List<AssessmentTypeWrapper>();
            }

            if (string.IsNullOrEmpty(projectName))
            {
                return assessmentTypes; // Return all if no project is specified
            }

            return assessmentTypes
                .Where(at => BelongsToProject(at, projectName))
                .ToList();
        }

        /// <summary>
        /// Determines if an assessment type belongs to a specific project
        /// </summary>
        /// <param name="assessmentType">The assessment type to check</param>
        /// <param name="projectName">Project name to check against</param>
        /// <returns>True if the assessment type belongs to the project</returns>
        private static bool BelongsToProject(AssessmentTypeWrapper assessmentType, string projectName)
        {
            // Check if any inputs are null
            if (assessmentType == null || string.IsNullOrEmpty(projectName))
            {
                return false;
            }

            // Check if the assessment type has a JSON definition
            if (assessmentType.JsonDefinition == null)
            {
                return false;
            }

            // Extract the directory paths from the assessment type
            string assessmentTypeDirectory = assessmentType.JsonDefinition.AssessmentTypeDirectory;

            // If no directory path is available, we can't determine the project
            if (string.IsNullOrEmpty(assessmentTypeDirectory))
            {
                return false;
            }

            // Check if the project name appears in the directory path
            // Try different patterns that might be present in the paths
            return assessmentTypeDirectory.Contains($"Models/{projectName}/", StringComparison.OrdinalIgnoreCase) ||
                   assessmentTypeDirectory.Contains($"Templates/{projectName}/", StringComparison.OrdinalIgnoreCase) ||
                   assessmentTypeDirectory.Contains($"/{projectName}/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
