using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace iTextDesignerWithGUI.Models.OralHealthRazerModels
{
    public class OralHealthRazerInstance
    {
        [JsonProperty("child_info")]
        public ChildInfo ChildInfo { get; set; }

        [JsonProperty("risk_factors")]
        public RiskFactors RiskFactors { get; set; }

        [JsonProperty("protective_factors")]
        public ProtectiveFactors ProtectiveFactors { get; set; }

        [JsonProperty("clinical_findings")]
        public ClinicalFindings ClinicalFindings { get; set; }

        [JsonProperty("assessment_plan")]
        public AssessmentPlan AssessmentPlan { get; set; }

        [JsonProperty("self_management_goals")]
        public SelfManagementGoals SelfManagementGoals { get; set; }

        [JsonProperty("nursing_recommendations")]
        public string NursingRecommendations { get; set; }
    }

    public class ChildInfo
    {
        [JsonProperty("child_name")]
        public string ChildName { get; set; }

        [JsonProperty("case_number")]
        public string CaseNumber { get; set; }

        [JsonProperty("assessment_date")]
        public string AssessmentDate { get; set; }

        [JsonProperty("date_of_birth")]
        public string DateOfBirth { get; set; }

        [JsonProperty("recent_or_upcoming_dental_appts")]
        public string RecentOrUpcomingDentalAppts { get; set; }

        [JsonProperty("nursing_note")]
        public string NursingNote { get; set; }

        [JsonProperty("required_comment")]
        public string RequiredComment { get; set; }

        [JsonProperty("rn_completing_assessment")]
        public string RNCompletingAssessment { get; set; }
    }

    public class RiskFactors
    {
        [JsonProperty("mother_active_decay")]
        public string MotherActiveDecay { get; set; }

        [JsonProperty("mother_no_dentist")]
        public string MotherNoDentist { get; set; }

        [JsonProperty("bottle_usage")]
        public string BottleUsage { get; set; }

        [JsonProperty("frequent_snacking")]
        public string FrequentSnacking { get; set; }

        [JsonProperty("special_health_care_needs")]
        public string SpecialHealthCareNeeds { get; set; }

        [JsonProperty("medicaid_eligible")]
        public string MedicaidEligible { get; set; }
    }

    public class ProtectiveFactors
    {
        [JsonProperty("existing_dental_home")]
        public bool ExistingDentalHome { get; set; }

        [JsonProperty("fluoridated_water")]
        public bool FluoridatedWater { get; set; }

        [JsonProperty("fluoride_varnish")]
        public string FluorideVarnish { get; set; }

        [JsonProperty("brushing_twice_daily")]
        public bool BrushingTwiceDaily { get; set; }
    }

    public class ClinicalFindings
    {
        [JsonProperty("white_spots")]
        public bool WhiteSpots { get; set; }

        [JsonProperty("obvious_decay")]
        public bool ObviousDecay { get; set; }

        [JsonProperty("fillings_present")]
        public bool FillingsPresent { get; set; }

        [JsonProperty("plaque_accumulation")]
        public bool PlaqueAccumulation { get; set; }

        [JsonProperty("gingivitis")]
        public bool Gingivitis { get; set; }

        [JsonProperty("teeth_present")]
        public bool TeethPresent { get; set; }

        [JsonProperty("healthy_teeth")]
        public bool HealthyTeeth { get; set; }
    }

    public class AssessmentPlan
    {
        [JsonProperty("caries_risk")]
        public string CariesRisk { get; set; }

        [JsonProperty("completed_actions")]
        public CompletedActions CompletedActions { get; set; }
    }

    public class CompletedActions
    {
        [JsonProperty("anticipatory_guidance")]
        public bool AnticipatoryGuidance { get; set; }

        [JsonProperty("fluoride_varnish")]
        public bool FluorideVarnish { get; set; }

        [JsonProperty("dental_referral")]
        public bool DentalReferral { get; set; }
    }

    public class SelfManagementGoals
    {
        [JsonProperty("regular_dental_visits")]
        public bool RegularDentalVisits { get; set; }

        [JsonProperty("dental_treatment_for_caregivers")]
        public bool DentalTreatmentForCaregivers { get; set; }

        [JsonProperty("brush_twice_daily")]
        public bool BrushTwiceDaily { get; set; }

        [JsonProperty("use_fluoride_toothpaste")]
        public bool UseFluorideToothpaste { get; set; }

        [JsonProperty("wean_bottle")]
        public bool WeanBottle { get; set; }

        [JsonProperty("less_or_nojuice")]
        public bool LessOrNoJuice { get; set; }

        [JsonProperty("water_in_sippy")]
        public bool WaterInSippy { get; set; }

        [JsonProperty("drink_tap")]
        public bool DrinkTap { get; set; }

        [JsonProperty("healthy_snacks")]
        public bool HealthySnacks { get; set; }

        [JsonProperty("less_or_nojunkfood")]
        public bool LessOrNoJunkfood { get; set; }

        [JsonProperty("no_soda")]
        public bool NoSoda { get; set; }

        [JsonProperty("xylitol")]
        public bool Xylitol { get; set; }
    }
}
