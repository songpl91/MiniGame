using System;

namespace UniFramework.ObjectPool.Enhanced
{
    /// <summary>
    /// 增强版对象池配置类
    /// 在极简版基础上增加必要的配置选项，为后期升级做准备
    /// </summary>
    [Serializable]
    public class EnhancedPoolConfig
    {
        #region 基础配置

        /// <summary>
        /// 初始容量（预创建对象数量）
        /// </summary>
        public int InitialCapacity { get; set; } = 0;

        /// <summary>
        /// 最大容量（池中最多保存的对象数量）
        /// </summary>
        public int MaxCapacity { get; set; } = 50;

        #endregion

        #region 统计配置

        /// <summary>
        /// 是否启用统计信息收集
        /// 启用后会收集命中率、创建数量等统计数据
        /// </summary>
        public bool EnableStatistics { get; set; } = true;

        #endregion

        #region 验证配置

        /// <summary>
        /// 是否在对象归还时验证对象有效性
        /// 启用后会检查归还的对象是否来自当前池
        /// </summary>
        public bool ValidateOnReturn { get; set; } = true;

        #endregion

        #region 扩展配置预留

        /// <summary>
        /// 是否启用调试模式
        /// 调试模式下会输出更多日志信息
        /// </summary>
        public bool EnableDebugMode { get; set; } = false;

        /// <summary>
        /// 自定义标签，用于标识不同用途的对象池
        /// </summary>
        public string Tag { get; set; } = string.Empty;

        #endregion

        #region 静态工厂方法

        /// <summary>
        /// 创建默认配置
        /// 适用于大多数场景的平衡配置
        /// </summary>
        /// <returns>默认配置实例</returns>
        public static EnhancedPoolConfig CreateDefault()
        {
            return new EnhancedPoolConfig
            {
                InitialCapacity = 0,
                MaxCapacity = 50,
                EnableStatistics = true,
                ValidateOnReturn = true,
                EnableDebugMode = false
            };
        }

        /// <summary>
        /// 创建性能优先配置
        /// 关闭验证和统计以获得最佳性能
        /// </summary>
        /// <returns>性能优先配置实例</returns>
        public static EnhancedPoolConfig CreatePerformance()
        {
            return new EnhancedPoolConfig
            {
                InitialCapacity = 10,
                MaxCapacity = 100,
                EnableStatistics = false,
                ValidateOnReturn = false,
                EnableDebugMode = false
            };
        }

        /// <summary>
        /// 创建调试配置
        /// 启用所有调试和验证功能
        /// </summary>
        /// <returns>调试配置实例</returns>
        public static EnhancedPoolConfig CreateDebug()
        {
            return new EnhancedPoolConfig
            {
                InitialCapacity = 5,
                MaxCapacity = 30,
                EnableStatistics = true,
                ValidateOnReturn = true,
                EnableDebugMode = true
            };
        }

        /// <summary>
        /// 创建小型对象池配置
        /// 适用于使用频率较低的对象
        /// </summary>
        /// <returns>小型对象池配置实例</returns>
        public static EnhancedPoolConfig CreateSmall()
        {
            return new EnhancedPoolConfig
            {
                InitialCapacity = 0,
                MaxCapacity = 20,
                EnableStatistics = true,
                ValidateOnReturn = true,
                EnableDebugMode = false
            };
        }

        /// <summary>
        /// 创建大型对象池配置
        /// 适用于使用频率很高的对象
        /// </summary>
        /// <returns>大型对象池配置实例</returns>
        public static EnhancedPoolConfig CreateLarge()
        {
            return new EnhancedPoolConfig
            {
                InitialCapacity = 20,
                MaxCapacity = 200,
                EnableStatistics = true,
                ValidateOnReturn = false, // 大型池关闭验证以提高性能
                EnableDebugMode = false
            };
        }

        /// <summary>
        /// 创建高性能配置（适用于频繁创建销毁的场景）
        /// 与 PoolConfig.CreateHighPerformance() 保持一致
        /// </summary>
        /// <returns>高性能配置实例</returns>
        public static EnhancedPoolConfig CreateHighPerformance()
        {
            return new EnhancedPoolConfig
            {
                InitialCapacity = 20,
                MaxCapacity = 200,
                EnableStatistics = false,
                ValidateOnReturn = false,
                EnableDebugMode = false
            };
        }

        /// <summary>
        /// 创建内存优化配置（适用于内存敏感的场景）
        /// 与 PoolConfig.CreateMemoryOptimized() 保持一致
        /// </summary>
        /// <returns>内存优化配置实例</returns>
        public static EnhancedPoolConfig CreateMemoryOptimized()
        {
            return new EnhancedPoolConfig
            {
                InitialCapacity = 5,
                MaxCapacity = 50,
                EnableStatistics = true,
                ValidateOnReturn = true,
                EnableDebugMode = false
            };
        }

        /// <summary>
        /// 创建平衡配置（性能和内存的平衡）
        /// 适用于大多数常规场景
        /// </summary>
        /// <returns>平衡配置实例</returns>
        public static EnhancedPoolConfig CreateBalanced()
        {
            return new EnhancedPoolConfig
            {
                InitialCapacity = 10,
                MaxCapacity = 100,
                EnableStatistics = true,
                ValidateOnReturn = true,
                EnableDebugMode = false
            };
        }

        #endregion

        #region 配置验证

        /// <summary>
        /// 验证配置的有效性
        /// </summary>
        /// <returns>配置是否有效</returns>
        public bool IsValid()
        {
            return InitialCapacity >= 0 && 
                   MaxCapacity > 0 && 
                   InitialCapacity <= MaxCapacity;
        }

        /// <summary>
        /// 获取配置的描述信息
        /// </summary>
        /// <returns>配置描述</returns>
        public string GetDescription()
        {
            return $"EnhancedPoolConfig[Initial:{InitialCapacity}, Max:{MaxCapacity}, " +
                   $"Stats:{EnableStatistics}, Validate:{ValidateOnReturn}, Debug:{EnableDebugMode}]";
        }

        #endregion
    }
}