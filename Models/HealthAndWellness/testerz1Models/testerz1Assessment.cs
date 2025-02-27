namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for testerz1 assessments
    /// </summary>
    public class testerz1Assessment : IAssessment
    {
        public string TemplateFileName => "HealthAndWellness/testerz1Template.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/HealthAndWellness/testerz1Data.json";
        public string DisplayName => "testerz1";
    }
}
