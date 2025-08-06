using Framework.SplUI.Core;
using UnityEngine;

namespace Framework.SplUI.Config
{
    /// <summary>
    /// SplUI框架配置类
    /// 提供UI框架的全局配置选项
    /// </summary>
    [CreateAssetMenu(fileName = "SplUIConfig", menuName = "SplUI/Config", order = 1)]
    public class SplUIConfig : ScriptableObject
    {
        [Header("基础设置")]
        [Tooltip("是否启用调试日志")]
        public bool enableDebugLog = true;
        
        [Tooltip("UI预制体路径（相对于Resources文件夹）")]
        public string uiPrefabPath = "UI/Panels/";
        
        [Tooltip("是否自动创建UI层级")]
        public bool autoCreateLayers = true;
        
        [Header("Canvas设置")]
        [Tooltip("Canvas渲染模式")]
        public RenderMode canvasRenderMode = RenderMode.ScreenSpaceOverlay;
        
        [Tooltip("Canvas排序顺序")]
        public int canvasSortingOrder = 0;
        
        [Tooltip("参考分辨率")]
        public Vector2 referenceResolution = new Vector2(1920, 1080);
        
        [Tooltip("屏幕匹配模式")]
        [Range(0f, 1f)]
        public float screenMatchMode = 0.5f;
        
        [Header("层级设置")]
        [Tooltip("普通面板层排序顺序")]
        public int normalLayerOrder = 0;
        
        [Tooltip("弹窗面板层排序顺序")]
        public int popupLayerOrder = 100;
        
        [Tooltip("系统面板层排序顺序")]
        public int systemLayerOrder = 200;
        
        [Tooltip("HUD面板层排序顺序")]
        public int hudLayerOrder = 300;
        
        [Header("动画设置")]
        [Tooltip("默认动画持续时间")]
        [Range(0.1f, 2f)]
        public float defaultAnimationDuration = 0.3f;
        
        [Tooltip("默认显示动画类型")]
        public SplUIAnimationType defaultShowAnimation = SplUIAnimationType.Fade;
        
        [Tooltip("默认隐藏动画类型")]
        public SplUIAnimationType defaultHideAnimation = SplUIAnimationType.Fade;
        
        [Header("性能设置")]
        [Tooltip("预制体缓存大小限制")]
        public int prefabCacheLimit = 50;
        
        [Tooltip("是否启用面板对象池")]
        public bool enablePanelPool = false;
        
        [Tooltip("对象池初始大小")]
        public int poolInitialSize = 5;
        
        [Header("输入设置")]
        [Tooltip("是否启用返回键支持")]
        public bool enableBackKeySupport = true;
        
        [Tooltip("返回键代码")]
        public KeyCode backKeyCode = KeyCode.Escape;
        
        [Header("安全设置")]
        [Tooltip("是否启用面板ID验证")]
        public bool enablePanelIdValidation = true;
        
        [Tooltip("允许的面板ID字符（正则表达式）")]
        public string allowedPanelIdPattern = @"^[a-zA-Z0-9_]+$";
        
        /// <summary>
        /// 获取层级排序顺序
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <returns>排序顺序</returns>
        public int GetLayerOrder(SplUIType panelType)
        {
            switch (panelType)
            {
                case SplUIType.Normal:
                    return normalLayerOrder;
                case SplUIType.Popup:
                    return popupLayerOrder;
                case SplUIType.System:
                    return systemLayerOrder;
                case SplUIType.HUD:
                    return hudLayerOrder;
                default:
                    return normalLayerOrder;
            }
        }
        
        /// <summary>
        /// 验证面板ID是否有效
        /// </summary>
        /// <param name="panelId">面板ID</param>
        /// <returns>是否有效</returns>
        public bool ValidatePanelId(string panelId)
        {
            if (!enablePanelIdValidation)
                return true;
            
            if (string.IsNullOrEmpty(panelId))
                return false;
            
            return System.Text.RegularExpressions.Regex.IsMatch(panelId, allowedPanelIdPattern);
        }
        
        /// <summary>
        /// 获取完整的预制体路径
        /// </summary>
        /// <param name="panelId">面板ID</param>
        /// <returns>完整路径</returns>
        public string GetPrefabPath(string panelId)
        {
            return uiPrefabPath + panelId;
        }
    }
}