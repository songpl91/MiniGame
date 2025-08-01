using UnityEngine;
using UnityEngine.UI;
using Framework.TraditionalUI.Panels;

namespace Framework.TraditionalUI.Examples
{
    /// <summary>
    /// 传统UI框架使用示例
    /// 演示如何使用传统UI框架的各种功能
    /// </summary>
    public class TraditionalUIExample : MonoBehaviour
    {
        #region UI组件引用
        
        [Header("测试按钮")]
        [SerializeField] private Button openMainMenuButton;
        [SerializeField] private Button openSettingsButton;
        [SerializeField] private Button openShopButton;
        [SerializeField] private Button showMessageBoxButton;
        [SerializeField] private Button showInfoButton;
        [SerializeField] private Button showWarningButton;
        [SerializeField] private Button showErrorButton;
        [SerializeField] private Button showConfirmButton;
        
        [Header("音效测试")]
        [SerializeField] private Button testButtonClickButton;
        [SerializeField] private Button testSuccessButton;
        [SerializeField] private Button testErrorButton;
        [SerializeField] private Button testPurchaseButton;
        
        [Header("动画测试")]
        [SerializeField] private Button testFadeButton;
        [SerializeField] private Button testScaleButton;
        [SerializeField] private Button testSlideButton;
        [SerializeField] private Button testBounceButton;
        [SerializeField] private Button testShakeButton;
        
        [Header("信息显示")]
        [SerializeField] private Text infoText;
        [SerializeField] private Text statusText;
        
        #endregion
        
        #region 初始化方法
        
        private void Start()
        {
            InitializeButtons();
            UpdateStatusText();
        }
        
        /// <summary>
        /// 初始化按钮事件
        /// </summary>
        private void InitializeButtons()
        {
            // UI面板测试按钮
            if (openMainMenuButton != null)
                openMainMenuButton.onClick.AddListener(OpenMainMenu);
            
            if (openSettingsButton != null)
                openSettingsButton.onClick.AddListener(OpenSettings);
            
            if (openShopButton != null)
                openShopButton.onClick.AddListener(OpenShop);
            
            if (showMessageBoxButton != null)
                showMessageBoxButton.onClick.AddListener(ShowMessageBox);
            
            if (showInfoButton != null)
                showInfoButton.onClick.AddListener(ShowInfo);
            
            if (showWarningButton != null)
                showWarningButton.onClick.AddListener(ShowWarning);
            
            if (showErrorButton != null)
                showErrorButton.onClick.AddListener(ShowError);
            
            if (showConfirmButton != null)
                showConfirmButton.onClick.AddListener(ShowConfirm);
            
            // 音效测试按钮
            if (testButtonClickButton != null)
                testButtonClickButton.onClick.AddListener(TestButtonClick);
            
            if (testSuccessButton != null)
                testSuccessButton.onClick.AddListener(TestSuccess);
            
            if (testErrorButton != null)
                testErrorButton.onClick.AddListener(TestError);
            
            if (testPurchaseButton != null)
                testPurchaseButton.onClick.AddListener(TestPurchase);
            
            // 动画测试按钮
            if (testFadeButton != null)
                testFadeButton.onClick.AddListener(TestFade);
            
            if (testScaleButton != null)
                testScaleButton.onClick.AddListener(TestScale);
            
            if (testSlideButton != null)
                testSlideButton.onClick.AddListener(TestSlide);
            
            if (testBounceButton != null)
                testBounceButton.onClick.AddListener(TestBounce);
            
            if (testShakeButton != null)
                testShakeButton.onClick.AddListener(TestShake);
        }
        
        #endregion
        
        #region UI面板测试方法
        
        /// <summary>
        /// 打开主菜单
        /// </summary>
        private void OpenMainMenu()
        {
            Debug.Log("[示例] 打开主菜单");
            UpdateInfoText("打开主菜单面板");
            
            TraditionalUIManager.Instance.OpenPanel("MainMenu");
        }
        
        /// <summary>
        /// 打开设置
        /// </summary>
        private void OpenSettings()
        {
            Debug.Log("[示例] 打开设置");
            UpdateInfoText("打开设置面板");
            
            TraditionalUIManager.Instance.OpenPanel("Settings");
        }
        
        /// <summary>
        /// 打开商店
        /// </summary>
        private void OpenShop()
        {
            Debug.Log("[示例] 打开商店");
            UpdateInfoText("打开商店面板");
            
            TraditionalUIManager.Instance.OpenPanel("Shop");
        }
        
