using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.SampleUI.Core
{
    /// <summary>
    /// UI面板基类
    /// 提供UI面板的基础实现和生命周期管理
    /// </summary>
    public abstract class SampleUIBase : MonoBehaviour, ISampleUIBase
    {
        #region 字段和属性
        
        [Header("面板配置")]
        [SerializeField] protected string panelId;
        [SerializeField] protected string displayName;
        [SerializeField] protected SampleUIBaseType panelType = SampleUIBaseType.Normal;
        [SerializeField] protected int priority = 0;
        
        [Header("动画配置")]
        [SerializeField] protected SampleUIAnimationType showAnimation = SampleUIAnimationType.None;
        [SerializeField] protected SampleUIAnimationType hideAnimation = SampleUIAnimationType.None;
        [SerializeField] protected float animationDuration = 0.3f;
        
        [Header("组件引用")]
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected RectTransform contentRoot;
        
        // 接口属性实现
        public string PanelId => panelId;
        public string DisplayName => displayName;
        public SampleUIBaseType PanelType => panelType;
        public bool IsShowing { get; private set; }
        public bool IsInitialized { get; private set; }
        public int Priority => priority;
        
        // 面板数据
        protected object currentData;
        
        // 组件系统
        protected Dictionary<Type, ISampleUIComponent> components = new Dictionary<Type, ISampleUIComponent>();
        
        // 动画相关
        private Coroutine currentAnimation;
        private Vector3 originalScale;
        private Vector2 originalPosition;
        
        #endregion
        
        #region 事件
        
        public event Action<ISampleUIBase> OnShowCompleted;
        public event Action<ISampleUIBase> OnHideCompleted;
        public event Action<ISampleUIBase> OnDestroyed;
        
        #endregion
        
        #region Unity生命周期
        
        protected virtual void Awake()
        {
            // 自动获取组件
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
            if (contentRoot == null)
                contentRoot = GetComponent<RectTransform>();
            
            // 记录原始变换信息
            originalScale = transform.localScale;
            originalPosition = contentRoot.anchoredPosition;
            
            // 如果没有设置面板ID，使用类名
            if (string.IsNullOrEmpty(panelId))
                panelId = GetType().Name;
            
            // 如果没有设置显示名称，使用面板ID
            if (string.IsNullOrEmpty(displayName))
                displayName = panelId;
        }
        
        protected virtual void Start()
        {
            // 初始状态设为隐藏
            if (!IsShowing)
            {
                SetVisibility(false, false);
            }
        }
        
        protected virtual void OnDestroy()
        {
            // 停止当前动画
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            
            // 清理组件
            foreach (var component in components.Values)
            {
                component?.OnDestroy();
            }
            components.Clear();
            
            // 触发销毁事件
            OnDestroyed?.Invoke(this);
        }
        
        #endregion
        
        #region 接口实现
        
        /// <summary>
        /// 初始化面板
        /// </summary>
        public virtual void Initialize()
        {
            if (IsInitialized)
                return;
            
            // 执行子类初始化
            OnInitialize();
            
            // 初始化组件
            InitializeComponents();
            
            IsInitialized = true;
            
            Debug.Log($"[SampleUI] 面板初始化完成: {PanelId}");
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
            
            // 执行显示前的处理
            OnBeforeShow(data);

            // 执行显示方法
            OnShow(data);
            
            // 设置为显示状态
            IsShowing = true;
            gameObject.SetActive(true);
            
            // 播放显示动画
            if (showAnimation != SampleUIAnimationType.None)
            {
                PlayShowAnimation();
            }
            else
            {
                SetVisibility(true, false);
                OnShowComplete();
            }
        }
        
        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void Hide()
        {
            if (!IsShowing)
                return;
            
            // 执行隐藏前的处理
            OnBeforeHide();

            // 执行隐藏方法
            OnHide();

            // 播放隐藏动画
            if (hideAnimation != SampleUIAnimationType.None)
            {
                PlayHideAnimation();
            }
            else
            {
                SetVisibility(false, false);
                OnHideComplete();
            }
        }
        
        /// <summary>
        /// 销毁面板
        /// </summary>
        public virtual void Destroy()
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
        
        #endregion
        
        #region 组件系统
        
        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>组件实例</returns>
        public T AddComponent<T>() where T : class, ISampleUIComponent, new()
        {
            Type componentType = typeof(T);
            
            if (components.ContainsKey(componentType))
            {
                return components[componentType] as T;
            }
            
            T component = new T();
            component.Initialize(this);
            components[componentType] = component;
            
            return component;
        }
        
        /// <summary>
        /// 获取组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>组件实例</returns>
        public T GetComponent<T>() where T : class
        {
            Type componentType = typeof(T);
            components.TryGetValue(componentType, out ISampleUIComponent component);
            return component as T;
        }
        
        /// <summary>
        /// 移除组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        public void RemoveComponent<T>() where T : class, ISampleUIComponent
        {
            Type componentType = typeof(T);
            
            if (components.TryGetValue(componentType, out ISampleUIComponent component))
            {
                component.OnDestroy();
                components.Remove(componentType);
            }
        }
        
        #endregion
        
        #region 动画系统
        
        /// <summary>
        /// 播放显示动画
        /// </summary>
        protected virtual void PlayShowAnimation()
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            
            currentAnimation = StartCoroutine(DoShowAnimation());
        }
        
        /// <summary>
        /// 播放隐藏动画
        /// </summary>
        protected virtual void PlayHideAnimation()
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            
            currentAnimation = StartCoroutine(DoHideAnimation());
        }
        
        /// <summary>
        /// 执行显示动画
        /// </summary>
        protected virtual IEnumerator DoShowAnimation()
        {
            SetVisibility(true, true);
            
            switch (showAnimation)
            {
                case SampleUIAnimationType.Fade:
                    yield return StartCoroutine(FadeIn());
                    break;
                case SampleUIAnimationType.Scale:
                    yield return StartCoroutine(ScaleIn());
                    break;
                case SampleUIAnimationType.SlideFromBottom:
                    yield return StartCoroutine(SlideInFromBottom());
                    break;
                default:
                    break;
            }
            
            OnShowComplete();
        }
        
        /// <summary>
        /// 执行隐藏动画
        /// </summary>
        protected virtual IEnumerator DoHideAnimation()
        {
            switch (hideAnimation)
            {
                case SampleUIAnimationType.Fade:
                    yield return StartCoroutine(FadeOut());
                    break;
                case SampleUIAnimationType.Scale:
                    yield return StartCoroutine(ScaleOut());
                    break;
                case SampleUIAnimationType.SlideToBottom:
                    yield return StartCoroutine(SlideOutToBottom());
                    break;
                default:
                    break;
            }
            
            SetVisibility(false, false);
            OnHideComplete();
        }
        
        #endregion
        
        #region 动画实现
        
        /// <summary>
        /// 淡入动画
        /// </summary>
        protected IEnumerator FadeIn()
        {
            float elapsed = 0f;
            canvasGroup.alpha = 0f;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / animationDuration);
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }
        
        /// <summary>
        /// 淡出动画
        /// </summary>
        protected IEnumerator FadeOut()
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / animationDuration);
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
        }
        
        /// <summary>
        /// 缩放进入动画
        /// </summary>
        protected IEnumerator ScaleIn()
        {
            float elapsed = 0f;
            contentRoot.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                
                // 使用缓动函数
                t = 1f - Mathf.Pow(1f - t, 3f); // EaseOutCubic
                
                contentRoot.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
            
            contentRoot.localScale = originalScale;
            canvasGroup.alpha = 1f;
        }
        
        /// <summary>
        /// 缩放退出动画
        /// </summary>
        protected IEnumerator ScaleOut()
        {
            float elapsed = 0f;
            Vector3 startScale = contentRoot.localScale;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                
                contentRoot.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }
            
            contentRoot.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
        }
        
        /// <summary>
        /// 从底部滑入动画
        /// </summary>
        protected IEnumerator SlideInFromBottom()
        {
            float elapsed = 0f;
            Vector2 startPos = originalPosition + Vector2.down * Screen.height;
            contentRoot.anchoredPosition = startPos;
            canvasGroup.alpha = 1f;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                
                // 使用缓动函数
                t = 1f - Mathf.Pow(1f - t, 3f); // EaseOutCubic
                
                contentRoot.anchoredPosition = Vector2.Lerp(startPos, originalPosition, t);
                yield return null;
            }
            
            contentRoot.anchoredPosition = originalPosition;
        }
        
        /// <summary>
        /// 滑出到底部动画
        /// </summary>
        protected IEnumerator SlideOutToBottom()
        {
            float elapsed = 0f;
            Vector2 startPos = contentRoot.anchoredPosition;
            Vector2 endPos = originalPosition + Vector2.down * Screen.height;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                
                contentRoot.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            contentRoot.anchoredPosition = endPos;
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 设置可见性
        /// </summary>
        /// <param name="visible">是否可见</param>
        /// <param name="immediate">是否立即设置</param>
        protected virtual void SetVisibility(bool visible, bool immediate)
        {
            if (immediate)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            
            gameObject.SetActive(visible);
        }
        
        #endregion
        
        #region 虚方法 - 子类重写
        
        /// <summary>
        /// 子类初始化方法
        /// </summary>
        protected virtual void OnInitialize() { }
        
        /// <summary>
        /// 初始化组件
        /// </summary>
        protected virtual void InitializeComponents() { }
        
        /// <summary>
        /// 显示前处理
        /// </summary>
        /// <param name="data">传递的数据</param>
        protected virtual void OnBeforeShow(object data) { }
        
        /// <summary>
        /// 面板显示时调用（子类重写）
        /// </summary>
        /// <param name="data">传递的数据，可为null</param>
        protected virtual void OnShow(object data = null) 
        {
            // 默认实现为空，子类可以重写此方法来处理显示逻辑
            // 子类可以通过检查 data 是否为 null 来判断是否有数据传入
        }
    
        /// <summary>
        /// 显示完成处理
        /// </summary>
        protected virtual void OnShowComplete()
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            OnShowCompleted?.Invoke(this);
        }
        
        /// <summary>
        /// 隐藏前处理
        /// </summary>
        protected virtual void OnBeforeHide() 
        {
            // 调用子类的OnHide方法
        }

        /// <summary>
        /// 面板隐藏时调用（子类重写）
        /// </summary>
        protected virtual void OnHide() { }
        
        /// <summary>
        /// 隐藏完成处理
        /// </summary>
        protected virtual void OnHideComplete()
        {
            IsShowing = false;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            OnHideCompleted?.Invoke(this);
        }
        
        /// <summary>
        /// 面板刷新时调用（子类重写）
        /// </summary>
        /// <param name="data">刷新数据</param>
        protected virtual void OnRefresh(object data) 
        {
            // 默认实现为空，子类可以重写此方法来处理刷新逻辑
        }
        
        #endregion
    }
    
    /// <summary>
    /// UI动画类型枚举
    /// </summary>
    public enum SampleUIAnimationType
    {
        None,               // 无动画
        Fade,               // 淡入淡出
        Scale,              // 缩放
        SlideFromLeft,      // 从左滑入
        SlideFromRight,     // 从右滑入
        SlideFromTop,       // 从上滑入
        SlideFromBottom,    // 从下滑入
        SlideToLeft,        // 滑出到左
        SlideToRight,       // 滑出到右
        SlideToTop,         // 滑出到上
        SlideToBottom       // 滑出到下
    }
}