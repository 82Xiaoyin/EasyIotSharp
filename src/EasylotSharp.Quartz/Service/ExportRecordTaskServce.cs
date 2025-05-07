using System;
using System.Collections.Generic;
using System.Text;
using Quartz;
using System.Threading.Tasks;
using UPrime;
using UPrime.Events.Bus;
using Microsoft.Extensions.Logging;
using EasyIotSharp.Core.Services.Project;

namespace EasylotSharp.Quartz.Service
{
    [DisallowConcurrentExecution] // 防止并发执行
    public class ExportRecordTaskServce : IJob
    {
        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Console.WriteLine($"当前时间：{DateTime.Now}，执行 Cron 任务逻辑");
                var _gatewayRepository = UPrimeEngine.Instance.Resolve<IGatewayService>();
                // 执行实际的业务逻辑

                var data = await _gatewayRepository.GetGateway("a");
                Console.WriteLine(data);
                // 处理数据...

            }
            catch (Exception ex)
            {
                throw; // 重新抛出异常，让 Quartz 知道任务失败
            }
        }
        //public async Task ProcessingExportRecord()
        //{
            
        //}
    }
}
