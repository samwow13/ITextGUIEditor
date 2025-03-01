namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for Registered Nurse Task Delegation and Assignment assessments
    /// </summary>
    public class RegisteredNurseTaskDelegAssessment : IAssessment
    {
        public string TemplateFileName => "RNTaskDelegTemplate.html";
        public string JsonDataPath => "ReferenceDataJsons/RegisteredNurseTaskDelegAndAssignData.json";
        public string DisplayName => "Registered Nurse Task Delegation and Assignment";
    }
}
