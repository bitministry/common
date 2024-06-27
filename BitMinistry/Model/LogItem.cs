using BitMinistry.Web;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace BitMinistry.Logger
{
    public class LogItem : ILogItem, IEntity 
    {
        private static string _appDomainName;

        protected static string AppDomainName
            => _appDomainName ?? (_appDomainName = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Name);

        public int? IpInt { get; set; }
        public IpLocation IpLocation { get; set; }

        [StringLength(111)]
        public string Action { get; set; }

        [StringLength(1111)]
        public virtual string Message { get; set; }
        [StringLength(3333)]
        public virtual string AdditionalMessage { get; set; }
        public Severity? Severity { get; set; }

        public DateTime? Created { get; set; } = DateTime.Now;

        private string _assemblyName;
        public virtual string AssemblyName {
            get
            {
                return _assemblyName ?? (_assemblyName = AppDomainName);
            }
            set
            {
                _assemblyName = value;
            }
        }
        public string CallingType { get; set; }
        public string Culture { get; set; }
        [StringLength(222)]
        public string Url { get; set; }
        public int? LogId { get; set; }

//        public object UserId { get; set; }

        public override string ToString()
        {
            return $"{Severity} {Action} {Message} - {CallingType} {AssemblyName} {Created}";
        }

    }


    public interface ILogItem
    {
        string Action { get; set; }
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
