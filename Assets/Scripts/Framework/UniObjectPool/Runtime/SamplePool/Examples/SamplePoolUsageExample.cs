using UnityEngine;
using UniFramework.ObjectPool.SamplePool;

namespace UniFramework.ObjectPool.SamplePool.Examples
{
    /// <summary>
    /// 极简版对象池使用示例
    /// 展示如何使用SamplePool进行基础的对象池化操作
    /// </summary>
    public class SamplePoolUsageExample : MonoBehaviour
    {
        [Header("预制体设置")]
        public GameObject bulletPrefab;
        public GameObject enemyPrefab;
        
        [Header("父对象设置")]
        public Transform bulletParent;
        public Transform enemyParent;
        
        // 对象池名称常量
        private const string BULLET_POOL = "BulletPool";
        private const string ENEMY_POOL = "EnemyPool";
        
        // 对象池引用
        private SampleGameObjectPool _bulletPool;
        private SampleGameObjectPool _enemyPool;
        
        private void Start()
        {
            // 初始化对象池管理器
            SamplePoolManager.Initialize();
            
            // 创建对象池
            CreatePools();
            
            Debug.Log("SamplePool 示例初始化完成");
        }
        
        /// <summary>
        /// 创建对象池
        /// </summary>
        private void CreatePools()
        {
            // 方式1: 直接创建GameObject对象池
            if (bulletPrefab != null)
            {
                _bulletPool = new SampleGameObjectPool(bulletPrefab, bulletParent, 30);
                _bulletPool.Prewarm(5); // 预热5个对象
                Debug.Log($"创建子弹对象池，预热5个对象");
            }
            
            // 方式2: 使用扩展方法创建
            if (enemyPrefab != null)
            {
                _enemyPool = enemyPrefab.CreateSamplePool(ENEMY_POOL, enemyParent, 20);
                _enemyPool.Prewarm(3); // 预热3个对象
                Debug.Log($"创建敌人对象池: {ENEMY_POOL}，预热3个对象");
            }
        }
        
        /// <summary>
        /// 生成子弹
        /// </summary>
        [ContextMenu("生成子弹")]
        public void SpawnBullets()
        {
            if (_bulletPool == null) return;
            
            for (int i = 0; i < 3; i++)
            {
                Vector3 position = new Vector3(i * 2f, 0, 0);
                var bullet = _bulletPool.Spawn(position);
                
                if (bullet != null)
                {
                    Debug.Log($"生成子弹 {i + 1} 在位置: {position}");
                    
                    // 3秒后自动回收
                    StartCoroutine(DespawnAfterDelay(bullet, 3f));
                }
            }
            
            Debug.Log(_bulletPool.GetStatusInfo());
        }
        
        /// <summary>
        /// 生成敌人
        /// </summary>
        [ContextMenu("生成敌人")]
        public void SpawnEnemies()
        {
            if (enemyPrefab == null) return;
            
            for (int i = 0; i < 2; i++)
            {
                Vector3 position = new Vector3(0, 0, i * 3f);
                var enemy = enemyPrefab.SpawnFromSamplePool(ENEMY_POOL, position);
                
                if (enemy != null)
                {
                    Debug.Log($"生成敌人 {i + 1} 在位置: {position}");
                    
                    // 5秒后自动回收
                    StartCoroutine(ReturnAfterDelay(enemy, ENEMY_POOL, 5f));
                }
            }
            
            var pool = SamplePoolManager.GetPool<GameObject>(ENEMY_POOL);
            if (pool != null)
            {
                Debug.Log(pool.GetStatusInfo());
            }
        }
        
        /// <summary>
        /// 测试数据类对象池
        /// </summary>
        [ContextMenu("测试数据类对象池")]
        public void TestDataClassPool()
        {
            // 创建字符串对象池
            var stringPool = SamplePoolManager.CreatePool(
                () => new System.Text.StringBuilder(),
                sb => sb.Clear(),
                10
            );
            
            // 获取和使用
            var sb1 = stringPool.Get();
            sb1.Append("Hello ");
            sb1.Append("World!");
            Debug.Log($"StringBuilder内容: {sb1}");
            
            // 归还
            stringPool.Return(sb1);
            
            // 再次获取（应该是同一个对象，但已被清理）
            var sb2 = stringPool.Get();
            Debug.Log($"重用的StringBuilder长度: {sb2.Length}"); // 应该是0
            
            stringPool.Return(sb2);
            
            Debug.Log(stringPool.GetStatusInfo());
        }
        
        /// <summary>
        /// 显示所有对象池状态
        /// </summary>
        [ContextMenu("显示对象池状态")]
        public void ShowPoolStatus()
        {
            Debug.Log("=== 对象池状态 ===");
            Debug.Log(SamplePoolManager.GetAllPoolsStatus());
            
            if (_bulletPool != null)
            {
                Debug.Log(_bulletPool.GetStatusInfo());
            }
            
            if (_enemyPool != null)
            {
                Debug.Log(_enemyPool.GetStatusInfo());
            }
        }
        
        /// <summary>
        /// 清理所有对象池
        /// </summary>
        [ContextMenu("清理对象池")]
        public void ClearPools()
        {
            SamplePoolManager.Clear();
            _bulletPool = null;
            _enemyPool = null;
            Debug.Log("所有对象池已清理");
        }
        
        /// <summary>
        /// 延迟回收GameObject的协程
        /// </summary>
        private System.Collections.IEnumerator DespawnAfterDelay(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (go != null && _bulletPool != null)
            {
                _bulletPool.Despawn(go);
                Debug.Log($"子弹已回收到对象池");
            }
        }
        
        /// <summary>
        /// 延迟归还GameObject的协程
        /// </summary>
        private System.Collections.IEnumerator ReturnAfterDelay(GameObject go, string poolName, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (go != null)
            {
                go.ReturnToSamplePool(poolName);
                Debug.Log($"对象已归还到对象池: {poolName}");
            }
        }
        
        private void OnDestroy()
        {
            // 清理资源
            SamplePoolManager.Destroy();
        }
    }
}