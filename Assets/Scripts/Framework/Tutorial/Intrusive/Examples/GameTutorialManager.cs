using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Framework.Tutorial.Intrusive.Examples
{
    /// <summary>
    /// 游戏引导管理器
    /// 整合引导触发系统和游戏数据，提供完整的引导解决方案
    /// </summary>
    public class GameTutorialManager : MonoBehaviour
    {
        [Header("系统设置")]
        [SerializeField] private bool enableTutorialSystem = true;
        [SerializeField] private bool enableDebugLog = true;
        [SerializeField] private float checkInterval = 1f; // 检查触发条件的间隔
        
        [Header("混合触发机制设置")]
        [SerializeField] private bool enableEventDrivenTrigger = true; // 启用事件驱动触发
        [SerializeField] private bool enableTimerBasedTrigger = true; // 启用定时检查触发
        [SerializeField] private float timerCheckInterval = 3f; // 定时检查间隔（秒）
        [SerializeField] private float eventTriggerDelay = 0.1f; // 事件触发延迟（秒）
        
        [Header("组件引用")]
        [SerializeField] private GameDataProvider gameDataProvider;
        [SerializeField] private IntrusiveTutorialManager tutorialManager;
        
        [Header("UI引用")]
        [SerializeField] private GameObject tutorialDebugPanel;
        [SerializeField] private UnityEngine.UI.Text debugInfoText;
        [SerializeField] private UnityEngine.UI.Button skipCurrentTutorialButton;
        [SerializeField] private UnityEngine.UI.Button resetTutorialDataButton;
        
        // 引导触发系统
        private TutorialTriggerSystem triggerSystem;
        
        // 已注册的引导触发器
        private List<TutorialTrigger> registeredTriggers = new List<TutorialTrigger>();
        
        // 检查协程
        private Coroutine checkCoroutine;
        
        // 事件回调
        public System.Action<string> OnTutorialStarted;
        public System.Action<string> OnTutorialCompleted;
        public System.Action<string> OnTutorialSkipped;
        
        void Awake()
        {
            // 初始化引导触发系统
            triggerSystem = TutorialTriggerSystem.Instance;
            
            // 查找组件
            if (gameDataProvider == null)
                gameDataProvider = FindObjectOfType<GameDataProvider>();
            
            if (tutorialManager == null)
                tutorialManager = FindObjectOfType<IntrusiveTutorialManager>();
        }
        
        void Start()
        {
            InitializeSystem();
        }
        
        void OnDestroy()
        {
            if (checkCoroutine != null)
            {
                StopCoroutine(checkCoroutine);
            }
        }
        
        /// <summary>
        /// 初始化引导系统
        /// </summary>
        private void InitializeSystem()
        {
            if (!enableTutorialSystem)
            {
                LogDebug("引导系统已禁用");
                return;
            }
            
            // 检查必要组件
            if (gameDataProvider == null)
            {
                LogError("GameDataProvider 未找到！");
                return;
            }
            
            if (tutorialManager == null)
            {
                LogError("IntrusiveTutorialManager 未找到！");
                return;
            }
            
            // 初始化触发系统
            triggerSystem.Initialize(gameDataProvider, tutorialManager);
            
            // 配置混合触发机制
            ConfigureTriggerMechanism();
            
            // 注册所有引导触发器
            RegisterAllTriggers();
            
            // 设置事件回调
            SetupEventCallbacks();
            
            // 初始化调试UI
            InitializeDebugUI();
            
            // 开始检查触发条件
            StartTriggerChecking();
            
            LogDebug("游戏引导系统初始化完成");
        }
        
        /// <summary>
        /// 配置混合触发机制
        /// </summary>
        private void ConfigureTriggerMechanism()
        {
            // 设置事件驱动触发开关
            triggerSystem.SetEventDrivenTrigger(enableEventDrivenTrigger);
            
            // 设置定时检查触发开关
            triggerSystem.SetTimerBasedTrigger(enableTimerBasedTrigger);
            
            // 设置定时检查间隔
            triggerSystem.SetTimerCheckInterval(timerCheckInterval);
            
            // 设置事件触发延迟
            triggerSystem.SetEventTriggerDelay(eventTriggerDelay);
            
            LogDebug($"混合触发机制配置完成: {triggerSystem.GetTriggerMechanismStatus()}");
        }
        
        /// <summary>
        /// 注册所有引导触发器
        /// </summary>
        private void RegisterAllTriggers()
        {
            // 注册新手引导
            RegisterTrigger(new GameTutorialTriggers.NewPlayerTutorial());
            
            // 注册背包系统引导
            RegisterTrigger(new GameTutorialTriggers.InventoryTutorial());
            
            // 注册装备系统引导
            RegisterTrigger(new GameTutorialTriggers.EquipmentTutorial());
            
            // 注册战斗系统引导
            RegisterTrigger(new GameTutorialTriggers.BattleTutorial());
            
            // 注册商店系统引导
            RegisterTrigger(new GameTutorialTriggers.ShopTutorial());
            
            // 注册公会系统引导
            RegisterTrigger(new GameTutorialTriggers.GuildTutorial());
            
            // 注册每日任务引导
            RegisterTrigger(new GameTutorialTriggers.DailyQuestTutorial());
            
            LogDebug($"已注册 {registeredTriggers.Count} 个引导触发器");
        }
        
        /// <summary>
        /// 注册引导触发器
        /// </summary>
        private void RegisterTrigger(TutorialTrigger trigger)
        {
            registeredTriggers.Add(trigger);
            triggerSystem.RegisterTrigger(trigger);
            LogDebug($"注册引导触发器: {trigger.TutorialName}");
        }
        
        /// <summary>
        /// 设置事件回调
        /// </summary>
        private void SetupEventCallbacks()
        {
            triggerSystem.OnTutorialTriggered += OnTutorialTriggered;
            triggerSystem.OnTutorialCompleted += OnTutorialCompletedCallback;
            triggerSystem.OnTutorialStopped += OnTutorialStoppedCallback;
        }
        
        /// <summary>
        /// 初始化调试UI
        /// </summary>
        private void InitializeDebugUI()
        {
            if (tutorialDebugPanel != null)
            {
                tutorialDebugPanel.SetActive(enableDebugLog);
            }
            
            if (skipCurrentTutorialButton != null)
            {
                skipCurrentTutorialButton.onClick.AddListener(SkipCurrentTutorial);
            }
            
            if (resetTutorialDataButton != null)
            {
                resetTutorialDataButton.onClick.AddListener(ResetAllTutorialData);
            }
        }
        
        /// <summary>
        /// 开始检查触发条件
        /// </summary>
        private void StartTriggerChecking()
        {
            if (checkCoroutine != null)
            {
                StopCoroutine(checkCoroutine);
            }
            
            checkCoroutine = StartCoroutine(TriggerCheckCoroutine());
        }
        
        /// <summary>
        /// 触发检查协程
        /// </summary>
        private IEnumerator TriggerCheckCoroutine()
        {
            while (enableTutorialSystem)
            {
                yield return new WaitForSeconds(checkInterval);
                
                // 检查所有触发器
                triggerSystem.CheckAllTriggers();
                
                // 更新调试信息
                UpdateDebugInfo();
            }
        }
        
        /// <summary>
        /// 引导触发回调
        /// </summary>
        private void OnTutorialTriggered(string tutorialId)
        {
            LogDebug($"引导触发: {tutorialId}");
            OnTutorialStarted?.Invoke(tutorialId);
        }
        
        /// <summary>
        /// 引导完成回调
        /// </summary>
        private void OnTutorialCompletedCallback(string tutorialId)
        {
            LogDebug($"引导完成: {tutorialId}");
            OnTutorialCompleted?.Invoke(tutorialId);
            
            // 标记引导完成状态
            gameDataProvider.SetCustomData($"Tutorial_{tutorialId}_Completed", true);
        }
        
        /// <summary>
        /// 引导停止回调
        /// </summary>
        private void OnTutorialStoppedCallback(string tutorialId)
        {
            LogDebug($"引导停止: {tutorialId}");
            OnTutorialSkipped?.Invoke(tutorialId);
        }
        
        /// <summary>
        /// 更新调试信息
        /// </summary>
        private void UpdateDebugInfo()
        {
            if (debugInfoText == null || !enableDebugLog)
                return;
            
            var info = "=== 引导系统状态 ===\n";
            info += $"系统启用: {enableTutorialSystem}\n";
            info += $"当前引导: {triggerSystem.GetCurrentTutorialId()}\n";
            info += $"已完成引导: {triggerSystem.GetCompletedTutorialCount()}\n";
            info += $"注册触发器: {registeredTriggers.Count}\n\n";
            
            info += "=== 混合触发机制 ===\n";
            info += triggerSystem.GetTriggerMechanismStatus() + "\n\n";
            
            info += "=== 游戏数据 ===\n";
            info += gameDataProvider.GetDebugInfo() + "\n\n";
            
            info += "=== 可触发引导 ===\n";
            foreach (var trigger in registeredTriggers)
            {
                if (!triggerSystem.IsTutorialCompleted(trigger.TutorialId))
                {
                    bool canTrigger = trigger.CheckCondition(gameDataProvider);
                    info += $"{trigger.TutorialName}: {(canTrigger ? "✓" : "✗")}\n";
                }
            }
            
            debugInfoText.text = info;
        }
        
        #region 公共接口
        
        /// <summary>
        /// 启用/禁用引导系统
        /// </summary>
        public void SetTutorialSystemEnabled(bool enabled)
        {
            enableTutorialSystem = enabled;
            
            if (!enabled && triggerSystem.IsAnyTutorialActive())
            {
                triggerSystem.StopCurrentTutorial();
            }
            
            LogDebug($"引导系统 {(enabled ? "启用" : "禁用")}");
        }
        
        /// <summary>
        /// 手动触发指定引导
        /// </summary>
        public void TriggerTutorial(string tutorialId)
        {
            var trigger = registeredTriggers.Find(t => t.TutorialId == tutorialId);
            if (trigger != null)
            {
                triggerSystem.ForceTriggerTutorial(tutorialId);
                LogDebug($"手动触发引导: {tutorialId}");
            }
            else
            {
                LogError($"未找到引导触发器: {tutorialId}");
            }
        }
        
        /// <summary>
        /// 跳过当前引导
        /// </summary>
        public void SkipCurrentTutorial()
        {
            if (triggerSystem.IsAnyTutorialActive())
            {
                triggerSystem.StopCurrentTutorial();
                LogDebug("跳过当前引导");
            }
        }
        
        /// <summary>
        /// 重置所有引导数据
        /// </summary>
        public void ResetAllTutorialData()
        {
            triggerSystem.ResetAllTutorialProgress();
            
            // 重置游戏数据中的引导完成标记
            foreach (var trigger in registeredTriggers)
            {
                gameDataProvider.SetCustomData($"Tutorial_{trigger.TutorialId}_Completed", false);
            }
            
            LogDebug("重置所有引导数据");
        }
        
        /// <summary>
        /// 获取引导完成状态
        /// </summary>
        public bool IsTutorialCompleted(string tutorialId)
        {
            return triggerSystem.IsTutorialCompleted(tutorialId);
        }
        
        /// <summary>
        /// 获取当前活动引导ID
        /// </summary>
        public string GetCurrentTutorialId()
        {
            return triggerSystem.GetCurrentTutorialId();
        }
        
        /// <summary>
        /// 获取系统状态信息
        /// </summary>
        public string GetSystemInfo()
        {
            return triggerSystem.GetSystemInfo();
        }
        
        #endregion
        
        #region 混合触发机制控制
        
        /// <summary>
        /// 设置事件驱动触发开关
        /// </summary>
        /// <param name="enabled">是否启用事件驱动触发</param>
        public void SetEventDrivenTrigger(bool enabled)
        {
            enableEventDrivenTrigger = enabled;
            triggerSystem.SetEventDrivenTrigger(enabled);
            LogDebug($"事件驱动触发已{(enabled ? "启用" : "禁用")}");
        }
        
        /// <summary>
        /// 设置定时检查触发开关
        /// </summary>
        /// <param name="enabled">是否启用定时检查触发</param>
        public void SetTimerBasedTrigger(bool enabled)
        {
            enableTimerBasedTrigger = enabled;
            triggerSystem.SetTimerBasedTrigger(enabled);
            LogDebug($"定时检查触发已{(enabled ? "启用" : "禁用")}");
        }
        
        /// <summary>
        /// 设置定时检查间隔
        /// </summary>
        /// <param name="interval">检查间隔（秒）</param>
        public void SetTimerCheckInterval(float interval)
        {
            timerCheckInterval = interval;
            triggerSystem.SetTimerCheckInterval(interval);
            LogDebug($"定时检查间隔已更新为 {interval} 秒");
        }
        
        /// <summary>
        /// 设置事件触发延迟
        /// </summary>
        /// <param name="delay">延迟时间（秒）</param>
        public void SetEventTriggerDelay(float delay)
        {
            eventTriggerDelay = delay;
            triggerSystem.SetEventTriggerDelay(delay);
            LogDebug($"事件触发延迟已更新为 {delay} 秒");
        }
        
        /// <summary>
        /// 获取触发机制状态
        /// </summary>
        /// <returns>触发机制状态信息</returns>
        public string GetTriggerMechanismStatus()
        {
            return triggerSystem.GetTriggerMechanismStatus();
        }
        
        /// <summary>
        /// 手动触发游戏状态改变检查
        /// </summary>
        public void TriggerGameStateChanged()
        {
            triggerSystem.OnGameStateChanged();
            LogDebug("手动触发游戏状态改变检查");
        }
        
        #endregion
        
        #region 测试用方法
        
        /// <summary>
        /// 模拟玩家升级（测试用）
        /// </summary>
        [ContextMenu("模拟升级")]
        public void SimulateLevelUp()
        {
            gameDataProvider.LevelUp();
            LogDebug($"模拟升级到 {gameDataProvider.GetPlayerLevel()} 级");
            
            // 触发事件驱动检查
            triggerSystem.OnPlayerLevelUp();
        }
        
        /// <summary>
        /// 模拟完成关卡（测试用）
        /// </summary>
        [ContextMenu("模拟完成关卡")]
        public void SimulateCompleteStage()
        {
            int nextStage = gameDataProvider.GetMaxPassedStageId() + 1;
            gameDataProvider.CompleteStage(nextStage);
            LogDebug($"模拟完成关卡 {nextStage}");
            
            // 触发事件驱动检查
            triggerSystem.OnStageCompleted(nextStage);
        }
        
        /// <summary>
        /// 模拟解锁功能（测试用）
        /// </summary>
        public void SimulateUnlockFunction(string functionName)
        {
            gameDataProvider.UnlockFunction(functionName);
            LogDebug($"模拟解锁功能: {functionName}");
            
            // 触发事件驱动检查
            triggerSystem.OnFunctionUnlocked(functionName);
        }
        
        /// <summary>
        /// 模拟首次游戏（测试用）
        /// </summary>
        [ContextMenu("模拟首次游戏")]
        public void SimulateFirstTimePlay()
        {
            gameDataProvider.SetFirstTimePlay(true);
            LogDebug("模拟首次游戏状态");
            
            // 触发事件驱动检查
            triggerSystem.OnFirstTimeEnter("Game");
        }
        
        #endregion
        
        #region 调试日志
        
        private void LogDebug(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[GameTutorialManager] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[GameTutorialManager] {message}");
        }
        
        #endregion
        
        #region Unity编辑器快捷键
        
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tutorial/Skip Current Tutorial %t")]
        private static void SkipCurrentTutorialMenuItem()
        {
            var manager = FindObjectOfType<GameTutorialManager>();
            if (manager != null)
            {
                manager.SkipCurrentTutorial();
            }
        }
        
        [UnityEditor.MenuItem("Tutorial/Reset Tutorial Data %r")]
        private static void ResetTutorialDataMenuItem()
        {
            var manager = FindObjectOfType<GameTutorialManager>();
            if (manager != null)
            {
                manager.ResetAllTutorialData();
            }
        }
        
        [UnityEditor.MenuItem("Tutorial/Simulate Level Up %l")]
        private static void SimulateLevelUpMenuItem()
        {
            var manager = FindObjectOfType<GameTutorialManager>();
            if (manager != null)
            {
                manager.SimulateLevelUp();
            }
        }
#endif
        
        #endregion
    }
}