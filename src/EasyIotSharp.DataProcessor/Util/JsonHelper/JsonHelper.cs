using System;
using System.Text.Json;

namespace EasyIotSharp.DataProcessor.Util
{
    public static class JsonHelper
    {
        /// <summary>
        /// 尝试从JsonElement中解析指定属性的值
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="element">JsonElement对象</param>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">解析后的值</param>
        /// <returns>是否解析成功</returns>
        public static bool TryParseJsonValue<T>(JsonElement element, string propertyName, out T value)
        {
            value = default;
            
            if (!element.TryGetProperty(propertyName, out JsonElement property))
                return false;
                
            try
            {
                if (typeof(T) == typeof(byte))
                {
                    if (byte.TryParse(property.GetString(), out byte byteValue))
                    {
                        value = (T)(object)byteValue;
                        return true;
                    }
                }
                else if (typeof(T) == typeof(ushort))
                {
                    if (ushort.TryParse(property.GetString(), out ushort ushortValue))
                    {
                        value = (T)(object)ushortValue;
                        return true;
                    }
                }
                else if (typeof(T) == typeof(double))
                {
                    if (double.TryParse(property.GetString(), out double doubleValue))
                    {
                        value = (T)(object)doubleValue;
                        return true;
                    }
                }
                else if (typeof(T) == typeof(int))
                {
                    if (int.TryParse(property.GetString(), out int intValue))
                    {
                        value = (T)(object)intValue;
                        return true;
                    }
                }
                else if (typeof(T) == typeof(string))
                {
                    value = (T)(object)property.GetString();
                    return true;
                }
            }
            catch
            {
                return false;
            }
            
            return false;
        }
    }
}