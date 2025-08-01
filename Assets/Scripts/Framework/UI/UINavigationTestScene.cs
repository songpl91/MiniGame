using UnityEngine;
using UnityEngine.UI;
using Framework.UI;

namespace Framework.UI.Examples
{
    /// <summary>
    /// UI导航系统测试场景
    /// 用于在Unity中快速测试和演示UI导航功能
    /// </summary>
    public class UINavigationTestScene : MonoBehaviour
    {
        [Header("UI导航系统")]
        public UINavigationSystem navigationSystem;
        public UINavigationExample navigationExample;
        
        [Header("测试UI面板")]
        public GameObject mainMenuPanel;
        public GameObject settingsPanel;
        public GameObject inventoryPanel;
        public GameObject shopPanel;
        public GameObject gamePlayPanel;
        public GameObject pausePanel;
        
        [Header("测试按钮")]
        public Button mainMenuButton;
        public Button settingsButton;
        public Button inventoryButton;
        public Button shopButton;
        public Button gamePlayButton;
        public Button pauseButton;
        public Button backButton;
        
        [Header("信息显示")]
        public Text currentPageText;
        public Text navigationStackText;
        public Text debugInfoText;
        
        void Start()
        {
            SetupTestScene();
            SetupButtonEvents();
            UpdateUI();
        }
        
        void Update()
        {
            UpdateUI();
        }
        
        /// <summary>
        /// 设置测试场景
        /// </summary>
        private void SetupTestScene()
        {
            Debug.Log("[测试场景] 开始设置UI导航测试场景");
            
            // 确保导航系统存在
            if (navigationSystem == null)
            {
                navigationSystem = FindObjectOfType<UINavigationSystem>();
                if (navigationSystem == null)
                {
                    Debug.LogError("[测试场景] 未找到UI导航系统");
                    return;
                }
            }
            
            // 确保示例脚本存在
            if (navigationExample == null)
            {
                navigationExample = FindObjectOfType<UINavigationExample>();
                if (navigationExample == null)
                {
                    Debug.LogError("[测试场景] 未找到UI导航示例脚本");
                    return;
                }
            }
            
            // 初始化所有面板为隐藏状态
            HideAllPanels();
            
            Debug.Log("[测试场景] UI导航测试场景设置完成");
        }
        
        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtonEvents()
        {
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(() => navigationExample.OpenMainMenu());
                
            if (settingsButton != null)
                settingsButton.onClick.AddListener(() => navigationExample.OpenSettings());
                
            if (inventoryButton != null)
                inventoryButton.onClick.AddListener(() => navigationExample.OpenInventory());
                
            if (shopButton != null)
                shopButton.onClick.AddListener(() => navigationExample.OpenShop());
                
            if (gamePlayButton != null)
                gamePlayButton.onClick.AddListener(() => navigationExample.StartGame());
                
            if (pauseButton != null)
                pauseButton.onClick.AddListener(() => navigationExample.PauseGame());
                
            if (backButton != null)
                backButton.onClick.AddListener(() => navigationExample.GoBack());
                
            Debug.Log("[测试场景] 按钮事件设置完成");
        }
        
        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUI()
        {
            if (navigationSystem == null) return;
            
            // 更新当前页面显示
            if (currentPageText != null)
            {
                // string currentPage = navigationSystem.GetCurrentPageId();
                // currentPageText.text = $"当前页面: {currentPage}";
            }
            
            // 更新导航栈显示
            if (navigationStackText != null)
            {
                // string stackInfo = navigationSystem.GetNavigationStackInfo();
                // navigationStackText.text = $"导航栈: {stackInfo}";
            }
            
            // 更新调试信息
            if (debugInfoText != null)
            {
                string debugInfo = navigationSystem.GetNavigationInfo();
                debugInfoText.text = debugInfo;
            }
        }
        
        /// <summary>
        /// 隐藏所有面板
        /// </summary>
        private void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
            if (gamePlayPanel != null) gamePlayPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
        }
        
        #region 测试方法
        
        /// <summary>
        /// 自动测试导航流程
        /// </summary>
        [ContextMenu("自动测试导航流程")]
        public void AutoTestNavigationFlow()
        {
            StartCoroutine(AutoTestCoroutine());
        }
        
        /// <summary>
        /// 自动测试协程
        /// </summary>
        private System.Collections.IEnumerator AutoTestCoroutine()
        {
            Debug.Log("[自动测试] 开始自动测试导航流程");
            
            // 测试主菜单
            navigationExample.OpenMainMenu();
            yield return new WaitForSeconds(1f);
            
            // 测试设置页面
            navigationExample.OpenSettings();
            yield return new WaitForSeconds(1f);
            
            // 返回
            navigationExample.GoBack();
            yield return new WaitForSeconds(1f);
            
            // 测试背包页面
            navigationExample.OpenInventory();
            yield return new WaitForSeconds(1f);
            
            // 测试商店页面（叠加显示）
            navigationExample.OpenShop();
            yield return new WaitForSeconds(1f);
            
            // 返回到背包
            navigationExample.GoBack();
            yield return new WaitForSeconds(1f);
            
            // 返回到主菜单
            navigationExample.GoBack();
            yield return new WaitForSeconds(1f);
            
            // 开始游戏
            navigationExample.StartGame();
            yield return new WaitForSeconds(1f);
            
            // 暂停游戏
            navigationExample.PauseGame();
            yield return new WaitForSeconds(1f);
            
            // 恢复游戏
            navigationExample.GoBack();
            yield return new WaitForSeconds(1f);
            
            Debug.Log("[自动测试] 自动测试导航流程完成");
        }
        
