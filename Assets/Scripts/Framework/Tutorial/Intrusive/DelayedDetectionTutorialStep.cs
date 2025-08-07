using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Framework.Tutorial.Intrusive.Steps
{
    /// <summary>
    /// 延迟检测引导步骤
    /// 基于第一性原理的简化界面切换解决方案
    /// 使用延迟 + 重试机制处理UI状态变化和界面切换
    /// </summary>
    public class DelayedDetectionTutorialStep : TutorialStepBase
    {
        #region 配置参数
        
        [Header("目标配置")]
        [SerializeField] private string targetPath;           // 目标UI路径
        [SerializeField] private string targetTag;            // 目标标签
        [SerializeField] private string hintText;             // 提示文本
        
        [Header("延迟检测配置")]
        [SerializeField] private float initialDelay = 1.0f;   // 初始延迟（界面切换缓冲时间）
        [SerializeField] private float retryInterval = 0.5f;  // 重试间隔
        [SerializeField] private int maxRetryCount = 20;      // 最大重试次数（10秒）
        [SerializeField] private float timeoutDuration = 15f; // 超时时间
        
        [Header("视觉效果")]
        [SerializeField] private bool enableHighlight = true;
        [SerializeField] private Color highlightColor = Color.yellow;
        
        #endregion
        
        #region 私有变量
        
        private Button targetButton;
        private GameObject highlightEffect;
        private Coroutine detectionCoroutine;
        private bool isDetecting = false;
        private int currentRetryCount = 0;
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">步骤ID</param>
        /// <param name="name">步骤名称</param>
        /// <param name="targetPath">目标UI路径或名称</param>
        /// <param name="hint">提示文本</param>
        /// <param name="initialDelay">初始延迟时间</param>
        /// <param name="skipable">是否可跳过</param>
        public DelayedDetectionTutorialStep(string id, string name, string targetPath, string hint = "", 
            float initialDelay = 1.0f, bool skipable = true) 
            : base(id, name, skipable)
        {
            this.targetPath = targetPath;
            this.hintText = hint;
            this.initialDelay = initialDelay;
        }
        
        #endregion
        
        #region 核心方法
        
        /// <summary>
        /// 步骤开始时的具体逻辑
        /// </summary>
        protected override void OnStepStart()
        {
            Debug.Log($"[DelayedDetectionStep] 开始延迟检测引导: {stepName}");
            Debug.Log($"[DelayedDetectionStep] 目标: {targetPath}, 初始延迟: {initialDelay}秒");
            
            // 开始延迟检测协程
            detectionCoroutine = TutorialCoroutineHelper.StartCoroutineStatic(DelayedDetectionCoroutine());
        }
        
        /// <summary>
        /// 延迟检测协程
        /// 核心思想：给界面切换足够的时间，然后持续重试检测
        /// </summary>
        private IEnumerator DelayedDetectionCoroutine()
        {
            isDetecting = true;
            currentRetryCount = 0;
            
            // 第一阶段：初始延迟（给界面切换留出时间）
            Debug.Log($"[DelayedDetectionStep] 等待界面稳定，延迟 {initialDelay} 秒...");
            yield return new WaitForSeconds(initialDelay);
            
            // 第二阶段：持续检测目标元素
            float startTime = Time.time;
            
            while (isDetecting && currentRetryCount < maxRetryCount)
            {
                // 检查是否超时
                if (Time.time - startTime > timeoutDuration)
                {
                    Debug.LogError($"[DelayedDetectionStep] 检测超时: {targetPath}");
                    OnDetectionTimeout();
                    yield break;
                }
                
                // 尝试查找目标
                if (TryFindTarget())
                {
                    Debug.Log($"[DelayedDetectionStep] 成功找到目标: {targetPath} (重试次数: {currentRetryCount})");
                    OnTargetFound();
                    yield break;
                }
                
                // 增加重试计数
                currentRetryCount++;
                Debug.Log($"[DelayedDetectionStep] 未找到目标，重试 {currentRetryCount}/{maxRetryCount}: {targetPath}");
                
                // 等待下次重试
                yield return new WaitForSeconds(retryInterval);
            }
            
            // 达到最大重试次数
            Debug.LogWarning($"[DelayedDetectionStep] 达到最大重试次数: {targetPath}");
            OnMaxRetryReached();
        }
        
        /// <summary>
        /// 尝试查找目标元素
        /// 使用多种查找策略提高成功率
        /// </summary>
        private bool TryFindTarget()
        {
            targetButton = null;
            
            // 策略1: 直接路径查找
            GameObject targetObj = GameObject.Find(targetPath);
            if (targetObj != null)
            {
                targetButton = targetObj.GetComponent<Button>();
                if (targetButton != null && targetButton.gameObject.activeInHierarchy)
                {
                    Debug.Log($"[DelayedDetectionStep] 通过路径找到目标: {targetPath}");
                    return true;
                }
            }
            
            // 策略2: 在所有Canvas中查找
            Canvas[] allCanvases = Object.FindObjectsOfType<Canvas>();
            foreach (var canvas in allCanvases)
            {
                if (!canvas.gameObject.activeInHierarchy) continue;
                
                Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
                foreach (var button in buttons)
                {
                    if ((button.name == targetPath || button.gameObject.name == targetPath) 
                        && button.gameObject.activeInHierarchy)
                    {
                        targetButton = button;
                        Debug.Log($"[DelayedDetectionStep] 在Canvas中找到目标: {targetPath}");
                        return true;
                    }
                }
            }
            
            // 策略3: 通过标签查找
            if (!string.IsNullOrEmpty(targetTag))
            {
                GameObject taggedObj = GameObject.FindGameObjectWithTag(targetTag);
                if (taggedObj != null && taggedObj.activeInHierarchy)
                {
                    targetButton = taggedObj.GetComponent<Button>();
                    if (targetButton != null)
                    {
                        Debug.Log($"[DelayedDetectionStep] 通过标签找到目标: {targetTag}");
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 找到目标时的处理
        /// </summary>
        private void OnTargetFound()
        {
            if (targetButton == null) return;
            
            // 添加点击监听
            targetButton.onClick.AddListener(OnButtonClicked);
            
            // 创建高亮效果
            if (enableHighlight)
            {
                CreateHighlightEffect();
            }
            
            // 显示提示
            ShowHint();
            
            Debug.Log($"[DelayedDetectionStep] 目标准备完成: {stepName}");
        }
        
        /// <summary>
        /// 检测超时处理
        /// </summary>
        private void OnDetectionTimeout()
        {
            Debug.LogError($"[DelayedDetectionStep] 检测超时，自动完成步骤: {stepName}");
            CompleteStep();
        }
        
        /// <summary>
        /// 达到最大重试次数处理
        /// </summary>
        private void OnMaxRetryReached()
        {
            Debug.LogWarning($"[DelayedDetectionStep] 达到最大重试次数，自动完成步骤: {stepName}");
            CompleteStep();
        }
        
        #endregion
        
        #region 视觉效果
        
        /// <summary>
        /// 创建高亮效果
        /// </summary>
        private void CreateHighlightEffect()
        {
            if (targetButton == null) return;
            
            // 移除旧的高亮效果
            RemoveHighlightEffect();
            
            // 创建新的高亮效果
            highlightEffect = new GameObject("DelayedDetectionHighlight");
            highlightEffect.transform.SetParent(targetButton.transform, false);
            
            // 添加Image组件作为高亮效果
            Image highlight = highlightEffect.AddComponent<Image>();
            highlight.color = new Color(highlightColor.r, highlightColor.g, highlightColor.b, 0.3f);
            highlight.raycastTarget = false; // 不阻挡点击
            
            // 设置RectTransform使其覆盖按钮
            RectTransform highlightRect = highlightEffect.GetComponent<RectTransform>();
            highlightRect.anchorMin = Vector2.zero;
            highlightRect.anchorMax = Vector2.one;
            highlightRect.offsetMin = new Vector2(-3, -3);
            highlightRect.offsetMax = new Vector2(3, 3);
            
            // 添加呼吸动画
            TutorialCoroutineHelper.StartCoroutineStatic(BreathingAnimation(highlight));
        }
        
        /// <summary>
        /// 呼吸动画协程
        /// </summary>
        private IEnumerator BreathingAnimation(Image image)
        {
            while (highlightEffect != null && !isCompleted)
            {
                // 淡入
                for (float t = 0; t < 1; t += Time.deltaTime * 1.5f)
                {
                    if (image == null) yield break;
                    Color color = image.color;
                    color.a = Mathf.Lerp(0.2f, 0.5f, t);
                    image.color = color;
                    yield return null;
                }
                
                // 淡出
                for (float t = 0; t < 1; t += Time.deltaTime * 1.5f)
                {
                    if (image == null) yield break;
                    Color color = image.color;
                    color.a = Mathf.Lerp(0.5f, 0.2f, t);
                    image.color = color;
                    yield return null;
                }
            }
        }
        
        /// <summary>
        /// 显示提示
        /// </summary>
        private void ShowHint()
        {
            if (!string.IsNullOrEmpty(hintText))
            {
                Debug.Log($"[延迟检测引导提示] {hintText}");
                // 这里可以显示UI提示框
            }
        }
        
        /// <summary>
        /// 移除高亮效果
        /// </summary>
        private void RemoveHighlightEffect()
        {
            if (highlightEffect != null)
            {
                Object.Destroy(highlightEffect);
                highlightEffect = null;
            }
        }
        
        #endregion
        
        #region 事件处理
        
        /// <summary>
        /// 按钮点击回调
        /// </summary>
        private void OnButtonClicked()
        {
            Debug.Log($"[DelayedDetectionStep] 按钮被点击: {targetPath}");
            CompleteStep();
        }
        
        #endregion
        
        #region 生命周期
        
        /// <summary>
        /// 步骤完成时的处理
        /// </summary>
        protected override void OnStepComplete()
        {
            Debug.Log($"[DelayedDetectionStep] 步骤完成: {stepName}");
            StopDetection();
            RemoveHighlightEffect();
        }
        
        /// <summary>
        /// 步骤被跳过时的处理
        /// </summary>
        protected override void OnStepSkip()
        {
            Debug.Log($"[DelayedDetectionStep] 步骤被跳过: {stepName}");
            StopDetection();
            RemoveHighlightEffect();
        }
        
        /// <summary>
        /// 步骤清理时的处理
        /// </summary>
        protected override void OnStepCleanup()
        {
            // 移除按钮监听
            if (targetButton != null)
            {
                targetButton.onClick.RemoveListener(OnButtonClicked);
            }
            
            StopDetection();
            RemoveHighlightEffect();
        }
        
        /// <summary>
        /// 停止检测
        /// </summary>
        private void StopDetection()
        {
            isDetecting = false;
            
            if (detectionCoroutine != null)
            {
                TutorialCoroutineHelper.StopCoroutineStatic(detectionCoroutine);
                detectionCoroutine = null;
            }
        }
        
        #endregion
        
        #region 公共接口
        
        /// <summary>
        /// 设置检测参数
        /// </summary>
        /// <param name="initialDelay">初始延迟</param>
        /// <param name="retryInterval">重试间隔</param>
        /// <param name="maxRetryCount">最大重试次数</param>
        public void SetDetectionParams(float initialDelay, float retryInterval, int maxRetryCount)
        {
            this.initialDelay = initialDelay;
            this.retryInterval = retryInterval;
            this.maxRetryCount = maxRetryCount;
        }
        
        /// <summary>
        /// 设置目标标签
        /// </summary>
        /// <param name="tag">标签名称</param>
        public void SetTargetTag(string tag)
        {
            this.targetTag = tag;
        }
        
        /// <summary>
        /// 获取检测状态信息
        /// </summary>
        /// <returns>状态信息</returns>
        public string GetDetectionStatus()
        {
            return $"检测状态: {(isDetecting ? "进行中" : "已停止")}\n" +
                   $"重试次数: {currentRetryCount}/{maxRetryCount}\n" +
                   $"目标路径: {targetPath}\n" +
                   $"是否找到: {(targetButton != null ? "是" : "否")}";
        }
        
        #endregion
    }
}