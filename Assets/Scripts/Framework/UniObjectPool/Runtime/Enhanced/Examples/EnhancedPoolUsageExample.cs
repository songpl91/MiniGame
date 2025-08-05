using System;
using System.Collections;
using UnityEngine;

namespace UniFramework.ObjectPool.Enhanced.Examples
{
    /// <summary>
    /// 增强版对象池使用示例
    /// 展示增强版对象池的各种功能和用法
    /// 包括配置、统计、验证、批量操作等高级特性
    /// </summary>
    public class EnhancedPoolUsageExample : MonoBehaviour
    {
        [Header("预制体设置")]
        public GameObject bulletPrefab;
        public GameObject enemyPrefab;
        public GameObject effectPrefab;

        [Header("对象池设置")]
        public Transform poolParent;
        public int initialCapacity = 10;
        public int maxCapacity = 100;

        // 对象池实例
        private EnhancedGameObjectPool _bulletPool;
        private EnhancedGameObjectPool _enemyPool;
        private EnhancedGameObjectPool _effectPool;
        private EnhancedPool<DataObject> _dataPool;

        // 统计和监控
        private bool _enableMonitoring = true;
        private float _monitoringInterval = 5f;

        #region Unity 生命周期

        private void Start()
        {
            InitializeEnhancedPools();
            StartCoroutine(MonitoringCoroutine());
        }

        private void Update()
        {
            HandleInput();
        }

        private void OnDestroy()
        {
            CleanupPools();
        }

        #endregion

        #region 对象池初始化

        /// <summary>
        /// 初始化增强版对象池
        /// </summary>
        private void InitializeEnhancedPools()
        {
            Debug.Log("=== 初始化增强版对象池 ===");

            // 初始化管理器
            var defaultConfig = EnhancedPoolConfig.CreateDefault()
                .WithInitialCapacity(initialCapacity)
                .WithMaxCapacity(maxCapacity)
                .WithStatistics(true)
                .WithValidation(true);

            EnhancedPoolManager.Initialize(defaultConfig);

            // 创建子弹对象池（高性能配置）
            if (bulletPrefab != null)
            {
                var bulletConfig = EnhancedPoolConfig.CreateHighPerformance()
                    .WithTag("BulletPool")
                    .WithMaxCapacity(200);

                _bulletPool = bulletPrefab.CreateEnhancedPool(
                    poolParent,
                    bulletConfig,
                    "BulletPool"
                );
                _bulletPool.Prewarm(20);
                Debug.Log($"子弹池创建完成: {_bulletPool.GetStatusInfo()}");
            }

            // 创建敌人对象池（内存优化配置）
            if (enemyPrefab != null)
            {
                var enemyConfig = EnhancedPoolConfig.CreateMemoryOptimized()
                    .WithTag("EnemyPool")
                    .WithMaxCapacity(50);

                _enemyPool = enemyPrefab.CreateEnhancedPool(
                    poolParent,
                    enemyConfig,
                    "EnemyPool"
                );
                _enemyPool.Prewarm(5);
                Debug.Log($"敌人池创建完成: {_enemyPool.GetStatusInfo()}");
            }

            // 创建特效对象池（平衡配置）
            if (effectPrefab != null)
            {
                var effectConfig = EnhancedPoolConfig.CreateBalanced()
                    .WithTag("EffectPool")
                    .WithMaxCapacity(30);

                _effectPool = effectPrefab.CreateEnhancedPool(
                    poolParent,
                    effectConfig,
                    "EffectPool"
                );
                Debug.Log($"特效池创建完成: {_effectPool.GetStatusInfo()}");
            }

            // 创建数据对象池
            _dataPool = EnhancedPoolManager.CreatePool<DataObject>(
                "DataPool",
                () => new DataObject(),
                data => data.Reset(),
                data => data.Dispose(),
                EnhancedPoolConfig.CreateDefault().WithTag("DataPool")
            );
            _dataPool.Prewarm(10);
            Debug.Log($"数据池创建完成: {_dataPool.GetStatusInfo()}");
        }

        #endregion

        #region 输入处理

        /// <summary>
        /// 处理用户输入
        /// </summary>
        private void HandleInput()
        {
            // 空格键：发射子弹
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SpawnBullet();
            }

            // E键：生成敌人
            if (Input.GetKeyDown(KeyCode.E))
            {
                SpawnEnemy();
            }

            // F键：播放特效
            if (Input.GetKeyDown(KeyCode.F))
            {
                PlayEffect();
            }

            // D键：创建数据对象
            if (Input.GetKeyDown(KeyCode.D))
            {
                CreateDataObject();
            }

