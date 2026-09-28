using System;
using System.IO;
using System.Runtime.CompilerServices;

internal static class UiAutomationDiagnostics
{
    [ThreadStatic] private static bool writing;

    [ModuleInitializer]
    internal static void Initialize()
    {
        var path = Environment.GetEnvironmentVariable("VOCABMATE_UIA_DIAGNOSTICS");
        if (string.IsNullOrWhiteSpace(path)) return;
        AppDomain.CurrentDomain.FirstChanceException += (_, arguments) =>
        {
            if (writing || arguments.Exception is OperationCanceledException) return;
            writing = true;
            try { File.AppendAllText(path, DateTimeOffset.UtcNow.ToString("O") + " " + arguments.Exception + Environment.NewLine); }
            catch { }
            finally { writing = false; }
        };
    }
}
