// using UnityEngine;
// using Framework.StateMachineUI.Core;
// using Framework.StateMachineUI.States;
//
// namespace Framework.StateMachineUI.Examples
// {
//     /// <summary>
//     /// 状态机UI框架使用示例
//     /// 演示如何初始化和使用状态机UI系统
//     /// </summary>
//     public class StateMachineUIExample : MonoBehaviour
//     {
//         [Header("UI根节点")]
//         [SerializeField] private Transform normalRoot;
//         [SerializeField] private Transform overlayRoot;
//         [SerializeField] private Transform systemRoot;
//         [SerializeField] private Transform topRoot;
//         
//         [Header("设置")]
//         [SerializeField] private bool autoStart = true;
//         [SerializeField] private bool enableDebugMode = true;
//         [SerializeField] private string defaultStateName = "MainMenu";
//         
//         private StateMachineUIManager uiManager;
//         
//         void Start()
//         {
//             if (autoStart)
//             {
//                 InitializeStateMachineUI();
//             }
//         }
//         
//         /// <summary>
//         /// 初始化状态机UI系统
//         /// </summary>
//         public void InitializeStateMachineUI()
//         {
//             Debug.Log("[示例] 初始化状态机UI系统");
//             
//             // 获取UI管理器实例
//             uiManager = StateMachineUIManager.Instance;
//             
//             // 设置UI根节点
//             SetupUIRoots();
//             
//             // 注册状态
//             RegisterStates();
//             
//             // 设置调试模式
//             uiManager.SetDebugMode(enableDebugMode);
//             
//             // 设置默认状态
//             if (!string.IsNullOrEmpty(defaultStateName))
//             {
//                 uiManager.SetDefaultState(defaultStateName);
//             }
//             
//             // 初始化管理器
//             uiManager.Initialize();
//             
//             // 监听事件
//             SubscribeToEvents();
//             
//             Debug.Log("[示例] 状态机UI系统初始化完成");
//         }
//         
//         /// <summary>
//         /// 设置UI根节点
//         /// </summary>
//         private void SetupUIRoots()
//         {
//             // 如果没有指定根节点，自动创建
//             if (normalRoot == null)
//                 normalRoot = CreateUIRoot("NormalRoot", 0);
//             
//             if (overlayRoot == null)
//                 overlayRoot = CreateUIRoot("OverlayRoot", 10);
//             
//             if (systemRoot == null)
//                 systemRoot = CreateUIRoot("SystemRoot", 20);
//             
//             if (topRoot == null)
//                 topRoot = CreateUIRoot("TopRoot", 30);
//             
//             // 设置到UI管理器
//             uiManager.SetUIRoot(UIStateType.Normal, normalRoot);
//             uiManager.SetUIRoot(UIStateType.Overlay, overlayRoot);
//             uiManager.SetUIRoot(UIStateType.System, systemRoot);
//             uiManager.SetUIRoot(UIStateType.Exclusive, topRoot);
//             uiManager.SetUIRoot(UIStateType.Temporary, topRoot);
//         }
//         
//         /// <summary>
//         /// 创建UI根节点
//         /// </summary>
//         private Transform CreateUIRoot(string name, int sortingOrder)
//         {
//             GameObject rootObj = new GameObject(name);
//             rootObj.transform.SetParent(transform);
//             
//             Canvas canvas = rootObj.AddComponent<Canvas>();
//             canvas.overrideSorting = true;
//             canvas.sortingOrder = sortingOrder;
//             
//             CanvasGroup canvasGroup = rootObj.AddComponent<CanvasGroup>();
//             
//             RectTransform rectTransform = rootObj.GetComponent<RectTransform>();
//             rectTransform.anchorMin = Vector2.zero;
//             rectTransform.anchorMax = Vector2.one;
//             rectTransform.offsetMin = Vector2.zero;
//             rectTransform.offsetMax = Vector2.zero;
//             
//             return rootObj.transform;
//         }
//         
//         /// <summary>
//         /// 注册状态
//         /// </summary>
//         private void RegisterStates()
//         {
//             Debug.Log("[示例] 注册UI状态");
//             
//             // 注册主菜单状态
//             uiManager.RegisterState<MainMenuState>("MainMenu");
//             
//             // 注册游戏状态
//             uiManager.RegisterState<GamePlayState>("GamePlay");
//             
//             // 注册设置状态
//             uiManager.RegisterState<SettingsState>("Settings");
//             
//             // 注册暂停状态
//             uiManager.RegisterState<PauseState>("Pause");
//             
//             // 注册加载状态
//             uiManager.RegisterState<LoadingState>("Loading");
//             
//             // 可以注册自定义状态
//             // uiManager.RegisterState<CustomState>("Custom");
//         }
//         
//         /// <summary>
//         /// 订阅事件
//         /// </summary>
//         private void SubscribeToEvents()
//         {
//             uiManager.OnInitialized += OnUIManagerInitialized;
//             uiManager.OnUIStateChanged += OnUIStateChanged;
//             uiManager.OnUIError += OnUIError;
//         }
//         
//         /// <summary>
//         /// UI管理器初始化完成事件
//         /// </summary>
//         private void OnUIManagerInitialized()
//         {
//             Debug.Log("[示例] UI管理器初始化完成");
//             
//             // 可以在这里执行初始化后的操作
//             // 比如显示启动画面、加载初始数据等
//             
//             // 示例：显示加载状态然后转到主菜单
//             ShowLoadingAndTransitionToMainMenu();
//         }
//         
//         /// <summary>
//         /// UI状态改变事件
//         /// </summary>
//         private void OnUIStateChanged(string fromState, string toState, object data)
//         {
//             Debug.Log($"[示例] UI状态改变: {fromState} -> {toState}");
//             
//             // 可以在这里处理状态改变的逻辑
//             // 比如播放转场音效、保存状态等
//         }
//         
//         /// <summary>
//         /// UI错误事件
//         /// </summary>
//         private void OnUIError(string error)
//         {
//             Debug.LogError($"[示例] UI错误: {error}");
//             
//             // 可以在这里处理错误
//             // 比如显示错误提示、回退到安全状态等
//         }
//         
//         /// <summary>
//         /// 显示加载状态然后转到主菜单
//         /// </summary>
//         private void ShowLoadingAndTransitionToMainMenu()
//         {
//             var loadingData = new LoadingData
//             {
//                 TargetStateName = "MainMenu",
//                 TargetStateData = null
//             };
//             
//             uiManager.TransitionToState("Loading", loadingData);
//         }
//         
//         #region 公共方法 - 供外部调用
//         
//         /// <summary>
//         /// 开始游戏
//         /// </summary>
//         public void StartGame()
//         {
//             Debug.Log("[示例] 开始游戏");
//             
//             var gameData = new GameData
//             {
//                 Level = 1,
//                 Score = 0,
//                 Lives = 3
//             };
//             
//             var loadingData = new LoadingData
//             {
//                 TargetStateName = "GamePlay",
//                 TargetStateData = gameData
//             };
//             
//             uiManager.TransitionToState("Loading", loadingData);
//         }
//         
//         /// <summary>
//         /// 显示设置
//         /// </summary>
//         public void ShowSettings()
//         {
//             Debug.Log("[示例] 显示设置");
//             uiManager.TransitionToState("Settings");
//         }
//         
//         /// <summary>
//         /// 暂停游戏
//         /// </summary>
//         public void PauseGame()
//         {
//             Debug.Log("[示例] 暂停游戏");
//             
//             var pauseData = new PauseData
//             {
//                 PreviousStateName = "GamePlay",
//                 PreviousStateData = null
//             };
//             
//             uiManager.TransitionToState("Pause", pauseData);
//         }
//         
//         /// <summary>
//         /// 返回主菜单
//         /// </summary>
//         public void ReturnToMainMenu()
//         {
//             Debug.Log("[示例] 返回主菜单");
//             uiManager.TransitionToState("MainMenu");
//         }
//         
//         /// <summary>
//         /// 返回上一个状态
//         /// </summary>
//         public void GoBack()
//         {
//             Debug.Log("[示例] 返回上一个状态");
//             uiManager.GoBack();
//         }
//         
//         /// <summary>
//         /// 获取当前状态信息
//         /// </summary>
//         public void PrintCurrentStateInfo()
//         {
//             var stateMachine = uiManager.GetStateMachine();
//             if (stateMachine != null)
//             {
//                 Debug.Log($"[示例] 当前活跃状态数量: {stateMachine.GetActiveStateCount()}");
//                 Debug.Log($"[示例] 状态历史数量: {stateMachine.GetStateHistoryCount()}");
//                 Debug.Log($"[示例] 注册状态数量: {stateMachine.GetRegisteredStateCount()}");
//                 
//                 var activeStates = stateMachine.GetActiveStates();
//                 foreach (var state in activeStates)
//                 {
//                     Debug.Log($"[示例] 活跃状态: {state.StateName} (类型: {state.StateType}, 优先级: {state.Priority})");
//                 }
//             }
//         }
//         
//         /// <summary>
//         /// 重置UI系统
//         /// </summary>
//         public void ResetUISystem()
//         {
//             Debug.Log("[示例] 重置UI系统");
//             uiManager.Reset();
//         }
//         
//         #endregion
//         
//         #region Unity事件
//         
//         void Update()
//         {
//             // 处理输入
//             HandleInput();
//         }
//         
//         /// <summary>
//         /// 处理输入
//         /// </summary>
//         private void HandleInput()
//         {
//             // ESC键返回
//             if (Input.GetKeyDown(KeyCode.Escape))
//             {
//                 GoBack();
//             }
//             
//             // F1显示调试信息
//             if (Input.GetKeyDown(KeyCode.F1))
//             {
//                 PrintCurrentStateInfo();
//             }
//             
//             // F5重置系统
//             if (Input.GetKeyDown(KeyCode.F5))
//             {
//                 ResetUISystem();
//             }
//             
//             // 数字键快速切换状态（用于测试）
//             if (Input.GetKeyDown(KeyCode.Alpha1))
//             {
//                 ReturnToMainMenu();
//             }
//             else if (Input.GetKeyDown(KeyCode.Alpha2))
//             {
//                 StartGame();
//             }
//             else if (Input.GetKeyDown(KeyCode.Alpha3))
//             {
//                 ShowSettings();
//             }
//             else if (Input.GetKeyDown(KeyCode.Alpha4))
//             {
//                 PauseGame();
//             }
//         }
//         
//         void OnDestroy()
//         {
//             // 取消事件订阅
//             if (uiManager != null)
//             {
//                 uiManager.OnInitialized -= OnUIManagerInitialized;
//                 uiManager.OnUIStateChanged -= OnUIStateChanged;
//                 uiManager.OnUIError -= OnUIError;
//             }
//         }
//         
//         #endregion
//         
//         #region 调试方法
//         
//         [ContextMenu("初始化UI系统")]
//         public void DebugInitializeUI()
//         {
//             InitializeStateMachineUI();
//         }
//         
//         [ContextMenu("开始游戏")]
//         public void DebugStartGame()
//         {
//             StartGame();
//         }
//         
//         [ContextMenu("显示设置")]
//         public void DebugShowSettings()
//         {
//             ShowSettings();
//         }
//         
//         [ContextMenu("暂停游戏")]
//         public void DebugPauseGame()
//         {
//             PauseGame();
//         }
//         
//         [ContextMenu("返回主菜单")]
//         public void DebugReturnToMainMenu()
//         {
//             ReturnToMainMenu();
//         }
//         
//         [ContextMenu("打印状态信息")]
//         public void DebugPrintStateInfo()
//         {
//             PrintCurrentStateInfo();
//         }
//         
//         [ContextMenu("重置UI系统")]
//         public void DebugResetUISystem()
//         {
//             ResetUISystem();
//         }
//         
//         #endregion
//     }
// }