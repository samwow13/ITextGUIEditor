namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for KansasTest assessments
    /// </summary>
    public class KansasTestAssessment : IAssessment
    {
        public string TemplateFileName => "Hotline/KansasTestTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/Hotline/KansasTestData.json";
        public string DisplayName => "KansasTest";
    }
}
