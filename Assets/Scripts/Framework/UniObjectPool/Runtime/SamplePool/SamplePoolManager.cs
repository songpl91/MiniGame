using System;
using System.Collections.Generic;

namespace UniFramework.ObjectPool.SamplePool
{
    /// <summary>
    /// 极简版对象池管理器
    /// 提供全局对象池管理功能，专为Demo设计
    /// 保留扩展接口，便于后期升级
    /// </summary>
    public static class SamplePoolManager
    {
        #region 私有字段
        
        private static readonly Dictionary<Type, object> _typePools = new Dictionary<Type, object>();
        private static readonly Dictionary<string, object> _namedPools = new Dictionary<string, object>();
        private static bool _isInitialized = false;
        
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

        #endregion

        #region 初始化和清理

        /// <summary>
        /// 初始化对象池管理器
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            
            // 为后期扩展预留初始化逻辑
            OnInitialize();
        }

        /// <summary>
        /// 清理所有对象池
        /// </summary>
        public static void Clear()
        {
            _typePools.Clear();
            _namedPools.Clear();
            
            // 为后期扩展预留清理逻辑
            OnClear();
        }

        /// <summary>
        /// 销毁管理器
        /// </summary>
        public static void Destroy()
        {
            Clear();
            _isInitialized = false;
            
            // 为后期扩展预留销毁逻辑
            OnDestroy();
        }

        #endregion

        #region 类型对象池管理

        /// <summary>
        /// 创建类型对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="createFunc">对象创建函数</param>
        /// <param name="resetAction">对象重置动作</param>
        /// <param name="maxCapacity">最大容量</param>
        /// <returns>对象池实例</returns>
        public static SamplePool<T> CreatePool<T>(
            Func<T> createFunc,
            Action<T> resetAction = null,
            int maxCapacity = 50) where T : class
        {
            if (!_isInitialized)
                Initialize();

            Type type = typeof(T);
            
            if (_typePools.ContainsKey(type))
            {
                return _typePools[type] as SamplePool<T>;
            }

            var pool = new SamplePool<T>(createFunc, resetAction, maxCapacity);
            _typePools[type] = pool;
            
            return pool;
        }

        /// <summary>
        /// 获取类型对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>对象池实例，如果不存在则返回 null</returns>
        public static SamplePool<T> GetPool<T>() where T : class
        {
            Type type = typeof(T);
            return _typePools.TryGetValue(type, out object pool) ? pool as SamplePool<T> : null;
        }

        /// <summary>
        /// 从类型对象池获取对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>对象实例</returns>
        public static T Get<T>() where T : class
        {
            var pool = GetPool<T>();
            return pool?.Get();
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
            return pool?.Return(item) ?? false;
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
        /// <param name="maxCapacity">最大容量</param>
        /// <returns>对象池实例</returns>
        public static SamplePool<T> CreatePool<T>(
            string poolName,
            Func<T> createFunc,
            Action<T> resetAction = null,
            int maxCapacity = 50) where T : class
        {
            if (!_isInitialized)
                Initialize();

            if (string.IsNullOrEmpty(poolName))
                throw new ArgumentException("对象池名称不能为空", nameof(poolName));

            if (_namedPools.ContainsKey(poolName))
            {
                return _namedPools[poolName] as SamplePool<T>;
            }

            var pool = new SamplePool<T>(createFunc, resetAction, maxCapacity);
            _namedPools[poolName] = pool;
            
            return pool;
        }

        /// <summary>
        /// 获取命名对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="poolName">对象池名称</param>
        /// <returns>对象池实例，如果不存在则返回 null</returns>
        public static SamplePool<T> GetPool<T>(string poolName) where T : class
        {
            if (string.IsNullOrEmpty(poolName))
                return null;

            return _namedPools.TryGetValue(poolName, out object pool) ? pool as SamplePool<T> : null;
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
            return pool?.Get();
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
            return pool?.Return(item) ?? false;
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 获取所有对象池的状态信息
        /// </summary>
        /// <returns>状态信息字符串</returns>
        public static string GetAllPoolsStatus()
        {
            var status = $"SamplePoolManager Status:\n";
            status += $"Total Pools: {PoolCount}\n";
            status += $"Type Pools: {_typePools.Count}\n";
            status += $"Named Pools: {_namedPools.Count}\n";
            
            return status;
        }

        #endregion

        #region 扩展接口（为后期升级预留）

        /// <summary>
        /// 初始化时调用（为后期扩展预留）
        /// </summary>
        private static void OnInitialize()
        {
            // 基础版本不实现，为后期扩展预留
        }

        /// <summary>
        /// 清理时调用（为后期扩展预留）
        /// </summary>
        private static void OnClear()
        {
            // 基础版本不实现，为后期扩展预留
        }

        /// <summary>
        /// 销毁时调用（为后期扩展预留）
        /// </summary>
        private static void OnDestroy()
        {
            // 基础版本不实现，为后期扩展预留
        }

        #endregion
    }
}