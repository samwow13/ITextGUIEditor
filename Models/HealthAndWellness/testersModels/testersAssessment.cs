namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for testers assessments
    /// </summary>
    public class testersAssessment : IAssessment
    {
        public string TemplateFileName => "HealthAndWellness/testersTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/HealthAndWellness/testersData.json";
        public string DisplayName => "testers";
    }
}
