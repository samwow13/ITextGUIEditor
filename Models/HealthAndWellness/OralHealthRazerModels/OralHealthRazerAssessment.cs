namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for OralHealthRazer assessments
    /// </summary>
    public class OralHealthRazerAssessment : IAssessment
    {
        public string TemplateFileName => "HealthAndWellness/OralHealthRazerTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/HealthAndWellness/OralHealthRazerData.json";
        public string DisplayName => "OralHealthRazer";
    }
}
