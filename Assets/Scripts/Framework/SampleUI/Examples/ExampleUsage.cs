using UnityEngine;
using Framework.SampleUI.Core;
using Framework.SampleUI.Config;

namespace Framework.SampleUI.Examples
{
    /// <summary>
    /// SampleUI框架使用示例
    /// 展示如何在项目中使用SampleUI框架
    /// </summary>
    public class ExampleUsage : MonoBehaviour
    {
        #region 字段
        
        [Header("配置")]
        [SerializeField] private SampleUIConfig uiConfig;
        
        [Header("测试按钮")]
        [SerializeField] private bool showExamplePanel = false;
        [SerializeField] private bool hideExamplePanel = false;
        [SerializeField] private bool showPopupPanel = false;
        [SerializeField] private bool hideAllPanels = false;
        [SerializeField] private bool goBackPanel = false;
        [SerializeField] private bool demoAnimations = false;
        [SerializeField] private bool demoSounds = false;
        
        private ExamplePanel examplePanel;
        
        #endregion
        
        #region Unity生命周期
        
        private void Start()
        {
            // 初始化UI管理器
            InitializeUIManager();
            
            // 创建示例面板
            CreateExamplePanels();
        }
        
        private void Update()
        {
            // 处理测试按钮（在Inspector中设置）
            HandleTestButtons();
            
            // 处理键盘快捷键
            HandleKeyboardShortcuts();
        }
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化UI管理器
        /// </summary>
        private void InitializeUIManager()
        {
            // 如果没有配置文件，创建默认配置
            if (uiConfig == null)
            {
                uiConfig = SampleUIConfig.CreateDefaultConfig();
                Debug.Log("[ExampleUsage] 使用默认UI配置");
            }
            
            // 初始化UI管理器
            var uiManager = SampleUIManager.Instance;
            if (uiManager != null)
            {
                Debug.Log("[ExampleUsage] UI管理器初始化成功");
            }
            else
            {
                Debug.LogError("[ExampleUsage] UI管理器初始化失败");
            }
        }
        
        /// <summary>
        /// 创建示例面板
        /// </summary>
        private void CreateExamplePanels()
        {
            // 这里可以预创建一些面板
            // 实际项目中通常通过预制体加载
            
            Debug.Log("[ExampleUsage] 示例面板创建完成");
        }
        
        #endregion
        
        #region 测试方法
        
        /// <summary>
        /// 处理测试按钮
        /// </summary>
        private void HandleTestButtons()
        {
            if (showExamplePanel)
            {
                showExamplePanel = false;
                ShowExamplePanel();
            }
            
            if (hideExamplePanel)
            {
                hideExamplePanel = false;
                HideExamplePanel();
            }
            
            if (showPopupPanel)
            {
                showPopupPanel = false;
                ShowPopupPanel();
            }
            
            if (hideAllPanels)
            {
                hideAllPanels = false;
                HideAllPanels();
            }
            
            if (goBackPanel)
            {
                goBackPanel = false;
                GoBackPanel();
            }
            
            if (demoAnimations)
            {
                demoAnimations = false;
                DemoAnimations();
            }
            
            if (demoSounds)
            {
                demoSounds = false;
                DemoSounds();
            }
        }
        
        /// <summary>
        /// 处理键盘快捷键
        /// </summary>
        private void HandleKeyboardShortcuts()
        {
            // F1 - 显示示例面板
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ShowExamplePanel();
            }
            
            // F2 - 显示弹窗面板
            if (Input.GetKeyDown(KeyCode.F2))
            {
                ShowPopupPanel();
            }
            
            // F3 - 隐藏所有面板
            if (Input.GetKeyDown(KeyCode.F3))
            {
                HideAllPanels();
            }
            
            // F4 - 返回上一个面板
            if (Input.GetKeyDown(KeyCode.F4))
            {
                GoBackPanel();
            }
            
            // F5 - 演示动画
            if (Input.GetKeyDown(KeyCode.F5))
            {
                DemoAnimations();
            }
            
            // F6 - 演示音效
            if (Input.GetKeyDown(KeyCode.F6))
            {
                DemoSounds();
            }
        }
        
        #endregion
        
        #region UI操作方法
        
