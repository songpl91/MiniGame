using System;

namespace UniFramework.ObjectPool
{
    /// <summary>
    /// 对象池配置类
    /// 定义对象池的各种配置参数
    /// </summary>
    [Serializable]
    public class PoolConfig
    {
        /// <summary>
        /// 初始容量（预创建对象数量）
        /// </summary>
        public int InitialCapacity { get; set; } = 0;

        /// <summary>
        /// 最大容量（池中最多保存的对象数量）
        /// </summary>
        public int MaxCapacity { get; set; } = 100;

        /// <summary>
        /// 是否启用自动清理
        /// </summary>
        public bool EnableAutoCleanup { get; set; } = true;

        /// <summary>
        /// 自动清理间隔时间（秒）
        /// </summary>
        public float CleanupInterval { get; set; } = 60f;

        /// <summary>
        /// 清理阈值（当池中对象数量超过此值时触发清理）
        /// </summary>
        public int CleanupThreshold { get; set; } = 50;

        /// <summary>
        /// 每次清理的对象数量
        /// </summary>
        public int CleanupCount { get; set; } = 10;

        /// <summary>
        /// 是否启用统计信息收集
        /// </summary>
        public bool EnableStatistics { get; set; } = true;

        /// <summary>
        /// 是否在对象归还时验证对象有效性
        /// </summary>
        public bool ValidateOnReturn { get; set; } = true;

        /// <summary>
        /// 创建默认配置
        /// </summary>
        /// <returns>默认配置实例</returns>
        public static PoolConfig CreateDefault()
        {
            return new PoolConfig();
        }

        /// <summary>
        /// 创建高性能配置（适用于频繁创建销毁的场景）
        /// </summary>
        /// <returns>高性能配置实例</returns>
        public static PoolConfig CreateHighPerformance()
        {
            return new PoolConfig
            {
                InitialCapacity = 20,
                MaxCapacity = 200,
                EnableAutoCleanup = true,
                CleanupInterval = 30f,
                CleanupThreshold = 100,
                CleanupCount = 20,
                EnableStatistics = false,
                ValidateOnReturn = false
            };
        }

        /// <summary>
        /// 创建内存优化配置（适用于内存敏感的场景）
        /// </summary>
        /// <returns>内存优化配置实例</returns>
        public static PoolConfig CreateMemoryOptimized()
        {
            return new PoolConfig
            {
                InitialCapacity = 5,
                MaxCapacity = 50,
                EnableAutoCleanup = true,
                CleanupInterval = 15f,
                CleanupThreshold = 25,
                CleanupCount = 15,
                EnableStatistics = true,
                ValidateOnReturn = true
            };
        }
    }
}