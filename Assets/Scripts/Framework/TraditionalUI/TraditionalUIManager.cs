using UnityEngine;
using System.Collections.Generic;
using System;

namespace Framework.TraditionalUI
{
    /// <summary>
    /// 传统UI管理器
    /// 提供基础的UI面板管理功能，采用传统的栈式管理方式
    /// </summary>
    public class TraditionalUIManager : MonoBehaviour
    {
        #region 单例模式
        
        private static TraditionalUIManager _instance;
        
        /// <summary>
        /// 获取UI管理器实例
        /// </summary>
        public static TraditionalUIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TraditionalUIManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("TraditionalUIManager");
                        _instance = go.AddComponent<TraditionalUIManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region 字段和属性
        
        [Header("UI根节点")]
        public Transform uiRoot;
        public Transform normalLayer;    // 普通UI层
        public Transform popupLayer;     // 弹窗层
        public Transform systemLayer;    // 系统UI层
        public Transform topLayer;       // 顶层UI（如加载界面）
        
        [Header("UI配置")]
        public bool enableUISound = true;
        public bool enableUIAnimation = true;
        public float defaultAnimationDuration = 0.3f;
        
        // UI面板字典 - 存储所有已创建的UI面板
        private Dictionary<string, TraditionalUIPanel> allPanels = new Dictionary<string, TraditionalUIPanel>();
        
        // UI栈 - 管理当前显示的UI面板
        private Stack<TraditionalUIPanel> uiStack = new Stack<TraditionalUIPanel>();
        
        // 弹窗列表 - 管理当前显示的弹窗
        private List<TraditionalUIPanel> popupList = new List<TraditionalUIPanel>();
        
        // UI预制体路径配置
        private Dictionary<string, string> uiPrefabPaths = new Dictionary<string, string>();
        
        // 事件委托
        public Action<string> OnPanelOpened;
        public Action<string> OnPanelClosed;
        public Action<string> OnPanelDestroyed;
        
        #endregion
        
        #region Unity生命周期
        