        /// <summary>
        /// 显示示例面板
        /// </summary>
        public void ShowExamplePanel()
        {
            Debug.Log("[ExampleUsage] 显示示例面板");
            
            // 准备要传递的数据
            var panelData = new System.Collections.Generic.Dictionary<string, object>
            {
                { "title", "数据驱动的示例面板" },
                { "content", "这个面板通过传递数据进行初始化！\n\n传递的数据包括：\n- 自定义标题\n- 动态内容\n- 音量设置\n- 动画效果" },
                { "volume", 0.8f },
                { "animation", "FadeIn" }
            };
            
            // 方法1：通过类型显示并传递数据
            examplePanel = SampleUIManager.Instance.ShowPanel<ExamplePanel>(panelData);
            
            // 方法2：通过ID显示并传递数据
            // SampleUIManager.Instance.ShowPanel("ExamplePanel", panelData);
            
            if (examplePanel != null)
            {
                Debug.Log("[ExampleUsage] 示例面板显示成功，已传递数据");
            }
            else
            {
                Debug.LogWarning("[ExampleUsage] 示例面板显示失败");
            }
        }
        
        /// <summary>
        /// 显示带简单数据的示例面板
        /// </summary>
        public void ShowExamplePanelWithSimpleData()
        {
            Debug.Log("[ExampleUsage] 显示带简单数据的示例面板");
            
            // 传递简单的字符串数据
            string message = "Hello, SampleUI Framework!";
            examplePanel = SampleUIManager.Instance.ShowPanel<ExamplePanel>(message);
        }
        
        /// <summary>
        /// 显示带数字数据的示例面板
        /// </summary>
        public void ShowExamplePanelWithNumber()
        {
            Debug.Log("[ExampleUsage] 显示带数字数据的示例面板");
            
            // 传递数字数据（面板编号）
            int panelNumber = UnityEngine.Random.Range(1, 100);
            examplePanel = SampleUIManager.Instance.ShowPanel<ExamplePanel>(panelNumber);
        }
        
        /// <summary>
        /// 隐藏示例面板
        /// </summary>
        public void HideExamplePanel()
        {
            Debug.Log("[ExampleUsage] 隐藏示例面板");
            
            if (examplePanel != null)
            {
                examplePanel.Hide();
            }
            else
            {
                // 通过管理器隐藏
                SampleUIManager.Instance.HidePanel("ExamplePanel");
            }
        }
        
        /// <summary>
        /// 显示弹窗面板
        /// </summary>
        public void ShowPopupPanel()
        {
            Debug.Log("[ExampleUsage] 显示弹窗面板");
            
            // 创建一个简单的弹窗面板
            var popupPanel = CreateSimplePopupPanel();
            if (popupPanel != null)
            {
                SampleUIManager.Instance.ShowPanel(popupPanel);
            }
        }
        
        /// <summary>
        /// 隐藏所有面板
        /// </summary>
        public void HideAllPanels()
        {
            Debug.Log("[ExampleUsage] 隐藏所有面板");
            
            SampleUIManager.Instance.HideAllPanels();
        }
        
        /// <summary>
        /// 返回上一个面板
        /// </summary>
        public void GoBackPanel()
        {
            Debug.Log("[ExampleUsage] 返回上一个面板");
            
            SampleUIManager.Instance.GoBack();
        }
        
        /// <summary>
        /// 演示动画效果
        /// </summary>
        public void DemoAnimations()
        {
            Debug.Log("[ExampleUsage] 演示动画效果");
            
            if (examplePanel != null)
            {
                examplePanel.DemoAllAnimations();
            }
            else
            {
                Debug.LogWarning("[ExampleUsage] 请先显示示例面板");
            }
        }
        
        /// <summary>
        /// 演示音效
        /// </summary>
        public void DemoSounds()
        {
            Debug.Log("[ExampleUsage] 演示音效");
            
            if (examplePanel != null)
            {
                examplePanel.DemoAllSounds();
            }
            else
            {
                Debug.LogWarning("[ExampleUsage] 请先显示示例面板");
            }
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 创建简单的弹窗面板
        /// </summary>
        /// <returns>弹窗面板</returns>
        private SampleUIBase CreateSimplePopupPanel()
        {
            // 创建一个简单的GameObject作为弹窗
            var popupGO = new GameObject("SimplePopup");
            var popupPanel = popupGO.AddComponent<SimplePopupPanel>();
            
            // 设置面板属性
            popupPanel.PanelId = "SimplePopup";
            popupPanel.DisplayName = "简单弹窗";
            popupPanel.PanelType = SampleUIBaseType.Popup;
            popupPanel.Priority = 10;
            
            return popupPanel;
        }
        
        #endregion
        
        #region GUI调试
        
        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("SampleUI框架测试面板", GUI.skin.box);
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("显示示例面板 (F1)"))
            {
                ShowExamplePanel();
            }
            
