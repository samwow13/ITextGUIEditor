using System;
using System.Collections.Generic;
using System.Linq;
using iTextDesignerWithGUI.Services;

namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Constants for assessment types that are used throughout the application
    /// These constants provide a way to reference specific assessment types by name
    /// without relying on the AssessmentType enum values
    /// </summary>
    public static class AssessmentTypeConstants
    {
        // Static fields to hold the constant values loaded from JSON
        private static Dictionary<string, string> _constants = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static bool _initialized = false;
        private static readonly object _lock = new object();

        // Default constants that will be used if JSON loading fails
        public const string OralCare = "OralCare";
        public const string RegisteredNurseTaskAndDelegation = "RegisteredNurseTaskAndDelegation";
        public const string TestRazorDataInstance = "TestRazorDataInstance";
        public const string Tester = "Tester";

        /// <summary>
        /// Static constructor to initialize constants from JSON
        /// </summary>
        static AssessmentTypeConstants()
        {
            InitializeConstants();
        }

        /// <summary>
        /// Initializes the constants from the JSON file
        /// </summary>
        private static void InitializeConstants()
        {
            if (_initialized)
                return;

            lock (_lock)
            {
                if (_initialized)
                    return;

                try
                {
                    // Load assessment types from JSON
                    var assessmentTypes = AssessmentTypeJsonLoader.Instance.LoadAssessmentTypes();

                    // Add each type to the constants dictionary
                    foreach (var type in assessmentTypes)
                    {
                        _constants[type.Name] = type.Name;
                    }

                    // If no types were loaded, add the default constants
                    if (_constants.Count == 0)
                    {
                        _constants[OralCare] = OralCare;
                        _constants[RegisteredNurseTaskAndDelegation] = RegisteredNurseTaskAndDelegation;
                        _constants[TestRazorDataInstance] = TestRazorDataInstance;
                        _constants[Tester] = Tester;
                    }

                    _initialized = true;
                }
                catch (Exception ex)
                {
                    // Log the error
                    System.Diagnostics.Debug.WriteLine($"Error loading assessment types from JSON: {ex.Message}");

                    // Fall back to default constants
                    _constants[OralCare] = OralCare;
                    _constants[RegisteredNurseTaskAndDelegation] = RegisteredNurseTaskAndDelegation;
                    _constants[TestRazorDataInstance] = TestRazorDataInstance;
                    _constants[Tester] = Tester;

                    _initialized = true;
                }
            }
        }

        /// <summary>
        /// Gets the value of a constant by name
        /// </summary>
        /// <param name="name">Name of the constant</param>
        /// <returns>Constant value or null if not found</returns>
        public static string GetConstant(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            InitializeConstants();

            if (_constants.TryGetValue(name, out string value))
                return value;

            return null;
        }
        
        /// <summary>
        /// Gets an array of all assessment type constants
        /// </summary>
        public static string[] GetAll()
        {
            InitializeConstants();
            return _constants.Values.ToArray();
        }
        
        /// <summary>
        /// Checks if the specified type name matches any of the constants
        /// </summary>
        public static bool IsKnownType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return false;
                
            InitializeConstants();
            return _constants.ContainsKey(typeName);
        }
        
        /// <summary>
        /// Forces a refresh of the constants from the JSON file
        /// </summary>
        public static void RefreshConstants()
        {
            lock (_lock)
            {
                _initialized = false;
                _constants.Clear();
                InitializeConstants();
            }
        }
    }
}
