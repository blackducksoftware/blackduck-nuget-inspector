using Com.Synopsys.Integration.Nuget.Inspection.Dispatch;
using Com.Synopsys.Integration.Nuget.Runner;
using System;

namespace IntegrationNugetInspectorPortable
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var dispatch = new InspectorDispatch();
                var runner = new CommandLineRunner(dispatch);
                runner.Execute(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                Environment.Exit(-1);
            }
        }
    }
}
