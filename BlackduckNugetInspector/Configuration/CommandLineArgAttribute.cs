using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Synopsys.Integration.Nuget.Configuration
{
    class CommandLineArgAttribute : Attribute
    {
        public string Key;
        public string Description;
        public CommandLineArgAttribute(string key, string description = "")
        {
            Key = key;
            Description = description;
        }
    }
}
