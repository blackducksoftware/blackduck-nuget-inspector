using Com.Synopsys.Integration.Nuget.Inspection.Model;

namespace Com.Synopsys.Integration.Nuget.Inspection
{
    interface IInspector
    {
        InspectionResult Inspect();
    }
}
