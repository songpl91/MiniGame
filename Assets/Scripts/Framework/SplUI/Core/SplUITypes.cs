using System;

namespace Framework.SplUI.Core
{
    /// <summary>
    /// SplUI框架类型定义
    /// 包含所有UI相关的枚举、常量和类型定义
    /// </summary>
    public static class SplUITypes
    {
        #region 常量定义
        
        /// <summary>
        /// 默认动画持续时间（秒）
        /// </summary>
        public const float DEFAULT_ANIMATION_DURATION = 0.3f;
        
        /// <summary>
        /// 最小动画持续时间（秒）
        /// </summary>
        public const float MIN_ANIMATION_DURATION = 0.1f;
        
        /// <summary>
        /// 最大动画持续时间（秒）
        /// </summary>
        public const float MAX_ANIMATION_DURATION = 2.0f;
        
        /// <summary>
        /// 默认层级排序间隔
        /// </summary>
        public const int LAYER_ORDER_INTERVAL = 100;
        
        #endregion
        
        #region 事件委托定义
        
        /// <summary>
        /// 面板状态变化事件委托
        /// </summary>
        /// <param name="panelId">面板ID</param>
        /// <param name="panel">面板实例</param>
        public delegate void PanelStateChangedHandler(string panelId, SplUIBase panel);
        
        /// <summary>
        /// 动画完成事件委托
        /// </summary>
        /// <param name="animationType">动画类型</param>
        /// <param name="isShow">是否为显示动画</param>
        public delegate void AnimationCompletedHandler(SplUIAnimationType animationType, bool isShow);
        
        /// <summary>
        /// 面板数据变化事件委托
        /// </summary>
        /// <param name="panelId">面板ID</param>
        /// <param name="data">数据对象</param>
        public delegate void PanelDataChangedHandler(string panelId, object data);
        
        #endregion
    }
    
    #region 枚举定义
    
    /// <summary>
    /// UI面板类型枚举
    /// 定义不同类型面板的显示行为和层级关系
    /// </summary>
    public enum SplUIType
    {
        /// <summary>
        /// 普通面板 - 全屏显示，会替换当前面板
        /// 适用于：主界面、设置界面、商店界面等
        /// </summary>
        Normal = 0,
        
        /// <summary>
        /// 弹窗面板 - 覆盖显示，不会替换当前面板
        /// 适用于：确认对话框、提示框、小窗口等
        /// </summary>
        Popup = 1,
        
        /// <summary>
        /// 系统面板 - 系统级面板，优先级最高
        /// 适用于：错误提示、网络断线提示、系统公告等
        /// </summary>
        System = 2,
        
        /// <summary>
        /// HUD面板 - 游戏内UI，常驻显示
        /// 适用于：血条、小地图、技能栏、聊天框等
        /// </summary>
        HUD = 3
    }
    
    /// <summary>
    /// UI动画类型枚举
    /// 定义面板显示和隐藏时的动画效果
    /// </summary>
    public enum SplUIAnimationType
    {
        /// <summary>
        /// 无动画 - 立即显示/隐藏
        /// 适用于：需要快速响应的界面
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 淡入淡出 - 透明度渐变
        /// 适用于：大部分界面的通用动画
        /// </summary>
        Fade = 1,
        
        /// <summary>
        /// 缩放 - 从小到大或从大到小
        /// 适用于：弹窗、按钮反馈等
        /// </summary>
        Scale = 2,
        
        /// <summary>
        /// 淡入淡出+缩放组合 - 同时进行透明度和缩放变化
        /// 适用于：重要弹窗、奖励界面等
        /// </summary>
        FadeScale = 3,
        
        /// <summary>
        /// 通用滑动 - 方向由SlideDirection控制
        /// 适用于：需要自定义滑动方向的界面
        /// </summary>
        Slide = 4,
        
        /// <summary>
        /// 从底部滑入 - 从屏幕底部向上滑入
        /// 适用于：底部菜单、操作面板等
        /// </summary>
        SlideFromBottom = 5,
        
        /// <summary>
        /// 滑出到底部 - 向下滑出到屏幕底部
        /// 适用于：底部菜单、操作面板的隐藏
        /// </summary>
        SlideToBottom = 6,
        
        /// <summary>
        /// 从顶部滑入 - 从屏幕顶部向下滑入
        /// 适用于：通知栏、顶部菜单等
        /// </summary>
        SlideFromTop = 7,
        
        /// <summary>
        /// 滑出到顶部 - 向上滑出到屏幕顶部
        /// 适用于：通知栏、顶部菜单的隐藏
        /// </summary>
        SlideToTop = 8,
        
        /// <summary>
        /// 从左侧滑入 - 从屏幕左侧向右滑入
        /// 适用于：侧边栏、抽屉菜单等
        /// </summary>
        SlideFromLeft = 9,
        
        /// <summary>
        /// 滑出到左侧 - 向左滑出到屏幕左侧
        /// 适用于：侧边栏、抽屉菜单的隐藏
        /// </summary>
        SlideToLeft = 10,
        
        /// <summary>
        /// 从右侧滑入 - 从屏幕右侧向左滑入
        /// 适用于：右侧边栏、设置面板等
        /// </summary>
        SlideFromRight = 11,
        
        /// <summary>
        /// 滑出到右侧 - 向右滑出到屏幕右侧
        /// 适用于：右侧边栏、设置面板的隐藏
        /// </summary>
        SlideToRight = 12
    }
    
    /// <summary>
    /// 滑动方向枚举
    /// 用于通用滑动动画的方向控制
    /// </summary>
    public enum SplUISlideDirection
    {
        /// <summary>
        /// 向左滑动
        /// </summary>
        Left = 0,
        
