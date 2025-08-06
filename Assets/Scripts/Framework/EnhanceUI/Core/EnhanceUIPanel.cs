using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Framework.EnhanceUI.Config;

namespace Framework.EnhanceUI.Core
{
    /// <summary>
    /// 增强UI面板基类
    /// 提供UI面板的基础功能和生命周期管理
    /// </summary>
    public abstract class EnhanceUIPanel : MonoBehaviour
    {
        #region 字段和属性
        
        [Header("面板配置")]
        [SerializeField] protected string panelName;
        [SerializeField] protected UILayerType layerType = UILayerType.Normal;
        
        [Header("UI组件引用")]
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected Button backgroundButton;
        [SerializeField] protected Button closeButton;
        [SerializeField] protected RectTransform contentRoot;
        
        /// <summary>
        /// 面板名称
        /// </summary>
        public string PanelName 
        { 
            get => string.IsNullOrEmpty(panelName) ? GetType().Name : panelName;
            set => panelName = value;
        }
        
        /// <summary>
        /// 面板层级类型
        /// </summary>
        public UILayerType LayerType 
        { 
            get => layerType;
            set => layerType = value;
        }
        
        /// <summary>
        /// 面板当前状态
        /// </summary>
        public UIState CurrentState { get; private set; } = UIState.None;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// 是否正在显示
        /// </summary>
        public bool IsShowing => currentState == UIState.Showing || currentState == UIState.Shown;
        
        /// <summary>
        /// 实例ID
        /// </summary>
        public string InstanceId => instanceId;
        
        /// <summary>
        /// UI配置数据
        /// </summary>
        public UIConfigData ConfigData => configData;
        
        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsVisible => gameObject.activeInHierarchy && (canvasGroup == null || canvasGroup.alpha > 0);
        
        /// <summary>
        /// UI配置数据
        /// </summary>
        public UIConfigData Config { get; private set; }
        

        
        /// <summary>
        /// 传递给面板的数据
        /// </summary>
        protected object panelData;
        
        /// <summary>
        /// 实例ID
        /// </summary>
        protected string instanceId;
        
        /// <summary>
        /// UI配置数据
        /// </summary>
        protected UIConfigData configData;
        
        // 动画相关字段
        /// <summary>
        /// 动画类型
        /// </summary>
        protected UIAnimationType animationType = UIAnimationType.Fade;
        
        /// <summary>
        /// 动画持续时间
        /// </summary>
        protected float animationDuration = 0.3f;
        
        /// <summary>
        /// 点击背景是否关闭面板
        /// </summary>
        protected bool closeOnBackgroundClick = false;
        
        /// <summary>
        /// 是否播放音效
        /// </summary>
        protected bool playSound = true;
        
        // 动画相关私有字段
        private Coroutine currentAnimation;
        private Vector3 originalScale;
        private Vector2 originalPosition;
        private UIState currentState;

        #endregion
        
        #region 事件委托
        
        /// <summary>
        /// 面板初始化完成事件
        /// </summary>
        public event Action<EnhanceUIPanel> OnInitialized;
        
        /// <summary>
        /// 面板显示开始事件
        /// </summary>
        public event Action<EnhanceUIPanel> OnShowStart;
        
        /// <summary>
        /// 面板显示完成事件
        /// </summary>
        public event Action<EnhanceUIPanel> OnShowComplete;
        
        /// <summary>
        /// 面板隐藏开始事件
        /// </summary>
        public event Action<EnhanceUIPanel> OnHideStart;
        
        /// <summary>
        /// 面板隐藏完成事件
        /// </summary>
        public event Action<EnhanceUIPanel> OnHideComplete;
        
        /// <summary>
        /// 面板销毁事件
        /// </summary>
        public event Action<EnhanceUIPanel> OnDestroyed;
        
        /// <summary>
        /// 面板状态改变事件
        /// </summary>
        public event Action<EnhanceUIPanel, UIState, UIState> OnStateChanged;
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 设置实例ID
        /// </summary>
        /// <param name="id">实例ID</param>
        public void SetInstanceId(string id)
        {
            instanceId = id;
        }
        
        /// <summary>
        /// 设置配置数据
        /// </summary>
        /// <param name="config">配置数据</param>
        public void SetConfig(UIConfigData config)
        {
            configData = config;
            Config = config;
            if (config != null)
            {
                panelName = config.uiName;
                layerType = config.layerType;
                animationType = config.showAnimation;
                animationDuration = config.animationDuration;
                closeOnBackgroundClick = config.closeOnBackgroundClick;
                playSound = config.playSound;
            }
        }
        
