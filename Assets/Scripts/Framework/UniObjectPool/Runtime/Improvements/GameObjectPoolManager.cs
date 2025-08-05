using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.ObjectPool
{
    /// <summary>
    /// GameObject专用对象池管理器
    /// 提供更便捷的GameObject对象池操作，自动处理脚本生命周期
    /// </summary>
    public static class GameObjectPoolManager
    {
        // GameObject对象池字典
        private static readonly Dictionary<string, UniObjectPool<GameObject>> _gameObjectPools = new Dictionary<string, UniObjectPool<GameObject>>();
        
        // Prefab到池名称的映射
        private static readonly Dictionary<GameObject, string> _prefabToPoolName = new Dictionary<GameObject, string>();
        
        /// <summary>
        /// 创建GameObject对象池
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <param name="prefab">预制体</param>
        /// <param name="parent">父对象（可选）</param>
        /// <param name="config">池配置（可选）</param>
        /// <returns>创建的对象池</returns>
        public static UniObjectPool<GameObject> CreateGameObjectPool(
            string poolName, 
            GameObject prefab, 
            Transform parent = null, 
            PoolConfig config = null)
        {
            if (string.IsNullOrEmpty(poolName))
            {
                throw new ArgumentException("对象池名称不能为空", nameof(poolName));
            }
            
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab), "预制体不能为空");
            }
            
            if (_gameObjectPools.ContainsKey(poolName))
            {
                Debug.LogWarning($"[GameObjectPoolManager] 对象池 '{poolName}' 已存在，将返回现有对象池");
                return _gameObjectPools[poolName];
            }
            
            // 使用默认配置
            if (config == null)
            {
                config = PoolConfig.CreateDefault();
            }
            
            // 创建GameObject工厂函数
            Func<GameObject> createFunc = () => CreateGameObject(prefab, parent);
            
            // 创建重置函数
            Action<GameObject> resetFunc = (obj) => ResetGameObject(obj);
            
            // 创建销毁函数
            Action<GameObject> destroyFunc = (obj) => DestroyGameObject(obj);
            
            // 创建对象池
            var pool = new UniObjectPool<GameObject>(createFunc, resetFunc, destroyFunc, config);
            
            // 注册对象池
            _gameObjectPools[poolName] = pool;
            _prefabToPoolName[prefab] = poolName;
            
            Debug.Log($"[GameObjectPoolManager] 创建GameObject对象池 '{poolName}' 成功，预制体: {prefab.name}");
            
            return pool;
        }
        
        /// <summary>
        /// 从对象池获取GameObject
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <param name="position">位置（可选）</param>
        /// <param name="rotation">旋转（可选）</param>
        /// <param name="parent">父对象（可选）</param>
        /// <returns>获取的GameObject</returns>
        public static GameObject Get(
            string poolName, 
            Vector3? position = null, 
            Quaternion? rotation = null, 
            Transform parent = null)
        {
            if (!_gameObjectPools.TryGetValue(poolName, out var pool))
            {
                Debug.LogError($"[GameObjectPoolManager] 对象池 '{poolName}' 不存在");
                return null;
            }
            
            var gameObject = pool.Get();
            if (gameObject == null)
            {
                Debug.LogError($"[GameObjectPoolManager] 从对象池 '{poolName}' 获取GameObject失败");
                return null;
            }
            
            // 设置位置和旋转
            var transform = gameObject.transform;
            if (position.HasValue)
            {
                transform.position = position.Value;
            }
            if (rotation.HasValue)
            {
                transform.rotation = rotation.Value;
            }
            if (parent != null)
            {
                transform.SetParent(parent);
            }
            
            return gameObject;
        }
        
        /// <summary>
        /// 从对象池获取GameObject（使用预制体）
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="position">位置（可选）</param>
        /// <param name="rotation">旋转（可选）</param>
        /// <param name="parent">父对象（可选）</param>
        /// <returns>获取的GameObject</returns>
        public static GameObject Get(
            GameObject prefab, 
            Vector3? position = null, 
            Quaternion? rotation = null, 
            Transform parent = null)
        {
            if (!_prefabToPoolName.TryGetValue(prefab, out var poolName))
            {
                Debug.LogError($"[GameObjectPoolManager] 预制体 '{prefab.name}' 没有对应的对象池");
                return null;
            }
            
            return Get(poolName, position, rotation, parent);
        }
        
        /// <summary>
        /// 归还GameObject到对象池
        /// </summary>
        /// <param name="gameObject">要归还的GameObject</param>
        /// <returns>是否归还成功</returns>
        public static bool Return(GameObject gameObject)
        {
            if (gameObject == null)
            {
                Debug.LogWarning("[GameObjectPoolManager] 尝试归还空的GameObject");
                return false;
            }
            
            // 查找对应的对象池
            foreach (var kvp in _gameObjectPools)
            {
                if (kvp.Value.Return(gameObject))
                {
                    return true;
                }
            }
            
            Debug.LogWarning($"[GameObjectPoolManager] GameObject '{gameObject.name}' 不属于任何对象池，将直接销毁");
            UnityEngine.Object.Destroy(gameObject);
            return false;
        }
        
        /// <summary>
        /// 延迟归还GameObject到对象池
        /// </summary>
        /// <param name="gameObject">要归还的GameObject</param>
        /// <param name="delay">延迟时间（秒）</param>
        public static void ReturnDelayed(GameObject gameObject, float delay)
        {
            if (gameObject == null) return;
            
            var delayedReturn = gameObject.GetComponent<DelayedPoolReturn>();
            if (delayedReturn == null)
            {
                delayedReturn = gameObject.AddComponent<DelayedPoolReturn>();
            }
            
            delayedReturn.StartDelayedReturn(delay);
        }
        
        /// <summary>
        /// 获取对象池
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <returns>对象池实例</returns>
        public static UniObjectPool<GameObject> GetPool(string poolName)
        {
            _gameObjectPools.TryGetValue(poolName, out var pool);
            return pool;
        }
        
        /// <summary>
        /// 检查对象池是否存在
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <returns>是否存在</returns>
        public static bool HasPool(string poolName)
        {
            return _gameObjectPools.ContainsKey(poolName);
        }
        
        /// <summary>
        /// 移除对象池
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <returns>是否移除成功</returns>
        public static bool RemovePool(string poolName)
        {
            if (_gameObjectPools.TryGetValue(poolName, out var pool))
            {
                pool.Dispose();
                _gameObjectPools.Remove(poolName);
                
                // 移除预制体映射
                var keysToRemove = new List<GameObject>();
                foreach (var kvp in _prefabToPoolName)
                {
                    if (kvp.Value == poolName)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
                
                for (int i = 0; i < keysToRemove.Count; i++)
                {
                    _prefabToPoolName.Remove(keysToRemove[i]);
                }
                
                Debug.Log($"[GameObjectPoolManager] 移除对象池 '{poolName}' 成功");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 清理所有对象池
        /// </summary>
        public static void CleanupAll()
        {
            foreach (var pool in _gameObjectPools.Values)
            {
                pool.Cleanup();
            }
            
            Debug.Log("[GameObjectPoolManager] 清理所有GameObject对象池完成");
        }
        
        /// <summary>
        /// 销毁所有对象池
        /// </summary>
        public static void DestroyAll()
        {
            foreach (var pool in _gameObjectPools.Values)
            {
                pool.Dispose();
            }
            
            _gameObjectPools.Clear();
            _prefabToPoolName.Clear();
            
            Debug.Log("[GameObjectPoolManager] 销毁所有GameObject对象池完成");
        }
        
        /// <summary>
        /// 获取所有对象池的统计信息
        /// </summary>
        /// <returns>统计信息字典</returns>
        public static Dictionary<string, PoolStatistics> GetAllStatistics()
        {
            var statistics = new Dictionary<string, PoolStatistics>();
            
            foreach (var kvp in _gameObjectPools)
            {
                statistics[kvp.Key] = kvp.Value.Statistics;
            }
            
            return statistics;
        }
        
        /// <summary>
        /// 创建GameObject实例
        /// </summary>
        private static GameObject CreateGameObject(GameObject prefab, Transform parent)
        {
            var instance = UnityEngine.Object.Instantiate(prefab, parent);
            
            // 确保GameObject有GameObjectPoolable组件
            var poolable = instance.GetComponent<GameObjectPoolable>();
            if (poolable == null)
            {
                poolable = instance.AddComponent<GameObjectPoolable>();
            }
            
            return instance;
        }
        
        /// <summary>
        /// 重置GameObject状态
        /// </summary>
        private static void ResetGameObject(GameObject gameObject)
        {
            if (gameObject == null) return;
            
            // GameObjectPoolable组件会自动处理重置逻辑
            // 这里可以添加额外的通用重置逻辑
            
            // 重置Transform
            var transform = gameObject.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        
        /// <summary>
        /// 销毁GameObject
        /// </summary>
        private static void DestroyGameObject(GameObject gameObject)
        {
            if (gameObject != null)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
        }
    }
    
    /// <summary>
    /// 延迟归还组件
    /// 用于实现延迟归还GameObject到对象池的功能
    /// </summary>
    public class DelayedPoolReturn : MonoBehaviour
    {
        private Coroutine _returnCoroutine;
        
        /// <summary>
        /// 开始延迟归还
        /// </summary>
        /// <param name="delay">延迟时间（秒）</param>
        public void StartDelayedReturn(float delay)
        {
            // 停止之前的协程
            if (_returnCoroutine != null)
            {
                StopCoroutine(_returnCoroutine);
            }
            
            _returnCoroutine = StartCoroutine(DelayedReturnCoroutine(delay));
        }
        
        /// <summary>
        /// 取消延迟归还
        /// </summary>
        public void CancelDelayedReturn()
        {
            if (_returnCoroutine != null)
            {
                StopCoroutine(_returnCoroutine);
                _returnCoroutine = null;
            }
        }
        
        /// <summary>
        /// 延迟归还协程
        /// </summary>
        private System.Collections.IEnumerator DelayedReturnCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            GameObjectPoolManager.Return(gameObject);
            _returnCoroutine = null;
        }
        
        private void OnDestroy()
        {
            CancelDelayedReturn();
        }
    }
}