namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for HotlineTestersz assessments
    /// </summary>
    public class HotlineTesterszAssessment : IAssessment
    {
        public string TemplateFileName => "Hotline/HotlineTesterszTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/Hotline/HotlineTesterszData.json";
        public string DisplayName => "HotlineTestersz";
    }
}
