using UnityEngine;
using UniFramework.ObjectPool;

namespace UniFramework.ObjectPool.Examples
{
    /// <summary>
    /// 为什么需要 PoolRegistry？
    /// 用最简单的例子说明问题
    /// </summary>
    public class WhyPoolRegistryExample : MonoBehaviour
    {
        [Header("预制体")]
        public GameObject bulletPrefab;
        
        [Header("父对象")]
        public Transform bulletParent;

        private void Start()
        {
            PoolManager.Initialize();
            
            Debug.Log("=== 场景1：没有 PoolRegistry 的问题 ===");
            ScenarioWithoutRegistry();
            
            Debug.Log("\n=== 场景2：使用 PoolRegistry 的解决方案 ===");
            ScenarioWithRegistry();
        }

        /// <summary>
        /// 场景1：没有 PoolRegistry 时的问题
        /// </summary>
        private void ScenarioWithoutRegistry()
        {
            // 清理之前的状态
            PoolManager.CleanupAll();
            
            // 假设你是武器系统的程序员
            WeaponSystemProgrammer();
            
            // 假设你是特效系统的程序员（不同的人）
            EffectSystemProgrammer();
            
            // 假设你是AI系统的程序员（又是不同的人）
            AISystemProgrammer();
        }

        /// <summary>
        /// 武器系统程序员的代码
        /// </summary>
        private void WeaponSystemProgrammer()
        {
            Debug.Log("【武器程序员】我需要一个子弹池");
            
            // 武器程序员创建了一个子弹池
            bulletPrefab.CreateGameObjectPool("WeaponBullets", bulletParent);
            
            Debug.Log("✓ 创建了池：WeaponBullets");
        }

        /// <summary>
        /// 特效系统程序员的代码
        /// </summary>
        private void EffectSystemProgrammer()
        {
            Debug.Log("【特效程序员】我也需要子弹池做击中特效");
            
            // 特效程序员不知道武器程序员已经创建了子弹池
            // 他只知道自己有 bulletPrefab，但不知道池名
            
            // 他只能猜测或者创建新的池
            if (!PoolManager.HasPool("BulletPool"))
            {
                bulletPrefab.CreateGameObjectPool("BulletPool", bulletParent);
                Debug.Log("✓ 创建了池：BulletPool");
            }
            
            Debug.LogWarning("❌ 问题：现在有两个子弹池了！WeaponBullets 和 BulletPool");
        }

        /// <summary>
        /// AI系统程序员的代码
        /// </summary>
        private void AISystemProgrammer()
        {
            Debug.Log("【AI程序员】我想让AI也能发射子弹");
            
            // AI程序员只有 bulletPrefab，但不知道池名
            // 他面临的问题：
            Debug.LogError("❌ 我有 bulletPrefab，但我不知道池的名字！");
            Debug.LogError("❌ 我该用 'WeaponBullets' 还是 'BulletPool'？");
            Debug.LogError("❌ 我怎么知道哪个池是正确的？");
            
            // 他只能硬编码尝试
            TryHardcodedPoolNames();
        }

        /// <summary>
        /// 硬编码尝试池名（不好的做法）
        /// </summary>
        private void TryHardcodedPoolNames()
        {
            string[] guessNames = { "BulletPool", "WeaponBullets", "Bullets", "ProjectilePool" };
            
            foreach (string poolName in guessNames)
            {
                if (PoolManager.HasPool(poolName))
                {
                    Debug.Log($"猜中了池名：{poolName}");
                    return;
                }
            }
            
            Debug.LogError("❌ 猜不到池名，只能自己再创建一个...");
        }

        /// <summary>
        /// 场景2：使用 PoolRegistry 的解决方案
        /// </summary>
        private void ScenarioWithRegistry()
        {
            // 清理之前的状态
            PoolManager.CleanupAll();
            PoolRegistry.Clear();
            
            // 现在用 PoolRegistry 重新实现
            WeaponSystemWithRegistry();
            EffectSystemWithRegistry();
            AISystemWithRegistry();
        }

        /// <summary>
        /// 武器系统使用 PoolRegistry
        /// </summary>
        private void WeaponSystemWithRegistry()
        {
            Debug.Log("【武器程序员】使用 PoolRegistry 创建子弹池");
            
            // 创建池
            bulletPrefab.CreateGameObjectPool("WeaponBullets", bulletParent);
            
            // 关键：注册到 PoolRegistry
            PoolRegistry.RegisterPool("WeaponBullets", bulletPrefab, bulletParent, typeof(GameObject), "Weapon");
            
            Debug.Log("✓ 创建并注册了池：WeaponBullets");
        }

        /// <summary>
        /// 特效系统使用 PoolRegistry
        /// </summary>
        private void EffectSystemWithRegistry()
        {
            Debug.Log("【特效程序员】检查是否已有子弹池");
            
            // 关键：根据预制体查找现有的池
            string existingPool = PoolRegistry.FindPoolNameByPrefab(bulletPrefab);
            
            if (!string.IsNullOrEmpty(existingPool))
            {
                Debug.Log($"✓ 发现已有池：{existingPool}，直接使用！");
                
                // 可以直接使用现有的池
                var bullet = PoolExtensions.SpawnFromPool(existingPool, Vector3.zero);
                if (bullet != null)
                {
                    Debug.Log("✓ 成功从现有池生成子弹");
                    bullet.ReturnToPool(existingPool);
                }
            }
            else
            {
                Debug.Log("没有找到现有池，创建新的");
            }
        }

        /// <summary>
        /// AI系统使用 PoolRegistry
        /// </summary>
        private void AISystemWithRegistry()
        {
            Debug.Log("【AI程序员】根据预制体查找池");
            
            // 关键：不需要猜测，直接根据预制体查找
            string poolName = PoolRegistry.FindPoolNameByPrefab(bulletPrefab);
            
            if (!string.IsNullOrEmpty(poolName))
            {
                Debug.Log($"✓ 找到池：{poolName}");
                
                // 直接使用
                var bullet = PoolExtensions.SpawnFromPool(poolName, Vector3.zero);
                if (bullet != null)
                {
                    Debug.Log("✓ AI成功发射子弹");
                    bullet.ReturnToPool(poolName);
                }
            }
            else
            {
                Debug.LogError("❌ 没有找到对应的池");
            }
        }

        /// <summary>
        /// 显示对比结果
        /// </summary>
        [ContextMenu("显示对比结果")]
        public void ShowComparison()
        {
            Debug.Log("=== 对比总结 ===");
            Debug.Log("没有 PoolRegistry：");
            Debug.Log("  ❌ 多个程序员可能创建重复的池");
            Debug.Log("  ❌ 只有预制体，不知道池名");
            Debug.Log("  ❌ 需要硬编码或猜测池名");
            Debug.Log("  ❌ 代码耦合度高");
            
            Debug.Log("\n有了 PoolRegistry：");
            Debug.Log("  ✓ 根据预制体自动找到池名");
            Debug.Log("  ✓ 避免重复创建");
            Debug.Log("  ✓ 不需要硬编码池名");
            Debug.Log("  ✓ 代码解耦，更易维护");
        }

        private void OnDestroy()
        {
            PoolManager.Destroy();
            PoolRegistry.Clear();
        }
    }
}