            // C键：清理所有池
            if (Input.GetKeyDown(KeyCode.C))
            {
                ClearAllPools();
            }

            // S键：显示统计信息
            if (Input.GetKeyDown(KeyCode.S))
            {
                ShowStatistics();
            }

            // B键：批量操作测试
            if (Input.GetKeyDown(KeyCode.B))
            {
                BatchOperationTest();
            }

            // H键：健康检查
            if (Input.GetKeyDown(KeyCode.H))
            {
                HealthCheck();
            }
        }

        #endregion

        #region 对象生成和回收

        /// <summary>
        /// 生成子弹
        /// </summary>
        private void SpawnBullet()
        {
            if (_bulletPool == null) return;

            var position = transform.position + Vector3.forward * 2f;
            var bullet = _bulletPool.Spawn(position, Quaternion.identity);
            
            if (bullet != null)
            {
                Debug.Log($"生成子弹: {bullet.name} at {position}");
                
                // 3秒后自动回收
                bullet.RecycleDelayed(_bulletPool, 3f);
            }
        }

        /// <summary>
        /// 生成敌人
        /// </summary>
        private void SpawnEnemy()
        {
            if (_enemyPool == null) return;

            var position = transform.position + new Vector3(
                UnityEngine.Random.Range(-5f, 5f),
                0f,
                UnityEngine.Random.Range(-5f, 5f)
            );
            
            var enemy = _enemyPool.Spawn(position, Quaternion.identity);
            
            if (enemy != null)
            {
                Debug.Log($"生成敌人: {enemy.name} at {position}");
                
                // 添加自动回收脚本
                var autoRecycler = enemy.GetComponent<AutoRecycler>();
                if (autoRecycler == null)
                {
                    autoRecycler = enemy.AddComponent<AutoRecycler>();
                }
                autoRecycler.Initialize(_enemyPool, 10f);
            }
        }

        /// <summary>
        /// 播放特效
        /// </summary>
        private void PlayEffect()
        {
            if (_effectPool == null) return;

            var position = transform.position + Vector3.up * 2f;
            var effect = _effectPool.Spawn(position, Quaternion.identity);
            
            if (effect != null)
            {
                Debug.Log($"播放特效: {effect.name} at {position}");
                
                // 2秒后回收
                effect.RecycleDelayed(_effectPool, 2f);
            }
        }

        /// <summary>
        /// 创建数据对象
        /// </summary>
        private void CreateDataObject()
        {
            if (_dataPool == null) return;

            var data = _dataPool.Get();
            if (data != null)
            {
                data.Initialize($"Data_{UnityEngine.Random.Range(1000, 9999)}");
                Debug.Log($"创建数据对象: {data.Name}");
                
                // 模拟使用后归还
                StartCoroutine(ReturnDataObjectDelayed(data, 1f));
            }
        }

        /// <summary>
        /// 延迟归还数据对象
        /// </summary>
        private IEnumerator ReturnDataObjectDelayed(DataObject data, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (_dataPool != null && data != null)
            {
                _dataPool.Return(data);
                Debug.Log($"归还数据对象: {data.Name}");
            }
        }

        #endregion

        #region 批量操作和测试

        /// <summary>
        /// 批量操作测试
        /// </summary>
        private void BatchOperationTest()
        {
            Debug.Log("=== 批量操作测试 ===");

            // 批量获取子弹
            if (_bulletPool != null)
            {
                var bullets = _bulletPool.GetMultiple(5);
                Debug.Log($"批量获取了 {bullets.Length} 个子弹");
                
                // 设置位置
                for (int i = 0; i < bullets.Length; i++)
                {
                    if (bullets[i] != null)
                    {
                        bullets[i].transform.position = transform.position + Vector3.right * i;
                    }
                }
                
                // 2秒后批量归还
                StartCoroutine(ReturnBulletsDelayed(bullets, 2f));
            }

            // 批量获取数据对象
            if (_dataPool != null)
            {
                var dataObjects = _dataPool.GetMultiple(3);
                Debug.Log($"批量获取了 {dataObjects.Length} 个数据对象");
                
                for (int i = 0; i < dataObjects.Length; i++)
                {
                    if (dataObjects[i] != null)
                    {
                        dataObjects[i].Initialize($"BatchData_{i}");
                    }
                }
                
                // 立即批量归还
                var returnedCount = _dataPool.ReturnMultiple(dataObjects);
                Debug.Log($"批量归还了 {returnedCount} 个数据对象");
            }
        }

