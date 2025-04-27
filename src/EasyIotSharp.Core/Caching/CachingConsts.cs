namespace EasyIotSharp.Core.Caching
{
    public class CachingConsts
    {
        /// <summary>
        /// 缓存的所有KEY的定义
        /// </summary>
        public static class Keys
        {
            /// <summary>
            /// 项目缓存前缀（清除所有用）
            /// </summary>
            public const string BASE = "service:EasyIotSharp";
            public const string TenantCache = BASE+ "Tenant_";
            public const string HardwareCache = BASE + "Hardware_";
            public const string Project = BASE + "Project_";
            public const string Queue = BASE + "Queue_";
            public const string Rule = BASE + "Rule_";
            public const string TenantAccount = BASE + "TenantAccount_";
            public const string Gateways = BASE + "Gateways_";
        }

        /// <summary>
        /// 缓存默认过期 = 12小时
        /// </summary>
        public const int DEFAULT_EXPIRES_MINUTES = 720;

        /// <summary>
        /// 检索相关的缓存过期时间（相对精准）
        /// </summary>
        public const int QUERY_EXPIRES_MINUTES = 120;

        /// <summary>
        /// 检索相关的缓存过期时间（非精准关键字） = 1小时
        /// </summary>
        public const int SEARCH_EXPIRES_MINUTES = 60;

        /// <summary>
        /// 租户相关的缓存过期时间 = 2小时
        /// </summary>
        public const int TENANT_EXPIRES_MINUTES = 120;

        /// <summary>
        /// 缓存10分钟过期
        /// </summary>
        public const int TEN_EXPIRES_MINUTES = 10;

        /// <summary>
        /// 缓存30分钟过期
        /// </summary>
        public const int THIRTY_EXPIRES_MINUTES = 30;

        /// <summary>
        /// 缓存24小时过期
        /// </summary>
        public const int TWENTY_FOUR_EXPIRES_HOURS = 24;
    }
}