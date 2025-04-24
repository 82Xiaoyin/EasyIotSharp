using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Domain.Rule;

namespace EasyIotSharp.Core.Dto.Rule.Params
{
    public class SceneManagementInput : PagingInput
    {
        public string Keyword { get; set; }

        public string ProjectId { get; set; }
    }
}
