using System;
using System.Collections.Generic;
using System.Linq;
using Com.Synopsys.Integration.Nuget.DependencyResolution.Nuget;
using NuGet.Packaging;

namespace Com.Synopsys.Integration.Nuget.DependencyResolution.PackagesConfig
{
    class PackagesConfigResolver : IDependencyResolver
    {
        private readonly string PackagesConfigPath;
        private readonly NugetSearchService NugetSearchService;

        public PackagesConfigResolver(string packagesConfigPath, NugetSearchService nugetSearchService)
        {
            PackagesConfigPath = packagesConfigPath;
            NugetSearchService = nugetSearchService;
        }

        public DependencyResult Process()
        {

            List<NugetDependency> dependencies = GetDependencies();

            var result = new DependencyResult();
            result.Packages = CreatePackageSets(dependencies);

            result.Dependencies = new List<Model.PackageId>();
            foreach (var package in result.Packages)
            {
                var anyPackageReferences = result.Packages.Where(pkg => pkg.Dependencies.Contains(package.PackageId)).Any();
                if (!anyPackageReferences)
                {
                    result.Dependencies.Add(package.PackageId);
                }
            }

            return result;
        }

        private List<NugetDependency> GetDependencies()
        {
            System.IO.Stream stream = new System.IO.FileStream(PackagesConfigPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            PackagesConfigReader reader = new PackagesConfigReader(stream);
            List<PackageReference> packages = reader.GetPackages().ToList();

            var dependencies = new List<NugetDependency>();

            foreach (var packageRef in packages)
            {
                string componentName = packageRef.PackageIdentity.Id;
                var version = packageRef.PackageIdentity.Version;
                var versionRange = new NuGet.Versioning.VersionRange(version, true, version, true);
                var framework = NuGet.Frameworks.NuGetFramework.Parse(packageRef.TargetFramework.Framework);

                //TODO: Check that this works.
                var dep = new NugetDependency(componentName, versionRange, framework);
                dependencies.Add(dep);
            }

            return dependencies;
        }

        private List<Model.PackageSet> CreatePackageSets(List<NugetDependency> dependencies)
        {
            try
            {
                var flatResolver = new NugetFlatResolver(NugetSearchService);
                var packages = flatResolver.ProcessAll(dependencies);
                return packages;
            }
            catch (Exception flatException)
            {
                Console.WriteLine("There was an issue processing packages.config as flat: " + flatException.Message);
                try
                {
                    var treeResolver = new NugetTreeResolver(NugetSearchService);
                    treeResolver.AddAll(dependencies);
                    return treeResolver.GetPackageList();
                }
                catch (Exception treeException)
                {
                    Console.WriteLine("There was an issue processing packages.config as a tree: " + treeException.Message);
                    var packages = new List<Model.PackageSet>(dependencies.Select(dependency => dependency.ToEmptyPackageSet()));
                    return packages;
                }
            }
        }
    }
}
