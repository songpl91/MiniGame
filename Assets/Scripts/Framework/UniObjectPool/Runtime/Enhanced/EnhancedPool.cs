using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.ObjectPool.Enhanced
{
    /// <summary>
    /// 增强版对象池核心类
    /// 在极简版基础上增加统计、验证、配置等功能
    /// 为后期升级到完整版UniObjectPool做准备
    /// </summary>
    /// <typeparam name="T">池化对象类型</typeparam>
    public class EnhancedPool<T> : IDisposable where T : class
    {
        #region 私有字段

        private readonly Stack<T> _pool;
        private readonly HashSet<T> _activeObjects;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _resetAction;
        private readonly Action<T> _destroyAction;
        private readonly EnhancedPoolConfig _config;
        private readonly EnhancedPoolStatistics _statistics;
        
        private bool _isDisposed;

        #endregion

        #region 属性

        /// <summary>
        /// 对象池配置
        /// </summary>
        public EnhancedPoolConfig Config => _config;

        /// <summary>
        /// 统计信息
        /// </summary>
        public EnhancedPoolStatistics Statistics => _statistics;

        /// <summary>
        /// 池中可用对象数量
        /// </summary>
        public int AvailableCount => _pool.Count;

        /// <summary>
        /// 当前活跃对象数量
        /// </summary>
        public int ActiveCount => _activeObjects.Count;

        /// <summary>
        /// 最大容量
        /// </summary>
        public int MaxCapacity => _config.MaxCapacity;

        /// <summary>
        /// 对象池是否已被释放
        /// </summary>
        public bool IsDisposed => _isDisposed;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="createFunc">对象创建函数</param>
        /// <param name="resetAction">对象重置动作（可选）</param>
        /// <param name="destroyAction">对象销毁动作（可选）</param>
        /// <param name="config">对象池配置（可选）</param>
        public EnhancedPool(
            Func<T> createFunc,
            Action<T> resetAction = null,
            Action<T> destroyAction = null,
            EnhancedPoolConfig config = null)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _resetAction = resetAction;
            _destroyAction = destroyAction;
            _config = config ?? EnhancedPoolConfig.CreateDefault();
            
            // 验证配置
            if (!_config.IsValid())
            {
                throw new ArgumentException("无效的对象池配置", nameof(config));
            }

            _statistics = _config.EnableStatistics ? new EnhancedPoolStatistics() : null;
            _pool = new Stack<T>(_config.MaxCapacity);
            _activeObjects = new HashSet<T>();

            // 预热对象池
            if (_config.InitialCapacity > 0)
            {
                Prewarm(_config.InitialCapacity);
            }

            LogDebug($"Enhanced Pool created: {_config.GetDescription()}");
        }

        #endregion

        #region 核心方法

        /// <summary>
        /// 从对象池获取对象
        /// </summary>
        /// <returns>池化对象实例</returns>
        public T Get()
        {
            ThrowIfDisposed();

            T item;
            bool fromPool = false;

            if (_pool.Count > 0)
            {
                // 从池中获取对象
                item = _pool.Pop();
                fromPool = true;
                LogDebug($"Get object from pool, remaining: {_pool.Count}");
            }
            else
            {
                // 创建新对象
                item = _createFunc();
                LogDebug($"Create new object, type: {typeof(T).Name}");
                
                // 调用创建回调
                if (item is IEnhancedPoolable enhancedPoolable)
                {
                    enhancedPoolable.OnCreate();
                }
            }

            // 跟踪活跃对象
            _activeObjects.Add(item);

            // 更新统计信息
            if (_statistics != null)
            {
                _statistics.RecordGet(fromPool);
                _statistics.ActiveCount = _activeObjects.Count;
                _statistics.AvailableCount = _pool.Count;
            }

            // 调用生命周期回调
            CallOnSpawn(item);

            // 调用重置动作
            _resetAction?.Invoke(item);

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
            {
                LogDebug("Cannot return to disposed pool");
                return false;
            }

            if (item == null)
            {
                LogDebug("Cannot return null object");
                return false;
            }

            bool successful = true;

            // 验证对象
            if (_config.ValidateOnReturn)
            {
                if (!_activeObjects.Contains(item))
                {
                    LogDebug($"Validation failed: object not from this pool");
                    if (_statistics != null)
                    {
                        _statistics.RecordReturn(false);
                    }
                    return false;
                }

                // 增强验证
                if (item is IEnhancedPoolable enhancedPoolable && !enhancedPoolable.CanReturn())
                {
                    LogDebug($"Validation failed: object cannot be returned");
                    if (_statistics != null)
                    {
                        _statistics.RecordReturn(false);
                    }
                    return false;
                }
            }

            // 从活跃对象中移除
            _activeObjects.Remove(item);

            // 检查池是否已满
            if (_pool.Count >= _config.MaxCapacity)
            {
                // 池已满，销毁对象
                DestroyObject(item);
                if (_statistics != null)
                {
                    _statistics.RecordDiscard();
                    _statistics.ActiveCount = _activeObjects.Count;
                }
                LogDebug($"Pool full, object discarded. Pool size: {_pool.Count}");
                return true;
            }

            // 调用生命周期回调
            CallOnDespawn(item);

            // 归还到池中
            _pool.Push(item);

            // 更新统计信息
            if (_statistics != null)
            {
                _statistics.RecordReturn(successful);
                _statistics.ActiveCount = _activeObjects.Count;
                _statistics.AvailableCount = _pool.Count;
            }

            LogDebug($"Object returned to pool, pool size: {_pool.Count}");
            return true;
        }

        /// <summary>
        /// 预热对象池（预创建指定数量的对象）
        /// </summary>
        /// <param name="count">预创建对象数量</param>
        public void Prewarm(int count)
        {
            ThrowIfDisposed();

            if (count <= 0)
                return;

            int actualCount = 0;
            for (int i = 0; i < count && _pool.Count < _config.MaxCapacity; i++)
            {
                T item = _createFunc();
                
                // 调用创建回调
                if (item is IEnhancedPoolable enhancedPoolable)
                {
                    enhancedPoolable.OnCreate();
                }

                // 调用Despawn准备归还状态
                CallOnDespawn(item);

                _pool.Push(item);
                actualCount++;

                // 更新统计信息
                if (_statistics != null)
                {
                    _statistics.TotalCreatedCount++;
                }
            }

            // 更新统计信息
            if (_statistics != null)
            {
                _statistics.AvailableCount = _pool.Count;
            }

            LogDebug($"Prewarmed {actualCount} objects, pool size: {_pool.Count}");
        }

        /// <summary>
        /// 清空对象池中的所有对象
        /// </summary>
        public void Clear()
        {
            if (_isDisposed)
                return;

            // 销毁池中的所有对象
            while (_pool.Count > 0)
            {
                T item = _pool.Pop();
                DestroyObject(item);
            }

            // 销毁所有活跃对象（可选，通常不建议）
            if (_config.EnableDebugMode)
            {
                foreach (T item in _activeObjects)
                {
                    LogDebug($"Warning: Destroying active object: {item}");
                    DestroyObject(item);
                }
            }
            _activeObjects.Clear();

            // 更新统计信息
            if (_statistics != null)
            {
                _statistics.AvailableCount = 0;
                _statistics.ActiveCount = 0;
            }

            LogDebug("Pool cleared");
        }

        #endregion

        #region 状态查询

        /// <summary>
        /// 获取对象池状态信息
        /// </summary>
        /// <returns>状态信息字符串</returns>
        public virtual string GetStatusInfo()
        {
            if (_isDisposed)
                return "Enhanced Pool [DISPOSED]";

            string basicInfo = $"Enhanced Pool<{typeof(T).Name}>[Available:{AvailableCount}, Active:{ActiveCount}, Max:{MaxCapacity}]";
            
            if (_statistics != null)
            {
                return basicInfo + "\n" + _statistics.GetSummary();
            }

            return basicInfo;
        }

        /// <summary>
        /// 获取详细的统计信息
        /// </summary>
        /// <returns>详细统计信息</returns>
        public string GetDetailedStatistics()
        {
            if (_statistics == null)
                return "Statistics disabled";

            return _statistics.GetDetailedInfo();
        }

        #endregion

        #region 配置应用

        /// <summary>
        /// 应用新的配置（为后期扩展预留）
        /// </summary>
        /// <param name="newConfig">新配置</param>
        public virtual void ApplyConfig(EnhancedPoolConfig newConfig)
        {
            if (newConfig == null || !newConfig.IsValid())
            {
                LogDebug("Invalid config provided");
                return;
            }

            // 当前版本只支持部分配置的动态更新
            // 完整版本可以支持更多配置的热更新
            LogDebug($"Config update requested: {newConfig.GetDescription()}");
            LogDebug("Note: Full config update will be supported in future versions");
        }

        #endregion

        #region IDisposable实现

        /// <summary>
        /// 释放对象池资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            Clear();
            _isDisposed = true;
            LogDebug("Enhanced Pool disposed");
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 检查对象池是否已被释放
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(EnhancedPool<T>));
        }

        /// <summary>
        /// 调用对象的OnSpawn方法
        /// </summary>
        /// <param name="item">对象实例</param>
        private void CallOnSpawn(T item)
        {
            try
            {
                if (item is IEnhancedPoolable enhancedPoolable)
                {
                    enhancedPoolable.OnSpawn();
                }
                else if (item is SamplePool.ISamplePoolable samplePoolable)
                {
                    // 兼容极简版接口
                    samplePoolable.OnSpawn();
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Error in OnSpawn: {ex.Message}");
            }
        }

        /// <summary>
        /// 调用对象的OnDespawn方法
        /// </summary>
        /// <param name="item">对象实例</param>
        private void CallOnDespawn(T item)
        {
            try
            {
                if (item is IEnhancedPoolable enhancedPoolable)
                {
                    enhancedPoolable.OnDespawn();
                }
                else if (item is SamplePool.ISamplePoolable samplePoolable)
                {
                    // 兼容极简版接口
                    samplePoolable.OnDespawn();
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Error in OnDespawn: {ex.Message}");
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

            try
            {
                // 调用增强版销毁回调
                if (item is IEnhancedPoolable enhancedPoolable)
                {
                    enhancedPoolable.OnDestroy();
                }

                // 调用自定义销毁动作
                _destroyAction?.Invoke(item);

                // 更新统计信息
                if (_statistics != null)
                {
                    _statistics.RecordDestroy();
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Error destroying object: {ex.Message}");
            }
        }

        /// <summary>
        /// 输出调试日志
        /// </summary>
        /// <param name="message">日志消息</param>
        private void LogDebug(string message)
        {
            if (_config.EnableDebugMode)
            {
                UnityEngine.Debug.Log($"[EnhancedPool<{typeof(T).Name}>] {message}");
            }
        }

        #endregion
    }
}