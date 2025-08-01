using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Framework.UI;

namespace Framework.UI.Examples
{
    /// <summary>
    /// 非线性导航示例
    /// 演示复杂的UI跳转场景，如A-B-C-D界面中从D直接跳转到B
    /// </summary>
    public class NonLinearNavigationExample : MonoBehaviour
    {
        [Header("导航系统")]
        public AdvancedUINavigationSystem navigationSystem;
        
        [Header("测试按钮")]
        public Button openAButton;
        public Button openBButton;
        public Button openCButton;
        public Button openDButton;
        public Button jumpToBFromDButton;
        public Button backButton;
        public Button clearStackButton;
        
        [Header("信息显示")]
        public Text stackInfoText;
        public Text currentPageText;
        public Text navigationHistoryText;
        
        [Header("测试场景")]
        public bool enableAutoTest = false;
        public float autoTestInterval = 2f;
        
        void Start()
        {
            SetupExample();
            SetupButtonEvents();
            
            if (enableAutoTest)
            {
                InvokeRepeating(nameof(RunAutoTest), 1f, autoTestInterval);
            }
        }
        
        void Update()
        {
            UpdateUI();
        }
        
        /// <summary>
        /// 设置示例
        /// </summary>
        private void SetupExample()
        {
            if (navigationSystem == null)
            {
                navigationSystem = FindObjectOfType<AdvancedUINavigationSystem>();
                if (navigationSystem == null)
                {
                    Debug.LogError("[非线性导航示例] 未找到高级UI导航系统");
                    return;
                }
            }
            
            // 监听导航事件
            navigationSystem.OnPageTransition += OnPageTransition;
            navigationSystem.OnStackChanged += OnStackChanged;
            
            Debug.Log("[非线性导航示例] 示例设置完成");
        }
        
        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtonEvents()
        {
            if (openAButton != null)
                openAButton.onClick.AddListener(() => OpenPageA());
                
            if (openBButton != null)
                openBButton.onClick.AddListener(() => OpenPageB());
                
            if (openCButton != null)
                openCButton.onClick.AddListener(() => OpenPageC());
                
            if (openDButton != null)
                openDButton.onClick.AddListener(() => OpenPageD());
                
            if (jumpToBFromDButton != null)
                jumpToBFromDButton.onClick.AddListener(() => JumpToBFromD());
                
            if (backButton != null)
                backButton.onClick.AddListener(() => navigationSystem.NavigateBack());
                
            if (clearStackButton != null)
                clearStackButton.onClick.AddListener(() => navigationSystem.ClearNavigationStack());
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
                string currentPage = navigationSystem.GetCurrentPageId();
                currentPageText.text = $"当前页面: {currentPage ?? "无"}";
            }
            
            // 更新栈信息显示
            if (stackInfoText != null)
            {
                stackInfoText.text = navigationSystem.GetNavigationStackInfo();
            }
            
            // 更新导航历史
            if (navigationHistoryText != null)
            {
                var history = navigationSystem.GetNavigationHistory();
                string historyText = "导航历史:\n";
                for (int i = history.Count - 1; i >= 0 && i >= history.Count - 5; i--)
                {
                    historyText += $"• {history[i]}\n";
                }
                navigationHistoryText.text = historyText;
            }
        }
        
        #region 页面导航方法
        
        /// <summary>
        /// 打开页面A
        /// </summary>
        public void OpenPageA()
        {
            navigationSystem.NavigateToPage("PageA", AdvancedUINavigationSystem.JumpOperation.BringToTop);
            Debug.Log("[示例] 打开页面A");
        }
        
        /// <summary>
        /// 打开页面B
        /// </summary>
        public void OpenPageB()
        {
            navigationSystem.NavigateToPage("PageB", AdvancedUINavigationSystem.JumpOperation.BringToTop);
            Debug.Log("[示例] 打开页面B");
        }
        
