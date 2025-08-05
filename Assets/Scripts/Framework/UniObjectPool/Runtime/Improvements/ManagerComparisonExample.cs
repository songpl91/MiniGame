using System;
using System.Collections;
using UnityEngine;

namespace UniFramework.ObjectPool.Examples
{
    /// <summary>
    /// 演示通用PoolManager vs 专门GameObjectPoolManager的区别
    /// </summary>
    public class ManagerComparisonExample : MonoBehaviour
    {
        [Header("预制体")]
        public GameObject bulletPrefab;
        public GameObject enemyPrefab;
        
        void Start()
        {
            // 演示不同Manager的使用场景
            DemonstrateDataObjectPooling();
            DemonstrateGameObjectPooling();
            DemonstrateGameObjectPoolingProblems();
        }
        
        /// <summary>
        /// 演示数据对象池化 - 使用通用PoolManager
        /// </summary>
        void DemonstrateDataObjectPooling()
        {
            Debug.Log("=== 数据对象池化（使用PoolManager）===");
            
            // 简单数据类，无特殊需求
            PoolManager.CreatePool<PlayerData>(
                () => new PlayerData(),
                data => data.Reset() // 简单重置
            );
            
            // 使用很简单
            var playerData = PoolManager.Get<PlayerData>();
            playerData.Name = "Player1";
            playerData.Level = 10;
            
            Debug.Log($"获取数据: {playerData.Name}, Level: {playerData.Level}");
            
            // 归还也很简单
            PoolManager.Return(playerData);
            
            Debug.Log("数据对象池化演示完成");
        }
        
        /// <summary>
        /// 演示GameObject池化 - 使用专门的GameObjectPoolManager
        /// </summary>
        void DemonstrateGameObjectPooling()
        {
            Debug.Log("=== GameObject池化（使用GameObjectPoolManager）===");
            
            if (bulletPrefab != null)
            {
                // 创建GameObject对象池 - 自动处理复杂逻辑
                GameObjectPoolManager.CreateGameObjectPool("BulletPool", bulletPrefab);
                
                // 获取GameObject - 支持位置、旋转等参数
                var bullet = GameObjectPoolManager.Get(
                    "BulletPool", 
                    position: new Vector3(0, 0, 0),
                    rotation: Quaternion.identity
                );
                
                Debug.Log($"获取子弹: {bullet.name}");
                
                // 延迟归还 - 专门功能
                GameObjectPoolManager.ReturnDelayed(bullet, 2f);
                
                Debug.Log("子弹将在2秒后自动归还");
            }
        }
        
        /// <summary>
        /// 演示如果用通用PoolManager处理GameObject会遇到的问题
        /// </summary>
        void DemonstrateGameObjectPoolingProblems()
        {
            Debug.Log("=== 用PoolManager处理GameObject的问题 ===");
            
            if (enemyPrefab != null)
            {
                // 用通用PoolManager创建GameObject池
                PoolManager.CreatePool<GameObject>(
                    () => Instantiate(enemyPrefab), // 手动Instantiate
                    obj => ResetGameObjectManually(obj), // 手动重置
                    obj => Destroy(obj) // 手动销毁
                );
                
                // 获取GameObject
                var enemy = PoolManager.Get<GameObject>();
                
                // 问题1：需要手动设置位置
                enemy.transform.position = new Vector3(5, 0, 0);
                
                // 问题2：脚本状态可能是脏的
                var enemyScript = enemy.GetComponent<ExampleEnemyScript>();
                if (enemyScript != null)
                {
                    Debug.LogWarning("敌人脚本状态可能未正确重置！");
                    // 需要手动调用重置方法
                    enemyScript.ManualReset();
                }
                
                // 问题3：归还时需要手动处理
                PoolManager.Return(enemy);
                
                Debug.Log("通用PoolManager处理GameObject的问题演示完成");
            }
        }
        
