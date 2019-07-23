using Com.Synopsys.Integration.Nuget.Inspection.Dispatch;
using Com.Synopsys.Integration.Nuget.Inspection.Model;
using Com.Synopsys.Integration.Nuget.Inspection.Writer;
using Com.Synopsys.Integration.Nuget.Runner;
using Microsoft.Build.Locator;
using System;

namespace IntegrationNugetInspectorPortable
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Registering MSBuild defaults.");
                MSBuildLocator.RegisterDefaults();
            } catch (Exception e)
            {
                Console.WriteLine("Failed to register defaults.");
                Console.Write(e);
            }

            try
            {
                var dispatch = new InspectorDispatch();
                var runner = new CommandLineRunner(dispatch);
                var inspectionResults = runner.Execute(args);
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

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                Environment.Exit(-1);
            }
        }
    }
}
