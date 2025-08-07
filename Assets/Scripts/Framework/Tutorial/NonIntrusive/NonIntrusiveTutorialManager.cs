using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Tutorial.NonIntrusive
{
    /// <summary>
    /// 非侵入式新手引导管理器
    /// 极简版本，通过配置文件驱动，无需修改业务代码
    /// </summary>
    public class NonIntrusiveTutorialManager : MonoBehaviour
    {
        [Header("系统设置")]
        [SerializeField] private bool enableSystem = true;
        [SerializeField] private bool debugMode = false;
        [SerializeField] private List<TutorialConfigData> tutorialConfigs = new List<TutorialConfigData>();
        
        private static NonIntrusiveTutorialManager _instance;
        public static NonIntrusiveTutorialManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<NonIntrusiveTutorialManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("NonIntrusiveTutorialManager");
                        _instance = go.AddComponent<NonIntrusiveTutorialManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        // 运行时状态
        private Dictionary<string, TutorialConfigData> _configCache = new Dictionary<string, TutorialConfigData>();
        private Dictionary<string, bool> _completedTutorials = new Dictionary<string, bool>();
        private TutorialConfigData _currentConfig;
        private bool _isExecuting = false;
        
        // 简化的事件
        public event Action<string> OnTutorialStart;
        public event Action<string> OnTutorialComplete;
        public event Action<string> OnStepComplete;
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSystem();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 初始化系统
        /// </summary>
        private void InitializeSystem()
        {
            // 注册配置
            foreach (var config in tutorialConfigs)
            {
                if (config != null && !string.IsNullOrEmpty(config.configId))
                {
                    _configCache[config.configId] = config;
                    _completedTutorials[config.configId] = LoadCompletionState(config.configId);
                }
            }
            
            if (debugMode)
                Debug.Log($"[NonIntrusiveTutorialManager] 系统初始化完成，加载了 {_configCache.Count} 个配置");
        }
        
        /// <summary>
        /// 开始引导
        /// </summary>
        /// <param name="configId">配置ID</param>
        public void StartTutorial(string configId)
        {
            if (!enableSystem)
            {
                Log("引导系统已禁用");
                return;
            }
            
            if (!_configCache.ContainsKey(configId))
            {
                LogError($"未找到引导配置: {configId}");
                return;
            }
            
            if (_isExecuting)
            {
                Log($"引导系统正在执行中，无法开始新的引导: {configId}");
                return;
            }
            
            if (_completedTutorials.ContainsKey(configId) && _completedTutorials[configId])
            {
                Log($"引导已完成: {configId}");
                return;
            }
            
            _currentConfig = _configCache[configId];
            _isExecuting = true;
            
            Log($"开始引导: {_currentConfig.configName}");
            OnTutorialStart?.Invoke(configId);
            
            StartCoroutine(ExecuteTutorial());
        }
        
        /// <summary>
        /// 停止当前引导
        /// </summary>
        public void StopCurrentTutorial()
        {
            if (_isExecuting)
            {
                Log($"停止引导: {_currentConfig?.configName}");
                _isExecuting = false;
                _currentConfig = null;
            }
        }
        
        /// <summary>
        /// 检查引导是否完成
        /// </summary>
        /// <param name="configId">配置ID</param>
        /// <returns>是否完成</returns>
        public bool IsTutorialCompleted(string configId)
        {
            return _completedTutorials.ContainsKey(configId) && _completedTutorials[configId];
        }
        
        /// <summary>
        /// 重置引导状态
        /// </summary>
        /// <param name="configId">配置ID</param>
        public void ResetTutorial(string configId)
        {
            if (_completedTutorials.ContainsKey(configId))
            {
                _completedTutorials[configId] = false;
                SaveCompletionState(configId, false);
                Log($"重置引导: {configId}");
            }
        }
        
        /// <summary>
        /// 执行引导
        /// </summary>
        /// <returns></returns>
        private IEnumerator ExecuteTutorial()
        {
            if (_currentConfig == null || _currentConfig.sequences == null)
                yield break;
            
            foreach (var sequence in _currentConfig.sequences)
            {
                if (!sequence.enabled) continue;
                
                Log($"开始序列: {sequence.sequenceName}");
                
                foreach (var step in sequence.steps)
                {
                    if (!step.enabled) continue;
                    if (!_isExecuting) yield break;
                    
                    Log($"执行步骤: {step.stepName}");
                    OnStepComplete?.Invoke(step.stepId);
                    
                    // 简单的步骤执行逻辑
                    yield return ExecuteStep(step);
                    
                    // 步骤间延迟
                    if (step.stepDelay > 0)
                        yield return new WaitForSeconds(step.stepDelay);
                }
                
                // 序列间延迟
                if (sequence.stepDelay > 0)
                    yield return new WaitForSeconds(sequence.stepDelay);
            }
            
            // 完成引导
            CompleteTutorial(_currentConfig.configId);
        }
        
        /// <summary>
        /// 执行单个步骤
        /// </summary>
        /// <param name="step">步骤配置</param>
        /// <returns></returns>
        private IEnumerator ExecuteStep(TutorialStepConfigData step)
        {
            switch (step.stepType)
            {
                case TutorialStepType.Message:
                    yield return ShowMessage(step);
                    break;
                case TutorialStepType.Highlight:
                    yield return HighlightElement(step);
                    break;
                case TutorialStepType.Wait:
                    yield return new WaitForSeconds(step.waitTime);
                    break;
                case TutorialStepType.Click:
                    yield return WaitForClick(step);
                    break;
                default:
                    yield return new WaitForSeconds(0.1f);
                    break;
            }
        }
        
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="step">步骤配置</param>
        /// <returns></returns>
        private IEnumerator ShowMessage(TutorialStepConfigData step)
        {
            Log($"显示消息: {step.message}");
            // 这里可以集成UI系统显示消息
            yield return new WaitForSeconds(step.displayTime > 0 ? step.displayTime : 2f);
        }
        
        /// <summary>
        /// 高亮元素
        /// </summary>
        /// <param name="step">步骤配置</param>
        /// <returns></returns>
        private IEnumerator HighlightElement(TutorialStepConfigData step)
        {
            Log($"高亮元素: {step.targetPath}");
            // 这里可以集成高亮效果
            yield return new WaitForSeconds(step.displayTime > 0 ? step.displayTime : 1f);
        }
        
        /// <summary>
        /// 等待点击
        /// </summary>
        /// <param name="step">步骤配置</param>
        /// <returns></returns>
        private IEnumerator WaitForClick(TutorialStepConfigData step)
        {
            Log($"等待点击: {step.targetPath}");
            // 这里可以集成点击检测逻辑
            yield return new WaitForSeconds(1f); // 简化为等待1秒
        }
        
        /// <summary>
        /// 完成引导
        /// </summary>
        /// <param name="configId">配置ID</param>
        private void CompleteTutorial(string configId)
        {
            _completedTutorials[configId] = true;
            SaveCompletionState(configId, true);
            
            Log($"引导完成: {configId}");
            OnTutorialComplete?.Invoke(configId);
            
            _isExecuting = false;
            _currentConfig = null;
        }
        
        /// <summary>
        /// 加载完成状态
        /// </summary>
        /// <param name="configId">配置ID</param>
        /// <returns>是否完成</returns>
        private bool LoadCompletionState(string configId)
        {
            return PlayerPrefs.GetInt($"Tutorial_Completed_{configId}", 0) == 1;
        }
        
        /// <summary>
        /// 保存完成状态
        /// </summary>
        /// <param name="configId">配置ID</param>
        /// <param name="completed">是否完成</param>
        private void SaveCompletionState(string configId, bool completed)
        {
            PlayerPrefs.SetInt($"Tutorial_Completed_{configId}", completed ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        private void Log(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[NonIntrusiveTutorialManager] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[NonIntrusiveTutorialManager] {message}");
        }
        
        private void OnDestroy()
        {
            StopCurrentTutorial();
        }
    }
}