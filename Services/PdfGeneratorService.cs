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
        private object _lastUsedData;
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
        /// Generates a PDF document for a given reference data item using HTML template
        /// </summary>
        /// <param name="data">The reference data item to generate PDF from</param>
        /// <param name="templateFileName">The name of the template file to use</param>
        /// <returns>The generated PDF as a byte array</returns>
        public byte[] GeneratePdf(object data, string templateFileName)
        {
            try
            {
                _lastUsedData = data;
                _lastUsedTemplate = templateFileName;

                var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(exePath))
                {
                    throw new InvalidOperationException("Could not determine application directory");
                }

                var templatePath = Path.Combine(exePath, "Templates", templateFileName);
                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException($"Template file not found: {templatePath}");
                }

                var templateContent = File.ReadAllText(templatePath);

                // Replace placeholders based on data type
                if (data is OralCareDataInstance oralCare)
                {
                    templateContent = ReplacePlaceholders(templateContent, oralCare);
                }
                else if (data is RegisteredNurseTaskDelegDataInstance nurseTask)
                {
                    templateContent = ReplacePlaceholders(templateContent, nurseTask);
                }
                else
                {
                    throw new ArgumentException("Unsupported data type");
                }

                using (var stream = new MemoryStream())
                {
                    var writer = new PdfWriter(stream);
                    var pdf = new PdfDocument(writer);
                    HtmlConverter.ConvertToPdf(templateContent, pdf, new ConverterProperties());
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generating PDF: {ex}");
                throw;
            }
        }

        private string ReplacePlaceholders(string template, OralCareDataInstance data)
        {
            return template
                // Child Information
                .Replace("[[CHILD_NAME]]", data.ChildInfo?.ChildName ?? "")
                .Replace("[[CASE_NUMBER]]", data.ChildInfo?.CaseNumber ?? "")
                .Replace("[[ASSESSMENT_DATE]]", data.ChildInfo?.AssessmentDate ?? "")
                .Replace("[[DATE_OF_BIRTH]]", data.ChildInfo?.DateOfBirth ?? "N/A")
                .Replace("[[DENTAL_APPTS]]", data.ChildInfo?.RecentOrUpcomingDentalAppts ?? "N/A")
                .Replace("[[NURSING_NOTE]]", data.ChildInfo?.NursingNote ?? "N/A")
                .Replace("[[REQUIRED_COMMENT]]", data.ChildInfo?.RequiredComment ?? "N/A")
                .Replace("[[RN_COMPLETING_ASSESSMENT]]", data.ChildInfo?.RNCompletingAssessment ?? "N/A")

                // Risk Factors
                .Replace("[[MOTHER_ACTIVE_DECAY_CLASS]]", GetCheckboxClass(data.RiskFactors?.MotherActiveDecay))
                .Replace("[[MOTHER_NO_DENTIST_CLASS]]", GetCheckboxClass(data.RiskFactors?.MotherNoDentist))
                .Replace("[[BOTTLE_USAGE_CLASS]]", GetCheckboxClass(data.RiskFactors?.BottleUsage))
                .Replace("[[FREQUENT_SNACKING_CLASS]]", GetCheckboxClass(data.RiskFactors?.FrequentSnacking))
                .Replace("[[SPECIAL_HEALTH_CARE_NEEDS_CLASS]]", GetCheckboxClass(data.RiskFactors?.SpecialHealthCareNeeds))
                .Replace("[[MEDICAID_ELIGIBLE_CLASS]]", GetCheckboxClass(data.RiskFactors?.MedicaidEligible))

                // Protective Factors
                .Replace("[[EXISTING_DENTAL_HOME_CLASS]]", GetCheckboxClass(data.ProtectiveFactors?.ExistingDentalHome))
                .Replace("[[FLUORIDATED_WATER_CLASS]]", GetCheckboxClass(data.ProtectiveFactors?.FluoridatedWater))
                .Replace("[[FLUORIDE_VARNISH_CLASS]]", GetCheckboxClass(data.ProtectiveFactors?.FluorideVarnish))
                .Replace("[[BRUSHING_TWICE_DAILY_CLASS]]", GetCheckboxClass(data.ProtectiveFactors?.BrushingTwiceDaily))

                // Clinical Findings
                .Replace("[[WHITE_SPOTS_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.WhiteSpots))
                .Replace("[[OBVIOUS_DECAY_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.ObviousDecay))
                .Replace("[[FILLINGS_PRESENT_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.FillingsPresent))
                .Replace("[[PLAQUE_ACCUMULATION_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.PlaqueAccumulation))
                .Replace("[[GINGIVITIS_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.Gingivitis))
                .Replace("[[TEETH_PRESENT_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.TeethPresent))
                .Replace("[[HEALTHY_TEETH_CLASS]]", GetCheckboxClass(data.ClinicalFindings?.HealthyTeeth))

                // Assessment Plan
                .Replace("[[CARIES_RISK]]", data.AssessmentPlan?.CariesRisk ?? "N/A")
                .Replace("[[ANTICIPATORY_GUIDANCE_CLASS]]", GetCheckboxClass(data.AssessmentPlan?.CompletedActions?.AnticipatoryGuidance))
                .Replace("[[FLUORIDE_VARNISH_COMPLETED_CLASS]]", GetCheckboxClass(data.AssessmentPlan?.CompletedActions?.FluorideVarnish))
                .Replace("[[DENTAL_REFERRAL_CLASS]]", GetCheckboxClass(data.AssessmentPlan?.CompletedActions?.DentalReferral))

                // Self Management Goals
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
                .Replace("[[XYLITOL_CLASS]]", GetCheckboxClass(data.SelfManagementGoals?.Xylitol))

                // Nursing Recommendations
                .Replace("[[NURSING_RECOMMENDATIONS]]", data.NursingRecommendations ?? "N/A");
        }

        private string ReplacePlaceholders(string template, RegisteredNurseTaskDelegDataInstance data)
        {
            return template
                .Replace("{{Name}}", data.ChildInfo?.Name ?? "")
                .Replace("{{DateOfBirth}}", data.ChildInfo?.DateOfBirth ?? "")
                .Replace("{{PersonNumber}}", data.ChildInfo?.PersonNumber ?? "")
                .Replace("{{DelegationName}}", data.CaregiverInfo?.DelegationName ?? "")
                .Replace("{{DelegationDate}}", data.CaregiverInfo?.DelegationDate ?? "")
                .Replace("{{DelegationOrAssignment}}", data.CaregiverInfo?.DelegationOrAssignment.ToString() ?? "")
                .Replace("{{SupervisoryVisit}}", data.CaregiverInfo?.SupervisoryVisit.ToString() ?? "")
                .Replace("{{OngoingSupervisoryVisit}}", data.CaregiverInfo?.OngoingSupervisoryVisit.ToString() ?? "")
                .Replace("{{InstructionsGiven}}", data.InstructionsGiven ?? "")
                .Replace("{{SupervisoryVisitNotes}}", data.SupervisoryVisitNotes ?? "")
                .Replace("{{ContinueDelegation}}", data.ContinueDelegation.ToString() ?? "")
                ;
        }

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
        /// Regenerates the PDF using the last used data and template
        /// </summary>
        /// <returns>The regenerated PDF as a byte array, or null if no previous data exists</returns>
        public byte[] RegeneratePdf()
        {
            if (_lastUsedData == null || string.IsNullOrEmpty(_lastUsedTemplate))
            {
                return null;
            }

            return GeneratePdf(_lastUsedData, _lastUsedTemplate);
        }
    }
}
