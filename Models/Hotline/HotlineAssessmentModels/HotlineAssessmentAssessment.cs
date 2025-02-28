namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for HotlineAssessment assessments
    /// </summary>
    public class HotlineAssessmentAssessment : IAssessment
    {
        public string TemplateFileName => "Hotline/HotlineAssessmentTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/Hotline/HotlineAssessmentData.json";
        public string DisplayName => "HotlineAssessment";
    }
}
