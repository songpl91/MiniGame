using System;

namespace UniFramework.ObjectPool
{
    /// <summary>
    /// 池化对象包装器
    /// 提供自动归还到对象池的功能
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public sealed class PooledObject<T> : IDisposable where T : class
    {
        private T _value;
        private UniObjectPool<T> _pool;
        private bool _isDisposed;

        /// <summary>
        /// 包装的对象实例
        /// </summary>
        public T Value
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(PooledObject<T>));
                return _value;
            }
        }

        /// <summary>
        /// 对象是否已被释放
        /// </summary>
        public bool IsDisposed => _isDisposed;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="value">要包装的对象</param>
        /// <param name="pool">对象池</param>
        internal PooledObject(T value, UniObjectPool<T> pool)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _isDisposed = false;
        }

        /// <summary>
        /// 释放对象并归还到对象池
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            if (_value != null && _pool != null && !_pool.IsDisposed)
            {
                _pool.Return(_value);
            }

            _value = null;
            _pool = null;
            _isDisposed = true;
        }

        /// <summary>
        /// 隐式转换为包装的对象类型
        /// </summary>
        /// <param name="pooledObject">池化对象包装器</param>
        public static implicit operator T(PooledObject<T> pooledObject)
        {
            return pooledObject?.Value;
        }

        /// <summary>
        /// 重写 ToString 方法
        /// </summary>
        /// <returns>对象的字符串表示</returns>
        public override string ToString()
        {
            if (_isDisposed)
                return "[Disposed PooledObject]";
            
            return _value?.ToString() ?? "[Null PooledObject]";
        }

        /// <summary>
        /// 重写 GetHashCode 方法
        /// </summary>
        /// <returns>哈希码</returns>
        public override int GetHashCode()
        {
            if (_isDisposed || _value == null)
                return 0;
            
            return _value.GetHashCode();
        }

        /// <summary>
        /// 重写 Equals 方法
        /// </summary>
        /// <param name="obj">要比较的对象</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            if (_isDisposed)
                return false;

            if (obj is PooledObject<T> other)
                return Equals(_value, other._value);
            
            if (obj is T directValue)
                return Equals(_value, directValue);
            
            return false;
        }
    }
}