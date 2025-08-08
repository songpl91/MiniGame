using System;
using Framework.CombatSystem.Core;
using Framework.CombatSystem.Interfaces;

namespace Framework.CombatSystem.Entities
{
    /// <summary>
    /// 玩家逻辑类
    /// 体现：继承的使用 - Player "Is-A" GameCharacterLogic
    /// 同时体现：组合的使用 - Player "Has-A" 各种组件能力
    /// </summary>
    public class PlayerLogic : GameCharacterLogic, IControllable
    {
        #region 字段
        
        /// <summary>
        /// 玩家等级
        /// </summary>
        private int _level;
        
        /// <summary>
        /// 经验值
        /// </summary>
        private int _experience;
        
        /// <summary>
        /// 是否启用控制
        /// </summary>
        private bool _isControlEnabled;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 玩家等级
        /// </summary>
        public int Level
        {
            get => _level;
            private set => _level = Math.Max(1, value);
        }
        
        /// <summary>
        /// 经验值
        /// </summary>
        public int Experience
        {
            get => _experience;
            private set => _experience = Math.Max(0, value);
        }
        
        /// <summary>
        /// 升级所需经验值
        /// </summary>
        public int ExperienceToNextLevel => _level * 100;
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 等级提升事件
        /// </summary>
        public event Action<int, int> OnLevelUp; // (oldLevel, newLevel)
        
        /// <summary>
        /// 经验值变化事件
        /// </summary>
        public event Action<int, int> OnExperienceChanged; // (oldExp, newExp)
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="characterId">角色ID</param>
        /// <param name="characterName">角色名称</param>
        /// <param name="maxHealth">最大生命值</param>
        public PlayerLogic(string characterId, string characterName, float maxHealth = 100f) 
            : base(characterId, characterName, maxHealth)
        {
            _level = 1;
            _experience = 0;
            _isControlEnabled = true;
        }
        
        #endregion
        
        #region 重写抽象方法
        
        /// <summary>
        /// 获取角色类型
        /// </summary>
        /// <returns>角色类型</returns>
        public override CharacterType GetCharacterType()
        {
            return CharacterType.Player;
        }
        
        /// <summary>
        /// 角色初始化
        /// </summary>
        public override void Initialize()
        {
            // 玩家特定的初始化逻辑
            // 可以在这里设置玩家的默认属性、技能等
        }
        
        /// <summary>
        /// 角色更新逻辑
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public override void Update(float deltaTime)
        {
            // 玩家特定的更新逻辑
            // 例如：技能冷却、状态效果等
        }
        
        #endregion
        
        #region 重写虚方法 - 体现多态性
        
        /// <summary>
        /// 受伤前处理 - 玩家可能有护甲减伤
        /// </summary>
        /// <param name="damage">原始伤害</param>
        /// <param name="attacker">攻击者</param>
        /// <returns>处理后的伤害</returns>
        protected override float OnBeforeTakeDamage(float damage, GameCharacterLogic attacker)
        {
            // 玩家可能有护甲减伤
            float armorReduction = _level * 0.5f; // 等级越高，护甲越强
            float reducedDamage = Math.Max(1f, damage - armorReduction);
            
            return reducedDamage;
        }
        
        /// <summary>
        /// 死亡处理 - 玩家死亡时的特殊逻辑
        /// </summary>
        protected override void OnDeath()
        {
            // 玩家死亡时的特殊处理
            // 例如：掉落物品、复活倒计时等
            
            base.OnDeath(); // 调用基类方法
        }
        
        #endregion
        
        #region IControllable实现
        
        /// <summary>
        /// 是否启用控制
        /// </summary>
        public bool IsControlEnabled
        {
            get => _isControlEnabled;
            set => _isControlEnabled = value;
        }
        
