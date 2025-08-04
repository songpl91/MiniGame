using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum PanelShowType
{
    /// <summary>
    /// 无任何动画
    /// </summary>
    None,

    /// <summary>
    /// 有弹性的掉落
    /// </summary>
    InOutBack,

    /// <summary>
    /// 变大
    /// </summary>
    Largen,

    /// <summary>
    /// 从中间展开
    /// </summary>
    Spread,

    /// <summary>
    /// 渐隐
    /// </summary>
    Fade,

    /// <summary>
    /// 开始变大  结束直接销毁
    /// </summary>
    StartLargen,

    /// <summary>
    /// 开始直接显示，结束变小
    /// </summary>
    EndLargen,
}

public enum UIType
{
    UI,
    UpUI,
    Dialog,
    UpDialog,
}

public class UIBase : MonoBehaviour
{
    // Panel出现和消失的动画类型
    public PanelShowType ShowType = PanelShowType.None;

    // ui类型
    public UIType UiType = UIType.UI;

    // UIPath 中路径;
    [System.NonSerialized] public string PanelPath;

    public RectTransform m_Content;

    protected CanvasGroup m_CanvasGroup;

    /// <summary>
    /// 界面全部打开回调
    /// </summary>
    public Action OnShowOverEvent;

    /// <summary>
    /// 界面全部关闭回调
    /// </summary>
    public Action OnCloseOverEvent;

    protected virtual void OnShow(object[] param)
    {
    }

    protected virtual void OnClose(Sequence sequence)
    {
    }

    protected virtual void OnShowOver()
    {
    }

    protected virtual void OnCloseOver()
    {
    }

    /// <summary>
    /// UIManager调用打开
    /// </summary>
    /// <param name="path"></param>
    /// <param name="param"></param>
    public void Show(string path, params object[] param)
    {
        OnShow(param);

        PanelPath = path;
        gameObject.SafeSetActive(true);
        InitCanvasGroup();

        if (m_Content == null || ShowType == PanelShowType.None)
        {
            DoShow();
            return;
        }

        Sequence ani = DOTween.Sequence();
        switch (ShowType)
        {
            case PanelShowType.InOutBack:
                m_Content.localPosition = Vector3.up * 2000f;
                ani.Append(m_Content.DOLocalMove(Vector3.zero, 1f).SetEase(Ease.InOutBack));
                break;
            case PanelShowType.Largen:
            case PanelShowType.StartLargen:
                m_Content.localScale = Vector3.one * 0.3f;
                m_CanvasGroup.alpha = 0;
                ani.Append(m_Content.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
                ani.Join(m_CanvasGroup.DOFade(1, 0.1f));
                break;
            case PanelShowType.Spread:
                m_Content.localScale = new Vector3(0, 1f, 1f);
                ani.Append(m_Content.DOScaleX(1f, 0.3f).SetEase(Ease.OutBack));
                break;
            case PanelShowType.Fade:
                m_CanvasGroup.alpha = 0;
                ani.Append(m_CanvasGroup.DOFade(1, 0.4f));
                break;
            case PanelShowType.None:
            case PanelShowType.EndLargen:
                break;
            default:
                break;
        }

        ani.OnComplete(DoShow);
        ani.Play();
    }

    /// <summary>
    /// UIManager调用关闭UI
    /// </summary>
    public void Close()
    {
        m_CanvasGroup.interactable = false;

        if (m_Content == null || ShowType == PanelShowType.None)
        {
            OnClose(null);
            DoClose();
            return;
        }

        Sequence ani = DOTween.Sequence();
        switch (ShowType)
        {
            case PanelShowType.InOutBack:
                ani.Append(m_Content.DOLocalMove(Vector3.up * 2000f, 1f).SetEase(Ease.InOutBack));
                break;
            case PanelShowType.Largen:
            case PanelShowType.EndLargen:
                ani.Append(m_Content.DOScale(Vector3.one * 0.3f, 0.3f).SetEase(Ease.InBack));
                ani.Insert(0.2f, m_CanvasGroup.DOFade(0, 0.1f));
                break;
            case PanelShowType.Spread:
                ani.Append(m_Content.DOScaleX(0f, 0.3f).SetEase(Ease.InBack));
                break;
            case PanelShowType.Fade:
                ani.Append(m_CanvasGroup.DOFade(0, 0.4f));
                break;
            case PanelShowType.None:
            case PanelShowType.StartLargen:
                break;
            default:
                break;
        }

        OnClose(ani);
        ani.OnComplete(DoClose);
        ani.Play();
    }

    /// <summary>
    /// UI打开动画播放完毕
    /// </summary>
    private void DoShow()
    {
        m_CanvasGroup.interactable = true;

        OnShowOver();

        OnShowOverEvent?.Invoke();
        OnShowOverEvent = null;
    }

    private void DoClose()
    {
        OnCloseOver();

        OnCloseOverEvent?.Invoke();
        OnCloseOverEvent = null;
        if (PanelPath == UIPath.LoadingPanel)
        {
            Destroy(gameObject);
        }
        else
        {
            AddressableManager.Instance.ReleaseInstance(gameObject);
        }
    }

    /// <summary>
    /// 初始化CanvasGroup
    /// </summary>
    private void InitCanvasGroup()
    {
        if (m_CanvasGroup != null)
        {
            return;
        }

        m_CanvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (m_CanvasGroup == null)
        {
            m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        m_CanvasGroup.interactable = false;
        m_CanvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// 关闭自身UI
    /// </summary>
    protected virtual void CloseSelfUI()
    {
        UIManager.Instance.CloseUI(this);
    }
}