using UnityEngine;
using System.Collections.Generic;
using Framework.TraditionalUI.Utils;

namespace Framework.TraditionalUI.Config
{
    /// <summary>
    /// UI配置文件
    /// 包含UI系统的各种配置信息
    /// </summary>
    [CreateAssetMenu(fileName = "UIConfig", menuName = "Framework/Traditional UI/UI Config")]
    public class UIConfig : ScriptableObject
    {
        #region 基础配置
        
        [Header("基础配置")]
        [Tooltip("UI预制体根路径")]
        public string uiPrefabRootPath = "UI/Prefabs/";
        
        [Tooltip("UI资源根路径")]
        public string uiResourceRootPath = "UI/Resources/";
        
        [Tooltip("是否启用UI音效")]
        public bool enableUIAudio = true;
        
        [Tooltip("是否启用UI动画")]
        public bool enableUIAnimation = true;
        
        [Tooltip("是否启用UI调试模式")]
        public bool enableDebugMode = false;
        
        #endregion
        
        #region 动画配置
        
        [Header("动画配置")]
        [Tooltip("默认动画持续时间")]
        [Range(0.1f, 2.0f)]
        public float defaultAnimationDuration = 0.3f;
        
        [Tooltip("淡入淡出动画持续时间")]
        [Range(0.1f, 2.0f)]
        public float fadeAnimationDuration = 0.3f;
        
        [Tooltip("缩放动画持续时间")]
        [Range(0.1f, 2.0f)]
        public float scaleAnimationDuration = 0.3f;
        
        [Tooltip("滑动动画持续时间")]
        [Range(0.1f, 2.0f)]
        public float slideAnimationDuration = 0.3f;
        
        [Tooltip("是否使用缓动动画")]
        public bool useEasing = true;
        
        #endregion
        
        #region 层级配置
        
        [Header("层级配置")]
        [Tooltip("普通UI层级起始值")]
        public int normalUIStartOrder = 0;
        
        [Tooltip("弹窗UI层级起始值")]
        public int popupUIStartOrder = 1000;
        
        [Tooltip("系统UI层级起始值")]
        public int systemUIStartOrder = 2000;
        
        [Tooltip("顶层UI层级起始值")]
        public int topUIStartOrder = 3000;
        
        [Tooltip("层级间隔")]
        public int layerOrderInterval = 10;
        
        #endregion
        
        #region 面板配置
        
        [Header("面板配置")]
        [Tooltip("面板配置列表")]
        public List<UIPanelConfig> panelConfigs = new List<UIPanelConfig>();
        
        #endregion
        
        #region 音效配置
        
        [Header("音效配置")]
        [Tooltip("UI音效资源")]
        public UIAudioClips audioClips;
        
        [Tooltip("默认音效音量")]
        [Range(0f, 1f)]
        public float defaultAudioVolume = 1.0f;
        
        #endregion
        
        #region 性能配置
        
        [Header("性能配置")]
        [Tooltip("最大同时显示的面板数量")]
        public int maxVisiblePanels = 10;
        
        [Tooltip("面板缓存池大小")]
        public int panelPoolSize = 5;
        
        [Tooltip("是否启用面板预加载")]
        public bool enablePanelPreload = true;
        
        [Tooltip("预加载的面板列表")]
        public List<string> preloadPanels = new List<string>();
        
        #endregion
        
        #region 输入配置
        
        [Header("输入配置")]
        [Tooltip("是否启用ESC键返回")]
        public bool enableEscapeBack = true;
        
        [Tooltip("是否启用点击空白区域关闭弹窗")]
        public bool enableClickOutsideToClose = true;
        
        [Tooltip("是否启用双击防抖")]
        public bool enableDoubleClickPrevention = true;
        
        [Tooltip("双击防抖时间间隔")]
        [Range(0.1f, 1.0f)]
        public float doubleClickPreventionTime = 0.3f;
        
        #endregion
        
        #region 调试配置
        
        [Header("调试配置")]
        [Tooltip("是否显示面板边框")]
        public bool showPanelBorders = false;
        
        [Tooltip("是否显示层级信息")]
        public bool showLayerInfo = false;
        
        [Tooltip("是否记录详细日志")]
        public bool enableVerboseLogging = false;
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 获取面板配置
        /// </summary>
        /// <param name="panelName">面板名称</param>
        /// <returns>面板配置</returns>
        public UIPanelConfig GetPanelConfig(string panelName)
        {
            return panelConfigs.Find(config => config.panelName == panelName);
        }
        
        /// <summary>
        /// 添加面板配置
        /// </summary>
        /// <param name="config">面板配置</param>
        public void AddPanelConfig(UIPanelConfig config)
        {
            if (GetPanelConfig(config.panelName) == null)
            {
                panelConfigs.Add(config);
            }
        }
        
        /// <summary>
        /// 移除面板配置
        /// </summary>
        /// <param name="panelName">面板名称</param>
        public void RemovePanelConfig(string panelName)
        {
            panelConfigs.RemoveAll(config => config.panelName == panelName);
        }
        
        /// <summary>
        /// 获取面板层级起始值
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <returns>层级起始值</returns>
        public int GetPanelLayerStartOrder(UIPanelType panelType)
        {
            switch (panelType)
            {
                case UIPanelType.Normal:
                    return normalUIStartOrder;
                case UIPanelType.Popup:
                    return popupUIStartOrder;
                case UIPanelType.System:
                    return systemUIStartOrder;
                case UIPanelType.Top:
                    return topUIStartOrder;
                default:
                    return normalUIStartOrder;
            }
        }
        
