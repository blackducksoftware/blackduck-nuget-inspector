using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Synopsys.Integration.Nuget.Inspection.Model
{
    class InspectionOptions
    {
        public string TargetPath { get; set; } = "";
        public bool Verbose { get; set; } = false;
        public string PackagesRepoUrl { get; set; } = "";
        public string NugetConfigPath { get; set; } = "";
        public string OutputDirectory { get; set; } = "";
        public string ExcludedModules { get; set; } = "";
        public string IncludedModules { get; set; } = "";
        public bool IgnoreFailure { get; set; } = false;
    }
}
