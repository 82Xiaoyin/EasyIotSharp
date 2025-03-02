﻿using EasyIotSharp.Core.Dto.Project;
using EasyIotSharp.Core.Dto.Project.Params;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Project
{
    public interface ISensorPointTypeService
    {
        /// <summary>
        /// 通过id获取一条测点类型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<SensorPointTypeDto> GetSensorPointType(string id);

        /// <summary>
        /// 根据条件分页查询项目分类列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<SensorPointTypeDto>> QuerySensorPointType(QuerySensorPointTypeInput input);

        /// <summary>
        /// 添加一条项目分类
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task InsertSensorPointType(InsertSensorPointTypeInput input);

        /// <summary>
        /// 通过id修改一条项目分类
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task UpdateSensorPointType(UpdateSensorPointTypeInput input);

        /// <summary>
        /// 通过id删除一条项目分类
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task DeleteSensorPointType(DeleteSensorPointTypeInput input);
    }
}
