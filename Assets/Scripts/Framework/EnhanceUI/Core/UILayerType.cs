using System;

namespace Framework.EnhanceUI.Core
{
    /// <summary>
    /// UI层级类型枚举
    /// 定义了UI在渲染层级中的位置，数值越大层级越高
    /// </summary>
    public enum UILayerType
    {
        /// <summary>
        /// 背景层 - 用于背景UI，如主界面背景
        /// </summary>
        Background = 0,
        
        /// <summary>
        /// 底层 - 用于基础UI，如HUD、状态栏
        /// </summary>
        Bottom = 100,
        
        /// <summary>
        /// 普通层 - 用于常规UI面板，如菜单、设置
        /// </summary>
        Normal = 200,
        
        /// <summary>
        /// 弹窗层 - 用于弹窗UI，如对话框、确认框
        /// </summary>
        Popup = 300,
        
        /// <summary>
        /// 系统层 - 用于系统级UI，如加载界面、网络提示
        /// </summary>
        System = 400,
        
        /// <summary>
        /// 顶层 - 用于最高优先级UI，如错误提示、强制更新
        /// </summary>
        Top = 500,
        
        /// <summary>
        /// 调试层 - 用于开发调试UI，如调试面板、性能监控
        /// </summary>
        Debug = 600
    }
    
    /// <summary>
    /// UI层级类型扩展方法
    /// </summary>
    public static class UILayerTypeExtensions
    {
        /// <summary>
        /// 获取层级的排序顺序值
        /// </summary>
        /// <param name="layerType">层级类型</param>
        /// <returns>排序顺序值</returns>
        public static int GetSortingOrder(this UILayerType layerType)
        {
            return (int)layerType;
        }
        
        /// <summary>
        /// 获取层级的显示名称
        /// </summary>
        /// <param name="layerType">层级类型</param>
        /// <returns>显示名称</returns>
        public static string GetDisplayName(this UILayerType layerType)
        {
            return layerType switch
            {
                UILayerType.Background => "背景层",
                UILayerType.Bottom => "底层",
                UILayerType.Normal => "普通层",
                UILayerType.Popup => "弹窗层",
                UILayerType.System => "系统层",
                UILayerType.Top => "顶层",
                UILayerType.Debug => "调试层",
                _ => layerType.ToString()
            };
        }
        
        /// <summary>
        /// 判断是否为弹窗类型的层级
        /// </summary>
        /// <param name="layerType">层级类型</param>
        /// <returns>是否为弹窗类型</returns>
        public static bool IsPopupLayer(this UILayerType layerType)
        {
            return layerType == UILayerType.Popup || 
                   layerType == UILayerType.System || 
                   layerType == UILayerType.Top;
        }
        
        /// <summary>
        /// 判断是否为系统级层级
        /// </summary>
        /// <param name="layerType">层级类型</param>
        /// <returns>是否为系统级</returns>
        public static bool IsSystemLayer(this UILayerType layerType)
        {
            return layerType == UILayerType.System || 
                   layerType == UILayerType.Top || 
                   layerType == UILayerType.Debug;
        }
    }
}