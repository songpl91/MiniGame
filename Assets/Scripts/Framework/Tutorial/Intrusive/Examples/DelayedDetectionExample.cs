using UnityEngine;
using UnityEngine.UI;
using Framework.Tutorial.Intrusive.Steps;
using System.Collections.Generic;

namespace Framework.Tutorial.Intrusive.Examples
{
    /// <summary>
    /// 延迟检测引导示例
    /// 展示如何使用简化的延迟检测方案替代复杂的跨界面管理
    /// </summary>
    public class DelayedDetectionExample : MonoBehaviour
    {
        #region UI组件
        
        [Header("UI控制")]
        [SerializeField] private Button startTutorialButton;
        [SerializeField] private Button switchPanelButton;
        [SerializeField] private Button createDynamicUIButton;
        [SerializeField] private Text statusText;
        
        [Header("面板管理")]
        [SerializeField] private GameObject panel1;
        [SerializeField] private GameObject panel2;
        [SerializeField] private Transform dynamicUIParent;
        
        #endregion
        
        #region 私有变量
        
        private IntrusiveTutorialManager tutorialManager;
        private string currentTutorialId = "delayed_detection_demo";
        private bool isPanel1Active = true;
        
        #endregion
        
        #region Unity生命周期
        
        void Start()
        {
            InitializeUI();
            InitializeTutorial();
        }
        
        void OnDestroy()
        {
            // 清理事件监听
            if (tutorialManager != null)
            {
                tutorialManager.OnTutorialStarted -= OnTutorialStarted;
                tutorialManager.OnTutorialCompleted -= OnTutorialCompleted;
                tutorialManager.OnTutorialStopped -= OnTutorialStopped;
            }
        }
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitializeUI()
        {
            // 绑定按钮事件
            if (startTutorialButton != null)
                startTutorialButton.onClick.AddListener(StartDelayedDetectionTutorial);
            
            if (switchPanelButton != null)
                switchPanelButton.onClick.AddListener(SwitchPanel);
            
            if (createDynamicUIButton != null)
                createDynamicUIButton.onClick.AddListener(CreateDynamicUI);
            
            // 初始状态
            UpdateStatusText("延迟检测引导示例已准备就绪");
            
            // 确保初始面板状态
            if (panel1 != null) panel1.SetActive(true);
            if (panel2 != null) panel2.SetActive(false);
        }
        
        /// <summary>
        /// 初始化引导系统
        /// </summary>
        private void InitializeTutorial()
        {
            tutorialManager = IntrusiveTutorialManager.Instance;
            if (tutorialManager == null)
            {
                Debug.LogError("[DelayedDetectionExample] 找不到 IntrusiveTutorialManager 实例");
                return;
            }
            
            // 注册事件监听
            tutorialManager.OnTutorialStarted += OnTutorialStarted;
            tutorialManager.OnTutorialCompleted += OnTutorialCompleted;
            tutorialManager.OnTutorialStopped += OnTutorialStopped;
            
            Debug.Log("[DelayedDetectionExample] 引导系统初始化完成");
        }
        
        #endregion
        
        #region 引导流程
        
        /// <summary>
        /// 开始延迟检测引导
        /// </summary>
        public void StartDelayedDetectionTutorial()
        {
            Debug.Log("[DelayedDetectionExample] 开始延迟检测引导演示");
            
            // 创建引导步骤序列
            var steps = CreateDelayedDetectionSequence();
            
            // 启动引导
            tutorialManager.StartTutorial(currentTutorialId, steps);
        }
        
        /// <summary>
        /// 创建延迟检测引导步骤序列
        /// 演示不同场景下的延迟检测应用
        /// </summary>
        private List<TutorialStepBase> CreateDelayedDetectionSequence()
        {
            var steps = new List<TutorialStepBase>();
            
            // 步骤1: 欢迎消息
            var welcomeStep = new ShowMessageStep(
                "welcome", 
                "欢迎", 
                "欢迎来到延迟检测引导演示！\n我们将展示如何用简单的延迟机制处理界面切换。", 
                false, 
                true
            );
            steps.Add(welcomeStep);
            
            // 步骤2: 点击当前面板的按钮（无延迟）
            var clickCurrentButton = new DelayedDetectionTutorialStep(
                "clickCurrent", 
                "点击当前按钮", 
                "SwitchPanelButton", 
                "请点击切换面板按钮，我们将切换到另一个面板",
                0.1f  // 很短的延迟，因为元素已存在
            );
            steps.Add(clickCurrentButton);
            
            // 步骤3: 等待面板切换后点击新按钮（中等延迟）
            var clickSwitchedButton = new DelayedDetectionTutorialStep(
                "clickSwitched", 
                "点击切换后的按钮", 
                "CreateDynamicUIButton", 
                "很好！现在请点击创建动态UI按钮",
                1.0f  // 给面板切换留出时间
            );
            clickSwitchedButton.SetDetectionParams(1.0f, 0.3f, 15); // 1秒初始延迟，0.3秒重试间隔，最多15次重试
            steps.Add(clickSwitchedButton);
            
            // 步骤4: 等待动态UI创建后点击（长延迟）
            var clickDynamicButton = new DelayedDetectionTutorialStep(
                "clickDynamic", 
                "点击动态按钮", 
                "DynamicTestButton", 
                "太棒了！现在请点击刚创建的动态按钮",
                2.0f  // 给动态UI创建留出更多时间
            );
            clickDynamicButton.SetDetectionParams(2.0f, 0.5f, 20); // 2秒初始延迟，0.5秒重试间隔，最多20次重试
            steps.Add(clickDynamicButton);
            
            // 步骤5: 完成提示
            var completeStep = new ShowMessageStep(
                "complete", 
                "完成", 
                "恭喜！您已经体验了延迟检测引导的强大功能。\n" +
                "这种方案比复杂的跨界面管理更简单、更可靠！", 
                false, 
                true
            );
            steps.Add(completeStep);
            
            return steps;
        }
        
