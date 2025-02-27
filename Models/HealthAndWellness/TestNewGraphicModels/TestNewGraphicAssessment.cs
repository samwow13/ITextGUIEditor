namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for TestNewGraphic assessments
    /// </summary>
    public class TestNewGraphicAssessment : IAssessment
    {
        public string TemplateFileName => "HealthAndWellness/TestNewGraphicTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/HealthAndWellness/TestNewGraphicData.json";
        public string DisplayName => "TestNewGraphic";
    }
}
