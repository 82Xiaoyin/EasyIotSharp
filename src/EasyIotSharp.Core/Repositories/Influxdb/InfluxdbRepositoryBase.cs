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
using System.Collections;

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

            // 如果实体是字典类型，直接处理字典的值
            if (entity is IDictionary<string, object> dictionary)
            {
                foreach (var kvp in dictionary)
                {
                    var key = kvp.Key.ToLower();
                    var value = kvp.Value;

                    // 跳过空值
                    if (value == null) continue;

                    // 处理特殊字段
                    if (key == "time" && value is DateTime dateTime)
                    {
                        point.Timestamp = dateTime;
                        continue;
                    }

                    // 处理标签字段
                    if (key == "projectid" || key == "pointtype" || key == "pointid")
                    {
                        point.Tags[key] = value.ToString();
                        continue;
                    }

                    // 处理普通字段，保持原始类型
                    switch (value)
                    {
                        case int intValue:
                            point.Fields[key] = intValue;
                            break;
                        case double doubleValue:
                            point.Fields[key] = doubleValue;
                            break;
                        case float floatValue:
                            point.Fields[key] = floatValue;
                            break;
                        case bool boolValue:
                            point.Fields[key] = boolValue;
                            break;
                        default:
                            point.Fields[key] = value.ToString();
                            break;
                    }
                }
            }
            else
            {
                // 如果不是字典类型，则按属性处理
                var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in properties)
                {
                    try
                    {
                        if (prop.GetIndexParameters().Length > 0) continue;

                        var value = prop.GetValue(entity);
                        if (value == null) continue;

                        var propertyName = prop.Name.ToLower();

                        if (propertyName == "time" && value is DateTime dateTime)
                        {
                            point.Timestamp = dateTime;
                            continue;
                        }

                        if (propertyName == "projectid" || propertyName == "pointtype" || propertyName == "pointid")
                        {
                            point.Tags[propertyName] = value.ToString();
                            continue;
                        }

                        // 处理普通字段值
                        switch (value)
                        {
                            case int intValue:
                                point.Fields[propertyName] = intValue;
                                break;
                            case double doubleValue:
                                point.Fields[propertyName] = doubleValue;
                                break;
                            case float floatValue:
                                point.Fields[propertyName] = floatValue;
                                break;
                            case bool boolValue:
                                point.Fields[propertyName] = boolValue;
                                break;
                            default:
                                point.Fields[propertyName] = value.ToString();
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"处理属性 {prop.Name} 时出错: {ex.Message}");
                    }
                }
            }

            return point;
        }

        /// <summary>
        /// 将值转换为InfluxDB支持的类型
        /// </summary>
        protected virtual object ConvertToInfluxValue(object value, Type propertyType)
        {
            if (value == null) return null;

            // 处理基本类型
            if (propertyType.IsPrimitive || propertyType == typeof(decimal))
                return value;

            // 处理日期时间
            if (propertyType == typeof(DateTime))
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.fff");

            // 处理字符串
            if (propertyType == typeof(string))
                return value;

            // 处理枚举
            if (propertyType.IsEnum)
                return value.ToString();

            // 处理布尔值
            if (propertyType == typeof(bool))
                return value;

            // 处理字典类型
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var dictionary = value as IDictionary;
                if (dictionary != null)
                {
                    // 将字典转换为简单的键值对字符串
                    var pairs = new List<string>();
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        pairs.Add($"{entry.Key}:{entry.Value}");
                    }
                    return string.Join(",", pairs);
                }
            }

            // 处理集合类型
            if (propertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(propertyType))
            {
                var collection = value as IEnumerable;
                if (collection != null)
                {
                    var items = new List<string>();
                    foreach (var item in collection)
                    {
                        items.Add(item?.ToString() ?? "null");
                    }
                    return string.Join(",", items);
                }
            }

            // 对于其他复杂类型，尝试序列化为JSON
            try
            {
                return System.Text.Json.JsonSerializer.Serialize(value);
            }
            catch
            {
                // 如果序列化失败，返回简单的字符串表示
                return value.ToString();
            }
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