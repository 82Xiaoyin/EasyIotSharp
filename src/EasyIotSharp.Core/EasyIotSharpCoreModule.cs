using MongoDB.Bson.Serialization.Conventions;
using System.Reflection;
using UPrime;
using UPrime.AutoMapper;
using UPrime.Dependency;
using UPrime.Elasticsearch;
using UPrime.Modules;
using UPrime.MongoDb;
using UPrime.Runtime.Caching.Redis;
using UPrime.SDK.Weixin.Configuration;
using EasyIotSharp.Core.Caching;
using EasyIotSharp.Core.Configuration;
using EasyIotSharp.Core.Queue;
using EasyIotSharp.Core.Queue.Impl;
using EasyIotSharp.Repositories.Elasticsearch;
using EasyIotSharp.Repositories.Mysql;
using EasyIotSharp.Core.Repositories.Mysql;
using EasyIotSharp.Core.Repositories.Tenant;
using EasyIotSharp.Core.Repositories.Tenant.Impl;
using EasyIotSharp.Core.Repositories.TenantAccount;
using EasyIotSharp.Core.Repositories.TenantAccount.Impl;
using EasyIotSharp.Core.Repositories.Project;
using EasyIotSharp.Core.Repositories.Project.Impl;
using EasyIotSharp.Core.Repositories.Hardware;
using EasyIotSharp.Core.Repositories.Hardware.Impl;
using EasyIotSharp.Core.Repositories.Influxdb;
using EasyIotSharp.Core.Repositories.Queue;
using EasyIotSharp.Core.Repositories.Queue.Impl;
using EasyIotSharp.Core.Services.IO;
using EasyIotSharp.Core.Services.IO.Impl;
using EasyIotSharp.Core.Repositories.Rule;
using EasyIotSharp.Core.Repositories.Rule.Impl;
using EasyIotSharp.Core.Services.Rule;
using EasyIotSharp.Core.Domain.Rule;

namespace EasyIotSharp.Core
{
    [DependsOn(
        typeof(UPrimeLeadershipModule),
        typeof(UPrimeRedisCacheModule),
        typeof(UPrimeAutoMapperModule)
       )]
    public class EasyIotSharpCoreModule : UPrimeModule
    {
        public override void PreInitialize()
        {
            var options = IocManager.Resolve<AppOptions>();
            IocManager.Register<IElasticsearchDatabaseProvider, EasyIotSharpElasticsearchDatabaseProvider>();

            IocManager.Register<ISqlSugarDatabaseProvider, SqlSugarDatabaseProvider>();

            IocManager.Register<IInfluxdbDatabaseProvider, InfluxdbDatabaseProvider>();

            IocManager.Register<IMinIOFileService, MinIOFileService>();


            var camelCaseConventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConventionPack, type => true);

            if (options.CachingOptions.RedisTwemProxyEnabled)
            {
                IocManager.Register<IUPrimeRedisCacheDatabaseProvider, CurrentRedisCacheDatabaseProvider_TwemProxy>(DependencyLifeStyle.Singleton);
            }
            else
            {
                IocManager.Register<IUPrimeRedisCacheDatabaseProvider, CurrentRedisCacheDatabaseProvider>(DependencyLifeStyle.Singleton);
            }

            IocManager.Register<IKafkaService, KafkaService>();

            IocManager.Register<ICorpWeixinSettings, CorpWeixinOptions>();

            //租户
            IocManager.Register<ITenantRepository, TenantRepository>();
            //系统管理
            IocManager.Register<IMenuRepository, MenuRepository>();
            IocManager.Register<IRoleMenuRepository, RoleMenuRepository>();
            IocManager.Register<IRoleRepository, RoleRepository>();
            IocManager.Register<ISoldierRepository, SoldierRepository>();
            IocManager.Register<ISoldierRoleRepository, SoldierRoleRepository>();
            //项目
            IocManager.Register<IProjectBaseRepository, ProjectBaseRepository>();
            IocManager.Register<IClassificationRepository, ClassificationRepository>();
            IocManager.Register<IGatewayProtocolRepository, GatewayProtocolRepository>();
            IocManager.Register<IGatewayProtocolConfigRepository, GatewayProtocolConfigRepository>();
            IocManager.Register<IGatewayRepository, GatewayRepository>();
            IocManager.Register<ISensorPointRepository, SensorPointRepository>();
            //协议测点
            IocManager.Register<IProtocolRepository, ProtocolRepository>();
            IocManager.Register<IProtocolConfigRepository, ProtocolConfigRepository>();
            IocManager.Register<IProtocolConfigExtRepository, ProtocolConfigExtRepository>();
            IocManager.Register<ISensorRepository, SensorRepository>();
            IocManager.Register<ISensorQuotaRepository, SensorQuotaRepository>();
            // 在现有的依赖注入注册代码中添加以下内容
            IocManager.Register<IRabbitServerInfoRepository, RabbitServerInfoRepository>();

            //配置信息
            IocManager.Register<IAlarmsConfigRepository, AlarmsConfigRepository>();
            IocManager.Register<INotifyRepository, NotifyRepository>();
            IocManager.Register<IRuleChainRepository, RuleChainRepository>();
            IocManager.Register<ISceneManagementRepository, SceneManagementRepository>();
            
        }

        /// <summary>
        /// </summary>
        public override void Initialize()
        {
            IocManager.RegisterAssembly(Assembly.GetExecutingAssembly());
        }
    }
}