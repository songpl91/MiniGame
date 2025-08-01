using UnityEngine;
using UnityEngine.UI;

namespace Framework.TraditionalUI.Panels
{
    /// <summary>
    /// 设置面板
    /// 游戏设置界面，包含音效、画质、语言等设置
    /// </summary>
    public class SettingsPanel : TraditionalUIPanel
    {
        #region UI组件引用
        
        [Header("音频设置")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Text masterVolumeText;
        [SerializeField] private Text musicVolumeText;
        [SerializeField] private Text sfxVolumeText;
        [SerializeField] private Toggle muteToggle;
        
        [Header("画质设置")]
        [SerializeField] private Dropdown qualityDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Dropdown resolutionDropdown;
        [SerializeField] private Toggle vsyncToggle;
        
        [Header("游戏设置")]
        [SerializeField] private Dropdown languageDropdown;
        [SerializeField] private Toggle autoSaveToggle;
        [SerializeField] private Slider mouseSensitivitySlider;
        [SerializeField] private Text mouseSensitivityText;
        
        [Header("按钮")]
        [SerializeField] private Button resetButton;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button backButton;
        
        #endregion
        
        #region 私有变量
        
        private SettingsData originalSettings;
        private SettingsData currentSettings;
        
        #endregion
        
        #region 初始化方法
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // 设置按钮事件
            SetupButtonEvents();
            
            // 设置滑动条事件
            SetupSliderEvents();
            
            // 设置下拉框事件
            SetupDropdownEvents();
            
            // 设置开关事件
            SetupToggleEvents();
            
            // 初始化下拉框选项
            InitializeDropdowns();
        }
        
        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtonEvents()
        {
            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetClick);
            
