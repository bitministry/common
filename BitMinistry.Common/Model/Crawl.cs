using System;

namespace BitMinistry.Common.Model
{
    public class Crawl : IEntity
    {
        public string url { get; set; }
        public DateTime added { get; set; }
        public DateTime? crawled { get; set; }
        public string html { get; set; }
        public DateTime? processed { get; set; }
        public short retry { get; set; }

    }
}
