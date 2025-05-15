using EasyIotSharp.Core;
using EasyIotSharp.GateWay.Core.Interfaces;
using EasyIotSharp.GateWay.Core.Socket.Service;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UPrime.Dependency;
using UPrime.Dependency.Impl;
using UPrime.Modules;
using MqttService = EasyIotSharp.GateWay.Core.Socket.Service.MqttService;
using IMqttService = EasyIotSharp.GateWay.Core.Interfaces.IMqttService;
using EasyIotSharp.GateWay.Core.Socket.Interfaces;
using EasyIotSharp.GateWay.Core.Services;

namespace EasyIotSharp.GateWay.Core
{
    [DependsOn(
   typeof(EasyIotSharpCoreModule)
   )]
    public class GateWayCoreModule : UPrimeModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssembly(Assembly.GetExecutingAssembly());
            
            // 注册 IDataRepository
            IocManager.Register<IDataRepository, InfluxDataRepository>(DependencyLifeStyle.Singleton);
            
            // 注册 MQTT 服务
            IocManager.Register<IMqttService, MqttService>(DependencyLifeStyle.Singleton);
        }
    }
}
