using UnityEngine;
using System.Collections.Generic;

namespace UniFramework.ObjectPool.Examples
{
    /// <summary>
    /// UniObjectPool 使用示例
    /// 展示各种对象池的使用方法
    /// </summary>
    public class UsageExample : MonoBehaviour
    {
        [Header("示例配置")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform bulletParent;
        [SerializeField] private Transform enemyParent;

        private UniObjectPool<PoolableStringBuilder> _stringBuilderPool;
        private UniObjectPool<PoolableList<int>> _intListPool;
        private UniObjectPool<GameObject> _bulletPool;
        private UniObjectPool<GameObject> _enemyPool;

        void Start()
        {
            // 初始化对象池管理器
            PoolManager.Initialize();

            // 创建各种类型的对象池
            CreatePools();

            // 演示基本用法
            DemonstrateBasicUsage();

            // 演示高级用法
            DemonstrateAdvancedUsage();

            // 演示 GameObject 池化
            DemonstrateGameObjectPooling();
        }

        /// <summary>
        /// 创建各种对象池
        /// </summary>
        private void CreatePools()
        {
            // 1. 创建 StringBuilder 对象池
            _stringBuilderPool = PoolableStringBuilder.CreatePool(
                initialCapacity: 256,
                config: PoolConfig.CreateHighPerformance()
            );

            // 2. 创建 List<int> 对象池
            _intListPool = PoolableList<int>.CreatePool(
                initialCapacity: 32,
                config: PoolConfig.CreateDefault()
            );

            // 3. 创建 GameObject 对象池（子弹）
            if (bulletPrefab != null)
            {
                _bulletPool = bulletPrefab.CreateGameObjectPool(
                    parent: bulletParent,
                    config: PoolConfig.CreateHighPerformance()
                );
                bulletPrefab.SpawnFromPool();
                bulletPrefab.ReturnToPool();
            }

            // 4. 创建 GameObject 对象池（敌人）
            if (enemyPrefab != null)
            {
                _enemyPool = enemyPrefab.CreateGameObjectPool(
                    parent: enemyParent,
                    config: PoolConfig.CreateMemoryOptimized()
                );

            }

            UniLogger.Log("所有对象池创建完成");
        }

        /// <summary>
        /// 演示基本用法
        /// </summary>
        private void DemonstrateBasicUsage()
        {
            UniLogger.Log("=== 演示基本用法 ===");

            // 使用 StringBuilder 对象池
            var sb = _stringBuilderPool.Get();
            sb.Append("Hello ").Append("World ").Append(123);
            string result = sb.ToString();
            UniLogger.Log($"StringBuilder 结果: {result}");
            _stringBuilderPool.Return(sb);

            // 使用 List<int> 对象池
            var list = _intListPool.Get();
            list.Add(1);
            list.Add(2);
            list.Add(3);
            UniLogger.Log($"List 元素数量: {list.Count}");
            _intListPool.Return(list);
        }

        /// <summary>
        /// 演示高级用法（使用 using 语句自动归还）
        /// </summary>
        private void DemonstrateAdvancedUsage()
        {
            UniLogger.Log("=== 演示高级用法 ===");

            // 使用 using 语句自动归还对象
            using (var pooledSb = _stringBuilderPool.Get().AsPooled(_stringBuilderPool))
            {
                pooledSb.Value.Append("自动归还测试: ").Append(Time.time);
                UniLogger.Log($"自动归还结果: {pooledSb.Value}");
                // 离开 using 作用域时自动归还到对象池
            }

            // 使用扩展方法
            using (var pooledList = _intListPool.Get().AsPooled(_intListPool))
            {
                for (int i = 0; i < 10; i++)
                {
                    pooledList.Value.Add(i * i);
                }
                UniLogger.Log($"平方数列表: [{string.Join(", ", pooledList.Value.ToArray())}]");
                // 自动归还
            }
        }

        /// <summary>
        /// 演示 GameObject 池化
        /// </summary>
        private void DemonstrateGameObjectPooling()
        {
            if (bulletPrefab == null || enemyPrefab == null)
            {
                UniLogger.Warning("预制体未设置，跳过 GameObject 池化演示");
                return;
            }

            UniLogger.Log("=== 演示 GameObject 池化 ===");

            // 创建一些子弹
            List<GameObject> bullets = new List<GameObject>();
            for (int i = 0; i < 5; i++)
            {
                var bullet = bulletPrefab.SpawnFromPool(
                    position: new Vector3(i * 2f, 0, 0),
                    rotation: Quaternion.identity,
                    parent: bulletParent
                );
                bullets.Add(bullet);
            }

            // 延迟归还子弹
            Invoke(nameof(ReturnBullets), 2f);

            // 创建一些敌人
            for (int i = 0; i < 3; i++)
            {
                var enemy = enemyPrefab.SpawnFromPool(
                    position: new Vector3(i * 3f, 0, 5f),
                    rotation: Quaternion.identity,
                    parent: enemyParent
                );
                
                // 3秒后归还敌人
                StartCoroutine(ReturnEnemyAfterDelay(enemy, 3f));
            }
        }

        /// <summary>
        /// 归还子弹到对象池
        /// </summary>
        private void ReturnBullets()
        {
            // 查找所有子弹并归还
            var bullets = FindObjectsOfType<GameObject>();
            foreach (var bullet in bullets)
            {
                if (bullet.name.Contains(bulletPrefab.name))
                {
                    bullet.ReturnToPool();
                }
            }
            UniLogger.Log("所有子弹已归还到对象池");
        }

        /// <summary>
        /// 延迟归还敌人
        /// </summary>
        /// <param name="enemy">敌人对象</param>
        /// <param name="delay">延迟时间</param>
        /// <returns>协程</returns>
        private System.Collections.IEnumerator ReturnEnemyAfterDelay(GameObject enemy, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (enemy != null)
            {
                enemy.ReturnToPool();
                UniLogger.Log($"敌人 {enemy.name} 已归还到对象池");
            }
        }

        /// <summary>
        /// 显示统计信息
        /// </summary>
        [ContextMenu("显示统计信息")]
        public void ShowStatistics()
        {
            string stats = PoolManager.GetAllStatistics();
            UniLogger.Log(stats);
            Debug.Log(stats);
        }

        [MenuItem("UniObjectPool/显示统计信息")]
        public static void ShowStatistics()
        {
            Debug.Log("=== 对象池统计信息 ===");
            
            // 显示StringBuilder池统计
            var stringBuilderPool = PoolManager.GetPool<StringBuilder>();
            if (stringBuilderPool != null)
            {
                var stats = stringBuilderPool.Statistics;
                Debug.Log($"StringBuilder池 - 可用: {stringBuilderPool.AvailableCount}, 活跃: {stringBuilderPool.ActiveCount}, 总创建: {stats.TotalCreated}, 总销毁: {stats.TotalDestroyed}");
            }
            
            // 显示List<int>池统计
            var listPool = PoolManager.GetPool<List<int>>();
            if (listPool != null)
            {
                var stats = listPool.Statistics;
                Debug.Log($"List<int>池 - 可用: {listPool.AvailableCount}, 活跃: {listPool.ActiveCount}, 总创建: {stats.TotalCreated}, 总销毁: {stats.TotalDestroyed}");
            }
            
            // 显示GameObject池统计
            var bulletPool = PoolManager.GetPool<GameObject>("BulletPool");
            if (bulletPool != null)
            {
                var stats = bulletPool.Statistics;
                Debug.Log($"子弹池 - 可用: {bulletPool.AvailableCount}, 活跃: {bulletPool.ActiveCount}, 总创建: {stats.TotalCreated}, 总销毁: {stats.TotalDestroyed}");
            }
            
            var enemyPool = PoolManager.GetPool<GameObject>("EnemyPool");
            if (enemyPool != null)
            {
                var stats = enemyPool.Statistics;
                Debug.Log($"敌人池 - 可用: {enemyPool.AvailableCount}, 活跃: {enemyPool.ActiveCount}, 总创建: {stats.TotalCreated}, 总销毁: {stats.TotalDestroyed}");
            }
        }

        [MenuItem("UniObjectPool/显示注册器信息")]
        public static void ShowRegistryInfo()
        {
            Debug.Log("=== 对象池注册器信息 ===");
            
            var registrations = PoolRegistry.GetAllRegistrations();
            foreach (var registration in registrations)
            {
                Debug.Log($"池名称: {registration.PoolName}");
                Debug.Log($"  预制体: {(registration.Prefab ? registration.Prefab.name : "无")}");
                Debug.Log($"  父对象: {(registration.Parent ? registration.Parent.name : "无")}");
                Debug.Log($"  类型: {registration.ObjectType.Name}");
                Debug.Log($"  注册时间: {registration.RegistrationTime}");
                Debug.Log($"  标签: {string.Join(", ", registration.Tags)}");
                Debug.Log("---");
            }
        }

        /// <summary>
        /// 清理所有对象池
        /// </summary>
        [ContextMenu("清理所有对象池")]
        public void CleanupAllPools()
        {
            PoolManager.CleanupAll();
            UniLogger.Log("所有对象池已清理");
        }

        void OnDestroy()
        {
            // 应用退出时清理资源
            PoolManager.Destroy();
        }

        void OnGUI()
        {
            // 在屏幕上显示简单的统计信息
            if (PoolManager.PoolCount > 0)
            {
                GUILayout.BeginArea(new Rect(10, 10, 300, 200));
                GUILayout.Label($"对象池数量: {PoolManager.PoolCount}");
                
                if (_stringBuilderPool != null)
                {
                    GUILayout.Label($"StringBuilder 池 - 可用: {_stringBuilderPool.AvailableCount}, 活跃: {_stringBuilderPool.ActiveCount}");
                }
                
                if (_intListPool != null)
                {
                    GUILayout.Label($"IntList 池 - 可用: {_intListPool.AvailableCount}, 活跃: {_intListPool.ActiveCount}");
                }
                
                if (_bulletPool != null)
                {
                    GUILayout.Label($"子弹池 - 可用: {_bulletPool.AvailableCount}, 活跃: {_bulletPool.ActiveCount}");
                }
                
                if (_enemyPool != null)
                {
                    GUILayout.Label($"敌人池 - 可用: {_enemyPool.AvailableCount}, 活跃: {_enemyPool.ActiveCount}");
                }
                
                if (GUILayout.Button("显示详细统计"))
                {
                    ShowStatistics();
                }
                
                if (GUILayout.Button("清理所有对象池"))
                {
                    CleanupAllPools();
                }
                
                GUILayout.EndArea();
            }
        }
    }
}