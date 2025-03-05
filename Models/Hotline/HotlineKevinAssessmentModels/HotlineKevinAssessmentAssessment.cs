namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for HotlineKevinAssessment assessments
    /// </summary>
    public class HotlineKevinAssessmentAssessment : IAssessment
    {
        public string TemplateFileName => "Hotline/HotlineKevinAssessmentTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/Hotline/HotlineKevinAssessmentData.json";
        public string DisplayName => "HotlineKevinAssessment";
    }
}
