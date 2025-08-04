using UnityEngine;
using Framework.EnhanceUI.Core;
using Framework.EnhanceUI.Config;

namespace Framework.EnhanceUI.Examples
{
    /// <summary>
    /// UI导航系统使用示例
    /// 演示如何在实际项目中使用多种UI打开策略和回退操作
    /// </summary>
    public class UINavigationExample : MonoBehaviour
    {
        [Header("示例UI配置")]
        [SerializeField] private string mainMenuUI = "MainMenu";
        [SerializeField] private string settingsUI = "Settings";
        [SerializeField] private string inventoryUI = "Inventory";
        [SerializeField] private string shopUI = "Shop";
        [SerializeField] private string dialogUI = "Dialog";
        [SerializeField] private string loadingUI = "Loading";
        
        [Header("测试配置")]
        [SerializeField] private bool enableTestButtons = true;
        [SerializeField] private KeyCode testKey1 = KeyCode.Alpha1;
        [SerializeField] private KeyCode testKey2 = KeyCode.Alpha2;
        [SerializeField] private KeyCode testKey3 = KeyCode.Alpha3;
        [SerializeField] private KeyCode testKey4 = KeyCode.Alpha4;
        [SerializeField] private KeyCode testKey5 = KeyCode.Alpha5;
        
        private EnhanceUIManager uiManager;
        private UINavigationManager navigationManager;
        private UINavigationInputHandler inputHandler;
        
        private void Start()
        {
            // 获取UI管理器实例
            uiManager = EnhanceUIManager.Instance;
            
            if (uiManager != null)
            {
                navigationManager = uiManager.NavigationManager;
                inputHandler = uiManager.NavigationInputHandler;
                
                // 绑定导航事件
                BindNavigationEvents();
                
                // 打开主菜单
                OpenMainMenu();
            }
            else
            {
                Debug.LogError("[UINavigationExample] 找不到EnhanceUIManager实例");
            }
        }
        
        private void Update()
        {
            if (!enableTestButtons)
                return;
            
            // 测试按键
            if (Input.GetKeyDown(testKey1))
            {
                TestSingleStrategy();
            }
            else if (Input.GetKeyDown(testKey2))
            {
                TestStackStrategy();
            }
            else if (Input.GetKeyDown(testKey3))
            {
                TestMultipleStrategy();
            }
            else if (Input.GetKeyDown(testKey4))
            {
                TestLimitedStrategy();
            }
            else if (Input.GetKeyDown(testKey5))
            {
                TestQueueStrategy();
            }
        }
        
        #region 导航事件处理
        
        /// <summary>
        /// 绑定导航事件
        /// </summary>
        private void BindNavigationEvents()
        {
            if (navigationManager != null)
            {
                navigationManager.OnUINavigatedBack += OnUINavigatedBack;
                navigationManager.OnNavigationStackChanged += OnNavigationStackChanged;
                navigationManager.OnNavigateBackFailed += OnNavigateBackFailed;
            }
            
            if (inputHandler != null)
            {
                inputHandler.OnBackInputReceived += OnBackInputReceived;
                inputHandler.OnBackOperationCompleted += OnBackOperationCompleted;
                inputHandler.OnExitApplicationRequested += OnExitApplicationRequested;
            }
        }
        
        private void OnUINavigatedBack(string uiName, UINavigationManager.NavigationContext context)
        {
            Debug.Log($"[导航示例] UI回退: {uiName}");
            
            if (context.autoReturnToSource && !string.IsNullOrEmpty(context.fromUI))
            {
                Debug.Log($"[导航示例] 自动返回来源UI: {context.fromUI}");
            }
        }
        
        private void OnNavigationStackChanged(System.Collections.Generic.List<UINavigationManager.NavigationStackItem> stack)
        {
            Debug.Log($"[导航示例] 导航栈变化，当前大小: {stack.Count}");
            
            // 打印当前栈内容（调试用）
            for (int i = 0; i < stack.Count; i++)
            {
                var item = stack[i];
                Debug.Log($"  [{i}] {item.uiName} ({item.strategy}) - {item.instanceId}");
            }
        }
        
