using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Framework.Tutorial.Intrusive
{
    /// <summary>
    /// 侵入式引导管理器
    /// 负责执行具体的引导步骤序列
    /// 与TutorialTriggerSystem配合使用，实现完整的自动引导系统
    /// </summary>
    public class IntrusiveTutorialManager : MonoBehaviour
    {
        [Header("引导配置")]
        [SerializeField] private bool enableTutorial = true;
        [SerializeField] private bool debugMode = false;
        [SerializeField] private float stepTransitionDelay = 0.5f; // 步骤间过渡延迟
        
        // 单例实例
        private static IntrusiveTutorialManager _instance;
        public static IntrusiveTutorialManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<IntrusiveTutorialManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("IntrusiveTutorialManager");
                        _instance = go.AddComponent<IntrusiveTutorialManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        // 当前执行状态
        private List<TutorialStepBase> currentSteps;
        private int currentStepIndex = 0;
        private bool isExecuting = false;
        private string currentTutorialId = "";
        
        // 事件回调
        public System.Action<string> OnTutorialStart;
        public System.Action<string> OnTutorialComplete;
        public System.Action<string> OnTutorialStop;
        public System.Action<string, string> OnStepStart; // tutorialId, stepId
        public System.Action<string, string> OnStepComplete; // tutorialId, stepId
        
        /// <summary>
        /// 引导系统是否启用
        /// </summary>
        public bool IsEnabled => enableTutorial;
        
        /// <summary>
        /// 是否有正在执行的引导
        /// </summary>
        public bool IsExecuting => isExecuting;
        
        /// <summary>
        /// 当前引导ID
        /// </summary>
        public string CurrentTutorialId => currentTutorialId;
        
        /// <summary>
        /// 当前步骤索引
        /// </summary>
        public int CurrentStepIndex => currentStepIndex;
        
        /// <summary>
        /// 总步骤数
        /// </summary>
        public int TotalSteps => currentSteps?.Count ?? 0;
        
        /// <summary>
        /// 当前步骤
        /// </summary>
        public TutorialStepBase CurrentStep => 
            currentSteps != null && currentStepIndex >= 0 && currentStepIndex < currentSteps.Count 
                ? currentSteps[currentStepIndex] : null;
        
        void Awake()
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
        
        /// <summary>
        /// 开始执行引导步骤序列
        /// </summary>
        /// <param name="tutorialId">引导ID</param>
        /// <param name="steps">引导步骤列表</param>
        /// <returns>是否成功开始</returns>
        public bool StartTutorial(string tutorialId, List<TutorialStepBase> steps)
        {
            if (!enableTutorial)
            {
                LogDebug("引导系统已禁用");
                return false;
            }
            
            if (isExecuting)
            {
                LogWarning($"已有引导正在执行: {currentTutorialId}，停止当前引导");
                StopCurrentTutorial();
            }
            
            if (steps == null || steps.Count == 0)
            {
                LogError($"引导 {tutorialId} 的步骤列表为空");
                return false;
            }
            
            currentTutorialId = tutorialId;
            currentSteps = new List<TutorialStepBase>(steps);
            currentStepIndex = 0;
            isExecuting = true;
            
            LogDebug($"开始执行引导: {tutorialId}，共 {steps.Count} 个步骤");
            
            OnTutorialStart?.Invoke(tutorialId);
            
            // 开始执行第一个步骤
            StartCoroutine(ExecuteStepsCoroutine());
            
            return true;
        }
        
        /// <summary>
        /// 停止当前引导
        /// </summary>
        public void StopCurrentTutorial()
        {
            if (!isExecuting) return;
            
            LogDebug($"停止引导: {currentTutorialId}");
            
            // 清理当前步骤
            CurrentStep?.Cleanup();
            
            string stoppedTutorialId = currentTutorialId;
            
            // 重置状态
            ResetState();
            
            OnTutorialStop?.Invoke(stoppedTutorialId);
        }
        
        /// <summary>
        /// 跳过当前步骤
        /// </summary>
        public void SkipCurrentStep()
        {
            if (!isExecuting || CurrentStep == null) return;
            
            LogDebug($"跳过步骤: {CurrentStep.StepId}");
            CurrentStep.SkipStep();
        }
        
        /// <summary>
        /// 跳转到指定步骤
        /// </summary>
        /// <param name="stepIndex">步骤索引</param>
        public void JumpToStep(int stepIndex)
        {
            if (!isExecuting || currentSteps == null) return;
            
            if (stepIndex < 0 || stepIndex >= currentSteps.Count)
            {
                LogError($"无效的步骤索引: {stepIndex}");
                return;
            }
            
            // 清理当前步骤
            CurrentStep?.Cleanup();
            
            currentStepIndex = stepIndex;
            LogDebug($"跳转到步骤 {stepIndex}: {CurrentStep?.StepId}");
            
            // 重新开始执行
            StartCoroutine(ExecuteStepsCoroutine());
        }
        
        /// <summary>
        /// 设置引导系统启用状态
        /// </summary>
        /// <param name="enabled">是否启用</param>
        public void SetEnabled(bool enabled)
        {
            enableTutorial = enabled;
            
            if (!enabled && isExecuting)
            {
                StopCurrentTutorial();
            }
            
            LogDebug($"引导系统 {(enabled ? "启用" : "禁用")}");
        }
        
        /// <summary>
        /// 获取引导进度信息
        /// </summary>
        /// <returns>进度信息字符串</returns>
        public string GetProgressInfo()
        {
            if (!isExecuting) return "无活动引导";
            
            return $"引导: {currentTutorialId} | 步骤: {currentStepIndex + 1}/{TotalSteps} | 当前: {CurrentStep?.StepName}";
        }
        
        /// <summary>
        /// 执行步骤协程
        /// </summary>
        private IEnumerator ExecuteStepsCoroutine()
        {
            while (isExecuting && currentStepIndex < currentSteps.Count)
            {
                var currentStep = currentSteps[currentStepIndex];
                
                LogDebug($"开始执行步骤 {currentStepIndex + 1}/{currentSteps.Count}: {currentStep.StepId} - {currentStep.StepName}");
                
                OnStepStart?.Invoke(currentTutorialId, currentStep.StepId);
                
                // 执行步骤
                bool stepCompleted = false;
                currentStep.ExecuteStep(() => {
                    stepCompleted = true;
                });
                
                // 等待步骤完成
                yield return new WaitUntil(() => stepCompleted);
                
                if (!isExecuting) break; // 检查是否被中断
                
                LogDebug($"步骤完成: {currentStep.StepId}");
                OnStepComplete?.Invoke(currentTutorialId, currentStep.StepId);
                
                // 移动到下一步
                currentStepIndex++;
                
                // 步骤间延迟
                if (stepTransitionDelay > 0 && currentStepIndex < currentSteps.Count)
                {
                    yield return new WaitForSeconds(stepTransitionDelay);
                }
            }
            
            // 所有步骤完成
            if (isExecuting)
            {
                CompleteTutorial();
            }
        }
        
        /// <summary>
        /// 完成引导
        /// </summary>
        private void CompleteTutorial()
        {
            LogDebug($"引导完成: {currentTutorialId}");
            
            string completedTutorialId = currentTutorialId;
            
            // 重置状态
            ResetState();
            
            OnTutorialComplete?.Invoke(completedTutorialId);
        }
        
        /// <summary>
        /// 重置状态
        /// </summary>
        private void ResetState()
        {
            // 清理所有步骤
            if (currentSteps != null)
            {
                foreach (var step in currentSteps)
                {
                    step.Cleanup();
                }
            }
            
            currentSteps = null;
            currentStepIndex = 0;
            isExecuting = false;
            currentTutorialId = "";
        }
        
        #region 调试功能
        
        /// <summary>
        /// 获取系统状态信息
        /// </summary>
        /// <returns>状态信息</returns>
        public string GetSystemInfo()
        {
            var info = "=== 侵入式引导管理器状态 ===\n";
            info += $"系统启用: {enableTutorial}\n";
            info += $"正在执行: {isExecuting}\n";
            info += $"当前引导: {currentTutorialId}\n";
            info += $"当前步骤: {currentStepIndex + 1}/{TotalSteps}\n";
            
            if (CurrentStep != null)
            {
                info += $"步骤详情: {CurrentStep.StepId} - {CurrentStep.StepName}\n";
            }
            
            return info;
        }
        
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[IntrusiveTutorialManager] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[IntrusiveTutorialManager] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[IntrusiveTutorialManager] {message}");
        }
        
        #endregion
        
        #region Unity编辑器快捷键
        
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tutorial/Stop Current Tutorial %q")]
        private static void StopCurrentTutorialMenuItem()
        {
            if (Instance.isExecuting)
            {
                Instance.StopCurrentTutorial();
            }
        }
        
        [UnityEditor.MenuItem("Tutorial/Skip Current Step %w")]
        private static void SkipCurrentStepMenuItem()
        {
            if (Instance.isExecuting)
            {
                Instance.SkipCurrentStep();
            }
        }
        
        [UnityEditor.MenuItem("Tutorial/Show System Info %i")]
        private static void ShowSystemInfoMenuItem()
        {
            Debug.Log(Instance.GetSystemInfo());
        }
#endif
        
        #endregion
    }
}