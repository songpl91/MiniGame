using System;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.ObjectPool;

namespace UniFramework.ObjectPool.Examples
{
    /// <summary>
    /// UniObjectPool 简洁使用示例
    /// 展示明确命名策略的最佳实践
    /// </summary>
    public class SimpleUsageExample : MonoBehaviour
    {
        [Header("预制体设置")]
        public GameObject bulletPrefab;
        public GameObject enemyPrefab;
        public GameObject effectPrefab;
        
        [Header("父对象设置")]
        public Transform bulletParent;
        public Transform enemyParent;
        public Transform effectParent;
        
        // 对象池名称常量 - 这是推荐的做法
        private const string BULLET_POOL = "BulletPool";
        private const string ENEMY_POOL = "EnemyPool";
        private const string EFFECT_POOL = "EffectPool";
        
        // 存储生成的对象，用于演示归还
        private List<GameObject> spawnedBullets = new List<GameObject>();
        private List<GameObject> spawnedEnemies = new List<GameObject>();
        
        private void Start()
        {
            // 初始化对象池管理器
            PoolManager.Initialize();
            
            // 创建对象池 - 使用明确的名称
            CreateObjectPools();
        }
        
        /// <summary>
        /// 创建所有需要的对象池
        /// </summary>
        private void CreateObjectPools()
        {
            Debug.Log("=== 创建对象池 ===");
            
            // 方式1: 使用明确指定的名称（推荐）
            if (bulletPrefab != null)
            {
                bulletPrefab.CreateGameObjectPool(
                    poolName: BULLET_POOL,
                    parent: bulletParent,
                    config: PoolConfig.CreateHighPerformance()
                );
                Debug.Log($"创建子弹对象池: {BULLET_POOL}");
            }
            
            if (enemyPrefab != null)
            {
                enemyPrefab.CreateGameObjectPool(
                    poolName: ENEMY_POOL,
                    parent: enemyParent,
                    config: PoolConfig.CreateDefault()
                );
                Debug.Log($"创建敌人对象池: {ENEMY_POOL}");
            }
            
            // 方式2: 使用默认命名规则（适合简单场景）
            if (effectPrefab != null)
            {
                effectPrefab.CreateGameObjectPoolWithDefaultName(
                    parent: effectParent,
                    config: PoolConfig.CreateMemoryOptimized()
                );
                Debug.Log($"创建特效对象池: {effectPrefab.name}Pool");
            }
        }
        
        /// <summary>
        /// 生成子弹
        /// </summary>
        [ContextMenu("生成子弹")]
        public void SpawnBullets()
        {
            if (bulletPrefab == null) return;
            
            for (int i = 0; i < 5; i++)
            {
                // 明确指定对象池名称
                var bullet = bulletPrefab.SpawnFromPool(
                    poolName: BULLET_POOL,
                    position: new Vector3(i * 2f, 0, 0),
                    rotation: Quaternion.identity
                );
                
                if (bullet != null)
                {
                    spawnedBullets.Add(bullet);
                    Debug.Log($"生成子弹 {i + 1}，来自对象池: {BULLET_POOL}");
                }
            }
        }
        
        /// <summary>
        /// 生成敌人
        /// </summary>
        [ContextMenu("生成敌人")]
        public void SpawnEnemies()
        {
            if (enemyPrefab == null) return;
            
            for (int i = 0; i < 3; i++)
            {
                var enemy = enemyPrefab.SpawnFromPool(
                    poolName: ENEMY_POOL,
                    position: new Vector3(0, 0, i * 3f),
                    rotation: Quaternion.identity
                );
                
                if (enemy != null)
                {
                    spawnedEnemies.Add(enemy);
                    Debug.Log($"生成敌人 {i + 1}，来自对象池: {ENEMY_POOL}");
                }
            }
        }
        
        /// <summary>
        /// 生成特效（使用默认命名）
        /// </summary>
        [ContextMenu("生成特效")]
        public void SpawnEffects()
        {
            if (effectPrefab == null) return;
            
            for (int i = 0; i < 2; i++)
            {
                // 使用默认命名规则的便捷方法
                var effect = effectPrefab.SpawnFromPoolWithDefaultName(
                    position: new Vector3(i * 1.5f, 2f, 0),
                    rotation: Quaternion.identity
                );
                
                if (effect != null)
                {
                    Debug.Log($"生成特效 {i + 1}，来自对象池: {effectPrefab.name}Pool");
                    
                    // 2秒后自动归还
                    StartCoroutine(ReturnEffectAfterDelay(effect, 2f));
                }
            }
        }
        
        /// <summary>
        /// 归还所有子弹
        /// </summary>
        [ContextMenu("归还所有子弹")]
        public void ReturnAllBullets()
        {
            Debug.Log("=== 归还所有子弹 ===");
            
            for (int i = spawnedBullets.Count - 1; i >= 0; i--)
            {
                if (spawnedBullets[i] != null)
                {
                    // 明确指定对象池名称
                    spawnedBullets[i].ReturnToPool(BULLET_POOL);
                    Debug.Log($"归还子弹到对象池: {BULLET_POOL}");
                } 
            }
            spawnedBullets.Clear();
        }
        
