using Com.Synopsys.Integration.Nuget.Inspection.Model;

namespace Com.Synopsys.Integration.Nuget.Inspection.Solution
{
    class SolutionInspectionOptions : InspectionOptions
    {
        public SolutionInspectionOptions() { }

        public SolutionInspectionOptions(InspectionOptions old)
        {
            this.TargetPath = old.TargetPath;
            this.Verbose = old.Verbose;
            this.PackagesRepoUrl = old.PackagesRepoUrl;
            this.OutputDirectory = old.OutputDirectory;
            this.ExcludedModules = old.ExcludedModules;
            this.IncludedModules = old.IncludedModules;
            this.IgnoreFailure = old.IgnoreFailure;
        }

        public string SolutionName { get; set; }
    }
}
