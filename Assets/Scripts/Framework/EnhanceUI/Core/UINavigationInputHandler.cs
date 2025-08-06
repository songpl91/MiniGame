using System;
using UnityEngine;
// using Framework.Input;

namespace Framework.EnhanceUI.Core
{
    /// <summary>
    /// UI导航输入处理器
    /// 处理UI回退相关的输入操作
    /// </summary>
    public class UINavigationInputHandler : MonoBehaviour
    {
        #region 配置字段
        
        [Header("输入配置")]
        [SerializeField] private KeyCode backKey = KeyCode.Escape;
        [SerializeField] private bool enableAndroidBackButton = true;
        [SerializeField] private bool enableDoubleClickBack = false;
        [SerializeField] private float doubleClickInterval = 0.5f;
        
        [Header("回退行为配置")]
        [SerializeField] private bool confirmBeforeExit = true;
        [SerializeField] private string exitConfirmUIName = "ExitConfirmDialog";
        [SerializeField] private bool enableBackToSpecificUI = true;
        [SerializeField] private string homeUIName = "MainMenu";
        
        [Header("调试配置")]
        [SerializeField] private bool enableDebugLog = false;
        [SerializeField] private bool showNavigationHistory = false;
        
        #endregion
        
        #region 私有字段
        
        private UINavigationManager navigationManager;
        private EnhanceUIManager uiManager;
        private float lastBackClickTime;
        private bool isProcessingBack;
        
        #endregion
        
        #region 事件委托
        
        /// <summary>
        /// 回退输入事件
        /// </summary>
        public event Action<BackInputType> OnBackInputReceived;
        
        /// <summary>
        /// 回退操作完成事件
        /// </summary>
        public event Action<bool> OnBackOperationCompleted;
        
        /// <summary>
        /// 退出应用请求事件
        /// </summary>
        public event Action OnExitApplicationRequested;
        
        #endregion
        
        #region 枚举定义
        
        /// <summary>
        /// 回退输入类型
        /// </summary>
        public enum BackInputType
        {
            /// <summary>
            /// 键盘回退键
            /// </summary>
            KeyboardBack,
            
            /// <summary>
            /// Android返回键
            /// </summary>
            AndroidBack,
            
            /// <summary>
            /// 双击回退
            /// </summary>
            DoubleClickBack,
            
            /// <summary>
            /// 程序调用
            /// </summary>
            ProgrammaticCall
        }
        
        #endregion
        
        #region 初始化方法
        
