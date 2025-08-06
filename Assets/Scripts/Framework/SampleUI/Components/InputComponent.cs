using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Framework.SampleUI.Core;

namespace Framework.SampleUI.Components
{
    /// <summary>
    /// 输入组件
    /// 为UI面板提供输入事件处理功能
    /// </summary>
    public class InputComponent : SampleUIComponent, IPointerEnterHandler, IPointerExitHandler, 
        IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        #region 字段和属性
        
        /// <summary>
        /// 输入配置
        /// </summary>
        public InputConfig Config { get; set; } = new InputConfig();
        
        /// <summary>
        /// 是否启用输入
        /// </summary>
        public bool InputEnabled { get; set; } = true;
        
        /// <summary>
        /// 是否正在拖拽
        /// </summary>
        public bool IsDragging { get; private set; }
        
        /// <summary>
        /// 是否鼠标悬停
        /// </summary>
        public bool IsHovering { get; private set; }
        
        /// <summary>
        /// 是否按下
        /// </summary>
        public bool IsPressed { get; private set; }
        
        /// <summary>
        /// 拖拽开始位置
        /// </summary>
        public Vector2 DragStartPosition { get; private set; }
        
        /// <summary>
        /// 当前拖拽位置
        /// </summary>
        public Vector2 CurrentDragPosition { get; private set; }
        
        /// <summary>
        /// 拖拽偏移量
        /// </summary>
        public Vector2 DragOffset => CurrentDragPosition - DragStartPosition;
        
        /// <summary>
        /// 键盘输入监听器
        /// </summary>
        private Dictionary<KeyCode, Action> keyListeners = new Dictionary<KeyCode, Action>();
        
        /// <summary>
        /// 组合键监听器
        /// </summary>
        private Dictionary<string, Action> comboKeyListeners = new Dictionary<string, Action>();
        
        /// <summary>
        /// 面板的RectTransform
        /// </summary>
        private RectTransform panelRect;
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 鼠标进入事件
        /// </summary>
        public event Action<PointerEventData> OnMouseEnter;
        
        /// <summary>
        /// 鼠标离开事件
        /// </summary>
        public event Action<PointerEventData> OnMouseExit;
        
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public event Action<PointerEventData> OnMouseDown;
        
        /// <summary>
        /// 鼠标抬起事件
        /// </summary>
        public event Action<PointerEventData> OnMouseUp;
        
        /// <summary>
        /// 鼠标点击事件
        /// </summary>
        public event Action<PointerEventData> OnMouseClick;
        
        /// <summary>
        /// 拖拽开始事件
        /// </summary>
        public event Action<PointerEventData> OnDragStart;
        
        /// <summary>
        /// 拖拽中事件
        /// </summary>
        public event Action<PointerEventData> OnDragging;
        
        /// <summary>
        /// 拖拽结束事件
        /// </summary>
        public event Action<PointerEventData> OnDragEnd;
        
        /// <summary>
        /// 键盘按键事件
        /// </summary>
        public event Action<KeyCode> OnKeyPressed;
        
        /// <summary>
        /// 组合键事件
        /// </summary>
        public event Action<string> OnComboKeyPressed;
        
        #endregion
        
        #region 初始化
        
        protected override void OnInitialize()
        {
            // 获取面板的RectTransform
            if (OwnerPanel is MonoBehaviour mono)
            {
                panelRect = mono.GetComponent<RectTransform>();
            }
            
            // 注册默认快捷键
            RegisterDefaultHotkeys();
        }
        
