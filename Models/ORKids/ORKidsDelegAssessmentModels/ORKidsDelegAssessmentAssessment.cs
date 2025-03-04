namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for ORKidsDelegAssessment assessments
    /// </summary>
    public class ORKidsDelegAssessmentAssessment : IAssessment
    {
        public string TemplateFileName => "ORKids/ORKidsDelegAssessmentTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/ORKids/ORKidsDelegAssessmentData.json";
        public string DisplayName => "ORKidsDelegAssessment";
    }
}
