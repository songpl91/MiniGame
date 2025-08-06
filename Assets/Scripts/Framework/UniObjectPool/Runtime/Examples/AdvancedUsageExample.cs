using System;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.ObjectPool;

namespace UniFramework.ObjectPool.Examples
{
    /// <summary>
    /// UniObjectPool 高级使用示例
    /// 展示改进后的对象池命名策略和 PoolRegistry 功能
    /// </summary>
    public class AdvancedUsageExample : MonoBehaviour
    {
        [Header("预制体设置")]
        public GameObject bulletPrefab;
        public GameObject enemyPrefab;
        public GameObject effectPrefab;
        
        [Header("父对象设置")]
        public Transform bulletParent;
        public Transform enemyParent;
        public Transform effectParent;
        
        private void Start()
        {
            // 初始化对象池管理器
            PoolManager.Initialize();
            
            // 演示不同的对象池创建方式
            DemonstratePoolCreation();
            
            // 演示对象池查找功能
            DemonstratePoolLookup();
            
            // 演示标签功能
            DemonstrateTagSystem();
        }
        
        /// <summary>
        /// 演示不同的对象池创建方式
        /// </summary>
        private void DemonstratePoolCreation()
        {
            Debug.Log("=== 演示对象池创建方式 ===");
            
            if (bulletPrefab != null)
            {
                // 方式1: 使用自动生成的唯一名称
                var bulletPool1 = bulletPrefab.CreateGameObjectPoolWithDefaultName(
                    parent: bulletParent,
                    config: PoolConfig.CreateDefault()
                );
                Debug.Log($"创建子弹池1，自动名称");
                
                // 方式2: 使用自定义名称
                var bulletPool2 = bulletPrefab.CreateGameObjectPool(
                    poolName: "CustomBulletPool",
                    parent: bulletParent,
                    config: PoolConfig.CreateDefault()
                );
                Debug.Log($"创建子弹池2，自定义名称: CustomBulletPool");
                
                // 方式3: 同一预制体，不同父对象
                var bulletPool3 = bulletPrefab.CreateGameObjectPoolWithDefaultName(
                    parent: null, // 无父对象
                    config: PoolConfig.CreateDefault()
                );
                Debug.Log($"创建子弹池3，无父对象");
            }
            
            if (enemyPrefab != null)
            {
                // 创建敌人池，并添加标签
                var enemyPool = enemyPrefab.CreateGameObjectPool(
                    poolName: "EnemyPool_Level1",
                    parent: enemyParent,
                    config: PoolConfig.CreateHighPerformance()
                );
                
                // 通过注册器添加标签
                PoolRegistry.RegisterPool("EnemyPool_Level1", enemyPrefab, enemyParent, typeof(GameObject), "Enemy", "Level1", "Combat");
                Debug.Log($"创建敌人池，添加标签: Enemy, Level1, Combat");
            }
        }
        
        /// <summary>
        /// 演示对象池查找功能
        /// </summary>
        private void DemonstratePoolLookup()
        {
            Debug.Log("=== 演示对象池查找功能 ===");
            
            if (bulletPrefab != null)
            {
                // 通过预制体查找对象池
                string poolName = PoolRegistry.FindPoolNameByPrefab(bulletPrefab, bulletParent);
                if (!string.IsNullOrEmpty(poolName))
                {
                    Debug.Log($"找到子弹池: {poolName}");
                    
                    // 从找到的池中获取对象
                    var bullet = bulletPrefab.SpawnFromPool(poolName: poolName);
                    if (bullet != null)
                    {
                        Debug.Log($"从池 {poolName} 获取子弹对象");
                        
                        // 2秒后归还
                        StartCoroutine(ReturnAfterDelay(bullet, poolName, 2f));
                    }
                }
                
                // 查找所有可能的对象池
                var possiblePools = PoolRegistry.FindPossiblePoolNames(bulletPrefab);
                Debug.Log($"子弹预制体可能的对象池数量: {possiblePools.Count}");
                foreach (var pool in possiblePools)
                {
                    Debug.Log($"  - {pool}");
                }
            }
        }
        
