using System;

namespace Com.Synopsys.Integration.Nuget.Inspection.Exceptions
{
    [Serializable]
    class BlackDuckInspectorException : Exception
    {
        public BlackDuckInspectorException() : base()
        {
        }

        public BlackDuckInspectorException(string message) : base(message)
        {
        }

        public BlackDuckInspectorException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}