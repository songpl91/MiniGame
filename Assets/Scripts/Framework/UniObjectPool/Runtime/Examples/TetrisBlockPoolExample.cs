using System.Collections.Generic;
using UnityEngine;
using UniFramework.ObjectPool;

namespace UniFramework.ObjectPool.Examples
{
    /// <summary>
    /// 俄罗斯方块方块类型枚举
    /// 顺序需与 Inspector 中的预制体数组保持一致
    /// </summary>
    public enum BlockType
    {
        I, J, L, O, S, T, Z
    }

    /// <summary>
    /// 方块信息脚本，用于在预制体上标记方块类型
    /// </summary>
    public class BlockInfo : MonoBehaviour
    {
        public BlockType Type;
    }

    /// <summary>
    /// 俄罗斯方块对象池完整示例
    /// 展示单一对象池 + 标签的最佳实践
    /// </summary>
    public class TetrisBlockPoolExample : MonoBehaviour
    {
        [Header("请按 BlockType 枚举顺序放置 7 个方块预制体")]
        public GameObject[] blockPrefabs;

        [Header("方块父对象 (可为空)")]
        public Transform blockParent;

        // 单一对象池名称常量
        private const string BLOCK_POOL = "BlockPool";

        // 存储活跃方块，便于归还
        private readonly List<GameObject> _activeBlocks = new List<GameObject>();

        private void Awake()
        {
            // 初始化对象池管理器
            PoolManager.Initialize();

            // 预创建对象池
            PrewarmPools();
        }

        /// <summary>
        /// 预热对象池：为每种方块各预生成一定数量对象
        /// </summary>
        [ContextMenu("预热对象池 (各10个)")]
        public void PrewarmPools()
        {
            if (blockPrefabs == null || blockPrefabs.Length == 0)
            {
                Debug.LogError("未配置方块预制体！");
                return;
            }

            // 使用单一名称注册所有方块
            for (int i = 0; i < blockPrefabs.Length; i++)
            {
                GameObject prefab = blockPrefabs[i];
                if (prefab == null) continue;

                // 若未注册则创建池
                if (!PoolManager.HasPool(BLOCK_POOL))
                {
                    prefab.CreateGameObjectPool(
                        poolName: BLOCK_POOL,
                        parent: blockParent,
                        config: PoolConfig.CreateHighPerformance());
                }

                // 预热 10 个实例
                PoolManager.GetPool<GameObject>(BLOCK_POOL).Prewarm(10);
            }

            Debug.Log("方块对象池预热完成！");
        }

        /// <summary>
        /// 生成随机方块（演示 Spawn）
        /// </summary>
        [ContextMenu("生成随机方块")]
        public void SpawnRandomBlock()
        {
            if (blockPrefabs == null || blockPrefabs.Length == 0) return;

            int index = Random.Range(0, blockPrefabs.Length);
            GameObject prefab = blockPrefabs[index];

            Vector3 spawnPos = new Vector3(Random.Range(-4, 4), 10, 0);
            GameObject block = prefab.SpawnFromPool(BLOCK_POOL, spawnPos, Quaternion.identity, blockParent);
            if (block != null)
            {
                _activeBlocks.Add(block);
                Debug.Log($"生成方块: {((BlockType)index).ToString()}，Active={_activeBlocks.Count}");
            }
        }

        /// <summary>
        /// 归还所有活跃方块（演示 Return）
        /// </summary>
        [ContextMenu("归还所有方块")]
        public void ReturnAllBlocks()
        {
            for (int i = _activeBlocks.Count - 1; i >= 0; i--)
            {
                GameObject block = _activeBlocks[i];
                if (block != null)
                {
                    block.ReturnToPool(BLOCK_POOL);
                }
            }
            _activeBlocks.Clear();
            Debug.Log("已归还全部方块");
        }

        /// <summary>
        /// OnGUI 提供简单按钮操作
        /// </summary>
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 220, 160), GUI.skin.box);
            GUILayout.Label("俄罗斯方块对象池示例");

            if (GUILayout.Button("预热对象池"))
            {
                PrewarmPools();
            }
            if (GUILayout.Button("生成随机方块"))
            {
                SpawnRandomBlock();
            }
            if (GUILayout.Button("归还所有方块"))
            {
                ReturnAllBlocks();
            }

            // 实时显示池状态
            var pool = PoolManager.GetPool<GameObject>(BLOCK_POOL);
            if (pool != null)
            {
                GUILayout.Label($"可用: {pool.AvailableCount} / 活跃: {pool.ActiveCount}");
            }

            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            PoolManager.Destroy();
        }
    }
}