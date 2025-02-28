namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for ORkidsTest assessments
    /// </summary>
    public class ORkidsTestAssessment : IAssessment
    {
        public string TemplateFileName => "ORKids/ORkidsTestTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/ORKids/ORkidsTestData.json";
        public string DisplayName => "ORkidsTest";
    }
}
