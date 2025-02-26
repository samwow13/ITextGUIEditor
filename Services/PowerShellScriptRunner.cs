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
        /// Example method that generates a cshtml file using PowerShell
        /// </summary>
        /// <param name="fileName">File name without extension</param>
        /// <param name="outputPath">Full path where to save the file</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool GenerateCshtmlFileWithPowerShell(string fileName, string outputPath)
        {
            try
            {
                string fullPath = System.IO.Path.Combine(outputPath, $"{fileName}.cshtml");
                
                // PowerShell script to generate the Razor (cshtml) file
                string script = @"
                param(
                    [string]$FileName,
                    [string]$OutputPath
                )
                
                $cshtmlContent = @""
@page
@model $FileName""Model""
@{
    ViewData[""Title""] = ""$FileName"";
}

<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>@ViewData[""Title""]</title>
    <link href=""~/css/globalStyles.css"" rel=""stylesheet"">
</head>
<body>
    <div class=""container"">
        <!-- Header with Title -->
        <header>
            <div class=""title"">
                @ViewData[""Title""]
            </div>
        </header>

        <!-- Content goes here -->
        <div class=""content"">
            <p>This is a template file for @ViewData[""Title""].</p>
            
            @* Razor code example *@
            <div>
                @for (int i = 0; i < 3; i++)
                {
                    <div>Item @i</div>
                }
            </div>
        </div>
    </div>
</body>
</html>
""@
                
                # Create the file
                $cshtmlContent | Out-File -FilePath $OutputPath -Encoding UTF8
                Write-Output ""Cshtml file generated at: $OutputPath""
                ";
                
                // Execute the script
                var parameters = new Dictionary<string, object>
                {
                    { "FileName", fileName },
                    { "OutputPath", fullPath }
                };
                
                var result = RunScript(script, parameters);
                
                if (File.Exists(fullPath))
                {
                    Debug.WriteLine($"Cshtml file generated successfully at: {fullPath}");
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
                Debug.WriteLine($"Error in GenerateCshtmlFileWithPowerShell: {ex.Message}");
                return false;
            }
        }
    }
}
