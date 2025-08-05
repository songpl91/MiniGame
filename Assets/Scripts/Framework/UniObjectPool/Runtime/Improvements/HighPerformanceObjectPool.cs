using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UniFramework.ObjectPool.Improvements
{
    /// <summary>
    /// 高性能对象池实现
    /// 解决原版本的性能瓶颈问题
    /// </summary>
    /// <typeparam name="T">池化对象类型</typeparam>
    public class HighPerformanceObjectPool<T> : IDisposable where T : class
    {
        // 使用无锁数据结构减少锁竞争
        private readonly ConcurrentStack<T> _pool;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _resetAction;
        private readonly Action<T> _destroyAction;
        private readonly PoolConfig _config;
        
        // 使用原子操作替代锁保护的计数器
        private int _activeCount;
        private int _totalCreated;
        private int _totalReturned;
        
        // 延迟统计更新，减少频繁操作
        private PoolStatistics _statistics;
        private DateTime _lastStatisticsUpdate;
        private readonly TimeSpan _statisticsUpdateInterval = TimeSpan.FromSeconds(1);
        
        private volatile bool _isDisposed;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="createFunc">对象创建函数</param>
        /// <param name="resetAction">对象重置动作</param>
        /// <param name="destroyAction">对象销毁动作</param>
        /// <param name="config">对象池配置</param>
        public HighPerformanceObjectPool(
            Func<T> createFunc,
            Action<T> resetAction = null,
            Action<T> destroyAction = null,
            PoolConfig config = null)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _resetAction = resetAction;
            _destroyAction = destroyAction;
            _config = config ?? PoolConfig.CreateDefault();
            
            _pool = new ConcurrentStack<T>();
            _statistics = new PoolStatistics();
            _lastStatisticsUpdate = DateTime.Now;
            
            // 预热对象池
            PrewarmPool();
        }

        /// <summary>
        /// 获取对象（高性能版本）
        /// </summary>
        /// <returns>池化对象实例</returns>
        public T Get()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(HighPerformanceObjectPool<T>));

            T item;
            
            // 尝试从池中获取对象（无锁操作）
            if (_pool.TryPop(out item))
            {
                // 缓存命中
                Interlocked.Increment(ref _activeCount);
                
                // 延迟更新统计信息
                if (_config.EnableStatistics)
                {
                    UpdateStatisticsIfNeeded();
                }
            }
            else
            {
                // 缓存未命中，创建新对象
                item = _createFunc();
                Interlocked.Increment(ref _activeCount);
                Interlocked.Increment(ref _totalCreated);
            }

            // 调用重置逻辑
            if (item is IPoolable poolable)
            {
                poolable.OnSpawn();
            }
            _resetAction?.Invoke(item);

            return item;
        }

        /// <summary>
        /// 归还对象（高性能版本）
        /// </summary>
        /// <param name="item">要归还的对象</param>
        /// <returns>是否成功归还</returns>
        public bool Return(T item)
        {
            if (_isDisposed || item == null)
                return false;

            // 检查池容量（原子操作）
            if (_pool.Count >= _config.MaxCapacity)
            {
                // 池已满，销毁对象
                DestroyObject(item);
                Interlocked.Decrement(ref _activeCount);
                return true;
            }

            // 调用清理逻辑
            if (item is IPoolable poolable)
            {
                poolable.OnDespawn();
            }

            // 归还到池（无锁操作）
            _pool.Push(item);
            Interlocked.Decrement(ref _activeCount);
            Interlocked.Increment(ref _totalReturned);

            return true;
        }

        /// <summary>
        /// 延迟更新统计信息，减少频繁操作开销
        /// </summary>
        private void UpdateStatisticsIfNeeded()
        {
            var now = DateTime.Now;
            if (now - _lastStatisticsUpdate > _statisticsUpdateInterval)
            {
                _statistics.ActiveCount = _activeCount;
                _statistics.AvailableCount = _pool.Count;
                _statistics.TotalCreatedCount = _totalCreated;
                _statistics.TotalReturnCount = _totalReturned;
                _lastStatisticsUpdate = now;
            }
        }

        /// <summary>
        /// 预热对象池
        /// </summary>
        private void PrewarmPool()
        {
            if (_config.InitialCapacity > 0)
            {
                var items = new T[_config.InitialCapacity];
                for (int i = 0; i < _config.InitialCapacity; i++)
                {
                    items[i] = _createFunc();
                    if (items[i] is IPoolable poolable)
                    {
                        poolable.OnDespawn();
                    }
                }
                
                // 批量推入池中
                _pool.PushRange(items);
                Interlocked.Add(ref _totalCreated, _config.InitialCapacity);
            }
        }

        /// <summary>
        /// 销毁对象
        /// </summary>
        /// <param name="item">要销毁的对象</param>
        private void DestroyObject(T item)
        {
            if (item == null) return;
            _destroyAction?.Invoke(item);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            
            _isDisposed = true;
            
            // 清空池中所有对象
            while (_pool.TryPop(out T item))
            {
                DestroyObject(item);
            }
        }

        /// <summary>
        /// 获取当前统计信息
        /// </summary>
        public PoolStatistics Statistics
        {
            get
            {
                if (_config.EnableStatistics)
                {
                    UpdateStatisticsIfNeeded();
                }
                return _statistics;
            }
        }

        /// <summary>
        /// 当前活跃对象数量
        /// </summary>
        public int ActiveCount => _activeCount;

        /// <summary>
        /// 池中可用对象数量
        /// </summary>
        public int AvailableCount => _pool.Count;
    }
}