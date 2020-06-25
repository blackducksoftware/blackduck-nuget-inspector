using System;

namespace Com.Synopsys.Integration.Nuget.Configuration
{
    class AppConfigArgAttribute : Attribute
    {
        public string Key;
        public AppConfigArgAttribute(string key)
        {
            Key = key;
        }
    }
}
