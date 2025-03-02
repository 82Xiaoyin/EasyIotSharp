﻿using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Repositories.Mysql;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Repositories.Project
{
    public interface IDeviceProtocolRepository : IMySqlRepositoryBase<DeviceProtocol, string>
    {
    }
}