        /// <summary>
        /// 手动重置GameObject的方法（繁琐且容易出错）
        /// </summary>
        void ResetGameObjectManually(GameObject obj)
        {
            // 重置位置
            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;
            
            // 激活对象
            obj.SetActive(true);
            
            // 手动重置所有脚本（容易遗漏）
            var scripts = obj.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                if (script is IManualReset resetable)
                {
                    resetable.ManualReset();
                }
            }
        }
    }
    
    /// <summary>
    /// 简单数据类示例
    /// </summary>
    public class PlayerData
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public float Health { get; set; }
        
        public void Reset()
        {
            Name = "";
            Level = 0;
            Health = 100f;
        }
    }
    
    /// <summary>
    /// 手动重置接口（用于演示问题）
    /// </summary>
    public interface IManualReset
    {
        void ManualReset();
    }
    
    /// <summary>
    /// 示例敌人脚本（演示GameObject脚本池化的复杂性）
    /// </summary>
    public class ExampleEnemyScript : MonoBehaviour, IGameObjectPoolable, IManualReset
    {
        [Header("敌人属性")]
        public float health = 100f;
        public float speed = 5f;
        public bool isActive = false;
        
        private Coroutine patrolCoroutine;
        private AudioSource audioSource;
        private ParticleSystem deathEffect;
        
        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            deathEffect = GetComponentInChildren<ParticleSystem>();
        }
        
        #region IGameObjectPoolable 实现（推荐方式）
        
        public void OnSpawnFromPool()
        {
            Debug.Log("[EnemyScript] 从对象池生成 - 自动重置状态");
            
            // 重置属性
            health = 100f;
            isActive = true;
            
            // 重启协程
            if (patrolCoroutine != null)
            {
                StopCoroutine(patrolCoroutine);
            }
            patrolCoroutine = StartCoroutine(PatrolCoroutine());
            
            // 播放生成音效
            if (audioSource != null)
            {
                audioSource.Play();
            }
            
            // 停止死亡特效
            if (deathEffect != null)
            {
                deathEffect.Stop();
            }
        }
        
        public void OnDespawnToPool()
        {
            Debug.Log("[EnemyScript] 归还到对象池 - 自动清理状态");
            
            // 停止所有协程
            if (patrolCoroutine != null)
            {
                StopCoroutine(patrolCoroutine);
                patrolCoroutine = null;
            }
            
            // 停止音效
            if (audioSource != null)
            {
                audioSource.Stop();
            }
            
            // 播放死亡特效
            if (deathEffect != null)
            {
                deathEffect.Play();
            }
            
            isActive = false;
        }
        
        #endregion
        
        #region IManualReset 实现（问题演示）
        
        public void ManualReset()
        {
            Debug.Log("[EnemyScript] 手动重置 - 容易遗漏某些状态");
            
            // 手动重置（容易遗漏某些状态）
            health = 100f;
            isActive = true;
            
            // 可能忘记重启协程
            // 可能忘记处理音效
            // 可能忘记处理特效
            // ...
        }
        
        #endregion
        
        /// <summary>
        /// 巡逻协程
        /// </summary>
        IEnumerator PatrolCoroutine()
        {
            while (isActive)
            {
                // 简单的巡逻逻辑
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
                yield return null;
            }
        }
        
        /// <summary>
        /// 敌人死亡
        /// </summary>
        public void Die()
        {
            // 归还到对象池而不是销毁
            GameObjectPoolManager.Return(gameObject);
        }
    }
    
    /// <summary>
    /// 总结注释
    /// </summary>
    public static class ManagerComparisonSummary
    {
        /*
         * 总结对比：
         * 
         * 1. 数据对象（PlayerData）：
         *    ✅ 使用 PoolManager - 简单、直接、高效
         *    ❌ 不需要专门Manager - 没有特殊需求
         * 
         * 2. GameObject对象：
         *    ❌ 使用 PoolManager - 需要大量手动处理
         *    ✅ 使用 GameObjectPoolManager - 自动处理复杂逻辑
         * 
         * 3. GameObjectPoolable的价值：
         *    - 自动调用所有脚本的生命周期方法
         *    - 避免状态污染和资源泄漏
         *    - 统一的脚本池化接口
         *    - 减少手动处理的复杂性和错误
         * 
         * 4. 设计原则：
         *    - 通用优先：大部分对象使用PoolManager
         *    - 按需特化：特殊需求才创建专门Manager
         *    - 避免过度设计：不要为每个类型都创建Manager
         */
    }
}