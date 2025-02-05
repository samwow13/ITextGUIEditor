using iText.Html2pdf;
using iText.Kernel.Pdf;
using OralCareReference.Models;
using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace OralCareReference.Services
{
    /// <summary>
    /// Service responsible for generating PDF documents from reference data using HTML templates
    /// </summary>
    public class PdfGeneratorService
    {
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
        public byte[] GeneratePdf(ReferenceDataItem data, string templateFileName)
        {
            try
            {
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
                Debug.WriteLine($"Populated HTML: {populatedHtml}");

                using (var memoryStream = new MemoryStream())
                {
                    try
                    {
                        ConverterProperties converterProperties = new ConverterProperties();
                        HtmlConverter.ConvertToPdf(populatedHtml, memoryStream, converterProperties);
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
