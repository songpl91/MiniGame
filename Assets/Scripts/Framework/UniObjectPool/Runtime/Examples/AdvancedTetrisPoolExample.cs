using System.Collections.Generic;
using UnityEngine;
using UniFramework.ObjectPool;

namespace UniFramework.ObjectPool.Examples
{
    /// <summary>
    /// 俄罗斯方块高级对象池示例
    /// 对比展示多池和单池两种策略
    /// </summary>
    public class AdvancedTetrisPoolExample : MonoBehaviour
    {
        [Header("方块预制体配置")]
        public GameObject[] blockPrefabs; // 按 BlockType 枚举顺序放置预制体

        [Header("父对象配置")]
        public Transform blockParent;

        [Header("对象池配置")]
        [Tooltip("预热时每种方块的预生成数量")]
        public int preloadCount = 10;

        [Tooltip("是否使用多池策略")]
        public bool useMultiPoolStrategy = false;

        // 池名称常量
        private const string BLOCK_POOL = "BlockPool"; // 单池名称
        private readonly string[] POOL_NAMES = new string[] // 多池名称
        {
            "BlockPool_I",
            "BlockPool_J",
            "BlockPool_L",
            "BlockPool_O",
            "BlockPool_S",
            "BlockPool_T",
            "BlockPool_Z"
        };

        // 性能测试数据
        private float _lastSpawnTime = 0f;
        private int _spawnCount = 0;
        private float _totalSpawnTime = 0f;

        // 存储活跃方块
        private readonly List<GameObject> _activeBlocks = new List<GameObject>();

        private void Awake()
        {
            // 初始化对象池管理器
            PoolManager.Initialize();

            // 创建对象池
            CreatePools();
        }

        /// <summary>
        /// 创建对象池 (根据选择的策略)
        /// </summary>
        private void CreatePools()
        {
            if (blockPrefabs == null || blockPrefabs.Length == 0)
            {
                Debug.LogError("未配置方块预制体！");
                return;
            }

            if (useMultiPoolStrategy)
            {
                CreateMultiplePools();
            }
            else
            {
                CreateSinglePool();
            }
        }

        /// <summary>
        /// 创建单一对象池策略
        /// </summary>
        private void CreateSinglePool()
        {
            Debug.Log("使用单池策略创建对象池");

            // 确保池不存在
            if (PoolManager.HasPool(BLOCK_POOL))
            {
                PoolManager.RemovePool(BLOCK_POOL);
            }

            // 创建并配置单一对象池
            blockPrefabs[0].CreateGameObjectPool(
                poolName: BLOCK_POOL,
                parent: blockParent,
                config: PoolConfig.CreateHighPerformance());

            // 预热所有方块类型
            for (int i = 0; i < blockPrefabs.Length; i++)
            {
                if (blockPrefabs[i] == null) continue;

                // 预热
                PoolManager.GetPool<GameObject>(BLOCK_POOL).Prewarm(preloadCount);
            }

            Debug.Log($"单池创建完成，已预热 {blockPrefabs.Length} 种方块各 {preloadCount} 个");
        }

        /// <summary>
        /// 创建多个对象池策略
        /// </summary>
        private void CreateMultiplePools()
        {
            Debug.Log("使用多池策略创建对象池");

            // 为每种方块创建独立的对象池
            for (int i = 0; i < blockPrefabs.Length; i++)
            {
                if (blockPrefabs[i] == null) continue;

                string poolName = POOL_NAMES[i];

                // 确保池不存在
                if (PoolManager.HasPool(poolName))
                {
                    PoolManager.RemovePool(poolName);
                }

                // 创建对象池
                blockPrefabs[i].CreateGameObjectPool(
                    poolName: poolName,
                    parent: blockParent,
                    config: PoolConfig.CreateHighPerformance());

                // 预热
                PoolManager.GetPool<GameObject>(poolName).Prewarm(preloadCount);
            }

            Debug.Log($"多池创建完成，为 {blockPrefabs.Length} 种方块创建了独立对象池并各预热 {preloadCount} 个");
        }

