using UnityEngine;

namespace UniFramework.ObjectPool.SamplePool
{
    /// <summary>
    /// 极简版GameObject对象池
    /// 专门用于Unity GameObject的池化管理
    /// 提供最常用的GameObject池化功能
    /// </summary>
    public class SampleGameObjectPool : SamplePool<GameObject>
    {
        #region 私有字段
        
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        
        #endregion

        #region 属性

        /// <summary>
        /// 预制体引用
        /// </summary>
        public GameObject Prefab => _prefab;

        /// <summary>
        /// 父对象
        /// </summary>
        public Transform Parent => _parent;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="parent">父对象</param>
        /// <param name="maxCapacity">最大容量</param>
        public SampleGameObjectPool(GameObject prefab, Transform parent = null, int maxCapacity = 50)
            : base(() => CreateGameObject(prefab, parent), ResetGameObject, maxCapacity)
        {
            _prefab = prefab;
            _parent = parent;
        }

        #endregion

        #region 静态方法

        /// <summary>
        /// 创建GameObject实例
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="parent">父对象</param>
        /// <returns>创建的GameObject</returns>
        public static GameObject CreateGameObject(GameObject prefab, Transform parent)
        {
            var go = Object.Instantiate(prefab, parent);
            go.SetActive(false); // 创建时默认不激活
            return go;
        }

        /// <summary>
        /// 重置GameObject状态
        /// </summary>
        /// <param name="go">要重置的GameObject</param>
        public static void ResetGameObject(GameObject go)
        {
            if (go != null)
            {
                go.SetActive(true);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
            }
        }

        #endregion

        #region 便捷方法

        /// <summary>
        /// 生成GameObject到指定位置
        /// </summary>
        /// <param name="position">世界坐标位置</param>
        /// <param name="rotation">旋转</param>
        /// <returns>生成的GameObject</returns>
        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            var go = Get();
            if (go != null)
            {
                go.transform.position = position;
                go.transform.rotation = rotation;
                go.SetActive(true);
            }
            return go;
        }

        /// <summary>
        /// 生成GameObject到指定位置（使用默认旋转）
        /// </summary>
        /// <param name="position">世界坐标位置</param>
        /// <returns>生成的GameObject</returns>
        public GameObject Spawn(Vector3 position)
        {
            return Spawn(position, Quaternion.identity);
        }

        /// <summary>
        /// 生成GameObject（使用默认位置和旋转）
        /// </summary>
        /// <returns>生成的GameObject</returns>
        public GameObject Spawn()
        {
            return Spawn(Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// 回收GameObject
        /// </summary>
        /// <param name="go">要回收的GameObject</param>
        /// <returns>是否成功回收</returns>
        public bool Despawn(GameObject go)
        {
            if (go != null)
            {
                go.SetActive(false);
                return Return(go);
            }
            return false;
        }

        #endregion

        #region 重写方法

        /// <summary>
        /// 获取池状态信息
        /// </summary>
        /// <returns>状态信息字符串</returns>
        public override string GetStatusInfo()
        {
            var prefabName = _prefab != null ? _prefab.name : "Unknown";
            return $"SampleGameObjectPool[{prefabName}]: Available={AvailableCount}, MaxCapacity={MaxCapacity}";
        }

        #endregion
    }
}