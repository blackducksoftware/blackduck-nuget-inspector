using Com.Synopsys.Integration.Nuget.DependencyResolution.Nuget;
using Com.Synopsys.Integration.Nuget.Inspection.Dispatch;
using Com.Synopsys.Integration.Nuget.Inspection.Model;
using System.Collections.Generic;
using System.IO;

namespace Com.Synopsys.Integration.Nuget.Runner
{
    class CommandLineRunner
    {
        private InspectorDispatch Dispatch;

        public CommandLineRunner(InspectorDispatch dispatch)
        {
            Dispatch = dispatch;
        }

        public List<InspectionResult> Execute(string[] args)
        {
            CommandLineRunOptionsParser parser = new CommandLineRunOptionsParser();
            RunOptions options = parser.ParseArguments(args);

            if (options == null) return null;

            if (!string.IsNullOrWhiteSpace(options.AppSettingsFile))
            {
                RunOptions appOptions = parser.LoadAppSettings(options.AppSettingsFile);
                options.Override(appOptions);
            }

            if (string.IsNullOrWhiteSpace(options.TargetPath))
            {
                options.TargetPath = Directory.GetCurrentDirectory();
            }

            InspectionOptions opts = new InspectionOptions()
            {
                ExcludedModules = options.ExcludedModules,
                IncludedModules = options.IncludedModules,
                IgnoreFailure = options.IgnoreFailures == "true",
                OutputDirectory = options.OutputDirectory,
                PackagesRepoUrl = options.PackagesRepoUrl,
                NugetConfigPath = options.NugetConfigPath,
                TargetPath = options.TargetPath,
                Verbose = options.Verbose
            };

            var searchService = new NugetSearchService(options.PackagesRepoUrl, options.NugetConfigPath);
            var inspectionResults = Dispatch.Inspect(opts, searchService);

            return inspectionResults;
        }
    }
}
