using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using iTextDesignerWithGUI.Extensions;

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
        /// Gets the type name that can be used to identify this assessment type
        /// This will be either the enum name or a custom id
        /// </summary>
        public string TypeName { get; private set; }

        /// <summary>
        /// Gets the custom assessment type ID (only valid if IsBuiltIn is false)
        /// </summary>
        public string CustomTypeId { get; private set; }

        /// <summary>
        /// Gets the display name for this assessment type
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets or sets the assessment class type
        /// </summary>
        public Type AssessmentClassType { get; private set; }

        /// <summary>
        /// Creates a new wrapper for a built-in assessment type
        /// </summary>
        public static AssessmentTypeWrapper FromBuiltIn(AssessmentType builtInType)
        {
            string typeName = builtInType.ToString();
            
            return new AssessmentTypeWrapper
            {
                IsBuiltIn = true,
                BuiltInType = builtInType,
                TypeName = typeName,
                CustomTypeId = null,
                DisplayName = typeName.SplitCamelCase(),
                AssessmentClassType = null // Will be set in GetAssessment
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
                TypeName = "Custom_" + customType.Id,
                CustomTypeId = customType.Id,
                DisplayName = customType.DisplayName,
                AssessmentClassType = null
            };
        }

        /// <summary>
        /// Creates a new wrapper from a discovered assessment class type
        /// </summary>
        public static AssessmentTypeWrapper FromDiscoveredType(Type assessmentType)
        {
            if (assessmentType == null)
                throw new ArgumentNullException(nameof(assessmentType));

            if (!typeof(IAssessment).IsAssignableFrom(assessmentType))
                throw new ArgumentException($"Type {assessmentType.Name} does not implement IAssessment");

            // Try to create an instance to get the display name
            IAssessment instance = (IAssessment)Activator.CreateInstance(assessmentType);

            // Check if this might be a built-in type that we should map to an enum
            bool isBuiltIn = false;
            AssessmentType? builtInType = null;
            
            try
            {
                string typeName = assessmentType.Name.Replace("Assessment", "");
                
                // Try to match with enum if available
                if (Enum.TryParse<AssessmentType>(typeName, out var enumValue))
                {
                    isBuiltIn = true;
                    builtInType = enumValue;
                }
            }
            catch { /* Ignore, not an enum value */ }

            return new AssessmentTypeWrapper
            {
                IsBuiltIn = isBuiltIn,
                BuiltInType = builtInType,
                TypeName = assessmentType.Name.Replace("Assessment", ""),
                CustomTypeId = isBuiltIn ? null : $"discovered_{assessmentType.FullName}",
                DisplayName = instance.DisplayName,
                AssessmentClassType = assessmentType
            };
        }

        /// <summary>
        /// Dynamically discovers all assessment types in the current assembly
        /// </summary>
        public static List<AssessmentTypeWrapper> DiscoverAssessmentTypes()
        {
            var result = new List<AssessmentTypeWrapper>();
            
            try
            {
                // Get all types from the current assembly that implement IAssessment and have a parameterless constructor
                var assessmentTypes = Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => typeof(IAssessment).IsAssignableFrom(t) && 
                               !t.IsInterface && 
                               !t.IsAbstract &&
                               t.GetConstructor(Type.EmptyTypes) != null)
                    .ToList();

                // Start with a list of type names from the constants
                // Even if the enum is missing some values, the constants will still be there
                var knownTypeNames = new HashSet<string>(AssessmentTypeConstants.GetAll(), StringComparer.OrdinalIgnoreCase);
                
                // Also add enum values if they still exist
                try
                {
                    foreach (AssessmentType enumType in Enum.GetValues(typeof(AssessmentType)))
                    {
                        knownTypeNames.Add(enumType.ToString());
                    }
                }
                catch { /* Ignore if enum is modified */ }

                foreach (var type in assessmentTypes)
                {
                    // Get the base name without "Assessment" suffix
                    string baseName = type.Name.Replace("Assessment", "");
                    
                    // Skip the type if it's already represented by the enum or constants
                    // We only want to discover truly new types that aren't in the known list
                    if (knownTypeNames.Contains(baseName) || type == typeof(DynamicAssessment))
                    {
                        continue;
                    }

                    try
                    {
                        result.Add(FromDiscoveredType(type));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error creating wrapper for {type.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error discovering assessment types: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Gets the IAssessment implementation for this wrapper
        /// </summary>
        public IAssessment GetAssessment()
        {
            // First check if we have a direct class reference
            if (AssessmentClassType != null)
            {
                return (IAssessment)Activator.CreateInstance(AssessmentClassType);
            }
            
            // Try to handle as a built-in type
            if (IsBuiltIn && BuiltInType.HasValue)
            {
                // Use the TypeName which is the string version of the enum
                string typeName = TypeName ?? BuiltInType.Value.ToString();
                
                switch (typeName)
                {
                    case AssessmentTypeConstants.OralCare:
                        return new OralCareAssessment();
                    case AssessmentTypeConstants.RegisteredNurseTaskAndDelegation:
                        return new RegisteredNurseTaskDelegAssessment();
                    case AssessmentTypeConstants.TestRazorDataInstance:
                        return new TestRazorDataAssessment();
                    case AssessmentTypeConstants.Tester:
                        // Try to find the testerAssessment class via reflection
                        var testerAssessmentType = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .FirstOrDefault(t => t.Name.Equals("testerAssessment", StringComparison.OrdinalIgnoreCase));
                        if (testerAssessmentType != null)
                        {
                            return (IAssessment)Activator.CreateInstance(testerAssessmentType);
                        }
                        
                        // If we can't find the class directly, look for classes in the testerModels directory
                        var modelsInTesterDir = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .Where(t => t.Namespace != null && 
                                        t.Namespace.Contains("testerModels") && 
                                        typeof(IAssessment).IsAssignableFrom(t))
                            .ToList();
                            
                        if (modelsInTesterDir.Count > 0)
                        {
                            return (IAssessment)Activator.CreateInstance(modelsInTesterDir[0]);
                        }
                        
                        throw new NotSupportedException($"Could not find testerAssessment class for {TypeName}");
                    default:
                        // Try to dynamically find a class that matches the type name
                        var assessmentType = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .FirstOrDefault(t => 
                                (t.Name.Equals(typeName + "Assessment", StringComparison.OrdinalIgnoreCase) ||
                                 t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)) &&
                                typeof(IAssessment).IsAssignableFrom(t) &&
                                !t.IsInterface && 
                                !t.IsAbstract &&
                                t.GetConstructor(Type.EmptyTypes) != null
                            );
                            
                        if (assessmentType != null)
                        {
                            return (IAssessment)Activator.CreateInstance(assessmentType);
                        }
                        
                        throw new NotSupportedException($"Built-in assessment type {TypeName} is not supported");
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