        /// <summary>
        /// 处理移动输入
        /// </summary>
        /// <param name="inputDirection">输入方向</param>
        public void HandleMoveInput(Vector3Logic inputDirection)
        {
            if (!_isControlEnabled || !IsAlive) return;
            
            // 获取移动组件并执行移动
            var movementComponent = GetComponent<IMovable>();
            if (movementComponent != null)
            {
                movementComponent.MoveInDirection(inputDirection, 0.016f); // 假设60FPS
            }
        }
        
        /// <summary>
        /// 处理攻击输入
        /// </summary>
        public void HandleAttackInput()
        {
            if (!_isControlEnabled || !IsAlive) return;
            
            // 获取攻击组件并执行攻击
            var attackComponent = GetComponent<IAttacker>();
            if (attackComponent != null && attackComponent.CanAttack)
            {
                // 这里需要找到最近的敌人作为目标
                // 在实际项目中，可能需要目标选择系统
                // 暂时留空，由外部系统处理目标选择
            }
        }
        
        /// <summary>
        /// 处理技能输入
        /// </summary>
        /// <param name="skillIndex">技能索引</param>
        public void HandleSkillInput(int skillIndex)
        {
            if (!_isControlEnabled || !IsAlive) return;
            
            // 技能系统的处理
            // 在实际项目中，这里会调用技能系统
        }
        
        /// <summary>
        /// 处理交互输入
        /// </summary>
        public void HandleInteractInput()
        {
            if (!_isControlEnabled || !IsAlive) return;
            
            // 交互系统的处理
            // 例如：拾取物品、与NPC对话等
        }
        
        /// <summary>
        /// 启用控制
        /// </summary>
        public void EnableControl()
        {
            _isControlEnabled = true;
        }
        
        /// <summary>
        /// 禁用控制
        /// </summary>
        public void DisableControl()
        {
            _isControlEnabled = false;
        }
        
        #endregion
        
        #region 玩家特有功能
        
        /// <summary>
        /// 获得经验值
        /// </summary>
        /// <param name="amount">经验值数量</param>
        public void GainExperience(int amount)
        {
            if (amount <= 0) return;
            
            int oldExperience = _experience;
            _experience += amount;
            
            // 触发经验值变化事件
            OnExperienceChanged?.Invoke(oldExperience, _experience);
            
            // 检查是否升级
            CheckLevelUp();
        }
        
        /// <summary>
        /// 检查是否升级
        /// </summary>
        private void CheckLevelUp()
        {
            while (_experience >= ExperienceToNextLevel)
            {
                int oldLevel = _level;
                _experience -= ExperienceToNextLevel;
                _level++;
                
                // 升级时提升属性
                OnLevelUpBonus();
                
                // 触发升级事件
                OnLevelUp?.Invoke(oldLevel, _level);
            }
        }
        
        /// <summary>
        /// 升级奖励
        /// </summary>
        private void OnLevelUpBonus()
        {
            // 升级时的属性提升
            // 例如：增加最大生命值、攻击力等
            // 这里可以根据游戏设计来调整
        }
        
        /// <summary>
        /// 使用物品
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <returns>是否使用成功</returns>
        public bool UseItem(string itemId)
        {
            if (!IsAlive) return false;
            
            // 物品使用逻辑
            // 在实际项目中，这里会调用物品系统
            return true;
        }
        
        /// <summary>
        /// 学习技能
        /// </summary>
        /// <param name="skillId">技能ID</param>
        /// <returns>是否学习成功</returns>
        public bool LearnSkill(string skillId)
        {
            // 技能学习逻辑
            // 在实际项目中，这里会调用技能系统
            return true;
        }
        
        #endregion
        
        #region 重写清理方法
        
        /// <summary>
        /// 清理资源
        /// </summary>
        public override void Cleanup()
        {
            // 清理玩家特有的事件
            OnLevelUp = null;
            OnExperienceChanged = null;
            
            // 调用基类清理
            base.Cleanup();
        }
        
        #endregion
    }
}