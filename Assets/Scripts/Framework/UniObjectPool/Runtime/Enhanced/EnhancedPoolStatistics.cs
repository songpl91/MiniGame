using System;

namespace UniFramework.ObjectPool.Enhanced
{
    /// <summary>
    /// 增强版对象池统计信息类
    /// 提供对象池使用情况的统计数据，便于性能分析和优化
    /// </summary>
    [Serializable]
    public class EnhancedPoolStatistics
    {
        #region 基础统计

        /// <summary>
        /// 当前池中可用对象数量
        /// </summary>
        public int AvailableCount { get; internal set; }

        /// <summary>
        /// 当前活跃对象数量（已从池中取出但未归还）
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

        #endregion

        #region 操作统计

        /// <summary>
        /// 总共获取对象的次数
        /// </summary>
        public int TotalGetCount { get; internal set; }

        /// <summary>
        /// 总共归还对象的次数
        /// </summary>
        public int TotalReturnCount { get; internal set; }

        /// <summary>
        /// 缓存命中次数（从池中获取到对象）
        /// </summary>
        public int CacheHitCount { get; internal set; }

        /// <summary>
        /// 缓存未命中次数（需要创建新对象）
        /// </summary>
        public int CacheMissCount { get; internal set; }

        #endregion

        #region 错误统计

        /// <summary>
        /// 验证失败次数（归还无效对象）
        /// </summary>
        public int ValidationFailureCount { get; internal set; }

        /// <summary>
        /// 池满时丢弃的对象数量
        /// </summary>
        public int DiscardedCount { get; internal set; }

        #endregion

        #region 时间统计

        /// <summary>
        /// 对象池创建时间
        /// </summary>
        public DateTime CreationTime { get; internal set; }

        /// <summary>
        /// 最后一次获取对象的时间
        /// </summary>
        public DateTime LastGetTime { get; internal set; }

        /// <summary>
        /// 最后一次归还对象的时间
        /// </summary>
        public DateTime LastReturnTime { get; internal set; }

        #endregion

        #region 计算属性

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
        /// 池使用效率（活跃对象数 / 总对象数）
        /// </summary>
        public float PoolEfficiency
        {
            get
            {
                int totalObjects = AvailableCount + ActiveCount;
                return totalObjects > 0 ? (float)ActiveCount / totalObjects : 0f;
            }
        }

        /// <summary>
        /// 运行时长（秒）
        /// </summary>
        public double RuntimeSeconds
        {
            get
            {
                return (DateTime.Now - CreationTime).TotalSeconds;
            }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public EnhancedPoolStatistics()
        {
            CreationTime = DateTime.Now;
            Reset();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 重置所有统计数据
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
            ValidationFailureCount = 0;
            DiscardedCount = 0;
            LastGetTime = DateTime.MinValue;
            LastReturnTime = DateTime.MinValue;
        }

        /// <summary>
        /// 获取统计信息的详细描述
        /// </summary>
        /// <returns>统计信息描述</returns>
        public string GetDetailedInfo()
        {
            return $"Enhanced Pool Statistics:\n" +
                   $"  Available: {AvailableCount}, Active: {ActiveCount}\n" +
                   $"  Created: {TotalCreatedCount}, Destroyed: {TotalDestroyedCount}\n" +
                   $"  Gets: {TotalGetCount}, Returns: {TotalReturnCount}\n" +
                   $"  Cache Hit Rate: {CacheHitRate:P2}\n" +
                   $"  Reuse Rate: {ReuseRate:F2}\n" +
                   $"  Pool Efficiency: {PoolEfficiency:P2}\n" +
                   $"  Validation Failures: {ValidationFailureCount}\n" +
                   $"  Discarded: {DiscardedCount}\n" +
                   $"  Runtime: {RuntimeSeconds:F1}s";
        }

        /// <summary>
        /// 获取简要统计信息
        /// </summary>
        /// <returns>简要统计信息</returns>
        public string GetSummary()
        {
            return $"Pool[Available:{AvailableCount}, Active:{ActiveCount}, " +
                   $"HitRate:{CacheHitRate:P1}, Efficiency:{PoolEfficiency:P1}]";
        }

        #endregion

        #region 内部更新方法

        /// <summary>
        /// 记录对象获取操作
        /// </summary>
        /// <param name="fromPool">是否从池中获取</param>
        internal void RecordGet(bool fromPool)
        {
            TotalGetCount++;
            LastGetTime = DateTime.Now;
            
            if (fromPool)
            {
                CacheHitCount++;
            }
            else
            {
                CacheMissCount++;
                TotalCreatedCount++;
            }
        }

        /// <summary>
        /// 记录对象归还操作
        /// </summary>
        /// <param name="successful">是否成功归还</param>
        internal void RecordReturn(bool successful)
        {
            TotalReturnCount++;
            LastReturnTime = DateTime.Now;
            
            if (!successful)
            {
                ValidationFailureCount++;
            }
        }

        /// <summary>
        /// 记录对象销毁操作
        /// </summary>
        internal void RecordDestroy()
        {
            TotalDestroyedCount++;
        }

        /// <summary>
        /// 记录对象丢弃操作（池满时）
        /// </summary>
        internal void RecordDiscard()
        {
            DiscardedCount++;
            TotalDestroyedCount++;
        }

        #endregion
    }
}