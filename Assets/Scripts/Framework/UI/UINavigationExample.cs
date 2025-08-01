using UnityEngine;
using Framework.UI;

namespace Framework.UI.Examples
{
    /// <summary>
    /// UI导航系统使用示例
    /// 展示如何在实际项目中配置和使用界面跳转
    /// </summary>
    public class UINavigationExample : MonoBehaviour
    {
        [Header("UI导航系统")]
        public UINavigationSystem navigationSystem;
        
        [Header("测试按钮")]
        public KeyCode openMainMenuKey = KeyCode.M;
        public KeyCode openSettingsKey = KeyCode.S;
        public KeyCode openInventoryKey = KeyCode.I;
        public KeyCode backKey = KeyCode.Escape;
        
        void Start()
        {
            InitializeNavigationSystem();
            SetupEventListeners();
        }
        
        void Update()
        {
            HandleTestInput();
        }
        
        /// <summary>
        /// 初始化导航系统
        /// </summary>
        private void InitializeNavigationSystem()
        {
            if (navigationSystem == null)
            {
                Debug.LogError("[UI导航示例] 未分配导航系统");
                return;
            }
            
            // 监听导航事件
            navigationSystem.OnPageTransition += OnPageTransition;
            navigationSystem.OnPageOpened += OnPageOpened;
            navigationSystem.OnPageClosed += OnPageClosed;
            
            Debug.Log("[UI导航示例] 导航系统初始化完成");
        }
        
        /// <summary>
        /// 设置事件监听
        /// </summary>
        private void SetupEventListeners()
        {
            // 这里可以设置UI按钮的点击事件
            // 例如：mainMenuButton.onClick.AddListener(() => OpenMainMenu());
        }
        
        /// <summary>
        /// 处理测试输入
        /// </summary>
        private void HandleTestInput()
        {
            if (Input.GetKeyDown(openMainMenuKey))
            {
                OpenMainMenu();
            }
            else if (Input.GetKeyDown(openSettingsKey))
            {
                OpenSettings();
            }
            else if (Input.GetKeyDown(openInventoryKey))
            {
                OpenInventory();
            }
            else if (Input.GetKeyDown(backKey))
            {
                GoBack();
            }
        }
        
        #region 公共导航方法
        
        /// <summary>
        /// 打开主菜单
        /// </summary>
        public void OpenMainMenu()
        {
            navigationSystem.NavigateTo("MainMenu", UINavigationSystem.UITransitionType.Replace);
            Debug.Log("[UI导航示例] 导航到主菜单");
        }
        
        /// <summary>
        /// 打开设置界面
        /// </summary>
        public void OpenSettings()
        {
            navigationSystem.NavigateTo("Settings", UINavigationSystem.UITransitionType.Push);
            Debug.Log("[UI导航示例] 导航到设置界面");
        }
        
        /// <summary>
        /// 打开背包界面
        /// </summary>
        public void OpenInventory()
        {
            navigationSystem.NavigateTo("Inventory", UINavigationSystem.UITransitionType.Push);
            Debug.Log("[UI导航示例] 导航到背包界面");
        }
        
        /// <summary>
        /// 打开商店界面
        /// </summary>
        public void OpenShop()
        {
            navigationSystem.NavigateTo("Shop", UINavigationSystem.UITransitionType.Overlay);
            Debug.Log("[UI导航示例] 导航到商店界面");
        }
        
        /// <summary>
        /// 打开游戏界面
        /// </summary>
        public void StartGame()
        {
            navigationSystem.NavigateTo("GamePlay", UINavigationSystem.UITransitionType.Replace);
            Debug.Log("[UI导航示例] 开始游戏");
        }
        
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            navigationSystem.NavigateTo("Pause", UINavigationSystem.UITransitionType.Push);
            Debug.Log("[UI导航示例] 暂停游戏");
        }
        
        /// <summary>
        /// 返回上一界面
        /// </summary>
        public void GoBack()
        {
            navigationSystem.NavigateBack();
            Debug.Log("[UI导航示例] 返回上一界面");
        }
        
