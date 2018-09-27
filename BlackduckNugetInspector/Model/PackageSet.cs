using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Synopsys.Integration.Nuget.Model
{
    public class PackageSet
    {
        public PackageId PackageId;
        public HashSet<PackageId> Dependencies;
    }
}
