namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for TestFileProgress assessments
    /// </summary>
    public class TestFileProgressAssessment : IAssessment
    {
        public string TemplateFileName => "HealthAndWellness/TestFileProgressTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/HealthAndWellness/TestFileProgressData.json";
        public string DisplayName => "TestFileProgress";
    }
}
