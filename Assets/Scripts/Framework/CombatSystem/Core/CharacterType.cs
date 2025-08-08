namespace Framework.CombatSystem.Core
{
    /// <summary>
    /// 角色类型枚举
    /// 用于区分不同类型的角色，便于系统管理和逻辑处理
    /// </summary>
    public enum CharacterType
    {
        /// <summary>
        /// 玩家角色
        /// </summary>
        Player = 0,
        
        /// <summary>
        /// 普通敌人
        /// </summary>
        Enemy = 1,
        
        /// <summary>
        /// 怪物
        /// </summary>
        Monster = 2,
        
        /// <summary>
        /// Boss
        /// </summary>
        Boss = 3,
        
        /// <summary>
        /// NPC（非战斗角色）
        /// </summary>
        NPC = 4,
        
        /// <summary>
        /// 中立角色
        /// </summary>
        Neutral = 5
    }
}