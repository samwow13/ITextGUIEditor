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
        private readonly string _templatePath;

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

                _templatePath = Path.Combine(exePath, "Templates", "AssessmentTemplate.html");
                Debug.WriteLine($"Template path: {_templatePath}");

                // Create Templates directory if it doesn't exist
                var templateDir = Path.GetDirectoryName(_templatePath);
                if (!string.IsNullOrEmpty(templateDir) && !Directory.Exists(templateDir))
                {
                    Directory.CreateDirectory(templateDir);
                    Debug.WriteLine($"Created template directory: {templateDir}");
                }

                // Check if template exists
                if (!File.Exists(_templatePath))
                {
                    Debug.WriteLine($"Template file not found at: {_templatePath}");
                    throw new FileNotFoundException($"Template file not found at: {_templatePath}");
                }
                else
                {
                    Debug.WriteLine($"Template file found at: {_templatePath}");
                    Debug.WriteLine($"Template content: {File.ReadAllText(_templatePath)}");
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
        /// <returns>The generated PDF as a byte array</returns>
        public byte[] GeneratePdf(ReferenceDataItem data)
        {
            try
            {
                if (data == null)
                    throw new ArgumentNullException(nameof(data), "Reference data cannot be null");

                if (!File.Exists(_templatePath))
                    throw new FileNotFoundException($"Template file not found at: {_templatePath}");

                // Read and populate the HTML template
                var htmlTemplate = File.ReadAllText(_templatePath);
                Debug.WriteLine($"Read template content: {htmlTemplate}");

                var title = $"Oral Care Assessment - {data.ChildInfo?.ChildName ?? "Unnamed Child"}";
                var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                var populatedHtml = htmlTemplate
                    .Replace("[[TITLE]]", title)
                    .Replace("[[DATE]]", date);
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
