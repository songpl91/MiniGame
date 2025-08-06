using UnityEngine;
using System;
using System.Collections;

namespace Framework.SplUI.Core
{
    /// <summary>
    /// SplUI动画控制器
    /// 负责处理面板的显示和隐藏动画
    /// </summary>
    public class SplUIAnimator : MonoBehaviour
    {
        [Header("动画设置")]
        [SerializeField]
        [Tooltip("显示动画类型")]
        private SplUIAnimationType showAnimationType = SplUIAnimationType.Fade;
        
        [SerializeField]
        [Tooltip("隐藏动画类型")]
        private SplUIAnimationType hideAnimationType = SplUIAnimationType.Fade;
        
        [SerializeField]
        [Tooltip("动画持续时间")]
        [Range(0.1f, 2f)]
        private float animationDuration = 0.3f;
        
        [SerializeField]
        [Tooltip("动画曲线")]
        private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("缩放动画设置")]
        [SerializeField]
        [Tooltip("起始缩放")]
        private Vector3 startScale = Vector3.zero;
        
        [SerializeField]
        [Tooltip("结束缩放")]
        private Vector3 endScale = Vector3.one;
        
        [Header("滑动动画设置")]
        [SerializeField]
        [Tooltip("滑动方向")]
        private SlideDirection slideDirection = SlideDirection.Bottom;
        
        [SerializeField]
        [Tooltip("滑动距离")]
        private float slideDistance = 500f;
        
        // 组件引用
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        
        // 动画状态
        private bool isAnimating = false;
        private Coroutine currentAnimation;
        
        // 原始状态
        private Vector2 originalPosition;
        private Vector3 originalScale;
        private float originalAlpha;
        
        /// <summary>
        /// 滑动方向枚举
        /// </summary>
        public enum SlideDirection
        {
            Left,
            Right,
            Top,
            Bottom
        }
        
        /// <summary>
        /// 是否正在播放动画
        /// </summary>
        public bool IsAnimating => isAnimating;
        
        /// <summary>
        /// 显示动画类型
        /// </summary>
        public SplUIAnimationType ShowAnimationType
        {
            get => showAnimationType;
            set => showAnimationType = value;
        }
        
        /// <summary>
        /// 隐藏动画类型
        /// </summary>
        public SplUIAnimationType HideAnimationType
        {
            get => hideAnimationType;
            set => hideAnimationType = value;
        }
        
        /// <summary>
        /// 动画持续时间
        /// </summary>
        public float AnimationDuration
        {
            get => animationDuration;
            set => animationDuration = Mathf.Max(0.1f, value);
        }
        
        /// <summary>
        /// Unity生命周期：Awake
        /// </summary>
        private void Awake()
        {
            InitializeComponents();
            SaveOriginalState();
        }
        
        /// <summary>
        /// 初始化组件引用
        /// </summary>
        private void InitializeComponents()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        /// <summary>
        /// 保存原始状态
        /// </summary>
        private void SaveOriginalState()
        {
            if (rectTransform != null)
            {
                originalPosition = rectTransform.anchoredPosition;
                originalScale = rectTransform.localScale;
            }
            
            if (canvasGroup != null)
            {
                originalAlpha = canvasGroup.alpha;
            }
        }
        
        /// <summary>
        /// 播放显示动画
        /// </summary>
        /// <param name="onComplete">完成回调</param>
        public void PlayShowAnimation(Action onComplete = null)
        {
            if (isAnimating)
            {
                StopCurrentAnimation();
            }
            
            currentAnimation = StartCoroutine(PlayAnimation(showAnimationType, true, onComplete));
        }
        
        /// <summary>
        /// 播放隐藏动画
        /// </summary>
        /// <param name="onComplete">完成回调</param>
        public void PlayHideAnimation(Action onComplete = null)
        {
            if (isAnimating)
            {
                StopCurrentAnimation();
            }
            
            currentAnimation = StartCoroutine(PlayAnimation(hideAnimationType, false, onComplete));
        }
        
        /// <summary>
        /// 立即显示（无动画）
        /// </summary>
        public void ShowImmediate()
        {
            StopCurrentAnimation();
            SetToShowState();
        }
        
        /// <summary>
        /// 立即隐藏（无动画）
        /// </summary>
        public void HideImmediate()
        {
            StopCurrentAnimation();
            SetToHideState();
        }
        
