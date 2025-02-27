namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for HotlineTestAssessment assessments
    /// </summary>
    public class HotlineTestAssessmentAssessment : IAssessment
    {
        public string TemplateFileName => "Hotline/HotlineTestAssessmentTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/Hotline/HotlineTestAssessmentData.json";
        public string DisplayName => "HotlineTestAssessment";
    }
}
