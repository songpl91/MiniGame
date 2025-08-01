using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 优化版状态机 - 性能和内存管理改进建议
/// </summary>
public class StateMachineOptimized
{
    // 使用Type作为Key，避免字符串比较开销
    private readonly Dictionary<Type, IStateNode> _nodes = new Dictionary<Type, IStateNode>(32);
    private readonly Dictionary<string, object> _blackboard = new Dictionary<string, object>(64);
    private readonly Stack<IStateNode> _stateStack = new Stack<IStateNode>(8);
    
    private IStateNode _currentNode;
    private IStateNode _previousNode;
    private IStateTransition _stateTransition = new DefaultStateTransition();
    
    // 对象池，避免频繁GC
    private readonly Queue<StateChangeEvent> _eventPool = new Queue<StateChangeEvent>(16);
    
    /// <summary>
    /// 状态变化事件对象（可复用）
    /// </summary>
    public class StateChangeEvent
    {
        public Type FromState { get; set; }
        public Type ToState { get; set; }
        public float Timestamp { get; set; }
        
        public void Reset()
        {
            FromState = null;
            ToState = null;
            Timestamp = 0f;
        }
    }
    
    /// <summary>
    /// 事件委托定义
    /// </summary>
    public event Action<StateChangeEvent> OnStateChanged;
    
    /// <summary>
    /// 优化的状态切换方法
    /// </summary>
    public void ChangeState<T>() where T : IStateNode
    {
        var targetType = typeof(T);
        
        // 快速查找，避免字符串操作
        if (!_nodes.TryGetValue(targetType, out IStateNode targetNode))
        {
            Debug.LogError($"状态节点未注册: {targetType.Name}");
            return;
        }
        
        // 状态验证（复用现有逻辑）
        if (_currentNode != null && !_currentNode.CanExit())
        {
            return;
        }
        
        if (_currentNode != null && !_stateTransition.CanTransition(_currentNode.GetType(), targetType))
        {
            return;
        }
        
        // 执行状态切换
        PerformStateTransition(targetNode);
    }
    
    /// <summary>
    /// 执行状态转换的核心逻辑
    /// </summary>
    private void PerformStateTransition(IStateNode targetNode)
    {
        var fromType = _currentNode?.GetType();
        var toType = targetNode.GetType();
        
        // 退出当前状态
        if (_currentNode != null)
        {
            _currentNode.OnExit();
        }
        
        // 更新状态引用
        _previousNode = _currentNode;
        _currentNode = targetNode;
        
        // 进入新状态
        _currentNode.OnEnter();
        
        // 触发事件（使用对象池）
        TriggerStateChangeEvent(fromType, toType);
    }
    
    /// <summary>
    /// 使用对象池触发状态变化事件
    /// </summary>
    private void TriggerStateChangeEvent(Type fromType, Type toType)
    {
        StateChangeEvent evt;
        
        if (_eventPool.Count > 0)
        {
            evt = _eventPool.Dequeue();
            evt.Reset();
        }
        else
        {
            evt = new StateChangeEvent();
        }
        
        evt.FromState = fromType;
        evt.ToState = toType;
        evt.Timestamp = Time.time;
        
        OnStateChanged?.Invoke(evt);
        
        // 事件使用完毕后回收
        _eventPool.Enqueue(evt);
    }
    
    /// <summary>
    /// 销毁状态机，清理资源
    /// </summary>
    public void Dispose()
    {
        // 退出当前状态
        if (_currentNode != null)
        {
            _currentNode.OnExit();
            _currentNode = null;
        }
        
        // 清理所有状态节点
        foreach (var node in _nodes.Values)
        {
            if (node is IDisposable disposableNode)
            {
                disposableNode.Dispose();
            }
        }
        
        // 清理集合
        _nodes.Clear();
        _blackboard.Clear();
        _stateStack.Clear();
        _eventPool.Clear();
        
        // 清理事件订阅
        OnStateChanged = null;
        
        Debug.Log("[状态机] 资源清理完成");
    }
    
    /// <summary>
    /// 获取状态机运行统计信息
    /// </summary>
    public StateMachineStats GetStats()
    {
        return new StateMachineStats
        {
            RegisteredStatesCount = _nodes.Count,
            BlackboardItemsCount = _blackboard.Count,
            StateStackDepth = _stateStack.Count,
            CurrentState = _currentNode?.GetType().Name ?? "None",
            EventPoolSize = _eventPool.Count
        };
    }
}

/// <summary>
/// 状态机统计信息
/// </summary>
public struct StateMachineStats
{
    public int RegisteredStatesCount;
    public int BlackboardItemsCount;
    public int StateStackDepth;
    public string CurrentState;
    public int EventPoolSize;
}