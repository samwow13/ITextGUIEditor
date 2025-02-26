using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Forms;

namespace iTextDesignerWithGUI.Services
{
    /// <summary>
    /// Utility class for running PowerShell scripts from C#
    /// </summary>
    public class PowerShellScriptRunner
    {
        /// <summary>
        /// Runs a PowerShell script and returns the output
        /// </summary>
        /// <param name="scriptText">The PowerShell script to run</param>
        /// <param name="parameters">Optional parameters to pass to the script</param>
        /// <returns>Output of the script execution</returns>
        public static string RunScript(string scriptText, Dictionary<string, object>? parameters = null)
        {
            StringBuilder outputBuilder = new StringBuilder();
            
            try
            {
                // Create a runspace
                using (Runspace runspace = RunspaceFactory.CreateRunspace())
                {
                    runspace.Open();
                    
                    // Create a pipeline
                    using (PowerShell powershell = PowerShell.Create())
                    {
                        powershell.Runspace = runspace;
                        
                        // Add the script to the pipeline
                        powershell.AddScript(scriptText);
                        
                        // Add parameters if specified
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                powershell.AddParameter(param.Key, param.Value);
                            }
                        }
                        
                        // Execute the script
                        Collection<PSObject> results = powershell.Invoke();
                        
                        // Process errors if any
                        if (powershell.Streams.Error.Count > 0)
                        {
                            foreach (var error in powershell.Streams.Error)
                            {
                                outputBuilder.AppendLine($"Error: {error.Exception.Message}");
                                Debug.WriteLine($"PowerShell Error: {error.Exception.Message}");
                            }
                        }
                        
                        // Process results
                        foreach (PSObject result in results)
                        {
                            if (result != null)
                            {
                                outputBuilder.AppendLine(result.ToString());
                            }
                        }
                    }
                }
                
                return outputBuilder.ToString();
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error executing PowerShell script: {ex.Message}";
                Debug.WriteLine(errorMessage);
                MessageBox.Show(errorMessage, "PowerShell Execution Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return errorMessage;
            }
        }
        
        /// <summary>
        /// Example method that generates an HTML file using PowerShell
        /// </summary>
        /// <param name="fileName">File name without extension</param>
        /// <param name="outputPath">Full path where to save the file</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool GenerateHtmlFileWithPowerShell(string fileName, string outputPath)
        {
            try
            {
                string fullPath = System.IO.Path.Combine(outputPath, $"{fileName}.html");
                
                // PowerShell script to generate the HTML file
                string script = @"
                param(
                    [string]$FileName,
                    [string]$OutputPath
                )
                
                $htmlContent = @""
<!DOCTYPE html>
<html lang=""en"">
  <head>
    <meta charset=""UTF-8"">
    <title>$FileName</title>
    <link href=""globalStyles.css"" rel=""stylesheet"">
  </head>
  <body>
    <div class=""container"">
      <!-- Header with Title -->
      <header>
        <div class=""title"">
          $FileName
        </div>
      </header>

      <!-- Content goes here -->
      <div class=""content"">
        <p>This is a template file for $FileName generated with PowerShell.</p>
        <p>Generated on: $(Get-Date)</p>
      </div>
    </div>
  </body>
</html>
""@
                
                # Create directory if it doesn't exist
                $directory = [System.IO.Path]::GetDirectoryName($OutputPath)
                if (!(Test-Path -Path $directory)) {
                    New-Item -ItemType Directory -Path $directory -Force | Out-Null
                }
                
                # Write the HTML content to the file
                Set-Content -Path $OutputPath -Value $htmlContent -Encoding UTF8
                
                return ""Successfully generated HTML file: $OutputPath""
                ";
                
                // Parameters to pass to the script
                var parameters = new Dictionary<string, object>
                {
                    { "FileName", fileName },
                    { "OutputPath", fullPath }
                };
                
                // Run the script
                string result = RunScript(script, parameters);
                
                // Check if the file was created
                bool success = System.IO.File.Exists(fullPath);
                
                if (success)
                {
                    Debug.WriteLine($"PowerShell successfully generated file: {fullPath}");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"PowerShell script executed but file not found: {fullPath}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GenerateHtmlFileWithPowerShell: {ex.Message}");
                return false;
            }
        }
    }
}