        /// <summary>
        /// 延迟归还子弹数组
        /// </summary>
        private IEnumerator ReturnBulletsDelayed(GameObject[] bullets, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (_bulletPool != null)
            {
                int returnedCount = 0;
                for (int i = 0; i < bullets.Length; i++)
                {
                    if (bullets[i] != null && _bulletPool.Return(bullets[i]))
                    {
                        returnedCount++;
                    }
                }
                Debug.Log($"延迟归还了 {returnedCount} 个子弹");
            }
        }

        #endregion

        #region 统计和监控

        /// <summary>
        /// 显示统计信息
        /// </summary>
        private void ShowStatistics()
        {
            Debug.Log("=== 对象池统计信息 ===");

            // 显示管理器全局统计
            Debug.Log(EnhancedPoolManager.GetGlobalStatisticsSummary());
            Debug.Log(EnhancedPoolManager.GetAllPoolsStatus());

            // 显示各个池的详细统计
            _bulletPool?.LogDetailedStatus("[子弹池] ");
            _enemyPool?.LogDetailedStatus("[敌人池] ");
            _effectPool?.LogDetailedStatus("[特效池] ");
            _dataPool?.LogDetailedStatus("[数据池] ");
        }

        /// <summary>
        /// 健康检查
        /// </summary>
        private void HealthCheck()
        {
            Debug.Log("=== 对象池健康检查 ===");

            if (_bulletPool != null)
            {
                var isHealthy = _bulletPool.IsHealthy();
                var rating = _bulletPool.GetPerformanceRating();
                Debug.Log($"子弹池健康状态: {(isHealthy ? "健康" : "异常")}, 性能评级: {rating}");
            }

            if (_enemyPool != null)
            {
                var isHealthy = _enemyPool.IsHealthy();
                var rating = _enemyPool.GetPerformanceRating();
                Debug.Log($"敌人池健康状态: {(isHealthy ? "健康" : "异常")}, 性能评级: {rating}");
            }

            if (_dataPool != null)
            {
                var isHealthy = _dataPool.IsHealthy();
                var rating = _dataPool.GetPerformanceRating();
                Debug.Log($"数据池健康状态: {(isHealthy ? "健康" : "异常")}, 性能评级: {rating}");
            }
        }

        /// <summary>
        /// 监控协程
        /// </summary>
        private IEnumerator MonitoringCoroutine()
        {
            while (_enableMonitoring)
            {
                yield return new WaitForSeconds(_monitoringInterval);
                
                // 定期输出简要统计信息
                Debug.Log($"[监控] {EnhancedPoolManager.GetGlobalStatisticsSummary()}");
                
                // 检查性能问题
                if (_bulletPool != null && !_bulletPool.IsHealthy())
                {
                    Debug.LogWarning("[监控] 子弹池性能异常，建议检查!");
                }
                
                if (_dataPool != null && !_dataPool.IsHealthy())
                {
                    Debug.LogWarning("[监控] 数据池性能异常，建议检查!");
                }
            }
        }

        #endregion

        #region 清理

