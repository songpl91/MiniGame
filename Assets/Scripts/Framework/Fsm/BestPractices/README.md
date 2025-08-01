# 状态机框架最佳实践指南

## 概述

本文档提供了状态机框架的详细使用指南和最佳实践，帮助开发者充分利用框架的功能，构建高效、可维护的游戏状态管理系统。

## 目录

1. [快速开始](#快速开始)
2. [核心概念](#核心概念)
3. [使用示例](#使用示例)
4. [性能优化](#性能优化)
5. [最佳实践](#最佳实践)
6. [常见问题](#常见问题)
7. [扩展功能](#扩展功能)

## 快速开始

### 基础使用

```csharp
// 1. 创建状态机
var stateMachine = StateMachineFactory.CreateGameStateMachine(this);

// 2. 添加状态
stateMachine.AddNode<MainMenuState>();
stateMachine.AddNode<GamePlayState>();
stateMachine.AddNode<PauseState>();

// 3. 设置状态转换规则
stateMachine.SetStateTransition(new GameStateTransition());

// 4. 启动状态机
stateMachine.Run<MainMenuState>();

// 5. 在Update中更新
void Update() {
    stateMachine.Update();
}
```

### 创建自定义状态

```csharp
public class MyGameState : EnhancedBaseStateNode
{
    public override int Priority => 5; // 设置优先级
    
    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("进入我的游戏状态");
        
        // 设置黑板数据
        SetBlackboardValue("GameStartTime", Time.time);
    }
    
    public override void OnUpdate()
    {
        // 游戏逻辑更新
        if (Input.GetKeyDown(KeyCode.P))
        {
            stateMachine.PushState<PauseState>();
        }
    }
    
    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("退出我的游戏状态");
    }
    
    public override bool CanExit()
    {
        // 检查是否可以安全退出
        return !IsInCriticalMoment();
    }
}
```

## 核心概念

### 1. 状态节点 (State Node)

状态节点是状态机的基本单元，每个状态负责特定的游戏逻辑。

**生命周期方法：**
- `OnCreate()`: 状态创建时调用（仅一次）
- `OnEnter()`: 进入状态时调用
- `OnUpdate()`: 每帧更新时调用
- `OnExit()`: 退出状态时调用
- `OnPause()`: 状态被暂停时调用（状态栈）
- `OnResume()`: 状态恢复时调用（状态栈）

**重要属性：**
- `Priority`: 状态优先级，高优先级状态可以打断低优先级状态
- `CanExit()`: 检查状态是否可以安全退出

### 2. 黑板系统 (Blackboard)

黑板系统用于在状态间共享数据，避免状态间的直接依赖。

```csharp
// 设置数据
SetBlackboardValue("PlayerHealth", 100f);
SetBlackboardValue("Score", 1500);
SetBlackboardValue("CurrentLevel", 3);

// 获取数据
float health = GetBlackboardValue<float>("PlayerHealth");
int score = GetBlackboardValue<int>("Score", 0); // 带默认值

// 检查数据存在
if (HasBlackboardValue("BossDefeated"))
{
    // 处理Boss被击败的逻辑
}
```

### 3. 状态栈 (State Stack)

状态栈允许暂停当前状态并切换到新状态，之后可以恢复到原状态。

```csharp
// 暂停当前状态，切换到暂停菜单
stateMachine.PushState<PauseMenuState>();

// 恢复到之前的状态
stateMachine.PopState();

// 清空状态栈
stateMachine.ClearStateStack();
```

### 4. 状态转换 (State Transition)

控制状态间的转换规则，确保状态切换的合法性。

```csharp
public class GameStateTransition : DefaultStateTransition
{
    public override bool CanTransition(Type fromState, Type toState)
    {
        // 自定义转换规则
        if (fromState == typeof(LoadingState))
        {
            return toState == typeof(MainMenuState);
        }
        
        return base.CanTransition(fromState, toState);
    }
    
    public override void OnTransition(Type fromState, Type toState)
    {
        Debug.Log($"状态转换: {fromState?.Name} -> {toState.Name}");
        
        // 执行转换特定逻辑
        PlayTransitionEffect(fromState, toState);
    }
}
```

## 使用示例

### 1. 游戏主流程状态机

适用于管理游戏的主要流程：加载、主菜单、游戏进行、暂停、结束等。

```csharp
public class GameFlowManager : MonoBehaviour
{
    private StateMachine gameStateMachine;
    
    void Start()
    {
        // 创建配置
        var config = new StateMachineConfig
        {
            EnableDebugLog = true,
            EnableStateHistory = true,
            MaxHistoryCount = 20
        };
        
        // 创建状态机
        gameStateMachine = StateMachineFactory.CreateGameStateMachine(this, config);
        
        // 添加状态
        gameStateMachine.AddNode<LoadingState>();
        gameStateMachine.AddNode<MainMenuState>();
        gameStateMachine.AddNode<GamePlayState>();
        gameStateMachine.AddNode<PauseState>();
        gameStateMachine.AddNode<GameOverState>();
        
        // 设置转换规则
        gameStateMachine.SetStateTransition(new GameStateTransition());
        
        // 监听事件
        gameStateMachine.OnStateChanged += OnGameStateChanged;
        
        // 启动
        gameStateMachine.Run<LoadingState>();
    }
    
    void Update()
    {
        gameStateMachine?.Update();
    }
    
    private void OnGameStateChanged(Type fromState, Type toState)
    {
        Debug.Log($"游戏状态变化: {fromState?.Name} -> {toState.Name}");
        
        // 可以在这里处理全局状态变化逻辑
        UpdateUI(toState);
        PlayStateChangeSound(toState);
    }
}
```

### 2. 角色AI状态机

适用于控制NPC的行为逻辑。

```csharp
public class EnemyAI : MonoBehaviour
{
    private StateMachine aiStateMachine;
    private Transform player;
    
    void Start()
    {
        // 创建AI状态机（较低更新频率）
        var config = new StateMachineConfig
        {
            EnableDebugLog = false, // AI通常不需要详细日志
            EnablePerformanceMonitoring = true
        };
        
        aiStateMachine = StateMachineFactory.CreateAIStateMachine(this, config);
        
        // 设置AI参数
        aiStateMachine.SetBlackboardValue("DetectionRange", 10f);
        aiStateMachine.SetBlackboardValue("AttackRange", 2f);
        aiStateMachine.SetBlackboardValue("PatrolSpeed", 2f);
        aiStateMachine.SetBlackboardValue("ChaseSpeed", 4f);
        
        // 添加AI状态
        aiStateMachine.AddNode<IdleState>();
        aiStateMachine.AddNode<PatrolState>();
        aiStateMachine.AddNode<ChaseState>();
        aiStateMachine.AddNode<AttackState>();
        aiStateMachine.AddNode<DeadState>();
        
        // 启动AI
        aiStateMachine.Run<IdleState>();
    }
    
    void Update()
    {
        // 更新玩家信息
        UpdatePlayerInfo();
        
        // 更新AI状态机
        aiStateMachine?.Update();
    }
    
    private void UpdatePlayerInfo()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            aiStateMachine.SetBlackboardValue("DistanceToPlayer", distance);
            aiStateMachine.SetBlackboardValue("PlayerPosition", player.position);
        }
    }
}
```

### 3. UI界面流程状态机

适用于管理复杂的UI界面流程。

```csharp
public class UIManager : MonoBehaviour
{
    private StateMachine uiStateMachine;
    
    void Start()
    {
        uiStateMachine = StateMachineFactory.CreateUIStateMachine(this);
        
        // 添加UI状态
        uiStateMachine.AddNode<LoginUIState>();
        uiStateMachine.AddNode<MainMenuUIState>();
        uiStateMachine.AddNode<InventoryUIState>();
        uiStateMachine.AddNode<ShopUIState>();
        uiStateMachine.AddNode<SettingsUIState>();
        
        // 启动UI流程
        uiStateMachine.Run<LoginUIState>();
    }
    
    void Update()
    {
        uiStateMachine?.Update();
        
        // 全局UI输入处理
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackButton();
        }
    }
    
    private void HandleBackButton()
    {
        // 如果有状态栈，返回上一界面
        if (uiStateMachine.GetStateStackDepth() > 0)
        {
            uiStateMachine.PopState();
        }
        else
        {
            // 否则执行默认返回逻辑
            Application.Quit();
        }
    }
}
```

## 性能优化

### 1. 使用高性能状态机

对于性能敏感的场景，使用优化版本的状态机：

```csharp
// 创建高性能状态机
var optimizedStateMachine = PerformanceOptimizedStateMachineFactory
    .CreateHighPerformanceGameStateMachine(this, 16, 0.016f);

// 设置更新间隔（降低更新频率）
optimizedStateMachine.SetUpdateInterval(0.033f); // 30 FPS

// 启用性能优化
optimizedStateMachine.SetPerformanceOptimization(true);
```

### 2. 缓存频繁访问的数据

```csharp
public class OptimizedGameState : EnhancedBaseStateNode
{
    // 缓存频繁访问的黑板数据
    private float cachedPlayerHealth;
    private Vector3 cachedPlayerPosition;
    private float lastCacheTime;
    private const float CACHE_INTERVAL = 0.1f; // 缓存更新间隔
    
    public override void OnEnter()
    {
        base.OnEnter();
        UpdateCache();
    }
    
    public override void OnUpdate()
    {
        // 定期更新缓存
        if (Time.time - lastCacheTime >= CACHE_INTERVAL)
        {
            UpdateCache();
        }
        
        // 使用缓存的数据进行逻辑处理
        ProcessGameLogic();
    }
    
    private void UpdateCache()
    {
        cachedPlayerHealth = GetBlackboardValue<float>("PlayerHealth");
        cachedPlayerPosition = GetBlackboardValue<Vector3>("PlayerPosition");
        lastCacheTime = Time.time;
    }
    
    private void ProcessGameLogic()
    {
        // 使用缓存数据，避免频繁的黑板访问
        if (cachedPlayerHealth <= 0)
        {
            ChangeState<GameOverState>();
        }
    }
}
```

### 3. 批量处理和对象池

```csharp
public class PooledEventState : EnhancedBaseStateNode
{
    // 使用对象池避免频繁分配
    private Queue<GameEvent> eventPool = new Queue<GameEvent>();
    private List<GameEvent> activeEvents = new List<GameEvent>();
    
    private GameEvent GetPooledEvent()
    {
        if (eventPool.Count > 0)
        {
            return eventPool.Dequeue();
        }
        return new GameEvent();
    }
    
    private void ReturnEventToPool(GameEvent evt)
    {
        evt.Reset();
        eventPool.Enqueue(evt);
    }
    
    public override void OnUpdate()
    {
        // 批量处理事件
        ProcessEventsInBatch();
    }
    
    private void ProcessEventsInBatch()
    {
        for (int i = activeEvents.Count - 1; i >= 0; i--)
        {
            GameEvent evt = activeEvents[i];
            if (evt.IsCompleted)
            {
                activeEvents.RemoveAt(i);
                ReturnEventToPool(evt);
            }
        }
    }
}
```

## 最佳实践

### 1. 状态设计原则

**单一职责原则**
```csharp
// ✅ 好的设计 - 每个状态职责单一
public class LoadingState : EnhancedBaseStateNode
{
    // 只负责资源加载
}

public class MainMenuState : EnhancedBaseStateNode
{
    // 只负责主菜单逻辑
}

// ❌ 不好的设计 - 状态职责混乱
public class GameState : EnhancedBaseStateNode
{
    // 同时处理加载、菜单、游戏逻辑等多种职责
}
```

**明确的状态转换**
```csharp
public class WellDesignedState : EnhancedBaseStateNode
{
    public override void OnUpdate()
    {
        // ✅ 明确的转换条件
        if (IsLoadingComplete())
        {
            ChangeState<MainMenuState>();
        }
        else if (HasError())
        {
            ChangeState<ErrorState>();
        }
    }
    
    public override bool CanExit()
    {
        // ✅ 明确的退出条件
        return !IsInCriticalOperation();
    }
}
```

### 2. 数据管理最佳实践

**使用黑板系统共享数据**
```csharp
// ✅ 好的做法 - 通过黑板共享数据
public class StateA : EnhancedBaseStateNode
{
    public override void OnEnter()
    {
        SetBlackboardValue("SharedData", someValue);
    }
}

public class StateB : EnhancedBaseStateNode
{
    public override void OnEnter()
    {
        var data = GetBlackboardValue<SomeType>("SharedData");
        // 使用共享数据
    }
}

// ❌ 不好的做法 - 状态间直接依赖
public class BadStateA : EnhancedBaseStateNode
{
    public static SomeType SharedData; // 静态变量
}

public class BadStateB : EnhancedBaseStateNode
{
    public override void OnEnter()
    {
        var data = BadStateA.SharedData; // 直接依赖
    }
}
```

**合理的数据生命周期管理**
```csharp
public class DataManagedState : EnhancedBaseStateNode
{
    public override void OnEnter()
    {
        // 设置状态相关数据
        SetBlackboardValue("StateSpecificData", initialValue);
    }
    
    public override void OnExit()
    {
        // 清理状态相关数据
        RemoveBlackboardValue("StateSpecificData");
        
        // 保留需要传递的数据
        // SetBlackboardValue("PersistentData", finalValue);
    }
}
```

### 3. 错误处理和调试

**完善的错误处理**
```csharp
public class RobustState : EnhancedBaseStateNode
{
    public override void OnEnter()
    {
        try
        {
            base.OnEnter();
            InitializeState();
        }
        catch (Exception e)
        {
            Debug.LogError($"状态初始化失败: {e.Message}");
            ChangeState<ErrorRecoveryState>();
        }
    }
    
    public override void OnUpdate()
    {
        try
        {
            UpdateLogic();
        }
        catch (Exception e)
        {
            Debug.LogError($"状态更新失败: {e.Message}");
            // 尝试恢复或切换到安全状态
            HandleError(e);
        }
    }
    
    private void HandleError(Exception e)
    {
        // 根据错误类型决定处理策略
        if (e is CriticalGameException)
        {
            ChangeState<GameOverState>();
        }
        else
        {
            // 记录错误但继续运行
            LogError(e);
        }
    }
}
```

**丰富的调试信息**
```csharp
public class DebuggableState : EnhancedBaseStateNode
{
    private float stateTime;
    private int updateCount;
    
    public override void OnEnter()
    {
        base.OnEnter();
        stateTime = 0f;
        updateCount = 0;
    }
    
    public override void OnUpdate()
    {
        stateTime += Time.deltaTime;
        updateCount++;
        
        // 游戏逻辑...
    }
    
    public override string GetDebugInfo()
    {
        return base.GetDebugInfo() + 
               $"\n状态运行时间: {stateTime:F2}秒" +
               $"\n更新次数: {updateCount}" +
               $"\n平均FPS: {updateCount / stateTime:F1}";
    }
}
```

### 4. 状态栈使用指南

**适合使用状态栈的场景**
```csharp
// ✅ 适合：暂停菜单
public class GamePlayState : EnhancedBaseStateNode
{
    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 暂停游戏，显示暂停菜单
            stateMachine.PushState<PauseMenuState>();
        }
    }
}

// ✅ 适合：弹出式对话框
public class DialogState : EnhancedBaseStateNode
{
    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // 关闭对话框，返回之前的状态
            stateMachine.PopState();
        }
    }
}
```

**不适合使用状态栈的场景**
```csharp
// ❌ 不适合：游戏流程转换
public class BadExample : EnhancedBaseStateNode
{
    public override void OnUpdate()
    {
        if (gameCompleted)
        {
            // 不应该用Push，应该用ChangeState
            stateMachine.PushState<GameOverState>(); // 错误
        }
    }
}
```

## 常见问题

### Q1: 状态切换失败，提示优先级不足

**问题原因：** 新状态的优先级低于当前状态。

**解决方案：**
```csharp
// 方案1：调整状态优先级
public class HighPriorityState : EnhancedBaseStateNode
{
    public override int Priority => 10; // 提高优先级
}

// 方案2：在当前状态中主动切换
public class CurrentState : EnhancedBaseStateNode
{
    public override void OnUpdate()
    {
        if (shouldChangeState)
        {
            ChangeState<TargetState>(); // 主动切换
        }
    }
}
```

### Q2: 黑板数据类型转换失败

**问题原因：** 数据类型不匹配或数据不存在。

**解决方案：**
```csharp
// 使用泛型方法和默认值
float health = GetBlackboardValue<float>("PlayerHealth", 100f);

// 检查数据存在性
if (HasBlackboardValue("PlayerHealth"))
{
    float health = GetBlackboardValue<float>("PlayerHealth");
}

// 类型安全的获取
if (TryGetBlackboardValue<float>("PlayerHealth", out float health))
{
    // 使用health
}
```

### Q3: 状态栈溢出或混乱

**问题原因：** 过度使用状态栈或忘记PopState。

**解决方案：**
```csharp
// 限制状态栈深度
public class SafeStateMachine : StateMachine
{
    private const int MAX_STACK_DEPTH = 5;
    
    public override void PushState<T>()
    {
        if (GetStateStackDepth() >= MAX_STACK_DEPTH)
        {
            Debug.LogWarning("状态栈深度超限，清空状态栈");
            ClearStateStack();
        }
        
        base.PushState<T>();
    }
}

// 自动清理状态栈
public class AutoCleanupState : EnhancedBaseStateNode
{
    public override void OnEnter()
    {
        base.OnEnter();
        
        // 在某些关键状态清空状态栈
        if (IsGameFlowState())
        {
            stateMachine.ClearStateStack();
        }
    }
}
```

### Q4: 性能问题

**问题原因：** 频繁的状态切换、复杂的Update逻辑、内存分配等。

**解决方案：**
```csharp
// 使用高性能状态机
var optimizedSM = PerformanceOptimizedStateMachineFactory
    .CreateHighPerformanceGameStateMachine(this);

// 降低更新频率
optimizedSM.SetUpdateInterval(0.033f); // 30 FPS

// 缓存频繁访问的数据
private float cachedData;
private float lastCacheTime;

public override void OnUpdate()
{
    if (Time.time - lastCacheTime > 0.1f)
    {
        cachedData = GetBlackboardValue<float>("ExpensiveData");
        lastCacheTime = Time.time;
    }
    
    // 使用cachedData进行逻辑处理
}
```

## 扩展功能

### 1. 自定义状态转换规则

```csharp
public class CustomStateTransition : DefaultStateTransition
{
    public override bool CanTransition(Type fromState, Type toState)
    {
        // 实现复杂的转换规则
        return CheckTransitionMatrix(fromState, toState) &&
               CheckGameConditions(fromState, toState) &&
               CheckPlayerPermissions(fromState, toState);
    }
    
    private bool CheckTransitionMatrix(Type from, Type to)
    {
        // 基于转换矩阵的检查
        var allowedTransitions = GetTransitionMatrix();
        return allowedTransitions.ContainsKey(from) &&
               allowedTransitions[from].Contains(to);
    }
    
    private bool CheckGameConditions(Type from, Type to)
    {
        // 基于游戏条件的检查
        if (to == typeof(BossState))
        {
            return PlayerLevel >= 10;
        }
        
        return true;
    }
}
```

### 2. 状态机组合和嵌套

```csharp
public class CompositeStateMachine : EnhancedBaseStateNode
{
    private StateMachine subStateMachine;
    
    public override void OnCreate()
    {
        base.OnCreate();
        
        // 创建子状态机
        subStateMachine = StateMachineFactory.CreateGameStateMachine(
            stateMachine.Owner);
        
        // 配置子状态机
        subStateMachine.AddNode<SubState1>();
        subStateMachine.AddNode<SubState2>();
    }
    
    public override void OnEnter()
    {
        base.OnEnter();
        subStateMachine.Run<SubState1>();
    }
    
    public override void OnUpdate()
    {
        subStateMachine.Update();
        
        // 检查子状态机完成条件
        if (IsSubStateMachineCompleted())
        {
            ChangeState<NextMainState>();
        }
    }
    
    public override void OnExit()
    {
        base.OnExit();
        subStateMachine.Dispose();
    }
}
```

### 3. 状态机持久化

```csharp
public class PersistentStateMachine : StateMachine
{
    public void SaveState(string filePath)
    {
        var saveData = new StateMachineSaveData
        {
            CurrentStateType = CurrentStateType?.AssemblyQualifiedName,
            BlackboardData = SerializeBlackboard(),
            StateStack = SerializeStateStack(),
            Timestamp = DateTime.Now
        };
        
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(filePath, json);
    }
    
    public void LoadState(string filePath)
    {
        if (!File.Exists(filePath))
            return;
        
        string json = File.ReadAllText(filePath);
        var saveData = JsonUtility.FromJson<StateMachineSaveData>(json);
        
        // 恢复黑板数据
        DeserializeBlackboard(saveData.BlackboardData);
        
        // 恢复状态栈
        DeserializeStateStack(saveData.StateStack);
        
        // 恢复当前状态
        if (!string.IsNullOrEmpty(saveData.CurrentStateType))
        {
            Type stateType = Type.GetType(saveData.CurrentStateType);
            if (stateType != null)
            {
                ChangeState(stateType);
            }
        }
    }
}
```

## 总结

状态机框架提供了强大而灵活的状态管理能力，通过合理使用框架的各项功能，可以构建出高效、可维护的游戏状态系统。关键要点：

1. **合理设计状态** - 遵循单一职责原则，明确状态转换条件
2. **善用黑板系统** - 通过黑板共享数据，避免状态间直接依赖
3. **适度使用状态栈** - 只在需要暂停/恢复的场景使用
4. **注重性能优化** - 使用缓存、对象池、批量处理等技术
5. **完善错误处理** - 添加适当的异常处理和恢复机制
6. **丰富调试信息** - 提供详细的调试和监控功能

通过遵循这些最佳实践，您可以充分发挥状态机框架的优势，构建出稳定、高效的游戏状态管理系统。