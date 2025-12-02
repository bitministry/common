using System;

namespace BitMinistry.Exceptions
{
    public class DenialOfServiceException : Exception
    {
        public string Ip { get; }
        public bool IsLongBan { get; }

        public DenialOfServiceException(string ip, bool isLongBan, string message)
            : base(message)
        {
            Ip = ip;
            IsLongBan = isLongBan;
        }
    }
}
