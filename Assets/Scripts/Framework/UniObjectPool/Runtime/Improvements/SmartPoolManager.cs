using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace UniFramework.ObjectPool.Improvements
{
    /// <summary>
    /// 智能对象池管理器
    /// 解决原版本的反射性能问题，提供更智能的管理功能
    /// </summary>
    public static class SmartPoolManager
    {
        /// <summary>
        /// 对象池包装器接口，避免反射调用
        /// </summary>
        private interface IPoolWrapper
        {
            void Cleanup(int count = -1);
            void Clear();
            void Dispose();
            object GetStatistics();
            Type ObjectType { get; }
            string PoolName { get; }
        }

        /// <summary>
        /// 泛型对象池包装器
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        private class PoolWrapper<T> : IPoolWrapper where T : class
        {
            private readonly UniObjectPool<T> _pool;
            private readonly string _poolName;

            public PoolWrapper(UniObjectPool<T> pool, string poolName)
            {
                _pool = pool;
                _poolName = poolName;
            }

            public void Cleanup(int count = -1) => _pool.Cleanup(count);
            public void Clear() => _pool.Clear();
            public void Dispose() => _pool.Dispose();
            public object GetStatistics() => _pool.Statistics;
            public Type ObjectType => typeof(T);
            public string PoolName => _poolName;
        }

        // 使用并发字典提高多线程性能
        private static readonly ConcurrentDictionary<Type, IPoolWrapper> _typePools = new ConcurrentDictionary<Type, IPoolWrapper>();
        private static readonly ConcurrentDictionary<string, IPoolWrapper> _namedPools = new ConcurrentDictionary<string, IPoolWrapper>();
        
        // 智能清理配置
        private static readonly Dictionary<string, PoolCleanupStrategy> _cleanupStrategies = new Dictionary<string, PoolCleanupStrategy>();
        
        private static bool _isInitialized = false;
        private static GameObject _driver = null;

        /// <summary>
        /// 对象池清理策略
        /// </summary>
        public class PoolCleanupStrategy
        {
            public float MemoryPressureThreshold { get; set; } = 0.8f; // 内存压力阈值
            public int MinIdleTime { get; set; } = 300; // 最小空闲时间（秒）
            public float IdleRatio { get; set; } = 0.7f; // 空闲对象比例阈值
            public bool EnableAdaptiveCleanup { get; set; } = true; // 启用自适应清理
        }

        /// <summary>
        /// 初始化智能对象池管理器
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            _driver = new GameObject("[SmartPoolManager]");
            _driver.AddComponent<SmartPoolManagerDriver>();
            UnityEngine.Object.DontDestroyOnLoad(_driver);
            
            _isInitialized = true;
            UniLogger.Log("SmartPoolManager 初始化完成");
        }

        /// <summary>
        /// 创建高性能对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="createFunc">对象创建函数</param>
        /// <param name="resetAction">对象重置动作</param>
        /// <param name="destroyAction">对象销毁动作</param>
        /// <param name="config">对象池配置</param>
        /// <param name="useHighPerformanceVersion">是否使用高性能版本</param>
        /// <returns>对象池实例</returns>
        public static UniObjectPool<T> CreatePool<T>(
            Func<T> createFunc,
            Action<T> resetAction = null,
            Action<T> destroyAction = null,
            PoolConfig config = null,
            bool useHighPerformanceVersion = false) where T : class
        {
            if (!_isInitialized) Initialize();

            Type type = typeof(T);
            
            // 检查是否已存在
            if (_typePools.ContainsKey(type))
            {
                UniLogger.Warning($"类型 {type.Name} 的对象池已存在");
                return GetPool<T>();
            }

            UniObjectPool<T> pool;
            
            if (useHighPerformanceVersion)
            {
                // 使用高性能版本
                pool = new HighPerformanceObjectPool<T>(createFunc, resetAction, destroyAction, config) as UniObjectPool<T>;
            }
            else
            {
                // 使用标准版本
                pool = new UniObjectPool<T>(createFunc, resetAction, destroyAction, config);
            }

            var wrapper = new PoolWrapper<T>(pool, type.Name);
            _typePools.TryAdd(type, wrapper);
            
            // 设置默认清理策略
            SetCleanupStrategy(type.Name, new PoolCleanupStrategy());
            
            UniLogger.Log($"创建智能对象池: {type.Name}");
            return pool;
        }

        /// <summary>
        /// 创建命名对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="poolName">对象池名称</param>
        /// <param name="createFunc">对象创建函数</param>
        /// <param name="resetAction">对象重置动作</param>
        /// <param name="destroyAction">对象销毁动作</param>
        /// <param name="config">对象池配置</param>
        /// <param name="useHighPerformanceVersion">是否使用高性能版本</param>
        /// <returns>对象池实例</returns>
        public static UniObjectPool<T> CreatePool<T>(
            string poolName,
            Func<T> createFunc,
            Action<T> resetAction = null,
            Action<T> destroyAction = null,
            PoolConfig config = null,
            bool useHighPerformanceVersion = false) where T : class
        {
            if (!_isInitialized) Initialize();

            if (string.IsNullOrEmpty(poolName))
                throw new ArgumentException("对象池名称不能为空", nameof(poolName));

            // 检查是否已存在
            if (_namedPools.ContainsKey(poolName))
            {
                UniLogger.Warning($"名称为 {poolName} 的对象池已存在");
                return GetPool<T>(poolName);
            }

            UniObjectPool<T> pool;
            
            if (useHighPerformanceVersion)
            {
                pool = new HighPerformanceObjectPool<T>(createFunc, resetAction, destroyAction, config) as UniObjectPool<T>;
            }
            else
            {
                pool = new UniObjectPool<T>(createFunc, resetAction, destroyAction, config);
            }

            var wrapper = new PoolWrapper<T>(pool, poolName);
            _namedPools.TryAdd(poolName, wrapper);
            
            // 设置默认清理策略
            SetCleanupStrategy(poolName, new PoolCleanupStrategy());
            
            UniLogger.Log($"创建智能命名对象池: {poolName}");
            return pool;
        }

        /// <summary>
        /// 获取类型对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>对象池实例</returns>
        public static UniObjectPool<T> GetPool<T>() where T : class
        {
            Type type = typeof(T);
            if (_typePools.TryGetValue(type, out IPoolWrapper wrapper))
            {
                return ((PoolWrapper<T>)wrapper)._pool;
            }
            return null;
        }

        /// <summary>
        /// 获取命名对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="poolName">对象池名称</param>
        /// <returns>对象池实例</returns>
        public static UniObjectPool<T> GetPool<T>(string poolName) where T : class
        {
            if (_namedPools.TryGetValue(poolName, out IPoolWrapper wrapper))
            {
                return ((PoolWrapper<T>)wrapper)._pool;
            }
            return null;
        }

        /// <summary>
        /// 设置对象池清理策略
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <param name="strategy">清理策略</param>
        public static void SetCleanupStrategy(string poolName, PoolCleanupStrategy strategy)
        {
            _cleanupStrategies[poolName] = strategy;
        }

        /// <summary>
        /// 智能清理所有对象池
        /// 基于内存压力和使用模式进行清理
        /// </summary>
        public static void SmartCleanupAll()
        {
            long totalMemory = GC.GetTotalMemory(false);
            long maxMemory = GC.MaxGeneration > 0 ? totalMemory * 2 : totalMemory; // 简化的内存压力计算
            float memoryPressure = (float)totalMemory / maxMemory;

            int cleanedPools = 0;

            // 清理类型对象池
            foreach (var kvp in _typePools)
            {
                if (ShouldCleanupPool(kvp.Value.PoolName, memoryPressure))
                {
                    kvp.Value.Cleanup();
                    cleanedPools++;
                }
            }

            // 清理命名对象池
            foreach (var kvp in _namedPools)
            {
                if (ShouldCleanupPool(kvp.Key, memoryPressure))
                {
                    kvp.Value.Cleanup();
                    cleanedPools++;
                }
            }

            if (cleanedPools > 0)
            {
                UniLogger.Log($"智能清理完成：清理了 {cleanedPools} 个对象池，内存压力: {memoryPressure:P2}");
            }
        }

        /// <summary>
        /// 判断是否应该清理指定对象池
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <param name="memoryPressure">当前内存压力</param>
        /// <returns>是否应该清理</returns>
        private static bool ShouldCleanupPool(string poolName, float memoryPressure)
        {
            if (!_cleanupStrategies.TryGetValue(poolName, out PoolCleanupStrategy strategy))
                return memoryPressure > 0.8f; // 默认策略

            if (!strategy.EnableAdaptiveCleanup)
                return false;

            // 基于内存压力判断
            if (memoryPressure > strategy.MemoryPressureThreshold)
                return true;

            // 可以添加更多智能判断逻辑
            // 例如：基于对象池的空闲时间、使用频率等

            return false;
        }

        /// <summary>
        /// 获取所有对象池的性能报告
        /// </summary>
        /// <returns>性能报告字符串</returns>
        public static string GetPerformanceReport()
        {
            var report = "=== 智能对象池管理器性能报告 ===\n";
            report += $"总对象池数量: {_typePools.Count + _namedPools.Count}\n";
            report += $"当前内存使用: {GC.GetTotalMemory(false) / 1024}KB\n\n";

            report += "类型对象池:\n";
            foreach (var kvp in _typePools)
            {
                var statistics = kvp.Value.GetStatistics();
                report += $"  {kvp.Value.PoolName}: {statistics}\n";
            }

            report += "\n命名对象池:\n";
            foreach (var kvp in _namedPools)
            {
                var statistics = kvp.Value.GetStatistics();
                report += $"  {kvp.Key}: {statistics}\n";
            }

            return report;
        }

        /// <summary>
        /// 销毁管理器
        /// </summary>
        public static void Destroy()
        {
            if (!_isInitialized) return;

            // 释放所有对象池
            foreach (var pool in _typePools.Values)
            {
                pool.Dispose();
            }
            _typePools.Clear();

            foreach (var pool in _namedPools.Values)
            {
                pool.Dispose();
            }
            _namedPools.Clear();

            _cleanupStrategies.Clear();

            if (_driver != null)
            {
                UnityEngine.Object.Destroy(_driver);
                _driver = null;
            }

            _isInitialized = false;
            UniLogger.Log("SmartPoolManager 已销毁");
        }
    }

    /// <summary>
    /// 智能对象池管理器驱动器
    /// </summary>
    public class SmartPoolManagerDriver : MonoBehaviour
    {
        private float _lastCleanupTime;
        private const float CLEANUP_INTERVAL = 30f; // 30秒清理一次

        private void Update()
        {
            if (Time.realtimeSinceStartup - _lastCleanupTime >= CLEANUP_INTERVAL)
            {
                SmartPoolManager.SmartCleanupAll();
                _lastCleanupTime = Time.realtimeSinceStartup;
            }
        }

        private void OnDestroy()
        {
            SmartPoolManager.Destroy();
        }
    }
}