using System;
using System.Collections;
using UnityEngine;
using Framework.SampleUI.Core;

namespace Framework.SampleUI.Components
{
    /// <summary>
    /// 动画组件
    /// 为UI面板提供高级动画功能
    /// </summary>
    public class AnimationComponent : SampleUIComponent
    {
        #region 字段和属性
        
        /// <summary>
        /// 动画配置
        /// </summary>
        public AnimationConfig Config { get; set; } = new AnimationConfig();
        
        /// <summary>
        /// 是否正在播放动画
        /// </summary>
        public bool IsPlaying { get; private set; }
        
        /// <summary>
        /// 当前动画协程
        /// </summary>
        private Coroutine currentAnimation;
        
        /// <summary>
        /// 面板的MonoBehaviour引用
        /// </summary>
        private MonoBehaviour panelMono;
        
        /// <summary>
        /// 面板的RectTransform
        /// </summary>
        private RectTransform panelRect;
        
        /// <summary>
        /// 面板的CanvasGroup
        /// </summary>
        private CanvasGroup panelCanvasGroup;
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 动画开始事件
        /// </summary>
        public event Action<string> OnAnimationStart;
        
        /// <summary>
        /// 动画完成事件
        /// </summary>
        public event Action<string> OnAnimationComplete;
        
        #endregion
        
        #region 初始化
        
        protected override void OnInitialize()
        {
            // 获取面板组件引用
            if (OwnerPanel is MonoBehaviour mono)
            {
                panelMono = mono;
                panelRect = mono.GetComponent<RectTransform>();
                panelCanvasGroup = mono.GetComponent<CanvasGroup>();
                
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = mono.gameObject.AddComponent<CanvasGroup>();
                }
            }
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 播放自定义动画
        /// </summary>
        /// <param name="animationType">动画类型</param>
        /// <param name="duration">动画时长</param>
        /// <param name="onComplete">完成回调</param>
        public void PlayAnimation(CustomAnimationType animationType, float duration = 0.3f, Action onComplete = null)
        {
            if (panelMono == null)
            {
                Debug.LogError("[AnimationComponent] 面板MonoBehaviour引用为空");
                return;
            }
            
            // 停止当前动画
            StopCurrentAnimation();
            
            // 播放新动画
            currentAnimation = panelMono.StartCoroutine(DoCustomAnimation(animationType, duration, onComplete));
        }
        
        /// <summary>
        /// 播放弹性动画
        /// </summary>
        /// <param name="targetScale">目标缩放</param>
        /// <param name="duration">动画时长</param>
        /// <param name="onComplete">完成回调</param>
        public void PlayBounceAnimation(Vector3 targetScale, float duration = 0.5f, Action onComplete = null)
        {
            if (panelMono == null) return;
            
            StopCurrentAnimation();
            currentAnimation = panelMono.StartCoroutine(DoBounceAnimation(targetScale, duration, onComplete));
        }
        
        /// <summary>
        /// 播放摇摆动画
        /// </summary>
        /// <param name="intensity">摇摆强度</param>
        /// <param name="duration">动画时长</param>
        /// <param name="onComplete">完成回调</param>
        public void PlayShakeAnimation(float intensity = 10f, float duration = 0.5f, Action onComplete = null)
        {
            if (panelMono == null) return;
            
            StopCurrentAnimation();
            currentAnimation = panelMono.StartCoroutine(DoShakeAnimation(intensity, duration, onComplete));
        }
        
        /// <summary>
        /// 播放脉冲动画
        /// </summary>
        /// <param name="pulseScale">脉冲缩放</param>
        /// <param name="duration">动画时长</param>
        /// <param name="onComplete">完成回调</param>
        public void PlayPulseAnimation(float pulseScale = 1.1f, float duration = 1f, Action onComplete = null)
        {
            if (panelMono == null) return;
            
            StopCurrentAnimation();
            currentAnimation = panelMono.StartCoroutine(DoPulseAnimation(pulseScale, duration, onComplete));
        }
        
        /// <summary>
        /// 停止当前动画
        /// </summary>
        public void StopAnimation()
        {
            StopCurrentAnimation();
        }
        
        #endregion
        
        #region 动画实现
        
