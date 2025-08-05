using System;
using UnityEngine;

namespace UniFramework.ObjectPool.Enhanced
{
    /// <summary>
    /// 增强版对象池扩展方法
    /// 提供便捷的对象池操作接口，简化常用操作
    /// 在极简版基础上增加更多实用的扩展功能
    /// </summary>
    public static class EnhancedPoolExtensions
    {
        #region GameObject 扩展

        /// <summary>
        /// 创建 GameObject 对象池
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="parent">父级变换</param>
        /// <param name="config">对象池配置</param>
        /// <param name="poolName">对象池名称</param>
        /// <returns>GameObject 对象池</returns>
        public static EnhancedGameObjectPool CreateEnhancedPool(
            this GameObject prefab,
            Transform parent = null,
            EnhancedPoolConfig config = null,
            string poolName = null)
        {
            return new EnhancedGameObjectPool(prefab, parent, config, poolName);
        }

        /// <summary>
        /// 生成 GameObject（从对象池获取）
        /// </summary>
        /// <param name="pool">GameObject 对象池</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <returns>GameObject 实例</returns>
        public static GameObject Spawn(this EnhancedGameObjectPool pool, Vector3 position, Quaternion rotation)
        {
            return pool.Get(position, rotation);
        }

        /// <summary>
        /// 生成 GameObject（从对象池获取）
        /// </summary>
        /// <param name="pool">GameObject 对象池</param>
        /// <param name="position">位置</param>
        /// <returns>GameObject 实例</returns>
        public static GameObject Spawn(this EnhancedGameObjectPool pool, Vector3 position)
        {
            return pool.Get(position);
        }

        /// <summary>
        /// 生成 GameObject（从对象池获取）
        /// </summary>
        /// <param name="pool">GameObject 对象池</param>
        /// <returns>GameObject 实例</returns>
        public static GameObject Spawn(this EnhancedGameObjectPool pool)
        {
            return pool.Get();
        }

        /// <summary>
        /// 回收 GameObject（归还到对象池）
        /// </summary>
        /// <param name="gameObject">要回收的 GameObject</param>
        /// <param name="pool">GameObject 对象池</param>
        /// <returns>是否成功回收</returns>
        public static bool Recycle(this GameObject gameObject, EnhancedGameObjectPool pool)
        {
            return pool.Return(gameObject);
        }

        /// <summary>
        /// 延迟回收 GameObject
        /// </summary>
        /// <param name="gameObject">要回收的 GameObject</param>
        /// <param name="pool">GameObject 对象池</param>
        /// <param name="delay">延迟时间（秒）</param>
        public static void RecycleDelayed(this GameObject gameObject, EnhancedGameObjectPool pool, float delay)
        {
            if (gameObject != null)
            {
                var recycler = gameObject.GetComponent<DelayedRecycler>();
                if (recycler == null)
                {
                    recycler = gameObject.AddComponent<DelayedRecycler>();
                }
                recycler.StartRecycle(pool, delay);
            }
        }

        #endregion

        #region 通用对象池扩展

        /// <summary>
        /// 获取或创建对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="createFunc">对象创建函数</param>
        /// <param name="resetAction">对象重置动作</param>
        /// <param name="destroyAction">对象销毁动作</param>
        /// <param name="config">对象池配置</param>
        /// <returns>对象池实例</returns>
        public static EnhancedPool<T> GetOrCreateEnhancedPool<T>(
            Func<T> createFunc,
            Action<T> resetAction = null,
            Action<T> destroyAction = null,
            EnhancedPoolConfig config = null) where T : class
        {
            var pool = EnhancedPoolManager.GetPool<T>();
            if (pool == null)
            {
                pool = EnhancedPoolManager.CreatePool(createFunc, resetAction, destroyAction, config);
            }
            return pool;
        }

        /// <summary>
        /// 获取或创建命名对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="poolName">对象池名称</param>
        /// <param name="createFunc">对象创建函数</param>
        /// <param name="resetAction">对象重置动作</param>
        /// <param name="destroyAction">对象销毁动作</param>
        /// <param name="config">对象池配置</param>
        /// <returns>对象池实例</returns>
        public static EnhancedPool<T> GetOrCreateEnhancedPool<T>(
            string poolName,
            Func<T> createFunc,
            Action<T> resetAction = null,
            Action<T> destroyAction = null,
            EnhancedPoolConfig config = null) where T : class
        {
            var pool = EnhancedPoolManager.GetPool<T>(poolName);
            if (pool == null)
            {
                pool = EnhancedPoolManager.CreatePool(poolName, createFunc, resetAction, destroyAction, config);
            }
            return pool;
        }

        /// <summary>
        /// 预热对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="pool">对象池</param>
        /// <param name="count">预热数量</param>
        /// <returns>对象池实例（用于链式调用）</returns>
        public static EnhancedPool<T> PrewarmEnhanced<T>(this EnhancedPool<T> pool, int count) where T : class
        {
            pool.Prewarm(count);
            return pool;
        }

