namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for TestGraphicAgain1 assessments
    /// </summary>
    public class TestGraphicAgain1Assessment : IAssessment
    {
        public string TemplateFileName => "HealthAndWellness/TestGraphicAgain1Template.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/HealthAndWellness/TestGraphicAgain1Data.json";
        public string DisplayName => "TestGraphicAgain1";
    }
}
