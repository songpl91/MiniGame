using System;
using UnityEngine;

namespace Framework.StateMachineUI.Core
{
    /// <summary>
    /// UI状态基类
    /// 提供UI状态的基础实现和通用功能
    /// </summary>
    public abstract class UIStateBase : IUIState
    {
        #region 属性
        
        /// <summary>
        /// 状态名称
        /// </summary>
        public virtual string StateName { get; protected set; }
        
        /// <summary>
        /// 状态类型
        /// </summary>
        public virtual UIStateType StateType { get; protected set; } = UIStateType.Normal;
        
        /// <summary>
        /// 是否为活跃状态
        /// </summary>
        public bool IsActive { get; private set; }
        
        /// <summary>
        /// 是否可以被中断
        /// </summary>
        public virtual bool CanBeInterrupted { get; protected set; } = true;
        
        /// <summary>
        /// 状态优先级
        /// </summary>
        public virtual int Priority { get; protected set; } = 0;
        
        /// <summary>
        /// 是否已暂停
        /// </summary>
        public bool IsPaused { get; private set; }
        
        /// <summary>
        /// 状态数据
        /// </summary>
        protected object stateData;
        
        /// <summary>
        /// 状态进入时间
        /// </summary>
        protected float enterTime;
        
        /// <summary>
        /// 状态持续时间
        /// </summary>
        protected float duration;
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 状态进入事件
        /// </summary>
        public event Action<IUIState, IUIState> OnStateEnter;
        
        /// <summary>
        /// 状态退出事件
        /// </summary>
        public event Action<IUIState, IUIState> OnStateExit;
        
        /// <summary>
        /// 状态暂停事件
        /// </summary>
        public event Action<IUIState> OnStatePause;
        
        /// <summary>
        /// 状态恢复事件
        /// </summary>
        public event Action<IUIState> OnStateResume;
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="stateName">状态名称</param>
        /// <param name="stateType">状态类型</param>
        /// <param name="priority">优先级</param>
        protected UIStateBase(string stateName, UIStateType stateType = UIStateType.Normal, int priority = 0)
        {
            StateName = stateName;
            StateType = stateType;
            Priority = priority;
        }
        
        #endregion
        
        #region IUIState 实现
        
        /// <summary>
        /// 进入状态
        /// </summary>
        /// <param name="previousState">前一个状态</param>
        /// <param name="data">传递的数据</param>
        public virtual void OnEnter(IUIState previousState, object data = null)
        {
            IsActive = true;
            IsPaused = false;
            stateData = data;
            enterTime = Time.time;
            
            Debug.Log($"[状态机UI] 进入状态: {StateName}");
            
            // 触发进入事件
            OnStateEnter?.Invoke(this, previousState);
            
            // 执行具体的进入逻辑
            OnEnterState(previousState, data);
        }
        
        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="deltaTime">时间间隔</param>
        public virtual void OnUpdate(float deltaTime)
        {
            if (!IsActive || IsPaused)
                return;
            
            duration = Time.time - enterTime;
            
            // 执行具体的更新逻辑
            OnUpdateState(deltaTime);
        }
        
        /// <summary>
        /// 退出状态
        /// </summary>
        /// <param name="nextState">下一个状态</param>
        public virtual void OnExit(IUIState nextState)
        {
            IsActive = false;
            IsPaused = false;
            
            Debug.Log($"[状态机UI] 退出状态: {StateName}");
            
            // 执行具体的退出逻辑
            OnExitState(nextState);
            
            // 触发退出事件
            OnStateExit?.Invoke(this, nextState);
        }
        
        /// <summary>
        /// 暂停状态
        /// </summary>
        public virtual void OnPause()
        {
            if (!IsActive || IsPaused)
                return;
            
            IsPaused = true;
            
            Debug.Log($"[状态机UI] 暂停状态: {StateName}");
            
            // 执行具体的暂停逻辑
            OnPauseState();
            
            // 触发暂停事件
            OnStatePause?.Invoke(this);
        }
        
        /// <summary>
        /// 恢复状态
        /// </summary>
        public virtual void OnResume()
        {
            if (!IsActive || !IsPaused)
                return;
            
            IsPaused = false;
            
            Debug.Log($"[状态机UI] 恢复状态: {StateName}");
            
            // 执行具体的恢复逻辑
            OnResumeState();
            
            // 触发恢复事件
            OnStateResume?.Invoke(this);
        }
        
