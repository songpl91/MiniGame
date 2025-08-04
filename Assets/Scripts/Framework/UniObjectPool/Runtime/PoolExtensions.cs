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
        /// 创建 GameObject 对象池的便捷方法
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="parent">父对象</param>
        /// <param name="config">对象池配置</param>
        /// <returns>GameObject 对象池</returns>
        public static UniObjectPool<GameObject> CreateGameObjectPool(
            this GameObject prefab,
            Transform parent = null,
            PoolConfig config = null)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            return PoolManager.CreatePool(
                poolName: prefab.name,
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
        }

        /// <summary>
        /// 从 GameObject 对象池获取对象
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="parent">父对象</param>
        /// <returns>GameObject 实例</returns>
        public static GameObject SpawnFromPool(
            this GameObject prefab,
            Vector3 position = default,
            Quaternion rotation = default,
            Transform parent = null)
        {
            var pool = PoolManager.GetPool<GameObject>(prefab.name);
            if (pool == null)
            {
                // 如果对象池不存在，自动创建
                pool = prefab.CreateGameObjectPool(parent);
            }

            var go = pool.Get();
            if (go != null)
            {
                go.transform.position = position;
                go.transform.rotation = rotation;
                if (parent != null)
                    go.transform.SetParent(parent);
            }

            return go;
        }

        /// <summary>
        /// 将 GameObject 归还到对象池
        /// </summary>
        /// <param name="gameObject">要归还的 GameObject</param>
        /// <param name="poolName">对象池名称（可选，如果不指定则使用对象名称）</param>
        /// <returns>是否成功归还</returns>
        public static bool ReturnToPool(this GameObject gameObject, string poolName = null)
        {
            if (gameObject == null)
                return false;

            string name = poolName ?? gameObject.name.Replace("(Clone)", "");
            var pool = PoolManager.GetPool<GameObject>(name);
            if (pool == null)
            {
                UniLogger.Warning($"GameObject 对象池 {name} 不存在，直接销毁对象");
                UnityEngine.Object.Destroy(gameObject);
                return false;
            }

            gameObject.SetActive(false);
            return pool.Return(gameObject);
        }

        /// <summary>
        /// 创建组件对象池的便捷方法
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="prefab">包含组件的预制体</param>
        /// <param name="parent">父对象</param>
        /// <param name="config">对象池配置</param>
        /// <returns>组件对象池</returns>
        public static UniObjectPool<T> CreateComponentPool<T>(
            this GameObject prefab,
            Transform parent = null,
            PoolConfig config = null) where T : Component
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            var component = prefab.GetComponent<T>();
            if (component == null)
                throw new ArgumentException($"预制体 {prefab.name} 上没有找到组件 {typeof(T).Name}");

            return PoolManager.CreatePool(
                poolName: $"{prefab.name}_{typeof(T).Name}",
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
                pool = prefab.CreateComponentPool<T>(parent);
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
    }
}