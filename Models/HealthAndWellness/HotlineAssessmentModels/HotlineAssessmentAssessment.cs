namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for HotlineAssessment assessments
    /// </summary>
    public class HotlineAssessmentAssessment : IAssessment
    {
        public string TemplateFileName => "HealthAndWellness/HotlineAssessmentTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/HealthAndWellness/HotlineAssessmentData.json";
        public string DisplayName => "HotlineAssessment";
    }
}
