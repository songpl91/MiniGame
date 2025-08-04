using UnityEngine;
using UnityEngine.UI;
using Framework.EnhanceUI.Core;

namespace Framework.EnhanceUI.Examples
{
    /// <summary>
    /// 示例主菜单面板
    /// 演示如何使用EnhanceUI框架创建UI面板
    /// </summary>
    public class ExampleMainMenuPanel : EnhanceUIPanel
    {
        #region UI组件引用
        
        [Header("主菜单UI组件")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text versionText;
        
        #endregion
        
        #region 重写方法
        
        /// <summary>
        /// 初始化面板
        /// </summary>
        /// <param name="data">传递的数据</param>
        public override void Initialize(object data = null)
        {
            base.Initialize(data);
            
            // 设置标题
            if (titleText != null)
            {
                titleText.text = "主菜单";
            }
            
            // 设置版本信息
            if (versionText != null)
            {
                versionText.text = $"版本: {Application.version}";
            }
            
            Debug.Log("[ExampleMainMenuPanel] 主菜单面板初始化完成");
        }
        
        /// <summary>
        /// 设置按钮事件
        /// </summary>
        protected override void SetupButtons()
        {
            base.SetupButtons();
            
            // 开始游戏按钮
            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(OnStartGameClicked);
            }
            
            // 设置按钮
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }
            
            // 退出按钮
            if (exitButton != null)
            {
                exitButton.onClick.AddListener(OnExitClicked);
            }
        }
        
        /// <summary>
        /// 面板显示时调用
        /// </summary>
        protected override void OnShow()
        {
            base.OnShow();
            
            Debug.Log("[ExampleMainMenuPanel] 主菜单面板显示");
            
            // 可以在这里添加显示时的特殊逻辑
            // 例如：播放背景音乐、更新数据等
        }
        
        /// <summary>
        /// 面板隐藏时调用
        /// </summary>
        protected override void OnHide()
        {
            base.OnHide();
            
            Debug.Log("[ExampleMainMenuPanel] 主菜单面板隐藏");
            
            // 可以在这里添加隐藏时的特殊逻辑
        }
        
        /// <summary>
        /// 重置到对象池状态
        /// </summary>
        protected override void OnResetToPool()
        {
            base.OnResetToPool();
            
            // 重置UI状态
            if (titleText != null)
            {
                titleText.text = "";
            }
            
            if (versionText != null)
            {
                versionText.text = "";
            }
            
            Debug.Log("[ExampleMainMenuPanel] 主菜单面板重置到对象池");
        }
        
        #endregion
        
        #region 按钮事件处理
        
        /// <summary>
        /// 开始游戏按钮点击
        /// </summary>
        private void OnStartGameClicked()
        {
            Debug.Log("[ExampleMainMenuPanel] 开始游戏按钮被点击");
            
            // 打开游戏场景选择面板
            EnhanceUIManager.Instance.OpenUIAsync("GameSceneSelectPanel", (panel) =>
            {
                if (panel != null)
                {
                    Debug.Log("游戏场景选择面板打开成功");
                }
                else
                {
                    Debug.LogError("游戏场景选择面板打开失败");
                }
            });
            
            // 关闭当前面板
            EnhanceUIManager.Instance.CloseUI(this);
        }
        
        /// <summary>
        /// 设置按钮点击
        /// </summary>
        private void OnSettingsClicked()
        {
            Debug.Log("[ExampleMainMenuPanel] 设置按钮被点击");
            
            // 打开设置面板（弹窗模式）
            EnhanceUIManager.Instance.OpenUIAsync("SettingsPanel", (panel) =>
            {
                if (panel != null)
                {
                    Debug.Log("设置面板打开成功");
                }
                else
                {
                    Debug.LogError("设置面板打开失败");
                }
            });
        }
        
        /// <summary>
        /// 退出按钮点击
        /// </summary>
        private void OnExitClicked()
        {
            Debug.Log("[ExampleMainMenuPanel] 退出按钮被点击");
            
            // 显示确认对话框
            EnhanceUIManager.Instance.OpenUIAsync("ConfirmDialog", (panel) =>
            {
                if (panel != null)
                {
                    // 传递确认对话框的数据
                    var dialogData = new ConfirmDialogData
                    {
                        Title = "确认退出",
                        Message = "确定要退出游戏吗？",
                        OnConfirm = () =>
                        {
                            Debug.Log("用户确认退出游戏");
                            Application.Quit();
                        },
                        OnCancel = () =>
                        {
                            Debug.Log("用户取消退出");
                        }
                    };
                    
                    panel.Initialize(dialogData);
                }
            });
        }
        
        #endregion
    }
    
    /// <summary>
    /// 确认对话框数据结构
    /// </summary>
    public class ConfirmDialogData
    {
        /// <summary>
        /// 对话框标题
        /// </summary>
        public string Title;
        
        /// <summary>
        /// 对话框消息
        /// </summary>
        public string Message;
        
        /// <summary>
        /// 确认回调
        /// </summary>
        public System.Action OnConfirm;
        
        /// <summary>
        /// 取消回调
        /// </summary>
        public System.Action OnCancel;
    }
}