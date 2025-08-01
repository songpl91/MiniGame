using UnityEngine;
using System.Collections.Generic;

namespace Framework.TraditionalUI.Utils
{
    /// <summary>
    /// UI音效管理器
    /// 管理UI相关的音效播放
    /// </summary>
    public class UIAudioManager : MonoBehaviour
    {
        #region 单例模式
        
        private static UIAudioManager _instance;
        
        public static UIAudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("UIAudioManager");
                    _instance = go.AddComponent<UIAudioManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region 序列化字段
        
        [Header("音效设置")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float defaultVolume = 1.0f;
        
        [Header("UI音效资源")]
        [SerializeField] private UIAudioClips audioClips;
        
        #endregion
        
        #region 私有变量
        
        private Dictionary<UIAudioType, AudioClip> audioClipDict;
        private bool isMuted = false;
        private float currentVolume = 1.0f;
        
        #endregion
        
        #region 初始化方法
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 初始化音效管理器
        /// </summary>
        private void Initialize()
        {
            // 创建音频源
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
            }
            
            // 设置默认音量
            currentVolume = defaultVolume;
            audioSource.volume = currentVolume;
            
            // 初始化音效字典
            InitializeAudioClips();
            
            // 加载设置
            LoadSettings();
        }
        
        /// <summary>
        /// 初始化音效字典
        /// </summary>
        private void InitializeAudioClips()
        {
            audioClipDict = new Dictionary<UIAudioType, AudioClip>();
            
            if (audioClips != null)
            {
                audioClipDict[UIAudioType.ButtonClick] = audioClips.buttonClick;
                audioClipDict[UIAudioType.ButtonHover] = audioClips.buttonHover;
                audioClipDict[UIAudioType.PanelOpen] = audioClips.panelOpen;
                audioClipDict[UIAudioType.PanelClose] = audioClips.panelClose;
                audioClipDict[UIAudioType.Success] = audioClips.success;
                audioClipDict[UIAudioType.Error] = audioClips.error;
                audioClipDict[UIAudioType.Warning] = audioClips.warning;
                audioClipDict[UIAudioType.Purchase] = audioClips.purchase;
                audioClipDict[UIAudioType.Coin] = audioClips.coin;
                audioClipDict[UIAudioType.Gem] = audioClips.gem;
                audioClipDict[UIAudioType.Notification] = audioClips.notification;
                audioClipDict[UIAudioType.Swipe] = audioClips.swipe;
                audioClipDict[UIAudioType.Toggle] = audioClips.toggle;
                audioClipDict[UIAudioType.Slider] = audioClips.slider;
                audioClipDict[UIAudioType.Dropdown] = audioClips.dropdown;
            }
        }
        
