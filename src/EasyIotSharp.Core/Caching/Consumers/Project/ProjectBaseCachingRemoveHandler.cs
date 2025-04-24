using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Project.Impl;
using EasyIotSharp.Core.Caching.Project;
using EasyIotSharp.Core.Events.Project;
using UPrime.Dependency;
using UPrime.Events.Bus.Handlers;

namespace EasyIotSharp.Core.Caching.Consumers.Project
{
    public class ProjectBaseCachingRemoveHandler : IEventHandler<ProjectBaseEventData>, ITransientDependency
    {
        private readonly IProjectBaseCacheService _projectBaseCacheService;
        public ProjectBaseCachingRemoveHandler(IProjectBaseCacheService projectBaseCacheService)
        {
            _projectBaseCacheService = projectBaseCacheService;
        }
        public void HandleEvent(ProjectBaseEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _projectBaseCacheService.Cache.RemoveAsync(ProjectBaseCacheService.KEY_PROJECT_PROJECTBASE_QUERY.FormatWith(i, 10));
            }
        }
    }
}