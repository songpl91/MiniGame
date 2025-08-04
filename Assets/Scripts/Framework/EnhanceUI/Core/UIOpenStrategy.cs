using System;

namespace Framework.EnhanceUI.Core
{
    /// <summary>
    /// UI打开策略枚举
    /// 定义了UI面板的多开行为策略
    /// </summary>
    public enum UIOpenStrategy
    {
        /// <summary>
        /// 单例模式 - 同时只能存在一个实例，新打开会关闭旧的
        /// </summary>
        Single,
        
        /// <summary>
        /// 多开模式 - 可以同时存在多个实例
        /// </summary>
        Multiple,
        
        /// <summary>
        /// 限制多开 - 可以存在多个实例，但有数量限制
        /// </summary>
        Limited,
        
        /// <summary>
        /// 栈模式 - 新打开的会压入栈顶，关闭时按栈顺序
        /// </summary>
        Stack,
        
        /// <summary>
        /// 队列模式 - 如果已有实例在打开，新的请求会排队等待
        /// </summary>
        Queue
    }
    
    /// <summary>
    /// UI动画类型枚举
    /// </summary>
    public enum UIAnimationType
    {
        /// <summary>
        /// 无动画
        /// </summary>
        None,
        
        /// <summary>
        /// 淡入淡出
        /// </summary>
        Fade,
        
        /// <summary>
        /// 缩放动画
        /// </summary>
        Scale,
        
        /// <summary>
        /// 从左滑入
        /// </summary>
        SlideFromLeft,
        
        /// <summary>
        /// 从右滑入
        /// </summary>
        SlideFromRight,
        
        /// <summary>
        /// 从上滑入
        /// </summary>
        SlideFromTop,
        
        /// <summary>
        /// 从下滑入
        /// </summary>
        SlideFromBottom,
        
        /// <summary>
        /// 自定义动画
        /// </summary>
        Custom
    }
    
    /// <summary>
    /// UI加载模式枚举
    /// </summary>
    public enum UILoadMode
    {
        /// <summary>
        /// 同步加载 - 立即加载并返回结果
        /// </summary>
        Sync,
        
        /// <summary>
        /// 异步加载 - 异步加载，通过回调返回结果
        /// </summary>
        Async
    }
    
    /// <summary>
    /// UI状态枚举
    /// </summary>
    public enum UIState
    {
        /// <summary>
        /// 未初始化
        /// </summary>
        None,
        
        /// <summary>
        /// 加载中
        /// </summary>
        Loading,
        
        /// <summary>
        /// 已加载但未显示
        /// </summary>
        Loaded,
        
        /// <summary>
        /// 显示中
        /// </summary>
        Showing,
        
        /// <summary>
        /// 已显示
        /// </summary>
        Shown,
        
        /// <summary>
        /// 隐藏中
        /// </summary>
        Hiding,
        
        /// <summary>
        /// 已隐藏
        /// </summary>
        Hidden,
        
        /// <summary>
        /// 销毁中
        /// </summary>
        Destroying,
        
        /// <summary>
        /// 已销毁
        /// </summary>
        Destroyed,
        
        /// <summary>
        /// 错误状态
        /// </summary>
        Error
    }
    
    /// <summary>
    /// UI打开策略扩展方法
    /// </summary>
    public static class UIOpenStrategyExtensions
    {
        /// <summary>
        /// 获取策略的显示名称
        /// </summary>
        /// <param name="strategy">打开策略</param>
        /// <returns>显示名称</returns>
        public static string GetDisplayName(this UIOpenStrategy strategy)
        {
            return strategy switch
            {
                UIOpenStrategy.Single => "单例模式",
                UIOpenStrategy.Multiple => "多开模式",
                UIOpenStrategy.Limited => "限制多开",
                UIOpenStrategy.Stack => "栈模式",
                UIOpenStrategy.Queue => "队列模式",
                _ => strategy.ToString()
            };
        }
        
        /// <summary>
        /// 判断是否支持多实例
        /// </summary>
        /// <param name="strategy">打开策略</param>
        /// <returns>是否支持多实例</returns>
        public static bool SupportsMultipleInstances(this UIOpenStrategy strategy)
        {
            return strategy == UIOpenStrategy.Multiple || 
                   strategy == UIOpenStrategy.Limited || 
                   strategy == UIOpenStrategy.Stack;
        }
        
        /// <summary>
        /// 判断是否需要排队处理
        /// </summary>
        /// <param name="strategy">打开策略</param>
        /// <returns>是否需要排队</returns>
        public static bool RequiresQueue(this UIOpenStrategy strategy)
        {
            return strategy == UIOpenStrategy.Queue;
        }
    }
}