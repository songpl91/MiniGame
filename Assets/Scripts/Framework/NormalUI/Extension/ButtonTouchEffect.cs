// using UnityEngine;
// using UnityEngine.UI;
// using DG.Tweening;
// using UnityEngine.EventSystems;
// using System;
//
// [RequireComponent(typeof(Button))]
// public class ButtonTouchEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
// {
//     [NonSerialized] public Button m_Btn;
//
//     [NonSerialized] public RectTransform m_RectTransform;
//     private float m_LocalScale;
//
//     private bool m_IsDelayInvoke;
//     private bool m_IsPlayAudio;
//     private bool m_IsVibrate;
//
//     private bool m_IsCanTouch;
//     private bool m_CanClick;
//     private Action m_Callback;
//
//     private Action<int> m_ParamsCallBack;
//     private int m_Param;
//
//     private Sequence m_Ani;
//
//     private void Awake()
//     {
//         m_Btn = GetComponent<Button>();
//         m_RectTransform = GetComponent<RectTransform>();
//         m_Btn.transition = Selectable.Transition.None;
//
//         m_LocalScale = transform.localScale.x;
//         if (m_LocalScale < 0.1f)
//         {
//             m_LocalScale = 1f;
//         }
//
//         m_IsCanTouch = true;
//         m_CanClick = true;
//     }
//
//     private void OnDestroy()
//     {
//         InitAni(false);
//     }
//
//     public void KillAni()
//     {
//         InitAni(false);
//     }
//
//     public void SetEnable(bool state)
//     {
//         GetButton().enabled = m_IsCanTouch = state;
//     }
//
//     public void OnPointerDown(PointerEventData pointerEventData)
//     {
//         if (!m_IsCanTouch)
//         {
//             transform.localScale = m_LocalScale * Vector3.one;
//             return;
//         }
//
//         InitAni(true);
//         m_Ani.Append(transform.DOScale(m_LocalScale * 0.96f, 0.2f));
//         m_Ani.Play();
//     }
//
//     public void OnPointerUp(PointerEventData pointerEventData)
//     {
//         if (!m_IsCanTouch)
//         {
//             transform.localScale = m_LocalScale * Vector3.one;
//             return;
//         }
//
//         InitAni(true);
//         m_Ani.Append(transform.DOScale(m_LocalScale * 1.1f, 0.13f));
//         m_Ani.Append(transform.DOScale(m_LocalScale * 1f, 0.12f));
//         m_Ani.Play();
//     }
//
//     public void AddListener(Action<int> callback, int param, bool isDelayClick = true, bool isPlayAudio = true,
//         bool isVibrate = true)
//     {
//         if (m_Callback != null || m_ParamsCallBack != null)
//         {
//             return;
//         }
//
//         if (m_ParamsCallBack == callback)
//         {
//             return;
//         }
//
//         m_IsDelayInvoke = isDelayClick;
//         m_ParamsCallBack = callback;
//         m_Param = param;
//         m_IsPlayAudio = isPlayAudio;
//         m_IsVibrate = isVibrate;
//
//         GetButton().onClick.AddListener(
//             delegate ()
//             {
//                 if (!m_CanClick)
//                 {
//                     return;
//                 }
//
//                 if (m_IsPlayAudio)
//                 {
//                     AudioManager.Instance.PlayAudioEffect(AudioConst.CommonBtnClick);
//                 }
//
//                 if (m_IsVibrate)
//                 {
//                     VibrateManager.Instance.CommonBtnVibrate();
//                 }
//
//                 m_CanClick = false;
//                 m_ParamsCallBack?.Invoke(m_Param);
//                 Invoke(nameof(DelayClick), m_IsDelayInvoke ? 0.3f : 0.1f);
//             });
//     }
//
//     public void AddListener(Action callback, bool isDelayClick = true, bool isPlayAudio = true,
//         bool isVibrate = true)
//     {
//         if (m_Callback != null || m_ParamsCallBack != null)
//         {
//             return;
//         }
//
//         if (m_Callback == callback)
//         {
//             return;
//         }
//
//         m_IsDelayInvoke = isDelayClick;
//         m_Callback = callback;
//         m_IsPlayAudio = isPlayAudio;
//         m_IsVibrate = isVibrate;
//
//         GetButton().onClick.AddListener(
//             delegate ()
//             {
//                 if (!m_CanClick)
//                 {
//                     return;
//                 }
//
//                 if (m_IsPlayAudio)
//                 {
//                     AudioManager.Instance.PlayAudioEffect(AudioConst.CommonBtnClick);
//                 }
//
//                 if (m_IsVibrate)
//                 {
//                     VibrateManager.Instance.CommonBtnVibrate();
//                 }
//
//                 m_CanClick = false;
//                 m_Callback?.Invoke();
//                 Invoke(nameof(DelayClick), m_IsDelayInvoke ? 0.3f : 0.1f);
//             });
//     }
//
//     private void DelayClick()
//     {
//         m_CanClick = true;
//     }
//
//     private Button GetButton()
//     {
//         if (m_Btn == null)
//         {
//             m_Btn = GetComponent<Button>();
//         }
//
//         return m_Btn;
//     }
//
//     private void InitAni(bool isInit)
//     {
//         m_Ani?.Kill();
//         if (isInit)
//         {
//             m_Ani = DOTween.Sequence();
//             m_Ani.OnComplete(delegate () { m_Ani = null; });
//         }
//         else
//         {
//             m_Ani = null;
//         }
//     }
//
//     public void RemoveAllListeners()
//     {
//         m_Callback = null;
//         m_ParamsCallBack = null;
//     }
// }