namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for test1 assessments
    /// </summary>
    public class test1Assessment : IAssessment
    {
        public string TemplateFileName => "Hotline/test1Template.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/Hotline/test1Data.json";
        public string DisplayName => "test1";
    }
}
