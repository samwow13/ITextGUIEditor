using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using iTextDesignerWithGUI.Extensions;
using iTextDesignerWithGUI.Services;

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
        public string BuiltInType { get; private set; }

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
        /// Gets the JSON definition for this assessment type (if available)
        /// </summary>
        public AssessmentTypeJsonDefinition JsonDefinition { get; private set; }

        /// <summary>
        /// Creates a new wrapper for a built-in assessment type
        /// </summary>
        public static AssessmentTypeWrapper FromBuiltIn(string builtInType)
        {
            if (string.IsNullOrEmpty(builtInType))
                throw new ArgumentNullException(nameof(builtInType));
                
            // Try to get the JSON definition for this built-in type
            var jsonDefinition = AssessmentTypeJsonLoader.Instance.GetAssessmentTypeByName(builtInType);
                
            return new AssessmentTypeWrapper
            {
                IsBuiltIn = true,
                BuiltInType = builtInType,
                TypeName = builtInType,
                CustomTypeId = null,
                DisplayName = jsonDefinition?.DisplayName ?? builtInType.SplitCamelCase(),
                AssessmentClassType = null, // Will be set in GetAssessment
                JsonDefinition = jsonDefinition
            };
        }

        /// <summary>
        /// Creates a new wrapper for a custom assessment type
        /// </summary>
        public static AssessmentTypeWrapper FromCustom(CustomAssessmentType customType)
        {
            if (customType == null)
                throw new ArgumentNullException(nameof(customType));

            // Try to get the JSON definition for this custom type
            var jsonDefinition = AssessmentTypeJsonLoader.Instance.GetAssessmentTypeByName(customType.Id);
                
            return new AssessmentTypeWrapper
            {
                IsBuiltIn = false,
                BuiltInType = null,
                TypeName = "Custom_" + customType.Id,
                CustomTypeId = customType.Id,
                DisplayName = jsonDefinition?.DisplayName ?? customType.DisplayName,
                AssessmentClassType = null,
                JsonDefinition = jsonDefinition
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

            // Get the type name without "Assessment" suffix
            string typeName = assessmentType.Name.Replace("Assessment", "");
            
            // Try to get the JSON definition for this type
            var jsonDefinition = AssessmentTypeJsonLoader.Instance.GetAssessmentTypeByName(typeName);

            // Check if this might be a built-in type that we should map to an enum
            bool isBuiltIn = AssessmentTypeConstants.IsKnownType(typeName);
            string builtInType = isBuiltIn ? typeName : null;
            
            return new AssessmentTypeWrapper
            {
                IsBuiltIn = isBuiltIn,
                BuiltInType = builtInType,
                TypeName = typeName,
                CustomTypeId = isBuiltIn ? null : $"discovered_{assessmentType.FullName}",
                DisplayName = jsonDefinition?.DisplayName ?? instance.DisplayName,
                AssessmentClassType = assessmentType,
                JsonDefinition = jsonDefinition
            };
        }

        /// <summary>
        /// Creates a new wrapper from a JSON assessment type definition
        /// </summary>
        public static AssessmentTypeWrapper FromJsonDefinition(AssessmentTypeJsonDefinition jsonDefinition)
        {
            if (jsonDefinition == null)
                throw new ArgumentNullException(nameof(jsonDefinition));

            // Check if this might be a built-in type that we should map to an enum
            bool isBuiltIn = AssessmentTypeConstants.IsKnownType(jsonDefinition.Name);
            string builtInType = isBuiltIn ? jsonDefinition.Name : null;

            return new AssessmentTypeWrapper
            {
                IsBuiltIn = isBuiltIn,
                BuiltInType = builtInType,
                TypeName = jsonDefinition.Name,
                CustomTypeId = isBuiltIn ? null : $"json_{jsonDefinition.Name}",
                DisplayName = jsonDefinition.DisplayName,
                AssessmentClassType = null,
                JsonDefinition = jsonDefinition
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
                // Load assessment types from the JSON file
                var jsonLoader = AssessmentTypeJsonLoader.Instance;
                var jsonTypes = jsonLoader.LoadAssessmentTypes(forceRefresh: true);
                
                // Create wrappers for each JSON type
                foreach (var jsonType in jsonTypes)
                {
                    try
                    {
                        result.Add(FromJsonDefinition(jsonType));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error creating wrapper for JSON type {jsonType.Name}: {ex.Message}");
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
            
            // If we have a JSON definition, try to create a dynamic assessment from it
            if (JsonDefinition != null)
            {
                try
                {
                    // Try to find the assessment type class from the JSON definition
                    if (!string.IsNullOrEmpty(JsonDefinition.AssessmentTypeDirectory))
                    {
                        // Convert the file path to a type name
                        string typeName = GetTypeNameFromPath(JsonDefinition.AssessmentTypeDirectory);
                        
                        // Try to find the type by name
                        var assessmentType = FindTypeByName(typeName);
                        if (assessmentType != null)
                        {
                            return (IAssessment)Activator.CreateInstance(assessmentType);
                        }
                    }
                    
                    // Create a custom assessment type from the JSON definition
                    var customType = new CustomAssessmentType
                    {
                        Id = $"json_{JsonDefinition.Name}",
                        DisplayName = JsonDefinition.DisplayName,
                        TemplateFileName = JsonDefinition.CshtmlTemplateDirectory,
                        JsonDataPath = JsonDefinition.JsonDataLocationDirectory
                    };
                    
                    return new DynamicAssessment(customType);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating assessment from JSON definition: {ex.Message}");
                    // Continue to try other methods
                }
            }
            
            // Try to handle as a built-in type
            if (IsBuiltIn && !string.IsNullOrEmpty(BuiltInType))
            {
                // Use the TypeName which is the string version of the enum
                string typeName = TypeName ?? BuiltInType;
                
                // Try to find the assessment type by name with "Assessment" suffix
                var assessmentType = FindTypeByName(typeName + "Assessment");
                if (assessmentType != null)
                {
                    return (IAssessment)Activator.CreateInstance(assessmentType);
                }
                
                // Fall back to the switch statement for known types
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
                        var testerAssessmentType = FindTypeByName("testerAssessment");
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
                        var dynamicAssessmentType = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .FirstOrDefault(t => 
                                (t.Name.Equals(typeName + "Assessment", StringComparison.OrdinalIgnoreCase) ||
                                 t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)) &&
                                typeof(IAssessment).IsAssignableFrom(t) &&
                                !t.IsInterface && 
                                !t.IsAbstract &&
                                t.GetConstructor(Type.EmptyTypes) != null
                            );
                            
                        if (dynamicAssessmentType != null)
                        {
                            return (IAssessment)Activator.CreateInstance(dynamicAssessmentType);
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
        
        /// <summary>
        /// Converts a file path to a type name
        /// </summary>
        private string GetTypeNameFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
                
            // Get the file name without extension
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            return fileName;
        }
        
        /// <summary>
        /// Finds a type by name in the current assembly
        /// </summary>
        private Type FindTypeByName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;
                
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .FirstOrDefault(t => 
                    t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase) &&
                    typeof(IAssessment).IsAssignableFrom(t) &&
                    !t.IsInterface && 
                    !t.IsAbstract &&
                    t.GetConstructor(Type.EmptyTypes) != null
                );
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