        /// <summary>
        /// 向右滑动
        /// </summary>
        Right = 1,
        
        /// <summary>
        /// 向上滑动
        /// </summary>
        Top = 2,
        
        /// <summary>
        /// 向下滑动
        /// </summary>
        Bottom = 3
    }
    
    /// <summary>
    /// 面板状态枚举
    /// 定义面板的当前状态
    /// </summary>
    public enum SplUIPanelState
    {
        /// <summary>
        /// 未初始化状态
        /// </summary>
        Uninitialized = 0,
        
        /// <summary>
        /// 已初始化但未显示
        /// </summary>
        Initialized = 1,
        
        /// <summary>
        /// 正在显示动画中
        /// </summary>
        Showing = 2,
        
        /// <summary>
        /// 已显示状态
        /// </summary>
        Shown = 3,
        
        /// <summary>
        /// 正在隐藏动画中
        /// </summary>
        Hiding = 4,
        
        /// <summary>
        /// 已隐藏状态
        /// </summary>
        Hidden = 5,
        
        /// <summary>
        /// 已销毁状态
        /// </summary>
        Destroyed = 6
    }
    
    /// <summary>
    /// 动画缓动类型枚举
    /// 定义动画的缓动效果
    /// </summary>
    public enum SplUIEaseType
    {
        /// <summary>
        /// 线性缓动 - 匀速运动
        /// </summary>
        Linear = 0,
        
        /// <summary>
        /// 缓入 - 慢速开始
        /// </summary>
        EaseIn = 1,
        
        /// <summary>
        /// 缓出 - 慢速结束
        /// </summary>
        EaseOut = 2,
        
        /// <summary>
        /// 缓入缓出 - 慢速开始和结束
        /// </summary>
        EaseInOut = 3,
        
        /// <summary>
        /// 弹性缓动 - 带有弹性效果
        /// </summary>
        Elastic = 4,
        
        /// <summary>
        /// 反弹缓动 - 带有反弹效果
        /// </summary>
        Bounce = 5,
        
        /// <summary>
        /// 回退缓动 - 先反向再正向
        /// </summary>
        Back = 6
    }
    
    /// <summary>
    /// 层级类型枚举
    /// 定义不同类型面板的层级范围
    /// </summary>
    public enum SplUILayerType
    {
        /// <summary>
        /// 背景层 - 最底层
        /// 层级范围：0-99
        /// </summary>
        Background = 0,
        
        /// <summary>
        /// 普通UI层 - 常规界面
        /// 层级范围：100-199
        /// </summary>
        Normal = 100,
        
        /// <summary>
        /// HUD层 - 游戏内UI
        /// 层级范围：200-299
        /// </summary>
        HUD = 200,
        
        /// <summary>
        /// 弹窗层 - 弹出窗口
        /// 层级范围：300-399
        /// </summary>
        Popup = 300,
        
        /// <summary>
        /// 系统层 - 系统级界面
        /// 层级范围：400-499
        /// </summary>
        System = 400,
        
        /// <summary>
        /// 顶层 - 最高优先级
        /// 层级范围：500+
        /// </summary>
        Top = 500
    }
    
    #endregion
    
    #region 结构体定义
    
    /// <summary>
    /// 动画配置结构体
    /// 包含动画的所有配置参数
    /// </summary>
    [Serializable]
    public struct SplUIAnimationConfig
    {
        /// <summary>
        /// 显示动画类型
        /// </summary>
        public SplUIAnimationType showAnimation;
        
        /// <summary>
        /// 隐藏动画类型
        /// </summary>
        public SplUIAnimationType hideAnimation;
        
        /// <summary>
        /// 动画持续时间
        /// </summary>
        public float duration;
        
        /// <summary>
        /// 缓动类型
        /// </summary>
        public SplUIEaseType easeType;
        
        /// <summary>
        /// 滑动方向（仅在使用Slide动画时有效）
        /// </summary>
        public SplUISlideDirection slideDirection;
        
        /// <summary>
        /// 是否忽略时间缩放
        /// </summary>
        public bool ignoreTimeScale;
        
        /// <summary>
        /// 创建默认动画配置
        /// </summary>
        /// <returns>默认配置</returns>
        public static SplUIAnimationConfig Default => new SplUIAnimationConfig
        {
            showAnimation = SplUIAnimationType.Fade,
            hideAnimation = SplUIAnimationType.Fade,
            duration = SplUITypes.DEFAULT_ANIMATION_DURATION,
            easeType = SplUIEaseType.EaseOut,
            slideDirection = SplUISlideDirection.Bottom,
            ignoreTimeScale = false
        };
    }
    
    /// <summary>
    /// 面板信息结构体
    /// 包含面板的基本信息
    /// </summary>
    [Serializable]
    public struct SplUIPanelInfo
    {
        /// <summary>
        /// 面板ID
        /// </summary>
        public string panelId;
        
        /// <summary>
        /// 显示名称
        /// </summary>
        public string displayName;
        
        /// <summary>
        /// 面板类型
        /// </summary>
        public SplUIType panelType;
        
        /// <summary>
        /// 层级排序
        /// </summary>
        public int sortingOrder;
        
        /// <summary>
        /// 是否为单例面板
        /// </summary>
        public bool isSingleton;
        
        /// <summary>
        /// 是否缓存面板
        /// </summary>
        public bool isCache;
        
        /// <summary>
        /// 预制体路径
        /// </summary>
        public string prefabPath;
    }
    
    #endregion
}