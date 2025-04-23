using System.ComponentModel.DataAnnotations;

namespace EasyIotSharp.Core.Dto
{
    /// <summary>
    /// 操作用户信息输入
    /// </summary>
    public class OperateUserInput
    {
        public string OperatorId { get; set; }

        public string OperatorName { get; set; }
    }
}