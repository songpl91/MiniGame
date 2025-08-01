using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

namespace Framework.UI
{
    /// <summary>
    /// 非线性导航测试管理器
    /// 提供完整的测试场景和演示功能
    /// </summary>
    public class NonLinearNavigationTestManager : MonoBehaviour
    {
        #region 字段和属性
        
        [Header("导航系统")]
        public AdvancedUINavigationSystem navigationSystem;
        
        [Header("UI引用")]
        public Transform uiParent;
        public Button[] testButtons;
        public TextMeshProUGUI stackInfoText;
        public TextMeshProUGUI currentPageText;
        public TextMeshProUGUI operationLogText;
        public ScrollRect logScrollRect;
        
        [Header("测试页面预制体")]
        public GameObject pageAPrefab;
        public GameObject pageBPrefab;
        public GameObject pageCPrefab;
        public GameObject pageDPrefab;
        public GameObject pageEPrefab;
        
        [Header("测试设置")]
        public bool autoTest = false;
        public float autoTestInterval = 2f;
        public bool enableLogging = true;
        public int maxLogEntries = 50;
        
        // 私有字段
        private Dictionary<string, GameObject> pageInstances = new Dictionary<string, GameObject>();
        private List<string> operationLog = new List<string>();
        private Coroutine autoTestCoroutine;
        private int testScenarioIndex = 0;
        
        #endregion
        
        #region Unity生命周期
        
        void Start()
        {
            InitializeTestManager();
            SetupTestButtons();
            CreateTestPages();
            
            if (autoTest)
            {
                StartAutoTest();
            }
        }
        
        void Update()
        {
            UpdateUI();
            HandleKeyboardInput();
        }
        
        void OnDestroy()
        {
            if (autoTestCoroutine != null)
            {
                StopCoroutine(autoTestCoroutine);
            }
        }
        
        #endregion
        
        #region 初始化方法
        
        /// <summary>
        /// 初始化测试管理器
        /// </summary>
        private void InitializeTestManager()
        {
            if (navigationSystem == null)
            {
                navigationSystem = FindObjectOfType<AdvancedUINavigationSystem>();
                if (navigationSystem == null)
                {
                    var go = new GameObject("AdvancedUINavigationSystem");
                    navigationSystem = go.AddComponent<AdvancedUINavigationSystem>();
                }
            }
            
            // 订阅导航事件
            if (navigationSystem != null)
            {
                // 这里可以添加事件订阅，如果导航系统支持的话
                LogOperation("测试管理器初始化完成");
            }
        }
        
        /// <summary>
        /// 设置测试按钮
        /// </summary>
        private void SetupTestButtons()
        {
            if (testButtons == null || testButtons.Length == 0)
                return;
            
            // 为每个按钮设置点击事件
            for (int i = 0; i < testButtons.Length && i < 10; i++)
            {
                int buttonIndex = i;
                testButtons[i].onClick.AddListener(() => OnTestButtonClick(buttonIndex));
            }
        }
        
        /// <summary>
        /// 创建测试页面
        /// </summary>
        private void CreateTestPages()
        {
            CreateTestPage("PageA", pageAPrefab, "页面A - 主页");
            CreateTestPage("PageB", pageBPrefab, "页面B - 商店");
            CreateTestPage("PageC", pageCPrefab, "页面C - 背包");
            CreateTestPage("PageD", pageDPrefab, "页面D - 设置");
            CreateTestPage("PageE", pageEPrefab, "页面E - 帮助");
        }
        
        /// <summary>
        /// 创建单个测试页面
        /// </summary>
        private void CreateTestPage(string pageId, GameObject prefab, string displayName)
        {
            GameObject pageInstance;
            
            if (prefab != null)
            {
                pageInstance = Instantiate(prefab, uiParent);
            }
            else
            {
                // 创建默认页面
                pageInstance = CreateDefaultPage(pageId, displayName);
            }
            
            pageInstance.name = pageId;
            pageInstance.SetActive(false);
            pageInstances[pageId] = pageInstance;
            
            // 注册到导航系统
            if (navigationSystem != null)
            {
                // 这里需要根据实际的导航系统API来注册页面
                LogOperation($"创建页面: {pageId} ({displayName})");
            }
        }
        
