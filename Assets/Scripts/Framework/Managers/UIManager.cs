using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO
/// 1、UI排序问题
/// 2、UI异步加载
/// 3、UI多开
/// 4、UI预加载
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public Camera m_UICamera;
    public GameObject m_EventSystem;
    public RectTransform m_CacheRoot;
    public RectTransform m_UI;
    public RectTransform m_UpUI;
    public RectTransform m_Dialog;
    public RectTransform m_UpDialog;
    public RectTransform m_Message;

    private Dictionary<string, UIBase> m_UIDics = new Dictionary<string, UIBase>();
    private List<string> m_UILists = new List<string>();

    private void Awake()
    {
        DontDestroyOnLoad(this);
        DontDestroyOnLoad(m_UICamera.gameObject);
        DontDestroyOnLoad(m_EventSystem);
        Instance = this;
        PreLoadAsset();
    }

    /// <summary>
    /// 预加载消息广播资源
    /// </summary>
    private void PreLoadAsset()
    {
    }

    /// <summary>
    /// 打开UI
    /// </summary>
    /// <param name="panelPath"></param>
    /// <param name="param"></param>
    public void ShowUI(string panelPath, params object[] param)
    {
        UIBase uiBase = GetUI(panelPath);
        if (uiBase == null)
        {
            // GameObject instantiate = AddressableManager.Instance.Instantiate(panelPath);
            GameObject instantiate = null;
            if (instantiate == null)
            {
                Debug.LogError($"实例化UI预制体失败:{panelPath}");
                return;
            }

            uiBase = instantiate.GetComponent<UIBase>();
            if (uiBase == null)
            {
                Debug.LogError($"获取UI组件为空:{panelPath}");
                return;
            }

            if (uiBase.UiType == UIType.UI)
            {
                uiBase.transform.SetParent(m_UI, false);
            }
            else if (uiBase.UiType == UIType.UpUI)
            {
                uiBase.transform.SetParent(m_UpUI, false);
            }
            else if (uiBase.UiType == UIType.Dialog)
            {
                uiBase.transform.SetParent(m_Dialog, false);
            }
            else if (uiBase.UiType == UIType.UpDialog)
            {
                uiBase.transform.SetParent(m_UpDialog, false);
            }

            m_UIDics.Add(panelPath, uiBase);

            uiBase.Show(panelPath, param);
        }
        else
        {
            m_UILists.Remove(panelPath);
        }

        m_UILists.Insert(0, panelPath);
    }

    /// <summary>
    /// 关闭UI根据引用关
    /// </summary>
    /// <param name="ui"></param>
    public void CloseUI(UIBase ui)
    {
        if (ui == null)
        {
            Debug.LogError("UI为空");
            return;
        }

        if (!m_UIDics.ContainsKey(ui.PanelPath))
        {
            return;
        }

        m_UIDics.Remove(ui.PanelPath);
        m_UILists.Remove(ui.PanelPath);

        ui.Close();
    }

    /// <summary>
    /// 关闭UI，根据UI路径关
    /// </summary>
    /// <param name="panelPath"></param>
    public void CloseUI(string panelPath)
    {
        UIBase uiBase = GetUI(panelPath);
        if (uiBase == null)
        {
            Debug.LogError($"关闭UI时UI为空:{panelPath}");
            return;
        }

        CloseUI(uiBase);
    }

    /// <summary>
    /// 获取已经打开或者缓存的UI
    /// </summary>
    /// <param name="panelPath"></param>
    /// <returns></returns>
    public UIBase GetUI(string panelPath)
    {
        m_UIDics.TryGetValue(panelPath, out UIBase ui);
        return ui;
    }
}