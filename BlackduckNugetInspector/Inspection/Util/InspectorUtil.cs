using System;
using System.Collections.Generic;
using System.IO;

namespace Com.Synopsys.Integration.Nuget.Inspection.Util
{
    class InspectorUtil
    {
        public const string DEFAULT_OUTPUT_DIRECTORY = "blackduck";

        public static string GetProjectAssemblyVersion(string projectDirectory)
        {
            string version = null;
            try
            {
                string[] assemblyInfoPaths = Directory.GetFiles(projectDirectory, "*AssemblyInfo.*", SearchOption.AllDirectories);
                foreach (string path in assemblyInfoPaths)
                {
                    if (path.EndsWith(".obj"))
                    {
                        Console.WriteLine("Will not use obj assembly path: {0}", path);
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("Looking for version in assembly path {0}", path);
                    }
                    if (File.Exists(path))
                    {

                        List<string> contents = new List<string>(File.ReadAllLines(path));
                        List<string> versionText = contents.FindAll(text => text.Contains("AssemblyFileVersion"));
                        if (versionText == null || versionText.Count == 0)
                        {
                            versionText = contents.FindAll(text => text.Contains("AssemblyVersion"));
                        }
                        if (versionText != null)
                        {
                            foreach (string text in versionText)
                            {
                                String versionLine = text.Trim();
                                if (!versionLine.StartsWith("//"))
                                {
                                    int firstParen = versionLine.IndexOf("(");
                                    int lastParen = versionLine.LastIndexOf(")");
                                    // exclude the '(' and the " characters
                                    int start = firstParen + 2;
                                    // exclude the ')' and the " characters
                                    int end = lastParen - 1;
                                    version = versionLine.Substring(start, (end - start));
                                    break;
                                }
                            }
                        }
                    }
                    if (version != null)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to find version for project directory: " + projectDirectory);
                Console.WriteLine("The issue was: " + e.Message);
            }

            return version;
        }
    }
}
