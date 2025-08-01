using System;
using UnityEngine;

namespace Framework.StateMachineUI.Core
{
    /// <summary>
    /// UI状态接口
    /// 定义UI状态的基本行为和生命周期
    /// </summary>
    public interface IUIState
    {
        /// <summary>
        /// 状态名称
        /// </summary>
        string StateName { get; }
        
        /// <summary>
        /// 状态类型
        /// </summary>
        UIStateType StateType { get; }
        
        /// <summary>
        /// 是否为活跃状态
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// 是否可以被中断
        /// </summary>
        bool CanBeInterrupted { get; }
        
        /// <summary>
        /// 状态优先级（数值越大优先级越高）
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// 进入状态时调用
        /// </summary>
        /// <param name="previousState">前一个状态</param>
        /// <param name="data">传递的数据</param>
        void OnEnter(IUIState previousState, object data = null);
        
        /// <summary>
        /// 状态更新时调用
        /// </summary>
        /// <param name="deltaTime">时间间隔</param>
        void OnUpdate(float deltaTime);
        
        /// <summary>
        /// 退出状态时调用
        /// </summary>
        /// <param name="nextState">下一个状态</param>
        void OnExit(IUIState nextState);
        
        /// <summary>
        /// 状态暂停时调用（被其他状态覆盖但不退出）
        /// </summary>
        void OnPause();
        
        /// <summary>
        /// 状态恢复时调用（从暂停状态恢复）
        /// </summary>
        void OnResume();
        
        /// <summary>
        /// 检查是否可以转换到指定状态
        /// </summary>
        /// <param name="targetState">目标状态</param>
        /// <returns>是否可以转换</returns>
        bool CanTransitionTo(IUIState targetState);
        
        /// <summary>
        /// 获取状态数据
        /// </summary>
        /// <returns>状态数据</returns>
        object GetStateData();
        
        /// <summary>
        /// 处理输入事件
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        /// <returns>是否处理了该事件</returns>
        bool HandleInput(UIInputEvent inputEvent);
        
        /// <summary>
        /// 状态进入事件
        /// </summary>
        event Action<IUIState, IUIState> OnStateEnter;
        
        /// <summary>
        /// 状态退出事件
        /// </summary>
        event Action<IUIState, IUIState> OnStateExit;
        
        /// <summary>
        /// 状态暂停事件
        /// </summary>
        event Action<IUIState> OnStatePause;
        
        /// <summary>
        /// 状态恢复事件
        /// </summary>
        event Action<IUIState> OnStateResume;
    }
    
    /// <summary>
    /// UI状态类型枚举
    /// </summary>
    public enum UIStateType
    {
        /// <summary>
        /// 普通状态 - 可以被其他状态替换
        /// </summary>
        Normal,
        
        /// <summary>
        /// 叠加状态 - 可以与其他状态共存
        /// </summary>
        Overlay,
        
        /// <summary>
        /// 独占状态 - 会关闭其他所有状态
        /// </summary>
        Exclusive,
        
        /// <summary>
        /// 系统状态 - 系统级状态，优先级最高
        /// </summary>
        System,
        
        /// <summary>
        /// 临时状态 - 短暂显示的状态
        /// </summary>
        Temporary
    }
    
    /// <summary>
    /// UI输入事件
    /// </summary>
    public class UIInputEvent
    {
        /// <summary>
        /// 事件类型
        /// </summary>
        public UIInputEventType EventType { get; set; }
        
        /// <summary>
        /// 按键代码
        /// </summary>
        public KeyCode KeyCode { get; set; }
        
        /// <summary>
        /// 鼠标位置
        /// </summary>
        public Vector2 MousePosition { get; set; }
        
        /// <summary>
        /// 鼠标按钮
        /// </summary>
        public int MouseButton { get; set; }
        
        /// <summary>
        /// 是否已被处理
        /// </summary>
        public bool IsHandled { get; set; }
        
        /// <summary>
        /// 附加数据
        /// </summary>
        public object Data { get; set; }
        
        /// <summary>
        /// 事件时间戳
        /// </summary>
        public float Timestamp { get; set; }
        
        public UIInputEvent(UIInputEventType eventType)
        {
            EventType = eventType;
            Timestamp = Time.time;
        }
    }
    
    /// <summary>
    /// UI输入事件类型
    /// </summary>
    public enum UIInputEventType
    {
        /// <summary>
        /// 按键按下
        /// </summary>
        KeyDown,
        
        /// <summary>
        /// 按键抬起
        /// </summary>
        KeyUp,
        
        /// <summary>
        /// 鼠标点击
        /// </summary>
        MouseClick,
        
        /// <summary>
        /// 鼠标按下
        /// </summary>
        MouseDown,
        
        /// <summary>
        /// 鼠标抬起
        /// </summary>
        MouseUp,
        
        /// <summary>
        /// 鼠标移动
        /// </summary>
        MouseMove,
        
        /// <summary>
        /// 鼠标滚轮
        /// </summary>
        MouseScroll,
        
        /// <summary>
        /// 触摸开始
        /// </summary>
        TouchBegin,
        
        /// <summary>
        /// 触摸结束
        /// </summary>
        TouchEnd,
        
        /// <summary>
        /// 触摸移动
        /// </summary>
        TouchMove,
        
        /// <summary>
        /// 自定义事件
        /// </summary>
        Custom
    }
}