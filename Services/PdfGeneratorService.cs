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
using System.Text.RegularExpressions;

namespace iTextDesignerWithGUI.Services
{
    /// <summary>
    /// In-memory project implementation for RazorLight template engine.
    /// Stores and manages templates in memory for PDF generation.
    /// </summary>
    public class RazorLightInMemoryProject : RazorLightProject
    {
        private readonly Dictionary<string, string> _templates = new Dictionary<string, string>();

        /// <summary>
        /// Retrieves a RazorLight project item from the in-memory project.
        /// </summary>
        /// <param name="templateKey">Key of the template to retrieve</param>
        /// <returns>RazorLight project item containing the template content</returns>
        public override Task<RazorLightProjectItem> GetItemAsync(string templateKey)
        {
            return Task.FromResult<RazorLightProjectItem>(new TextSourceRazorProjectItem(templateKey, _templates.TryGetValue(templateKey, out string template) ? template : string.Empty));
        }

        /// <summary>
        /// Retrieves a list of imports for a given template key.
        /// </summary>
        /// <param name="templateKey">Key of the template to retrieve imports for</param>
        /// <returns>Empty list of RazorLight project items (no imports are used in this implementation)</returns>
        public override Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey)
        {
            return Task.FromResult<IEnumerable<RazorLightProjectItem>>(Array.Empty<RazorLightProjectItem>());
        }