        /// <summary>
        /// 初始化输入处理器
        /// </summary>
        /// <param name="navManager">导航管理器</param>
        /// <param name="uiMgr">UI管理器</param>
        public void Initialize(UINavigationManager navManager, EnhanceUIManager uiMgr)
        {
            navigationManager = navManager ?? throw new ArgumentNullException(nameof(navManager));
            uiManager = uiMgr ?? throw new ArgumentNullException(nameof(uiMgr));
            
            LogDebug("UI导航输入处理器初始化完成");
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 手动触发回退操作
        /// </summary>
        /// <returns>是否成功处理</returns>
        public bool TriggerBack()
        {
            return ProcessBackInput(BackInputType.ProgrammaticCall);
        }
        
        /// <summary>
        /// 回退到主界面
        /// </summary>
        /// <returns>是否成功</returns>
        public bool BackToHome()
        {
            if (string.IsNullOrEmpty(homeUIName))
            {
                LogDebug("未配置主界面UI名称");
                return false;
            }
            
            if (navigationManager == null)
                return false;
            
            bool success = navigationManager.NavigateBackTo(homeUIName);
            
            if (!success)
            {
                // 如果回退失败，尝试直接打开主界面
                if (uiManager != null)
                {
                    uiManager.OpenUI(homeUIName);
                    success = true;
                    LogDebug($"直接打开主界面：{homeUIName}");
                }
            }
            
            OnBackOperationCompleted?.Invoke(success);
            return success;
        }
        
        /// <summary>
        /// 清空所有UI并回到主界面
        /// </summary>
        /// <returns>是否成功</returns>
        public bool ClearAllAndBackToHome()
        {
            if (navigationManager == null)
                return false;
            
            int clearedCount = navigationManager.ClearAllBackableUI();
            LogDebug($"清空了 {clearedCount} 个UI");
            
            return BackToHome();
        }
        
        /// <summary>
        /// 显示导航历史（调试用）
        /// </summary>
        public void ShowNavigationHistory()
        {
            if (navigationManager != null && showNavigationHistory)
            {
                string history = navigationManager.GetNavigationHistory();
                Debug.Log($"[UI导航历史] {history}");
            }
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 处理回退输入
        /// </summary>
        /// <param name="inputType">输入类型</param>
        /// <returns>是否成功处理</returns>
        private bool ProcessBackInput(BackInputType inputType)
        {
            if (isProcessingBack)
            {
                LogDebug("正在处理回退操作，忽略新的输入");
                return false;
            }
            
            if (navigationManager == null)
            {
                LogDebug("导航管理器未初始化");
                return false;
            }
            
            isProcessingBack = true;
            
            try
            {
                // 触发回退输入事件
                OnBackInputReceived?.Invoke(inputType);
                
                // 检查是否有可回退的UI
                if (!navigationManager.CanNavigateBack)
                {
                    LogDebug("没有可回退的UI，处理退出逻辑");
                    return HandleExitLogic();
                }
                
                // 执行回退操作
                bool success = navigationManager.NavigateBack();
                
                LogDebug($"回退操作结果：{(success ? "成功" : "失败")} (输入类型: {inputType})");
                
                // 触发回退完成事件
                OnBackOperationCompleted?.Invoke(success);
                
                return success;
            }
            finally
            {
                isProcessingBack = false;
            }
        }
        
        /// <summary>
        /// 处理退出逻辑
        /// </summary>
        /// <returns>是否处理了退出</returns>
        private bool HandleExitLogic()
        {
            // 如果启用了回退到特定UI
            if (enableBackToSpecificUI && !string.IsNullOrEmpty(homeUIName))
            {
                string currentTopUI = navigationManager.GetTopUI();
                
                // 如果当前不在主界面，回到主界面
                if (currentTopUI != homeUIName)
                {
                    return BackToHome();
                }
            }
            
            // 如果需要确认退出
            if (confirmBeforeExit && !string.IsNullOrEmpty(exitConfirmUIName))
            {
                if (uiManager != null)
                {
                    uiManager.OpenUI(exitConfirmUIName);
                    LogDebug($"显示退出确认对话框：{exitConfirmUIName}");
                    return true;
                }
            }
            
            // 直接退出应用
            LogDebug("请求退出应用");
            OnExitApplicationRequested?.Invoke();
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            
            return true;
        }
        
        /// <summary>
        /// 检查双击回退
        /// </summary>
        /// <returns>是否为双击</returns>
        private bool CheckDoubleClick()
        {
            float currentTime = Time.time;
            bool isDoubleClick = (currentTime - lastBackClickTime) <= doubleClickInterval;
            lastBackClickTime = currentTime;
            return isDoubleClick;
        }
        
        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="message">日志消息</param>
        private void LogDebug(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[UI导航输入] {message}");
            }
        }
        
        #endregion
        
        #region Unity生命周期
        
        private void Update()
        {
            // 处理键盘回退键
            if (Input.GetKeyDown(backKey))
            {
                if (enableDoubleClickBack)
                {
                    if (CheckDoubleClick())
                    {
                        ProcessBackInput(BackInputType.DoubleClickBack);
                    }
                }
                else
                {
                    ProcessBackInput(BackInputType.KeyboardBack);
                }
            }
            
            // 处理Android返回键
            if (enableAndroidBackButton && Application.platform == RuntimePlatform.Android)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ProcessBackInput(BackInputType.AndroidBack);
                }
            }
        }
        
        #endregion
    }
}