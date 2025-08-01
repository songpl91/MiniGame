using UnityEngine;
using UnityEngine.UI;

namespace Framework.TraditionalUI.Panels
{
    /// <summary>
    /// 消息框面板
    /// 通用的消息提示和确认对话框
    /// </summary>
    public class MessageBoxPanel : TraditionalUIPanel
    {
        #region UI组件引用
        
        [Header("标题和内容")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text messageText;
        [SerializeField] private Image iconImage;
        
        [Header("按钮")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text confirmButtonText;
        [SerializeField] private Text cancelButtonText;
        
        [Header("图标精灵")]
        [SerializeField] private Sprite infoIcon;
        [SerializeField] private Sprite warningIcon;
        [SerializeField] private Sprite errorIcon;
        [SerializeField] private Sprite questionIcon;
        
        #endregion
        
        #region 私有变量
        
        private MessageBoxData messageData;
        
        #endregion
        
        #region 初始化方法
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // 设置按钮事件
            SetupButtonEvents();
        }
        
        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtonEvents()
        {
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClick);
            
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClick);
            
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClick);
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 设置消息框数据
        /// </summary>
        /// <param name="data">消息框数据</param>
        public void SetData(MessageBoxData data)
        {
            messageData = data;
            UpdateUI();
        }
        
        /// <summary>
        /// 显示信息消息框
        /// </summary>
        public static void ShowInfo(string title, string message, System.Action onConfirm = null)
        {
            // var data = new MessageBoxData
            // {
            //     title = title,
            //     message = message,
            //     messageType = MessageType.Info,
            //     showCancelButton = false,
            //     onConfirm = onConfirm
            // };
            //
            // TraditionalUIManager.Instance.OpenPanel("MessageBox", data);
        }
        
        /// <summary>
        /// 显示警告消息框
        /// </summary>
        public static void ShowWarning(string title, string message, System.Action onConfirm = null)
        {
            // var data = new MessageBoxData
            // {
            //     title = title,
            //     message = message,
            //     messageType = MessageType.Warning,
            //     showCancelButton = false,
            //     onConfirm = onConfirm
            // };
            //
            // TraditionalUIManager.Instance.OpenPanel("MessageBox", data);
        }
        
        /// <summary>
        /// 显示错误消息框
        /// </summary>
        public static void ShowError(string title, string message, System.Action onConfirm = null)
        {
            // var data = new MessageBoxData
            // {
            //     title = title,
            //     message = message,
            //     messageType = MessageType.Error,
            //     showCancelButton = false,
            //     onConfirm = onConfirm
            // };
            //
            // TraditionalUIManager.Instance.OpenPanel("MessageBox", data);
        }
        
