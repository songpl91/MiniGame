using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UniFramework.ObjectPool.Enhanced
{
    /// <summary>
    /// 增强版GameObject对象池异步操作扩展
    /// 专门为GameObject提供异步加载和实例化支持
    /// </summary>
    public static class EnhancedGameObjectPoolAsync
    {
        /// <summary>
        /// 异步预热GameObject对象池
        /// </summary>
        /// <param name="pool">GameObject对象池</param>
        /// <param name="count">预热数量</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="progressCallback">进度回调</param>
        /// <returns>异步任务</returns>
        public static async Task PrewarmAsync(this EnhancedGameObjectPool pool, int count,
            CancellationToken cancellationToken = default, IProgress<float> progressCallback = null)
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            if (count <= 0) return;

            var createdObjects = new List<GameObject>(count);
            
            try
            {
                // 分批创建，避免长时间阻塞主线程
                const int batchSize = 5; // GameObject创建相对较重，减少批次大小
                int totalBatches = Mathf.CeilToInt((float)count / batchSize);
                
                for (int batch = 0; batch < totalBatches; batch++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    int currentBatchSize = Mathf.Min(batchSize, count - batch * batchSize);
                    
                    // 创建当前批次的GameObject
                    for (int i = 0; i < currentBatchSize; i++)
                    {
                        var gameObj = pool.Spawn();
                        createdObjects.Add(gameObj);
                        
                        // GameObject创建后立即禁用，避免影响性能
                        gameObj.SetActive(false);
                    }
                    
                    // 报告进度
                    float progress = (float)(batch + 1) / totalBatches;
                    progressCallback?.Report(progress);
                    
                    // 让出控制权，避免阻塞主线程
                    if (batch < totalBatches - 1)
                    {
                        await Task.Yield();
                    }
                }
                
                // 将所有GameObject归还到池中
                foreach (var gameObj in createdObjects)
                {
                    pool.Despawn(gameObj);
                }
            }
            catch (OperationCanceledException)
            {
                // 清理已创建的GameObject
                foreach (var gameObj in createdObjects)
                {
                    if (gameObj != null)
                    {
                        pool.Despawn(gameObj);
                    }
                }
                throw;
            }
        }

        /// <summary>
        /// 异步生成GameObject
        /// </summary>
        /// <param name="pool">GameObject对象池</param>
        /// <param name="parent">父物体</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>异步任务</returns>
        public static async Task<GameObject> SpawnAsync(this EnhancedGameObjectPool pool,
            Transform parent = null, Vector3? position = null, Quaternion? rotation = null,
            CancellationToken cancellationToken = default)
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            
            cancellationToken.ThrowIfCancellationRequested();
            
            // 让出控制权，模拟异步操作
            await Task.Yield();
            
            var gameObj = pool.Spawn(parent, position ?? Vector3.zero, rotation ?? Quaternion.identity);
            return gameObj;
        }

        /// <summary>
        /// 批量异步生成GameObject
        /// </summary>
        /// <param name="pool">GameObject对象池</param>
        /// <param name="count">生成数量</param>
        /// <param name="parent">父物体</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="progressCallback">进度回调</param>
        /// <returns>异步任务</returns>
        public static async Task<GameObject[]> SpawnMultipleAsync(this EnhancedGameObjectPool pool, int count,
            Transform parent = null, CancellationToken cancellationToken = default, 
            IProgress<float> progressCallback = null)
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            if (count <= 0) return new GameObject[0];

            var results = new GameObject[count];
            
            // 分批生成，避免长时间阻塞
            const int batchSize = 10;
            int totalBatches = Mathf.CeilToInt((float)count / batchSize);
            
            for (int batch = 0; batch < totalBatches; batch++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                int startIndex = batch * batchSize;
                int currentBatchSize = Mathf.Min(batchSize, count - startIndex);
                
                // 生成当前批次的GameObject
                for (int i = 0; i < currentBatchSize; i++)
                {
                    results[startIndex + i] = pool.Spawn(parent);
                }
                
                // 报告进度
                float progress = (float)(batch + 1) / totalBatches;
                progressCallback?.Report(progress);
                
                // 让出控制权
                if (batch < totalBatches - 1)
                {
                    await Task.Yield();
                }
            }
            
            return results;
        }

        /// <summary>
        /// 批量异步回收GameObject
        /// </summary>
        /// <param name="pool">GameObject对象池</param>
        /// <param name="gameObjects">要回收的GameObject数组</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="progressCallback">进度回调</param>
        /// <returns>异步任务</returns>
        public static async Task DespawnMultipleAsync(this EnhancedGameObjectPool pool, GameObject[] gameObjects,
            CancellationToken cancellationToken = default, IProgress<float> progressCallback = null)
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            if (gameObjects == null || gameObjects.Length == 0) return;

            // 分批回收，避免长时间阻塞
            const int batchSize = 15;
            int totalBatches = Mathf.CeilToInt((float)gameObjects.Length / batchSize);
            
            for (int batch = 0; batch < totalBatches; batch++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                int startIndex = batch * batchSize;
                int currentBatchSize = Mathf.Min(batchSize, gameObjects.Length - startIndex);
                
                // 回收当前批次的GameObject
                for (int i = 0; i < currentBatchSize; i++)
                {
                    var gameObj = gameObjects[startIndex + i];
                    if (gameObj != null)
                    {
                        pool.Despawn(gameObj);
                    }
                }
                
                // 报告进度
                float progress = (float)(batch + 1) / totalBatches;
                progressCallback?.Report(progress);
                
                // 让出控制权
                if (batch < totalBatches - 1)
                {
                    await Task.Yield();
                }
            }
        }

        /// <summary>
        /// 延迟异步回收GameObject
        /// </summary>
        /// <param name="pool">GameObject对象池</param>
        /// <param name="gameObject">要回收的GameObject</param>
        /// <param name="delaySeconds">延迟时间（秒）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>异步任务</returns>
        public static async Task DespawnDelayedAsync(this EnhancedGameObjectPool pool, GameObject gameObject,
            float delaySeconds, CancellationToken cancellationToken = default)
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            if (gameObject == null) return;
            if (delaySeconds <= 0)
            {
                pool.Despawn(gameObject);
                return;
            }

            try
            {
                // 等待指定时间
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                
                // 检查GameObject是否仍然有效
                if (gameObject != null)
                {
                    pool.Despawn(gameObject);
                }
            }
            catch (OperationCanceledException)
            {
                // 如果操作被取消，仍然尝试回收GameObject
                if (gameObject != null)
                {
                    pool.Despawn(gameObject);
                }
                throw;
            }
        }
    }

    /// <summary>
    /// 增强版GameObject对象池协程操作扩展
    /// </summary>
    public static class EnhancedGameObjectPoolCoroutine
    {
        /// <summary>
        /// 协程预热GameObject对象池
        /// </summary>
        /// <param name="pool">GameObject对象池</param>
        /// <param name="count">预热数量</param>
        /// <param name="progressCallback">进度回调</param>
        /// <returns>协程</returns>
        public static IEnumerator PrewarmCoroutine(this EnhancedGameObjectPool pool, int count,
            System.Action<float> progressCallback = null)
        {
            if (pool == null || count <= 0) yield break;

            var createdObjects = new List<GameObject>(count);
            
            // 分批创建
            const int batchSize = 5;
            int totalBatches = Mathf.CeilToInt((float)count / batchSize);
            
            for (int batch = 0; batch < totalBatches; batch++)
            {
                int currentBatchSize = Mathf.Min(batchSize, count - batch * batchSize);
                
                // 创建当前批次的GameObject
                for (int i = 0; i < currentBatchSize; i++)
                {
                    var gameObj = pool.Spawn();
                    createdObjects.Add(gameObj);
                    gameObj.SetActive(false);
                }
                
                // 报告进度
                float progress = (float)(batch + 1) / totalBatches;
                progressCallback?.Invoke(progress);
                
                // 让出控制权
                yield return null;
            }
            
            // 将所有GameObject归还到池中
            foreach (var gameObj in createdObjects)
            {
                pool.Despawn(gameObj);
            }
        }

        /// <summary>
        /// 协程批量生成GameObject
        /// </summary>
        /// <param name="pool">GameObject对象池</param>
        /// <param name="count">生成数量</param>
        /// <param name="parent">父物体</param>
        /// <param name="resultCallback">结果回调</param>
        /// <param name="progressCallback">进度回调</param>
        /// <returns>协程</returns>
        public static IEnumerator SpawnMultipleCoroutine(this EnhancedGameObjectPool pool, int count,
            Transform parent, System.Action<GameObject[]> resultCallback, 
            System.Action<float> progressCallback = null)
        {
            if (pool == null || count <= 0)
            {
                resultCallback?.Invoke(new GameObject[0]);
                yield break;
            }

            var results = new GameObject[count];
            
            // 分批生成
            const int batchSize = 10;
            int totalBatches = Mathf.CeilToInt((float)count / batchSize);
            
            for (int batch = 0; batch < totalBatches; batch++)
            {
                int startIndex = batch * batchSize;
                int currentBatchSize = Mathf.Min(batchSize, count - startIndex);
                
                // 生成当前批次的GameObject
                for (int i = 0; i < currentBatchSize; i++)
                {
                    results[startIndex + i] = pool.Spawn(parent);
                }
                
                // 报告进度
                float progress = (float)(batch + 1) / totalBatches;
                progressCallback?.Invoke(progress);
                
                // 让出控制权
                yield return null;
            }
            
            resultCallback?.Invoke(results);
        }

        /// <summary>
        /// 协程延迟回收GameObject
        /// </summary>
        /// <param name="pool">GameObject对象池</param>
        /// <param name="gameObject">要回收的GameObject</param>
        /// <param name="delaySeconds">延迟时间（秒）</param>
        /// <returns>协程</returns>
        public static IEnumerator DespawnDelayedCoroutine(this EnhancedGameObjectPool pool, 
            GameObject gameObject, float delaySeconds)
        {
            if (pool == null || gameObject == null) yield break;
            
            if (delaySeconds > 0)
            {
                yield return new WaitForSeconds(delaySeconds);
            }
            
            // 检查GameObject是否仍然有效
            if (gameObject != null)
            {
                pool.Despawn(gameObject);
            }
        }
    }

    /// <summary>
    /// GameObject异步操作配置
    /// </summary>
    [System.Serializable]
    public class GameObjectAsyncConfig
    {
        /// <summary>
        /// 批处理大小
        /// </summary>
        [SerializeField] private int _batchSize = 10;

        /// <summary>
        /// 是否在预热时禁用GameObject
        /// </summary>
        [SerializeField] private bool _disableOnPrewarm = true;

        /// <summary>
        /// 异步操作超时时间（秒）
        /// </summary>
        [SerializeField] private float _timeoutSeconds = 30f;

        /// <summary>
        /// 批处理大小
        /// </summary>
        public int BatchSize
        {
            get => _batchSize;
            set => _batchSize = Mathf.Max(1, value);
        }

        /// <summary>
        /// 是否在预热时禁用GameObject
        /// </summary>
        public bool DisableOnPrewarm
        {
            get => _disableOnPrewarm;
            set => _disableOnPrewarm = value;
        }

        /// <summary>
        /// 异步操作超时时间（秒）
        /// </summary>
        public float TimeoutSeconds
        {
            get => _timeoutSeconds;
            set => _timeoutSeconds = Mathf.Max(0.1f, value);
        }

        /// <summary>
        /// 创建默认配置
        /// </summary>
        public static GameObjectAsyncConfig Default()
        {
            return new GameObjectAsyncConfig
            {
                BatchSize = 10,
                DisableOnPrewarm = true,
                TimeoutSeconds = 30f
            };
        }

        /// <summary>
        /// 创建高性能配置
        /// </summary>
        public static GameObjectAsyncConfig HighPerformance()
        {
            return new GameObjectAsyncConfig
            {
                BatchSize = 20,
                DisableOnPrewarm = true,
                TimeoutSeconds = 10f
            };
        }

        /// <summary>
        /// 创建内存优化配置
        /// </summary>
        public static GameObjectAsyncConfig MemoryOptimized()
        {
            return new GameObjectAsyncConfig
            {
                BatchSize = 5,
                DisableOnPrewarm = true,
                TimeoutSeconds = 60f
            };
        }
    }
}