using iTextDesignerWithGUI.Models;
using iTextDesignerWithGUI.Models.TestRazorDataModels;
using System;

namespace iTextDesignerWithGUI.Services.HelperMethods
{
    /// <summary>
    /// Static class containing helper methods for HTML template manipulation and processing.
    /// Provides functionality for placeholder replacement and checkbox styling in HTML templates.
    /// </summary>
    public static class HTMLTemplateMethods
    {
        // Methods will be moved here:
        // - ReplacePlaceholders (OralCareDataInstance version)
        // - ReplacePlaceholders (RegisteredNurseTaskDelegDataInstance version)
        // - GetCheckboxClass





    /// <summary>
    /// Determines the appropriate checkbox CSS class based on a value and checkbox type.
    /// </summary>
    /// <param name="value">Value to evaluate</param>
    /// <param name="checkboxType">The type of checkbox: "yes", "no", "unknown", or "na"</param>
    /// <returns>CSS class name for checkbox styling</returns>
    public static string GetCheckboxClassForType(object value, string checkboxType)
    {
        // Handle null values
        if (value == null)
            return checkboxType.Equals("unknown", StringComparison.OrdinalIgnoreCase) ? "checked" : "";

        // Handle boolean values
        if (value is bool boolValue)
        {
            if (checkboxType.Equals("yes", StringComparison.OrdinalIgnoreCase))
                return boolValue ? "checked" : "";
            else if (checkboxType.Equals("no", StringComparison.OrdinalIgnoreCase))
                return !boolValue ? "checked" : "";
            else
                return "";
        }

        // Handle string values
        string strValue = value.ToString().ToLower().Trim();
        
        // Yes checkbox should be checked when value is yes/true
        if (checkboxType.Equals("yes", StringComparison.OrdinalIgnoreCase))
            return (strValue.Equals("yes", StringComparison.OrdinalIgnoreCase) || 
                    strValue.Equals("true", StringComparison.OrdinalIgnoreCase)) ? "checked" : "";
        
        // No checkbox should be checked when value is no/false
        if (checkboxType.Equals("no", StringComparison.OrdinalIgnoreCase))
            return (strValue.Equals("no", StringComparison.OrdinalIgnoreCase) || 
                    strValue.Equals("false", StringComparison.OrdinalIgnoreCase)) ? "checked" : "";
        
        // Unknown checkbox should be checked when value is unknown
        if (checkboxType.Equals("unknown", StringComparison.OrdinalIgnoreCase))
            return strValue.Equals("unknown", StringComparison.OrdinalIgnoreCase) ? "checked" : "";
        
        // NA checkbox should be checked when value is na
        if (checkboxType.Equals("na", StringComparison.OrdinalIgnoreCase))
            return strValue.Equals("na", StringComparison.OrdinalIgnoreCase) ? "checked" : "";
        
        return "";
    }


    /// <summary>
    /// Determines the appropriate checkbox CSS class based on a value.
    /// </summary>
    /// <param name="value">Value to evaluate</param>
    /// <returns>CSS class name for checkbox styling</returns>
    /// <remarks>
    /// Supports multiple value formats:
    /// - Boolean: true/false
    /// - String: "yes"/"no", "true"/"false"
    /// - Special values: "unknown", "na"
    /// 
    /// Returns:
    /// - "checked" for true/yes values
    /// - "" (empty string) for false/no values
    /// - "unknown" for null, unknown, or NA values
    /// </remarks>
    public static string GetCheckboxClass(object value)
    {
        if (value == null)
            return "unknown";

        if (value is bool boolValue)
            return boolValue ? "checked" : "";

        string strValue = value.ToString().ToLower().Trim();
        
        // Handle Yes/No values
        if (strValue.Equals("yes", StringComparison.OrdinalIgnoreCase))
            return "checked";
        if (strValue.Equals("no", StringComparison.OrdinalIgnoreCase))
            return "";
            
        // Handle True/False values
        if (strValue.Equals("true", StringComparison.OrdinalIgnoreCase))
            return "checked";
        if (strValue.Equals("false", StringComparison.OrdinalIgnoreCase))
            return "";
            
        // Handle special values
        if (strValue.Equals("unknown", StringComparison.OrdinalIgnoreCase) ||
            strValue.Equals("na", StringComparison.OrdinalIgnoreCase))
            return "unknown";
            
        return "unknown";
    }

