using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace VeroEngine.Core.Generic;

// Amazing log class ( Not biased at all )
public static class Log
{
    public static void Info(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "")
    {
        Console.ForegroundColor = ConsoleColor.Cyan; // Info in Cyan
        PrintLog("Info", message, memberName, filePath);
        Console.ResetColor();
    }

    public static void Warn(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "")
    {
        Console.ForegroundColor = ConsoleColor.Yellow; // Warnings in Yellow
        PrintLog("Warn", message, memberName, filePath);
        Console.ResetColor();
    }

    public static void Error(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "")
    {
        Console.ForegroundColor = ConsoleColor.Red; // Errors in Red
        PrintLog("Error", message, memberName, filePath);
        Console.ResetColor();
    }

    private static void PrintLog(string level, string message, string memberName, string filePath)
    {
        // Extracting the class name from the file path (by trimming off everything before the file name)
        var className = Path.GetFileNameWithoutExtension(filePath);

        // Print the log with the class and method info
        Console.WriteLine($"[{level}] {DateTime.Now} - {className}.{memberName}: {message}");
    }
}