        /// <summary>
        /// 执行自定义动画
        /// </summary>
        private IEnumerator DoCustomAnimation(CustomAnimationType animationType, float duration, Action onComplete)
        {
            IsPlaying = true;
            OnAnimationStart?.Invoke(animationType.ToString());
            
            switch (animationType)
            {
                case CustomAnimationType.FadeInOut:
                    yield return StartCoroutine(DoFadeInOut(duration));
                    break;
                case CustomAnimationType.ScaleBounce:
                    yield return StartCoroutine(DoScaleBounce(duration));
                    break;
                case CustomAnimationType.SlideInFromLeft:
                    yield return StartCoroutine(DoSlideInFromLeft(duration));
                    break;
                case CustomAnimationType.SlideInFromRight:
                    yield return StartCoroutine(DoSlideInFromRight(duration));
                    break;
                case CustomAnimationType.RotateIn:
                    yield return StartCoroutine(DoRotateIn(duration));
                    break;
                case CustomAnimationType.FlipIn:
                    yield return StartCoroutine(DoFlipIn(duration));
                    break;
            }
            
            IsPlaying = false;
            OnAnimationComplete?.Invoke(animationType.ToString());
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 淡入淡出动画
        /// </summary>
        private IEnumerator DoFadeInOut(float duration)
        {
            float halfDuration = duration * 0.5f;
            
            // 淡入
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / halfDuration);
                yield return null;
            }
            
