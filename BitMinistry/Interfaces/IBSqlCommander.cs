using BitMinistry.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BitMinistry
{
    public interface IBSqlCommander : IBSqlRawCommander
    {
        int Delete<T>(object idValue, Expression<Func<T, object>> idCol = null);
        int Delete<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : IEntity;
        object GetValueWhere<TSqlQueryable>(Expression<Func<TSqlQueryable, object>> prop, Expression<Func<TSqlQueryable, bool>> where) where TSqlQueryable : ISqlQueryable;
        T LoadById<T>(Expression<Func<T, object>> idCol, object idValue, string[] excludeProps = null) where T : ISqlQueryable;
        IEnumerable<TSqlQueryable> QueryForEntity<TSqlQueryable>(bool reset, string sqlWhereAndOrderBy = null, int? top = null) where TSqlQueryable : ISqlQueryable;
        IEnumerable<TSqlQueryable> QueryForEntity<TSqlQueryable>(Expression<Func<TSqlQueryable, bool>> where, bool reset, int? top = null) where TSqlQueryable : ISqlQueryable;
        IEnumerable<TSqlQueryable> QueryForSql<TSqlQueryable>(Expression<Func<TSqlQueryable, bool>> where, bool reset, int? top = null) where TSqlQueryable : ISqlQueryable;
        TSqlQueryable[] QueryForSql<TSqlQueryable>(string query, bool reset) where TSqlQueryable : ISqlQueryable;
        void SetValueFromLocation<TSqlQueryable>(TSqlQueryable obj, Expression<Func<TSqlQueryable, object>> prop, Expression<Func<TSqlQueryable, bool>> where) where TSqlQueryable : ISqlQueryable;
    }
}