using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Synopsys.Integration.Nuget.DependencyResolution.Nuget
{
    public class NugetFramework
    {
        public string Identifier;
        public int Major;
        public int Minor;

        public NugetFramework(string id, int major, int minor)
        {
            Identifier = id;
            Major = major;
            Minor = minor;
        }
    }
}
