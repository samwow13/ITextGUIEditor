namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for Tester assessments
    /// </summary>
    public class TesterAssessment : IAssessment
    {
        public string TemplateFileName => "Hotline/TesterTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/Hotline/TesterData.json";
        public string DisplayName => "Tester";
    }
}
