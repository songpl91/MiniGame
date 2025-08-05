using System.Collections.Generic;
using UnityEngine;
using UniFramework.ObjectPool;

namespace UniFramework.ObjectPool.Examples
{
    /// <summary>
    /// 常量池名 vs PoolRegistry 对比示例
    /// 展示两种方案的优缺点
    /// </summary>
    public class ConstantVsRegistryComparison : MonoBehaviour
    {
        [Header("预制体")]
        public GameObject bulletPrefab;
        public GameObject enemyPrefab;
        public GameObject explosionPrefab;
        
        [Header("父对象")]
        public Transform poolParent;

        private void Start()
        {
            PoolManager.Initialize();
            
            Debug.Log("=== 常量池名 vs PoolRegistry 对比 ===");
            
            // 方案1：常量池名
            DemonstrateConstantPoolNames();
            
            // 方案2：PoolRegistry
            DemonstratePoolRegistry();
            
            // 对比分析
            CompareApproaches();
        }

        #region 方案1：常量池名

        /// <summary>
        /// 方案1：使用常量池名
        /// </summary>
        private void DemonstrateConstantPoolNames()
        {
            Debug.Log("\n=== 方案1：常量池名 ===");
            
            // 创建池
            CreatePoolsWithConstants();
            
            // 使用池
            UsePoolsWithConstants();
        }

        /// <summary>
        /// 使用常量创建池
        /// </summary>
        private void CreatePoolsWithConstants()
        {
            Debug.Log("创建对象池（常量方式）");
            
            // 直接使用常量名创建池
            bulletPrefab.CreateGameObjectPool(PoolNames.BULLET_POOL, poolParent);
            enemyPrefab.CreateGameObjectPool(PoolNames.ENEMY_POOL, poolParent);
            explosionPrefab.CreateGameObjectPool(PoolNames.EXPLOSION_POOL, poolParent);
            
            Debug.Log("✓ 所有池创建完成");
        }

        /// <summary>
        /// 使用常量池名
        /// </summary>
        private void UsePoolsWithConstants()
        {
            Debug.Log("使用对象池（常量方式）");
            
            // 武器系统
            WeaponSystemWithConstants();
            
            // 敌人系统
            EnemySystemWithConstants();
            
            // 特效系统
            EffectSystemWithConstants();
        }

        private void WeaponSystemWithConstants()
        {
            Debug.Log("【武器系统】发射子弹");
            var bullet = PoolExtensions.SpawnFromPool(PoolNames.BULLET_POOL, Vector3.zero);
            if (bullet != null)
            {
                Debug.Log("✓ 子弹生成成功");
                bullet.ReturnToPool(PoolNames.BULLET_POOL);
            }
        }

        private void EnemySystemWithConstants()
        {
            Debug.Log("【敌人系统】生成敌人");
            var enemy = PoolExtensions.SpawnFromPool(PoolNames.ENEMY_POOL, Vector3.zero);
            if (enemy != null)
            {
                Debug.Log("✓ 敌人生成成功");
                enemy.ReturnToPool(PoolNames.ENEMY_POOL);
            }
        }

        private void EffectSystemWithConstants()
        {
            Debug.Log("【特效系统】播放爆炸");
            var explosion = PoolExtensions.SpawnFromPool(PoolNames.EXPLOSION_POOL, Vector3.zero);
            if (explosion != null)
            {
                Debug.Log("✓ 爆炸特效生成成功");
                explosion.ReturnToPool(PoolNames.EXPLOSION_POOL);
            }
        }

        #endregion

        #region 方案2：PoolRegistry

        /// <summary>
        /// 方案2：使用PoolRegistry
        /// </summary>
        private void DemonstratePoolRegistry()
        {
            Debug.Log("\n=== 方案2：PoolRegistry ===");
            
            // 清理之前的池
            PoolManager.CleanupAll();
            PoolRegistry.Clear();
            
            // 创建池并注册
            CreatePoolsWithRegistry();
            
            // 使用池
            UsePoolsWithRegistry();
        }