        /// <summary>
        /// 创建默认页面
        /// </summary>
        private GameObject CreateDefaultPage(string pageId, string displayName)
        {
            var pageGO = new GameObject(pageId);
            pageGO.transform.SetParent(uiParent);
            
            // 添加Canvas组件
            var canvas = pageGO.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            
            // 添加背景
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(pageGO.transform);
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = GetPageColor(pageId);
            
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            // 添加标题文本
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(pageGO.transform);
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = displayName;
            titleText.fontSize = 24;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.8f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // 添加导航按钮
            CreateNavigationButtons(pageGO, pageId);
            
            return pageGO;
        }
        
        /// <summary>
        /// 创建导航按钮
        /// </summary>
        private void CreateNavigationButtons(GameObject pageGO, string pageId)
        {
            var buttonContainer = new GameObject("NavigationButtons");
            buttonContainer.transform.SetParent(pageGO.transform);
            
            var containerRect = buttonContainer.GetComponent<RectTransform>();
            if (containerRect == null)
                containerRect = buttonContainer.AddComponent<RectTransform>();
            
            containerRect.anchorMin = new Vector2(0.1f, 0.1f);
            containerRect.anchorMax = new Vector2(0.9f, 0.7f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            
            // 添加垂直布局组
            var layoutGroup = buttonContainer.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 10;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            
            // 创建具体的导航按钮
            CreateNavigationButton(buttonContainer, "返回上一页", () => NavigateBack());
            CreateNavigationButton(buttonContainer, "跳转到页面A", () => NavigateToPage("PageA"));
            CreateNavigationButton(buttonContainer, "跳转到页面B", () => NavigateToPage("PageB"));
            CreateNavigationButton(buttonContainer, "跳转到页面C", () => NavigateToPage("PageC"));
            CreateNavigationButton(buttonContainer, "跳转到页面D", () => NavigateToPage("PageD"));
            
            if (pageId == "PageD")
            {
                CreateNavigationButton(buttonContainer, "非线性跳转到B", () => JumpToBFromD());
            }
        }
        
        /// <summary>
        /// 创建单个导航按钮
        /// </summary>
        private void CreateNavigationButton(GameObject parent, string text, System.Action onClick)
        {
            var buttonGO = new GameObject("Button_" + text);
            buttonGO.transform.SetParent(parent.transform);
            
            var button = buttonGO.AddComponent<Button>();
            var buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.3f, 0.8f, 0.8f);
            
            var buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(200, 40);
            
            // 添加按钮文本
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform);
            var buttonText = textGO.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 16;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            button.onClick.AddListener(() => onClick?.Invoke());
        }
        
        /// <summary>
        /// 获取页面颜色
        /// </summary>
        private Color GetPageColor(string pageId)
        {
            switch (pageId)
            {
                case "PageA": return new Color(0.8f, 0.2f, 0.2f, 0.8f); // 红色
                case "PageB": return new Color(0.2f, 0.8f, 0.2f, 0.8f); // 绿色
                case "PageC": return new Color(0.2f, 0.2f, 0.8f, 0.8f); // 蓝色
                case "PageD": return new Color(0.8f, 0.8f, 0.2f, 0.8f); // 黄色
                case "PageE": return new Color(0.8f, 0.2f, 0.8f, 0.8f); // 紫色
                default: return Color.gray;
            }
        }
        
        #endregion
        
        #region 导航方法
        
        /// <summary>
        /// 导航到指定页面
        /// </summary>
        public void NavigateToPage(string pageId)
        {
            if (navigationSystem != null)
            {
                navigationSystem.NavigateToPage(pageId);
                UpdatePageVisibility();
                LogOperation($"导航到页面: {pageId}");
            }
        }
        
        /// <summary>
        /// 返回上一页
        /// </summary>
        public void NavigateBack()
        {
            if (navigationSystem != null)
            {
                navigationSystem.NavigateBack();
                UpdatePageVisibility();
                LogOperation("返回上一页");
            }
        }
        
        /// <summary>
        /// 从D页面非线性跳转到B页面
        /// </summary>
        public void JumpToBFromD()
        {
            if (navigationSystem != null)
            {
                navigationSystem.NavigateToPage("PageB", AdvancedUINavigationSystem.JumpOperation.BringToTop);
                UpdatePageVisibility();
                LogOperation("非线性跳转: D → B (BringToTop)");
            }
        }
        
