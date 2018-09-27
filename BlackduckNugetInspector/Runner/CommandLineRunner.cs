using Com.Synopsys.Integration.Nuget.Configuration;
using Com.Synopsys.Integration.Nuget.DependencyResolution.Nuget;
using Com.Synopsys.Integration.Nuget.Inspection.Dispatch;
using Com.Synopsys.Integration.Nuget.Inspection.Model;
using Com.Synopsys.Integration.Nuget.Inspection.Writer;
using Mono.Options;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Reflection;

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

            if (inspectionResults != null)
            {
                foreach (var result in inspectionResults)
                {
                    try
                    {
                        if (result.ResultName != null)
                        {
                            Console.WriteLine("Inspection: " + result.ResultName);
                        }
                        if (result.Status == InspectionResult.ResultStatus.Success)
                        {
                            Console.WriteLine("Inspection Result: Success");
                            var writer = new InspectionResultJsonWriter(result);
                            writer.Write();
                            Console.WriteLine("Info file created at {0}", writer.FilePath());
                        }
                        else
                        {
                            Console.WriteLine("Inspection Result: Error");
                            if (result.Exception != null)
                            {
                                Console.WriteLine("Exception:");
                                Console.WriteLine(result.Exception);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error processing inspection result.");
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }

                }
            }

            return inspectionResults;
        }
    }
}