        /// <summary>
        /// 重置到对象池状态
        /// </summary>
        public virtual void ResetToPool()
        {
            // 重置状态
            currentState = UIState.Hidden;
            IsInitialized = false;
            panelData = null;
            
            // 重置UI组件
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            
            // 停止动画
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                currentAnimation = null;
            }
            
            // 重置Transform
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            
            // 调用子类重置方法
            OnResetToPool();
        }
        
        /// <summary>
        /// 子类重写此方法来实现自定义的对象池重置逻辑
        /// </summary>
        protected virtual void OnResetToPool()
        {
            // 子类可以重写此方法
        }
        
        #endregion
        
        #region Unity生命周期
        
        protected virtual void Awake()
        {
            // 生成实例ID
            instanceId = Guid.NewGuid().ToString();
            
            // 获取必要组件
            InitializeComponents();
            
            // 记录原始变换信息
            originalScale = transform.localScale;
            if (contentRoot != null)
                originalPosition = contentRoot.anchoredPosition;
            else
                originalPosition = GetComponent<RectTransform>().anchoredPosition;
            
            // 设置按钮事件
            SetupButtons();
            
            Debug.Log($"[EnhanceUIPanel] 面板创建: {PanelName} ({InstanceId})");
        }
        
        protected virtual void Start()
        {
            // 初始状态设为隐藏
            if (CurrentState == UIState.None)
            {
                SetVisibility(false, false);
                ChangeState(UIState.Hidden);
            }
        }
        
        protected virtual void OnDestroy()
        {
            // 停止当前动画
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                currentAnimation = null;
            }
            
            // 触发销毁事件
            OnDestroyed?.Invoke(this);
            