    /// <summary>
    /// Replace checkbox placeholders in format [FIELD_NAME_YES_CLASS], [FIELD_NAME_NO_CLASS], etc.
    /// Maintains backward compatibility by also supporting the old [FIELD_NAME_CLASS] format
    /// </summary>
    /// <param name="template">The HTML template</param>
    /// <param name="placeholder">The base placeholder name (e.g., "MOTHER_ACTIVE_DECAY")</param>
    /// <param name="value">The value to use for determining checkbox status</param>
    /// <returns>Template with placeholders replaced</returns>
    private static string ReplaceCheckboxPlaceholders(string template, string placeholder, object value)
    {
        // Support for new format (separate placeholders for Yes/No/Unknown/NA)
        template = template.Replace($"[[{placeholder}_YES_CLASS]]", GetCheckboxClassForType(value, "yes"));
        template = template.Replace($"[[{placeholder}_NO_CLASS]]", GetCheckboxClassForType(value, "no"));
        template = template.Replace($"[[{placeholder}_UNKNOWN_CLASS]]", GetCheckboxClassForType(value, "unknown"));
        template = template.Replace($"[[{placeholder}_NA_CLASS]]", GetCheckboxClassForType(value, "na"));
        
        // Backward compatibility with old format
        template = template.Replace($"[[{placeholder}_CLASS]]", GetCheckboxClass(value));
        
        return template;
    }

