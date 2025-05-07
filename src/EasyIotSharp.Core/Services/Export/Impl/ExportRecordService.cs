using Newtonsoft.Json;
using EasyIotSharp.Core.Domain.Export;
using EasyIotSharp.Core.Dto.Export;
using EasyIotSharp.Core.Dto.Export.Params;
using EasyIotSharp.Core.Repositories.Export;
using MongoDB.Bson.IO;
using System;
using System.Threading.Tasks;
using UPrime.AutoMapper;
using UPrime.Services.Dto;
using System.Collections.Generic;

namespace EasyIotSharp.Core.Services.Export.Impl
{
    /// <summary>
    /// 导出记录服务实现类
    /// </summary>
    public class ExportRecordService : ServiceBase, IExportRecordService
    {
        private readonly IExportRecordRepository _exportRecordRepository;

        public ExportRecordService(IExportRecordRepository exportRecordRepository)
        {
            _exportRecordRepository = exportRecordRepository;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<ExportRecordDto>> QueryExportRecord()
        {
            return await _exportRecordRepository.QueryExportRecord();
        }

        /// <summary>
        /// 分页查询导出记录
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>分页结果</returns>
        public async Task<PagedResultDto<ExportDataRecordDto>> QueryExportRecord(ExportRecordInput input)
        {
            var (totalCount, items) = await _exportRecordRepository.QueryExportRecordPage(ContextUser?.TenantId ?? "", input.ProjectId, input.PageIndex, input.PageSize, input.IsPage);
            return new PagedResultDto<ExportDataRecordDto>
            {
                TotalCount = totalCount,
                Items = items
            };
        }

        /// <summary>
        /// 创建导出记录
        /// </summary>
        /// <param name="input">创建参数</param>
        /// <returns>新创建的记录ID</returns>
        public async Task<string> CreateExportRecord(ExportRecordInsert input)
        {
            if (!string.IsNullOrEmpty(input.ConditionJson))
            {
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<ExportDataInput>(input.ConditionJson);
                if (data.StartTime.Value.Date.AddDays(60) > data.EndTime.Value.Date)
                {
                    throw new BizException(BizError.NO_HANDLER_FOUND, "一次性导出最多60天的数据");
                }
            }

            if (string.IsNullOrEmpty(input.Name))
            {
                input.Name = "Data";
            }
            var entity = new ExportRecord
            {
                Id = Guid.NewGuid().ToString().Replace("-", ""),
                TenantId = ContextUser.TenantId,
                ProjectId = input.ProjectId,
                Name = input.Name + "-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                ConditionJson = input.ConditionJson,
                State = 0, // 初始状态：未执行
                CreationTime = DateTime.Now,
                UpdatedAt = DateTime.Now,
                OperatorId = ContextUser.UserId,
                OperatorName = ContextUser.UserName
            };

            await _exportRecordRepository.InsertAsync(entity);
            return entity.Id;
        }

        /// <summary>
        /// 更新导出记录
        /// </summary>
        /// <param name="input">更新参数</param>
        public async Task UpdateExportRecord(ExportRecordInsert input)
        {
            var entity = await _exportRecordRepository.FirstOrDefaultAsync(x => x.Id == input.Id && x.IsDelete == false);
            if (entity == null)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定记录");
            }
            if (string.IsNullOrEmpty(input.Name))
            {
                input.Name = "Data";
            }

            entity.Name = input.Name + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            entity.ConditionJson = input.ConditionJson;
            entity.State = input.State;
            entity.UpdatedAt = DateTime.Now;
            entity.OperatorId = ContextUser.UserId;
            entity.OperatorName = ContextUser.UserName;

            await _exportRecordRepository.UpdateAsync(entity);
        }

        /// <summary>
        /// 删除导出记录
        /// </summary>
        /// <param name="id">记录ID</param>
        public async Task DeleteExportRecord(string id)
        {
            var entity = await _exportRecordRepository.FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == false);
            if (entity == null)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定记录");
            }

            entity.IsDelete = true;
            entity.UpdatedAt = DateTime.Now;
            entity.OperatorId = ContextUser.UserId;
            entity.OperatorName = ContextUser.UserName;

            await _exportRecordRepository.UpdateAsync(entity);
        }
    }
}