        /// <summary>
        /// 生成随机方块
        /// </summary>
        public GameObject SpawnRandomBlock()
        {
            if (blockPrefabs == null || blockPrefabs.Length == 0) return null;

            // 随机选择方块类型
            int index = Random.Range(0, blockPrefabs.Length);
            GameObject prefab = blockPrefabs[index];
            if (prefab == null) return null;

            // 记录开始时间（性能测试）
            _lastSpawnTime = Time.realtimeSinceStartup;

            // 根据策略使用不同的池
            GameObject block;
            if (useMultiPoolStrategy)
            {
                // 多池策略：使用特定类型的池
                block = prefab.SpawnFromPool(
                    poolName: POOL_NAMES[index],
                    position: new Vector3(Random.Range(-4, 4), 10, 0),
                    rotation: Quaternion.identity,
                    parent: blockParent);
            }
            else
            {
                // 单池策略：所有类型共用一个池
                block = prefab.SpawnFromPool(
                    poolName: BLOCK_POOL,
                    position: new Vector3(Random.Range(-4, 4), 10, 0),
                    rotation: Quaternion.identity,
                    parent: blockParent);
            }

            // 计算耗时（性能测试）
            float spawnTime = Time.realtimeSinceStartup - _lastSpawnTime;
            _totalSpawnTime += spawnTime;
            _spawnCount++;

            if (block != null)
            {
                // 记录活跃方块
                _activeBlocks.Add(block);
                Debug.Log($"生成方块: {((BlockType)index).ToString()}，耗时: {spawnTime * 1000:F2}ms，平均: {(_totalSpawnTime / _spawnCount) * 1000:F2}ms");
            }

            return block;
        }

        /// <summary>
        /// 批量生成多个方块（性能测试）
        /// </summary>
        [ContextMenu("批量生成100个方块")]
        public void SpawnMultipleBlocks()
        {
            // 重置性能计数
            _spawnCount = 0;
            _totalSpawnTime = 0f;

            // 批量生成
            for (int i = 0; i < 100; i++)
            {
                SpawnRandomBlock();
            }

            Debug.Log($"批量生成完成，平均耗时: {(_totalSpawnTime / 100) * 1000:F2}ms");
        }

        /// <summary>
        /// 归还随机一个方块
        /// </summary>
        [ContextMenu("归还随机方块")]
        public void ReturnRandomBlock()
        {
            if (_activeBlocks.Count == 0) return;

            int index = Random.Range(0, _activeBlocks.Count);
            GameObject block = _activeBlocks[index];

            if (block != null)
            {
                // 根据策略使用不同的池名称归还
                if (useMultiPoolStrategy)
                {
                    // 多池策略：需要找到正确的池
                    BlockInfo blockInfo = block.GetComponent<BlockInfo>();
                    if (blockInfo != null)
                    {
                        int typeIndex = (int)blockInfo.Type;
                        block.ReturnToPool(POOL_NAMES[typeIndex]);
                    }
                    else
                    {
                        // 降级处理：如果找不到类型信息，尝试暴力匹配或销毁
                        Debug.LogWarning("无法确定方块类型，尝试销毁");
                        GameObject.Destroy(block);
                    }
                }
                else
                {
                    // 单池策略：直接归还到唯一池
                    block.ReturnToPool(BLOCK_POOL);
                }

                _activeBlocks.RemoveAt(index);
                Debug.Log($"归还方块，剩余活跃: {_activeBlocks.Count}");
            }
        }

        /// <summary>
        /// 归还所有活跃方块
        /// </summary>
        [ContextMenu("归还所有方块")]
        public void ReturnAllBlocks()
        {
            for (int i = _activeBlocks.Count - 1; i >= 0; i--)
            {
                GameObject block = _activeBlocks[i];
                if (block != null)
                {
                    // 根据策略使用不同的池名称归还
                    if (useMultiPoolStrategy)
                    {
                        // 多池策略：需要找到正确的池
                        BlockInfo blockInfo = block.GetComponent<BlockInfo>();
                        if (blockInfo != null)
                        {
                            int typeIndex = (int)blockInfo.Type;
                            block.ReturnToPool(POOL_NAMES[typeIndex]);
                        }
                        else
                        {
                            // 降级处理：销毁
                            GameObject.Destroy(block);
                        }
                    }
                    else
                    {
                        // 单池策略：直接归还到唯一池
                        block.ReturnToPool(BLOCK_POOL);
                    }
                }
            }

            _activeBlocks.Clear();
            Debug.Log("已归还全部方块");
        }

