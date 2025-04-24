using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Hardware.Impl;
using EasyIotSharp.Core.Caching.Hardware;
using EasyIotSharp.Core.Events.Hardware;
using UPrime.Dependency;
using UPrime.Events.Bus.Handlers;
using EasyIotSharp.Core.Events.Project;
using EasyIotSharp.Core.Caching.Project;
using EasyIotSharp.Core.Caching.Project.Impl;

namespace EasyIotSharp.Core.Caching.Consumers.Project
{
    public class ClassificationCachingRemoveHandler : IEventHandler<ClassificationEventData>, ITransientDependency
    {
        private readonly IClassificationCacheService _classificationCacheService;
        public ClassificationCachingRemoveHandler(IClassificationCacheService classificationCacheService)
        {
            _classificationCacheService = classificationCacheService;
        }
        public void HandleEvent(ClassificationEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _classificationCacheService.Cache.RemoveAsync(ClassificationCacheService.KEY_PROJECT_CLASSIFICATION_QUERY.FormatWith(i, 10));
            }
        }
    }
}