using System;
using UnityEngine;

namespace UniFramework.ObjectPool
{
    /// <summary>
    /// 对象池扩展方法类
    /// 提供便捷的对象池操作扩展方法
    /// </summary>
    public static class PoolExtensions
    {
        /// <summary>
        /// 为对象添加自动归还功能
        /// 当对象实现 IDisposable 时，在 Dispose 时自动归还到对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="item">对象实例</param>
        /// <param name="pool">对象池</param>
        /// <returns>包装后的对象</returns>
        public static PooledObject<T> AsPooled<T>(this T item, UniObjectPool<T> pool) where T : class
        {
            return new PooledObject<T>(item, pool);
        }

        /// <summary>
        /// 为对象添加自动归还功能（使用类型对象池）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="item">对象实例</param>
        /// <returns>包装后的对象</returns>
        public static PooledObject<T> AsPooled<T>(this T item) where T : class
        {
            var pool = PoolManager.GetPool<T>();
            if (pool == null)
                throw new InvalidOperationException($"类型 {typeof(T).Name} 的对象池不存在");
            
            return new PooledObject<T>(item, pool);
        }

        /// <summary>
        /// 为GameObject创建对象池
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="poolName">对象池名称（必须指定，确保用户明确知道池的名称）</param>
        /// <param name="parent">父对象</param>
        /// <param name="config">对象池配置</param>
        /// <returns>对象池实例</returns>
        public static UniObjectPool<GameObject> CreateGameObjectPool(
            this GameObject prefab,
            string poolName,
            Transform parent = null,
            PoolConfig config = null)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));
            
            if (string.IsNullOrEmpty(poolName))
                throw new ArgumentException("对象池名称不能为空。请为对象池指定一个明确的名称，例如: 'BulletPool', 'EnemyPool' 等", nameof(poolName));

            config = config ?? PoolConfig.CreateDefault();

            // 检查是否已存在同名对象池
            if (PoolManager.HasPool(poolName))
            {
                UniLogger.Warning($"对象池 '{poolName}' 已存在，将返回现有对象池");
                return PoolManager.GetPool<GameObject>(poolName);
            }

            var pool = PoolManager.CreatePool(
                poolName: poolName,
                createFunc: () => UnityEngine.Object.Instantiate(prefab, parent),
                resetAction: go =>
                {
                    go.SetActive(true);
                    go.transform.SetParent(parent);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;
                },
                destroyAction: go =>
                {
                    if (go != null)
                        UnityEngine.Object.Destroy(go);
                },
                config: config
            );

            // 注册对象池到注册器
            PoolRegistry.RegisterPool(poolName, prefab, parent, typeof(GameObject));

            return pool;
        }

        /// <summary>
        /// 为GameObject创建对象池（使用默认命名规则的便捷方法）
        /// 默认命名规则：{预制体名称}Pool
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="parent">父对象</param>
        /// <param name="config">对象池配置</param>
        /// <returns>对象池实例</returns>
        public static UniObjectPool<GameObject> CreateGameObjectPoolWithDefaultName(
            this GameObject prefab,
            Transform parent = null,
            PoolConfig config = null)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            // 使用简单明确的默认命名规则
            string defaultPoolName = PoolRegistry.GenerateDefaultPoolName(prefab);
            
            return prefab.CreateGameObjectPool(defaultPoolName, parent, config);
        }

        /// <summary>
        /// 从GameObject对象池获取对象
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="poolName">对象池名称（必须指定）</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="parent">父对象</param>
        /// <returns>从对象池获取的GameObject</returns>
        public static GameObject SpawnFromPool(
            this GameObject prefab,
            string poolName,
            Vector3 position = default,
            Quaternion rotation = default,
            Transform parent = null)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));
            
            if (string.IsNullOrEmpty(poolName))
                throw new ArgumentException("对象池名称不能为空。请指定要使用的对象池名称", nameof(poolName));

            // 获取对象池
            var pool = PoolManager.GetPool<GameObject>(poolName);
            if (pool == null)
            {
                throw new InvalidOperationException($"对象池 '{poolName}' 不存在。请先使用 CreateGameObjectPool 创建对象池");
            }

            // 从对象池获取对象
            var obj = pool.Get();
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                if (parent != null)
                    obj.transform.SetParent(parent);
            }

            return obj;
        }

        /// <summary>
        /// 从GameObject对象池获取对象（使用默认池名称）
        /// 默认池名称：{预制体名称}Pool
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="parent">父对象</param>
        /// <returns>从对象池获取的GameObject</returns>
        public static GameObject SpawnFromPoolWithDefaultName(
            this GameObject prefab,
            Vector3 position = default,
            Quaternion rotation = default,
            Transform parent = null)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            string defaultPoolName = PoolRegistry.GenerateDefaultPoolName(prefab);
            return prefab.SpawnFromPool(defaultPoolName, position, rotation, parent);
        }

        /// <summary>
        /// 将GameObject归还到对象池
        /// </summary>
        /// <param name="gameObject">要归还的游戏对象</param>
        /// <param name="poolName">对象池名称（必须指定）</param>
        public static void ReturnToPool(this GameObject gameObject, string poolName)
        {
            if (gameObject == null)
                return;
            
            if (string.IsNullOrEmpty(poolName))
                throw new ArgumentException("对象池名称不能为空。请指定要归还到的对象池名称", nameof(poolName));

            var pool = PoolManager.GetPool<GameObject>(poolName);
            if (pool != null)
            {
                gameObject.SetActive(false);
                pool.Return(gameObject);
            }
            else
            {
                // 如果找不到对象池，直接销毁对象
                UnityEngine.Object.Destroy(gameObject);
                UniLogger.Warning($"未找到对象池 '{poolName}'，直接销毁对象 {gameObject.name}");
            }
        }

        /// <summary>
        /// 将GameObject归还到对象池（使用默认池名称）
        /// 默认池名称：{对象名称去掉(Clone)}Pool
        /// </summary>
        /// <param name="gameObject">要归还的游戏对象</param>
        public static void ReturnToPoolWithDefaultName(this GameObject gameObject)
        {
            if (gameObject == null)
                return;

            // 移除(Clone)后缀
            string objectName = gameObject.name.Replace("(Clone)", "");
            
            // 创建一个临时GameObject来使用默认命名规则
            var tempGO = new GameObject(objectName);
            string defaultName = PoolRegistry.GenerateDefaultPoolName(tempGO);
            UnityEngine.Object.DestroyImmediate(tempGO);
            
            gameObject.ReturnToPool(defaultName);
        }

        /// <summary>
        /// 创建组件对象池
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="prefab">包含组件的预制体</param>
        /// <param name="poolName">对象池名称（必须指定）</param>
        /// <param name="parent">父对象</param>
        /// <param name="config">对象池配置</param>
        /// <returns>组件对象池</returns>
        public static UniObjectPool<T> CreateComponentPool<T>(
            this GameObject prefab,
            string poolName,
            Transform parent = null,
            PoolConfig config = null) where T : Component
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));
            
            if (string.IsNullOrEmpty(poolName))
                throw new ArgumentException("对象池名称不能为空。请为组件对象池指定一个明确的名称", nameof(poolName));

            var component = prefab.GetComponent<T>();
            if (component == null)
                throw new ArgumentException($"预制体 {prefab.name} 上没有找到组件 {typeof(T).Name}");

            config = config ?? PoolConfig.CreateDefault();

            // 检查是否已存在同名对象池
            if (PoolManager.HasPool(poolName))
            {
                UniLogger.Warning($"组件对象池 '{poolName}' 已存在，将返回现有对象池");
                return PoolManager.GetPool<T>(poolName);
            }
            
            var pool = PoolManager.CreatePool(
                poolName: poolName,
                createFunc: () =>
                {
                    var go = UnityEngine.Object.Instantiate(prefab, parent);
                    return go.GetComponent<T>();
                },
                resetAction: comp =>
                {
                    if (comp != null)
                    {
                        comp.gameObject.SetActive(true);
                        comp.transform.SetParent(parent);
                        comp.transform.localPosition = Vector3.zero;
                        comp.transform.localRotation = Quaternion.identity;
                        comp.transform.localScale = Vector3.one;
                    }
                },
                destroyAction: comp =>
                {
                    if (comp != null && comp.gameObject != null)
                        UnityEngine.Object.Destroy(comp.gameObject);
                },
                config: config
            );

            // 注册对象池到注册器
            PoolRegistry.RegisterPool(poolName, prefab, parent, typeof(T));

            return pool;
        }

        /// <summary>
        /// 创建组件对象池（使用默认命名规则）
        /// 默认命名规则：{组件类型名称}_{预制体名称}Pool
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="prefab">包含组件的预制体</param>
        /// <param name="parent">父对象</param>
        /// <param name="config">对象池配置</param>
        /// <returns>组件对象池</returns>
        public static UniObjectPool<T> CreateComponentPoolWithDefaultName<T>(
            this GameObject prefab,
            Transform parent = null,
            PoolConfig config = null) where T : Component
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            // 对于组件池，使用组件类型名称作为前缀
            string baseName = PoolRegistry.GenerateDefaultPoolName(prefab);
            string defaultPoolName = $"{typeof(T).Name}_{baseName}";
            
            return prefab.CreateComponentPool<T>(defaultPoolName, parent, config);
        }

        /// <summary>
        /// 从组件对象池获取组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="prefab">预制体</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="parent">父对象</param>
        /// <returns>组件实例</returns>
        public static T SpawnComponentFromPool<T>(
            this GameObject prefab,
            Vector3 position = default,
            Quaternion rotation = default,
            Transform parent = null) where T : Component
        {
            string poolName = $"{prefab.name}_{typeof(T).Name}";
            var pool = PoolManager.GetPool<T>(poolName);
            if (pool == null)
            {
                // 如果对象池不存在，自动创建
                // pool = prefab.CreateComponentPool<T>(parent);
            }

            var component = pool.Get();
            if (component != null)
            {
                component.transform.position = position;
                component.transform.rotation = rotation;
                if (parent != null)
                    component.transform.SetParent(parent);
            }

            return component;
        }

        /// <summary>
        /// 将组件归还到对象池
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="component">要归还的组件</param>
        /// <param name="poolName">对象池名称（可选）</param>
        /// <returns>是否成功归还</returns>
        public static bool ReturnToPool<T>(this T component, string poolName = null) where T : Component
        {
            if (component == null)
                return false;

            string name = poolName ?? $"{component.name.Replace("(Clone)", "")}_{typeof(T).Name}";
            var pool = PoolManager.GetPool<T>(name);
            if (pool == null)
            {
                UniLogger.Warning($"组件对象池 {name} 不存在，直接销毁对象");
                if (component.gameObject != null)
                    UnityEngine.Object.Destroy(component.gameObject);
                return false;
            }

            component.gameObject.SetActive(false);
            return pool.Return(component);
        }

        #region 静态便捷方法

        /// <summary>
        /// 从对象池获取GameObject（静态方法）
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="parent">父对象</param>
        /// <returns>从对象池获取的GameObject</returns>
        public static GameObject SpawnFromPool(
            string poolName,
            Vector3 position = default,
            Quaternion rotation = default,
            Transform parent = null)
        {
            if (string.IsNullOrEmpty(poolName))
                throw new ArgumentException("对象池名称不能为空。请指定要使用的对象池名称", nameof(poolName));

            // 获取对象池
            var pool = PoolManager.GetPool<GameObject>(poolName);
            if (pool == null)
            {
                throw new InvalidOperationException($"对象池 '{poolName}' 不存在。请先使用 CreateGameObjectPool 创建对象池");
            }

            // 从对象池获取对象
            var obj = pool.Get();
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                if (parent != null)
                    obj.transform.SetParent(parent);
            }

            return obj;
        }

        /// <summary>
        /// 从对象池获取GameObject（静态方法，仅指定池名称）
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <returns>从对象池获取的GameObject</returns>
        public static GameObject SpawnFromPool(string poolName)
        {
            return SpawnFromPool(poolName, Vector3.zero, Quaternion.identity, null);
        }

        #endregion

        #region 私有辅助方法



        #endregion
    }
}