        /// <summary>
        /// 打开页面C
        /// </summary>
        public void OpenPageC()
        {
            navigationSystem.NavigateToPage("PageC", AdvancedUINavigationSystem.JumpOperation.BringToTop);
            Debug.Log("[示例] 打开页面C");
        }
        
        /// <summary>
        /// 打开页面D
        /// </summary>
        public void OpenPageD()
        {
            navigationSystem.NavigateToPage("PageD", AdvancedUINavigationSystem.JumpOperation.BringToTop);
            Debug.Log("[示例] 打开页面D");
        }
        
        /// <summary>
        /// 从D页面跳转到B页面（核心场景演示）
        /// 这将把导航栈从 A-B-C-D 变成 A-C-D-B
        /// </summary>
        public void JumpToBFromD()
        {
            string currentPage = navigationSystem.GetCurrentPageId();
            if (currentPage != "PageD")
            {
                Debug.LogWarning("[示例] 当前不在页面D，无法演示跳转场景");
                return;
            }
            
            Debug.Log("[示例] === 开始演示非线性跳转 ===");
            Debug.Log("[示例] 当前栈状态: " + GetSimpleStackInfo());
            
            // 执行非线性跳转：将B页面移到栈顶
            navigationSystem.NavigateToPage("PageB", AdvancedUINavigationSystem.JumpOperation.BringToTop);
            
            Debug.Log("[示例] 跳转后栈状态: " + GetSimpleStackInfo());
            Debug.Log("[示例] === 非线性跳转完成 ===");
        }
        
        #endregion
        
        #region 测试场景
        
        /// <summary>
        /// 建立测试场景：A-B-C-D
        /// </summary>
        [ContextMenu("建立测试场景")]
        public void SetupTestScenario()
        {
            Debug.Log("[示例] === 开始建立测试场景 ===");
            
            // 清空当前栈
            navigationSystem.ClearNavigationStack();
            
            // 依次打开A-B-C-D
            OpenPageA();
            Debug.Log("[示例] 步骤1: " + GetSimpleStackInfo());
            
            OpenPageB();
            Debug.Log("[示例] 步骤2: " + GetSimpleStackInfo());
            
            OpenPageC();
            Debug.Log("[示例] 步骤3: " + GetSimpleStackInfo());
            
            OpenPageD();
            Debug.Log("[示例] 步骤4: " + GetSimpleStackInfo());
            
            Debug.Log("[示例] === 测试场景建立完成 ===");
            Debug.Log("[示例] 现在可以点击'从D跳转到B'按钮来演示非线性跳转");
        }
        
        /// <summary>
        /// 自动测试
        /// </summary>
        private void RunAutoTest()
        {
            if (!enableAutoTest) return;
            
            // 随机执行一些操作
            int action = Random.Range(0, 6);
            switch (action)
            {
                case 0: OpenPageA(); break;
                case 1: OpenPageB(); break;
                case 2: OpenPageC(); break;
                case 3: OpenPageD(); break;
                case 4: 
                    if (navigationSystem.GetCurrentPageId() == "PageD")
                        JumpToBFromD();
                    break;
                case 5: navigationSystem.NavigateBack(); break;
            }
        }
        
        /// <summary>
        /// 演示所有跳转操作类型
        /// </summary>
        [ContextMenu("演示所有跳转类型")]
        public void DemonstrateAllJumpOperations()
        {
            StartCoroutine(DemonstrateJumpOperationsCoroutine());
        }
        