        /// <summary>
        /// 显示消息框
        /// </summary>
        private void ShowMessageBox()
        {
            Debug.Log("[示例] 显示消息框");
            UpdateInfoText("显示自定义消息框");
            
            // var messageData = new MessageBoxData
            // {
            //     title = "自定义消息框",
            //     message = "这是一个自定义的消息框示例，您可以自定义标题、内容和按钮。",
            //     messageType = MessageType.Info,
            //     showCancelButton = true,
            //     confirmButtonText = "好的",
            //     cancelButtonText = "取消",
            //     onConfirm = () => {
            //         Debug.Log("[示例] 用户点击了确定");
            //         UpdateInfoText("用户点击了确定按钮");
            //     },
            //     onCancel = () => {
            //         Debug.Log("[示例] 用户点击了取消");
            //         UpdateInfoText("用户点击了取消按钮");
            //     }
            // };
            //
            // TraditionalUIManager.Instance.OpenPanel("MessageBox", messageData);
        }
        
        /// <summary>
        /// 显示信息消息
        /// </summary>
        private void ShowInfo()
        {
            Debug.Log("[示例] 显示信息消息");
            UpdateInfoText("显示信息类型消息框");
            
            MessageBoxPanel.ShowInfo("信息", "这是一个信息类型的消息框。", () => {
                UpdateInfoText("信息消息框已关闭");
            });
        }
        
        /// <summary>
        /// 显示警告消息
        /// </summary>
        private void ShowWarning()
        {
            Debug.Log("[示例] 显示警告消息");
            UpdateInfoText("显示警告类型消息框");
            
            MessageBoxPanel.ShowWarning("警告", "这是一个警告类型的消息框，请注意！", () => {
                UpdateInfoText("警告消息框已关闭");
            });
        }
        
        /// <summary>
        /// 显示错误消息
        /// </summary>
        private void ShowError()
        {
            Debug.Log("[示例] 显示错误消息");
            UpdateInfoText("显示错误类型消息框");
            
            MessageBoxPanel.ShowError("错误", "这是一个错误类型的消息框，操作失败！", () => {
                UpdateInfoText("错误消息框已关闭");
            });
        }
        
        /// <summary>
        /// 显示确认消息
        /// </summary>
        private void ShowConfirm()
        {
            Debug.Log("[示例] 显示确认消息");
            UpdateInfoText("显示确认类型消息框");
            
            MessageBoxPanel.ShowConfirm("确认", "您确定要执行这个操作吗？", 
                () => {
                    Debug.Log("[示例] 用户确认操作");
                    UpdateInfoText("用户确认了操作");
                },
                () => {
                    Debug.Log("[示例] 用户取消操作");
                    UpdateInfoText("用户取消了操作");
                });
        }
        
        #endregion
        
        #region 音效测试方法
        
        /// <summary>
        /// 测试按钮点击音效
        /// </summary>
        private void TestButtonClick()
        {
            Debug.Log("[示例] 测试按钮点击音效");
            UpdateInfoText("播放按钮点击音效");
            
            Utils.UIAudioManager.Instance.PlayButtonClick();
        }
        
        /// <summary>
        /// 测试成功音效
        /// </summary>
        private void TestSuccess()
        {
            Debug.Log("[示例] 测试成功音效");
            UpdateInfoText("播放成功音效");
            
            Utils.UIAudioManager.Instance.PlaySuccess();
        }
        
        /// <summary>
        /// 测试错误音效
        /// </summary>
        private void TestError()
        {
            Debug.Log("[示例] 测试错误音效");
            UpdateInfoText("播放错误音效");
            
            Utils.UIAudioManager.Instance.PlayError();
        }
        
        /// <summary>
        /// 测试购买音效
        /// </summary>
        private void TestPurchase()
        {
            Debug.Log("[示例] 测试购买音效");
            UpdateInfoText("播放购买音效");
            
            Utils.UIAudioManager.Instance.PlayPurchase();
        }
        
        #endregion
        
        #region 动画测试方法
        
        /// <summary>
        /// 测试淡入淡出动画
        /// </summary>
        private void TestFade()
        {
            Debug.Log("[示例] 测试淡入淡出动画");
            UpdateInfoText("测试淡入淡出动画");
            
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
            Utils.UIAnimationHelper.FadeOut(canvasGroup, 0.5f, () => {
                Utils.UIAnimationHelper.FadeIn(canvasGroup, 0.5f, () => {
                    UpdateInfoText("淡入淡出动画完成");
                });
            });
        }
        
        /// <summary>
        /// 测试缩放动画
        /// </summary>
        private void TestScale()
        {
            Debug.Log("[示例] 测试缩放动画");
            UpdateInfoText("测试缩放动画");
            
            Utils.UIAnimationHelper.ScaleOut(transform, 0.3f, () => {
                Utils.UIAnimationHelper.ScaleIn(transform, 0.3f, () => {
                    UpdateInfoText("缩放动画完成");
                });
            });
        }
        
