using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.SampleUI.Config
{
    /// <summary>
    /// SampleUI框架配置
    /// 提供框架的全局配置和设置
    /// </summary>
    [CreateAssetMenu(fileName = "SampleUIConfig", menuName = "SampleUI/UI Config")]
    public class SampleUIConfig : ScriptableObject
    {
        #region 基础配置
        
        [Header("基础设置")]
        [Tooltip("是否启用调试模式")]
        public bool enableDebugMode = false;
        
        [Tooltip("是否启用性能监控")]
        public bool enablePerformanceMonitoring = false;
        
        [Tooltip("最大同时显示的面板数量")]
        [Range(1, 20)]
        public int maxConcurrentPanels = 10;
        
        [Tooltip("UI根节点名称")]
        public string uiRootName = "UIRoot";
        
        #endregion
        
        #region 层级配置
        
        [Header("层级设置")]
        [Tooltip("普通层级配置")]
        public LayerConfig normalLayer = new LayerConfig("Normal", 0, 100);
        
        [Tooltip("弹窗层级配置")]
        public LayerConfig popupLayer = new LayerConfig("Popup", 100, 200);
        
        [Tooltip("系统层级配置")]
        public LayerConfig systemLayer = new LayerConfig("System", 200, 300);
        
        [Tooltip("HUD层级配置")]
        public LayerConfig hudLayer = new LayerConfig("HUD", 300, 400);
        
        #endregion
        
        #region 动画配置
        
        [Header("动画设置")]
        [Tooltip("默认动画时长")]
        [Range(0.1f, 2f)]
        public float defaultAnimationDuration = 0.3f;
        
        [Tooltip("是否启用动画")]
        public bool enableAnimations = true;
        
        [Tooltip("动画缓动类型")]
        public AnimationEaseType defaultEaseType = AnimationEaseType.EaseOutCubic;
        
        [Tooltip("是否使用非缩放时间")]
        public bool useUnscaledTime = false;
        
        #endregion
        
        #region 音频配置
        
        [Header("音频设置")]
        [Tooltip("是否启用UI音效")]
        public bool enableUIAudio = true;
        
        [Tooltip("默认音量")]
        [Range(0f, 1f)]
        public float defaultVolume = 0.8f;
        
        [Tooltip("音效资源路径")]
        public string audioResourcePath = "UI/Audio/";
        
        #endregion
        
        #region 输入配置
        
        [Header("输入设置")]
        [Tooltip("是否启用全局输入")]
        public bool enableGlobalInput = true;
        
        [Tooltip("是否启用ESC键关闭")]
        public bool enableEscapeToClose = true;
        
        [Tooltip("是否启用拖拽")]
        public bool enableDragAndDrop = false;
        
        [Tooltip("双击检测时间")]
        [Range(0.1f, 1f)]
        public float doubleClickTime = 0.3f;
        
        #endregion
        
        #region 资源配置
        
        [Header("资源设置")]
        [Tooltip("UI预制体资源路径")]
        public string prefabResourcePath = "UI/Prefabs/";
        
        [Tooltip("是否使用对象池")]
        public bool useObjectPool = true;
        
        [Tooltip("对象池初始大小")]
        [Range(1, 10)]
        public int objectPoolInitialSize = 3;
        
        [Tooltip("对象池最大大小")]
        [Range(5, 50)]
        public int objectPoolMaxSize = 20;
        
        #endregion
        
        #region 性能配置
        
        [Header("性能设置")]
        [Tooltip("是否启用面板缓存")]
        public bool enablePanelCaching = true;
        
        [Tooltip("缓存清理间隔（秒）")]
        [Range(30f, 300f)]
        public float cacheCleanupInterval = 60f;
        
        [Tooltip("最大缓存面板数量")]
        [Range(5, 50)]
        public int maxCachedPanels = 20;
        
        [Tooltip("是否启用批量渲染")]
        public bool enableBatchRendering = true;
        
        #endregion
        
        #region 面板配置
        
        [Header("面板配置")]
        [Tooltip("面板配置列表")]
        public List<PanelConfig> panelConfigs = new List<PanelConfig>();
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 获取面板配置
        /// </summary>
        /// <param name="panelId">面板ID</param>
        /// <returns>面板配置</returns>
        public PanelConfig GetPanelConfig(string panelId)
        {
            for (int i = 0; i < panelConfigs.Count; i++)
            {
                if (panelConfigs[i].panelId == panelId)
                {
                    return panelConfigs[i];
                }
            }
            return null;
        }
        
        /// <summary>
        /// 添加面板配置
        /// </summary>
        /// <param name="config">面板配置</param>
        public void AddPanelConfig(PanelConfig config)
        {
            if (config != null && !string.IsNullOrEmpty(config.panelId))
            {
                // 检查是否已存在
                for (int i = 0; i < panelConfigs.Count; i++)
                {
                    if (panelConfigs[i].panelId == config.panelId)
                    {
                        panelConfigs[i] = config; // 更新现有配置
                        return;
                    }
                }
                
                panelConfigs.Add(config); // 添加新配置
            }
        }
        
        /// <summary>
        /// 移除面板配置
        /// </summary>
        /// <param name="panelId">面板ID</param>
        public void RemovePanelConfig(string panelId)
        {
            for (int i = panelConfigs.Count - 1; i >= 0; i--)
            {
                if (panelConfigs[i].panelId == panelId)
                {
                    panelConfigs.RemoveAt(i);
                    break;
                }
            }
        }
        
        /// <summary>
        /// 获取层级配置
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <returns>层级配置</returns>
        public LayerConfig GetLayerConfig(SampleUIBaseType panelType)
        {
            switch (panelType)
            {
                case SampleUIBaseType.Normal:
                    return normalLayer;
                case SampleUIBaseType.Popup:
                    return popupLayer;
                case SampleUIBaseType.System:
                    return systemLayer;
                case SampleUIBaseType.HUD:
                    return hudLayer;
                default:
                    return normalLayer;
            }
        }
        
        /// <summary>
        /// 验证配置
        /// </summary>
        /// <returns>是否有效</returns>
        public bool ValidateConfig()
        {
            bool isValid = true;
            
            // 检查基础配置
            if (maxConcurrentPanels <= 0)
            {
                Debug.LogError("[SampleUIConfig] maxConcurrentPanels 必须大于0");
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(uiRootName))
            {
                Debug.LogError("[SampleUIConfig] uiRootName 不能为空");
                isValid = false;
            }
            
            // 检查层级配置
            if (!ValidateLayerConfig(normalLayer, "normalLayer")) isValid = false;
            if (!ValidateLayerConfig(popupLayer, "popupLayer")) isValid = false;
            if (!ValidateLayerConfig(systemLayer, "systemLayer")) isValid = false;
            if (!ValidateLayerConfig(hudLayer, "hudLayer")) isValid = false;
            
            // 检查动画配置
            if (defaultAnimationDuration <= 0)
            {
                Debug.LogError("[SampleUIConfig] defaultAnimationDuration 必须大于0");
                isValid = false;
            }
            
            // 检查对象池配置
            if (objectPoolInitialSize > objectPoolMaxSize)
            {
                Debug.LogError("[SampleUIConfig] objectPoolInitialSize 不能大于 objectPoolMaxSize");
                isValid = false;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// 验证层级配置
        /// </summary>
        /// <param name="layerConfig">层级配置</param>
        /// <param name="configName">配置名称</param>
        /// <returns>是否有效</returns>
        private bool ValidateLayerConfig(LayerConfig layerConfig, string configName)
        {
            if (layerConfig == null)
            {
                Debug.LogError($"[SampleUIConfig] {configName} 不能为空");
                return false;
            }
            
            if (string.IsNullOrEmpty(layerConfig.layerName))
            {
                Debug.LogError($"[SampleUIConfig] {configName}.layerName 不能为空");
                return false;
            }
            
            if (layerConfig.minSortingOrder > layerConfig.maxSortingOrder)
            {
                Debug.LogError($"[SampleUIConfig] {configName}.minSortingOrder 不能大于 maxSortingOrder");
                return false;
            }
            
            return true;
        }
        
        #endregion
        
        #region 默认配置
        
        /// <summary>
        /// 创建默认配置
        /// </summary>
        /// <returns>默认配置</returns>
        public static SampleUIConfig CreateDefaultConfig()
        {
            var config = CreateInstance<SampleUIConfig>();
            
            // 设置默认值
            config.enableDebugMode = false;
            config.enablePerformanceMonitoring = false;
            config.maxConcurrentPanels = 10;
            config.uiRootName = "UIRoot";
            
            // 设置默认层级
            config.normalLayer = new LayerConfig("Normal", 0, 100);
            config.popupLayer = new LayerConfig("Popup", 100, 200);
            config.systemLayer = new LayerConfig("System", 200, 300);
            config.hudLayer = new LayerConfig("HUD", 300, 400);
            
            // 设置默认动画
            config.defaultAnimationDuration = 0.3f;
            config.enableAnimations = true;
            config.defaultEaseType = AnimationEaseType.EaseOutCubic;
            config.useUnscaledTime = false;
            
            // 设置默认音频
            config.enableUIAudio = true;
            config.defaultVolume = 0.8f;
            config.audioResourcePath = "UI/Audio/";
            
            // 设置默认输入
            config.enableGlobalInput = true;
            config.enableEscapeToClose = true;
            config.enableDragAndDrop = false;
            config.doubleClickTime = 0.3f;
            
            // 设置默认资源
            config.prefabResourcePath = "UI/Prefabs/";
            config.useObjectPool = true;
            config.objectPoolInitialSize = 3;
            config.objectPoolMaxSize = 20;
            
            // 设置默认性能
            config.enablePanelCaching = true;
            config.cacheCleanupInterval = 60f;
            config.maxCachedPanels = 20;
            config.enableBatchRendering = true;
            
            return config;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 层级配置
    /// </summary>
    [System.Serializable]
    public class LayerConfig
    {
        [Tooltip("层级名称")]
        public string layerName;
        
        [Tooltip("最小排序顺序")]
        public int minSortingOrder;
        
        [Tooltip("最大排序顺序")]
        public int maxSortingOrder;
        
        [Tooltip("当前排序顺序")]
        public int currentSortingOrder;
        
        public LayerConfig()
        {
            layerName = "";
            minSortingOrder = 0;
            maxSortingOrder = 100;
            currentSortingOrder = 0;
        }
        
        public LayerConfig(string name, int min, int max)
        {
            layerName = name;
            minSortingOrder = min;
            maxSortingOrder = max;
            currentSortingOrder = min;
        }
        
        /// <summary>
        /// 获取下一个排序顺序
        /// </summary>
        /// <returns>排序顺序</returns>
        public int GetNextSortingOrder()
        {
            currentSortingOrder++;
            if (currentSortingOrder > maxSortingOrder)
            {
                currentSortingOrder = maxSortingOrder;
            }
            return currentSortingOrder;
        }
        
        /// <summary>
        /// 重置排序顺序
        /// </summary>
        public void ResetSortingOrder()
        {
            currentSortingOrder = minSortingOrder;
        }
    }
    
    /// <summary>
    /// 面板配置
    /// </summary>
    [System.Serializable]
    public class PanelConfig
    {
        [Tooltip("面板ID")]
        public string panelId;
        
        [Tooltip("面板显示名称")]
        public string displayName;
        
        [Tooltip("面板类型")]
        public SampleUIBaseType panelType;
        
        [Tooltip("预制体路径")]
        public string prefabPath;
        
        [Tooltip("是否预加载")]
        public bool preload;
        
        [Tooltip("是否缓存")]
        public bool cache;
        
        [Tooltip("优先级")]
        public int priority;
        
        [Tooltip("是否单例")]
        public bool singleton;
        
        [Tooltip("动画类型")]
        public SampleUIAnimationType animationType;
        
        [Tooltip("自定义配置")]
        public string customConfig;
        
        public PanelConfig()
        {
            panelId = "";
            displayName = "";
            panelType = SampleUIBaseType.Normal;
            prefabPath = "";
            preload = false;
            cache = true;
            priority = 0;
            singleton = false;
            animationType = SampleUIAnimationType.Fade;
            customConfig = "";
        }
    }
    
    /// <summary>
    /// 动画缓动类型
    /// </summary>
    public enum AnimationEaseType
    {
        Linear,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseInQuart,
        EaseOutQuart,
        EaseInOutQuart,
        EaseInBounce,
        EaseOutBounce,
        EaseInOutBounce,
        EaseInBack,
        EaseOutBack,
        EaseInOutBack
    }
    
    /// <summary>
    /// UI面板类型（从Core命名空间引用）
    /// </summary>
    public enum SampleUIBaseType
    {
        Normal,     // 普通面板
        Popup,      // 弹窗面板
        System,     // 系统面板
        HUD         // HUD面板
    }
    
    /// <summary>
    /// UI动画类型（从Core命名空间引用）
    /// </summary>
    public enum SampleUIAnimationType
    {
        None,           // 无动画
        Fade,           // 淡入淡出
        Scale,          // 缩放
        SlideLeft,      // 左滑
        SlideRight,     // 右滑
        SlideUp,        // 上滑
        SlideDown,      // 下滑
        Bounce,         // 弹跳
        Elastic,        // 弹性
        Custom          // 自定义
    }
}