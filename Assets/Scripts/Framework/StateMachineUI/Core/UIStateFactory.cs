// using System;
// using System.Collections.Generic;
// using UnityEngine;
//
// namespace Framework.StateMachineUI.Core
// {
//     /// <summary>
//     /// UI状态工厂
//     /// 负责创建和管理UI状态实例
//     /// </summary>
//     public class UIStateFactory
//     {
//         #region 字段和属性
//         
//         /// <summary>
//         /// UI管理器引用
//         /// </summary>
//         private readonly StateMachineUIManager uiManager;
//         
//         /// <summary>
//         /// 状态类型注册表
//         /// </summary>
//         private readonly Dictionary<string, Type> stateTypeRegistry = new Dictionary<string, Type>();
//         
//         /// <summary>
//         /// 状态实例缓存
//         /// </summary>
//         private readonly Dictionary<string, IUIState> stateInstanceCache = new Dictionary<string, IUIState>();
//         
//         /// <summary>
//         /// 状态配置缓存
//         /// </summary>
//         private readonly Dictionary<string, UIStateConfig> stateConfigCache = new Dictionary<string, UIStateConfig>();
//         
//         /// <summary>
//         /// 是否启用状态缓存
//         /// </summary>
//         public bool EnableStateCache { get; set; } = true;
//         
//         /// <summary>
//         /// 最大缓存状态数量
//         /// </summary>
//         public int MaxCacheSize { get; set; } = 20;
//         
//         #endregion
//         
//         #region 构造函数
//         
//         /// <summary>
//         /// 构造函数
//         /// </summary>
//         /// <param name="uiManager">UI管理器</param>
//         public UIStateFactory(StateMachineUIManager uiManager)
//         {
//             this.uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
//             
//             // 注册默认状态类型
//             RegisterDefaultStateTypes();
//         }
//         
//         #endregion
//         
//         #region 状态类型注册
//         
//         /// <summary>
//         /// 注册状态类型
//         /// </summary>
//         /// <typeparam name="T">状态类型</typeparam>
//         /// <param name="stateName">状态名称</param>
//         public void RegisterStateType<T>(string stateName) where T : IUIState
//         {
//             if (string.IsNullOrEmpty(stateName))
//             {
//                 Debug.LogError("[状态工厂] 状态名称不能为空");
//                 return;
//             }
//             
//             Type stateType = typeof(T);
//             
//             if (stateTypeRegistry.ContainsKey(stateName))
//             {
//                 Debug.LogWarning($"[状态工厂] 状态类型已存在，将被覆盖: {stateName}");
//             }
//             
//             stateTypeRegistry[stateName] = stateType;
//             Debug.Log($"[状态工厂] 注册状态类型: {stateName} -> {stateType.Name}");
//         }
//         
//         /// <summary>
//         /// 注册状态类型
//         /// </summary>
//         /// <param name="stateName">状态名称</param>
//         /// <param name="stateType">状态类型</param>
//         public void RegisterStateType(string stateName, Type stateType)
//         {
//             if (string.IsNullOrEmpty(stateName))
//             {
//                 Debug.LogError("[状态工厂] 状态名称不能为空");
//                 return;
//             }
//             
//             if (stateType == null)
//             {
//                 Debug.LogError("[状态工厂] 状态类型不能为空");
//                 return;
//             }
//             
//             if (!typeof(IUIState).IsAssignableFrom(stateType))
//             {
//                 Debug.LogError($"[状态工厂] 状态类型必须实现IUIState接口: {stateType.Name}");
//                 return;
//             }
//             
//             if (stateTypeRegistry.ContainsKey(stateName))
//             {
//                 Debug.LogWarning($"[状态工厂] 状态类型已存在，将被覆盖: {stateName}");
//             }
//             
//             stateTypeRegistry[stateName] = stateType;
//             Debug.Log($"[状态工厂] 注册状态类型: {stateName} -> {stateType.Name}");
//         }
//         
//         /// <summary>
//         /// 注销状态类型
//         /// </summary>
//         /// <param name="stateName">状态名称</param>
//         /// <returns>是否注销成功</returns>
//         public bool UnregisterStateType(string stateName)
//         {
//             if (string.IsNullOrEmpty(stateName))
//                 return false;
//             
//             bool removed = stateTypeRegistry.Remove(stateName);
//             if (removed)
//             {
//                 Debug.Log($"[状态工厂] 注销状态类型: {stateName}");
//                 
//                 // 同时清理相关缓存
//                 stateInstanceCache.Remove(stateName);
//                 stateConfigCache.Remove(stateName);
//             }
//             
//             return removed;
//         }
//         
//         /// <summary>
//         /// 检查状态类型是否已注册
//         /// </summary>
//         /// <param name="stateName">状态名称</param>
//         /// <returns>是否已注册</returns>
//         public bool IsStateTypeRegistered(string stateName)
//         {
//             return !string.IsNullOrEmpty(stateName) && stateTypeRegistry.ContainsKey(stateName);
//         }
//         
//         /// <summary>
//         /// 获取已注册的状态类型
//         /// </summary>
//         /// <param name="stateName">状态名称</param>
//         /// <returns>状态类型，如果未找到返回null</returns>
//         public Type GetRegisteredStateType(string stateName)
//         {
//             if (string.IsNullOrEmpty(stateName))
//                 return null;
//             
//             stateTypeRegistry.TryGetValue(stateName, out Type stateType);
//             return stateType;
//         }
//         
//         /// <summary>
//         /// 获取所有已注册的状态名称
//         /// </summary>
//         /// <returns>状态名称列表</returns>
//         public List<string> GetRegisteredStateNames()
//         {
//             return new List<string>(stateTypeRegistry.Keys);
//         }
//         
//         /// <summary>
//         /// 注册默认状态类型
//         /// </summary>
//         private void RegisterDefaultStateTypes()
//         {
//             // 这里可以注册一些默认的状态类型
//             // 具体的状态类将在后续创建
//         }
//         
//         #endregion
//         
//         #region 状态创建
//         
//         /// <summary>
//         /// 创建状态实例
//         /// </summary>
//         /// <typeparam name="T">状态类型</typeparam>
//         /// <param name="stateName">状态名称</param>
//         /// <param name="stateType">UI状态类型</param>
//         /// <param name="priority">优先级</param>
//         /// <returns>创建的状态实例</returns>
//         public T CreateState<T>(string stateName, UIStateType stateType = UIStateType.Normal, int priority = 0) 
//             where T : UIStateBase, new()
//         {
//             if (string.IsNullOrEmpty(stateName))
//             {
//                 Debug.LogError("[状态工厂] 状态名称不能为空");
//                 return null;
//             }
//             
//             // 检查缓存
//             if (EnableStateCache && stateInstanceCache.TryGetValue(stateName, out IUIState cachedState))
//             {
//                 if (cachedState is T typedState)
//                 {
//                     Debug.Log($"[状态工厂] 从缓存获取状态: {stateName}");
//                     return typedState;
//                 }
//             }
//             
//             try
//             {
//                 // 创建新实例
//                 T state = new T();
//                 
//                 // 设置基本属性
//                 state.StateName = stateName;
//                 state.StateType = stateType;
//                 state.Priority = priority;
//                 
//                 // 初始化状态
//                 InitializeState(state);
//                 
//                 // 添加到缓存
//                 if (EnableStateCache)
//                 {
//                     AddToCache(stateName, state);
//                 }
//                 
//                 Debug.Log($"[状态工厂] 创建状态实例: {stateName} ({typeof(T).Name})");
//                 return state;
//             }
//             catch (Exception ex)
//             {
//                 Debug.LogError($"[状态工厂] 创建状态失败: {stateName}, 错误: {ex.Message}");
//                 return null;
//             }
//         }
//         
//         /// <summary>
//         /// 创建状态实例
//         /// </summary>
//         /// <param name="stateName">状态名称</param>
//         /// <param name="stateType">UI状态类型</param>
//         /// <param name="priority">优先级</param>
//         /// <returns>创建的状态实例</returns>
//         public IUIState CreateState(string stateName, UIStateType stateType = UIStateType.Normal, int priority = 0)
//         {
//             if (string.IsNullOrEmpty(stateName))
//             {
//                 Debug.LogError("[状态工厂] 状态名称不能为空");
//                 return null;
//             }
//             
//             // 检查缓存
//             if (EnableStateCache && stateInstanceCache.TryGetValue(stateName, out IUIState cachedState))
//             {
//                 Debug.Log($"[状态工厂] 从缓存获取状态: {stateName}");
//                 return cachedState;
//             }
//             
//             // 检查是否有注册的类型
//             if (!stateTypeRegistry.TryGetValue(stateName, out Type registeredType))
//             {
//                 Debug.LogWarning($"[状态工厂] 未找到注册的状态类型: {stateName}，使用默认状态类型");
//                 registeredType = typeof(UIStateBase);
//             }
//             
//             try
//             {
//                 // 创建实例
//                 IUIState state = (IUIState)Activator.CreateInstance(registeredType);
//                 
//                 // 设置基本属性
//                 state.StateName = stateName;
//                 state.StateType = stateType;
//                 state.Priority = priority;
//                 
//                 // 初始化状态
//                 InitializeState(state);
//                 
//                 // 添加到缓存
//                 if (EnableStateCache)
//                 {
//                     AddToCache(stateName, state);
//                 }
//                 
//                 Debug.Log($"[状态工厂] 创建状态实例: {stateName} ({registeredType.Name})");
//                 return state;
//             }
//             catch (Exception ex)
//             {
//                 Debug.LogError($"[状态工厂] 创建状态失败: {stateName}, 错误: {ex.Message}");
//                 return null;
//             }
//         }
//         
//         /// <summary>
//         /// 创建带配置的状态实例
//         /// </summary>
//         /// <param name="config">状态配置</param>
//         /// <returns>创建的状态实例</returns>
//         public IUIState CreateStateWithConfig(UIStateConfig config)
//         {
//             if (config == null)
//             {
//                 Debug.LogError("[状态工厂] 状态配置不能为空");
//                 return null;
//             }
//             
//             // 创建状态
//             IUIState state = CreateState(config.StateName, config.StateType, config.Priority);
//             if (state == null)
//                 return null;
//             
//             // 应用配置
//             ApplyStateConfig(state, config);
//             
//             return state;
//         }
//         
//         /// <summary>
//         /// 初始化状态
//         /// </summary>
//         /// <param name="state">状态实例</param>
//         private void InitializeState(IUIState state)
//         {
//             if (state == null)
//                 return;
//             
//             // 设置UI管理器引用
//             if (state is UIStateBase baseState)
//             {
//                 baseState.SetUIManager(uiManager);
//             }
//             
//             // 加载状态配置
//             LoadStateConfig(state);
//         }
//         
//         /// <summary>
//         /// 加载状态配置
//         /// </summary>
//         /// <param name="state">状态实例</param>
//         private void LoadStateConfig(IUIState state)
//         {
//             string stateName = state.StateName;
//             
//             // 检查配置缓存
//             if (stateConfigCache.TryGetValue(stateName, out UIStateConfig config))
//             {
//                 ApplyStateConfig(state, config);
//                 return;
//             }
//             
//             // 尝试从资源加载配置
//             config = LoadStateConfigFromResources(stateName);
//             if (config != null)
//             {
//                 stateConfigCache[stateName] = config;
//                 ApplyStateConfig(state, config);
//             }
//         }
//         
//         /// <summary>
//         /// 从资源加载状态配置
//         /// </summary>
//         /// <param name="stateName">状态名称</param>
//         /// <returns>状态配置</returns>
//         private UIStateConfig LoadStateConfigFromResources(string stateName)
//         {
//             try
//             {
//                 string configPath = $"Configs/States/{stateName}Config";
//                 UIStateConfig config = Resources.Load<UIStateConfig>(configPath);
//                 
//                 if (config != null)
//                 {
//                     Debug.Log($"[状态工厂] 加载状态配置: {stateName}");
//                 }
//                 
//                 return config;
//             }
//             catch (Exception ex)
//             {
//                 Debug.LogWarning($"[状态工厂] 加载状态配置失败: {stateName}, 错误: {ex.Message}");
//                 return null;
//             }
//         }
//         
//         /// <summary>
//         /// 应用状态配置
//         /// </summary>
//         /// <param name="state">状态实例</param>
//         /// <param name="config">状态配置</param>
//         private void ApplyStateConfig(IUIState state, UIStateConfig config)
//         {
//             if (state == null || config == null)
//                 return;
//             
//             // 应用基本配置
//             state.StateType = config.StateType;
//             state.Priority = config.Priority;
//             state.CanBeInterrupted = config.CanBeInterrupted;
//             
//             // 应用扩展配置
//             if (state is UIStateBase baseState)
//             {
//                 baseState.ApplyConfig(config);
//             }
//         }
//         
//         #endregion
//         
//         #region 缓存管理
//         
//         /// <summary>
//         /// 添加到缓存
//         /// </summary>
//         /// <param name="stateName">状态名称</param>
//         /// <param name="state">状态实例</param>
//         private void AddToCache(string stateName, IUIState state)
//         {
//             if (!EnableStateCache || string.IsNullOrEmpty(stateName) || state == null)
//                 return;
//             
//             // 检查缓存大小限制
//             if (stateInstanceCache.Count >= MaxCacheSize)
//             {
//                 // 移除最旧的缓存项
//                 RemoveOldestCacheItem();
//             }
//             
//             stateInstanceCache[stateName] = state;
//         }
//         
//         /// <summary>
//         /// 从缓存移除
//         /// </summary>
//         /// <param name="stateName">状态名称</param>
//         /// <returns>是否移除成功</returns>
//         public bool RemoveFromCache(string stateName)
//         {
//             if (string.IsNullOrEmpty(stateName))
//                 return false;
//             
//             bool removed = stateInstanceCache.Remove(stateName);
//             if (removed)
//             {
//                 Debug.Log($"[状态工厂] 从缓存移除状态: {stateName}");
//             }
//             
//             return removed;
//         }
//         
//         /// <summary>
//         /// 清理缓存
//         /// </summary>
//         public void ClearCache()
//         {
//             stateInstanceCache.Clear();
//             stateConfigCache.Clear();
//             Debug.Log("[状态工厂] 缓存已清理");
//         }
//         
//         /// <summary>
//         /// 移除最旧的缓存项
//         /// </summary>
//         private void RemoveOldestCacheItem()
//         {
//             if (stateInstanceCache.Count == 0)
//                 return;
//             
//             // 简单实现：移除第一个项
//             // 在实际项目中可以使用LRU算法
//             var enumerator = stateInstanceCache.GetEnumerator();
//             if (enumerator.MoveNext())
//             {
//                 string oldestKey = enumerator.Current.Key;
//                 stateInstanceCache.Remove(oldestKey);
//                 Debug.Log($"[状态工厂] 移除最旧缓存项: {oldestKey}");
//             }
//         }
//         
//         /// <summary>
//         /// 获取缓存状态数量
//         /// </summary>
//         /// <returns>缓存状态数量</returns>
//         public int GetCacheSize()
//         {
//             return stateInstanceCache.Count;
//         }
//         
//         /// <summary>
//         /// 获取缓存中的状态名称列表
//         /// </summary>
//         /// <returns>状态名称列表</returns>
//         public List<string> GetCachedStateNames()
//         {
//             return new List<string>(stateInstanceCache.Keys);
//         }
//         
//         #endregion
//         
//         #region 查询方法
//         
//         /// <summary>
//         /// 检查状态是否在缓存中
//         /// </summary>
//         /// <param name="stateName">状态名称</param>
//         /// <returns>是否在缓存中</returns>
//         public bool IsStateInCache(string stateName)
//         {
//             return !string.IsNullOrEmpty(stateName) && stateInstanceCache.ContainsKey(stateName);
//         }
//         
//         /// <summary>
//         /// 从缓存获取状态
//         /// </summary>
//         /// <param name="stateName">状态名称</param>
//         /// <returns>状态实例，如果不存在返回null</returns>
//         public IUIState GetStateFromCache(string stateName)
//         {
//             if (string.IsNullOrEmpty(stateName))
//                 return null;
//             
//             stateInstanceCache.TryGetValue(stateName, out IUIState state);
//             return state;
//         }
//         
//         #endregion
//         
//         #region 调试方法
//         
//         /// <summary>
//         /// 获取工厂信息
//         /// </summary>
//         /// <returns>工厂信息字符串</returns>
//         public string GetFactoryInfo()
//         {
//             var info = "状态工厂信息:\n";
//             info += $"- 启用缓存: {EnableStateCache}\n";
//             info += $"- 最大缓存大小: {MaxCacheSize}\n";
//             info += $"- 当前缓存大小: {stateInstanceCache.Count}\n";
//             info += $"- 注册状态类型数量: {stateTypeRegistry.Count}\n";
//             info += $"- 配置缓存数量: {stateConfigCache.Count}\n";
//             
//             if (stateTypeRegistry.Count > 0)
//             {
//                 info += "\n注册的状态类型:\n";
//                 foreach (var kvp in stateTypeRegistry)
//                 {
//                     info += $"  - {kvp.Key}: {kvp.Value.Name}\n";
//                 }
//             }
//             
//             if (stateInstanceCache.Count > 0)
//             {
//                 info += "\n缓存的状态实例:\n";
//                 foreach (var kvp in stateInstanceCache)
//                 {
//                     info += $"  - {kvp.Key}: {kvp.Value.GetType().Name}\n";
//                 }
//             }
//             
//             return info;
//         }
//         
//         /// <summary>
//         /// 输出工厂调试信息
//         /// </summary>
//         public void LogFactoryInfo()
//         {
//             Debug.Log($"[状态工厂] {GetFactoryInfo()}");
//         }
//         
//         #endregion
//     }
//     
//     /// <summary>
//     /// UI状态配置
//     /// </summary>
//     [CreateAssetMenu(fileName = "UIStateConfig", menuName = "StateMachineUI/State Config")]
//     public class UIStateConfig : ScriptableObject
//     {
//         [Header("基本设置")]
//         public string StateName;
//         public UIStateType StateType = UIStateType.Normal;
//         public int Priority = 0;
//         public bool CanBeInterrupted = true;
//         
//         [Header("UI设置")]
//         public string PrefabPath;
//         public bool UseAnimation = true;
//         public float AnimationDuration = 0.3f;
//         
//         [Header("行为设置")]
//         public bool AutoClose = false;
//         public float AutoCloseDelay = 5f;
//         public bool BlockInput = false;
//         public bool PauseGame = false;
//         
//         [Header("音效设置")]
//         public string EnterSound;
//         public string ExitSound;
//         public float SoundVolume = 1f;
//         
//         [Header("自定义数据")]
//         public string[] CustomData;
//         
//         /// <summary>
//         /// 验证配置
//         /// </summary>
//         /// <returns>是否有效</returns>
//         public bool IsValid()
//         {
//             return !string.IsNullOrEmpty(StateName);
//         }
//         
//         /// <summary>
//         /// 获取自定义数据
//         /// </summary>
//         /// <param name="key">键</param>
//         /// <returns>值</returns>
//         public string GetCustomData(string key)
//         {
//             if (CustomData == null || string.IsNullOrEmpty(key))
//                 return null;
//             
//             foreach (string data in CustomData)
//             {
//                 if (data.StartsWith(key + "="))
//                 {
//                     return data.Substring(key.Length + 1);
//                 }
//             }
//             
//             return null;
//         }
//         
//         /// <summary>
//         /// 设置自定义数据
//         /// </summary>
//         /// <param name="key">键</param>
//         /// <param name="value">值</param>
//         public void SetCustomData(string key, string value)
//         {
//             if (string.IsNullOrEmpty(key))
//                 return;
//             
//             if (CustomData == null)
//             {
//                 CustomData = new string[0];
//             }
//             
//             string dataEntry = key + "=" + (value ?? "");
//             
//             // 查找是否已存在
//             for (int i = 0; i < CustomData.Length; i++)
//             {
//                 if (CustomData[i].StartsWith(key + "="))
//                 {
//                     CustomData[i] = dataEntry;
//                     return;
//                 }
//             }
//             
//             // 添加新项
//             Array.Resize(ref CustomData, CustomData.Length + 1);
//             CustomData[CustomData.Length - 1] = dataEntry;
//         }
//     }
// }