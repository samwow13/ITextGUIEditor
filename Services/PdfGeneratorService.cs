using iText.Html2pdf;
using iText.Kernel.Pdf;
using iTextDesignerWithGUI.Models;
using iTextDesignerWithGUI.Models.TestRazorDataModels;
using RazorLight;
using RazorLight.Razor;
using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace iTextDesignerWithGUI.Services
{
    public class RazorLightInMemoryProject : RazorLightProject
    {
        private readonly Dictionary<string, string> _templates = new Dictionary<string, string>();

        public override Task<RazorLightProjectItem> GetItemAsync(string templateKey)
        {
            return Task.FromResult<RazorLightProjectItem>(new TextSourceRazorProjectItem(templateKey, _templates.TryGetValue(templateKey, out string template) ? template : string.Empty));
        }

        public override Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey)
        {
            return Task.FromResult<IEnumerable<RazorLightProjectItem>>(Array.Empty<RazorLightProjectItem>());
        }

        public void AddTemplate(string key, string template)
        {
            _templates[key] = template;
        }
    }

    /// <summary>
    /// Service responsible for generating PDF documents from reference data using HTML templates
    /// </summary>
    public class PdfGeneratorService
    {
        private readonly string _tempPdfPath;
        private readonly IRazorLightEngine _razorEngine;
        private readonly RazorLightInMemoryProject _project;
        private object _lastUsedData;
        private string _lastUsedTemplate;

        public PdfGeneratorService()
        {
            try
            {
                var exePath = AppDomain.CurrentDomain.BaseDirectory;
                _tempPdfPath = Path.Combine(exePath, "temp_assessment.pdf");

                // Initialize RazorLight engine
                _project = new RazorLightInMemoryProject();
                _razorEngine = new RazorLightEngineBuilder()
                    .UseMemoryCachingProvider()
                    .UseProject(_project)
                    .Build();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error initializing PdfGeneratorService: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Generates a PDF document from a template file and data
        /// </summary>
        public byte[] GeneratePdf(object data, string templateFileName)
        {
            try
            {
                _lastUsedData = data;
                _lastUsedTemplate = templateFileName;

                var exePath = AppDomain.CurrentDomain.BaseDirectory;
                var templatePath = Path.Combine(exePath, "Templates", templateFileName);
                string templateContent;

                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException($"Template file not found: {templatePath}");
                }

                if (templateFileName.EndsWith(".cshtml"))
                {
                    // Process Razor template
                    templateContent = Task.Run(async () =>
                    {
                        var key = Path.GetFileNameWithoutExtension(templateFileName);
                        var template = await File.ReadAllTextAsync(templatePath);
                        
                        // Add template to the project
                        _project.AddTemplate(key, template);

                        try
                        {
                            return await _razorEngine.CompileRenderStringAsync(key, template, data);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"Error compiling Razor template: {ex}");
                            throw;
                        }
                    }).Result;
                }
                else
                {
                    // Process regular HTML template
                    templateContent = File.ReadAllText(templatePath);
                    if (data is OralCareDataInstance oralCare)
                    {
                        templateContent = ReplacePlaceholders(templateContent, oralCare);
                    }
                    else if (data is RegisteredNurseTaskDelegDataInstance nurseTask)
                    {
                        templateContent = ReplacePlaceholders(templateContent, nurseTask);
                    }
                    else if (data is TestRazorDataInstance)
                    {
                        // For Razor templates, we don't need to replace placeholders
                        // The Razor engine will handle the data binding
                    }
                    else
                    {
                        throw new ArgumentException("Unsupported data type");
                    }
                }

                using var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var converterProperties = new ConverterProperties();

                HtmlConverter.ConvertToPdf(templateContent, pdf, converterProperties);

                return stream.ToArray();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error generating PDF: {ex}");
                throw;
            }
        }

        public byte[] GenerateTestRazorDataPdf(object data)
        {
            try
            {
                return GeneratePdf(data, "TestRazorDataAssessment.cshtml");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error generating Test Razor Data PDF: {ex}");
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
                // Child Info
                .Replace("{{child_info.name}}", data.ChildInfo?.Name ?? "")
                .Replace("{{child_info.date_of_birth}}", data.ChildInfo?.DateOfBirth ?? "")
                .Replace("{{child_info.person_number}}", data.ChildInfo?.PersonNumber ?? "")

                // Caregiver Info
                .Replace("{{caregiver_info.delegation_name}}", data.CaregiverInfo?.DelegationName ?? "")
                .Replace("{{caregiver_info.delegation_date}}", data.CaregiverInfo?.DelegationDate ?? "")
                .Replace("[[INITIAL_DELEGATION_CLASS]]", GetCheckboxClass(data.CaregiverInfo?.DelegationOrAssignment))
                .Replace("[[SUPERVISORY_VISIT_CLASS]]", GetCheckboxClass(data.CaregiverInfo?.SupervisoryVisit))
                .Replace("[[ONGOING_SUPERVISORY_VISIT_CLASS]]", GetCheckboxClass(data.CaregiverInfo?.OngoingSupervisoryVisit))

                // Delegated Tasks - Checkbox Classes
                .Replace("[[GASTRIC_TUBE_FEEDING_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.GastricTubeFeedingAndCare))
                .Replace("[[GASTRIC_TUBE_PUMP_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.GastricTubeFeedingPump))
                .Replace("[[NASOGASTRIC_FEEDING_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.NasogastricFeedingAndCare))
                .Replace("[[JEJUNOSTOMY_FEEDING_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.JejunostomyFeedingAndCare))
                .Replace("[[OSTOMY_CARE_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.OstomyCare))
                .Replace("[[DRESSING_CHANGES_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.DressingChanges))
                .Replace("[[OXYGEN_ADMIN_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.OxygenAdministration))
                .Replace("[[OXYGEN_ADMIN_CHANGES_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.OxygenAdministrationWithChanges))
                .Replace("[[PULSE_OXIMETER_CLASS]]", GetCheckboxClass(data.DelegatedTasks?.PulseOximeter))

                // Training Details
                .Replace("{{instructions_given}}", data.InstructionsGiven ?? "")
                .Replace("{{supervisory_visit_notes}}", data.SupervisoryVisitNotes ?? "")
                .Replace("[[CONTINUE_DELEGATION_CLASS]]", GetCheckboxClass(data.ContinueDelegation))

                // Signatures
                .Replace("{{signatures.rn_signature}}", data.Signatures?.RegisteredNurseSignature ?? "")
                .Replace("{{signatures.rn_date}}", data.Signatures?.RegisteredNurseSignatureDate ?? "")
                .Replace("{{signatures.caregiver_signature}}", data.Signatures?.CaregiverSignature ?? "")
                .Replace("{{signatures.caregiver_date}}", data.Signatures?.CaregiverSignatureDate ?? "");
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
