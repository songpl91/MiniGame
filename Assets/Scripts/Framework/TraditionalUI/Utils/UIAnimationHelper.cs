using UnityEngine;
using System.Collections;

namespace Framework.TraditionalUI.Utils
{
    /// <summary>
    /// UI动画辅助工具类
    /// 提供常用的UI动画效果
    /// </summary>
    public static class UIAnimationHelper
    {
        #region 淡入淡出动画
        
        /// <summary>
        /// 淡入动画
        /// </summary>
        /// <param name="canvasGroup">画布组</param>
        /// <param name="duration">持续时间</param>
        /// <param name="onComplete">完成回调</param>
        /// <returns>协程</returns>
        public static Coroutine FadeIn(CanvasGroup canvasGroup, float duration = 0.3f, System.Action onComplete = null)
        {
            if (canvasGroup == null) return null;
            
            return CoroutineRunner.Instance.StartCoroutine(FadeInCoroutine(canvasGroup, duration, onComplete));
        }
        
        /// <summary>
        /// 淡出动画
        /// </summary>
        /// <param name="canvasGroup">画布组</param>
        /// <param name="duration">持续时间</param>
        /// <param name="onComplete">完成回调</param>
        /// <returns>协程</returns>
        public static Coroutine FadeOut(CanvasGroup canvasGroup, float duration = 0.3f, System.Action onComplete = null)
        {
            if (canvasGroup == null) return null;
            
            return CoroutineRunner.Instance.StartCoroutine(FadeOutCoroutine(canvasGroup, duration, onComplete));
        }
        
        private static IEnumerator FadeInCoroutine(CanvasGroup canvasGroup, float duration, System.Action onComplete)
        {
            float startAlpha = canvasGroup.alpha;
            float targetAlpha = 1f;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / duration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
                yield return null;
            }
            
            canvasGroup.alpha = targetAlpha;
            onComplete?.Invoke();
        }
        
        private static IEnumerator FadeOutCoroutine(CanvasGroup canvasGroup, float duration, System.Action onComplete)
        {
            float startAlpha = canvasGroup.alpha;
            float targetAlpha = 0f;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / duration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
                yield return null;
            }
            
