using System;

namespace BitMinistry.Common.Settings
{
    public class Setting : IEntity
    {
        [BEntityId]
        public virtual string Name { get; set; }
        public virtual string NTextValue { get; set; }
        public virtual decimal? NumericValue { get; set; }
        public virtual DateTime? DateTimeValue { get; set; }

    }
}
