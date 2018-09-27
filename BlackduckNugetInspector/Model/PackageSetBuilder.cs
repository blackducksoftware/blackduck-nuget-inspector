using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace Com.Synopsys.Integration.Nuget.Model
{
    public class PackageSetBuilder
    {
        private Dictionary<PackageId, PackageSet> packageSets = new Dictionary<PackageId, PackageSet>();
        private Dictionary<PackageId, VersionPair> versions = new Dictionary<PackageId, VersionPair>();


        public bool DoesPackageExist(PackageId id)
        {
            return packageSets.ContainsKey(id);
        }

        public PackageSet GetOrCreatePackageSet(PackageId package)
        {
            PackageSet set;
            if (packageSets.TryGetValue(package, out set))
            {
                return set;
            }
            else
            {
                set = new PackageSet();
                set.PackageId = package;
                set.Dependencies = new HashSet<PackageId>();
                packageSets[package] = set;

                NuGet.Versioning.NuGetVersion version = null;
                NuGet.Versioning.NuGetVersion.TryParse(package.Version, out version);
                versions[package] = new VersionPair() { rawVersion = package.Version, version = version };
                return set;
            }
        }

        public void AddOrUpdatePackage(PackageId id)
        {
            var set = GetOrCreatePackageSet(id);
        }

        public void AddOrUpdatePackage(PackageId id, PackageId dependency)
        {
            var set = GetOrCreatePackageSet(id);
            set.Dependencies.Add(dependency);
        }

        public void AddOrUpdatePackage(PackageId id, HashSet<PackageId> dependencies)
        {
            var set = GetOrCreatePackageSet(id);
            set.Dependencies.UnionWith(dependencies);
        }

        public List<PackageSet> GetPackageList()
        {
            return packageSets.Values.ToList();
        }

        private class VersionPair
        {
            public string rawVersion;
            public NuGet.Versioning.NuGetVersion version;
        }

        public string GetBestVersion(string name, NuGet.Versioning.VersionRange range)
        {
            var allVersions = versions.Keys.Where(key => key.Name == name).Select(key => versions[key]);

            var best = range.FindBestMatch(allVersions.Select(ver => ver.version));

            foreach (var pair in versions)
            {
                if (pair.Key.Name == name && pair.Value.version == best) return pair.Key.Version;
            }

            return null;
        }

    }
}
