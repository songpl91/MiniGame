using UnityEngine;

namespace Framework.Tutorial.Intrusive
{
    /// <summary>
    /// 新手引导步骤基类
    /// 提供引导步骤的基础实现，子类可以继承并扩展特定功能
    /// </summary>
    public abstract class TutorialStepBase : ITutorialStep
    {
        [SerializeField] protected string stepId;
        [SerializeField] protected string stepName;
        [SerializeField] protected bool canSkip = true;
        
        protected bool isCompleted = false;
        protected System.Action onCompleteCallback;
        
        /// <summary>
        /// 步骤唯一标识符
        /// </summary>
        public virtual string StepId => stepId;
        
        /// <summary>
        /// 步骤名称
        /// </summary>
        public virtual string StepName => stepName;
        
        /// <summary>
        /// 步骤是否已完成
        /// </summary>
        public virtual bool IsCompleted => isCompleted;
        
        /// <summary>
        /// 步骤是否可以跳过
        /// </summary>
        public virtual bool CanSkip => canSkip;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">步骤ID</param>
        /// <param name="name">步骤名称</param>
        /// <param name="skipable">是否可跳过</param>
        public TutorialStepBase(string id, string name, bool skipable = true)
        {
            stepId = id;
            stepName = name;
            canSkip = skipable;
        }
        
        /// <summary>
        /// 开始执行引导步骤
        /// </summary>
        /// <param name="onComplete">完成回调</param>
        public virtual void StartStep(System.Action onComplete)
        {
            if (isCompleted)
            {
                onComplete?.Invoke();
                return;
            }
            
            onCompleteCallback = onComplete;
            OnStepStart();
        }
        
        /// <summary>
        /// 强制完成步骤
        /// </summary>
        public virtual void ForceComplete()
        {
            if (isCompleted) return;
            
            OnStepForceComplete();
            CompleteStep();
        }
        
        /// <summary>
        /// 跳过当前步骤
        /// </summary>
        public virtual void SkipStep()
        {
            if (!canSkip || isCompleted) return;
            
            OnStepSkip();
            CompleteStep();
        }
        
        /// <summary>
        /// 重置步骤状态
        /// </summary>
        public virtual void ResetStep()
        {
            isCompleted = false;
            onCompleteCallback = null;
            OnStepReset();
        }
        
        /// <summary>
        /// 清理步骤资源
        /// </summary>
        public virtual void Cleanup()
        {
            onCompleteCallback = null;
            OnStepCleanup();
        }
        
        /// <summary>
        /// 完成步骤的内部方法
        /// </summary>
        protected virtual void CompleteStep()
        {
            if (isCompleted) return;
            
            isCompleted = true;
            OnStepComplete();
            onCompleteCallback?.Invoke();
        }
        
        // 抽象方法，子类必须实现
        /// <summary>
        /// 步骤开始时的具体逻辑
        /// </summary>
        protected abstract void OnStepStart();
        
        // 虚方法，子类可以选择重写
        /// <summary>
        /// 步骤完成时的处理
        /// </summary>
        protected virtual void OnStepComplete() { }
        
        /// <summary>
        /// 步骤被强制完成时的处理
        /// </summary>
        protected virtual void OnStepForceComplete() { }
        
        /// <summary>
        /// 步骤被跳过时的处理
        /// </summary>
        protected virtual void OnStepSkip() { }
        
        /// <summary>
        /// 步骤重置时的处理
        /// </summary>
        protected virtual void OnStepReset() { }
        
        /// <summary>
        /// 步骤清理时的处理
        /// </summary>
        protected virtual void OnStepCleanup() { }
    }
}