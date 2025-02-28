namespace iTextDesignerWithGUI.Models
{
    /// <summary>
    /// Implementation of IAssessment for TestersRFF assessments
    /// </summary>
    public class TestersRFFAssessment : IAssessment
    {
        public string TemplateFileName => "RFI/TestersRFFTemplate.cshtml";
        public string JsonDataPath => "ReferenceDataJsons/RFI/TestersRFFData.json";
        public string DisplayName => "TestersRFF";
    }
}
