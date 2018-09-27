using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Synopsys.Integration.Nuget.Model
{
    public class Container
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Type { get; set; } = "Solution";
        public string SourcePath { get; set; }
        public List<string> OutputPaths { get; set; } = new List<string>();
        public List<PackageSet> Packages { get; set; } = new List<PackageSet>();
        public List<PackageId> Dependencies { get; set; } = new List<PackageId>();
        public List<Container> Children { get; set; } = new List<Container>();
    }
}
