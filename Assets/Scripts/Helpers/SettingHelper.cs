using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingHelper
{
    /// <summary>
    /// 震动状态
    /// </summary>
    public static bool VibrateState
    {
        get { return PlayerPrefsUtil.GetBool(GameConst.VibrateState, true); }
        set { PlayerPrefsUtil.SetBool(GameConst.VibrateState, value); }
    }

    /// <summary>
    /// 音乐状态
    /// </summary>
    public static bool MusicState
    {
        get { return PlayerPrefsUtil.GetBool(GameConst.MusicState, true); }
        set
        {
            PlayerPrefsUtil.SetBool(GameConst.MusicState, value);
            if (value)
            {
                AudioManager.Instance.PlayBackgroundMusic();
            }
            else
            {
                AudioManager.Instance.PauseBackgroundMusic();
            }
        }
    }

    /// <summary>
    /// 音效状态
    /// </summary>
    public static bool SoundState
    {
        get { return PlayerPrefsUtil.GetBool(GameConst.SoundState, true); }
        set { PlayerPrefsUtil.SetBool(GameConst.SoundState, value); }
    }
}
