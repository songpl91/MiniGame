using UnityEngine;
using UnityEngine.UI;
using Framework.SplUI.Core;

namespace Framework.SplUI.Examples
{
    /// <summary>
    /// 示例面板
    /// 展示如何使用SplUI框架创建UI面板
    /// </summary>
    public class ExamplePanel : SplUIBase
    {
        [Header("UI组件")]
        [SerializeField]
        private Button closeButton;
        
        [SerializeField]
        private Button confirmButton;
        
        [SerializeField]
        private Text titleText;
        
        [SerializeField]
        private Text contentText;
        
        [SerializeField]
        private InputField inputField;
        
        // 面板数据
        private string panelTitle = "示例面板";
        private string panelContent = "这是一个示例面板，展示SplUI框架的基本用法。";
        
        /// <summary>
        /// 初始化面板
        /// </summary>
        protected override void OnInitialize()
        {
            // 设置面板基本信息
            panelId = "ExamplePanel";
            displayName = "示例面板";
            panelType = SplUIType.Popup;
            priority = 1;
            
            // 设置动画
            showAnimation = SplUIAnimationType.FadeScale;
            hideAnimation = SplUIAnimationType.FadeScale;
            animationDuration = 0.3f;
            
            // 初始化UI组件
            InitializeUI();
            
            Debug.Log($"[ExamplePanel] 面板初始化完成: {panelId}");
        }
        
        /// <summary>
        /// 面板显示时调用
        /// </summary>
        /// <param name="data">传递的数据</param>
        protected override void OnShow(object data = null)
        {
            Debug.Log($"[ExamplePanel] 面板显示: {panelId}");
            
            // 更新UI内容
            UpdateUI();
            
            // 注册事件
            RegisterEvents();
        }
        
        /// <summary>
        /// 面板隐藏时调用
        /// </summary>
        protected override void OnHide()
        {
            Debug.Log($"[ExamplePanel] 面板隐藏: {panelId}");
            
            // 注销事件
            UnregisterEvents();
        }
        
        /// <summary>
        /// 面板销毁时调用
        /// </summary>
        protected override void OnPanelDestroy()
        {
            Debug.Log($"[ExamplePanel] 面板销毁: {panelId}");
            
            // 清理资源
            UnregisterEvents();
        }
        
        /// <summary>
        /// 面板刷新时调用
        /// </summary>
        /// <param name="data">刷新数据</param>
        protected override void OnRefresh(object data)
        {
            Debug.Log("[ExamplePanel] 面板刷新");
            
            // 刷新UI显示
            UpdateUI();
        }
        
        /// <summary>
        /// 初始化UI组件
        /// </summary>
        private void InitializeUI()
        {
            // 查找UI组件（如果没有在Inspector中设置）
            if (closeButton == null)
                closeButton = transform.Find("CloseButton")?.GetComponent<Button>();
            
            if (confirmButton == null)
                confirmButton = transform.Find("ConfirmButton")?.GetComponent<Button>();
            
            if (titleText == null)
                titleText = transform.Find("TitleText")?.GetComponent<Text>();
            
            if (contentText == null)
                contentText = transform.Find("ContentText")?.GetComponent<Text>();
            
            if (inputField == null)
                inputField = transform.Find("InputField")?.GetComponent<InputField>();
        }
        
        /// <summary>
        /// 更新UI内容
        /// </summary>
        private void UpdateUI()
        {
            if (titleText != null)
                titleText.text = panelTitle;
            
            if (contentText != null)
                contentText.text = panelContent;
            
            if (inputField != null)
                inputField.text = "";
        }
        
        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseButtonClick);
            
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmButtonClick);
        }
        
        /// <summary>
        /// 注销事件
        /// </summary>
        private void UnregisterEvents()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCloseButtonClick);
            
            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(OnConfirmButtonClick);
        }
        
        /// <summary>
        /// 关闭按钮点击事件
        /// </summary>
        private void OnCloseButtonClick()
        {
            Debug.Log("[ExamplePanel] 关闭按钮被点击");
            
            // 隐藏面板
            SplUIManager.Instance.HidePanel(panelId);
        }
        
        /// <summary>
        /// 确认按钮点击事件
        /// </summary>
        private void OnConfirmButtonClick()
        {
            Debug.Log("[ExamplePanel] 确认按钮被点击");
            
            string inputText = inputField != null ? inputField.text : "";
            Debug.Log($"[ExamplePanel] 输入内容: {inputText}");
            
            // 处理确认逻辑
            ProcessConfirm(inputText);
        }
        
        /// <summary>
        /// 处理确认逻辑
        /// </summary>
        /// <param name="inputText">输入文本</param>
        private void ProcessConfirm(string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                Debug.LogWarning("[ExamplePanel] 输入内容为空");
                return;
            }
            
            // 这里可以添加具体的业务逻辑
            Debug.Log($"[ExamplePanel] 处理确认逻辑: {inputText}");
            
            // 隐藏面板
            SplUIManager.Instance.HidePanel(panelId);
        }
        
        /// <summary>
        /// 设置面板标题
        /// </summary>
        /// <param name="title">标题</param>
        public void SetTitle(string title)
        {
            panelTitle = title;
            if (titleText != null)
                titleText.text = panelTitle;
        }
        
        /// <summary>
        /// 设置面板内容
        /// </summary>
        /// <param name="content">内容</param>
        public void SetContent(string content)
        {
            panelContent = content;
            if (contentText != null)
                contentText.text = panelContent;
        }
        
        /// <summary>
        /// 获取输入内容
        /// </summary>
        /// <returns>输入文本</returns>
        public string GetInputText()
        {
            return inputField != null ? inputField.text : "";
        }
        
        /// <summary>
        /// 设置输入内容
        /// </summary>
        /// <param name="text">输入文本</param>
        public void SetInputText(string text)
        {
            if (inputField != null)
                inputField.text = text;
        }
    }
}