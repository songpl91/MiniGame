// using UnityEngine;
// using UnityEngine.UI;
// using Framework.StateMachineUI.Core;
// using System.Collections;
//
// namespace Framework.StateMachineUI.States
// {
//     /// <summary>
//     /// 暂停状态
//     /// 管理游戏暂停界面
//     /// </summary>
//     public class PauseState : UIStateBase
//     {
//         #region UI组件引用
//         
//         [Header("主要按钮")]
//         [SerializeField] private Button resumeButton;
//         [SerializeField] private Button settingsButton;
//         [SerializeField] private Button saveButton;
//         [SerializeField] private Button loadButton;
//         [SerializeField] private Button mainMenuButton;
//         [SerializeField] private Button quitButton;
//         
//         [Header("信息显示")]
//         [SerializeField] private Text pauseTitle;
//         [SerializeField] private Text gameTimeText;
//         [SerializeField] private Text levelText;
//         [SerializeField] private Text scoreText;
//         [SerializeField] private Image backgroundOverlay;
//         
//         [Header("快速保存/加载")]
//         [SerializeField] private Button[] quickSaveSlots;
//         [SerializeField] private Text[] saveSlotTexts;
//         [SerializeField] private Image[] saveSlotImages;
//         
//         [Header("动画组件")]
//         [SerializeField] private Animator pauseAnimator;
//         [SerializeField] private CanvasGroup canvasGroup;
//         
//         #endregion
//         
//         #region 私有变量
//         
//         private bool isInitialized = false;
//         private float pauseStartTime;
//         private string previousStateName;
//         private object previousStateData;
//         
//         // 快速保存槽数据
//         private SaveSlotData[] saveSlots = new SaveSlotData[3];
//         
//         #endregion
//         
//         #region 构造函数
//         
//         public PauseState()
//         {
//             StateName = "Pause";
//             StateType = UIStateType.Overlay; // 暂停界面是叠加显示
//             Priority = 10; // 高优先级
//             CanBeInterrupted = false; // 暂停状态不能被中断
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
//             Debug.Log("[暂停状态] 游戏暂停");
//             
//             // 记录暂停开始时间
//             pauseStartTime = Time.realtimeSinceStartup;
//             
//             // 暂停游戏时间
//             Time.timeScale = 0f;
//             
//             // 处理传入的数据
//             if (data is PauseData pauseData)
//             {
//                 previousStateName = pauseData.PreviousStateName;
//                 previousStateData = pauseData.PreviousStateData;
//             }
//             
//             // 创建或获取UI
//             CreateUI();
//             
//             // 初始化UI
//             InitializeUI();
//             
//             // 更新游戏信息显示
//             UpdateGameInfo();
//             
//             // 加载保存槽信息
//             LoadSaveSlots();
//             
//             // 播放进入动画
//             PlayEnterAnimation();
//             
//             // 播放暂停音效
//             PlayPauseSound();
//         }
//         
//         public override void OnUpdate(float deltaTime)
//         {
//             base.OnUpdate(deltaTime);
//             
//             // 使用未缩放的时间更新
//             float unscaledDeltaTime = Time.unscaledDeltaTime;
//             
//             // 更新游戏时间显示
//             UpdateGameTimeDisplay();
//         }
//         
//         public override void OnExit()
//         {
//             Debug.Log("[暂停状态] 恢复游戏");
//             
//             // 恢复游戏时间
//             Time.timeScale = 1f;
//             
//             // 播放退出动画
//             PlayExitAnimation();
//             
//             // 播放恢复音效
//             PlayResumeSound();
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
//             Debug.Log("[暂停状态] 暂停状态被暂停");
//         }
//         
//         public override void OnResume()
//         {
//             base.OnResume();
//             Debug.Log("[暂停状态] 暂停状态恢复");
//             
//             // 重新暂停游戏时间
//             Time.timeScale = 0f;
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
//                     GetUIComponents();
//                 }
//                 else
//                 {
//                     Debug.LogWarning($"[暂停状态] 无法加载UI预制体: {StateName}");
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
//             // 获取主要按钮
//             resumeButton = FindUIComponent<Button>("ResumeButton");
//             settingsButton = FindUIComponent<Button>("SettingsButton");
//             saveButton = FindUIComponent<Button>("SaveButton");
//             loadButton = FindUIComponent<Button>("LoadButton");
//             mainMenuButton = FindUIComponent<Button>("MainMenuButton");
//             quitButton = FindUIComponent<Button>("QuitButton");
//             
//             // 获取信息显示组件
//             pauseTitle = FindUIComponent<Text>("PauseTitle");
//             gameTimeText = FindUIComponent<Text>("GameTimeText");
//             levelText = FindUIComponent<Text>("LevelText");
//             scoreText = FindUIComponent<Text>("ScoreText");
//             backgroundOverlay = FindUIComponent<Image>("BackgroundOverlay");
//             
//             // 获取快速保存槽
//             quickSaveSlots = FindUIComponents<Button>("QuickSaveSlot");
//             saveSlotTexts = FindUIComponents<Text>("SaveSlotText");
//             saveSlotImages = FindUIComponents<Image>("SaveSlotImage");
//             
//             // 获取动画组件
//             pauseAnimator = uiGameObject.GetComponent<Animator>();
//             canvasGroup = uiGameObject.GetComponent<CanvasGroup>();
//         }
//         
//         /// <summary>
//         /// 创建默认UI
//         /// </summary>
//         private void CreateDefaultUI()
//         {
//             Debug.Log("[暂停状态] 创建默认暂停UI");
//             
//             // 创建根对象
//             uiGameObject = new GameObject("PauseUI");
//             uiGameObject.transform.SetParent(uiManager?.GetUIRoot(StateType) ?? GameObject.Find("Canvas")?.transform);
//             
//             // 添加Canvas组件
//             Canvas canvas = uiGameObject.AddComponent<Canvas>();
//             canvas.overrideSorting = true;
//             canvas.sortingOrder = 100;
//             
//             // 添加CanvasGroup
//             canvasGroup = uiGameObject.AddComponent<CanvasGroup>();
//             
//             // 创建背景遮罩
//             CreateBackgroundOverlay();
//             
//             // 创建暂停面板
//             CreatePausePanel();
//             
//             // 设置UI位置和大小
//             RectTransform rectTransform = uiGameObject.GetComponent<RectTransform>();
//             rectTransform.anchorMin = Vector2.zero;
//             rectTransform.anchorMax = Vector2.one;
//             rectTransform.offsetMin = Vector2.zero;
//             rectTransform.offsetMax = Vector2.zero;
//         }
//         
//         /// <summary>
//         /// 创建背景遮罩
//         /// </summary>
//         private void CreateBackgroundOverlay()
//         {
//             GameObject overlay = new GameObject("BackgroundOverlay");
//             overlay.transform.SetParent(uiGameObject.transform);
//             
//             backgroundOverlay = overlay.AddComponent<Image>();
//             backgroundOverlay.color = new Color(0, 0, 0, 0.7f);
//             
//             RectTransform overlayRect = overlay.GetComponent<RectTransform>();
//             overlayRect.anchorMin = Vector2.zero;
//             overlayRect.anchorMax = Vector2.one;
//             overlayRect.offsetMin = Vector2.zero;
//             overlayRect.offsetMax = Vector2.zero;
//         }
//         
//         /// <summary>
//         /// 创建暂停面板
//         /// </summary>
//         private void CreatePausePanel()
//         {
//             GameObject panel = new GameObject("PausePanel");
//             panel.transform.SetParent(uiGameObject.transform);
//             
//             Image panelImage = panel.AddComponent<Image>();
//             panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
//             
//             RectTransform panelRect = panel.GetComponent<RectTransform>();
//             panelRect.anchorMin = new Vector2(0.5f, 0.5f);
//             panelRect.anchorMax = new Vector2(0.5f, 0.5f);
//             panelRect.sizeDelta = new Vector2(400, 500);
//             panelRect.anchoredPosition = Vector2.zero;
//             
//             // 创建标题
//             CreateTitle(panel.transform);
//             
//             // 创建按钮
//             CreateButtons(panel.transform);
//             
//             // 创建信息显示
//             CreateInfoDisplay(panel.transform);
//         }
//         
//         /// <summary>
//         /// 创建标题
//         /// </summary>
//         private void CreateTitle(Transform parent)
//         {
//             GameObject titleObj = new GameObject("PauseTitle");
//             titleObj.transform.SetParent(parent);
//             
//             pauseTitle = titleObj.AddComponent<Text>();
//             pauseTitle.text = "游戏暂停";
//             pauseTitle.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//             pauseTitle.fontSize = 24;
//             pauseTitle.color = Color.white;
//             pauseTitle.alignment = TextAnchor.MiddleCenter;
//             
//             RectTransform titleRect = titleObj.GetComponent<RectTransform>();
//             titleRect.anchorMin = new Vector2(0, 0.8f);
//             titleRect.anchorMax = new Vector2(1, 0.9f);
//             titleRect.offsetMin = Vector2.zero;
//             titleRect.offsetMax = Vector2.zero;
//         }
//         
//         /// <summary>
//         /// 创建按钮
//         /// </summary>
//         private void CreateButtons(Transform parent)
//         {
//             string[] buttonNames = { "继续游戏", "设置", "保存", "加载", "主菜单", "退出" };
//             Button[] buttons = new Button[buttonNames.Length];
//             
//             for (int i = 0; i < buttonNames.Length; i++)
//             {
//                 GameObject buttonObj = new GameObject($"Button_{i}");
//                 buttonObj.transform.SetParent(parent);
//                 
//                 Image buttonImage = buttonObj.AddComponent<Image>();
//                 buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
//                 
//                 Button button = buttonObj.AddComponent<Button>();
//                 buttons[i] = button;
//                 
//                 // 创建按钮文本
//                 GameObject textObj = new GameObject("Text");
//                 textObj.transform.SetParent(buttonObj.transform);
//                 
//                 Text buttonText = textObj.AddComponent<Text>();
//                 buttonText.text = buttonNames[i];
//                 buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//                 buttonText.fontSize = 16;
//                 buttonText.color = Color.white;
//                 buttonText.alignment = TextAnchor.MiddleCenter;
//                 
//                 RectTransform textRect = textObj.GetComponent<RectTransform>();
//                 textRect.anchorMin = Vector2.zero;
//                 textRect.anchorMax = Vector2.one;
//                 textRect.offsetMin = Vector2.zero;
//                 textRect.offsetMax = Vector2.zero;
//                 
//                 // 设置按钮位置
//                 RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
//                 buttonRect.anchorMin = new Vector2(0.1f, 0.65f - i * 0.08f);
//                 buttonRect.anchorMax = new Vector2(0.9f, 0.7f - i * 0.08f);
//                 buttonRect.offsetMin = Vector2.zero;
//                 buttonRect.offsetMax = Vector2.zero;
//             }
//             
//             // 分配按钮引用
//             resumeButton = buttons[0];
//             settingsButton = buttons[1];
//             saveButton = buttons[2];
//             loadButton = buttons[3];
//             mainMenuButton = buttons[4];
//             quitButton = buttons[5];
//         }
//         
//         /// <summary>
//         /// 创建信息显示
//         /// </summary>
//         private void CreateInfoDisplay(Transform parent)
//         {
//             // 创建游戏时间显示
//             GameObject timeObj = new GameObject("GameTimeText");
//             timeObj.transform.SetParent(parent);
//             
//             gameTimeText = timeObj.AddComponent<Text>();
//             gameTimeText.text = "游戏时间: 00:00:00";
//             gameTimeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//             gameTimeText.fontSize = 14;
//             gameTimeText.color = Color.white;
//             gameTimeText.alignment = TextAnchor.MiddleLeft;
//             
//             RectTransform timeRect = timeObj.GetComponent<RectTransform>();
//             timeRect.anchorMin = new Vector2(0.1f, 0.15f);
//             timeRect.anchorMax = new Vector2(0.9f, 0.2f);
//             timeRect.offsetMin = Vector2.zero;
//             timeRect.offsetMax = Vector2.zero;
//             
//             // 创建关卡显示
//             GameObject levelObj = new GameObject("LevelText");
//             levelObj.transform.SetParent(parent);
//             
//             levelText = levelObj.AddComponent<Text>();
//             levelText.text = "关卡: 1";
//             levelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//             levelText.fontSize = 14;
//             levelText.color = Color.white;
//             levelText.alignment = TextAnchor.MiddleLeft;
//             
//             RectTransform levelRect = levelObj.GetComponent<RectTransform>();
//             levelRect.anchorMin = new Vector2(0.1f, 0.1f);
//             levelRect.anchorMax = new Vector2(0.9f, 0.15f);
//             levelRect.offsetMin = Vector2.zero;
//             levelRect.offsetMax = Vector2.zero;
//             
//             // 创建分数显示
//             GameObject scoreObj = new GameObject("ScoreText");
//             scoreObj.transform.SetParent(parent);
//             
//             scoreText = scoreObj.AddComponent<Text>();
//             scoreText.text = "分数: 0";
//             scoreText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//             scoreText.fontSize = 14;
//             scoreText.color = Color.white;
//             scoreText.alignment = TextAnchor.MiddleLeft;
//             
//             RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
//             scoreRect.anchorMin = new Vector2(0.1f, 0.05f);
//             scoreRect.anchorMax = new Vector2(0.9f, 0.1f);
//             scoreRect.offsetMin = Vector2.zero;
//             scoreRect.offsetMax = Vector2.zero;
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
//             // 设置初始UI状态
//             SetInitialUIState();
//             
//             isInitialized = true;
//         }
//         
//         /// <summary>
//         /// 设置按钮事件
//         /// </summary>
//         private void SetupButtonEvents()
//         {
//             if (resumeButton != null)
//                 resumeButton.onClick.AddListener(OnResumeClicked);
//             
//             if (settingsButton != null)
//                 settingsButton.onClick.AddListener(OnSettingsClicked);
//             
//             if (saveButton != null)
//                 saveButton.onClick.AddListener(OnSaveClicked);
//             
//             if (loadButton != null)
//                 loadButton.onClick.AddListener(OnLoadClicked);
//             
//             if (mainMenuButton != null)
//                 mainMenuButton.onClick.AddListener(OnMainMenuClicked);
//             
//             if (quitButton != null)
//                 quitButton.onClick.AddListener(OnQuitClicked);
//             
//             // 设置快速保存槽事件
//             if (quickSaveSlots != null)
//             {
//                 for (int i = 0; i < quickSaveSlots.Length; i++)
//                 {
//                     if (quickSaveSlots[i] != null)
//                     {
//                         int slotIndex = i; // 闭包变量
//                         quickSaveSlots[i].onClick.AddListener(() => OnQuickSaveSlotClicked(slotIndex));
//                     }
//                 }
//             }
//         }
//         
//         /// <summary>
//         /// 设置初始UI状态
//         /// </summary>
//         private void SetInitialUIState()
//         {
//             if (canvasGroup != null)
//             {
//                 canvasGroup.alpha = 0f;
//                 canvasGroup.interactable = false;
//                 canvasGroup.blocksRaycasts = true;
//             }
//             
//             // 设置背景遮罩透明度
//             if (backgroundOverlay != null)
//             {
//                 Color overlayColor = backgroundOverlay.color;
//                 overlayColor.a = 0f;
//                 backgroundOverlay.color = overlayColor;
//             }
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
//             if (pauseAnimator != null)
//             {
//                 pauseAnimator.SetTrigger("Enter");
//             }
//             else
//             {
//                 // 使用代码动画
//                 StartCoroutine(FadeInAnimation());
//             }
//         }
//         
//         /// <summary>
//         /// 播放退出动画
//         /// </summary>
//         private void PlayExitAnimation()
//         {
//             if (pauseAnimator != null)
//             {
//                 pauseAnimator.SetTrigger("Exit");
//             }
//             else
//             {
//                 // 使用代码动画
//                 StartCoroutine(FadeOutAnimation());
//             }
//         }
//         
//         /// <summary>
//         /// 淡入动画协程
//         /// </summary>
//         private IEnumerator FadeInAnimation()
//         {
//             float duration = 0.3f;
//             float elapsed = 0f;
//             
//             while (elapsed < duration)
//             {
//                 elapsed += Time.unscaledDeltaTime;
//                 float progress = elapsed / duration;
//                 
//                 if (canvasGroup != null)
//                 {
//                     canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
//                 }
//                 
//                 if (backgroundOverlay != null)
//                 {
//                     Color overlayColor = backgroundOverlay.color;
//                     overlayColor.a = Mathf.Lerp(0f, 0.7f, progress);
//                     backgroundOverlay.color = overlayColor;
//                 }
//                 
//                 yield return null;
//             }
//             
//             if (canvasGroup != null)
//             {
//                 canvasGroup.alpha = 1f;
//                 canvasGroup.interactable = true;
//             }
//         }
//         
//         /// <summary>
//         /// 淡出动画协程
//         /// </summary>
//         private IEnumerator FadeOutAnimation()
//         {
//             float duration = 0.2f;
//             float elapsed = 0f;
//             
//             if (canvasGroup != null)
//                 canvasGroup.interactable = false;
//             
//             while (elapsed < duration)
//             {
//                 elapsed += Time.unscaledDeltaTime;
//                 float progress = elapsed / duration;
//                 
//                 if (canvasGroup != null)
//                 {
//                     canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
//                 }
//                 
//                 if (backgroundOverlay != null)
//                 {
//                     Color overlayColor = backgroundOverlay.color;
//                     overlayColor.a = Mathf.Lerp(0.7f, 0f, progress);
//                     backgroundOverlay.color = overlayColor;
//                 }
//                 
//                 yield return null;
//             }
//         }
//         
//         #endregion
//         
//         #region 音效处理
//         
//         /// <summary>
//         /// 播放暂停音效
//         /// </summary>
//         private void PlayPauseSound()
//         {
//             // TODO: 播放暂停音效
//             Debug.Log("[暂停状态] 播放暂停音效");
//         }
//         
//         /// <summary>
//         /// 播放恢复音效
//         /// </summary>
//         private void PlayResumeSound()
//         {
//             // TODO: 播放恢复音效
//             Debug.Log("[暂停状态] 播放恢复音效");
//         }
//         
//         /// <summary>
//         /// 播放按钮点击音效
//         /// </summary>
//         private void PlayButtonClickSound()
//         {
//             // TODO: 播放按钮点击音效
//             Debug.Log("[暂停状态] 播放按钮点击音效");
//         }
//         
//         #endregion
//         
//         #region 按钮事件处理
//         
//         /// <summary>
//         /// 继续游戏按钮点击
//         /// </summary>
//         private void OnResumeClicked()
//         {
//             PlayButtonClickSound();
//             Debug.Log("[暂停状态] 点击继续游戏");
//             
//             // 返回到之前的状态
//             if (!string.IsNullOrEmpty(previousStateName))
//             {
//                 uiManager?.GetStateMachine()?.TransitionToState(previousStateName, previousStateData);
//             }
//             else
//             {
//                 // 默认返回游戏状态
//                 uiManager?.GetStateMachine()?.TransitionToState("GamePlay");
//             }
//         }
//         
//         /// <summary>
//         /// 设置按钮点击
//         /// </summary>
//         private void OnSettingsClicked()
//         {
//             PlayButtonClickSound();
//             Debug.Log("[暂停状态] 点击设置");
//             
//             // 转换到设置状态
//             uiManager?.GetStateMachine()?.TransitionToState("Settings");
//         }
//         
//         /// <summary>
//         /// 保存按钮点击
//         /// </summary>
//         private void OnSaveClicked()
//         {
//             PlayButtonClickSound();
//             Debug.Log("[暂停状态] 点击保存游戏");
//             
//             // TODO: 实现保存游戏逻辑
//             ShowMessage("游戏已保存");
//         }
//         
//         /// <summary>
//         /// 加载按钮点击
//         /// </summary>
//         private void OnLoadClicked()
//         {
//             PlayButtonClickSound();
//             Debug.Log("[暂停状态] 点击加载游戏");
//             
//             // TODO: 实现加载游戏逻辑
//             ShowMessage("游戏已加载");
//         }
//         
//         /// <summary>
//         /// 主菜单按钮点击
//         /// </summary>
//         private void OnMainMenuClicked()
//         {
//             PlayButtonClickSound();
//             Debug.Log("[暂停状态] 点击返回主菜单");
//             
//             // 显示确认对话框
//             ShowConfirmDialog("确定要返回主菜单吗？未保存的进度将会丢失。", () =>
//             {
//                 // 确认返回主菜单
//                 uiManager?.GetStateMachine()?.TransitionToState("MainMenu");
//             });
//         }
//         
//         /// <summary>
//         /// 退出按钮点击
//         /// </summary>
//         private void OnQuitClicked()
//         {
//             PlayButtonClickSound();
//             Debug.Log("[暂停状态] 点击退出游戏");
//             
//             // 显示确认对话框
//             ShowConfirmDialog("确定要退出游戏吗？未保存的进度将会丢失。", () =>
//             {
//                 // 确认退出游戏
//                 Application.Quit();
// #if UNITY_EDITOR
//                 UnityEditor.EditorApplication.isPlaying = false;
// #endif
//             });
//         }
//         
//         /// <summary>
//         /// 快速保存槽点击
//         /// </summary>
//         private void OnQuickSaveSlotClicked(int slotIndex)
//         {
//             PlayButtonClickSound();
//             Debug.Log($"[暂停状态] 点击快速保存槽 {slotIndex}");
//             
//             // TODO: 实现快速保存/加载逻辑
//             if (saveSlots[slotIndex] != null && saveSlots[slotIndex].HasData)
//             {
//                 // 有存档数据，询问是否加载
//                 ShowConfirmDialog($"是否加载存档槽 {slotIndex + 1}？", () =>
//                 {
//                     LoadQuickSave(slotIndex);
//                 });
//             }
//             else
//             {
//                 // 没有存档数据，直接保存
//                 SaveQuickSave(slotIndex);
//             }
//         }
//         
//         #endregion
//         
//         #region 游戏信息更新
//         
//         /// <summary>
//         /// 更新游戏信息显示
//         /// </summary>
//         private void UpdateGameInfo()
//         {
//             // 更新游戏时间
//             UpdateGameTimeDisplay();
//             
//             // 更新关卡信息
//             if (levelText != null)
//             {
//                 // TODO: 从游戏管理器获取当前关卡
//                 levelText.text = "关卡: 1";
//             }
//             
//             // 更新分数信息
//             if (scoreText != null)
//             {
//                 // TODO: 从游戏管理器获取当前分数
//                 scoreText.text = "分数: 0";
//             }
//         }
//         
//         /// <summary>
//         /// 更新游戏时间显示
//         /// </summary>
//         private void UpdateGameTimeDisplay()
//         {
//             if (gameTimeText != null)
//             {
//                 // TODO: 从游戏管理器获取游戏时间
//                 float gameTime = Time.time; // 临时使用
//                 int hours = Mathf.FloorToInt(gameTime / 3600f);
//                 int minutes = Mathf.FloorToInt((gameTime % 3600f) / 60f);
//                 int seconds = Mathf.FloorToInt(gameTime % 60f);
//                 
//                 gameTimeText.text = $"游戏时间: {hours:00}:{minutes:00}:{seconds:00}";
//             }
//         }
//         
//         #endregion
//         
//         #region 保存/加载系统
//         
//         /// <summary>
//         /// 加载保存槽信息
//         /// </summary>
//         private void LoadSaveSlots()
//         {
//             for (int i = 0; i < saveSlots.Length; i++)
//             {
//                 saveSlots[i] = LoadSaveSlotData(i);
//                 UpdateSaveSlotUI(i);
//             }
//         }
//         
//         /// <summary>
//         /// 加载保存槽数据
//         /// </summary>
//         private SaveSlotData LoadSaveSlotData(int slotIndex)
//         {
//             string key = $"QuickSave_{slotIndex}";
//             if (PlayerPrefs.HasKey(key))
//             {
//                 string jsonData = PlayerPrefs.GetString(key);
//                 return JsonUtility.FromJson<SaveSlotData>(jsonData);
//             }
//             
//             return new SaveSlotData { SlotIndex = slotIndex, HasData = false };
//         }
//         
//         /// <summary>
//         /// 更新保存槽UI
//         /// </summary>
//         private void UpdateSaveSlotUI(int slotIndex)
//         {
//             if (saveSlotTexts != null && slotIndex < saveSlotTexts.Length && saveSlotTexts[slotIndex] != null)
//             {
//                 SaveSlotData slotData = saveSlots[slotIndex];
//                 if (slotData.HasData)
//                 {
//                     saveSlotTexts[slotIndex].text = $"槽位 {slotIndex + 1}\n{slotData.SaveTime}\n关卡 {slotData.Level}";
//                 }
//                 else
//                 {
//                     saveSlotTexts[slotIndex].text = $"槽位 {slotIndex + 1}\n空";
//                 }
//             }
//         }
//         
//         /// <summary>
//         /// 快速保存
//         /// </summary>
//         private void SaveQuickSave(int slotIndex)
//         {
//             SaveSlotData saveData = new SaveSlotData
//             {
//                 SlotIndex = slotIndex,
//                 HasData = true,
//                 SaveTime = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm"),
//                 Level = 1, // TODO: 从游戏管理器获取
//                 Score = 0, // TODO: 从游戏管理器获取
//                 GameTime = Time.time // TODO: 从游戏管理器获取
//             };
//             
//             string jsonData = JsonUtility.ToJson(saveData);
//             PlayerPrefs.SetString($"QuickSave_{slotIndex}", jsonData);
//             PlayerPrefs.Save();
//             
//             saveSlots[slotIndex] = saveData;
//             UpdateSaveSlotUI(slotIndex);
//             
//             ShowMessage($"已保存到槽位 {slotIndex + 1}");
//         }
//         
//         /// <summary>
//         /// 快速加载
//         /// </summary>
//         private void LoadQuickSave(int slotIndex)
//         {
//             SaveSlotData saveData = saveSlots[slotIndex];
//             if (saveData != null && saveData.HasData)
//             {
//                 // TODO: 实现加载游戏数据逻辑
//                 ShowMessage($"已加载槽位 {slotIndex + 1}");
//                 
//                 // 返回游戏状态
//                 uiManager?.GetStateMachine()?.TransitionToState("GamePlay");
//             }
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
//             if (inputEvent.EventType == UIInputEventType.KeyDown)
//             {
//                 switch (inputEvent.KeyCode)
//                 {
//                     case KeyCode.Escape:
//                         // ESC键继续游戏
//                         OnResumeClicked();
//                         break;
//                     
//                     case KeyCode.F5:
//                         // F5快速保存
//                         SaveQuickSave(0);
//                         break;
//                     
//                     case KeyCode.F9:
//                         // F9快速加载
//                         if (saveSlots[0] != null && saveSlots[0].HasData)
//                             LoadQuickSave(0);
//                         break;
//                 }
//             }
//         }
//         
//         #endregion
//         
//         #region 辅助方法
//         
//         /// <summary>
//         /// 显示消息
//         /// </summary>
//         private void ShowMessage(string message)
//         {
//             Debug.Log($"[暂停状态] {message}");
//             // TODO: 显示UI消息提示
//         }
//         
//         /// <summary>
//         /// 显示确认对话框
//         /// </summary>
//         private void ShowConfirmDialog(string message, System.Action onConfirm)
//         {
//             Debug.Log($"[暂停状态] 确认对话框: {message}");
//             // TODO: 显示确认对话框UI
//             // 临时直接执行确认操作
//             onConfirm?.Invoke();
//         }
//         
//         /// <summary>
//         /// 清理UI
//         /// </summary>
//         private void CleanupUI()
//         {
//             if (uiGameObject != null)
//             {
//                 // 移除按钮事件监听
//                 if (resumeButton != null)
//                     resumeButton.onClick.RemoveAllListeners();
//                 if (settingsButton != null)
//                     settingsButton.onClick.RemoveAllListeners();
//                 if (saveButton != null)
//                     saveButton.onClick.RemoveAllListeners();
//                 if (loadButton != null)
//                     loadButton.onClick.RemoveAllListeners();
//                 if (mainMenuButton != null)
//                     mainMenuButton.onClick.RemoveAllListeners();
//                 if (quitButton != null)
//                     quitButton.onClick.RemoveAllListeners();
//                 
//                 // 移除快速保存槽事件
//                 if (quickSaveSlots != null)
//                 {
//                     foreach (var slot in quickSaveSlots)
//                     {
//                         if (slot != null)
//                             slot.onClick.RemoveAllListeners();
//                     }
//                 }
//             }
//         }
//         
//         #endregion
//         
//         #region 状态转换检查
//         
//         public override bool CanTransitionTo(string targetStateName)
//         {
//             // 暂停状态可以转换到大部分状态
//             return true;
//         }
//         
//         #endregion
//         
//         #region 数据处理
//         
//         public override object GetStateData()
//         {
//             var baseData = base.GetStateData();
//             
//             var pauseStateData = new PauseStateData
//             {
//                 BaseData = baseData,
//                 PauseStartTime = pauseStartTime,
//                 PreviousStateName = previousStateName,
//                 PreviousStateData = previousStateData,
//                 SaveSlots = saveSlots
//             };
//             
//             return pauseStateData;
//         }
//         
//         #endregion
//     }
//     
//     /// <summary>
//     /// 暂停状态数据
//     /// </summary>
//     [System.Serializable]
//     public class PauseData
//     {
//         public string PreviousStateName;
//         public object PreviousStateData;
//     }
//     
//     /// <summary>
//     /// 暂停状态完整数据
//     /// </summary>
//     [System.Serializable]
//     public class PauseStateData
//     {
//         public object BaseData;
//         public float PauseStartTime;
//         public string PreviousStateName;
//         public object PreviousStateData;
//         public SaveSlotData[] SaveSlots;
//     }
//     
//     /// <summary>
//     /// 保存槽数据
//     /// </summary>
//     [System.Serializable]
//     public class SaveSlotData
//     {
//         public int SlotIndex;
//         public bool HasData;
//         public string SaveTime;
//         public int Level;
//         public int Score;
//         public float GameTime;
//     }
// }