        /// <summary>
        /// 关闭当前界面
        /// </summary>
        public void CloseCurrent()
        {
            navigationSystem.CloseCurrent();
            Debug.Log("[UI导航示例] 关闭当前界面");
        }
        
        #endregion
        
        #region 事件触发导航
        
        /// <summary>
        /// 通过事件触发导航
        /// </summary>
        public void TriggerNavigationEvent(string eventName)
        {
            navigationSystem.NavigateByEvent(eventName);
            Debug.Log($"[UI导航示例] 触发导航事件: {eventName}");
        }
        
        /// <summary>
        /// 玩家登录成功
        /// </summary>
        public void OnPlayerLoginSuccess()
        {
            TriggerNavigationEvent("LoginSuccess");
        }
        
        /// <summary>
        /// 玩家登录失败
        /// </summary>
        public void OnPlayerLoginFailed()
        {
            TriggerNavigationEvent("LoginFailed");
        }
        
        /// <summary>
        /// 游戏结束
        /// </summary>
        public void OnGameOver()
        {
            TriggerNavigationEvent("GameOver");
        }
        
        /// <summary>
        /// 关卡完成
        /// </summary>
        public void OnLevelComplete()
        {
            TriggerNavigationEvent("LevelComplete");
        }
        
        #endregion
        
        #region 事件回调
        
        /// <summary>
        /// 页面跳转回调
        /// </summary>
        private void OnPageTransition(string fromPageId, string toPageId)
        {
            Debug.Log($"[UI导航示例] 页面跳转: {fromPageId} → {toPageId}");
            
            // 可以在这里添加全局的页面跳转逻辑
            // 例如：播放音效、记录统计数据等
        }
        
        /// <summary>
        /// 页面打开回调
        /// </summary>
        private void OnPageOpened(string pageId)
        {
            Debug.Log($"[UI导航示例] 页面打开: {pageId}");
            
            // 可以在这里添加页面打开时的逻辑
            // 例如：更新UI状态、加载数据等
        }
        
        /// <summary>
        /// 页面关闭回调
        /// </summary>
        private void OnPageClosed(string pageId)
        {
            Debug.Log($"[UI导航示例] 页面关闭: {pageId}");
            
            // 可以在这里添加页面关闭时的逻辑
            // 例如：保存数据、清理资源等
        }
        
        #endregion
        
        #region 高级用法示例
        
        /// <summary>
        /// 带参数的页面导航
        /// </summary>
        public void OpenShopWithCategory(string category)
        {
            navigationSystem.NavigateTo("Shop", UINavigationSystem.UITransitionType.Push, category);
            Debug.Log($"[UI导航示例] 打开商店 - 分类: {category}");
        }
        
        /// <summary>
        /// 条件导航示例
        /// </summary>
        public void OpenInventoryIfAllowed()
        {
            // 检查是否允许打开背包
            if (CanOpenInventory())
            {
                OpenInventory();
            }
            else
            {
                Debug.LogWarning("[UI导航示例] 当前状态不允许打开背包");
                // 可以显示提示信息
                ShowMessage("当前状态不允许打开背包");
            }
        }
        
        /// <summary>
        /// 检查是否可以打开背包
        /// </summary>
        private bool CanOpenInventory()
        {
            // 这里可以添加具体的条件检查逻辑
            // 例如：游戏状态、玩家权限等
            return true;
        }
        
        /// <summary>
        /// 显示消息提示
        /// </summary>
        private void ShowMessage(string message)
        {
            // 这里可以显示消息提示UI
            Debug.Log($"[消息提示] {message}");
        }
        
        /// <summary>
        /// 批量页面操作示例
        /// </summary>
        public void ShowGameHUD()
        {
            // 同时显示多个游戏内UI
            navigationSystem.NavigateTo("HealthBar", UINavigationSystem.UITransitionType.Parallel);
            navigationSystem.NavigateTo("MiniMap", UINavigationSystem.UITransitionType.Parallel);
            navigationSystem.NavigateTo("SkillBar", UINavigationSystem.UITransitionType.Parallel);
            
            Debug.Log("[UI导航示例] 显示游戏HUD");
        }
        
