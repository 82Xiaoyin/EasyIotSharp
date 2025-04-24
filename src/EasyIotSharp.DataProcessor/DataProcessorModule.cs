using EasyIotSharp.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UPrime.Dependency.Impl;
using UPrime.Modules;

namespace EasyIotSharp.DataProcessor
{
    [DependsOn(
   typeof(EasyIotSharpCoreModule)
   )]
    public class DataProcessorModule : UPrimeModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
