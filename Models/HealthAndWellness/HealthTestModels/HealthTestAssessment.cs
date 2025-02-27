namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for HealthTest assessments
    /// </summary>
    public class HealthTestAssessment : IAssessment
    {
        public string TemplateFileName => "HealthAndWellness/HealthTestTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/HealthAndWellness/HealthTestData.json";
        public string DisplayName => "HealthTest";
    }
}