        /// <summary>
        /// 批量获取对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="pool">对象池</param>
        /// <param name="count">获取数量</param>
        /// <returns>对象数组</returns>
        public static T[] GetMultiple<T>(this EnhancedPool<T> pool, int count) where T : class
        {
            if (count <= 0)
                return new T[0];

            var items = new T[count];
            for (int i = 0; i < count; i++)
            {
                items[i] = pool.Get();
            }
            return items;
        }

        /// <summary>
        /// 批量归还对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="pool">对象池</param>
        /// <param name="items">要归还的对象数组</param>
        /// <returns>成功归还的数量</returns>
        public static int ReturnMultiple<T>(this EnhancedPool<T> pool, params T[] items) where T : class
        {
            if (items == null || items.Length == 0)
                return 0;

            int returnedCount = 0;
            for (int i = 0; i < items.Length; i++)
            {
                if (pool.Return(items[i]))
                {
                    returnedCount++;
                }
            }
            return returnedCount;
        }

        #endregion

        #region 配置扩展

        /// <summary>
        /// 设置最大容量
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <param name="maxCapacity">最大容量</param>
        /// <returns>配置对象（用于链式调用）</returns>
        public static EnhancedPoolConfig WithMaxCapacity(this EnhancedPoolConfig config, int maxCapacity)
        {
            config.MaxCapacity = maxCapacity;
            return config;
        }

        /// <summary>
        /// 设置初始容量
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <param name="initialCapacity">初始容量</param>
        /// <returns>配置对象（用于链式调用）</returns>
        public static EnhancedPoolConfig WithInitialCapacity(this EnhancedPoolConfig config, int initialCapacity)
        {
            config.InitialCapacity = initialCapacity;
            return config;
        }

        /// <summary>
        /// 启用统计信息收集
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <param name="enabled">是否启用</param>
        /// <returns>配置对象（用于链式调用）</returns>
        public static EnhancedPoolConfig WithStatistics(this EnhancedPoolConfig config, bool enabled = true)
        {
            config.EnableStatistics = enabled;
            return config;
        }

        /// <summary>
        /// 启用对象验证
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <param name="enabled">是否启用</param>
        /// <returns>配置对象（用于链式调用）</returns>
        public static EnhancedPoolConfig WithValidation(this EnhancedPoolConfig config, bool enabled = true)
        {
            config.EnableValidation = enabled;
            return config;
        }

        /// <summary>
        /// 设置标签
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <param name="tag">标签</param>
        /// <returns>配置对象（用于链式调用）</returns>
        public static EnhancedPoolConfig WithTag(this EnhancedPoolConfig config, string tag)
        {
            config.Tag = tag;
            return config;
        }

        #endregion

        #region 统计信息扩展

        /// <summary>
        /// 获取命中率百分比字符串
        /// </summary>
        /// <param name="statistics">统计信息</param>
        /// <returns>命中率百分比字符串</returns>
        public static string GetHitRateString(this EnhancedPoolStatistics statistics)
        {
            return $"{statistics.HitRate:P2}";
        }

        /// <summary>
        /// 获取复用率百分比字符串
        /// </summary>
        /// <param name="statistics">统计信息</param>
        /// <returns>复用率百分比字符串</returns>
        public static string GetReuseRateString(this EnhancedPoolStatistics statistics)
        {
            return $"{statistics.ReuseRate:P2}";
        }

        /// <summary>
        /// 获取效率百分比字符串
        /// </summary>
        /// <param name="statistics">统计信息</param>
        /// <returns>效率百分比字符串</returns>
        public static string GetEfficiencyString(this EnhancedPoolStatistics statistics)
        {
            return $"{statistics.Efficiency:P2}";
        }

        /// <summary>
        /// 获取运行时长字符串
        /// </summary>
        /// <param name="statistics">统计信息</param>
        /// <returns>运行时长字符串</returns>
        public static string GetUptimeString(this EnhancedPoolStatistics statistics)
        {
            var uptime = statistics.Uptime;
            if (uptime.TotalDays >= 1)
                return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
            else if (uptime.TotalHours >= 1)
                return $"{uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
            else if (uptime.TotalMinutes >= 1)
                return $"{uptime.Minutes}m {uptime.Seconds}s";
            else
                return $"{uptime.TotalSeconds:F1}s";
        }

        #endregion

        #region 调试和监控扩展

        /// <summary>
        /// 记录对象池状态到控制台
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="pool">对象池</param>
        /// <param name="prefix">日志前缀</param>
        public static void LogStatus<T>(this EnhancedPool<T> pool, string prefix = "") where T : class
        {
            var status = pool.GetStatusInfo();
            Debug.Log($"{prefix}{status}");
        }

        /// <summary>
        /// 记录详细状态到控制台
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="pool">对象池</param>
        /// <param name="prefix">日志前缀</param>
        public static void LogDetailedStatus<T>(this EnhancedPool<T> pool, string prefix = "") where T : class
        {
            var detailedStatus = pool.GetDetailedStatusInfo();
            Debug.Log($"{prefix}{detailedStatus}");
        }

