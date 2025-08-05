using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.ObjectPool.Improvements
{
    /// <summary>
    /// 内存优化的对象池实现
    /// 移除不必要的内存开销，适用于内存敏感场景
    /// </summary>
    /// <typeparam name="T">池化对象类型</typeparam>
    public class MemoryOptimizedPool<T> : IDisposable where T : class
    {
        // 使用简单的Stack，避免HashSet的额外内存开销
        private readonly Stack<T> _pool;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _resetAction;
        private readonly Action<T> _destroyAction;
        private readonly PoolConfig _config;
        
        // 简化的计数器，只保留必要信息
        private int _activeCount;
        private bool _isDisposed;
        
        // 对象验证机制（可选）
        private readonly HashSet<T> _trackedObjects;
        private readonly bool _enableObjectTracking;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="createFunc">对象创建函数</param>
        /// <param name="resetAction">对象重置动作</param>
        /// <param name="destroyAction">对象销毁动作</param>
        /// <param name="config">对象池配置</param>
        /// <param name="enableObjectTracking">是否启用对象追踪（用于验证）</param>
        public MemoryOptimizedPool(
            Func<T> createFunc,
            Action<T> resetAction = null,
            Action<T> destroyAction = null,
            PoolConfig config = null,
            bool enableObjectTracking = false)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _resetAction = resetAction;
            _destroyAction = destroyAction;
            _config = config ?? PoolConfig.CreateDefault();
            _enableObjectTracking = enableObjectTracking;
            
            _pool = new Stack<T>(_config.MaxCapacity);
            
            // 只在需要时创建追踪集合
            if (_enableObjectTracking)
            {
                _trackedObjects = new HashSet<T>();
            }
            
            PrewarmPool();
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <returns>池化对象实例</returns>
        public T Get()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(MemoryOptimizedPool<T>));

            T item;
            
            if (_pool.Count > 0)
            {
                item = _pool.Pop();
            }
            else
            {
                item = _createFunc();
            }
            
            _activeCount++;
            
            // 可选的对象追踪
            if (_enableObjectTracking)
            {
                _trackedObjects.Add(item);
            }

            // 调用生命周期方法
            if (item is IPoolable poolable)
            {
                poolable.OnSpawn();
            }
            _resetAction?.Invoke(item);

            return item;
        }

        /// <summary>
        /// 归还对象
        /// </summary>
        /// <param name="item">要归还的对象</param>
        /// <returns>是否成功归还</returns>
        public bool Return(T item)
        {
            if (_isDisposed || item == null)
                return false;

            // 对象验证（如果启用）
            if (_enableObjectTracking)
            {
                if (!_trackedObjects.Remove(item))
                {
                    UniLogger.Warning($"尝试归还不属于此池的对象: {typeof(T).Name}");
                    return false;
                }
            }

            _activeCount--;

            // 检查池容量
            if (_pool.Count >= _config.MaxCapacity)
            {
                DestroyObject(item);
                return true;
            }

            // 调用生命周期方法
            if (item is IPoolable poolable)
            {
                poolable.OnDespawn();
            }

            _pool.Push(item);
            return true;
        }

        /// <summary>
        /// 智能清理：基于内存压力和使用模式
        /// </summary>
        public void SmartCleanup()
        {
            if (_isDisposed) return;

            // 获取当前内存使用情况
            long memoryBefore = GC.GetTotalMemory(false);
            
            // 计算建议清理数量
            int suggestedCleanupCount = CalculateCleanupCount();
            
            if (suggestedCleanupCount > 0)
            {
                int cleanedCount = 0;
                while (_pool.Count > 0 && cleanedCount < suggestedCleanupCount)
                {
                    T item = _pool.Pop();
                    DestroyObject(item);
                    cleanedCount++;
                }
                
                // 强制垃圾回收（可选）
                if (cleanedCount > 10)
                {
                    GC.Collect();
                    long memoryAfter = GC.GetTotalMemory(true);
                    UniLogger.Log($"智能清理完成：清理了 {cleanedCount} 个对象，释放内存 {(memoryBefore - memoryAfter) / 1024} KB");
                }
            }
        }

        /// <summary>
        /// 计算建议的清理数量
        /// </summary>
        /// <returns>建议清理的对象数量</returns>
        private int CalculateCleanupCount()
        {
            int availableCount = _pool.Count;
            
            // 如果池中对象很少，不清理
            if (availableCount <= _config.InitialCapacity)
                return 0;
            
            // 基于活跃对象比例决定清理数量
            float activeRatio = _activeCount > 0 ? (float)_activeCount / (availableCount + _activeCount) : 0f;
            
            if (activeRatio < 0.1f) // 活跃对象很少，可以大量清理
            {
                return availableCount / 2;
            }
            else if (activeRatio < 0.3f) // 活跃对象较少，适度清理
            {
                return availableCount / 4;
            }
            else if (activeRatio < 0.5f) // 活跃对象适中，少量清理
            {
                return Math.Min(availableCount / 8, _config.CleanupCount);
            }
            
            return 0; // 活跃对象很多，不清理
        }

        /// <summary>
        /// 预热对象池
        /// </summary>
        private void PrewarmPool()
        {
            for (int i = 0; i < _config.InitialCapacity; i++)
            {
                T item = _createFunc();
                if (item is IPoolable poolable)
                {
                    poolable.OnDespawn();
                }
                _pool.Push(item);
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
            while (_pool.Count > 0)
            {
                T item = _pool.Pop();
                DestroyObject(item);
            }
            
            // 清空追踪对象（如果启用）
            if (_enableObjectTracking)
            {
                foreach (T item in _trackedObjects)
                {
                    DestroyObject(item);
                }
                _trackedObjects.Clear();
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

        /// <summary>
        /// 获取内存使用情况
        /// </summary>
        /// <returns>内存使用信息字符串</returns>
        public string GetMemoryInfo()
        {
            long totalMemory = GC.GetTotalMemory(false);
            return $"池状态: 可用={AvailableCount}, 活跃={ActiveCount}, 总内存={totalMemory / 1024}KB";
        }
    }
}