using System;
using System.Collections.Generic;
using AdvancedGameStates;
using UnityEngine;

/// <summary>
/// 高级游戏管理器 - 展示状态机最佳实践用例
/// 集成了性能优化、扩展功能和完整的游戏流程管理
/// </summary>
public class AdvancedGameManager : MonoBehaviour
{
    [Header("状态机配置")]
    [SerializeField] private StateMachineConfig stateMachineConfig;
    [SerializeField] private bool enablePerformanceMonitoring = true;
    [SerializeField] private bool enableStateHistory = true;
    
    [Header("游戏配置")]
    [SerializeField] private float gameSessionTimeout = 300f; // 5分钟游戏超时
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float pauseMenuDelay = 0.5f;
    
    // 核心组件
    private StateMachine stateMachine;
    private StateMachineExtensions.StateHistory stateHistory;
    private StateMachineProfiler performanceProfiler;
    
    // 游戏数据
    private GameSessionData currentSession;
    private PlayerData playerData;
    
    // 事件系统
    public event Action<GameSessionData> OnGameSessionStarted;
    public event Action<GameSessionData> OnGameSessionEnded;
    public event Action<int> OnScoreChanged;
    public event Action<int> OnLivesChanged;
    
    #region Unity生命周期
    
    void Awake()
    {
        // 初始化游戏数据
        InitializeGameData();
        
        // 创建状态机配置
        if (stateMachineConfig == null)
        {
            stateMachineConfig = CreateDefaultConfig();
        }
    }
    
    void Start()
    {
        // 初始化状态机系统
        InitializeStateMachine();
        
        // 启动游戏
        StartGameSession();
    }
    
    void Update()
    {
        // 更新状态机
        stateMachine?.Update();
        
        // 检查游戏超时
        CheckGameSessionTimeout();
        
        // 处理全局输入
        HandleGlobalInput();
    }
    
    void OnDestroy()
    {
        // 清理资源
        CleanupStateMachine();
    }
    
    #endregion
    
    #region 状态机初始化
    
    /// <summary>
    /// 初始化状态机系统
    /// </summary>
    private void InitializeStateMachine()
    {
        // 使用工厂方法创建状态机
        stateMachine = StateMachineFactory.CreateGameStateMachine(gameObject, stateMachineConfig);
        
        // 设置自定义状态转换规则
        stateMachine.SetStateTransition(new AdvancedGameStateTransition());
        
        // 初始化性能监控
        if (enablePerformanceMonitoring)
        {
            InitializePerformanceMonitoring();
        }
        
        // 初始化状态历史
        if (enableStateHistory)
        {
            InitializeStateHistory();
        }
        
        // 注册状态机事件
        RegisterStateMachineEvents();
        
        // 添加所有游戏状态
        RegisterGameStates();
        
        Debug.Log("[游戏管理器] 状态机系统初始化完成");
    }
    
    /// <summary>
    /// 初始化性能监控
    /// </summary>
    private void InitializePerformanceMonitoring()
    {
        performanceProfiler = new StateMachineProfiler();
        
        stateMachine.OnStateEntered += (stateName) => 
        {
            performanceProfiler.OnStateEntered(stateName);
        };
        
        stateMachine.OnStateExited += (stateName) => 
        {
            performanceProfiler.OnStateExited(stateName);
        };
    }
    
    /// <summary>
    /// 初始化状态历史记录
    /// </summary>
    private void InitializeStateHistory()
    {
        stateHistory = new StateMachineExtensions.StateHistory(stateMachineConfig.maxHistorySize);
        
        stateMachine.OnStateChanged += (fromState, toState) => 
        {
            // 记录状态变化历史
            Type fromType = !string.IsNullOrEmpty(fromState) ? Type.GetType(fromState) : null;
            Type toType = Type.GetType(toState);
            stateHistory.RecordStateChange(fromType, toType, Time.time);
        };
    }
    
    /// <summary>
    /// 注册状态机事件
    /// </summary>
    private void RegisterStateMachineEvents()
    {
        stateMachine.OnStateChanged += OnStateMachineStateChanged;
        stateMachine.OnStateEntered += OnStateMachineStateEntered;
        stateMachine.OnStateExited += OnStateMachineStateExited;
        stateMachine.OnBlackboardValueChanged += OnStateMachineBlackboardChanged;
    }
    
