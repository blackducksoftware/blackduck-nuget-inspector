using Com.Synopsys.Integration.Nuget.Inspection.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Synopsys.Integration.Nuget.Inspection
{
    interface IInspector
    {
        InspectionResult Inspect();
    }
}
