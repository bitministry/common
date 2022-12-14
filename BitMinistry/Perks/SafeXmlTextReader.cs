using System.IO;
using System.Xml;

namespace BitMinistry
{
    public class SafeXmlTextReader : XmlTextReader
    {
        public SafeXmlTextReader(StreamReader sr) : base(sr) { }

        public override void ResolveEntity()
        {
    
        }
  
    }

}
