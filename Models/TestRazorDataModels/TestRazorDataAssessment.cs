namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for Registered Nurse Task Delegation and Assignment assessments
    /// </summary>
    public class TestRazorDataAssessment : IAssessment
    {
        public string TemplateFileName => "TestRazorDataAssessment.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/TestRazorData.json";
        public string DisplayName => "Test Razor Data";
    }
}
