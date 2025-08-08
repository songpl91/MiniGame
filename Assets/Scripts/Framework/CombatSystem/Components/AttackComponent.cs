using System;
using Framework.CombatSystem.Core;
using Framework.CombatSystem.Interfaces;

namespace Framework.CombatSystem.Components
{
    /// <summary>
    /// 攻击组件基类
    /// 体现：抽象类的使用 - 为不同类型的攻击提供通用基础
    /// </summary>
    public abstract class AttackComponent : ICharacterComponent, IAttacker
    {
        #region 字段
        
        /// <summary>
        /// 拥有此组件的角色
        /// </summary>
        protected GameCharacterLogic _owner;
        
        /// <summary>
        /// 攻击力
        /// </summary>
        protected float _attackPower;
        
        /// <summary>
        /// 攻击范围
        /// </summary>
        protected float _attackRange;
        
        /// <summary>
        /// 攻击冷却时间
        /// </summary>
        protected float _attackCooldown;
        
        /// <summary>
        /// 当前冷却剩余时间
        /// </summary>
        protected float _currentCooldown;
        
        #endregion
        
        #region IAttacker实现
        
        /// <summary>
        /// 攻击力
        /// </summary>
        public virtual float AttackPower
        {
            get => _attackPower;
            set => _attackPower = Math.Max(0f, value);
        }
        
        /// <summary>
        /// 攻击范围
        /// </summary>
        public virtual float AttackRange
        {
            get => _attackRange;
            set => _attackRange = Math.Max(0f, value);
        }
        
        /// <summary>
        /// 攻击冷却时间
        /// </summary>
        public virtual float AttackCooldown
        {
            get => _attackCooldown;
            set => _attackCooldown = Math.Max(0f, value);
        }
        
        /// <summary>
        /// 是否可以攻击
        /// </summary>
        public virtual bool CanAttack => _currentCooldown <= 0f && _owner != null && _owner.IsAlive;
        
        /// <summary>
        /// 攻击目标 - 抽象方法，子类必须实现具体攻击逻辑
        /// </summary>
        /// <param name="target">攻击目标</param>
        /// <returns>是否攻击成功</returns>
        public abstract bool Attack(GameCharacterLogic target);
        
        /// <summary>
        /// 检查是否在攻击范围内
        /// </summary>
        /// <param name="target">目标</param>
        /// <returns>是否在攻击范围内</returns>
        public virtual bool IsInAttackRange(GameCharacterLogic target)
        {
            if (target == null || _owner == null) return false;
            
            float distance = Vector3Logic.Distance(_owner.Position, target.Position);
            return distance <= _attackRange;
        }
        
        /// <summary>
        /// 攻击事件
        /// </summary>
        public event Action<GameCharacterLogic, GameCharacterLogic, float> OnAttackPerformed;
        
        #endregion
        
        #region ICharacterComponent实现
        
        /// <summary>
        /// 组件初始化
        /// </summary>
        /// <param name="owner">拥有此组件的角色</param>
        public virtual void Initialize(GameCharacterLogic owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _attackPower = 10f; // 默认攻击力
            _attackRange = 1.5f; // 默认攻击范围
            _attackCooldown = 1f; // 默认攻击冷却时间
            _currentCooldown = 0f;
        }
        
        /// <summary>
        /// 组件更新
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public virtual void Update(float deltaTime)
        {
            // 更新攻击冷却
            if (_currentCooldown > 0f)
            {
                _currentCooldown = Math.Max(0f, _currentCooldown - deltaTime);
            }
        }
        
        /// <summary>
        /// 组件清理
        /// </summary>
        public virtual void Cleanup()
        {
            _owner = null;
            OnAttackPerformed = null;
        }
        
        #endregion
        
        #region 受保护的辅助方法
        
        /// <summary>
        /// 执行攻击前的检查
        /// </summary>
        /// <param name="target">攻击目标</param>
        /// <returns>是否可以执行攻击</returns>
        protected virtual bool CanPerformAttack(GameCharacterLogic target)
        {
            return CanAttack && 
                   target != null && 
                   target.IsAlive && 
                   target != _owner && 
                   IsInAttackRange(target);
        }
        
        /// <summary>
        /// 开始攻击冷却
        /// </summary>
        protected virtual void StartCooldown()
        {
            _currentCooldown = _attackCooldown;
        }
        
