// using UnityEngine;
// using UnityEngine.UI;
// using Framework.StateMachineUI.Core;
// using System.Collections.Generic;
//
// namespace Framework.StateMachineUI.States
// {
//     /// <summary>
//     /// 设置状态
//     /// 管理游戏设置界面
//     /// </summary>
//     public class SettingsState : UIStateBase
//     {
//         #region UI组件引用
//
//         [Header("设置分类")] [SerializeField] private Button[] categoryButtons;
//         [SerializeField] private Text[] categoryTexts;
//         [SerializeField] private GameObject[] categoryPanels;
//
//         [Header("音频设置")] [SerializeField] private Slider masterVolumeSlider;
//         [SerializeField] private Slider musicVolumeSlider;
//         [SerializeField] private Slider sfxVolumeSlider;
//         [SerializeField] private Toggle muteToggle;
//         [SerializeField] private Text masterVolumeText;
//         [SerializeField] private Text musicVolumeText;
//         [SerializeField] private Text sfxVolumeText;
//
//         [Header("画质设置")] [SerializeField] private Dropdown qualityDropdown;
//         [SerializeField] private Dropdown resolutionDropdown;
//         [SerializeField] private Toggle fullscreenToggle;
//         [SerializeField] private Toggle vsyncToggle;
//         [SerializeField] private Slider fpsLimitSlider;
//         [SerializeField] private Text fpsLimitText;
//
//         [Header("游戏设置")] [SerializeField] private Dropdown languageDropdown;
//         [SerializeField] private Slider mouseSensitivitySlider;
//         [SerializeField] private Toggle autoSaveToggle;
//         [SerializeField] private Slider autoSaveIntervalSlider;
//         [SerializeField] private Text mouseSensitivityText;
//         [SerializeField] private Text autoSaveIntervalText;
//
//         [Header("控制设置")] [SerializeField] private Button[] keyBindingButtons;
//         [SerializeField] private Text[] keyBindingTexts;
//         [SerializeField] private Toggle invertYToggle;
//         [SerializeField] private Slider deadZoneSlider;
//         [SerializeField] private Text deadZoneText;
//
//         [Header("操作按钮")] [SerializeField] private Button applyButton;
//         [SerializeField] private Button resetButton;
//         [SerializeField] private Button defaultButton;
//         [SerializeField] private Button backButton;
//
//         #endregion
//
//         #region 设置数据
//
//         private SettingsData currentSettings;
//         private SettingsData originalSettings;
//         private int currentCategoryIndex = 0;
//         private bool isWaitingForKeyInput = false;
//         private int keyBindingIndex = -1;
//
//         // 设置分类
//         private readonly string[] categoryNames = { "音频", "画质", "游戏", "控制" };
//
//         // 分辨率选项
//         private Resolution[] availableResolutions;
//         private List<string> resolutionOptions = new List<string>();
//
//         // 语言选项
//         private readonly string[] languageOptions = { "中文", "English", "日本語", "한국어" };
//
//         // 按键绑定名称
//         private readonly string[] keyBindingNames =
//         {
//             "前进", "后退", "左移", "右移", "跳跃", "攻击", "防御", "使用物品"
//         };
//
//         #endregion
//
//         #region 构造函数
//
//         public SettingsState()
//         {
//             StateName = "Settings";
//             StateType = UIStateType.Overlay; // 设置界面通常是叠加显示
//             Priority = 5;
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
//             Debug.Log("[设置状态] 进入设置界面");
//
//             // 创建或获取UI
//             CreateUI();
//
//             // 加载设置数据
//             LoadSettings();
//
//             // 初始化UI
//             InitializeUI();
//
//             // 播放进入动画
//             PlayEnterAnimation();
//         }
//
//         public override void OnUpdate(float deltaTime)
//         {
//             base.OnUpdate(deltaTime);
//
//             // 检查按键绑定输入
//             if (isWaitingForKeyInput)
//             {
//                 CheckKeyBindingInput();
//             }
//         }
//
//         public override void OnExit()
//         {
//             Debug.Log("[设置状态] 退出设置界面");
//
//             // 播放退出动画
//             PlayExitAnimation();
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
//             Debug.Log("[设置状态] 暂停设置界面");
//         }
//
//         public override void OnResume()
//         {
//             base.OnResume();
//             Debug.Log("[设置状态] 恢复设置界面");
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
//                     Debug.LogWarning($"[设置状态] 无法加载UI预制体: {StateName}");
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
//             // 获取分类相关组件
//             categoryButtons = FindUIComponents<Button>("CategoryButton");
//             categoryTexts = FindUIComponents<Text>("CategoryText");
//             categoryPanels = FindUIComponents<GameObject>("CategoryPanel");
//
//             // 获取音频设置组件
//             masterVolumeSlider = FindUIComponent<Slider>("MasterVolumeSlider");
//             musicVolumeSlider = FindUIComponent<Slider>("MusicVolumeSlider");
//             sfxVolumeSlider = FindUIComponent<Slider>("SFXVolumeSlider");
//             muteToggle = FindUIComponent<Toggle>("MuteToggle");
//             masterVolumeText = FindUIComponent<Text>("MasterVolumeText");
//             musicVolumeText = FindUIComponent<Text>("MusicVolumeText");
//             sfxVolumeText = FindUIComponent<Text>("SFXVolumeText");
//
//             // 获取画质设置组件
//             qualityDropdown = FindUIComponent<Dropdown>("QualityDropdown");
//             resolutionDropdown = FindUIComponent<Dropdown>("ResolutionDropdown");
//             fullscreenToggle = FindUIComponent<Toggle>("FullscreenToggle");
//             vsyncToggle = FindUIComponent<Toggle>("VSyncToggle");
//             fpsLimitSlider = FindUIComponent<Slider>("FPSLimitSlider");
//             fpsLimitText = FindUIComponent<Text>("FPSLimitText");
//
//             // 获取游戏设置组件
//             languageDropdown = FindUIComponent<Dropdown>("LanguageDropdown");
//             mouseSensitivitySlider = FindUIComponent<Slider>("MouseSensitivitySlider");
//             autoSaveToggle = FindUIComponent<Toggle>("AutoSaveToggle");
//             autoSaveIntervalSlider = FindUIComponent<Slider>("AutoSaveIntervalSlider");
//             mouseSensitivityText = FindUIComponent<Text>("MouseSensitivityText");
//             autoSaveIntervalText = FindUIComponent<Text>("AutoSaveIntervalText");
//
//             // 获取控制设置组件
//             keyBindingButtons = FindUIComponents<Button>("KeyBindingButton");
//             keyBindingTexts = FindUIComponents<Text>("KeyBindingText");
//             invertYToggle = FindUIComponent<Toggle>("InvertYToggle");
//             deadZoneSlider = FindUIComponent<Slider>("DeadZoneSlider");
//             deadZoneText = FindUIComponent<Text>("DeadZoneText");
//
//             // 获取操作按钮
//             applyButton = FindUIComponent<Button>("ApplyButton");
//             resetButton = FindUIComponent<Button>("ResetButton");
//             defaultButton = FindUIComponent<Button>("DefaultButton");
//             backButton = FindUIComponent<Button>("BackButton");
//         }
//
//         /// <summary>
//         /// 创建默认UI
//         /// </summary>
//         private void CreateDefaultUI()
//         {
//             Debug.Log("[设置状态] 创建默认设置UI");
//
//             // 创建简单的设置界面
//             var settingsGO = new GameObject("SettingsUI");
//             var canvas = settingsGO.AddComponent<Canvas>();
//             canvas.renderMode = RenderMode.ScreenSpaceOverlay;
//             canvas.sortingOrder = 20;
//
//             uiGameObject = settingsGO;
//
//             // 创建基本设置元素
//             CreateDefaultSettingsUI();
//         }
//
//         /// <summary>
//         /// 创建默认设置UI
//         /// </summary>
//         private void CreateDefaultSettingsUI()
//         {
//             if (uiGameObject == null)
//                 return;
//
//             // 创建背景
//             CreateBackground();
//
//             // 创建标题
//             CreateTitle();
//
//             // 创建基本设置项
//             CreateBasicSettings();
//
//             // 创建操作按钮
//             CreateActionButtons();
//         }
//
//         /// <summary>
//         /// 创建背景
//         /// </summary>
//         private void CreateBackground()
//         {
//             var bgGO = new GameObject("Background");
//             bgGO.transform.SetParent(uiGameObject.transform, false);
//
//             var rectTransform = bgGO.AddComponent<RectTransform>();
//             rectTransform.anchorMin = Vector2.zero;
//             rectTransform.anchorMax = Vector2.one;
//             rectTransform.sizeDelta = Vector2.zero;
//             rectTransform.anchoredPosition = Vector2.zero;
//
//             var image = bgGO.AddComponent<Image>();
//             image.color = new Color(0f, 0f, 0f, 0.8f);
//         }
//
//         /// <summary>
//         /// 创建标题
//         /// </summary>
//         private void CreateTitle()
//         {
//             var titleGO = new GameObject("Title");
//             titleGO.transform.SetParent(uiGameObject.transform, false);
//
//             var rectTransform = titleGO.AddComponent<RectTransform>();
//             rectTransform.anchorMin = new Vector2(0.5f, 1f);
//             rectTransform.anchorMax = new Vector2(0.5f, 1f);
//             rectTransform.sizeDelta = new Vector2(200, 50);
//             rectTransform.anchoredPosition = new Vector2(0, -50);
//
//             var text = titleGO.AddComponent<Text>();
//             text.text = "设置";
//             text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//             text.fontSize = 24;
//             text.color = Color.white;
//             text.alignment = TextAnchor.MiddleCenter;
//         }
//
//         /// <summary>
//         /// 创建基本设置项
//         /// </summary>
//         private void CreateBasicSettings()
//         {
//             // 创建主音量滑动条
//             masterVolumeSlider = CreateSlider("主音量", new Vector2(0, 100), 0f, 1f, 1f);
//
//             // 创建全屏开关
//             fullscreenToggle = CreateToggle("全屏", new Vector2(0, 50), Screen.fullScreen);
//
//             // 创建自动保存开关
//             autoSaveToggle = CreateToggle("自动保存", new Vector2(0, 0), true);
//         }
//
//         /// <summary>
//         /// 创建滑动条
//         /// </summary>
//         /// <param name="label">标签</param>
//         /// <param name="position">位置</param>
//         /// <param name="minValue">最小值</param>
//         /// <param name="maxValue">最大值</param>
//         /// <param name="value">当前值</param>
//         /// <returns>创建的滑动条</returns>
//         private Slider CreateSlider(string label, Vector2 position, float minValue, float maxValue, float value)
//         {
//             var sliderGO = new GameObject(label + "Slider");
//             sliderGO.transform.SetParent(uiGameObject.transform, false);
//
//             var rectTransform = sliderGO.AddComponent<RectTransform>();
//             rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
//             rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
//             rectTransform.sizeDelta = new Vector2(300, 30);
//             rectTransform.anchoredPosition = position;
//
//             var slider = sliderGO.AddComponent<Slider>();
//             slider.minValue = minValue;
//             slider.maxValue = maxValue;
//             slider.value = value;
//
//             // 创建背景
//             var background = new GameObject("Background");
//             background.transform.SetParent(sliderGO.transform, false);
//             var bgRect = background.AddComponent<RectTransform>();
//             bgRect.anchorMin = Vector2.zero;
//             bgRect.anchorMax = Vector2.one;
//             bgRect.sizeDelta = Vector2.zero;
//             bgRect.anchoredPosition = Vector2.zero;
//             var bgImage = background.AddComponent<Image>();
//             bgImage.color = Color.gray;
//             slider.targetGraphic = bgImage;
//
//             // 创建填充
//             var fillArea = new GameObject("Fill Area");
//             fillArea.transform.SetParent(sliderGO.transform, false);
//             var fillAreaRect = fillArea.AddComponent<RectTransform>();
//             fillAreaRect.anchorMin = Vector2.zero;
//             fillAreaRect.anchorMax = Vector2.one;
//             fillAreaRect.sizeDelta = Vector2.zero;
//             fillAreaRect.anchoredPosition = Vector2.zero;
//
//             var fill = new GameObject("Fill");
//             fill.transform.SetParent(fillArea.transform, false);
//             var fillRect = fill.AddComponent<RectTransform>();
//             fillRect.anchorMin = Vector2.zero;
//             fillRect.anchorMax = Vector2.one;
//             fillRect.sizeDelta = Vector2.zero;
//             fillRect.anchoredPosition = Vector2.zero;
//             var fillImage = fill.AddComponent<Image>();
//             fillImage.color = Color.white;
//             slider.fillRect = fillRect;
//
//             // 创建标签
//             var labelGO = new GameObject("Label");
//             labelGO.transform.SetParent(sliderGO.transform, false);
//             var labelRect = labelGO.AddComponent<RectTransform>();
//             labelRect.anchorMin = new Vector2(0f, 0.5f);
//             labelRect.anchorMax = new Vector2(0f, 0.5f);
//             labelRect.sizeDelta = new Vector2(80, 30);
//             labelRect.anchoredPosition = new Vector2(-100, 0);
//             var labelText = labelGO.AddComponent<Text>();
//             labelText.text = label;
//             labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//             labelText.fontSize = 14;
//             labelText.color = Color.white;
//             labelText.alignment = TextAnchor.MiddleRight;
//
//             return slider;
//         }
//
//         /// <summary>
//         /// 创建开关
//         /// </summary>
//         /// <param name="label">标签</param>
//         /// <param name="position">位置</param>
//         /// <param name="isOn">是否开启</param>
//         /// <returns>创建的开关</returns>
//         private Toggle CreateToggle(string label, Vector2 position, bool isOn)
//         {
//             var toggleGO = new GameObject(label + "Toggle");
//             toggleGO.transform.SetParent(uiGameObject.transform, false);
//
//             var rectTransform = toggleGO.AddComponent<RectTransform>();
//             rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
//             rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
//             rectTransform.sizeDelta = new Vector2(200, 30);
//             rectTransform.anchoredPosition = position;
//
//             var toggle = toggleGO.AddComponent<Toggle>();
//             toggle.isOn = isOn;
//
//             // 创建背景
//             var background = new GameObject("Background");
//             background.transform.SetParent(toggleGO.transform, false);
//             var bgRect = background.AddComponent<RectTransform>();
//             bgRect.anchorMin = new Vector2(0f, 0.5f);
//             bgRect.anchorMax = new Vector2(0f, 0.5f);
//             bgRect.sizeDelta = new Vector2(20, 20);
//             bgRect.anchoredPosition = new Vector2(10, 0);
//             var bgImage = background.AddComponent<Image>();
//             bgImage.color = Color.white;
//             toggle.targetGraphic = bgImage;
//
//             // 创建勾选标记
//             var checkmark = new GameObject("Checkmark");
//             checkmark.transform.SetParent(background.transform, false);
//             var checkRect = checkmark.AddComponent<RectTransform>();
//             checkRect.anchorMin = Vector2.zero;
//             checkRect.anchorMax = Vector2.one;
//             checkRect.sizeDelta = Vector2.zero;
//             checkRect.anchoredPosition = Vector2.zero;
//             var checkImage = checkmark.AddComponent<Image>();
//             checkImage.color = Color.green;
//             toggle.graphic = checkImage;
//
//             // 创建标签
//             var labelGO = new GameObject("Label");
//             labelGO.transform.SetParent(toggleGO.transform, false);
//             var labelRect = labelGO.AddComponent<RectTransform>();
//             labelRect.anchorMin = new Vector2(0f, 0.5f);
//             labelRect.anchorMax = new Vector2(1f, 0.5f);
//             labelRect.sizeDelta = Vector2.zero;
//             labelRect.anchoredPosition = new Vector2(20, 0);
//             var labelText = labelGO.AddComponent<Text>();
//             labelText.text = label;
//             labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//             labelText.fontSize = 14;
//             labelText.color = Color.white;
//             labelText.alignment = TextAnchor.MiddleLeft;
//
//             return toggle;
//         }
//
//         /// <summary>
//         /// 创建操作按钮
//         /// </summary>
//         private void CreateActionButtons()
//         {
//             // 创建应用按钮
//             applyButton = CreateButton("应用", new Vector2(-100, -200));
//
//             // 创建重置按钮
//             resetButton = CreateButton("重置", new Vector2(0, -200));
//
//             // 创建返回按钮
//             backButton = CreateButton("返回", new Vector2(100, -200));
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
//             rectTransform.sizeDelta = new Vector2(80, 40);
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
//             textComponent.fontSize = 14;
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
//             // 设置事件监听
//             SetupEventListeners();
//
//             // 初始化分辨率选项
//             InitializeResolutionOptions();
//
//             // 初始化下拉框选项
//             InitializeDropdownOptions();
//
//             // 更新UI显示
//             UpdateUIFromSettings();
//
//             // 显示默认分类
//             ShowCategory(0);
//
//             Debug.Log("[设置状态] UI初始化完成");
//         }
//
//         /// <summary>
//         /// 设置事件监听
//         /// </summary>
//         private void SetupEventListeners()
//         {
//             // 分类按钮事件
//             if (categoryButtons != null)
//             {
//                 for (int i = 0; i < categoryButtons.Length; i++)
//                 {
//                     int categoryIndex = i; // 闭包变量
//                     if (categoryButtons[i] != null)
//                     {
//                         categoryButtons[i].onClick.AddListener(() => OnCategoryClick(categoryIndex));
//                     }
//                 }
//             }
//
//             // 音频设置事件
//             if (masterVolumeSlider != null)
//                 masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
//
//             if (musicVolumeSlider != null)
//                 musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
//
//             if (sfxVolumeSlider != null)
//                 sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
//
//             if (muteToggle != null)
//                 muteToggle.onValueChanged.AddListener(OnMuteToggleChanged);
//
//             // 画质设置事件
//             if (qualityDropdown != null)
//                 qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
//
//             if (resolutionDropdown != null)
//                 resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
//
//             if (fullscreenToggle != null)
//                 fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);
//
//             if (vsyncToggle != null)
//                 vsyncToggle.onValueChanged.AddListener(OnVSyncToggleChanged);
//
//             if (fpsLimitSlider != null)
//                 fpsLimitSlider.onValueChanged.AddListener(OnFPSLimitChanged);
//
//             // 游戏设置事件
//             if (languageDropdown != null)
//                 languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
//
//             if (mouseSensitivitySlider != null)
//                 mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
//
//             if (autoSaveToggle != null)
//                 autoSaveToggle.onValueChanged.AddListener(OnAutoSaveToggleChanged);
//
//             if (autoSaveIntervalSlider != null)
//                 autoSaveIntervalSlider.onValueChanged.AddListener(OnAutoSaveIntervalChanged);
//
//             // 控制设置事件
//             if (keyBindingButtons != null)
//             {
//                 for (int i = 0; i < keyBindingButtons.Length; i++)
//                 {
//                     int keyIndex = i; // 闭包变量
//                     if (keyBindingButtons[i] != null)
//                     {
//                         keyBindingButtons[i].onClick.AddListener(() => OnKeyBindingClick(keyIndex));
//                     }
//                 }
//             }
//
//             if (invertYToggle != null)
//                 invertYToggle.onValueChanged.AddListener(OnInvertYToggleChanged);
//
//             if (deadZoneSlider != null)
//                 deadZoneSlider.onValueChanged.AddListener(OnDeadZoneChanged);
//
//             // 操作按钮事件
//             if (applyButton != null)
//                 applyButton.onClick.AddListener(OnApplyClick);
//
//             if (resetButton != null)
//                 resetButton.onClick.AddListener(OnResetClick);
//
//             if (defaultButton != null)
//                 defaultButton.onClick.AddListener(OnDefaultClick);
//
//             if (backButton != null)
//                 backButton.onClick.AddListener(OnBackClick);
//         }
//
//         #endregion
//
//         #region 设置数据管理
//
//         /// <summary>
//         /// 加载设置
//         /// </summary>
//         private void LoadSettings()
//         {
//             currentSettings = new SettingsData();
//             currentSettings.LoadFromPlayerPrefs();
//
//             // 备份原始设置
//             originalSettings = currentSettings.Clone();
//
//             Debug.Log("[设置状态] 设置数据已加载");
//         }
//
//         /// <summary>
//         /// 保存设置
//         /// </summary>
//         private void SaveSettings()
//         {
//             if (currentSettings == null)
//                 return;
//
//             currentSettings.SaveToPlayerPrefs();
//             ApplySettings();
//
//             // 更新备份
//             originalSettings = currentSettings.Clone();
//
//             Debug.Log("[设置状态] 设置数据已保存");
//         }
//
//         /// <summary>
//         /// 应用设置
//         /// </summary>
//         private void ApplySettings()
//         {
//             if (currentSettings == null)
//                 return;
//
//             // 应用音频设置
//             AudioListener.volume = currentSettings.IsMuted ? 0f : currentSettings.MasterVolume;
//
//             // 应用画质设置
//             QualitySettings.SetQualityLevel(currentSettings.QualityLevel);
//             Screen.SetResolution(currentSettings.ResolutionWidth, currentSettings.ResolutionHeight,
//                 currentSettings.IsFullscreen);
//             QualitySettings.vSyncCount = currentSettings.IsVSyncEnabled ? 1 : 0;
//             Application.targetFrameRate = currentSettings.FPSLimit;
//
//             // 应用游戏设置
//             // 这里可以设置语言、鼠标灵敏度等
//
//             Debug.Log("[设置状态] 设置已应用");
//         }
//
//         /// <summary>
//         /// 重置设置
//         /// </summary>
//         private void ResetSettings()
//         {
//             if (originalSettings == null)
//                 return;
//
//             currentSettings = originalSettings.Clone();
//             UpdateUIFromSettings();
//
//             Debug.Log("[设置状态] 设置已重置");
//         }
//
//         /// <summary>
//         /// 恢复默认设置
//         /// </summary>
//         private void RestoreDefaultSettings()
//         {
//             currentSettings = new SettingsData(); // 使用默认值
//             UpdateUIFromSettings();
//
//             Debug.Log("[设置状态] 已恢复默认设置");
//         }
//
//         #endregion
//
//         #region UI更新
//
//         /// <summary>
//         /// 从设置更新UI
//         /// </summary>
//         private void UpdateUIFromSettings()
//         {
//             if (currentSettings == null)
//                 return;
//
//             // 更新音频设置UI
//             UpdateAudioUI();
//
//             // 更新画质设置UI
//             UpdateGraphicsUI();
//
//             // 更新游戏设置UI
//             UpdateGameplayUI();
//
//             // 更新控制设置UI
//             UpdateControlsUI();
//         }
//
//         /// <summary>
//         /// 更新音频UI
//         /// </summary>
//         private void UpdateAudioUI()
//         {
//             if (masterVolumeSlider != null)
//                 masterVolumeSlider.value = currentSettings.MasterVolume;
//
//             if (musicVolumeSlider != null)
//                 musicVolumeSlider.value = currentSettings.MusicVolume;
//
//             if (sfxVolumeSlider != null)
//                 sfxVolumeSlider.value = currentSettings.SFXVolume;
//
//             if (muteToggle != null)
//                 muteToggle.isOn = currentSettings.IsMuted;
//
//             UpdateVolumeTexts();
//         }
//
//         /// <summary>
//         /// 更新画质UI
//         /// </summary>
//         private void UpdateGraphicsUI()
//         {
//             if (qualityDropdown != null)
//                 qualityDropdown.value = currentSettings.QualityLevel;
//
//             if (resolutionDropdown != null)
//             {
//                 // 查找匹配的分辨率索引
//                 for (int i = 0; i < availableResolutions.Length; i++)
//                 {
//                     if (availableResolutions[i].width == currentSettings.ResolutionWidth &&
//                         availableResolutions[i].height == currentSettings.ResolutionHeight)
//                     {
//                         resolutionDropdown.value = i;
//                         break;
//                     }
//                 }
//             }
//
//             if (fullscreenToggle != null)
//                 fullscreenToggle.isOn = currentSettings.IsFullscreen;
//
//             if (vsyncToggle != null)
//                 vsyncToggle.isOn = currentSettings.IsVSyncEnabled;
//
//             if (fpsLimitSlider != null)
//                 fpsLimitSlider.value = currentSettings.FPSLimit;
//
//             UpdateFPSLimitText();
//         }
//
//         /// <summary>
//         /// 更新游戏设置UI
//         /// </summary>
//         private void UpdateGameplayUI()
//         {
//             if (languageDropdown != null)
//                 languageDropdown.value = currentSettings.LanguageIndex;
//
//             if (mouseSensitivitySlider != null)
//                 mouseSensitivitySlider.value = currentSettings.MouseSensitivity;
//
//             if (autoSaveToggle != null)
//                 autoSaveToggle.isOn = currentSettings.IsAutoSaveEnabled;
//
//             if (autoSaveIntervalSlider != null)
//                 autoSaveIntervalSlider.value = currentSettings.AutoSaveInterval;
//
//             UpdateMouseSensitivityText();
//             UpdateAutoSaveIntervalText();
//         }
//
//         /// <summary>
//         /// 更新控制设置UI
//         /// </summary>
//         private void UpdateControlsUI()
//         {
//             if (keyBindingTexts != null && currentSettings.KeyBindings != null)
//             {
//                 for (int i = 0; i < keyBindingTexts.Length && i < currentSettings.KeyBindings.Length; i++)
//                 {
//                     if (keyBindingTexts[i] != null)
//                     {
//                         keyBindingTexts[i].text = currentSettings.KeyBindings[i].ToString();
//                     }
//                 }
//             }
//
//             if (invertYToggle != null)
//                 invertYToggle.isOn = currentSettings.IsInvertY;
//
//             if (deadZoneSlider != null)
//                 deadZoneSlider.value = currentSettings.DeadZone;
//
//             UpdateDeadZoneText();
//         }
//
//         /// <summary>
//         /// 更新音量文本
//         /// </summary>
//         private void UpdateVolumeTexts()
//         {
//             if (masterVolumeText != null)
//                 masterVolumeText.text = Mathf.RoundToInt(currentSettings.MasterVolume * 100) + "%";
//
//             if (musicVolumeText != null)
//                 musicVolumeText.text = Mathf.RoundToInt(currentSettings.MusicVolume * 100) + "%";
//
//             if (sfxVolumeText != null)
//                 sfxVolumeText.text = Mathf.RoundToInt(currentSettings.SFXVolume * 100) + "%";
//         }
//
//         /// <summary>
//         /// 更新FPS限制文本
//         /// </summary>
//         private void UpdateFPSLimitText()
//         {
//             if (fpsLimitText != null)
//             {
//                 if (currentSettings.FPSLimit <= 0)
//                     fpsLimitText.text = "无限制";
//                 else
//                     fpsLimitText.text = currentSettings.FPSLimit.ToString();
//             }
//         }
//
//         /// <summary>
//         /// 更新鼠标灵敏度文本
//         /// </summary>
//         private void UpdateMouseSensitivityText()
//         {
//             if (mouseSensitivityText != null)
//                 mouseSensitivityText.text = currentSettings.MouseSensitivity.ToString("F2");
//         }
//
//         /// <summary>
//         /// 更新自动保存间隔文本
//         /// </summary>
//         private void UpdateAutoSaveIntervalText()
//         {
//             if (autoSaveIntervalText != null)
//                 autoSaveIntervalText.text = currentSettings.AutoSaveInterval + "分钟";
//         }
//
//         /// <summary>
//         /// 更新死区文本
//         /// </summary>
//         private void UpdateDeadZoneText()
//         {
//             if (deadZoneText != null)
//                 deadZoneText.text = currentSettings.DeadZone.ToString("F2");
//         }
//
//         #endregion
//
//         #region 分类管理
//
//         /// <summary>
//         /// 显示指定分类
//         /// </summary>
//         /// <param name="categoryIndex">分类索引</param>
//         private void ShowCategory(int categoryIndex)
//         {
//             if (categoryIndex < 0 || categoryIndex >= categoryNames.Length)
//                 return;
//
//             currentCategoryIndex = categoryIndex;
//
//             // 更新分类按钮状态
//             UpdateCategoryButtons();
//
//             // 显示对应的面板
//             ShowCategoryPanel(categoryIndex);
//
//             Debug.Log($"[设置状态] 显示分类: {categoryNames[categoryIndex]}");
//         }
//
//         /// <summary>
//         /// 更新分类按钮状态
//         /// </summary>
//         private void UpdateCategoryButtons()
//         {
//             if (categoryButtons == null)
//                 return;
//
//             for (int i = 0; i < categoryButtons.Length; i++)
//             {
//                 if (categoryButtons[i] != null)
//                 {
//                     // 设置按钮颜色或状态来表示选中状态
//                     var colors = categoryButtons[i].colors;
//                     colors.normalColor = (i == currentCategoryIndex) ? Color.yellow : Color.white;
//                     categoryButtons[i].colors = colors;
//                 }
//             }
//         }
//
//         /// <summary>
//         /// 显示分类面板
//         /// </summary>
//         /// <param name="categoryIndex">分类索引</param>
//         private void ShowCategoryPanel(int categoryIndex)
//         {
//             if (categoryPanels == null)
//                 return;
//
//             // 隐藏所有面板
//             for (int i = 0; i < categoryPanels.Length; i++)
//             {
//                 if (categoryPanels[i] != null)
//                 {
//                     categoryPanels[i].SetActive(false);
//                 }
//             }
//
//             // 显示指定面板
//             if (categoryIndex < categoryPanels.Length && categoryPanels[categoryIndex] != null)
//             {
//                 categoryPanels[categoryIndex].SetActive(true);
//             }
//         }
//
//         #endregion
//
//         #region 初始化选项
//
//         /// <summary>
//         /// 初始化分辨率选项
//         /// </summary>
//         private void InitializeResolutionOptions()
//         {
//             availableResolutions = Screen.resolutions;
//             resolutionOptions.Clear();
//
//             foreach (var resolution in availableResolutions)
//             {
//                 string option = $"{resolution.width} x {resolution.height}";
//                 if (resolution.refreshRate > 0)
//                     option += $" @ {resolution.refreshRate}Hz";
//
//                 resolutionOptions.Add(option);
//             }
//
//             if (resolutionDropdown != null)
//             {
//                 resolutionDropdown.ClearOptions();
//                 resolutionDropdown.AddOptions(resolutionOptions);
//             }
//         }
//
//         /// <summary>
//         /// 初始化下拉框选项
//         /// </summary>
//         private void InitializeDropdownOptions()
//         {
//             // 初始化画质选项
//             if (qualityDropdown != null)
//             {
//                 qualityDropdown.ClearOptions();
//                 var qualityOptions = new List<string>();
//                 for (int i = 0; i < QualitySettings.names.Length; i++)
//                 {
//                     qualityOptions.Add(QualitySettings.names[i]);
//                 }
//
//                 qualityDropdown.AddOptions(qualityOptions);
//             }
//
//             // 初始化语言选项
//             if (languageDropdown != null)
//             {
//                 languageDropdown.ClearOptions();
//                 languageDropdown.AddOptions(new List<string>(languageOptions));
//             }
//         }
//
//         #endregion
//
//         #region 事件处理
//
//         /// <summary>
//         /// 分类按钮点击
//         /// </summary>
//         /// <param name="categoryIndex">分类索引</param>
//         private void OnCategoryClick(int categoryIndex)
//         {
//             Debug.Log($"[设置状态] 点击分类: {categoryNames[categoryIndex]}");
//             ShowCategory(categoryIndex);
//         }
//
//         /// <summary>
//         /// 主音量改变
//         /// </summary>
//         /// <param name="value">音量值</param>
//         private void OnMasterVolumeChanged(float value)
//         {
//             currentSettings.MasterVolume = value;
//             UpdateVolumeTexts();
//         }
//
//         /// <summary>
//         /// 音乐音量改变
//         /// </summary>
//         /// <param name="value">音量值</param>
//         private void OnMusicVolumeChanged(float value)
//         {
//             currentSettings.MusicVolume = value;
//             UpdateVolumeTexts();
//         }
//
//         /// <summary>
//         /// 音效音量改变
//         /// </summary>
//         /// <param name="value">音量值</param>
//         private void OnSFXVolumeChanged(float value)
//         {
//             currentSettings.SFXVolume = value;
//             UpdateVolumeTexts();
//         }
//
//         /// <summary>
//         /// 静音开关改变
//         /// </summary>
//         /// <param name="isOn">是否静音</param>
//         private void OnMuteToggleChanged(bool isOn)
//         {
//             currentSettings.IsMuted = isOn;
//         }
//
//         /// <summary>
//         /// 画质等级改变
//         /// </summary>
//         /// <param name="value">画质等级</param>
//         private void OnQualityChanged(int value)
//         {
//             currentSettings.QualityLevel = value;
//         }
//
//         /// <summary>
//         /// 分辨率改变
//         /// </summary>
//         /// <param name="value">分辨率索引</param>
//         private void OnResolutionChanged(int value)
//         {
//             if (value < availableResolutions.Length)
//             {
//                 currentSettings.ResolutionWidth = availableResolutions[value].width;
//                 currentSettings.ResolutionHeight = availableResolutions[value].height;
//             }
//         }
//
//         /// <summary>
//         /// 全屏开关改变
//         /// </summary>
//         /// <param name="isOn">是否全屏</param>
//         private void OnFullscreenToggleChanged(bool isOn)
//         {
//             currentSettings.IsFullscreen = isOn;
//         }
//
//         /// <summary>
//         /// 垂直同步开关改变
//         /// </summary>
//         /// <param name="isOn">是否启用垂直同步</param>
//         private void OnVSyncToggleChanged(bool isOn)
//         {
//             currentSettings.IsVSyncEnabled = isOn;
//         }
//
//         /// <summary>
//         /// FPS限制改变
//         /// </summary>
//         /// <param name="value">FPS限制值</param>
//         private void OnFPSLimitChanged(float value)
//         {
//             currentSettings.FPSLimit = Mathf.RoundToInt(value);
//             UpdateFPSLimitText();
//         }
//
//         /// <summary>
//         /// 语言改变
//         /// </summary>
//         /// <param name="value">语言索引</param>
//         private void OnLanguageChanged(int value)
//         {
//             currentSettings.LanguageIndex = value;
//         }
//
//         /// <summary>
//         /// 鼠标灵敏度改变
//         /// </summary>
//         /// <param name="value">灵敏度值</param>
//         private void OnMouseSensitivityChanged(float value)
//         {
//             currentSettings.MouseSensitivity = value;
//             UpdateMouseSensitivityText();
//         }
//
//         /// <summary>
//         /// 自动保存开关改变
//         /// </summary>
//         /// <param name="isOn">是否启用自动保存</param>
//         private void OnAutoSaveToggleChanged(bool isOn)
//         {
//             currentSettings.IsAutoSaveEnabled = isOn;
//         }
//
//         /// <summary>
//         /// 自动保存间隔改变
//         /// </summary>
//         /// <param name="value">间隔值</param>
//         private void OnAutoSaveIntervalChanged(float value)
//         {
//             currentSettings.AutoSaveInterval = Mathf.RoundToInt(value);
//             UpdateAutoSaveIntervalText();
//         }
//
//         /// <summary>
//         /// 按键绑定按钮点击
//         /// </summary>
//         /// <param name="keyIndex">按键索引</param>
//         private void OnKeyBindingClick(int keyIndex)
//         {
//             Debug.Log($"[设置状态] 设置按键绑定: {keyBindingNames[keyIndex]}");
//
//             isWaitingForKeyInput = true;
//             keyBindingIndex = keyIndex;
//
//             // 更新按钮文本提示
//             if (keyBindingTexts != null && keyIndex < keyBindingTexts.Length && keyBindingTexts[keyIndex] != null)
//             {
//                 keyBindingTexts[keyIndex].text = "按任意键...";
//             }
//         }
//
//         /// <summary>
//         /// Y轴反转开关改变
//         /// </summary>
//         /// <param name="isOn">是否反转Y轴</param>
//         private void OnInvertYToggleChanged(bool isOn)
//         {
//             currentSettings.IsInvertY = isOn;
//         }
//
//         /// <summary>
//         /// 死区改变
//         /// </summary>
//         /// <param name="value">死区值</param>
//         private void OnDeadZoneChanged(float value)
//         {
//             currentSettings.DeadZone = value;
//             UpdateDeadZoneText();
//         }
//
//         /// <summary>
//         /// 应用按钮点击
//         /// </summary>
//         private void OnApplyClick()
//         {
//             Debug.Log("[设置状态] 点击应用");
//             SaveSettings();
//         }
//
//         /// <summary>
//         /// 重置按钮点击
//         /// </summary>
//         private void OnResetClick()
//         {
//             Debug.Log("[设置状态] 点击重置");
//             ResetSettings();
//         }
//
//         /// <summary>
//         /// 默认按钮点击
//         /// </summary>
//         private void OnDefaultClick()
//         {
//             Debug.Log("[设置状态] 点击恢复默认");
//             RestoreDefaultSettings();
//         }
//
//         /// <summary>
//         /// 返回按钮点击
//         /// </summary>
//         private void OnBackClick()
//         {
//             Debug.Log("[设置状态] 点击返回");
//
//             // 返回上一个状态
//             if (stateMachine != null)
//             {
//                 stateMachine.GoBack();
//             }
//         }
//
//         #endregion
//
//         #region 按键绑定
//
//         /// <summary>
//         /// 检查按键绑定输入
//         /// </summary>
//         private void CheckKeyBindingInput()
//         {
//             if (!isWaitingForKeyInput || keyBindingIndex < 0)
//                 return;
//
//             // 检查所有按键
//             foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
//             {
//                 if (Input.GetKeyDown(keyCode))
//                 {
//                     // 设置新的按键绑定
//                     if (currentSettings.KeyBindings != null && keyBindingIndex < currentSettings.KeyBindings.Length)
//                     {
//                         currentSettings.KeyBindings[keyBindingIndex] = keyCode;
//
//                         // 更新UI显示
//                         if (keyBindingTexts != null && keyBindingIndex < keyBindingTexts.Length &&
//                             keyBindingTexts[keyBindingIndex] != null)
//                         {
//                             keyBindingTexts[keyBindingIndex].text = keyCode.ToString();
//                         }
//
//                         Debug.Log($"[设置状态] 设置按键绑定: {keyBindingNames[keyBindingIndex]} = {keyCode}");
//                     }
//
//                     // 结束按键绑定模式
//                     isWaitingForKeyInput = false;
//                     keyBindingIndex = -1;
//                     break;
//                 }
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
//             switch (inputEvent.EventType)
//             {
//                 case UIInputEventType.KeyDown:
//                     HandleKeyInput(inputEvent.KeyCode);
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
//             // 如果正在等待按键绑定输入，不处理其他按键
//             if (isWaitingForKeyInput)
//                 return;
//
//             switch (keyCode)
//             {
//                 case KeyCode.Escape:
//                     OnBackClick();
//                     break;
//
//                 case KeyCode.Tab:
//                     // 切换到下一个分类
//                     int nextCategory = (currentCategoryIndex + 1) % categoryNames.Length;
//                     ShowCategory(nextCategory);
//                     break;
//             }
//         }
//
//         #endregion
//
//         #region 动画
//
//         /// <summary>
//         /// 播放进入动画
//         /// </summary>
//         private void PlayEnterAnimation()
//         {
//             if (uiGameObject == null)
//                 return;
//
//             // 简单的淡入动画
//             var canvasGroup = uiGameObject.GetComponent<CanvasGroup>();
//             if (canvasGroup == null)
//                 canvasGroup = uiGameObject.AddComponent<CanvasGroup>();
//
//             canvasGroup.alpha = 0f;
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
//             float duration = 0.3f;
//             float elapsed = 0f;
//
//             while (elapsed < duration)
//             {
//                 elapsed += Time.unscaledDeltaTime;
//                 canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
//                 yield return null;
//             }
//
//             canvasGroup.alpha = 1f;
//         }
//
//         /// <summary>
//         /// 淡出协程
//         /// </summary>
//         private System.Collections.IEnumerator FadeOutCoroutine(CanvasGroup canvasGroup)
//         {
//             float duration = 0.2f;
//             float elapsed = 0f;
//             float startAlpha = canvasGroup.alpha;
//
//             while (elapsed < duration)
//             {
//                 elapsed += Time.unscaledDeltaTime;
//                 canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
//                 yield return null;
//             }
//
//             canvasGroup.alpha = 0f;
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
//             // 移除所有事件监听
//             RemoveEventListeners();
//
//             // 重置状态
//             isWaitingForKeyInput = false;
//             keyBindingIndex = -1;
//         }
//
//         /// <summary>
//         /// 移除事件监听
//         /// </summary>
//         private void RemoveEventListeners()
//         {
//             // 移除分类按钮事件
//             if (categoryButtons != null)
//             {
//                 foreach (var button in categoryButtons)
//                 {
//                     if (button != null)
//                         button.onClick.RemoveAllListeners();
//                 }
//             }
//
//             // 移除音频设置事件
//             if (masterVolumeSlider != null)
//                 masterVolumeSlider.onValueChanged.RemoveAllListeners();
//
//             if (musicVolumeSlider != null)
//                 musicVolumeSlider.onValueChanged.RemoveAllListeners();
//
//             if (sfxVolumeSlider != null)
//                 sfxVolumeSlider.onValueChanged.RemoveAllListeners();
//
//             if (muteToggle != null)
//                 muteToggle.onValueChanged.RemoveAllListeners();
//
//             // 移除画质设置事件
//             if (qualityDropdown != null)
//                 qualityDropdown.onValueChanged.RemoveAllListeners();
//
//             if (resolutionDropdown != null)
//                 resolutionDropdown.onValueChanged.RemoveAllListeners();
//
//             if (fullscreenToggle != null)
//                 fullscreenToggle.onValueChanged.RemoveAllListeners();
//
//             if (vsyncToggle != null)
//                 vsyncToggle.onValueChanged.RemoveAllListeners();
//
//             if (fpsLimitSlider != null)
//                 fpsLimitSlider.onValueChanged.RemoveAllListeners();
//
//             // 移除游戏设置事件
//             if (languageDropdown != null)
//                 languageDropdown.onValueChanged.RemoveAllListeners();
//
//             if (mouseSensitivitySlider != null)
//                 mouseSensitivitySlider.onValueChanged.RemoveAllListeners();
//
//             if (autoSaveToggle != null)
//                 autoSaveToggle.onValueChanged.RemoveAllListeners();
//
//             if (autoSaveIntervalSlider != null)
//                 autoSaveIntervalSlider.onValueChanged.RemoveAllListeners();
//
//             // 移除控制设置事件
//             if (keyBindingButtons != null)
//             {
//                 foreach (var button in keyBindingButtons)
//                 {
//                     if (button != null)
//                         button.onClick.RemoveAllListeners();
//                 }
//             }
//
//             if (invertYToggle != null)
//                 invertYToggle.onValueChanged.RemoveAllListeners();
//
//             if (deadZoneSlider != null)
//                 deadZoneSlider.onValueChanged.RemoveAllListeners();
//
//             // 移除操作按钮事件
//             if (applyButton != null)
//                 applyButton.onClick.RemoveAllListeners();
//
//             if (resetButton != null)
//                 resetButton.onClick.RemoveAllListeners();
//
//             if (defaultButton != null)
//                 defaultButton.onClick.RemoveAllListeners();
//
//             if (backButton != null)
//                 backButton.onClick.RemoveAllListeners();
//         }
//
//         #endregion
//
//         #region 状态转换检查
//
//         public override bool CanTransitionTo(string targetStateName)
//         {
//             // 设置状态可以转换到大部分状态
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
//             var settingsStateData = new SettingsStateData
//             {
//                 BaseData = baseData,
//                 CurrentSettings = currentSettings,
//                 OriginalSettings = originalSettings,
//                 CurrentCategoryIndex = currentCategoryIndex,
//                 IsWaitingForKeyInput = isWaitingForKeyInput,
//                 KeyBindingIndex = keyBindingIndex
//             };
//
//             return settingsStateData;
//         }
//
//         #endregion
//     }
//
//     /// <summary>
//     /// 设置数据
//     /// </summary>
//     [System.Serializable]
//     public class SettingsData
//     {
//         [Header("音频设置")] public float MasterVolume = 1f;
//         public float MusicVolume = 0.8f;
//         public float SFXVolume = 1f;
//         public bool IsMuted = false;
//
//         [Header("画质设置")] public int QualityLevel = 2;
//         public int ResolutionWidth = 1920;
//         public int ResolutionHeight = 1080;
//         public bool IsFullscreen = true;
//         public bool IsVSyncEnabled = true;
//         public int FPSLimit = 60;
//
//         [Header("游戏设置")] public int LanguageIndex = 0;
//         public float MouseSensitivity = 1f;
//         public bool IsAutoSaveEnabled = true;
//         public int AutoSaveInterval = 5;
//
//         [Header("控制设置")] public KeyCode[] KeyBindings =
//         {
//             KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D,
//             KeyCode.Space, KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.E
//         };
//
//         public bool IsInvertY = false;
//         public float DeadZone = 0.1f;
//
//         /// <summary>
//         /// 从PlayerPrefs加载设置
//         /// </summary>
//         public void LoadFromPlayerPrefs()
//         {
//             MasterVolume = PlayerPrefs.GetFloat("Settings_MasterVolume", 1f);
//             MusicVolume = PlayerPrefs.GetFloat("Settings_MusicVolume", 0.8f);
//             SFXVolume = PlayerPrefs.GetFloat("Settings_SFXVolume", 1f);
//             IsMuted = PlayerPrefs.GetInt("Settings_IsMuted", 0) == 1;
//
//             QualityLevel = PlayerPrefs.GetInt("Settings_QualityLevel", 2);
//             ResolutionWidth = PlayerPrefs.GetInt("Settings_ResolutionWidth", 1920);
//             ResolutionHeight = PlayerPrefs.GetInt("Settings_ResolutionHeight", 1080);
//             IsFullscreen = PlayerPrefs.GetInt("Settings_IsFullscreen", 1) == 1;
//             IsVSyncEnabled = PlayerPrefs.GetInt("Settings_IsVSyncEnabled", 1) == 1;
//             FPSLimit = PlayerPrefs.GetInt("Settings_FPSLimit", 60);
//
//             LanguageIndex = PlayerPrefs.GetInt("Settings_LanguageIndex", 0);
//             MouseSensitivity = PlayerPrefs.GetFloat("Settings_MouseSensitivity", 1f);
//             IsAutoSaveEnabled = PlayerPrefs.GetInt("Settings_IsAutoSaveEnabled", 1) == 1;
//             AutoSaveInterval = PlayerPrefs.GetInt("Settings_AutoSaveInterval", 5);
//
//             IsInvertY = PlayerPrefs.GetInt("Settings_IsInvertY", 0) == 1;
//             DeadZone = PlayerPrefs.GetFloat("Settings_DeadZone", 0.1f);
//
//             // 加载按键绑定
//             for (int i = 0; i < KeyBindings.Length; i++)
//             {
//                 string keyName = PlayerPrefs.GetString($"Settings_KeyBinding_{i}", KeyBindings[i].ToString());
//                 if (System.Enum.TryParse(keyName, out KeyCode keyCode))
//                 {
//                     KeyBindings[i] = keyCode;
//                 }
//             }
//         }
//
//         /// <summary>
//         /// 保存设置到PlayerPrefs
//         /// </summary>
//         public void SaveToPlayerPrefs()
//         {
//             PlayerPrefs.SetFloat("Settings_MasterVolume", MasterVolume);
//             PlayerPrefs.SetFloat("Settings_MusicVolume", MusicVolume);
//             PlayerPrefs.SetFloat("Settings_SFXVolume", SFXVolume);
//             PlayerPrefs.SetInt("Settings_IsMuted", IsMuted ? 1 : 0);
//
//             PlayerPrefs.SetInt("Settings_QualityLevel", QualityLevel);
//             PlayerPrefs.SetInt("Settings_ResolutionWidth", ResolutionWidth);
//             PlayerPrefs.SetInt("Settings_ResolutionHeight", ResolutionHeight);
//             PlayerPrefs.SetInt("Settings_IsFullscreen", IsFullscreen ? 1 : 0);
//             PlayerPrefs.SetInt("Settings_IsVSyncEnabled", IsVSyncEnabled ? 1 : 0);
//             PlayerPrefs.SetInt("Settings_FPSLimit", FPSLimit);
//
//             PlayerPrefs.SetInt("Settings_LanguageIndex", LanguageIndex);
//             PlayerPrefs.SetFloat("Settings_MouseSensitivity", MouseSensitivity);
//             PlayerPrefs.SetInt("Settings_IsAutoSaveEnabled", IsAutoSaveEnabled ? 1 : 0);
//             PlayerPrefs.SetInt("Settings_AutoSaveInterval", AutoSaveInterval);
//
//             PlayerPrefs.SetInt("Settings_IsInvertY", IsInvertY ? 1 : 0);
//             PlayerPrefs.SetFloat("Settings_DeadZone", DeadZone);
//
//             // 保存按键绑定
//             for (int i = 0; i < KeyBindings.Length; i++)
//             {
//                 PlayerPrefs.SetString($"Settings_KeyBinding_{i}", KeyBindings[i].ToString());
//             }
//
//             PlayerPrefs.Save();
//         }
//
//         /// <summary>
//         /// 克隆设置数据
//         /// </summary>
//         /// <returns>克隆的设置数据</returns>
//         public SettingsData Clone()
//         {
//             return null;
//         }
//     }
// }