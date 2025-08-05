using UnityEngine;
using UnityEngine.UI;
using Framework.SampleUI.Core;
using Framework.SampleUI.Components;

namespace Framework.SampleUI.Examples
{
    /// <summary>
    /// 示例面板
    /// 展示SampleUI框架的基本使用方法
    /// </summary>
    public class ExamplePanel : SampleUIBase
    {
        #region UI组件引用
        
        [Header("UI组件")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button animationButton;
        [SerializeField] private Button audioButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text contentText;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Toggle muteToggle;
        
        #endregion
        
        #region 组件引用
        
        private AnimationComponent animationComponent;
        private AudioComponent audioComponent;
        private InputComponent inputComponent;
        
        #endregion
        
        #region 初始化
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            // 设置面板基本信息
            PanelId = "ExamplePanel";
            DisplayName = "示例面板";
            PanelType = SampleUIBaseType.Normal;
            Priority = 1;
            
            // 添加组件
            SetupComponents();
            
            // 绑定UI事件
            BindUIEvents();
            
            // 初始化UI
            InitializeUI();
        }
        
        /// <summary>
        /// 设置组件
        /// </summary>
        private void SetupComponents()
        {
            // 添加动画组件
            animationComponent = AddComponent<AnimationComponent>();
            if (animationComponent != null)
            {
                animationComponent.OnAnimationStart += OnAnimationStart;
                animationComponent.OnAnimationComplete += OnAnimationComplete;
            }
            
            // 添加音频组件
            audioComponent = AddComponent<AudioComponent>();
            if (audioComponent != null)
            {
                audioComponent.OnAudioStart += OnAudioStart;
                audioComponent.OnAudioEnd += OnAudioEnd;
            }
            
            // 添加输入组件
            inputComponent = AddComponent<InputComponent>();
            if (inputComponent != null)
            {
                inputComponent.OnMouseClick += OnPanelClick;
                inputComponent.OnKeyPressed += OnKeyPressed;
                
                // 注册快捷键
                inputComponent.RegisterKey(KeyCode.Space, OnSpacePressed);
                inputComponent.RegisterComboKey("Ctrl+A", OnCtrlAPressed);
            }
        }
        
