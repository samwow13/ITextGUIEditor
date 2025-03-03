namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for TestProd assessments
    /// </summary>
    public class TestProdAssessment : IAssessment
    {
        public string TemplateFileName => "HealthAndWellnessProduction/TestProdTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/HealthAndWellnessProduction/TestProdData.json";
        public string DisplayName => "TestProd";
    }
}
