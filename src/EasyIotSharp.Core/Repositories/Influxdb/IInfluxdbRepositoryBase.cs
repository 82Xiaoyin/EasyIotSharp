using InfluxData.Net.Common.Enums;
using InfluxData.Net.InfluxDb;
using InfluxData.Net.InfluxDb.Models;
using InfluxData.Net.InfluxDb.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UPrime.Domain.Entities;

namespace EasyIotSharp.Core.Repositories.Influxdb
{
    public interface IInfluxdbRepositoryBase<TEntity>
    {
        #region 查询操作

        /// <summary>
        /// 获取所有数据
        /// </summary>
        Task<IQueryable<Serie>> GetAll();

        /// <summary>
        /// 通过Sql执行
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        Task<Serie> GetAsync(string sql);

        /// <summary>
        /// 根据ID异步获取实体
        /// </summary>
        Task<Serie> GetAsync(Serie id);

        /// <summary>
        /// 根据条件查询
        /// </summary>
        Task<List<Serie>> QueryAsync(string whereClause);

        #endregion

        #region 写入操作

        /// <summary>
        /// 异步插入实体
        /// </summary>
        Task InsertAsync(TEntity entity);

        /// <summary>
        /// 批量插入
        /// </summary>
        Task BulkInsertAsync(IEnumerable<TEntity> entities);

        #endregion
    }
}
