// using UnityEngine;
// using UnityEngine.UI;
// using Framework.StateMachineUI.Core;
//
// namespace Framework.StateMachineUI.States
// {
//     /// <summary>
//     /// 游戏状态
//     /// 管理游戏进行中的UI界面
//     /// </summary>
//     public class GamePlayState : UIStateBase
//     {
//         #region UI组件引用
//         
//         [Header("游戏HUD")]
//         [SerializeField] private Text scoreText;
//         [SerializeField] private Text timeText;
//         [SerializeField] private Text livesText;
//         [SerializeField] private Slider healthBar;
//         [SerializeField] private Slider energyBar;
//         
//         [Header("控制按钮")]
//         [SerializeField] private Button pauseButton;
//         [SerializeField] private Button settingsButton;
//         [SerializeField] private Button helpButton;
//         
//         [Header("小地图")]
//         [SerializeField] private RawImage miniMapImage;
//         [SerializeField] private Transform playerMarker;
//         [SerializeField] private Transform[] enemyMarkers;
//         
//         [Header("技能栏")]
//         [SerializeField] private Button[] skillButtons;
//         [SerializeField] private Image[] skillCooldownImages;
//         [SerializeField] private Text[] skillCooldownTexts;
//         
//         [Header("物品栏")]
//         [SerializeField] private Button inventoryButton;
//         [SerializeField] private Text itemCountText;
//         [SerializeField] private Image quickUseItemImage;
//         [SerializeField] private Button quickUseButton;
//         
//         #endregion
//         
//         #region 游戏数据
//         
//         private GameData gameData;
//         private float gameTime;
//         private bool isGamePaused;
//         private bool isGameOver;
//         
//         // 技能冷却时间
//         private float[] skillCooldowns;
//         private float[] skillMaxCooldowns = { 5f, 8f, 12f, 15f }; // 示例冷却时间
//         
//         #endregion
//         
//         #region 构造函数
//         
//         public GamePlayState()
//         {
//             StateName = "GamePlay";
//             StateType = UIStateType.Exclusive; // 游戏状态是独占的
//             Priority = 10;
//             CanBeInterrupted = true; // 可以被暂停菜单等中断
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
//             Debug.Log("[游戏状态] 进入游戏");
//             
//             // 处理传入的游戏数据
//             ProcessGameData(data);
//             
//             // 创建或获取UI
//             CreateUI();
//             
//             // 初始化游戏状态
//             InitializeGameState();
//             
//             // 初始化UI
//             InitializeUI();
//             
//             // 开始游戏
//             StartGame();
//         }
//         
//         public override void OnUpdate(float deltaTime)
//         {
//             base.OnUpdate(deltaTime);
//             
//             if (isGamePaused || isGameOver)
//                 return;
//             
//             // 更新游戏时间
//             gameTime += deltaTime;
//             
//             // 更新游戏逻辑
//             UpdateGameLogic(deltaTime);
//             
//             // 更新UI显示
//             UpdateUIDisplay(deltaTime);
//             
//             // 更新技能冷却
//             UpdateSkillCooldowns(deltaTime);
//             
//             // 检查游戏结束条件
//             CheckGameOverConditions();
//         }
//         
//         public override void OnExit()
//         {
//             Debug.Log("[游戏状态] 退出游戏");
//             
//             // 保存游戏数据
//             SaveGameData();
//             
//             // 停止游戏
//             StopGame();
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
//             Debug.Log("[游戏状态] 暂停游戏");
//             
//             isGamePaused = true;
//             
//             // 暂停游戏逻辑
//             Time.timeScale = 0f;
//             
//             // 显示暂停UI
//             ShowPauseOverlay();
//         }
//         
//         public override void OnResume()
//         {
//             base.OnResume();
//             
//             Debug.Log("[游戏状态] 恢复游戏");
//             
//             isGamePaused = false;
//             
//             // 恢复游戏逻辑
//             Time.timeScale = 1f;
//             
//             // 隐藏暂停UI
//             HidePauseOverlay();
//         }
//         
//         #endregion
//         
//         #region 游戏数据处理
//         
//         /// <summary>
//         /// 处理游戏数据
//         /// </summary>
//         /// <param name="data">传入的数据</param>
//         private void ProcessGameData(object data)
//         {
//             if (data is GameData inputGameData)
//             {
//                 gameData = inputGameData;
//                 Debug.Log($"[游戏状态] 加载游戏数据: 关卡 {gameData.Level}");
//             }
//             else
//             {
//                 // 创建新的游戏数据
//                 gameData = new GameData
//                 {
//                     Level = 1,
//                     Score = 0,
//                     Lives = 3,
//                     Health = 100f,
//                     Energy = 100f,
//                     ItemCount = 0
//                 };
//                 Debug.Log("[游戏状态] 创建新游戏数据");
//             }
//         }
//         
//         /// <summary>
//         /// 保存游戏数据
//         /// </summary>
//         private void SaveGameData()
//         {
//             if (gameData == null)
//                 return;
//             
//             // 更新游戏数据
//             gameData.PlayTime = gameTime;
//             
//             // 保存到PlayerPrefs或其他存储系统
//             PlayerPrefs.SetInt("LastLevel", gameData.Level);
//             PlayerPrefs.SetInt("HighScore", Mathf.Max(PlayerPrefs.GetInt("HighScore", 0), gameData.Score));
//             PlayerPrefs.SetFloat("TotalPlayTime", PlayerPrefs.GetFloat("TotalPlayTime", 0f) + gameTime);
//             
//             Debug.Log("[游戏状态] 游戏数据已保存");
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
//                     Debug.LogWarning($"[游戏状态] 无法加载UI预制体: {StateName}");
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
//             // 获取HUD组件
//             scoreText = FindUIComponent<Text>("ScoreText");
//             timeText = FindUIComponent<Text>("TimeText");
//             livesText = FindUIComponent<Text>("LivesText");
//             healthBar = FindUIComponent<Slider>("HealthBar");
//             energyBar = FindUIComponent<Slider>("EnergyBar");
//             
//             // 获取控制按钮
//             pauseButton = FindUIComponent<Button>("PauseButton");
//             settingsButton = FindUIComponent<Button>("SettingsButton");
//             helpButton = FindUIComponent<Button>("HelpButton");
//             
//             // 获取小地图组件
//             miniMapImage = FindUIComponent<RawImage>("MiniMapImage");
//             playerMarker = FindUIComponent<Transform>("PlayerMarker");
//             
//             // 获取技能栏组件
//             skillButtons = FindUIComponents<Button>("SkillButton");
//             skillCooldownImages = FindUIComponents<Image>("SkillCooldownImage");
//             skillCooldownTexts = FindUIComponents<Text>("SkillCooldownText");
//             
//             // 获取物品栏组件
//             inventoryButton = FindUIComponent<Button>("InventoryButton");
//             itemCountText = FindUIComponent<Text>("ItemCountText");
//             quickUseItemImage = FindUIComponent<Image>("QuickUseItemImage");
//             quickUseButton = FindUIComponent<Button>("QuickUseButton");
//         }
//         
//         /// <summary>
//         /// 创建默认UI
//         /// </summary>
//         private void CreateDefaultUI()
//         {
//             Debug.Log("[游戏状态] 创建默认游戏UI");
//             
//             // 创建简单的游戏HUD
//             var hudGO = new GameObject("GameHUD");
//             var canvas = hudGO.AddComponent<Canvas>();
//             canvas.renderMode = RenderMode.ScreenSpaceOverlay;
//             canvas.sortingOrder = 10;
//             
//             uiGameObject = hudGO;
//             
//             // 创建基本HUD元素
//             CreateDefaultHUD();
//         }
//         
//         /// <summary>
//         /// 创建默认HUD
//         /// </summary>
//         private void CreateDefaultHUD()
//         {
//             if (uiGameObject == null)
//                 return;
//             
//             // 创建分数显示
//             scoreText = CreateHUDText("分数: 0", new Vector2(-200, 200), TextAnchor.UpperLeft);
//             
//             // 创建时间显示
//             timeText = CreateHUDText("时间: 00:00", new Vector2(0, 200), TextAnchor.UpperCenter);
//             
//             // 创建生命显示
//             livesText = CreateHUDText("生命: 3", new Vector2(200, 200), TextAnchor.UpperRight);
//             
//             // 创建暂停按钮
//             pauseButton = CreateHUDButton("暂停", new Vector2(200, -200));
//         }
//         
//         /// <summary>
//         /// 创建HUD文本
//         /// </summary>
//         /// <param name="text">文本内容</param>
//         /// <param name="position">位置</param>
//         /// <param name="alignment">对齐方式</param>
//         /// <returns>创建的文本组件</returns>
//         private Text CreateHUDText(string text, Vector2 position, TextAnchor alignment)
//         {
//             var textGO = new GameObject("HUDText");
//             textGO.transform.SetParent(uiGameObject.transform, false);
//             
//             var rectTransform = textGO.AddComponent<RectTransform>();
//             rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
//             rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
//             rectTransform.sizeDelta = new Vector2(200, 30);
//             rectTransform.anchoredPosition = position;
//             
//             var textComponent = textGO.AddComponent<Text>();
//             textComponent.text = text;
//             textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//             textComponent.fontSize = 16;
//             textComponent.color = Color.white;
//             textComponent.alignment = alignment;
//             
//             // 添加阴影效果
//             var shadow = textGO.AddComponent<Shadow>();
//             shadow.effectColor = Color.black;
//             shadow.effectDistance = new Vector2(1, -1);
//             
//             return textComponent;
//         }
//         
//         /// <summary>
//         /// 创建HUD按钮
//         /// </summary>
//         /// <param name="text">按钮文本</param>
//         /// <param name="position">位置</param>
//         /// <returns>创建的按钮</returns>
//         private Button CreateHUDButton(string text, Vector2 position)
//         {
//             var buttonGO = new GameObject("HUDButton");
//             buttonGO.transform.SetParent(uiGameObject.transform, false);
//             
//             var rectTransform = buttonGO.AddComponent<RectTransform>();
//             rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
//             rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
//             rectTransform.sizeDelta = new Vector2(100, 40);
//             rectTransform.anchoredPosition = position;
//             
//             var image = buttonGO.AddComponent<Image>();
//             image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
//             
//             var button = buttonGO.AddComponent<Button>();
//             
//             // 创建按钮文本
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
//             textComponent.fontSize = 14;
//             textComponent.color = Color.white;
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
//             // 设置按钮事件
//             SetupButtonEvents();
//             
//             // 初始化技能冷却
//             InitializeSkillCooldowns();
//             
//             // 更新UI显示
//             UpdateUIDisplay(0f);
//             
//             Debug.Log("[游戏状态] UI初始化完成");
//         }
//         
//         /// <summary>
//         /// 设置按钮事件
//         /// </summary>
//         private void SetupButtonEvents()
//         {
//             if (pauseButton != null)
//                 pauseButton.onClick.AddListener(OnPauseClick);
//             
//             if (settingsButton != null)
//                 settingsButton.onClick.AddListener(OnSettingsClick);
//             
//             if (helpButton != null)
//                 helpButton.onClick.AddListener(OnHelpClick);
//             
//             if (inventoryButton != null)
//                 inventoryButton.onClick.AddListener(OnInventoryClick);
//             
//             if (quickUseButton != null)
//                 quickUseButton.onClick.AddListener(OnQuickUseClick);
//             
//             // 设置技能按钮事件
//             if (skillButtons != null)
//             {
//                 for (int i = 0; i < skillButtons.Length; i++)
//                 {
//                     int skillIndex = i; // 闭包变量
//                     if (skillButtons[i] != null)
//                     {
//                         skillButtons[i].onClick.AddListener(() => OnSkillClick(skillIndex));
//                     }
//                 }
//             }
//         }
//         
//         #endregion
//         
//         #region 游戏逻辑
//         
//         /// <summary>
//         /// 初始化游戏状态
//         /// </summary>
//         private void InitializeGameState()
//         {
//             gameTime = 0f;
//             isGamePaused = false;
//             isGameOver = false;
//             
//             // 初始化技能冷却数组
//             skillCooldowns = new float[skillMaxCooldowns.Length];
//             
//             Debug.Log("[游戏状态] 游戏状态初始化完成");
//         }
//         
//         /// <summary>
//         /// 开始游戏
//         /// </summary>
//         private void StartGame()
//         {
//             Debug.Log("[游戏状态] 游戏开始");
//             
//             // 确保时间缩放正常
//             Time.timeScale = 1f;
//             
//             // 播放游戏开始音效
//             PlayGameStartSound();
//             
//             // 显示游戏开始提示
//             ShowGameStartMessage();
//         }
//         
//         /// <summary>
//         /// 停止游戏
//         /// </summary>
//         private void StopGame()
//         {
//             Debug.Log("[游戏状态] 游戏停止");
//             
//             // 恢复时间缩放
//             Time.timeScale = 1f;
//             
//             // 播放游戏结束音效
//             PlayGameEndSound();
//         }
//         
//         /// <summary>
//         /// 更新游戏逻辑
//         /// </summary>
//         /// <param name="deltaTime">时间增量</param>
//         private void UpdateGameLogic(float deltaTime)
//         {
//             // 这里可以添加具体的游戏逻辑更新
//             // 比如敌人AI、物理模拟、碰撞检测等
//             
//             // 示例：随机增加分数
//             if (Random.Range(0f, 1f) < 0.01f) // 1%概率
//             {
//                 AddScore(10);
//             }
//             
//             // 示例：随机消耗能量
//             if (Random.Range(0f, 1f) < 0.005f) // 0.5%概率
//             {
//                 ConsumeEnergy(5f);
//             }
//         }
//         
//         /// <summary>
//         /// 检查游戏结束条件
//         /// </summary>
//         private void CheckGameOverConditions()
//         {
//             if (isGameOver)
//                 return;
//             
//             // 检查生命值
//             if (gameData.Lives <= 0)
//             {
//                 GameOver(false); // 失败
//                 return;
//             }
//             
//             // 检查血量
//             if (gameData.Health <= 0)
//             {
//                 gameData.Lives--;
//                 gameData.Health = 100f; // 重置血量
//                 
//                 if (gameData.Lives <= 0)
//                 {
//                     GameOver(false);
//                 }
//                 else
//                 {
//                     ShowRespawnMessage();
//                 }
//                 return;
//             }
//             
//             // 检查胜利条件（示例：分数达到1000）
//             if (gameData.Score >= 1000)
//             {
//                 GameOver(true); // 胜利
//                 return;
//             }
//         }
//         
//         /// <summary>
//         /// 游戏结束
//         /// </summary>
//         /// <param name="isVictory">是否胜利</param>
//         private void GameOver(bool isVictory)
//         {
//             isGameOver = true;
//             
//             Debug.Log($"[游戏状态] 游戏结束 - {(isVictory ? "胜利" : "失败")}");
//             
//             // 保存游戏数据
//             SaveGameData();
//             
//             // 显示游戏结束界面
//             ShowGameOverScreen(isVictory);
//         }
//         
//         #endregion
//         
//         #region UI更新
//         
//         /// <summary>
//         /// 更新UI显示
//         /// </summary>
//         /// <param name="deltaTime">时间增量</param>
//         private void UpdateUIDisplay(float deltaTime)
//         {
//             if (gameData == null)
//                 return;
//             
//             // 更新分数
//             if (scoreText != null)
//                 scoreText.text = $"分数: {gameData.Score}";
//             
//             // 更新时间
//             if (timeText != null)
//             {
//                 int minutes = Mathf.FloorToInt(gameTime / 60f);
//                 int seconds = Mathf.FloorToInt(gameTime % 60f);
//                 timeText.text = $"时间: {minutes:00}:{seconds:00}";
//             }
//             
//             // 更新生命
//             if (livesText != null)
//                 livesText.text = $"生命: {gameData.Lives}";
//             
//             // 更新血量条
//             if (healthBar != null)
//                 healthBar.value = gameData.Health / 100f;
//             
//             // 更新能量条
//             if (energyBar != null)
//                 energyBar.value = gameData.Energy / 100f;
//             
//             // 更新物品数量
//             if (itemCountText != null)
//                 itemCountText.text = gameData.ItemCount.ToString();
//             
//             // 更新小地图
//             UpdateMiniMap();
//         }
//         
//         /// <summary>
//         /// 更新小地图
//         /// </summary>
//         private void UpdateMiniMap()
//         {
//             if (miniMapImage == null || playerMarker == null)
//                 return;
//             
//             // 这里可以更新小地图的显示
//             // 比如玩家位置、敌人位置等
//         }
//         
//         /// <summary>
//         /// 初始化技能冷却
//         /// </summary>
//         private void InitializeSkillCooldowns()
//         {
//             if (skillCooldowns == null)
//                 return;
//             
//             for (int i = 0; i < skillCooldowns.Length; i++)
//             {
//                 skillCooldowns[i] = 0f;
//             }
//             
//             UpdateSkillCooldownUI();
//         }
//         
//         /// <summary>
//         /// 更新技能冷却
//         /// </summary>
//         /// <param name="deltaTime">时间增量</param>
//         private void UpdateSkillCooldowns(float deltaTime)
//         {
//             if (skillCooldowns == null)
//                 return;
//             
//             bool needUpdate = false;
//             
//             for (int i = 0; i < skillCooldowns.Length; i++)
//             {
//                 if (skillCooldowns[i] > 0f)
//                 {
//                     skillCooldowns[i] -= deltaTime;
//                     if (skillCooldowns[i] < 0f)
//                         skillCooldowns[i] = 0f;
//                     
//                     needUpdate = true;
//                 }
//             }
//             
//             if (needUpdate)
//             {
//                 UpdateSkillCooldownUI();
//             }
//         }
//         
//         /// <summary>
//         /// 更新技能冷却UI
//         /// </summary>
//         private void UpdateSkillCooldownUI()
//         {
//             if (skillButtons == null || skillCooldownImages == null || skillCooldownTexts == null)
//                 return;
//             
//             for (int i = 0; i < skillCooldowns.Length && i < skillButtons.Length; i++)
//             {
//                 bool isOnCooldown = skillCooldowns[i] > 0f;
//                 
//                 // 更新按钮可交互状态
//                 if (skillButtons[i] != null)
//                     skillButtons[i].interactable = !isOnCooldown;
//                 
//                 // 更新冷却遮罩
//                 if (skillCooldownImages[i] != null)
//                 {
//                     skillCooldownImages[i].fillAmount = isOnCooldown ? 
//                         (skillCooldowns[i] / skillMaxCooldowns[i]) : 0f;
//                 }
//                 
//                 // 更新冷却文本
//                 if (skillCooldownTexts[i] != null)
//                 {
//                     skillCooldownTexts[i].text = isOnCooldown ? 
//                         Mathf.CeilToInt(skillCooldowns[i]).ToString() : "";
//                 }
//             }
//         }
//         
//         #endregion
//         
//         #region 按钮事件处理
//         
//         /// <summary>
//         /// 暂停按钮点击
//         /// </summary>
//         private void OnPauseClick()
//         {
//             Debug.Log("[游戏状态] 点击暂停");
//             
//             // 转换到暂停状态
//             if (stateMachine != null)
//             {
//                 stateMachine.TransitionToState("Pause");
//             }
//         }
//         
//         /// <summary>
//         /// 设置按钮点击
//         /// </summary>
//         private void OnSettingsClick()
//         {
//             Debug.Log("[游戏状态] 点击设置");
//             
//             // 转换到设置状态（叠加模式）
//             if (stateMachine != null)
//             {
//                 stateMachine.TransitionToState("Settings");
//             }
//         }
//         
//         /// <summary>
//         /// 帮助按钮点击
//         /// </summary>
//         private void OnHelpClick()
//         {
//             Debug.Log("[游戏状态] 点击帮助");
//             
//             // 转换到帮助状态
//             if (stateMachine != null)
//             {
//                 stateMachine.TransitionToState("Help");
//             }
//         }
//         
//         /// <summary>
//         /// 背包按钮点击
//         /// </summary>
//         private void OnInventoryClick()
//         {
//             Debug.Log("[游戏状态] 点击背包");
//             
//             // 转换到背包状态
//             if (stateMachine != null)
//             {
//                 stateMachine.TransitionToState("Inventory");
//             }
//         }
//         
//         /// <summary>
//         /// 快速使用按钮点击
//         /// </summary>
//         private void OnQuickUseClick()
//         {
//             Debug.Log("[游戏状态] 快速使用物品");
//             
//             if (gameData.ItemCount > 0)
//             {
//                 UseItem();
//             }
//         }
//         
//         /// <summary>
//         /// 技能按钮点击
//         /// </summary>
//         /// <param name="skillIndex">技能索引</param>
//         private void OnSkillClick(int skillIndex)
//         {
//             Debug.Log($"[游戏状态] 使用技能 {skillIndex + 1}");
//             
//             if (skillIndex < 0 || skillIndex >= skillCooldowns.Length)
//                 return;
//             
//             // 检查冷却时间
//             if (skillCooldowns[skillIndex] > 0f)
//             {
//                 Debug.Log($"[游戏状态] 技能 {skillIndex + 1} 还在冷却中");
//                 return;
//             }
//             
//             // 检查能量
//             float energyCost = 20f + skillIndex * 10f; // 示例能量消耗
//             if (gameData.Energy < energyCost)
//             {
//                 Debug.Log($"[游戏状态] 能量不足，无法使用技能 {skillIndex + 1}");
//                 return;
//             }
//             
//             // 使用技能
//             UseSkill(skillIndex);
//         }
//         
//         #endregion
//         
//         #region 游戏操作
//         
//         /// <summary>
//         /// 增加分数
//         /// </summary>
//         /// <param name="points">分数</param>
//         public void AddScore(int points)
//         {
//             if (gameData == null)
//                 return;
//             
//             gameData.Score += points;
//             Debug.Log($"[游戏状态] 获得分数: +{points} (总分: {gameData.Score})");
//         }
//         
//         /// <summary>
//         /// 消耗能量
//         /// </summary>
//         /// <param name="amount">消耗量</param>
//         public void ConsumeEnergy(float amount)
//         {
//             if (gameData == null)
//                 return;
//             
//             gameData.Energy -= amount;
//             if (gameData.Energy < 0f)
//                 gameData.Energy = 0f;
//             
//             Debug.Log($"[游戏状态] 消耗能量: -{amount} (剩余: {gameData.Energy})");
//         }
//         
//         /// <summary>
//         /// 恢复能量
//         /// </summary>
//         /// <param name="amount">恢复量</param>
//         public void RestoreEnergy(float amount)
//         {
//             if (gameData == null)
//                 return;
//             
//             gameData.Energy += amount;
//             if (gameData.Energy > 100f)
//                 gameData.Energy = 100f;
//             
//             Debug.Log($"[游戏状态] 恢复能量: +{amount} (当前: {gameData.Energy})");
//         }
//         
//         /// <summary>
//         /// 受到伤害
//         /// </summary>
//         /// <param name="damage">伤害值</param>
//         public void TakeDamage(float damage)
//         {
//             if (gameData == null)
//                 return;
//             
//             gameData.Health -= damage;
//             if (gameData.Health < 0f)
//                 gameData.Health = 0f;
//             
//             Debug.Log($"[游戏状态] 受到伤害: -{damage} (剩余血量: {gameData.Health})");
//             
//             // 播放受伤效果
//             PlayDamageEffect();
//         }
//         
//         /// <summary>
//         /// 使用技能
//         /// </summary>
//         /// <param name="skillIndex">技能索引</param>
//         private void UseSkill(int skillIndex)
//         {
//             if (skillIndex < 0 || skillIndex >= skillCooldowns.Length)
//                 return;
//             
//             // 消耗能量
//             float energyCost = 20f + skillIndex * 10f;
//             ConsumeEnergy(energyCost);
//             
//             // 设置冷却时间
//             skillCooldowns[skillIndex] = skillMaxCooldowns[skillIndex];
//             
//             // 执行技能效果
//             ExecuteSkillEffect(skillIndex);
//             
//             Debug.Log($"[游戏状态] 技能 {skillIndex + 1} 使用成功");
//         }
//         
//         /// <summary>
//         /// 执行技能效果
//         /// </summary>
//         /// <param name="skillIndex">技能索引</param>
//         private void ExecuteSkillEffect(int skillIndex)
//         {
//             switch (skillIndex)
//             {
//                 case 0: // 治疗技能
//                     gameData.Health += 30f;
//                     if (gameData.Health > 100f)
//                         gameData.Health = 100f;
//                     Debug.Log("[游戏状态] 使用治疗技能");
//                     break;
//                     
//                 case 1: // 攻击技能
//                     AddScore(50);
//                     Debug.Log("[游戏状态] 使用攻击技能");
//                     break;
//                     
//                 case 2: // 防御技能
//                     Debug.Log("[游戏状态] 使用防御技能");
//                     break;
//                     
//                 case 3: // 特殊技能
//                     RestoreEnergy(50f);
//                     Debug.Log("[游戏状态] 使用特殊技能");
//                     break;
//             }
//         }
//         
//         /// <summary>
//         /// 使用物品
//         /// </summary>
//         private void UseItem()
//         {
//             if (gameData == null || gameData.ItemCount <= 0)
//                 return;
//             
//             gameData.ItemCount--;
//             
//             // 执行物品效果（示例：恢复血量）
//             gameData.Health += 20f;
//             if (gameData.Health > 100f)
//                 gameData.Health = 100f;
//             
//             Debug.Log("[游戏状态] 使用物品，恢复血量");
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
//             if (isGamePaused || isGameOver)
//                 return;
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
//                     OnPauseClick();
//                     break;
//                     
//                 case KeyCode.Tab:
//                     OnInventoryClick();
//                     break;
//                     
//                 case KeyCode.Alpha1:
//                     OnSkillClick(0);
//                     break;
//                     
//                 case KeyCode.Alpha2:
//                     OnSkillClick(1);
//                     break;
//                     
//                 case KeyCode.Alpha3:
//                     OnSkillClick(2);
//                     break;
//                     
//                 case KeyCode.Alpha4:
//                     OnSkillClick(3);
//                     break;
//                     
//                 case KeyCode.Space:
//                     OnQuickUseClick();
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
//             // 这里可以处理鼠标相关的游戏输入
//         }
//         
//         #endregion
//         
//         #region 音效和特效
//         
//         /// <summary>
//         /// 播放游戏开始音效
//         /// </summary>
//         private void PlayGameStartSound()
//         {
//             Debug.Log("[游戏状态] 播放游戏开始音效");
//         }
//         
//         /// <summary>
//         /// 播放游戏结束音效
//         /// </summary>
//         private void PlayGameEndSound()
//         {
//             Debug.Log("[游戏状态] 播放游戏结束音效");
//         }
//         
//         /// <summary>
//         /// 播放受伤效果
//         /// </summary>
//         private void PlayDamageEffect()
//         {
//             Debug.Log("[游戏状态] 播放受伤效果");
//             
//             // 这里可以添加屏幕震动、红色闪烁等效果
//         }
//         
//         #endregion
//         
//         #region 消息显示
//         
//         /// <summary>
//         /// 显示游戏开始消息
//         /// </summary>
//         private void ShowGameStartMessage()
//         {
//             Debug.Log("[游戏状态] 显示游戏开始消息");
//         }
//         
//         /// <summary>
//         /// 显示重生消息
//         /// </summary>
//         private void ShowRespawnMessage()
//         {
//             Debug.Log("[游戏状态] 显示重生消息");
//         }
//         
//         /// <summary>
//         /// 显示游戏结束界面
//         /// </summary>
//         /// <param name="isVictory">是否胜利</param>
//         private void ShowGameOverScreen(bool isVictory)
//         {
//             Debug.Log($"[游戏状态] 显示游戏结束界面 - {(isVictory ? "胜利" : "失败")}");
//             
//             // 转换到游戏结束状态
//             if (stateMachine != null)
//             {
//                 var gameOverData = new GameOverData
//                 {
//                     IsVictory = isVictory,
//                     FinalScore = gameData.Score,
//                     PlayTime = gameTime,
//                     Level = gameData.Level
//                 };
//                 
//                 stateMachine.TransitionToState("GameOver", gameOverData);
//             }
//         }
//         
//         /// <summary>
//         /// 显示暂停覆盖层
//         /// </summary>
//         private void ShowPauseOverlay()
//         {
//             Debug.Log("[游戏状态] 显示暂停覆盖层");
//         }
//         
//         /// <summary>
//         /// 隐藏暂停覆盖层
//         /// </summary>
//         private void HidePauseOverlay()
//         {
//             Debug.Log("[游戏状态] 隐藏暂停覆盖层");
//         }
//         
//         #endregion
//         
//         #region 清理
//         
//         /// <summary>
//         /// 清理UI
//         /// </summary>
//         private void CleanupUI()
//         {
//             // 移除按钮事件监听
//             if (pauseButton != null)
//                 pauseButton.onClick.RemoveAllListeners();
//             
//             if (settingsButton != null)
//                 settingsButton.onClick.RemoveAllListeners();
//             
//             if (helpButton != null)
//                 helpButton.onClick.RemoveAllListeners();
//             
//             if (inventoryButton != null)
//                 inventoryButton.onClick.RemoveAllListeners();
//             
//             if (quickUseButton != null)
//                 quickUseButton.onClick.RemoveAllListeners();
//             
//             if (skillButtons != null)
//             {
//                 foreach (var button in skillButtons)
//                 {
//                     if (button != null)
//                         button.onClick.RemoveAllListeners();
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
//             // 游戏状态可以转换到暂停、设置、帮助、背包等状态
//             switch (targetStateName)
//             {
//                 case "Pause":
//                 case "Settings":
//                 case "Help":
//                 case "Inventory":
//                 case "GameOver":
//                     return true;
//                     
//                 case "MainMenu":
//                     return isGameOver; // 只有游戏结束后才能返回主菜单
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
//             var baseData = base.GetStateData();
//             
//             var gamePlayData = new GamePlayData
//             {
//                 BaseData = baseData,
//                 GameData = gameData,
//                 GameTime = gameTime,
//                 IsGamePaused = isGamePaused,
//                 IsGameOver = isGameOver,
//                 SkillCooldowns = skillCooldowns
//             };
//             
//             return gamePlayData;
//         }
//         
//         #endregion
//     }
//     
//     /// <summary>
//     /// 游戏数据
//     /// </summary>
//     [System.Serializable]
//     public class GameData
//     {
//         public int Level = 1;
//         public int Score = 0;
//         public int Lives = 3;
//         public float Health = 100f;
//         public float Energy = 100f;
//         public int ItemCount = 0;
//         public float PlayTime = 0f;
//     }
//     
//     /// <summary>
//     /// 游戏状态数据
//     /// </summary>
//     [System.Serializable]
//     public class GamePlayData
//     {
//         public object BaseData;
//         public GameData GameData;
//         public float GameTime;
//         public bool IsGamePaused;
//         public bool IsGameOver;
//         public float[] SkillCooldowns;
//     }
//     
//     /// <summary>
//     /// 游戏结束数据
//     /// </summary>
//     [System.Serializable]
//     public class GameOverData
//     {
//         public bool IsVictory;
//         public int FinalScore;
//         public float PlayTime;
//         public int Level;
//     }
// }