            Debug.Log($"[EnhanceUIPanel] 面板销毁: {PanelName} ({InstanceId})");
        }
        
        #endregion
        
        #region 初始化方法
        
        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponents()
        {
            // 获取CanvasGroup组件
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
            // 获取内容根节点
            if (contentRoot == null)
                contentRoot = GetComponent<RectTransform>();
        }
        

        
        /// <summary>
        /// 初始化面板
        /// </summary>
        /// <param name="data">初始化数据</param>
        public virtual void Initialize(object data = null)
        {
            if (IsInitialized)
                return;
            
            panelData = data;
            
            try
            {
                // 执行子类初始化
                OnInitialize(data);
                
                IsInitialized = true;
                ChangeState(UIState.Loaded);
                
                // 触发初始化完成事件
                OnInitialized?.Invoke(this);
                
                Debug.Log($"[EnhanceUIPanel] 面板初始化完成: {PanelName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnhanceUIPanel] 面板初始化失败: {PanelName}, 错误: {e.Message}");
                ChangeState(UIState.Error);
                throw;
            }
        }
        
        /// <summary>
        /// 初始化面板（泛型版本，避免装箱）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">初始化数据</param>
        public virtual void Initialize<T>(T data)
        {
            if (IsInitialized)
                return;
            
            panelData = data;
            
            try
            {
                // 执行子类初始化
                OnInitialize(data);
                
                IsInitialized = true;
                ChangeState(UIState.Loaded);
                
                // 触发初始化完成事件
                OnInitialized?.Invoke(this);
                
                Debug.Log($"[EnhanceUIPanel] 面板初始化完成: {PanelName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnhanceUIPanel] 面板初始化失败: {PanelName}, 错误: {e.Message}");
                ChangeState(UIState.Error);
                throw;
            }
        }
        
        /// <summary>
        /// 无参数初始化面板
        /// </summary>
        public virtual void Initialize()
        {
            Initialize((object)null);
        }
        
        /// <summary>
        /// 子类重写的初始化方法
        /// </summary>
        /// <param name="data">初始化数据</param>
        protected virtual void OnInitialize(object data)
        {
            // 子类可以重写此方法进行特定初始化
        }
        
        /// <summary>
        /// 子类重写的初始化方法（泛型版本）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">初始化数据</param>
        protected virtual void OnInitialize<T>(T data)
        {
            // 默认调用object版本
            OnInitialize((object)data);
        }
        
        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtons()
        {
            // 设置背景按钮事件
            if (backgroundButton != null)
            {
                backgroundButton.onClick.AddListener(OnBackgroundClick);
            }
            
            // 设置关闭按钮事件
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClick);
            }
        }
        
        #endregion
        
        #region 显示隐藏方法
        
        /// <summary>
        /// 显示面板
        /// </summary>
        /// <param name="data">传递给面板的数据</param>
        /// <param name="skipAnimation">是否跳过动画</param>
        public virtual void Show(object data = null, bool skipAnimation = false)
        {
            if (IsShowing)
            {
                Debug.LogWarning($"[EnhanceUIPanel] 面板已在显示中: {PanelName}");
                return;
            }
            
            // 确保面板已初始化
            if (!IsInitialized)
                Initialize(data);
            else if (data != null)
                panelData = data;
            
            try
            {
                // 执行显示前的处理
                OnBeforeShow(data);
                
                // 改变状态
                ChangeState(UIState.Showing);
                
                // 激活游戏对象
                gameObject.SetActive(true);
                
                // 触发显示开始事件
                OnShowStart?.Invoke(this);
                
                // 播放显示动画
                bool useAnimation = !skipAnimation && Config != null && Config.showAnimation != UIAnimationType.None;
                if (useAnimation)
                {
                    PlayShowAnimation();
                }
                else
                {
                    SetVisibility(true, false);
                    CompleteShow();
                }
                
                Debug.Log($"[EnhanceUIPanel] 面板显示: {PanelName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnhanceUIPanel] 面板显示失败: {PanelName}, 错误: {e.Message}");
                ChangeState(UIState.Error);
                throw;
            }
        }
        
        /// <summary>
        /// 显示面板（无参数版本）
        /// </summary>
        /// <param name="skipAnimation">是否跳过动画</param>
        public virtual void Show(bool skipAnimation = false)
        {
            Show((object)null, skipAnimation);
        }
        
        /// <summary>
        /// 显示面板（泛型版本，避免装箱）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">传递给面板的数据</param>
        /// <param name="skipAnimation">是否跳过动画</param>
        public virtual void Show<T>(T data, bool skipAnimation = false)
        {
            if (IsShowing)
            {
                Debug.LogWarning($"[EnhanceUIPanel] 面板已在显示中: {PanelName}");
                return;
            }
            
            // 确保面板已初始化
            if (!IsInitialized)
                Initialize(data);
            else
                panelData = data;
            
            try
            {
                // 执行显示前的处理
                OnBeforeShow(panelData);
                
                // 改变状态
                ChangeState(UIState.Showing);
                
                // 激活游戏对象
                gameObject.SetActive(true);
                
                // 触发显示开始事件
                OnShowStart?.Invoke(this);
                
                // 播放显示动画
                bool useAnimation = !skipAnimation && Config != null && Config.showAnimation != UIAnimationType.None;
                if (useAnimation)
                {
                    PlayShowAnimation();
                }
                else
                {
                    SetVisibility(true, false);
                    CompleteShow();
                }
                
                Debug.Log($"[EnhanceUIPanel] 面板显示: {PanelName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnhanceUIPanel] 面板显示失败: {PanelName}, 错误: {e.Message}");
                ChangeState(UIState.Error);
                throw;
            }
        }
        
        /// <summary>
        /// 隐藏面板
        /// </summary>
        /// <param name="skipAnimation">是否跳过动画</param>
        public virtual void Hide(bool skipAnimation = false)
        {
            if (CurrentState == UIState.Hidden || CurrentState == UIState.Hiding)
            {
                Debug.LogWarning($"[EnhanceUIPanel] 面板已隐藏或正在隐藏: {PanelName}");
                return;
            }
            
            try
            {
                // 执行隐藏前的处理
                OnBeforeHide();
                
                // 改变状态
                ChangeState(UIState.Hiding);
                
                // 触发隐藏开始事件
                OnHideStart?.Invoke(this);
                
                // 播放隐藏动画
                bool useAnimation = !skipAnimation && Config != null && Config.hideAnimation != UIAnimationType.None;
                if (useAnimation)
                {
                    PlayHideAnimation();
                }
                else
                {
                    SetVisibility(false, false);
                    CompleteHide();
                }
                
                Debug.Log($"[EnhanceUIPanel] 面板隐藏: {PanelName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnhanceUIPanel] 面板隐藏失败: {PanelName}, 错误: {e.Message}");
                ChangeState(UIState.Error);
                throw;
            }
        }
        
        /// <summary>
        /// 销毁面板
        /// </summary>
        public virtual void DestroyPanel()
        {
            if (CurrentState == UIState.Destroying || CurrentState == UIState.Destroyed)
                return;
            
            try
            {
                // 执行销毁前的处理
                OnBeforeDestroy();
                
                // 改变状态
                ChangeState(UIState.Destroying);
                
                // 停止所有动画
                if (currentAnimation != null)
                {
                    StopCoroutine(currentAnimation);
                    currentAnimation = null;
                }
                
                // 销毁游戏对象
                Destroy(gameObject);
                
                // 改变状态
                ChangeState(UIState.Destroyed);
                
                Debug.Log($"[EnhanceUIPanel] 面板销毁: {PanelName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnhanceUIPanel] 面板销毁失败: {PanelName}, 错误: {e.Message}");
                throw;
            }
        }
        
        #endregion
        
        #region 生命周期回调方法
        
        /// <summary>
        /// 显示前回调
        /// </summary>
        /// <param name="data">传递的数据</param>
        protected virtual void OnBeforeShow(object data)
        {
            // 子类可以重写此方法
        }
        
        /// <summary>
        /// 显示前回调（泛型版本）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">传递的数据</param>
        protected virtual void OnBeforeShow<T>(T data)
        {
            // 默认调用object版本
            OnBeforeShow((object)data);
        }
        
        /// <summary>
        /// 显示完成回调
        /// </summary>
        protected virtual void OnAfterShow()
        {
            // 子类可以重写此方法
        }
        
        /// <summary>
        /// 隐藏前回调
        /// </summary>
        protected virtual void OnBeforeHide()
        {
            // 子类可以重写此方法
        }
        
        /// <summary>
        /// 隐藏完成回调
        /// </summary>
        protected virtual void OnAfterHide()
        {
            // 子类可以重写此方法
        }
        
        /// <summary>
        /// 销毁前回调
        /// </summary>
        protected virtual void OnBeforeDestroy()
        {
            // 子类可以重写此方法
        }
        
        #endregion
        
        #region 按钮事件处理
        
        /// <summary>
        /// 背景点击事件
        /// </summary>
        protected virtual void OnBackgroundClick()
        {
            if (Config != null && Config.closeOnBackgroundClick)
            {
                Hide();
            }
        }
        
        /// <summary>
        /// 关闭按钮点击事件
        /// </summary>
        protected virtual void OnCloseButtonClick()
        {
            Hide();
        }
        
        #endregion
        
        #region 动画方法
        
        /// <summary>
        /// 播放显示动画
        /// </summary>
        private void PlayShowAnimation()
        {
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);
            
            float duration = Config?.animationDuration ?? 0.3f;
            UIAnimationType animationType = Config?.showAnimation ?? UIAnimationType.Fade;
            
            currentAnimation = StartCoroutine(PlayShowAnimationCoroutine(animationType, duration));
        }
        
        /// <summary>
        /// 播放隐藏动画
        /// </summary>
        private void PlayHideAnimation()
        {
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);
            
            float duration = Config?.animationDuration ?? 0.3f;
            UIAnimationType animationType = Config?.hideAnimation ?? UIAnimationType.Fade;
            
            currentAnimation = StartCoroutine(PlayHideAnimationCoroutine(animationType, duration));
        }
        
        /// <summary>
        /// 显示动画协程
        /// </summary>
        private IEnumerator PlayShowAnimationCoroutine(UIAnimationType animationType, float duration)
        {
            SetVisibility(true, true);
            
            switch (animationType)
            {
                case UIAnimationType.Fade:
                    yield return StartCoroutine(FadeAnimation(0f, 1f, duration));
                    break;
                case UIAnimationType.Scale:
                    yield return StartCoroutine(ScaleAnimation(Vector3.zero, originalScale, duration));
                    break;
                case UIAnimationType.SlideFromLeft:
                    yield return StartCoroutine(SlideAnimation(new Vector2(-Screen.width, originalPosition.y), originalPosition, duration));
                    break;
                case UIAnimationType.SlideFromRight:
                    yield return StartCoroutine(SlideAnimation(new Vector2(Screen.width, originalPosition.y), originalPosition, duration));
                    break;
                case UIAnimationType.SlideFromTop:
                    yield return StartCoroutine(SlideAnimation(new Vector2(originalPosition.x, Screen.height), originalPosition, duration));
                    break;
                case UIAnimationType.SlideFromBottom:
                    yield return StartCoroutine(SlideAnimation(new Vector2(originalPosition.x, -Screen.height), originalPosition, duration));
                    break;
                default:
                    SetVisibility(true, false);
                    break;
            }
            
            CompleteShow();
        }
        
        /// <summary>
        /// 隐藏动画协程
        /// </summary>
        private IEnumerator PlayHideAnimationCoroutine(UIAnimationType animationType, float duration)
        {
            switch (animationType)
            {
                case UIAnimationType.Fade:
                    yield return StartCoroutine(FadeAnimation(1f, 0f, duration));
                    break;
                case UIAnimationType.Scale:
                    yield return StartCoroutine(ScaleAnimation(originalScale, Vector3.zero, duration));
                    break;
                case UIAnimationType.SlideFromLeft:
                    yield return StartCoroutine(SlideAnimation(originalPosition, new Vector2(-Screen.width, originalPosition.y), duration));
                    break;
                case UIAnimationType.SlideFromRight:
                    yield return StartCoroutine(SlideAnimation(originalPosition, new Vector2(Screen.width, originalPosition.y), duration));
                    break;
                case UIAnimationType.SlideFromTop:
                    yield return StartCoroutine(SlideAnimation(originalPosition, new Vector2(originalPosition.x, Screen.height), duration));
                    break;
                case UIAnimationType.SlideFromBottom:
                    yield return StartCoroutine(SlideAnimation(originalPosition, new Vector2(originalPosition.x, -Screen.height), duration));
                    break;
                default:
                    SetVisibility(false, false);
                    break;
            }
            
            CompleteHide();
        }
        
        /// <summary>
        /// 淡入淡出动画
        /// </summary>
        private IEnumerator FadeAnimation(float fromAlpha, float toAlpha, float duration)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsedTime / duration);
                
                if (canvasGroup != null)
                    canvasGroup.alpha = alpha;
                
                yield return null;
            }
            
            if (canvasGroup != null)
                canvasGroup.alpha = toAlpha;
        }
        
        /// <summary>
        /// 缩放动画
        /// </summary>
        private IEnumerator ScaleAnimation(Vector3 fromScale, Vector3 toScale, float duration)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                Vector3 scale = Vector3.Lerp(fromScale, toScale, elapsedTime / duration);
                transform.localScale = scale;
                
                yield return null;
            }
            
            transform.localScale = toScale;
        }
        
        /// <summary>
        /// 滑动动画
        /// </summary>
        private IEnumerator SlideAnimation(Vector2 fromPosition, Vector2 toPosition, float duration)
        {
            float elapsedTime = 0f;
            RectTransform rectTransform = contentRoot ?? GetComponent<RectTransform>();
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                Vector2 position = Vector2.Lerp(fromPosition, toPosition, elapsedTime / duration);
                rectTransform.anchoredPosition = position;
                
                yield return null;
            }
            
            rectTransform.anchoredPosition = toPosition;
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 设置可见性
        /// </summary>
        /// <param name="visible">是否可见</param>
        /// <param name="immediate">是否立即设置</param>
        private void SetVisibility(bool visible, bool immediate)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            
            if (immediate)
            {
                gameObject.SetActive(visible);
            }
        }
        
        /// <summary>
        /// 完成显示
        /// </summary>
        private void CompleteShow()
        {
            currentAnimation = null;
            ChangeState(UIState.Shown);
            OnAfterShow();
            OnShowComplete?.Invoke(this);
        }
        
        /// <summary>
        /// 完成隐藏
        /// </summary>
        private void CompleteHide()
        {
            currentAnimation = null;
            gameObject.SetActive(false);
            ChangeState(UIState.Hidden);
            OnAfterHide();
            OnHideComplete?.Invoke(this);
        }
        
        /// <summary>
        /// 改变状态
        /// </summary>
        /// <param name="newState">新状态</param>
        private void ChangeState(UIState newState)
        {
            UIState oldState = CurrentState;
            CurrentState = newState;
            OnStateChanged?.Invoke(this, oldState, newState);
        }
        
        /// <summary>
        /// 获取传递的数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>数据对象</returns>
        protected T GetData<T>() where T : class
        {
            return panelData as T;
        }
        
        /// <summary>
        /// 获取传递的数据（值类型）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>数据值</returns>
        protected T GetDataValue<T>() where T : struct
        {
            if (panelData is T value)
                return value;
            return default(T);
        }
        
        #endregion
    }
}