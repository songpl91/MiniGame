using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.StateMachineUI.Core
{
    /// <summary>
    /// UI状态机
    /// 管理UI状态的转换、生命周期和状态栈
    /// </summary>
    public class UIStateMachine : MonoBehaviour
    {
        #region 字段和属性
        
        /// <summary>
        /// 当前活跃状态列表（按优先级排序）
        /// </summary>
        private List<IUIState> activeStates = new List<IUIState>();
        
        /// <summary>
        /// 状态历史栈
        /// </summary>
        private Stack<IUIState> stateHistory = new Stack<IUIState>();
        
        /// <summary>
        /// 注册的状态字典
        /// </summary>
        private Dictionary<string, IUIState> registeredStates = new Dictionary<string, IUIState>();
        
        /// <summary>
        /// 状态转换规则
        /// </summary>
        private Dictionary<string, List<string>> transitionRules = new Dictionary<string, List<string>>();
        
        /// <summary>
        /// 是否启用状态历史
        /// </summary>
        [SerializeField] private bool enableStateHistory = true;
        
        /// <summary>
        /// 最大状态历史数量
        /// </summary>
        [SerializeField] private int maxHistoryCount = 10;
        
        /// <summary>
        /// 是否启用调试日志
        /// </summary>
        [SerializeField] private bool enableDebugLog = true;
        
        /// <summary>
        /// 当前主状态（优先级最高的Normal或Exclusive状态）
        /// </summary>
        public IUIState CurrentMainState
        {
            get
            {
                foreach (var state in activeStates)
                {
                    if (state.StateType == UIStateType.Normal || state.StateType == UIStateType.Exclusive)
                        return state;
                }
                return null;
            }
        }
        
        /// <summary>
        /// 当前活跃状态数量
        /// </summary>
        public int ActiveStateCount => activeStates.Count;
        
        /// <summary>
        /// 是否有活跃状态
        /// </summary>
        public bool HasActiveStates => activeStates.Count > 0;
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 状态改变事件
        /// </summary>
        public event Action<IUIState, IUIState> OnStateChanged;
        
        /// <summary>
        /// 状态添加事件
        /// </summary>
        public event Action<IUIState> OnStateAdded;
        
        /// <summary>
        /// 状态移除事件
        /// </summary>
        public event Action<IUIState> OnStateRemoved;
        
        /// <summary>
        /// 状态机重置事件
        /// </summary>
        public event Action OnStateMachineReset;
        
        #endregion
        
        #region Unity生命周期
        
        private void Update()
        {
            // 更新所有活跃状态
            float deltaTime = Time.deltaTime;
            for (int i = activeStates.Count - 1; i >= 0; i--)
            {
                if (i < activeStates.Count) // 防止在更新过程中状态被移除
                {
                    activeStates[i].OnUpdate(deltaTime);
                }
            }
            
            // 处理输入事件
            HandleInputEvents();
        }
        
        #endregion
        
        #region 状态注册和管理
        
        /// <summary>
        /// 注册状态
        /// </summary>
        /// <param name="state">要注册的状态</param>
        public void RegisterState(IUIState state)
        {
            if (state == null)
            {
                Debug.LogError("[状态机UI] 尝试注册空状态");
                return;
            }
            
            if (registeredStates.ContainsKey(state.StateName))
            {
                Debug.LogWarning($"[状态机UI] 状态 {state.StateName} 已存在，将被覆盖");
            }
            
            registeredStates[state.StateName] = state;
            
            if (enableDebugLog)
                Debug.Log($"[状态机UI] 注册状态: {state.StateName}");
        }
        
        /// <summary>
        /// 注销状态
        /// </summary>
        /// <param name="stateName">状态名称</param>
        public void UnregisterState(string stateName)
        {
            if (registeredStates.ContainsKey(stateName))
            {
                var state = registeredStates[stateName];
                
                // 如果状态正在活跃，先移除它
                if (activeStates.Contains(state))
                {
                    RemoveState(stateName);
                }
                
                registeredStates.Remove(stateName);
                
                if (enableDebugLog)
                    Debug.Log($"[状态机UI] 注销状态: {stateName}");
            }
        }
        
        /// <summary>
        /// 获取注册的状态
        /// </summary>
        /// <param name="stateName">状态名称</param>
        /// <returns>状态实例</returns>
        public IUIState GetRegisteredState(string stateName)
        {
            registeredStates.TryGetValue(stateName, out IUIState state);
            return state;
        }
        
        /// <summary>
        /// 检查状态是否已注册
        /// </summary>
        /// <param name="stateName">状态名称</param>
        /// <returns>是否已注册</returns>
        public bool IsStateRegistered(string stateName)
        {
            return registeredStates.ContainsKey(stateName);
        }
        
        #endregion
        
        #region 状态转换
        
        /// <summary>
        /// 转换到指定状态
        /// </summary>
        /// <param name="stateName">目标状态名称</param>
        /// <param name="data">传递的数据</param>
        /// <param name="forceTransition">是否强制转换</param>
        /// <returns>是否转换成功</returns>
        public bool TransitionToState(string stateName, object data = null, bool forceTransition = false)
        {
            if (!registeredStates.TryGetValue(stateName, out IUIState targetState))
            {
                Debug.LogError($"[状态机UI] 状态 {stateName} 未注册");
                return false;
            }
            
            return TransitionToState(targetState, data, forceTransition);
        }
        
        /// <summary>
        /// 转换到指定状态
        /// </summary>
        /// <param name="targetState">目标状态</param>
        /// <param name="data">传递的数据</param>
        /// <param name="forceTransition">是否强制转换</param>
        /// <returns>是否转换成功</returns>
        public bool TransitionToState(IUIState targetState, object data = null, bool forceTransition = false)
        {
            if (targetState == null)
            {
                Debug.LogError("[状态机UI] 目标状态为空");
                return false;
            }
            
            // 检查状态是否已经活跃
            if (activeStates.Contains(targetState))
            {
                if (enableDebugLog)
                    Debug.Log($"[状态机UI] 状态 {targetState.StateName} 已经活跃");
                return true;
            }
            
            // 检查转换规则
            if (!forceTransition && !CanTransitionToState(targetState))
            {
                if (enableDebugLog)
                    Debug.Log($"[状态机UI] 无法转换到状态 {targetState.StateName}");
                return false;
            }
            
            IUIState previousMainState = CurrentMainState;
            
            // 根据状态类型处理转换
            switch (targetState.StateType)
            {
                case UIStateType.Exclusive:
                    // 独占状态：关闭所有其他状态
                    ClearAllStates();
                    AddStateToActive(targetState, previousMainState, data);
                    break;
                
                case UIStateType.System:
                    // 系统状态：暂停其他状态，添加到最高优先级
                    PauseAllStates();
                    AddStateToActive(targetState, previousMainState, data);
                    break;
                
                case UIStateType.Normal:
                    // 普通状态：替换当前主状态
                    RemoveStatesOfType(UIStateType.Normal, UIStateType.Exclusive);
                    AddStateToActive(targetState, previousMainState, data);
                    break;
                
                case UIStateType.Overlay:
                    // 叠加状态：直接添加
                    AddStateToActive(targetState, previousMainState, data);
                    break;
                
                case UIStateType.Temporary:
                    // 临时状态：添加到顶层
                    AddStateToActive(targetState, previousMainState, data);
                    break;
            }
            
            // 触发状态改变事件
            OnStateChanged?.Invoke(targetState, previousMainState);
            
            return true;
        }
        
        /// <summary>
        /// 添加状态到活跃列表
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="previousState">前一个状态</param>
        /// <param name="data">数据</param>
        private void AddStateToActive(IUIState state, IUIState previousState, object data)
        {
            // 添加到历史记录
            if (enableStateHistory && previousState != null)
            {
                stateHistory.Push(previousState);
                
                // 限制历史记录数量
                while (stateHistory.Count > maxHistoryCount)
                {
                    var oldStates = new IUIState[stateHistory.Count];
                    stateHistory.CopyTo(oldStates, 0);
                    stateHistory.Clear();
                    
                    for (int i = 1; i < oldStates.Length; i++)
                    {
                        stateHistory.Push(oldStates[i]);
                    }
                }
            }
            
            // 添加到活跃列表
            activeStates.Add(state);
            
            // 按优先级排序
            activeStates.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            
            // 进入状态
            state.OnEnter(previousState, data);
            
            // 触发状态添加事件
            OnStateAdded?.Invoke(state);
            
            if (enableDebugLog)
                Debug.Log($"[状态机UI] 添加活跃状态: {state.StateName}");
        }
        
        #endregion
        
        #region 状态移除
        
        /// <summary>
        /// 移除指定状态
        /// </summary>
        /// <param name="stateName">状态名称</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveState(string stateName)
        {
            var state = activeStates.Find(s => s.StateName == stateName);
            if (state != null)
            {
                return RemoveState(state);
            }
            return false;
        }
        
        /// <summary>
        /// 移除指定状态
        /// </summary>
        /// <param name="state">状态实例</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveState(IUIState state)
        {
            if (state == null || !activeStates.Contains(state))
                return false;
            
            // 退出状态
            state.OnExit(CurrentMainState);
            
            // 从活跃列表移除
            activeStates.Remove(state);
            
            // 触发状态移除事件
            OnStateRemoved?.Invoke(state);
            
            if (enableDebugLog)
                Debug.Log($"[状态机UI] 移除活跃状态: {state.StateName}");
            
            return true;
        }
        
        /// <summary>
        /// 移除指定类型的状态
        /// </summary>
        /// <param name="stateTypes">状态类型</param>
        private void RemoveStatesOfType(params UIStateType[] stateTypes)
        {
            for (int i = activeStates.Count - 1; i >= 0; i--)
            {
                var state = activeStates[i];
                foreach (var stateType in stateTypes)
                {
                    if (state.StateType == stateType)
                    {
                        RemoveState(state);
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// 清空所有活跃状态
        /// </summary>
        public void ClearAllStates()
        {
            for (int i = activeStates.Count - 1; i >= 0; i--)
            {
                RemoveState(activeStates[i]);
            }
        }
        
        #endregion
        
        #region 状态查询
        
        /// <summary>
        /// 检查状态是否活跃
        /// </summary>
        /// <param name="stateName">状态名称</param>
        /// <returns>是否活跃</returns>
        public bool IsStateActive(string stateName)
        {
            return activeStates.Find(s => s.StateName == stateName) != null;
        }
        
        /// <summary>
        /// 获取活跃状态
        /// </summary>
        /// <param name="stateName">状态名称</param>
        /// <returns>状态实例</returns>
        public IUIState GetActiveState(string stateName)
        {
            return activeStates.Find(s => s.StateName == stateName);
        }
        
        /// <summary>
        /// 获取所有活跃状态
        /// </summary>
        /// <returns>活跃状态列表</returns>
        public List<IUIState> GetActiveStates()
        {
            return new List<IUIState>(activeStates);
        }
        
        /// <summary>
        /// 获取指定类型的活跃状态
        /// </summary>
        /// <param name="stateType">状态类型</param>
        /// <returns>状态列表</returns>
        public List<IUIState> GetActiveStatesByType(UIStateType stateType)
        {
            var result = new List<IUIState>();
            foreach (var state in activeStates)
            {
                if (state.StateType == stateType)
                    result.Add(state);
            }
            return result;
        }
        
        #endregion
        
        #region 状态历史和导航
        
        /// <summary>
        /// 返回到上一个状态
        /// </summary>
        /// <returns>是否返回成功</returns>
        public bool GoBack()
        {
            if (!enableStateHistory || stateHistory.Count == 0)
                return false;
            
            var previousState = stateHistory.Pop();
            return TransitionToState(previousState);
        }
        
        /// <summary>
        /// 清空状态历史
        /// </summary>
        public void ClearHistory()
        {
            stateHistory.Clear();
        }
        
        /// <summary>
        /// 获取状态历史数量
        /// </summary>
        /// <returns>历史数量</returns>
        public int GetHistoryCount()
        {
            return stateHistory.Count;
        }
        
        #endregion
        
        #region 状态转换规则
        
        /// <summary>
        /// 添加状态转换规则
        /// </summary>
        /// <param name="fromState">源状态</param>
        /// <param name="toState">目标状态</param>
        public void AddTransitionRule(string fromState, string toState)
        {
            if (!transitionRules.ContainsKey(fromState))
            {
                transitionRules[fromState] = new List<string>();
            }
            
            if (!transitionRules[fromState].Contains(toState))
            {
                transitionRules[fromState].Add(toState);
            }
        }
        
        /// <summary>
        /// 移除状态转换规则
        /// </summary>
        /// <param name="fromState">源状态</param>
        /// <param name="toState">目标状态</param>
        public void RemoveTransitionRule(string fromState, string toState)
        {
            if (transitionRules.ContainsKey(fromState))
            {
                transitionRules[fromState].Remove(toState);
            }
        }
        
        /// <summary>
        /// 检查是否可以转换到指定状态
        /// </summary>
        /// <param name="targetState">目标状态</param>
        /// <returns>是否可以转换</returns>
        private bool CanTransitionToState(IUIState targetState)
        {
            // 系统状态和独占状态总是可以转换
            if (targetState.StateType == UIStateType.System || targetState.StateType == UIStateType.Exclusive)
                return true;
            
            // 检查当前活跃状态是否允许转换
            foreach (var activeState in activeStates)
            {
                if (!activeState.CanTransitionTo(targetState))
                    return false;
            }
            
            // 检查转换规则
            var currentMainState = CurrentMainState;
            if (currentMainState != null && transitionRules.ContainsKey(currentMainState.StateName))
            {
                return transitionRules[currentMainState.StateName].Contains(targetState.StateName);
            }
            
            return true;
        }
        
        #endregion
        
        #region 输入处理
        
        /// <summary>
        /// 处理输入事件
        /// </summary>
        private void HandleInputEvents()
        {
            // 检查ESC键
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var inputEvent = new UIInputEvent(UIInputEventType.KeyDown)
                {
                    KeyCode = KeyCode.Escape
                };
                
                HandleInputEvent(inputEvent);
            }
            
            // 检查鼠标点击
            if (Input.GetMouseButtonDown(0))
            {
                var inputEvent = new UIInputEvent(UIInputEventType.MouseClick)
                {
                    MousePosition = Input.mousePosition,
                    MouseButton = 0
                };
                
                HandleInputEvent(inputEvent);
            }
        }
        
        /// <summary>
        /// 处理输入事件
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        public void HandleInputEvent(UIInputEvent inputEvent)
        {
            // 按优先级顺序处理输入事件
            foreach (var state in activeStates)
            {
                if (state.HandleInput(inputEvent))
                {
                    inputEvent.IsHandled = true;
                    break;
                }
            }
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 暂停所有状态
        /// </summary>
        private void PauseAllStates()
        {
            foreach (var state in activeStates)
            {
                state.OnPause();
            }
        }
        
        /// <summary>
        /// 恢复所有状态
        /// </summary>
        private void ResumeAllStates()
        {
            foreach (var state in activeStates)
            {
                state.OnResume();
            }
        }
        
        /// <summary>
        /// 重置状态机
        /// </summary>
        public void Reset()
        {
            ClearAllStates();
            ClearHistory();
            
            OnStateMachineReset?.Invoke();
            
            if (enableDebugLog)
                Debug.Log("[状态机UI] 状态机已重置");
        }
        
        #endregion
        
        #region 调试方法
        
        /// <summary>
        /// 获取状态机信息
        /// </summary>
        /// <returns>状态机信息字符串</returns>
        public string GetStateMachineInfo()
        {
            var info = $"活跃状态数: {activeStates.Count}, 历史数: {stateHistory.Count}\n";
            info += "活跃状态列表:\n";
            
            foreach (var state in activeStates)
            {
                info += $"  - {state.StateName} ({state.StateType}, 优先级: {state.Priority})\n";
            }
            
            return info;
        }
        
        /// <summary>
        /// 输出状态机调试信息
        /// </summary>
        public void LogStateMachineInfo()
        {
            Debug.Log($"[状态机UI] {GetStateMachineInfo()}");
        }
        
        #endregion
    }
}