        /// <summary>
        /// 归还所有敌人
        /// </summary>
        [ContextMenu("归还所有敌人")]
        public void ReturnAllEnemies()
        {
            Debug.Log("=== 归还所有敌人 ===");
            
            for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
            {
                if (spawnedEnemies[i] != null)
                {
                    spawnedEnemies[i].ReturnToPool(ENEMY_POOL);
                    Debug.Log($"归还敌人到对象池: {ENEMY_POOL}");
                }
            }
            spawnedEnemies.Clear();
        }
        
        /// <summary>
        /// 延迟归还特效的协程
        /// </summary>
        private System.Collections.IEnumerator ReturnEffectAfterDelay(GameObject effect, float delay)
        {
            yield return new UnityEngine.WaitForSeconds(delay);
            
            if (effect != null)
            {
                // 使用默认命名规则归还
                effect.ReturnToPoolWithDefaultName();
                Debug.Log($"特效已自动归还到对象池: {effectPrefab.name}Pool");
            }
        }
        
        /// <summary>
        /// 显示对象池状态
        /// </summary>
        [ContextMenu("显示对象池状态")]
        public void ShowPoolStatus()
        {
            Debug.Log("=== 对象池状态 ===");
            
            // 显示子弹池状态
            var bulletPool = PoolManager.GetPool<GameObject>(BULLET_POOL);
            if (bulletPool != null)
            {
                Debug.Log($"{BULLET_POOL}: 可用 {bulletPool.AvailableCount}, 活跃 {bulletPool.ActiveCount}");
            }
            
            // 显示敌人池状态
            var enemyPool = PoolManager.GetPool<GameObject>(ENEMY_POOL);
            if (enemyPool != null)
            {
                Debug.Log($"{ENEMY_POOL}: 可用 {enemyPool.AvailableCount}, 活跃 {enemyPool.ActiveCount}");
            }
            
            // 显示特效池状态
            if (effectPrefab != null)
            {
                string effectPoolName = $"{effectPrefab.name}Pool";
                var effectPool = PoolManager.GetPool<GameObject>(effectPoolName);
                if (effectPool != null)
                {
                    Debug.Log($"{effectPoolName}: 可用 {effectPool.AvailableCount}, 活跃 {effectPool.ActiveCount}");
                }
            }
        }
        
        /// <summary>
        /// 演示错误处理
        /// </summary>
        [ContextMenu("演示错误处理")]
        public void DemonstrateErrorHandling()
        {
            Debug.Log("=== 演示错误处理 ===");
            
            try
            {
                // 尝试从不存在的对象池获取对象
                if (bulletPrefab != null)
                {
                    var bullet = bulletPrefab.SpawnFromPool("NonExistentPool");
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogError($"预期的错误: {ex.Message}");
            }
            
            try
            {
                // 尝试创建没有名称的对象池
                if (bulletPrefab != null)
                {
                    bulletPrefab.CreateGameObjectPool("");
                }
            }
            catch (ArgumentException ex)
            {
                Debug.LogError($"预期的错误: {ex.Message}");
            }
        }
        
        private void OnDestroy()
        {
            // 清理所有对象池
            PoolManager.Destroy();
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 500));
            GUILayout.Label("UniObjectPool 简洁示例", GUI.skin.box);
            
            if (GUILayout.Button("生成子弹"))
            {
                SpawnBullets();
            }
            
            if (GUILayout.Button("生成敌人"))
            {
                SpawnEnemies();
            }
            
            if (GUILayout.Button("生成特效"))
            {
                SpawnEffects();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("归还所有子弹"))
            {
                ReturnAllBullets();
            }
            
            if (GUILayout.Button("归还所有敌人"))
            {
                ReturnAllEnemies();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("显示对象池状态"))
            {
                ShowPoolStatus();
            }
            
            if (GUILayout.Button("演示错误处理"))
            {
                DemonstrateErrorHandling();
            }
            
            GUILayout.Space(10);
            
            // 实时显示对象池状态
            GUILayout.Label("实时状态:", GUI.skin.box);
            
            var bulletPool = PoolManager.GetPool<GameObject>(BULLET_POOL);
            if (bulletPool != null)
            {
                GUILayout.Label($"子弹池: 可用{bulletPool.AvailableCount} 活跃{bulletPool.ActiveCount}");
            }
            
            var enemyPool = PoolManager.GetPool<GameObject>(ENEMY_POOL);
            if (enemyPool != null)
            {
                GUILayout.Label($"敌人池: 可用{enemyPool.AvailableCount} 活跃{enemyPool.ActiveCount}");
            }
            
            if (effectPrefab != null)
            {
                string effectPoolName = $"{effectPrefab.name}Pool";
                var effectPool = PoolManager.GetPool<GameObject>(effectPoolName);
                if (effectPool != null)
                {
                    GUILayout.Label($"特效池: 可用{effectPool.AvailableCount} 活跃{effectPool.ActiveCount}");
                }
            }
            
            GUILayout.EndArea();
        }
    }
}