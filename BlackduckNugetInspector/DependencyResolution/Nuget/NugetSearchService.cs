using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.IO;
using System.Diagnostics;

namespace Com.Synopsys.Integration.Nuget.DependencyResolution.Nuget
{
    public class NugetSearchService
    {
        readonly List<PackageMetadataResource> MetadataResourceList = new List<PackageMetadataResource>();
        readonly List<DependencyInfoResource> DependencyInfoResourceList = new List<DependencyInfoResource>();
        private readonly Dictionary<String, List<IPackageSearchMetadata>> lookupCache = new Dictionary<string, List<IPackageSearchMetadata>>();

        public NugetSearchService(string packagesRepoUrl, string nugetConfig)
        {
            List<Lazy<INuGetResourceProvider>> providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());  // Add v3 API support
            //providers.AddRange(Repository.Provider.GetCoreV2());  // Add v2 API support
            CreateResourceLists(providers, packagesRepoUrl, nugetConfig);
        }

        public IPackageSearchMetadata FindBestPackage(String id, VersionRange versionRange)
        {
            List<IPackageSearchMetadata> matchingPackages = FindPackages(id);
            if (matchingPackages == null) return null;

            var versions = matchingPackages.Select(package => package.Identity.Version);
            var bestVersion = versionRange.FindBestMatch(versions);
            return matchingPackages.Where(package => package.Identity.Version == bestVersion).FirstOrDefault();
        }

        public List<IPackageSearchMetadata> FindPackages(String id)
        {
            if (lookupCache.ContainsKey(id))
            {
                Console.WriteLine("Already looked up package '" + id + "', using the cache.");
            }
            else
            {
                Console.WriteLine("Have not looked up package '" + id + "', using metadata resources.");
                lookupCache[id] = FindPackagesOnline(id);
            }

            return lookupCache[id];
        }

        private List<IPackageSearchMetadata> FindPackagesOnline(String id)
        {
            var matchingPackages = new List<IPackageSearchMetadata>();
            List<Exception> exceptions = new List<Exception>();

            foreach (PackageMetadataResource metadataResource in MetadataResourceList)
            {
                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    var context = new SourceCacheContext();
                    var metaResult = metadataResource.GetMetadataAsync(id, includePrerelease: true, includeUnlisted: true, sourceCacheContext: context, log: new NugetLogger(), token: CancellationToken.None).Result;
                    Console.WriteLine("Took " + stopWatch.ElapsedMilliseconds + " ms to communicate with metadata resource about '" + id + "'");
                    if (metaResult.Count() > 0)
                    {
                        matchingPackages.AddRange(metaResult);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (matchingPackages.Count > 0)
            {
                return matchingPackages;
            }
            else if (exceptions.Count > 0)
            {
                Console.WriteLine($"No packages were found for {id}, and an exception occured in one or more meta data resources.");
                foreach (Exception ex in exceptions)
                {
                    Console.WriteLine("A meta data resource was unable to load it's packages: " + ex.Message);
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine("The reason: " + ex.InnerException.Message);
                    }
                }
                return null;
            }
            else
            {
                Console.WriteLine($"No packages were found for {id} in any meta data resources.");
                return null;
            }
        }

        private void CreateResourceLists(List<Lazy<INuGetResourceProvider>> providers, string packagesRepoUrl, string nugetConfig)
        {
            if (!String.IsNullOrWhiteSpace(nugetConfig))
            {
                if (File.Exists(nugetConfig))
                {
                    string parent = Directory.GetParent(nugetConfig).FullName;
                    string nugetFile = Path.GetFileName(nugetConfig);

                    Console.WriteLine($"Loading nuget config {nugetFile} at {parent}.");
                    var setting = Settings.LoadSpecificSettings(parent, nugetFile);

                    //https://stackoverflow.com/questions/49789283/read-nuget-config-programmatically-in-c-sharp
                    var packageSourceProvider = new PackageSourceProvider(setting);
                    var sources = packageSourceProvider.LoadPackageSources();
                    Console.WriteLine($"Loaded {sources.Count()} package sources from nuget config.");
                    foreach (var source in sources)
                    {
                        Console.WriteLine($"Found package source: {source.Source}");
                        AddPackageSource(providers, source);
                    }
                }
                else
                {
                    Console.WriteLine($"Nuget config path did not exist.");
                }
            }


            string[] splitRepoUrls = packagesRepoUrl.Split(new char[] { ',' });
            foreach (string repoUrl in splitRepoUrls)
            {
                string url = repoUrl.Trim();
                if (!String.IsNullOrWhiteSpace(url))
                {
                    PackageSource packageSource = new PackageSource(url);
                    AddPackageSource(providers, packageSource);
                }
            }

        }

        private void AddPackageSource(List<Lazy<INuGetResourceProvider>> providers, PackageSource packageSource)
        {
            SourceRepository sourceRepository = new SourceRepository(packageSource, providers);
            try
            {
                PackageMetadataResource packageMetadataResource = sourceRepository.GetResource<PackageMetadataResource>();
                MetadataResourceList.Add(packageMetadataResource);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading NuGet Package Meta Data Resource resoure for url: " + packageSource.SourceUri);
                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.Message);
                }
            }
            try
            {
                DependencyInfoResource dependencyInfoResource = sourceRepository.GetResource<DependencyInfoResource>();
                DependencyInfoResourceList.Add(dependencyInfoResource);
                Console.WriteLine("Succesfully added dependency info resource: " + sourceRepository.PackageSource.SourceUri);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading NuGet Dependency Resource resoure for url: " + packageSource.SourceUri);
                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.Message);
                }
            }
        }

        public IEnumerable<PackageDependency> DependenciesForPackage(PackageIdentity identity, NuGet.Frameworks.NuGetFramework framework)
        {
            foreach (DependencyInfoResource dependencyInfoResource in DependencyInfoResourceList)
            {
                try
                {
                    var context = new SourceCacheContext();
                    var infoTask = dependencyInfoResource.ResolvePackage(identity, framework, cacheContext: context, log: new NugetLogger(), token: CancellationToken.None);
                    var result = infoTask.Result;
                    return result.Dependencies;
                }
                catch (Exception e)
                {
                    Console.WriteLine("A dependency resource was unable to load for package: " + identity);
                    if (e.InnerException != null)
                    {
                        Console.WriteLine(e.InnerException.Message);
                    }
                }
            }
            return new List<PackageDependency>();
        }

        private bool FrameworksMatch(PackageDependencyGroup framework1, NugetFramework framework2)
        {
            if (framework1.TargetFramework.IsAny)
            {
                return true;
            }
            else if (framework1.TargetFramework.IsAgnostic)
            {
                return true;
            }
            else if (framework1.TargetFramework.IsSpecificFramework)
            {
                bool majorMatch = framework1.TargetFramework.Version.Major == framework2.Major;
                bool minorMatch = framework1.TargetFramework.Version.Minor == framework2.Minor;
                return majorMatch && minorMatch;
            }
            else if (framework1.TargetFramework.IsUnsupported)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
