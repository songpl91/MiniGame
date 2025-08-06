using System;
using System.Collections.Generic;
using UnityEngine;
using Framework.SampleUI.Core;

namespace Framework.SampleUI.Components
{
    /// <summary>
    /// 音频组件
    /// 为UI面板提供音效播放功能
    /// </summary>
    public class AudioComponent : SampleUIComponent
    {
        #region 字段和属性
        
        /// <summary>
        /// 音频配置
        /// </summary>
        public AudioConfig Config { get; set; } = new AudioConfig();
        
        /// <summary>
        /// 音频源组件
        /// </summary>
        private AudioSource audioSource;
        
        /// <summary>
        /// 音效剪辑字典
        /// </summary>
        private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
        
        /// <summary>
        /// 是否已静音
        /// </summary>
        public bool IsMuted { get; private set; }
        
        /// <summary>
        /// 当前音量
        /// </summary>
        public float Volume 
        { 
            get { return audioSource != null ? audioSource.volume : 0f; }
            set 
            { 
                if (audioSource != null) 
                {
                    audioSource.volume = Mathf.Clamp01(value);
                }
            }
        }
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 音效播放开始事件
        /// </summary>
        public event Action<string> OnAudioStart;
        
        /// <summary>
        /// 音效播放结束事件
        /// </summary>
        public event Action<string> OnAudioEnd;
        
        #endregion
        
        #region 初始化
        
        protected override void OnInitialize()
        {
            // 获取或创建AudioSource组件
            if (OwnerPanel is MonoBehaviour mono)
            {
                audioSource = mono.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = mono.gameObject.AddComponent<AudioSource>();
                }
                
                // 配置AudioSource
                ConfigureAudioSource();
                
                // 预加载默认音效
                LoadDefaultAudioClips();
            }
        }
        
        /// <summary>
        /// 配置音频源
        /// </summary>
        private void ConfigureAudioSource()
        {
            if (audioSource == null) return;
            
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = Config.defaultVolume;
            audioSource.pitch = 1f;
            audioSource.spatialBlend = 0f; // 2D音效
        }
        
