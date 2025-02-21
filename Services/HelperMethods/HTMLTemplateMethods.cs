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
    /// Replaces placeholders in HTML template with OralCareDataInstance values.
    /// </summary>
    /// <param name="template">HTML template with placeholders</param>
    /// <param name="data">OralCareDataInstance containing values to insert</param>
    /// <returns>HTML with placeholders replaced with actual values</returns>
    /// <remarks>
    /// Handles placeholders for:
    /// - Child Information
    /// - Risk Factors
    /// - Protective Factors
    /// - Clinical Findings
    /// - Assessment Plan
    /// - Self Management Goals
    /// - Nursing Recommendations
    /// 
    /// Uses null coalescing to handle null values gracefully.
    /// </remarks>
    public static string ReplacePlaceholders(string template, OralCareDataInstance data)
    {
        return template
            // Child Information
            .Replace("[[CHILD_NAME]]", data.ChildInfo?.ChildName ?? "")
            .Replace("[[CASE_NUMBER]]", data.ChildInfo?.CaseNumber ?? "")
            .Replace("[[ASSESSMENT_DATE]]", data.ChildInfo?.AssessmentDate ?? "")
            .Replace("[[DATE_OF_BIRTH]]", data.ChildInfo?.DateOfBirth ?? "N/A")
            .Replace("[[DENTAL_APPTS]]", data.ChildInfo?.RecentOrUpcomingDentalAppts ?? "N/A")
            .Replace("[[NURSING_NOTE]]", data.ChildInfo?.NursingNote ?? "N/A")
            .Replace("[[REQUIRED_COMMENT]]", data.ChildInfo?.RequiredComment ?? "N/A")
            .Replace("[[RN_COMPLETING_ASSESSMENT]]", data.ChildInfo?.RNCompletingAssessment ?? "N/A")

            // Risk Factors
            .Replace("[[MOTHER_ACTIVE_DECAY_CLASS]]", GetCheckboxClass(data.RiskFactors?.MotherActiveDecay))
            .Replace("[[MOTHER_NO_DENTIST_CLASS]]", GetCheckboxClass(data.RiskFactors?.MotherNoDentist))
            .Replace("[[BOTTLE_USAGE_CLASS]]", GetCheckboxClass(data.RiskFactors?.BottleUsage))
            .Replace("[[FREQUENT_SNACKING_CLASS]]", GetCheckboxClass(data.RiskFactors?.FrequentSnacking))
            .Replace("[[SPECIAL_HEALTH_CARE_NEEDS_CLASS]]", GetCheckboxClass(data.RiskFactors?.SpecialHealthCareNeeds))
            .Replace("[[MEDICAID_ELIGIBLE_CLASS]]", GetCheckboxClass(data.RiskFactors?.MedicaidEligible))

            // Protective Factors
            .Replace("[[EXISTING_DENTAL_HOME_CLASS]]", GetCheckboxClass(data.ProtectiveFactors?.ExistingDentalHome))
            .Replace("[[FLUORIDATED_WATER_CLASS]]", GetCheckboxClass(data.ProtectiveFactors?.FluoridatedWater))
            .Replace("[[FLUORIDE_VARNISH_CLASS]]", GetCheckboxClass(data.ProtectiveFactors?.FluorideVarnish))
            .Replace("[[BRUSHING_TWICE_DAILY_CLASS]]", GetCheckboxClass(data.ProtectiveFactors?.BrushingTwiceDaily))

            // Clinical Findings
            .Replace("[[WHITE_SPOTS_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.WhiteSpots))
            .Replace("[[OBVIOUS_DECAY_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.ObviousDecay))
            .Replace("[[FILLINGS_PRESENT_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.FillingsPresent))
            .Replace("[[PLAQUE_ACCUMULATION_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.PlaqueAccumulation))
            .Replace("[[GINGIVITIS_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.Gingivitis))
            .Replace("[[TEETH_PRESENT_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.TeethPresent))
            .Replace("[[HEALTHY_TEETH_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.HealthyTeeth))

            // Assessment Plan
            .Replace("[[CARIES_RISK]]", data.AssessmentPlan?.CariesRisk ?? "N/A")
            .Replace("[[ANTICIPATORY_GUIDANCE_CLASS]]", GetCheckboxClass(data.AssessmentPlan?.CompletedActions?.AnticipatoryGuidance))
            .Replace("[[FLUORIDE_VARNISH_COMPLETED_CLASS]]", GetCheckboxClass(data.AssessmentPlan?.CompletedActions?.FluorideVarnish))
            .Replace("[[DENTAL_REFERRAL_CLASS]]", GetCheckboxClass(data.AssessmentPlan?.CompletedActions?.DentalReferral))

            // Self Management Goals
            .Replace("[[REGULAR_DENTAL_VISITS_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.RegularDentalVisits))
            .Replace("[[DENTAL_TREATMENT_CAREGIVERS_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.DentalTreatmentForCaregivers))
            .Replace("[[BRUSH_TWICE_DAILY_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.BrushTwiceDaily))
            .Replace("[[USE_FLUORIDE_TOOTHPASTE_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.UseFluorideToothpaste))
            .Replace("[[WEAN_BOTTLE_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.WeanBottle))
            .Replace("[[LESS_OR_NO_JUICE_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.LessOrNoJuice))
            .Replace("[[WATER_IN_SIPPY_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.WaterInSippy))
            .Replace("[[DRINK_TAP_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.DrinkTap))
            .Replace("[[HEALTHY_SNACKS_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.HealthySnacks))
            .Replace("[[LESS_OR_NO_JUNKFOOD_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.LessOrNoJunkfood))
            .Replace("[[NO_SODA_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.NoSoda))
            .Replace("[[XYLITOL_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.Xylitol))

            // Nursing Recommendations
            .Replace("[[NURSING_RECOMMENDATIONS]]", data.NursingRecommendations ?? "N/A");
    }

    /// <summary>
    /// Replaces placeholders in HTML template with RegisteredNurseTaskDelegDataInstance values.
    /// </summary>
    /// <param name="template">HTML template with placeholders</param>
    /// <param name="data">RegisteredNurseTaskDelegDataInstance containing values to insert</param>
    /// <returns>HTML with placeholders replaced with actual values</returns>
    /// <remarks>
    /// Handles placeholders for:
    /// - Child Info
    /// - Caregiver Info
    /// - Delegated Tasks
    /// - Training Details
    /// - Signatures
    /// 
    /// Uses null coalescing to handle null values gracefully.
    /// </remarks>
    public static string ReplacePlaceholders(string template, RegisteredNurseTaskDelegDataInstance data)
    {
        return template
            // Child Info
            .Replace("{{child_info.name}}", data.ChildInfo?.Name ?? "")
            .Replace("{{child_info.date_of_birth}}", data.ChildInfo?.DateOfBirth ?? "")
            .Replace("{{child_info.person_number}}", data.ChildInfo?.PersonNumber ?? "")

            // Caregiver Info
            .Replace("{{caregiver_info.delegation_name}}", data.CaregiverInfo?.DelegationName ?? "")
            .Replace("{{caregiver_info.delegation_date}}", data.CaregiverInfo?.DelegationDate ?? "")
            .Replace("[[INITIAL_DELEGATION_CLASS]]", GetCheckboxClass(data.CaregiverInfo?.DelegationOrAssignment))
            .Replace("[[SUPERVISORY_VISIT_CLASS]]", GetCheckboxClass(data.CaregiverInfo?.SupervisoryVisit))
            .Replace("[[ONGOING_SUPERVISORY_VISIT_CLASS]]", GetCheckboxClass(data.CaregiverInfo?.OngoingSupervisoryVisit))

            // Delegated Tasks - Checkbox Classes
            .Replace("[[GASTRIC_TUBE_FEEDING_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.GastricTubeFeedingAndCare))
            .Replace("[[GASTRIC_TUBE_PUMP_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.GastricTubeFeedingPump))
            .Replace("[[NASOGASTRIC_FEEDING_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.NasogastricFeedingAndCare))
            .Replace("[[JEJUNOSTOMY_FEEDING_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.JejunostomyFeedingAndCare))
            .Replace("[[OSTOMY_CARE_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.OstomyCare))
            .Replace("[[DRESSING_CHANGES_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.DressingChanges))
            .Replace("[[OXYGEN_ADMIN_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.OxygenAdministration))
            .Replace("[[OXYGEN_ADMIN_CHANGES_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.OxygenAdministrationWithChanges))
            .Replace("[[PULSE_OXIMETER_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.PulseOximeter))

            // Training Details
            .Replace("{{instructions_given}}", data.InstructionsGiven ?? "")
            .Replace("{{supervisory_visit_notes}}", data.SupervisoryVisitNotes ?? "")
            .Replace("[[CONTINUE_DELEGATION_CLASS]]", GetCheckboxClass(data.ContinueDelegation))

            // Signatures
            .Replace("{{signatures.rn_signature}}", data.Signatures?.RegisteredNurseSignature ?? "")
            .Replace("{{signatures.rn_date}}", data.Signatures?.RegisteredNurseSignatureDate ?? "")
            .Replace("{{signatures.caregiver_signature}}", data.Signatures?.CaregiverSignature ?? "")
            .Replace("{{signatures.caregiver_date}}", data.Signatures?.CaregiverSignatureDate ?? "");
    }




    }
}