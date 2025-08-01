/// <summary>
/// 状态节点接口 - 定义状态的生命周期和控制能力
/// </summary>
public interface IStateNode
{
    /// <summary>
    /// 状态创建时调用，用于初始化状态
    /// </summary>
    /// <param name="machine">状态机引用</param>
    void OnCreate(StateMachine machine);
    
    /// <summary>
    /// 进入状态时调用
    /// </summary>
    void OnEnter();
    
    /// <summary>
    /// 状态更新时调用
    /// </summary>
    void OnUpdate();
    
    /// <summary>
    /// 退出状态时调用
    /// </summary>
    void OnExit();
    
    /// <summary>
    /// 检查状态是否可以被打断退出
    /// </summary>
    /// <returns>true表示可以退出，false表示不能被打断</returns>
    bool CanExit();
    
    /// <summary>
    /// 状态优先级，用于处理状态冲突
    /// 数值越大优先级越高
    /// </summary>
    int Priority { get; }
}