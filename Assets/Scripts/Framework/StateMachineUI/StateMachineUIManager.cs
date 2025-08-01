// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using Framework.StateMachineUI.Core;
// using Framework.StateMachineUI.States;
//
// namespace Framework.StateMachineUI
// {
//     /// <summary>
//     /// 状态机UI管理器
//     /// 整个状态机UI框架的主要入口点和管理中心
//     /// </summary>
//     public class StateMachineUIManager : MonoBehaviour
//     {
//         #region 单例模式
//         
//         private static StateMachineUIManager instance;
//         
//         /// <summary>
//         /// 单例实例
//         /// </summary>
//         public static StateMachineUIManager Instance
//         {
//             get
//             {
//                 if (instance == null)
//                 {
//                     instance = FindObjectOfType<StateMachineUIManager>();
//                     if (instance == null)
//                     {
//                         var go = new GameObject("StateMachineUIManager");
//                         instance = go.AddComponent<StateMachineUIManager>();
//                         DontDestroyOnLoad(go);
//                     }
//                 }
//                 return instance;
//             }
//         }
//         
//         #endregion
//         
//         #region 字段和属性
//         
//         [Header("UI根节点设置")]
//         [SerializeField] private Canvas uiCanvas;
//         [SerializeField] private Transform normalUIRoot;
//         [SerializeField] private Transform overlayUIRoot;
//         [SerializeField] private Transform systemUIRoot;
//         [SerializeField] private Transform topUIRoot;
//         
//         [Header("状态机设置")]
//         [SerializeField] private bool autoInitialize = true;
//         [SerializeField] private bool enableDebugMode = true;
//         [SerializeField] private string defaultStateName = "MainMenu";
//         
//         [Header("资源设置")]
//         [SerializeField] private string uiPrefabPath = "UI/States/";
//         [SerializeField] private bool useAsyncLoading = false;
//         
//         /// <summary>
//         /// 状态机实例
//         /// </summary>
//         public UIStateMachine StateMachine { get; private set; }
//         
//         /// <summary>
//         /// 状态工厂
//         /// </summary>
//         public UIStateFactory StateFactory { get; private set; }
//         
//         /// <summary>
//         /// UI根节点字典
//         /// </summary>
//         private Dictionary<UIStateType, Transform> uiRoots;
//         
//         /// <summary>
//         /// 状态预制体缓存
//         /// </summary>
//         private Dictionary<string, GameObject> statePrefabCache = new Dictionary<string, GameObject>();
//         
//         /// <summary>
//         /// 是否已初始化
//         /// </summary>
//         public bool IsInitialized { get; private set; }
//         
//         #endregion
//         
//         #region 事件
//         
//         /// <summary>
//         /// 管理器初始化完成事件
//         /// </summary>
//         public event Action OnManagerInitialized;
//         
//         /// <summary>
//         /// UI状态改变事件
//         /// </summary>
//         public event Action<string, string> OnUIStateChanged;
//         
//         /// <summary>
//         /// UI错误事件
//         /// </summary>
//         public event Action<string, Exception> OnUIError;
//         
//         #endregion
//         
//         #region Unity生命周期
//         
//         private void Awake()
//         {
//             // 确保单例
//             if (instance == null)
//             {
//                 instance = this;
//                 DontDestroyOnLoad(gameObject);
//                 
//                 if (autoInitialize)
//                 {
//                     Initialize();
//                 }
//             }
//             else if (instance != this)
//             {
//                 Destroy(gameObject);
//             }
//         }
//         
//         private void Start()
//         {
//             if (IsInitialized && !string.IsNullOrEmpty(defaultStateName))
//             {
//                 // 启动默认状态
//                 TransitionToState(defaultStateName);
//             }
//         }
//         
//         private void OnDestroy()
//         {
//             if (instance == this)
//             {
//                 instance = null;
//             }
//         }
//         
//         #endregion
//         
//         #region 初始化
//         
//         /// <summary>
//         /// 初始化管理器
//         /// </summary>
//         public void Initialize()
//         {
//             if (IsInitialized)
//             {
//                 Debug.LogWarning("[状态机UI] 管理器已经初始化");
//                 return;
//             }
//             
//             try
//             {
//                 // 初始化UI根节点
//                 InitializeUIRoots();
//                 
//                 // 创建状态机
//                 StateMachine = gameObject.GetComponent<UIStateMachine>();
//                 if (StateMachine == null)
//                 {
//                     StateMachine = gameObject.AddComponent<UIStateMachine>();
//                 }
//                 
//                 // 创建状态工厂
//                 StateFactory = new UIStateFactory(this);
//                 
//                 // 注册状态机事件
//                 RegisterStateMachineEvents();
//                 
//                 // 注册默认状态
//                 RegisterDefaultStates();
//                 
//                 IsInitialized = true;
//                 
//                 Debug.Log("[状态机UI] 管理器初始化完成");
//                 OnManagerInitialized?.Invoke();
//             }
//             catch (Exception ex)
//             {
//                 Debug.LogError($"[状态机UI] 管理器初始化失败: {ex.Message}");
//                 OnUIError?.Invoke("初始化失败", ex);
//             }
//         }
//         
//         /// <summary>
//         /// 初始化UI根节点
//         /// </summary>
//         private void InitializeUIRoots()
//         {
//             // 如果没有指定Canvas，尝试查找或创建
//             if (uiCanvas == null)
//             {
//                 uiCanvas = FindObjectOfType<Canvas>();
//                 if (uiCanvas == null)
//                 {
//                     var canvasGO = new GameObject("UICanvas");
//                     uiCanvas = canvasGO.AddComponent<Canvas>();
//                     uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
//                     canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
//                     canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
//                 }
//             }
//             
//             // 创建UI根节点
//             CreateUIRoots();
//             
//             // 初始化根节点字典
//             uiRoots = new Dictionary<UIStateType, Transform>
//             {
//                 { UIStateType.Normal, normalUIRoot },
//                 { UIStateType.Overlay, overlayUIRoot },
//                 { UIStateType.System, systemUIRoot },
//                 { UIStateType.Exclusive, normalUIRoot }, // 独占状态使用普通UI根节点
//                 { UIStateType.Temporary, topUIRoot }
//             };
//         }
//         
//         /// <summary>
//         /// 创建UI根节点
//         /// </summary>
//         private void CreateUIRoots()
//         {
//             if (normalUIRoot == null)
//             {
//                 normalUIRoot = CreateUIRoot("NormalUI", 0);
//             }
//             
//             if (overlayUIRoot == null)
//             {
//                 overlayUIRoot = CreateUIRoot("OverlayUI", 100);
//             }
//             
//             if (systemUIRoot == null)
//             {
//                 systemUIRoot = CreateUIRoot("SystemUI", 200);
//             }
//             
//             if (topUIRoot == null)
//             {
//                 topUIRoot = CreateUIRoot("TopUI", 300);
//             }
//         }
//         
//         /// <summary>
//         /// 创建UI根节点
//         /// </summary>
//         /// <param name="name">节点名称</param>
//         /// <param name="sortingOrder">排序顺序</param>
//         /// <returns>创建的根节点</returns>
//         private Transform CreateUIRoot(string name, int sortingOrder)
//         {
//             var rootGO = new GameObject(name);
//             rootGO.transform.SetParent(uiCanvas.transform, false);
//             
//             var rectTransform = rootGO.AddComponent<RectTransform>();
//             rectTransform.anchorMin = Vector2.zero;
//             rectTransform.anchorMax = Vector2.one;
//             rectTransform.sizeDelta = Vector2.zero;
//             rectTransform.anchoredPosition = Vector2.zero;
//             
//             var canvas = rootGO.AddComponent<Canvas>();
//             canvas.overrideSorting = true;
//             canvas.sortingOrder = sortingOrder;
//             
//             rootGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
//             
//             return rootGO.transform;
//         }
//         
//         /// <summary>
//         /// 注册状态机事件
//         /// </summary>
//         private void RegisterStateMachineEvents()
//         {
//             StateMachine.OnStateChanged += (newState, oldState) =>
//             {
//                 string newStateName = newState?.StateName ?? "None";
//                 string oldStateName = oldState?.StateName ?? "None";
//                 
//                 if (enableDebugMode)
//                     Debug.Log($"[状态机UI] 状态改变: {oldStateName} -> {newStateName}");
//                 
//                 OnUIStateChanged?.Invoke(newStateName, oldStateName);
//             };
//             
//             StateMachine.OnStateAdded += (state) =>
//             {
//                 if (enableDebugMode)
//                     Debug.Log($"[状态机UI] 状态添加: {state.StateName}");
//             };
//             
//             StateMachine.OnStateRemoved += (state) =>
//             {
//                 if (enableDebugMode)
//                     Debug.Log($"[状态机UI] 状态移除: {state.StateName}");
//             };
//         }
//         
//         /// <summary>
//         /// 注册默认状态
//         /// </summary>
//         private void RegisterDefaultStates()
//         {
//             // 这里可以注册一些默认的状态
//             // 具体的状态注册将在后续的状态类中实现
//         }
//         
//         #endregion
//         
//         #region 状态管理
//         
//         /// <summary>
//         /// 转换到指定状态
//         /// </summary>
//         /// <param name="stateName">状态名称</param>
//         /// <param name="data">传递的数据</param>
//         /// <param name="forceTransition">是否强制转换</param>
//         /// <returns>是否转换成功</returns>
//         public bool TransitionToState(string stateName, object data = null, bool forceTransition = false)
//         {
//             if (!IsInitialized)
//             {
//                 Debug.LogError("[状态机UI] 管理器未初始化");
//                 return false;
//             }
//             
//             try
//             {
//                 return StateMachine.TransitionToState(stateName, data, forceTransition);
//             }
//             catch (Exception ex)
//             {
//                 Debug.LogError($"[状态机UI] 状态转换失败: {ex.Message}");
//                 OnUIError?.Invoke($"状态转换失败: {stateName}", ex);
//                 return false;
//             }
//         }
//         
//         /// <summary>
//         /// 注册状态
//         /// </summary>
//         /// <param name="state">状态实例</param>
//         public void RegisterState(IUIState state)
//         {
//             if (!IsInitialized)
//             {
//                 Debug.LogError("[状态机UI] 管理器未初始化");
//                 return;
//             }
//             
//             StateMachine.RegisterState(state);
//         }
//         
//         /// <summary>
//         /// 创建并注册状态
//         /// </summary>
//         /// <typeparam name="T">状态类型</typeparam>
//         /// <param name="stateName">状态名称</param>
//         /// <param name="stateType">状态类型</param>
//         /// <param name="priority">优先级</param>
//         /// <returns>创建的状态实例</returns>
//         public T CreateAndRegisterState<T>(string stateName, UIStateType stateType = UIStateType.Normal, int priority = 0) 
//             where T : UIStateBase, new()
//         {
//             var state = StateFactory.CreateState<T>(stateName, stateType, priority);
//             RegisterState(state);
//             return state;
//         }
//         
//         /// <summary>
//         /// 移除状态
//         /// </summary>
//         /// <param name="stateName">状态名称</param>
//         /// <returns>是否移除成功</returns>
//         public bool RemoveState(string stateName)
//         {
//             if (!IsInitialized)
//             {
//                 Debug.LogError("[状态机UI] 管理器未初始化");
//                 return false;
//             }
//             
//             return StateMachine.RemoveState(stateName);
//         }
//         
//         /// <summary>
//         /// 返回上一个状态
//         /// </summary>
//         /// <returns>是否返回成功</returns>
//         public bool GoBack()
//         {
//             if (!IsInitialized)
//             {
//                 Debug.LogError("[状态机UI] 管理器未初始化");
//                 return false;
//             }
//             
//             return StateMachine.GoBack();
//         }
//         
//         #endregion
//         
//         #region UI根节点管理
//         
//         /// <summary>
//         /// 获取指定类型的UI根节点
//         /// </summary>
//         /// <param name="stateType">状态类型</param>
//         /// <returns>UI根节点</returns>
//         public Transform GetUIRoot(UIStateType stateType)
//         {
//             if (uiRoots != null && uiRoots.TryGetValue(stateType, out Transform root))
//             {
//                 return root;
//             }
//             
//             Debug.LogWarning($"[状态机UI] 未找到状态类型 {stateType} 对应的UI根节点");
//             return normalUIRoot; // 默认返回普通UI根节点
//         }
//         
//         /// <summary>
//         /// 设置UI根节点
//         /// </summary>
//         /// <param name="stateType">状态类型</param>
//         /// <param name="root">根节点</param>
//         public void SetUIRoot(UIStateType stateType, Transform root)
//         {
//             if (uiRoots == null)
//             {
//                 uiRoots = new Dictionary<UIStateType, Transform>();
//             }
//             
//             uiRoots[stateType] = root;
//         }
//         
//         #endregion
//         
//         #region 资源管理
//         
//         /// <summary>
//         /// 加载状态预制体
//         /// </summary>
//         /// <param name="stateName">状态名称</param>
//         /// <returns>预制体GameObject</returns>
//         public GameObject LoadStatePrefab(string stateName)
//         {
//             // 检查缓存
//             if (statePrefabCache.TryGetValue(stateName, out GameObject cachedPrefab))
//             {
//                 return cachedPrefab;
//             }
//             
//             // 从Resources加载
//             string prefabPath = uiPrefabPath + stateName;
//             GameObject prefab = Resources.Load<GameObject>(prefabPath);
//             
//             if (prefab != null)
//             {
//                 statePrefabCache[stateName] = prefab;
//                 if (enableDebugMode)
//                     Debug.Log($"[状态机UI] 加载状态预制体: {stateName}");
//             }
//             else
//             {
//                 Debug.LogWarning($"[状态机UI] 未找到状态预制体: {prefabPath}");
//             }
//             
//             return prefab;
//         }
//         
//         /// <summary>
//         /// 实例化状态UI
//         /// </summary>
//         /// <param name="stateName">状态名称</param>
//         /// <param name="parent">父节点</param>
//         /// <returns>实例化的GameObject</returns>
//         public GameObject InstantiateStateUI(string stateName, Transform parent = null)
//         {
//             GameObject prefab = LoadStatePrefab(stateName);
//             if (prefab == null)
//                 return null;
//             
//             GameObject instance = Instantiate(prefab, parent);
//             instance.name = stateName + "UI";
//             
//             return instance;
//         }
//         
//         /// <summary>
//         /// 清理预制体缓存
//         /// </summary>
//         public void ClearPrefabCache()
//         {
//             statePrefabCache.Clear();
//             if (enableDebugMode)
//                 Debug.Log("[状态机UI] 预制体缓存已清理");
//         }
//         
//         #endregion
//         
//         #region 查询方法
//         
//         /// <summary>
//         /// 检查状态是否活跃
//         /// </summary>
//         /// <param name="stateName">状态名称</param>
//         /// <returns>是否活跃</returns>
//         public bool IsStateActive(string stateName)
//         {
//             return IsInitialized && StateMachine.IsStateActive(stateName);
//         }
//         
//         /// <summary>
//         /// 获取当前主状态
//         /// </summary>
//         /// <returns>当前主状态</returns>
//         public IUIState GetCurrentMainState()
//         {
//             return IsInitialized ? StateMachine.CurrentMainState : null;
//         }
//         
//         /// <summary>
//         /// 获取当前主状态名称
//         /// </summary>
//         /// <returns>当前主状态名称</returns>
//         public string GetCurrentMainStateName()
//         {
//             var mainState = GetCurrentMainState();
//             return mainState?.StateName ?? "None";
//         }
//         
//         /// <summary>
//         /// 获取活跃状态数量
//         /// </summary>
//         /// <returns>活跃状态数量</returns>
//         public int GetActiveStateCount()
//         {
//             return IsInitialized ? StateMachine.ActiveStateCount : 0;
//         }
//         
//         /// <summary>
//         /// 获取所有活跃状态名称
//         /// </summary>
//         /// <returns>活跃状态名称列表</returns>
//         public List<string> GetActiveStateNames()
//         {
//             if (!IsInitialized)
//                 return new List<string>();
//             
//             var activeStates = StateMachine.GetActiveStates();
//             var stateNames = new List<string>();
//             
//             foreach (var state in activeStates)
//             {
//                 stateNames.Add(state.StateName);
//             }
//             
//             return stateNames;
//         }
//         
//         #endregion
//         
//         #region 配置方法
//         
//         /// <summary>
//         /// 设置调试模式
//         /// </summary>
//         /// <param name="enabled">是否启用</param>
//         public void SetDebugMode(bool enabled)
//         {
//             enableDebugMode = enabled;
//         }
//         
//         /// <summary>
//         /// 设置UI预制体路径
//         /// </summary>
//         /// <param name="path">预制体路径</param>
//         public void SetUIPrefabPath(string path)
//         {
//             uiPrefabPath = path;
//         }
//         
//         /// <summary>
//         /// 设置默认状态
//         /// </summary>
//         /// <param name="stateName">默认状态名称</param>
//         public void SetDefaultState(string stateName)
//         {
//             defaultStateName = stateName;
//         }
//         
//         #endregion
//         
//         #region 调试方法
//         
//         /// <summary>
//         /// 获取管理器信息
//         /// </summary>
//         /// <returns>管理器信息字符串</returns>
//         public string GetManagerInfo()
//         {
//             if (!IsInitialized)
//                 return "管理器未初始化";
//             
//             var info = $"状态机UI管理器信息:\n";
//             info += $"- 是否初始化: {IsInitialized}\n";
//             info += $"- 调试模式: {enableDebugMode}\n";
//             info += $"- 默认状态: {defaultStateName}\n";
//             info += $"- UI预制体路径: {uiPrefabPath}\n";
//             info += $"- 预制体缓存数量: {statePrefabCache.Count}\n";
//             info += $"- 当前主状态: {GetCurrentMainStateName()}\n";
//             info += $"- 活跃状态数量: {GetActiveStateCount()}\n";
//             
//             if (StateMachine != null)
//             {
//                 info += "\n" + StateMachine.GetStateMachineInfo();
//             }
//             
//             return info;
//         }
//         
//         /// <summary>
//         /// 输出管理器调试信息
//         /// </summary>
//         public void LogManagerInfo()
//         {
//             Debug.Log($"[状态机UI] {GetManagerInfo()}");
//         }
//         
//         /// <summary>
//         /// 重置管理器
//         /// </summary>
//         public void ResetManager()
//         {
//             if (IsInitialized && StateMachine != null)
//             {
//                 StateMachine.Reset();
//                 ClearPrefabCache();
//                 
//                 Debug.Log("[状态机UI] 管理器已重置");
//             }
//         }
//         
//         #endregion
//     }
// }