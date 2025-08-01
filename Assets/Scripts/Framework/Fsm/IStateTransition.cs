using System;

/// <summary>
/// 状态转换接口 - 定义状态转换的条件和回调
/// </summary>
public interface IStateTransition
{
    /// <summary>
    /// 检查是否可以从一个状态转换到另一个状态
    /// </summary>
    /// <param name="fromState">源状态类型</param>
    /// <param name="toState">目标状态类型</param>
    /// <returns>true表示可以转换，false表示不允许转换</returns>
    bool CanTransition(Type fromState, Type toState);
    
    /// <summary>
    /// 状态转换时的回调
    /// </summary>
    /// <param name="fromState">源状态类型</param>
    /// <param name="toState">目标状态类型</param>
    void OnTransition(Type fromState, Type toState);
}

/// <summary>
/// 默认状态转换实现 - 允许所有转换
/// </summary>
public class DefaultStateTransition : IStateTransition
{
    /// <summary>
    /// 默认允许所有状态转换
    /// </summary>
    public virtual bool CanTransition(Type fromState, Type toState)
    {
        return true;
    }
    
    /// <summary>
    /// 状态转换回调，可在子类中重写
    /// </summary>
    public virtual void OnTransition(Type fromState, Type toState)
    {
        // 默认实现为空，子类可以重写添加转换逻辑
    }
}