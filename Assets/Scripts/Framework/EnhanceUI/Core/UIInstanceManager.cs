using System;
using System.Collections.Generic;
using UnityEngine;
using Framework.EnhanceUI.Config;

namespace Framework.EnhanceUI.Core
{
    /// <summary>
    /// UI实例管理器
    /// 负责管理UI面板的实例化、多开策略、生命周期等
    /// </summary>
    public class UIInstanceManager : MonoBehaviour
    {
        #region 字段和属性
        
        [Header("实例管理配置")]
        [SerializeField] private int maxInstancesPerUI = 10;
        [SerializeField] private bool enableInstancePooling = true;
        [SerializeField] private int poolInitialSize = 5;
        [SerializeField] private int poolMaxSize = 20;
        
        // UI实例字典 - UI名称 -> 实例列表
        private Dictionary<string, List<UIInstance>> uiInstances = new Dictionary<string, List<UIInstance>>();
        
        // 活跃实例字典 - 实例ID -> UI实例
        private Dictionary<string, UIInstance> activeInstances = new Dictionary<string, UIInstance>();
        
        // UI实例池 - UI名称 -> 实例池
        private Dictionary<string, Queue<EnhanceUIPanel>> instancePools = new Dictionary<string, Queue<EnhanceUIPanel>>();
        
        // 层级管理器引用
        private UILayerManager layerManager;
        
        /// <summary>
        /// 活跃实例数量
        /// </summary>
        public int ActiveInstanceCount => activeInstances.Count;
        
        /// <summary>
        /// 总实例数量
        /// </summary>
        public int TotalInstanceCount 
        { 
            get 
            {
                int totalCount = 0;
                // 遍历所有UI实例列表，累加数量
                foreach (var instanceList in uiInstances.Values)
                {
                    totalCount += instanceList.Count;
                }
                return totalCount;
            }
        }
        
        #endregion
        
        #region 事件委托
        
        /// <summary>
        /// 实例创建事件
        /// </summary>
        public event Action<UIInstance> OnInstanceCreated;
        
        /// <summary>
        /// 实例显示事件
        /// </summary>
        public event Action<UIInstance> OnInstanceShown;
        
        /// <summary>
        /// 实例隐藏事件
        /// </summary>
        public event Action<UIInstance> OnInstanceHidden;
        
        /// <summary>
        /// 实例销毁事件
        /// </summary>
        public event Action<UIInstance> OnInstanceDestroyed;
        
        /// <summary>
        /// 策略冲突事件
        /// </summary>
        public event Action<string, UIOpenStrategy, UIInstance> OnStrategyConflict;
        
        #endregion
        
        #region 初始化方法
        
        /// <summary>
        /// 初始化实例管理器
        /// </summary>
        /// <param name="layerMgr">层级管理器</param>
        public void Initialize(UILayerManager layerMgr)
        {
            layerManager = layerMgr ?? throw new ArgumentNullException(nameof(layerMgr));
            
            Debug.Log("[UIInstanceManager] 实例管理器初始化完成");
        }
        
        #endregion
        
        #region 实例管理方法
        