    /// <summary>
    /// Replaces placeholders in HTML template with OralCareDataInstance values.
    /// </summary>
    /// <param name="template">HTML template with placeholders</param>
    /// <param name="data">OralCareDataInstance containing values to insert</param>
    /// <returns>HTML with placeholders replaced with actual values</returns>
    public static string ReplacePlaceholders(string template, OralCareDataInstance data)
    {
        // Child Information
        template = template
            .Replace("[[CHILD_NAME]]", data.ChildInfo?.ChildName ?? "")
            .Replace("[[CASE_NUMBER]]", data.ChildInfo?.CaseNumber ?? "")
            .Replace("[[ASSESSMENT_DATE]]", data.ChildInfo?.AssessmentDate ?? "")
            .Replace("[[DATE_OF_BIRTH]]", data.ChildInfo?.DateOfBirth ?? "N/A")
            .Replace("[[DENTAL_APPTS]]", data.ChildInfo?.RecentOrUpcomingDentalAppts ?? "N/A")
            .Replace("[[NURSING_NOTE]]", data.ChildInfo?.NursingNote ?? "N/A")
            .Replace("[[REQUIRED_COMMENT]]", data.ChildInfo?.RequiredComment ?? "N/A")
            .Replace("[[RN_COMPLETING_ASSESSMENT]]", data.ChildInfo?.RNCompletingAssessment ?? "N/A");

        // Risk Factors
        template = ReplaceCheckboxPlaceholders(template, "MOTHER_ACTIVE_DECAY", data.RiskFactors?.MotherActiveDecay);
        template = ReplaceCheckboxPlaceholders(template, "MOTHER_NO_DENTIST", data.RiskFactors?.MotherNoDentist);
        template = ReplaceCheckboxPlaceholders(template, "BOTTLE_USAGE", data.RiskFactors?.BottleUsage);
        template = ReplaceCheckboxPlaceholders(template, "FREQUENT_SNACKING", data.RiskFactors?.FrequentSnacking);
        template = ReplaceCheckboxPlaceholders(template, "SPECIAL_HEALTH_CARE_NEEDS", data.RiskFactors?.SpecialHealthCareNeeds);
        template = ReplaceCheckboxPlaceholders(template, "MEDICAID_ELIGIBLE", data.RiskFactors?.MedicaidEligible);

        // Protective Factors
        template = ReplaceCheckboxPlaceholders(template, "EXISTING_DENTAL_HOME", data.ProtectiveFactors?.ExistingDentalHome);
        template = ReplaceCheckboxPlaceholders(template, "FLUORIDATED_WATER", data.ProtectiveFactors?.FluoridatedWater);
        template = ReplaceCheckboxPlaceholders(template, "FLUORIDE_VARNISH", data.ProtectiveFactors?.FluorideVarnish);
        template = ReplaceCheckboxPlaceholders(template, "BRUSHING_TWICE_DAILY", data.ProtectiveFactors?.BrushingTwiceDaily);

        // Clinical Findings
        template = ReplaceCheckboxPlaceholders(template, "WHITE_SPOTS", data.ClinicalFindings?.WhiteSpots);
        template = ReplaceCheckboxPlaceholders(template, "OBVIOUS_DECAY", data.ClinicalFindings?.ObviousDecay);
        template = ReplaceCheckboxPlaceholders(template, "FILLINGS_PRESENT", data.ClinicalFindings?.FillingsPresent);
        template = ReplaceCheckboxPlaceholders(template, "PLAQUE_ACCUMULATION", data.ClinicalFindings?.PlaqueAccumulation);
        template = ReplaceCheckboxPlaceholders(template, "GINGIVITIS", data.ClinicalFindings?.Gingivitis);
        template = ReplaceCheckboxPlaceholders(template, "TEETH_PRESENT", data.ClinicalFindings?.TeethPresent);
        template = ReplaceCheckboxPlaceholders(template, "HEALTHY_TEETH", data.ClinicalFindings?.HealthyTeeth);

        // Assessment Plan
        template = template.Replace("[[CARIES_RISK]]", data.AssessmentPlan?.CariesRisk ?? "N/A");
        
        // Handle caries risk checkboxes specially 
        string cariesRisk = data.AssessmentPlan?.CariesRisk?.ToLower() ?? "";
        template = template
            .Replace("[[CARIES_RISK_LOW_CLASS]]", cariesRisk == "low" ? "checked" : "")
            .Replace("[[CARIES_RISK_HIGH_CLASS]]", cariesRisk == "high" ? "checked" : "");
        
        template = ReplaceCheckboxPlaceholders(template, "ANTICIPATORY_GUIDANCE", data.AssessmentPlan?.CompletedActions?.AnticipatoryGuidance);
        template = ReplaceCheckboxPlaceholders(template, "FLUORIDE_VARNISH_COMPLETED", data.AssessmentPlan?.CompletedActions?.FluorideVarnish);
        template = ReplaceCheckboxPlaceholders(template, "DENTAL_REFERRAL", data.AssessmentPlan?.CompletedActions?.DentalReferral);

        // Self Management Goals
        template = ReplaceCheckboxPlaceholders(template, "REGULAR_DENTAL_VISITS", data.SelfManagementGoals?.RegularDentalVisits);
        template = ReplaceCheckboxPlaceholders(template, "DENTAL_TREATMENT_CAREGIVERS", data.SelfManagementGoals?.DentalTreatmentForCaregivers);
        template = ReplaceCheckboxPlaceholders(template, "BRUSH_TWICE_DAILY", data.SelfManagementGoals?.BrushTwiceDaily);
        template = ReplaceCheckboxPlaceholders(template, "USE_FLUORIDE_TOOTHPASTE", data.SelfManagementGoals?.UseFluorideToothpaste);
        template = ReplaceCheckboxPlaceholders(template, "WEAN_BOTTLE", data.SelfManagementGoals?.WeanBottle);
        template = ReplaceCheckboxPlaceholders(template, "LESS_OR_NO_JUICE", data.SelfManagementGoals?.LessOrNoJuice);
        template = ReplaceCheckboxPlaceholders(template, "WATER_IN_SIPPY", data.SelfManagementGoals?.WaterInSippy);
        template = ReplaceCheckboxPlaceholders(template, "DRINK_TAP", data.SelfManagementGoals?.DrinkTap);
        template = ReplaceCheckboxPlaceholders(template, "HEALTHY_SNACKS", data.SelfManagementGoals?.HealthySnacks);
        template = ReplaceCheckboxPlaceholders(template, "LESS_OR_NO_JUNKFOOD", data.SelfManagementGoals?.LessOrNoJunkfood);
        template = ReplaceCheckboxPlaceholders(template, "NO_SODA", data.SelfManagementGoals?.NoSoda);
        template = ReplaceCheckboxPlaceholders(template, "XYLITOL", data.SelfManagementGoals?.Xylitol);

        // Nursing Recommendations
        template = template.Replace("[[NURSING_RECOMMENDATIONS]]", data.NursingRecommendations ?? "N/A");

        return template;
    }