            canvasGroup.alpha = targetAlpha;
            onComplete?.Invoke();
        }
        
        #endregion
        
        #region 缩放动画
        
        /// <summary>
        /// 缩放进入动画
        /// </summary>
        /// <param name="transform">变换组件</param>
        /// <param name="duration">持续时间</param>
        /// <param name="onComplete">完成回调</param>
        /// <returns>协程</returns>
        public static Coroutine ScaleIn(Transform transform, float duration = 0.3f, System.Action onComplete = null)
        {
            if (transform == null) return null;
            
            return CoroutineRunner.Instance.StartCoroutine(ScaleInCoroutine(transform, duration, onComplete));
        }
        
        /// <summary>
        /// 缩放退出动画
        /// </summary>
        /// <param name="transform">变换组件</param>
        /// <param name="duration">持续时间</param>
        /// <param name="onComplete">完成回调</param>
        /// <returns>协程</returns>
        public static Coroutine ScaleOut(Transform transform, float duration = 0.3f, System.Action onComplete = null)
        {
            if (transform == null) return null;
            
            return CoroutineRunner.Instance.StartCoroutine(ScaleOutCoroutine(transform, duration, onComplete));
        }
        
        private static IEnumerator ScaleInCoroutine(Transform transform, float duration, System.Action onComplete)
        {
            Vector3 startScale = Vector3.zero;
            Vector3 targetScale = Vector3.one;
            float elapsedTime = 0f;
            
            transform.localScale = startScale;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / duration;
                
                // 使用缓动函数
                float easedProgress = EaseOutBack(progress);
                transform.localScale = Vector3.Lerp(startScale, targetScale, easedProgress);
                
                yield return null;
            }
            
            transform.localScale = targetScale;
            onComplete?.Invoke();
        }
        
        private static IEnumerator ScaleOutCoroutine(Transform transform, float duration, System.Action onComplete)
        {
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = Vector3.zero;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / duration;
                
                // 使用缓动函数
                float easedProgress = EaseInBack(progress);
                transform.localScale = Vector3.Lerp(startScale, targetScale, easedProgress);
                
                yield return null;
            }
            
            transform.localScale = targetScale;
            onComplete?.Invoke();
        }
        
        #endregion
        
        #region 滑动动画
        
        /// <summary>
        /// 从左侧滑入
        /// </summary>
        /// <param name="rectTransform">矩形变换</param>
        /// <param name="duration">持续时间</param>
        /// <param name="onComplete">完成回调</param>
        /// <returns>协程</returns>
        public static Coroutine SlideInFromLeft(RectTransform rectTransform, float duration = 0.3f, System.Action onComplete = null)
        {
            if (rectTransform == null) return null;
            
            return CoroutineRunner.Instance.StartCoroutine(SlideInFromLeftCoroutine(rectTransform, duration, onComplete));
        }
        
        /// <summary>
        /// 从右侧滑入
        /// </summary>
        /// <param name="rectTransform">矩形变换</param>
        /// <param name="duration">持续时间</param>
        /// <param name="onComplete">完成回调</param>
        /// <returns>协程</returns>
        public static Coroutine SlideInFromRight(RectTransform rectTransform, float duration = 0.3f, System.Action onComplete = null)
        {
            if (rectTransform == null) return null;
            
            return CoroutineRunner.Instance.StartCoroutine(SlideInFromRightCoroutine(rectTransform, duration, onComplete));
        }
        
        /// <summary>
        /// 向左滑出
        /// </summary>
        /// <param name="rectTransform">矩形变换</param>
        /// <param name="duration">持续时间</param>
        /// <param name="onComplete">完成回调</param>
        /// <returns>协程</returns>
        public static Coroutine SlideOutToLeft(RectTransform rectTransform, float duration = 0.3f, System.Action onComplete = null)
        {
            if (rectTransform == null) return null;
            
            return CoroutineRunner.Instance.StartCoroutine(SlideOutToLeftCoroutine(rectTransform, duration, onComplete));
        }
        
        /// <summary>
        /// 向右滑出
        /// </summary>
        /// <param name="rectTransform">矩形变换</param>
        /// <param name="duration">持续时间</param>
        /// <param name="onComplete">完成回调</param>
        /// <returns>协程</returns>
        public static Coroutine SlideOutToRight(RectTransform rectTransform, float duration = 0.3f, System.Action onComplete = null)
        {
            if (rectTransform == null) return null;
            
            return CoroutineRunner.Instance.StartCoroutine(SlideOutToRightCoroutine(rectTransform, duration, onComplete));
        }
        
        private static IEnumerator SlideInFromLeftCoroutine(RectTransform rectTransform, float duration, System.Action onComplete)
        {
            Vector2 startPos = new Vector2(-Screen.width, rectTransform.anchoredPosition.y);
            Vector2 targetPos = rectTransform.anchoredPosition;
            float elapsedTime = 0f;
            
            rectTransform.anchoredPosition = startPos;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / duration;
                float easedProgress = EaseOutQuart(progress);
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, easedProgress);
                yield return null;
            }
            
            rectTransform.anchoredPosition = targetPos;
            onComplete?.Invoke();
        }
        
        private static IEnumerator SlideInFromRightCoroutine(RectTransform rectTransform, float duration, System.Action onComplete)
        {
            Vector2 startPos = new Vector2(Screen.width, rectTransform.anchoredPosition.y);
            Vector2 targetPos = rectTransform.anchoredPosition;
            float elapsedTime = 0f;
            
            rectTransform.anchoredPosition = startPos;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / duration;
                float easedProgress = EaseOutQuart(progress);
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, easedProgress);
                yield return null;
            }
            
            rectTransform.anchoredPosition = targetPos;
            onComplete?.Invoke();
        }
        
        private static IEnumerator SlideOutToLeftCoroutine(RectTransform rectTransform, float duration, System.Action onComplete)
        {
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 targetPos = new Vector2(-Screen.width, rectTransform.anchoredPosition.y);
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / duration;
                float easedProgress = EaseInQuart(progress);
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, easedProgress);
                yield return null;
            }
            
            rectTransform.anchoredPosition = targetPos;
            onComplete?.Invoke();
        }
        
        private static IEnumerator SlideOutToRightCoroutine(RectTransform rectTransform, float duration, System.Action onComplete)
        {
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 targetPos = new Vector2(Screen.width, rectTransform.anchoredPosition.y);
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / duration;
                float easedProgress = EaseInQuart(progress);
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, easedProgress);
                yield return null;
            }
            
            rectTransform.anchoredPosition = targetPos;
            onComplete?.Invoke();
        }
        
        #endregion
        
        #region 弹跳动画
        
        /// <summary>
        /// 弹跳动画
        /// </summary>
        /// <param name="transform">变换组件</param>
        /// <param name="bounceScale">弹跳缩放</param>
        /// <param name="duration">持续时间</param>
        /// <param name="onComplete">完成回调</param>
        /// <returns>协程</returns>
        public static Coroutine Bounce(Transform transform, float bounceScale = 1.2f, float duration = 0.3f, System.Action onComplete = null)
        {
            if (transform == null) return null;
            
            return CoroutineRunner.Instance.StartCoroutine(BounceCoroutine(transform, bounceScale, duration, onComplete));
        }
        
        private static IEnumerator BounceCoroutine(Transform transform, float bounceScale, float duration, System.Action onComplete)
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * bounceScale;
            
            float halfDuration = duration * 0.5f;
            float elapsedTime = 0f;
            
            // 放大阶段
            while (elapsedTime < halfDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / halfDuration;
                transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
                yield return null;
            }
            
            elapsedTime = 0f;
            
            // 缩小阶段
            while (elapsedTime < halfDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / halfDuration;
                transform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
                yield return null;
            }
            
            transform.localScale = originalScale;
            onComplete?.Invoke();
        }
        
        #endregion
        
        #region 摇摆动画
        
        /// <summary>
        /// 摇摆动画
        /// </summary>
        /// <param name="transform">变换组件</param>
        /// <param name="angle">摇摆角度</param>
        /// <param name="duration">持续时间</param>
        /// <param name="onComplete">完成回调</param>
        /// <returns>协程</returns>
        public static Coroutine Shake(Transform transform, float angle = 10f, float duration = 0.5f, System.Action onComplete = null)
        {
            if (transform == null) return null;
            
            return CoroutineRunner.Instance.StartCoroutine(ShakeCoroutine(transform, angle, duration, onComplete));
        }
        
        private static IEnumerator ShakeCoroutine(Transform transform, float angle, float duration, System.Action onComplete)
        {
            Vector3 originalRotation = transform.localEulerAngles;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / duration;
                
                // 计算摇摆角度
                float currentAngle = Mathf.Sin(progress * Mathf.PI * 8) * angle * (1 - progress);
                transform.localEulerAngles = originalRotation + new Vector3(0, 0, currentAngle);
                
                yield return null;
            }
            
            transform.localEulerAngles = originalRotation;
            onComplete?.Invoke();
        }
        
        #endregion
        
        #region 缓动函数
        
        /// <summary>
        /// 缓出回弹
        /// </summary>
        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
        
        /// <summary>
        /// 缓入回弹
        /// </summary>
        private static float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            
            return c3 * t * t * t - c1 * t * t;
        }
        
        /// <summary>
        /// 缓出四次方
        /// </summary>
        private static float EaseOutQuart(float t)
        {
            return 1f - Mathf.Pow(1f - t, 4f);
        }
        
        /// <summary>
        /// 缓入四次方
        /// </summary>
        private static float EaseInQuart(float t)
        {
            return t * t * t * t;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 协程运行器
    /// 用于在静态方法中运行协程
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;
        
        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("CoroutineRunner");
                    _instance = go.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}