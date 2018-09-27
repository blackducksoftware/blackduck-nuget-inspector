using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Synopsys.Integration.Nuget.DependencyResolution
{
    class DependencyResult
    {
        public bool Success { get; set; } = true;
        public string ProjectVersion { get; set; } = null;
        public List<Model.PackageSet> Packages { get; set; } = new List<Model.PackageSet>();
        public List<Model.PackageId> Dependencies { get; set; } = new List<Model.PackageId>();
    }
}
