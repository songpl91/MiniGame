using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Text;

/// <summary>
/// 高性能状态机实现 - 展示性能优化的最佳实践
/// </summary>
public class PerformanceOptimizedStateMachine : IDisposable
{
    // 核心组件
    private GameObject owner;
    private Dictionary<Type, IStateNode> stateNodes;
    private Dictionary<Type, StateNodeInfo> stateInfoCache;
    private IStateTransition stateTransition;
    
    // 状态管理
    private IStateNode currentState;
    private Type currentStateType;
    private Stack<StateStackInfo> stateStack;
    
    // 黑板系统 - 使用更高效的数据结构
    private Dictionary<string, object> blackboard;
    private Dictionary<string, Type> blackboardTypes; // 缓存类型信息
    private HashSet<string> dirtyBlackboardKeys; // 标记变化的键
    
    // 事件系统 - 使用对象池
    private Queue<StateChangeEvent> eventPool;
    private List<Action<Type, Type>> stateChangeListeners;
    private List<Action<Type>> stateEnterListeners;
    private List<Action<Type>> stateExitListeners;
    
    // 性能优化
    private float lastUpdateTime;
    private float updateInterval = 0.016f; // 60 FPS
    private bool enablePerformanceOptimization = true;
    
    // 缓存和预分配
    private List<Type> tempTypeList; // 复用的临时列表
    private StringBuilder debugStringBuilder; // 复用的StringBuilder
    
    // 统计信息
    private Dictionary<Type, StatePerformanceInfo> performanceStats;
    private int totalStateChanges;
    private float totalRunTime;
    
    public GameObject Owner => owner;
    public Type CurrentStateType => currentStateType;
    public int StateStackDepth => stateStack?.Count ?? 0;
    
    // 事件
    public event Action<Type, Type> OnStateChanged;
    public event Action<Type> OnStateEntered;
    public event Action<Type> OnStateExited;
    public event Action<string, object> OnBlackboardValueChanged;
    
    /// <summary>
    /// 状态节点信息缓存
    /// </summary>
    private class StateNodeInfo
    {
        public int Priority;
        public bool CanBeInterrupted;
        public float LastEnterTime;
        public float TotalRunTime;
        public int EnterCount;
        
        public StateNodeInfo(IStateNode node)
        {
            Priority = node.Priority;
            CanBeInterrupted = true; // 默认可被打断
        }
    }
    
    /// <summary>
    /// 状态栈信息
    /// </summary>
    private class StateStackInfo
    {
        public Type StateType;
        public IStateNode StateNode;
        public float PauseTime;
        
        public StateStackInfo(Type stateType, IStateNode stateNode)
        {
            StateType = stateType;
            StateNode = stateNode;
            PauseTime = Time.time;
        }
    }
    
    /// <summary>
    /// 状态变化事件 - 使用对象池
    /// </summary>
    private class StateChangeEvent
    {
        public Type FromState;
        public Type ToState;
        public float Timestamp;
        
        public void Reset()
        {
            FromState = null;
            ToState = null;
            Timestamp = 0f;
        }
    }
    
    /// <summary>
    /// 状态性能信息
    /// </summary>
    private class StatePerformanceInfo
    {
        public int EnterCount;
        public float TotalRunTime;
        public float AverageRunTime => EnterCount > 0 ? TotalRunTime / EnterCount : 0f;
        public float LastEnterTime;
        public float MaxRunTime;
        public float MinRunTime = float.MaxValue;
    }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public PerformanceOptimizedStateMachine(GameObject owner, int initialCapacity = 16)
    {
        this.owner = owner;
        
        // 预分配集合，避免运行时扩容
        stateNodes = new Dictionary<Type, IStateNode>(initialCapacity);
        stateInfoCache = new Dictionary<Type, StateNodeInfo>(initialCapacity);
        stateStack = new Stack<StateStackInfo>(8); // 通常状态栈不会很深
        
        blackboard = new Dictionary<string, object>(32);
        blackboardTypes = new Dictionary<string, Type>(32);
        dirtyBlackboardKeys = new HashSet<string>();
        
        // 事件系统初始化
        eventPool = new Queue<StateChangeEvent>(16);
        stateChangeListeners = new List<Action<Type, Type>>(8);
        stateEnterListeners = new List<Action<Type>>(8);
        stateExitListeners = new List<Action<Type>>(8);
        
        // 预分配对象池
        for (int i = 0; i < 16; i++)
        {
            eventPool.Enqueue(new StateChangeEvent());
        }
        
        // 性能统计
        performanceStats = new Dictionary<Type, StatePerformanceInfo>(initialCapacity);
        
        // 复用对象
        tempTypeList = new List<Type>(8);
        debugStringBuilder = new StringBuilder(256);
        
        // 设置默认状态转换
        stateTransition = new DefaultStateTransition();
        
        Debug.Log("[性能优化状态机] 初始化完成");
    }
    
