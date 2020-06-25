using System.Diagnostics;
using NuGet.Common;
using System.Threading.Tasks;

namespace Com.Synopsys.Integration.Nuget.DependencyResolution.Nuget
{
    // For the NuGet API
    public class NugetLogger : NuGet.Common.ILogger
    {
        public void LogDebug(string data) => Trace.WriteLine($"DEBUG: {data}");
        public void LogVerbose(string data) => Trace.WriteLine($"VERBOSE: {data}");
        public void LogInformation(string data) => Trace.WriteLine($"INFORMATION: {data}");
        public void LogMinimal(string data) => Trace.WriteLine($"MINIMAL: {data}");
        public void LogWarning(string data) => Trace.WriteLine($"WARNING: {data}");
        public void LogError(string data) => Trace.WriteLine($"ERROR: {data}");
        public void LogSummary(string data) => Trace.WriteLine($"SUMMARY: {data}");

        public void LogInformationSummary(string data)
        {
            Trace.WriteLine($"INFORMATION SUMMARY: {data}");
        }

        public void LogErrorSummary(string data)
        {
            Trace.WriteLine($"ERROR SUMMARY: {data}");
        }

        public void Log(LogLevel level, string data) => Trace.WriteLine($"{level.ToString()}: {data}");
        public Task LogAsync(LogLevel level, string data) => Task.Run(() => Trace.WriteLine($"{level.ToString()}: {data}"));
        public void Log(ILogMessage message) => Trace.WriteLine($"{message.Level.ToString()}: {message.Message}");
        public Task LogAsync(ILogMessage message) => Task.Run(() => Trace.WriteLine($"{message.Level.ToString()}: {message.Message}"));
    }
}