        /// <summary>
        /// 测试滑动动画
        /// </summary>
        private void TestSlide()
        {
            Debug.Log("[示例] 测试滑动动画");
            UpdateInfoText("测试滑动动画");
            
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Utils.UIAnimationHelper.SlideOutToLeft(rectTransform, 0.3f, () => {
                    Utils.UIAnimationHelper.SlideInFromRight(rectTransform, 0.3f, () => {
                        UpdateInfoText("滑动动画完成");
                    });
                });
            }
        }
        
        /// <summary>
        /// 测试弹跳动画
        /// </summary>
        private void TestBounce()
        {
            Debug.Log("[示例] 测试弹跳动画");
            UpdateInfoText("测试弹跳动画");
            
            Utils.UIAnimationHelper.Bounce(transform, 1.2f, 0.5f, () => {
                UpdateInfoText("弹跳动画完成");
            });
        }
        
        /// <summary>
        /// 测试摇摆动画
        /// </summary>
        private void TestShake()
        {
            Debug.Log("[示例] 测试摇摆动画");
            UpdateInfoText("测试摇摆动画");
            
            Utils.UIAnimationHelper.Shake(transform, 15f, 1.0f, () => {
                UpdateInfoText("摇摆动画完成");
            });
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 更新信息文本
        /// </summary>
        /// <param name="info">信息内容</param>
        private void UpdateInfoText(string info)
        {
            if (infoText != null)
            {
                infoText.text = $"[{System.DateTime.Now:HH:mm:ss}] {info}";
            }
        }
        
        /// <summary>
        /// 更新状态文本
        /// </summary>
        private void UpdateStatusText()
        {
            // if (statusText != null)
            // {
            //     var manager = TraditionalUIManager.Instance;
            //     int openPanelCount = manager.GetOpenPanelCount();
            //     int popupCount = manager.GetPopupCount();
            //     
            //     statusText.text = $"打开面板数: {openPanelCount} | 弹窗数: {popupCount}";
            // }
        }
        
        #endregion
        
        #region 生命周期方法
        
        private void Update()
        {
            // 定期更新状态文本
            if (Time.frameCount % 60 == 0) // 每秒更新一次
            {
                UpdateStatusText();
            }
            
            // 处理ESC键
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[示例] ESC键按下，尝试返回");
                UpdateInfoText("ESC键按下，尝试返回");
                
                // if (!TraditionalUIManager.Instance.GoBack())
                // {
                //     Debug.Log("[示例] 没有可返回的面板");
                //     UpdateInfoText("没有可返回的面板");
                // }
            }
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 清空所有面板
        /// </summary>
        public void ClearAllPanels()
        {
            Debug.Log("[示例] 清空所有面板");
            UpdateInfoText("清空所有面板");
            
            TraditionalUIManager.Instance.CloseAllPanels();
        }
        
        /// <summary>
        /// 显示框架信息
        /// </summary>
        public void ShowFrameworkInfo()
        {
            Debug.Log("[示例] 显示框架信息");
            
            string info = "传统UI框架特性：\n" +
                         "• 完整的面板生命周期管理\n" +
                         "• 多种动画效果支持\n" +
                         "• 音效系统集成\n" +
                         "• 层级管理和UI栈\n" +
                         "• 配置化面板管理\n" +
                         "• 丰富的消息框类型\n" +
                         "• 性能优化和缓存机制";
            
            MessageBoxPanel.ShowInfo("框架信息", info);
        }
        
        /// <summary>
        /// 测试所有功能
        /// </summary>
        public void TestAllFeatures()
        {
            Debug.Log("[示例] 开始测试所有功能");
            UpdateInfoText("开始自动测试所有功能");
            
            StartCoroutine(TestAllFeaturesCoroutine());
        }
        
        /// <summary>
        /// 测试所有功能的协程
        /// </summary>
        private System.Collections.IEnumerator TestAllFeaturesCoroutine()
        {
            // 测试面板打开
            OpenMainMenu();
            yield return new WaitForSeconds(1f);
            
            OpenSettings();
            yield return new WaitForSeconds(1f);
            
            // 测试音效
            TestButtonClick();
            yield return new WaitForSeconds(0.5f);
            
            TestSuccess();
            yield return new WaitForSeconds(0.5f);
            
            // 测试动画
            TestBounce();
            yield return new WaitForSeconds(1f);
            
            // 测试消息框
            ShowInfo();
            yield return new WaitForSeconds(2f);
            
            UpdateInfoText("自动测试完成");
        }
        
        #endregion
    }
}