        /// <summary>
        /// 更新页面可见性
        /// </summary>
        private void UpdatePageVisibility()
        {
            // 隐藏所有页面
            foreach (var kvp in pageInstances)
            {
                kvp.Value.SetActive(false);
            }
            
            // 显示当前页面
            string currentPageId = navigationSystem?.GetCurrentPageId();
            if (!string.IsNullOrEmpty(currentPageId) && pageInstances.ContainsKey(currentPageId))
            {
                pageInstances[currentPageId].SetActive(true);
            }
        }
        
        #endregion
        
        #region 测试方法
        
        /// <summary>
        /// 测试按钮点击处理
        /// </summary>
        private void OnTestButtonClick(int buttonIndex)
        {
            switch (buttonIndex)
            {
                case 0: // 建立A-B-C-D场景
                    BuildABCDScenario();
                    break;
                case 1: // 非线性跳转演示
                    DemonstrateNonLinearJump();
                    break;
                case 2: // 清空导航栈
                    ClearNavigationStack();
                    break;
                case 3: // 自动测试
                    ToggleAutoTest();
                    break;
                case 4: // 压力测试
                    StartStressTest();
                    break;
                case 5: // 导出日志
                    ExportLog();
                    break;
                default:
                    LogOperation($"测试按钮 {buttonIndex} 被点击");
                    break;
            }
        }
        
        /// <summary>
        /// 建立A-B-C-D场景
        /// </summary>
        public void BuildABCDScenario()
        {
            StartCoroutine(BuildABCDScenarioCoroutine());
        }
        
        private IEnumerator BuildABCDScenarioCoroutine()
        {
            LogOperation("开始建立A-B-C-D场景");
            
            ClearNavigationStack();
            yield return new WaitForSeconds(0.5f);
            
            NavigateToPage("PageA");
            yield return new WaitForSeconds(0.5f);
            
            NavigateToPage("PageB");
            yield return new WaitForSeconds(0.5f);
            
            NavigateToPage("PageC");
            yield return new WaitForSeconds(0.5f);
            
            NavigateToPage("PageD");
            yield return new WaitForSeconds(0.5f);
            
            LogOperation("A-B-C-D场景建立完成");
        }
        
        /// <summary>
        /// 演示非线性跳转
        /// </summary>
        public void DemonstrateNonLinearJump()
        {
            StartCoroutine(DemonstrateNonLinearJumpCoroutine());
        }
        
        private IEnumerator DemonstrateNonLinearJumpCoroutine()
        {
            LogOperation("开始演示非线性跳转");
            
            // 确保有A-B-C-D场景
            if (navigationSystem.GetStackSize() < 4)
            {
                yield return StartCoroutine(BuildABCDScenarioCoroutine());
            }
            
            yield return new WaitForSeconds(1f);
            
            // 执行非线性跳转
            LogOperation("当前栈: A-B-C-D，即将跳转到B");
            yield return new WaitForSeconds(1f);
            
            JumpToBFromD();
            yield return new WaitForSeconds(1f);
            
            LogOperation("跳转完成，当前栈: A-C-D-B");
            LogOperation("非线性跳转演示完成");
        }
        
        /// <summary>
        /// 清空导航栈
        /// </summary>
        public void ClearNavigationStack()
        {
            if (navigationSystem != null)
            {
                navigationSystem.ClearNavigationStack();
                UpdatePageVisibility();
                LogOperation("导航栈已清空");
            }
        }
        
        /// <summary>
        /// 切换自动测试
        /// </summary>
        public void ToggleAutoTest()
        {
            autoTest = !autoTest;
            
            if (autoTest)
            {
                StartAutoTest();
            }
            else
            {
                StopAutoTest();
            }
        }
        
        /// <summary>
        /// 开始自动测试
        /// </summary>
        private void StartAutoTest()
        {
            if (autoTestCoroutine != null)
            {
                StopCoroutine(autoTestCoroutine);
            }
            
            autoTestCoroutine = StartCoroutine(AutoTestCoroutine());
            LogOperation("自动测试已开始");
        }
        
        /// <summary>
        /// 停止自动测试
        /// </summary>
        private void StopAutoTest()
        {
            if (autoTestCoroutine != null)
            {
                StopCoroutine(autoTestCoroutine);
                autoTestCoroutine = null;
            }
            
            LogOperation("自动测试已停止");
        }
        
