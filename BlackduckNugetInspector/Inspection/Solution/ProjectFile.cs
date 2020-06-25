using System.Linq;

namespace Com.Synopsys.Integration.Nuget.Inspection.Solution
{
    public class ProjectFile
    {
        public string TypeGUID;
        public string Name;
        public string GUID;
        public string Path;

        public static ProjectFile Parse(string projectLine)
        {
            //projectLine format: Project(type) = name, file, guid
            //projectLine example: Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "NUnitFramework", "NUnitFramework", "{5D8A9D62-C11C-45B2-8965-43DE8160B558}"

            var equalSplit = projectLine.Split('=').Select(s => s.Trim()).ToList();
            if (equalSplit.Count() < 2) return null;

            var file = new ProjectFile();
            string leftSide = equalSplit[0];
            string rightSide = equalSplit[1];
            if (leftSide.StartsWith("Project(\"") && leftSide.EndsWith("\")"))
            {
                file.TypeGUID = MiddleOfString(leftSide, "Project(\"".Length, "\")".Length);
            }
            var opts = rightSide.Split(',').Select(s => s.Trim()).ToList();
            if (opts.Count() >= 1) file.Name = MiddleOfString(opts[0], 1, 1); //strip quotes
            if (opts.Count() >= 2) file.Path = MiddleOfString(opts[1], 1, 1); //strip quotes
            if (opts.Count() >= 3) file.GUID = MiddleOfString(opts[2], 1, 1); //strip quotes

            return file;
        }

        private static string MiddleOfString(string source, int fromLeft, int fromRight)
        {
            var left = source.Substring(fromLeft);
            return left.Substring(0, left.Length - fromRight);
        }

    }
}