        /// <summary>
        /// 使用PoolRegistry创建池
        /// </summary>
        private void CreatePoolsWithRegistry()
        {
            Debug.Log("创建对象池（PoolRegistry方式）");
            
            // 创建池并注册到Registry
            bulletPrefab.CreateGameObjectPool(PoolNames.BULLET_POOL, poolParent);
            PoolRegistry.RegisterPool(PoolNames.BULLET_POOL, bulletPrefab, poolParent, typeof(GameObject), "Weapon", "Projectile");
            
            enemyPrefab.CreateGameObjectPool(PoolNames.ENEMY_POOL, poolParent);
            PoolRegistry.RegisterPool(PoolNames.ENEMY_POOL, enemyPrefab, poolParent, typeof(GameObject), "AI", "Character");
            
            explosionPrefab.CreateGameObjectPool(PoolNames.EXPLOSION_POOL, poolParent);
            PoolRegistry.RegisterPool(PoolNames.EXPLOSION_POOL, explosionPrefab, poolParent, typeof(GameObject), "Effect", "Visual");
            
            Debug.Log("✓ 所有池创建并注册完成");
        }

        /// <summary>
        /// 使用PoolRegistry
        /// </summary>
        private void UsePoolsWithRegistry()
        {
            Debug.Log("使用对象池（PoolRegistry方式）");
            
            // 武器系统：根据预制体查找池
            WeaponSystemWithRegistry();
            
            // 敌人系统：根据预制体查找池
            EnemySystemWithRegistry();
            
            // 特效系统：根据标签查找池
            EffectSystemWithRegistry();
        }

        private void WeaponSystemWithRegistry()
        {
            Debug.Log("【武器系统】根据预制体查找池");
            string poolName = PoolRegistry.FindPoolNameByPrefab(bulletPrefab);
            if (!string.IsNullOrEmpty(poolName))
            {
                var bullet = PoolExtensions.SpawnFromPool(poolName, Vector3.zero);
                if (bullet != null)
                {
                    Debug.Log($"✓ 子弹生成成功，使用池：{poolName}");
                    bullet.ReturnToPool(poolName);
                }
            }
        }

        private void EnemySystemWithRegistry()
        {
            Debug.Log("【敌人系统】根据预制体查找池");
            string poolName = PoolRegistry.FindPoolNameByPrefab(enemyPrefab);
            if (!string.IsNullOrEmpty(poolName))
            {
                var enemy = PoolExtensions.SpawnFromPool(poolName, Vector3.zero);
                if (enemy != null)
                {
                    Debug.Log($"✓ 敌人生成成功，使用池：{poolName}");
                    enemy.ReturnToPool(poolName);
                }
            }
        }

        private void EffectSystemWithRegistry()
        {
            Debug.Log("【特效系统】根据标签查找特效池");
            var effectPools = PoolRegistry.FindPoolsByTag("Effect");
            foreach (string poolName in effectPools)
            {
                var effect = PoolExtensions.SpawnFromPool(poolName, Vector3.zero);
                if (effect != null)
                {
                    Debug.Log($"✓ 特效生成成功，使用池：{poolName}");
                    effect.ReturnToPool(poolName);
                }
            }
        }

        #endregion

        #region 对比分析