        /// <summary>
        /// 创建UI实例（泛型版本，避免装箱）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="panel">UI面板</param>
        /// <param name="config">UI配置</param>
        /// <param name="data">传递的数据</param>
        /// <returns>UI实例</returns>
        public UIInstance CreateInstance<T>(EnhanceUIPanel panel, UIConfigData config, T data)
        {
            if (panel == null || config == null)
            {
                Debug.LogError("[UIInstanceManager] 创建实例失败：面板或配置为空");
                return null;
            }
            
            try
            {
                // 检查多开策略
                if (!CanCreateInstance(config))
                {
                    Debug.LogWarning($"[UIInstanceManager] 根据策略 {config.openStrategy} 无法创建新实例: {config.uiName}");
                    return null;
                }
                
                // 处理现有实例（根据策略）
                HandleExistingInstances(config);
                
                // 创建新实例
                UIInstance instance = new UIInstance
                {
                    InstanceId = GenerateInstanceId(config.uiName),
                    UIName = config.uiName,
                    Panel = panel,
                    Config = config,
                    Data = data, // 这里仍然会装箱，但这是UIInstance的限制
                    CreateTime = Time.time,
                    State = UIInstanceState.Created
                };
                
                // 设置面板的实例ID
                panel.SetInstanceId(instance.InstanceId);
                
                // 初始化面板（使用泛型方法）
                panel.Initialize<T>(data);
                
                // 添加到管理字典
                AddInstanceToManagement(instance);
                
                // 设置层级
                SetInstanceLayer(instance);
                
                // 触发创建事件
                OnInstanceCreated?.Invoke(instance);
                
                Debug.Log($"[UIInstanceManager] 创建实例成功: {config.uiName} (ID: {instance.InstanceId})");
                return instance;
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIInstanceManager] 创建实例异常: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 创建UI实例
        /// </summary>
        /// <param name="panel">UI面板</param>
        /// <param name="config">UI配置</param>
        /// <param name="data">传递的数据</param>
        /// <returns>UI实例</returns>
        public UIInstance CreateInstance(EnhanceUIPanel panel, UIConfigData config, object data = null)
        {
            if (panel == null || config == null)
            {
                Debug.LogError("[UIInstanceManager] 创建实例失败：面板或配置为空");
                return null;
            }
            
            try
            {
                // 检查多开策略
                if (!CanCreateInstance(config))
                {
                    Debug.LogWarning($"[UIInstanceManager] 根据策略 {config.openStrategy} 无法创建新实例: {config.uiName}");
                    return null;
                }
                
                // 处理现有实例（根据策略）
                HandleExistingInstances(config);
                
                // 创建新实例
                UIInstance instance = new UIInstance
                {
                    InstanceId = GenerateInstanceId(config.uiName),
                    UIName = config.uiName,
                    Panel = panel,
                    Config = config,
                    Data = data,
                    CreateTime = Time.time,
                    State = UIInstanceState.Created
                };
                
                // 设置面板的实例ID
                panel.SetInstanceId(instance.InstanceId);
                
                // 添加到管理字典
                AddInstanceToManagement(instance);
                
                // 设置层级
                SetInstanceLayer(instance);
                
                // 触发创建事件
                OnInstanceCreated?.Invoke(instance);
                
                Debug.Log($"[UIInstanceManager] 创建实例成功: {config.uiName} (ID: {instance.InstanceId})");
                return instance;
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIInstanceManager] 创建实例异常: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 显示UI实例
        /// </summary>
        /// <param name="instance">UI实例</param>
        /// <returns>是否成功显示</returns>
        public bool ShowInstance(UIInstance instance)
        {
            if (instance == null || instance.Panel == null)
            {
                Debug.LogError("[UIInstanceManager] 显示实例失败：实例或面板为空");
                return false;
            }
            
            try
            {
                // 检查实例状态
                if (instance.State == UIInstanceState.Showing || instance.State == UIInstanceState.Shown)
                {
                    Debug.LogWarning($"[UIInstanceManager] 实例已在显示状态: {instance.UIName}");
                    return true;
                }
                
                // 更新状态
                instance.State = UIInstanceState.Showing;
                instance.LastShowTime = Time.time;
                
                // 显示面板（明确调用无参数版本）
                instance.Panel.Show(false);
                
                // 更新状态为已显示
                instance.State = UIInstanceState.Shown;
                
                // 触发显示事件
                OnInstanceShown?.Invoke(instance);
                
                Debug.Log($"[UIInstanceManager] 显示实例成功: {instance.UIName} (ID: {instance.InstanceId})");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIInstanceManager] 显示实例异常: {e.Message}");
                instance.State = UIInstanceState.Error;
                return false;
            }
        }
        
        /// <summary>
        /// 隐藏UI实例
        /// </summary>
        /// <param name="instance">UI实例</param>
        /// <returns>是否成功隐藏</returns>
        public bool HideInstance(UIInstance instance)
        {
            if (instance == null || instance.Panel == null)
            {
                Debug.LogError("[UIInstanceManager] 隐藏实例失败：实例或面板为空");
                return false;
            }
            
            try
            {
                // 检查实例状态
                if (instance.State == UIInstanceState.Hiding || instance.State == UIInstanceState.Hidden)
                {
                    Debug.LogWarning($"[UIInstanceManager] 实例已在隐藏状态: {instance.UIName}");
                    return true;
                }
                
                // 更新状态
                instance.State = UIInstanceState.Hiding;
                instance.LastHideTime = Time.time;
                
                // 隐藏面板
                instance.Panel.Hide();
                
                // 更新状态为已隐藏
                instance.State = UIInstanceState.Hidden;
                
                // 触发隐藏事件
                OnInstanceHidden?.Invoke(instance);
                
                Debug.Log($"[UIInstanceManager] 隐藏实例成功: {instance.UIName} (ID: {instance.InstanceId})");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIInstanceManager] 隐藏实例异常: {e.Message}");
                instance.State = UIInstanceState.Error;
                return false;
            }
        }
        
