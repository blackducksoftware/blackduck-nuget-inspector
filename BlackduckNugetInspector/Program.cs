using Com.Synopsys.Integration.Nuget.DependencyResolution.Nuget;
using Com.Synopsys.Integration.Nuget.Inspection.Dispatch;
using Com.Synopsys.Integration.Nuget.Inspection.Model;
using Com.Synopsys.Integration.Nuget.Inspection.Writer;
using Com.Synopsys.Integration.Nuget.Runner;
using Microsoft.Build.Locator;
using NuGet.Packaging.Signing;
using System;
using System.IO;

namespace IntegrationNugetInspectorPortable
{
    class Program
    {
        static void Main(string[] args)
        {
            RegisterMSBuild();
            
            int exitCode = 0;
            bool ignoreFailure = false;

            var options = ParseOptions(args);

            if (options.Success && options.Options != null)
            {
                ignoreFailure = options.Options.IgnoreFailure;

                var execution = ExecuteInspectors(options.Options);

                if (execution.ExitCode != 0)
                {
                    exitCode = execution.ExitCode;
                }
            } 
            else
            {
                exitCode = options.ExitCode;
            }

            if (ignoreFailure)
            {
                Console.WriteLine("Desired exit code was ignored: " + exitCode.ToString());
                Environment.Exit(0);
            }
            else
            {
                Environment.Exit(exitCode);
            }
        }

        static void RegisterMSBuild()
        {
            try
            {
                Console.WriteLine("Registering MSBuild defaults.");
                MSBuildLocator.RegisterDefaults();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to register defaults.");
                Console.Write(e);
            }
        }

        static ExecutionResult ExecuteInspectors(InspectionOptions options)
        {
            bool anyFailed = false;

            try
            {
                var dispatch = new InspectorDispatch();
                var searchService = new NugetSearchService(options.PackagesRepoUrl, options.NugetConfigPath);
                var inspectionResults = dispatch.Inspect(options, searchService);
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
                                    anyFailed = true;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error processing inspection result.");
                            Console.WriteLine(e.Message);
                            Console.WriteLine(e.StackTrace);
                            anyFailed = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error iterating inspection results.");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                anyFailed = true;
            }

            if (anyFailed)
            {
                return ExecutionResult.Failed();
            }
            else
            {
                return ExecutionResult.Succeeded();
            }
        }

        static ParsedOptions ParseOptions(string[] args)
        {
            InspectionOptions options = null;
            try
            {
                CommandLineRunOptionsParser parser = new CommandLineRunOptionsParser();
                RunOptions parsedOptions = parser.ParseArguments(args);

                if (parsedOptions == null)
                {
                    return ParsedOptions.Failed();
                }

                if (!string.IsNullOrWhiteSpace(parsedOptions.AppSettingsFile))
                {
                    RunOptions appOptions = parser.LoadAppSettings(parsedOptions.AppSettingsFile);
                    parsedOptions.Override(appOptions);
                }

                if (string.IsNullOrWhiteSpace(parsedOptions.TargetPath))
                {
                    parsedOptions.TargetPath = Directory.GetCurrentDirectory();
                }

                options = new InspectionOptions()
                {
                    ExcludedModules = parsedOptions.ExcludedModules,
                    IncludedModules = parsedOptions.IncludedModules,
                    IgnoreFailure = parsedOptions.IgnoreFailures == "true",
                    OutputDirectory = parsedOptions.OutputDirectory,
                    PackagesRepoUrl = parsedOptions.PackagesRepoUrl,
                    NugetConfigPath = parsedOptions.NugetConfigPath,
                    TargetPath = parsedOptions.TargetPath,
                    Verbose = parsedOptions.Verbose
                };

                return ParsedOptions.Succeeded(options);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to parse options.");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return ParsedOptions.Failed();
            }
        }

        class ParsedOptions
        {
            public InspectionOptions Options;
            public bool Success = false;
            public bool IgnoreFailures = false;
            public int ExitCode = 0;

            public static ParsedOptions Failed(int exitCode = -1)
            {
                return new ParsedOptions()
                {
                    Success = false,
                    ExitCode = exitCode
                };
            }

            public static ParsedOptions Succeeded(InspectionOptions options)
            {
                return new ParsedOptions
                {
                    Success = true,
                    Options = options
                };
            }
        }

        class ExecutionResult
        {
            public bool Success;
            public int ExitCode = 0;

            public static ExecutionResult Failed(int exitCode = -1)
            {
                return new ExecutionResult()
                {
                    Success = false,
                    ExitCode = exitCode
                };
            }

            public static ExecutionResult Succeeded()
            {
                return new ExecutionResult
                {
                    Success = true
                };
            }
        }
    }
}
