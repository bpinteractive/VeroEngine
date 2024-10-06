using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace VeroEngine.Core.Generic;

// Amazing log class ( Not biased at all )
public static class Log
{

    public class LogEntry
    {
        public string Message { get; set; }
        public string Level { get; set; }
    }
    
    public static readonly List<LogEntry> LogsSent = new(); // Stores the last 100 logs sent
    public static void Info(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "")
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        PrintLog("Info", message, memberName, filePath);
        Console.ResetColor();
    }

    public static void Warn(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "")
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        PrintLog("Warn", message, memberName, filePath);
        Console.ResetColor();
    }

    public static void Error(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "")
    {
        Console.ForegroundColor = ConsoleColor.Red;
        PrintLog("Error", message, memberName, filePath);
        Console.ResetColor();
    }

    private static void PrintLog(string level, string message, string memberName, string filePath)
    {
        var className = Path.GetFileNameWithoutExtension(filePath);
        
        LogsSent.Add(new()
        {
            Level = level,
            Message = $"[{level}] {DateTime.Now} - {className}.{memberName}: {message}",
        });
        if (LogsSent.Count > 100)
        {
            for (int i = 0; i < LogsSent.Count - 100; i++)
            {
                LogsSent.RemoveAt(0);
            }
        }
        Console.WriteLine($"[{level}] {DateTime.Now} - {className}.{memberName}: {message}");
    }
}