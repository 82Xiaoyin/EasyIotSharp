using EasyIotSharp.GateWay.Core.Socket.Service;
using EasyIotSharp.GateWay.Core.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.GateWay.Core.Socket.Factory
{
    public class EasyTCPFactory
    {
        public static EasyTCPSuper CreateManufacturer(string manufacturer)
        {
            EasyTCPSuper tcpSuper = null;
            try
            {
                switch (manufacturer)
                {
                    case "modbusRTU"://Modbus应答式
                        tcpSuper = new ModbusDTU();
                        break;
                    default:
                        break;
                }
                return tcpSuper;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return tcpSuper;
            }
        }
    }
}
