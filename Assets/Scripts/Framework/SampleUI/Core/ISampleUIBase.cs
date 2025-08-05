using System;
using UnityEngine;

namespace Framework.SampleUI.Core
{
    /// <summary>
    /// UI面板接口
    /// 定义UI面板的基本行为和生命周期
    /// </summary>
    public interface ISampleUIBase
    {
        /// <summary>
        /// 面板唯一标识
        /// </summary>
        string PanelId { get; }
        
        /// <summary>
        /// 面板显示名称
        /// </summary>
        string DisplayName { get; }
        
        /// <summary>
        /// 面板类型
        /// </summary>
        SampleUIBaseType PanelType { get; }
        
        /// <summary>
        /// 是否正在显示
        /// </summary>
        bool IsShowing { get; }
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// 面板优先级（数值越大优先级越高）
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// 初始化面板
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 显示面板
        /// </summary>
        /// <param name="data">传递的数据</param>
        void Show(object data = null);
        
        /// <summary>
        /// 隐藏面板
        /// </summary>
        void Hide();
        
        /// <summary>
        /// 销毁面板
        /// </summary>
        void Destroy();
        
        /// <summary>
        /// 刷新面板数据
        /// </summary>
        /// <param name="data">新数据</param>
        void Refresh(object data = null);
        
        /// <summary>
        /// 面板显示完成事件
        /// </summary>
        event Action<ISampleUIBase> OnShowCompleted;
        
        /// <summary>
        /// 面板隐藏完成事件
        /// </summary>
        event Action<ISampleUIBase> OnHideCompleted;
        
        /// <summary>
        /// 面板销毁事件
        /// </summary>
        event Action<ISampleUIBase> OnDestroyed;
    }
    
    /// <summary>
    /// UI面板类型枚举
    /// </summary>
    public enum SampleUIBaseType
    {
        /// <summary>
        /// 普通面板 - 全屏显示，会替换当前面板
        /// </summary>
        Normal = 0,
        
        /// <summary>
        /// 弹窗面板 - 覆盖显示，不会替换当前面板
        /// </summary>
        Popup = 1,
        
        /// <summary>
        /// 系统面板 - 系统级面板，优先级最高
        /// </summary>
        System = 2,
        
        /// <summary>
        /// HUD面板 - 游戏内UI，常驻显示
        /// </summary>
        HUD = 3
    }
}