        /// <summary>
        /// 演示跳转操作的协程
        /// </summary>
        private System.Collections.IEnumerator DemonstrateJumpOperationsCoroutine()
        {
            Debug.Log("[示例] === 开始演示所有跳转操作类型 ===");
            
            // 建立初始场景
            SetupTestScenario();
            yield return new WaitForSeconds(1f);
            
            // 1. BringToTop - 将B移到栈顶
            Debug.Log("[示例] 1. 演示 BringToTop 操作");
            navigationSystem.NavigateToPage("PageB", AdvancedUINavigationSystem.JumpOperation.BringToTop);
            Debug.Log("[示例] 结果: " + GetSimpleStackInfo());
            yield return new WaitForSeconds(2f);
            
            // 重建场景
            SetupTestScenario();
            yield return new WaitForSeconds(1f);
            
            // 2. ReplaceTop - 用B替换栈顶
            Debug.Log("[示例] 2. 演示 ReplaceTop 操作");
            navigationSystem.NavigateToPage("PageB", AdvancedUINavigationSystem.JumpOperation.ReplaceTop);
            Debug.Log("[示例] 结果: " + GetSimpleStackInfo());
            yield return new WaitForSeconds(2f);
            
            // 重建场景
            SetupTestScenario();
            yield return new WaitForSeconds(1f);
            
            // 3. RemoveAndJump - 移除中间页面并跳转
            Debug.Log("[示例] 3. 演示 RemoveAndJump 操作");
            navigationSystem.NavigateToPage("PageB", AdvancedUINavigationSystem.JumpOperation.RemoveAndJump);
            Debug.Log("[示例] 结果: " + GetSimpleStackInfo());
            yield return new WaitForSeconds(2f);
            
            Debug.Log("[示例] === 所有跳转操作演示完成 ===");
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 获取简化的栈信息
        /// </summary>
        private string GetSimpleStackInfo()
        {
            var stackInfo = navigationSystem.GetNavigationStackInfo();
            if (stackInfo.Contains("导航栈为空"))
                return "空栈";
            
            // 提取页面名称
            var lines = stackInfo.Split('\n');
            var pages = new List<string>();
            
            foreach (var line in lines)
            {
                if (line.Contains("Page"))
                {
                    var parts = line.Split('.');
                    if (parts.Length > 1)
                    {
                        var pageName = parts[1].Split(' ')[1];
                        pages.Add(pageName);
                    }
                }
            }
            
            return string.Join("-", pages);
        }
        
        /// <summary>
        /// 页面跳转事件回调
        /// </summary>
        private void OnPageTransition(string fromPageId, string toPageId)
        {
            Debug.Log($"[示例] 页面跳转: {fromPageId} → {toPageId}");
        }
        
        /// <summary>
        /// 栈变化事件回调
        /// </summary>
        private void OnStackChanged(List<AdvancedUINavigationSystem.NavigationStackItem> stack)
        {
            Debug.Log($"[示例] 导航栈已更新，当前大小: {stack.Count}");
        }
        
        #endregion
        
        #region 实际应用场景示例
        
        /// <summary>
        /// 游戏场景：从游戏界面快速跳转到背包
        /// </summary>
        [ContextMenu("游戏场景示例")]
        public void GameScenarioExample()
        {
            Debug.Log("[游戏场景] === 模拟游戏中的快速跳转 ===");
            
            // 模拟游戏流程：主菜单 → 游戏 → 暂停 → 设置 → 需要快速打开背包
            navigationSystem.ClearNavigationStack();
            
            navigationSystem.NavigateToPage("MainMenu");
            Debug.Log("[游戏场景] 1. 主菜单");
            
            navigationSystem.NavigateToPage("GamePlay");
            Debug.Log("[游戏场景] 2. 进入游戏: " + GetSimpleStackInfo());
            
            navigationSystem.NavigateToPage("PauseMenu");
            Debug.Log("[游戏场景] 3. 暂停游戏: " + GetSimpleStackInfo());
            
            navigationSystem.NavigateToPage("Settings");
            Debug.Log("[游戏场景] 4. 打开设置: " + GetSimpleStackInfo());
            
            // 现在玩家想要快速打开背包，但背包在游戏界面时就打开过
            // 使用非线性跳转直接跳到背包，保持其他界面在栈中
            navigationSystem.NavigateToPage("Inventory", AdvancedUINavigationSystem.JumpOperation.BringToTop);
            Debug.Log("[游戏场景] 5. 快速跳转到背包: " + GetSimpleStackInfo());
            
            Debug.Log("[游戏场景] === 场景演示完成 ===");
        }
        
        /// <summary>
        /// 电商应用场景：商品详情页快速跳转
        /// </summary>
        [ContextMenu("电商场景示例")]
        public void ECommerceScenarioExample()
        {
            Debug.Log("[电商场景] === 模拟电商应用的快速跳转 ===");
            
            // 模拟电商流程：首页 → 分类 → 商品列表 → 商品详情 → 需要快速跳转到购物车
            navigationSystem.ClearNavigationStack();
            
            navigationSystem.NavigateToPage("HomePage");
            navigationSystem.NavigateToPage("CategoryPage");
            navigationSystem.NavigateToPage("ProductList");
            navigationSystem.NavigateToPage("ProductDetail");
            Debug.Log("[电商场景] 浏览流程: " + GetSimpleStackInfo());
            
            // 用户想要查看购物车（之前在分类页面时添加过商品）
            navigationSystem.NavigateToPage("ShoppingCart", AdvancedUINavigationSystem.JumpOperation.BringToTop);
            Debug.Log("[电商场景] 快速查看购物车: " + GetSimpleStackInfo());
            
            Debug.Log("[电商场景] === 场景演示完成 ===");
        }
        
        #endregion
        
        #region Unity生命周期
        
        void OnDestroy()
        {
            // 清理事件监听
            if (navigationSystem != null)
            {
                navigationSystem.OnPageTransition -= OnPageTransition;
                navigationSystem.OnStackChanged -= OnStackChanged;
            }
            
            // 清理按钮事件
            if (openAButton != null) openAButton.onClick.RemoveAllListeners();
            if (openBButton != null) openBButton.onClick.RemoveAllListeners();
            if (openCButton != null) openCButton.onClick.RemoveAllListeners();
            if (openDButton != null) openDButton.onClick.RemoveAllListeners();
            if (jumpToBFromDButton != null) jumpToBFromDButton.onClick.RemoveAllListeners();
            if (backButton != null) backButton.onClick.RemoveAllListeners();
            if (clearStackButton != null) clearStackButton.onClick.RemoveAllListeners();
        }
        
        #endregion
    }
    
