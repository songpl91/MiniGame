using System;

namespace Framework.SampleUI.Core
{
    /// <summary>
    /// UI组件接口
    /// 定义UI组件的基本行为，用于扩展UI面板功能
    /// </summary>
    public interface ISampleUIComponent
    {
        /// <summary>
        /// 组件名称
        /// </summary>
        string ComponentName { get; }
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// 关联的UI面板
        /// </summary>
        ISampleUIBase OwnerPanel { get; }
        
        /// <summary>
        /// 初始化组件
        /// </summary>
        /// <param name="panel">所属面板</param>
        void Initialize(ISampleUIBase panel);
        
        /// <summary>
        /// 组件更新
        /// </summary>
        /// <param name="deltaTime">时间间隔</param>
        void OnUpdate(float deltaTime);
        
        /// <summary>
        /// 面板显示时调用
        /// </summary>
        void OnPanelShow();
        
        /// <summary>
        /// 面板隐藏时调用
        /// </summary>
        void OnPanelHide();
        
        /// <summary>
        /// 销毁组件
        /// </summary>
        void OnDestroy();
    }
    
    /// <summary>
    /// UI组件基类
    /// 提供UI组件的基础实现
    /// </summary>
    public abstract class SampleUIComponent : ISampleUIComponent
    {
        #region 字段和属性
        
        /// <summary>
        /// 组件名称
        /// </summary>
        public virtual string ComponentName => GetType().Name;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// 关联的UI面板
        /// </summary>
        public ISampleUIBase OwnerPanel { get; private set; }
        
        #endregion
        
        #region 接口实现
        
        /// <summary>
        /// 初始化组件
        /// </summary>
        /// <param name="panel">所属面板</param>
        public virtual void Initialize(ISampleUIBase panel)
        {
            if (IsInitialized)
                return;
            
            OwnerPanel = panel;
            
            // 注册面板事件
            if (panel != null)
            {
                panel.OnShowCompleted += OnPanelShowCompleted;
                panel.OnHideCompleted += OnPanelHideCompleted;
            }
            
            // 执行子类初始化
            OnInitialize();
            
            IsInitialized = true;
        }
        
        /// <summary>
        /// 组件更新
        /// </summary>
        /// <param name="deltaTime">时间间隔</param>
        public virtual void OnUpdate(float deltaTime)
        {
            // 子类可重写
        }
        
        /// <summary>
        /// 面板显示时调用
        /// </summary>
        public virtual void OnPanelShow()
        {
            // 子类可重写
        }
        
        /// <summary>
        /// 面板隐藏时调用
        /// </summary>
        public virtual void OnPanelHide()
        {
            // 子类可重写
        }
        
        /// <summary>
        /// 销毁组件
        /// </summary>
        public virtual void OnDestroy()
        {
            // 注销面板事件
            if (OwnerPanel != null)
            {
                OwnerPanel.OnShowCompleted -= OnPanelShowCompleted;
                OwnerPanel.OnHideCompleted -= OnPanelHideCompleted;
            }
            
            // 执行子类销毁
            OnDestroyed();
            
            IsInitialized = false;
            OwnerPanel = null;
        }
        
        #endregion
        
        #region 事件处理
        
        /// <summary>
        /// 面板显示完成事件处理
        /// </summary>
        /// <param name="panel">面板实例</param>
        private void OnPanelShowCompleted(ISampleUIBase panel)
        {
            OnPanelShow();
        }
        
        /// <summary>
        /// 面板隐藏完成事件处理
        /// </summary>
        /// <param name="panel">面板实例</param>
        private void OnPanelHideCompleted(ISampleUIBase panel)
        {
            OnPanelHide();
        }
        
        #endregion
        
        #region 虚方法 - 子类重写
        
        /// <summary>
        /// 子类初始化方法
        /// </summary>
        protected virtual void OnInitialize() { }
        
        /// <summary>
        /// 子类销毁方法
        /// </summary>
        protected virtual void OnDestroyed() { }
        
        #endregion
    }
}