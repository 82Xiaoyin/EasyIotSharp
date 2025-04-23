using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace EasyIotSharp.Core.Domain.Rule
{
    /// <summary>
    /// 场景管理
    /// </summary>
    [SugarTable("SceneManagement")]
    public class SceneManagement : BaseEntity<string>
    {
        /// <summary>
        /// 场景名称
        /// </summary>
        public string SceneName { get; set; }

        /// <summary>
        /// 场景描述
        /// </summary>
        public string SceneRemark { get; set; }

        /// <summary>
        /// json字符串
        /// </summary>
        public string ActionJSON { get; set; }
    }
}
