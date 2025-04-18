using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.DataProcessor.Util.ModbusUtil
{
    public class ModbusCRC
    {
        public static byte[] ModbusCRC16(byte[] pDataBytes, int startIndex, int length)
        {
            ushort crc = 0xffff;
            ushort polynom = 0xA001;
            byte[] bResult;
            try
            {
                for (int i = startIndex; i < length; i++)
                {
                    crc ^= pDataBytes[i];
                    for (int j = 0; j < 8; j++)
                    {
                        if ((crc & 0x01) == 0x01)
                        {
                            crc >>= 1;
                            crc ^= polynom;
                        }
                        else
                        {
                            crc >>= 1;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return bResult = new byte[2];
            }
            return bResult = BitConverter.GetBytes(crc);
        }
    }
}
