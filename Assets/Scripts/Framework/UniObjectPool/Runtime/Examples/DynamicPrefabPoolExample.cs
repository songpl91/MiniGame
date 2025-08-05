using System.Collections.Generic;
using UnityEngine;
using UniFramework.ObjectPool;

namespace UniFramework.ObjectPool.Examples
{
    /// <summary>
    /// 动态加载预制体的对象池管理示例
    /// 展示如何在运行时动态加载预制体并使用PoolRegistry
    /// </summary>
    public class DynamicPrefabPoolExample : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private string[] prefabPaths = {
            "Prefabs/Weapons/Bullet",
            "Prefabs/Enemies/Zombie",
            "Prefabs/Effects/Explosion"
        };

        [Header("父对象")]
        public Transform poolParent;

        // 解决方案1：全局预制体管理器
        private static Dictionary<string, GameObject> _loadedPrefabs = new Dictionary<string, GameObject>();
        
        // 解决方案2：预制体ID映射
        private static Dictionary<int, GameObject> _prefabIdMap = new Dictionary<int, GameObject>();

        private void Start()
        {
            PoolManager.Initialize();
            
            Debug.Log("=== 动态加载预制体的对象池管理 ===");
            
            // 演示不同的解决方案
            DemonstrateGlobalPrefabManager();
            DemonstratePrefabIdMapping();
            DemonstrateTagBasedAccess();
        }

        /// <summary>
        /// 解决方案1：全局预制体管理器
        /// 通过统一的管理器来获取预制体引用
        /// </summary>
        private void DemonstrateGlobalPrefabManager()
        {
            Debug.Log("\n=== 解决方案1：全局预制体管理器 ===");
            
            // 模拟动态加载预制体
            LoadPrefabsToGlobalManager();
            
            // 其他系统通过管理器获取预制体
            UseGlobalPrefabManager();
        }

        /// <summary>
        /// 加载预制体到全局管理器
        /// </summary>
        private void LoadPrefabsToGlobalManager()
        {
            Debug.Log("【资源管理器】动态加载预制体...");
            
            foreach (string path in prefabPaths)
            {
                // 模拟从Resources或Addressables加载
                GameObject prefab = LoadPrefabFromPath(path);
                if (prefab != null)
                {
                    // 存储到全局管理器
                    string key = System.IO.Path.GetFileNameWithoutExtension(path);
                    _loadedPrefabs[key] = prefab;
                    
                    // 创建对象池并注册
                    string poolName = $"{key}Pool";
                    prefab.CreateGameObjectPool(poolName, poolParent);
                    PoolRegistry.RegisterPool(poolName, prefab, poolParent, typeof(GameObject), "Dynamic", key);
                    
                    Debug.Log($"✓ 加载并注册：{key} -> {poolName}");
                }
            }
        }

        /// <summary>
        /// 其他系统使用全局预制体管理器
        /// </summary>
        private void UseGlobalPrefabManager()
        {
            Debug.Log("【武器系统】需要子弹预制体");
            
            // 通过预制体名称获取
            GameObject bulletPrefab = GetPrefabByName("Bullet");
            if (bulletPrefab != null)
            {
                // 使用PoolRegistry根据预制体找池
                string poolName = PoolRegistry.FindPoolNameByPrefab(bulletPrefab);
                if (!string.IsNullOrEmpty(poolName))
                {
                    var bullet = PoolExtensions.SpawnFromPool(poolName, Vector3.zero);
                    Debug.Log($"✓ 武器系统成功生成子弹，使用池：{poolName}");
                    if (bullet != null) bullet.ReturnToPool(poolName);
                }
            }
        }

        /// <summary>
        /// 解决方案2：预制体ID映射
        /// 使用数字ID来标识预制体，避免字符串查找
        /// </summary>
        private void DemonstratePrefabIdMapping()
        {
            Debug.Log("\n=== 解决方案2：预制体ID映射 ===");
            
            // 定义预制体ID常量
            const int BULLET_ID = 1001;
            const int ZOMBIE_ID = 2001;
            const int EXPLOSION_ID = 3001;
            
            // 加载并映射
            LoadPrefabsWithIdMapping(BULLET_ID, "Bullet");
            LoadPrefabsWithIdMapping(ZOMBIE_ID, "Zombie");
            LoadPrefabsWithIdMapping(EXPLOSION_ID, "Explosion");
            
            // 使用ID获取预制体
            UsePrefabById(BULLET_ID, "【AI系统】");
            UsePrefabById(ZOMBIE_ID, "【敌人生成器】");
        }

