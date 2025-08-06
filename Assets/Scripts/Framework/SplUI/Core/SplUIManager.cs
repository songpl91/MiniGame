using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.SplUI.Core
{
    /// <summary>
    /// SplUI管理器
    /// 极简UI框架的核心管理类，负责UI面板的生命周期管理
    /// 采用单例模式，提供全局访问点
    /// </summary>
    public class SplUIManager : MonoBehaviour
    {
        #region 单例模式
        
        private static SplUIManager instance;
        
        /// <summary>
        /// 单例实例
        /// </summary>
        public static SplUIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SplUIManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SplUIManager");
                        instance = go.AddComponent<SplUIManager>();
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
        
        /// <summary>
        /// 所有面板字典（包括未激活的）
        /// </summary>
        private Dictionary<string, SplUIBase> allPanels = new Dictionary<string, SplUIBase>();
        
        /// <summary>
        /// 当前激活的面板字典
        /// </summary>
        private Dictionary<string, SplUIBase> activePanels = new Dictionary<string, SplUIBase>();
        
        /// <summary>
        /// 普通面板栈（用于返回上一个面板）
        /// </summary>
        private Stack<SplUIBase> normalPanelStack = new Stack<SplUIBase>();
        
        /// <summary>
        /// 弹窗面板列表
        /// </summary>
        private List<SplUIBase> popupPanels = new List<SplUIBase>();
        
        /// <summary>
        /// 预制体缓存
        /// </summary>
        private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
        
        /// <summary>
        /// 层级映射
        /// </summary>
        private Dictionary<SplUIType, Transform> layerMap = new Dictionary<SplUIType, Transform>();
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// 当前活跃面板数量
        /// </summary>
        public int ActivePanelCount => activePanels.Count;
        
        /// <summary>
        /// 当前普通面板栈深度
        /// </summary>
        public int NormalPanelStackCount => normalPanelStack.Count;
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 面板显示事件
        /// </summary>
        public event Action<string, SplUIBase> OnPanelShown;
        
        /// <summary>
        /// 面板隐藏事件
        /// </summary>
        public event Action<string, SplUIBase> OnPanelHidden;
        
        /// <summary>
        /// 面板创建事件
        /// </summary>
        public event Action<string, SplUIBase> OnPanelCreated;
        
        /// <summary>
        /// 面板销毁事件
        /// </summary>
        public event Action<string, SplUIBase> OnPanelDestroyed;
        
        #endregion
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法
        /// </summary>
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
        
        /// <summary>
        /// Unity Update方法
        /// </summary>
        private void Update()
        {
            // 更新所有活跃面板的组件
            float deltaTime = Time.deltaTime;
            foreach (var panel in activePanels.Values)
            {
                UpdatePanelComponents(panel, deltaTime);
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
                    Debug.Log("[SplUI] 管理器初始化完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SplUI] 管理器初始化失败: {ex.Message}");
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
            layerMap[SplUIType.Normal] = normalLayer;
            layerMap[SplUIType.Popup] = popupLayer;
            layerMap[SplUIType.System] = systemLayer;
            layerMap[SplUIType.HUD] = hudLayer;
        }
        
        #endregion
        
        #region 核心API
        
        /// <summary>
        /// 显示面板
        /// </summary>
        /// <param name="panelId">面板ID</param>
        /// <param name="data">传递的数据</param>
        /// <returns>面板实例</returns>
        public SplUIBase ShowPanel(string panelId, object data = null)
        {
            if (string.IsNullOrEmpty(panelId))
            {
                Debug.LogError("[SplUI] 面板ID不能为空");
                return null;
            }
            
            try
            {
                // 获取或创建面板
                SplUIBase panel = GetOrCreatePanel(panelId);
                if (panel == null)
                {
                    Debug.LogError($"[SplUI] 无法创建面板: {panelId}");
                    return null;
                }
                
                // 处理面板类型逻辑
                HandlePanelShowLogic(panel);
                
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
                    Debug.Log($"[SplUI] 显示面板: {panelId}");
                
                return panel;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SplUI] 显示面板失败 {panelId}: {ex.Message}");
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
                Debug.LogError("[SplUI] 面板ID不能为空");
                return;
            }
            
            if (!activePanels.TryGetValue(panelId, out SplUIBase panel))
            {
                if (enableDebugLog)
                    Debug.LogWarning($"[SplUI] 面板未激活: {panelId}");
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
                    Debug.Log($"[SplUI] 隐藏面板: {panelId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SplUI] 隐藏面板失败 {panelId}: {ex.Message}");
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
                Debug.LogError("[SplUI] 面板ID不能为空");
                return;
            }
            
            if (allPanels.TryGetValue(panelId, out SplUIBase panel))
            {
                // 先隐藏面板
                if (activePanels.ContainsKey(panelId))
                {
                    HidePanel(panelId);
                }
                
                // 销毁面板
                panel.DestroyPanel();
                
                // 从所有面板列表移除
                allPanels.Remove(panelId);
                
                // 触发事件
                OnPanelDestroyed?.Invoke(panelId, panel);
                
                if (enableDebugLog)
                    Debug.Log($"[SplUI] 销毁面板: {panelId}");
            }
        }
        
        /// <summary>
        /// 获取面板
        /// </summary>
        /// <param name="panelId">面板ID</param>
        /// <returns>面板实例</returns>
        public SplUIBase GetPanel(string panelId)
        {
            allPanels.TryGetValue(panelId, out SplUIBase panel);
            return panel;
        }
        
        /// <summary>
        /// 获取活跃面板
        /// </summary>
        /// <param name="panelId">面板ID</param>
        /// <returns>面板实例</returns>
        public SplUIBase GetActivePanel(string panelId)
        {
            activePanels.TryGetValue(panelId, out SplUIBase panel);
            return panel;
        }
        
        /// <summary>
        /// 返回上一个普通面板
        /// </summary>
        public void GoBack()
        {
            if (normalPanelStack.Count <= 1)
            {
                if (enableDebugLog)
                    Debug.LogWarning("[SplUI] 没有可返回的面板");
                return;
            }
            
            // 隐藏当前面板
            SplUIBase currentPanel = normalPanelStack.Pop();
            if (currentPanel != null)
            {
                HidePanel(currentPanel.PanelId);
            }
            
            // 显示上一个面板
            if (normalPanelStack.Count > 0)
            {
                SplUIBase previousPanel = normalPanelStack.Peek();
                if (previousPanel != null)
                {
                    ShowPanel(previousPanel.PanelId);
                }
            }
        }
        
        /// <summary>
        /// 隐藏所有弹窗
        /// </summary>
        public void HideAllPopups()
        {
            for (int i = popupPanels.Count - 1; i >= 0; i--)
            {
                SplUIBase popup = popupPanels[i];
                if (popup != null)
                {
                    HidePanel(popup.PanelId);
                }
            }
        }
        
        /// <summary>
        /// 清理所有面板
        /// </summary>
        public void ClearAllPanels()
        {
            // 隐藏所有活跃面板
            var activePanelIds = new List<string>(activePanels.Keys);
            foreach (string panelId in activePanelIds)
            {
                HidePanel(panelId);
            }
            
            // 销毁所有面板
            var allPanelIds = new List<string>(allPanels.Keys);
            foreach (string panelId in allPanelIds)
            {
                DestroyPanel(panelId);
            }
            
            // 清理栈和列表
            normalPanelStack.Clear();
            popupPanels.Clear();
        }
        
        #endregion
        
        #region 内部方法
        
        /// <summary>
        /// 获取或创建面板
        /// </summary>
        /// <param name="panelId">面板ID</param>
        /// <returns>面板实例</returns>
        private SplUIBase GetOrCreatePanel(string panelId)
        {
            // 如果面板已存在，直接返回
            if (allPanels.TryGetValue(panelId, out SplUIBase existingPanel))
            {
                return existingPanel;
            }
            
            // 尝试从Resources加载预制体
            GameObject prefab = LoadPanelPrefab(panelId);
            if (prefab == null)
            {
                Debug.LogError($"[SplUI] 找不到面板预制体: {panelId}");
                return null;
            }
            
            // 实例化面板
            GameObject panelGO = Instantiate(prefab);
            SplUIBase panel = panelGO.GetComponent<SplUIBase>();
            
            if (panel == null)
            {
                Debug.LogError($"[SplUI] 预制体缺少SplUIBase组件: {panelId}");
                Destroy(panelGO);
                return null;
            }
            
            // 设置父级
            Transform parentLayer = GetPanelLayer(panel.PanelType);
            panelGO.transform.SetParent(parentLayer, false);
            
            // 添加到面板字典
            allPanels[panelId] = panel;
            
            // 触发创建事件
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
            // 先从缓存查找
            if (prefabCache.TryGetValue(panelId, out GameObject cachedPrefab))
            {
                return cachedPrefab;
            }
            
            // 从Resources加载
            string prefabPath = uiPrefabPath + panelId;
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
        private Transform GetPanelLayer(SplUIType panelType)
        {
            if (layerMap.TryGetValue(panelType, out Transform layer))
            {
                return layer;
            }
            
            // 默认返回普通层
            return normalLayer;
        }
        
        /// <summary>
        /// 处理面板显示逻辑
        /// </summary>
        /// <param name="panel">面板实例</param>
        private void HandlePanelShowLogic(SplUIBase panel)
        {
            switch (panel.PanelType)
            {
                case SplUIType.Normal:
                    // 隐藏当前普通面板
                    if (normalPanelStack.Count > 0)
                    {
                        SplUIBase currentPanel = normalPanelStack.Peek();
                        if (currentPanel != null && currentPanel != panel)
                        {
                            currentPanel.Hide();
                            activePanels.Remove(currentPanel.PanelId);
                        }
                    }
                    
                    // 添加到栈顶
                    if (normalPanelStack.Count == 0 || normalPanelStack.Peek() != panel)
                    {
                        normalPanelStack.Push(panel);
                    }
                    break;
                
                case SplUIType.Popup:
                    // 添加到弹窗列表
                    if (!popupPanels.Contains(panel))
                    {
                        popupPanels.Add(panel);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 处理面板隐藏逻辑
        /// </summary>
        /// <param name="panel">面板实例</param>
        private void HandlePanelHideLogic(SplUIBase panel)
        {
            switch (panel.PanelType)
            {
                case SplUIType.Popup:
                    // 从弹窗列表移除
                    popupPanels.Remove(panel);
                    break;
            }
        }
        
        /// <summary>
        /// 更新面板组件
        /// </summary>
        /// <param name="panel">面板实例</param>
        /// <param name="deltaTime">时间间隔</param>
        private void UpdatePanelComponents(SplUIBase panel, float deltaTime)
        {
            // 这里可以添加组件更新逻辑
            // 由于组件系统相对简单，暂时留空
        }
        
        #endregion
        
        #region 调试方法
        
        /// <summary>
        /// 获取调试信息
        /// </summary>
        /// <returns>调试信息字符串</returns>
        public string GetDebugInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine($"[SplUI] 调试信息:");
            info.AppendLine($"- 总面板数: {allPanels.Count}");
            info.AppendLine($"- 活跃面板数: {activePanels.Count}");
            info.AppendLine($"- 普通面板栈深度: {normalPanelStack.Count}");
            info.AppendLine($"- 弹窗面板数: {popupPanels.Count}");
            
            info.AppendLine("活跃面板列表:");
            foreach (var kvp in activePanels)
            {
                info.AppendLine($"  - {kvp.Key} ({kvp.Value.PanelType})");
            }
            
            return info.ToString();
        }
        
        #endregion
    }
}