    /// <summary>
    /// Replaces placeholders in HTML template with RegisteredNurseTaskDelegDataInstance values.
    /// </summary>
    /// <param name="template">HTML template with placeholders</param>
    /// <param name="data">RegisteredNurseTaskDelegDataInstance containing values to insert</param>
    /// <returns>HTML with placeholders replaced with actual values</returns>
    public static string ReplacePlaceholders(string template, RegisteredNurseTaskDelegDataInstance data)
    {
        // Child Info
        template = template
            .Replace("{{child_info.name}}", data.ChildInfo?.Name ?? "")
            .Replace("{{child_info.date_of_birth}}", data.ChildInfo?.DateOfBirth ?? "")
            .Replace("{{child_info.person_number}}", data.ChildInfo?.PersonNumber ?? "")
            
            // Caregiver Info
            .Replace("{{caregiver_info.delegation_name}}", data.CaregiverInfo?.DelegationName ?? "")
            .Replace("{{caregiver_info.delegation_date}}", data.CaregiverInfo?.DelegationDate ?? "");

        // Caregiver Info - Checkboxes
        template = ReplaceCheckboxPlaceholders(template, "INITIAL_DELEGATION", data.CaregiverInfo?.DelegationOrAssignment);
        template = ReplaceCheckboxPlaceholders(template, "SUPERVISORY_VISIT", data.CaregiverInfo?.SupervisoryVisit);
        template = ReplaceCheckboxPlaceholders(template, "ONGOING_SUPERVISORY_VISIT", data.CaregiverInfo?.OngoingSupervisoryVisit);

        // Delegated Tasks - Checkboxes
        template = ReplaceCheckboxPlaceholders(template, "GASTRIC_TUBE_FEEDING", data.DelegatedTasks?.GastricTubeFeedingAndCare);
        template = ReplaceCheckboxPlaceholders(template, "GASTRIC_TUBE_PUMP", data.DelegatedTasks?.GastricTubeFeedingPump);
        template = ReplaceCheckboxPlaceholders(template, "NASOGASTRIC_FEEDING", data.DelegatedTasks?.NasogastricFeedingAndCare);
        template = ReplaceCheckboxPlaceholders(template, "JEJUNOSTOMY_FEEDING", data.DelegatedTasks?.JejunostomyFeedingAndCare);
        template = ReplaceCheckboxPlaceholders(template, "OSTOMY_CARE", data.DelegatedTasks?.OstomyCare);
        template = ReplaceCheckboxPlaceholders(template, "DRESSING_CHANGES", data.DelegatedTasks?.DressingChanges);
        template = ReplaceCheckboxPlaceholders(template, "OXYGEN_ADMIN", data.DelegatedTasks?.OxygenAdministration);
        template = ReplaceCheckboxPlaceholders(template, "OXYGEN_ADMIN_CHANGES", data.DelegatedTasks?.OxygenAdministrationWithChanges);
        template = ReplaceCheckboxPlaceholders(template, "PULSE_OXIMETER", data.DelegatedTasks?.PulseOximeter);

        // Training Details
        template = template
            .Replace("{{instructions_given}}", data.InstructionsGiven ?? "")
            .Replace("{{supervisory_visit_notes}}", data.SupervisoryVisitNotes ?? "");
        
        template = ReplaceCheckboxPlaceholders(template, "CONTINUE_DELEGATION", data.ContinueDelegation);

        // Signatures
        template = template
            .Replace("{{signatures.rn_signature}}", data.Signatures?.RegisteredNurseSignature ?? "")
            .Replace("{{signatures.rn_date}}", data.Signatures?.RegisteredNurseSignatureDate ?? "")
            .Replace("{{signatures.caregiver_signature}}", data.Signatures?.CaregiverSignature ?? "")
            .Replace("{{signatures.caregiver_date}}", data.Signatures?.CaregiverSignatureDate ?? "");

        return template;
    }




    }
}