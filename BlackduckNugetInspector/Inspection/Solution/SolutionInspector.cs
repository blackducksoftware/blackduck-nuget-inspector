using Com.Synopsys.Integration.Nuget.DependencyResolution.Nuget;
using System;
using System.Collections.Generic;
using System.Text;
using Com.Synopsys.Integration.Nuget.Inspection.Model;
using System.IO;
using Com.Synopsys.Integration.Nuget.Inspection.Util;
using Com.Synopsys.Integration.Nuget.Model;
using System.Diagnostics;
using System.Linq;
using Com.Synopsys.Integration.Nuget.Inspection.Project;
using Com.Synopsys.Integration.Nuget.Inspection.Exceptions;

namespace Com.Synopsys.Integration.Nuget.Inspection.Solution
{
    class SolutionInspector : IInspector
    {
        private List<string> ExcludedProjectTypeGUIDs = new List<string>() {
            "{2150E333-8FDC-42A3-9474-1A3956D46DE8}"    //Ignore 'Solution Folders'
        };

        public SolutionInspectionOptions Options;
        public NugetSearchService NugetService;

        public SolutionInspector(SolutionInspectionOptions options, NugetSearchService nugetService)
        {
            Options = options;
            NugetService = nugetService;
            if (Options == null)
            {
                throw new Exception("Must provide a valid options object.");
            }

            if (String.IsNullOrWhiteSpace(Options.OutputDirectory))
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                Options.OutputDirectory = PathUtil.Combine(currentDirectory, InspectorUtil.DEFAULT_OUTPUT_DIRECTORY);
            }
            if (String.IsNullOrWhiteSpace(Options.SolutionName))
            {
                Options.SolutionName = Path.GetFileNameWithoutExtension(Options.TargetPath);
            }
        }

        public InspectionResult Inspect()
        {
            try
            {
                return new InspectionResult()
                {
                    Status = InspectionResult.ResultStatus.Success,
                    ResultName = Options.SolutionName,
                    OutputDirectory = Options.OutputDirectory,
                    Containers = new List<Container>() { GetContainer() }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.ToString());
                return new InspectionResult()
                {
                    Status = InspectionResult.ResultStatus.Error,
                    Exception = ex
                };
            }

        }

        public Container GetContainer()
        {
            Console.WriteLine("Processing Solution: " + Options.TargetPath);
            var stopwatch = Stopwatch.StartNew();
            Container solution = new Container();
            solution.Name = Options.SolutionName;
            solution.SourcePath = Options.TargetPath;
            solution.Type = "Solution";
            try
            {

                List<ProjectFile> projectFiles = FindProjectFilesFromSolutionFile(Options.TargetPath, ExcludedProjectTypeGUIDs);
                Console.WriteLine("Parsed Solution File");
                if (projectFiles.Count > 0)
                {
                    string solutionDirectory = Path.GetDirectoryName(Options.TargetPath);
                    Console.WriteLine("Solution directory: {0}", solutionDirectory);

                    var duplicateNames = projectFiles
                        .GroupBy(project => project.Name)
                        .Where(group => group.Count() > 1)
                        .Select(group => group.Key);

                    var duplicatePaths = projectFiles
                        .GroupBy(project => project.Path)
                        .Where(group => group.Count() > 1)
                        .Select(group => group.Key);

                    foreach (ProjectFile project in projectFiles)
                    {
                        try
                        {
                            string projectRelativePath = project.Path;
                            List<string> projectPathSegments = new List<string>();
                            projectPathSegments.Add(solutionDirectory);
                            projectPathSegments.Add(projectRelativePath);

                            string projectPath = InspectorUtil.CreatePath(projectPathSegments);
                            string projectName = project.Name;
                            if (duplicateNames.Contains(projectName))
                            {
                                Console.WriteLine($"Duplicate project name '{projectName}' found. Using GUID instead.");
                                projectName = project.GUID;
                            }

                            ProjectInspector projectInspector = new ProjectInspector(new ProjectInspectionOptions(Options)
                            {
                                ProjectName = projectName,
                                TargetPath = projectPath
                            }, NugetService);

                            InspectionResult projectResult = projectInspector.Inspect();
                            if (projectResult != null && projectResult.Containers != null)
                            {
                                solution.Children.AddRange(projectResult.Containers);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            if (Options.IgnoreFailure)
                            {

                                Console.WriteLine("Error inspecting project: {0}", project.Path);
                                Console.WriteLine("Error inspecting project. Cause: {0}", ex);
                            }
                            else
                            {
                                throw ex;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No project data found for solution {0}", Options.TargetPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                if (Options.IgnoreFailure)
                {

                    Console.WriteLine("Error executing Build BOM task. Cause: {0}", ex);
                }
                else
                {
                    throw ex;
                }
            }

            if (solution != null && solution.Children != null)
            {
                Console.WriteLine("Found " + solution.Children.Count + " children.");
            }
            Console.WriteLine("Finished processing solution: " + Options.TargetPath);
            Console.WriteLine("Took " + stopwatch.ElapsedMilliseconds + " ms to process.");
            return solution;
        }

        private List<ProjectFile> FindProjectFilesFromSolutionFile(string solutionPath, List<string> excludedTypeGUIDs)
        {
            var projects = new List<ProjectFile>();
            // Visual Studio right now is not resolving the Microsoft.Build.Construction.SolutionFile type
            // parsing the solution file manually for now.
            if (File.Exists(solutionPath))
            {
                List<string> contents = new List<string>(File.ReadAllLines(solutionPath));
                var projectLines = contents.FindAll(text => text.StartsWith("Project("));
                foreach (string projectText in projectLines)
                {
                    ProjectFile file = ProjectFile.Parse(projectText);
                    if (file != null)
                    {
                        if (!excludedTypeGUIDs.Contains(file.TypeGUID))
                        {
                            projects.Add(file);
                        }
                    }
                }
                Console.WriteLine("Nuget Inspector found {0} project elements, processed {1} project elements for data", projectLines.Count(), projects.Count());
            }
            else
            {
                throw new BlackDuckInspectorException("Solution File " + solutionPath + " not found");
            }

            return projects;
        }
    }
}
