using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UPrime.Domain.Entities;

namespace EasyIotSharp.Core.Repositories.Mysql
{
    public abstract class MySqlRepositoryBase<TEntity, TPrimaryKey> : IMySqlRepositoryBase<TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>, new()
    {
        public ISqlSugarClient Client { get; }

        public MySqlRepositoryBase(ISqlSugarDatabaseProvider databaseProvider)
        {
            Client = databaseProvider.Client;
        }

        public virtual async Task<int> InsertAsync(TEntity entity)
        {
            return await Client.Insertable(entity).ExecuteCommandAsync();
        }

        public virtual async Task<int> InserManyAsync(List<TEntity> entities)
        {
            return await Client.Insertable(entities).ExecuteCommandAsync();
        }

        public virtual async Task<bool> DeleteByIdAsync(object id)
        {
            return await Client.Deleteable<TEntity>().In(id).ExecuteCommandAsync() > 0;
        }

        public virtual async Task<bool> DeleteAsync(TEntity entity)
        {
            return await Client.Deleteable(entity).ExecuteCommandAsync() > 0;
        }

        public virtual async Task<bool> UpdateAsync(TEntity entity)
        {
            var result = await Client.Updateable(entity).ExecuteCommandAsync();
            return result > 0;
        }
        public virtual bool Update(TEntity entity)
        {
            var result =  Client.Updateable(entity).ExecuteCommand();
            return result > 0;
        }
        public virtual async Task<TEntity> GetByIdAsync(object id)
        {
            return await Client.Queryable<TEntity>().InSingleAsync(id);
        }

        public virtual async Task<List<TEntity>> GetAllAsync()
        {
            return await Client.Queryable<TEntity>().ToListAsync();
        }

        public virtual List<TEntity> GetAll()
        {
            return Client.Queryable<TEntity>().ToList();
        }

        public virtual async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Client.Queryable<TEntity>().FirstAsync(predicate);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Client.Queryable<TEntity>().AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync()
        {
            return await Client.Queryable<TEntity>().CountAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Client.Queryable<TEntity>().CountAsync(predicate);
        }

        public virtual async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Client.Queryable<TEntity>().Where(predicate).ToListAsync();
        }

        public virtual async Task<List<TEntity>> GetPagedListAsync(Expression<Func<TEntity, bool>> predicate, int pageIndex, int pageSize)
        {
            return await Client.Queryable<TEntity>()
                .Where(predicate)
                .ToPageListAsync(pageIndex, pageSize);
        }

        public void Dispose()
        {
            // 不再直接调用 Client.Dispose，因为每个方法都使用独立的 Client 实例
        }
    }
}