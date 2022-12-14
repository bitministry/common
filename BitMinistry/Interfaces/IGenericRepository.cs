using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BitMinistry
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> All { get; }

        void DeleteById(object id);

        void Delete(TEntity entity);
        IList<TEntity> Find(Expression<Func<TEntity, bool>> criteria);
        TEntity Get(object id);
        void SaveOrUpdate(TEntity obj);
        void Refresh(TEntity obj);
    }
}