        /// <summary>
        /// 显示确认消息框
        /// </summary>
        public static void ShowConfirm(string title, string message, System.Action onConfirm = null, System.Action onCancel = null)
        {
            // var data = new MessageBoxData
            // {
            //     title = title,
            //     message = message,
            //     messageType = MessageType.Question,
            //     showCancelButton = true,
            //     onConfirm = onConfirm,
            //     onCancel = onCancel
            // };
            //
            // TraditionalUIManager.Instance.OpenPanel("MessageBox", data);
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUI()
        {
            if (messageData == null) return;
            
            // 设置标题
            if (titleText != null)
            {
                titleText.text = messageData.title;
            }
            
            // 设置消息内容
            if (messageText != null)
            {
                messageText.text = messageData.message;
            }
            
            // 设置图标
            SetIcon();
            
            // 设置按钮
            SetupButtons();
        }
        
        /// <summary>
        /// 设置图标
        /// </summary>
        private void SetIcon()
        {
            if (iconImage == null) return;
            //
            // Sprite iconSprite = null;
            // switch (messageData.messageType)
            // {
            //     case MessageType.Info:
            //         iconSprite = infoIcon;
            //         break;
            //     case MessageType.Warning:
            //         iconSprite = warningIcon;
            //         break;
            //     case MessageType.Error:
            //         iconSprite = errorIcon;
            //         break;
            //     case MessageType.Question:
            //         iconSprite = questionIcon;
            //         break;
            // }
            
            // if (iconSprite != null)
            // {
            //     iconImage.sprite = iconSprite;
            //     iconImage.gameObject.SetActive(true);
            // }
            // else
            // {
            //     iconImage.gameObject.SetActive(false);
            // }
        }
        
        /// <summary>
        /// 设置按钮
        /// </summary>
        private void SetupButtons()
        {
            // // 设置确认按钮
            // if (confirmButton != null)
            // {
            //     confirmButton.gameObject.SetActive(true);
            //     
            //     if (confirmButtonText != null)
            //     {
            //         confirmButtonText.text = string.IsNullOrEmpty(messageData.confirmButtonText) 
            //             ? "确定" : messageData.confirmButtonText;
            //     }
            // }
            //
            // // 设置取消按钮
            // if (cancelButton != null)
            // {
            //     cancelButton.gameObject.SetActive(messageData.showCancelButton);
            //     
            //     if (cancelButtonText != null && messageData.showCancelButton)
            //     {
            //         cancelButtonText.text = string.IsNullOrEmpty(messageData.cancelButtonText) 
            //             ? "取消" : messageData.cancelButtonText;
            //     }
            // }
            //
            // // 设置关闭按钮
            // if (closeButton != null)
            // {
            //     closeButton.gameObject.SetActive(messageData.showCloseButton);
            // }
        }
        
        #endregion
        
        #region 事件处理方法
        
        /// <summary>
        /// 确认按钮点击
        /// </summary>
        private void OnConfirmClick()
        {
            Debug.Log("[消息框] 确认按钮点击");
            
            // 执行确认回调
            messageData?.onConfirm?.Invoke();
            
            // 关闭消息框
            // TraditionalUIManager.Instance.ClosePanel(this);
        }
        
        /// <summary>
        /// 取消按钮点击
        /// </summary>
        private void OnCancelClick()
        {
            Debug.Log("[消息框] 取消按钮点击");
            
            // 执行取消回调
            messageData?.onCancel?.Invoke();
            
            // 关闭消息框
            // TraditionalUIManager.Instance.ClosePanel(this);
        }
        
        /// <summary>
        /// 关闭按钮点击
        /// </summary>
        private void OnCloseClick()
        {
            Debug.Log("[消息框] 关闭按钮点击");
            
            // 执行取消回调（关闭按钮等同于取消）
            messageData?.onCancel?.Invoke();
            
            // 关闭消息框
            // TraditionalUIManager.Instance.ClosePanel(this);
        }
        
        #endregion
        
        #region 输入处理
        
        // protected override void Update()
        // {
        //     base.Update();
        //     
        //     // 处理键盘输入
        //     HandleKeyboardInput();
        // }
        
        /// <summary>
        /// 处理键盘输入
        /// </summary>
        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                // 回车键确认
                OnConfirmClick();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                // ESC键取消
                if (messageData != null && messageData.showCancelButton)
                {
                    OnCancelClick();
                }
                else
                {
                    OnCloseClick();
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 消息类型枚举
    /// </summary>
    public enum MessageType
    {
        Info,       // 信息
        Warning,    // 警告
        Error,      // 错误
        Question    // 询问
    }
    
    /// <summary>
    /// 消息框数据类（扩展版本）
    /// </summary>
    // public class MessageBoxData
    // {
    //     [Header("基本信息")]
    //     public string title = "提示";
    //     public string message = "";
    //     public MessageType messageType = MessageType.Info;
    //     
    //     [Header("按钮设置")]
    //     public bool showCancelButton = false;
    //     public bool showCloseButton = true;
    //     public string confirmButtonText = "";
    //     public string cancelButtonText = "";
    //     
    //     [Header("回调事件")]
    //     public System.Action onConfirm;
    //     public System.Action onCancel;
    //     
    //     [Header("高级设置")]
    //     public bool autoClose = false;
    //     public float autoCloseTime = 3.0f;
    //     public bool pauseGame = false;
    //     public bool blockInput = true;
    // }
}