        /// <summary>
        /// 绑定UI事件
        /// </summary>
        private void BindUIEvents()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClick);
            }
            
            if (animationButton != null)
            {
                animationButton.onClick.AddListener(OnAnimationButtonClick);
            }
            
            if (audioButton != null)
            {
                audioButton.onClick.AddListener(OnAudioButtonClick);
            }
            
            if (volumeSlider != null)
            {
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }
            
            if (muteToggle != null)
            {
                muteToggle.onValueChanged.AddListener(OnMuteToggleChanged);
            }
        }
        
        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitializeUI()
        {
            if (titleText != null)
            {
                titleText.text = DisplayName;
            }
            
            if (contentText != null)
            {
                contentText.text = "这是一个示例面板，展示了SampleUI框架的基本功能：\n\n" +
                                  "1. 面板生命周期管理\n" +
                                  "2. 组件系统扩展\n" +
                                  "3. 动画效果\n" +
                                  "4. 音效播放\n" +
                                  "5. 输入处理\n\n" +
                                  "快捷键：\n" +
                                  "ESC - 关闭面板\n" +
                                  "Space - 播放动画\n" +
                                  "Ctrl+A - 播放音效";
            }
            
            if (volumeSlider != null)
            {
                volumeSlider.value = audioComponent != null ? audioComponent.Volume : 1f;
            }
            
            if (muteToggle != null)
            {
                muteToggle.isOn = audioComponent != null ? audioComponent.IsMuted : false;
            }
        }
        
        #endregion
        
        #region 面板生命周期
        
        /// <summary>
        /// 面板显示时调用（重写基类方法）
        /// </summary>
        /// <param name="data">传递的数据，可为null</param>
        protected override void OnShow(object data = null)
        {
            base.OnShow(data);
            
            if (data != null)
            {
                Debug.Log($"[ExamplePanel] 面板显示（带数据）: {data}");
                // 根据传入的数据更新UI
                UpdateUIWithData(data);
            }
            else
            {
                Debug.Log("[ExamplePanel] 面板显示（无数据）");
            }
            
            // 播放打开音效
            if (audioComponent != null)
            {
                audioComponent.PlayOpenSound();
            }
            
            // 播放显示动画
            if (animationComponent != null)
            {
                animationComponent.PlayAnimation(CustomAnimationType.ScaleBounce, 0.5f);
            }
        }
        
        protected override void OnHide()
        {
            base.OnHide();
            
            Debug.Log("[ExamplePanel] 面板隐藏");
            
            // 播放关闭音效
            if (audioComponent != null)
            {
                audioComponent.PlayCloseSound();
            }
        }
        
        protected override void OnRefresh(object data)
        {
            base.OnRefresh(data);
            
            if (data != null)
            {
                Debug.Log($"[ExamplePanel] 面板刷新，收到数据: {data}");
                // 根据新数据更新UI
                UpdateUIWithData(data);
            }
            else
            {
                Debug.Log("[ExamplePanel] 面板刷新，无新数据");
            }
            
            // 更新UI显示
            UpdateUI();
        }
        
        #endregion
        
        #region UI事件处理
        
        /// <summary>
        /// 关闭按钮点击
        /// </summary>
        private void OnCloseButtonClick()
        {
            Debug.Log("[ExamplePanel] 关闭按钮点击");
            
            // 播放点击音效
            if (audioComponent != null)
            {
                audioComponent.PlayClickSound();
            }
            
            // 关闭面板
            Hide();
        }
        
        /// <summary>
        /// 动画按钮点击
        /// </summary>
        private void OnAnimationButtonClick()
        {
            Debug.Log("[ExamplePanel] 动画按钮点击");
            
            // 播放点击音效
            if (audioComponent != null)
            {
                audioComponent.PlayClickSound();
            }
            
            // 播放随机动画
            if (animationComponent != null)
            {
                var animationTypes = System.Enum.GetValues(typeof(CustomAnimationType));
                var randomType = (CustomAnimationType)animationTypes.GetValue(Random.Range(0, animationTypes.Length));
                animationComponent.PlayAnimation(randomType, 0.8f);
            }
        }
        
        /// <summary>
        /// 音频按钮点击
        /// </summary>
        private void OnAudioButtonClick()
        {
            Debug.Log("[ExamplePanel] 音频按钮点击");
            
            // 播放成功音效
            if (audioComponent != null)
            {
                audioComponent.PlaySuccessSound();
            }
        }
        
        /// <summary>
        /// 音量滑块变化
        /// </summary>
        /// <param name="value">音量值</param>
        private void OnVolumeChanged(float value)
        {
            if (audioComponent != null)
            {
                audioComponent.Volume = value;
            }
        }
        
        /// <summary>
        /// 静音开关变化
        /// </summary>
        /// <param name="muted">是否静音</param>
        private void OnMuteToggleChanged(bool muted)
        {
            if (audioComponent != null)
            {
                audioComponent.SetMuted(muted);
            }
        }
        
        #endregion
        
        #region 组件事件处理
        
        /// <summary>
        /// 动画开始事件
        /// </summary>
        /// <param name="animationName">动画名称</param>
        private void OnAnimationStart(string animationName)
        {
            Debug.Log($"[ExamplePanel] 动画开始: {animationName}");
        }
        
        /// <summary>
        /// 动画完成事件
        /// </summary>
        /// <param name="animationName">动画名称</param>
        private void OnAnimationComplete(string animationName)
        {
            Debug.Log($"[ExamplePanel] 动画完成: {animationName}");
        }
        
        /// <summary>
        /// 音效开始事件
        /// </summary>
        /// <param name="audioName">音效名称</param>
        private void OnAudioStart(string audioName)
        {
            Debug.Log($"[ExamplePanel] 音效开始: {audioName}");
        }
        
        /// <summary>
        /// 音效结束事件
        /// </summary>
        /// <param name="audioName">音效名称</param>
        private void OnAudioEnd(string audioName)
        {
            Debug.Log($"[ExamplePanel] 音效结束: {audioName}");
        }
        
        /// <summary>
        /// 面板点击事件
        /// </summary>
        /// <param name="eventData">事件数据</param>
        private void OnPanelClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            Debug.Log("[ExamplePanel] 面板被点击");
            
            // 播放悬停音效
            if (audioComponent != null)
            {
                audioComponent.PlayHoverSound();
            }
        }
        
        /// <summary>
        /// 按键事件
        /// </summary>
        /// <param name="key">按键</param>
        private void OnKeyPressed(KeyCode key)
        {
            Debug.Log($"[ExamplePanel] 按键按下: {key}");
        }
        
        /// <summary>
        /// 空格键事件
        /// </summary>
        private void OnSpacePressed()
        {
            Debug.Log("[ExamplePanel] 空格键按下 - 播放弹跳动画");
            
            if (animationComponent != null)
            {
                animationComponent.PlayBounceAnimation(Vector3.one * 1.2f, 0.6f);
            }
        }
        
        /// <summary>
        /// Ctrl+A组合键事件
        /// </summary>
        private void OnCtrlAPressed()
        {
            Debug.Log("[ExamplePanel] Ctrl+A按下 - 播放错误音效");
            
            if (audioComponent != null)
            {
                audioComponent.PlayErrorSound();
            }
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUI()
        {
            if (volumeSlider != null && audioComponent != null)
            {
                volumeSlider.value = audioComponent.Volume;
            }
            
            if (muteToggle != null && audioComponent != null)
            {
                muteToggle.isOn = audioComponent.IsMuted;
            }
        }
        
        /// <summary>
        /// 根据传入数据更新UI
        /// </summary>
        /// <param name="data">传入的数据</param>
        private void UpdateUIWithData(object data)
        {
            // 处理不同类型的数据
            switch (data)
            {
                case string message:
                    // 如果是字符串，更新内容文本
                    if (contentText != null)
                    {
                        contentText.text = $"接收到消息: {message}";
                    }
                    Debug.Log($"[ExamplePanel] 更新文本内容: {message}");
                    break;
                    
                case int number:
                    // 如果是数字，更新标题
                    if (titleText != null)
                    {
                        titleText.text = $"示例面板 #{number}";
                    }
                    Debug.Log($"[ExamplePanel] 更新面板编号: {number}");
                    break;
                    
                case float volume when volume >= 0f && volume <= 1f:
                    // 如果是音量值，更新音量滑块
                    if (volumeSlider != null)
                    {
                        volumeSlider.value = volume;
                    }
                    if (audioComponent != null)
                    {
                        audioComponent.SetVolume(volume);
                    }
                    Debug.Log($"[ExamplePanel] 更新音量: {volume}");
                    break;
                    
                case System.Collections.Generic.Dictionary<string, object> dataDict:
                    // 如果是字典，处理复合数据
                    foreach (var kvp in dataDict)
                    {
                        Debug.Log($"[ExamplePanel] 数据项 {kvp.Key}: {kvp.Value}");
                        
                        switch (kvp.Key.ToLower())
                        {
                            case "title":
                                if (titleText != null && kvp.Value is string title)
                                {
                                    titleText.text = title;
                                }
                                break;
                                
                            case "content":
                                if (contentText != null && kvp.Value is string content)
                                {
                                    contentText.text = content;
                                }
                                break;
                                
                            case "volume":
                                if (kvp.Value is float vol && volumeSlider != null)
                                {
                                    volumeSlider.value = vol;
                                    audioComponent?.SetVolume(vol);
                                }
                                break;
                                
                            case "animation":
                                if (kvp.Value is string animName && animationComponent != null)
                                {
                                    if (System.Enum.TryParse<CustomAnimationType>(animName, out var animType))
                                    {
                                        animationComponent.PlayAnimation(animType, 0.8f);
                                    }
                                }
                                break;
                        }
                    }
                    break;
                    
                default:
                    // 其他类型数据的通用处理
                    if (contentText != null)
                    {
                        contentText.text = $"接收到数据: {data?.ToString() ?? "null"}";
                    }
                    Debug.Log($"[ExamplePanel] 处理未知类型数据: {data?.GetType().Name}");
                    break;
            }
        }
        
        /// <summary>
        /// 演示所有动画效果
        /// </summary>
        public void DemoAllAnimations()
        {
            if (animationComponent == null) return;
            
            StartCoroutine(PlayAnimationSequence());
        }
        
        /// <summary>
        /// 播放动画序列
        /// </summary>
        private System.Collections.IEnumerator PlayAnimationSequence()
        {
            var animationTypes = System.Enum.GetValues(typeof(CustomAnimationType));
            
            foreach (CustomAnimationType animationType in animationTypes)
            {
                Debug.Log($"[ExamplePanel] 播放动画: {animationType}");
                animationComponent.PlayAnimation(animationType, 1f);
                
                yield return new WaitForSeconds(1.5f);
            }
            
            Debug.Log("[ExamplePanel] 动画序列播放完成");
        }
        
        /// <summary>
        /// 演示所有音效
        /// </summary>
        public void DemoAllSounds()
        {
            if (audioComponent == null) return;
            
            StartCoroutine(PlaySoundSequence());
        }
        
        /// <summary>
        /// 播放音效序列
        /// </summary>
        private System.Collections.IEnumerator PlaySoundSequence()
        {
            string[] soundNames = { "click", "hover", "open", "close", "error", "success" };
            
            foreach (string soundName in soundNames)
            {
                Debug.Log($"[ExamplePanel] 播放音效: {soundName}");
                audioComponent.PlaySound(soundName);
                
                yield return new WaitForSeconds(0.8f);
            }
            
            Debug.Log("[ExamplePanel] 音效序列播放完成");
        }
        
        #endregion
        
        #region 销毁
        
        protected override void OnDestroyed()
        {
            // 解绑UI事件
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
            }
            
            if (animationButton != null)
            {
                animationButton.onClick.RemoveAllListeners();
            }
            
            if (audioButton != null)
            {
                audioButton.onClick.RemoveAllListeners();
            }
            
            if (volumeSlider != null)
            {
                volumeSlider.onValueChanged.RemoveAllListeners();
            }
            
            if (muteToggle != null)
            {
                muteToggle.onValueChanged.RemoveAllListeners();
            }
            
            // 解绑组件事件
            if (animationComponent != null)
            {
                animationComponent.OnAnimationStart -= OnAnimationStart;
                animationComponent.OnAnimationComplete -= OnAnimationComplete;
            }
            
            if (audioComponent != null)
            {
                audioComponent.OnAudioStart -= OnAudioStart;
                audioComponent.OnAudioEnd -= OnAudioEnd;
            }
            
            if (inputComponent != null)
            {
                inputComponent.OnMouseClick -= OnPanelClick;
                inputComponent.OnKeyPressed -= OnKeyPressed;
            }
            
            base.OnDestroyed();
        }
        
        #endregion
    }
}