        /// <summary>
        /// 注册默认快捷键
        /// </summary>
        private void RegisterDefaultHotkeys()
        {
            if (Config.enableEscapeKey)
            {
                RegisterKey(KeyCode.Escape, () => {
                    if (OwnerPanel != null)
                    {
                        OwnerPanel.Hide();
                    }
                });
            }
            
            if (Config.enableEnterKey)
            {
                RegisterKey(KeyCode.Return, () => {
                    OnKeyPressed?.Invoke(KeyCode.Return);
                });
                
                RegisterKey(KeyCode.KeypadEnter, () => {
                    OnKeyPressed?.Invoke(KeyCode.KeypadEnter);
                });
            }
            
            if (Config.enableTabKey)
            {
                RegisterKey(KeyCode.Tab, () => {
                    OnKeyPressed?.Invoke(KeyCode.Tab);
                });
            }
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 注册按键监听
        /// </summary>
        /// <param name="key">按键</param>
        /// <param name="callback">回调</param>
        public void RegisterKey(KeyCode key, Action callback)
        {
            keyListeners[key] = callback;
        }
        
        /// <summary>
        /// 注销按键监听
        /// </summary>
        /// <param name="key">按键</param>
        public void UnregisterKey(KeyCode key)
        {
            keyListeners.Remove(key);
        }
        
        /// <summary>
        /// 注册组合键监听
        /// </summary>
        /// <param name="comboKey">组合键字符串（如"Ctrl+S"）</param>
        /// <param name="callback">回调</param>
        public void RegisterComboKey(string comboKey, Action callback)
        {
            comboKeyListeners[comboKey.ToLower()] = callback;
        }
        
        /// <summary>
        /// 注销组合键监听
        /// </summary>
        /// <param name="comboKey">组合键字符串</param>
        public void UnregisterComboKey(string comboKey)
        {
            comboKeyListeners.Remove(comboKey.ToLower());
        }
        
        /// <summary>
        /// 启用输入
        /// </summary>
        public void EnableInput()
        {
            InputEnabled = true;
        }
        
        /// <summary>
        /// 禁用输入
        /// </summary>
        public void DisableInput()
        {
            InputEnabled = false;
            
            // 重置状态
            IsHovering = false;
            IsPressed = false;
            IsDragging = false;
        }
        
        /// <summary>
        /// 模拟点击
        /// </summary>
        public void SimulateClick()
        {
            if (!InputEnabled) return;
            
            var eventData = new PointerEventData(EventSystem.current);
            OnPointerClick(eventData);
        }
        
        #endregion
        
        #region 更新
        
        protected void OnUpdate()
        {
            if (!InputEnabled) return;
            
            // 检查键盘输入
            CheckKeyboardInput();
            
            // 检查组合键输入
            CheckComboKeyInput();
        }
        
        /// <summary>
        /// 检查键盘输入
        /// </summary>
        private void CheckKeyboardInput()
        {
            foreach (var kvp in keyListeners)
            {
                if (Input.GetKeyDown(kvp.Key))
                {
                    kvp.Value?.Invoke();
                    OnKeyPressed?.Invoke(kvp.Key);
                }
            }
        }
        
        /// <summary>
        /// 检查组合键输入
        /// </summary>
        private void CheckComboKeyInput()
        {
            foreach (var kvp in comboKeyListeners)
            {
                if (IsComboKeyPressed(kvp.Key))
                {
                    kvp.Value?.Invoke();
                    OnComboKeyPressed?.Invoke(kvp.Key);
                }
            }
        }
        
        /// <summary>
        /// 检查组合键是否按下
        /// </summary>
        /// <param name="comboKey">组合键字符串</param>
        /// <returns>是否按下</returns>
        private bool IsComboKeyPressed(string comboKey)
        {
            string[] keys = comboKey.Split('+');
            if (keys.Length < 2) return false;
            
            bool allPressed = true;
            KeyCode mainKey = KeyCode.None;
            
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i].Trim().ToLower();
                
                if (i == keys.Length - 1)
                {
                    // 最后一个键需要是GetKeyDown
                    if (System.Enum.TryParse(key, true, out mainKey))
                    {
                        if (!Input.GetKeyDown(mainKey))
                        {
                            allPressed = false;
                            break;
                        }
                    }
                }
                else
                {
                    // 修饰键需要是GetKey
                    KeyCode modifierKey = GetModifierKey(key);
                    if (modifierKey != KeyCode.None)
                    {
                        if (!Input.GetKey(modifierKey))
                        {
                            allPressed = false;
                            break;
                        }
                    }
                }
            }
            