            if (GUILayout.Button("隐藏示例面板"))
            {
                HideExamplePanel();
            }
            
            if (GUILayout.Button("显示弹窗面板 (F2)"))
            {
                ShowPopupPanel();
            }
            
            if (GUILayout.Button("隐藏所有面板 (F3)"))
            {
                HideAllPanels();
            }
            
            if (GUILayout.Button("返回上一个面板 (F4)"))
            {
                GoBackPanel();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("演示动画效果 (F5)"))
            {
                DemoAnimations();
            }
            
            if (GUILayout.Button("演示音效 (F6)"))
            {
                DemoSounds();
            }
            
            GUILayout.Space(10);
            
            // 显示当前面板信息
            var uiManager = SampleUIManager.Instance;
            if (uiManager != null)
            {
                GUILayout.Label($"当前面板数量: {uiManager.GetActivePanelCount()}");
                GUILayout.Label($"弹窗数量: {uiManager.GetPopupCount()}");
            }
            
            GUILayout.EndArea();
        }
        
        #endregion
    }
    
    /// <summary>
    /// 简单弹窗面板
    /// 用于演示弹窗功能
    /// </summary>
    public class SimplePopupPanel : SampleUIBase
    {
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            // 创建简单的UI
            CreateSimpleUI();
        }
        
        /// <summary>
        /// 创建简单的UI
        /// </summary>
        private void CreateSimpleUI()
        {
            // 添加Canvas组件
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            
            // 添加GraphicRaycaster
            gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // 创建背景
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(transform);
            
            var bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            var bgImage = bgGO.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0, 0, 0, 0.5f);
            
            // 创建内容面板
            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(transform);
            
            var contentRect = contentGO.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(300, 200);
            contentRect.anchoredPosition = Vector2.zero;
            
            var contentImage = contentGO.AddComponent<UnityEngine.UI.Image>();
            contentImage.color = Color.white;
            
            // 创建标题文本
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(contentGO.transform);
            
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.7f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = new Vector2(10, 0);
            titleRect.offsetMax = new Vector2(-10, -10);
            
            var titleText = titleGO.AddComponent<UnityEngine.UI.Text>();
            titleText.text = "简单弹窗";
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 18;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.black;
            
            // 创建关闭按钮
            var closeButtonGO = new GameObject("CloseButton");
            closeButtonGO.transform.SetParent(contentGO.transform);
            
            var closeButtonRect = closeButtonGO.AddComponent<RectTransform>();
            closeButtonRect.anchorMin = new Vector2(0.3f, 0.1f);
            closeButtonRect.anchorMax = new Vector2(0.7f, 0.4f);
            closeButtonRect.offsetMin = Vector2.zero;
            closeButtonRect.offsetMax = Vector2.zero;
            
            var closeButtonImage = closeButtonGO.AddComponent<UnityEngine.UI.Image>();
            closeButtonImage.color = new Color(0.8f, 0.8f, 0.8f);
            
            var closeButton = closeButtonGO.AddComponent<UnityEngine.UI.Button>();
            closeButton.onClick.AddListener(() => {
                Debug.Log("[SimplePopupPanel] 关闭按钮点击");
                Hide();
            });
            
            // 创建按钮文本
            var buttonTextGO = new GameObject("Text");
            buttonTextGO.transform.SetParent(closeButtonGO.transform);
            
            var buttonTextRect = buttonTextGO.AddComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            var buttonText = buttonTextGO.AddComponent<UnityEngine.UI.Text>();
            buttonText.text = "关闭";
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 14;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.black;
        }
        
        protected override void OnShow()
        {
            base.OnShow();
            Debug.Log("[SimplePopupPanel] 简单弹窗显示");
        }
        
        protected override void OnHide()
        {
            base.OnHide();
            Debug.Log("[SimplePopupPanel] 简单弹窗隐藏");
        }
    }
}