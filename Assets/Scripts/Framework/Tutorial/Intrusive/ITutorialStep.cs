using UnityEngine;

namespace Framework.Tutorial.Intrusive
{
    /// <summary>
    /// 新手引导步骤接口
    /// 定义每个引导步骤必须实现的基本功能
    /// </summary>
    public interface ITutorialStep
    {
        /// <summary>
        /// 步骤唯一标识符
        /// </summary>
        string StepId { get; }
        
        /// <summary>
        /// 步骤名称（用于调试和显示）
        /// </summary>
        string StepName { get; }
        
        /// <summary>
        /// 步骤是否已完成
        /// </summary>
        bool IsCompleted { get; }
        
        /// <summary>
        /// 步骤是否可以跳过
        /// </summary>
        bool CanSkip { get; }
        
        /// <summary>
        /// 开始执行引导步骤
        /// </summary>
        /// <param name="onComplete">步骤完成回调</param>
        void StartStep(System.Action onComplete);
        
        /// <summary>
        /// 强制完成步骤
        /// </summary>
        void ForceComplete();
        
        /// <summary>
        /// 跳过当前步骤
        /// </summary>
        void SkipStep();
        
        /// <summary>
        /// 重置步骤状态
        /// </summary>
        void ResetStep();
        
        /// <summary>
        /// 清理步骤资源
        /// </summary>
        void Cleanup();
    }
}