        /// <summary>
        /// Adds a template to the in-memory project.
        /// </summary>
        /// <param name="key">Key of the template to add</param>
        /// <param name="template">Content of the template to add</param>
        public void AddTemplate(string key, string template)
        {
            _templates[key] = template;
        }
    }

    /// <summary>
    /// Service responsible for generating PDF documents from HTML templates and data models.
    /// Supports both Razor templates (.cshtml) and regular HTML templates with placeholder replacement.
    /// </summary>
    public class PdfGeneratorService
    {
        private readonly string _tempPdfPath;
        private readonly IRazorLightEngine _razorEngine;
        private readonly RazorLightInMemoryProject _project;
        private object _lastUsedData;
        private string _lastUsedTemplate;
        private readonly string _globalStylesPath;

        public PdfGeneratorService()
        {
            try
            {
                var exePath = AppDomain.CurrentDomain.BaseDirectory;
                _tempPdfPath = Path.Combine(exePath, "temp_assessment.pdf");
                _globalStylesPath = Path.Combine(exePath, "Templates", "globalStyles.css");

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
        /// Injects global CSS styles into the HTML content before PDF generation.
        /// </summary>
        /// <param name="htmlContent">The HTML content to inject styles into</param>
        /// <returns>HTML content with injected global styles</returns>
        /// <remarks>
        /// Reads styles from globalStyles.css and injects them into the head section.
        /// If the styles file is not found or head tag is missing, returns original content.
        /// </remarks>
        private string InjectGlobalStyles(string htmlContent)
        {
            try
            {
                if (!File.Exists(_globalStylesPath))
                {
                    Trace.WriteLine($"Global styles file not found: {_globalStylesPath}");
                    return htmlContent;
                }

                var cssContent = File.ReadAllText(_globalStylesPath);
                
                // Find the closing head tag
                const string headEndTag = "</head>";
                var headEndIndex = htmlContent.IndexOf(headEndTag, StringComparison.OrdinalIgnoreCase);
                
                if (headEndIndex == -1)
                {
                    Trace.WriteLine("No </head> tag found in HTML content");
                    return htmlContent;
                }

                // Inject the CSS content within a style tag before the </head>
                var styleTag = $"<style>{cssContent}</style>";
                return htmlContent.Insert(headEndIndex, styleTag);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error injecting global styles: {ex}");
                return htmlContent;
            }
        }

        /// <summary>
        /// Processes image paths in HTML content to convert them to base64 data URIs.
        /// </summary>
        /// <param name="htmlContent">The HTML content containing image tags</param>
        /// <param name="basePath">Base path for resolving relative image paths</param>
        /// <returns>HTML content with image paths converted to base64 data URIs</returns>
        /// <remarks>
        /// This ensures images are embedded in the PDF rather than requiring external files.
        /// If an image cannot be processed, the original img tag is preserved.
        /// </remarks>
        private string ProcessImagePaths(string htmlContent, string basePath)
        {
            try
            {
                // Find all img tags with src attributes
                var imgPattern = @"<img[^>]*src\s*=\s*[""']([^""']*)[""'][^>]*>";
                return Regex.Replace(htmlContent, imgPattern, match =>
                {
                    var imgTag = match.Value;
                    var srcPath = match.Groups[1].Value;

                    // Remove leading '/' or './' if present
                    srcPath = srcPath.TrimStart('/', '.');
                    
                    // Construct full path
                    var fullPath = Path.Combine(basePath, srcPath);
                    
                    if (File.Exists(fullPath))
                    {
                        try
                        {
                            // Read image and convert to base64
                            var imageBytes = File.ReadAllBytes(fullPath);
                            var base64String = Convert.ToBase64String(imageBytes);
                            var mimeType = "image/" + Path.GetExtension(fullPath).TrimStart('.').ToLower();
                            
                            // Replace src with data URI
                            return imgTag.Replace(match.Groups[1].Value, $"data:{mimeType};base64,{base64String}");
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"Error processing image {fullPath}: {ex}");
                            return imgTag; // Keep original tag if processing fails
                        }
                    }
                    
                    Trace.WriteLine($"Image not found: {fullPath}");
                    return imgTag; // Keep original tag if file not found
                });
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in ProcessImagePaths: {ex}");
                return htmlContent; // Return original content if processing fails
            }
        }

        /// <summary>
        /// Generates a PDF document from template and data using iText PDF library.
        /// </summary>
        /// <param name="data">Data model to bind to the template</param>
        /// <param name="templateFileName">Name of the template file in Templates directory</param>
        /// <returns>Generated PDF as byte array</returns>
        /// <remarks>
        /// Supports two template types:
        /// 1. Razor templates (.cshtml) - Uses RazorLight for template compilation
        /// 2. HTML templates - Uses direct placeholder replacement
        /// 
        /// Process flow:
        /// 1. Stores data and template for potential regeneration
        /// 2. Loads and processes template based on type
        /// 3. Injects global styles
        /// 4. Processes images to base64
        /// 5. Converts to PDF using iText
        /// </remarks>
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
                            var renderedHtml = await _razorEngine.CompileRenderStringAsync(key, template, data);
                            return InjectGlobalStyles(renderedHtml);
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
                    templateContent = InjectGlobalStyles(templateContent);
                    
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
                    // ADD FORMS HERE:
                    // For HTML/CSS templates:
                    // 1. Add a new else if block for your form's data model
                    // 2. Create a corresponding ReplacePlaceholders method for the new form type
                    // 3. Implement placeholder replacements using [[PLACEHOLDER]] or {{placeholder}} syntax
                    //
                    // For Razor templates (.cshtml):
                    // 1. Add a new else if block for your form's data model (e.g., TestRazorDataInstance)
                    // 2. Create a .cshtml template in the Templates directory
                    // 3. Use @Model to access your data model properties directly in the template
                    else
                    {
                        throw new ArgumentException("Unsupported data type");
                    }
                }

                // Process image paths before converting to PDF
                var templatesPath = Path.Combine(exePath, "Templates");
                templateContent = ProcessImagePaths(templateContent, templatesPath);

                using var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var converterProperties = new ConverterProperties();
                
                // Set both the base URI and working directory for iText to resolve relative paths
                var templateDir = Path.Combine(exePath, "Templates");
                converterProperties.SetBaseUri(templateDir);
                converterProperties.SetBaseUri(new FileInfo(templatePath).DirectoryName);

                HtmlConverter.ConvertToPdf(templateContent, pdf, converterProperties);

                return stream.ToArray();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error generating PDF: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Generates a PDF using the TestRazorDataAssessment.cshtml template.
        /// </summary>
        /// <param name="data">Data model to bind to the Razor template</param>
        /// <returns>Generated PDF as byte array</returns>
        /// <remarks>
        /// Convenience method that calls GeneratePdf with the TestRazorDataAssessment template.
        /// Used specifically for test data assessment generation.
        /// </remarks>
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

        /// <summary>
        /// Replaces placeholders in HTML template with OralCareDataInstance values.
        /// </summary>
        /// <param name="template">HTML template with placeholders</param>
        /// <param name="data">OralCareDataInstance containing values to insert</param>
        /// <returns>HTML with placeholders replaced with actual values</returns>
        /// <remarks>
        /// Handles placeholders for:
        /// - Child Information
        /// - Risk Factors
        /// - Protective Factors
        /// - Clinical Findings
        /// - Assessment Plan
        /// - Self Management Goals
        /// - Nursing Recommendations
        /// 
        /// Uses null coalescing to handle null values gracefully.
        /// </remarks>
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

        /// <summary>
        /// Replaces placeholders in HTML template with RegisteredNurseTaskDelegDataInstance values.
        /// </summary>
        /// <param name="template">HTML template with placeholders</param>
        /// <param name="data">RegisteredNurseTaskDelegDataInstance containing values to insert</param>
        /// <returns>HTML with placeholders replaced with actual values</returns>
        /// <remarks>
        /// Handles placeholders for:
        /// - Child Info
        /// - Caregiver Info
        /// - Delegated Tasks
        /// - Training Details
        /// - Signatures
        /// 
        /// Uses null coalescing to handle null values gracefully.
        /// </remarks>
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

        /// <summary>
        /// Determines the appropriate checkbox CSS class based on a value.
        /// </summary>
        /// <param name="value">Value to evaluate</param>
        /// <returns>CSS class name for checkbox styling</returns>
        /// <remarks>
        /// Supports multiple value formats:
        /// - Boolean: true/false
        /// - String: "yes"/"no", "true"/"false"
        /// - Special values: "unknown", "na"
        /// 
        /// Returns:
        /// - "checked" for true/yes values
        /// - "" (empty string) for false/no values
        /// - "unknown" for null, unknown, or NA values
        /// </remarks>
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
        /// Regenerates the PDF using the last used data and template.
        /// </summary>
        /// <returns>The regenerated PDF as byte array, or null if no previous data exists</returns>
        /// <remarks>
        /// Uses cached _lastUsedData and _lastUsedTemplate to regenerate PDF.
        /// Useful for refreshing PDF content without requiring new data input.
        /// Returns null if no previous generation data exists.
        /// </remarks>
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
