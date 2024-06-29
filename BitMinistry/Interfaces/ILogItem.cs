using System;
using System.IO;

namespace BitMinistry
{
    public interface ILogItem
    {
        // string Action { get; set; }
        string Message { get; set; }
        string AdditionalMessage { get; set; }
        Severity? Severity { get; set; }

        DateTime? Created { get; set; }

        string AssemblyName { get; set; }
        string CallingType { get; set; }
        string Culture { get; set; }
    }

    public enum Severity
    {
        Debug,
        Info,
        Warn,
        Error
    }

}
