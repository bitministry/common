using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace BitMinistry
{
    public class LogItem<TActionEnum> : ILogItem, IEntity where TActionEnum : Enum
    {
        private static string _appDomainName;

        protected static string AppDomainName
            => _appDomainName ?? (_appDomainName = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Name);


        // first item for error (default, for ErrorReport)
        public virtual TActionEnum Action { get; set; }

        [StringLength(1111)]
        public virtual string Message { get; set; }
        [StringLength(2222)]
        public virtual string AdditionalMessage { get; set; }
        public Severity? Severity { get; set; }

        public DateTime? Created { get; set; } = DateTime.Now;

        private string _assemblyName;

        [StringLength(33)]
        public virtual string AssemblyName
        {
            get
            {
                return _assemblyName ?? (_assemblyName = AppDomainName);
            }
            set
            {
                _assemblyName = value;
            }
        }

        [StringLength(55)]
        public virtual string CallingType { get; set; }
        public virtual string Culture { get; set; }
        [StringLength(222)]
        public virtual string Url { get; set; }
        public int? LogId { get; set; }



        public override string ToString()
        {
            return $"{Severity} {Message} - {CallingType} {AssemblyName} {Created}";
        }

    }



}
