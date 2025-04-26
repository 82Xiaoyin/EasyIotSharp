using EasyIotSharp.Core;
using EasyIotSharp.Core.Caching;
using EasyIotSharp.Core.Configuration;
using EasyIotSharp.Core.Queue.Impl;
using EasyIotSharp.Core.Queue;
using EasyIotSharp.Core.Repositories.Hardware.Impl;
using EasyIotSharp.Core.Repositories.Hardware;
using EasyIotSharp.Core.Repositories.Influxdb;
using EasyIotSharp.Core.Repositories.Mysql;
using EasyIotSharp.Core.Repositories.Project.Impl;
using EasyIotSharp.Core.Repositories.Project;
using EasyIotSharp.Core.Repositories.Queue.Impl;
using EasyIotSharp.Core.Repositories.Queue;
using EasyIotSharp.Core.Repositories.Rule.Impl;
using EasyIotSharp.Core.Repositories.Rule;
using EasyIotSharp.Core.Repositories.Tenant.Impl;
using EasyIotSharp.Core.Repositories.Tenant;
using EasyIotSharp.Core.Repositories.TenantAccount.Impl;
using EasyIotSharp.Core.Repositories.TenantAccount;
using EasyIotSharp.Core.Services.IO.Impl;
using EasyIotSharp.Core.Services.IO;
using EasyIotSharp.Repositories.Elasticsearch;
using EasyIotSharp.Repositories.Mysql;
using MongoDB.Bson.Serialization.Conventions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UPrime.Dependency;
using UPrime.Dependency.Impl;
using UPrime.Elasticsearch;
using UPrime.Modules;
using UPrime.Runtime.Caching.Redis;
using UPrime.SDK.Weixin.Configuration;

namespace EasyIotSharp.DataProcessor
{
    [DependsOn(
   typeof(EasyIotSharpCoreModule)
   )]
    public class DataProcessorModule : UPrimeModule
    {
        public override void PreInitialize()
        {

        }
        public override void Initialize()
        {
            IocManager.RegisterAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
