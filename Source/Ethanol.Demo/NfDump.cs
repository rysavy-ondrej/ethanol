using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.IO;

public static class NfDump
{

    /// <summary>
    /// Executes the nfdump for the given input file to produce a target csv file. 
    /// </summary>
    /// <param name="sourceFile">The source nfdump file.</param>
    /// <param name="targetFile">The target CSV file.</param>
    /// <param name="filter">An nfdump filter to be applied.</param>
    /// <returns>true on success; false otherwise.</returns>
    public static bool Execute(string sourceFile, string targetFile, string filter)
    {
        using var targetWriter = File.CreateText(targetFile);
        var startInfo = new ProcessStartInfo()
        {
            FileName = "/usr/local/bin/nfdump",
            ArgumentList = { "-R", sourceFile, "-o", "csv", filter },
            UseShellExecute = false, 
            CreateNoWindow = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var process = new Process()
        {
            StartInfo = startInfo,
        };
        Console.WriteLine($"INFO: {startInfo.FileName} {string.Join(" ", startInfo.ArgumentList)}");

        process.OutputDataReceived += (sender, data) => {
            targetWriter.Write(data.Data);
        };
        process.ErrorDataReceived += (sender, data) => {
            System.Console.WriteLine(data.Data);
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            targetWriter.Close();
            return true;
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"ERROR: execution failed: {ex.Message}");
            return false;
        }
    }
}

