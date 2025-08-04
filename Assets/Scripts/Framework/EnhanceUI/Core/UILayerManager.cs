using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace Framework.EnhanceUI.Core
{
    /// <summary>
    /// UI层级管理器
    /// 负责管理UI的层级结构和渲染顺序
    /// </summary>
    public class UILayerManager : MonoBehaviour
    {
        #region 字段和属性
        
        [Header("UI根节点")]
        [SerializeField] private Transform uiRoot;
        
        [Header("层级配置")]
        [SerializeField] private bool autoCreateLayers = true;
        [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);
        [SerializeField] private float matchWidthOrHeight = 0.5f;
        
        // 层级字典 - 存储各个层级的Transform
        private Dictionary<UILayerType, Transform> layerTransforms = new Dictionary<UILayerType, Transform>();
        
        // 层级Canvas字典 - 存储各个层级的Canvas组件
        private Dictionary<UILayerType, Canvas> layerCanvases = new Dictionary<UILayerType, Canvas>();
        
        // 层级面板计数 - 记录每个层级的面板数量
        private Dictionary<UILayerType, int> layerPanelCounts = new Dictionary<UILayerType, int>();
        
        /// <summary>
        /// UI根节点
        /// </summary>
        public Transform UIRoot => uiRoot;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        #endregion
        
        #region 事件委托
        
        /// <summary>
        /// 层级创建事件
        /// </summary>
        public event Action<UILayerType, Transform> OnLayerCreated;
        
        /// <summary>
        /// 面板添加到层级事件
        /// </summary>
        public event Action<UILayerType, EnhanceUIPanel> OnPanelAddedToLayer;
        
        /// <summary>
        /// 面板从层级移除事件
        /// </summary>
        public event Action<UILayerType, EnhanceUIPanel> OnPanelRemovedFromLayer;
        
        #endregion
        
        #region 初始化方法
        
        /// <summary>
        /// 初始化层级管理器
        /// </summary>
        /// <param name="rootTransform">UI根节点</param>
        public void Initialize(Transform rootTransform = null)
        {
            if (IsInitialized)
            {
                Debug.LogWarning("[UILayerManager] 层级管理器已初始化");
                return;
            }
            
            try
            {
                // 设置UI根节点
                if (rootTransform != null)
                    uiRoot = rootTransform;
                else if (uiRoot == null)
                    CreateUIRoot();
                
                // 自动创建层级
                if (autoCreateLayers)
                    CreateAllLayers();
                
                IsInitialized = true;
                Debug.Log("[UILayerManager] 层级管理器初始化完成");
            }
            catch (Exception e)
            {
                Debug.LogError($"[UILayerManager] 层级管理器初始化失败: {e.Message}");
                throw;
            }
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
            CanvasScaler scaler = uiRootGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = matchWidthOrHeight;
            
            // 添加GraphicRaycaster组件
            uiRootGO.AddComponent<GraphicRaycaster>();
            
            Debug.Log("[UILayerManager] UI根节点创建完成");
        }
        
        /// <summary>
        /// 创建所有预定义层级
        /// </summary>
        private void CreateAllLayers()
        {
            // 按照枚举顺序创建所有层级
            foreach (UILayerType layerType in Enum.GetValues(typeof(UILayerType)))
            {
                CreateLayer(layerType);
            }
        }
        
        /// <summary>
        /// 创建指定层级
        /// </summary>
        /// <param name="layerType">层级类型</param>
        /// <returns>层级Transform</returns>
        public Transform CreateLayer(UILayerType layerType)
        {
            if (layerTransforms.ContainsKey(layerType))
            {
                Debug.LogWarning($"[UILayerManager] 层级已存在: {layerType}");
                return layerTransforms[layerType];
            }
            
            try
            {
                // 创建层级GameObject
                string layerName = $"{layerType}Layer";
                GameObject layerGO = new GameObject(layerName);
                Transform layerTransform = layerGO.transform;
                layerTransform.SetParent(uiRoot);
                
                // 设置RectTransform
                RectTransform rectTransform = layerGO.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                rectTransform.localScale = Vector3.one;
                
                // 添加Canvas组件用于层级控制
                Canvas canvas = layerGO.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = layerType.GetSortingOrder();
                
                // 添加GraphicRaycaster组件
                layerGO.AddComponent<GraphicRaycaster>();
                
                // 存储到字典
                layerTransforms[layerType] = layerTransform;
                layerCanvases[layerType] = canvas;
                layerPanelCounts[layerType] = 0;
                
                // 触发层级创建事件
                OnLayerCreated?.Invoke(layerType, layerTransform);
                
                Debug.Log($"[UILayerManager] 层级创建完成: {layerType} (SortingOrder: {canvas.sortingOrder})");
                return layerTransform;
            }
            catch (Exception e)
            {
                Debug.LogError($"[UILayerManager] 创建层级失败: {layerType}, 错误: {e.Message}");
                throw;
            }
        }
        
        #endregion
        
        #region 层级管理方法
        
        /// <summary>
        /// 获取指定层级的Transform
        /// </summary>
        /// <param name="layerType">层级类型</param>
        /// <returns>层级Transform，如果不存在则自动创建</returns>
        public Transform GetLayer(UILayerType layerType)
        {
            if (!layerTransforms.ContainsKey(layerType))
            {
                Debug.LogWarning($"[UILayerManager] 层级不存在，自动创建: {layerType}");
                return CreateLayer(layerType);
            }
            
            return layerTransforms[layerType];
        }
        
        /// <summary>
        /// 获取指定层级的Canvas
        /// </summary>
        /// <param name="layerType">层级类型</param>
        /// <returns>层级Canvas</returns>
        public Canvas GetLayerCanvas(UILayerType layerType)
        {
            if (!layerCanvases.ContainsKey(layerType))
            {
                CreateLayer(layerType);
            }
            
            return layerCanvases[layerType];
        }
        
        /// <summary>
        /// 将面板添加到指定层级
        /// </summary>
        /// <param name="panel">UI面板</param>
        /// <param name="layerType">层级类型</param>
        public void AddPanelToLayer(EnhanceUIPanel panel, UILayerType layerType)
        {
            if (panel == null)
            {
                Debug.LogError("[UILayerManager] 尝试添加空面板到层级");
                return;
            }
            
            try
            {
                // 获取层级Transform
                Transform layerTransform = GetLayer(layerType);
                
                // 设置面板的父节点
                panel.transform.SetParent(layerTransform, false);
                
                // 更新面板的层级类型
                panel.LayerType = layerType;
                
                // 增加层级面板计数
                if (layerPanelCounts.ContainsKey(layerType))
                    layerPanelCounts[layerType]++;
                else
                    layerPanelCounts[layerType] = 1;
                
                // 触发面板添加事件
                OnPanelAddedToLayer?.Invoke(layerType, panel);
                
                Debug.Log($"[UILayerManager] 面板添加到层级: {panel.PanelName} -> {layerType}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[UILayerManager] 添加面板到层级失败: {panel.PanelName} -> {layerType}, 错误: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 从层级移除面板
        /// </summary>
        /// <param name="panel">UI面板</param>
        public void RemovePanelFromLayer(EnhanceUIPanel panel)
        {
            if (panel == null)
            {
                Debug.LogError("[UILayerManager] 尝试从层级移除空面板");
                return;
            }
            
            try
            {
                UILayerType layerType = panel.LayerType;
                
                // 减少层级面板计数
                if (layerPanelCounts.ContainsKey(layerType) && layerPanelCounts[layerType] > 0)
                    layerPanelCounts[layerType]--;
                
                // 触发面板移除事件
                OnPanelRemovedFromLayer?.Invoke(layerType, panel);
                
                Debug.Log($"[UILayerManager] 面板从层级移除: {panel.PanelName} <- {layerType}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[UILayerManager] 从层级移除面板失败: {panel.PanelName}, 错误: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 调整层级的排序顺序
        /// </summary>
        /// <param name="layerType">层级类型</param>
        /// <param name="sortingOrder">新的排序顺序</param>
        public void SetLayerSortingOrder(UILayerType layerType, int sortingOrder)
        {
            if (!layerCanvases.ContainsKey(layerType))
            {
                Debug.LogWarning($"[UILayerManager] 层级不存在: {layerType}");
                return;
            }
            
            try
            {
                Canvas canvas = layerCanvases[layerType];
                int oldSortingOrder = canvas.sortingOrder;
                canvas.sortingOrder = sortingOrder;
                
                Debug.Log($"[UILayerManager] 层级排序顺序调整: {layerType} ({oldSortingOrder} -> {sortingOrder})");
            }
            catch (Exception e)
            {
                Debug.LogError($"[UILayerManager] 调整层级排序顺序失败: {layerType}, 错误: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 获取层级的面板数量
        /// </summary>
        /// <param name="layerType">层级类型</param>
        /// <returns>面板数量</returns>
        public int GetLayerPanelCount(UILayerType layerType)
        {
            return layerPanelCounts.ContainsKey(layerType) ? layerPanelCounts[layerType] : 0;
        }
        
        /// <summary>
        /// 获取层级中的所有面板
        /// </summary>
        /// <param name="layerType">层级类型</param>
        /// <returns>面板列表</returns>
        public List<EnhanceUIPanel> GetLayerPanels(UILayerType layerType)
        {
            var panels = new List<EnhanceUIPanel>();
            
            if (!layerTransforms.ContainsKey(layerType))
                return panels;
            
            Transform layerTransform = layerTransforms[layerType];
            for (int i = 0; i < layerTransform.childCount; i++)
            {
                var panel = layerTransform.GetChild(i).GetComponent<EnhanceUIPanel>();
                if (panel != null)
                    panels.Add(panel);
            }
            
            return panels;
        }
        
        /// <summary>
        /// 检查层级冲突
        /// </summary>
        /// <returns>冲突信息列表</returns>
        public List<string> CheckLayerConflicts()
        {
            var conflicts = new List<string>();
            var sortingOrders = new Dictionary<int, List<UILayerType>>();
            
            // 收集所有层级的排序顺序
            foreach (var kvp in layerCanvases)
            {
                int sortingOrder = kvp.Value.sortingOrder;
                if (!sortingOrders.ContainsKey(sortingOrder))
                    sortingOrders[sortingOrder] = new List<UILayerType>();
                
                sortingOrders[sortingOrder].Add(kvp.Key);
            }
            
            // 检查冲突
            foreach (var kvp in sortingOrders)
            {
                if (kvp.Value.Count > 1)
                {
                    string layerNames = string.Join(", ", kvp.Value);
                    conflicts.Add($"排序顺序 {kvp.Key} 存在冲突的层级: {layerNames}");
                }
            }
            
            return conflicts;
        }
        
        /// <summary>
        /// 重置所有层级的排序顺序
        /// </summary>
        public void ResetLayerSortingOrders()
        {
            foreach (var kvp in layerCanvases)
            {
                UILayerType layerType = kvp.Key;
                Canvas canvas = kvp.Value;
                canvas.sortingOrder = layerType.GetSortingOrder();
            }
            
            Debug.Log("[UILayerManager] 所有层级排序顺序已重置");
        }
        
        /// <summary>
        /// 获取所有层级信息
        /// </summary>
        /// <returns>层级信息字典</returns>
        public Dictionary<UILayerType, LayerInfo> GetAllLayerInfo()
        {
            var layerInfos = new Dictionary<UILayerType, LayerInfo>();
            
            foreach (var layerType in layerTransforms.Keys)
            {
                var info = new LayerInfo
                {
                    LayerType = layerType,
                    Transform = layerTransforms[layerType],
                    Canvas = layerCanvases.ContainsKey(layerType) ? layerCanvases[layerType] : null,
                    PanelCount = GetLayerPanelCount(layerType),
                    SortingOrder = layerCanvases.ContainsKey(layerType) ? layerCanvases[layerType].sortingOrder : 0
                };
                
                layerInfos[layerType] = info;
            }
            
            return layerInfos;
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 清理空的层级
        /// </summary>
        public void CleanupEmptyLayers()
        {
            var layersToRemove = new List<UILayerType>();
            
            foreach (var kvp in layerTransforms)
            {
                UILayerType layerType = kvp.Key;
                Transform layerTransform = kvp.Value;
                
                // 检查层级是否为空且不是系统层级
                if (layerTransform.childCount == 0 && !layerType.IsSystemLayer())
                {
                    layersToRemove.Add(layerType);
                }
            }
            
            // 移除空层级
            foreach (var layerType in layersToRemove)
            {
                RemoveLayer(layerType);
            }
            
            if (layersToRemove.Count > 0)
            {
                Debug.Log($"[UILayerManager] 清理了 {layersToRemove.Count} 个空层级");
            }
        }
        
        /// <summary>
        /// 移除层级
        /// </summary>
        /// <param name="layerType">层级类型</param>
        private void RemoveLayer(UILayerType layerType)
        {
            if (layerTransforms.ContainsKey(layerType))
            {
                Transform layerTransform = layerTransforms[layerType];
                if (layerTransform != null)
                    DestroyImmediate(layerTransform.gameObject);
                
                layerTransforms.Remove(layerType);
            }
            
            if (layerCanvases.ContainsKey(layerType))
                layerCanvases.Remove(layerType);
            
            if (layerPanelCounts.ContainsKey(layerType))
                layerPanelCounts.Remove(layerType);
        }
        
        #endregion
        
        #region Unity生命周期
        
        private void OnDestroy()
        {
            // 清理资源
            layerTransforms.Clear();
            layerCanvases.Clear();
            layerPanelCounts.Clear();
            
            Debug.Log("[UILayerManager] 层级管理器已销毁");
        }
        
        #endregion
    }
    
    /// <summary>
    /// 层级信息结构
    /// </summary>
    [Serializable]
    public struct LayerInfo
    {
        /// <summary>
        /// 层级类型
        /// </summary>
        public UILayerType LayerType;
        
        /// <summary>
        /// 层级Transform
        /// </summary>
        public Transform Transform;
        
        /// <summary>
        /// 层级Canvas
        /// </summary>
        public Canvas Canvas;
        
        /// <summary>
        /// 面板数量
        /// </summary>
        public int PanelCount;
        
        /// <summary>
        /// 排序顺序
        /// </summary>
        public int SortingOrder;
    }
}