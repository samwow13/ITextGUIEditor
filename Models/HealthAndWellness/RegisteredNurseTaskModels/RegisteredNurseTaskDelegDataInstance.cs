using System;
using System.Text.Json.Serialization;

namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Represents a registered nurse task delegation data entry.
    /// </summary>
    public class RegisteredNurseTaskDelegDataInstance
    {
        [JsonPropertyName("child_info")]
        public ChildInfoRegisteredNurse ChildInfo { get; set; }

        [JsonPropertyName("caregiver_info")]
        public CaregiverInfo CaregiverInfo { get; set; }

        [JsonPropertyName("delegated_tasks")]
        public DelegatedTasks DelegatedTasks { get; set; }

        [JsonPropertyName("training_details")]
        public TrainingDetails TrainingDetails { get; set; }

        [JsonPropertyName("non_delegated_tasks")]
        public NonDelegatedTasks NonDelegatedTasks { get; set; }

        [JsonPropertyName("nursing_rules")]
        public NursingRules NursingRules { get; set; }

        [JsonPropertyName("instructions_given")]
        public string InstructionsGiven { get; set; }

        [JsonPropertyName("supervisory_visit_notes")]
        public string SupervisoryVisitNotes { get; set; }

        [JsonPropertyName("continue_delegation")]
        public bool ContinueDelegation { get; set; }

        [JsonPropertyName("signatures")]
        public Signatures Signatures { get; set; }
    }

    public class ChildInfoRegisteredNurse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("date_of_birth")]
        public string DateOfBirth { get; set; }

        [JsonPropertyName("person_number")]
        public string PersonNumber { get; set; }
    }

    public class CaregiverInfo
    {
        [JsonPropertyName("delegation_name")]
        public string DelegationName { get; set; }

        [JsonPropertyName("delegation_date")]
        public string DelegationDate { get; set; }

        [JsonPropertyName("delegation_or_assignment")]
        public bool DelegationOrAssignment { get; set; }

        [JsonPropertyName("supervisory_visit")]
        public bool SupervisoryVisit { get; set; }

        [JsonPropertyName("ongoing_supervisory_visit")]
        public bool OngoingSupervisoryVisit { get; set; }
    }

    public class DelegatedTasks
    {
        [JsonPropertyName("gastric_tube_feeding_and_care")]
        public bool GastricTubeFeedingAndCare { get; set; }

        [JsonPropertyName("gastric_tube_feeding_pump")]
        public bool GastricTubeFeedingPump { get; set; }

        [JsonPropertyName("nasogastric_feeding_and_care")]
        public bool NasogastricFeedingAndCare { get; set; }

        [JsonPropertyName("jejunostomy_feeding_and_care")]
        public bool JejunostomyFeedingAndCare { get; set; }

        [JsonPropertyName("ostomy_care")]
        public bool OstomyCare { get; set; }

        [JsonPropertyName("dressing_changes")]
        public bool DressingChanges { get; set; }

        [JsonPropertyName("oxygen_administration")]
        public bool OxygenAdministration { get; set; }

        [JsonPropertyName("oxygen_administration_with_changes")]
        public bool OxygenAdministrationWithChanges { get; set; }

        [JsonPropertyName("pulse_oximeter")]
        public bool PulseOximeter { get; set; }

        [JsonPropertyName("apnea_monitor")]
        public bool ApneaMonitor { get; set; }

        [JsonPropertyName("tracheotomy_care")]
        public bool TracheotomyCare { get; set; }

        [JsonPropertyName("oral_pharyngeal_suctioning")]
        public bool OralPharyngealSuctioning { get; set; }

        [JsonPropertyName("tracheal_suctioning")]
        public bool TrachealSuctioning { get; set; }

        [JsonPropertyName("cpap_bipap")]
        public bool CpapBipap { get; set; }

        [JsonPropertyName("injections")]
        public bool Injections { get; set; }

        [JsonPropertyName("blood_glucose_testing")]
        public bool BloodGlucoseTesting { get; set; }

        [JsonPropertyName("urinary_catheter_care")]
        public bool UrinaryCatheterCare { get; set; }

        [JsonPropertyName("emergent_medications")]
        public bool EmergentMedications { get; set; }

        [JsonPropertyName("specialized_infection_control")]
        public bool SpecializedInfectionControl { get; set; }

        [JsonPropertyName("other_bool")]
        public bool OtherBool { get; set; }

        [JsonPropertyName("other_string")]
        public string OtherString { get; set; }
    }

    public class TrainingDetails
    {
        [JsonPropertyName("explanation_given")]
        public bool ExplanationGiven { get; set; }

        [JsonPropertyName("discussion_occurred")]
        public bool DiscussionOccurred { get; set; }

        [JsonPropertyName("demonstration_performed")]
        public bool DemonstrationPerformed { get; set; }

        [JsonPropertyName("time_for_questions")]
        public bool TimeForQuestions { get; set; }

        [JsonPropertyName("return_demonstration")]
        public bool ReturnDemonstration { get; set; }

        [JsonPropertyName("additional_information")]
        public string AdditionalInformation { get; set; }
    }

    public class NonDelegatedTasks
    {
        [JsonPropertyName("ventilator_care")]
        public bool VentilatorCare { get; set; }

        [JsonPropertyName("central_line_care")]
        public bool CentralLineCare { get; set; }

        [JsonPropertyName("other_bool")]
        public bool OtherBool { get; set; }

        [JsonPropertyName("other_string")]
        public string OtherString { get; set; }
    }

    public class NursingRules
    {
        [JsonPropertyName("delegated_per_OSBN")]
        public bool DelegatedPerOSBN { get; set; }

        [JsonPropertyName("assigned_and_supervised")]
        public bool AssignedAndSupervised { get; set; }

        [JsonPropertyName("caregiver_OSBN_license")]
        public int CaregiverOSBNLicense { get; set; }
    }

    public class Signatures
    {
        [JsonPropertyName("caregiver_signature")]
        public string CaregiverSignature { get; set; }

        [JsonPropertyName("caregiver_signature_date")]
        public string CaregiverSignatureDate { get; set; }

        [JsonPropertyName("registered_nurse_signature")]
        public string RegisteredNurseSignature { get; set; }

        [JsonPropertyName("registered_nurse_signature_date")]
        public string RegisteredNurseSignatureDate { get; set; }
    }
}