    /// <summary>
    /// 添加状态节点 - 优化版本
    /// </summary>
    public void AddNode<T>() where T : class, IStateNode, new()
    {
        Type stateType = typeof(T);
        
        if (stateNodes.ContainsKey(stateType))
        {
            Debug.LogWarning($"[性能优化状态机] 状态 {stateType.Name} 已存在");
            return;
        }
        
        // 创建状态节点
        T stateNode = new T();
        // stateNode.SetStateMachine(this);
        // stateNode.OnCreate();
        
        // 缓存状态信息
        StateNodeInfo stateInfo = new StateNodeInfo(stateNode);
        
        stateNodes[stateType] = stateNode;
        stateInfoCache[stateType] = stateInfo;
        
        // 初始化性能统计
        performanceStats[stateType] = new StatePerformanceInfo();
        
        Debug.Log($"[性能优化状态机] 添加状态: {stateType.Name}, 优先级: {stateInfo.Priority}");
    }
    
    /// <summary>
    /// 运行状态机 - 优化版本
    /// </summary>
    public void Run<T>() where T : class, IStateNode
    {
        Type stateType = typeof(T);
        
        if (!stateNodes.TryGetValue(stateType, out IStateNode stateNode))
        {
            Debug.LogError($"[性能优化状态机] 状态 {stateType.Name} 不存在");
            return;
        }
        
        // 快速路径：如果已经是当前状态，直接返回
        if (currentStateType == stateType)
        {
            return;
        }
        
        ChangeStateInternal(stateType, stateNode);
    }
    
    /// <summary>
    /// 更新状态机 - 优化版本
    /// </summary>
    public void Update()
    {
        // 性能优化：控制更新频率
        if (enablePerformanceOptimization)
        {
            float currentTime = Time.time;
            if (currentTime - lastUpdateTime < updateInterval)
            {
                return;
            }
            lastUpdateTime = currentTime;
        }
        
        // 更新当前状态
        if (currentState != null)
        {
            float startTime = Time.realtimeSinceStartup;
            
            currentState.OnUpdate();
            
            // 记录性能数据
            if (performanceStats.TryGetValue(currentStateType, out StatePerformanceInfo perfInfo))
            {
                float updateTime = Time.realtimeSinceStartup - startTime;
                perfInfo.TotalRunTime += updateTime;
            }
        }
        
        totalRunTime += Time.deltaTime;
        
        // 批量处理黑板变化事件
        ProcessBlackboardChanges();
    }
    
