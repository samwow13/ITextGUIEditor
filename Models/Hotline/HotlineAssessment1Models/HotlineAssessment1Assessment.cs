namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for HotlineAssessment1 assessments
    /// </summary>
    public class HotlineAssessment1Assessment : IAssessment
    {
        public string TemplateFileName => "Hotline/HotlineAssessment1Template.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/Hotline/HotlineAssessment1Data.json";
        public string DisplayName => "HotlineAssessment1";
    }
}
