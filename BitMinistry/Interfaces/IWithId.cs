using System;

namespace BitMinistry
{

    public interface IWithId : IEntity
    {
        object Id { get; }

    }


    public interface IWithIntId : IEntity
    {
        int Id { get; }

    }

    public interface IWithStringId : IEntity
    {
        string Id { get; }
    }


}
