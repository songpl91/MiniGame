// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
//
// [RequireComponent(typeof(Button))]
// public class SwitchButton : MonoBehaviour
// {
//     protected Button m_Btn;
//
//     [SerializeField] protected Image m_Image;
//     [SerializeField] protected Sprite m_SpriteOn;
//     [SerializeField] protected Sprite m_SpriteOff;
//
//     [SerializeField] protected Image m_Backgroud;
//     [SerializeField] protected Sprite m_BgOn;
//     [SerializeField] protected Sprite m_BgOff;
//
//     protected bool isOn;
//     protected Action<bool> clickAction;
//
//     public bool Enabled
//     {
//         get { return m_Btn.enabled; }
//         set { m_Btn.enabled = value; }
//     }
//
//     private void Awake()
//     {
//         m_Btn = GetComponent<Button>();
//     }
//
//     protected virtual void Start()
//     {
//         m_Btn.onClick.AddListener(SwitchState);
//         UpdateState();
//     }
//
//     public virtual void AddListener(bool isOn, Action<bool> clickAction)
//     {
//         this.isOn = isOn;
//         this.clickAction = clickAction;
//         UpdateState();
//     }
//
//     protected virtual void SwitchState()
//     {
//         isOn = !isOn;
//         clickAction?.Invoke(isOn);
//         UpdateState();
//         AudioManager.Instance.PlayAudioEffect(AudioConst.CommonBtnClick);
//     }
//
//     protected virtual void UpdateState()
//     {
//         SetBGSprite();
//         if (m_Image != null)
//         {
//             m_Image.sprite = isOn ? m_SpriteOn : m_SpriteOff;
//             m_Image.SetNativeSize();
//         }
//     }
//
//     private void SetBGSprite()
//     {
//         if (m_Backgroud == null)
//         {
//             return;
//         }
//
//         m_Backgroud.sprite = isOn ? m_BgOn : m_BgOff;
//     }
// }
