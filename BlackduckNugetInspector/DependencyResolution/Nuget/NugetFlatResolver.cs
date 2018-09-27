using NuGet.Packaging.Core;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Synopsys.Integration.Nuget.DependencyResolution.Nuget
{
    //Given a list of dependencies, resolve them such that all packages are shared in a flat list
    //Essentially means that no nodes in the tree that refer to the same package may have different versions.
    //As closely follows the packages.config strategy as it is outlined here: 
    //https://docs.microsoft.com/en-us/nuget/consume-packages/dependency-resolution#dependency-resolution-with-packagesconfig
    public class NugetFlatResolver
    {

        private class ResolutionData
        {
            public string Name;
            public NuGetVersion CurrentVersion;
            public VersionRange ExternalVersionRange = null;
            public Dictionary<string, VersionRange> Dependencies = new Dictionary<string, VersionRange>();
        }

        private NugetSearchService nuget;
        private Dictionary<string, ResolutionData> resolutionData = new Dictionary<string, ResolutionData>();

        public NugetFlatResolver(NugetSearchService service)
        {
            nuget = service;
        }

        private List<VersionRange> FindAllVersionRangesFor(string id)
        {
            id = id.ToLower();
            List<VersionRange> result = new List<VersionRange>();
            foreach (var pkg in resolutionData.Values)
            {
                foreach (var depPair in pkg.Dependencies)
                {
                    if (depPair.Key == id)
                    {
                        result.Add(depPair.Value);
                    }
                }
            }
            return result;
        }

        public List<Model.PackageSet> ProcessAll(List<NugetDependency> packages)
        {
            foreach (NugetDependency package in packages)
            {
                Add(package.Name, package.Name, package.VersionRange, package.Framework);
            }

            var builder = new Model.PackageSetBuilder();
            foreach (ResolutionData data in resolutionData.Values)
            {
                var deps = new HashSet<Model.PackageId>();
                foreach (var dep in data.Dependencies.Keys)
                {
                    if (!resolutionData.ContainsKey(dep))
                    {
                        throw new Exception($"Encountered a dependency but was unable to resolve a package for it: {dep}");
                    }
                    else
                    {
                        deps.Add(new Model.PackageId(resolutionData[dep].Name, resolutionData[dep].CurrentVersion.ToNormalizedString()));
                    }
                }
                builder.AddOrUpdatePackage(new Model.PackageId(data.Name, data.CurrentVersion.ToNormalizedString()), deps);
            }

            return builder.GetPackageList();
        }

        public void Add(string id, string name, VersionRange range, NuGet.Frameworks.NuGetFramework framework)
        {
            id = id.ToLower();
            Resolve(id, name, framework, range);
        }

        private void Resolve(string id, string name, NuGet.Frameworks.NuGetFramework framework = null, VersionRange overrideRange = null)
        {
            id = id.ToLower();
            ResolutionData data;
            if (resolutionData.ContainsKey(id))
            {
                data = resolutionData[id];
                if (overrideRange != null)
                {
                    if (data.ExternalVersionRange == null)
                    {
                        data.ExternalVersionRange = overrideRange;
                    }
                    else
                    {
                        throw new Exception("Can't set more than one external version range.");
                    }
                }
            }
            else
            {
                data = new ResolutionData();
                data.ExternalVersionRange = overrideRange;
                data.Name = name;
                resolutionData[id] = data;
            }

            var allVersions = FindAllVersionRangesFor(id);
            if (data.ExternalVersionRange != null)
            {
                allVersions.Add(data.ExternalVersionRange);
            }
            var combo = VersionRange.CommonSubSet(allVersions);
            var best = nuget.FindBestPackage(id, combo);

            if (best == null)
            {
                Console.WriteLine($"Unable to find package for '{id}' with range '{combo.ToString()}'. Likely a conflict exists in packages.config or the nuget metadata service configured incorrectly.");
                if (data.CurrentVersion == null) data.CurrentVersion = combo.MinVersion;
                return;
            }

            if (data.CurrentVersion == best.Identity.Version) return;

            data.CurrentVersion = best.Identity.Version;
            data.Dependencies.Clear();

            var packages = nuget.DependenciesForPackage(best.Identity, framework);
            foreach (PackageDependency dependency in packages)
            {
                if (!data.Dependencies.ContainsKey(dependency.Id.ToLower()))
                {
                    data.Dependencies.Add(dependency.Id.ToLower(), dependency.VersionRange);
                    Resolve(dependency.Id.ToLower(), dependency.Id, framework);
                }
            }

        }

    }
}