            if (applyButton != null)
                applyButton.onClick.AddListener(OnApplyClick);
            
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClick);
            
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClick);
        }
        
        /// <summary>
        /// 设置滑动条事件
        /// </summary>
        private void SetupSliderEvents()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            
            if (mouseSensitivitySlider != null)
                mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        }
        
        /// <summary>
        /// 设置下拉框事件
        /// </summary>
        private void SetupDropdownEvents()
        {
            if (qualityDropdown != null)
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            
            if (resolutionDropdown != null)
                resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            
            if (languageDropdown != null)
                languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }
        
        /// <summary>
        /// 设置开关事件
        /// </summary>
        private void SetupToggleEvents()
        {
            if (muteToggle != null)
                muteToggle.onValueChanged.AddListener(OnMuteToggleChanged);
            
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);
            
            if (vsyncToggle != null)
                vsyncToggle.onValueChanged.AddListener(OnVsyncToggleChanged);
            
            if (autoSaveToggle != null)
                autoSaveToggle.onValueChanged.AddListener(OnAutoSaveToggleChanged);
        }
        
        /// <summary>
        /// 初始化下拉框选项
        /// </summary>
        private void InitializeDropdowns()
        {
            // 初始化画质下拉框
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "低", "中", "高", "极高"
                });
            }
            
            // 初始化分辨率下拉框
            if (resolutionDropdown != null)
            {
                resolutionDropdown.ClearOptions();
                resolutionDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "1280x720", "1920x1080", "2560x1440", "3840x2160"
                });
            }
            
            // 初始化语言下拉框
            if (languageDropdown != null)
            {
                languageDropdown.ClearOptions();
                languageDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "简体中文", "English", "日本語", "한국어"
                });
            }
        }
        
        #endregion
        
        #region 生命周期回调
        
        // protected override void OnBeforeShow()
        // {
        //     base.OnBeforeShow();
        //     
        //     // 加载当前设置
        //     LoadCurrentSettings();
        //     
        //     // 备份原始设置
        //     originalSettings = currentSettings.Clone();
        //     
        //     // 更新UI显示
        //     UpdateUI();
        // }
        
        #endregion
        
        #region 设置加载和保存
        
        /// <summary>
        /// 加载当前设置
        /// </summary>
        private void LoadCurrentSettings()
        {
            // 这里应该从设置管理器加载设置
            // currentSettings = SettingsManager.Instance.GetSettings();
            
            // 模拟数据
            currentSettings = new SettingsData
            {
                masterVolume = 0.8f,
                musicVolume = 0.7f,
                sfxVolume = 0.9f,
                isMuted = false,
                qualityLevel = 2,
                isFullscreen = true,
                resolutionIndex = 1,
                isVsyncEnabled = true,
                languageIndex = 0,
                isAutoSaveEnabled = true,
                mouseSensitivity = 0.5f
            };
        }
        
        /// <summary>
        /// 保存设置
        /// </summary>
        private void SaveSettings()
        {
            // 这里应该通过设置管理器保存设置
            // SettingsManager.Instance.SaveSettings(currentSettings);
            
            Debug.Log("[设置] 设置已保存");
        }
        
        /// <summary>
        /// 应用设置
        /// </summary>
        private void ApplySettings()
        {
            // 应用音频设置
            // AudioManager.Instance.SetMasterVolume(currentSettings.masterVolume);
            // AudioManager.Instance.SetMusicVolume(currentSettings.musicVolume);
            // AudioManager.Instance.SetSfxVolume(currentSettings.sfxVolume);
            // AudioManager.Instance.SetMute(currentSettings.isMuted);
            
            // 应用画质设置
            QualitySettings.SetQualityLevel(currentSettings.qualityLevel);
            Screen.fullScreen = currentSettings.isFullscreen;
            QualitySettings.vSyncCount = currentSettings.isVsyncEnabled ? 1 : 0;
            
            // 应用分辨率设置
            ApplyResolution();
            
            // 应用语言设置
            // LocalizationManager.Instance.SetLanguage(currentSettings.languageIndex);
            
            Debug.Log("[设置] 设置已应用");
        }
        
        /// <summary>
        /// 应用分辨率设置
        /// </summary>
        private void ApplyResolution()
        {
            string[] resolutions = { "1280x720", "1920x1080", "2560x1440", "3840x2160" };
            if (currentSettings.resolutionIndex >= 0 && currentSettings.resolutionIndex < resolutions.Length)
            {
                string resolution = resolutions[currentSettings.resolutionIndex];
                string[] parts = resolution.Split('x');
                if (parts.Length == 2)
                {
                    int width = int.Parse(parts[0]);
                    int height = int.Parse(parts[1]);
                    Screen.SetResolution(width, height, currentSettings.isFullscreen);
                }
            }
        }
        
        #endregion
        
        #region UI更新方法
        
        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUI()
        {
            // 更新音频设置UI
            UpdateAudioUI();
            
            // 更新画质设置UI
            UpdateGraphicsUI();
            
            // 更新游戏设置UI
            UpdateGameplayUI();
        }
        
        /// <summary>
        /// 更新音频设置UI
        /// </summary>
        private void UpdateAudioUI()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = currentSettings.masterVolume;
            
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = currentSettings.musicVolume;
            
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = currentSettings.sfxVolume;
            
            if (muteToggle != null)
                muteToggle.isOn = currentSettings.isMuted;
            
            UpdateVolumeTexts();
        }
        
        /// <summary>
        /// 更新画质设置UI
        /// </summary>
        private void UpdateGraphicsUI()
        {
            if (qualityDropdown != null)
                qualityDropdown.value = currentSettings.qualityLevel;
            
            if (fullscreenToggle != null)
                fullscreenToggle.isOn = currentSettings.isFullscreen;
            
            if (resolutionDropdown != null)
                resolutionDropdown.value = currentSettings.resolutionIndex;
            
            if (vsyncToggle != null)
                vsyncToggle.isOn = currentSettings.isVsyncEnabled;
        }
        
        /// <summary>
        /// 更新游戏设置UI
        /// </summary>
        private void UpdateGameplayUI()
        {
            if (languageDropdown != null)
                languageDropdown.value = currentSettings.languageIndex;
            
            if (autoSaveToggle != null)
                autoSaveToggle.isOn = currentSettings.isAutoSaveEnabled;
            
            if (mouseSensitivitySlider != null)
                mouseSensitivitySlider.value = currentSettings.mouseSensitivity;
            
            UpdateMouseSensitivityText();
        }
        
        /// <summary>
        /// 更新音量文本显示
        /// </summary>
        private void UpdateVolumeTexts()
        {
            if (masterVolumeText != null)
                masterVolumeText.text = $"{(int)(currentSettings.masterVolume * 100)}%";
            
            if (musicVolumeText != null)
                musicVolumeText.text = $"{(int)(currentSettings.musicVolume * 100)}%";
            
            if (sfxVolumeText != null)
                sfxVolumeText.text = $"{(int)(currentSettings.sfxVolume * 100)}%";
        }
        
        /// <summary>
        /// 更新鼠标灵敏度文本显示
        /// </summary>
        private void UpdateMouseSensitivityText()
        {
            if (mouseSensitivityText != null)
                mouseSensitivityText.text = $"{(int)(currentSettings.mouseSensitivity * 100)}%";
        }
        
        #endregion
        
        #region 事件处理方法
        
        // 音频设置事件
        private void OnMasterVolumeChanged(float value)
        {
            currentSettings.masterVolume = value;
            UpdateVolumeTexts();
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            currentSettings.musicVolume = value;
            UpdateVolumeTexts();
        }
        
        private void OnSfxVolumeChanged(float value)
        {
            currentSettings.sfxVolume = value;
            UpdateVolumeTexts();
        }
        
        private void OnMuteToggleChanged(bool value)
        {
            currentSettings.isMuted = value;
        }
        
        // 画质设置事件
        private void OnQualityChanged(int value)
        {
            currentSettings.qualityLevel = value;
        }
        
        private void OnFullscreenToggleChanged(bool value)
        {
            currentSettings.isFullscreen = value;
        }
        
        private void OnResolutionChanged(int value)
        {
            currentSettings.resolutionIndex = value;
        }
        
        private void OnVsyncToggleChanged(bool value)
        {
            currentSettings.isVsyncEnabled = value;
        }
        
        // 游戏设置事件
        private void OnLanguageChanged(int value)
        {
            currentSettings.languageIndex = value;
        }
        
        private void OnAutoSaveToggleChanged(bool value)
        {
            currentSettings.isAutoSaveEnabled = value;
        }
        
        private void OnMouseSensitivityChanged(float value)
        {
            currentSettings.mouseSensitivity = value;
            UpdateMouseSensitivityText();
        }
        
        // 按钮事件
        private void OnResetClick()
        {
            Debug.Log("[设置] 重置为默认设置");
            
            // TraditionalUIManager.Instance.OpenPanel("MessageBox", new MessageBoxData
            // {
            //     title = "重置设置",
            //     message = "确定要重置为默认设置吗？",
            //     showCancelButton = true,
            //     onConfirm = () => {
            //         ResetToDefault();
            //         UpdateUI();
            //     }
            // });
        }
        
        private void OnApplyClick()
        {
            Debug.Log("[设置] 应用设置");
            ApplySettings();
            SaveSettings();
            originalSettings = currentSettings.Clone();
        }
        
        private void OnCancelClick()
        {
            Debug.Log("[设置] 取消设置");
            currentSettings = originalSettings.Clone();
            UpdateUI();
        }
        
        private void OnBackClick()
        {
            Debug.Log("[设置] 返回");
            
            // 检查是否有未保存的更改
            if (!currentSettings.Equals(originalSettings))
            {
                // TraditionalUIManager.Instance.OpenPanel("MessageBox", new MessageBoxData
                // {
                //     title = "未保存的更改",
                //     message = "您有未保存的更改，是否要保存？",
                //     showCancelButton = true,
                //     onConfirm = () => {
                //         ApplySettings();
                //         SaveSettings();
                //         TraditionalUIManager.Instance.ClosePanel(this);
                //     },
                //     onCancel = () => {
                //         TraditionalUIManager.Instance.ClosePanel(this);
                //     }
                // });
            }
            else
            {
                // TraditionalUIManager.Instance.ClosePanel(this);
            }
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 重置为默认设置
        /// </summary>
        private void ResetToDefault()
        {
            currentSettings = new SettingsData
            {
                masterVolume = 1.0f,
                musicVolume = 0.8f,
                sfxVolume = 1.0f,
                isMuted = false,
                qualityLevel = 2,
                isFullscreen = true,
                resolutionIndex = 1,
                isVsyncEnabled = true,
                languageIndex = 0,
                isAutoSaveEnabled = true,
                mouseSensitivity = 0.5f
            };
        }
        
        #endregion
    }
    
    /// <summary>
    /// 设置数据类
    /// </summary>
    [System.Serializable]
    public class SettingsData
    {
        [Header("音频设置")]
        public float masterVolume = 1.0f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1.0f;
        public bool isMuted = false;
        
        [Header("画质设置")]
        public int qualityLevel = 2;
        public bool isFullscreen = true;
        public int resolutionIndex = 1;
        public bool isVsyncEnabled = true;
        
        [Header("游戏设置")]
        public int languageIndex = 0;
        public bool isAutoSaveEnabled = true;
        public float mouseSensitivity = 0.5f;
        
        /// <summary>
        /// 克隆设置数据
        /// </summary>
        public SettingsData Clone()
        {
            return new SettingsData
            {
                masterVolume = this.masterVolume,
                musicVolume = this.musicVolume,
                sfxVolume = this.sfxVolume,
                isMuted = this.isMuted,
                qualityLevel = this.qualityLevel,
                isFullscreen = this.isFullscreen,
                resolutionIndex = this.resolutionIndex,
                isVsyncEnabled = this.isVsyncEnabled,
                languageIndex = this.languageIndex,
                isAutoSaveEnabled = this.isAutoSaveEnabled,
                mouseSensitivity = this.mouseSensitivity
            };
        }
        
        /// <summary>
        /// 比较设置数据是否相等
        /// </summary>
        public bool Equals(SettingsData other)
        {
            if (other == null) return false;
            
            return masterVolume == other.masterVolume &&
                   musicVolume == other.musicVolume &&
                   sfxVolume == other.sfxVolume &&
                   isMuted == other.isMuted &&
                   qualityLevel == other.qualityLevel &&
                   isFullscreen == other.isFullscreen &&
                   resolutionIndex == other.resolutionIndex &&
                   isVsyncEnabled == other.isVsyncEnabled &&
                   languageIndex == other.languageIndex &&
                   isAutoSaveEnabled == other.isAutoSaveEnabled &&
                   mouseSensitivity == other.mouseSensitivity;
        }
    }
}