        #endregion
        
        #region UI操作
        
        /// <summary>
        /// 切换面板
        /// 模拟界面切换场景
        /// </summary>
        private void SwitchPanel()
        {
            Debug.Log("[DelayedDetectionExample] 切换面板");
            
            isPanel1Active = !isPanel1Active;
            
            if (panel1 != null) panel1.SetActive(isPanel1Active);
            if (panel2 != null) panel2.SetActive(!isPanel1Active);
            
            UpdateStatusText($"已切换到面板{(isPanel1Active ? "1" : "2")}");
            
            // 模拟界面切换的延迟
            Invoke(nameof(OnPanelSwitchComplete), 0.5f);
        }
        
        /// <summary>
        /// 面板切换完成回调
        /// </summary>
        private void OnPanelSwitchComplete()
        {
            Debug.Log("[DelayedDetectionExample] 面板切换完成");
            UpdateStatusText($"面板{(isPanel1Active ? "1" : "2")}已准备就绪");
        }
        
        /// <summary>
        /// 创建动态UI
        /// 模拟动态UI创建场景
        /// </summary>
        private void CreateDynamicUI()
        {
            Debug.Log("[DelayedDetectionExample] 开始创建动态UI");
            UpdateStatusText("正在创建动态UI...");
            
            // 模拟异步创建过程
            Invoke(nameof(CreateDynamicUIDelayed), 1.5f);
        }
        
        /// <summary>
        /// 延迟创建动态UI
        /// </summary>
        private void CreateDynamicUIDelayed()
        {
            if (dynamicUIParent == null)
            {
                Debug.LogError("[DelayedDetectionExample] 动态UI父节点未设置");
                return;
            }
            
            // 创建动态按钮
            GameObject dynamicButton = new GameObject("DynamicTestButton");
            dynamicButton.transform.SetParent(dynamicUIParent, false);
            
            // 添加Button组件
            Button button = dynamicButton.AddComponent<Button>();
            
            // 添加Image组件（Button需要）
            Image image = dynamicButton.AddComponent<Image>();
            image.color = Color.green;
            
            // 设置RectTransform
            RectTransform rect = dynamicButton.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 50);
            
            // 添加文本
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(dynamicButton.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.text = "动态按钮";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // 添加点击事件
            button.onClick.AddListener(() => {
                Debug.Log("[DelayedDetectionExample] 动态按钮被点击");
                UpdateStatusText("动态按钮被点击！");
            });
            
            Debug.Log("[DelayedDetectionExample] 动态UI创建完成");
            UpdateStatusText("动态UI创建完成！");
        }
        
        #endregion
        
        #region 状态更新
        
        /// <summary>
        /// 更新状态文本
        /// </summary>
        /// <param name="message">状态消息</param>
        private void UpdateStatusText(string message)
        {
            if (statusText != null)
            {
                statusText.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
            }
            Debug.Log($"[DelayedDetectionExample] {message}");
        }
        
        #endregion
        
        #region 引导事件回调
        
        private void OnTutorialStarted(string tutorialId)
        {
            Debug.Log($"[DelayedDetectionExample] 引导开始: {tutorialId}");
            UpdateStatusText($"引导开始: {tutorialId}");
        }
        
        private void OnTutorialCompleted(string tutorialId)
        {
            Debug.Log($"[DelayedDetectionExample] 引导完成: {tutorialId}");
            UpdateStatusText("延迟检测引导演示完成！");
        }
        
        private void OnTutorialStopped(string tutorialId)
        {
            Debug.Log($"[DelayedDetectionExample] 引导停止: {tutorialId}");
            UpdateStatusText($"引导停止: {tutorialId}");
        }
        
        #endregion
        
        #region 调试功能
        
        /// <summary>
        /// 重置演示状态
        /// </summary>
        [ContextMenu("重置演示状态")]
        public void ResetDemoState()
        {
            // 重置面板状态
            isPanel1Active = true;
            if (panel1 != null) panel1.SetActive(true);
            if (panel2 != null) panel2.SetActive(false);
            
            // 清理动态UI
            if (dynamicUIParent != null)
            {
                for (int i = dynamicUIParent.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(dynamicUIParent.GetChild(i).gameObject);
                }
            }
            
            // 停止当前引导
            if (tutorialManager != null)
            {
                tutorialManager.StopCurrentTutorial();
            }
            
            UpdateStatusText("演示状态已重置");
        }
        
        /// <summary>
        /// 获取系统状态信息
        /// </summary>
        /// <returns>状态信息</returns>
        public string GetSystemStatus()
        {
            return $"延迟检测引导示例状态:\n" +
                   $"当前面板: {(isPanel1Active ? "面板1" : "面板2")}\n" +
                   $"动态UI数量: {(dynamicUIParent != null ? dynamicUIParent.childCount : 0)}\n" +
                   $"引导管理器: {(tutorialManager != null ? "已连接" : "未连接")}";
        }
        
        #endregion
    }
}