        void Awake()
        {
            // 确保单例
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeUIManager();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        void Update()
        {
            HandleInput();
        }
        
        #endregion
        
        #region 初始化方法
        
        /// <summary>
        /// 初始化UI管理器
        /// </summary>
        private void InitializeUIManager()
        {
            // 如果没有指定UI根节点，自动创建
            if (uiRoot == null)
            {
                CreateUIRoot();
            }
            
            // 初始化UI预制体路径
            InitializeUIPrefabPaths();
            
            Debug.Log("[传统UI管理器] 初始化完成");
        }
        
        /// <summary>
        /// 创建UI根节点
        /// </summary>
        private void CreateUIRoot()
        {
            GameObject uiRootGO = new GameObject("UIRoot");
            uiRoot = uiRootGO.transform;
            uiRoot.SetParent(transform);
            
            // 添加Canvas组件
            Canvas canvas = uiRootGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            
            // 添加CanvasScaler组件
            var scaler = uiRootGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            // 添加GraphicRaycaster组件
            uiRootGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // 创建UI层级
            CreateUILayers();
        }
        
        /// <summary>
        /// 创建UI层级
        /// </summary>
        private void CreateUILayers()
        {
            normalLayer = CreateUILayer("NormalLayer", 0);
            popupLayer = CreateUILayer("PopupLayer", 100);
            systemLayer = CreateUILayer("SystemLayer", 200);
            topLayer = CreateUILayer("TopLayer", 300);
        }
        
        /// <summary>
        /// 创建UI层
        /// </summary>
        private Transform CreateUILayer(string layerName, int sortingOrder)
        {
            GameObject layerGO = new GameObject(layerName);
            layerGO.transform.SetParent(uiRoot);
            
            // 设置RectTransform
            RectTransform rectTransform = layerGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // 添加Canvas组件用于层级控制
            Canvas canvas = layerGO.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
            
            return layerGO.transform;
        }
        
        /// <summary>
        /// 初始化UI预制体路径配置
        /// </summary>
        private void InitializeUIPrefabPaths()
        {
            // 这里可以配置UI预制体的路径
            // 实际项目中可以从配置文件或ScriptableObject中读取
            uiPrefabPaths["MainMenu"] = "UI/Panels/MainMenuPanel";
            uiPrefabPaths["Settings"] = "UI/Panels/SettingsPanel";
            uiPrefabPaths["Inventory"] = "UI/Panels/InventoryPanel";
            uiPrefabPaths["Shop"] = "UI/Panels/ShopPanel";
            uiPrefabPaths["MessageBox"] = "UI/Popups/MessageBoxPopup";
            uiPrefabPaths["Loading"] = "UI/System/LoadingPanel";
        }
        
        #endregion
        
        #region 核心UI管理方法
        
        /// <summary>
        /// 打开UI面板
        /// </summary>
        /// <param name="panelName">面板名称</param>
        /// <param name="data">传递给面板的数据</param>
        /// <returns>打开的UI面板实例</returns>
        public TraditionalUIPanel OpenPanel(string panelName, object data = null)
        {
            if (string.IsNullOrEmpty(panelName))
            {
                Debug.LogError("[传统UI管理器] 面板名称不能为空");
                return null;
            }
            
            TraditionalUIPanel panel = GetOrCreatePanel(panelName);
            if (panel == null)
            {
                Debug.LogError($"[传统UI管理器] 无法创建面板: {panelName}");
                return null;
            }
            
            // 根据面板类型处理显示逻辑
            switch (panel.PanelType)
            {
                case UIPanelType.Normal:
                    OpenNormalPanel(panel, data);
                    break;
                case UIPanelType.Popup:
                    OpenPopupPanel(panel, data);
                    break;
                case UIPanelType.System:
                    OpenSystemPanel(panel, data);
                    break;
            }
            
            // 触发事件
            OnPanelOpened?.Invoke(panelName);
            
            Debug.Log($"[传统UI管理器] 打开面板: {panelName}");
            return panel;
        }
        
        /// <summary>
        /// 关闭UI面板
        /// </summary>
        /// <param name="panelName">面板名称</param>
        public void ClosePanel(string panelName)
        {
            if (string.IsNullOrEmpty(panelName))
            {
                Debug.LogError("[传统UI管理器] 面板名称不能为空");
                return;
            }
            
            if (!allPanels.ContainsKey(panelName))
            {
                Debug.LogWarning($"[传统UI管理器] 面板不存在: {panelName}");
                return;
            }
            
            TraditionalUIPanel panel = allPanels[panelName];
            ClosePanelInternal(panel);
            
            // 触发事件
            OnPanelClosed?.Invoke(panelName);
            
            Debug.Log($"[传统UI管理器] 关闭面板: {panelName}");
        }
        
        /// <summary>
        /// 关闭当前顶层面板
        /// </summary>
        public void CloseTopPanel()
        {
            if (uiStack.Count > 0)
            {
                TraditionalUIPanel topPanel = uiStack.Peek();
                ClosePanel(topPanel.PanelName);
            }
        }
        
        /// <summary>
        /// 关闭所有弹窗
        /// </summary>
        public void CloseAllPopups()
        {
            for (int i = popupList.Count - 1; i >= 0; i--)
            {
                ClosePanelInternal(popupList[i]);
            }
            popupList.Clear();
        }
        
        /// <summary>
        /// 关闭所有面板
        /// </summary>
        public void CloseAllPanels()
        {
            // 关闭所有弹窗
            CloseAllPopups();
            
            // 关闭栈中的所有面板
            while (uiStack.Count > 0)
            {
                TraditionalUIPanel panel = uiStack.Pop();
                panel.Hide();
            }
        }
        
        /// <summary>
        /// 销毁UI面板
        /// </summary>
        /// <param name="panelName">面板名称</param>
        public void DestroyPanel(string panelName)
        {
            if (allPanels.ContainsKey(panelName))
            {
                TraditionalUIPanel panel = allPanels[panelName];
                
                // 先关闭面板
                ClosePanelInternal(panel);
                
                // 从字典中移除
                allPanels.Remove(panelName);
                
                // 销毁GameObject
                if (panel != null && panel.gameObject != null)
                {
                    Destroy(panel.gameObject);
                }
                
                // 触发事件
                OnPanelDestroyed?.Invoke(panelName);
                
                Debug.Log($"[传统UI管理器] 销毁面板: {panelName}");
            }
        }
        
        #endregion
        
        #region 内部方法
        
        /// <summary>
        /// 获取或创建UI面板
        /// </summary>
        private TraditionalUIPanel GetOrCreatePanel(string panelName)
        {
            // 如果面板已存在，直接返回
            if (allPanels.ContainsKey(panelName))
            {
                return allPanels[panelName];
            }
            
            // 创建新面板
            return CreatePanel(panelName);
        }
        
        /// <summary>
        /// 创建UI面板
        /// </summary>
        private TraditionalUIPanel CreatePanel(string panelName)
        {
            // 获取预制体路径
            if (!uiPrefabPaths.ContainsKey(panelName))
            {
                Debug.LogError($"[传统UI管理器] 未配置面板预制体路径: {panelName}");
                return null;
            }
            
            string prefabPath = uiPrefabPaths[panelName];
            
            // 加载预制体
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[传统UI管理器] 无法加载面板预制体: {prefabPath}");
                return null;
            }
            
            // 实例化面板
            GameObject panelGO = Instantiate(prefab);
            TraditionalUIPanel panel = panelGO.GetComponent<TraditionalUIPanel>();
            
            if (panel == null)
            {
                Debug.LogError($"[传统UI管理器] 面板预制体缺少TraditionalUIPanel组件: {panelName}");
                Destroy(panelGO);
                return null;
            }
            
            // 设置面板属性
            panel.PanelName = panelName;
            panel.Initialize();
            
            // 设置父节点
            Transform parentLayer = GetPanelLayer(panel.PanelType);
            panelGO.transform.SetParent(parentLayer, false);
            
            // 添加到字典
            allPanels[panelName] = panel;
            
            return panel;
        }
        
