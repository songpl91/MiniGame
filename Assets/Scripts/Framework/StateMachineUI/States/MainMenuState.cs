// using UnityEngine;
// using UnityEngine.UI;
// using Framework.StateMachineUI.Core;
//
// namespace Framework.StateMachineUI.States
// {
//     /// <summary>
//     /// 主菜单状态
//     /// 游戏的主菜单界面状态
//     /// </summary>
//     public class MainMenuState : UIStateBase
//     {
//         #region UI组件引用
//         
//         [Header("主菜单UI组件")]
//         [SerializeField] private Button startGameButton;
//         [SerializeField] private Button settingsButton;
//         [SerializeField] private Button achievementsButton;
//         [SerializeField] private Button shopButton;
//         [SerializeField] private Button exitButton;
//         
//         [Header("信息显示")]
//         [SerializeField] private Text titleText;
//         [SerializeField] private Text versionText;
//         [SerializeField] private Text playerNameText;
//         [SerializeField] private Text playerLevelText;
//         
//         [Header("背景和装饰")]
//         [SerializeField] private Image backgroundImage;
//         [SerializeField] private ParticleSystem backgroundParticles;
//         [SerializeField] private Animator logoAnimator;
//         
//         #endregion
//         
//         #region 私有变量
//         
//         private bool isInitialized = false;
//         private float idleTime = 0f;
//         private const float IDLE_TIMEOUT = 30f; // 30秒无操作显示演示
//         
//         #endregion
//         
//         #region 构造函数
//         
//         public MainMenuState()
//         {
//             StateName = "MainMenu";
//             StateType = UIStateType.Normal;
//             Priority = 0;
//             CanBeInterrupted = true;
//         }
//         
//         #endregion
//         
//         #region 状态生命周期
//         
//         public override void OnEnter(object data = null)
//         {
//             base.OnEnter(data);
//             
//             Debug.Log("[主菜单状态] 进入主菜单");
//             
//             // 创建或获取UI
//             CreateUI();
//             
//             // 初始化UI
//             InitializeUI();
//             
//             // 播放进入动画
//             PlayEnterAnimation();
//             
//             // 播放背景音乐
//             PlayBackgroundMusic();
//             
//             // 重置空闲时间
//             idleTime = 0f;
//         }
//         
//         public override void OnUpdate(float deltaTime)
//         {
//             base.OnUpdate(deltaTime);
//             
//             // 更新空闲时间
//             idleTime += deltaTime;
//             
//             // 检查空闲超时
//             if (idleTime >= IDLE_TIMEOUT)
//             {
//                 ShowIdleDemo();
//                 idleTime = 0f;
//             }
//             
//             // 更新UI动画
//             UpdateUIAnimations(deltaTime);
//         }
//         
//         public override void OnExit()
//         {
//             Debug.Log("[主菜单状态] 退出主菜单");
//             
//             // 播放退出动画
//             PlayExitAnimation();
//             
//             // 停止背景音乐
//             StopBackgroundMusic();
//             
//             // 清理UI
//             CleanupUI();
//             
//             base.OnExit();
//         }
//         
//         public override void OnPause()
//         {
//             base.OnPause();
//             
//             Debug.Log("[主菜单状态] 暂停主菜单");
//             
//             // 暂停背景动画
//             if (backgroundParticles != null)
//                 backgroundParticles.Pause();
//             
//             if (logoAnimator != null)
//                 logoAnimator.speed = 0f;
//         }
//         
//         public override void OnResume()
//         {
//             base.OnResume();
//             
//             Debug.Log("[主菜单状态] 恢复主菜单");
//             
//             // 恢复背景动画
//             if (backgroundParticles != null)
//                 backgroundParticles.Play();
//             
//             if (logoAnimator != null)
//                 logoAnimator.speed = 1f;
//             
//             // 重置空闲时间
//             idleTime = 0f;
//         }
//         
//         #endregion
//         
//         #region UI创建和初始化
//         
//         /// <summary>
//         /// 创建UI
//         /// </summary>
//         private void CreateUI()
//         {
//             if (uiGameObject != null)
//                 return;
//             
//             // 从UI管理器加载预制体
//             if (uiManager != null)
//             {
//                 Transform uiRoot = uiManager.GetUIRoot(StateType);
//                 uiGameObject = uiManager.InstantiateStateUI(StateName, uiRoot);
//                 
//                 if (uiGameObject != null)
//                 {
//                     // 获取UI组件引用
//                     GetUIComponents();
//                 }
//                 else
//                 {
//                     Debug.LogWarning($"[主菜单状态] 无法加载UI预制体: {StateName}");
//                     CreateDefaultUI();
//                 }
//             }
//             else
//             {
//                 CreateDefaultUI();
//             }
//         }
//         
//         /// <summary>
//         /// 获取UI组件引用
//         /// </summary>
//         private void GetUIComponents()
//         {
//             if (uiGameObject == null)
//                 return;
//             
//             // 获取按钮组件
//             startGameButton = FindUIComponent<Button>("StartGameButton");
//             settingsButton = FindUIComponent<Button>("SettingsButton");
//             achievementsButton = FindUIComponent<Button>("AchievementsButton");
//             shopButton = FindUIComponent<Button>("ShopButton");
//             exitButton = FindUIComponent<Button>("ExitButton");
//             
//             // 获取文本组件
//             titleText = FindUIComponent<Text>("TitleText");
//             versionText = FindUIComponent<Text>("VersionText");
//             playerNameText = FindUIComponent<Text>("PlayerNameText");
//             playerLevelText = FindUIComponent<Text>("PlayerLevelText");
//             
//             // 获取其他组件
//             backgroundImage = FindUIComponent<Image>("BackgroundImage");
//             backgroundParticles = FindUIComponent<ParticleSystem>("BackgroundParticles");
//             logoAnimator = FindUIComponent<Animator>("LogoAnimator");
//         }
//         
//         /// <summary>
//         /// 创建默认UI
//         /// </summary>
//         private void CreateDefaultUI()
//         {
//             Debug.Log("[主菜单状态] 创建默认UI");
//             
//             // 创建简单的UI结构
//             var canvasGO = new GameObject("MainMenuCanvas");
//             var canvas = canvasGO.AddComponent<Canvas>();
//             canvas.renderMode = RenderMode.ScreenSpaceOverlay;
//             
//             var rectTransform = canvasGO.GetComponent<RectTransform>();
//             rectTransform.anchorMin = Vector2.zero;
//             rectTransform.anchorMax = Vector2.one;
//             rectTransform.sizeDelta = Vector2.zero;
//             rectTransform.anchoredPosition = Vector2.zero;
//             
//             uiGameObject = canvasGO;
//             
//             // 创建基本按钮
//             CreateDefaultButtons();
//         }
//         
//         /// <summary>
//         /// 创建默认按钮
//         /// </summary>
//         private void CreateDefaultButtons()
//         {
//             if (uiGameObject == null)
//                 return;
//             
//             // 创建开始游戏按钮
//             startGameButton = CreateButton("开始游戏", new Vector2(0, 100));
//             settingsButton = CreateButton("设置", new Vector2(0, 50));
//             achievementsButton = CreateButton("成就", new Vector2(0, 0));
//             shopButton = CreateButton("商店", new Vector2(0, -50));
//             exitButton = CreateButton("退出", new Vector2(0, -100));
//         }
//         
//         /// <summary>
//         /// 创建按钮
//         /// </summary>
//         /// <param name="text">按钮文本</param>
//         /// <param name="position">位置</param>
//         /// <returns>创建的按钮</returns>
//         private Button CreateButton(string text, Vector2 position)
//         {
//             var buttonGO = new GameObject(text + "Button");
//             buttonGO.transform.SetParent(uiGameObject.transform, false);
//             
//             var rectTransform = buttonGO.AddComponent<RectTransform>();
//             rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
//             rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
//             rectTransform.sizeDelta = new Vector2(200, 50);
//             rectTransform.anchoredPosition = position;
//             
//             var image = buttonGO.AddComponent<Image>();
//             image.color = Color.white;
//             
//             var button = buttonGO.AddComponent<Button>();
//             
//             // 创建文本
//             var textGO = new GameObject("Text");
//             textGO.transform.SetParent(buttonGO.transform, false);
//             
//             var textRect = textGO.AddComponent<RectTransform>();
//             textRect.anchorMin = Vector2.zero;
//             textRect.anchorMax = Vector2.one;
//             textRect.sizeDelta = Vector2.zero;
//             textRect.anchoredPosition = Vector2.zero;
//             
//             var textComponent = textGO.AddComponent<Text>();
//             textComponent.text = text;
//             textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//             textComponent.fontSize = 16;
//             textComponent.color = Color.black;
//             textComponent.alignment = TextAnchor.MiddleCenter;
//             
//             return button;
//         }
//         
//         /// <summary>
//         /// 初始化UI
//         /// </summary>
//         private void InitializeUI()
//         {
//             if (isInitialized)
//                 return;
//             
//             // 设置按钮事件
//             SetupButtonEvents();
//             
//             // 更新UI显示
//             UpdateUIDisplay();
//             
//             // 设置初始状态
//             SetInitialUIState();
//             
//             isInitialized = true;
//             Debug.Log("[主菜单状态] UI初始化完成");
//         }
//         
//         /// <summary>
//         /// 设置按钮事件
//         /// </summary>
//         private void SetupButtonEvents()
//         {
//             if (startGameButton != null)
//                 startGameButton.onClick.AddListener(OnStartGameClick);
//             
//             if (settingsButton != null)
//                 settingsButton.onClick.AddListener(OnSettingsClick);
//             
//             if (achievementsButton != null)
//                 achievementsButton.onClick.AddListener(OnAchievementsClick);
//             
//             if (shopButton != null)
//                 shopButton.onClick.AddListener(OnShopClick);
//             
//             if (exitButton != null)
//                 exitButton.onClick.AddListener(OnExitClick);
//         }
//         
//         /// <summary>
//         /// 更新UI显示
//         /// </summary>
//         private void UpdateUIDisplay()
//         {
//             // 更新标题
//             if (titleText != null)
//                 titleText.text = "迷你游戏框架";
//             
//             // 更新版本信息
//             if (versionText != null)
//                 versionText.text = $"版本 {Application.version}";
//             
//             // 更新玩家信息
//             UpdatePlayerInfo();
//         }
//         
//         /// <summary>
//         /// 更新玩家信息
//         /// </summary>
//         private void UpdatePlayerInfo()
//         {
//             // 这里可以从游戏数据管理器获取玩家信息
//             if (playerNameText != null)
//                 playerNameText.text = "玩家: " + (PlayerPrefs.GetString("PlayerName", "新玩家"));
//             
//             if (playerLevelText != null)
//                 playerLevelText.text = "等级: " + PlayerPrefs.GetInt("PlayerLevel", 1).ToString();
//         }
//         
//         /// <summary>
//         /// 设置初始UI状态
//         /// </summary>
//         private void SetInitialUIState()
//         {
//             // 设置按钮初始状态
//             SetButtonsInteractable(true);
//             
//             // 启动背景效果
//             if (backgroundParticles != null)
//                 backgroundParticles.Play();
//             
//             if (logoAnimator != null)
//                 logoAnimator.SetTrigger("StartIdle");
//         }
//         
//         #endregion
//         
//         #region 动画处理
//         
//         /// <summary>
//         /// 播放进入动画
//         /// </summary>
//         private void PlayEnterAnimation()
//         {
//             if (uiGameObject == null)
//                 return;
//             
//             // 淡入动画
//             var canvasGroup = uiGameObject.GetComponent<CanvasGroup>();
//             if (canvasGroup == null)
//                 canvasGroup = uiGameObject.AddComponent<CanvasGroup>();
//             
//             canvasGroup.alpha = 0f;
//             canvasGroup.interactable = false;
//             
//             // 使用LeanTween或DOTween进行动画，这里用简单的协程实现
//             StartCoroutine(FadeInCoroutine(canvasGroup));
//         }
//         
//         /// <summary>
//         /// 播放退出动画
//         /// </summary>
//         private void PlayExitAnimation()
//         {
//             if (uiGameObject == null)
//                 return;
//             
//             var canvasGroup = uiGameObject.GetComponent<CanvasGroup>();
//             if (canvasGroup != null)
//             {
//                 StartCoroutine(FadeOutCoroutine(canvasGroup));
//             }
//         }
//         
//         /// <summary>
//         /// 淡入协程
//         /// </summary>
//         private System.Collections.IEnumerator FadeInCoroutine(CanvasGroup canvasGroup)
//         {
//             float duration = 0.5f;
//             float elapsed = 0f;
//             
//             while (elapsed < duration)
//             {
//                 elapsed += Time.deltaTime;
//                 canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
//                 yield return null;
//             }
//             
//             canvasGroup.alpha = 1f;
//             canvasGroup.interactable = true;
//         }
//         
//         /// <summary>
//         /// 淡出协程
//         /// </summary>
//         private System.Collections.IEnumerator FadeOutCoroutine(CanvasGroup canvasGroup)
//         {
//             float duration = 0.3f;
//             float elapsed = 0f;
//             float startAlpha = canvasGroup.alpha;
//             
//             canvasGroup.interactable = false;
//             
//             while (elapsed < duration)
//             {
//                 elapsed += Time.deltaTime;
//                 canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
//                 yield return null;
//             }
//             
//             canvasGroup.alpha = 0f;
//         }
//         
//         /// <summary>
//         /// 更新UI动画
//         /// </summary>
//         /// <param name="deltaTime">时间增量</param>
//         private void UpdateUIAnimations(float deltaTime)
//         {
//             // 这里可以添加一些持续的UI动画效果
//             // 比如按钮的呼吸效果、背景的移动等
//         }
//         
//         #endregion
//         
//         #region 音效处理
//         
//         /// <summary>
//         /// 播放背景音乐
//         /// </summary>
//         private void PlayBackgroundMusic()
//         {
//             // 这里可以通过音频管理器播放背景音乐
//             Debug.Log("[主菜单状态] 播放背景音乐");
//         }
//         
//         /// <summary>
//         /// 停止背景音乐
//         /// </summary>
//         private void StopBackgroundMusic()
//         {
//             // 这里可以通过音频管理器停止背景音乐
//             Debug.Log("[主菜单状态] 停止背景音乐");
//         }
//         
//         /// <summary>
//         /// 播放按钮点击音效
//         /// </summary>
//         private void PlayButtonClickSound()
//         {
//             // 这里可以通过音频管理器播放按钮点击音效
//             Debug.Log("[主菜单状态] 播放按钮点击音效");
//         }
//         
//         #endregion
//         
//         #region 按钮事件处理
//         
//         /// <summary>
//         /// 开始游戏按钮点击
//         /// </summary>
//         private void OnStartGameClick()
//         {
//             Debug.Log("[主菜单状态] 点击开始游戏");
//             PlayButtonClickSound();
//             ResetIdleTime();
//             
//             // 转换到游戏状态
//             if (stateMachine != null)
//             {
//                 stateMachine.TransitionToState("GamePlay");
//             }
//         }
//         
//         /// <summary>
//         /// 设置按钮点击
//         /// </summary>
//         private void OnSettingsClick()
//         {
//             Debug.Log("[主菜单状态] 点击设置");
//             PlayButtonClickSound();
//             ResetIdleTime();
//             
//             // 转换到设置状态
//             if (stateMachine != null)
//             {
//                 stateMachine.TransitionToState("Settings");
//             }
//         }
//         
//         /// <summary>
//         /// 成就按钮点击
//         /// </summary>
//         private void OnAchievementsClick()
//         {
//             Debug.Log("[主菜单状态] 点击成就");
//             PlayButtonClickSound();
//             ResetIdleTime();
//             
//             // 转换到成就状态
//             if (stateMachine != null)
//             {
//                 stateMachine.TransitionToState("Achievements");
//             }
//         }
//         
//         /// <summary>
//         /// 商店按钮点击
//         /// </summary>
//         private void OnShopClick()
//         {
//             Debug.Log("[主菜单状态] 点击商店");
//             PlayButtonClickSound();
//             ResetIdleTime();
//             
//             // 转换到商店状态
//             if (stateMachine != null)
//             {
//                 stateMachine.TransitionToState("Shop");
//             }
//         }
//         
//         /// <summary>
//         /// 退出按钮点击
//         /// </summary>
//         private void OnExitClick()
//         {
//             Debug.Log("[主菜单状态] 点击退出");
//             PlayButtonClickSound();
//             ResetIdleTime();
//             
//             // 显示退出确认对话框
//             ShowExitConfirmDialog();
//         }
//         
//         #endregion
//         
//         #region 输入处理
//         
//         public override void HandleInput(UIInputEvent inputEvent)
//         {
//             base.HandleInput(inputEvent);
//             
//             ResetIdleTime();
//             
//             switch (inputEvent.EventType)
//             {
//                 case UIInputEventType.KeyDown:
//                     HandleKeyInput(inputEvent.KeyCode);
//                     break;
//                     
//                 case UIInputEventType.MouseClick:
//                     HandleMouseInput(inputEvent.MousePosition);
//                     break;
//             }
//         }
//         
//         /// <summary>
//         /// 处理键盘输入
//         /// </summary>
//         /// <param name="keyCode">按键代码</param>
//         private void HandleKeyInput(KeyCode keyCode)
//         {
//             switch (keyCode)
//             {
//                 case KeyCode.Escape:
//                     OnExitClick();
//                     break;
//                     
//                 case KeyCode.Return:
//                 case KeyCode.KeypadEnter:
//                     OnStartGameClick();
//                     break;
//                     
//                 case KeyCode.F1:
//                     ShowHelpDialog();
//                     break;
//             }
//         }
//         
//         /// <summary>
//         /// 处理鼠标输入
//         /// </summary>
//         /// <param name="mousePosition">鼠标位置</param>
//         private void HandleMouseInput(Vector2 mousePosition)
//         {
//             // 这里可以处理鼠标相关的输入
//         }
//         
//         #endregion
//         
//         #region 辅助方法
//         
//         /// <summary>
//         /// 重置空闲时间
//         /// </summary>
//         private void ResetIdleTime()
//         {
//             idleTime = 0f;
//         }
//         
//         /// <summary>
//         /// 显示空闲演示
//         /// </summary>
//         private void ShowIdleDemo()
//         {
//             Debug.Log("[主菜单状态] 显示空闲演示");
//             
//             // 这里可以播放游戏演示视频或动画
//             if (logoAnimator != null)
//                 logoAnimator.SetTrigger("PlayDemo");
//         }
//         
//         /// <summary>
//         /// 设置按钮可交互状态
//         /// </summary>
//         /// <param name="interactable">是否可交互</param>
//         private void SetButtonsInteractable(bool interactable)
//         {
//             if (startGameButton != null)
//                 startGameButton.interactable = interactable;
//             
//             if (settingsButton != null)
//                 settingsButton.interactable = interactable;
//             
//             if (achievementsButton != null)
//                 achievementsButton.interactable = interactable;
//             
//             if (shopButton != null)
//                 shopButton.interactable = interactable;
//             
//             if (exitButton != null)
//                 exitButton.interactable = interactable;
//         }
//         
//         /// <summary>
//         /// 显示退出确认对话框
//         /// </summary>
//         private void ShowExitConfirmDialog()
//         {
//             // 这里可以显示确认对话框
//             // 暂时直接退出应用
//             Debug.Log("[主菜单状态] 退出应用");
//             
//             #if UNITY_EDITOR
//                 UnityEditor.EditorApplication.isPlaying = false;
//             #else
//                 Application.Quit();
//             #endif
//         }
//         
//         /// <summary>
//         /// 显示帮助对话框
//         /// </summary>
//         private void ShowHelpDialog()
//         {
//             Debug.Log("[主菜单状态] 显示帮助信息");
//             
//             // 这里可以显示帮助对话框或转换到帮助状态
//             if (stateMachine != null)
//             {
//                 stateMachine.TransitionToState("Help");
//             }
//         }
//         
//         /// <summary>
//         /// 清理UI
//         /// </summary>
//         private void CleanupUI()
//         {
//             // 移除按钮事件监听
//             if (startGameButton != null)
//                 startGameButton.onClick.RemoveAllListeners();
//             
//             if (settingsButton != null)
//                 settingsButton.onClick.RemoveAllListeners();
//             
//             if (achievementsButton != null)
//                 achievementsButton.onClick.RemoveAllListeners();
//             
//             if (shopButton != null)
//                 shopButton.onClick.RemoveAllListeners();
//             
//             if (exitButton != null)
//                 exitButton.onClick.RemoveAllListeners();
//             
//             // 停止背景效果
//             if (backgroundParticles != null)
//                 backgroundParticles.Stop();
//             
//             isInitialized = false;
//         }
//         
//         #endregion
//         
//         #region 状态转换检查
//         
//         public override bool CanTransitionTo(string targetStateName)
//         {
//             // 主菜单可以转换到大部分状态
//             switch (targetStateName)
//             {
//                 case "GamePlay":
//                 case "Settings":
//                 case "Achievements":
//                 case "Shop":
//                 case "Help":
//                     return true;
//                     
//                 default:
//                     return base.CanTransitionTo(targetStateName);
//             }
//         }
//         
//         #endregion
//         
//         #region 数据处理
//         
//         public override object GetStateData()
//         {
//             var data = base.GetStateData();
//             
//             // 添加主菜单特有的数据
//             var mainMenuData = new MainMenuData
//             {
//                 BaseData = data,
//                 IdleTime = idleTime,
//                 IsInitialized = isInitialized,
//                 PlayerName = playerNameText?.text ?? "",
//                 PlayerLevel = PlayerPrefs.GetInt("PlayerLevel", 1)
//             };
//             
//             return mainMenuData;
//         }
//         
//         #endregion
//     }
//     
//     /// <summary>
//     /// 主菜单数据
//     /// </summary>
//     [System.Serializable]
//     public class MainMenuData
//     {
//         public object BaseData;
//         public float IdleTime;
//         public bool IsInitialized;
//         public string PlayerName;
//         public int PlayerLevel;
//     }
// }