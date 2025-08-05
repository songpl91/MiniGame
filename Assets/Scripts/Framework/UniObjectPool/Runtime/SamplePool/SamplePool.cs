using System;
using System.Collections.Generic;

namespace UniFramework.ObjectPool.SamplePool
{
    /// <summary>
    /// 极简版对象池
    /// 提供最基础的对象池功能，专为Demo设计
    /// 保留扩展接口，便于后期升级到完整版本
    /// </summary>
    /// <typeparam name="T">池化对象类型</typeparam>
    public class SamplePool<T> where T : class
    {
        #region 核心字段
        
        private readonly Stack<T> _pool;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _resetAction;
        private readonly int _maxCapacity;
        
        #endregion

        #region 属性

        /// <summary>
        /// 池中可用对象数量
        /// </summary>
        public int AvailableCount => _pool.Count;

        /// <summary>
        /// 最大容量
        /// </summary>
        public int MaxCapacity => _maxCapacity;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="createFunc">对象创建函数</param>
        /// <param name="resetAction">对象重置动作（可选）</param>
        /// <param name="maxCapacity">最大容量，默认50</param>
        public SamplePool(Func<T> createFunc, Action<T> resetAction = null, int maxCapacity = 50)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _resetAction = resetAction;
            _maxCapacity = maxCapacity > 0 ? maxCapacity : 50;
            
            _pool = new Stack<T>(_maxCapacity);
        }

        #endregion

        #region 核心方法

        /// <summary>
        /// 从对象池获取对象
        /// </summary>
        /// <returns>池化对象实例</returns>
        public T Get()
        {
            T item;
            
            // 从池中获取或创建新对象
            if (_pool.Count > 0)
            {
                item = _pool.Pop();
            }
            else
            {
                item = _createFunc();
            }

            // 调用对象的 OnSpawn 方法
            if (item is ISamplePoolable poolable)
            {
                poolable.OnSpawn();
            }

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
            if (item == null)
                return false;

            // 检查池是否已满
            if (_pool.Count >= _maxCapacity)
            {
                // 池已满，不归还（让GC处理）
                return false;
            }

            // 调用对象的 OnDespawn 方法
            if (item is ISamplePoolable poolable)
            {
                poolable.OnDespawn();
            }

            _pool.Push(item);
            return true;
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        public void Clear()
        {
            _pool.Clear();
        }

        /// <summary>
        /// 预热对象池（创建指定数量的对象）
        /// </summary>
        /// <param name="count">预创建的对象数量</param>
        public void Prewarm(int count)
        {
            count = Math.Min(count, _maxCapacity);
            
            for (int i = 0; i < count; i++)
            {
                if (_pool.Count >= _maxCapacity)
                    break;
                    
                var item = _createFunc();
                if (item is ISamplePoolable poolable)
                {
                    poolable.OnDespawn(); // 确保对象处于正确的初始状态
                }
                _pool.Push(item);
            }
        }

        #endregion

        #region 扩展接口（为后期升级预留）

        /// <summary>
        /// 获取池状态信息（为后期扩展预留）
        /// </summary>
        /// <returns>状态信息字符串</returns>
        public virtual string GetStatusInfo()
        {
            return $"SamplePool<{typeof(T).Name}>: Available={AvailableCount}, MaxCapacity={MaxCapacity}";
        }

        /// <summary>
        /// 扩展配置方法（为后期升级预留）
        /// 子类可以重写此方法来支持更复杂的配置
        /// </summary>
        /// <param name="config">配置对象</param>
        protected virtual void ApplyConfig(object config)
        {
            // 基础版本不实现，为后期扩展预留
        }

        #endregion
    }
}