using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerPrefsUtil
{
    private const string s_listSeparator = ",";

    public static bool Exists(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    public static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }

    public static string GetString(string key, string defalutStr = "")
    {
        return PlayerPrefs.GetString(key, defalutStr);
    }

    public static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

    public static decimal GetDecimal(string key, decimal defalutStr = 0)
    {
        return decimal.Parse(PlayerPrefs.GetString(key, defalutStr.ToString()));
    }

    public static void SetDecimal(string key, decimal value)
    {
        PlayerPrefs.SetString(key, value.ToString());
    }

    public static bool GetBool(string key, bool value = false)
    {
        return PlayerPrefs.GetString(key, value ? "1" : "0") == "1";
    }

    public static void SetBool(string key, bool value)
    {
        PlayerPrefs.SetString(key, value ? "1" : "0");
    }

    public static int GetInt(string key, int defaultNum = 0)
    {
        return PlayerPrefs.GetInt(key, defaultNum);
    }

    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }

    public static double GetDouble(string key, double value = 0)
    {
        if (!Exists(key))
        {
            return value;
        }

        return GetFloat(key);
    }

    public static void SetDouble(string key, double value)
    {
        SetFloat(key, (float)value);
    }

    public static float GetFloat(string key, float defaultNum = 0f)
    {
        return PlayerPrefs.GetFloat(key, defaultNum);
    }

    public static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }

    public static DateTime GetDateTime(string key)
    {
        return GetDateTime(key, new DateTime(1970, 1, 1));
    }

    public static DateTime GetDateTime(string key, DateTime dateTime)
    {
        if (!Exists(key))
        {
            return dateTime;
        }

        bool isSuccess = long.TryParse(GetString(key), out long ticks);
        if (!isSuccess)
        {
            return new DateTime(1970, 1, 1);
        }

        return new DateTime(ticks);
    }

    public static void SetDateTime(string key, DateTime dateTime)
    {
        SetString(key, dateTime.Ticks.ToString());
    }

    public static List<string> GetStringList(string key)
    {
        List<string> list = new List<string>();
        string str = GetString(key);
        if (string.IsNullOrEmpty(str))
        {
            return list;
        }

        list.AddRange(str.Split(new string[] { s_listSeparator }, StringSplitOptions.RemoveEmptyEntries));
        return list;
    }

    public static void SetStringList(string key, List<string> list)
    {
        if (list == null || list.Count == 0)
        {
            SetString(key, string.Empty);
            return;
        }

        SetString(key, string.Join(s_listSeparator, list));
    }

    public static List<int> GetIntList(string key)
    {
        List<int> list = new List<int>();
        string str = GetString(key);
        if (string.IsNullOrEmpty(str))
        {
            return list;
        }

        string[] array = str.Split(new string[] { s_listSeparator }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string s in array)
        {
            list.Add(int.Parse(s));
        }

        return list;
    }

    public static void SetIntList(string key, List<int> list)
    {
        if (list == null || list.Count == 0)
        {
            SetString(key, string.Empty);
            return;
        }

        SetString(key, string.Join(s_listSeparator, list));
    }

    public static T Get<T>(string key, T t = null) where T : class
    {
        if (PlayerPrefs.HasKey(key))
        {
            return JsonUtility.FromJson<T>(PlayerPrefs.GetString(key));
        }
        else
        {
            return t;
        }
    }

    public static void Set<T>(string key, T t) where T : class
    {
        try
        {
            PlayerPrefs.SetString(key, JsonUtility.ToJson(t));
        }
        catch (Exception e)
        {
            //Firebase.Crashlytics.Crashlytics.LogException(new Exception("Save Level : "+e.Message));
        }
    }

    public static void DeleteVector2(string key)
    {
        DeleteKey(string.Format("{0}_1", key));
        DeleteKey(string.Format("{0}_2", key));
    }

    public static void SetVector2(string key, Vector2 v2)
    {
        SetFloat(string.Format("{0}_1", key), v2.x);
        SetFloat(string.Format("{0}_2", key), v2.y);
    }

    public static Vector2 GetVector2(string key)
    {
        float x = GetFloat(string.Format("{0}_1", key));
        float y = GetFloat(string.Format("{0}_2", key));
        return new Vector2(x, y);
    }

    public static Dictionary<T, M> GetDic<T, M>(string key) where T : struct where M : struct
    {
        Dictionary<T, M> dic = new Dictionary<T, M>();

        if (Exists(key))
        {
            List<string> dataList = GetStringList(key);
            foreach (string str in dataList)
            {
                string[] strs = str.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                if (strs == null || strs.Length < 2)
                {
                    continue;
                }

                dic.Add((T)Convert.ChangeType(strs[0], typeof(T)), (M)Convert.ChangeType(strs[1], typeof(M)));
            }
        }

        return dic;
    }

    public static void SetDic<T, M>(string key, Dictionary<T, M> dic)
    {
        List<string> dataList = new List<string>();

        if (dic != null && dic.Count > 0)
        {
            foreach (T type in dic.Keys)
            {
                dataList.Add(string.Format("{0}_{1}", type.ToString(), dic[type].ToString()));
            }
        }

        SetStringList(key, dataList);
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteAll();
    }
}