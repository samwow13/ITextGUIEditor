namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for GenerateTest assessments
    /// </summary>
    public class GenerateTestAssessment : IAssessment
    {
        public string TemplateFileName => "HealthAndWellness/GenerateTestTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/HealthAndWellness/GenerateTestData.json";
        public string DisplayName => "GenerateTest";
    }
}
