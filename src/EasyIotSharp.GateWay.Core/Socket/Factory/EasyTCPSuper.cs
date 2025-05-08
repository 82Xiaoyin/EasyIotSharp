using EasyIotSharp.GateWay.Core.Model.SocketDTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyIotSharp.GateWay.Core.Socket.Factory
{
    public abstract class EasyTCPSuper
    {
        public abstract Task DecodeData(TaskInfo taskData);
    }
}
