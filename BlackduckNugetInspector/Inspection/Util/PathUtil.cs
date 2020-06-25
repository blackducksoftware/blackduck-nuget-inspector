using System;
using System.Collections.Generic;
using System.IO;

namespace Com.Synopsys.Integration.Nuget.Inspection.Util
{
    static class PathUtil
    {
        public static string Combine(List<string> pathSegments)
        {
            return Combine(pathSegments.ToArray());
        }

        public static string Combine(params string[] pathSegments)
        {
            String path = Path.Combine(pathSegments);
            return path.Replace("\\", "/");
        }

        public static string Sanitize(String path)
        {
            return path.Replace("\\", "/");
        }
    }
}
