using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.UI
{
    /// <summary>
    /// UI导航系统 - 混合状态机和传统UI管理的优势
    /// 适用于复杂界面流程的配置化管理
    /// </summary>
    public class UINavigationSystem : MonoBehaviour
    {
        #region 配置数据结构
        
        /// <summary>
        /// UI导航配置
        /// </summary>
        [System.Serializable]
        public class UINavigationConfig
        {
            [Header("基本信息")]
            public string configName = "默认UI导航配置";
            
            [Header("界面定义")]
            public List<UIPageConfig> pages = new List<UIPageConfig>();
            
            [Header("跳转规则")]
            public List<UITransitionRule> transitionRules = new List<UITransitionRule>();
            
            [Header("全局设置")]
            public bool enableBackStack = true;
            public int maxStackDepth = 10;
            public bool enableTransitionAnimation = true;
            public float defaultTransitionDuration = 0.3f;
        }
        
        /// <summary>
        /// UI页面配置
        /// </summary>
        [System.Serializable]
        public class UIPageConfig
        {
            [Header("页面信息")]
            public string pageId;
            public string displayName;
            public string prefabPath;
            
            [Header("页面属性")]
            public UIPageType pageType = UIPageType.Normal;
            public UILayerType layerType = UILayerType.UI;
            public bool isModal = false;
            public bool allowMultipleInstances = false;
            
            [Header("生命周期")]
            public bool preload = false;
            public bool cacheWhenClosed = false;
            public float autoCloseDelay = 0f;
            
            [Header("动画设置")]
            public PanelShowType showAnimation = PanelShowType.None;
            public PanelShowType hideAnimation = PanelShowType.None;
        }
        
        /// <summary>
        /// UI跳转规则
        /// </summary>
        [System.Serializable]
        public class UITransitionRule
        {
            [Header("跳转条件")]
            public string fromPageId;
            public string toPageId;
            public string triggerEvent;
            
            [Header("跳转行为")]
            public UITransitionType transitionType = UITransitionType.Replace;
            public bool validateCondition = false;
            public string conditionMethod;
            
            [Header("参数传递")]
            public List<string> parameterKeys = new List<string>();
        }
        
        /// <summary>
        /// UI页面类型
        /// </summary>
        public enum UIPageType
        {
            Normal,     // 普通页面
            Popup,      // 弹窗
            Dialog,     // 对话框
            Loading,    // 加载页面
            HUD         // 游戏内UI
        }
        
        /// <summary>
        /// UI层级类型
        /// </summary>
        public enum UILayerType
        {
            Background, // 背景层
            UI,         // 普通UI层
            Popup,      // 弹窗层
            Dialog,     // 对话框层
            System,     // 系统层
            Debug       // 调试层
        }
        
        /// <summary>
        /// UI跳转类型
        /// </summary>
        public enum UITransitionType
        {
            Replace,    // 替换当前页面
            Push,       // 压入栈中
            Pop,        // 弹出栈顶
            Overlay,    // 覆盖显示
            Parallel    // 并行显示
        }
        
        #endregion
        
        #region 字段和属性
        
        [Header("配置文件")]
        [SerializeField] private UINavigationConfig navigationConfig;
        
        [Header("运行时状态")]
        [SerializeField] private bool enableDebugLog = true;
        [SerializeField] private bool showRuntimeInfo = false;
        
        // 运行时数据
        private Dictionary<string, UIPageConfig> pageConfigs;
        private Dictionary<string, UIBase> activePages;
        private Stack<string> navigationStack;
        private Dictionary<string, UIBase> cachedPages;
        private Dictionary<UILayerType, Transform> layerContainers;
        
        // 事件系统
        public event Action<string, string> OnPageTransition;
        public event Action<string> OnPageOpened;
        public event Action<string> OnPageClosed;
        
        #endregion
        
        #region 初始化
        
        void Awake()
        {
            InitializeSystem();
        }
        
        /// <summary>
        /// 初始化UI导航系统
        /// </summary>
        private void InitializeSystem()
        {
            // 初始化数据结构
            pageConfigs = new Dictionary<string, UIPageConfig>();
            activePages = new Dictionary<string, UIBase>();
            navigationStack = new Stack<string>();
            cachedPages = new Dictionary<string, UIBase>();
            layerContainers = new Dictionary<UILayerType, Transform>();
            
            // 加载配置
            LoadNavigationConfig();
            
            // 创建UI层级
            CreateUILayers();
            
            // 预加载页面
            PreloadPages();
            
            if (enableDebugLog)
                Debug.Log("[UI导航] 系统初始化完成");
        }
        
        /// <summary>
        /// 加载导航配置
        /// </summary>
        private void LoadNavigationConfig()
        {
            if (navigationConfig == null)
            {
                Debug.LogWarning("[UI导航] 未配置导航规则，使用默认配置");
                navigationConfig = CreateDefaultConfig();
            }
            
            // 构建页面配置字典
            foreach (var pageConfig in navigationConfig.pages)
            {
                if (!string.IsNullOrEmpty(pageConfig.pageId))
                {
                    pageConfigs[pageConfig.pageId] = pageConfig;
                }
            }
            
            if (enableDebugLog)
                Debug.Log($"[UI导航] 加载了 {pageConfigs.Count} 个页面配置");
        }
        
        /// <summary>
        /// 创建UI层级容器
        /// </summary>
        private void CreateUILayers()
        {
            var uiManager = UIManager.Instance;
            if (uiManager == null)
            {
                Debug.LogError("[UI导航] 未找到UIManager实例");
                return;
            }
            
            // 映射UI层级
            layerContainers[UILayerType.Background] = uiManager.m_UI;
            layerContainers[UILayerType.UI] = uiManager.m_UI;
            layerContainers[UILayerType.Popup] = uiManager.m_UpUI;
            layerContainers[UILayerType.Dialog] = uiManager.m_Dialog;
            layerContainers[UILayerType.System] = uiManager.m_UpDialog;
            layerContainers[UILayerType.Debug] = uiManager.m_Message;
        }
        
        /// <summary>
        /// 预加载页面
        /// </summary>
        private void PreloadPages()
        {
            foreach (var config in pageConfigs.Values)
            {
                if (config.preload)
                {
                    PreloadPage(config.pageId);
                }
            }
        }
        
        #endregion
        
        #region 核心导航方法
        
        /// <summary>
        /// 导航到指定页面
        /// </summary>
        /// <param name="pageId">页面ID</param>
        /// <param name="transitionType">跳转类型</param>
        /// <param name="parameters">传递参数</param>
        public void NavigateTo(string pageId, UITransitionType transitionType = UITransitionType.Replace, params object[] parameters)
        {
            if (!pageConfigs.ContainsKey(pageId))
            {
                Debug.LogError($"[UI导航] 未找到页面配置: {pageId}");
                return;
            }
            
            var config = pageConfigs[pageId];
            string currentPageId = GetCurrentPageId();
            
            // 验证跳转条件
            if (!ValidateTransition(currentPageId, pageId))
            {
                Debug.LogWarning($"[UI导航] 跳转被阻止: {currentPageId} -> {pageId}");
                return;
            }
            
            // 执行跳转
            ExecuteTransition(currentPageId, pageId, transitionType, parameters);
        }
        
        /// <summary>
        /// 根据配置的规则进行导航
        /// </summary>
        /// <param name="triggerEvent">触发事件</param>
        /// <param name="parameters">参数</param>
        public void NavigateByEvent(string triggerEvent, params object[] parameters)
        {
            string currentPageId = GetCurrentPageId();
            
            // 查找匹配的跳转规则
            var rule = FindTransitionRule(currentPageId, triggerEvent);
            if (rule == null)
            {
                Debug.LogWarning($"[UI导航] 未找到匹配的跳转规则: {currentPageId} + {triggerEvent}");
                return;
            }
            
            // 执行配置的跳转
            NavigateTo(rule.toPageId, rule.transitionType, parameters);
        }
        
        /// <summary>
        /// 返回上一页面
        /// </summary>
        public void NavigateBack()
        {
            if (!navigationConfig.enableBackStack || navigationStack.Count == 0)
            {
                Debug.LogWarning("[UI导航] 无法返回，导航栈为空");
                return;
            }
            
            string previousPageId = navigationStack.Pop();
            NavigateTo(previousPageId, UITransitionType.Replace);
        }
        
        /// <summary>
        /// 关闭当前页面
        /// </summary>
        public void CloseCurrent()
        {
            string currentPageId = GetCurrentPageId();
            if (!string.IsNullOrEmpty(currentPageId))
            {
                ClosePage(currentPageId);
            }
        }
        
        #endregion
        
        #region 页面管理
        
        /// <summary>
        /// 打开页面
        /// </summary>
        private UIBase OpenPage(string pageId, params object[] parameters)
        {
            var config = pageConfigs[pageId];
            UIBase page = null;
            
            // 检查是否已经打开
            if (activePages.ContainsKey(pageId))
            {
                if (!config.allowMultipleInstances)
                {
                    page = activePages[pageId];
                    page.gameObject.SetActive(true);
                    return page;
                }
            }
            
            // 从缓存中获取或创建新实例
            page = GetOrCreatePage(pageId);
            if (page == null)
            {
                Debug.LogError($"[UI导航] 创建页面失败: {pageId}");
                return null;
            }
            
            // 设置到正确的层级
            SetPageLayer(page, config.layerType);
            
            // 激活页面
            page.gameObject.SetActive(true);
            activePages[pageId] = page;
            
            // 播放显示动画
            PlayShowAnimation(page, config.showAnimation);
            
            // 触发事件
            OnPageOpened?.Invoke(pageId);
            
            if (enableDebugLog)
                Debug.Log($"[UI导航] 打开页面: {pageId}");
            
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
            var config = pageConfigs[pageId];
            
            // 播放隐藏动画
            PlayHideAnimation(page, config.hideAnimation, () =>
            {
                // 动画完成后的回调
                page.gameObject.SetActive(false);
                activePages.Remove(pageId);
                
                // 是否缓存页面
                if (config.cacheWhenClosed)
                {
                    cachedPages[pageId] = page;
                }
                else
                {
                    DestroyPage(page);
                }
                
                // 触发事件
                OnPageClosed?.Invoke(pageId);
                
                if (enableDebugLog)
                    Debug.Log($"[UI导航] 关闭页面: {pageId}");
            });
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 获取当前活跃的页面ID
        /// </summary>
        private string GetCurrentPageId()
        {
            // 简化实现：返回最后打开的页面
            foreach (var kvp in activePages)
            {
                if (kvp.Value.gameObject.activeInHierarchy)
                    return kvp.Key;
            }
            return null;
        }
        
        /// <summary>
        /// 验证页面跳转
        /// </summary>
        private bool ValidateTransition(string fromPageId, string toPageId)
        {
            // 查找跳转规则
            var rule = FindTransitionRule(fromPageId, toPageId);
            if (rule != null && rule.validateCondition)
            {
                // 这里可以调用自定义的验证方法
                return CallValidationMethod(rule.conditionMethod);
            }
            
            return true; // 默认允许跳转
        }
        
        /// <summary>
        /// 查找跳转规则
        /// </summary>
        private UITransitionRule FindTransitionRule(string fromPageId, string identifier)
        {
            foreach (var rule in navigationConfig.transitionRules)
            {
                if (rule.fromPageId == fromPageId && 
                    (rule.toPageId == identifier || rule.triggerEvent == identifier))
                {
                    return rule;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 执行页面跳转
        /// </summary>
        private void ExecuteTransition(string fromPageId, string toPageId, UITransitionType transitionType, object[] parameters)
        {
            switch (transitionType)
            {
                case UITransitionType.Replace:
                    if (!string.IsNullOrEmpty(fromPageId))
                    {
                        ClosePage(fromPageId);
                        if (navigationConfig.enableBackStack)
                            navigationStack.Push(fromPageId);
                    }
                    OpenPage(toPageId, parameters);
                    break;
                    
                case UITransitionType.Push:
                    if (navigationConfig.enableBackStack && !string.IsNullOrEmpty(fromPageId))
                        navigationStack.Push(fromPageId);
                    OpenPage(toPageId, parameters);
                    break;
                    
                case UITransitionType.Pop:
                    ClosePage(toPageId);
                    NavigateBack();
                    break;
                    
                case UITransitionType.Overlay:
                    OpenPage(toPageId, parameters);
                    break;
                    
                case UITransitionType.Parallel:
                    OpenPage(toPageId, parameters);
                    break;
            }
            
            // 触发跳转事件
            OnPageTransition?.Invoke(fromPageId, toPageId);
        }
        
        /// <summary>
        /// 获取或创建页面实例
        /// </summary>
        private UIBase GetOrCreatePage(string pageId)
        {
            // 先检查缓存
            if (cachedPages.ContainsKey(pageId))
            {
                var cachedPage = cachedPages[pageId];
                cachedPages.Remove(pageId);
                return cachedPage;
            }
            
            // 创建新实例
            var config = pageConfigs[pageId];
            // 这里应该通过资源管理器加载预制体
            // GameObject prefab = ResourceManager.LoadPrefab(config.prefabPath);
            // GameObject instance = Instantiate(prefab);
            // return instance.GetComponent<UIBase>();
            
            // 临时返回null，实际项目中需要实现资源加载
            Debug.LogWarning($"[UI导航] 页面创建未实现: {pageId}");
            return null;
        }
        
        /// <summary>
        /// 设置页面层级
        /// </summary>
        private void SetPageLayer(UIBase page, UILayerType layerType)
        {
            if (layerContainers.ContainsKey(layerType))
            {
                page.transform.SetParent(layerContainers[layerType], false);
            }
        }
        
        /// <summary>
        /// 播放显示动画
        /// </summary>
        private void PlayShowAnimation(UIBase page, PanelShowType animationType)
        {
            // 这里应该实现具体的动画逻辑
            // 可以使用DOTween或Unity Animation
            if (enableDebugLog)
                Debug.Log($"[UI导航] 播放显示动画: {animationType}");
        }
        
        /// <summary>
        /// 播放隐藏动画
        /// </summary>
        private void PlayHideAnimation(UIBase page, PanelShowType animationType, Action onComplete)
        {
            // 这里应该实现具体的动画逻辑
            if (enableDebugLog)
                Debug.Log($"[UI导航] 播放隐藏动画: {animationType}");
            
            // 临时直接调用完成回调
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 销毁页面
        /// </summary>
        private void DestroyPage(UIBase page)
        {
            if (page != null)
            {
                Destroy(page.gameObject);
            }
        }
        
        /// <summary>
        /// 预加载页面
        /// </summary>
        private void PreloadPage(string pageId)
        {
            var page = GetOrCreatePage(pageId);
            if (page != null)
            {
                page.gameObject.SetActive(false);
                cachedPages[pageId] = page;
                
                if (enableDebugLog)
                    Debug.Log($"[UI导航] 预加载页面: {pageId}");
            }
        }
        
        /// <summary>
        /// 调用验证方法
        /// </summary>
        private bool CallValidationMethod(string methodName)
        {
            // 这里可以通过反射或委托调用自定义验证方法
            // 简化实现，直接返回true
            return true;
        }
        
        /// <summary>
        /// 创建默认配置
        /// </summary>
        private UINavigationConfig CreateDefaultConfig()
        {
            return new UINavigationConfig
            {
                configName = "默认配置",
                enableBackStack = true,
                maxStackDepth = 10,
                enableTransitionAnimation = true,
                defaultTransitionDuration = 0.3f
            };
        }
        
        #endregion
        
        #region 调试和工具方法
        
        /// <summary>
        /// 获取当前导航状态信息
        /// </summary>
        public string GetNavigationInfo()
        {
            var info = $"当前活跃页面: {activePages.Count}\n";
            info += $"导航栈深度: {navigationStack.Count}\n";
            info += $"缓存页面: {cachedPages.Count}\n";
            info += $"配置页面: {pageConfigs.Count}";
            return info;
        }
        
        /// <summary>
        /// 清理所有页面
        /// </summary>
        public void ClearAllPages()
        {
            // 关闭所有活跃页面
            var activePageIds = new List<string>(activePages.Keys);
            foreach (var pageId in activePageIds)
            {
                ClosePage(pageId);
            }
            
            // 清理缓存
            foreach (var page in cachedPages.Values)
            {
                DestroyPage(page);
            }
            cachedPages.Clear();
            
            // 清理导航栈
            navigationStack.Clear();
            
            if (enableDebugLog)
                Debug.Log("[UI导航] 清理所有页面完成");
        }
        
        #endregion
        
        #region Unity编辑器支持
        
#if UNITY_EDITOR
        [ContextMenu("创建示例配置")]
        private void CreateExampleConfig()
        {
            navigationConfig = new UINavigationConfig
            {
                configName = "示例游戏UI配置",
                pages = new List<UIPageConfig>
                {
                    new UIPageConfig
                    {
                        pageId = "MainMenu",
                        displayName = "主菜单",
                        prefabPath = "UI/MainMenuPanel",
                        pageType = UIPageType.Normal,
                        layerType = UILayerType.UI,
                        preload = true
                    },
                    new UIPageConfig
                    {
                        pageId = "Settings",
                        displayName = "设置",
                        prefabPath = "UI/SettingsPanel",
                        pageType = UIPageType.Popup,
                        layerType = UILayerType.Popup,
                        isModal = true
                    }
                },
                transitionRules = new List<UITransitionRule>
                {
                    new UITransitionRule
                    {
                        fromPageId = "MainMenu",
                        toPageId = "Settings",
                        triggerEvent = "OpenSettings",
                        transitionType = UITransitionType.Push
                    }
                }
            };
            
            Debug.Log("[UI导航] 创建示例配置完成");
        }
#endif
        
        #endregion
    }
}