        /// <summary>
        /// 测试事件触发导航
        /// </summary>
        [ContextMenu("测试事件导航")]
        public void TestEventNavigation()
        {
            Debug.Log("[事件测试] 开始测试事件触发导航");
            
            // 模拟登录成功
            navigationExample.OnPlayerLoginSuccess();
            
            // 等待一段时间后模拟游戏结束
            Invoke(nameof(SimulateGameOver), 3f);
        }
        
        /// <summary>
        /// 模拟游戏结束
        /// </summary>
        private void SimulateGameOver()
        {
            navigationExample.OnGameOver();
            Debug.Log("[事件测试] 模拟游戏结束事件");
        }
        
        /// <summary>
        /// 压力测试导航系统
        /// </summary>
        [ContextMenu("压力测试")]
        public void StressTestNavigation()
        {
            StartCoroutine(StressTestCoroutine());
        }
        
        /// <summary>
        /// 压力测试协程
        /// </summary>
        private System.Collections.IEnumerator StressTestCoroutine()
        {
            Debug.Log("[压力测试] 开始压力测试导航系统");
            
            string[] pages = { "MainMenu", "Settings", "Inventory", "Shop", "GamePlay" };
            
            for (int i = 0; i < 100; i++)
            {
                // 随机选择页面
                string randomPage = pages[Random.Range(0, pages.Length)];
                
                // 随机选择跳转类型
                UINavigationSystem.UITransitionType transitionType = 
                    (UINavigationSystem.UITransitionType)Random.Range(0, 4);
                
                navigationSystem.NavigateTo(randomPage, transitionType);
                
                // 随机决定是否返回
                if (Random.Range(0f, 1f) > 0.7f)
                {
                    navigationExample.GoBack();
                }
                
                yield return new WaitForSeconds(0.1f);
            }
            
            Debug.Log("[压力测试] 压力测试完成");
        }
        
        #endregion
        
        #region UI事件处理
        
        /// <summary>
        /// 重置导航系统
        /// </summary>
        public void ResetNavigation()
        {
            // if (navigationSystem != null)
            // {
            //     navigationSystem.ClearNavigationStack();
            //     HideAllPanels();
            //     Debug.Log("[测试场景] 导航系统已重置");
            // }
        }
        
        /// <summary>
        /// 显示导航信息
        /// </summary>
        public void ShowNavigationInfo()
        {
            if (navigationSystem != null)
            {
                string info = navigationSystem.GetNavigationInfo();
                Debug.Log($"[导航信息]\n{info}");
            }
        }
        
        /// <summary>
        /// 切换调试模式
        /// </summary>
        public void ToggleDebugMode()
        {
            if (navigationSystem != null)
            {
                // bool currentDebugMode = navigationSystem.IsDebugMode();
                // navigationSystem.SetDebugMode(!currentDebugMode);
                // Debug.Log($"[测试场景] 调试模式: {!currentDebugMode}");
            }
        }
        
        #endregion
        
        #region Unity生命周期
        
        void OnGUI()
        {
            // 在屏幕上显示快捷键提示
            GUI.Label(new Rect(10, 10, 300, 200), GetShortcutInfo());
        }
        
        /// <summary>
        /// 获取快捷键信息
        /// </summary>
        private string GetShortcutInfo()
        {
            return "快捷键:\n" +
                   "M - 主菜单\n" +
                   "S - 设置\n" +
                   "I - 背包\n" +
                   "ESC - 返回\n" +
                   "F1 - 自动测试\n" +
                   "F2 - 事件测试\n" +
                   "F3 - 压力测试\n" +
                   "F4 - 重置导航\n" +
                   "F5 - 显示信息";
        }
        
        void OnDestroy()
        {
            // 清理资源
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveAllListeners();
            if (settingsButton != null) settingsButton.onClick.RemoveAllListeners();
            if (inventoryButton != null) inventoryButton.onClick.RemoveAllListeners();
            if (shopButton != null) shopButton.onClick.RemoveAllListeners();
            if (gamePlayButton != null) gamePlayButton.onClick.RemoveAllListeners();
            if (pauseButton != null) pauseButton.onClick.RemoveAllListeners();
            if (backButton != null) backButton.onClick.RemoveAllListeners();
        }
        
        #endregion
    }
    
    /// <summary>
    /// UI导航测试数据
    /// </summary>
    [System.Serializable]
    public class UINavigationTestData
    {
        [Header("测试配置")]
        public bool enableAutoTest = false;
        public float autoTestInterval = 2f;
        public bool enableStressTest = false;
        public int stressTestIterations = 100;
        
        [Header("性能监控")]
        public bool enablePerformanceMonitoring = true;
        public bool logNavigationEvents = true;
        public bool showDebugInfo = true;
        
        [Header("错误处理")]
        public bool enableErrorRecovery = true;
        public string fallbackPageId = "MainMenu";
        public int maxNavigationStackSize = 10;
    }
}