    /// <summary>
    /// 状态切换 - 内部优化实现
    /// </summary>
    private void ChangeStateInternal(Type newStateType, IStateNode newStateNode)
    {
        Type oldStateType = currentStateType;
        IStateNode oldState = currentState;
        
        // 检查状态转换是否允许
        if (!stateTransition.CanTransition(oldStateType, newStateType))
        {
            Debug.LogWarning($"[性能优化状态机] 状态转换被拒绝: {oldStateType?.Name ?? "None"} -> {newStateType.Name}");
            return;
        }
        
        // 检查当前状态是否可以退出
        if (oldState != null && !oldState.CanExit())
        {
            Debug.LogWarning($"[性能优化状态机] 当前状态 {oldStateType.Name} 不允许退出");
            return;
        }
        
        // 检查优先级
        if (oldState != null && stateInfoCache.TryGetValue(oldStateType, out StateNodeInfo oldInfo) &&
            stateInfoCache.TryGetValue(newStateType, out StateNodeInfo newInfo))
        {
            if (newInfo.Priority < oldInfo.Priority)
            {
                Debug.LogWarning($"[性能优化状态机] 新状态优先级不足: {newInfo.Priority} < {oldInfo.Priority}");
                return;
            }
        }
        
        float transitionStartTime = Time.realtimeSinceStartup;
        
        // 退出旧状态
        if (oldState != null)
        {
            oldState.OnExit();
            
            // 更新性能统计
            if (performanceStats.TryGetValue(oldStateType, out StatePerformanceInfo oldPerfInfo))
            {
                float runTime = Time.time - oldPerfInfo.LastEnterTime;
                oldPerfInfo.TotalRunTime += runTime;
                oldPerfInfo.MaxRunTime = Mathf.Max(oldPerfInfo.MaxRunTime, runTime);
                oldPerfInfo.MinRunTime = Mathf.Min(oldPerfInfo.MinRunTime, runTime);
            }
            
            // 触发退出事件
            TriggerStateExitEvent(oldStateType);
        }
        
        // 设置新状态
        currentState = newStateNode;
        currentStateType = newStateType;
        
        // 进入新状态
        newStateNode.OnEnter();
        
        // 更新性能统计
        if (performanceStats.TryGetValue(newStateType, out StatePerformanceInfo newPerfInfo))
        {
            newPerfInfo.EnterCount++;
            newPerfInfo.LastEnterTime = Time.time;
        }
        
        // 执行状态转换回调
        stateTransition.OnTransition(oldStateType, newStateType);
        
        // 触发状态变化事件
        TriggerStateChangeEvent(oldStateType, newStateType);
        TriggerStateEnterEvent(newStateType);
        
        totalStateChanges++;
        
        float transitionTime = Time.realtimeSinceStartup - transitionStartTime;
        Debug.Log($"[性能优化状态机] 状态切换: {oldStateType?.Name ?? "None"} -> {newStateType.Name} (耗时: {transitionTime * 1000:F2}ms)");
    }
    
    /// <summary>
    /// 压入状态 - 优化版本
    /// </summary>
    public void PushState<T>() where T : class, IStateNode
    {
        Type stateType = typeof(T);
        
        if (!stateNodes.TryGetValue(stateType, out IStateNode stateNode))
        {
            Debug.LogError($"[性能优化状态机] 状态 {stateType.Name} 不存在");
            return;
        }
        
        // 暂停当前状态
        if (currentState != null)
        {
            if (currentState is EnhancedBaseStateNode enhancedState)
            {
                enhancedState.OnPause();
            }
            
            // 压入状态栈
            stateStack.Push(new StateStackInfo(currentStateType, currentState));
        }
        
        // 切换到新状态
        ChangeStateInternal(stateType, stateNode);
    }
    
    /// <summary>
    /// 弹出状态 - 优化版本
    /// </summary>
    public void PopState()
    {
        if (stateStack.Count == 0)
        {
            Debug.LogWarning("[性能优化状态机] 状态栈为空，无法弹出");
            return;
        }
        
        // 退出当前状态
        if (currentState != null)
        {
            currentState.OnExit();
            TriggerStateExitEvent(currentStateType);
        }
        
        // 恢复上一个状态
        StateStackInfo stackInfo = stateStack.Pop();
        currentState = stackInfo.StateNode;
        currentStateType = stackInfo.StateType;
        
        // 恢复状态
        if (currentState is EnhancedBaseStateNode enhancedState)
        {
            enhancedState.OnResume();
        }
        
        TriggerStateEnterEvent(currentStateType);
        
        float pauseDuration = Time.time - stackInfo.PauseTime;
        Debug.Log($"[性能优化状态机] 恢复状态: {currentStateType.Name} (暂停时长: {pauseDuration:F2}秒)");
    }
    
