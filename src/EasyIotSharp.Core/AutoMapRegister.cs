using AutoMapper;
using System;
using UPrime;
using UPrime.AutoMapper;
using EasyIotSharp.Core.Extensions;
using static EasyIotSharp.Core.GlobalConsts;
using EasyIotSharp.Core.Dto.Queue;
using EasyIotSharp.Core.Domain.Queue;

namespace EasyIotSharp.Core
{
    public class AutoMapRegister : IAutoMapRegistrar
    {
        public void RegisterMaps(IMapperConfigurationExpression config)
        {
            config.CreateMap<RabbitServerInfo, RabbitServerInfoDto>();
           
            
            //config.CreateMap<PolyvWatchLog_ES, ExportPolyvWatchLogDataDto>()
            //.ForMember(dto => dto.UserMobile, opt => opt.MapFrom(f => f.UserMobile.EncryptMobileNumber()));
        }
    }
}