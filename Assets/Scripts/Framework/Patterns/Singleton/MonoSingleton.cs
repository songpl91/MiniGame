using System;
using UnityEngine;

/// <summary>
/// MonoBehaviour单例
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class MonoSingleton<T> : MonoBehaviour, ISingleton where T : MonoSingleton<T>
{
    private static T m_Instance = null;
    public bool IsInited { get; private set; }

    public static T Instance
    {
        get 
        {
            if (m_Instance == null) 
            {
                Type type = typeof(T);
                GameObject gameObject = new GameObject(type.ToString(), type);
                m_Instance = gameObject.GetComponent<T>();
                m_Instance.Init();
                DontDestroyOnLoad(gameObject);
            }
            return m_Instance;
        }
    }

    public static bool CheckInstance()
    {
        return m_Instance != null;
    }

    protected virtual void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this as T;
            m_Instance.Init();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (m_Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    protected virtual void OnDestroy()
    {
        m_Instance = null;
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
        Destroy(gameObject);
    }
}