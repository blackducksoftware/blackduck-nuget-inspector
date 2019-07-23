using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Synopsys.Integration.Nuget.Model
{
    public class InspectionOutput
    {
        public string Name = "Nuget Inspector Inspection Result";
        public string Version = "1.0.0";
        public List<Container> Containers = new List<Container>();

    }
}
