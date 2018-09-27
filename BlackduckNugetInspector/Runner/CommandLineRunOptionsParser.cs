using Com.Synopsys.Integration.Nuget.Configuration;
using Mono.Options;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Text;

namespace Com.Synopsys.Integration.Nuget.Runner
{
    class CommandLineRunOptionsParser
    {
        public RunOptions ParseArguments(string[] args)
        {
            RunOptions result = new RunOptions();
            OptionSet commandOptions = new OptionSet();

            foreach (var field in typeof(RunOptions).GetFields())
            {
                var attr = GetAttr<Configuration.CommandLineArgAttribute>(field);
                if (attr != null)
                {
                    commandOptions.Add($"{attr.Key}=", attr.Description, (value) => { field.SetValue(result, value); });
                }
            }

            commandOptions.Add("?|h|help", "Display the information on how to use this executable.", value => result.ShowHelp = value != null);
            commandOptions.Add("v|verbose", "Display more messages when the executable runs.", value => result.Verbose = value != null);

            try
            {
                commandOptions.Parse(args);
            }
            catch (OptionException)
            {
                ShowHelpMessage("Error processing command line, usage is: IntegrationNugetInspector.exe [OPTIONS]", commandOptions);
                return null;
            }

            if (result.ShowHelp)
            {
                LogOptions(result);
                ShowHelpMessage("Usage is IntegrationNugetInspector.exe [OPTIONS]", commandOptions);
                return null;
            }

            return result;
        }

        private void ShowHelpMessage(string message, OptionSet optionSet)
        {
            Console.Error.WriteLine(message);
            optionSet.WriteOptionDescriptions(Console.Error);
        }

        private void LogOptions(RunOptions options)
        {
            Console.WriteLine("Configuration Properties: ");
            Console.WriteLine("Property {0} = {1}", CommandLineArgKeys.AppSettingsFile, options.AppSettingsFile);
            Console.WriteLine("Property {0} = {1}", CommandLineArgKeys.TargetPath, options.TargetPath);
            Console.WriteLine("Property {0} = {1}", CommandLineArgKeys.OutputDirectory, options.OutputDirectory);
            Console.WriteLine("Property {0} = {1}", CommandLineArgKeys.IncludedModules, options.IncludedModules);
            Console.WriteLine("Property {0} = {1}", CommandLineArgKeys.ExcludedModules, options.ExcludedModules);
            Console.WriteLine("Property {0} = {1}", CommandLineArgKeys.IgnoreFailures, options.IgnoreFailures);
            Console.WriteLine("Property {0} = {1}", CommandLineArgKeys.PackagesRepoUrl, options.PackagesRepoUrl);
            Console.WriteLine("Property {0} = {1}", CommandLineArgKeys.NugetConfigPath, options.NugetConfigPath);
        }

        public RunOptions LoadAppSettings(string path)
        {

            RunOptions result = new RunOptions();
            System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(result.AppSettingsFile);
            foreach (KeyValueConfigurationElement element in config.AppSettings.Settings)
            {
                foreach (var field in typeof(RunOptions).GetFields())
                {
                    var attr = GetAttr<AppConfigArgAttribute>(field);
                    if (attr != null && element.Key == attr.Key)
                    {
                        field.SetValue(result, element.Value);
                    }
                }
            }

            return result;
        }

        private T GetAttr<T>(FieldInfo field) where T : class
        {
            var attrs = field.GetCustomAttributes(typeof(T), false);
            if (attrs.Length > 0)
            {
                return attrs[0] as T;
            }
            return null;
        }
    }
}