        /// <summary>
        /// 对比两种方案
        /// </summary>
        private void CompareApproaches()
        {
            Debug.Log("\n=== 对比分析 ===");
            
            Debug.Log("【常量池名方案】");
            Debug.Log("✓ 优点：");
            Debug.Log("  - 简单直接，容易理解");
            Debug.Log("  - 性能最好，直接字符串查找");
            Debug.Log("  - 编译时检查，类型安全");
            Debug.Log("  - 代码量少，维护成本低");
            Debug.Log("  - 不需要额外的注册步骤");
            
            Debug.Log("❌ 缺点：");
            Debug.Log("  - 硬编码池名，耦合度高");
            Debug.Log("  - 无法根据预制体反查池名");
            Debug.Log("  - 缺乏元数据（标签、分类等）");
            Debug.Log("  - 难以实现动态池管理");
            Debug.Log("  - 调试信息有限");
            
            Debug.Log("\n【PoolRegistry方案】");
            Debug.Log("✓ 优点：");
            Debug.Log("  - 支持预制体反查池名");
            Debug.Log("  - 丰富的元数据支持");
            Debug.Log("  - 支持标签分类查找");
            Debug.Log("  - 便于调试和监控");
            Debug.Log("  - 支持动态池管理");
            Debug.Log("  - 避免重复创建池");
            
            Debug.Log("❌ 缺点：");
            Debug.Log("  - 增加了系统复杂度");
            Debug.Log("  - 需要额外的注册步骤");
            Debug.Log("  - 轻微的性能开销");
            Debug.Log("  - 学习成本稍高");
            Debug.Log("  - 代码量增加");
        }

        /// <summary>
        /// 展示具体场景下的差异
        /// </summary>
        [ContextMenu("展示场景差异")]
        public void ShowScenarioDifferences()
        {
            Debug.Log("=== 具体场景对比 ===");
            
            Debug.Log("\n【场景1：新手程序员】");
            Debug.Log("常量方案：⭐⭐⭐⭐⭐ 简单易懂，立即上手");
            Debug.Log("Registry方案：⭐⭐⭐ 需要理解注册概念");
            
            Debug.Log("\n【场景2：小型项目（<10个池）】");
            Debug.Log("常量方案：⭐⭐⭐⭐⭐ 完全够用，简单高效");
            Debug.Log("Registry方案：⭐⭐ 过度设计，增加复杂度");
            
            Debug.Log("\n【场景3：大型项目（>50个池）】");
            Debug.Log("常量方案：⭐⭐ 管理困难，容易出错");
            Debug.Log("Registry方案：⭐⭐⭐⭐⭐ 统一管理，便于维护");
            
            Debug.Log("\n【场景4：动态加载预制体】");
            Debug.Log("常量方案：⭐ 难以处理，需要额外机制");
            Debug.Log("Registry方案：⭐⭐⭐⭐⭐ 天然支持，根据预制体查找");
            
            Debug.Log("\n【场景5：多人协作开发】");
            Debug.Log("常量方案：⭐⭐⭐ 需要约定命名规范");
            Debug.Log("Registry方案：⭐⭐⭐⭐ 统一注册，减少冲突");
            
            Debug.Log("\n【场景6：性能敏感应用】");
            Debug.Log("常量方案：⭐⭐⭐⭐⭐ 零开销，直接查找");
            Debug.Log("Registry方案：⭐⭐⭐⭐ 轻微开销，可接受");
        }

        #endregion

        private void OnDestroy()
        {
            PoolManager.Destroy();
            PoolRegistry.Clear();
        }
    }

    /// <summary>
    /// 池名常量定义
    /// 这是常量方案的核心
    /// </summary>
    public static class PoolNames
    {
        // 武器相关
        public const string BULLET_POOL = "BulletPool";
        public const string ROCKET_POOL = "RocketPool";
        public const string GRENADE_POOL = "GrenadePool";
        
        // 敌人相关
        public const string ENEMY_POOL = "EnemyPool";
        public const string BOSS_POOL = "BossPool";
        
        // 特效相关
        public const string EXPLOSION_POOL = "ExplosionPool";
        public const string SMOKE_POOL = "SmokePool";
        public const string SPARK_POOL = "SparkPool";
        
        // UI相关
        public const string DAMAGE_TEXT_POOL = "DamageTextPool";
        public const string POPUP_POOL = "PopupPool";
    }
}