            // 淡出
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                panelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / halfDuration);
                yield return null;
            }
            
            panelCanvasGroup.alpha = 1f; // 恢复原状
        }
        
        /// <summary>
        /// 缩放弹跳动画
        /// </summary>
        private IEnumerator DoScaleBounce(float duration)
        {
            Vector3 originalScale = panelRect.localScale;
            Vector3 targetScale = originalScale * 1.2f;
            
            float halfDuration = duration * 0.5f;
            
            // 放大
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                t = EaseOutBounce(t);
                panelRect.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            // 缩小
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                t = EaseInBounce(t);
                panelRect.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            panelRect.localScale = originalScale;
        }
        
        /// <summary>
        /// 从左滑入动画
        /// </summary>
        private IEnumerator DoSlideInFromLeft(float duration)
        {
            Vector2 originalPos = panelRect.anchoredPosition;
            Vector2 startPos = originalPos + Vector2.left * Screen.width;
            
            panelRect.anchoredPosition = startPos;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = EaseOutCubic(t);
                panelRect.anchoredPosition = Vector2.Lerp(startPos, originalPos, t);
                yield return null;
            }
            
            panelRect.anchoredPosition = originalPos;
        }
        
        /// <summary>
        /// 从右滑入动画
        /// </summary>
        private IEnumerator DoSlideInFromRight(float duration)
        {
            Vector2 originalPos = panelRect.anchoredPosition;
            Vector2 startPos = originalPos + Vector2.right * Screen.width;
            
            panelRect.anchoredPosition = startPos;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = EaseOutCubic(t);
                panelRect.anchoredPosition = Vector2.Lerp(startPos, originalPos, t);
                yield return null;
            }
            
            panelRect.anchoredPosition = originalPos;
        }
        
        /// <summary>
        /// 旋转进入动画
        /// </summary>
        private IEnumerator DoRotateIn(float duration)
        {
            Vector3 originalRotation = panelRect.localEulerAngles;
            Vector3 startRotation = originalRotation + Vector3.forward * 360f;
            
            panelRect.localEulerAngles = startRotation;
            panelRect.localScale = Vector3.zero;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = EaseOutBack(t);
                
                panelRect.localEulerAngles = Vector3.Lerp(startRotation, originalRotation, t);
                panelRect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                yield return null;
            }
            
            panelRect.localEulerAngles = originalRotation;
            panelRect.localScale = Vector3.one;
        }
        
        /// <summary>
        /// 翻转进入动画
        /// </summary>
        private IEnumerator DoFlipIn(float duration)
        {
            Vector3 originalScale = panelRect.localScale;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                float scaleX = Mathf.Cos(t * Mathf.PI);
                panelRect.localScale = new Vector3(scaleX, originalScale.y, originalScale.z);
                
                yield return null;
            }
            
            panelRect.localScale = originalScale;
        }
        
        /// <summary>
        /// 弹跳动画
        /// </summary>
        private IEnumerator DoBounceAnimation(Vector3 targetScale, float duration, Action onComplete)
        {
            IsPlaying = true;
            OnAnimationStart?.Invoke("Bounce");
            
            Vector3 originalScale = panelRect.localScale;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = EaseOutBounce(t);
                
                panelRect.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            // 回弹
            elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration * 0.5f);
                
                panelRect.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            panelRect.localScale = originalScale;
            
            IsPlaying = false;
            OnAnimationComplete?.Invoke("Bounce");
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 摇摆动画
        /// </summary>
        private IEnumerator DoShakeAnimation(float intensity, float duration, Action onComplete)
        {
            IsPlaying = true;
            OnAnimationStart?.Invoke("Shake");
            
            Vector2 originalPos = panelRect.anchoredPosition;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                
                float x = UnityEngine.Random.Range(-intensity, intensity);
                float y = UnityEngine.Random.Range(-intensity, intensity);
                
                panelRect.anchoredPosition = originalPos + new Vector2(x, y);
                
                yield return null;
            }
            
            panelRect.anchoredPosition = originalPos;
            
            IsPlaying = false;
            OnAnimationComplete?.Invoke("Shake");
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 脉冲动画
        /// </summary>
        private IEnumerator DoPulseAnimation(float pulseScale, float duration, Action onComplete)
        {
            IsPlaying = true;
            OnAnimationStart?.Invoke("Pulse");
            
            Vector3 originalScale = panelRect.localScale;
            Vector3 targetScale = originalScale * pulseScale;
            
            float halfDuration = duration * 0.5f;
            
            while (true)
            {
                // 放大
                float elapsed = 0f;
                while (elapsed < halfDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / halfDuration;
                    panelRect.localScale = Vector3.Lerp(originalScale, targetScale, t);
                    yield return null;
                }
                
                // 缩小
                elapsed = 0f;
                while (elapsed < halfDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / halfDuration;
                    panelRect.localScale = Vector3.Lerp(targetScale, originalScale, t);
                    yield return null;
                }
            }
        }
        
        #endregion
        
        #region 缓动函数
        
        /// <summary>
        /// EaseOutBounce缓动函数
        /// </summary>
        private float EaseOutBounce(float t)
        {
            if (t < 1f / 2.75f)
            {
                return 7.5625f * t * t;
            }
            else if (t < 2f / 2.75f)
            {
                return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
            }
            else if (t < 2.5f / 2.75f)
            {
                return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
            }
            else
            {
                return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
            }
        }
        
        /// <summary>
        /// EaseInBounce缓动函数
        /// </summary>
        private float EaseInBounce(float t)
        {
            return 1f - EaseOutBounce(1f - t);
        }
        
        /// <summary>
        /// EaseOutCubic缓动函数
        /// </summary>
        private float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }
        
        /// <summary>
        /// EaseOutBack缓动函数
        /// </summary>
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 停止当前动画
        /// </summary>
        private void StopCurrentAnimation()
        {
            if (currentAnimation != null && panelMono != null)
            {
                panelMono.StopCoroutine(currentAnimation);
                currentAnimation = null;
                IsPlaying = false;
            }
        }
        
        #endregion
        
        #region 销毁
        
        protected override void OnDestroyed()
        {
            StopCurrentAnimation();
            panelMono = null;
            panelRect = null;
            panelCanvasGroup = null;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 自定义动画类型
    /// </summary>
    public enum CustomAnimationType
    {
        FadeInOut,          // 淡入淡出
        ScaleBounce,        // 缩放弹跳
        SlideInFromLeft,    // 从左滑入
        SlideInFromRight,   // 从右滑入
        RotateIn,           // 旋转进入
        FlipIn              // 翻转进入
    }
    
    /// <summary>
    /// 动画配置
    /// </summary>
    [System.Serializable]
    public class AnimationConfig
    {
        [Header("基础设置")]
        public float defaultDuration = 0.3f;
        public bool enableEasing = true;
        
        [Header("特效设置")]
        public bool enableParticles = false;
        public bool enableSound = false;
        
        [Header("性能设置")]
        public bool useUnscaledTime = false;
        public int maxConcurrentAnimations = 3;
    }
}