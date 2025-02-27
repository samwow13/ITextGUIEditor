namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for tester assessments
    /// </summary>
    public class testerAssessment : IAssessment
    {
        public string TemplateFileName => "HealthAndWellness/testerTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/HealthAndWellness/testerData.json";
        public string DisplayName => "tester";
    }
}
