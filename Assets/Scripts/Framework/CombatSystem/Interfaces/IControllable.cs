using Framework.CombatSystem.Core;

namespace Framework.CombatSystem.Interfaces
{
    /// <summary>
    /// 可控制接口
    /// 体现：接口隔离原则 - 定义控制能力的契约
    /// 实现此接口的角色可以被控制（玩家输入或AI）
    /// </summary>
    public interface IControllable
    {
        /// <summary>
        /// 是否启用控制
        /// </summary>
        bool IsControlEnabled { get; set; }
        
        /// <summary>
        /// 处理移动输入
        /// </summary>
        /// <param name="inputDirection">输入方向</param>
        void HandleMoveInput(Vector3Logic inputDirection);
        
        /// <summary>
        /// 处理攻击输入
        /// </summary>
        void HandleAttackInput();
        
        /// <summary>
        /// 处理技能输入
        /// </summary>
        /// <param name="skillIndex">技能索引</param>
        void HandleSkillInput(int skillIndex);
        
        /// <summary>
        /// 处理交互输入
        /// </summary>
        void HandleInteractInput();
        
        /// <summary>
        /// 启用控制
        /// </summary>
        void EnableControl();
        
        /// <summary>
        /// 禁用控制
        /// </summary>
        void DisableControl();
    }
}