        private void OnNavigateBackFailed(string reason)
        {
            Debug.LogWarning($"[导航示例] 回退失败: {reason}");
        }
        
        private void OnBackInputReceived(UINavigationInputHandler.BackInputType inputType)
        {
            Debug.Log($"[导航示例] 收到回退输入: {inputType}");
        }
        
        private void OnBackOperationCompleted(bool success)
        {
            Debug.Log($"[导航示例] 回退操作完成: {(success ? "成功" : "失败")}");
        }
        
        private void OnExitApplicationRequested()
        {
            Debug.Log("[导航示例] 请求退出应用");
            
            // 在实际项目中，这里可以显示确认对话框
            // 或者保存游戏数据等操作
        }
        
        #endregion
        
        #region 基础UI操作
        
        /// <summary>
        /// 打开主菜单
        /// </summary>
        public void OpenMainMenu()
        {
            if (uiManager != null)
            {
                // Single策略：确保只有一个主菜单实例
                uiManager.OpenUI(mainMenuUI);
                Debug.Log("[导航示例] 打开主菜单 (Single策略)");
            }
        }
        
        /// <summary>
        /// 打开设置界面
        /// </summary>
        public void OpenSettings()
        {
            if (uiManager != null)
            {
                var context = new UINavigationManager.NavigationContext
                {
                    fromUI = mainMenuUI,
                    reason = "用户点击设置按钮",
                    autoReturnToSource = false
                };
                
                // 这里需要在实际的OpenUI调用中传递context
                // 由于当前API限制，我们在UI打开后手动记录导航
                uiManager.OpenUI(settingsUI);
                Debug.Log("[导航示例] 打开设置界面 (Single策略)");
            }
        }
        
        /// <summary>
        /// 回到主菜单
        /// </summary>
        public void BackToMainMenu()
        {
            if (inputHandler != null)
            {
                inputHandler.BackToHome();
                Debug.Log("[导航示例] 回到主菜单");
            }
        }
        
        /// <summary>
        /// 清空所有UI并回到主菜单
        /// </summary>
        public void ClearAllAndBackToHome()
        {
            if (inputHandler != null)
            {
                inputHandler.ClearAllAndBackToHome();
                Debug.Log("[导航示例] 清空所有UI并回到主菜单");
            }
        }
        
        #endregion
        
        #region 策略测试方法
        
        /// <summary>
        /// 测试Single策略
        /// </summary>
        private void TestSingleStrategy()
        {
            Debug.Log("[导航示例] 测试Single策略");
            
            // Single策略：同一时间只能有一个实例
            uiManager?.OpenUI(settingsUI);
            
            // 再次打开应该替换之前的实例
            uiManager?.OpenUI(settingsUI);
        }
        
        /// <summary>
        /// 测试Stack策略
        /// </summary>
        private void TestStackStrategy()
        {
            Debug.Log("[导航示例] 测试Stack策略");
            
            // Stack策略：按栈的方式管理，支持回退
            uiManager?.OpenUI(inventoryUI);
            
            // 延迟打开第二个，模拟栈式导航
            StartCoroutine(DelayedOpenUI(shopUI, 1f));
        }
        
        /// <summary>
        /// 测试Multiple策略
        /// </summary>
        private void TestMultipleStrategy()
        {
            Debug.Log("[导航示例] 测试Multiple策略");
            
            // Multiple策略：允许多个实例同时存在
            uiManager?.OpenUI(dialogUI);
            uiManager?.OpenUI(dialogUI);
            uiManager?.OpenUI(dialogUI);
        }
        
        /// <summary>
        /// 测试Limited策略
        /// </summary>
        private void TestLimitedStrategy()
        {
            Debug.Log("[导航示例] 测试Limited策略");
            
            // Limited策略：限制实例数量
            for (int i = 0; i < 5; i++)
            {
                uiManager?.OpenUI(loadingUI);
            }
        }
        
        /// <summary>
        /// 测试Queue策略
        /// </summary>
        private void TestQueueStrategy()
        {
            Debug.Log("[导航示例] 测试Queue策略");
            
            // Queue策略：排队显示
            uiManager?.OpenUIAsync(dialogUI);
            uiManager?.OpenUIAsync(dialogUI);
            uiManager?.OpenUIAsync(dialogUI);
        }
        