        /// <summary>
        /// 清理所有对象池
        /// </summary>
        private void ClearAllPools()
        {
            Debug.Log("=== 清理所有对象池 ===");
            
            EnhancedPoolManager.ClearAllPools();
            Debug.Log("所有对象池已清理");
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private void CleanupPools()
        {
            _enableMonitoring = false;
            
            _bulletPool?.Dispose();
            _enemyPool?.Dispose();
            _effectPool?.Dispose();
            _dataPool?.Dispose();
            
            EnhancedPoolManager.Destroy();
            
            Debug.Log("对象池资源已清理");
        }

        #region 异步操作示例

        /// <summary>
        /// 异步预热示例
        /// </summary>
        private async void AsyncPrewarmExample()
        {
            try
            {
                Debug.Log("开始异步预热GameObject对象池...");
                
                // 异步预热，带进度回调
                var progress = new System.Progress<float>(p => 
                {
                    Debug.Log($"预热进度: {p:P1}");
                });
                
                await _bulletPool.PrewarmAsync(50, System.Threading.CancellationToken.None, progress);
                
                Debug.Log("异步预热完成！");
                Debug.Log($"对象池状态: {_bulletPool.GetDetailedInfo()}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"异步预热失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 异步批量操作示例
        /// </summary>
        private async void AsyncBatchOperationExample()
        {
            try
            {
                Debug.Log("开始异步批量操作...");
                
                // 异步批量生成GameObject
                var gameObjects = await _bulletPool.SpawnMultipleAsync(20);
                Debug.Log($"异步生成了 {gameObjects.Length} 个GameObject");
                
                // 等待一段时间
                await System.Threading.Tasks.Task.Delay(2000);
                
                // 异步批量回收
                await _bulletPool.DespawnMultipleAsync(gameObjects);
                Debug.Log("异步批量回收完成");
                
                // 异步批量获取数据对象
                var dataObjects = await _dataPool.GetMultipleAsync(100);
                Debug.Log($"异步获取了 {dataObjects.Length} 个数据对象");
                
                // 批量归还
                await _dataPool.ReturnMultipleAsync(dataObjects);
                Debug.Log("异步批量归还完成");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"异步批量操作失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 延迟回收示例
        /// </summary>
        private async void DelayedDespawnExample()
        {
            var gameObj = _bulletPool.Spawn();
            Debug.Log($"生成GameObject: {gameObj.name}");
            
            // 3秒后自动回收
            await _bulletPool.DespawnDelayedAsync(gameObj, 3.0f);
            Debug.Log("GameObject已延迟回收");
        }

        /// <summary>
        /// 协程操作示例
        /// </summary>
        private void CoroutineOperationExample()
        {
            StartCoroutine(PrewarmCoroutineExample());
        }

        /// <summary>
        /// 协程预热示例
        /// </summary>
        private System.Collections.IEnumerator PrewarmCoroutineExample()
        {
            Debug.Log("开始协程预热...");
            
            yield return _bulletPool.PrewarmCoroutine(30, progress =>
            {
                Debug.Log($"协程预热进度: {progress:P1}");
            });
            
            Debug.Log("协程预热完成！");
        }

        #endregion

        #endregion

        #region GUI 显示

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("增强版对象池示例", GUI.skin.box);
            
            GUILayout.Label("操作说明:");
            GUILayout.Label("空格键 - 发射子弹");
            GUILayout.Label("E键 - 生成敌人");
            GUILayout.Label("F键 - 播放特效");
            GUILayout.Label("D键 - 创建数据对象");
            GUILayout.Label("B键 - 批量操作测试");
            GUILayout.Label("S键 - 显示统计信息");
            GUILayout.Label("H键 - 健康检查");
            GUILayout.Label("C键 - 清理所有池");
            
            GUILayout.Space(10);
            
            if (EnhancedPoolManager.IsInitialized)
            {
                GUILayout.Label($"对象池数量: {EnhancedPoolManager.PoolCount}");
                
                if (_bulletPool != null)
                    GUILayout.Label($"子弹池: {_bulletPool.AvailableCount}/{_bulletPool.ActiveCount}");
                    
                if (_enemyPool != null)
                    GUILayout.Label($"敌人池: {_enemyPool.AvailableCount}/{_enemyPool.ActiveCount}");
                    
                if (_dataPool != null)
                    GUILayout.Label($"数据池: {_dataPool.AvailableCount}/{_dataPool.ActiveCount}");
            }
            
            GUILayout.EndArea();
        }

        #endregion
    }

    /// <summary>
    /// 示例数据对象
    /// 实现 IEnhancedPoolable 接口
    /// </summary>
    public class DataObject : IEnhancedPoolable
    {
        public string Name { get; private set; }
        public DateTime CreatedTime { get; private set; }
        public bool IsInPool { get; private set; }

        public void Initialize(string name)
        {
            Name = name;
            CreatedTime = DateTime.Now;
        }

        public void Reset()
        {
            Name = null;
            // 保留创建时间用于调试
        }

        public void Dispose()
        {
            Name = null;
            CreatedTime = default;
        }

        #region IEnhancedPoolable 实现

        public void OnCreate()
        {
            CreatedTime = DateTime.Now;
            IsInPool = false;
        }

        public void OnSpawn()
        {
            IsInPool = false;
        }

        public void OnDespawn()
        {
            IsInPool = true;
        }

        public void OnReset()
        {
            // 重置时保留基本信息
        }

        public void OnDestroy()
        {
            Name = null;
            CreatedTime = default;
            IsInPool = false;
        }

        public bool CanReturn()
        {
            return !string.IsNullOrEmpty(Name);
        }

        public string GetDebugInfo()
        {
            return $"DataObject[{Name}] Created:{CreatedTime:HH:mm:ss} InPool:{IsInPool}";
        }

        #endregion
    }

    /// <summary>
    /// 自动回收组件
    /// 用于 GameObject 的自动回收
    /// </summary>
    public class AutoRecycler : MonoBehaviour
    {
        private EnhancedGameObjectPool _pool;
        private float _lifetime;
        private float _timer;

        public void Initialize(EnhancedGameObjectPool pool, float lifetime)
        {
            _pool = pool;
            _lifetime = lifetime;
            _timer = 0f;
        }

        private void Update()
        {
            if (_pool == null) return;

            _timer += Time.deltaTime;
            if (_timer >= _lifetime)
            {
                _pool.Return(gameObject);
            }
        }

        private void OnDisable()
        {
            _timer = 0f;
        }
    }
}