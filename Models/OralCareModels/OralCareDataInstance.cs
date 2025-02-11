using System;
using System.Text.Json.Serialization;

namespace OralCareReference.Models
{
    /// <summary>
    /// Represents a child's oral care reference data entry
    /// </summary>
    public class OralCareDataInstance
    {
        [JsonPropertyName("child_info")]
        public ChildInfo ChildInfo { get; set; }

        [JsonPropertyName("risk_factors")]
        public RiskFactors RiskFactors { get; set; }

        [JsonPropertyName("protective_factors")]
        public ProtectiveFactors ProtectiveFactors { get; set; }

        [JsonPropertyName("clinical_findings")]
        public ClinicalFindings ClinicalFindings { get; set; }

        [JsonPropertyName("assessment_plan")]
        public AssessmentPlan AssessmentPlan { get; set; }

        [JsonPropertyName("self_management_goals")]
        public SelfManagementGoals SelfManagementGoals { get; set; }

        [JsonPropertyName("nursing_recommendations")]
        public string NursingRecommendations { get; set; }
    }

    public class ChildInfo
    {
        [JsonPropertyName("child_name")]
        public string ChildName { get; set; }

        [JsonPropertyName("case_number")]
        public string CaseNumber { get; set; }

        [JsonPropertyName("assessment_date")]
        public string AssessmentDate { get; set; }

        [JsonPropertyName("date_of_birth")]
        public string DateOfBirth { get; set; }

        [JsonPropertyName("recent_or_upcoming_dental_appts")]
        public string RecentOrUpcomingDentalAppts { get; set; }

        [JsonPropertyName("nursing_note")]
        public string NursingNote { get; set; }

        [JsonPropertyName("required_comment")]
        public string RequiredComment { get; set; }

        [JsonPropertyName("rn_completing_assessment")]
        public string RNCompletingAssessment { get; set; }
    }

    public class RiskFactors
    {
        [JsonPropertyName("mother_active_decay")]
        public string MotherActiveDecay { get; set; }

        [JsonPropertyName("mother_no_dentist")]
        public string MotherNoDentist { get; set; }

        [JsonPropertyName("bottle_usage")]
        public string BottleUsage { get; set; }

        [JsonPropertyName("frequent_snacking")]
        public string FrequentSnacking { get; set; }

        [JsonPropertyName("special_health_care_needs")]
        public string SpecialHealthCareNeeds { get; set; }

        [JsonPropertyName("medicaid_eligible")]
        public string MedicaidEligible { get; set; }
    }

    public class ProtectiveFactors
    {
        [JsonPropertyName("existing_dental_home")]
        public bool ExistingDentalHome { get; set; }

        [JsonPropertyName("fluoridated_water")]
        public bool FluoridatedWater { get; set; }

        [JsonPropertyName("fluoride_varnish")]
        public string FluorideVarnish { get; set; }

        [JsonPropertyName("brushing_twice_daily")]
        public bool BrushingTwiceDaily { get; set; }
    }

    public class ClinicalFindings
    {
        [JsonPropertyName("white_spots")]
        public bool WhiteSpots { get; set; }

        [JsonPropertyName("obvious_decay")]
        public bool ObviousDecay { get; set; }

        [JsonPropertyName("fillings_present")]
        public bool FillingsPresent { get; set; }

        [JsonPropertyName("plaque_accumulation")]
        public bool PlaqueAccumulation { get; set; }

        [JsonPropertyName("gingivitis")]
        public bool Gingivitis { get; set; }

        [JsonPropertyName("teeth_present")]
        public bool TeethPresent { get; set; }

        [JsonPropertyName("healthy_teeth")]
        public bool HealthyTeeth { get; set; }
    }

    public class AssessmentPlan
    {
        [JsonPropertyName("caries_risk")]
        public string CariesRisk { get; set; }

        [JsonPropertyName("completed_actions")]
        public CompletedActions CompletedActions { get; set; }
    }

    public class CompletedActions
    {
        [JsonPropertyName("anticipatory_guidance")]
        public bool AnticipatoryGuidance { get; set; }

        [JsonPropertyName("fluoride_varnish")]
        public bool FluorideVarnish { get; set; }

        [JsonPropertyName("dental_referral")]
        public bool DentalReferral { get; set; }
    }

    public class SelfManagementGoals
    {
        [JsonPropertyName("regular_dental_visits")]
        public bool RegularDentalVisits { get; set; }

        [JsonPropertyName("dental_treatment_for_caregivers")]
        public bool DentalTreatmentForCaregivers { get; set; }

        [JsonPropertyName("brush_twice_daily")]
        public bool BrushTwiceDaily { get; set; }

        [JsonPropertyName("use_fluoride_toothpaste")]
        public bool UseFluorideToothpaste { get; set; }

        [JsonPropertyName("wean_bottle")]
        public bool WeanBottle { get; set; }

        [JsonPropertyName("less_or_nojuice")]
        public bool LessOrNoJuice { get; set; }

        [JsonPropertyName("water_in_sippy")]
        public bool WaterInSippy { get; set; }

        [JsonPropertyName("drink_tap")]
        public bool DrinkTap { get; set; }

        [JsonPropertyName("healthy_snacks")]
        public bool HealthySnacks { get; set; }

        [JsonPropertyName("less_or_nojunkfood")]
        public bool LessOrNoJunkfood { get; set; }

        [JsonPropertyName("no_soda")]
        public bool NoSoda { get; set; }

        [JsonPropertyName("xylitol")]
        public bool Xylitol { get; set; }
    }
}
