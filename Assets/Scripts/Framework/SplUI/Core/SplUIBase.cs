using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.SplUI.Core
{
    /// <summary>
    /// SplUI面板基类
    /// 极简UI框架的核心抽象类，提供UI面板的基础实现和生命周期管理
    /// 所有UI面板都应该继承此抽象类
    /// </summary>
    public abstract class SplUIBase : MonoBehaviour
    {
        #region 字段和属性
        
        [Header("面板基础配置")]
        [SerializeField] protected string panelId;
        [SerializeField] protected string displayName;
        [SerializeField] protected SplUIType panelType = SplUIType.Normal;
        [SerializeField] protected int priority = 0;
        
        [Header("组件引用")]
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected RectTransform rectTransform;
        
        /// <summary>
        /// 面板唯一标识
        /// </summary>
        public string PanelId => panelId;
        
        /// <summary>
        /// 面板显示名称
        /// </summary>
        public string DisplayName => displayName;
        
        /// <summary>
        /// 面板类型
        /// </summary>
        public SplUIType PanelType => panelType;
        
        /// <summary>
        /// 是否正在显示
        /// </summary>
        public bool IsShowing { get; private set; }
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// 面板优先级（数值越大优先级越高）
        /// </summary>
        public int Priority => priority;
        
        /// <summary>
        /// 当前传入的数据
        /// </summary>
        protected object currentData;
        
        /// <summary>
        /// 组件字典
        /// </summary>
        protected Dictionary<Type, ISplUIComponent> components = new Dictionary<Type, ISplUIComponent>();
        
        /// <summary>
        /// 组件列表
        /// </summary>
        private readonly List<ISplUIComponent> componentList = new List<ISplUIComponent>();
        
        /// <summary>
        /// 动画组件
        /// </summary>
        private SplUIAnimationComponent animationComponent;
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 面板显示完成事件
        /// </summary>
        public event Action<SplUIBase> OnShowCompleted;
        
        /// <summary>
        /// 面板隐藏完成事件
        /// </summary>
        public event Action<SplUIBase> OnHideCompleted;
        
        /// <summary>
        /// 面板销毁事件
        /// </summary>
        public event Action<SplUIBase> OnDestroyed;
        
        #endregion
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法
        /// </summary>
        protected virtual void Awake()
        {
            // 自动获取必要组件
            InitializeComponents();
            
            // 自动设置面板ID
            if (string.IsNullOrEmpty(panelId))
                panelId = GetType().Name;
            
            // 自动设置显示名称
            if (string.IsNullOrEmpty(displayName))
                displayName = panelId;
        }
        
        /// <summary>
        /// Unity Start方法
        /// </summary>
        protected virtual void Start()
        {
            // 初始状态设为隐藏
            if (!IsShowing)
            {
                SetVisibility(false);
            }
        }
        
        /// <summary>
        /// Unity生命周期：Update
        /// </summary>
        protected virtual void Update()
        {
            // 更新所有组件
            UpdateComponents();
            
            // 子类可重写此方法添加更新逻辑
        }
        
        /// <summary>
        /// 更新所有组件
        /// </summary>
        private void UpdateComponents()
        {
            for (int i = 0; i < componentList.Count; i++)
            {
                if (componentList[i] != null && componentList[i].IsInitialized)
                {
                    componentList[i].OnUpdate();
                }
            }
        }
        
        /// <summary>
        /// Unity OnDestroy方法
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 销毁所有组件
            DestroyAllComponents();
            
            // 触发销毁事件
            OnDestroyed?.Invoke(this);
            
            // 调用子类销毁逻辑
            OnPanelDestroy();
        }
        
        /// <summary>
        /// 销毁所有组件
        /// </summary>
        private void DestroyAllComponents()
        {
            // 销毁所有组件
            for (int i = 0; i < componentList.Count; i++)
            {
                if (componentList[i] != null)
                {
                    componentList[i].OnDestroy();
                }
            }
            
            components.Clear();
            componentList.Clear();
        }
        
        #endregion
        
        #region 核心API
        
        /// <summary>
        /// 初始化面板
        /// </summary>
        public virtual void Initialize()
        {
            if (IsInitialized)
                return;
            
            // 初始化动画组件
            InitializeAnimationComponent();
            
            // 执行子类初始化
            OnInitialize();
            
            IsInitialized = true;
            
            Debug.Log($"[SplUI] 面板初始化完成: {PanelId}");
        }
        
        /// <summary>
        /// 显示面板
        /// </summary>
        /// <param name="data">传递的数据</param>
        public void Show(object data = null)
        {
            if (IsShowing)
                return;
            
            // 确保面板已初始化
            if (!IsInitialized)
                Initialize();
            
            // 保存数据
            currentData = data;
            
            // 执行显示前处理
            OnBeforeShow(data);
            
            // 设置为显示状态
            IsShowing = true;
            gameObject.SetActive(true);
            
            // 执行子类显示逻辑
            OnShow(data);
            
            // 播放显示动画
            if (animationComponent != null)
            {
                animationComponent.PlayShowAnimation(() => OnShowAnimationComplete());
            }
            else
            {
                SetVisibility(true);
                OnShowAnimationComplete();
            }
        }
        
        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void Hide()
        {
            Hide(null);
        }
        
        /// <summary>
        /// 隐藏面板（支持回调）
        /// </summary>
        /// <param name="onComplete">隐藏完成后的回调函数</param>
        public void Hide(System.Action onComplete)
        {
            if (!IsShowing)
            {
                onComplete?.Invoke();
                return;
            }
            
            // 执行隐藏前处理
            OnBeforeHide();
            
            // 执行子类隐藏逻辑
            OnHide();
            
            // 播放隐藏动画
            if (animationComponent != null)
            {
                animationComponent.PlayHideAnimation(() => 
                {
                    OnHideAnimationComplete();
                    onComplete?.Invoke();
                });
            }
            else
            {
                SetVisibility(false);
                OnHideAnimationComplete();
                onComplete?.Invoke();
            }
        }
        
        /// <summary>
        /// 销毁面板
        /// </summary>
        public virtual void DestroyPanel()
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 刷新面板数据
        /// </summary>
        /// <param name="data">新数据</param>
        public void Refresh(object data = null)
        {
            if (data != null)
                currentData = data;
            
            OnRefresh(currentData);
        }
        
        /// <summary>
        /// 停止当前动画
        /// </summary>
        public void StopCurrentAnimation()
        {
            if (animationComponent != null)
            {
                animationComponent.StopCurrentAnimation();
            }
        }
        
        /// <summary>
        /// 设置可见性
        /// </summary>
        /// <param name="visible">是否可见</param>
        public void SetVisibility(bool visible)
        {
            if (canvasGroup == null) return;
            
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            
            if (!visible)
            {
                gameObject.SetActive(false);
            }
        }
        
        #endregion
        
        #region 动画系统
        
        /// <summary>
        /// 初始化动画组件
        /// </summary>
        private void InitializeAnimationComponent()
        {
            if (animationComponent == null)
            {
                animationComponent = AddComponent<SplUIAnimationComponent>();
            }
        }
        
        /// <summary>
        /// 获取动画组件
        /// </summary>
        /// <returns>动画组件实例</returns>
        public SplUIAnimationComponent GetAnimationComponent()
        {
            if (animationComponent == null)
            {
                InitializeAnimationComponent();
            }
            return animationComponent;
        }
        
        /// <summary>
        /// 设置显示动画
        /// </summary>
        /// <param name="animationType">动画类型</param>
        /// <param name="duration">动画持续时间</param>
        public void SetShowAnimation(SplUIAnimationType animationType, float duration = 0.3f)
        {
            var anim = GetAnimationComponent();
            anim.ShowAnimation = animationType;
            anim.AnimationDuration = duration;
        }
        
        /// <summary>
        /// 设置隐藏动画
        /// </summary>
        /// <param name="animationType">动画类型</param>
        /// <param name="duration">动画持续时间</param>
        public void SetHideAnimation(SplUIAnimationType animationType, float duration = 0.3f)
        {
            var anim = GetAnimationComponent();
            anim.HideAnimation = animationType;
            anim.AnimationDuration = duration;
        }
        
        #endregion
        
        #region 组件系统
        
        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>组件实例</returns>
        public T AddComponent<T>() where T : class, ISplUIComponent, new()
        {
            Type componentType = typeof(T);
            
            if (components.ContainsKey(componentType))
            {
                return components[componentType] as T;
            }
            
            T component = new T();
            component.Initialize(this);
            components[componentType] = component;
            componentList.Add(component);
            
            return component;
        }
        
        /// <summary>
        /// 注册组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="component">组件实例</param>
        public void RegisterComponent<T>(T component) where T : ISplUIComponent
        {
            System.Type type = typeof(T);
            if (components.ContainsKey(type))
            {
                Debug.LogWarning($"[SplUIBase] 组件类型 {type.Name} 已存在", this);
                return;
            }
            
            components[type] = component;
            componentList.Add(component);
            component.SetOwnerPanel(this);
            
            if (!component.IsInitialized)
            {
                component.Initialize();
            }
        }
        
        /// <summary>
        /// 注册组件（通过实例）
        /// </summary>
        /// <param name="component">组件实例</param>
        public void RegisterComponent(ISplUIComponent component)
        {
            if (component == null)
                return;
            
            System.Type type = component.GetType();
            if (components.ContainsKey(type))
            {
                Debug.LogWarning($"[SplUIBase] 组件类型 {type.Name} 已存在", this);
                return;
            }
            
            components[type] = component;
            componentList.Add(component);
            component.SetOwnerPanel(this);
            
            if (!component.IsInitialized)
            {
                component.Initialize();
            }
        }
        
        /// <summary>
        /// 获取组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>组件实例</returns>
        public T GetComponent<T>() where T : class
        {
            Type componentType = typeof(T);
            components.TryGetValue(componentType, out ISplUIComponent component);
            return component as T;
        }
        
        /// <summary>
        /// 移除组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        public void RemoveComponent<T>() where T : class, ISplUIComponent
        {
            Type componentType = typeof(T);
            
            if (components.TryGetValue(componentType, out ISplUIComponent component))
            {
                component.OnDestroy();
                components.Remove(componentType);
            }
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponents()
        {
            // 自动获取CanvasGroup组件
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
            // 自动获取RectTransform组件
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
        }
        
        #endregion
        
        #region 虚方法 - 子类重写
        
        /// <summary>
        /// 子类初始化方法
        /// </summary>
        protected virtual void OnInitialize() { }
        
        /// <summary>
        /// 显示前处理
        /// </summary>
        /// <param name="data">传递的数据</param>
        protected virtual void OnBeforeShow(object data) { }
        
        /// <summary>
        /// 面板显示时调用（子类重写）
        /// </summary>
        /// <param name="data">传递的数据</param>
        protected virtual void OnShow(object data = null) { }
        
        /// <summary>
        /// 显示动画完成处理
        /// </summary>
        protected virtual void OnShowAnimationComplete()
        {
            OnShowCompleted?.Invoke(this);
        }
        
        /// <summary>
        /// 隐藏前处理
        /// </summary>
        protected virtual void OnBeforeHide() { }
        
        /// <summary>
        /// 面板隐藏时调用（子类重写）
        /// </summary>
        protected virtual void OnHide() { }
        
        /// <summary>
        /// 隐藏动画完成处理
        /// </summary>
        protected virtual void OnHideAnimationComplete()
        {
            IsShowing = false;
            gameObject.SetActive(false);
            OnHideCompleted?.Invoke(this);
        }
        
        /// <summary>
        /// 面板刷新时调用（子类重写）
        /// </summary>
        /// <param name="data">刷新数据</param>
        protected virtual void OnRefresh(object data) { }
        
        /// <summary>
        /// 面板销毁时调用（子类重写）
        /// </summary>
        protected virtual void OnPanelDestroy() { }
        
        #endregion
    }
}