using InfluxData.Net.Common.Enums;
using InfluxData.Net.InfluxDb;
using InfluxData.Net.InfluxDb.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using UPrime.Domain.Entities;
using InfluxData.Net.InfluxDb.Models.Responses;
using UPrime.Domain.Entities.Auditing;
using UPrime.Domain.Repositories;

namespace EasyIotSharp.Core.Repositories.Influxdb
{
    public class InfluxdbRepositoryBase<TEntity> : IInfluxdbRepositoryBase<TEntity>
    {
        private readonly IInfluxdbDatabaseProvider _databaseProvider;

        /// <summary>
        /// InfluxDB 客户端
        /// </summary>
        public virtual IInfluxDbClient Client => _databaseProvider.Client;

        /// <summary>
        /// 测量名称(相当于表名)
        /// </summary>
        public readonly string _measurementName;

        /// <summary>
        /// 租户数据库名称
        /// </summary>
        public readonly string _tenantDatabase;

        /// <summary>
        /// 默认数据库名称
        /// </summary>
        public string DefaultDatabase => "default";

        public InfluxdbRepositoryBase(IInfluxdbDatabaseProvider databaseProvider,string measurementName,string tenantDatabase)
        {
            _databaseProvider = databaseProvider;
            _measurementName = measurementName;
            _tenantDatabase = tenantDatabase;
            InitializeDatabase(_databaseProvider).Wait();
        }

        public async Task InitializeDatabase(IInfluxdbDatabaseProvider provider)
        {
            var requiredDatabases = new[] { DefaultDatabase, _tenantDatabase };
            var dataBases = await provider.Client.Database.GetDatabasesAsync();
            foreach (var dbName in requiredDatabases)
            {
                var exists = dataBases.FirstOrDefault(s => s.Name == dbName);
                if (exists.IsNull())
                {
                    await provider.Client.Database.CreateDatabaseAsync(dbName);
                }
            }
        }

        #region 查询操作

        /// <summary>
        /// 获取所有数据
        /// </summary>
        public async Task<IQueryable<Serie>> GetAll()
        {
            var query = $"SELECT * FROM {_measurementName}";
            var result = await _databaseProvider.Client.Client.QueryAsync(query, _tenantDatabase);
            return result.AsQueryable();
        }

        /// <summary>
        /// 根据ID异步获取实体
        /// </summary>
        public async Task<Serie> GetAsync(string sql)
        {
            var result = await _databaseProvider.Client.Client.QueryAsync(sql, _tenantDatabase);
            var entity = result.FirstOrDefault();

            return entity;
        }

        /// <summary>
        /// 根据ID异步获取实体
        /// </summary>
        public async Task<Serie> GetAsync(Serie id)
        {
            var query = $"SELECT * FROM {_measurementName} WHERE id='{id}'";
            var result = await _databaseProvider.Client.Client.QueryAsync(query, _tenantDatabase);
            var entity = result.FirstOrDefault();

            return entity;
        }

        /// <summary>
        /// 根据条件查询
        /// </summary>
        public async Task<List<Serie>> QueryAsync(string whereClause)
        {
            var query = $"SELECT * FROM {_measurementName}";
            if (!string.IsNullOrWhiteSpace(whereClause))
            {
                query += $" WHERE {whereClause}";
            }

            var result = await _databaseProvider.Client.Client.QueryAsync(query, _tenantDatabase);
            return result.ToList();
        }

        #endregion

        #region 写入操作

        /// <summary>
        /// 异步插入实体
        /// </summary>
        public async Task InsertAsync(TEntity entity)
        {
            SetCreationAuditProperties(entity);
            CheckAndSetDefaultValue(entity);

            var point = ConvertEntityToPoint(entity);
            await _databaseProvider.Client.Client.WriteAsync(point, _tenantDatabase);
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        public async Task BulkInsertAsync(IEnumerable<TEntity> entities)
        {
            var points = entities.Select(entity =>
            {
                SetCreationAuditProperties(entity);
                CheckAndSetDefaultValue(entity);
                return ConvertEntityToPoint(entity);
            });

            await _databaseProvider.Client.Client.WriteAsync(points, _tenantDatabase);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 将实体转换为Point
        /// </summary>
        protected virtual Point ConvertEntityToPoint(TEntity entity)
        {
            var point = new Point
            {
                Name = _measurementName,
                Timestamp = GetEntityTimestamp(entity),
                Tags = new Dictionary<string, object>(),
                Fields = new Dictionary<string, object>()
            };

            var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(entity);
                if (value == null) continue;

                // ID作为Tag
                if (prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    point.Tags["id"] = value.ToString();
                    continue;
                }

                // 审计字段处理
                if (prop.Name.Equals("CreationTime", StringComparison.OrdinalIgnoreCase))
                    continue;

                // 根据类型决定存储方式
                if (ShouldStoreAsTag(prop))
                {
                    point.Tags[prop.Name] = value.ToString();
                }
                else
                {
                    point.Fields[prop.Name] = value;
                }
            }

            return point;
        }

        /// <summary>
        /// 判断属性是否应存储为Tag
        /// </summary>
        protected virtual bool ShouldStoreAsTag(PropertyInfo property)
        {
            var type = property.PropertyType;
            return type == typeof(string) || type.IsEnum || type == typeof(bool);
        }

        /// <summary>
        /// 获取实体时间戳
        /// </summary>
        protected virtual DateTime GetEntityTimestamp(TEntity entity)
        {
            if (entity is IHasCreationTime hasCreationTime)
                return hasCreationTime.CreationTime;

            return DateTime.UtcNow;
        }

        #endregion

        #region 继承自基类的方法

        private void SetCreationAuditProperties(object entityAsObj)
        {
            if (entityAsObj is IHasCreationTime hasCreationTime)
            {
                if (hasCreationTime.CreationTime == default(DateTime))
                {
                    hasCreationTime.CreationTime = DateTime.UtcNow;
                }
            }
        }

        private void CheckAndSetDefaultValue(object entityAsObj)
        {
            var properties = entityAsObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(string) && prop.GetValue(entityAsObj) == null)
                {
                    prop.SetValue(entityAsObj, string.Empty);
                }
            }
        }

        #endregion
    }
}