        /// <summary>
        /// 检查是否可以转换到指定状态
        /// </summary>
        /// <param name="targetState">目标状态</param>
        /// <returns>是否可以转换</returns>
        public virtual bool CanTransitionTo(IUIState targetState)
        {
            // 默认实现：检查是否可以被中断
            if (!CanBeInterrupted)
                return false;
            
            // 检查状态类型兼容性
            return CheckStateCompatibility(targetState);
        }
        
        /// <summary>
        /// 获取状态数据
        /// </summary>
        /// <returns>状态数据</returns>
        public virtual object GetStateData()
        {
            return stateData;
        }
        
        /// <summary>
        /// 处理输入事件
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        /// <returns>是否处理了该事件</returns>
        public virtual bool HandleInput(UIInputEvent inputEvent)
        {
            if (!IsActive || IsPaused)
                return false;
            
            return OnHandleInput(inputEvent);
        }
        
        #endregion
        
        #region 抽象方法和虚方法
        
        /// <summary>
        /// 具体的进入状态逻辑（子类实现）
        /// </summary>
        /// <param name="previousState">前一个状态</param>
        /// <param name="data">传递的数据</param>
        protected abstract void OnEnterState(IUIState previousState, object data);
        
        /// <summary>
        /// 具体的更新状态逻辑（子类实现）
        /// </summary>
        /// <param name="deltaTime">时间间隔</param>
        protected virtual void OnUpdateState(float deltaTime)
        {
            // 默认空实现
        }
        
        /// <summary>
        /// 具体的退出状态逻辑（子类实现）
        /// </summary>
        /// <param name="nextState">下一个状态</param>
        protected abstract void OnExitState(IUIState nextState);
        
        /// <summary>
        /// 具体的暂停状态逻辑（子类可重写）
        /// </summary>
        protected virtual void OnPauseState()
        {
            // 默认空实现
        }
        
        /// <summary>
        /// 具体的恢复状态逻辑（子类可重写）
        /// </summary>
        protected virtual void OnResumeState()
        {
            // 默认空实现
        }
        
        /// <summary>
        /// 具体的输入处理逻辑（子类可重写）
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        /// <returns>是否处理了该事件</returns>
        protected virtual bool OnHandleInput(UIInputEvent inputEvent)
        {
            return false;
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 检查状态兼容性
        /// </summary>
        /// <param name="targetState">目标状态</param>
        /// <returns>是否兼容</returns>
        protected virtual bool CheckStateCompatibility(IUIState targetState)
        {
            // 系统状态优先级最高
            if (targetState.StateType == UIStateType.System)
                return true;
            
            // 独占状态会关闭其他所有状态
            if (targetState.StateType == UIStateType.Exclusive)
                return true;
            
            // 叠加状态可以与其他状态共存
            if (targetState.StateType == UIStateType.Overlay)
                return true;
            
            // 普通状态和临时状态的转换规则
            if (StateType == UIStateType.Normal || StateType == UIStateType.Temporary)
                return true;
            
            // 其他情况根据优先级判断
            return targetState.Priority >= Priority;
        }
        
        /// <summary>
        /// 设置状态数据
        /// </summary>
        /// <param name="data">状态数据</param>
        protected void SetStateData(object data)
        {
            stateData = data;
        }
        
        /// <summary>
        /// 获取状态持续时间
        /// </summary>
        /// <returns>持续时间（秒）</returns>
        protected float GetStateDuration()
        {
            return duration;
        }
        
        /// <summary>
        /// 检查状态是否超时
        /// </summary>
        /// <param name="timeoutSeconds">超时时间（秒）</param>
        /// <returns>是否超时</returns>
        protected bool IsStateTimeout(float timeoutSeconds)
        {
            return duration >= timeoutSeconds;
        }
        
        #endregion
        
        #region 调试方法
        
        /// <summary>
        /// 获取状态信息字符串
        /// </summary>
        /// <returns>状态信息</returns>
        public virtual string GetStateInfo()
        {
            return $"状态: {StateName}, 类型: {StateType}, 活跃: {IsActive}, 暂停: {IsPaused}, 优先级: {Priority}, 持续时间: {duration:F2}s";
        }
        
        /// <summary>
        /// 输出状态调试信息
        /// </summary>
        public void LogStateInfo()
        {
            Debug.Log($"[状态机UI] {GetStateInfo()}");
        }
        
        #endregion
    }
}