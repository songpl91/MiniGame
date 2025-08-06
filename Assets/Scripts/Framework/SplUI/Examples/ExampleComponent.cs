using UnityEngine;
using UnityEngine.UI;
using Framework.SplUI.Core;

namespace Framework.SplUI.Examples
{
    /// <summary>
    /// 示例UI组件
    /// 展示如何创建可复用的UI组件
    /// </summary>
    public class ExampleComponent : SplUIComponent
    {
        [Header("组件UI")]
        [SerializeField]
        private Button actionButton;
        
        [SerializeField]
        private Text statusText;
        
        [SerializeField]
        private Image iconImage;
        
        [SerializeField]
        private Slider progressSlider;
        
        [Header("组件设置")]
        [SerializeField]
        private string defaultStatusText = "准备就绪";
        
        [SerializeField]
        private Color normalColor = Color.white;
        
        [SerializeField]
        private Color activeColor = Color.green;
        
        [SerializeField]
        private Color disabledColor = Color.gray;
        
        // 组件状态
        private bool isWorking = false;
        private float progress = 0f;
        
        /// <summary>
        /// 是否正在工作
        /// </summary>
        public bool IsWorking => isWorking;
        
        /// <summary>
        /// 当前进度
        /// </summary>
        public float Progress => progress;
        
        /// <summary>
        /// 初始化组件
        /// </summary>
        protected override void OnInitialize()
        {
            Debug.Log($"[ExampleComponent] 组件初始化: {name}");
            
            // 初始化UI组件
            InitializeUI();
            
            // 设置初始状态
            SetStatus(defaultStatusText);
            SetProgress(0f);
            
            // 注册事件
            RegisterEvents();
        }
        
        /// <summary>
        /// 组件更新
        /// </summary>
        protected override void OnComponentUpdate()
        {
            // 模拟工作进度更新
            if (isWorking)
            {
                UpdateWorkProgress();
            }
        }
        
        /// <summary>
        /// 组件销毁
        /// </summary>
        protected override void OnComponentDestroy()
        {
            Debug.Log($"[ExampleComponent] 组件销毁: {name}");
            
            // 注销事件
            UnregisterEvents();
        }
        
        /// <summary>
        /// 组件激活
        /// </summary>
        protected override void OnActivate()
        {
            Debug.Log($"[ExampleComponent] 组件激活: {name}");
            UpdateVisualState();
        }
        
        /// <summary>
        /// 组件停用
        /// </summary>
        protected override void OnDeactivate()
        {
            Debug.Log($"[ExampleComponent] 组件停用: {name}");
            StopWork();
        }
        
        /// <summary>
        /// 组件重置
        /// </summary>
        protected override void OnReset()
        {
            Debug.Log($"[ExampleComponent] 组件重置: {name}");
            
            StopWork();
            SetStatus(defaultStatusText);
            SetProgress(0f);
            UpdateVisualState();
        }
        
        /// <summary>
        /// 初始化UI组件
        /// </summary>
        private void InitializeUI()
        {
            // 查找UI组件（如果没有在Inspector中设置）
            if (actionButton == null)
                actionButton = GetComponentInChildren<Button>();
            
            if (statusText == null)
                statusText = GetComponentInChildren<Text>();
            
            if (iconImage == null)
                iconImage = GetComponentInChildren<Image>();
            
            if (progressSlider == null)
                progressSlider = GetComponentInChildren<Slider>();
            
            // 初始化进度条
            if (progressSlider != null)
            {
                progressSlider.minValue = 0f;
                progressSlider.maxValue = 1f;
                progressSlider.value = 0f;
            }
        }
        
        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            if (actionButton != null)
                actionButton.onClick.AddListener(OnActionButtonClick);
        }
        
        /// <summary>
        /// 注销事件
        /// </summary>
        private void UnregisterEvents()
        {
            if (actionButton != null)
                actionButton.onClick.RemoveListener(OnActionButtonClick);
        }
        
        /// <summary>
        /// 动作按钮点击事件
        /// </summary>
        private void OnActionButtonClick()
        {
            Debug.Log($"[ExampleComponent] 动作按钮被点击: {name}");
            
            if (isWorking)
            {
                StopWork();
            }
            else
            {
                StartWork();
            }
        }
        
        /// <summary>
        /// 开始工作
        /// </summary>
        public void StartWork()
        {
            if (isWorking)
                return;
            
            Debug.Log($"[ExampleComponent] 开始工作: {name}");
            
            isWorking = true;
            progress = 0f;
            
            SetStatus("工作中...");
            UpdateVisualState();
        }
        
        /// <summary>
        /// 停止工作
        /// </summary>
        public void StopWork()
        {
            if (!isWorking)
                return;
            
            Debug.Log($"[ExampleComponent] 停止工作: {name}");
            
            isWorking = false;
            
            SetStatus(progress >= 1f ? "工作完成" : "工作停止");
            UpdateVisualState();
        }
        
        /// <summary>
        /// 更新工作进度
        /// </summary>
        private void UpdateWorkProgress()
        {
            progress += Time.deltaTime * 0.2f; // 5秒完成
            progress = Mathf.Clamp01(progress);
            
            SetProgress(progress);
            
            if (progress >= 1f)
            {
                StopWork();
            }
        }
        
        /// <summary>
        /// 设置状态文本
        /// </summary>
        /// <param name="status">状态文本</param>
        public void SetStatus(string status)
        {
            if (statusText != null)
                statusText.text = status;
        }
        
        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="value">进度值（0-1）</param>
        public void SetProgress(float value)
        {
            progress = Mathf.Clamp01(value);
            
            if (progressSlider != null)
                progressSlider.value = progress;
        }
        
        /// <summary>
        /// 更新视觉状态
        /// </summary>
        private void UpdateVisualState()
        {
            Color targetColor = normalColor;
            
            if (!IsActive)
            {
                targetColor = disabledColor;
            }
            else if (isWorking)
            {
                targetColor = activeColor;
            }
            
            // 更新图标颜色
            if (iconImage != null)
                iconImage.color = targetColor;
            
            // 更新按钮文本
            if (actionButton != null)
            {
                Text buttonText = actionButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = isWorking ? "停止" : "开始";
                }
            }
        }
        
        /// <summary>
        /// 设置图标精灵
        /// </summary>
        /// <param name="sprite">精灵</param>
        public void SetIcon(Sprite sprite)
        {
            if (iconImage != null)
                iconImage.sprite = sprite;
        }
        
        /// <summary>
        /// 设置按钮可交互性
        /// </summary>
        /// <param name="interactable">是否可交互</param>
        public void SetButtonInteractable(bool interactable)
        {
            if (actionButton != null)
                actionButton.interactable = interactable;
        }
        
        /// <summary>
        /// 获取组件信息
        /// </summary>
        /// <returns>组件信息字符串</returns>
        public string GetComponentInfo()
        {
            return $"组件: {name}, 状态: {(isWorking ? "工作中" : "空闲")}, 进度: {progress:P0}";
        }
    }
}