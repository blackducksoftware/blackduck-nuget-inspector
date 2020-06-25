using System.Collections.Generic;

namespace Com.Synopsys.Integration.Nuget.DependencyResolution.Project
{
    class ProjectJsonResolver : IDependencyResolver
    {
        private readonly string ProjectName;
        private readonly string ProjectJsonPath;

        public ProjectJsonResolver(string projectName, string projectJsonPath)
        {
            ProjectName = projectName;
            ProjectJsonPath = projectJsonPath;
        }

        public DependencyResult Process()
        {
            var result = new DependencyResult();

            NuGet.ProjectModel.PackageSpec model = NuGet.ProjectModel.JsonPackageSpecReader.GetPackageSpec(ProjectName, ProjectJsonPath);
            IList<NuGet.LibraryModel.LibraryDependency> packages = model.Dependencies;

            foreach (NuGet.LibraryModel.LibraryDependency package in packages)
            {
                var set = new Model.PackageSet
                {
                    PackageId = new Model.PackageId(package.Name, package.LibraryRange.VersionRange.OriginalString)
                };
                result.Packages.Add(set);
                result.Dependencies.Add(set.PackageId);
            }
            return result;
        }
    }
}
