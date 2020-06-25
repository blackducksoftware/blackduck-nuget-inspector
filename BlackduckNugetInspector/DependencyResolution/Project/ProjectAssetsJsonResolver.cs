using Com.Synopsys.Integration.Nuget.DependencyResolution.Nuget;

namespace Com.Synopsys.Integration.Nuget.DependencyResolution.Project
{
    class ProjectAssetsJsonResolver : IDependencyResolver
    {
        private readonly string ProjectAssetsJsonPath;

        public ProjectAssetsJsonResolver(string projectAssetsJsonPath)
        {
            ProjectAssetsJsonPath = projectAssetsJsonPath;
        }

        public DependencyResult Process()
        {

            NuGet.ProjectModel.LockFile lockFile = NuGet.ProjectModel.LockFileUtilities.GetLockFile(ProjectAssetsJsonPath, null);

            var resolver = new NugetLockFileResolver(lockFile);

            return resolver.Process();
        }

    }
}