        /// <summary>
        /// 停止当前动画
        /// </summary>
        public void StopCurrentAnimation()
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                currentAnimation = null;
            }
            isAnimating = false;
        }
        
        /// <summary>
        /// 播放动画协程
        /// </summary>
        /// <param name="animationType">动画类型</param>
        /// <param name="isShow">是否为显示动画</param>
        /// <param name="onComplete">完成回调</param>
        /// <returns>协程</returns>
        private IEnumerator PlayAnimation(SplUIAnimationType animationType, bool isShow, Action onComplete)
        {
            isAnimating = true;
            
            // 设置初始状态
            if (isShow)
            {
                SetToHideState(animationType);
                gameObject.SetActive(true);
            }
            
            float elapsedTime = 0f;
            
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / animationDuration;
                float curveValue = animationCurve.Evaluate(progress);
                
                if (!isShow)
                {
                    curveValue = 1f - curveValue;
                }
                
                ApplyAnimationProgress(animationType, curveValue);
                
                yield return null;
            }
            
            // 设置最终状态
            if (isShow)
            {
                SetToShowState();
            }
            else
            {
                SetToHideState();
                gameObject.SetActive(false);
            }
            
            isAnimating = false;
            currentAnimation = null;
            
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 应用动画进度
        /// </summary>
        /// <param name="animationType">动画类型</param>
        /// <param name="progress">进度值（0-1）</param>
        private void ApplyAnimationProgress(SplUIAnimationType animationType, float progress)
        {
            switch (animationType)
            {
                case SplUIAnimationType.None:
                    break;
                    
                case SplUIAnimationType.Fade:
                    ApplyFadeProgress(progress);
                    break;
                    
                case SplUIAnimationType.Scale:
                    ApplyScaleProgress(progress);
                    break;
                    
                case SplUIAnimationType.Slide:
                    ApplySlideProgress(progress);
                    break;
                    
                case SplUIAnimationType.FadeScale:
                    ApplyFadeProgress(progress);
                    ApplyScaleProgress(progress);
                    break;
            }
        }
        
        /// <summary>
        /// 应用淡入淡出进度
        /// </summary>
        /// <param name="progress">进度值</param>
        private void ApplyFadeProgress(float progress)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, originalAlpha, progress);
            }
        }
        
        /// <summary>
        /// 应用缩放进度
        /// </summary>
        /// <param name="progress">进度值</param>
        private void ApplyScaleProgress(float progress)
        {
            if (rectTransform != null)
            {
                Vector3 currentScale = Vector3.Lerp(startScale, endScale, progress);
                rectTransform.localScale = currentScale;
            }
        }
        
        /// <summary>
        /// 应用滑动进度
        /// </summary>
        /// <param name="progress">进度值</param>
        private void ApplySlideProgress(float progress)
        {
            if (rectTransform != null)
            {
                Vector2 offset = GetSlideOffset();
                Vector2 startPosition = (Vector2)originalPosition + offset;
                Vector2 currentPosition = Vector2.Lerp(startPosition, (Vector2)originalPosition, progress);
                rectTransform.anchoredPosition = currentPosition;
            }
        }
        
        /// <summary>
        /// 获取滑动偏移
        /// </summary>
        /// <returns>偏移向量</returns>
        private Vector2 GetSlideOffset()
        {
            switch (slideDirection)
            {
                case SlideDirection.Left:
                    return Vector2.left * slideDistance;
                case SlideDirection.Right:
                    return Vector2.right * slideDistance;
                case SlideDirection.Top:
                    return Vector2.up * slideDistance;
                case SlideDirection.Bottom:
                    return Vector2.down * slideDistance;
                default:
                    return Vector2.zero;
            }
        }
        
        /// <summary>
        /// 设置为显示状态
        /// </summary>
        private void SetToShowState()
        {
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = originalPosition;
                rectTransform.localScale = originalScale;
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = originalAlpha;
            }
        }
        
        /// <summary>
        /// 设置为隐藏状态
        /// </summary>
        /// <param name="animationType">动画类型</param>
        private void SetToHideState(SplUIAnimationType animationType = SplUIAnimationType.None)
        {
            if (animationType == SplUIAnimationType.None)
            {
                animationType = hideAnimationType;
            }
            
            switch (animationType)
            {
                case SplUIAnimationType.Fade:
                case SplUIAnimationType.FadeScale:
                    if (canvasGroup != null)
                        canvasGroup.alpha = 0f;
                    break;
            }
            
            switch (animationType)
            {
                case SplUIAnimationType.Scale:
                case SplUIAnimationType.FadeScale:
                    if (rectTransform != null)
                        rectTransform.localScale = startScale;
                    break;
            }
            
            switch (animationType)
            {
                case SplUIAnimationType.Slide:
                    if (rectTransform != null)
                    {
                        Vector2 offset = GetSlideOffset();
                        rectTransform.anchoredPosition = (Vector2)originalPosition + offset;
                    }
                    break;
            }
        }
    }
}