    /// <summary>
    /// 非线性导航的最佳实践指南
    /// </summary>
    public static class NonLinearNavigationBestPractices
    {
        /// <summary>
        /// 使用场景建议
        /// </summary>
        public static class UsageScenarios
        {
            // 1. 游戏中的快速界面切换
            // 例如：游戏中快速打开背包、技能树、地图等
            
            // 2. 电商应用的购物流程
            // 例如：商品浏览过程中快速查看购物车、收藏夹
            
            // 3. 社交应用的消息处理
            // 例如：聊天过程中快速切换到联系人列表、设置
            
            // 4. 办公应用的文档编辑
            // 例如：编辑文档时快速切换到文件管理、最近文档
        }
        
        /// <summary>
        /// 跳转操作选择指南
        /// </summary>
        public static class OperationSelection
        {
            // BringToTop: 保留所有页面，只改变顺序（推荐用于大多数场景）
            // ReplaceTop: 替换当前页面（用于相似功能的页面切换）
            // RemoveAndJump: 清理中间页面（用于简化导航栈）
            // SwapPosition: 交换位置（用于特殊的UI布局需求）
        }
        
        /// <summary>
        /// 性能优化建议
        /// </summary>
        public static class PerformanceOptimization
        {
            // 1. 启用页面缓存，避免频繁创建销毁
            // 2. 限制导航栈大小，防止内存泄漏
            // 3. 使用重复页面检测，避免栈中出现多个相同页面
            // 4. 合理设置页面预加载策略
        }
    }
}