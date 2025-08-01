using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework.UI
{
    /// <summary>
    /// 高级UI导航系统
    /// 专门处理复杂的非线性跳转场景，如A-B-C-D界面中从D直接跳转到B，形成A-C-D-B的导航栈
    /// </summary>
    public class AdvancedUINavigationSystem : MonoBehaviour
    {
        #region 数据结构定义
        
        /// <summary>
        /// 导航栈项
        /// </summary>
        [System.Serializable]
        public class NavigationStackItem
        {
            public string pageId;           // 页面ID
            public DateTime openTime;       // 打开时间
            public object[] parameters;     // 页面参数
            public bool isActive;          // 是否激活状态
            public int stackIndex;         // 在栈中的索引
            
            public NavigationStackItem(string pageId, object[] parameters = null)
            {
                this.pageId = pageId;
                this.parameters = parameters ?? new object[0];
                this.openTime = DateTime.Now;
                this.isActive = true;
            }
        }
        
        /// <summary>
        /// 导航策略类型
        /// </summary>
        public enum NavigationStrategy
        {
            Linear,         // 线性导航（传统栈模式）
            NonLinear,      // 非线性导航（支持跨层级跳转）
            Hybrid,         // 混合模式（智能选择）
            Custom          // 自定义策略
        }
        
        /// <summary>
        /// 跳转操作类型
        /// </summary>
        public enum JumpOperation
        {
            BringToTop,     // 将目标页面移到栈顶
            ReplaceTop,     // 替换栈顶页面
            InsertAfter,    // 在指定页面后插入
            RemoveAndJump,  // 移除中间页面并跳转
            SwapPosition    // 交换页面位置
        }
        
        /// <summary>
        /// 导航配置
        /// </summary>
        [System.Serializable]
        public class AdvancedNavigationConfig
        {
            [Header("导航策略")]
            public NavigationStrategy strategy = NavigationStrategy.Hybrid;
            
            [Header("栈管理")]
            public int maxStackSize = 20;
            public bool enableStackOptimization = true;
            public bool enableDuplicateDetection = true;
            
            [Header("非线性跳转")]
            public bool enableNonLinearJump = true;
            public JumpOperation defaultJumpOperation = JumpOperation.BringToTop;
            public bool preserveIntermediatePages = true;
            
            [Header("性能优化")]
            public bool enableLazyLoading = true;
            public bool enablePagePreloading = false;
            public int maxCachedPages = 10;
            
            [Header("调试")]
            public bool enableDebugLog = true;
            public bool enableStackVisualization = false;
        }
        
        #endregion
        
        #region 字段和属性
        
        [Header("配置")]
        public AdvancedNavigationConfig config = new AdvancedNavigationConfig();
        
        [Header("UI引用")]
        public Transform uiRoot;
        public Canvas mainCanvas;
        
        // 导航栈（使用List而不是Stack，支持随机访问）
        private List<NavigationStackItem> navigationStack = new List<NavigationStackItem>();
        
        // 页面管理
        private Dictionary<string, UIBase> activePages = new Dictionary<string, UIBase>();
        private Dictionary<string, UIBase> cachedPages = new Dictionary<string, UIBase>();
        private Dictionary<string, GameObject> pagePrefabs = new Dictionary<string, GameObject>();
        
        // 导航历史
        private List<string> navigationHistory = new List<string>();
        
        // 事件
        public event Action<string, string> OnPageTransition;
        public event Action<string> OnPageOpened;
        public event Action<string> OnPageClosed;
        public event Action<List<NavigationStackItem>> OnStackChanged;
        
        #endregion
        
        #region Unity生命周期
        
        void Awake()
        {
            InitializeSystem();
        }
        
        void Start()
        {
            SetupDefaultPages();
        }
        
        #endregion
        
        #region 系统初始化
        
        /// <summary>
        /// 初始化系统
        /// </summary>
        private void InitializeSystem()
        {
            if (uiRoot == null)
                uiRoot = transform;
                
            if (mainCanvas == null)
                mainCanvas = GetComponentInParent<Canvas>();
                
            Debug.Log("[高级UI导航] 系统初始化完成");
        }
        
        /// <summary>
        /// 设置默认页面
        /// </summary>
        private void SetupDefaultPages()
        {
            // 这里可以预加载一些常用页面
            if (config.enablePagePreloading)
            {
                PreloadCommonPages();
            }
        }
        
        #endregion
        
        #region 核心导航方法
        
        /// <summary>
        /// 导航到指定页面（支持非线性跳转）
        /// </summary>
        /// <param name="targetPageId">目标页面ID</param>
        /// <param name="jumpOperation">跳转操作类型</param>
        /// <param name="parameters">页面参数</param>
        public void NavigateToPage(string targetPageId, JumpOperation jumpOperation = JumpOperation.BringToTop, params object[] parameters)
        {
            if (string.IsNullOrEmpty(targetPageId))
            {
                Debug.LogError("[高级UI导航] 目标页面ID不能为空");
                return;
            }
            
            string currentPageId = GetCurrentPageId();
            
            if (config.enableDebugLog)
                Debug.Log($"[高级UI导航] 导航请求: {currentPageId} → {targetPageId}, 操作: {jumpOperation}");
            
            // 检查目标页面是否已在栈中
            int existingIndex = FindPageInStack(targetPageId);
            
            if (existingIndex >= 0)
            {
                // 页面已存在，执行非线性跳转
                HandleNonLinearJump(targetPageId, existingIndex, jumpOperation, parameters);
            }
            else
            {
                // 页面不存在，执行常规导航
                HandleLinearNavigation(targetPageId, parameters);
            }
            
            // 记录导航历史
            navigationHistory.Add($"{currentPageId}→{targetPageId}");
            
            // 触发事件
            OnPageTransition?.Invoke(currentPageId, targetPageId);
            OnStackChanged?.Invoke(navigationStack);
        }
        
        /// <summary>
        /// 处理非线性跳转
        /// </summary>
        private void HandleNonLinearJump(string targetPageId, int existingIndex, JumpOperation jumpOperation, object[] parameters)
        {
            if (!config.enableNonLinearJump)
            {
                Debug.LogWarning("[高级UI导航] 非线性跳转已禁用，使用线性导航");
                HandleLinearNavigation(targetPageId, parameters);
                return;
            }
            
            var targetItem = navigationStack[existingIndex];
            
            switch (jumpOperation)
            {
                case JumpOperation.BringToTop:
                    BringPageToTop(targetPageId, existingIndex, parameters);
                    break;
                    
                case JumpOperation.ReplaceTop:
                    ReplaceTopPage(targetPageId, existingIndex, parameters);
                    break;
                    
                case JumpOperation.RemoveAndJump:
                    RemoveIntermediatePagesAndJump(targetPageId, existingIndex, parameters);
                    break;
                    
                case JumpOperation.SwapPosition:
                    SwapPagePositions(targetPageId, existingIndex);
                    break;
                    
                default:
                    BringPageToTop(targetPageId, existingIndex, parameters);
                    break;
            }
        }
        
        /// <summary>
        /// 将页面移到栈顶（你的场景：A-B-C-D → A-C-D-B）
        /// </summary>
        private void BringPageToTop(string targetPageId, int existingIndex, object[] parameters)
        {
            // 获取目标页面项
            var targetItem = navigationStack[existingIndex];
            
            // 从原位置移除
            navigationStack.RemoveAt(existingIndex);
            
            // 添加到栈顶
            targetItem.parameters = parameters ?? targetItem.parameters;
            targetItem.openTime = DateTime.Now;
            navigationStack.Add(targetItem);
            
            // 更新页面显示状态
            UpdatePageVisibility();
            
            if (config.enableDebugLog)
            {
                Debug.Log($"[高级UI导航] 页面移至栈顶: {targetPageId}");
                LogCurrentStack();
            }
        }
        
        /// <summary>
        /// 替换栈顶页面
        /// </summary>
        private void ReplaceTopPage(string targetPageId, int existingIndex, object[] parameters)
        {
            // 关闭当前栈顶页面
            if (navigationStack.Count > 0)
            {
                var currentTop = navigationStack[navigationStack.Count - 1];
                ClosePage(currentTop.pageId);
                navigationStack.RemoveAt(navigationStack.Count - 1);
            }
            
            // 将目标页面移到栈顶
            BringPageToTop(targetPageId, existingIndex, parameters);
        }
        
        /// <summary>
        /// 移除中间页面并跳转
        /// </summary>
        private void RemoveIntermediatePagesAndJump(string targetPageId, int existingIndex, object[] parameters)
        {
            // 关闭目标页面之后的所有页面
            for (int i = navigationStack.Count - 1; i > existingIndex; i--)
            {
                var item = navigationStack[i];
                ClosePage(item.pageId);
                navigationStack.RemoveAt(i);
            }
            
            // 激活目标页面
            var targetItem = navigationStack[existingIndex];
            targetItem.parameters = parameters ?? targetItem.parameters;
            OpenPage(targetPageId, targetItem.parameters);
            
            UpdatePageVisibility();
        }
        
        /// <summary>
        /// 交换页面位置
        /// </summary>
        private void SwapPagePositions(string targetPageId, int existingIndex)
        {
            if (navigationStack.Count < 2) return;
            
            int topIndex = navigationStack.Count - 1;
            if (existingIndex == topIndex) return;
            
            // 交换位置
            var temp = navigationStack[existingIndex];
            navigationStack[existingIndex] = navigationStack[topIndex];
            navigationStack[topIndex] = temp;
            
            UpdatePageVisibility();
        }
        
        /// <summary>
        /// 处理线性导航
        /// </summary>
        private void HandleLinearNavigation(string targetPageId, object[] parameters)
        {
            // 创建新的导航项
            var newItem = new NavigationStackItem(targetPageId, parameters);
            
            // 检查栈大小限制
            if (navigationStack.Count >= config.maxStackSize)
            {
                OptimizeNavigationStack();
            }
            
            // 添加到栈顶
            navigationStack.Add(newItem);
            
            // 打开页面
            OpenPage(targetPageId, parameters);
            
            UpdatePageVisibility();
        }
        
        #endregion
        
        #region 页面管理
        
        /// <summary>
        /// 打开页面
        /// </summary>
        private UIBase OpenPage(string pageId, object[] parameters = null)
        {
            UIBase page = null;
            
            // 检查是否已激活
            if (activePages.ContainsKey(pageId))
            {
                page = activePages[pageId];
                page.gameObject.SetActive(true);
                return page;
            }
            
            // 从缓存获取或创建新实例
            if (cachedPages.ContainsKey(pageId))
            {
                page = cachedPages[pageId];
                cachedPages.Remove(pageId);
            }
            else
            {
                page = CreatePageInstance(pageId);
            }
            
            if (page == null)
            {
                Debug.LogError($"[高级UI导航] 创建页面失败: {pageId}");
                return null;
            }
            
            // 激活并添加到活跃页面
            page.gameObject.SetActive(true);
            activePages[pageId] = page;
            
            // 触发事件
            OnPageOpened?.Invoke(pageId);
            
            if (config.enableDebugLog)
                Debug.Log($"[高级UI导航] 打开页面: {pageId}");
            
            return page;
        }
        
        /// <summary>
        /// 关闭页面
        /// </summary>
        private void ClosePage(string pageId)
        {
            if (!activePages.ContainsKey(pageId))
                return;
            
            var page = activePages[pageId];
            activePages.Remove(pageId);
            
            // 决定是缓存还是销毁
            if (cachedPages.Count < config.maxCachedPages)
            {
                page.gameObject.SetActive(false);
                cachedPages[pageId] = page;
            }
            else
            {
                DestroyImmediate(page.gameObject);
            }
            
            // 触发事件
            OnPageClosed?.Invoke(pageId);
            
            if (config.enableDebugLog)
                Debug.Log($"[高级UI导航] 关闭页面: {pageId}");
        }
        
        /// <summary>
        /// 创建页面实例
        /// </summary>
        private UIBase CreatePageInstance(string pageId)
        {
            if (!pagePrefabs.ContainsKey(pageId))
            {
                // 尝试从Resources加载
                var prefab = Resources.Load<GameObject>($"UI/{pageId}");
                if (prefab != null)
                {
                    pagePrefabs[pageId] = prefab;
                }
                else
                {
                    Debug.LogError($"[高级UI导航] 未找到页面预制体: {pageId}");
                    return null;
                }
            }
            
            var instance = Instantiate(pagePrefabs[pageId], uiRoot);
            return instance.GetComponent<UIBase>();
        }
        
        /// <summary>
        /// 更新页面可见性
        /// </summary>
        private void UpdatePageVisibility()
        {
            // 隐藏所有页面
            foreach (var page in activePages.Values)
            {
                page.gameObject.SetActive(false);
            }
            
            // 显示栈顶页面
            if (navigationStack.Count > 0)
            {
                var topItem = navigationStack[navigationStack.Count - 1];
                if (activePages.ContainsKey(topItem.pageId))
                {
                    activePages[topItem.pageId].gameObject.SetActive(true);
                }
            }
        }
        
        #endregion
        
        #region 导航栈管理
        
        /// <summary>
        /// 在栈中查找页面
        /// </summary>
        private int FindPageInStack(string pageId)
        {
            for (int i = 0; i < navigationStack.Count; i++)
            {
                if (navigationStack[i].pageId == pageId)
                    return i;
            }
            return -1;
        }
        
        /// <summary>
        /// 获取当前页面ID
        /// </summary>
        public string GetCurrentPageId()
        {
            if (navigationStack.Count == 0)
                return null;
            return navigationStack[navigationStack.Count - 1].pageId;
        }
        
        /// <summary>
        /// 返回上一页面
        /// </summary>
        public void NavigateBack()
        {
            if (navigationStack.Count <= 1)
            {
                Debug.LogWarning("[高级UI导航] 无法返回，栈中只有一个或没有页面");
                return;
            }
            
            // 移除当前页面
            var currentItem = navigationStack[navigationStack.Count - 1];
            navigationStack.RemoveAt(navigationStack.Count - 1);
            ClosePage(currentItem.pageId);
            
            // 激活上一个页面
            if (navigationStack.Count > 0)
            {
                var previousItem = navigationStack[navigationStack.Count - 1];
                OpenPage(previousItem.pageId, previousItem.parameters);
            }
            
            UpdatePageVisibility();
            OnStackChanged?.Invoke(navigationStack);
        }
        
        /// <summary>
        /// 优化导航栈
        /// </summary>
        private void OptimizeNavigationStack()
        {
            if (!config.enableStackOptimization)
                return;
            
            // 移除重复页面（保留最新的）
            if (config.enableDuplicateDetection)
            {
                RemoveDuplicatePages();
            }
            
            // 如果栈仍然太大，移除最旧的页面
            while (navigationStack.Count >= config.maxStackSize)
            {
                var oldestItem = navigationStack[0];
                navigationStack.RemoveAt(0);
                ClosePage(oldestItem.pageId);
            }
        }
        
        /// <summary>
        /// 移除重复页面
        /// </summary>
        private void RemoveDuplicatePages()
        {
            var uniquePages = new Dictionary<string, int>();
            var toRemove = new List<int>();
            
            // 从后往前遍历，保留最新的页面
            for (int i = navigationStack.Count - 1; i >= 0; i--)
            {
                string pageId = navigationStack[i].pageId;
                if (uniquePages.ContainsKey(pageId))
                {
                    toRemove.Add(i);
                }
                else
                {
                    uniquePages[pageId] = i;
                }
            }
            
            // 移除重复页面
            toRemove.Sort();
            for (int i = toRemove.Count - 1; i >= 0; i--)
            {
                var item = navigationStack[toRemove[i]];
                navigationStack.RemoveAt(toRemove[i]);
                ClosePage(item.pageId);
            }
        }
        
        /// <summary>
        /// 清空导航栈
        /// </summary>
        public void ClearNavigationStack()
        {
            foreach (var item in navigationStack)
            {
                ClosePage(item.pageId);
            }
            navigationStack.Clear();
            OnStackChanged?.Invoke(navigationStack);
        }
        
        #endregion
        
        #region 高级功能
        
        /// <summary>
        /// 预加载常用页面
        /// </summary>
        private void PreloadCommonPages()
        {
            string[] commonPages = { "MainMenu", "Settings", "Pause" };
            foreach (string pageId in commonPages)
            {
                var page = CreatePageInstance(pageId);
                if (page != null)
                {
                    page.gameObject.SetActive(false);
                    cachedPages[pageId] = page;
                }
            }
        }
        
        /// <summary>
        /// 获取导航栈信息
        /// </summary>
        public string GetNavigationStackInfo()
        {
            if (navigationStack.Count == 0)
                return "导航栈为空";
            
            var info = "导航栈 (从底到顶):\n";
            for (int i = 0; i < navigationStack.Count; i++)
            {
                var item = navigationStack[i];
                string marker = (i == navigationStack.Count - 1) ? " ← 当前" : "";
                info += $"{i + 1}. {item.pageId} (打开时间: {item.openTime:HH:mm:ss}){marker}\n";
            }
            return info;
        }
        
        /// <summary>
        /// 记录当前栈状态
        /// </summary>
        private void LogCurrentStack()
        {
            if (!config.enableDebugLog) return;
            
            string stackInfo = GetNavigationStackInfo();
            Debug.Log($"[高级UI导航] {stackInfo}");
        }
        
        /// <summary>
        /// 获取导航历史
        /// </summary>
        public List<string> GetNavigationHistory()
        {
            return new List<string>(navigationHistory);
        }
        
        #endregion
        
        #region 公共接口
        
        /// <summary>
        /// 注册页面预制体
        /// </summary>
        public void RegisterPagePrefab(string pageId, GameObject prefab)
        {
            pagePrefabs[pageId] = prefab;
        }
        
        /// <summary>
        /// 检查页面是否在栈中
        /// </summary>
        public bool IsPageInStack(string pageId)
        {
            return FindPageInStack(pageId) >= 0;
        }
        
        /// <summary>
        /// 获取栈中页面数量
        /// </summary>
        public int GetStackSize()
        {
            return navigationStack.Count;
        }
        
        /// <summary>
        /// 获取页面在栈中的位置
        /// </summary>
        public int GetPageStackPosition(string pageId)
        {
            return FindPageInStack(pageId);
        }
        
        #endregion
    }
}