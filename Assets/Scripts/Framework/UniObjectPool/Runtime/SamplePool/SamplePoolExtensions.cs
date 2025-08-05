using UnityEngine;

namespace UniFramework.ObjectPool.SamplePool
{
    /// <summary>
    /// 极简版对象池扩展方法
    /// 提供便捷的对象池操作方法
    /// </summary>
    public static class SamplePoolExtensions
    {
        #region GameObject扩展方法

        /// <summary>
        /// 为GameObject创建对象池
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="poolName">对象池名称</param>
        /// <param name="parent">父对象</param>
        /// <param name="maxCapacity">最大容量</param>
        /// <returns>创建的对象池</returns>
        public static SampleGameObjectPool CreateSamplePool(
            this GameObject prefab,
            string poolName,
            Transform parent = null,
            int maxCapacity = 50)
        {
            var pool = new SampleGameObjectPool(prefab, parent, maxCapacity);
            
            // 注册到管理器
            SamplePoolManager.CreatePool(poolName, 
                () => pool.Get(), 
                go => pool.Return(go), 
                maxCapacity);
            
            return pool;
        }

        /// <summary>
        /// 从对象池生成GameObject
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="poolName">对象池名称</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <returns>生成的GameObject</returns>
        public static GameObject SpawnFromSamplePool(
            this GameObject prefab,
            string poolName,
            Vector3 position,
            Quaternion rotation)
        {
            var go = SamplePoolManager.Get<GameObject>(poolName);
            if (go != null)
            {
                go.transform.position = position;
                go.transform.rotation = rotation;
                go.SetActive(true);
            }
            return go;
        }

        /// <summary>
        /// 从对象池生成GameObject（使用默认旋转）
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="poolName">对象池名称</param>
        /// <param name="position">位置</param>
        /// <returns>生成的GameObject</returns>
        public static GameObject SpawnFromSamplePool(
            this GameObject prefab,
            string poolName,
            Vector3 position)
        {
            return prefab.SpawnFromSamplePool(poolName, position, Quaternion.identity);
        }

        /// <summary>
        /// 将GameObject归还到对象池
        /// </summary>
        /// <param name="go">要归还的GameObject</param>
        /// <param name="poolName">对象池名称</param>
        /// <returns>是否成功归还</returns>
        public static bool ReturnToSamplePool(this GameObject go, string poolName)
        {
            if (go != null)
            {
                go.SetActive(false);
                return SamplePoolManager.Return(poolName, go);
            }
            return false;
        }

        #endregion

        #region 通用扩展方法

        /// <summary>
        /// 获取或创建类型对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="createFunc">创建函数</param>
        /// <param name="resetAction">重置动作</param>
        /// <param name="maxCapacity">最大容量</param>
        /// <returns>对象池实例</returns>
        public static SamplePool<T> GetOrCreateSamplePool<T>(
            System.Func<T> createFunc,
            System.Action<T> resetAction = null,
            int maxCapacity = 50) where T : class
        {
            var pool = SamplePoolManager.GetPool<T>();
            if (pool == null)
            {
                pool = SamplePoolManager.CreatePool(createFunc, resetAction, maxCapacity);
            }
            return pool;
        }

        /// <summary>
        /// 获取或创建命名对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="poolName">对象池名称</param>
        /// <param name="createFunc">创建函数</param>
        /// <param name="resetAction">重置动作</param>
        /// <param name="maxCapacity">最大容量</param>
        /// <returns>对象池实例</returns>
        public static SamplePool<T> GetOrCreateSamplePool<T>(
            string poolName,
            System.Func<T> createFunc,
            System.Action<T> resetAction = null,
            int maxCapacity = 50) where T : class
        {
            var pool = SamplePoolManager.GetPool<T>(poolName);
            if (pool == null)
            {
                pool = SamplePoolManager.CreatePool(poolName, createFunc, resetAction, maxCapacity);
            }
            return pool;
        }

        #endregion

        #region 预热扩展方法

        /// <summary>
        /// 预热对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="pool">对象池</param>
        /// <param name="count">预热数量</param>
        /// <returns>对象池实例（用于链式调用）</returns>
        public static SamplePool<T> PrewarmSamplePool<T>(this SamplePool<T> pool, int count) where T : class
        {
            pool?.Prewarm(count);
            return pool;
        }

        #endregion
    }
}