        /// <summary>
        /// 获取面板对应的层级
        /// </summary>
        private Transform GetPanelLayer(UIPanelType panelType)
        {
            switch (panelType)
            {
                case UIPanelType.Normal:
                    return normalLayer;
                case UIPanelType.Popup:
                    return popupLayer;
                case UIPanelType.System:
                    return systemLayer;
                case UIPanelType.Top:
                    return topLayer;
                default:
                    return normalLayer;
            }
        }
        
        /// <summary>
        /// 打开普通面板
        /// </summary>
        private void OpenNormalPanel(TraditionalUIPanel panel, object data)
        {
            // 隐藏当前顶层面板
            if (uiStack.Count > 0)
            {
                TraditionalUIPanel currentTop = uiStack.Peek();
                currentTop.Hide();
            }
            
            // 将新面板压入栈
            uiStack.Push(panel);
            
            // 显示面板
            panel.Show(data);
        }
        
        /// <summary>
        /// 打开弹窗面板
        /// </summary>
        private void OpenPopupPanel(TraditionalUIPanel panel, object data)
        {
            // 添加到弹窗列表
            if (!popupList.Contains(panel))
            {
                popupList.Add(panel);
            }
            
            // 显示面板
            panel.Show(data);
        }
        
        /// <summary>
        /// 打开系统面板
        /// </summary>
        private void OpenSystemPanel(TraditionalUIPanel panel, object data)
        {
            // 系统面板直接显示，不影响其他面板
            panel.Show(data);
        }
        
