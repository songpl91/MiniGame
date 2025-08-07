using System;
using System.Collections;
using UnityEngine;

namespace Framework.SplUI.Core
{
    /// <summary>
    /// UI动画组件
    /// 负责处理UI面板的显示和隐藏动画
    /// </summary>
    public class SplUIAnimationComponent : ISplUIComponent
    {
        #region 字段和属性
        
        /// <summary>
        /// 所属面板
        /// </summary>
        private SplUIBase ownerPanel;
        
        /// <summary>
        /// 组件所属的UI面板（ISplUIComponent接口实现）
        /// </summary>
        public SplUIBase OwnerPanel => ownerPanel;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// 显示动画类型
        /// </summary>
        public SplUIAnimationType ShowAnimation { get; set; } = SplUIAnimationType.None;
        
        /// <summary>
        /// 隐藏动画类型
        /// </summary>
        public SplUIAnimationType HideAnimation { get; set; } = SplUIAnimationType.None;
        
        /// <summary>
        /// 动画持续时间
        /// </summary>
        public float AnimationDuration { get; set; } = 0.3f;
        
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
        /// Canvas组件引用
        /// </summary>
        private CanvasGroup canvasGroup;
        
        /// <summary>
        /// RectTransform组件引用
        /// </summary>
        private RectTransform rectTransform;
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 显示动画完成事件
        /// </summary>
        public event Action OnShowAnimationCompleted;
        
        /// <summary>
        /// 隐藏动画完成事件
        /// </summary>
        public event Action OnHideAnimationCompleted;
        
        #endregion
        
        #region ISplUIComponent实现
        
        /// <summary>
        /// 设置所属面板
        /// </summary>
        /// <param name="panel">面板实例</param>
        public void SetOwnerPanel(SplUIBase panel)
        {
            ownerPanel = panel;
        }
        
        /// <summary>
        /// 初始化组件
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized || ownerPanel == null)
                return;
            
            // 获取必要组件
            canvasGroup = ownerPanel.GetComponent<CanvasGroup>();
            rectTransform = ownerPanel.GetComponent<RectTransform>();
            
            if (canvasGroup == null)
            {
                canvasGroup = ownerPanel.gameObject.AddComponent<CanvasGroup>();
            }
            
            // 记录原始变换信息
            originalScale = ownerPanel.transform.localScale;
            originalPosition = rectTransform.anchoredPosition;
            
