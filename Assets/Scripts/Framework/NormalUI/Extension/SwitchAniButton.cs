using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchAniButton : SwitchButton
{
    [SerializeField] protected RectTransform m_Handler;

    [SerializeField] protected GameObject m_OnMark;
    [SerializeField] protected GameObject m_OffMark;

    [SerializeField] protected float m_MoveTime = 0.2f;
    [SerializeField] protected Vector2 m_HandlerOnPos = new Vector2(50f, 0);

    protected bool m_IsPlaying;

    protected override void Start()
    {
        base.Start();

        m_Handler.anchoredPosition = isOn ? m_HandlerOnPos : -m_HandlerOnPos;
        SetOnOffMarkActive(isOn, !isOn);
    }

    public override void AddListener(bool isOn, Action<bool> clickAction)
    {
        base.AddListener(isOn, clickAction);

        m_Handler.anchoredPosition = isOn ? m_HandlerOnPos : -m_HandlerOnPos;
        SetOnOffMarkActive(isOn, !isOn);
    }

    public bool ChangeState(bool isOn)
    {
        if (this.isOn == isOn)
        {
            return this.isOn;
        }

        if (m_IsPlaying)
        {
            return this.isOn;
        }

        SwitchState();
        return this.isOn;
    }

    protected override void SwitchState()
    {
        if (m_IsPlaying)
        {
            return;
        }

        base.SwitchState();

        StartCoroutine(MoveHandle());
    }

    protected IEnumerator MoveHandle()
    {
        m_IsPlaying = true;
        SetOnOffMarkActive(false, false);

        Vector2 PosX;
        Vector2 targetValue = isOn ? m_HandlerOnPos : -m_HandlerOnPos;
        float time = 0f;

        while (time < m_MoveTime)
        {
            yield return new WaitForEndOfFrame();

            time += Time.deltaTime;
            PosX = time / m_MoveTime * targetValue * 2 - targetValue;
            m_Handler.anchoredPosition = PosX;
        }

        m_Handler.anchoredPosition = targetValue;

        SetOnOffMarkActive(isOn, !isOn);
        m_IsPlaying = false;
    }

    private void SetOnOffMarkActive(bool on, bool off)
    {
        if (m_OnMark != null)
        {
            m_OnMark.SafeSetActive(on);
        }

        if (m_OffMark != null)
        {
            m_OffMark.SafeSetActive(off);
        }
    }
}
