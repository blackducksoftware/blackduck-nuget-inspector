using Com.Synopsys.Integration.Nuget.Model;
using System;
using System.Collections.Generic;

namespace Com.Synopsys.Integration.Nuget.Inspection.Model
{
    class InspectionResult
    {
        public enum ResultStatus
        {
            Success,
            Error
        }

        public string ResultName;
        public string OutputDirectory;
        public ResultStatus Status;
        public List<Container> Containers = new List<Container>();
        public Exception Exception;

    }
}