        /// <summary>
        /// 内部关闭面板方法
        /// </summary>
        private void ClosePanelInternal(TraditionalUIPanel panel)
        {
            if (panel == null) return;
            
            // 隐藏面板
            panel.Hide();
            
            // 根据面板类型处理关闭逻辑
            switch (panel.PanelType)
            {
                case UIPanelType.Normal:
                    CloseNormalPanel(panel);
                    break;
                case UIPanelType.Popup:
                    ClosePopupPanel(panel);
                    break;
                case UIPanelType.System:
                    // 系统面板直接关闭，无需特殊处理
                    break;
            }
        }
        
        /// <summary>
        /// 关闭普通面板
        /// </summary>
        private void CloseNormalPanel(TraditionalUIPanel panel)
        {
            // 从栈中移除
            if (uiStack.Count > 0 && uiStack.Peek() == panel)
            {
                uiStack.Pop();
                
                // 显示下一个面板
                if (uiStack.Count > 0)
                {
                    TraditionalUIPanel nextPanel = uiStack.Peek();
                    nextPanel.Show();
                }
            }
        }
        
        /// <summary>
        /// 关闭弹窗面板
        /// </summary>
        private void ClosePopupPanel(TraditionalUIPanel panel)
        {
            // 从弹窗列表中移除
            popupList.Remove(panel);
        }
        
        #endregion
        
        #region 输入处理
        
        /// <summary>
        /// 处理输入
        /// </summary>
        private void HandleInput()
        {
            // ESC键返回
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleBackInput();
            }
        }
        
        /// <summary>
        /// 处理返回输入
        /// </summary>
        private void HandleBackInput()
        {
            // 优先关闭弹窗
            if (popupList.Count > 0)
            {
                TraditionalUIPanel topPopup = popupList[popupList.Count - 1];
                ClosePanelInternal(topPopup);
                return;
            }
            
            // 关闭顶层面板
            if (uiStack.Count > 1) // 保留至少一个面板
            {
                CloseTopPanel();
            }
        }
        
        #endregion
        
        #region 查询方法
        
        /// <summary>
        /// 检查面板是否存在
        /// </summary>
        public bool HasPanel(string panelName)
        {
            return allPanels.ContainsKey(panelName);
        }
        
        /// <summary>
        /// 检查面板是否正在显示
        /// </summary>
        public bool IsPanelShowing(string panelName)
        {
            if (!allPanels.ContainsKey(panelName))
                return false;
            
            return allPanels[panelName].IsShowing;
        }
        
        /// <summary>
        /// 获取当前顶层面板
        /// </summary>
        public TraditionalUIPanel GetTopPanel()
        {
            return uiStack.Count > 0 ? uiStack.Peek() : null;
        }
        
        /// <summary>
        /// 获取UI栈信息
        /// </summary>
        public string GetUIStackInfo()
        {
            if (uiStack.Count == 0)
                return "UI栈为空";
            
            string info = "UI栈信息:\n";
            TraditionalUIPanel[] panels = uiStack.ToArray();
            
            for (int i = panels.Length - 1; i >= 0; i--)
            {
                info += $"{panels.Length - i}. {panels[i].PanelName}\n";
            }
            
            return info;
        }
        
        #endregion
        
        #region 配置方法
        
        /// <summary>
        /// 注册UI预制体路径
        /// </summary>
        public void RegisterUIPrefabPath(string panelName, string prefabPath)
        {
            uiPrefabPaths[panelName] = prefabPath;
        }
        
        /// <summary>
        /// 设置UI音效开关
        /// </summary>
        public void SetUISoundEnabled(bool enabled)
        {
            enableUISound = enabled;
        }
        
        /// <summary>
        /// 设置UI动画开关
        /// </summary>
        public void SetUIAnimationEnabled(bool enabled)
        {
            enableUIAnimation = enabled;
        }
        
        #endregion
    }
}