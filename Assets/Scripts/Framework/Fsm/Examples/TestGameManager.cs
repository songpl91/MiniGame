using UnityEngine;
using GameStateExamples;

/// <summary>
/// 游戏管理器示例 - 展示如何在实际项目中使用增强后的状态机
/// </summary>
public class TestGameManager : MonoBehaviour
{
    [Header("状态机配置")]
    [SerializeField] private bool enableDebugLog = true;
    
    private StateMachine stateMachine;
    
    void Start()
    {
        InitializeStateMachine();
    }
    
    void Update()
    {
        // 更新状态机
        stateMachine?.Update();
        
        // 调试信息显示
        if (enableDebugLog && Input.GetKeyDown(KeyCode.F1))
        {
            ShowDebugInfo();
        }
    }
    
    /// <summary>
    /// 初始化状态机
    /// </summary>
    private void InitializeStateMachine()
    {
        // 创建状态机实例
        stateMachine = new StateMachine(this);
        
        // 设置自定义状态转换管理器
        stateMachine.SetStateTransition(new GameStateTransition());
        
        // 注册状态变化事件
        stateMachine.OnStateChanged += OnStateChanged;
        stateMachine.OnStateEntered += OnStateEntered;
        stateMachine.OnStateExited += OnStateExited;
        stateMachine.OnBlackboardValueChanged += OnBlackboardValueChanged;
        
        // 添加所有状态节点
        stateMachine.AddNode<MainMenuState>();
        stateMachine.AddNode<GamePlayState>();
        stateMachine.AddNode<PauseState>();
        stateMachine.AddNode<GameOverState>();
        
        // 启动状态机，从主菜单开始
        stateMachine.Run<MainMenuState>();
        
        Debug.Log("[游戏管理器] 状态机初始化完成");
    }
    
    /// <summary>
    /// 状态变化事件处理
    /// </summary>
    /// <param name="fromState">源状态</param>
    /// <param name="toState">目标状态</param>
    private void OnStateChanged(string fromState, string toState)
    {
        Debug.Log($"[事件] 状态变化: {fromState} -> {toState}");
        
        // 根据状态变化执行相应的UI更新
        UpdateUIForState(toState);
    }
    
    /// <summary>
    /// 状态进入事件处理
    /// </summary>
    /// <param name="stateName">状态名称</param>
    private void OnStateEntered(string stateName)
    {
        Debug.Log($"[事件] 进入状态: {stateName}");
        
        // 可以在这里添加状态进入时的音效、动画等
        PlayStateEnterEffect(stateName);
    }
    
    /// <summary>
    /// 状态退出事件处理
    /// </summary>
    /// <param name="stateName">状态名称</param>
    private void OnStateExited(string stateName)
    {
        Debug.Log($"[事件] 退出状态: {stateName}");
    }
    
    /// <summary>
    /// 黑板数据变化事件处理
    /// </summary>
    /// <param name="key">键值</param>
    /// <param name="oldValue">旧值</param>
    /// <param name="newValue">新值</param>
    private void OnBlackboardValueChanged(string key, object oldValue, object newValue)
    {
        Debug.Log($"[黑板] {key}: {oldValue} -> {newValue}");
        
        // 根据数据变化更新UI
        UpdateUIForBlackboardChange(key, newValue);
    }
    
    /// <summary>
    /// 根据状态更新UI
    /// </summary>
    /// <param name="stateName">状态名称</param>
    private void UpdateUIForState(string stateName)
    {
        // 这里可以根据状态名称更新相应的UI
        switch (stateName)
        {
            case "MainMenuState":
                // 显示主菜单UI
                break;
            case "GamePlayState":
                // 显示游戏UI
                break;
            case "PauseState":
                // 显示暂停UI
                break;
            case "GameOverState":
                // 显示游戏结束UI
                break;
        }
    }
    
    /// <summary>
    /// 播放状态进入特效
    /// </summary>
    /// <param name="stateName">状态名称</param>
    private void PlayStateEnterEffect(string stateName)
    {
        // 这里可以播放状态切换的音效或动画
        // 例如：AudioManager.PlaySound("StateTransition");
    }
    
    /// <summary>
    /// 根据黑板数据变化更新UI
    /// </summary>
    /// <param name="key">数据键值</param>
    /// <param name="newValue">新值</param>
    private void UpdateUIForBlackboardChange(string key, object newValue)
    {
        switch (key)
        {
            case "GameTime":
                // 更新游戏时间显示
                if (newValue is float gameTime)
                {
                    // UI更新逻辑
                }
                break;
            case "FinalScore":
                // 更新最终得分显示
                if (newValue is int score)
                {
                    // UI更新逻辑
                }
                break;
        }
    }
    
    /// <summary>
    /// 显示调试信息
    /// </summary>
    private void ShowDebugInfo()
    {
        if (stateMachine == null) return;
        
        Debug.Log("=== 状态机调试信息 ===");
        Debug.Log($"当前状态: {stateMachine.CurrentNode}");
        Debug.Log($"上一个状态: {stateMachine.PreviousNode}");
        Debug.Log($"状态栈深度: {stateMachine.GetStateStackDepth()}");
        
        // 显示黑板数据
        Debug.Log("黑板数据:");
        if (stateMachine.HasBlackboardValue("GameTime"))
        {
            float gameTime = stateMachine.GetBlackboardValue<float>("GameTime");
            Debug.Log($"  GameTime: {gameTime:F2}");
        }
        if (stateMachine.HasBlackboardValue("FinalScore"))
        {
            int score = stateMachine.GetBlackboardValue<int>("FinalScore");
            Debug.Log($"  FinalScore: {score}");
        }
    }
    
    /// <summary>
    /// 公共接口：强制切换到指定状态（用于外部调用）
    /// </summary>
    /// <typeparam name="T">目标状态类型</typeparam>
    public void ForceChangeState<T>() where T : IStateNode
    {
        stateMachine?.ChangeState<T>();
    }
    
    /// <summary>
    /// 公共接口：暂停当前状态
    /// </summary>
    /// <typeparam name="T">暂停后要切换到的状态类型</typeparam>
    public void PauseCurrentState<T>() where T : IStateNode
    {
        stateMachine?.PushState<T>();
    }
    
    /// <summary>
    /// 公共接口：恢复被暂停的状态
    /// </summary>
    public void ResumeState()
    {
        stateMachine?.PopState();
    }
    
    void OnDestroy()
    {
        // 清理事件订阅
        if (stateMachine != null)
        {
            stateMachine.OnStateChanged -= OnStateChanged;
            stateMachine.OnStateEntered -= OnStateEntered;
            stateMachine.OnStateExited -= OnStateExited;
            stateMachine.OnBlackboardValueChanged -= OnBlackboardValueChanged;
        }
    }
}