        /// <summary>
        /// 验证配置
        /// </summary>
        /// <returns>是否有效</returns>
        public bool ValidateConfig()
        {
            // 检查路径配置
            if (string.IsNullOrEmpty(uiPrefabRootPath))
            {
                Debug.LogError("[UI配置] UI预制体根路径不能为空");
                return false;
            }
            
            // 检查层级配置
            if (normalUIStartOrder >= popupUIStartOrder ||
                popupUIStartOrder >= systemUIStartOrder ||
                systemUIStartOrder >= topUIStartOrder)
            {
                Debug.LogError("[UI配置] UI层级配置错误，层级值必须递增");
                return false;
            }
            
            // 检查动画时间配置
            if (defaultAnimationDuration <= 0 ||
                fadeAnimationDuration <= 0 ||
                scaleAnimationDuration <= 0 ||
                slideAnimationDuration <= 0)
            {
                Debug.LogError("[UI配置] 动画时间配置错误，时间必须大于0");
                return false;
            }
            
            // 检查面板配置
            var panelNames = new HashSet<string>();
            foreach (var config in panelConfigs)
            {
                if (string.IsNullOrEmpty(config.panelName))
                {
                    Debug.LogError("[UI配置] 面板名称不能为空");
                    return false;
                }
                
                if (panelNames.Contains(config.panelName))
                {
                    Debug.LogError($"[UI配置] 重复的面板名称: {config.panelName}");
                    return false;
                }
                
                panelNames.Add(config.panelName);
            }
            
            return true;
        }
        
        #endregion
        
        #region 编辑器方法
        
        #if UNITY_EDITOR
        
        /// <summary>
        /// 重置为默认配置
        /// </summary>
        [ContextMenu("重置为默认配置")]
        public void ResetToDefault()
        {
            uiPrefabRootPath = "UI/Prefabs/";
            uiResourceRootPath = "UI/Resources/";
            enableUIAudio = true;
            enableUIAnimation = true;
            enableDebugMode = false;
            
            defaultAnimationDuration = 0.3f;
            fadeAnimationDuration = 0.3f;
            scaleAnimationDuration = 0.3f;
            slideAnimationDuration = 0.3f;
            useEasing = true;
            
            normalUIStartOrder = 0;
            popupUIStartOrder = 1000;
            systemUIStartOrder = 2000;
            topUIStartOrder = 3000;
            layerOrderInterval = 10;
            
            defaultAudioVolume = 1.0f;
            
            maxVisiblePanels = 10;
            panelPoolSize = 5;
            enablePanelPreload = true;
            
            enableEscapeBack = true;
            enableClickOutsideToClose = true;
            enableDoubleClickPrevention = true;
            doubleClickPreventionTime = 0.3f;
            
            showPanelBorders = false;
            showLayerInfo = false;
            enableVerboseLogging = false;
            
            panelConfigs.Clear();
            preloadPanels.Clear();
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
        
        /// <summary>
        /// 自动生成面板配置
        /// </summary>
        [ContextMenu("自动生成面板配置")]
        public void AutoGeneratePanelConfigs()
        {
            panelConfigs.Clear();
            
            // 添加常用面板配置
            panelConfigs.Add(new UIPanelConfig
            {
                panelName = "MainMenu",
                prefabPath = "MainMenuPanel",
                panelType = UIPanelType.Normal,
                animationType = UIAnimationType.Fade,
                isModal = false,
                destroyOnClose = false
            });
            
            panelConfigs.Add(new UIPanelConfig
            {
                panelName = "Settings",
                prefabPath = "SettingsPanel",
                panelType = UIPanelType.Popup,
                animationType = UIAnimationType.Scale,
                isModal = true,
                destroyOnClose = false
            });
            
            panelConfigs.Add(new UIPanelConfig
            {
                panelName = "MessageBox",
                prefabPath = "MessageBoxPanel",
                panelType = UIPanelType.Popup,
                animationType = UIAnimationType.Scale,
                isModal = true,
                destroyOnClose = true
            });
            
            panelConfigs.Add(new UIPanelConfig
            {
                panelName = "Shop",
                prefabPath = "ShopPanel",
                panelType = UIPanelType.Normal,
                animationType = UIAnimationType.SlideFromRight,
                isModal = false,
                destroyOnClose = false
            });
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
        
        #endif
        
        #endregion
    }
    
    /// <summary>
    /// UI面板配置
    /// </summary>
    [System.Serializable]
    public class UIPanelConfig
    {
        [Header("基本信息")]
        [Tooltip("面板名称")]
        public string panelName;
        
        [Tooltip("预制体路径")]
        public string prefabPath;
        
        [Tooltip("面板类型")]
        public UIPanelType panelType = UIPanelType.Normal;
        
        [Header("显示设置")]
        [Tooltip("动画类型")]
        public UIAnimationType animationType = UIAnimationType.Fade;
        
        [Tooltip("是否为模态面板")]
        public bool isModal = false;
        
        [Tooltip("关闭时是否销毁")]
        public bool destroyOnClose = false;
        
        [Tooltip("是否缓存")]
        public bool enableCache = true;
        
        [Header("高级设置")]
        [Tooltip("自定义层级偏移")]
        public int customLayerOffset = 0;
        
        [Tooltip("是否允许多实例")]
        public bool allowMultipleInstances = false;
        
        [Tooltip("是否在启动时预加载")]
        public bool preloadOnStart = false;
    }
}