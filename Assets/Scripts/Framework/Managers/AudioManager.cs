using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 待完善！！！
/// </summary>
public class AudioManager : MonoSingleton<AudioManager>
{
    /// <summary>
    /// 所有音频缓存
    /// </summary>
    private Dictionary<string, AudioClip> AudioDic;

    /// <summary>
    /// 所有音效播放组件
    /// </summary>
    private List<AudioSource> m_EffectAudioSourceList;

    /// <summary>
    /// 背景音乐播放组件
    /// </summary>
    private AudioSource m_BackgroundAudioSource;

    /// <summary>
    /// 低频滤波
    /// </summary>
    private AudioLowPassFilter m_AudioLowPassFilter;

    /// <summary>
    /// 允许同时播放的音效数量
    /// </summary>
    private const int MaxEffectAudioNum = 10;

    private int m_CurrentIndex = 0;

    /// <summary>
    /// 当前选择播放的背景音乐名字
    /// </summary>
    private string m_CurSelectBackgroundName;

    private Dictionary<int, AudioSource> m_AudioSourceDic = new Dictionary<int, AudioSource>();

    public void Init()
    {
        AudioDic = new Dictionary<string, AudioClip>();
        m_BackgroundAudioSource = gameObject.AddComponent<AudioSource>();
        m_EffectAudioSourceList = new List<AudioSource>(MaxEffectAudioNum);
        for (int i = 0; i < MaxEffectAudioNum; i++)
        {
            AudioSource Audio = gameObject.AddComponent<AudioSource>();
            m_EffectAudioSourceList.Add(Audio);
        }

        m_AudioLowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
        m_AudioLowPassFilter.enabled = false;
    }

    public AudioClip GetAudioClip(string sound)
    {
        AudioDic.TryGetValue(sound, out AudioClip audioClip);
        if (audioClip == null)
        {
            // audioClip = AddressableManager.Instance.LoadResource<AudioClip>(AudioConst.AudioResRootPath + sound);
            if (audioClip != null && !AudioDic.ContainsKey(audioClip.name))
            {
                AudioDic.Add(audioClip.name, audioClip);
            }
        }

        return audioClip;
    }

    #region Music

    public void PlayBackgroundMusic(string AudioName, float volume = 1)
    {
        if (string.IsNullOrEmpty(AudioName) || m_CurSelectBackgroundName == AudioName)
        {
            // m_BackgroundAudioSource.DOFade(volume, 3f);
            return;
        }

        StopBackgroundMusic();
        m_CurSelectBackgroundName = AudioName;
        if (SettingHelper.MusicState)
        {
            PlayBackgroundMusic(AudioName, true, volume);
        }
    }

    public void PlayBackgroundMusic()
    {
        if (string.IsNullOrEmpty(m_CurSelectBackgroundName))
        {
            return;
        }

        if (SettingHelper.MusicState)
        {
            PlayBackgroundMusic(m_CurSelectBackgroundName, true);
        }
    }

    public void PauseBackgroundMusic()
    {
        if (!m_BackgroundAudioSource.isPlaying)
        {
            return;
        }

        m_BackgroundAudioSource.Pause();
    }

    public void FadeBackgroundMusic(float value)
    {
        if (!m_BackgroundAudioSource.isPlaying)
        {
            return;
        }

        // m_BackgroundAudioSource.DOFade(value, 2f);
    }

    private void ResumeBackgroundMusic()
    {
        if (m_BackgroundAudioSource == null)
        {
            return;
        }

        if (!SettingHelper.MusicState)
        {
            return;
        }

        // Debug.Log("AudioManager audio mute state "+ m_BackgroundAudioSource.mute);

        m_BackgroundAudioSource.mute = false;

        m_BackgroundAudioSource.UnPause();
    }