        /// <summary>
        /// 销毁UI实例
        /// </summary>
        /// <param name="instance">UI实例</param>
        /// <returns>是否成功销毁</returns>
        public bool DestroyInstance(UIInstance instance)
        {
            if (instance == null)
            {
                Debug.LogError("[UIInstanceManager] 销毁实例失败：实例为空");
                return false;
            }
            
            try
            {
                // 更新状态
                instance.State = UIInstanceState.Destroying;
                
                // 从管理字典移除
                RemoveInstanceFromManagement(instance);
                
                // 回收到对象池或销毁
                if (enableInstancePooling && instance.Panel != null)
                {
                    RecycleInstanceToPool(instance);
                }
                else if (instance.Panel != null)
                {
                    DestroyImmediate(instance.Panel.gameObject);
                }
                
                // 更新状态为已销毁
                instance.State = UIInstanceState.Destroyed;
                instance.DestroyTime = Time.time;
                
                // 触发销毁事件
                OnInstanceDestroyed?.Invoke(instance);
                
                Debug.Log($"[UIInstanceManager] 销毁实例成功: {instance.UIName} (ID: {instance.InstanceId})");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIInstanceManager] 销毁实例异常: {e.Message}");
                instance.State = UIInstanceState.Error;
                return false;
            }
        }
        
        #endregion
        
        #region 查询方法
        
        /// <summary>
        /// 根据实例ID获取实例
        /// </summary>
        /// <param name="instanceId">实例ID</param>
        /// <returns>UI实例</returns>
        public UIInstance GetInstance(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
                return null;
            
            activeInstances.TryGetValue(instanceId, out UIInstance instance);
            return instance;
        }
        
        /// <summary>
        /// 根据UI名称获取所有实例
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <returns>实例列表</returns>
        public List<UIInstance> GetInstances(string uiName)
        {
            if (string.IsNullOrEmpty(uiName))
                return new List<UIInstance>();
            
            if (uiInstances.TryGetValue(uiName, out List<UIInstance> instances))
            {
                return new List<UIInstance>(instances);
            }
            
            return new List<UIInstance>();
        }
        
        /// <summary>
        /// 根据UI名称获取活跃实例
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <returns>活跃实例列表</returns>
        public List<UIInstance> GetActiveInstances(string uiName)
        {
            var allInstances = GetInstances(uiName);
            var activeInstances = new List<UIInstance>();
            
            // 遍历所有实例，筛选出活跃的实例
            for (int i = 0; i < allInstances.Count; i++)
            {
                if (allInstances[i].IsActive())
                {
                    activeInstances.Add(allInstances[i]);
                }
            }
            
            return activeInstances;
        }
        
        /// <summary>
        /// 获取所有活跃实例
        /// </summary>
        /// <returns>所有活跃实例</returns>
        public List<UIInstance> GetAllActiveInstances()
        {
            return new List<UIInstance>(activeInstances.Values);
        }
        
        /// <summary>
        /// 检查UI是否有活跃实例
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <returns>是否有活跃实例</returns>
        public bool HasActiveInstance(string uiName)
        {
            return GetActiveInstances(uiName).Count > 0;
        }
        
        /// <summary>
        /// 获取UI的实例数量
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <returns>实例数量</returns>
        public int GetInstanceCount(string uiName)
        {
            return GetInstances(uiName).Count;
        }
        
        #endregion
        
        #region 策略处理方法
        
        /// <summary>
        /// 检查是否可以创建实例
        /// </summary>
        /// <param name="config">UI配置</param>
        /// <returns>是否可以创建</returns>
        private bool CanCreateInstance(UIConfigData config)
        {
            var activeInstances = GetActiveInstances(config.uiName);
            
            switch (config.openStrategy)
            {
                case UIOpenStrategy.Single:
                    return activeInstances.Count == 0;
                
                case UIOpenStrategy.Multiple:
                    return activeInstances.Count < maxInstancesPerUI;
                
                case UIOpenStrategy.Limited:
                    return activeInstances.Count < config.maxInstances;
                
                case UIOpenStrategy.Stack:
                case UIOpenStrategy.Queue:
                    return true; // 这些策略总是允许创建，但会处理现有实例
                
                default:
                    return true;
            }
        }
        
