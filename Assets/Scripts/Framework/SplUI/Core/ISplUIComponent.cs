using System;

namespace Framework.SplUI.Core
{
    /// <summary>
    /// SplUI组件接口
    /// 定义UI组件的基本行为，用于扩展UI面板功能
    /// 
    /// 设计理念：
    /// 1. 多态性：支持不同类型组件的统一管理
    /// 2. 扩展性：允许第三方开发者创建自定义组件
    /// 3. 灵活性：不强制继承特定基类，支持多种实现方式
    /// 4. 契约性：定义组件必须遵循的行为规范
    /// </summary>
    public interface ISplUIComponent
    {
        /// <summary>
        /// 组件所属的UI面板
        /// </summary>
        SplUIBase OwnerPanel { get; }
        
        /// <summary>
        /// 组件是否已初始化
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// 初始化组件
        /// 注意：这里提供两个重载版本以支持不同的初始化方式
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 初始化组件（带面板参数）
        /// </summary>
        /// <param name="ownerPanel">所属面板</param>
        void Initialize(SplUIBase ownerPanel);
        
        /// <summary>
        /// 更新组件（每帧调用）
        /// 简化版本，不需要deltaTime参数，组件内部可以使用Time.deltaTime
        /// </summary>
        void OnUpdate();
        
        /// <summary>
        /// 销毁组件时调用
        /// 用于清理资源、取消订阅事件等清理工作
        /// </summary>
        void OnDestroy();
        
        /// <summary>
        /// 设置所属面板
        /// 用于动态绑定组件到面板
        /// </summary>
        /// <param name="panel">面板实例</param>
        void SetOwnerPanel(SplUIBase panel);
    }
}