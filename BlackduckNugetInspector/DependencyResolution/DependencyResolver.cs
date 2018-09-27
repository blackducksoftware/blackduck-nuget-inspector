using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Synopsys.Integration.Nuget.DependencyResolution
{
    interface DependencyResolver
    {
        DependencyResult Process();
    }
}
