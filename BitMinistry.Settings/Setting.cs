using System;

namespace BitMinistry.Settings
{
    public class Setting : IEntity
    {
        [BEntityId]
        public virtual string Name { get; set; }
        public virtual string NTextValue { get; set; }
        public virtual decimal? NumericValue { get; set; }
        public virtual DateTime? DateTimeValue { get; set; }

        public virtual string Comment { get; set; }

        public override string ToString() => $"{Name} {NTextValue} {NumericValue} {DateTimeValue} {Comment}";

    }
}
