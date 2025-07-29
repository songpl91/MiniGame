using System;

/// <summary>
/// 普通单例
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> : ISingleton where T : Singleton<T>, new()
{
    private static T m_Instance = null;
    public bool IsInited { get; private set; }

    public static T Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new T();
                m_Instance.Init();
            }
            return m_Instance;
        }
    }

    public static bool CheckInstance() 
    {
        return m_Instance != null;
    }

    public void Init()
    {
        if (IsInited)
        {
            return;
        }

        OnInit();
        IsInited = true;
    }

    protected virtual void OnInit() { }

    public virtual void Destroy() 
    {
        m_Instance = null;
    }
}