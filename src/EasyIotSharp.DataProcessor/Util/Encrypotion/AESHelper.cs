using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EasyIotSharp.DataProcessor.Util.Encrypotion
{
    /// <summary>
    /// AES加密解密
    /// </summary>
    public class AESHelper
    {
        /// <summary>
        /// 使用AES加密字符串
        /// </summary>
        /// <param name="content">加密内容</param>
        /// <param name="key">秘钥</param>
        /// <returns>Base64字符串结果</returns>
        public static string Encrypt(string content, string key)
        {
            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(content);
            SymmetricAlgorithm des = Aes.Create();
            des.Key = keyArray;
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = des.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray);
        }

        /// <summary>
        /// 使用AES解密字符串
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="key">秘钥</param>
        /// <returns>UTF8解密结果</returns>
        public static string Decrypt(string content, string key)
        {
            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = Convert.FromBase64String(content);

            SymmetricAlgorithm des = Aes.Create();
            des.Key = keyArray;
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = des.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }
    }
}
