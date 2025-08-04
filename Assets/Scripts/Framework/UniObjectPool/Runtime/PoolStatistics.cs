using System;

namespace UniFramework.ObjectPool
{
    /// <summary>
    /// 对象池统计信息类
    /// 用于监控对象池的性能和使用情况
    /// </summary>
    [Serializable]
    public class PoolStatistics
    {
        /// <summary>
        /// 池中当前可用对象数量
        /// </summary>
        public int AvailableCount { get; internal set; }

        /// <summary>
        /// 当前正在使用的对象数量
        /// </summary>
        public int ActiveCount { get; internal set; }

        /// <summary>
        /// 总共创建的对象数量
        /// </summary>
        public int TotalCreatedCount { get; internal set; }

        /// <summary>
        /// 总共销毁的对象数量
        /// </summary>
        public int TotalDestroyedCount { get; internal set; }

        /// <summary>
        /// 总共获取对象的次数
        /// </summary>
        public int TotalGetCount { get; internal set; }

        /// <summary>
        /// 总共归还对象的次数
        /// </summary>
        public int TotalReturnCount { get; internal set; }

        /// <summary>
        /// 缓存命中次数（从池中获取到对象的次数）
        /// </summary>
        public int CacheHitCount { get; internal set; }

        /// <summary>
        /// 缓存未命中次数（需要创建新对象的次数）
        /// </summary>
        public int CacheMissCount { get; internal set; }

        /// <summary>
        /// 自动清理执行次数
        /// </summary>
        public int CleanupExecutedCount { get; internal set; }

        /// <summary>
        /// 最后一次清理时间
        /// </summary>
        public DateTime LastCleanupTime { get; internal set; }

        /// <summary>
        /// 对象池创建时间
        /// </summary>
        public DateTime CreationTime { get; internal set; }

        /// <summary>
        /// 缓存命中率（0-1之间的值）
        /// </summary>
        public float CacheHitRate
        {
            get
            {
                int totalRequests = CacheHitCount + CacheMissCount;
                return totalRequests > 0 ? (float)CacheHitCount / totalRequests : 0f;
            }
        }

        /// <summary>
        /// 对象复用率（0-1之间的值）
        /// </summary>
        public float ReuseRate
        {
            get
            {
                return TotalCreatedCount > 0 ? (float)TotalGetCount / TotalCreatedCount : 0f;
            }
        }

        /// <summary>
        /// 池的使用效率（活跃对象数 / 总创建对象数）
        /// </summary>
        public float PoolEfficiency
        {
            get
            {
                return TotalCreatedCount > 0 ? (float)ActiveCount / TotalCreatedCount : 0f;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public PoolStatistics()
        {
            CreationTime = DateTime.Now;
            LastCleanupTime = DateTime.MinValue;
        }

        /// <summary>
        /// 重置统计信息
        /// </summary>
        public void Reset()
        {
            AvailableCount = 0;
            ActiveCount = 0;
            TotalCreatedCount = 0;
            TotalDestroyedCount = 0;
            TotalGetCount = 0;
            TotalReturnCount = 0;
            CacheHitCount = 0;
            CacheMissCount = 0;
            CleanupExecutedCount = 0;
            CreationTime = DateTime.Now;
            LastCleanupTime = DateTime.MinValue;
        }

        /// <summary>
        /// 获取统计信息的字符串表示
        /// </summary>
        /// <returns>格式化的统计信息字符串</returns>
        public override string ToString()
        {
            return $"Pool Statistics:\n" +
                   $"  Available: {AvailableCount}, Active: {ActiveCount}\n" +
                   $"  Total Created: {TotalCreatedCount}, Total Destroyed: {TotalDestroyedCount}\n" +
                   $"  Total Get: {TotalGetCount}, Total Return: {TotalReturnCount}\n" +
                   $"  Cache Hit Rate: {CacheHitRate:P2}, Reuse Rate: {ReuseRate:P2}\n" +
                   $"  Pool Efficiency: {PoolEfficiency:P2}\n" +
                   $"  Cleanup Executed: {CleanupExecutedCount} times\n" +
                   $"  Created: {CreationTime:yyyy-MM-dd HH:mm:ss}";
        }
    }
}