        /// <summary>
        /// 触发攻击事件
        /// </summary>
        /// <param name="target">攻击目标</param>
        /// <param name="damage">造成的伤害</param>
        protected virtual void TriggerAttackEvent(GameCharacterLogic target, float damage)
        {
            OnAttackPerformed?.Invoke(_owner, target, damage);
        }
        
        /// <summary>
        /// 计算实际伤害 - 虚方法，子类可重写实现特殊伤害计算
        /// </summary>
        /// <param name="target">攻击目标</param>
        /// <returns>实际伤害值</returns>
        protected virtual float CalculateDamage(GameCharacterLogic target)
        {
            return _attackPower;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 近战攻击组件
    /// 体现：继承的使用 - 实现具体的近战攻击逻辑
    /// </summary>
    public class MeleeAttackComponent : AttackComponent
    {
        #region 构造函数
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="owner">拥有此组件的角色</param>
        /// <param name="attackPower">攻击力</param>
        /// <param name="attackRange">攻击范围</param>
        /// <param name="attackCooldown">攻击冷却时间</param>
        public MeleeAttackComponent(GameCharacterLogic owner, float attackPower = 10f, float attackRange = 1.5f, float attackCooldown = 1f)
        {
            Initialize(owner);
            AttackPower = attackPower;
            AttackRange = attackRange;
            AttackCooldown = attackCooldown;
        }
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="owner">拥有此组件的角色</param>
        /// <param name="attackPower">攻击力</param>
        /// <param name="attackRange">攻击范围</param>
        /// <param name="attackCooldown">攻击冷却时间</param>
        /// <param name="projectileSpeed">弹道速度</param>
        public RangedAttackComponent(GameCharacterLogic owner, float attackPower = 10f, float attackRange = 8f, float attackCooldown = 1.5f, float projectileSpeed = 10f)
        {
            Initialize(owner);
            AttackPower = attackPower;
            AttackRange = attackRange;
            AttackCooldown = attackCooldown;
            ProjectileSpeed = projectileSpeed;
        }
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="owner">拥有此组件的角色</param>
        /// <param name="attackPower">攻击力</param>
        /// <param name="attackRange">攻击范围</param>
        /// <param name="attackCooldown">攻击冷却时间</param>
        /// <param name="maxMana">最大魔法值</param>
        public MagicAttackComponent(GameCharacterLogic owner, float attackPower = 15f, float attackRange = 6f, float attackCooldown = 2f, float maxMana = 100f)
        {
            Initialize(owner);
            AttackPower = attackPower;
            AttackRange = attackRange;
            AttackCooldown = attackCooldown;
            MaxMana = maxMana;
            CurrentMana = maxMana; // 初始化时魔法值满
        }
        
        #endregion
        
        #region 重写方法
        
        /// <summary>
        /// 攻击目标
        /// </summary>
        /// <param name="target">攻击目标</param>
        /// <returns>是否攻击成功</returns>
        public override bool Attack(GameCharacterLogic target)
        {
            if (!CanPerformAttack(target)) return false;
            
            // 计算伤害
            float damage = CalculateDamage(target);
            
            // 造成伤害
            float actualDamage = target.TakeDamage(damage, _owner);
            
            // 开始冷却
            StartCooldown();
            
            // 触发攻击事件
            TriggerAttackEvent(target, actualDamage);
            
            return true;
        }
        
        /// <summary>
        /// 组件初始化
        /// </summary>
        /// <param name="owner">拥有此组件的角色</param>
        public override void Initialize(GameCharacterLogic owner)
        {
            base.Initialize(owner);
            
            // 近战攻击的特定设置
            _attackRange = 1.5f; // 近战攻击范围较小
            _attackCooldown = 1f; // 近战攻击冷却较短
        }
        
        #endregion
    }
    
    /// <summary>
    /// 远程攻击组件
    /// 体现：继承的使用 - 实现具体的远程攻击逻辑
    /// </summary>
    public class RangedAttackComponent : AttackComponent
    {
        #region 字段
        
        /// <summary>
        /// 弹道速度
        /// </summary>
        private float _projectileSpeed = 10f;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 弹道速度
        /// </summary>
        public float ProjectileSpeed
        {
            get => _projectileSpeed;
            set => _projectileSpeed = Math.Max(0f, value);
        }
        
        #endregion
        
        #region 重写方法
        
        /// <summary>
        /// 攻击目标
        /// </summary>
        /// <param name="target">攻击目标</param>
        /// <returns>是否攻击成功</returns>
        public override bool Attack(GameCharacterLogic target)
        {
            if (!CanPerformAttack(target)) return false;
            
            // 远程攻击需要考虑弹道时间
            float distance = Vector3Logic.Distance(_owner.Position, target.Position);
            float travelTime = distance / _projectileSpeed;
            
            // 这里可以创建弹道对象，但在纯逻辑层我们简化处理
            // 实际项目中可能需要弹道系统
            
            // 计算伤害
            float damage = CalculateDamage(target);
            
            // 造成伤害（简化处理，实际应该在弹道到达时造成伤害）
            float actualDamage = target.TakeDamage(damage, _owner);
            
            // 开始冷却
            StartCooldown();
            
            // 触发攻击事件
            TriggerAttackEvent(target, actualDamage);
            
            return true;
        }
        
        /// <summary>
        /// 组件初始化
        /// </summary>
        /// <param name="owner">拥有此组件的角色</param>
        public override void Initialize(GameCharacterLogic owner)
        {
            base.Initialize(owner);
            
            // 远程攻击的特定设置
            _attackRange = 8f; // 远程攻击范围较大
            _attackCooldown = 1.5f; // 远程攻击冷却较长
        }
        
        #endregion
    }
    
    /// <summary>
    /// 魔法攻击组件
    /// 体现：继承的使用 - 实现具体的魔法攻击逻辑
    /// </summary>
    public class MagicAttackComponent : AttackComponent
    {
        #region 字段
        
        /// <summary>
        /// 魔法消耗
        /// </summary>
        private float _manaCost = 10f;
        
        /// <summary>
        /// 当前魔法值
        /// </summary>
        private float _currentMana = 100f;
        
        /// <summary>
        /// 最大魔法值
        /// </summary>
        private float _maxMana = 100f;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 魔法消耗
        /// </summary>
        public float ManaCost
        {
            get => _manaCost;
            set => _manaCost = Math.Max(0f, value);
        }
        
        /// <summary>
        /// 当前魔法值
        /// </summary>
        public float CurrentMana
        {
            get => _currentMana;
            set => _currentMana = Math.Max(0f, Math.Min(_maxMana, value));
        }
        
        /// <summary>
        /// 最大魔法值
        /// </summary>
        public float MaxMana
        {
            get => _maxMana;
            set
            {
                _maxMana = Math.Max(0f, value);
                _currentMana = Math.Min(_currentMana, _maxMana);
            }
        }
        
        #endregion
        
        #region 重写方法
        
        /// <summary>
        /// 是否可以攻击（需要检查魔法值）
        /// </summary>
        public override bool CanAttack => base.CanAttack && _currentMana >= _manaCost;
        
        /// <summary>
        /// 攻击目标
        /// </summary>
        /// <param name="target">攻击目标</param>
        /// <returns>是否攻击成功</returns>
        public override bool Attack(GameCharacterLogic target)
        {
            if (!CanPerformAttack(target)) return false;
            
            // 消耗魔法
            _currentMana -= _manaCost;
            
            // 计算伤害（魔法攻击可能有特殊计算方式）
            float damage = CalculateDamage(target);
            
            // 造成伤害
            float actualDamage = target.TakeDamage(damage, _owner);
            
            // 开始冷却
            StartCooldown();
            
            // 触发攻击事件
            TriggerAttackEvent(target, actualDamage);
            
            return true;
        }
        
        /// <summary>
        /// 组件初始化
        /// </summary>
        /// <param name="owner">拥有此组件的角色</param>
        public override void Initialize(GameCharacterLogic owner)
        {
            base.Initialize(owner);
            
            // 魔法攻击的特定设置
            _attackRange = 6f; // 魔法攻击范围中等
            _attackCooldown = 2f; // 魔法攻击冷却较长
            _attackPower = 15f; // 魔法攻击力较高
        }
        
        /// <summary>
        /// 计算实际伤害（魔法攻击可能有特殊加成）
        /// </summary>
        /// <param name="target">攻击目标</param>
        /// <returns>实际伤害值</returns>
        protected override float CalculateDamage(GameCharacterLogic target)
        {
            // 魔法攻击可能有智力加成等特殊计算
            float baseDamage = base.CalculateDamage(target);
            float magicBonus = _currentMana / _maxMana * 0.2f; // 魔法值越高，伤害加成越高
            return baseDamage * (1f + magicBonus);
        }
        
        #endregion
        
        #region 魔法值管理
        
        /// <summary>
        /// 恢复魔法值
        /// </summary>
        /// <param name="amount">恢复量</param>
        public void RestoreMana(float amount)
        {
            CurrentMana += amount;
        }
        
        #endregion
    }
}