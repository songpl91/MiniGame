using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.ObjectPool
{
    /// <summary>
    /// 对象池管理器
    /// 提供统一的对象池管理接口
    /// </summary>
    public static class PoolManager
    {
        private static readonly Dictionary<Type, object> _pools = new Dictionary<Type, object>();
        private static readonly Dictionary<string, object> _namedPools = new Dictionary<string, object>();
        private static readonly object _lockObject = new object();
        private static bool _isInitialized = false;
        private static GameObject _driver = null;

        /// <summary>
        /// 已注册的对象池数量
        /// </summary>
        public static int PoolCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _pools.Count + _namedPools.Count;
                }
            }
        }

        /// <summary>
        /// 初始化对象池管理器
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
            {
                UniLogger.Warning("PoolManager 已经初始化过了");
                return;
            }

            lock (_lockObject)
            {
                if (!_isInitialized)
                {
                    // 创建驱动器
                    _driver = new GameObject("[PoolManager]");
                    _driver.AddComponent<PoolManagerDriver>();
                    UnityEngine.Object.DontDestroyOnLoad(_driver);
                    
                    _isInitialized = true;
                    UniLogger.Log("PoolManager 初始化完成");
                }
            }
        }

        /// <summary>
        /// 销毁对象池管理器
        /// </summary>
        public static void Destroy()
        {
            if (!_isInitialized)
                return;

            lock (_lockObject)
            {
                // 释放所有对象池
                foreach (var pool in _pools.Values)
                {
                    if (pool is IDisposable disposable)
                        disposable.Dispose();
                }
                _pools.Clear();

                foreach (var pool in _namedPools.Values)
                {
                    if (pool is IDisposable disposable)
                        disposable.Dispose();
                }
                _namedPools.Clear();

                // 清理对象池注册器
                PoolRegistry.Clear();

                // 销毁驱动器
                if (_driver != null)
                {
                    UnityEngine.Object.Destroy(_driver);
                    _driver = null;
                }

                _isInitialized = false;
                UniLogger.Log("PoolManager 已销毁");
            }
        }

        /// <summary>
        /// 创建类型对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="createFunc">对象创建函数</param>
        /// <param name="resetAction">对象重置动作</param>
        /// <param name="destroyAction">对象销毁动作</param>
        /// <param name="config">对象池配置</param>
        /// <returns>对象池实例</returns>
        public static UniObjectPool<T> CreatePool<T>(
            Func<T> createFunc,
            Action<T> resetAction = null,
            Action<T> destroyAction = null,
            PoolConfig config = null) where T : class
        {
            if (!_isInitialized)
                Initialize();

            Type type = typeof(T);
            lock (_lockObject)
            {
                if (_pools.ContainsKey(type))
                {
                    UniLogger.Warning($"类型 {type.Name} 的对象池已存在");
                    return _pools[type] as UniObjectPool<T>;
                }

                var pool = new UniObjectPool<T>(createFunc, resetAction, destroyAction, config);
                _pools[type] = pool;
                UniLogger.Log($"创建类型对象池: {type.Name}");
                return pool;
            }
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
        /// <returns>对象池实例</returns>
        public static UniObjectPool<T> CreatePool<T>(
            string poolName,
            Func<T> createFunc,
            Action<T> resetAction = null,
            Action<T> destroyAction = null,
            PoolConfig config = null) where T : class
        {
            if (!_isInitialized)
                Initialize();

            if (string.IsNullOrEmpty(poolName))
                throw new ArgumentException("对象池名称不能为空", nameof(poolName));

            lock (_lockObject)
            {
                if (_namedPools.ContainsKey(poolName))
                {
                    UniLogger.Warning($"名称为 {poolName} 的对象池已存在");
                    return _namedPools[poolName] as UniObjectPool<T>;
                }

                var pool = new UniObjectPool<T>(createFunc, resetAction, destroyAction, config);
                _namedPools[poolName] = pool;
                UniLogger.Log($"创建命名对象池: {poolName}");
                return pool;
            }
        }

        /// <summary>
        /// 获取类型对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>对象池实例，如果不存在则返回 null</returns>
        public static UniObjectPool<T> GetPool<T>() where T : class
        {
            Type type = typeof(T);
            lock (_lockObject)
            {
                return _pools.TryGetValue(type, out object pool) ? pool as UniObjectPool<T> : null;
            }
        }

        /// <summary>
        /// 获取命名对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="poolName">对象池名称</param>
        /// <returns>对象池实例，如果不存在则返回 null</returns>
        public static UniObjectPool<T> GetPool<T>(string poolName) where T : class
        {
            if (string.IsNullOrEmpty(poolName))
                return null;

            lock (_lockObject)
            {
                return _namedPools.TryGetValue(poolName, out object pool) ? pool as UniObjectPool<T> : null;
            }
        }

        /// <summary>
        /// 检查类型对象池是否存在
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>是否存在</returns>
        public static bool HasPool<T>() where T : class
        {
            Type type = typeof(T);
            lock (_lockObject)
            {
                return _pools.ContainsKey(type);
            }
        }

        /// <summary>
        /// 检查命名对象池是否存在
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <returns>是否存在</returns>
        public static bool HasPool(string poolName)
        {
            if (string.IsNullOrEmpty(poolName))
                return false;

            lock (_lockObject)
            {
                return _namedPools.ContainsKey(poolName);
            }
        }

        /// <summary>
        /// 移除类型对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>是否成功移除</returns>
        public static bool RemovePool<T>() where T : class
        {
            Type type = typeof(T);
            lock (_lockObject)
            {
                if (_pools.TryGetValue(type, out object pool))
                {
                    if (pool is IDisposable disposable)
                        disposable.Dispose();
                    
                    _pools.Remove(type);
                    UniLogger.Log($"移除类型对象池: {type.Name}");
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 移除命名对象池
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <returns>是否成功移除</returns>
        public static bool RemovePool(string poolName)
        {
            if (string.IsNullOrEmpty(poolName))
                return false;

            lock (_lockObject)
            {
                if (_namedPools.TryGetValue(poolName, out object pool))
                {
                    if (pool is IDisposable disposable)
                        disposable.Dispose();
                    
                    _namedPools.Remove(poolName);
                    
                    // 从注册器中移除
                    PoolRegistry.UnregisterPool(poolName);
                    
                    UniLogger.Log($"移除命名对象池: {poolName}");
                    return true;
                }
                return false;
            }
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
                UniLogger.Error($"类型 {typeof(T).Name} 的对象池不存在");
                return null;
            }
            return pool.Get();
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
                UniLogger.Error($"名称为 {poolName} 的对象池不存在");
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
                UniLogger.Error($"类型 {typeof(T).Name} 的对象池不存在");
                return false;
            }
            return pool.Return(item);
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
                UniLogger.Error($"名称为 {poolName} 的对象池不存在");
                return false;
            }
            return pool.Return(item);
        }

        /// <summary>
        /// 清理所有对象池
        /// </summary>
        public static void CleanupAll()
        {
            lock (_lockObject)
            {
                foreach (var pool in _pools.Values)
                {
                    if (pool is UniObjectPool<object> objectPool)
                    {
                        // 使用反射调用 Cleanup 方法
                        var cleanupMethod = pool.GetType().GetMethod("Cleanup");
                        cleanupMethod?.Invoke(pool, new object[] { -1 });
                    }
                }

                foreach (var pool in _namedPools.Values)
                {
                    if (pool is UniObjectPool<object> objectPool)
                    {
                        // 使用反射调用 Cleanup 方法
                        var cleanupMethod = pool.GetType().GetMethod("Cleanup");
                        cleanupMethod?.Invoke(pool, new object[] { -1 });
                    }
                }
            }
        }

        /// <summary>
        /// 获取所有对象池的统计信息
        /// </summary>
        /// <returns>统计信息字符串</returns>
        public static string GetAllStatistics()
        {
            var result = "=== 对象池管理器统计信息 ===\n";
            result += $"总对象池数量: {PoolCount}\n\n";

            lock (_lockObject)
            {
                result += "类型对象池:\n";
                foreach (var kvp in _pools)
                {
                    var type = kvp.Key;
                    var pool = kvp.Value;
                    var statisticsProperty = pool.GetType().GetProperty("Statistics");
                    if (statisticsProperty != null)
                    {
                        var statistics = statisticsProperty.GetValue(pool);
                        result += $"  {type.Name}: {statistics}\n";
                    }
                }

                result += "\n命名对象池:\n";
                foreach (var kvp in _namedPools)
                {
                    var name = kvp.Key;
                    var pool = kvp.Value;
                    var statisticsProperty = pool.GetType().GetProperty("Statistics");
                    if (statisticsProperty != null)
                    {
                        var statistics = statisticsProperty.GetValue(pool);
                        result += $"  {name}: {statistics}\n";
                    }
                }
            }

            return result;
        }
    }
}