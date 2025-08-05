using System;
using UnityEngine;

namespace UniFramework.ObjectPool
{
    /// <summary>
    /// GameObject对象池接口，用于处理GameObject及其脚本的生命周期
    /// 当GameObject从对象池中取出或归还时，会自动调用相应方法
    /// </summary>
    public interface IGameObjectPoolable
    {
        /// <summary>
        /// 当GameObject从对象池中取出时调用
        /// 用于初始化状态、注册事件、启动协程等
        /// </summary>
        void OnSpawnFromPool();
        
        /// <summary>
        /// 当GameObject归还到对象池时调用
        /// 用于清理状态、注销事件、停止协程等
        /// </summary>
        void OnDespawnToPool();
    }

    /// <summary>
    /// GameObject对象池管理器，自动处理GameObject上所有实现IGameObjectPoolable接口的脚本
    /// 建议将此脚本添加到需要池化的Prefab根节点上
    /// </summary>
    public class GameObjectPoolable : MonoBehaviour, IPoolable
    {
        [Header("调试信息")]
        [SerializeField] private bool _enableDebugLog = false;
        [SerializeField] private int _spawnCount = 0;
        [SerializeField] private int _despawnCount = 0;
        
        // 缓存所有实现IGameObjectPoolable接口的组件，避免重复查找
        private IGameObjectPoolable[] _poolableComponents;
        private bool _isInitialized = false;

        /// <summary>
        /// 初始化时缓存所有可池化组件
        /// </summary>
        private void Awake()
        {
            InitializePoolableComponents();
        }

        /// <summary>
        /// 初始化可池化组件缓存
        /// </summary>
        private void InitializePoolableComponents()
        {
            if (_isInitialized) return;

            // 获取GameObject及其所有子对象上的IGameObjectPoolable组件
            _poolableComponents = GetComponentsInChildren<IGameObjectPoolable>(true);
            _isInitialized = true;

            if (_enableDebugLog)
            {
                Debug.Log($"[GameObjectPoolable] 初始化完成，找到 {_poolableComponents.Length} 个可池化组件");
            }
        }

        /// <summary>
        /// 实现IPoolable接口 - 从对象池取出时调用
        /// </summary>
        public void OnSpawn()
        {
            if (!_isInitialized)
            {
                InitializePoolableComponents();
            }

            _spawnCount++;

            // 激活GameObject
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }

            // 调用所有可池化组件的OnSpawnFromPool方法
            for (int i = 0; i < _poolableComponents.Length; i++)
            {
                try
                {
                    _poolableComponents[i]?.OnSpawnFromPool();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GameObjectPoolable] 组件 {_poolableComponents[i].GetType().Name} 的OnSpawnFromPool方法执行失败: {ex.Message}", this);
                }
            }

            if (_enableDebugLog)
            {
                Debug.Log($"[GameObjectPoolable] {gameObject.name} 从对象池取出 (第{_spawnCount}次)", this);
            }
        }

        /// <summary>
        /// 实现IPoolable接口 - 归还到对象池时调用
        /// </summary>
        public void OnDespawn()
        {
            _despawnCount++;

            // 调用所有可池化组件的OnDespawnToPool方法
            for (int i = 0; i < _poolableComponents.Length; i++)
            {
                try
                {
                    _poolableComponents[i]?.OnDespawnToPool();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GameObjectPoolable] 组件 {_poolableComponents[i].GetType().Name} 的OnDespawnToPool方法执行失败: {ex.Message}", this);
                }
            }

            // 停用GameObject（可选，根据需求决定）
            if (gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
            }

            if (_enableDebugLog)
            {
                Debug.Log($"[GameObjectPoolable] {gameObject.name} 归还到对象池 (第{_despawnCount}次)", this);
            }
        }

        /// <summary>
        /// 手动刷新可池化组件缓存（当运行时动态添加组件时使用）
        /// </summary>
        public void RefreshPoolableComponents()
        {
            _isInitialized = false;
            InitializePoolableComponents();
        }

        /// <summary>
        /// 获取统计信息
        /// </summary>
        public (int spawnCount, int despawnCount) GetStatistics()
        {
            return (_spawnCount, _despawnCount);
        }

        /// <summary>
        /// 重置统计信息
        /// </summary>
        public void ResetStatistics()
        {
            _spawnCount = 0;
            _despawnCount = 0;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器下显示调试信息
        /// </summary>
        private void OnValidate()
        {
            if (Application.isPlaying && _isInitialized)
            {
                // 在Inspector中实时显示组件数量
                var components = GetComponentsInChildren<IGameObjectPoolable>(true);
                if (components.Length != _poolableComponents.Length)
                {
                    RefreshPoolableComponents();
                }
            }
        }
#endif
    }
}