using UnityEngine;

namespace Framework.Tutorial.Intrusive.Steps
{
    /// <summary>
    /// 等待时间引导步骤
    /// 等待指定时间后自动完成
    /// </summary>
    public class WaitTimeStep : TutorialStepBase
    {
        private float waitTime;
        private float currentTime;
        private bool isWaiting;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">步骤ID</param>
        /// <param name="name">步骤名称</param>
        /// <param name="waitTime">等待时间（秒）</param>
        /// <param name="skipable">是否可跳过</param>
        public WaitTimeStep(string id, string name, float waitTime, bool skipable = true) 
            : base(id, name, skipable)
        {
            this.waitTime = waitTime;
        }
        
        /// <summary>
        /// 步骤开始时的具体逻辑
        /// </summary>
        protected override void OnStepStart()
        {
            currentTime = 0f;
            isWaiting = true;
            
            Debug.Log($"[WaitTimeStep] 开始等待: {stepName} - 等待时间: {waitTime}秒");
            
            // 启动等待协程
            TutorialCoroutineHelper.StartCoroutineStatic(WaitCoroutine());
        }
        
        /// <summary>
        /// 等待协程
        /// </summary>
        private System.Collections.IEnumerator WaitCoroutine()
        {
            while (isWaiting && currentTime < waitTime)
            {
                currentTime += Time.deltaTime;
                
                // 可以在这里显示倒计时UI
                float remainingTime = waitTime - currentTime;
                if (remainingTime > 0)
                {
                    Debug.Log($"[WaitTimeStep] 剩余时间: {remainingTime:F1}秒");
                }
                
                yield return null;
            }
            
            if (isWaiting)
            {
                CompleteStep();
            }
        }
        
        /// <summary>
        /// 步骤完成时的处理
        /// </summary>
        protected override void OnStepComplete()
        {
            isWaiting = false;
            Debug.Log($"[WaitTimeStep] 等待完成: {stepName}");
        }
        
        /// <summary>
        /// 步骤被跳过时的处理
        /// </summary>
        protected override void OnStepSkip()
        {
            isWaiting = false;
            Debug.Log($"[WaitTimeStep] 等待被跳过: {stepName}");
        }
        
        /// <summary>
        /// 步骤重置时的处理
        /// </summary>
        protected override void OnStepReset()
        {
            isWaiting = false;
            currentTime = 0f;
        }
        
        /// <summary>
        /// 步骤清理时的处理
        /// </summary>
        protected override void OnStepCleanup()
        {
            isWaiting = false;
        }
    }
}