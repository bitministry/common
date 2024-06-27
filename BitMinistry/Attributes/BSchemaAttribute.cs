using System;

namespace BitMinistry
{
    public class BSchemaAttribute : Attribute
    {
        public BSchemaAttribute(string sc) {
            Schema = sc;
        }
        public string Schema { get; set; }
    }
}