    private void PlayBackgroundMusic(string AudioName, bool delay, float volume = 1)
    {
        AudioClip Music = GetAudioClip(AudioName);
        if (Music == null) return;
        if (delay)
        {
            StartCoroutine(PlayMusicDelay(Music, volume));
        }
        else
        {
            m_BackgroundAudioSource.clip = Music;
            m_BackgroundAudioSource.loop = true;
            m_BackgroundAudioSource.playOnAwake = false;
            m_BackgroundAudioSource.volume = volume;
            if (!m_BackgroundAudioSource.isPlaying)
            {
                m_BackgroundAudioSource.Play();
            }
            else
            {
                m_BackgroundAudioSource.UnPause();
            }
        }
    }

    private IEnumerator PlayMusicDelay(AudioClip Music, float volume = 1)
    {
        yield return new WaitForEndOfFrame();
        m_BackgroundAudioSource.volume = 0f;
        m_BackgroundAudioSource.clip = Music;
        m_BackgroundAudioSource.loop = true;
        m_BackgroundAudioSource.playOnAwake = false;
        if (!m_BackgroundAudioSource.isPlaying)
        {
            m_BackgroundAudioSource.Play();
        }
        else
        {
            m_BackgroundAudioSource.UnPause();
        }

        // m_BackgroundAudioSource.DOFade(volume, 3f);
    }

    public void StopBackgroundMusic()
    {
        if (!m_BackgroundAudioSource.isPlaying)
        {
            return;
        }

        m_BackgroundAudioSource.Stop();
        m_CurSelectBackgroundName = string.Empty;
    }

    #endregion

    #region LowPass

    /// <summary>
    /// 设置低频滤波值
    /// </summary>
    /// <param name="value"></param>
    public void SetLowPassCutoffFrequency(float value)
    {
        if (m_AudioLowPassFilter == null)
        {
            return;
        }

        m_AudioLowPassFilter.cutoffFrequency = value;
        m_BackgroundAudioSource.volume = 0.5f;
        m_AudioLowPassFilter.enabled = true;
    }

    /// <summary>
    /// 重置低频滤波值
    /// </summary>
    public void ResetLowPassCutoffFrequency()
    {
        if (m_AudioLowPassFilter == null)
        {
            return;
        }

        m_AudioLowPassFilter.cutoffFrequency = 5000f;
        m_BackgroundAudioSource.volume = 1;
        m_AudioLowPassFilter.enabled = false;
    }

    #endregion

    #region Sound

    public void PlayContinousSound(int index, string audioName)
    {
        if (m_AudioSourceDic.ContainsKey(index))
        {
            if (m_AudioSourceDic[index].isPlaying)
            {
                return;
            }
            else
            {
                m_AudioSourceDic[index].Play();
            }
        }
        else
        {
            AudioClip audioClip = GetAudioClip(audioName);
            if (audioClip == null) return;
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            if (audioSource == null)
                return;
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            audioSource.Play();
            m_AudioSourceDic.Add(index, audioSource);
        }
    }

    public void StopContinousSound(int index)
    {
        if (m_AudioSourceDic.ContainsKey(index))
        {
            if (m_AudioSourceDic[index].isPlaying)
            {
                m_AudioSourceDic[index].Stop();
            }
        }
    }

    public void PlayAudioEffect(string AudioName, float pitch = 1)
    {
        if (string.IsNullOrEmpty(AudioName))
            return;

        if (!SettingHelper.SoundState)
        {
            return;
        }

        AudioClip Effect = GetAudioClip(AudioName);
        if (Effect == null) return;
        m_CurrentIndex++;
        if (m_CurrentIndex >= MaxEffectAudioNum)
        {
            m_CurrentIndex = 0;
        }

        AudioSource Audio = m_EffectAudioSourceList[m_CurrentIndex];
        if (Audio == null)
            return;
        if (Audio.isPlaying)
        {
            Audio.Stop();
        }

        Audio.pitch = pitch;
        Audio.clip = Effect;
        Audio.loop = false;
        Audio.playOnAwake = false;
        Audio.Play();
    }

    private void StopAudioEffect()
    {
        for (int i = 0; i < m_EffectAudioSourceList.Count; i++)
        {
            if (m_EffectAudioSourceList[i].isPlaying)
            {
                if (null == m_EffectAudioSourceList[i].clip)
                {
                    continue;
                }

                m_EffectAudioSourceList[i].Stop();
            }
        }
    }

    #endregion
}