    /// <summary>
    /// 黑板数据设置 - 优化版本
    /// </summary>
    public void SetBlackboardValue<T>(string key, T value)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("[性能优化状态机] 黑板键不能为空");
            return;
        }
        
        // 检查值是否真的发生了变化
        if (blackboard.TryGetValue(key, out object oldValue))
        {
            if (EqualityComparer<T>.Default.Equals((T)oldValue, value))
            {
                return; // 值没有变化，直接返回
            }
        }
        
        blackboard[key] = value;
        blackboardTypes[key] = typeof(T);
        dirtyBlackboardKeys.Add(key); // 标记为脏数据，稍后批量处理事件
    }
    
    /// <summary>
    /// 黑板数据获取 - 优化版本
    /// </summary>
    public T GetBlackboardValue<T>(string key, T defaultValue = default(T))
    {
        if (string.IsNullOrEmpty(key))
        {
            return defaultValue;
        }
        
        if (blackboard.TryGetValue(key, out object value))
        {
            // 类型检查优化
            if (blackboardTypes.TryGetValue(key, out Type cachedType) && cachedType == typeof(T))
            {
                return (T)value;
            }
            
            // 尝试转换
            if (value is T directValue)
            {
                return directValue;
            }
            
            Debug.LogWarning($"[性能优化状态机] 黑板数据类型不匹配: {key}, 期望: {typeof(T)}, 实际: {value.GetType()}");
        }
        
        return defaultValue;
    }
    
    /// <summary>
    /// 批量处理黑板变化事件
    /// </summary>
    private void ProcessBlackboardChanges()
    {
        if (dirtyBlackboardKeys.Count == 0)
            return;
        
        // 批量触发事件，减少事件调用开销
        foreach (string key in dirtyBlackboardKeys)
        {
            if (blackboard.TryGetValue(key, out object value))
            {
                OnBlackboardValueChanged?.Invoke(key, value);
            }
        }
        
        dirtyBlackboardKeys.Clear();
    }
    
    /// <summary>
    /// 触发状态变化事件 - 使用对象池
    /// </summary>
    private void TriggerStateChangeEvent(Type fromState, Type toState)
    {
        OnStateChanged?.Invoke(fromState, toState);
        
        // 使用对象池减少GC
        StateChangeEvent evt = GetPooledEvent();
        evt.FromState = fromState;
        evt.ToState = toState;
        evt.Timestamp = Time.time;
        
        // 处理事件后归还到池中
        // StartCoroutine(ReturnEventToPool(evt));
    }
    
    /// <summary>
    /// 从对象池获取事件
    /// </summary>
    private StateChangeEvent GetPooledEvent()
    {
        if (eventPool.Count > 0)
        {
            return eventPool.Dequeue();
        }
        
        return new StateChangeEvent();
    }
    
    /// <summary>
    /// 归还事件到对象池
    /// </summary>
    private IEnumerator ReturnEventToPool(StateChangeEvent evt)
    {
        yield return null; // 等待一帧，确保事件处理完成
        
        evt.Reset();
        eventPool.Enqueue(evt);
    }
    
    /// <summary>
    /// 触发状态进入事件
    /// </summary>
    private void TriggerStateEnterEvent(Type stateType)
    {
        OnStateEntered?.Invoke(stateType);
    }
    
    /// <summary>
    /// 触发状态退出事件
    /// </summary>
    private void TriggerStateExitEvent(Type stateType)
    {
        OnStateExited?.Invoke(stateType);
    }
    
    /// <summary>
    /// 获取性能报告
    /// </summary>
    public string GetPerformanceReport()
    {
        debugStringBuilder.Clear();
        debugStringBuilder.AppendLine("=== 状态机性能报告 ===");
        debugStringBuilder.AppendLine($"总运行时间: {totalRunTime:F2}秒");
        debugStringBuilder.AppendLine($"总状态切换次数: {totalStateChanges}");
        debugStringBuilder.AppendLine($"平均切换频率: {totalStateChanges / totalRunTime:F2}次/秒");
        debugStringBuilder.AppendLine();
        
        debugStringBuilder.AppendLine("状态性能统计:");
        foreach (var kvp in performanceStats)
        {
            Type stateType = kvp.Key;
            StatePerformanceInfo info = kvp.Value;
            
            debugStringBuilder.AppendLine($"  {stateType.Name}:");
            debugStringBuilder.AppendLine($"    进入次数: {info.EnterCount}");
            debugStringBuilder.AppendLine($"    总运行时间: {info.TotalRunTime:F3}秒");
            debugStringBuilder.AppendLine($"    平均运行时间: {info.AverageRunTime:F3}秒");
            debugStringBuilder.AppendLine($"    最长运行时间: {info.MaxRunTime:F3}秒");
            debugStringBuilder.AppendLine($"    最短运行时间: {(info.MinRunTime == float.MaxValue ? 0 : info.MinRunTime):F3}秒");
        }
        
        return debugStringBuilder.ToString();
    }
    
    /// <summary>
    /// 设置更新间隔
    /// </summary>
    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(0.001f, interval);
    }
    
    /// <summary>
    /// 启用/禁用性能优化
    /// </summary>
    public void SetPerformanceOptimization(bool enabled)
    {
        enablePerformanceOptimization = enabled;
    }
    
    /// <summary>
    /// 清理资源
    /// </summary>
    public void Dispose()
    {
        // 退出当前状态
        if (currentState != null)
        {
            currentState.OnExit();
            currentState = null;
        }
        
        // 清理状态栈
        while (stateStack.Count > 0)
        {
            StateStackInfo stackInfo = stateStack.Pop();
            if (stackInfo.StateNode is IDisposable disposableState)
            {
                disposableState.Dispose();
            }
        }
        
        // 清理状态节点
        foreach (var kvp in stateNodes)
        {
            if (kvp.Value is IDisposable disposableState)
            {
                disposableState.Dispose();
            }
        }
        
        // 清理集合
        stateNodes?.Clear();
        stateInfoCache?.Clear();
        blackboard?.Clear();
        blackboardTypes?.Clear();
        dirtyBlackboardKeys?.Clear();
        performanceStats?.Clear();
        
        // 清理事件
        OnStateChanged = null;
        OnStateEntered = null;
        OnStateExited = null;
        OnBlackboardValueChanged = null;
        
        Debug.Log("[性能优化状态机] 资源清理完成");
    }
    
    /// <summary>
    /// 获取调试信息
    /// </summary>
    public string GetDebugInfo()
    {
        debugStringBuilder.Clear();
        debugStringBuilder.AppendLine("=== 状态机调试信息 ===");
        debugStringBuilder.AppendLine($"当前状态: {currentStateType?.Name ?? "None"}");
        debugStringBuilder.AppendLine($"状态栈深度: {stateStack.Count}");
        debugStringBuilder.AppendLine($"总状态数: {stateNodes.Count}");
        debugStringBuilder.AppendLine($"黑板数据数: {blackboard.Count}");
        debugStringBuilder.AppendLine($"脏数据数: {dirtyBlackboardKeys.Count}");
        debugStringBuilder.AppendLine($"事件池大小: {eventPool.Count}");
        
        if (currentState != null && currentState is EnhancedBaseStateNode enhancedState)
        {
            debugStringBuilder.AppendLine();
            debugStringBuilder.AppendLine("当前状态详情:");
            debugStringBuilder.AppendLine(enhancedState.GetDebugInfo());
        }
        
        return debugStringBuilder.ToString();
    }
}

