using AutoMapper;
using EasyIotSharp.Core.Caching.Queue;
using EasyIotSharp.Core.Caching.Queue.Impl;
using EasyIotSharp.Core.Domain.Queue;
using EasyIotSharp.Core.Dto.Queue;
using EasyIotSharp.Core.Dto.Queue.Params;
using EasyIotSharp.Core.Events.Queue;
using EasyIotSharp.Core.Events.Tenant;
using EasyIotSharp.Core.Repositories.Queue;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UPrime;
using UPrime.AutoMapper;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Queue.Impl
{
    /// <summary>
    /// RabbitMQ服务器配置信息服务实现类
    /// </summary>
    public class RabbitServerInfoService : ServiceBase, IRabbitServerInfoService
    {
        private readonly IRabbitServerInfoRepository _rabbitServerInfoRepository;
        private readonly IRabbitServerInfoCacheService _rabbitServerInfoCacheService;

        public RabbitServerInfoService(IRabbitServerInfoRepository rabbitServerInfoRepository,
                                       IRabbitServerInfoCacheService rabbitServerInfoCacheService)
        {
            _rabbitServerInfoRepository = rabbitServerInfoRepository;
            _rabbitServerInfoCacheService = rabbitServerInfoCacheService;
        }

        /// <summary>
        /// 通过ID获取一条RabbitMQ服务器配置信息
        /// </summary>
        public async Task<RabbitServerInfoDto> GetRabbitServerInfo(string id)
        {
            var info = await _rabbitServerInfoRepository.FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == false);
            return info.MapTo<RabbitServerInfoDto>();
        }

        /// <summary>
        /// 添加一条RabbitMQ服务器配置信息
        /// </summary>
        public async Task InsertRabbitServerInfo(InsertRabbitServerInfoInput input)
        {
            // 参数校验
            if (string.IsNullOrWhiteSpace(input.Host))
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "主机地址不能为空");

            if (input.Port <= 0)
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "端口号必须大于0");

            if (string.IsNullOrWhiteSpace(input.Username))
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "用户名不能为空");

            if (string.IsNullOrWhiteSpace(input.Password))
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "密码不能为空");

            var entity = new RabbitServerInfo
            {
                Id = Guid.NewGuid().ToString().Replace("-", ""),
                TenantNumId = ContextUser.TenantNumId,
                Host = input.Host.Trim(),
                Port = input.Port,
                Username = input.Username.Trim(),
                Password = input.Password,
                VirtualHost = input.VirtualHost?.Trim(),
                Exchange = input.Exchange?.Trim(),
                IsEnable = input.IsEnable,
                CreationTime = DateTime.Now,
                UpdatedAt = DateTime.Now,
                OperatorId = ContextUser.UserId,
                OperatorName = ContextUser.UserName
            };

            await _rabbitServerInfoRepository.InsertAsync(entity);

            //清除缓存
            await EventBus.TriggerAsync(new RabbitServerInfoEventData() { });
            await EventBus.TriggerAsync(new RabbitProjectEventData() { });
        }

        /// <summary>
        /// 修改一条RabbitMQ服务器配置信息
        /// </summary>
        public async Task UpdateRabbitServerInfo(UpdateRabbitServerInfoInput input)
        {
            var entity = await _rabbitServerInfoRepository.FirstOrDefaultAsync(x => x.Id == input.Id && x.IsDelete == false);
            if (entity.IsNull())
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定的资源");

            // 参数校验
            if (string.IsNullOrWhiteSpace(input.Host))
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "主机地址不能为空");

            if (input.Port <= 0)
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "端口号必须大于0");

            if (string.IsNullOrWhiteSpace(input.Username))
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "用户名不能为空");

            if (string.IsNullOrWhiteSpace(input.Password))
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "密码不能为空");

            entity.Host = input.Host.Trim();
            entity.Port = input.Port;
            entity.Username = input.Username.Trim();
            entity.Password = input.Password;
            entity.VirtualHost = input.VirtualHost?.Trim();
            entity.Exchange = input.Exchange?.Trim();
            entity.IsEnable = input.IsEnable;
            entity.UpdatedAt = DateTime.Now;
            entity.OperatorId = ContextUser.UserId;
            entity.OperatorName = ContextUser.UserName;

            await _rabbitServerInfoRepository.UpdateAsync(entity);

            //清除缓存
            await EventBus.TriggerAsync(new RabbitServerInfoEventData() { });
            await EventBus.TriggerAsync(new RabbitProjectEventData() { });
        }

        /// <summary>
        /// 删除一条RabbitMQ服务器配置信息
        /// </summary>
        public async Task DeleteRabbitServerInfo(DeleteRabbitServerInfoInput input)
        {
            var entity = await _rabbitServerInfoRepository.FirstOrDefaultAsync(x => x.Id == input.Id && x.IsDelete == false);
            if (entity.IsNull())
                throw new BizException(BizError.NO_HANDLER_FOUND, "未找到指定的资源");

            entity.IsDelete = true;
            entity.DeleteTime = DateTime.Now;
            entity.OperatorId = ContextUser.UserId;
            entity.OperatorName = ContextUser.UserName;

            await _rabbitServerInfoRepository.UpdateAsync(entity);

            //清除缓存
            await EventBus.TriggerAsync(new RabbitServerInfoEventData() { });
            await EventBus.TriggerAsync(new RabbitProjectEventData() { });
        }

        /// <summary>
        /// 分页查询RabbitMQ服务器配置信息列表
        /// </summary>
        public async Task<PagedResultDto<RabbitServerInfoDto>> QueryRabbitServerInfo(QueryRabbitServerInfoInput input)
        {
            if (string.IsNullOrEmpty(input.Keyword)
               && input.IsEnable.Equals(-1)
               && input.IsPage.Equals(true)
               && input.PageIndex <= 5 && input.PageSize == 10)
            {
                return await _rabbitServerInfoCacheService.QueryRabbitServerInfo(input, async () =>
                {
                    var query = await _rabbitServerInfoRepository.Query(ContextUser.TenantNumId, input.Keyword, input.IsEnable, input.PageIndex, input.PageSize, input.IsPage);
                    var list = query.items.MapTo<List<RabbitServerInfoDto>>();
                    return new PagedResultDto<RabbitServerInfoDto>(query.totalCount, list);
                });
            }
            else
            {
                var query = await _rabbitServerInfoRepository.Query(ContextUser.TenantNumId, input.Keyword, input.IsEnable, input.PageIndex, input.PageSize, input.IsPage);
                var list = query.items.MapTo<List<RabbitServerInfoDto>>();
                return new PagedResultDto<RabbitServerInfoDto>(query.totalCount, list);
            }
        }

        /// <summary>
        /// 获取rabbit配置信息列表
        /// </summary>
        /// <returns></returns>
        public List<RabbitServerInfoDto> GetRabbitProject()
        {
            var list = _rabbitServerInfoCacheService.GetRabbitProject(() => { return _rabbitServerInfoRepository.GetRabbitProject(); });
            if (list.Count == 0)
            {
                list = _rabbitServerInfoRepository.GetRabbitProject();
            }
            return list;
        }
    }
}