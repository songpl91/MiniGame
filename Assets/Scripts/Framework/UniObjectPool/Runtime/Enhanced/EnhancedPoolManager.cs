using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UniFramework.ObjectPool.Enhanced
{
    /// <summary>
    /// 增强版对象池管理器
    /// 在极简版基础上增加更强大的管理功能和统计信息
    /// 支持配置管理、批量操作、性能监控等高级功能
    /// </summary>
    public static class EnhancedPoolManager
    {
        #region 私有字段

        private static readonly Dictionary<Type, object> _typePools = new Dictionary<Type, object>();
        private static readonly Dictionary<string, object> _namedPools = new Dictionary<string, object>();
        private static readonly Dictionary<string, Type> _namedPoolTypes = new Dictionary<string, Type>();
        private static bool _isInitialized = false;
        private static EnhancedPoolConfig _defaultConfig;
        private static bool _enableGlobalStatistics = true;

        #endregion

        #region 属性

        /// <summary>
        /// 已注册的对象池数量
        /// </summary>
        public static int PoolCount => _typePools.Count + _namedPools.Count;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// 默认配置
        /// </summary>
        public static EnhancedPoolConfig DefaultConfig => _defaultConfig;

        /// <summary>
        /// 是否启用全局统计
        /// </summary>
        public static bool EnableGlobalStatistics
        {
            get => _enableGlobalStatistics;
            set => _enableGlobalStatistics = value;
        }

        #endregion

        #region 初始化和配置

        /// <summary>
        /// 初始化对象池管理器
        /// </summary>
        /// <param name="defaultConfig">默认配置</param>
        public static void Initialize(EnhancedPoolConfig defaultConfig = null)
        {
            if (_isInitialized)
                return;

            _defaultConfig = defaultConfig ?? EnhancedPoolConfig.CreateDefault();
            _isInitialized = true;

            Debug.Log($"[EnhancedPoolManager] Initialized with config: {_defaultConfig.GetDescription()}");
            
            // 为后期扩展预留初始化逻辑
            OnInitialize();
        }

        /// <summary>
        /// 设置默认配置
        /// </summary>
        /// <param name="config">新的默认配置</param>
        public static void SetDefaultConfig(EnhancedPoolConfig config)
        {
            if (config == null || !config.IsValid())
            {
                Debug.LogWarning("[EnhancedPoolManager] Invalid default config provided");
                return;
            }

            _defaultConfig = config;
            Debug.Log($"[EnhancedPoolManager] Default config updated: {config.GetDescription()}");
        }

        #endregion

        #region 类型对象池管理

        /// <summary>
        /// 创建类型对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="createFunc">对象创建函数</param>
        /// <param name="resetAction">对象重置动作</param>
        /// <param name="destroyAction">对象销毁动作</param>
        /// <param name="config">对象池配置</param>
        /// <returns>对象池实例</returns>
        public static EnhancedPool<T> CreatePool<T>(
            Func<T> createFunc,
            Action<T> resetAction = null,
            Action<T> destroyAction = null,
            EnhancedPoolConfig config = null) where T : class
        {
            if (!_isInitialized)
                Initialize();

            Type type = typeof(T);
            
            if (_typePools.ContainsKey(type))
            {
                Debug.LogWarning($"[EnhancedPoolManager] Pool for type {type.Name} already exists");
                return _typePools[type] as EnhancedPool<T>;
            }

            var poolConfig = config ?? _defaultConfig ?? EnhancedPoolConfig.CreateDefault();
            var pool = new EnhancedPool<T>(createFunc, resetAction, destroyAction, poolConfig);
            _typePools[type] = pool;

            Debug.Log($"[EnhancedPoolManager] Created type pool for {type.Name}");
            return pool;
        }

        /// <summary>
        /// 获取类型对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>对象池实例，如果不存在则返回 null</returns>
        public static EnhancedPool<T> GetPool<T>() where T : class
        {
            Type type = typeof(T);
            return _typePools.TryGetValue(type, out object pool) ? pool as EnhancedPool<T> : null;
        }

        /// <summary>
        /// 从类型对象池获取对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>对象实例</returns>
        public static T Get<T>() where T : class
        {
            var pool = GetPool<T>();
            if (pool == null)
            {
                Debug.LogWarning($"[EnhancedPoolManager] No pool found for type {typeof(T).Name}");
                return null;
            }
            return pool.Get();
        }

        /// <summary>
        /// 将对象归还到类型对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="item">要归还的对象</param>
        /// <returns>是否成功归还</returns>
        public static bool Return<T>(T item) where T : class
        {
            var pool = GetPool<T>();
            if (pool == null)
            {
                Debug.LogWarning($"[EnhancedPoolManager] No pool found for type {typeof(T).Name}");
                return false;
            }
            return pool.Return(item);
        }

        #endregion

        #region 命名对象池管理

        /// <summary>
        /// 创建命名对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="poolName">对象池名称</param>
        /// <param name="createFunc">对象创建函数</param>
        /// <param name="resetAction">对象重置动作</param>
        /// <param name="destroyAction">对象销毁动作</param>
        /// <param name="config">对象池配置</param>
        /// <returns>对象池实例</returns>
        public static EnhancedPool<T> CreatePool<T>(
            string poolName,
            Func<T> createFunc,
            Action<T> resetAction = null,
            Action<T> destroyAction = null,
            EnhancedPoolConfig config = null) where T : class
        {
            if (!_isInitialized)
                Initialize();

            if (string.IsNullOrEmpty(poolName))
                throw new ArgumentException("对象池名称不能为空", nameof(poolName));

            if (_namedPools.ContainsKey(poolName))
            {
                Debug.LogWarning($"[EnhancedPoolManager] Named pool '{poolName}' already exists");
                return _namedPools[poolName] as EnhancedPool<T>;
            }

            var poolConfig = config ?? _defaultConfig ?? EnhancedPoolConfig.CreateDefault();
            poolConfig.Tag = poolName; // 设置标签
            
            var pool = new EnhancedPool<T>(createFunc, resetAction, destroyAction, poolConfig);
            _namedPools[poolName] = pool;
            _namedPoolTypes[poolName] = typeof(T);

            Debug.Log($"[EnhancedPoolManager] Created named pool '{poolName}' for type {typeof(T).Name}");
            return pool;
        }

        /// <summary>
        /// 获取命名对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="poolName">对象池名称</param>
        /// <returns>对象池实例，如果不存在则返回 null</returns>
        public static EnhancedPool<T> GetPool<T>(string poolName) where T : class
        {
            if (string.IsNullOrEmpty(poolName))
                return null;

            return _namedPools.TryGetValue(poolName, out object pool) ? pool as EnhancedPool<T> : null;
        }

        /// <summary>
        /// 从命名对象池获取对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="poolName">对象池名称</param>
        /// <returns>对象实例</returns>
        public static T Get<T>(string poolName) where T : class
        {
            var pool = GetPool<T>(poolName);
            if (pool == null)
            {
                Debug.LogWarning($"[EnhancedPoolManager] No named pool found: '{poolName}'");
                return null;
            }
            return pool.Get();
        }

        /// <summary>
        /// 将对象归还到命名对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="poolName">对象池名称</param>
        /// <param name="item">要归还的对象</param>
        /// <returns>是否成功归还</returns>
        public static bool Return<T>(string poolName, T item) where T : class
        {
            var pool = GetPool<T>(poolName);
            if (pool == null)
            {
                Debug.LogWarning($"[EnhancedPoolManager] No named pool found: '{poolName}'");
                return false;
            }
            return pool.Return(item);
        }

        #endregion

        #region 批量操作

        /// <summary>
        /// 预热所有对象池
        /// </summary>
        /// <param name="count">每个池预热的对象数量</param>
        public static void PrewarmAllPools(int count = 5)
        {
            int prewarmedCount = 0;

            // 预热类型池
            foreach (var kvp in _typePools)
            {
                if (kvp.Value is IDisposable pool)
                {
                    try
                    {
                        var prewarmMethod = pool.GetType().GetMethod("Prewarm");
                        prewarmMethod?.Invoke(pool, new object[] { count });
                        prewarmedCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EnhancedPoolManager] Error prewarming type pool {kvp.Key.Name}: {ex.Message}");
                    }
                }
            }

            // 预热命名池
            foreach (var kvp in _namedPools)
            {
                if (kvp.Value is IDisposable pool)
                {
                    try
                    {
                        var prewarmMethod = pool.GetType().GetMethod("Prewarm");
                        prewarmMethod?.Invoke(pool, new object[] { count });
                        prewarmedCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EnhancedPoolManager] Error prewarming named pool '{kvp.Key}': {ex.Message}");
                    }
                }
            }

            Debug.Log($"[EnhancedPoolManager] Prewarmed {prewarmedCount} pools with {count} objects each");
        }

        /// <summary>
        /// 清理所有对象池
        /// </summary>
        public static void ClearAllPools()
        {
            int clearedCount = 0;

            // 清理类型池
            foreach (var kvp in _typePools)
            {
                if (kvp.Value is IDisposable pool)
                {
                    try
                    {
                        var clearMethod = pool.GetType().GetMethod("Clear");
                        clearMethod?.Invoke(pool, null);
                        clearedCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EnhancedPoolManager] Error clearing type pool {kvp.Key.Name}: {ex.Message}");
                    }
                }
            }

            // 清理命名池
            foreach (var kvp in _namedPools)
            {
                if (kvp.Value is IDisposable pool)
                {
                    try
                    {
                        var clearMethod = pool.GetType().GetMethod("Clear");
                        clearMethod?.Invoke(pool, null);
                        clearedCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EnhancedPoolManager] Error clearing named pool '{kvp.Key}': {ex.Message}");
                    }
                }
            }

            Debug.Log($"[EnhancedPoolManager] Cleared {clearedCount} pools");
        }

        #endregion

        #region 统计和监控

        /// <summary>
        /// 获取所有对象池的状态信息
        /// </summary>
        /// <returns>状态信息字符串</returns>
        public static string GetAllPoolsStatus()
        {
            if (!_enableGlobalStatistics)
                return "Global statistics disabled";

            var sb = new StringBuilder();
            sb.AppendLine("=== Enhanced Pool Manager Status ===");
            sb.AppendLine($"Total Pools: {PoolCount}");
            sb.AppendLine($"Type Pools: {_typePools.Count}");
            sb.AppendLine($"Named Pools: {_namedPools.Count}");
            sb.AppendLine();

            // 类型池状态
            if (_typePools.Count > 0)
            {
                sb.AppendLine("Type Pools:");
                foreach (var kvp in _typePools)
                {
                    try
                    {
                        var statusMethod = kvp.Value.GetType().GetMethod("GetStatusInfo");
                        var status = statusMethod?.Invoke(kvp.Value, null) as string;
                        sb.AppendLine($"  {status}");
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"  {kvp.Key.Name}: Error getting status - {ex.Message}");
                    }
                }
                sb.AppendLine();
            }

            // 命名池状态
            if (_namedPools.Count > 0)
            {
                sb.AppendLine("Named Pools:");
                foreach (var kvp in _namedPools)
                {
                    try
                    {
                        var statusMethod = kvp.Value.GetType().GetMethod("GetStatusInfo");
                        var status = statusMethod?.Invoke(kvp.Value, null) as string;
                        sb.AppendLine($"  {kvp.Key}: {status}");
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"  {kvp.Key}: Error getting status - {ex.Message}");
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取全局统计摘要
        /// </summary>
        /// <returns>统计摘要</returns>
        public static string GetGlobalStatisticsSummary()
        {
            if (!_enableGlobalStatistics)
                return "Global statistics disabled";

            int totalAvailable = 0;
            int totalActive = 0;
            int totalCreated = 0;
            int totalReturned = 0;

            // 统计类型池
            foreach (var pool in _typePools.Values)
            {
                try
                {
                    var availableProperty = pool.GetType().GetProperty("AvailableCount");
                    var activeProperty = pool.GetType().GetProperty("ActiveCount");
                    var statisticsProperty = pool.GetType().GetProperty("Statistics");

                    if (availableProperty != null && activeProperty != null)
                    {
                        totalAvailable += (int)availableProperty.GetValue(pool);
                        totalActive += (int)activeProperty.GetValue(pool);
                    }

                    if (statisticsProperty != null)
                    {
                        var stats = statisticsProperty.GetValue(pool);
                        if (stats != null)
                        {
                            var createdProperty = stats.GetType().GetProperty("TotalCreatedCount");
                            var returnedProperty = stats.GetType().GetProperty("TotalReturnCount");
                            
                            if (createdProperty != null)
                                totalCreated += (int)createdProperty.GetValue(stats);
                            if (returnedProperty != null)
                                totalReturned += (int)returnedProperty.GetValue(stats);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EnhancedPoolManager] Error collecting statistics: {ex.Message}");
                }
            }

            // 统计命名池
            foreach (var pool in _namedPools.Values)
            {
                try
                {
                    var availableProperty = pool.GetType().GetProperty("AvailableCount");
                    var activeProperty = pool.GetType().GetProperty("ActiveCount");
                    var statisticsProperty = pool.GetType().GetProperty("Statistics");

                    if (availableProperty != null && activeProperty != null)
                    {
                        totalAvailable += (int)availableProperty.GetValue(pool);
                        totalActive += (int)activeProperty.GetValue(pool);
                    }

                    if (statisticsProperty != null)
                    {
                        var stats = statisticsProperty.GetValue(pool);
                        if (stats != null)
                        {
                            var createdProperty = stats.GetType().GetProperty("TotalCreatedCount");
                            var returnedProperty = stats.GetType().GetProperty("TotalReturnCount");
                            
                            if (createdProperty != null)
                                totalCreated += (int)createdProperty.GetValue(stats);
                            if (returnedProperty != null)
                                totalReturned += (int)returnedProperty.GetValue(stats);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EnhancedPoolManager] Error collecting statistics: {ex.Message}");
                }
            }

            return $"Global Summary: Pools:{PoolCount}, Available:{totalAvailable}, " +
                   $"Active:{totalActive}, Created:{totalCreated}, Returned:{totalReturned}";
        }

        #endregion

        #region 清理和销毁

        /// <summary>
        /// 清理所有对象池
        /// </summary>
        public static void Clear()
        {
            ClearAllPools();
            _typePools.Clear();
            _namedPools.Clear();
            _namedPoolTypes.Clear();
            
            Debug.Log("[EnhancedPoolManager] All pools cleared");
            
            // 为后期扩展预留清理逻辑
            OnClear();
        }

        /// <summary>
        /// 销毁管理器
        /// </summary>
        public static void Destroy()
        {
            // 销毁所有池
            foreach (var pool in _typePools.Values)
            {
                if (pool is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            foreach (var pool in _namedPools.Values)
            {
                if (pool is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            Clear();
            _isInitialized = false;
            _defaultConfig = null;
            
            Debug.Log("[EnhancedPoolManager] Manager destroyed");
            
            // 为后期扩展预留销毁逻辑
            OnDestroy();
        }

        #endregion

        #region 扩展接口预留

        /// <summary>
        /// 初始化时调用（为后期扩展预留）
        /// </summary>
        private static void OnInitialize()
        {
            // 后期可以在这里添加更多初始化逻辑
            // 例如：注册Unity生命周期回调、启动后台清理线程等
        }

        /// <summary>
        /// 清理时调用（为后期扩展预留）
        /// </summary>
        private static void OnClear()
        {
            // 后期可以在这里添加更多清理逻辑
        }

        /// <summary>
        /// 销毁时调用（为后期扩展预留）
        /// </summary>
        private static void OnDestroy()
        {
            // 后期可以在这里添加更多销毁逻辑
        }

        #endregion
    }
}