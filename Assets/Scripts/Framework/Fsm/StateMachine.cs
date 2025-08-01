using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 增强版状态机 - 支持事件系统、状态转换验证和改进的黑板系统
/// </summary>
public class StateMachine
{
    private readonly Dictionary<string, System.Object> _blackboard = new Dictionary<string, object>(100);
    private readonly Dictionary<string, IStateNode> _nodes = new Dictionary<string, IStateNode>(100);
    private readonly Stack<IStateNode> _stateStack = new Stack<IStateNode>(); // 状态栈，支持状态暂停/恢复
    private IStateNode _curNode;
    private IStateNode _preNode;
    private IStateTransition _stateTransition = new DefaultStateTransition(); // 状态转换管理器

    /// <summary>
    /// 状态变化事件 - 参数：(fromState, toState)
    /// </summary>
    public event System.Action<string, string> OnStateChanged;

    /// <summary>
    /// 状态进入事件 - 参数：(stateName)
    /// </summary>
    public event System.Action<string> OnStateEntered;

    /// <summary>
    /// 状态退出事件 - 参数：(stateName)
    /// </summary>
    public event System.Action<string> OnStateExited;

    /// <summary>
    /// 黑板数据变化事件 - 参数：(key, oldValue, newValue)
    /// </summary>
    public event System.Action<string, object, object> OnBlackboardValueChanged;

    /// <summary>
    /// 状态机持有者
    /// </summary>
    public System.Object Owner { private set; get; }

    /// <summary>
    /// 当前运行的节点名称
    /// </summary>
    public string CurrentNode
    {
        get { return _curNode != null ? _curNode.GetType().FullName : string.Empty; }
    }

    /// <summary>
    /// 之前运行的节点名称
    /// </summary>
    public string PreviousNode
    {
        get { return _preNode != null ? _preNode.GetType().FullName : string.Empty; }
    }


    private StateMachine() { }
    public StateMachine(System.Object owner)
    {
        Owner = owner;
    }

    /// <summary>
    /// 设置状态转换管理器
    /// </summary>
    /// <param name="stateTransition">状态转换管理器实例</param>
    public void SetStateTransition(IStateTransition stateTransition)
    {
        _stateTransition = stateTransition ?? new DefaultStateTransition();
    }

    /// <summary>
    /// 更新状态机
    /// </summary>
    public void Update()
    {
        if (_curNode != null)
            _curNode.OnUpdate();
    }

    /// <summary>
    /// 启动状态机
    /// </summary>
    public void Run<TNode>() where TNode : IStateNode
    {
        var nodeType = typeof(TNode);
        var nodeName = nodeType.FullName;
        Run(nodeName);
    }
    public void Run(Type entryNode)
    {
        var nodeName = entryNode.FullName;
        Run(nodeName);
    }
    public void Run(string entryNode)
    {
        _curNode = TryGetNode(entryNode);
        _preNode = _curNode;

        if (_curNode == null)
            throw new Exception($"Not found entry node: {entryNode}");

        _curNode.OnEnter();
    }

    /// <summary>
    /// 加入一个节点
    /// </summary>
    public void AddNode<TNode>() where TNode : IStateNode
    {
        var nodeType = typeof(TNode);
        var stateNode = Activator.CreateInstance(nodeType) as IStateNode;
        AddNode(stateNode);
    }
    public void AddNode(IStateNode stateNode)
    {
        if (stateNode == null)
            throw new ArgumentNullException();

        var nodeType = stateNode.GetType();
        var nodeName = nodeType.FullName;

        if (_nodes.ContainsKey(nodeName) == false)
        {
            stateNode.OnCreate(this);
            _nodes.Add(nodeName, stateNode);
        }
        else
        {
            Debug.LogError($"State node already existed : {nodeName}");
        }
    }

    /// <summary>
    /// 转换状态节点
    /// </summary>
    public void ChangeState<TNode>() where TNode : IStateNode
    {
        var nodeType = typeof(TNode);
        var nodeName = nodeType.FullName;
        ChangeState(nodeName);
    }
    public void ChangeState(Type nodeType)
    {
        var nodeName = nodeType.FullName;
        ChangeState(nodeName);
    }
    public void ChangeState(string nodeName)
    {
        if (string.IsNullOrEmpty(nodeName))
            throw new Exception("状态名称不能为空");

        IStateNode node = TryGetNode(nodeName);
        if (node == null)
        {
            Debug.LogError($"找不到状态节点: {nodeName}");
            return;
        }

        // 检查当前状态是否可以退出
        if (_curNode != null && !_curNode.CanExit())
        {
            Debug.LogWarning($"当前状态 {_curNode.GetType().Name} 不允许被打断");
            return;
        }

        // 检查状态转换是否被允许
        if (_curNode != null && !_stateTransition.CanTransition(_curNode.GetType(), node.GetType()))
        {
            Debug.LogWarning($"不允许从状态 {_curNode.GetType().Name} 转换到 {node.GetType().Name}");
            return;
        }

        // 检查优先级（如果目标状态优先级更低，则不允许转换）
        if (_curNode != null && node.Priority < _curNode.Priority)
        {
            Debug.LogWarning($"目标状态 {node.GetType().Name} 优先级({node.Priority}) 低于当前状态 {_curNode.GetType().Name} 优先级({_curNode.Priority})");
            return;
        }

        string fromStateName = _curNode?.GetType().Name ?? "None";
        string toStateName = node.GetType().Name;

        Debug.Log($"[状态机] 状态转换: {fromStateName} --> {toStateName}");

        // 执行状态转换
        _preNode = _curNode;

        // 触发状态转换回调
        if (_curNode != null)
        {
            _stateTransition.OnTransition(_curNode.GetType(), node.GetType());
            _curNode.OnExit();
            OnStateExited?.Invoke(fromStateName);
        }

        _curNode = node;
        _curNode.OnEnter();

        // 触发事件
        OnStateEntered?.Invoke(toStateName);
        OnStateChanged?.Invoke(fromStateName, toStateName);
    }