/// <summary>
/// 性能优化状态机工厂
/// </summary>
public static class PerformanceOptimizedStateMachineFactory
{
    /// <summary>
    /// 创建高性能游戏状态机
    /// </summary>
    public static PerformanceOptimizedStateMachine CreateHighPerformanceGameStateMachine(
        GameObject owner, 
        int expectedStateCount = 16,
        float updateInterval = 0.016f)
    {
        var stateMachine = new PerformanceOptimizedStateMachine(owner, expectedStateCount);
        stateMachine.SetUpdateInterval(updateInterval);
        stateMachine.SetPerformanceOptimization(true);
        
        Debug.Log($"[工厂] 创建高性能游戏状态机，预期状态数: {expectedStateCount}，更新间隔: {updateInterval:F3}秒");
        return stateMachine;
    }
    
    /// <summary>
    /// 创建高性能AI状态机
    /// </summary>
    public static PerformanceOptimizedStateMachine CreateHighPerformanceAIStateMachine(
        GameObject owner,
        float updateInterval = 0.1f) // AI通常不需要60FPS更新
    {
        var stateMachine = new PerformanceOptimizedStateMachine(owner, 8);
        stateMachine.SetUpdateInterval(updateInterval);
        stateMachine.SetPerformanceOptimization(true);
        
        Debug.Log($"[工厂] 创建高性能AI状态机，更新间隔: {updateInterval:F3}秒");
        return stateMachine;
    }
}