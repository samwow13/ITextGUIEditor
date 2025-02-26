using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using iText.Layout;
using iText.Layout.Element;
using RazorLight;
using RazorLight.Razor;
using iTextDesignerWithGUI.Services.HelperMethods;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using iTextDesignerWithGUI.Models;
using iTextDesignerWithGUI.Models.TestRazorDataModels;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;

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
        /// Processes the template based on its type (Razor or HTML) and returns the rendered content.
        /// </summary>
        private string ProcessTemplate(string templatePath, string templateFileName, object data)
        {
            if (templateFileName.EndsWith(".cshtml"))
            {
                return ProcessRazorTemplate(templatePath, templateFileName, data);
            }
            return ProcessHtmlTemplate(templatePath, data);
        }

        /// <summary>
        /// Processes a Razor template using the RazorLight engine.
        /// </summary>
        private string ProcessRazorTemplate(string templatePath, string templateFileName, object data)
        {
            return Task.Run(async () =>
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

        /// <summary>
        /// Processes an HTML template by replacing placeholders based on the data type.
        /// </summary>
        private string ProcessHtmlTemplate(string templatePath, object data)
        {
            var templateContent = File.ReadAllText(templatePath);
            templateContent = InjectGlobalStyles(templateContent);
            
            return ProcessDataModel(templateContent, data);
        }

        /// <summary>
        /// Processes the data model and replaces placeholders in the template content.
        /// </summary>
        private string ProcessDataModel(string templateContent, object data)
        {
            switch (data)
            {//ADD FORMS HERE
                case OralCareDataInstance oralCare:
                    return HTMLTemplateMethods.ReplacePlaceholders(templateContent, oralCare);
                case RegisteredNurseTaskDelegDataInstance nurseTask:
                    return HTMLTemplateMethods.ReplacePlaceholders(templateContent, nurseTask);
                case TestRazorDataInstance _:
                    return templateContent; // Razor templates handle data binding internally
                default:
                    throw new ArgumentException("Unsupported data type");
            }
        }

        /// <summary>
        /// Converts the processed HTML content to a PDF document.
        /// </summary>
        private byte[] ConvertToPdf(string templateContent, string templatePath, string exePath)
        {
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
                var binTemplatesPath = Path.Combine(exePath, "Templates", templateFileName);
                
                // Also check for templates in the source directory
                var projectDir = Path.GetFullPath(Path.Combine(exePath, "..\\..\\.."));
                var sourceTemplatesPath = Path.Combine(projectDir, "Templates", templateFileName);
                
                string templatePath;
                
                // Prioritize source templates over compiled templates
                if (File.Exists(sourceTemplatesPath))
                {
                    templatePath = sourceTemplatesPath;
                    Trace.WriteLine($"Using source template: {templatePath}");
                }
                else if (File.Exists(binTemplatesPath))
                {
                    templatePath = binTemplatesPath;
                    Trace.WriteLine($"Using compiled template: {templatePath}");
                }
                else
                {
                    throw new FileNotFoundException($"Template file not found in either source or bin directories: {templateFileName}");
                }

                // Process the template based on its type
                var templateContent = ProcessTemplate(templatePath, templateFileName, data);

                // Process image paths before converting to PDF
                // Use the directory of the template we found
                var templatesPath = Path.GetDirectoryName(Path.GetDirectoryName(templatePath));
                templateContent = ProcessImagePaths(templateContent, templatesPath);

                // Convert the processed content to PDF
                return ConvertToPdf(templateContent, templatePath, exePath);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error generating PDF: {ex}");
                throw;
            }
        }
    }
}