        /// <summary>
        /// 检查对象池健康状态
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="pool">对象池</param>
        /// <returns>是否健康</returns>
        public static bool IsHealthy<T>(this EnhancedPool<T> pool) where T : class
        {
            var stats = pool.Statistics;
            
            // 检查基本指标
            if (stats.ValidationFailureCount > stats.TotalGetCount * 0.1f) // 验证失败率超过10%
                return false;
                
            if (stats.DiscardedCount > stats.TotalCreatedCount * 0.2f) // 丢弃率超过20%
                return false;
                
            if (pool.AvailableCount == 0 && pool.ActiveCount > pool.Config.MaxCapacity * 0.9f) // 接近容量上限且无可用对象
                return false;
                
            return true;
        }

        /// <summary>
        /// 获取性能评级
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="pool">对象池</param>
        /// <returns>性能评级字符串</returns>
        public static string GetPerformanceRating<T>(this EnhancedPool<T> pool) where T : class, new()
        {
            if (pool?.Statistics == null) return "未知";
            
            var rating = pool.GetPerformanceRating();
            return rating switch
            {
                PerformanceRating.Excellent => "优秀",
                PerformanceRating.Good => "良好", 
                PerformanceRating.Average => "一般",
                PerformanceRating.Poor => "较差",
                PerformanceRating.Critical => "严重",
                _ => "未知"
            };
        }

        #region 异步操作快捷方法

        /// <summary>
        /// 异步预热对象池（快捷方法）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="pool">对象池</param>
        /// <param name="count">预热数量</param>
        /// <returns>异步任务</returns>
        public static System.Threading.Tasks.Task PrewarmAsync<T>(this EnhancedPool<T> pool, int count) 
            where T : class, new()
        {
            return EnhancedPoolAsync.PrewarmAsync(pool, count);
        }

        /// <summary>
        /// 异步获取对象（快捷方法）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="pool">对象池</param>
        /// <returns>异步任务</returns>
        public static System.Threading.Tasks.Task<T> GetAsync<T>(this EnhancedPool<T> pool) 
            where T : class, new()
        {
            return EnhancedPoolAsync.GetAsync(pool);
        }

        /// <summary>
        /// 批量异步获取对象（快捷方法）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="pool">对象池</param>
        /// <param name="count">获取数量</param>
        /// <returns>异步任务</returns>
        public static System.Threading.Tasks.Task<T[]> GetMultipleAsync<T>(this EnhancedPool<T> pool, int count) 
            where T : class, new()
        {
            return EnhancedPoolAsync.GetMultipleAsync(pool, count);
        }

        /// <summary>
        /// 异步预热GameObject对象池（快捷方法）
        /// </summary>
        /// <param name="pool">GameObject对象池</param>
        /// <param name="count">预热数量</param>
        /// <returns>异步任务</returns>
        public static System.Threading.Tasks.Task PrewarmAsync(this EnhancedGameObjectPool pool, int count)
        {
            return EnhancedGameObjectPoolAsync.PrewarmAsync(pool, count);
        }

        /// <summary>
        /// 异步生成GameObject（快捷方法）
        /// </summary>
        /// <param name="pool">GameObject对象池</param>
        /// <param name="parent">父物体</param>
        /// <returns>异步任务</returns>
        public static System.Threading.Tasks.Task<UnityEngine.GameObject> SpawnAsync(this EnhancedGameObjectPool pool, 
            UnityEngine.Transform parent = null)
        {
            return EnhancedGameObjectPoolAsync.SpawnAsync(pool, parent);
        }

        /// <summary>
        /// 延迟异步回收GameObject（快捷方法）
        /// </summary>
        /// <param name="pool">GameObject对象池</param>
        /// <param name="gameObject">要回收的GameObject</param>
        /// <param name="delaySeconds">延迟时间（秒）</param>
        /// <returns>异步任务</returns>
        public static System.Threading.Tasks.Task DespawnDelayedAsync(this EnhancedGameObjectPool pool, 
            UnityEngine.GameObject gameObject, float delaySeconds)
        {
            return EnhancedGameObjectPoolAsync.DespawnDelayedAsync(pool, gameObject, delaySeconds);
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// 延迟回收组件
    /// 用于实现 GameObject 的延迟回收功能
    /// </summary>
    internal class DelayedRecycler : MonoBehaviour
    {
        private EnhancedGameObjectPool _pool;
        private float _delay;
        private float _timer;
        private bool _isRecycling;

        /// <summary>
        /// 开始延迟回收
        /// </summary>
        /// <param name="pool">对象池</param>
        /// <param name="delay">延迟时间</param>
        public void StartRecycle(EnhancedGameObjectPool pool, float delay)
        {
            if (_isRecycling)
                return;

            _pool = pool;
            _delay = delay;
            _timer = 0f;
            _isRecycling = true;
        }

        private void Update()
        {
            if (!_isRecycling)
                return;

            _timer += Time.deltaTime;
            if (_timer >= _delay)
            {
                _isRecycling = false;
                if (_pool != null && gameObject != null)
                {
                    _pool.Return(gameObject);
                }
            }
        }

        private void OnDisable()
        {
            _isRecycling = false;
        }
    }
}