    /// <summary>
    /// 设置黑板数据
    /// </summary>
    /// <param name="key">键值</param>
    /// <param name="value">数据值</param>
    public void SetBlackboardValue(string key, System.Object value)
    {
        System.Object oldValue = null;
        bool hasOldValue = _blackboard.TryGetValue(key, out oldValue);

        if (!hasOldValue)
        {
            _blackboard.Add(key, value);
        }
        else
        {
            _blackboard[key] = value;
        }

        // 触发数据变化事件
        OnBlackboardValueChanged?.Invoke(key, oldValue, value);
    }

    /// <summary>
    /// 获取黑板数据
    /// </summary>
    /// <param name="key">键值</param>
    /// <returns>数据值，如果不存在返回null</returns>
    public System.Object GetBlackboardValue(string key)
    {
        if (_blackboard.TryGetValue(key, out System.Object value))
        {
            return value;
        }
        else
        {
            Debug.LogWarning($"黑板中未找到数据: {key}");
            return null;
        }
    }

    /// <summary>
    /// 获取黑板数据的泛型方法
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="key">键值</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>黑板中的数据或默认值</returns>
    public T GetBlackboardValue<T>(string key, T defaultValue = default(T))
    {
        if (_blackboard.TryGetValue(key, out System.Object value) && value is T)
        {
            return (T)value;
        }
        return defaultValue;
    }

    /// <summary>
    /// 检查黑板是否包含指定键值
    /// </summary>
    /// <param name="key">键值</param>
    /// <returns>true表示包含，false表示不包含</returns>
    public bool HasBlackboardValue(string key)
    {
        return _blackboard.ContainsKey(key);
    }

    /// <summary>
    /// 移除黑板数据
    /// </summary>
    /// <param name="key">键值</param>
    /// <returns>true表示移除成功，false表示键值不存在</returns>
    public bool RemoveBlackboardValue(string key)
    {
        if (_blackboard.TryGetValue(key, out System.Object oldValue))
        {
            _blackboard.Remove(key);
            OnBlackboardValueChanged?.Invoke(key, oldValue, null);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 清空黑板数据
    /// </summary>
    public void ClearBlackboard()
    {
        _blackboard.Clear();
        Debug.Log("[状态机] 黑板数据已清空");
    }

    /// <summary>
    /// 暂停当前状态并切换到新状态（将当前状态压入栈中）
    /// 适用于暂停游戏、打开菜单等场景
    /// </summary>
    /// <typeparam name="TNode">目标状态类型</typeparam>
    public void PushState<TNode>() where TNode : IStateNode
    {
        var nodeType = typeof(TNode);
        var nodeName = nodeType.FullName;
        PushState(nodeName);
    }

    /// <summary>
    /// 暂停当前状态并切换到新状态
    /// </summary>
    /// <param name="nodeName">目标状态名称</param>
    public void PushState(string nodeName)
    {
        if (_curNode == null)
        {
            Debug.LogWarning("[状态机] 当前没有活动状态，无法执行Push操作");
            return;
        }

        IStateNode node = TryGetNode(nodeName);
        if (node == null)
        {
            Debug.LogError($"找不到状态节点: {nodeName}");
            return;
        }

        Debug.Log($"[状态机] 暂停状态: {_curNode.GetType().Name}, 切换到: {node.GetType().Name}");

        // 将当前状态压入栈中（不调用OnExit）
        _stateStack.Push(_curNode);

        // 切换到新状态
        _preNode = _curNode;
        _curNode = node;
        _curNode.OnEnter();

        OnStateEntered?.Invoke(node.GetType().Name);
    }

    /// <summary>
    /// 恢复上一个被暂停的状态
    /// 适用于关闭菜单、恢复游戏等场景
    /// </summary>
    /// <returns>true表示恢复成功，false表示栈为空</returns>
    public bool PopState()
    {
        if (_stateStack.Count == 0)
        {
            Debug.LogWarning("[状态机] 状态栈为空，无法执行Pop操作");
            return false;
        }

        // 退出当前状态
        string currentStateName = _curNode?.GetType().Name ?? "None";
        if (_curNode != null)
        {
            _curNode.OnExit();
            OnStateExited?.Invoke(currentStateName);
        }

        // 恢复栈顶状态
        _preNode = _curNode;
        _curNode = _stateStack.Pop();

        string restoredStateName = _curNode.GetType().Name;
        Debug.Log($"[状态机] 恢复状态: {restoredStateName}");

        // 注意：恢复状态时不调用OnEnter，因为状态只是被暂停了
        OnStateChanged?.Invoke(currentStateName, restoredStateName);

        return true;
    }

    /// <summary>
    /// 清空状态栈
    /// </summary>
    public void ClearStateStack()
    {
        _stateStack.Clear();
        Debug.Log("[状态机] 状态栈已清空");
    }

    /// <summary>
    /// 获取状态栈深度
    /// </summary>
    /// <returns>栈中状态的数量</returns>
    public int GetStateStackDepth()
    {
        return _stateStack.Count;
    }

    private IStateNode TryGetNode(string nodeName)
    {
        _nodes.TryGetValue(nodeName, out IStateNode result);
        return result;
    }
}
