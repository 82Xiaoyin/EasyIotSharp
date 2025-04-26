using System.Security.Claims;
using System.Security.Principal;

namespace EasyIotSharp.Core.Configuration
{
    public class UserTokenData
    {
        public UserTokenData()
        {
            UserId = string.Empty;
            UserName = string.Empty;
            TenantId = string.Empty;
            TenantAbbreviation = string.Empty;
            TenantNumId = default;
        }

        /// <summary>
        /// 当前用户Id
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 当前用户名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 当前租户Id
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// 租户简称
        /// </summary>
        public string TenantAbbreviation { get; set; }

        /// <summary>
        /// 当前租户NumId
        /// </summary>
        public int TenantNumId { get; set; }
    }

    public class UserTokenIdentity : IIdentity
    {
        public UserTokenIdentity(UserTokenData identifier)
        {
            SetIdentifier(identifier);
        }

        private UserTokenData identifier = new UserTokenData();

        public UserTokenData GetIdentifier()
        {
            return identifier;
        }

        public void SetIdentifier(UserTokenData value)
        {
            identifier = value;
        }

        public string AuthenticationType => "Token";

        public bool IsAuthenticated => GetIdentifier() != null;

        public string Name => GetIdentifier() == null ? "" : GetIdentifier().UserId;
    }

    public class UserTokenPrincipal : ClaimsPrincipal
    {
        private readonly UserTokenIdentity _userTokenIdentity;

        public UserTokenPrincipal(UserTokenIdentity userTokenIdentity)
        {
            _userTokenIdentity = userTokenIdentity;
        }

        public override IIdentity Identity => _userTokenIdentity;
    }
}