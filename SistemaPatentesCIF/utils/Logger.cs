using System.Diagnostics;

namespace SistemaPatentesCIF.utils;

public static class Logger
{
    public static void Info(string source, string message)
    {
        Debug.WriteLine(
       $"🦇 [{DateTime.Now:HH:mm:ss}] [{source}] {message} \n");
        Debug.WriteLine("--- Con tecnologia Logger Copilot 🚔 ---\n");
    }
}
