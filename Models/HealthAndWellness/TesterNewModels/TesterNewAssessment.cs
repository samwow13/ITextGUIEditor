namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for TesterNew assessments
    /// </summary>
    public class TesterNewAssessment : IAssessment
    {
        public string TemplateFileName => "HealthAndWellness/TesterNewTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/HealthAndWellness/TesterNewData.json";
        public string DisplayName => "TesterNew";
    }
}
