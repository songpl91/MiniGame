using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.ObjectPool
{
    /// <summary>
    /// 泛型对象池核心类
    /// 提供高效的对象池化管理功能
    /// </summary>
    /// <typeparam name="T">池化对象类型</typeparam>
    public class UniObjectPool<T> : IDisposable where T : class
    {
        private readonly Stack<T> _pool;
        private readonly HashSet<T> _activeObjects;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _resetAction;
        private readonly Action<T> _destroyAction;
        private readonly PoolConfig _config;
        private readonly PoolStatistics _statistics;
        private readonly object _lockObject;
        
        private float _lastCleanupTime;
        private bool _isDisposed;

        /// <summary>
        /// 对象池配置
        /// </summary>
        public PoolConfig Config => _config;

        /// <summary>
        /// 统计信息
        /// </summary>
        public PoolStatistics Statistics => _statistics;

        /// <summary>
        /// 池中可用对象数量
        /// </summary>
        public int AvailableCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _pool.Count;
                }
            }
        }

        /// <summary>
        /// 当前活跃对象数量
        /// </summary>
        public int ActiveCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _activeObjects.Count;
                }
            }
        }

        /// <summary>
        /// 对象池是否已被释放
        /// </summary>
        public bool IsDisposed => _isDisposed;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="createFunc">对象创建函数</param>
        /// <param name="resetAction">对象重置动作（可选）</param>
        /// <param name="destroyAction">对象销毁动作（可选）</param>
        /// <param name="config">对象池配置（可选）</param>
        public UniObjectPool(
            Func<T> createFunc,
            Action<T> resetAction = null,
            Action<T> destroyAction = null,
            PoolConfig config = null)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _resetAction = resetAction;
            _destroyAction = destroyAction;
            _config = config ?? PoolConfig.CreateDefault();
            _statistics = new PoolStatistics();
            _lockObject = new object();

            _pool = new Stack<T>(_config.MaxCapacity);
            _activeObjects = new HashSet<T>();
            _lastCleanupTime = Time.realtimeSinceStartup;

            // 预创建初始对象
            PrewarmPool();
        }

        /// <summary>
        /// 从对象池获取对象
        /// </summary>
        /// <returns>对象实例</returns>
        public T Get()
        {
            if (_isDisposed)
                return null;

            T item;
            bool fromPool = false;
            
            lock (_lockObject)
            {
                // 尝试从池中获取对象
                if (_pool.Count > 0)
                {
                    item = _pool.Pop();
                    fromPool = true;
                    
                    if (_config.EnableStatistics)
                    {
                        _statistics.CacheHitCount++;
                    }
                }
                else
                {
                    // 池为空，创建新对象
                    item = _createFunc();
                    fromPool = false;
                    
                    if (_config.EnableStatistics)
                    {
                        _statistics.TotalCreatedCount++;
                        _statistics.CacheMissCount++;
                    }
                }

                // 添加到活跃对象集合
                _activeObjects.Add(item);

                // 更新统计信息
                if (_config.EnableStatistics)
                {
                    _statistics.TotalGetCount++;
                    _statistics.AvailableCount = _pool.Count;
                    _statistics.ActiveCount = _activeObjects.Count;
                }
            }

            // 调用重置动作
            _resetAction?.Invoke(item);

            // 调用对象的 OnSpawn 方法
            if (item is IPoolable poolable)
            {
                poolable.OnSpawn();
            }

            return item;
        }



        /// <summary>
        /// 将对象归还到对象池
        /// </summary>
        /// <param name="item">要归还的对象</param>
        /// <returns>是否成功归还</returns>
        public bool Return(T item)
        {
            if (_isDisposed)
                return false;

            if (item == null)
                return false;

            lock (_lockObject)
            {
                // 验证对象是否来自此池
                if (_config.ValidateOnReturn && !_activeObjects.Contains(item))
                {
                    UniLogger.Warning($"尝试归还不属于此池的对象: {typeof(T).Name}");
                    return false;
                }

                _activeObjects.Remove(item);

                // 检查池是否已满
                if (_pool.Count >= _config.MaxCapacity)
                {
                    // 池已满，销毁对象
                    DestroyObject(item);
                    if (_config.EnableStatistics)
                    {
                        _statistics.ActiveCount = _activeObjects.Count;
                    }
                    return true;
                }

                // 调用对象的 OnDespawn 方法
                if (item is IPoolable poolable)
                {
                    poolable.OnDespawn();
                }

                _pool.Push(item);
                if (_config.EnableStatistics)
                {
                    _statistics.TotalReturnCount++;
                    _statistics.AvailableCount = _pool.Count;
                    _statistics.ActiveCount = _activeObjects.Count;
                }
            }

            // 检查是否需要自动清理
            CheckAutoCleanup();

            return true;
        }

        /// <summary>
        /// 预热对象池（预创建指定数量的对象）
        /// </summary>
        /// <param name="count">预创建对象数量，如果不指定则使用配置中的初始容量</param>
        public void Prewarm(int count = -1)
        {
            if (_isDisposed)
                return;

            if (count < 0)
                count = _config.InitialCapacity;

            lock (_lockObject)
            {
                for (int i = 0; i < count && _pool.Count < _config.MaxCapacity; i++)
                {
                    T item = _createFunc();
                    if (item is IPoolable poolable)
                    {
                        poolable.OnDespawn();
                    }
                    _pool.Push(item);

                    if (_config.EnableStatistics)
                    {
                        _statistics.TotalCreatedCount++;
                        _statistics.AvailableCount = _pool.Count;
                    }
                }
            }
        }

        /// <summary>
        /// 清理对象池中的部分对象
        /// </summary>
        /// <param name="count">要清理的对象数量，如果不指定则使用配置中的清理数量</param>
        public void Cleanup(int count = -1)
        {
            if (_isDisposed)
                return;

            if (count < 0)
                count = _config.CleanupCount;

            lock (_lockObject)
            {
                int cleanedCount = 0;
                while (_pool.Count > 0 && cleanedCount < count)
                {
                    T item = _pool.Pop();
                    DestroyObject(item);
                    cleanedCount++;
                }

                if (_config.EnableStatistics)
                {
                    _statistics.AvailableCount = _pool.Count;
                    if (cleanedCount > 0)
                    {
                        _statistics.CleanupExecutedCount++;
                        _statistics.LastCleanupTime = DateTime.Now;
                    }
                }
            }

            _lastCleanupTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// 清空对象池中的所有对象
        /// </summary>
        public void Clear()
        {
            if (_isDisposed)
                return;

            lock (_lockObject)
            {
                // 销毁池中的所有对象
                while (_pool.Count > 0)
                {
                    T item = _pool.Pop();
                    DestroyObject(item);
                }

                // 销毁所有活跃对象
                foreach (T item in _activeObjects)
                {
                    DestroyObject(item);
                }
                _activeObjects.Clear();

                if (_config.EnableStatistics)
                {
                    _statistics.AvailableCount = 0;
                    _statistics.ActiveCount = 0;
                }
            }
        }

        /// <summary>
        /// 释放对象池资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            Clear();
            _isDisposed = true;
        }

        /// <summary>
        /// 预热对象池
        /// </summary>
        private void PrewarmPool()
        {
            if (_config.InitialCapacity > 0)
            {
                Prewarm(_config.InitialCapacity);
            }
        }

        /// <summary>
        /// 检查是否需要自动清理
        /// </summary>
        private void CheckAutoCleanup()
        {
            if (!_config.EnableAutoCleanup)
                return;

            float currentTime = Time.realtimeSinceStartup;
            if (currentTime - _lastCleanupTime >= _config.CleanupInterval)
            {
                lock (_lockObject)
                {
                    if (_pool.Count > _config.CleanupThreshold)
                    {
                        Cleanup();
                    }
                }
            }
        }

        /// <summary>
        /// 销毁对象
        /// </summary>
        /// <param name="item">要销毁的对象</param>
        private void DestroyObject(T item)
        {
            if (item == null)
                return;

            _destroyAction?.Invoke(item);

            if (_config.EnableStatistics)
            {
                _statistics.TotalDestroyedCount++;
            }
        }
    }
}