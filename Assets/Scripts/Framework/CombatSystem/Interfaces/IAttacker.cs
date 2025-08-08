using System;
using Framework.CombatSystem.Core;

namespace Framework.CombatSystem.Interfaces
{
    /// <summary>
    /// 攻击者接口
    /// 体现：接口隔离原则 - 定义攻击能力的契约
    /// 实现此接口的角色具备攻击能力
    /// </summary>
    public interface IAttacker
    {
        /// <summary>
        /// 攻击力
        /// </summary>
        float AttackPower { get; set; }
        
        /// <summary>
        /// 攻击范围
        /// </summary>
        float AttackRange { get; set; }
        
        /// <summary>
        /// 攻击冷却时间
        /// </summary>
        float AttackCooldown { get; set; }
        
        /// <summary>
        /// 是否可以攻击
        /// </summary>
        bool CanAttack { get; }
        
        /// <summary>
        /// 攻击目标
        /// </summary>
        /// <param name="target">攻击目标</param>
        /// <returns>是否攻击成功</returns>
        bool Attack(GameCharacterLogic target);
        
        /// <summary>
        /// 检查是否在攻击范围内
        /// </summary>
        /// <param name="target">目标</param>
        /// <returns>是否在攻击范围内</returns>
        bool IsInAttackRange(GameCharacterLogic target);
        
        /// <summary>
        /// 攻击事件
        /// </summary>
        event Action<GameCharacterLogic, GameCharacterLogic, float> OnAttackPerformed; // (attacker, target, damage)
    }
}