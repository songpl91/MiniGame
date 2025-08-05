using System.Collections.Generic;
using UnityEngine;
using UniFramework.ObjectPool;

namespace UniFramework.ObjectPool.Examples
{
    /// <summary>
    /// PoolRegistry 设计理念演示
    /// 展示为什么需要 PoolRegistry 以及它解决的核心问题
    /// </summary>
    public class PoolRegistryConceptExample : MonoBehaviour
    {
        [Header("游戏预制体")]
        public GameObject bulletPrefab;
        public GameObject enemyPrefab;
        public GameObject explosionPrefab;
        
        [Header("父对象")]
        public Transform bulletParent;
        public Transform enemyParent;
        public Transform effectParent;

        private void Start()
        {
            // 初始化对象池管理器
            PoolManager.Initialize();
            
            // 演示 PoolRegistry 解决的问题
            DemonstrateProblemsWithoutRegistry();
            DemonstrateSolutionsWithRegistry();
        }

        /// <summary>
        /// 演示没有 PoolRegistry 时遇到的问题
        /// </summary>
        private void DemonstrateProblemsWithoutRegistry()
        {
            Debug.Log("=== 没有 PoolRegistry 的问题 ===");
            
            // 问题1: 多个系统可能重复创建同一个预制体的池
            CreateBulletPoolInWeaponSystem();
            CreateBulletPoolInEffectSystem(); // 可能重复创建！
            
            // 问题2: 只有预制体引用，不知道池名
            TryFindPoolByPrefabWithoutRegistry();
            
            // 问题3: 无法按业务逻辑分类管理
            TryFindPoolsByCategory();
        }

        /// <summary>
        /// 武器系统创建子弹池
        /// </summary>
        private void CreateBulletPoolInWeaponSystem()
        {
            Debug.Log("武器系统: 创建子弹池");
            
            // 武器系统不知道其他地方是否已经创建了子弹池
            if (!PoolManager.Exists("WeaponBulletPool"))
            {
                bulletPrefab.CreateGameObjectPool("WeaponBulletPool", bulletParent);
                Debug.Log("✓ 武器系统创建了 WeaponBulletPool");
            }
        }

        /// <summary>
        /// 特效系统也想创建子弹池（可能重复）
        /// </summary>
        private void CreateBulletPoolInEffectSystem()
        {
            Debug.Log("特效系统: 也想创建子弹池");
            
            // 特效系统也不知道武器系统已经创建了
            if (!PoolManager.Exists("EffectBulletPool"))
            {
                bulletPrefab.CreateGameObjectPool("EffectBulletPool", effectParent);
                Debug.Log("✓ 特效系统创建了 EffectBulletPool");
                Debug.LogWarning("⚠️ 问题：同一个预制体有了两个不同的池！");
            }
        }

        /// <summary>
        /// 尝试根据预制体查找池（没有 Registry 的困难）
        /// </summary>
        private void TryFindPoolByPrefabWithoutRegistry()
        {
            Debug.Log("AI系统: 我只有 bulletPrefab，怎么知道池名？");
            
            // ❌ 没有 PoolRegistry，只能猜测或硬编码
            string[] possibleNames = { "BulletPool", "WeaponBulletPool", "EffectBulletPool" };
            
            foreach (var name in possibleNames)
            {
                if (PoolManager.Exists(name))
                {
                    Debug.Log($"猜测找到池: {name}");
                }
            }
            
            Debug.LogWarning("⚠️ 问题：只能通过猜测或硬编码来查找池！");
        }

        /// <summary>
        /// 尝试按分类查找池
        /// </summary>
        private void TryFindPoolsByCategory()
        {
            Debug.Log("管理系统: 我想找到所有武器相关的池");
            Debug.LogWarning("⚠️ 问题：没有分类机制，无法按业务逻辑查找！");
        }

        /// <summary>
        /// 演示 PoolRegistry 提供的解决方案
        /// </summary>
        private void DemonstrateSolutionsWithRegistry()
        {
            Debug.Log("\n=== PoolRegistry 的解决方案 ===");
            
            // 清理之前的池，重新开始
            PoolManager.CleanupAll();
            PoolRegistry.Clear();
            
            // 解决方案1: 注册时建立预制体与池的关联
            CreatePoolsWithRegistry();
            
            // 解决方案2: 根据预制体快速查找池
            FindPoolByPrefabWithRegistry();
            
            // 解决方案3: 按标签分类管理
            FindPoolsByTagWithRegistry();
            
            // 解决方案4: 避免重复创建
            AvoidDuplicateCreationWithRegistry();
        }

