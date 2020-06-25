using System.Collections.Generic;

namespace Com.Synopsys.Integration.Nuget.Model
{
    public class PackageSet
    {
        public PackageId PackageId;
        public HashSet<PackageId> Dependencies = new HashSet<PackageId>();
    }
}
