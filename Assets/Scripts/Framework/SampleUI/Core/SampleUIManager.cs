using System;
using System.Collections.Generic;
using UnityEngine;
using Framework.SampleUI.Core;

namespace Framework.SampleUI
{
    /// <summary>
    /// SampleUI管理器
    /// 极简UI框架的核心管理类，负责UI面板的生命周期管理
    /// </summary>
    public class SampleUIManager : MonoBehaviour
    {
        #region 单例模式
        
        private static SampleUIManager instance;
        
        /// <summary>
        /// 单例实例
        /// </summary>
        public static SampleUIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SampleUIManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SampleUIManager");
                        instance = go.AddComponent<SampleUIManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        #endregion
        
        #region 字段和属性
        
        [Header("UI根节点配置")]
        [SerializeField] private Canvas uiCanvas;
        [SerializeField] private Transform normalLayer;     // 普通面板层
        [SerializeField] private Transform popupLayer;      // 弹窗层
        [SerializeField] private Transform systemLayer;     // 系统层
        [SerializeField] private Transform hudLayer;        // HUD层
        
        [Header("管理器配置")]
        [SerializeField] private bool autoCreateLayers = true;
        [SerializeField] private bool enableDebugLog = true;
        [SerializeField] private string uiPrefabPath = "UI/Panels/";
        
        // 面板管理
        private Dictionary<string, ISampleUIBase> allPanels = new Dictionary<string, ISampleUIBase>();
        private Dictionary<string, ISampleUIBase> activePanels = new Dictionary<string, ISampleUIBase>();
        private Stack<ISampleUIBase> normalPanelStack = new Stack<ISampleUIBase>();
        private List<ISampleUIBase> popupPanels = new List<ISampleUIBase>();
        
        // 预制体缓存
        private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
        
        // 层级映射
        private Dictionary<SampleUIBaseType, Transform> layerMap = new Dictionary<SampleUIBaseType, Transform>();
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// 当前活跃面板数量
        /// </summary>
        public int ActivePanelCount => activePanels.Count;
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 面板显示事件
        /// </summary>
        public event Action<string, ISampleUIBase> OnPanelShown;
        
        /// <summary>
        /// 面板隐藏事件
        /// </summary>
        public event Action<string, ISampleUIBase> OnPanelHidden;
        
        /// <summary>
        /// 面板创建事件
        /// </summary>
        public event Action<string, ISampleUIBase> OnPanelCreated;
        
        /// <summary>
        /// 面板销毁事件
        /// </summary>
        public event Action<string, ISampleUIBase> OnPanelDestroyed;
        
        #endregion
        
        #region Unity生命周期
        
        private void Awake()
        {
            // 确保单例
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            // 更新所有活跃面板的组件
            float deltaTime = Time.deltaTime;
            foreach (var panel in activePanels.Values)
            {
                if (panel is SampleUIBase samplePanel)
                {
                    // 更新面板组件
                    UpdatePanelComponents(samplePanel, deltaTime);
                }
            }
        }
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化管理器
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized)
                return;
            