    /// <summary>
    /// 注册所有游戏状态
    /// </summary>
    private void RegisterGameStates()
    {
        // 核心游戏状态
        stateMachine.AddNode<AdvancedMainMenuState>();
        stateMachine.AddNode<AdvancedGamePlayState>();
        stateMachine.AddNode<AdvancedPauseState>();
        stateMachine.AddNode<AdvancedGameOverState>();
        
        // 扩展状态
        stateMachine.AddNode<LoadingState>();
        stateMachine.AddNode<SettingsState>();
        stateMachine.AddNode<LeaderboardState>();
        stateMachine.AddNode<TutorialState>();
        
        Debug.Log("[游戏管理器] 所有游戏状态注册完成");
    }
    
    #endregion
    
    #region 游戏会话管理
    
    /// <summary>
    /// 开始游戏会话
    /// </summary>
    private void StartGameSession()
    {
        currentSession = new GameSessionData
        {
            SessionId = Guid.NewGuid().ToString(),
            StartTime = Time.time,
            InitialLives = maxLives
        };
        
        // 设置初始黑板数据
        SetupInitialBlackboardData();
        
        // 启动状态机
        stateMachine.Run<LoadingState>();
        
        OnGameSessionStarted?.Invoke(currentSession);
        Debug.Log($"[游戏会话] 开始新会话: {currentSession.SessionId}");
    }
    
    /// <summary>
    /// 结束游戏会话
    /// </summary>
    private void EndGameSession()
    {
        if (currentSession != null)
        {
            currentSession.EndTime = Time.time;
            currentSession.Duration = currentSession.EndTime - currentSession.StartTime;
            
            // 保存会话数据
            SaveGameSessionData();
            
            OnGameSessionEnded?.Invoke(currentSession);
            Debug.Log($"[游戏会话] 会话结束，持续时间: {currentSession.Duration:F2}秒");
        }
    }
    
    /// <summary>
    /// 检查游戏会话超时
    /// </summary>
    private void CheckGameSessionTimeout()
    {
        if (currentSession != null && 
            Time.time - currentSession.StartTime > gameSessionTimeout)
        {
            Debug.LogWarning("[游戏会话] 会话超时，自动结束游戏");
            ForceEndGame("会话超时");
        }
    }
    
    #endregion
    
    #region 状态机事件处理
    
    /// <summary>
    /// 状态变化事件处理
    /// </summary>
    private void OnStateMachineStateChanged(string fromState, string toState)
    {
        Debug.Log($"[状态变化] {fromState} -> {toState}");
        
        // 更新会话统计
        if (currentSession != null)
        {
            currentSession.StateChangeCount++;
        }
        
        // 根据状态变化执行特定逻辑
        HandleStateSpecificLogic(toState);
    }
    
    /// <summary>
    /// 状态进入事件处理
    /// </summary>
    private void OnStateMachineStateEntered(string stateName)
    {
        Debug.Log($"[状态进入] {stateName}");
        
        // 播放状态进入音效
        PlayStateTransitionSound(stateName);
        
        // 更新UI显示
        UpdateUIForState(stateName);
    }
    
    /// <summary>
    /// 状态退出事件处理
    /// </summary>
    private void OnStateMachineStateExited(string stateName)
    {
        Debug.Log($"[状态退出] {stateName}");
        
        // 清理状态相关资源
        CleanupStateResources(stateName);
    }
    
    /// <summary>
    /// 黑板数据变化事件处理
    /// </summary>
    private void OnStateMachineBlackboardChanged(string key, object oldValue, object newValue)
    {
        Debug.Log($"[黑板变化] {key}: {oldValue} -> {newValue}");
        
        // 处理特定数据变化
        switch (key)
        {
            case "Score":
                if (newValue is int score)
                {
                    OnScoreChanged?.Invoke(score);
                    CheckScoreMilestones(score);
                }
                break;
                
            case "Lives":
                if (newValue is int lives)
                {
                    OnLivesChanged?.Invoke(lives);
                    CheckGameOverCondition(lives);
                }
                break;
                
            case "Level":
                if (newValue is int level)
                {
                    HandleLevelChange(level);
                }
                break;
        }
    }
    
    #endregion
    
    #region 游戏逻辑处理
    
    /// <summary>
    /// 处理状态特定逻辑
    /// </summary>
    private void HandleStateSpecificLogic(string stateName)
    {
        switch (stateName)
        {
            case "AdvancedGamePlayState":
                // 游戏开始时的逻辑
                StartGameplayTimer();
                break;
                
            case "AdvancedPauseState":
                // 暂停时的逻辑
                PauseGameplayTimer();
                break;
                
            case "AdvancedGameOverState":
                // 游戏结束时的逻辑
                EndGameSession();
                break;
        }
    }
    
    /// <summary>
    /// 检查分数里程碑
    /// </summary>
    private void CheckScoreMilestones(int score)
    {
        // 每1000分触发一次里程碑事件
        if (score > 0 && score % 1000 == 0)
        {
            Debug.Log($"[里程碑] 达到 {score} 分！");
            // 可以在这里添加奖励逻辑
            TriggerScoreMilestone(score);
        }
    }
    
