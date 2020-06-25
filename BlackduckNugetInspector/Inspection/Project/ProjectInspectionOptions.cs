using Com.Synopsys.Integration.Nuget.Inspection.Model;

namespace Com.Synopsys.Integration.Nuget.Inspection.Project
{
    class ProjectInspectionOptions : InspectionOptions
    {
        public ProjectInspectionOptions() { }

        public ProjectInspectionOptions(InspectionOptions old)
        {
            this.TargetPath = old.TargetPath;
            this.Verbose = old.Verbose;
            this.PackagesRepoUrl = old.PackagesRepoUrl;
            this.OutputDirectory = old.OutputDirectory;
            this.ExcludedModules = old.ExcludedModules;
            this.IncludedModules = old.IncludedModules;
            this.IgnoreFailure = old.IgnoreFailure;
        }

        public string ProjectName { get; set; }
        public string ProjectDirectory { get; set; }
        public string VersionName { get; set; }
        public string PackagesConfigPath { get; set; }
        public string ProjectJsonPath { get; set; }
        public string ProjectJsonLockPath { get; set; }
        public string ProjectAssetsJsonPath { get; set; }
    }
}