        /// <summary>
        /// 加载默认音效
        /// </summary>
        private void LoadDefaultAudioClips()
        {
            // 这里可以加载一些默认的UI音效
            // 实际项目中应该从Resources或AssetBundle加载
            LoadAudioClip("click", "UI/Sounds/click");
            LoadAudioClip("hover", "UI/Sounds/hover");
            LoadAudioClip("open", "UI/Sounds/open");
            LoadAudioClip("close", "UI/Sounds/close");
            LoadAudioClip("error", "UI/Sounds/error");
            LoadAudioClip("success", "UI/Sounds/success");
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clipName">音效名称</param>
        /// <param name="volume">音量（可选）</param>
        public void PlaySound(string clipName, float volume = -1f)
        {
            if (!Config.enableAudio || IsMuted)
                return;
                
            if (audioClips.TryGetValue(clipName, out AudioClip clip))
            {
                if (clip != null && audioSource != null)
                {
                    float playVolume = volume >= 0f ? volume : Config.defaultVolume;
                    audioSource.PlayOneShot(clip, playVolume);
                    
                    OnAudioStart?.Invoke(clipName);
                    
                    // 延迟触发结束事件
                    if (OwnerPanel is MonoBehaviour mono)
                    {
                        mono.StartCoroutine(DelayedAudioEndEvent(clipName, clip.length));
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[AudioComponent] 音效 '{clipName}' 未找到");
            }
        }
        
        /// <summary>
        /// 播放点击音效
        /// </summary>
        public void PlayClickSound()
        {
            PlaySound("click", Config.clickVolume);
        }
        
        /// <summary>
        /// 播放悬停音效
        /// </summary>
        public void PlayHoverSound()
        {
            PlaySound("hover", Config.hoverVolume);
        }
        
        /// <summary>
        /// 播放打开音效
        /// </summary>
        public void PlayOpenSound()
        {
            PlaySound("open", Config.openVolume);
        }
        
        /// <summary>
        /// 播放关闭音效
        /// </summary>
        public void PlayCloseSound()
        {
            PlaySound("close", Config.closeVolume);
        }
        
        /// <summary>
        /// 播放错误音效
        /// </summary>
        public void PlayErrorSound()
        {
            PlaySound("error", Config.errorVolume);
        }
        
        /// <summary>
        /// 播放成功音效
        /// </summary>
        public void PlaySuccessSound()
        {
            PlaySound("success", Config.successVolume);
        }
        
        /// <summary>
        /// 加载音效剪辑
        /// </summary>
        /// <param name="clipName">音效名称</param>
        /// <param name="resourcePath">资源路径</param>
        public void LoadAudioClip(string clipName, string resourcePath)
        {
            try
            {
                AudioClip clip = Resources.Load<AudioClip>(resourcePath);
                if (clip != null)
                {
                    audioClips[clipName] = clip;
                }
                else
                {
                    Debug.LogWarning($"[AudioComponent] 无法加载音效: {resourcePath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AudioComponent] 加载音效异常: {e.Message}");
            }
        }
        
        /// <summary>
        /// 添加音效剪辑
        /// </summary>
        /// <param name="clipName">音效名称</param>
        /// <param name="clip">音效剪辑</param>
        public void AddAudioClip(string clipName, AudioClip clip)
        {
            if (clip != null)
            {
                audioClips[clipName] = clip;
            }
        }
        
        /// <summary>
        /// 移除音效剪辑
        /// </summary>
        /// <param name="clipName">音效名称</param>
        public void RemoveAudioClip(string clipName)
        {
            audioClips.Remove(clipName);
        }
        
        /// <summary>
        /// 停止所有音效
        /// </summary>
        public void StopAllSounds()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }
        
        /// <summary>
        /// 设置静音状态
        /// </summary>
        /// <param name="muted">是否静音</param>
        public void SetMuted(bool muted)
        {
            IsMuted = muted;
            if (audioSource != null)
            {
                audioSource.mute = muted;
            }
        }
        
        /// <summary>
        /// 切换静音状态
        /// </summary>
        public void ToggleMute()
        {
            SetMuted(!IsMuted);
        }
        
        /// <summary>
        /// 淡入音量
        /// </summary>
        /// <param name="targetVolume">目标音量</param>
        /// <param name="duration">淡入时长</param>
        public void FadeIn(float targetVolume, float duration = 1f)
        {
            if (OwnerPanel is MonoBehaviour mono)
            {
                mono.StartCoroutine(FadeVolume(0f, targetVolume, duration));
            }
        }
        
        /// <summary>
        /// 淡出音量
        /// </summary>
        /// <param name="duration">淡出时长</param>
        public void FadeOut(float duration = 1f)
        {
            if (OwnerPanel is MonoBehaviour mono)
            {
                mono.StartCoroutine(FadeVolume(Volume, 0f, duration));
            }
        }
        
        #endregion
        
        #region 面板事件处理

        public override void OnPanelShow()
        {
            if (Config.playOpenSoundOnShow)
            {
                PlayOpenSound();
            }
        }

        public override void OnPanelHide()
        {
            if (Config.playCloseSoundOnHide)
            {
                PlayCloseSound();
            }
            
            // 面板隐藏时停止所有音效
            if (Config.stopSoundsOnHide)
            {
                StopAllSounds();
            }
        }
        
        #endregion
        
        #region 协程方法
        
        /// <summary>
        /// 延迟触发音效结束事件
        /// </summary>
        private System.Collections.IEnumerator DelayedAudioEndEvent(string clipName, float delay)
        {
            yield return new WaitForSeconds(delay);
            OnAudioEnd?.Invoke(clipName);
        }
        
        /// <summary>
        /// 音量淡入淡出
        /// </summary>
        private System.Collections.IEnumerator FadeVolume(float startVolume, float targetVolume, float duration)
        {
            if (audioSource == null) yield break;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }
            
            audioSource.volume = targetVolume;
        }
        
        #endregion
        
        #region 销毁
        
        protected override void OnDestroyed()
        {
            StopAllSounds();
            
            // 清理音效剪辑
            audioClips.Clear();
            
            audioSource = null;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 音频配置
    /// </summary>
    [System.Serializable]
    public class AudioConfig
    {
        [Header("基础设置")]
        public bool enableAudio = true;
        public float defaultVolume = 1f;
        
        [Header("UI音效音量")]
        [Range(0f, 1f)]
        public float clickVolume = 0.8f;
        [Range(0f, 1f)]
        public float hoverVolume = 0.5f;
        [Range(0f, 1f)]
        public float openVolume = 0.7f;
        [Range(0f, 1f)]
        public float closeVolume = 0.7f;
        [Range(0f, 1f)]
        public float errorVolume = 0.9f;
        [Range(0f, 1f)]
        public float successVolume = 0.8f;
        
        [Header("自动播放设置")]
        public bool playOpenSoundOnShow = true;
        public bool playCloseSoundOnHide = true;
        public bool stopSoundsOnHide = true;
        
        [Header("高级设置")]
        public bool use3DAudio = false;
        public float minDistance = 1f;
        public float maxDistance = 500f;
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    }
}