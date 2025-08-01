using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace Framework.TraditionalUI
{
    /// <summary>
    /// UI面板类型枚举
    /// </summary>
    public enum UIPanelType
    {
        Normal,     // 普通面板（会被压入栈中）
        Popup,      // 弹窗面板（不会被压入栈中）
        System,     // 系统面板（如加载界面）
        Top         // 顶层面板（最高优先级）
    }
    
    /// <summary>
    /// UI动画类型枚举
    /// </summary>
    public enum UIAnimationType
    {
        None,           // 无动画
        Fade,           // 淡入淡出
        Scale,          // 缩放
        Slide,          // 滑动
        SlideFromLeft,  // 从左滑入
        SlideFromRight, // 从右滑入
        SlideFromTop,   // 从上滑入
        SlideFromBottom // 从下滑入
    }
    
    /// <summary>
    /// 传统UI面板基类
    /// 提供UI面板的基础功能和生命周期管理
    /// </summary>
    public abstract class TraditionalUIPanel : MonoBehaviour
    {
        #region 字段和属性
        
        [Header("面板配置")]
        [SerializeField] private UIPanelType panelType = UIPanelType.Normal;
        [SerializeField] private UIAnimationType showAnimation = UIAnimationType.Fade;
        [SerializeField] private UIAnimationType hideAnimation = UIAnimationType.Fade;
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private bool closeOnBackgroundClick = false;
        [SerializeField] private bool playSound = true;
        
        [Header("UI组件引用")]
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected Button backgroundButton;
        [SerializeField] protected Button closeButton;
        
        // 面板属性
        public string PanelName { get; set; }
        public UIPanelType PanelType => panelType;
        public bool IsShowing { get; private set; }
        public bool IsInitialized { get; private set; }
        
        // 面板数据
        protected object panelData;
        
        // 动画相关
        private Coroutine currentAnimation;
        private Vector3 originalScale;
        private Vector2 originalPosition;
        
        // 事件委托
        public Action<TraditionalUIPanel> OnPanelShown;
        public Action<TraditionalUIPanel> OnPanelHidden;
        public Action<TraditionalUIPanel> OnPanelDestroyed;
        
        #endregion
        
        #region Unity生命周期
        
        protected virtual void Awake()
        {
            // 获取必要组件
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
            // 记录原始变换信息
            originalScale = transform.localScale;
            originalPosition = GetComponent<RectTransform>().anchoredPosition;
            
            // 设置按钮事件
            SetupButtons();
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
            
            // 触发销毁事件
            OnPanelDestroyed?.Invoke(this);
        }
        
        #endregion
        
        #region 初始化方法
        
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
            Debug.Log($"[传统UI面板] 初始化完成: {PanelName}");
        }
        
        /// <summary>
        /// 子类重写的初始化方法
        /// </summary>
        protected virtual void OnInitialize()
        {
            // 子类可以重写此方法进行特定初始化
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
        public virtual void Show(object data = null)
        {
            if (IsShowing)
                return;
            
            // 保存面板数据
            panelData = data;
            
            // 确保面板已初始化
            if (!IsInitialized)
                Initialize();
            
            // 执行显示前的处理
            OnBeforeShow(data);
            
            // 设置为显示状态
            IsShowing = true;
            gameObject.SetActive(true);
            
            // 播放显示动画
            if (TraditionalUIManager.Instance.enableUIAnimation && showAnimation != UIAnimationType.None)
            {
                PlayShowAnimation();
            }
            else
            {
                SetVisibility(true, false);
                OnShowComplete();
            }
            
            // 播放音效
            if (playSound && TraditionalUIManager.Instance.enableUISound)
            {
                PlayShowSound();
            }
            
            Debug.Log($"[传统UI面板] 显示面板: {PanelName}");
        }
        
        /// <summary>
        /// 隐藏面板
        /// </summary>
        public virtual void Hide()
        {
            if (!IsShowing)
                return;
            
            // 执行隐藏前的处理
            OnBeforeHide();
            
            // 设置为隐藏状态
            IsShowing = false;
            
            // 播放隐藏动画
            if (TraditionalUIManager.Instance.enableUIAnimation && hideAnimation != UIAnimationType.None)
            {
                PlayHideAnimation();
            }
            else
            {
                SetVisibility(false, false);
                OnHideComplete();
            }
            
            // 播放音效
            if (playSound && TraditionalUIManager.Instance.enableUISound)
            {
                PlayHideSound();
            }
            
            Debug.Log($"[传统UI面板] 隐藏面板: {PanelName}");
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
            
            currentAnimation = StartCoroutine(ShowAnimationCoroutine());
        }
        
        /// <summary>
        /// 播放隐藏动画
        /// </summary>
        private void PlayHideAnimation()
        {
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);
            
            currentAnimation = StartCoroutine(HideAnimationCoroutine());
        }
        
        /// <summary>
        /// 显示动画协程
        /// </summary>
        private IEnumerator ShowAnimationCoroutine()
        {
            // 设置初始状态
            SetAnimationStartState(showAnimation);
            SetVisibility(true, false);
            
            float elapsedTime = 0f;
            
            while (elapsedTime < animationDuration)
            {
                float progress = elapsedTime / animationDuration;
                progress = EaseInOutQuad(progress); // 使用缓动函数
                
                ApplyAnimationProgress(showAnimation, progress, true);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 确保动画完成状态
            ApplyAnimationProgress(showAnimation, 1f, true);
            OnShowComplete();
            
            currentAnimation = null;
        }
        
        /// <summary>
        /// 隐藏动画协程
        /// </summary>
        private IEnumerator HideAnimationCoroutine()
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < animationDuration)
            {
                float progress = elapsedTime / animationDuration;
                progress = EaseInOutQuad(progress); // 使用缓动函数
                
                ApplyAnimationProgress(hideAnimation, progress, false);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 确保动画完成状态
            ApplyAnimationProgress(hideAnimation, 1f, false);
            SetVisibility(false, false);
            OnHideComplete();
            
            currentAnimation = null;
        }
        
        /// <summary>
        /// 设置动画开始状态
        /// </summary>
        private void SetAnimationStartState(UIAnimationType animationType)
        {
            switch (animationType)
            {
                case UIAnimationType.Fade:
                    canvasGroup.alpha = 0f;
                    break;
                case UIAnimationType.Scale:
                    transform.localScale = Vector3.zero;
                    break;
                case UIAnimationType.SlideFromLeft:
                    GetComponent<RectTransform>().anchoredPosition = originalPosition + Vector2.left * Screen.width;
                    break;
                case UIAnimationType.SlideFromRight:
                    GetComponent<RectTransform>().anchoredPosition = originalPosition + Vector2.right * Screen.width;
                    break;
                case UIAnimationType.SlideFromTop:
                    GetComponent<RectTransform>().anchoredPosition = originalPosition + Vector2.up * Screen.height;
                    break;
                case UIAnimationType.SlideFromBottom:
                    GetComponent<RectTransform>().anchoredPosition = originalPosition + Vector2.down * Screen.height;
                    break;
            }
        }
        
        /// <summary>
        /// 应用动画进度
        /// </summary>
        private void ApplyAnimationProgress(UIAnimationType animationType, float progress, bool isShowing)
        {
            float actualProgress = isShowing ? progress : 1f - progress;
            
            switch (animationType)
            {
                case UIAnimationType.Fade:
                    canvasGroup.alpha = actualProgress;
                    break;
                case UIAnimationType.Scale:
                    transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, actualProgress);
                    break;
                case UIAnimationType.SlideFromLeft:
                    Vector2 leftTarget = Vector2.Lerp(originalPosition + Vector2.left * Screen.width, originalPosition, actualProgress);
                    GetComponent<RectTransform>().anchoredPosition = leftTarget;
                    break;
                case UIAnimationType.SlideFromRight:
                    Vector2 rightTarget = Vector2.Lerp(originalPosition + Vector2.right * Screen.width, originalPosition, actualProgress);
                    GetComponent<RectTransform>().anchoredPosition = rightTarget;
                    break;
                case UIAnimationType.SlideFromTop:
                    Vector2 topTarget = Vector2.Lerp(originalPosition + Vector2.up * Screen.height, originalPosition, actualProgress);
                    GetComponent<RectTransform>().anchoredPosition = topTarget;
                    break;
                case UIAnimationType.SlideFromBottom:
                    Vector2 bottomTarget = Vector2.Lerp(originalPosition + Vector2.down * Screen.height, originalPosition, actualProgress);
                    GetComponent<RectTransform>().anchoredPosition = bottomTarget;
                    break;
            }
        }
        
        /// <summary>
        /// 缓动函数 - 先慢后快再慢
        /// </summary>
        private float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
        }
        
        #endregion
        
        #region 可见性控制
        
        /// <summary>
        /// 设置面板可见性
        /// </summary>
        /// <param name="visible">是否可见</param>
        /// <param name="immediate">是否立即设置</param>
        private void SetVisibility(bool visible, bool immediate)
        {
            if (immediate)
            {
                gameObject.SetActive(visible);
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = visible ? 1f : 0f;
                    canvasGroup.interactable = visible;
                    canvasGroup.blocksRaycasts = visible;
                }
            }
            else
            {
                if (canvasGroup != null)
                {
                    canvasGroup.interactable = visible;
                    canvasGroup.blocksRaycasts = visible;
                }
                
                if (!visible)
                {
                    gameObject.SetActive(false);
                }
            }
        }
        
        #endregion
        
        #region 事件处理方法
        
        /// <summary>
        /// 背景点击事件
        /// </summary>
        private void OnBackgroundClick()
        {
            if (closeOnBackgroundClick)
            {
                OnCloseButtonClick();
            }
        }
        
        /// <summary>
        /// 关闭按钮点击事件
        /// </summary>
        private void OnCloseButtonClick()
        {
            TraditionalUIManager.Instance.ClosePanel(PanelName);
        }
        
        #endregion
        
        #region 生命周期回调方法
        
        /// <summary>
        /// 显示前回调
        /// </summary>
        /// <param name="data">面板数据</param>
        protected virtual void OnBeforeShow(object data)
        {
            // 子类可以重写此方法
        }
        
        /// <summary>
        /// 显示完成回调
        /// </summary>
        protected virtual void OnShowComplete()
        {
            // 触发显示完成事件
            OnPanelShown?.Invoke(this);
            
            // 子类可以重写此方法
            OnAfterShow();
        }
        
        /// <summary>
        /// 显示后回调
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
        protected virtual void OnHideComplete()
        {
            // 触发隐藏完成事件
            OnPanelHidden?.Invoke(this);
            
            // 子类可以重写此方法
            OnAfterHide();
        }
        
        /// <summary>
        /// 隐藏后回调
        /// </summary>
        protected virtual void OnAfterHide()
        {
            // 子类可以重写此方法
        }
        
        #endregion
        
        #region 音效方法
        
        /// <summary>
        /// 播放显示音效
        /// </summary>
        protected virtual void PlayShowSound()
        {
            // 子类可以重写此方法播放特定音效
            // AudioManager.Instance.PlayUISound("ui_panel_show");
        }
        
        /// <summary>
        /// 播放隐藏音效
        /// </summary>
        protected virtual void PlayHideSound()
        {
            // 子类可以重写此方法播放特定音效
            // AudioManager.Instance.PlayUISound("ui_panel_hide");
        }
        
        #endregion
        
        #region 工具方法
        
        /// <summary>
        /// 获取面板数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>转换后的数据</returns>
        protected T GetPanelData<T>() where T : class
        {
            return panelData as T;
        }
        
        /// <summary>
        /// 设置面板交互性
        /// </summary>
        /// <param name="interactable">是否可交互</param>
        public void SetInteractable(bool interactable)
        {
            if (canvasGroup != null)
            {
                canvasGroup.interactable = interactable;
            }
        }
        
        /// <summary>
        /// 设置面板透明度
        /// </summary>
        /// <param name="alpha">透明度值</param>
        public void SetAlpha(float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Clamp01(alpha);
            }
        }
        
        #endregion
    }
}