        /// <summary>
        /// 加载设置
        /// </summary>
        private void LoadSettings()
        {
            // 从PlayerPrefs加载设置
            isMuted = PlayerPrefs.GetInt("UI_Audio_Muted", 0) == 1;
            currentVolume = PlayerPrefs.GetFloat("UI_Audio_Volume", defaultVolume);
            
            UpdateAudioSource();
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 播放UI音效
        /// </summary>
        /// <param name="audioType">音效类型</param>
        /// <param name="volume">音量（可选）</param>
        public void PlayUISound(UIAudioType audioType, float volume = -1f)
        {
            if (isMuted || audioSource == null) return;
            
            if (audioClipDict.TryGetValue(audioType, out AudioClip clip) && clip != null)
            {
                float playVolume = volume >= 0 ? volume : currentVolume;
                audioSource.PlayOneShot(clip, playVolume);
            }
            else
            {
                Debug.LogWarning($"[UI音效] 未找到音效: {audioType}");
            }
        }
        
        /// <summary>
        /// 播放按钮点击音效
        /// </summary>
        public void PlayButtonClick()
        {
            PlayUISound(UIAudioType.ButtonClick);
        }
        
        /// <summary>
        /// 播放按钮悬停音效
        /// </summary>
        public void PlayButtonHover()
        {
            PlayUISound(UIAudioType.ButtonHover);
        }
        
        /// <summary>
        /// 播放面板打开音效
        /// </summary>
        public void PlayPanelOpen()
        {
            PlayUISound(UIAudioType.PanelOpen);
        }
        
        /// <summary>
        /// 播放面板关闭音效
        /// </summary>
        public void PlayPanelClose()
        {
            PlayUISound(UIAudioType.PanelClose);
        }
        
        /// <summary>
        /// 播放成功音效
        /// </summary>
        public void PlaySuccess()
        {
            PlayUISound(UIAudioType.Success);
        }
        
        /// <summary>
        /// 播放错误音效
        /// </summary>
        public void PlayError()
        {
            PlayUISound(UIAudioType.Error);
        }
        
        /// <summary>
        /// 播放警告音效
        /// </summary>
        public void PlayWarning()
        {
            PlayUISound(UIAudioType.Warning);
        }
        
        /// <summary>
        /// 播放购买音效
        /// </summary>
        public void PlayPurchase()
        {
            PlayUISound(UIAudioType.Purchase);
        }
        
        /// <summary>
        /// 播放金币音效
        /// </summary>
        public void PlayCoin()
        {
            PlayUISound(UIAudioType.Coin);
        }
        
        /// <summary>
        /// 播放宝石音效
        /// </summary>
        public void PlayGem()
        {
            PlayUISound(UIAudioType.Gem);
        }
        
        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="volume">音量值（0-1）</param>
        public void SetVolume(float volume)
        {
            currentVolume = Mathf.Clamp01(volume);
            UpdateAudioSource();
            SaveSettings();
        }
        
        /// <summary>
        /// 获取当前音量
        /// </summary>
        /// <returns>当前音量</returns>
        public float GetVolume()
        {
            return currentVolume;
        }
        
        /// <summary>
        /// 设置静音状态
        /// </summary>
        /// <param name="muted">是否静音</param>
        public void SetMuted(bool muted)
        {
            isMuted = muted;
            UpdateAudioSource();
            SaveSettings();
        }
        
        /// <summary>
        /// 获取静音状态
        /// </summary>
        /// <returns>是否静音</returns>
        public bool IsMuted()
        {
            return isMuted;
        }
        
        /// <summary>
        /// 切换静音状态
        /// </summary>
        public void ToggleMute()
        {
            SetMuted(!isMuted);
        }
        
        /// <summary>
        /// 设置音效资源
        /// </summary>
        /// <param name="clips">音效资源</param>
        public void SetAudioClips(UIAudioClips clips)
        {
            audioClips = clips;
            InitializeAudioClips();
        }
        
        /// <summary>
        /// 添加自定义音效
        /// </summary>
        /// <param name="audioType">音效类型</param>
        /// <param name="clip">音效片段</param>
        public void AddCustomAudioClip(UIAudioType audioType, AudioClip clip)
        {
            if (audioClipDict == null)
                audioClipDict = new Dictionary<UIAudioType, AudioClip>();
            
            audioClipDict[audioType] = clip;
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 更新音频源
        /// </summary>
        private void UpdateAudioSource()
        {
            if (audioSource != null)
            {
                audioSource.volume = isMuted ? 0f : currentVolume;
            }
        }
        
        /// <summary>
        /// 保存设置
        /// </summary>
        private void SaveSettings()
        {
            PlayerPrefs.SetInt("UI_Audio_Muted", isMuted ? 1 : 0);
            PlayerPrefs.SetFloat("UI_Audio_Volume", currentVolume);
            PlayerPrefs.Save();
        }
        
        #endregion
    }
    
    /// <summary>
    /// UI音效类型枚举
    /// </summary>
    public enum UIAudioType
    {
        ButtonClick,    // 按钮点击
        ButtonHover,    // 按钮悬停
        PanelOpen,      // 面板打开
        PanelClose,     // 面板关闭
        Success,        // 成功
        Error,          // 错误
        Warning,        // 警告
        Purchase,       // 购买
        Coin,           // 金币
        Gem,            // 宝石
        Notification,   // 通知
        Swipe,          // 滑动
        Toggle,         // 开关
        Slider,         // 滑动条
        Dropdown        // 下拉框
    }
    
    /// <summary>
    /// UI音效资源配置
    /// </summary>
    [System.Serializable]
    public class UIAudioClips
    {
        [Header("按钮音效")]
        public AudioClip buttonClick;
        public AudioClip buttonHover;
        
        [Header("面板音效")]
        public AudioClip panelOpen;
        public AudioClip panelClose;
        
        [Header("反馈音效")]
        public AudioClip success;
        public AudioClip error;
        public AudioClip warning;
        
        [Header("游戏音效")]
        public AudioClip purchase;
        public AudioClip coin;
        public AudioClip gem;
        
        [Header("交互音效")]
        public AudioClip notification;
        public AudioClip swipe;
        public AudioClip toggle;
        public AudioClip slider;
        public AudioClip dropdown;
    }
    
    /// <summary>
    /// UI音效按钮组件
    /// 自动为按钮添加音效
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class UIAudioButton : MonoBehaviour
    {
        [Header("音效设置")]
        [SerializeField] private UIAudioType clickAudioType = UIAudioType.ButtonClick;
        [SerializeField] private UIAudioType hoverAudioType = UIAudioType.ButtonHover;
        [SerializeField] private bool playClickSound = true;
        [SerializeField] private bool playHoverSound = false;
        [SerializeField] private float volume = 1.0f;
        
        private UnityEngine.UI.Button button;
        
        private void Awake()
        {
            button = GetComponent<UnityEngine.UI.Button>();
            
            if (playClickSound)
            {
                button.onClick.AddListener(PlayClickSound);
            }
        }
        
        private void PlayClickSound()
        {
            UIAudioManager.Instance.PlayUISound(clickAudioType, volume);
        }
        
        private void PlayHoverSound()
        {
            if (playHoverSound)
            {
                UIAudioManager.Instance.PlayUISound(hoverAudioType, volume);
            }
        }
        
        // 可以通过EventTrigger组件调用
        public void OnPointerEnter()
        {
            PlayHoverSound();
        }
    }
}