        /// <summary>
        /// 使用 PoolRegistry 创建池
        /// </summary>
        private void CreatePoolsWithRegistry()
        {
            Debug.Log("使用 PoolRegistry 创建池并注册");
            
            // 创建池的同时自动注册到 PoolRegistry
            bulletPrefab.CreateGameObjectPool("BulletPool", bulletParent);
            PoolRegistry.RegisterPool("BulletPool", bulletPrefab, bulletParent, typeof(GameObject), "Weapon", "Projectile");
            
            enemyPrefab.CreateGameObjectPool("EnemyPool", enemyParent);
            PoolRegistry.RegisterPool("EnemyPool", enemyPrefab, enemyParent, typeof(GameObject), "AI", "Character");
            
            explosionPrefab.CreateGameObjectPool("ExplosionPool", effectParent);
            PoolRegistry.RegisterPool("ExplosionPool", explosionPrefab, effectParent, typeof(GameObject), "Effect", "Visual");
            
            Debug.Log("✓ 所有池都已注册到 PoolRegistry");
        }

        /// <summary>
        /// 根据预制体查找池
        /// </summary>
        private void FindPoolByPrefabWithRegistry()
        {
            Debug.Log("AI系统: 使用 PoolRegistry 根据预制体查找池");
            
            // ✅ 现在可以直接根据预制体找到池名
            string poolName = PoolRegistry.FindPoolNameByPrefab(bulletPrefab);
            if (!string.IsNullOrEmpty(poolName))
            {
                Debug.Log($"✓ 找到池: {poolName}");
                
                // 可以直接使用
                var bullet = PoolExtensions.SpawnFromPool(poolName, Vector3.zero);
                if (bullet != null)
                {
                    Debug.Log("✓ 成功生成子弹");
                    bullet.ReturnToPool(poolName);
                }
            }
        }

        /// <summary>
        /// 按标签查找池
        /// </summary>
        private void FindPoolsByTagWithRegistry()
        {
            Debug.Log("管理系统: 使用标签查找相关池");
            
            // ✅ 按标签查找所有武器相关的池
            var weaponPools = PoolRegistry.FindPoolsByTag("Weapon");
            Debug.Log($"✓ 找到 {weaponPools.Count} 个武器池: {string.Join(", ", weaponPools)}");
            
            // ✅ 按标签查找所有特效相关的池
            var effectPools = PoolRegistry.FindPoolsByTag("Effect");
            Debug.Log($"✓ 找到 {effectPools.Count} 个特效池: {string.Join(", ", effectPools)}");
        }

        /// <summary>
        /// 避免重复创建
        /// </summary>
        private void AvoidDuplicateCreationWithRegistry()
        {
            Debug.Log("特效系统: 检查是否已有子弹池");
            
            // ✅ 先检查是否已经有这个预制体的池
            var existingPools = PoolRegistry.FindPoolNamesByPrefab(bulletPrefab);
            if (existingPools.Count > 0)
            {
                Debug.Log($"✓ 发现已存在的池: {string.Join(", ", existingPools)}");
                Debug.Log("✓ 直接复用，避免重复创建");
            }
            else
            {
                Debug.Log("没有找到现有池，创建新的");
            }
        }

        /// <summary>
        /// 显示当前注册信息
        /// </summary>
        [ContextMenu("显示注册信息")]
        public void ShowRegistryInfo()
        {
            Debug.Log("=== 当前 PoolRegistry 信息 ===");
            
            var registrations = PoolRegistry.GetAllRegistrations();
            foreach (var kvp in registrations)
            {
                var reg = kvp.Value;
                Debug.Log($"池名: {reg.PoolName}");
                Debug.Log($"  预制体: {reg.Prefab?.name ?? "None"}");
                Debug.Log($"  父对象: {reg.Parent?.name ?? "None"}");
                Debug.Log($"  标签: {string.Join(", ", reg.Tags ?? new string[0])}");
                Debug.Log($"  注册时间: {reg.RegisterTime}");
            }
        }

        private void OnDestroy()
        {
            // 清理
            PoolManager.Destroy();
            PoolRegistry.Clear();
        }
    }
}