        /// <summary>
        /// 处理现有实例（根据策略）
        /// </summary>
        /// <param name="config">UI配置</param>
        private void HandleExistingInstances(UIConfigData config)
        {
            var activeInstances = GetActiveInstances(config.uiName);
            
            switch (config.openStrategy)
            {
                case UIOpenStrategy.Single:
                    // 单例模式：关闭所有现有实例
                    foreach (var instance in activeInstances)
                    {
                        DestroyInstance(instance);
                        OnStrategyConflict?.Invoke(config.uiName, config.openStrategy, instance);
                    }
                    break;
                
                case UIOpenStrategy.Stack:
                    // 栈模式：隐藏最顶层的实例
                    if (activeInstances.Count > 0)
                    {
                        // 找到创建时间最晚的实例（最顶层）
                        UIInstance topInstance = activeInstances[0];
                        for (int i = 1; i < activeInstances.Count; i++)
                        {
                            if (activeInstances[i].CreateTime > topInstance.CreateTime)
                            {
                                topInstance = activeInstances[i];
                            }
                        }
                        HideInstance(topInstance);
                    }
                    break;
                
                case UIOpenStrategy.Queue:
                    // 队列模式：关闭最早的实例
                    if (activeInstances.Count >= config.maxInstances)
                    {
                        // 找到创建时间最早的实例
                        UIInstance oldestInstance = activeInstances[0];
                        for (int i = 1; i < activeInstances.Count; i++)
                        {
                            if (activeInstances[i].CreateTime < oldestInstance.CreateTime)
                            {
                                oldestInstance = activeInstances[i];
                            }
                        }
                        DestroyInstance(oldestInstance);
                        OnStrategyConflict?.Invoke(config.uiName, config.openStrategy, oldestInstance);
                    }
                    break;
                
                case UIOpenStrategy.Limited:
                    // 限制模式：如果超过限制，关闭最早的实例
                    while (activeInstances.Count >= config.maxInstances)
                    {
                        // 找到创建时间最早的实例
                        UIInstance oldestInstance = activeInstances[0];
                        for (int i = 1; i < activeInstances.Count; i++)
                        {
                            if (activeInstances[i].CreateTime < oldestInstance.CreateTime)
                            {
                                oldestInstance = activeInstances[i];
                            }
                        }
                        DestroyInstance(oldestInstance);
                        OnStrategyConflict?.Invoke(config.uiName, config.openStrategy, oldestInstance);
                        activeInstances.Remove(oldestInstance);
                    }
                    break;
            }
        }
        
        #endregion
        
        #region 对象池管理
        