            try
            {
                // 初始化UI层级
                InitializeUILayers();
                
                // 构建层级映射
                BuildLayerMap();
                
                IsInitialized = true;
                
                if (enableDebugLog)
                    Debug.Log("[SampleUI] 管理器初始化完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SampleUI] 管理器初始化失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 初始化UI层级
        /// </summary>
        private void InitializeUILayers()
        {
            // 如果没有指定Canvas，尝试查找或创建
            if (uiCanvas == null)
            {
                uiCanvas = FindObjectOfType<Canvas>();
                if (uiCanvas == null)
                {
                    CreateUICanvas();
                }
            }
            
            // 自动创建层级
            if (autoCreateLayers)
            {
                CreateUILayers();
            }
        }
        
        /// <summary>
        /// 创建UI Canvas
        /// </summary>
        private void CreateUICanvas()
        {
            GameObject canvasGO = new GameObject("UICanvas");
            canvasGO.transform.SetParent(transform);
            
            uiCanvas = canvasGO.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiCanvas.sortingOrder = 0;
            
            // 添加CanvasScaler
            var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            // 添加GraphicRaycaster
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        /// <summary>
        /// 创建UI层级
        /// </summary>
        private void CreateUILayers()
        {
            if (normalLayer == null)
                normalLayer = CreateUILayer("NormalLayer", 0);
            
            if (popupLayer == null)
                popupLayer = CreateUILayer("PopupLayer", 100);
            
            if (systemLayer == null)
                systemLayer = CreateUILayer("SystemLayer", 200);
            
            if (hudLayer == null)
                hudLayer = CreateUILayer("HUDLayer", 300);
        }
        
        /// <summary>
        /// 创建UI层
        /// </summary>
        /// <param name="layerName">层名称</param>
        /// <param name="sortingOrder">排序顺序</param>
        /// <returns>层Transform</returns>
        private Transform CreateUILayer(string layerName, int sortingOrder)
        {
            GameObject layerGO = new GameObject(layerName);
            layerGO.transform.SetParent(uiCanvas.transform);
            
            // 设置RectTransform
            RectTransform rectTransform = layerGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // 添加Canvas用于层级控制
            Canvas layerCanvas = layerGO.AddComponent<Canvas>();
            layerCanvas.overrideSorting = true;
            layerCanvas.sortingOrder = sortingOrder;
            
            return layerGO.transform;
        }
        
        /// <summary>
        /// 构建层级映射
        /// </summary>
        private void BuildLayerMap()
        {
            layerMap[SampleUIBaseType.Normal] = normalLayer;
            layerMap[SampleUIBaseType.Popup] = popupLayer;
            layerMap[SampleUIBaseType.System] = systemLayer;
            layerMap[SampleUIBaseType.HUD] = hudLayer;
        }
        
        #endregion
        
        #region 核心API
        
        /// <summary>
        /// 显示面板
        /// </summary>
        /// <param name="panelId">面板ID</param>
        /// <param name="data">传递的数据</param>
        /// <returns>面板实例</returns>
        public ISampleUIBase ShowPanel(string panelId, object data = null)
        {
            if (string.IsNullOrEmpty(panelId))
            {
                Debug.LogError("[SampleUI] 面板ID不能为空");
                return null;
            }
            
            try
            {
                // 获取或创建面板
                ISampleUIBase panel = GetOrCreatePanel(panelId);
                if (panel == null)
                {
                    Debug.LogError($"[SampleUI] 无法创建面板: {panelId}");
                    return null;
                }
                
                // 处理面板类型逻辑
                HandlePanelTypeLogic(panel);
                
                // 显示面板
                panel.Show(data);
                
                // 添加到活跃面板列表
                if (!activePanels.ContainsKey(panelId))
                {
                    activePanels[panelId] = panel;
                }
                
                // 触发事件
                OnPanelShown?.Invoke(panelId, panel);
                
                if (enableDebugLog)
                    Debug.Log($"[SampleUI] 显示面板: {panelId}");
                
                return panel;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SampleUI] 显示面板失败 {panelId}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 隐藏面板
        /// </summary>
        /// <param name="panelId">面板ID</param>
        public void HidePanel(string panelId)
        {
            if (string.IsNullOrEmpty(panelId))
            {
                Debug.LogError("[SampleUI] 面板ID不能为空");
                return;
            }
            
            if (!activePanels.TryGetValue(panelId, out ISampleUIBase panel))
            {
                if (enableDebugLog)
                    Debug.LogWarning($"[SampleUI] 面板未激活: {panelId}");
                return;
            }
            
            try
            {
                // 隐藏面板
                panel.Hide();
                
                // 从活跃面板列表移除
                activePanels.Remove(panelId);
                
                // 处理面板类型逻辑
                HandlePanelHideLogic(panel);
                
                // 触发事件
                OnPanelHidden?.Invoke(panelId, panel);
                
                if (enableDebugLog)
                    Debug.Log($"[SampleUI] 隐藏面板: {panelId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SampleUI] 隐藏面板失败 {panelId}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 销毁面板
        /// </summary>
        /// <param name="panelId">面板ID</param>
        public void DestroyPanel(string panelId)
        {
            if (string.IsNullOrEmpty(panelId))
            {
                Debug.LogError("[SampleUI] 面板ID不能为空");
                return;
            }
            
            if (allPanels.TryGetValue(panelId, out ISampleUIBase panel))
            {
                // 先隐藏面板
                if (activePanels.ContainsKey(panelId))
                {
                    HidePanel(panelId);
                }
                
                // 销毁面板
                panel.Destroy();
                
                // 从所有面板列表移除
                allPanels.Remove(panelId);
                
                // 触发事件
                OnPanelDestroyed?.Invoke(panelId, panel);
                
                if (enableDebugLog)
                    Debug.Log($"[SampleUI] 销毁面板: {panelId}");
            }
        }
        
        /// <summary>
        /// 获取面板
        /// </summary>
        /// <param name="panelId">面板ID</param>
        /// <returns>面板实例</returns>
        public ISampleUIBase GetPanel(string panelId)
        {
            allPanels.TryGetValue(panelId, out ISampleUIBase panel);
            return panel;
        }
        
        /// <summary>
        /// 获取面板（泛型版本）
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        /// <param name="panelId">面板ID</param>
        /// <returns>面板实例</returns>
        public T GetPanel<T>(string panelId) where T : class, ISampleUIBase
        {
            return GetPanel(panelId) as T;
        }
        
        /// <summary>
        /// 检查面板是否存在
        /// </summary>
        /// <param name="panelId">面板ID</param>
        /// <returns>是否存在</returns>
        public bool HasPanel(string panelId)
        {
            return allPanels.ContainsKey(panelId);
        }
        
        /// <summary>
        /// 检查面板是否激活
        /// </summary>
        /// <param name="panelId">面板ID</param>
        /// <returns>是否激活</returns>
        public bool IsPanelActive(string panelId)
        {
            return activePanels.ContainsKey(panelId);
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 获取或创建面板
        /// </summary>
        /// <param name="panelId">面板ID</param>
        /// <returns>面板实例</returns>
        private ISampleUIBase GetOrCreatePanel(string panelId)
        {
            // 先从已创建的面板中查找
            if (allPanels.TryGetValue(panelId, out ISampleUIBase existingPanel))
            {
                return existingPanel;
            }
            
            // 创建新面板
            return CreatePanel(panelId);
        }
        
        /// <summary>
        /// 创建面板
        /// </summary>
        /// <param name="panelId">面板ID</param>
        /// <returns>面板实例</returns>
        private ISampleUIBase CreatePanel(string panelId)
        {
            // 加载预制体
            GameObject prefab = LoadPanelPrefab(panelId);
            if (prefab == null)
            {
                Debug.LogError($"[SampleUI] 无法加载面板预制体: {panelId}");
                return null;
            }
            
            // 实例化预制体
            GameObject panelGO = Instantiate(prefab);
            
            // 获取面板组件
            ISampleUIBase panel = panelGO.GetComponent<ISampleUIBase>();
            if (panel == null)
            {
                Debug.LogError($"[SampleUI] 预制体缺少ISampleUIBase组件: {panelId}");
                Destroy(panelGO);
                return null;
            }
            
            // 设置父级
            Transform parentLayer = GetPanelLayer(panel.PanelType);
            if (parentLayer != null)
            {
                panelGO.transform.SetParent(parentLayer, false);
            }
            
            // 注册面板事件
            RegisterPanelEvents(panel);
            
            // 添加到面板列表
            allPanels[panelId] = panel;
            
            // 触发事件
            OnPanelCreated?.Invoke(panelId, panel);
            
            return panel;
        }
        
        /// <summary>
        /// 加载面板预制体
        /// </summary>
        /// <param name="panelId">面板ID</param>
        /// <returns>预制体GameObject</returns>
        private GameObject LoadPanelPrefab(string panelId)
        {
            // 先从缓存中查找
            if (prefabCache.TryGetValue(panelId, out GameObject cachedPrefab))
            {
                return cachedPrefab;
            }
            
            // 从Resources加载
            string prefabPath = $"{uiPrefabPath}{panelId}";
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            
            if (prefab != null)
            {
                prefabCache[panelId] = prefab;
            }
            
            return prefab;
        }
        
        /// <summary>
        /// 获取面板层级
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <returns>层级Transform</returns>
        private Transform GetPanelLayer(SampleUIBaseType panelType)
        {
            layerMap.TryGetValue(panelType, out Transform layer);
            return layer;
        }
        
        /// <summary>
        /// 注册面板事件
        /// </summary>
        /// <param name="panel">面板实例</param>
        private void RegisterPanelEvents(ISampleUIBase panel)
        {
            panel.OnDestroyed += OnPanelDestroyedHandler;
        }
        
        /// <summary>
        /// 面板销毁事件处理
        /// </summary>
        /// <param name="panel">面板实例</param>
        private void OnPanelDestroyedHandler(ISampleUIBase panel)
        {
            // 从所有列表中移除
            string panelId = panel.PanelId;
            allPanels.Remove(panelId);
            activePanels.Remove(panelId);
            
            // 从栈中移除
            if (normalPanelStack.Contains(panel))
            {
                var tempStack = new Stack<ISampleUIBase>();
                while (normalPanelStack.Count > 0)
                {
                    var p = normalPanelStack.Pop();
                    if (p != panel)
                        tempStack.Push(p);
                }
                
                while (tempStack.Count > 0)
                {
                    normalPanelStack.Push(tempStack.Pop());
                }
            }
            
            // 从弹窗列表移除
            popupPanels.Remove(panel);
        }
        
        /// <summary>
        /// 处理面板类型逻辑
        /// </summary>
        /// <param name="panel">面板实例</param>
        private void HandlePanelTypeLogic(ISampleUIBase panel)
        {
            switch (panel.PanelType)
            {
                case SampleUIBaseType.Normal:
                    // 普通面板：隐藏当前栈顶面板，将新面板压入栈
                    if (normalPanelStack.Count > 0)
                    {
                        var currentPanel = normalPanelStack.Peek();
                        if (currentPanel.IsShowing)
                        {
                            currentPanel.Hide();
                        }
                    }
                    normalPanelStack.Push(panel);
                    break;
                    
                case SampleUIBaseType.Popup:
                    // 弹窗面板：添加到弹窗列表
                    if (!popupPanels.Contains(panel))
                    {
                        popupPanels.Add(panel);
                    }
                    break;
                    
                case SampleUIBaseType.System:
                case SampleUIBaseType.HUD:
                    // 系统面板和HUD：直接显示，不影响其他面板
                    break;
            }
        }
        
        /// <summary>
        /// 处理面板隐藏逻辑
        /// </summary>
        /// <param name="panel">面板实例</param>
        private void HandlePanelHideLogic(ISampleUIBase panel)
        {
            switch (panel.PanelType)
            {
                case SampleUIBaseType.Normal:
                    // 普通面板：从栈中移除，显示下一个面板
                    if (normalPanelStack.Count > 0 && normalPanelStack.Peek() == panel)
                    {
                        normalPanelStack.Pop();
                        
                        // 显示下一个面板
                        if (normalPanelStack.Count > 0)
                        {
                            var nextPanel = normalPanelStack.Peek();
                            if (!nextPanel.IsShowing)
                            {
                                nextPanel.Show();
                            }
                        }
                    }
                    break;
                    
                case SampleUIBaseType.Popup:
                    // 弹窗面板：从弹窗列表移除
                    popupPanels.Remove(panel);
                    break;
            }
        }
        
        /// <summary>
        /// 更新面板组件
        /// </summary>
        /// <param name="panel">面板实例</param>
        /// <param name="deltaTime">时间间隔</param>
        private void UpdatePanelComponents(SampleUIBase panel, float deltaTime)
        {
            // 这里可以添加组件更新逻辑
            // 由于组件系统在面板内部，这里暂时留空
        }
        
        #endregion
        
        #region 便捷方法
        
        /// <summary>
        /// 显示面板（泛型版本）
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        /// <param name="data">传递的数据</param>
        /// <returns>面板实例</returns>
        public T ShowPanel<T>(object data = null) where T : class, ISampleUIBase
        {
            string panelId = typeof(T).Name;
            return ShowPanel(panelId, data) as T;
        }
        
        /// <summary>
        /// 隐藏面板（泛型版本）
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        public void HidePanel<T>() where T : class, ISampleUIBase
        {
            string panelId = typeof(T).Name;
            HidePanel(panelId);
        }
        
        /// <summary>
        /// 返回上一个面板
        /// </summary>
        public void GoBack()
        {
            if (normalPanelStack.Count > 1)
            {
                var currentPanel = normalPanelStack.Peek();
                HidePanel(currentPanel.PanelId);
            }
        }
        
        /// <summary>
        /// 关闭所有弹窗
        /// </summary>
        public void CloseAllPopups()
        {
            var popupsToClose = new List<ISampleUIBase>(popupPanels);
            foreach (var popup in popupsToClose)
            {
                HidePanel(popup.PanelId);
            }
        }
        
        /// <summary>
        /// 清理所有面板
        /// </summary>
        public void ClearAllPanels()
        {
            var panelsToDestroy = new List<string>(allPanels.Keys);
            foreach (var panelId in panelsToDestroy)
            {
                DestroyPanel(panelId);
            }
        }
        
        #endregion
    }
}