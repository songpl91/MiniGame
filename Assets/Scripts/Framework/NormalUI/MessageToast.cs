using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 消息广告公共UI
/// </summary>
public class MessageToast : MonoBehaviour
{
    public CanvasGroup m_CanvasGroup;
    public RectTransform m_CtnBg;
    public TMP_Text m_LabelTip;
    private Sequence m_Ani;

    public void Show(string message, Action complete)
    {
        m_LabelTip.text = message;

        m_CtnBg.localScale = Vector3.zero;
        m_CtnBg.anchoredPosition = new Vector2(0, 405);
        m_CanvasGroup.alpha = 1;
        m_CanvasGroup.blocksRaycasts = false;

        m_Ani?.Kill();
        m_Ani = DOTween.Sequence();

        m_Ani.Append(m_CtnBg.DOScale(Vector3.one, 0.2f));
        m_Ani.Append(m_CtnBg.DOAnchorPosY(548, 0.5f));
        m_Ani.Append(m_CanvasGroup.DOFade(0, 0.3f));

        m_Ani.OnComplete(delegate ()
        {
            m_Ani = null;
            complete?.Invoke();
        });
        m_Ani.Play();
    }
}