        /// <summary>
        /// 隐藏游戏HUD
        /// </summary>
        public void HideGameHUD()
        {
            // 关闭所有游戏内UI
            navigationSystem.CloseCurrent(); // 这里需要扩展支持批量关闭
            
            Debug.Log("[UI导航示例] 隐藏游戏HUD");
        }
        
        #endregion
        
        #region Unity生命周期
        
        void OnDestroy()
        {
            // 清理事件监听
            if (navigationSystem != null)
            {
                navigationSystem.OnPageTransition -= OnPageTransition;
                navigationSystem.OnPageOpened -= OnPageOpened;
                navigationSystem.OnPageClosed -= OnPageClosed;
            }
        }
        
        #endregion
        
        #region 调试和测试方法
        
        /// <summary>
        /// 打印当前导航状态
        /// </summary>
        [ContextMenu("打印导航状态")]
        public void PrintNavigationState()
        {
            if (navigationSystem != null)
            {
                string info = navigationSystem.GetNavigationInfo();
                Debug.Log($"[UI导航示例] 当前状态:\n{info}");
            }
        }
        
        /// <summary>
        /// 测试所有页面
        /// </summary>
        [ContextMenu("测试所有页面")]
        public void TestAllPages()
        {
            // 依次测试所有配置的页面
            string[] testPages = { "MainMenu", "Settings", "Inventory", "Shop", "GamePlay" };
            
            foreach (string pageId in testPages)
            {
                Debug.Log($"[UI导航示例] 测试页面: {pageId}");
                navigationSystem.NavigateTo(pageId, UINavigationSystem.UITransitionType.Replace);
                
                // 在实际项目中，这里应该添加延迟或等待用户确认
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// UI导航最佳实践指南
    /// </summary>
    public static class UINavigationBestPractices
    {
        /// <summary>
        /// 界面跳转配置的最佳实践
        /// </summary>
        public static class ConfigurationGuidelines
        {
            // 1. 页面ID命名规范
            public const string MAIN_MENU = "MainMenu";
            public const string SETTINGS = "Settings";
            public const string INVENTORY = "Inventory";
            public const string SHOP = "Shop";
            public const string GAME_PLAY = "GamePlay";
            public const string PAUSE = "Pause";
            public const string GAME_OVER = "GameOver";
            
            // 2. 事件命名规范
            public const string LOGIN_SUCCESS = "LoginSuccess";
            public const string LOGIN_FAILED = "LoginFailed";
            public const string GAME_START = "GameStart";
            public const string GAME_PAUSE = "GamePause";
            public const string GAME_RESUME = "GameResume";
            public const string GAME_OVER_EVENT = "GameOver";
            public const string LEVEL_COMPLETE = "LevelComplete";
            
            // 3. 跳转类型使用建议
            // Replace: 主要页面切换（如主菜单→游戏）
            // Push: 临时页面（如设置、背包）
            // Overlay: 不阻塞的信息显示（如商店、提示）
            // Parallel: 同时显示的UI元素（如HUD组件）
        }
        
        /// <summary>
        /// 性能优化建议
        /// </summary>
        public static class PerformanceOptimization
        {
            // 1. 预加载常用页面
            // 2. 缓存频繁使用的页面
            // 3. 延迟加载大型页面
            // 4. 使用对象池管理UI实例
            // 5. 合理设置页面层级
        }
        
        /// <summary>
        /// 错误处理建议
        /// </summary>
        public static class ErrorHandling
        {
            // 1. 验证页面配置的完整性
            // 2. 处理页面加载失败的情况
            // 3. 提供默认的错误页面
            // 4. 记录导航错误日志
            // 5. 实现导航回退机制
        }
    }
}