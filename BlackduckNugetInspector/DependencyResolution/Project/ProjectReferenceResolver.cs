using Com.Synopsys.Integration.Nuget.DependencyResolution.Nuget;
using Com.Synopsys.Integration.Nuget.Model;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.Synopsys.Integration.Nuget.DependencyResolution.Project
{
    class ProjectReferenceResolver : IDependencyResolver
    {

        private readonly string ProjectPath;
        private readonly NugetSearchService NugetSearchService;
        public ProjectReferenceResolver(string projectPath, NugetSearchService nugetSearchService)
        {
            ProjectPath = projectPath;
            NugetSearchService = nugetSearchService;
        }

        public DependencyResult Process()
        {
            try
            {
                var tree = new NugetTreeResolver(NugetSearchService);

                Microsoft.Build.Evaluation.Project proj = new Microsoft.Build.Evaluation.Project(ProjectPath);

                List<NugetDependency> deps = new List<NugetDependency>();
                foreach (ProjectItem reference in proj.GetItemsIgnoringCondition("PackageReference"))
                {
                    var versionMetaData = reference.Metadata.Where(meta => meta.Name == "Version").FirstOrDefault().EvaluatedValue;
                    if (NuGet.Versioning.VersionRange.TryParse(versionMetaData, out NuGet.Versioning.VersionRange version))
                    {
                        var dep = new NugetDependency(reference.EvaluatedInclude, version);
                        deps.Add(dep);
                    }
                    else
                    {
                        Console.WriteLine("Framework dependency had no version, will not be included: " + reference.EvaluatedInclude);
                    }
                }

                foreach (ProjectItem reference in proj.GetItemsIgnoringCondition("Reference"))
                {
                    if (reference.Xml != null && !String.IsNullOrWhiteSpace(reference.Xml.Include) && reference.Xml.Include.Contains("Version="))
                    {

                        string packageInfo = reference.Xml.Include;

                        var artifact = packageInfo.Substring(0, packageInfo.IndexOf(","));

                        string versionKey = "Version=";
                        int versionKeyIndex = packageInfo.IndexOf(versionKey);
                        int versionStartIndex = versionKeyIndex + versionKey.Length;
                        string packageInfoAfterVersionKey = packageInfo.Substring(versionStartIndex);

                        string seapirater = ",";
                        string version;
                        if (packageInfoAfterVersionKey.Contains(seapirater))
                        {
                            int firstSeapirater = packageInfoAfterVersionKey.IndexOf(seapirater);
                            version = packageInfoAfterVersionKey.Substring(0, firstSeapirater);
                        }
                        else
                        {
                            version = packageInfoAfterVersionKey;
                        }

                        var dep = new NugetDependency(artifact, NuGet.Versioning.VersionRange.Parse(version));
                        deps.Add(dep);
                    }
                }
                ProjectCollection.GlobalProjectCollection.UnloadProject(proj);

                foreach (var dep in deps)
                {
                    tree.Add(dep);
                }

                var result = new DependencyResult()
                {
                    Success = true,
                    Packages = tree.GetPackageList(),
                    Dependencies = new List<PackageId>()
                };

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
            catch (InvalidProjectFileException e)
            {
                return new DependencyResult()
                {
                    Success = false
                };
            }
        }
    }
}