        /// <summary>
        /// 切换对象池策略
        /// </summary>
        [ContextMenu("切换池策略")]
        public void TogglePoolStrategy()
        {
            // 先归还所有活跃方块
            ReturnAllBlocks();

            // 切换策略
            useMultiPoolStrategy = !useMultiPoolStrategy;

            // 重建对象池
            CreatePools();

            Debug.Log($"已切换到{(useMultiPoolStrategy ? "多池" : "单池")}策略");
        }

        /// <summary>
        /// 打印对象池状态信息
        /// </summary>
        [ContextMenu("打印对象池状态")]
        public void PrintPoolStatus()
        {
            Debug.Log($"===== 对象池状态 ({(useMultiPoolStrategy ? "多池" : "单池")}策略) =====");

            if (useMultiPoolStrategy)
            {
                // 多池策略：打印每个池的状态
                for (int i = 0; i < POOL_NAMES.Length; i++)
                {
                    var pool = PoolManager.GetPool<GameObject>(POOL_NAMES[i]);
                    if (pool != null)
                    {
                        Debug.Log($"{POOL_NAMES[i]}: 可用 {pool.AvailableCount}, 活跃 {pool.ActiveCount}");
                    }
                }
            }
            else
            {
                // 单池策略：打印唯一池的状态
                var pool = PoolManager.GetPool<GameObject>(BLOCK_POOL);
                if (pool != null)
                {
                    Debug.Log($"{BLOCK_POOL}: 可用 {pool.AvailableCount}, 活跃 {pool.ActiveCount}");
                }
            }
        }

        /// <summary>
        /// OnGUI 提供控制界面
        /// </summary>
        private void OnGUI()
        {
            // 主控制区
            GUILayout.BeginArea(new Rect(10, 10, 250, 350), GUI.skin.box);
            GUILayout.Label("俄罗斯方块对象池高级示例");
            GUILayout.Label($"当前策略: {(useMultiPoolStrategy ? "多池" : "单池")}");

            if (GUILayout.Button("切换池策略"))
            {
                TogglePoolStrategy();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("生成随机方块"))
            {
                SpawnRandomBlock();
            }

            if (GUILayout.Button("批量生成100个方块"))
            {
                SpawnMultipleBlocks();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("归还随机方块"))
            {
                ReturnRandomBlock();
            }

            if (GUILayout.Button("归还所有方块"))
            {
                ReturnAllBlocks();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("打印对象池状态"))
            {
                PrintPoolStatus();
            }

            // 当前状态
            GUILayout.Space(10);
            GUILayout.Label("当前状态:", GUI.skin.box);
            GUILayout.Label($"活跃方块数量: {_activeBlocks.Count}");
            
            if (useMultiPoolStrategy)
            {
                // 多池模式下显示每个池的状态
                foreach (string poolName in POOL_NAMES)
                {
                    var pool = PoolManager.GetPool<GameObject>(poolName);
                    if (pool != null)
                    {
                        GUILayout.Label($"{poolName}: 可用{pool.AvailableCount} 活跃{pool.ActiveCount}");
                    }
                }
            }
            else
            {
                // 单池模式下显示唯一池的状态
                var pool = PoolManager.GetPool<GameObject>(BLOCK_POOL);
                if (pool != null)
                {
                    GUILayout.Label($"{BLOCK_POOL}: 可用{pool.AvailableCount} 活跃{pool.ActiveCount}");
                }
            }

            if (_spawnCount > 0)
            {
                GUILayout.Label($"平均生成耗时: {(_totalSpawnTime / _spawnCount) * 1000:F2}ms");
            }

            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            PoolManager.Destroy();
        }
    }
}