using EasyIotSharp.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UPrime.Dependency.Impl;
using UPrime.Modules;

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
        }
    }
}
