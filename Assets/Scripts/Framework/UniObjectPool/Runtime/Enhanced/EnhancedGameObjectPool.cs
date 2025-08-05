using System;
using UnityEngine;

namespace UniFramework.ObjectPool.Enhanced
{
    /// <summary>
    /// 增强版 GameObject 对象池
    /// 专门用于 Unity GameObject 的池化管理
    /// 在极简版基础上增加了更多 Unity 特定的功能和优化
    /// </summary>
    public class EnhancedGameObjectPool : IDisposable
    {
        #region 私有字段

        private readonly EnhancedPool<GameObject> _pool;
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly bool _worldPositionStays;
        private readonly Vector3 _spawnPosition;
        private readonly Quaternion _spawnRotation;
        private readonly Vector3 _spawnScale;
        private readonly bool _setActiveOnGet;
        private readonly bool _setInactiveOnReturn;
        private readonly string _poolName;

        #endregion

        #region 属性

        /// <summary>
        /// 可用对象数量
        /// </summary>
        public int AvailableCount => _pool.AvailableCount;

        /// <summary>
        /// 活跃对象数量
        /// </summary>
        public int ActiveCount => _pool.ActiveCount;

        /// <summary>
        /// 对象池配置
        /// </summary>
        public EnhancedPoolConfig Config => _pool.Config;

        /// <summary>
        /// 统计信息
        /// </summary>
        public EnhancedPoolStatistics Statistics => _pool.Statistics;

        /// <summary>
        /// 预制体引用
        /// </summary>
        public GameObject Prefab => _prefab;

        /// <summary>
        /// 父级变换
        /// </summary>
        public Transform Parent => _parent;

        /// <summary>
        /// 对象池名称
        /// </summary>
        public string PoolName => _poolName;

        #endregion

        #region 构造函数

