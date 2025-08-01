using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状态机扩展功能 - 为现有框架添加高级特性
/// </summary>
public static class StateMachineExtensions
{
    /// <summary>
    /// 状态历史记录
    /// </summary>
    public class StateHistory
    {
        private readonly Queue<StateRecord> _history = new Queue<StateRecord>(50);
        private readonly int _maxHistorySize;
        
        public StateHistory(int maxSize = 50)
        {
            _maxHistorySize = maxSize;
        }
        
        public void RecordStateChange(Type fromState, Type toState, float timestamp)
        {
            if (_history.Count >= _maxHistorySize)
            {
                _history.Dequeue();
            }
            
            _history.Enqueue(new StateRecord
            {
                FromState = fromState?.Name ?? "None",
                ToState = toState.Name,
                Timestamp = timestamp,
                Duration = 0f // 会在下次状态变化时计算
            });
        }
        
        public StateRecord[] GetHistory()
        {
            return _history.ToArray();
        }
        
        public void Clear()
        {
            _history.Clear();
        }
    }
    
    /// <summary>
    /// 状态记录
    /// </summary>
    [System.Serializable]
    public struct StateRecord
    {
        public string FromState;
        public string ToState;
        public float Timestamp;
        public float Duration;
    }
}

/// <summary>
/// 增强版状态节点接口 - 添加更多生命周期方法
/// </summary>
public interface IEnhancedStateNode : IStateNode
{
    /// <summary>
    /// 状态暂停时调用（PushState时）
    /// </summary>
    void OnPause();
    
    /// <summary>
    /// 状态恢复时调用（PopState时）
    /// </summary>
    void OnResume();
    
    /// <summary>
    /// 状态被强制中断时调用
    /// </summary>
    void OnInterrupted();
    
    /// <summary>
    /// 获取状态的调试信息
    /// </summary>
    string GetDebugInfo();
}

/// <summary>
/// 增强版基础状态节点
/// </summary>
public abstract class EnhancedBaseStateNode : BaseStateNode, IEnhancedStateNode
{
    protected float _enterTime;
    protected bool _isPaused;
    
    public override void OnEnter()
    {
        base.OnEnter();
        _enterTime = Time.time;
        _isPaused = false;
    }
    
    /// <summary>
    /// 状态暂停
    /// </summary>
    public virtual void OnPause()
    {
        _isPaused = true;
        Debug.Log($"[状态] {GetType().Name} 已暂停");
    }
    
    /// <summary>
    /// 状态恢复
    /// </summary>
    public virtual void OnResume()
    {
        _isPaused = false;
        Debug.Log($"[状态] {GetType().Name} 已恢复");
    }
    
    /// <summary>
    /// 状态被强制中断
    /// </summary>
    public virtual void OnInterrupted()
    {
        Debug.Log($"[状态] {GetType().Name} 被强制中断");
    }
    
    /// <summary>
    /// 获取状态运行时间
    /// </summary>
    protected float GetStateRunTime()
    {
        return Time.time - _enterTime;
    }
    
    /// <summary>
    /// 获取调试信息
    /// </summary>
    public virtual string GetDebugInfo()
    {
        return $"状态: {GetType().Name}\n" +
               $"运行时间: {GetStateRunTime():F2}秒\n" +
               $"是否暂停: {_isPaused}\n" +
               $"优先级: {Priority}";
    }
}

/// <summary>
/// 状态机配置类 - 用于初始化和配置状态机
/// </summary>
[System.Serializable]
public class StateMachineConfig
{
    [Header("基础配置")]
    public bool enableDebugLog = true;
    public bool enableStateHistory = true;
    public int maxHistorySize = 50;
    
    [Header("性能配置")]
    public int initialNodeCapacity = 32;
    public int initialBlackboardCapacity = 64;
    public int eventPoolSize = 16;
    
    [Header("安全配置")]
    public bool enableStateValidation = true;
    public bool enablePriorityCheck = true;
    public float maxStateTransitionTime = 0.1f; // 状态转换超时检测
}

/// <summary>
/// 状态机工厂类 - 简化状态机创建和配置
/// </summary>
public static class StateMachineFactory
{
    /// <summary>
    /// 创建标准游戏状态机
    /// </summary>
    public static StateMachine CreateGameStateMachine(GameObject owner, StateMachineConfig config = null)
    {
        config = config ?? new StateMachineConfig();
        
        var stateMachine = new StateMachine(owner);
        
        // 根据配置设置状态机
        if (config.enableStateHistory)
        {
            // 添加状态历史记录功能
            var history = new StateMachineExtensions.StateHistory(config.maxHistorySize);
            stateMachine.OnStateChanged += (from, to) => 
            {
                // 这里需要扩展StateMachine来支持Type参数的事件
                Debug.Log($"状态历史记录: {from} -> {to}");
            };
        }
        
        return stateMachine;
    }
    
    /// <summary>
    /// 创建UI状态机（针对UI界面管理优化）
    /// </summary>
    public static StateMachine CreateUIStateMachine(GameObject owner)
    {
        var config = new StateMachineConfig
        {
            enableDebugLog = false, // UI状态机通常不需要详细日志
            enableStateHistory = false,
            initialNodeCapacity = 16, // UI状态通常较少
            enablePriorityCheck = true // UI状态需要严格的优先级控制
        };
        
        return CreateGameStateMachine(owner, config);
    }
}

/// <summary>
/// 状态机性能监控器
/// </summary>
public class StateMachineProfiler
{
    private readonly Dictionary<string, float> _stateEnterTimes = new Dictionary<string, float>();
    private readonly Dictionary<string, int> _stateEnterCounts = new Dictionary<string, int>();
    private readonly Dictionary<string, float> _stateTotalTimes = new Dictionary<string, float>();
    
    public void OnStateEntered(string stateName)
    {
        _stateEnterTimes[stateName] = Time.time;
        _stateEnterCounts[stateName] = _stateEnterCounts.GetValueOrDefault(stateName, 0) + 1;
    }
    
    public void OnStateExited(string stateName)
    {
        if (_stateEnterTimes.TryGetValue(stateName, out float enterTime))
        {
            float duration = Time.time - enterTime;
            _stateTotalTimes[stateName] = _stateTotalTimes.GetValueOrDefault(stateName, 0f) + duration;
        }
    }
    
    public void PrintReport()
    {
        Debug.Log("=== 状态机性能报告 ===");
        foreach (var kvp in _stateEnterCounts)
        {
            string stateName = kvp.Key;
            int enterCount = kvp.Value;
            float totalTime = _stateTotalTimes.GetValueOrDefault(stateName, 0f);
            float avgTime = enterCount > 0 ? totalTime / enterCount : 0f;
            
            Debug.Log($"{stateName}: 进入{enterCount}次, 总时间{totalTime:F2}秒, 平均{avgTime:F2}秒");
        }
    }
}