        /// <summary>
        /// 自动测试协程
        /// </summary>
        private IEnumerator AutoTestCoroutine()
        {
            var testScenarios = new System.Action[]
            {
                () => BuildABCDScenario(),
                () => DemonstrateNonLinearJump(),
                () => ClearNavigationStack(),
                () => NavigateToPage("PageA"),
                () => NavigateToPage("PageE"),
                () => NavigateBack(),
            };
            
            while (autoTest)
            {
                var scenario = testScenarios[testScenarioIndex % testScenarios.Length];
                scenario.Invoke();
                
                testScenarioIndex++;
                yield return new WaitForSeconds(autoTestInterval);
            }
        }
        
        /// <summary>
        /// 开始压力测试
        /// </summary>
        public void StartStressTest()
        {
            StartCoroutine(StressTestCoroutine());
        }
        
        /// <summary>
        /// 压力测试协程
        /// </summary>
        private IEnumerator StressTestCoroutine()
        {
            LogOperation("开始压力测试");
            
            var pages = new string[] { "PageA", "PageB", "PageC", "PageD", "PageE" };
            
            for (int i = 0; i < 100; i++)
            {
                var randomPage = pages[Random.Range(0, pages.Length)];
                NavigateToPage(randomPage);
                
                if (i % 10 == 0)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
            
            LogOperation("压力测试完成");
        }
        
        #endregion
        
        #region UI更新方法
        
        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUI()
        {
            if (navigationSystem == null) return;
            
            // 更新栈信息
            if (stackInfoText != null)
            {
                stackInfoText.text = navigationSystem.GetNavigationStackInfo();
            }
            
            // 更新当前页面
            if (currentPageText != null)
            {
                string currentPage = navigationSystem.GetCurrentPageId() ?? "无";
                currentPageText.text = $"当前页面: {currentPage}";
            }
            
            // 更新操作日志
            if (operationLogText != null)
            {
                operationLogText.text = string.Join("\n", operationLog);
                
                // 自动滚动到底部
                if (logScrollRect != null)
                {
                    Canvas.ForceUpdateCanvases();
                    logScrollRect.verticalNormalizedPosition = 0f;
                }
            }
        }
        
        /// <summary>
        /// 处理键盘输入
        /// </summary>
        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) NavigateToPage("PageA");
            if (Input.GetKeyDown(KeyCode.Alpha2)) NavigateToPage("PageB");
            if (Input.GetKeyDown(KeyCode.Alpha3)) NavigateToPage("PageC");
            if (Input.GetKeyDown(KeyCode.Alpha4)) NavigateToPage("PageD");
            if (Input.GetKeyDown(KeyCode.Alpha5)) NavigateToPage("PageE");
            if (Input.GetKeyDown(KeyCode.Escape)) NavigateBack();
            if (Input.GetKeyDown(KeyCode.Space)) BuildABCDScenario();
            if (Input.GetKeyDown(KeyCode.Return)) DemonstrateNonLinearJump();
        }
        
        #endregion
        
        #region 日志方法
        
        /// <summary>
        /// 记录操作日志
        /// </summary>
        private void LogOperation(string message)
        {
            if (!enableLogging) return;
            
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            string logEntry = $"[{timestamp}] {message}";
            
            operationLog.Add(logEntry);
            
            // 限制日志条目数量
            if (operationLog.Count > maxLogEntries)
            {
                operationLog.RemoveAt(0);
            }
            
            Debug.Log($"[导航测试] {logEntry}");
        }
        
        /// <summary>
        /// 导出日志
        /// </summary>
        public void ExportLog()
        {
            if (operationLog.Count == 0)
            {
                LogOperation("没有可导出的日志");
                return;
            }
            
            string logContent = "非线性UI导航测试日志\n";
            logContent += "========================\n\n";
            logContent += string.Join("\n", operationLog);
            
            string fileName = $"navigation_test_log_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
            
            try
            {
                System.IO.File.WriteAllText(filePath, logContent);
                LogOperation($"日志已导出到: {filePath}");
            }
            catch (System.Exception e)
            {
                LogOperation($"导出日志失败: {e.Message}");
            }
        }
        
        #endregion
    }
}