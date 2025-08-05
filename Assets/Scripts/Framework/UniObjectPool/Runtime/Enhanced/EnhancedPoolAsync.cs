using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UniFramework.ObjectPool.Enhanced
{
    /// <summary>
    /// 增强版对象池异步操作支持
    /// 提供轻量级的异步加载功能，不依赖外部资源系统
    /// </summary>
    public static class EnhancedPoolAsync
    {
        /// <summary>
        /// 异步操作的默认超时时间（毫秒）
        /// </summary>
        public static int DefaultTimeoutMs { get; set; } = 30000; // 30秒

        /// <summary>
        /// 异步预热对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="pool">对象池</param>
        /// <param name="count">预热数量</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="progressCallback">进度回调</param>
        /// <returns>异步任务</returns>
        public static async Task PrewarmAsync<T>(this EnhancedPool<T> pool, int count, 
            CancellationToken cancellationToken = default, IProgress<float> progressCallback = null) 
            where T : class, new()
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            if (count <= 0) return;

            var createdObjects = new List<T>(count);
            
            try
            {
                // 分批创建，避免长时间阻塞
                const int batchSize = 10;
                int totalBatches = Mathf.CeilToInt((float)count / batchSize);
                
                for (int batch = 0; batch < totalBatches; batch++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    int currentBatchSize = Mathf.Min(batchSize, count - batch * batchSize);
                    
                    // 创建当前批次的对象
                    for (int i = 0; i < currentBatchSize; i++)
                    {
                        var obj = pool.Get();
                        createdObjects.Add(obj);
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
                
                // 将所有对象归还到池中
                foreach (var obj in createdObjects)
                {
                    pool.Return(obj);
                }
            }
            catch (OperationCanceledException)
            {
                // 清理已创建的对象
                foreach (var obj in createdObjects)
                {
                    pool.Return(obj);
                }
                throw;
            }
        }

        /// <summary>
        /// 异步获取对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="pool">对象池</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>异步任务</returns>
        public static async Task<T> GetAsync<T>(this EnhancedPool<T> pool, 
            CancellationToken cancellationToken = default) where T : class, new()
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            
            cancellationToken.ThrowIfCancellationRequested();
            
            // 对于简单对象，直接同步获取
            // 这里预留了异步扩展的可能性
            await Task.Yield();
            
            return pool.Get();
        }

        /// <summary>
        /// 批量异步获取对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="pool">对象池</param>
        /// <param name="count">获取数量</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="progressCallback">进度回调</param>
        /// <returns>异步任务</returns>
        public static async Task<T[]> GetMultipleAsync<T>(this EnhancedPool<T> pool, int count,
            CancellationToken cancellationToken = default, IProgress<float> progressCallback = null) 
            where T : class, new()
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            if (count <= 0) return new T[0];

            var results = new T[count];
            
            // 分批获取，避免长时间阻塞
            const int batchSize = 20;
            int totalBatches = Mathf.CeilToInt((float)count / batchSize);
            
            for (int batch = 0; batch < totalBatches; batch++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                int startIndex = batch * batchSize;
                int currentBatchSize = Mathf.Min(batchSize, count - startIndex);
                
                // 获取当前批次的对象
                for (int i = 0; i < currentBatchSize; i++)
                {
                    results[startIndex + i] = pool.Get();
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
        /// 批量异步归还对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="pool">对象池</param>
        /// <param name="objects">要归还的对象数组</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="progressCallback">进度回调</param>
        /// <returns>异步任务</returns>
        public static async Task ReturnMultipleAsync<T>(this EnhancedPool<T> pool, T[] objects,
            CancellationToken cancellationToken = default, IProgress<float> progressCallback = null) 
            where T : class, new()
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            if (objects == null || objects.Length == 0) return;

            // 分批归还，避免长时间阻塞
            const int batchSize = 20;
            int totalBatches = Mathf.CeilToInt((float)objects.Length / batchSize);
            
            for (int batch = 0; batch < totalBatches; batch++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                int startIndex = batch * batchSize;
                int currentBatchSize = Mathf.Min(batchSize, objects.Length - startIndex);
                
                // 归还当前批次的对象
                for (int i = 0; i < currentBatchSize; i++)
                {
                    var obj = objects[startIndex + i];
                    if (obj != null)
                    {
                        pool.Return(obj);
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
    }

    /// <summary>
    /// 增强版对象池协程操作支持
    /// 为不支持async/await的环境提供协程版本
    /// </summary>
    public static class EnhancedPoolCoroutine
    {
        /// <summary>
        /// 协程预热对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="pool">对象池</param>
        /// <param name="count">预热数量</param>
        /// <param name="progressCallback">进度回调</param>
        /// <returns>协程</returns>
        public static IEnumerator PrewarmCoroutine<T>(this EnhancedPool<T> pool, int count, 
            System.Action<float> progressCallback = null) where T : class, new()
        {
            if (pool == null || count <= 0) yield break;

            var createdObjects = new List<T>(count);
            
            // 分批创建
            const int batchSize = 10;
            int totalBatches = Mathf.CeilToInt((float)count / batchSize);
            
            for (int batch = 0; batch < totalBatches; batch++)
            {
                int currentBatchSize = Mathf.Min(batchSize, count - batch * batchSize);
                
                // 创建当前批次的对象
                for (int i = 0; i < currentBatchSize; i++)
                {
                    var obj = pool.Get();
                    createdObjects.Add(obj);
                }
                
                // 报告进度
                float progress = (float)(batch + 1) / totalBatches;
                progressCallback?.Invoke(progress);
                
                // 让出控制权
                yield return null;
            }
            
            // 将所有对象归还到池中
            foreach (var obj in createdObjects)
            {
                pool.Return(obj);
            }
        }

        /// <summary>
        /// 协程批量获取对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="pool">对象池</param>
        /// <param name="count">获取数量</param>
        /// <param name="resultCallback">结果回调</param>
        /// <param name="progressCallback">进度回调</param>
        /// <returns>协程</returns>
        public static IEnumerator GetMultipleCoroutine<T>(this EnhancedPool<T> pool, int count,
            System.Action<T[]> resultCallback, System.Action<float> progressCallback = null) 
            where T : class, new()
        {
            if (pool == null || count <= 0)
            {
                resultCallback?.Invoke(new T[0]);
                yield break;
            }

            var results = new T[count];
            
            // 分批获取
            const int batchSize = 20;
            int totalBatches = Mathf.CeilToInt((float)count / batchSize);
            
            for (int batch = 0; batch < totalBatches; batch++)
            {
                int startIndex = batch * batchSize;
                int currentBatchSize = Mathf.Min(batchSize, count - startIndex);
                
                // 获取当前批次的对象
                for (int i = 0; i < currentBatchSize; i++)
                {
                    results[startIndex + i] = pool.Get();
                }
                
                // 报告进度
                float progress = (float)(batch + 1) / totalBatches;
                progressCallback?.Invoke(progress);
                
                // 让出控制权
                yield return null;
            }
            
            resultCallback?.Invoke(results);
        }
    }

    /// <summary>
    /// 异步操作结果包装器
    /// </summary>
    /// <typeparam name="T">结果类型</typeparam>
    public class AsyncOperationResult<T>
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 操作结果
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// 操作耗时（毫秒）
        /// </summary>
        public long ElapsedMs { get; set; }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static AsyncOperationResult<T> Success(T result, long elapsedMs = 0)
        {
            return new AsyncOperationResult<T>
            {
                IsSuccess = true,
                Result = result,
                ElapsedMs = elapsedMs
            };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static AsyncOperationResult<T> Failure(string error, long elapsedMs = 0)
        {
            return new AsyncOperationResult<T>
            {
                IsSuccess = false,
                Error = error,
                ElapsedMs = elapsedMs
            };
        }
    }
}