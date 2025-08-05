using System;
using UnityEngine;

namespace UniFramework.ObjectPool.Examples
{
    /// <summary>
    /// 演示三个核心类关系的示例
    /// </summary>
    public class RelationshipExample : MonoBehaviour
    {
        // 示例类
        public class Bullet
        {
            public Vector3 Position { get; set; }
            public Vector3 Velocity { get; set; }
            public bool IsActive { get; set; }

            public void Reset()
            {
                Position = Vector3.zero;
                Velocity = Vector3.zero;
                IsActive = false;
            }
        }

        void Start()
        {
            // 演示三种不同的使用方式
            DemonstratePoolManagerUsage();
            DemonstrateDirectPoolUsage();
            DemonstratePooledObjectUsage();
        }

        /// <summary>
        /// 方式1：通过 PoolManager 使用（最常用）
        /// </summary>
        void DemonstratePoolManagerUsage()
        {
            Debug.Log("=== PoolManager 使用方式 ===");

            // 1. 创建对象池（PoolManager 内部会创建 UniObjectPool<Bullet> 实例）
            PoolManager.CreatePool<Bullet>(
                createFunc: () => new Bullet(),
                resetAction: bullet => bullet.Reset(),
                destroyAction: bullet => { /* 清理逻辑 */ }
            );

            // 2. 获取对象（PoolManager 转发到内部的 UniObjectPool<Bullet>）
            Bullet bullet1 = PoolManager.Get<Bullet>();
            bullet1.Position = new Vector3(1, 0, 0);
            bullet1.IsActive = true;

            Debug.Log($"获取子弹: Position={bullet1.Position}, Active={bullet1.IsActive}");

            // 3. 归还对象（PoolManager 转发到内部的 UniObjectPool<Bullet>）
            PoolManager.Return(bullet1);

            Debug.Log("子弹已归还到池中");
        }

        /// <summary>
        /// 方式2：直接操作 UniObjectPool 实例（高性能）
        /// </summary>
        void DemonstrateDirectPoolUsage()
        {
            Debug.Log("=== 直接操作 UniObjectPool 方式 ===");

            // 1. 获取池实例（避免每次通过 PoolManager 查找）
            UniObjectPool<Bullet> bulletPool = PoolManager.GetPool<Bullet>();

            if (bulletPool != null)
            {
                // 2. 直接从池获取对象
                Bullet bullet2 = bulletPool.Get();
                bullet2.Position = new Vector3(2, 0, 0);
                bullet2.IsActive = true;

                Debug.Log($"直接获取子弹: Position={bullet2.Position}, Active={bullet2.IsActive}");
                Debug.Log($"池状态: Available={bulletPool.AvailableCount}, Active={bulletPool.ActiveCount}");

                // 3. 直接归还到池
                bulletPool.Return(bullet2);

                Debug.Log($"归还后池状态: Available={bulletPool.AvailableCount}, Active={bulletPool.ActiveCount}");
            }
        }

        /// <summary>
        /// 方式3：使用 PooledObject 包装器（最安全）
        /// </summary>
        void DemonstratePooledObjectUsage()
        {
            Debug.Log("=== PooledObject 包装器方式 ===");

            UniObjectPool<Bullet> bulletPool = PoolManager.GetPool<Bullet>();

            if (bulletPool != null)
            {
                // 1. 创建 PooledObject 包装器
                using (var pooledBullet = new PooledObject<Bullet>(bulletPool.Get(), bulletPool))
                {
                    // 2. 通过 Value 属性访问实际对象
                    Bullet bullet3 = pooledBullet.Value;
                    bullet3.Position = new Vector3(3, 0, 0);
                    bullet3.IsActive = true;

                    Debug.Log($"包装器子弹: Position={bullet3.Position}, Active={bullet3.IsActive}");

                    // 3. 支持隐式转换
                    Bullet implicitBullet = pooledBullet; // 隐式转换为 Bullet
                    Debug.Log($"隐式转换: Position={implicitBullet.Position}");

                    // 4. using 语句结束时自动归还（调用 Dispose）
                    Debug.Log("即将自动归还到池中...");
                }

                Debug.Log("PooledObject 已自动归还");
            }
        }

        /// <summary>
        /// 演示命名池的使用
        /// </summary>
        void DemonstrateNamedPools()
        {
            Debug.Log("=== 命名池使用方式 ===");

            // 创建不同用途的子弹池
            PoolManager.CreatePool<Bullet>("PlayerBullets", () => new Bullet());
            PoolManager.CreatePool<Bullet>("EnemyBullets", () => new Bullet());

            // 从不同的池获取对象
            Bullet playerBullet = PoolManager.Get<Bullet>("PlayerBullets");
            Bullet enemyBullet = PoolManager.Get<Bullet>("EnemyBullets");

            // 归还到对应的池
            PoolManager.Return("PlayerBullets", playerBullet);
            PoolManager.Return("EnemyBullets", enemyBullet);

            Debug.Log("命名池演示完成");
        }

        /// <summary>
        /// 演示错误的使用方式
        /// </summary>
        void DemonstrateCommonMistakes()
        {
            Debug.Log("=== 常见错误演示 ===");

            // 错误1：忘记归还对象
            Bullet forgottenBullet = PoolManager.Get<Bullet>();
            // 忘记调用 PoolManager.Return(forgottenBullet); 
            // 这会导致对象泄漏

            // 错误2：重复归还同一个对象
            Bullet bullet = PoolManager.Get<Bullet>();
            PoolManager.Return(bullet);
            // PoolManager.Return(bullet); // 错误：重复归还

            // 错误3：归还后继续使用对象
            Bullet returnedBullet = PoolManager.Get<Bullet>();
            PoolManager.Return(returnedBullet);
            // returnedBullet.Position = Vector3.zero; // 错误：归还后继续使用

            Debug.Log("错误演示完成（实际代码中应避免这些错误）");
        }

        void OnDestroy()
        {
            // 清理所有对象池
            PoolManager.Destroy();
        }
    }

    /// <summary>
    /// 展示三者关系的简化版本
    /// </summary>
    public static class RelationshipSummary
    {
        /*
         * 关系总结：
         * 
         * 1. UniObjectPool<T>：
         *    - 核心实现类
         *    - 管理具体类型的对象池
         *    - 提供 Get/Return/Cleanup 等方法
         * 
         * 2. PoolManager：
         *    - 全局管理器
         *    - 管理多个 UniObjectPool<T> 实例
         *    - 提供统一的访问接口
         *    - 支持类型池和命名池
         * 
         * 3. PooledObject<T>：
         *    - 包装器类
         *    - 包装从 UniObjectPool<T> 获取的对象
         *    - 提供 RAII 语义，自动归还
         * 
         * 数据流：
         * PoolManager.Get<T>() 
         *   → 查找 UniObjectPool<T> 
         *   → 调用 pool.Get() 
         *   → 返回 T 对象
         *   → 可选择用 PooledObject<T> 包装
         * 
         * PoolManager.Return<T>(obj) 
         *   → 查找 UniObjectPool<T> 
         *   → 调用 pool.Return(obj)
         * 
         * PooledObject<T>.Dispose() 
         *   → 调用 pool.Return(value)
         */
    }
}