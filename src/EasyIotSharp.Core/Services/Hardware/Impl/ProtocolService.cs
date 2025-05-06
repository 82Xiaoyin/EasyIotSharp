using EasyIotSharp.Core.Caching.Hardware;
using EasyIotSharp.Core.Domain.Hardware;
using EasyIotSharp.Core.Dto.Hardware;
using EasyIotSharp.Core.Dto.Hardware.Params;
using EasyIotSharp.Core.Events.Hardware;
using EasyIotSharp.Core.Events.Tenant;
using EasyIotSharp.Core.Repositories.Hardware;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPrime.AutoMapper;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Hardware.Impl
{
    public class ProtocolService : ServiceBase, IProtocolService
    {
        private readonly IProtocolRepository _protocolRepository;
        private readonly IProtocolCacheService _protocolCacheService;


        public ProtocolService(IProtocolRepository protocolRepository,
            IProtocolCacheService protocolCacheService)
        {
            _protocolRepository = protocolRepository;
            _protocolCacheService = protocolCacheService;
        }

        public async Task<ProtocolDto> GetProtocol(string id)
        {
            var info = await _protocolRepository.FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == false);
            return info.MapTo<ProtocolDto>();
        }
        
        /// <summary>
        /// 根据条件分页查询协议列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<ProtocolDto>> QueryProtocol(QueryProtocolInput input)
        {
            if(string.IsNullOrEmpty(input.Keyword) && input.IsEnable.Equals(-1)
                && input.IsPage.Equals(true)
                && input.PageIndex <= 5 && input.PageSize == 10)
            {
                return await _protocolCacheService.QueryProtocol(input, async () =>
                {
                    var query = await _protocolRepository.Query(input.IsEnable, input.Keyword, input.PageIndex, input.PageSize, input.IsPage);
                    int totalCount = query.totalCount;
                    var list = query.items.MapTo<List<ProtocolDto>>();

                    return new PagedResultDto<ProtocolDto>() { TotalCount = totalCount, Items = list };
                });
            }
            else
            {
                var query = await _protocolRepository.Query(input.IsEnable, input.Keyword, input.PageIndex, input.PageSize, input.IsPage);
                int totalCount = query.totalCount;
                var list = query.items.MapTo<List<ProtocolDto>>();

                return new PagedResultDto<ProtocolDto>() { TotalCount = totalCount, Items = list };
            }
        }

        /// <summary>
        /// 添加一条协议信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task InsertProtocol(InsertProtocolInput input)
        {
            var isExistName = await _protocolRepository.FirstOrDefaultAsync(x => x.Name == input.Name && x.IsDelete == false);
            if (isExistName.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "协议名称重复");
            }
            var model = new Protocol();
            model.Id = Guid.NewGuid().ToString().Replace("-", "");
            model.Name = input.Name;
            model.Remark = input.Remark;
            model.IsEnable = input.IsEnable;
            model.IsDelete = false;
            model.CreationTime = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _protocolRepository.InsertAsync(model);

            //清除缓存
            await EventBus.TriggerAsync(new ProtocolEventData() { });
        }

        /// <summary>
        /// 通过id修改一条协议信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task UpdateProtocol(UpdateProtocolInput input)
        {
            var info = await _protocolRepository.FirstOrDefaultAsync(x => x.Id == input.Id && x.IsDelete == false);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "协议不存在");
            }
            var isExistName = await _protocolRepository.FirstOrDefaultAsync(x => x.Name == input.Name && x.IsDelete == false);
            if (isExistName.IsNotNull() && isExistName.Id != input.Id)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "协议名称重复");
            }
            info.Name = input.Name;
            info.IsEnable = input.IsEnable;
            info.Remark = input.Remark;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            await _protocolRepository.UpdateAsync(info);

            //清除缓存
            await EventBus.TriggerAsync(new ProtocolEventData() { });
        }

        /// <summary>
        /// 通过id删除一条协议信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task DeleteProtocol(DeleteProtocolInput input)
        {
            var info = await _protocolRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }
            if (info.IsDelete != input.IsDelete)
            {
                info.IsDelete = input.IsDelete;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                info.UpdatedAt = DateTime.Now;
                await _protocolRepository.UpdateAsync(info);
            }

            //清除缓存
            await EventBus.TriggerAsync(new ProtocolEventData() { });
        }
    }
}
