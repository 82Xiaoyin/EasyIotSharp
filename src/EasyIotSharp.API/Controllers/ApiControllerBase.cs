using EasyIotSharp.Core.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace EasyIotSharp.API.Controllers
{
    public class ApiControllerBase : Controller
    {
        /// <summary>
        /// ��ȡ�û���ʶ
        /// </summary>
        public string TokenUserId
        {
            get
            {
                var userId = HttpContext.User.Identity.GetUserTokenIdentifier()?.UserId;
                return userId.IsNotNullOrEmpty() ? userId : string.Empty;
            }
        }
    }
}