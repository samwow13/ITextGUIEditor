namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for Oral Care assessments
    /// </summary>
    public class OralCareAssessment : IAssessment
    {
        public string TemplateFileName => "AssessmentTemplate.html";
        public string JsonDataPath => "ReferenceDataJsons/OralCareReferenceData.json";
        public string DisplayName => "Oral Care Assessment";
    }
}
