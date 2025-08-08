using System.Collections.Generic;
using Framework.CombatSystem.Core;

namespace Framework.CombatSystem.Interfaces
{
    /// <summary>
    /// AI行为接口
    /// 体现：接口隔离原则 - 定义AI行为的契约
    /// 实现此接口的角色具备AI行为能力
    /// </summary>
    public interface IAIBehavior
    {
        /// <summary>
        /// AI状态
        /// </summary>
        AIState CurrentState { get; }
        
        /// <summary>
        /// 视野范围
        /// </summary>
        float VisionRange { get; set; }
        
        /// <summary>
        /// 追击范围
        /// </summary>
        float ChaseRange { get; set; }
        
        /// <summary>
        /// 当前目标
        /// </summary>
        GameCharacterLogic CurrentTarget { get; }
        
        /// <summary>
        /// 执行AI逻辑
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        void ExecuteAI(float deltaTime);
        
        /// <summary>
        /// 寻找目标
        /// </summary>
        /// <param name="potentialTargets">潜在目标列表</param>
        /// <returns>找到的目标</returns>
        GameCharacterLogic FindTarget(List<GameCharacterLogic> potentialTargets);
        
        /// <summary>
        /// 设置目标
        /// </summary>
        /// <param name="target">目标</param>
        void SetTarget(GameCharacterLogic target);
        
        /// <summary>
        /// 清除目标
        /// </summary>
        void ClearTarget();
        
        /// <summary>
        /// 切换AI状态
        /// </summary>
        /// <param name="newState">新状态</param>
        void ChangeState(AIState newState);
    }
    
    /// <summary>
    /// AI状态枚举
    /// </summary>
    public enum AIState
    {
        /// <summary>
        /// 空闲状态
        /// </summary>
        Idle,
        
        /// <summary>
        /// 巡逻状态
        /// </summary>
        Patrol,
        
        /// <summary>
        /// 追击状态
        /// </summary>
        Chase,
        
        /// <summary>
        /// 攻击状态
        /// </summary>
        Attack,
        
        /// <summary>
        /// 返回状态
        /// </summary>
        Return,
        
        /// <summary>
        /// 死亡状态
        /// </summary>
        Dead
    }
}