using System;

namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Base interface for all assessment types
    /// </summary>
    public interface IAssessment
    {
        /// <summary>
        /// Gets the template file name for PDF generation
        /// </summary>
        string TemplateFileName { get; }

        /// <summary>
        /// Gets the JSON data file path for this assessment type
        /// </summary>
        string JsonDataPath { get; }

        /// <summary>
        /// Gets the display name for this assessment type
        /// </summary>
        string DisplayName { get; }
    }

    /// <summary>
    /// Enumeration of available assessment types
    /// </summary>
    public enum AssessmentType
    {
        OralCare,
        RegisteredNurseTaskAndDelegation,
        TestRazorDataInstance
        //ADD FORMS HERE
    }
}