            IsInitialized = true;
        }
        
        /// <summary>
        /// 初始化组件（带参数）
        /// </summary>
        /// <param name="panel">面板实例</param>
        public void Initialize(SplUIBase panel)
        {
            SetOwnerPanel(panel);
            Initialize();
        }
        
        /// <summary>
        /// 更新组件
        /// </summary>
        public void OnUpdate()
        {
            // 动画组件通常不需要每帧更新
        }
        
        /// <summary>
        /// 销毁组件
        /// </summary>
        public void OnDestroy()
        {
            // 停止当前动画
            StopCurrentAnimation();
            
            OnShowAnimationCompleted = null;
            OnHideAnimationCompleted = null;
        }
        
        #endregion
        
        #region 公共API
        
        /// <summary>
        /// 播放显示动画
        /// </summary>
        /// <param name="onComplete">完成回调</param>
        public void PlayShowAnimation(Action onComplete = null)
        {
            if (ShowAnimation == SplUIAnimationType.None)
            {
                SetVisibility(true, true);
                onComplete?.Invoke();
                OnShowAnimationCompleted?.Invoke();
                return;
            }
            
            if (currentAnimationCoroutine != null)
            {
                ownerPanel.StopCoroutine(currentAnimationCoroutine);
            }
            
            currentAnimationCoroutine = ownerPanel.StartCoroutine(DoShowAnimation(onComplete));
        }
        
        /// <summary>
        /// 播放隐藏动画
        /// </summary>
        /// <param name="onComplete">完成回调</param>
        public void PlayHideAnimation(Action onComplete = null)
        {
            if (HideAnimation == SplUIAnimationType.None)
            {
                SetVisibility(false, true);
                onComplete?.Invoke();
                OnHideAnimationCompleted?.Invoke();
                return;
            }
            
            if (currentAnimationCoroutine != null)
            {
                ownerPanel.StopCoroutine(currentAnimationCoroutine);
            }
            
            currentAnimationCoroutine = ownerPanel.StartCoroutine(DoHideAnimation(onComplete));
        }
        
        /// <summary>
        /// 停止当前动画
        /// </summary>
        public void StopCurrentAnimation()
        {
            if (currentAnimationCoroutine != null && ownerPanel != null)
            {
                ownerPanel.StopCoroutine(currentAnimationCoroutine);
                currentAnimationCoroutine = null;
            }
        }
        
        /// <summary>
        /// 设置可见性
        /// </summary>
        /// <param name="visible">是否可见</param>
        /// <param name="immediate">是否立即设置</param>
        public void SetVisibility(bool visible, bool immediate = false)
        {
            if (canvasGroup == null) return;
            
            if (immediate)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
                
                if (!visible && ownerPanel != null)
                {
                    ownerPanel.gameObject.SetActive(false);
                }
            }
        }
        
        #endregion
        
        #region 动画实现
        
        /// <summary>
        /// 执行显示动画
        /// </summary>
        private IEnumerator DoShowAnimation(Action onComplete)
        {
            SetVisibility(true, false);
            
            switch (ShowAnimation)
            {
                case SplUIAnimationType.Fade:
                    yield return ownerPanel.StartCoroutine(FadeIn());
                    break;
                case SplUIAnimationType.Scale:
                    yield return ownerPanel.StartCoroutine(ScaleIn());
                    break;
                case SplUIAnimationType.FadeScale:
                    yield return ownerPanel.StartCoroutine(FadeScaleIn());
                    break;
                case SplUIAnimationType.SlideFromBottom:
                    yield return ownerPanel.StartCoroutine(SlideInFromBottom());
                    break;
                case SplUIAnimationType.SlideFromTop:
                    yield return ownerPanel.StartCoroutine(SlideInFromTop());
                    break;
                case SplUIAnimationType.SlideFromLeft:
                    yield return ownerPanel.StartCoroutine(SlideInFromLeft());
                    break;
                case SplUIAnimationType.SlideFromRight:
                    yield return ownerPanel.StartCoroutine(SlideInFromRight());
                    break;
            }
            
            onComplete?.Invoke();
            OnShowAnimationCompleted?.Invoke();
        }
        
        /// <summary>
        /// 执行隐藏动画
        /// </summary>
        private IEnumerator DoHideAnimation(Action onComplete)
        {
            switch (HideAnimation)
            {
                case SplUIAnimationType.Fade:
                    yield return ownerPanel.StartCoroutine(FadeOut());
                    break;
                case SplUIAnimationType.Scale:
                    yield return ownerPanel.StartCoroutine(ScaleOut());
                    break;
                case SplUIAnimationType.FadeScale:
                    yield return ownerPanel.StartCoroutine(FadeScaleOut());
                    break;
                case SplUIAnimationType.SlideToBottom:
                    yield return ownerPanel.StartCoroutine(SlideOutToBottom());
                    break;
                case SplUIAnimationType.SlideToTop:
                    yield return ownerPanel.StartCoroutine(SlideOutToTop());
                    break;
                case SplUIAnimationType.SlideToLeft:
                    yield return ownerPanel.StartCoroutine(SlideOutToLeft());
                    break;
                case SplUIAnimationType.SlideToRight:
                    yield return ownerPanel.StartCoroutine(SlideOutToRight());
                    break;
            }
            
            SetVisibility(false, true);
            onComplete?.Invoke();
            OnHideAnimationCompleted?.Invoke();
        }
        
        /// <summary>
        /// 淡入动画
        /// </summary>
        private IEnumerator FadeIn()
        {
            float elapsed = 0f;
            canvasGroup.alpha = 0f;
            
            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / AnimationDuration);
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }
        
        /// <summary>
        /// 淡出动画
        /// </summary>
        private IEnumerator FadeOut()
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / AnimationDuration);
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
        }
        
        /// <summary>
        /// 缩放进入动画
        /// </summary>
        private IEnumerator ScaleIn()
        {
            float elapsed = 0f;
            rectTransform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
            
            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / AnimationDuration;
                
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
        private IEnumerator ScaleOut()
        {
            float elapsed = 0f;
            Vector3 startScale = rectTransform.localScale;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / AnimationDuration;
                
                rectTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }
            
            rectTransform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
        }
        
        /// <summary>
        /// 淡入缩放组合动画
        /// </summary>
        private IEnumerator FadeScaleIn()
        {
            float elapsed = 0f;
            rectTransform.localScale = originalScale * 0.8f;
            canvasGroup.alpha = 0f;
            
            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / AnimationDuration;
                
                // 使用缓动函数
                t = 1f - Mathf.Pow(1f - t, 3f); // EaseOutCubic
                
                rectTransform.localScale = Vector3.Lerp(originalScale * 0.8f, originalScale, t);
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
            
            rectTransform.localScale = originalScale;
            canvasGroup.alpha = 1f;
        }
        
        /// <summary>
        /// 淡出缩放组合动画
        /// </summary>
        private IEnumerator FadeScaleOut()
        {
            float elapsed = 0f;
            Vector3 startScale = rectTransform.localScale;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / AnimationDuration;
                
                rectTransform.localScale = Vector3.Lerp(startScale, originalScale * 0.8f, t);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }
            
            rectTransform.localScale = originalScale * 0.8f;
            canvasGroup.alpha = 0f;
        }
        
        /// <summary>
        /// 从底部滑入动画
        /// </summary>
        private IEnumerator SlideInFromBottom()
        {
            float elapsed = 0f;
            Vector2 startPos = originalPosition + Vector2.down * Screen.height;
            rectTransform.anchoredPosition = startPos;
            canvasGroup.alpha = 1f;
            
            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / AnimationDuration;
                
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
        private IEnumerator SlideOutToBottom()
        {
            float elapsed = 0f;
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = originalPosition + Vector2.down * Screen.height;
            
            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / AnimationDuration;
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = endPos;
        }
        
        /// <summary>
        /// 从顶部滑入动画
        /// </summary>
        private IEnumerator SlideInFromTop()
        {
            float elapsed = 0f;
            Vector2 startPos = originalPosition + Vector2.up * Screen.height;
            rectTransform.anchoredPosition = startPos;
            canvasGroup.alpha = 1f;
            
            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / AnimationDuration;
                
                t = 1f - Mathf.Pow(1f - t, 3f); // EaseOutCubic
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, originalPosition, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = originalPosition;
        }
        
        /// <summary>
        /// 滑出到顶部动画
        /// </summary>
        private IEnumerator SlideOutToTop()
        {
            float elapsed = 0f;
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = originalPosition + Vector2.up * Screen.height;
            
            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / AnimationDuration;
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = endPos;
        }
        
        /// <summary>
        /// 从左侧滑入动画
        /// </summary>
        private IEnumerator SlideInFromLeft()
        {
            float elapsed = 0f;
            Vector2 startPos = originalPosition + Vector2.left * Screen.width;
            rectTransform.anchoredPosition = startPos;
            canvasGroup.alpha = 1f;
            
            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / AnimationDuration;
                
                t = 1f - Mathf.Pow(1f - t, 3f); // EaseOutCubic
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, originalPosition, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = originalPosition;
        }
        
        /// <summary>
        /// 滑出到左侧动画
        /// </summary>
        private IEnumerator SlideOutToLeft()
        {
            float elapsed = 0f;
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = originalPosition + Vector2.left * Screen.width;
            
            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / AnimationDuration;
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = endPos;
        }
        
        /// <summary>
        /// 从右侧滑入动画
        /// </summary>
        private IEnumerator SlideInFromRight()
        {
            float elapsed = 0f;
            Vector2 startPos = originalPosition + Vector2.right * Screen.width;
            rectTransform.anchoredPosition = startPos;
            canvasGroup.alpha = 1f;
            
            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / AnimationDuration;
                
                t = 1f - Mathf.Pow(1f - t, 3f); // EaseOutCubic
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, originalPosition, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = originalPosition;
        }
        
        /// <summary>
        /// 滑出到右侧动画
        /// </summary>
        private IEnumerator SlideOutToRight()
        {
            float elapsed = 0f;
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = originalPosition + Vector2.right * Screen.width;
            
            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / AnimationDuration;
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = endPos;
        }
        
        #endregion
    }
}