        /// <summary>
        /// 延迟打开UI
        /// </summary>
        private System.Collections.IEnumerator DelayedOpenUI(string uiName, float delay)
        {
            yield return new WaitForSeconds(delay);
            uiManager?.OpenUI(uiName);
        }
        
        #endregion
        
        #region 高级导航操作
        
        /// <summary>
        /// 演示复杂的导航场景
        /// </summary>
        public void DemoComplexNavigation()
        {
            StartCoroutine(ComplexNavigationSequence());
        }
        
        /// <summary>
        /// 复杂导航序列
        /// </summary>
        private System.Collections.IEnumerator ComplexNavigationSequence()
        {
            Debug.Log("[导航示例] 开始复杂导航演示");
            
            // 1. 打开主菜单 (Single)
            uiManager?.OpenUI(mainMenuUI);
            yield return new WaitForSeconds(1f);
            
            // 2. 打开背包 (Stack)
            uiManager?.OpenUI(inventoryUI);
            yield return new WaitForSeconds(1f);
            
            // 3. 打开商店 (Stack)
            uiManager?.OpenUI(shopUI);
            yield return new WaitForSeconds(1f);
            
            // 4. 打开设置 (Single，会替换主菜单)
            uiManager?.OpenUI(settingsUI);
            yield return new WaitForSeconds(1f);
            
            // 5. 打开多个对话框 (Multiple)
            uiManager?.OpenUI(dialogUI);
            uiManager?.OpenUI(dialogUI);
            yield return new WaitForSeconds(2f);
            
            // 6. 显示导航历史
            if (navigationManager != null)
            {
                string history = navigationManager.GetNavigationHistory();
                Debug.Log($"[导航示例] 当前导航历史: {history}");
            }
            
            // 7. 执行回退操作
            yield return new WaitForSeconds(1f);
            navigationManager?.NavigateBack();
            
            yield return new WaitForSeconds(1f);
            navigationManager?.NavigateBack();
            
            // 8. 回到主菜单
            yield return new WaitForSeconds(1f);
            inputHandler?.BackToHome();
            
            Debug.Log("[导航示例] 复杂导航演示完成");
        }
        
        #endregion
        
        #region GUI调试界面
        
        private void OnGUI()
        {
            if (!enableTestButtons)
                return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("UI导航系统测试", GUI.skin.box);
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("打开主菜单"))
            {
                OpenMainMenu();
            }
            
            if (GUILayout.Button("打开设置"))
            {
                OpenSettings();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("测试Single策略 (1)"))
            {
                TestSingleStrategy();
            }
            
            if (GUILayout.Button("测试Stack策略 (2)"))
            {
                TestStackStrategy();
            }
            
            if (GUILayout.Button("测试Multiple策略 (3)"))
            {
                TestMultipleStrategy();
            }
            
            if (GUILayout.Button("测试Limited策略 (4)"))
            {
                TestLimitedStrategy();
            }
            
            if (GUILayout.Button("测试Queue策略 (5)"))
            {
                TestQueueStrategy();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("回退"))
            {
                navigationManager?.NavigateBack();
            }
            
            if (GUILayout.Button("回到主菜单"))
            {
                BackToMainMenu();
            }
            
            if (GUILayout.Button("清空所有UI"))
            {
                ClearAllAndBackToHome();
            }
            
            if (GUILayout.Button("复杂导航演示"))
            {
                DemoComplexNavigation();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("显示导航历史"))
            {
                if (navigationManager != null)
                {
                    string history = navigationManager.GetNavigationHistory();
                    Debug.Log($"导航历史: {history}");
                }
            }
            
            // 显示当前状态
            GUILayout.Space(10);
            GUILayout.Label("当前状态:", GUI.skin.box);
            
            if (navigationManager != null)
            {
                GUILayout.Label($"栈大小: {navigationManager.StackSize}");
                GUILayout.Label($"可回退: {navigationManager.CanNavigateBack}");
                GUILayout.Label($"顶层UI: {navigationManager.GetTopUI() ?? "无"}");
            }
            
            GUILayout.EndArea();
        }
        
        #endregion
    }
}