    /// <summary>
    /// 检查游戏结束条件
    /// </summary>
    private void CheckGameOverCondition(int lives)
    {
        if (lives <= 0)
        {
            Debug.Log("[游戏逻辑] 生命值耗尽，游戏结束");
            stateMachine.ChangeState<AdvancedGameOverState>();
        }
    }
    
    /// <summary>
    /// 处理关卡变化
    /// </summary>
    private void HandleLevelChange(int newLevel)
    {
        Debug.Log($"[关卡变化] 进入第 {newLevel} 关");
        
        // 增加游戏难度
        AdjustGameDifficulty(newLevel);
        
        // 保存进度
        SaveLevelProgress(newLevel);
    }
    
    #endregion
    
    #region 输入处理
    
    /// <summary>
    /// 处理全局输入
    /// </summary>
    private void HandleGlobalInput()
    {
        // ESC键 - 智能暂停/返回
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
        
        // F1键 - 显示调试信息
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ShowDebugInformation();
        }
        
        // F2键 - 性能报告
        if (Input.GetKeyDown(KeyCode.F2) && enablePerformanceMonitoring)
        {
            performanceProfiler?.PrintReport();
        }
        
        // F3键 - 状态历史
        if (Input.GetKeyDown(KeyCode.F3) && enableStateHistory)
        {
            ShowStateHistory();
        }
    }
    
    /// <summary>
    /// 处理ESC键的智能逻辑
    /// </summary>
    private void HandleEscapeKey()
    {
        string currentState = stateMachine.CurrentNode;
        
        switch (currentState)
        {
            case "AdvancedGamePlayState":
                // 游戏中按ESC暂停
                stateMachine.PushState<AdvancedPauseState>();
                break;
                
            case "AdvancedPauseState":
                // 暂停中按ESC返回主菜单
                stateMachine.ClearStateStack();
                stateMachine.ChangeState<AdvancedMainMenuState>();
                break;
                
            case "SettingsState":
            case "LeaderboardState":
                // 设置或排行榜中按ESC返回主菜单
                stateMachine.ChangeState<AdvancedMainMenuState>();
                break;
                
            default:
                Debug.Log($"[输入] 当前状态 {currentState} 不处理ESC键");
                break;
        }
    }
    
    #endregion
    
    #region 公共接口
    
    /// <summary>
    /// 强制结束游戏
    /// </summary>
    public void ForceEndGame(string reason)
    {
        Debug.Log($"[游戏管理器] 强制结束游戏: {reason}");
        
        // 记录结束原因
        if (currentSession != null)
        {
            currentSession.EndReason = reason;
        }
        
        // 切换到游戏结束状态
        stateMachine.ChangeState<AdvancedGameOverState>();
    }
    
    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("[游戏管理器] 重新开始游戏");
        
        // 结束当前会话
        EndGameSession();
        
        // 清理状态栈
        stateMachine.ClearStateStack();
        
        // 开始新会话
        StartGameSession();
    }
    
    /// <summary>
    /// 获取当前游戏统计
    /// </summary>
    public GameStatistics GetCurrentStatistics()
    {
        return new GameStatistics
        {
            CurrentScore = stateMachine.GetBlackboardValue<int>("Score"),
            CurrentLevel = stateMachine.GetBlackboardValue<int>("Level"),
            RemainingLives = stateMachine.GetBlackboardValue<int>("Lives"),
            PlayTime = currentSession?.Duration ?? 0f,
            StateChangeCount = currentSession?.StateChangeCount ?? 0
        };
    }
    
    /// <summary>
    /// 获取状态机调试信息
    /// </summary>
    public StateMachineDebugInfo GetDebugInfo()
    {
        return new StateMachineDebugInfo
        {
            CurrentState = stateMachine.CurrentNode,
            PreviousState = stateMachine.PreviousNode,
            StateStackDepth = stateMachine.GetStateStackDepth(),
            BlackboardItemCount = GetBlackboardItemCount(),
            SessionId = currentSession?.SessionId ?? "None"
        };
    }
    
    #endregion
    
    #region 辅助方法
    
    /// <summary>
    /// 创建默认配置
    /// </summary>
    private StateMachineConfig CreateDefaultConfig()
    {
        return new StateMachineConfig
        {
            enableDebugLog = true,
            enableStateHistory = true,
            maxHistorySize = 50,
            initialNodeCapacity = 16,
            initialBlackboardCapacity = 32,
            eventPoolSize = 8,
            enableStateValidation = true,
            enablePriorityCheck = true,
            maxStateTransitionTime = 0.1f
        };
    }
    
    /// <summary>
    /// 初始化游戏数据
    /// </summary>
    private void InitializeGameData()
    {
        playerData = new PlayerData
        {
            PlayerId = SystemInfo.deviceUniqueIdentifier,
            PlayerName = "Player",
            HighScore = PlayerPrefs.GetInt("HighScore", 0),
            TotalPlayTime = PlayerPrefs.GetFloat("TotalPlayTime", 0f)
        };
    }
    
    /// <summary>
    /// 设置初始黑板数据
    /// </summary>
    private void SetupInitialBlackboardData()
    {
        stateMachine.SetBlackboardValue("Score", 0);
        stateMachine.SetBlackboardValue("Lives", maxLives);
        stateMachine.SetBlackboardValue("Level", 1);
        stateMachine.SetBlackboardValue("GameStartTime", Time.time);
        stateMachine.SetBlackboardValue("PlayerData", playerData);
    }
    
    /// <summary>
    /// 显示调试信息
    /// </summary>
    private void ShowDebugInformation()
    {
        var debugInfo = GetDebugInfo();
        var statistics = GetCurrentStatistics();
        
        Debug.Log("=== 游戏调试信息 ===");
        Debug.Log($"当前状态: {debugInfo.CurrentState}");
        Debug.Log($"上一状态: {debugInfo.PreviousState}");
        Debug.Log($"状态栈深度: {debugInfo.StateStackDepth}");
        Debug.Log($"黑板项目数: {debugInfo.BlackboardItemCount}");
        Debug.Log($"当前分数: {statistics.CurrentScore}");
        Debug.Log($"当前关卡: {statistics.CurrentLevel}");
        Debug.Log($"剩余生命: {statistics.RemainingLives}");
        Debug.Log($"游戏时间: {statistics.PlayTime:F2}秒");
    }
    
    /// <summary>
    /// 显示状态历史
    /// </summary>
    private void ShowStateHistory()
    {
        if (stateHistory == null) return;
        
        var history = stateHistory.GetHistory();
        Debug.Log("=== 状态变化历史 ===");
        
        for (int i = 0; i < history.Length; i++)
        {
            var record = history[i];
            Debug.Log($"{i + 1}. {record.FromState} -> {record.ToState} (时间: {record.Timestamp:F2})");
        }
    }
    
    /// <summary>
    /// 清理状态机资源
    /// </summary>
    private void CleanupStateMachine()
    {
        if (stateMachine != null)
        {
            // 取消事件订阅
            stateMachine.OnStateChanged -= OnStateMachineStateChanged;
            stateMachine.OnStateEntered -= OnStateMachineStateEntered;
            stateMachine.OnStateExited -= OnStateMachineStateExited;
            stateMachine.OnBlackboardValueChanged -= OnStateMachineBlackboardChanged;
            
            // 清理状态机（如果实现了IDisposable）
            Debug.Log("[游戏管理器] 状态机资源清理完成");
        }
    }
    
    // 其他辅助方法的空实现，实际项目中需要具体实现
    private void PlayStateTransitionSound(string stateName) { }
    private void UpdateUIForState(string stateName) { }
    private void CleanupStateResources(string stateName) { }
    private void StartGameplayTimer() { }
    private void PauseGameplayTimer() { }
    private void TriggerScoreMilestone(int score) { }
    private void AdjustGameDifficulty(int level) { }
    private void SaveLevelProgress(int level) { }
    private void SaveGameSessionData() { }
    private int GetBlackboardItemCount() { return 0; }
    
    #endregion
}

#region 数据结构定义

/// <summary>
/// 游戏会话数据
/// </summary>
[System.Serializable]
public class GameSessionData
{
    public string SessionId;
    public float StartTime;
    public float EndTime;
    public float Duration;
    public int InitialLives;
    public int StateChangeCount;
    public string EndReason;
}

/// <summary>
/// 玩家数据
/// </summary>
[System.Serializable]
public class PlayerData
{
    public string PlayerId;
    public string PlayerName;
    public int HighScore;
    public float TotalPlayTime;
}

/// <summary>
/// 游戏统计信息
/// </summary>
[System.Serializable]
public class GameStatistics
{
    public int CurrentScore;
    public int CurrentLevel;
    public int RemainingLives;
    public float PlayTime;
    public int StateChangeCount;
}

/// <summary>
/// 状态机调试信息
/// </summary>
[System.Serializable]
public class StateMachineDebugInfo
{
    public string CurrentState;
    public string PreviousState;
    public int StateStackDepth;
    public int BlackboardItemCount;
    public string SessionId;
}

#endregion