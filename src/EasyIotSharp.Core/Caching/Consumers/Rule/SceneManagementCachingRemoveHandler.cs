using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Rule.Impl;
using EasyIotSharp.Core.Caching.Rule;
using EasyIotSharp.Core.Events.Rule;
using UPrime.Dependency;
using UPrime.Events.Bus.Handlers;

namespace EasyIotSharp.Core.Caching.Consumers.Rule
{
    public class SceneManagementCachingRemoveHandler : IEventHandler<SceneManagementEventData>, ITransientDependency
    {
        private readonly ISceneManagementCacheService _sceneManagementCacheService;
        public SceneManagementCachingRemoveHandler(ISceneManagementCacheService sceneManagementCacheService)
        {
            _sceneManagementCacheService = sceneManagementCacheService;
        }
        public void HandleEvent(SceneManagementEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _sceneManagementCacheService.Cache.RemoveAsync(SceneManagementCacheService.KEY_RULE_SCENEMANAGEMENT_QUERY.FormatWith(i, 10));
            }
        }
    }
}
