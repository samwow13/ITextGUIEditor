using System;
using iTextDesignerWithGUI.Extensions; // Add this line

namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Wrapper class that can represent either an enum-based AssessmentType or a custom assessment type
    /// </summary>
    public class AssessmentTypeWrapper
    {
        /// <summary>
        /// Gets whether this wrapper represents a built-in assessment type
        /// </summary>
        public bool IsBuiltIn { get; private set; }

        /// <summary>
        /// Gets the built-in assessment type (only valid if IsBuiltIn is true)
        /// </summary>
        public AssessmentType? BuiltInType { get; private set; }

        /// <summary>
        /// Gets the custom assessment type ID (only valid if IsBuiltIn is false)
        /// </summary>
        public string CustomTypeId { get; private set; }

        /// <summary>
        /// Gets the display name for this assessment type
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Creates a new wrapper for a built-in assessment type
        /// </summary>
        public static AssessmentTypeWrapper FromBuiltIn(AssessmentType builtInType)
        {
            return new AssessmentTypeWrapper
            {
                IsBuiltIn = true,
                BuiltInType = builtInType,
                CustomTypeId = null,
                DisplayName = builtInType.ToString().SplitCamelCase()
            };
        }

        /// <summary>
        /// Creates a new wrapper for a custom assessment type
        /// </summary>
        public static AssessmentTypeWrapper FromCustom(CustomAssessmentType customType)
        {
            if (customType == null)
                throw new ArgumentNullException(nameof(customType));

            return new AssessmentTypeWrapper
            {
                IsBuiltIn = false,
                BuiltInType = null,
                CustomTypeId = customType.Id,
                DisplayName = customType.DisplayName
            };
        }

        /// <summary>
        /// Gets the IAssessment implementation for this wrapper
        /// </summary>
        public IAssessment GetAssessment()
        {
            if (IsBuiltIn && BuiltInType.HasValue)
            {
                // Use existing code to create built-in assessment
                switch (BuiltInType.Value)
                {
                    case AssessmentType.OralCare:
                        return new OralCareAssessment();
                    case AssessmentType.RegisteredNurseTaskAndDelegation:
                        return new RegisteredNurseTaskDelegAssessment();
                    case AssessmentType.TestRazorDataInstance:
                        return new TestRazorDataAssessment();
                    default:
                        throw new NotSupportedException($"Built-in assessment type {BuiltInType} is not supported");
                }
            }
            else if (!string.IsNullOrEmpty(CustomTypeId))
            {
                // Get custom assessment type
                var customType = CustomAssessmentTypeManager.GetCustomTypeById(CustomTypeId);
                if (customType == null)
                    throw new InvalidOperationException($"Custom assessment type with ID {CustomTypeId} not found");

                // Create a dynamic assessment
                return new DynamicAssessment(customType);
            }

            throw new InvalidOperationException("Invalid assessment type wrapper state");
        }
    }

    /// <summary>
    /// Implementation of IAssessment for custom assessment types
    /// </summary>
    public class DynamicAssessment : IAssessment
    {
        private readonly CustomAssessmentType _customType;

        public DynamicAssessment(CustomAssessmentType customType)
        {
            _customType = customType ?? throw new ArgumentNullException(nameof(customType));
        }

        public string TemplateFileName => _customType.TemplateFileName;

        public string JsonDataPath => _customType.JsonDataPath;

        public string DisplayName => _customType.DisplayName;
    }
}
