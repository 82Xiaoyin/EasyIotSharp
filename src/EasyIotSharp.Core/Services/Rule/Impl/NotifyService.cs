using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyIotSharp.Core.Domain.Rule;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto.Rule.Params;
using EasyIotSharp.Core.Repositories.Rule;
using UPrime.AutoMapper;
using UPrime.Services.Dto;
using SqlSugar;
using EasyIotSharp.Core.Dto;
using System.Linq;
using Nest;

namespace EasyIotSharp.Core.Services.Rule.Impl
{
    public class NotifyService : ServiceBase, INotifyService
    {
        private readonly INotifyRepository _notifyRepository;

        public NotifyService(INotifyRepository notifyRepository)
        {
            _notifyRepository = notifyRepository;
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<NotifyDto>> QueryNotifyConfig(Dto.PagingInput input)
        {
            var query = await _notifyRepository.Query(input.PageIndex, input.PageSize);
            int totalCount = query.totalCount;
            var list = query.items.MapTo<List<NotifyDto>>();
            return new PagedResultDto<NotifyDto>() { TotalCount = totalCount, Items = list };
        }

        /// <summary>
        /// 新增通知
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BizException"></exception>
        public async Task InsertNotifyConfig(InsertNotify input)
        {
            var info = await _notifyRepository.FirstOrDefaultAsync(x => x.NotifyName == input.NotifyName && x.IsDelete == false);
            if (info.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "通知名称重复");
            }

            var model = new Notify();
            model.Id = Guid.NewGuid().ToString().Replace("-", "");
            model.NotifyName = input.NotifyName;
            model.Type = input.Type;
            model.State = input.State;
            model.Remark = input.Remark;
            model.NotifyContent = input.NotifyContent;
            model.IsDelete = false;
            model.CreationTime = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _notifyRepository.InsertAsync(model);

            if (input.Type == 1)
            {
                await AddUserNotify(input.Users, model.Id);
            }

        }

        /// <summary>
        /// 修改通知
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BizException"></exception>
        public async Task UpdateNotifyConfig(InsertNotify input)
        {
            var info = await _notifyRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }
            var isExist = await _notifyRepository.FirstOrDefaultAsync(x => x.NotifyName == input.NotifyName && x.Id != input.Id && x.IsDelete == false);
            if (isExist.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "通知名称重复");
            }

            info.NotifyName = input.NotifyName;
            info.Type = input.Type;
            info.State = input.State;
            info.Remark = input.Remark;
            info.NotifyContent = input.NotifyContent;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;

            if (input.Type == 1 && info.Type != 1)
            {
                await AddUserNotify(input.Users, info.Id);
            }
            else if (input.Type != 1 && info.Type == 1)
            {
                var notifyUsers = await _notifyRepository.QueryUserNotify(input.Id);
                await _notifyRepository.DeleteableUserNotifyList(notifyUsers);
            }
            else if (input.Type == 1 && info.Type == 1)
            {
                var notifyUsers = await _notifyRepository.QueryUserNotify(input.Id);
                await _notifyRepository.DeleteableUserNotifyList(notifyUsers);

                await AddUserNotify(input.Users, info.Id);
            }

            await _notifyRepository.UpdateAsync(info);
        }

        /// <summary>
        /// 删除通知
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BizException"></exception>
        public async Task DeleteNotifyConfig(DeleteInput input)
        {
            var info = await _notifyRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }

            info.IsDelete = true;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            if (info.Type == 1)
            {
                var notifyUsers = await _notifyRepository.QueryUserNotify(input.Id);
                await _notifyRepository.DeleteableUserNotifyList(notifyUsers);
            }
            await _notifyRepository.UpdateAsync(info);
        }

        /// <summary>
        /// 修改状态
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BizException"></exception>
        public async Task UpdateNotifyConfigState(InsertNotify input)
        {
            var info = await _notifyRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }

            info.State = input.State;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            await _notifyRepository.UpdateAsync(info);
        }

        /// <summary>
        /// 通知记录列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<NotifyRecordDto>> QueryNotifyRecord(NotifyRecordInput input)
        {
            var query = await _notifyRepository.QueryNotifyRecordDto(input.Type, input.KeyWord, input.StarTime, input.EndTime, input.PageIndex, input.PageSize);
            int totalCount = query.totalCount;
            var list = query.items.MapTo<List<NotifyRecordDto>>();
            return new PagedResultDto<NotifyRecordDto>() { TotalCount = totalCount, Items = list };

        }

        private async Task AddUserNotify(List<UserNotifyDto> users, string id)
        {
            var uerNotifyList = new List<UserNotify>();

            foreach (var item in users)
            {
                var dto = new UserNotify();
                dto.Id = Guid.NewGuid().ToString();
                dto.IsEmail = item.IsEmail;
                dto.IsPhone = item.IsPhone;
                dto.IsSms = item.IsSms;
                dto.NotifyId = id;
                dto.UserId = item.UserId;
                dto.IsDelete = false;
                dto.CreationTime = DateTime.Now;
                dto.UpdatedAt = DateTime.Now;
                dto.OperatorId = ContextUser.UserId;
                dto.OperatorName = ContextUser.UserName;
                uerNotifyList.Add(dto);
            }

            await _notifyRepository.InsertUserNotifyList(uerNotifyList);
        }
    }
}