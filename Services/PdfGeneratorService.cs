using iText.Html2pdf;
using iText.Kernel.Pdf;
using iTextDesignerWithGUI.Models;
using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace iTextDesignerWithGUI.Services
{
    /// <summary>
    /// Service responsible for generating PDF documents from reference data using HTML templates
    /// </summary>
    public class PdfGeneratorService
    {
        private readonly string _tempPdfPath;
        private OralCareDataInstance _lastUsedData;
        private string _lastUsedTemplate;

        public PdfGeneratorService()
        {
            try
            {
                // Get the directory where the application is running
                var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(exePath))
                {
                    throw new InvalidOperationException("Could not determine application directory");
                }

                // Create Templates directory if it doesn't exist
                var templateDir = Path.Combine(exePath, "Templates");
                if (!string.IsNullOrEmpty(templateDir) && !Directory.Exists(templateDir))
                {
                    Directory.CreateDirectory(templateDir);
                    Debug.WriteLine($"Created template directory: {templateDir}");
                }

                // Set up consistent temporary file path
                _tempPdfPath = Path.Combine(exePath, "temp_assessment.pdf");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PdfGeneratorService constructor: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Regenerates the PDF using the last used data and template
        /// </summary>
        /// <returns>The regenerated PDF as a byte array, or null if no previous data exists</returns>
        public byte[] RegeneratePdf()
        {
            if (_lastUsedData == null || string.IsNullOrEmpty(_lastUsedTemplate))
            {
                Debug.WriteLine("Cannot regenerate PDF: No previous data or template available");
                return null;
            }

            return GeneratePdf(_lastUsedData, _lastUsedTemplate);
        }

        /// <summary>
        /// Gets the path to the last generated temporary PDF file
        /// </summary>
        public string TempPdfPath => _tempPdfPath;

        /// <summary>
        /// Converts a value to its corresponding checkbox class
        /// </summary>
        private string GetCheckboxClass(object value)
        {
            if (value == null)
                return "unknown";

            if (value is bool boolValue)
                return boolValue ? "checked" : "";

            string strValue = value.ToString().ToLower().Trim();
            
            // Handle Yes/No values
            if (strValue.Equals("yes", StringComparison.OrdinalIgnoreCase))
                return "checked";
            if (strValue.Equals("no", StringComparison.OrdinalIgnoreCase))
                return "";
                
            // Handle True/False values
            if (strValue.Equals("true", StringComparison.OrdinalIgnoreCase))
                return "checked";
            if (strValue.Equals("false", StringComparison.OrdinalIgnoreCase))
                return "";
                
            // Handle special values
            if (strValue.Equals("unknown", StringComparison.OrdinalIgnoreCase) ||
                strValue.Equals("na", StringComparison.OrdinalIgnoreCase))
                return "unknown";
                
            return "unknown";
        }

        /// <summary>
        /// Generates a PDF document for a given reference data item using HTML template
        /// </summary>
        /// <param name="data">The reference data item to generate PDF from</param>
        /// <param name="templateFileName">The name of the template file to use</param>
        /// <returns>The generated PDF as a byte array</returns>
        public byte[] GeneratePdf(OralCareDataInstance data, string templateFileName)
        {
            try
            {
                // Store the data and template for potential regeneration
                _lastUsedData = data;
                _lastUsedTemplate = templateFileName;

                if (data == null)
                    throw new ArgumentNullException(nameof(data), "Reference data cannot be null");

                // Create Templates directory if it doesn't exist
                var templatesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
                if (!Directory.Exists(templatesDir))
                {
                    Directory.CreateDirectory(templatesDir);
                }

                var templatePath = Path.Combine(templatesDir, templateFileName);
                if (!File.Exists(templatePath))
                    throw new FileNotFoundException($"Template file not found at: {templatePath}");

                // Read and populate the HTML template
                var htmlTemplate = File.ReadAllText(templatePath);
                Debug.WriteLine($"Read template content: {htmlTemplate}");

                var title = $"Oral Care Assessment - {data.ChildInfo?.ChildName ?? "Unnamed Child"}";
                var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                var populatedHtml = htmlTemplate
                    .Replace("[[TITLE]]", title)
                    .Replace("[[DATE]]", date)
                    .Replace("[[CHILD_NAME]]", data.ChildInfo?.ChildName ?? "N/A")
                    .Replace("[[CASE_NUMBER]]", data.ChildInfo?.CaseNumber ?? "N/A")
                    .Replace("[[ASSESSMENT_DATE]]", data.ChildInfo?.AssessmentDate ?? "N/A")
                    .Replace("[[DATE_OF_BIRTH]]", data.ChildInfo?.DateOfBirth ?? "N/A")
                    .Replace("[[DENTAL_APPTS]]", data.ChildInfo?.RecentOrUpcomingDentalAppts ?? "N/A")
                    .Replace("[[NURSING_NOTE]]", data.ChildInfo?.NursingNote ?? "N/A")
                    .Replace("[[REQUIRED_COMMENT]]", data.ChildInfo?.RequiredComment ?? "N/A")
                    .Replace("[[RN_COMPLETING_ASSESSMENT]]", data.ChildInfo?.RNCompletingAssessment ?? "N/A");

                // Risk Factors
                populatedHtml = populatedHtml
                    .Replace("[[MOTHER_ACTIVE_DECAY_CLASS]]", GetCheckboxClass(data.RiskFactors?.MotherActiveDecay))
                    .Replace("[[MOTHER_NO_DENTIST_CLASS]]", GetCheckboxClass(data.RiskFactors?.MotherNoDentist))
                    .Replace("[[BOTTLE_USAGE_CLASS]]", GetCheckboxClass(data.RiskFactors?.BottleUsage))
                    .Replace("[[FREQUENT_SNACKING_CLASS]]", GetCheckboxClass(data.RiskFactors?.FrequentSnacking))
                    .Replace("[[SPECIAL_HEALTH_CARE_NEEDS_CLASS]]", GetCheckboxClass(data.RiskFactors?.SpecialHealthCareNeeds))
                    .Replace("[[MEDICAID_ELIGIBLE_CLASS]]", GetCheckboxClass(data.RiskFactors?.MedicaidEligible));

                // Protective Factors
                populatedHtml = populatedHtml
                    .Replace("[[EXISTING_DENTAL_HOME_CLASS]]", GetCheckboxClass(data.ProtectiveFactors?.ExistingDentalHome))
                    .Replace("[[FLUORIDATED_WATER_CLASS]]", GetCheckboxClass(data.ProtectiveFactors?.FluoridatedWater))
                    .Replace("[[FLUORIDE_VARNISH_CLASS]]", GetCheckboxClass(data.ProtectiveFactors?.FluorideVarnish))
                    .Replace("[[BRUSHING_TWICE_DAILY_CLASS]]", GetCheckboxClass(data.ProtectiveFactors?.BrushingTwiceDaily));

                // Clinical Findings
                populatedHtml = populatedHtml
                    .Replace("[[WHITE_SPOTS_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.WhiteSpots))
                    .Replace("[[OBVIOUS_DECAY_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.ObviousDecay))
                    .Replace("[[FILLINGS_PRESENT_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.FillingsPresent))
                    .Replace("[[PLAQUE_ACCUMULATION_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.PlaqueAccumulation))
                    .Replace("[[GINGIVITIS_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.Gingivitis))
                    .Replace("[[TEETH_PRESENT_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.TeethPresent))
                    .Replace("[[HEALTHY_TEETH_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.HealthyTeeth));

                // Assessment Plan
                populatedHtml = populatedHtml
                    .Replace("[[CARIES_RISK]]", data.AssessmentPlan?.CariesRisk ?? "N/A")
                    .Replace("[[ANTICIPATORY_GUIDANCE_CLASS]]", GetCheckboxClass(data.AssessmentPlan?.CompletedActions?.AnticipatoryGuidance))
                    .Replace("[[FLUORIDE_VARNISH_COMPLETED_CLASS]]", GetCheckboxClass(data.AssessmentPlan?.CompletedActions?.FluorideVarnish))
                    .Replace("[[DENTAL_REFERRAL_CLASS]]", GetCheckboxClass(data.AssessmentPlan?.CompletedActions?.DentalReferral));

                // Self Management Goals
                populatedHtml = populatedHtml
                    .Replace("[[REGULAR_DENTAL_VISITS_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.RegularDentalVisits))
                    .Replace("[[DENTAL_TREATMENT_CAREGIVERS_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.DentalTreatmentForCaregivers))
                    .Replace("[[BRUSH_TWICE_DAILY_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.BrushTwiceDaily))
                    .Replace("[[USE_FLUORIDE_TOOTHPASTE_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.UseFluorideToothpaste))
                    .Replace("[[WEAN_BOTTLE_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.WeanBottle))
                    .Replace("[[LESS_OR_NO_JUICE_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.LessOrNoJuice))
                    .Replace("[[WATER_IN_SIPPY_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.WaterInSippy))
                    .Replace("[[DRINK_TAP_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.DrinkTap))
                    .Replace("[[HEALTHY_SNACKS_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.HealthySnacks))
                    .Replace("[[LESS_OR_NO_JUNKFOOD_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.LessOrNoJunkfood))
                    .Replace("[[NO_SODA_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.NoSoda))
                    .Replace("[[XYLITOL_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.Xylitol));

                // Nursing Recommendations
                populatedHtml = populatedHtml
                    .Replace("[[NURSING_RECOMMENDATIONS]]", data.NursingRecommendations ?? "N/A");

                Debug.WriteLine($"Populated HTML: {populatedHtml}");

                using (var memoryStream = new MemoryStream())
                {
                    try
                    {
                        ConverterProperties converterProperties = new ConverterProperties();
                        HtmlConverter.ConvertToPdf(populatedHtml, memoryStream, converterProperties);

                        // Save to consistent temporary file location
                        if (File.Exists(_tempPdfPath))
                        {
                            File.Delete(_tempPdfPath);
                        }
                        File.WriteAllBytes(_tempPdfPath, memoryStream.ToArray());
                        
                        return memoryStream.ToArray();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error converting HTML to PDF: {ex}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GeneratePdf: {ex}");
                throw;
            }
        }
    }
}