        /// <summary>
        /// 通过ID加载和映射预制体
        /// </summary>
        private void LoadPrefabsWithIdMapping(int prefabId, string prefabName)
        {
            GameObject prefab = GetPrefabByName(prefabName);
            if (prefab != null)
            {
                _prefabIdMap[prefabId] = prefab;
                Debug.Log($"✓ 映射预制体 ID:{prefabId} -> {prefabName}");
            }
        }

        /// <summary>
        /// 通过ID使用预制体
        /// </summary>
        private void UsePrefabById(int prefabId, string systemName)
        {
            Debug.Log($"{systemName} 需要预制体 ID:{prefabId}");
            
            if (_prefabIdMap.TryGetValue(prefabId, out GameObject prefab))
            {
                string poolName = PoolRegistry.FindPoolNameByPrefab(prefab);
                if (!string.IsNullOrEmpty(poolName))
                {
                    var obj = PoolExtensions.SpawnFromPool(poolName, Vector3.zero);
                    Debug.Log($"✓ {systemName} 成功生成对象，使用池：{poolName}");
                    if (obj != null) obj.ReturnToPool(poolName);
                }
            }
        }

        /// <summary>
        /// 解决方案3：基于标签的访问
        /// 不需要具体的预制体引用，通过业务标签来使用
        /// </summary>
        private void DemonstrateTagBasedAccess()
        {
            Debug.Log("\n=== 解决方案3：基于标签的访问 ===");
            
            // 通过标签查找池，不需要预制体引用
            UsePoolsByTag("Dynamic", "【关卡管理器】");
            UsePoolsByTag("Bullet", "【伤害系统】");
        }

        /// <summary>
        /// 通过标签使用对象池
        /// </summary>
        private void UsePoolsByTag(string tag, string systemName)
        {
            Debug.Log($"{systemName} 查找标签为 '{tag}' 的池");
            
            var poolNames = PoolRegistry.FindPoolsByTag(tag);
            if (poolNames.Count > 0)
            {
                foreach (string poolName in poolNames)
                {
                    var obj = PoolExtensions.SpawnFromPool(poolName, Vector3.zero);
                    Debug.Log($"✓ {systemName} 从池 {poolName} 生成对象");
                    if (obj != null) obj.ReturnToPool(poolName);
                }
            }
            else
            {
                Debug.LogWarning($"❌ {systemName} 没有找到标签为 '{tag}' 的池");
            }
        }

        /// <summary>
        /// 模拟从路径加载预制体
        /// </summary>
        private GameObject LoadPrefabFromPath(string path)
        {
            // 实际项目中这里会是：
            // return Resources.Load<GameObject>(path);
            // 或者 Addressables.LoadAssetAsync<GameObject>(path);
            
            // 这里为了演示，创建一个简单的GameObject
            GameObject prefab = new GameObject(System.IO.Path.GetFileNameWithoutExtension(path));
            
            // 添加一个标识组件
            var identifier = prefab.AddComponent<PrefabIdentifier>();
            identifier.prefabName = prefab.name;
            
            return prefab;
        }

        /// <summary>
        /// 全局预制体管理器的访问接口
        /// </summary>
        public static GameObject GetPrefabByName(string prefabName)
        {
            _loadedPrefabs.TryGetValue(prefabName, out GameObject prefab);
            return prefab;
        }

        /// <summary>
        /// 通过ID获取预制体
        /// </summary>
        public static GameObject GetPrefabById(int prefabId)
        {
            _prefabIdMap.TryGetValue(prefabId, out GameObject prefab);
            return prefab;
        }

        /// <summary>
        /// 显示当前状态
        /// </summary>
        [ContextMenu("显示当前状态")]
        public void ShowCurrentStatus()
        {
            Debug.Log("=== 当前加载的预制体 ===");
            foreach (var kvp in _loadedPrefabs)
            {
                Debug.Log($"名称: {kvp.Key} -> {kvp.Value?.name}");
            }
            
            Debug.Log("=== ID映射 ===");
            foreach (var kvp in _prefabIdMap)
            {
                Debug.Log($"ID: {kvp.Key} -> {kvp.Value?.name}");
            }
            
            Debug.Log("=== PoolRegistry 信息 ===");
            var registrations = PoolRegistry.GetAllRegistrations();
            foreach (var kvp in registrations)
            {
                var reg = kvp.Value;
                Debug.Log($"池: {reg.PoolName}, 标签: {string.Join(",", reg.Tags ?? new string[0])}");
            }
        }

        private void OnDestroy()
        {
            // 清理
            _loadedPrefabs.Clear();
            _prefabIdMap.Clear();
            PoolManager.Destroy();
            PoolRegistry.Clear();
        }
    }

    /// <summary>
    /// 预制体标识组件
    /// </summary>
    public class PrefabIdentifier : MonoBehaviour
    {
        public string prefabName;
        public int prefabId;
    }
}