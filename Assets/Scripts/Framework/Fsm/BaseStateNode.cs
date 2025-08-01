using UnityEngine;

/// <summary>
/// 基础状态节点抽象类 - 提供状态节点的默认实现
/// 继承此类可以减少重复代码，只需重写需要的方法
/// </summary>
public abstract class BaseStateNode : IStateNode
{
    /// <summary>
    /// 状态机引用
    /// </summary>
    protected StateMachine stateMachine;
    
    /// <summary>
    /// 状态优先级，默认为0，子类可以重写
    /// </summary>
    public virtual int Priority => 0;
    
    /// <summary>
    /// 状态创建时调用
    /// </summary>
    /// <param name="machine">状态机引用</param>
    public virtual void OnCreate(StateMachine machine)
    {
        stateMachine = machine;
        Debug.Log($"[状态机] 创建状态: {GetType().Name}");
    }
    
    /// <summary>
    /// 进入状态时调用
    /// </summary>
    public virtual void OnEnter()
    {
        Debug.Log($"[状态机] 进入状态: {GetType().Name}");
    }
    
    /// <summary>
    /// 状态更新时调用
    /// </summary>
    public virtual void OnUpdate()
    {
        // 默认空实现，子类根据需要重写
    }
    
    /// <summary>
    /// 退出状态时调用
    /// </summary>
    public virtual void OnExit()
    {
        Debug.Log($"[状态机] 退出状态: {GetType().Name}");
    }
    
    /// <summary>
    /// 检查状态是否可以被打断，默认允许
    /// </summary>
    /// <returns>默认返回true，子类可以重写实现特殊逻辑</returns>
    public virtual bool CanExit()
    {
        return true;
    }
    
    /// <summary>
    /// 获取黑板数据的泛型方法
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="key">键值</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>黑板中的数据或默认值</returns>
    protected T GetBlackboardValue<T>(string key, T defaultValue = default(T))
    {
        var value = stateMachine.GetBlackboardValue(key);
        if (value != null && value is T)
        {
            return (T)value;
        }
        return defaultValue;
    }
    
    /// <summary>
    /// 设置黑板数据
    /// </summary>
    /// <param name="key">键值</param>
    /// <param name="value">数据</param>
    protected void SetBlackboardValue(string key, object value)
    {
        stateMachine.SetBlackboardValue(key, value);
    }
    
    /// <summary>
    /// 切换到指定状态
    /// </summary>
    /// <typeparam name="T">目标状态类型</typeparam>
    protected void ChangeState<T>() where T : IStateNode
    {
        stateMachine.ChangeState<T>();
    }
}