            return allPressed;
        }
        
        /// <summary>
        /// 获取修饰键
        /// </summary>
        /// <param name="keyName">键名</param>
        /// <returns>键码</returns>
        private KeyCode GetModifierKey(string keyName)
        {
            switch (keyName)
            {
                case "ctrl":
                case "control":
                    return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ? KeyCode.LeftControl : KeyCode.None;
                case "shift":
                    return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? KeyCode.LeftShift : KeyCode.None;
                case "alt":
                    return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ? KeyCode.LeftAlt : KeyCode.None;
                default:
                    if (System.Enum.TryParse(keyName, true, out KeyCode key))
                    {
                        return key;
                    }
                    return KeyCode.None;
            }
        }
        
        #endregion
        
        #region 鼠标事件实现
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!InputEnabled) return;
            
            IsHovering = true;
            OnMouseEnter?.Invoke(eventData);
            
            if (Config.enableHoverEffects)
            {
                // 可以在这里添加悬停效果
                Debug.Log("[InputComponent] 鼠标进入");
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!InputEnabled) return;
            
            IsHovering = false;
            OnMouseExit?.Invoke(eventData);
            
            if (Config.enableHoverEffects)
            {
                Debug.Log("[InputComponent] 鼠标离开");
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!InputEnabled) return;
            
            IsPressed = true;
            OnMouseDown?.Invoke(eventData);
            
            if (Config.enableClickEffects)
            {
                Debug.Log("[InputComponent] 鼠标按下");
            }
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!InputEnabled) return;
            
            IsPressed = false;
            OnMouseUp?.Invoke(eventData);
            
            if (Config.enableClickEffects)
            {
                Debug.Log("[InputComponent] 鼠标抬起");
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!InputEnabled) return;
            
            OnMouseClick?.Invoke(eventData);
            
            if (Config.enableClickEffects)
            {
                Debug.Log("[InputComponent] 鼠标点击");
            }
        }
        
        #endregion
        
        #region 拖拽事件实现
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!InputEnabled || !Config.enableDrag) return;
            
            IsDragging = true;
            DragStartPosition = eventData.position;
            CurrentDragPosition = eventData.position;
            
            OnDragStart?.Invoke(eventData);
            
            Debug.Log("[InputComponent] 开始拖拽");
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!InputEnabled || !Config.enableDrag || !IsDragging) return;
            
            CurrentDragPosition = eventData.position;
            
            if (Config.enablePanelDrag && panelRect != null)
            {
                // 移动面板
                Vector2 deltaPosition = eventData.delta;
                panelRect.anchoredPosition += deltaPosition;
            }
            
            OnDragging?.Invoke(eventData);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!InputEnabled || !Config.enableDrag) return;
            
            IsDragging = false;
            CurrentDragPosition = eventData.position;
            
            OnDragEnd?.Invoke(eventData);
            
            Debug.Log("[InputComponent] 结束拖拽");
        }
        
        #endregion
        
        #region 销毁
        
        protected override void OnDestroyed()
        {
            // 清理监听器
            keyListeners.Clear();
            comboKeyListeners.Clear();
            
            panelRect = null;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 输入配置
    /// </summary>
    [System.Serializable]
    public class InputConfig
    {
        [Header("基础设置")]
        public bool enableInput = true;
        
        [Header("鼠标设置")]
        public bool enableHoverEffects = true;
        public bool enableClickEffects = true;
        
        [Header("拖拽设置")]
        public bool enableDrag = false;
        public bool enablePanelDrag = false;
        public float dragThreshold = 5f;
        
        [Header("键盘设置")]
        public bool enableEscapeKey = true;
        public bool enableEnterKey = true;
        public bool enableTabKey = true;
        
        [Header("高级设置")]
        public bool blockRaycast = true;
        public bool ignoreParentGroups = false;
        public float doubleClickTime = 0.3f;
    }
}