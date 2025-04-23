using System.ComponentModel.DataAnnotations;

namespace EasyIotSharp.Core.Dto
{
    /// <summary>
    /// 删除输入参数
    /// </summary>
    public class DeleteInput : OperateUserInput
    {
        /// <summary>
        /// 对象id
        /// </summary>
        public string Id { get; set; }
    }
}