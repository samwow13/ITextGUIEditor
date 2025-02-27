using System;

namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Constants for assessment types that are used throughout the application
    /// These constants provide a way to reference specific assessment types by name
    /// without relying on the AssessmentType enum values
    /// </summary>
    public static class AssessmentTypeConstants
    {
        public const string OralCare = "OralCare";
        public const string RegisteredNurseTaskAndDelegation = "RegisteredNurseTaskAndDelegation";
        public const string TestRazorDataInstance = "TestRazorDataInstance";
        public const string Tester = "Tester";
        
        /// <summary>
        /// Gets an array of all assessment type constants
        /// </summary>
        public static string[] GetAll()
        {
            return new[]
            {
                OralCare,
                RegisteredNurseTaskAndDelegation,
                TestRazorDataInstance,
                Tester
            };
        }
        
        /// <summary>
        /// Checks if the specified type name matches any of the constants
        /// </summary>
        public static bool IsKnownType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return false;
                
            foreach (var type in GetAll())
            {
                if (string.Equals(type, typeName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            
            return false;
        }
    }
}
