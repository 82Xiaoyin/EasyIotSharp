using System.Security.Principal;
using EasyIotSharp.Core.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using EasyIotSharp.Core.Dto.Users;

namespace EasyIotSharp.Core.Extensions
{
    public static class TokenExtensions
    {
        /// <summary>
        /// 获取Token用户身份对象
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static UserTokenData GetUserTokenIdentifier(this IIdentity identity)
        {
            return (identity as UserTokenIdentity)?.GetIdentifier();
        }

        /// <summary>
        /// 生成 JWT Token
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="userName">用户名称</param>
        /// <param name="tenantId">租户 ID</param>
        /// <param name="expireMinutes">过期时间（分钟）默认七天</param>
        /// <returns>生成的 Token</returns>
        public static UserTokenDto GenerateToken(string userId, string userName, string tenantId,string tenantNumId, string abbreviation, int expireMinutes = 10080)
        {
            // 1. 定义 Payload
            var claims = new[]
            {
            new Claim("UserId", userId), // 用户 ID
            new Claim("UserName", userName), // 用户名称
            new Claim("TenantId", tenantId), // 租户 ID
            new Claim("TenantNumId", tenantNumId), // 租户 NumID
            new Claim("Abbreviation", abbreviation), // 租户数据库简称
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Token ID
        };

            // 2. 获取 RSA 私钥
            var rsa = GetCachedPrivateKey();

            // 3. 定义签名凭证
            var credentials = new SigningCredentials(
                new RsaSecurityKey(rsa),
                SecurityAlgorithms.RsaSsaPssSha256 // 使用 RS256 算法
            );

            // 4. 定义 Token 描述
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes), // 过期时间
                SigningCredentials = credentials
            };

            // 5. 生成 Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // 6. 返回生成的 Token
            return new UserTokenDto() { AccessToken = tokenHandler.WriteToken(token), ExpirationTime = DateTime.UtcNow.AddMinutes(expireMinutes) };
        }
        public static RSA LoadPrivateKeyFromPem(string pem)
        {
            // 去掉 PEM 文件的头和尾
            var base64 = pem
                .Replace("-----BEGIN PRIVATE KEY-----", "")
                .Replace("-----END PRIVATE KEY-----", "")
                .Replace("\n", "")
                .Replace("\r", "");

            // 解码 Base64
            var privateKeyBytes = Convert.FromBase64String(base64);

            // 导入私钥
            var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
            return rsa;
        }

        private static readonly Lazy<RSA> _rsaPrivateKey = new Lazy<RSA>(() =>
        {
            return LoadPrivateKeyFromPem(JWTTokenOptions.PrivateKey);
        });

        public static RSA GetCachedPrivateKey()
        {
            return _rsaPrivateKey.Value;
        }
    }
}