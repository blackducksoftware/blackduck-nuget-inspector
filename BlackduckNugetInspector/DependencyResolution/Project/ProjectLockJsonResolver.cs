using Com.Synopsys.Integration.Nuget.DependencyResolution.Nuget;

namespace Com.Synopsys.Integration.Nuget.DependencyResolution.Project
{
    class ProjectLockJsonResolver : IDependencyResolver
    {
        private string ProjectLockJsonPath;

        public ProjectLockJsonResolver(string projectLockJsonPath)
        {
            ProjectLockJsonPath = projectLockJsonPath;
        }

        public DependencyResult Process()
        {

            NuGet.ProjectModel.LockFile lockFile = NuGet.ProjectModel.LockFileUtilities.GetLockFile(ProjectLockJsonPath, null);

            var resolver = new NugetLockFileResolver(lockFile);

            return resolver.Process();
        }

    }
}
