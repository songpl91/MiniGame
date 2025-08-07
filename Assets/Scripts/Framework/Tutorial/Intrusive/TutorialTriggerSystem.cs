using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Tutorial.Intrusive
{
    /// <summary>
    /// 引导触发系统
    /// 基于游戏状态和条件自动触发引导，而不是手动调用
    /// </summary>
    public class TutorialTriggerSystem : MonoBehaviour
    {
        private static TutorialTriggerSystem instance;
        public static TutorialTriggerSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("TutorialTriggerSystem");
                    instance = go.AddComponent<TutorialTriggerSystem>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }
        
        [Header("系统设置")]
        public bool enableDebugLog = true;
        
        [Header("触发方式控制")]
        [Tooltip("是否启用事件驱动触发")]
        public bool enableEventDrivenTrigger = true;
        
        [Tooltip("是否启用定时检查触发")]
        public bool enableTimerBasedTrigger = true;
        
        [Tooltip("定时检查间隔（秒）")]
        public float timerCheckInterval = 10f; // 降低频率到10秒
        
        [Tooltip("事件驱动检查的延迟（秒），避免频繁触发")]
        public float eventTriggerDelay = 0.1f;
        
        // 已注册的引导触发器
        private List<TutorialTrigger> registeredTriggers = new List<TutorialTrigger>();
        
        // 已完成的引导记录
        private HashSet<string> completedTutorials = new HashSet<string>();
        
        // 当前正在执行的引导
        private TutorialTrigger currentActiveTrigger;
        
        // 游戏数据访问器
        private IGameDataProvider gameDataProvider;
        
        // 引导管理器
        private IntrusiveTutorialManager tutorialManager;
        
        // 事件
        public System.Action<string> OnTutorialTriggered;
        public System.Action<string> OnTutorialCompleted;
        public System.Action<string> OnTutorialSkipped;
        public System.Action<string> OnTutorialStopped;
        
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 初始化系统
        /// </summary>
        private void Initialize()
        {
            // 获取引导管理器实例
            tutorialManager = IntrusiveTutorialManager.Instance;
            
            // 监听引导完成事件
            tutorialManager.OnTutorialComplete += OnTutorialSequenceCompleted;
            tutorialManager.OnTutorialStop += OnTutorialSequenceStopped;
            
            // 加载已完成的引导记录
            LoadCompletedTutorials();
            
            // 注册内置引导触发器
            RegisterBuiltInTriggers();
            
            // 根据设置启动相应的检查机制
            StartTriggerMechanisms();
            
            DebugLog("[TutorialTriggerSystem] 引导触发系统初始化完成");
        }
        
        /// <summary>
        /// 启动触发机制
        /// </summary>
        private void StartTriggerMechanisms()
        {
            // 启动定时检查（主要用于时间相关的触发器）
            if (enableTimerBasedTrigger)
            {
                InvokeRepeating(nameof(CheckTimerBasedTriggers), 1f, timerCheckInterval);
                DebugLog($"[TutorialTriggerSystem] 定时检查已启动，间隔: {timerCheckInterval}秒");
            }
            
            DebugLog($"[TutorialTriggerSystem] 事件驱动触发: {(enableEventDrivenTrigger ? "已启用" : "已禁用")}");
        }
        
        /// <summary>
        /// 设置游戏数据提供者
        /// </summary>
        public void SetGameDataProvider(IGameDataProvider provider)
        {
            gameDataProvider = provider;
            DebugLog("[TutorialTriggerSystem] 游戏数据提供者已设置");
        }
        
        /// <summary>
        /// 注册引导触发器
        /// </summary>
        public void RegisterTrigger(TutorialTrigger trigger)
        {
            if (trigger == null)
            {
                Debug.LogError("[TutorialTriggerSystem] 尝试注册空的引导触发器");
                return;
            }
            
            if (registeredTriggers.Any(t => t.TutorialId == trigger.TutorialId))
            {
                Debug.LogWarning($"[TutorialTriggerSystem] 引导触发器已存在: {trigger.TutorialId}");
                return;
            }
            
            registeredTriggers.Add(trigger);
            DebugLog($"[TutorialTriggerSystem] 注册引导触发器: {trigger.TutorialId}");
        }
        
        /// <summary>
        /// 注销引导触发器
        /// </summary>
        public void UnregisterTrigger(string tutorialId)
        {
            var trigger = registeredTriggers.FirstOrDefault(t => t.TutorialId == tutorialId);
            if (trigger != null)
            {
                registeredTriggers.Remove(trigger);
                DebugLog($"[TutorialTriggerSystem] 注销引导触发器: {tutorialId}");
            }
        }
        
        /// <summary>
        /// 定时检查触发器（主要用于时间相关的触发器）
        /// </summary>
        private void CheckTimerBasedTriggers()
        {
            if (!enableTimerBasedTrigger)
                return;
                
            DebugLog("[TutorialTriggerSystem] 执行定时检查");
            
            // 只检查时间相关的触发器
            CheckTriggersByType(typeof(TimeBasedTrigger), "定时检查");
        }
        
        /// <summary>
        /// 检查所有触发器（兼容旧接口）
        /// </summary>
        private void CheckTriggerConditions()
        {
            CheckAllTriggerTypes("通用检查");
        }
        
        /// <summary>
        /// 检查所有类型的触发器
        /// </summary>
        private void CheckAllTriggerTypes(string source)
        {
            // 如果当前有引导在执行，跳过检查
            if (currentActiveTrigger != null)
                return;
            
            // 如果游戏数据提供者未设置，跳过检查
            if (gameDataProvider == null)
                return;
            
            // 按优先级排序检查所有触发器
            var sortedTriggers = new List<TutorialTrigger>();
            foreach (var trigger in registeredTriggers)
            {
                if (!completedTutorials.Contains(trigger.TutorialId))
                {
                    sortedTriggers.Add(trigger);
                }
            }
            
            // 手动排序（不使用LINQ）
            sortedTriggers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            
            foreach (var trigger in sortedTriggers)
            {
                if (CheckTriggerCondition(trigger))
                {
                    DebugLog($"[TutorialTriggerSystem] {source} 触发引导: {trigger.TutorialId}");
                    TriggerTutorial(trigger);
                    break; // 一次只触发一个引导
                }
            }
        }
        
        /// <summary>
        /// 检查特定类型的触发器
        /// </summary>
        private void CheckTriggersByType(System.Type triggerType, string source)
        {
            // 如果当前有引导在执行，跳过检查
            if (currentActiveTrigger != null)
                return;
            
            // 如果游戏数据提供者未设置，跳过检查
            if (gameDataProvider == null)
                return;
            
            // 筛选指定类型的触发器
            var typedTriggers = new List<TutorialTrigger>();
            foreach (var trigger in registeredTriggers)
            {
                if (trigger.GetType() == triggerType && !completedTutorials.Contains(trigger.TutorialId))
                {
                    typedTriggers.Add(trigger);
                }
            }
            
            // 手动排序
            typedTriggers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            
            foreach (var trigger in typedTriggers)
            {
                if (CheckTriggerCondition(trigger))
                {
                    DebugLog($"[TutorialTriggerSystem] {source} 触发引导: {trigger.TutorialId}");
                    TriggerTutorial(trigger);
                    break; // 一次只触发一个引导
                }
            }
        }
        
        /// <summary>
        /// 检查单个触发器条件
        /// </summary>
        private bool CheckTriggerCondition(TutorialTrigger trigger)
        {
            try
            {
                // 检查前置条件
                if (!CheckPrerequisites(trigger))
                    return false;
                
                // 检查触发条件
                return trigger.CheckCondition(gameDataProvider);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[TutorialTriggerSystem] 检查触发条件时出错: {trigger.TutorialId}, 错误: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 检查前置条件
        /// </summary>
        private bool CheckPrerequisites(TutorialTrigger trigger)
        {
            if (trigger.Prerequisites == null || trigger.Prerequisites.Count == 0)
                return true;
            
            foreach (var prerequisite in trigger.Prerequisites)
            {
                if (!completedTutorials.Contains(prerequisite))
                {
                    DebugLog($"[TutorialTriggerSystem] 前置条件未满足: {trigger.TutorialId} 需要 {prerequisite}");
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 触发引导
        /// </summary>
        private void TriggerTutorial(TutorialTrigger trigger)
        {
            DebugLog($"[TutorialTriggerSystem] 触发引导: {trigger.TutorialId}");
            
            currentActiveTrigger = trigger;
            
            // 创建引导步骤列表
            var steps = trigger.CreateTutorialSequence();
            if (steps != null && steps.Count > 0)
            {
                // 启动引导
                tutorialManager.StartTutorial(trigger.TutorialId, steps);
                OnTutorialTriggered?.Invoke(trigger.TutorialId);
            }
            else
            {
                Debug.LogError($"[TutorialTriggerSystem] 无法创建引导序列: {trigger.TutorialId}");
                currentActiveTrigger = null;
            }
        }
        
        /// <summary>
        /// 引导序列完成回调
        /// </summary>
        private void OnTutorialSequenceCompleted(string tutorialId)
        {
            if (currentActiveTrigger != null && currentActiveTrigger.TutorialId == tutorialId)
            {
                // 标记为已完成
                MarkTutorialCompleted(tutorialId);
                
                // 清除当前触发器
                currentActiveTrigger = null;
                
                OnTutorialCompleted?.Invoke(tutorialId);
                DebugLog($"[TutorialTriggerSystem] 引导完成: {tutorialId}");
            }
        }
        
        /// <summary>
        /// 引导序列停止回调
        /// </summary>
        private void OnTutorialSequenceStopped(string tutorialId)
        {
            if (currentActiveTrigger != null && currentActiveTrigger.TutorialId == tutorialId)
            {
                // 清除当前触发器
                currentActiveTrigger = null;
                
                OnTutorialSkipped?.Invoke(tutorialId);
                OnTutorialStopped?.Invoke(tutorialId);
                DebugLog($"[TutorialTriggerSystem] 引导停止: {tutorialId}");
            }
        }
        
        /// <summary>
        /// 标记引导为已完成
        /// </summary>
        public void MarkTutorialCompleted(string tutorialId)
        {
            if (!completedTutorials.Contains(tutorialId))
            {
                completedTutorials.Add(tutorialId);
                SaveCompletedTutorials();
                DebugLog($"[TutorialTriggerSystem] 标记引导已完成: {tutorialId}");
            }
        }
        
        /// <summary>
        /// 检查引导是否已完成
        /// </summary>
        public bool IsTutorialCompleted(string tutorialId)
        {
            return completedTutorials.Contains(tutorialId);
        }
        
        /// <summary>
        /// 重置引导状态（用于测试）
        /// </summary>
        public void ResetTutorialState(string tutorialId)
        {
            completedTutorials.Remove(tutorialId);
            SaveCompletedTutorials();
            DebugLog($"[TutorialTriggerSystem] 重置引导状态: {tutorialId}");
        }
        
        /// <summary>
        /// 重置所有引导状态
        /// </summary>
        public void ResetAllTutorials()
        {
            completedTutorials.Clear();
            SaveCompletedTutorials();
            DebugLog("[TutorialTriggerSystem] 重置所有引导状态");
        }
        
        /// <summary>
        /// 强制触发引导（用于测试）
        /// </summary>
        public void ForceTriggerTutorial(string tutorialId)
        {
            var trigger = registeredTriggers.FirstOrDefault(t => t.TutorialId == tutorialId);
            if (trigger != null)
            {
                DebugLog($"[TutorialTriggerSystem] 强制触发引导: {tutorialId}");
                TriggerTutorial(trigger);
            }
            else
            {
                Debug.LogError($"[TutorialTriggerSystem] 找不到引导触发器: {tutorialId}");
            }
        }
        
        /// <summary>
        /// 注册内置引导触发器
        /// </summary>
        private void RegisterBuiltInTriggers()
        {
            // 这里可以注册一些内置的引导触发器
            // 实际项目中，这些触发器通常从配置文件加载
        }
        
        /// <summary>
        /// 加载已完成的引导记录
        /// </summary>
        private void LoadCompletedTutorials()
        {
            string key = "CompletedTutorials";
            if (PlayerPrefs.HasKey(key))
            {
                string data = PlayerPrefs.GetString(key);
                if (!string.IsNullOrEmpty(data))
                {
                    var tutorials = data.Split(',');
                    foreach (var tutorial in tutorials)
                    {
                        if (!string.IsNullOrEmpty(tutorial))
                        {
                            completedTutorials.Add(tutorial);
                        }
                    }
                }
            }
            
            DebugLog($"[TutorialTriggerSystem] 加载已完成引导: {completedTutorials.Count} 个");
        }
        
        /// <summary>
        /// 保存已完成的引导记录
        /// </summary>
        private void SaveCompletedTutorials()
        {
            string key = "CompletedTutorials";
            string data = string.Join(",", completedTutorials);
            PlayerPrefs.SetString(key, data);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 获取当前引导ID
        /// </summary>
        public string GetCurrentTutorialId()
        {
            return currentActiveTrigger?.TutorialId ?? "";
        }
        
        /// <summary>
        /// 检查是否有活动的引导
        /// </summary>
        public bool IsAnyTutorialActive()
        {
            return currentActiveTrigger != null;
        }
        
        /// <summary>
        /// 停止当前引导
        /// </summary>
        public void StopCurrentTutorial()
        {
            if (currentActiveTrigger != null)
            {
                tutorialManager.StopCurrentTutorial();
            }
        }
        
        /// <summary>
        /// 重置所有引导进度
        /// </summary>
        public void ResetAllTutorialProgress()
        {
            ResetAllTutorials();
        }
        
        /// <summary>
        /// 获取已完成引导数量
        /// </summary>
        public int GetCompletedTutorialCount()
        {
            return completedTutorials.Count;
        }
        
        /// <summary>
        /// 检查所有触发器（公共接口）
        /// </summary>
        public void CheckAllTriggers()
        {
            CheckTriggerConditions();
        }
        
        #region 事件驱动触发接口
        
        /// <summary>
        /// 玩家升级时触发检查
        /// </summary>
        public void OnPlayerLevelUp()
        {
            if (!enableEventDrivenTrigger) return;
            
            DebugLog("[TutorialTriggerSystem] 玩家升级事件触发");
            Invoke(nameof(CheckLevelBasedTriggers), eventTriggerDelay);
        }
        
        /// <summary>
        /// 完成关卡时触发检查
        /// </summary>
        public void OnStageCompleted(int stageId)
        {
            if (!enableEventDrivenTrigger) return;
            
            DebugLog($"[TutorialTriggerSystem] 关卡完成事件触发: {stageId}");
            Invoke(nameof(CheckStageBasedTriggers), eventTriggerDelay);
        }
        
        /// <summary>
        /// 功能解锁时触发检查
        /// </summary>
        public void OnFunctionUnlocked(string functionName)
        {
            if (!enableEventDrivenTrigger) return;
            
            DebugLog($"[TutorialTriggerSystem] 功能解锁事件触发: {functionName}");
            Invoke(nameof(CheckFunctionUnlockTriggers), eventTriggerDelay);
        }
        
        /// <summary>
        /// 首次进入场景或UI时触发检查
        /// </summary>
        public void OnFirstTimeEnter(string sceneOrUI)
        {
            if (!enableEventDrivenTrigger) return;
            
            DebugLog($"[TutorialTriggerSystem] 首次进入事件触发: {sceneOrUI}");
            Invoke(nameof(CheckFirstTimeBasedTriggers), eventTriggerDelay);
        }
        
        /// <summary>
        /// 游戏状态改变时触发检查（通用事件）
        /// </summary>
        public void OnGameStateChanged()
        {
            if (!enableEventDrivenTrigger) return;
            
            DebugLog("[TutorialTriggerSystem] 游戏状态改变事件触发");
            Invoke(nameof(CheckAllTriggerTypes_Event), eventTriggerDelay);
        }
        
        #endregion
        
        #region 事件驱动检查方法
        
        /// <summary>
        /// 检查等级相关的触发器
        /// </summary>
        private void CheckLevelBasedTriggers()
        {
            CheckTriggersByType(typeof(LevelBasedTrigger), "等级事件");
        }
        
        /// <summary>
        /// 检查关卡相关的触发器
        /// </summary>
        private void CheckStageBasedTriggers()
        {
            CheckTriggersByType(typeof(StageBasedTrigger), "关卡事件");
        }
        
        /// <summary>
        /// 检查功能解锁相关的触发器
        /// </summary>
        private void CheckFunctionUnlockTriggers()
        {
            CheckTriggersByType(typeof(FunctionUnlockTrigger), "功能解锁事件");
        }
        
        /// <summary>
        /// 检查首次进入相关的触发器
        /// </summary>
        private void CheckFirstTimeBasedTriggers()
        {
            CheckTriggersByType(typeof(FirstTimeBasedTrigger), "首次进入事件");
        }
        
        /// <summary>
        /// 事件驱动的全类型检查
        /// </summary>
        private void CheckAllTriggerTypes_Event()
        {
            CheckAllTriggerTypes("事件驱动");
        }
        
        #endregion
        
        #region 控制开关方法
        
        /// <summary>
        /// 设置事件驱动触发开关
        /// </summary>
        /// <param name="enabled">是否启用事件驱动触发</param>
        public void SetEventDrivenTrigger(bool enabled)
        {
            enableEventDrivenTrigger = enabled;
            DebugLog($"[TutorialTriggerSystem] 事件驱动触发已{(enabled ? "启用" : "禁用")}");
        }
        
        /// <summary>
        /// 设置定时检查触发开关
        /// </summary>
        /// <param name="enabled">是否启用定时检查触发</param>
        public void SetTimerBasedTrigger(bool enabled)
        {
            bool wasEnabled = enableTimerBasedTrigger;
            enableTimerBasedTrigger = enabled;
            
            if (wasEnabled && !enabled)
            {
                // 停止定时检查
                CancelInvoke(nameof(CheckTimerBasedTriggers));
                DebugLog("[TutorialTriggerSystem] 定时检查触发已禁用");
            }
            else if (!wasEnabled && enabled)
            {
                // 启动定时检查
                InvokeRepeating(nameof(CheckTimerBasedTriggers), 1f, timerCheckInterval);
                DebugLog("[TutorialTriggerSystem] 定时检查触发已启用");
            }
        }
        
        /// <summary>
        /// 设置定时检查间隔
        /// </summary>
        /// <param name="interval">检查间隔（秒）</param>
        public void SetTimerCheckInterval(float interval)
        {
            timerCheckInterval = interval;
            
            // 如果定时检查正在运行，重新启动以应用新间隔
            if (enableTimerBasedTrigger)
            {
                CancelInvoke(nameof(CheckTimerBasedTriggers));
                InvokeRepeating(nameof(CheckTimerBasedTriggers), 1f, timerCheckInterval);
                DebugLog($"[TutorialTriggerSystem] 定时检查间隔已更新为 {interval} 秒");
            }
        }
        
        /// <summary>
        /// 设置事件触发延迟
        /// </summary>
        /// <param name="delay">延迟时间（秒）</param>
        public void SetEventTriggerDelay(float delay)
        {
            eventTriggerDelay = delay;
            DebugLog($"[TutorialTriggerSystem] 事件触发延迟已更新为 {delay} 秒");
        }
        
        /// <summary>
        /// 获取当前触发机制状态
        /// </summary>
        /// <returns>触发机制状态信息</returns>
        public string GetTriggerMechanismStatus()
        {
            return $"事件驱动: {(enableEventDrivenTrigger ? "启用" : "禁用")}, " +
                   $"定时检查: {(enableTimerBasedTrigger ? "启用" : "禁用")}, " +
                   $"检查间隔: {timerCheckInterval}秒, " +
                   $"事件延迟: {eventTriggerDelay}秒";
        }
        
        #endregion
        
        /// <summary>
        /// 获取系统状态信息
        /// </summary>
        public string GetSystemInfo()
        {
            return $"注册触发器: {registeredTriggers.Count}\n" +
                   $"已完成引导: {completedTutorials.Count}\n" +
                   $"当前活动引导: {(currentActiveTrigger?.TutorialId ?? "无")}\n" +
                   $"数据提供者: {(gameDataProvider != null ? "已设置" : "未设置")}";
        }
        
        /// <summary>
        /// 调试日志
        /// </summary>
        private void DebugLog(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log(message);
            }
        }
        
        void OnDestroy()
        {
            // 清理事件监听
            if (tutorialManager != null)
            {
                tutorialManager.OnTutorialComplete -= OnTutorialSequenceCompleted;
                tutorialManager.OnTutorialStop -= OnTutorialSequenceStopped;
            }
            
            // 停止检查循环
            CancelInvoke();
        }
    }
}