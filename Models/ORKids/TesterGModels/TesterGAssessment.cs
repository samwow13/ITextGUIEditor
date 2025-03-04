namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for TesterG assessments
    /// </summary>
    public class TesterGAssessment : IAssessment
    {
        public string TemplateFileName => "ORKids/TesterGTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/ORKids/TesterGData.json";
        public string DisplayName => "TesterG";
    }
}