        /// <summary>
        /// 演示标签系统
        /// </summary>
        private void DemonstrateTagSystem()
        {
            Debug.Log("=== 演示标签系统 ===");
            
            // 查找带有特定标签的对象池
            var enemyPools = PoolRegistry.FindPoolsByTag("Enemy");
            Debug.Log($"找到 {enemyPools.Count} 个敌人相关的对象池:");
            foreach (var poolName in enemyPools)
            {
                Debug.Log($"  - {poolName}");
            }
            
            var level1Pools = PoolRegistry.FindPoolsByTag("Level1");
            Debug.Log($"找到 {level1Pools.Count} 个Level1相关的对象池:");
            foreach (var poolName in level1Pools)
            {
                Debug.Log($"  - {poolName}");
            }
            
            // 获取注册信息
            var registrations = PoolRegistry.GetAllRegistrations();
            Debug.Log($"总共注册了 {registrations.Count} 个对象池");
        }
        
        /// <summary>
        /// 延迟归还对象的协程
        /// </summary>
        private System.Collections.IEnumerator ReturnAfterDelay(GameObject obj, string poolName, float delay)
        {
            yield return new UnityEngine.WaitForSeconds(delay);
            
            if (obj != null)
            {
                obj.ReturnToPool(poolName);
                Debug.Log($"对象已归还到池: {poolName}");
            }
        }
        
        /// <summary>
        /// 演示智能对象归还
        /// </summary>
        [ContextMenu("演示智能归还")]
        public void DemonstrateSmartReturn()
        {
            if (bulletPrefab != null)
            {
                // 获取对象但不指定池名称
                var bullet = bulletPrefab.SpawnFromPoolWithDefaultName();
                if (bullet != null)
                {
                    Debug.Log("获取子弹对象（自动池名称）");
                    
                    // 归还时也不指定池名称，让系统自动查找
                    StartCoroutine(SmartReturnAfterDelay(bullet, 3f));
                }
            }
        }
        
        /// <summary>
        /// 智能归还的协程
        /// </summary>
        private System.Collections.IEnumerator SmartReturnAfterDelay(GameObject obj, float delay)
        {
            yield return new UnityEngine.WaitForSeconds(delay);
            
            if (obj != null)
            {
                // 不指定池名称，让系统自动查找
                obj.ReturnToPoolWithDefaultName();
                Debug.Log("对象已智能归还（自动查找池）");
            }
        }
        
        /// <summary>
        /// 显示当前对象池状态
        /// </summary>
        [ContextMenu("显示对象池状态")]
        public void ShowPoolStatus()
        {
            Debug.Log("=== 当前对象池状态 ===");
            
            var registrations = PoolRegistry.GetAllRegistrations();
            foreach (var registration in registrations)
            {
                var pool = PoolManager.GetPool<GameObject>(registration.Value.PoolName);
                if (pool != null)
                {
                    Debug.Log($"池: {registration.Value.PoolName}");
                    Debug.Log($"  可用: {pool.AvailableCount}, 活跃: {pool.ActiveCount}");
                    Debug.Log($"  预制体: {(registration.Value.Prefab ? registration.Value.Prefab.name : "无")}");
                    Debug.Log($"  标签: {string.Join(", ", registration.Value.Tags)}");
                }
            }
        }
        
        private void OnDestroy()
        {
            // 清理所有对象池
            PoolManager.Destroy();
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("UniObjectPool 高级示例", GUI.skin.box);
            
            if (GUILayout.Button("演示对象池创建"))
            {
                DemonstratePoolCreation();
            }
            
            if (GUILayout.Button("演示对象池查找"))
            {
                DemonstratePoolLookup();
            }
            
            if (GUILayout.Button("演示标签系统"))
            {
                DemonstrateTagSystem();
            }
            
            if (GUILayout.Button("演示智能归还"))
            {
                DemonstrateSmartReturn();
            }
            
            if (GUILayout.Button("显示对象池状态"))
            {
                ShowPoolStatus();
            }
            
            GUILayout.Space(10);
            
            // 显示注册器统计
            var registrations = PoolRegistry.GetAllRegistrations();
            GUILayout.Label($"已注册对象池: {registrations.Count}", GUI.skin.box);
            
            foreach (var registration in registrations)
            {
                var pool = PoolManager.GetPool<GameObject>(registration.Value.PoolName);
                if (pool != null)
                {
                    GUILayout.Label($"{registration.Value.PoolName}: 可用{pool.AvailableCount} 活跃{pool.ActiveCount}");
                }
            }
            
            GUILayout.EndArea();
        }
    }
}