        /// <summary>
        /// 创建 GameObject 对象池
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="parent">父级变换</param>
        /// <param name="config">对象池配置</param>
        /// <param name="poolName">对象池名称</param>
        /// <param name="worldPositionStays">设置父级时是否保持世界坐标</param>
        /// <param name="spawnPosition">生成位置</param>
        /// <param name="spawnRotation">生成旋转</param>
        /// <param name="spawnScale">生成缩放</param>
        /// <param name="setActiveOnGet">获取时是否激活</param>
        /// <param name="setInactiveOnReturn">归还时是否禁用</param>
        public EnhancedGameObjectPool(
            GameObject prefab,
            Transform parent = null,
            EnhancedPoolConfig config = null,
            string poolName = null,
            bool worldPositionStays = false,
            Vector3? spawnPosition = null,
            Quaternion? spawnRotation = null,
            Vector3? spawnScale = null,
            bool setActiveOnGet = true,
            bool setInactiveOnReturn = true)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab), "预制体不能为空");

            _prefab = prefab;
            _parent = parent;
            _worldPositionStays = worldPositionStays;
            _spawnPosition = spawnPosition ?? Vector3.zero;
            _spawnRotation = spawnRotation ?? Quaternion.identity;
            _spawnScale = spawnScale ?? Vector3.one;
            _setActiveOnGet = setActiveOnGet;
            _setInactiveOnReturn = setInactiveOnReturn;
            _poolName = poolName ?? $"{prefab.name}_Pool";

            // 设置配置标签
            var poolConfig = config ?? EnhancedPoolConfig.CreateDefault();
            if (string.IsNullOrEmpty(poolConfig.Tag))
                poolConfig.Tag = _poolName;

            // 创建内部对象池
            _pool = new EnhancedPool<GameObject>(
                CreateGameObject,
                ResetGameObject,
                DestroyGameObject,
                poolConfig
            );

            Debug.Log($"[EnhancedGameObjectPool] Created pool '{_poolName}' for prefab '{prefab.name}'");
        }

        #endregion

        #region 核心方法

        /// <summary>
        /// 获取 GameObject
        /// </summary>
        /// <returns>GameObject 实例</returns>
        public GameObject Get()
        {
            var gameObject = _pool.Get();
            
            if (gameObject != null)
            {
                // 设置变换
                gameObject.transform.position = _spawnPosition;
                gameObject.transform.rotation = _spawnRotation;
                gameObject.transform.localScale = _spawnScale;

                // 设置父级
                if (_parent != null)
                {
                    gameObject.transform.SetParent(_parent, _worldPositionStays);
                }

                // 设置激活状态
                if (_setActiveOnGet && !gameObject.activeInHierarchy)
                {
                    gameObject.SetActive(true);
                }

                // 调用 IEnhancedPoolable 接口
                var poolable = gameObject.GetComponent<IEnhancedPoolable>();
                poolable?.OnSpawn();
            }

            return gameObject;
        }

        /// <summary>
        /// 获取 GameObject 并设置位置
        /// </summary>
        /// <param name="position">位置</param>
        /// <returns>GameObject 实例</returns>
        public GameObject Get(Vector3 position)
        {
            var gameObject = Get();
            if (gameObject != null)
            {
                gameObject.transform.position = position;
            }
            return gameObject;
        }

        /// <summary>
        /// 获取 GameObject 并设置位置和旋转
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <returns>GameObject 实例</returns>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            var gameObject = Get();
            if (gameObject != null)
            {
                gameObject.transform.position = position;
                gameObject.transform.rotation = rotation;
            }
            return gameObject;
        }

        /// <summary>
        /// 获取 GameObject 并设置变换
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="scale">缩放</param>
        /// <returns>GameObject 实例</returns>
        public GameObject Get(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var gameObject = Get();
            if (gameObject != null)
            {
                gameObject.transform.position = position;
                gameObject.transform.rotation = rotation;
                gameObject.transform.localScale = scale;
            }
            return gameObject;
        }

        /// <summary>
        /// 归还 GameObject
        /// </summary>
        /// <param name="gameObject">要归还的 GameObject</param>
        /// <returns>是否成功归还</returns>
        public bool Return(GameObject gameObject)
        {
            if (gameObject == null)
                return false;

            // 调用 IEnhancedPoolable 接口
            var poolable = gameObject.GetComponent<IEnhancedPoolable>();
            if (poolable != null && !poolable.CanReturn())
            {
                Debug.LogWarning($"[EnhancedGameObjectPool] GameObject '{gameObject.name}' cannot be returned");
                return false;
            }

            poolable?.OnDespawn();

            // 设置非激活状态
            if (_setInactiveOnReturn && gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
            }

            return _pool.Return(gameObject);
        }

        /// <summary>
        /// 预热对象池
        /// </summary>
        /// <param name="count">预热数量</param>
        public void Prewarm(int count)
        {
            _pool.Prewarm(count);
            Debug.Log($"[EnhancedGameObjectPool] Prewarmed '{_poolName}' with {count} objects");
        }

        /// <summary>
        /// 清理对象池
        /// </summary>
        public void Clear()
        {
            _pool.Clear();
            Debug.Log($"[EnhancedGameObjectPool] Cleared pool '{_poolName}'");
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 创建 GameObject
        /// </summary>
        /// <returns>新的 GameObject 实例</returns>
        private GameObject CreateGameObject()
        {
            var gameObject = UnityEngine.Object.Instantiate(_prefab, _parent, _worldPositionStays);
            gameObject.name = _prefab.name; // 移除 "(Clone)" 后缀

            // 初始设置为非激活状态
            if (_setInactiveOnReturn)
            {
                gameObject.SetActive(false);
            }

            // 调用 IEnhancedPoolable 接口
            var poolable = gameObject.GetComponent<IEnhancedPoolable>();
            poolable?.OnCreate();

            return gameObject;
        }

        /// <summary>
        /// 重置 GameObject
        /// </summary>
        /// <param name="gameObject">要重置的 GameObject</param>
        private void ResetGameObject(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            // 重置变换
            gameObject.transform.position = _spawnPosition;
            gameObject.transform.rotation = _spawnRotation;
            gameObject.transform.localScale = _spawnScale;

            // 设置父级
            if (_parent != null)
            {
                gameObject.transform.SetParent(_parent, _worldPositionStays);
            }

            // 设置非激活状态
            if (_setInactiveOnReturn && gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
            }

            // 调用 IEnhancedPoolable 接口
            var poolable = gameObject.GetComponent<IEnhancedPoolable>();
            poolable?.OnReset();
        }

        /// <summary>
        /// 销毁 GameObject
        /// </summary>
        /// <param name="gameObject">要销毁的 GameObject</param>
        private void DestroyGameObject(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            // 调用 IEnhancedPoolable 接口
            var poolable = gameObject.GetComponent<IEnhancedPoolable>();
            poolable?.OnDestroy();

            UnityEngine.Object.Destroy(gameObject);
        }

        #endregion

        #region 状态和调试

        /// <summary>
        /// 获取状态信息
        /// </summary>
        /// <returns>状态信息字符串</returns>
        public string GetStatusInfo()
        {
            return $"[{_poolName}] Prefab:{_prefab.name}, Available:{AvailableCount}, " +
                   $"Active:{ActiveCount}, Parent:{(_parent?.name ?? "None")}";
        }

        /// <summary>
        /// 获取详细状态信息
        /// </summary>
        /// <returns>详细状态信息</returns>
        public string GetDetailedStatusInfo()
        {
            return $"Enhanced GameObject Pool '{_poolName}':\n" +
                   $"  Prefab: {_prefab.name}\n" +
                   $"  Parent: {(_parent?.name ?? "None")}\n" +
                   $"  Available: {AvailableCount}\n" +
                   $"  Active: {ActiveCount}\n" +
                   $"  Statistics: {Statistics.GetSummary()}\n" +
                   $"  Config: {Config.GetDescription()}";
        }

        #endregion

        #region 扩展接口预留

        /// <summary>
        /// 应用配置（为后期扩展预留）
        /// </summary>
        /// <param name="config">新配置</param>
        public void ApplyConfig(EnhancedPoolConfig config)
        {
            _pool.ApplyConfig(config);
        }

        /// <summary>
        /// 释放资源（为后期扩展预留）
        /// </summary>
        public void ReleaseResources()
        {
            // 后期可以在这里添加资源释放逻辑
            // 例如：释放纹理、音频等资源
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _pool?.Dispose();
            Debug.Log($"[EnhancedGameObjectPool] Disposed pool '{_poolName}'");
        }

        #endregion
    }
}