using System;
using System.Collections;
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
        
        [Header("动画配置")]
        [SerializeField] protected SplUIAnimationType showAnimation = SplUIAnimationType.None;
        [SerializeField] protected SplUIAnimationType hideAnimation = SplUIAnimationType.None;
        [SerializeField] protected float animationDuration = 0.3f;
        
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
        /// 显示动画类型
        /// </summary>
        public SplUIAnimationType ShowAnimation 
        { 
            get => showAnimation; 
            set => showAnimation = value; 
        }
        
        /// <summary>
        /// 隐藏动画类型
        /// </summary>
        public SplUIAnimationType HideAnimation 
        { 
            get => hideAnimation; 
            set => hideAnimation = value; 
        }
        
        /// <summary>
        /// 动画持续时间
        /// </summary>
        public float AnimationDuration 
        { 
            get => animationDuration; 
            set => animationDuration = value; 
        }
        
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
        /// 当前动画协程
        /// </summary>
        private Coroutine currentAnimationCoroutine;
        
        /// <summary>
        /// 原始缩放值
        /// </summary>
        private Vector3 originalScale;
        
        /// <summary>
        /// 原始位置
        /// </summary>
        private Vector2 originalPosition;
        
        /// <summary>
        /// 隐藏完成回调函数
        /// </summary>
        private System.Action hideCompleteCallback;
        
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
            
            // 记录原始变换信息
            originalScale = transform.localScale;
            originalPosition = rectTransform.anchoredPosition;
            
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
                SetVisibility(false, true);
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
            // 停止动画
            if (currentAnimationCoroutine != null)
            {
                StopCoroutine(currentAnimationCoroutine);
                currentAnimationCoroutine = null;
            }
            
            // 销毁所有组件
            DestroyAllComponents();
            
            // 调用子类销毁逻辑
            OnPanelDestroy();
        }
        
        /// <summary>
        /// 销毁所有组件
        /// </summary>
        private void DestroyAllComponents()
        {
            // 对于MonoBehaviour组件，Unity会自动调用OnDestroy
            // 对于非MonoBehaviour组件，如果需要清理，应该在各自的OnDestroy中处理
            
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
            if (showAnimation != SplUIAnimationType.None)
            {
                PlayShowAnimation();
            }
            else
            {
                SetVisibility(true, true);
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
            
            // 保存回调函数
            hideCompleteCallback = onComplete;
            
            // 执行隐藏前处理
            OnBeforeHide();
            
            // 执行子类隐藏逻辑
            OnHide();
            
            // 播放隐藏动画
            if (hideAnimation != SplUIAnimationType.None)
            {
                PlayHideAnimation();
            }
            else
            {
                SetVisibility(false, true);
                OnHideAnimationComplete();
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
            if (currentAnimationCoroutine != null)
            {
                StopCoroutine(currentAnimationCoroutine);
                currentAnimationCoroutine = null;
            }
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
        
        #region 动画系统
        
        /// <summary>
        /// 播放显示动画
        /// </summary>
        protected virtual void PlayShowAnimation()
        {
            if (currentAnimationCoroutine != null)
            {
                StopCoroutine(currentAnimationCoroutine);
            }
            
            currentAnimationCoroutine = StartCoroutine(DoShowAnimation());
        }
        
        /// <summary>
        /// 播放隐藏动画
        /// </summary>
        protected virtual void PlayHideAnimation()
        {
            if (currentAnimationCoroutine != null)
            {
                StopCoroutine(currentAnimationCoroutine);
            }
            
            currentAnimationCoroutine = StartCoroutine(DoHideAnimation());
        }
        
        /// <summary>
        /// 执行显示动画
        /// </summary>
        protected virtual IEnumerator DoShowAnimation()
        {
            SetVisibility(true, false);
            
            switch (showAnimation)
            {
                case SplUIAnimationType.Fade:
                    yield return StartCoroutine(FadeIn());
                    break;
                case SplUIAnimationType.Scale:
                    yield return StartCoroutine(ScaleIn());
                    break;
                case SplUIAnimationType.FadeScale:
                    yield return StartCoroutine(FadeScaleIn());
                    break;
                case SplUIAnimationType.Slide:
                    // 通用滑动默认从底部滑入
                    yield return StartCoroutine(SlideInFromBottom());
                    break;
                case SplUIAnimationType.SlideFromBottom:
                    yield return StartCoroutine(SlideInFromBottom());
                    break;
                case SplUIAnimationType.SlideFromTop:
                    yield return StartCoroutine(SlideInFromTop());
                    break;
                case SplUIAnimationType.SlideFromLeft:
                    yield return StartCoroutine(SlideInFromLeft());
                    break;
                case SplUIAnimationType.SlideFromRight:
                    yield return StartCoroutine(SlideInFromRight());
                    break;
                default:
                    break;
            }
            
            OnShowAnimationComplete();
        }
        
        /// <summary>
        /// 执行隐藏动画
        /// </summary>
        protected virtual IEnumerator DoHideAnimation()
        {
            switch (hideAnimation)
            {
                case SplUIAnimationType.Fade:
                    yield return StartCoroutine(FadeOut());
                    break;
                case SplUIAnimationType.Scale:
                    yield return StartCoroutine(ScaleOut());
                    break;
                case SplUIAnimationType.FadeScale:
                    yield return StartCoroutine(FadeScaleOut());
                    break;
                case SplUIAnimationType.Slide:
                    // 通用滑动默认滑出到底部
                    yield return StartCoroutine(SlideOutToBottom());
                    break;
                case SplUIAnimationType.SlideToBottom:
                    yield return StartCoroutine(SlideOutToBottom());
                    break;
                case SplUIAnimationType.SlideToTop:
                    yield return StartCoroutine(SlideOutToTop());
                    break;
                case SplUIAnimationType.SlideToLeft:
                    yield return StartCoroutine(SlideOutToLeft());
                    break;
                case SplUIAnimationType.SlideToRight:
                    yield return StartCoroutine(SlideOutToRight());
                    break;
                default:
                    break;
            }
            
            SetVisibility(false, true);
            OnHideAnimationComplete();
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
            rectTransform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                
                // 使用缓动函数
                t = 1f - Mathf.Pow(1f - t, 3f); // EaseOutCubic
                
                rectTransform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
            
            rectTransform.localScale = originalScale;
            canvasGroup.alpha = 1f;
        }
        
        /// <summary>
        /// 缩放退出动画
        /// </summary>
        protected IEnumerator ScaleOut()
        {
            float elapsed = 0f;
            Vector3 startScale = rectTransform.localScale;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                
                rectTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }
            
            rectTransform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
        }
        
        /// <summary>
        /// 从底部滑入动画
        /// </summary>
        protected IEnumerator SlideInFromBottom()
        {
            float elapsed = 0f;
            Vector2 startPos = originalPosition + Vector2.down * Screen.height;
            rectTransform.anchoredPosition = startPos;
            canvasGroup.alpha = 1f;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                
                // 使用缓动函数
                t = 1f - Mathf.Pow(1f - t, 3f); // EaseOutCubic
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, originalPosition, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = originalPosition;
        }
        
        /// <summary>
        /// 滑出到底部动画
        /// </summary>
        protected IEnumerator SlideOutToBottom()
        {
            float elapsed = 0f;
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = originalPosition + Vector2.down * Screen.height;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = endPos;
        }
        
        /// <summary>
        /// 淡入淡出+缩放进入动画
        /// </summary>
        protected IEnumerator FadeScaleIn()
        {
            float elapsed = 0f;
            rectTransform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                
                // 使用缓动函数
                t = 1f - Mathf.Pow(1f - t, 3f); // EaseOutCubic
                
                rectTransform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
            
            rectTransform.localScale = originalScale;
            canvasGroup.alpha = 1f;
        }
        
        /// <summary>
        /// 淡入淡出+缩放退出动画
        /// </summary>
        protected IEnumerator FadeScaleOut()
        {
            float elapsed = 0f;
            Vector3 startScale = rectTransform.localScale;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                
                // 使用缓动函数
                t = Mathf.Pow(t, 3f); // EaseInCubic
                
                rectTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }
            
            rectTransform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
        }
        
        /// <summary>
        /// 从顶部滑入动画
        /// </summary>
        protected IEnumerator SlideInFromTop()
        {
            float elapsed = 0f;
            Vector2 startPos = originalPosition + Vector2.up * Screen.height;
            rectTransform.anchoredPosition = startPos;
            canvasGroup.alpha = 1f;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                
                // 使用缓动函数
                t = 1f - Mathf.Pow(1f - t, 3f); // EaseOutCubic
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, originalPosition, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = originalPosition;
        }
        
        /// <summary>
        /// 滑出到顶部动画
        /// </summary>
        protected IEnumerator SlideOutToTop()
        {
            float elapsed = 0f;
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = originalPosition + Vector2.up * Screen.height;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = endPos;
        }
        
        /// <summary>
        /// 从左侧滑入动画
        /// </summary>
        protected IEnumerator SlideInFromLeft()
        {
            float elapsed = 0f;
            Vector2 startPos = originalPosition + Vector2.left * Screen.width;
            rectTransform.anchoredPosition = startPos;
            canvasGroup.alpha = 1f;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                
                // 使用缓动函数
                t = 1f - Mathf.Pow(1f - t, 3f); // EaseOutCubic
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, originalPosition, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = originalPosition;
        }
        
        /// <summary>
        /// 滑出到左侧动画
        /// </summary>
        protected IEnumerator SlideOutToLeft()
        {
            float elapsed = 0f;
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = originalPosition + Vector2.left * Screen.width;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = endPos;
        }
        
        /// <summary>
        /// 从右侧滑入动画
        /// </summary>
        protected IEnumerator SlideInFromRight()
        {
            float elapsed = 0f;
            Vector2 startPos = originalPosition + Vector2.right * Screen.width;
            rectTransform.anchoredPosition = startPos;
            canvasGroup.alpha = 1f;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                
                // 使用缓动函数
                t = 1f - Mathf.Pow(1f - t, 3f); // EaseOutCubic
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, originalPosition, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = originalPosition;
        }
        
        /// <summary>
        /// 滑出到右侧动画
        /// </summary>
        protected IEnumerator SlideOutToRight()
        {
            float elapsed = 0f;
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = originalPosition + Vector2.right * Screen.width;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = endPos;
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
            }
            
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            gameObject.SetActive(visible);
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
            
            // 调用隐藏完成回调
            hideCompleteCallback?.Invoke();
            hideCompleteCallback = null; // 清空回调，避免重复调用
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
    
    /// <summary>
    /// UI面板类型枚举
    /// </summary>
    public enum SplUIType
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
    
    /// <summary>
    /// UI动画类型枚举
    /// </summary>
    public enum SplUIAnimationType
    {
        /// <summary>
        /// 无动画
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 淡入淡出
        /// </summary>
        Fade = 1,
        
        /// <summary>
        /// 缩放
        /// </summary>
        Scale = 2,
        
        /// <summary>
        /// 淡入淡出+缩放组合
        /// </summary>
        FadeScale = 3,
        
        /// <summary>
        /// 通用滑动（方向由SlideDirection控制）
        /// </summary>
        Slide = 4,
        
        /// <summary>
        /// 从底部滑入
        /// </summary>
        SlideFromBottom = 5,
        
        /// <summary>
        /// 滑出到底部
        /// </summary>
        SlideToBottom = 6,
        
        /// <summary>
        /// 从顶部滑入
        /// </summary>
        SlideFromTop = 7,
        
        /// <summary>
        /// 滑出到顶部
        /// </summary>
        SlideToTop = 8,
        
        /// <summary>
        /// 从左侧滑入
        /// </summary>
        SlideFromLeft = 9,
        
        /// <summary>
        /// 滑出到左侧
        /// </summary>
        SlideToLeft = 10,
        
        /// <summary>
        /// 从右侧滑入
        /// </summary>
        SlideFromRight = 11,
        
        /// <summary>
        /// 滑出到右侧
        /// </summary>
        SlideToRight = 12
    }
}