        /// <summary>
        /// 从对象池获取实例
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <returns>UI面板</returns>
        public EnhanceUIPanel GetInstanceFromPool(string uiName)
        {
            if (!enableInstancePooling || string.IsNullOrEmpty(uiName))
                return null;
            
            if (instancePools.TryGetValue(uiName, out Queue<EnhanceUIPanel> pool) && pool.Count > 0)
            {
                var panel = pool.Dequeue();
                if (panel != null)
                {
                    panel.gameObject.SetActive(true);
                    Debug.Log($"[UIInstanceManager] 从对象池获取实例: {uiName}");
                    return panel;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 回收实例到对象池
        /// </summary>
        /// <param name="instance">UI实例</param>
        private void RecycleInstanceToPool(UIInstance instance)
        {
            if (instance?.Panel == null)
                return;
            
            string uiName = instance.UIName;
            
            // 确保对象池存在
            if (!instancePools.ContainsKey(uiName))
            {
                instancePools[uiName] = new Queue<EnhanceUIPanel>();
            }
            
            var pool = instancePools[uiName];
            
            // 检查对象池大小限制
            if (pool.Count >= poolMaxSize)
            {
                DestroyImmediate(instance.Panel.gameObject);
                Debug.Log($"[UIInstanceManager] 对象池已满，直接销毁实例: {uiName}");
                return;
            }
            
            // 重置面板状态
            instance.Panel.ResetToPool();
            instance.Panel.gameObject.SetActive(false);
            
            // 添加到对象池
            pool.Enqueue(instance.Panel);
            
            Debug.Log($"[UIInstanceManager] 回收实例到对象池: {uiName}");
        }
        
        /// <summary>
        /// 预热对象池
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="prefab">预制体</param>
        /// <param name="count">预热数量</param>
        public void WarmupPool(string uiName, GameObject prefab, int count = -1)
        {
            if (!enableInstancePooling || string.IsNullOrEmpty(uiName) || prefab == null)
                return;
            
            if (count <= 0)
                count = poolInitialSize;
            
            // 确保对象池存在
            if (!instancePools.ContainsKey(uiName))
            {
                instancePools[uiName] = new Queue<EnhanceUIPanel>();
            }
            
            var pool = instancePools[uiName];
            
            // 创建预热实例
            for (int i = 0; i < count && pool.Count < poolMaxSize; i++)
            {
                var go = Instantiate(prefab, transform);
                var panel = go.GetComponent<EnhanceUIPanel>();
                
                if (panel != null)
                {
                    panel.ResetToPool();
                    go.SetActive(false);
                    pool.Enqueue(panel);
                }
                else
                {
                    DestroyImmediate(go);
                }
            }
            
            Debug.Log($"[UIInstanceManager] 预热对象池完成: {uiName}, 数量: {pool.Count}");
        }
        
        /// <summary>
        /// 清理对象池
        /// </summary>
        /// <param name="uiName">UI名称，为空则清理所有</param>
        public void ClearPool(string uiName = null)
        {
            if (string.IsNullOrEmpty(uiName))
            {
                // 清理所有对象池
                foreach (var pool in instancePools.Values)
                {
                    while (pool.Count > 0)
                    {
                        var panel = pool.Dequeue();
                        if (panel != null)
                            DestroyImmediate(panel.gameObject);
                    }
                }
                instancePools.Clear();
                Debug.Log("[UIInstanceManager] 清理所有对象池");
            }
            else
            {
                // 清理指定对象池
                if (instancePools.TryGetValue(uiName, out Queue<EnhanceUIPanel> pool))
                {
                    while (pool.Count > 0)
                    {
                        var panel = pool.Dequeue();
                        if (panel != null)
                            DestroyImmediate(panel.gameObject);
                    }
                    instancePools.Remove(uiName);
                    Debug.Log($"[UIInstanceManager] 清理对象池: {uiName}");
                }
            }
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 生成实例ID
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <returns>实例ID</returns>
        private string GenerateInstanceId(string uiName)
        {
            return $"{uiName}_{Time.time:F3}_{UnityEngine.Random.Range(1000, 9999)}";
        }
        
        /// <summary>
        /// 添加实例到管理字典
        /// </summary>
        /// <param name="instance">UI实例</param>
        private void AddInstanceToManagement(UIInstance instance)
        {
            // 添加到UI实例字典
            if (!uiInstances.ContainsKey(instance.UIName))
            {
                uiInstances[instance.UIName] = new List<UIInstance>();
            }
            uiInstances[instance.UIName].Add(instance);
            
            // 添加到活跃实例字典
            activeInstances[instance.InstanceId] = instance;
        }
        
        /// <summary>
        /// 从管理字典移除实例
        /// </summary>
        /// <param name="instance">UI实例</param>
        private void RemoveInstanceFromManagement(UIInstance instance)
        {
            // 从UI实例字典移除
            if (uiInstances.TryGetValue(instance.UIName, out List<UIInstance> instances))
            {
                instances.Remove(instance);
                if (instances.Count == 0)
                {
                    uiInstances.Remove(instance.UIName);
                }
            }
            
            // 从活跃实例字典移除
            activeInstances.Remove(instance.InstanceId);
        }
        
        /// <summary>
        /// 设置实例层级
        /// </summary>
        /// <param name="instance">UI实例</param>
        private void SetInstanceLayer(UIInstance instance)
        {
            if (layerManager != null && instance.Panel != null)
            {
                Transform layerTransform = layerManager.GetLayer(instance.Config.layerType);
                if (layerTransform != null)
                {
                    instance.Panel.transform.SetParent(layerTransform, false);
                }
            }
        }
        
        /// <summary>
        /// 获取实例管理状态
        /// </summary>
        /// <returns>管理状态</returns>
        public InstanceManagerStatus GetStatus()
        {
            // 计算对象池中的总实例数量
            int poolCount = 0;
            foreach (var pool in instancePools.Values)
            {
                poolCount += pool.Count;
            }
            
            return new InstanceManagerStatus
            {
                ActiveInstanceCount = activeInstances.Count,
                TotalInstanceCount = TotalInstanceCount,
                UITypeCount = uiInstances.Count,
                PoolCount = poolCount,
                PoolTypeCount = instancePools.Count
            };
        }
        
        #endregion
        
        #region Unity生命周期
        
        private void Update()
        {
            // 清理无效实例
            CleanupInvalidInstances();
        }
        
        /// <summary>
        /// 清理无效实例
        /// </summary>
        private void CleanupInvalidInstances()
        {
            var instancesToRemove = new List<UIInstance>();
            
            foreach (var instance in activeInstances.Values)
            {
                if (instance.Panel == null || instance.Panel.gameObject == null)
                {
                    instancesToRemove.Add(instance);
                }
            }
            
            foreach (var instance in instancesToRemove)
            {
                RemoveInstanceFromManagement(instance);
                Debug.LogWarning($"[UIInstanceManager] 清理无效实例: {instance.UIName}");
            }
        }
        
        private void OnDestroy()
        {
            // 清理所有实例
            var allInstances = new List<UIInstance>(activeInstances.Values);
            foreach (var instance in allInstances)
            {
                DestroyInstance(instance);
            }
            
            // 清理对象池
            ClearPool();
            
            Debug.Log("[UIInstanceManager] 实例管理器已销毁");
        }
        
        #endregion
    }
    
    /// <summary>
    /// UI实例类
    /// </summary>
    [Serializable]
    public class UIInstance
    {
        /// <summary>
        /// 实例ID
        /// </summary>
        public string InstanceId;
        
        /// <summary>
        /// UI名称
        /// </summary>
        public string UIName;
        
        /// <summary>
        /// UI面板
        /// </summary>
        public EnhanceUIPanel Panel;
        
        /// <summary>
        /// UI配置
        /// </summary>
        public UIConfigData Config;
        
        /// <summary>
        /// 传递的数据
        /// </summary>
        public object Data;
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public float CreateTime;
        
        /// <summary>
        /// 最后显示时间
        /// </summary>
        public float LastShowTime;
        
        /// <summary>
        /// 最后隐藏时间
        /// </summary>
        public float LastHideTime;
        
        /// <summary>
        /// 销毁时间
        /// </summary>
        public float DestroyTime;
        
        /// <summary>
        /// 实例状态
        /// </summary>
        public UIInstanceState State;
        
        /// <summary>
        /// 是否活跃
        /// </summary>
        /// <returns>是否活跃</returns>
        public bool IsActive()
        {
            return State == UIInstanceState.Shown || State == UIInstanceState.Showing;
        }
        
        /// <summary>
        /// 是否有效
        /// </summary>
        /// <returns>是否有效</returns>
        public bool IsValid()
        {
            return Panel != null && Panel.gameObject != null && State != UIInstanceState.Destroyed;
        }
        
        /// <summary>
        /// 获取存活时间
        /// </summary>
        /// <returns>存活时间（秒）</returns>
        public float GetLifeTime()
        {
            if (State == UIInstanceState.Destroyed)
                return DestroyTime - CreateTime;
            else
                return Time.time - CreateTime;
        }
    }
    
    /// <summary>
    /// UI实例状态枚举
    /// </summary>
    public enum UIInstanceState
    {
        /// <summary>
        /// 已创建
        /// </summary>
        Created,
        
        /// <summary>
        /// 正在显示
        /// </summary>
        Showing,
        
        /// <summary>
        /// 已显示
        /// </summary>
        Shown,
        
        /// <summary>
        /// 正在隐藏
        /// </summary>
        Hiding,
        
        /// <summary>
        /// 已隐藏
        /// </summary>
        Hidden,
        
        /// <summary>
        /// 正在销毁
        /// </summary>
        Destroying,
        
        /// <summary>
        /// 已销毁
        /// </summary>
        Destroyed,
        
        /// <summary>
        /// 错误状态
        /// </summary>
        Error
    }
    
    /// <summary>
    /// 实例管理器状态结构
    /// </summary>
    [Serializable]
    public struct InstanceManagerStatus
    {
        /// <summary>
        /// 活跃实例数量
        /// </summary>
        public int ActiveInstanceCount;
        
        /// <summary>
        /// 总实例数量
        /// </summary>
        public int TotalInstanceCount;
        
        /// <summary>
        /// UI类型数量
        /// </summary>
        public int UITypeCount;
        
        /// <summary>
        /// 对象池实例数量
        /// </summary>
        public int PoolCount;
        
        /// <summary>
        /// 对象池类型数量
        /// </summary>
        public int PoolTypeCount;
    }
}