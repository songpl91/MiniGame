using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

/*
使用样例：
m_HoldDetector = GetComponent<ButtonHoldDetector>();
if (m_HoldDetector)
{
    m_HoldDetector.requiredHoldTime = holdTime;
    m_HoldDetector.intervalTime = intervalTime;
    m_HoldDetector.detectFirstHolding = true;
    m_HoldDetector.onHold.AddListener(OnHold);
    m_HoldDetector.onHolding.AddListener(evUseBombInterval.Invoke);
    m_HoldDetector.onStartHold.AddListener(OnStartHold);
    m_HoldDetector.onAbortHold.AddListener(OnAbortHold);
}
*/
/// <summary>
/// 长按按钮事件
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonHoldDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Serializable]
    public class ButtonHoldEvent : UnityEvent
    {
    }

    [Serializable]
    public class ButtonHoldingEvent : UnityEvent
    {
    }

    private Button m_Btn;

    private void Awake()
    {
        m_Btn = GetComponent<Button>();
    }

    [FormerlySerializedAs("onStartHold")] [SerializeField]
    private ButtonHoldEvent m_OnStartHold = new ButtonHoldEvent();

    public ButtonHoldEvent onStartHold
    {
        get { return m_OnStartHold; }
        set { m_OnStartHold = value; }
    }

    [FormerlySerializedAs("onAbortHold")] [SerializeField]
    private ButtonHoldEvent m_OnAbortHold = new ButtonHoldEvent();

    public ButtonHoldEvent onAbortHold
    {
        get { return m_OnAbortHold; }
        set { m_OnAbortHold = value; }
    }

    [FormerlySerializedAs("onFinishHold")] [SerializeField]
    private ButtonHoldEvent m_OnFinishHold = new ButtonHoldEvent();

    public ButtonHoldEvent onFinishHold
    {
        get { return m_OnFinishHold; }
        set { onFinishHold = value; }
    }

    [FormerlySerializedAs("onHold")] [SerializeField]
    private ButtonHoldEvent m_OnHold = new ButtonHoldEvent();

    public ButtonHoldEvent onHold
    {
        get { return m_OnHold; }
        set { m_OnHold = value; }
    }

    [FormerlySerializedAs("onHolding")] [SerializeField]
    private ButtonHoldingEvent m_OnHolding = new ButtonHoldingEvent();

    public ButtonHoldingEvent onHolding
    {
        get { return m_OnHolding; }
        set { m_OnHolding = value; }
    }

    public Button Button
    {
        get => m_Btn;
    }

    /// <summary>
    /// Hold事件的触发时间
    /// </summary>
    public float requiredHoldTime = 1.0f; // Time in seconds

    /// <summary>
    /// Holding事件的时间间隔
    /// </summary>
    public float intervalTime = 1.0f;

    /// <summary>
    /// 触发Hold事件之后，是否立即触发一次Holding
    /// </summary>
    public bool detectFirstHolding = true;

    public float catchClickTime = 0.3f;

    private bool isPointerDown = false;
    private bool prepareHold = false;
    private float pointerDownTimer = 0;
    private float intervalTimer = 0.0f;
    private bool isHolding = false;
    private bool startHold = false;


    public void OnPointerDown(PointerEventData eventData)
    {
        if (m_Btn.IsInteractable())
        {
            isPointerDown = true;
            prepareHold = true;
            pointerDownTimer = 0;
            m_OnStartHold.Invoke();
            eventData.Use();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        // if (isHolding)
        {
            m_OnFinishHold.Invoke();
        }
        isHolding = false;
        Reset();
        if (startHold)
        {
            m_OnAbortHold.Invoke();
            // eventData.Use();
            startHold = false;
        }

        // eventData.Use();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerDown = false;
        if (isHolding)
        {
            m_OnFinishHold.Invoke();
        }

        isHolding = false;
        Reset();
    }

    /// <summary>
    /// 手动解除按下状态
    /// </summary>
    public void DoFinishedHold()
    {
        if (isHolding)
        {
            isHolding = false;
            isPointerDown = false;
            Reset();
            m_OnFinishHold.Invoke();
        }
    }

    private void Update()
    {
        if (isPointerDown)
        {
            if (prepareHold)
            {
                pointerDownTimer += Time.deltaTime;
                if (!startHold && pointerDownTimer >= catchClickTime)
                {
                    m_OnStartHold.Invoke();
                    startHold = true;
                }

                if (pointerDownTimer >= requiredHoldTime)
                {
                    m_OnHold.Invoke();
                    isHolding = true;
                    startHold = false;
                    Reset();
                    if (detectFirstHolding)
                    {
                        m_OnHolding.Invoke();
                    }

                    intervalTimer = 0.0f;
                }
            }

            if (isHolding)
            {
                intervalTimer += Time.deltaTime;
                if (intervalTimer >= intervalTime)
                {
                    // Debug.Log("Holding");
                    m_OnHolding.Invoke();
                    intervalTimer = 0.0f;
                }
            }
        }
    }

    private void Reset()
    {
        prepareHold = false;
        pointerDownTimer = 0;
    }

    public void Clear()
    {
        isPointerDown = false;
        isHolding = false;
        prepareHold = false;
        pointerDownTimer = 0;
        startHold = false;
    }
}