using System;
using System.Collections.Generic;
using Framework.CombatSystem.Core;
using Framework.CombatSystem.Interfaces;

namespace Framework.CombatSystem.Entities
{
    /// <summary>
    /// 怪物逻辑类
    /// 体现：继承的使用 - Monster "Is-A" GameCharacterLogic
    /// 怪物与敌人的区别：怪物更强大，有特殊能力，AI更复杂
    /// </summary>
    public class MonsterLogic : GameCharacterLogic, IAIBehavior
    {
        #region 字段
        
        /// <summary>
        /// 怪物等级
        /// </summary>
        private int _monsterLevel;
        
        /// <summary>
        /// 击败奖励经验值
        /// </summary>
        private int _rewardExperience;
        
        /// <summary>
        /// 怪物类型
        /// </summary>
        private MonsterType _monsterType;
        
        /// <summary>
        /// AI状态
        /// </summary>
        private AIState _currentState;
        
        /// <summary>
        /// 视野范围
        /// </summary>
        private float _visionRange;
        
        /// <summary>
        /// 追击范围
        /// </summary>
        private float _chaseRange;
        
        /// <summary>
        /// 当前目标
        /// </summary>
        private GameCharacterLogic _currentTarget;
        
        /// <summary>
        /// 初始位置
        /// </summary>
        private Vector3Logic _initialPosition;
        
        /// <summary>
        /// 领域范围（怪物的活动范围）
        /// </summary>
        private float _territoryRange;
        
        /// <summary>
        /// 特殊能力冷却时间
        /// </summary>
        private float _specialAbilityCooldown;
        
        /// <summary>
        /// 当前特殊能力冷却
        /// </summary>
        private float _currentSpecialCooldown;
        
        /// <summary>
        /// 愤怒状态（生命值低于50%时触发）
        /// </summary>
        private bool _isEnraged;
        
        /// <summary>
        /// 状态计时器
        /// </summary>
        private float _stateTimer;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 怪物等级
        /// </summary>
        public int MonsterLevel
        {
            get => _monsterLevel;
            set => _monsterLevel = Math.Max(1, value);
        }
        
        /// <summary>
        /// 击败奖励经验值
        /// </summary>
        public int RewardExperience
        {
            get => _rewardExperience;
            set => _rewardExperience = Math.Max(0, value);
        }
        
        /// <summary>
        /// 怪物类型
        /// </summary>
        public MonsterType MonsterType
        {
            get => _monsterType;
            set => _monsterType = value;
        }
        
        /// <summary>
        /// 是否愤怒
        /// </summary>
        public bool IsEnraged => _isEnraged;
        
        /// <summary>
        /// 领域范围
        /// </summary>
        public float TerritoryRange
        {
            get => _territoryRange;
            set => _territoryRange = Math.Max(0f, value);
        }
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 愤怒状态变化事件
        /// </summary>
        public event Action<bool> OnEnrageStateChanged;
        
        /// <summary>
        /// 特殊能力使用事件
        /// </summary>
        public event Action<string> OnSpecialAbilityUsed;
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="characterId">角色ID</param>
        /// <param name="characterName">角色名称</param>
        /// <param name="maxHealth">最大生命值</param>
        /// <param name="monsterLevel">怪物等级</param>
        /// <param name="monsterType">怪物类型</param>
        public MonsterLogic(string characterId, string characterName, float maxHealth = 150f, 
                           int monsterLevel = 5, MonsterType monsterType = MonsterType.Beast) 
            : base(characterId, characterName, maxHealth)
        {
            _monsterLevel = monsterLevel;
            _monsterType = monsterType;
            _rewardExperience = monsterLevel * 25; // 怪物给予更多经验
            _currentState = AIState.Idle;
            _visionRange = 8f; // 怪物视野更广
            _chaseRange = 12f; // 怪物追击范围更大
            _territoryRange = 15f; // 怪物有自己的领域
            _specialAbilityCooldown = 10f; // 特殊能力冷却时间
            _currentSpecialCooldown = 0f;
            _isEnraged = false;
            _stateTimer = 0f;
        }
        
        #endregion
        
        #region 重写抽象方法
        
        /// <summary>
        /// 获取角色类型
        /// </summary>
        /// <returns>角色类型</returns>
        public override CharacterType GetCharacterType()
        {
            return CharacterType.Monster;
        }
        
        /// <summary>
        /// 角色初始化
        /// </summary>
        public override void Initialize()
        {
            // 怪物特定的初始化逻辑
            _initialPosition = Position;
            _currentState = AIState.Idle;
            
            // 根据等级和类型调整属性
            AdjustAttributesByLevelAndType();
        }
        
        /// <summary>
        /// 角色更新逻辑
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public override void Update(float deltaTime)
        {
            if (!IsAlive)
            {
                _currentState = AIState.Dead;
                return;
            }
            
            // 更新特殊能力冷却
            if (_currentSpecialCooldown > 0f)
            {
                _currentSpecialCooldown = Math.Max(0f, _currentSpecialCooldown - deltaTime);
            }
            
            // 检查愤怒状态
            CheckEnrageState();
            
            // 执行AI逻辑
            ExecuteAI(deltaTime);
        }
        
        #endregion
        
        #region 重写虚方法 - 体现多态性
        
        /// <summary>
        /// 受伤前处理 - 怪物有更强的防御能力
        /// </summary>
        /// <param name="damage">原始伤害</param>
        /// <param name="attacker">攻击者</param>
        /// <returns>处理后的伤害</returns>
        protected override float OnBeforeTakeDamage(float damage, GameCharacterLogic attacker)
        {
            // 怪物受到攻击时的特殊处理
            if (_currentTarget == null && attacker != null)
            {
                SetTarget(attacker);
                ChangeState(AIState.Chase);
            }
            
            // 根据怪物类型计算防御
            float defense = CalculateDefenseByType();
            float reducedDamage = Math.Max(1f, damage - defense);
            
            // 愤怒状态下受到的伤害减少
            if (_isEnraged)
            {
                reducedDamage *= 0.8f; // 愤怒时减少20%伤害
            }
            
            return reducedDamage;
        }
        
        /// <summary>
        /// 受伤后处理 - 怪物可能有反击能力
        /// </summary>
        /// <param name="damage">实际伤害</param>
        /// <param name="attacker">攻击者</param>
        protected override void OnAfterTakeDamage(float damage, GameCharacterLogic attacker)
        {
            // 某些类型的怪物受伤后会反击
            if (_monsterType == MonsterType.Elemental && attacker != null)
            {
                // 元素怪物受伤后有概率反击
                if (UnityEngine.Random.Range(0f, 1f) < 0.3f) // 30%概率
                {
                    UseSpecialAbility("ElementalRetaliation", attacker);
                }
            }
        }
        
        /// <summary>
        /// 死亡处理 - 怪物死亡时的特殊逻辑
        /// </summary>
        protected override void OnDeath()
        {
            // 怪物死亡时给予玩家更多经验值
            if (_currentTarget is PlayerLogic player)
            {
                player.GainExperience(_rewardExperience);
            }
            
            // 某些怪物死亡时有特殊效果
            if (_monsterType == MonsterType.Undead)
            {
                // 亡灵怪物死亡时可能复活
                if (UnityEngine.Random.Range(0f, 1f) < 0.1f) // 10%概率
                {
                    UseSpecialAbility("Resurrection", null);
                    return; // 不执行正常死亡流程
                }
            }
            
            // 清除目标
            ClearTarget();
            
            base.OnDeath(); // 调用基类方法
        }
        
        #endregion
        
        #region IAIBehavior实现
        
        /// <summary>
        /// AI状态
        /// </summary>
        public AIState CurrentState => _currentState;
        
        /// <summary>
        /// 视野范围
        /// </summary>
        public float VisionRange
        {
            get => _visionRange;
            set => _visionRange = Math.Max(0f, value);
        }
        
        /// <summary>
        /// 追击范围
        /// </summary>
        public float ChaseRange
        {
            get => _chaseRange;
            set => _chaseRange = Math.Max(0f, value);
        }
        
        /// <summary>
        /// 当前目标
        /// </summary>
        public GameCharacterLogic CurrentTarget => _currentTarget;
        
        /// <summary>
        /// 执行AI逻辑
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public void ExecuteAI(float deltaTime)
        {
            _stateTimer += deltaTime;
            
            switch (_currentState)
            {
                case AIState.Idle:
                    ExecuteIdleState(deltaTime);
                    break;
                case AIState.Patrol:
                    ExecutePatrolState(deltaTime);
                    break;
                case AIState.Chase:
                    ExecuteChaseState(deltaTime);
                    break;
                case AIState.Attack:
                    ExecuteAttackState(deltaTime);
                    break;
                case AIState.Return:
                    ExecuteReturnState(deltaTime);
                    break;
                case AIState.Dead:
                    // 死亡状态不需要处理
                    break;
            }
        }
        
        /// <summary>
        /// 寻找目标
        /// </summary>
        /// <param name="potentialTargets">潜在目标列表</param>
        /// <returns>找到的目标</returns>
        public GameCharacterLogic FindTarget(List<GameCharacterLogic> potentialTargets)
        {
            if (potentialTargets == null || potentialTargets.Count == 0) return null;
            
            GameCharacterLogic closestTarget = null;
            float closestDistance = float.MaxValue;
            
            foreach (var target in potentialTargets)
            {
                if (target == null || !target.IsAlive || target == this) continue;
                
                // 怪物可以攻击玩家和敌人
                var targetType = target.GetCharacterType();
                if (targetType != CharacterType.Player && targetType != CharacterType.Enemy) continue;
                
                float distance = Vector3Logic.Distance(Position, target.Position);
                if (distance <= _visionRange && distance < closestDistance)
                {
                    closestTarget = target;
                    closestDistance = distance;
                }
            }
            
            return closestTarget;
        }
        
        /// <summary>
        /// 设置目标
        /// </summary>
        /// <param name="target">目标</param>
        public void SetTarget(GameCharacterLogic target)
        {
            _currentTarget = target;
        }
        
        /// <summary>
        /// 清除目标
        /// </summary>
        public void ClearTarget()
        {
            _currentTarget = null;
        }
        
        /// <summary>
        /// 切换AI状态
        /// </summary>
        /// <param name="newState">新状态</param>
        public void ChangeState(AIState newState)
        {
            if (_currentState != newState)
            {
                _currentState = newState;
                _stateTimer = 0f;
            }
        }
        
        #endregion
        
        #region AI状态执行方法
        
        /// <summary>
        /// 执行空闲状态
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        private void ExecuteIdleState(float deltaTime)
        {
            // 怪物在空闲状态下会随机移动
            if (_stateTimer > 3f)
            {
                ChangeState(AIState.Patrol);
            }
        }
        
        /// <summary>
        /// 执行巡逻状态
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        private void ExecutePatrolState(float deltaTime)
        {
            var movementComponent = GetComponent<IMovable>();
            if (movementComponent == null)
            {
                ChangeState(AIState.Idle);
                return;
            }
            
            // 在领域范围内随机移动
            if (_stateTimer > 5f || !movementComponent.IsMoving)
            {
                Vector3Logic randomDirection = GetRandomDirectionInTerritory();
                Vector3Logic targetPosition = Position + randomDirection * 3f;
                
                // 确保不超出领域范围
                if (Vector3Logic.Distance(_initialPosition, targetPosition) <= _territoryRange)
                {
                    movementComponent.MoveTo(targetPosition);
                }
                
                _stateTimer = 0f;
            }
        }
        
        /// <summary>
        /// 执行追击状态
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        private void ExecuteChaseState(float deltaTime)
        {
            if (_currentTarget == null || !_currentTarget.IsAlive)
            {
                ClearTarget();
                ChangeState(AIState.Return);
                return;
            }
            
            float distance = Vector3Logic.Distance(Position, _currentTarget.Position);
            
            // 检查是否超出追击范围或领域范围
            float distanceFromHome = Vector3Logic.Distance(Position, _initialPosition);
            if (distance > _chaseRange || distanceFromHome > _territoryRange)
            {
                ClearTarget();
                ChangeState(AIState.Return);
                return;
            }
            
            var attackComponent = GetComponent<IAttacker>();
            if (attackComponent != null && attackComponent.IsInAttackRange(_currentTarget))
            {
                // 进入攻击范围，切换到攻击状态
                ChangeState(AIState.Attack);
                return;
            }
            
            // 继续追击
            var movementComponent = GetComponent<IMovable>();
            if (movementComponent != null)
            {
                Vector3Logic direction = (_currentTarget.Position - Position).Normalized;
                movementComponent.MoveInDirection(direction, deltaTime);
            }
        }
        
        /// <summary>
        /// 执行攻击状态
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        private void ExecuteAttackState(float deltaTime)
        {
            if (_currentTarget == null || !_currentTarget.IsAlive)
            {
                ClearTarget();
                ChangeState(AIState.Return);
                return;
            }
            
            var attackComponent = GetComponent<IAttacker>();
            if (attackComponent == null)
            {
                ChangeState(AIState.Chase);
                return;
            }
            
            // 检查是否还在攻击范围内
            if (!attackComponent.IsInAttackRange(_currentTarget))
            {
                ChangeState(AIState.Chase);
                return;
            }
            
            // 执行普通攻击
            if (attackComponent.CanAttack)
            {
                attackComponent.Attack(_currentTarget);
            }
            
            // 怪物有概率使用特殊能力
            if (_currentSpecialCooldown <= 0f && UnityEngine.Random.Range(0f, 1f) < 0.2f) // 20%概率
            {
                UseSpecialAbility(GetRandomSpecialAbility(), _currentTarget);
            }
        }
        
        /// <summary>
        /// 执行返回状态
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        private void ExecuteReturnState(float deltaTime)
        {
            float distance = Vector3Logic.Distance(Position, _initialPosition);
            
            if (distance <= 1f)
            {
                // 已返回初始位置
                ChangeState(AIState.Idle);
                return;
            }
            
            // 移动回初始位置
            var movementComponent = GetComponent<IMovable>();
            if (movementComponent != null)
            {
                Vector3Logic direction = (_initialPosition - Position).Normalized;
                movementComponent.MoveInDirection(direction, deltaTime);
            }
        }
        
        #endregion
        
        #region 怪物特有功能
        
        /// <summary>
        /// 根据等级和类型调整属性
        /// </summary>
        private void AdjustAttributesByLevelAndType()
        {
            // 根据等级调整基础属性
            var attackComponent = GetComponent<IAttacker>();
            if (attackComponent != null)
            {
                attackComponent.AttackPower = 15f + _monsterLevel * 5f;
            }
            
            var movementComponent = GetComponent<IMovable>();
            if (movementComponent != null)
            {
                movementComponent.MoveSpeed = 2f + _monsterLevel * 0.3f;
            }
            
            // 根据类型调整特殊属性
            switch (_monsterType)
            {
                case MonsterType.Beast:
                    // 野兽类：高攻击力，快速移动
                    if (attackComponent != null) attackComponent.AttackPower *= 1.2f;
                    if (movementComponent != null) movementComponent.MoveSpeed *= 1.3f;
                    break;
                    
                case MonsterType.Elemental:
                    // 元素类：魔法攻击，中等移动速度
                    _specialAbilityCooldown *= 0.8f; // 特殊能力冷却更短
                    break;
                    
                case MonsterType.Undead:
                    // 亡灵类：高生命值，慢速移动
                    // MaxHealth 在构造函数中已设置，这里可以进一步调整
                    if (movementComponent != null) movementComponent.MoveSpeed *= 0.8f;
                    break;
                    
                case MonsterType.Dragon:
                    // 龙类：全面强化
                    if (attackComponent != null) attackComponent.AttackPower *= 1.5f;
                    _visionRange *= 1.5f;
                    _chaseRange *= 1.5f;
                    break;
            }
        }
        
        /// <summary>
        /// 检查愤怒状态
        /// </summary>
        private void CheckEnrageState()
        {
            bool shouldBeEnraged = CurrentHealth / MaxHealth < 0.5f;
            
            if (shouldBeEnraged != _isEnraged)
            {
                _isEnraged = shouldBeEnraged;
                OnEnrageStateChanged?.Invoke(_isEnraged);
                
                if (_isEnraged)
                {
                    // 进入愤怒状态时的属性提升
                    var attackComponent = GetComponent<IAttacker>();
                    if (attackComponent != null)
                    {
                        attackComponent.AttackPower *= 1.3f; // 攻击力提升30%
                        attackComponent.AttackCooldown *= 0.7f; // 攻击速度提升
                    }
                    
                    var movementComponent = GetComponent<IMovable>();
                    if (movementComponent != null)
                    {
                        movementComponent.MoveSpeed *= 1.2f; // 移动速度提升20%
                    }
                }
            }
        }
        
        /// <summary>
        /// 根据类型计算防御力
        /// </summary>
        /// <returns>防御力</returns>
        private float CalculateDefenseByType()
        {
            float baseDefense = _monsterLevel * 1f;
            
            switch (_monsterType)
            {
                case MonsterType.Beast:
                    return baseDefense * 0.8f; // 野兽防御较低
                case MonsterType.Elemental:
                    return baseDefense * 1.2f; // 元素防御中等
                case MonsterType.Undead:
                    return baseDefense * 1.5f; // 亡灵防御较高
                case MonsterType.Dragon:
                    return baseDefense * 2f; // 龙类防御最高
                default:
                    return baseDefense;
            }
        }
        
        /// <summary>
        /// 使用特殊能力
        /// </summary>
        /// <param name="abilityName">能力名称</param>
        /// <param name="target">目标</param>
        private void UseSpecialAbility(string abilityName, GameCharacterLogic target)
        {
            if (_currentSpecialCooldown > 0f) return;
            
            // 根据能力名称执行不同的特殊能力
            switch (abilityName)
            {
                case "Roar":
                    // 咆哮：对周围敌人造成恐惧效果
                    break;
                case "FireBreath":
                    // 火焰吐息：对前方区域造成伤害
                    break;
                case "Heal":
                    // 治疗：恢复生命值
                    Heal(MaxHealth * 0.2f);
                    break;
                case "ElementalRetaliation":
                    // 元素反击：对攻击者造成反伤
                    if (target != null)
                    {
                        target.TakeDamage(10f, this);
                    }
                    break;
                case "Resurrection":
                    // 复活：恢复部分生命值
                    // 这里简化处理，实际项目中可能需要更复杂的复活逻辑
                    break;
            }
            
            // 开始冷却
            _currentSpecialCooldown = _specialAbilityCooldown;
            
            // 触发事件
            OnSpecialAbilityUsed?.Invoke(abilityName);
        }
        
        /// <summary>
        /// 获取随机特殊能力
        /// </summary>
        /// <returns>特殊能力名称</returns>
        private string GetRandomSpecialAbility()
        {
            switch (_monsterType)
            {
                case MonsterType.Beast:
                    return UnityEngine.Random.Range(0, 2) == 0 ? "Roar" : "Heal";
                case MonsterType.Elemental:
                    return "FireBreath";
                case MonsterType.Undead:
                    return "Heal";
                case MonsterType.Dragon:
                    string[] dragonAbilities = { "Roar", "FireBreath", "Heal" };
                    return dragonAbilities[UnityEngine.Random.Range(0, dragonAbilities.Length)];
                default:
                    return "Roar";
            }
        }
        
        /// <summary>
        /// 获取领域内的随机方向
        /// </summary>
        /// <returns>随机方向</returns>
        private Vector3Logic GetRandomDirectionInTerritory()
        {
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            return new Vector3Logic(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        }
        
        #endregion
        
        #region 重写清理方法
        
        /// <summary>
        /// 清理资源
        /// </summary>
        public override void Cleanup()
        {
            // 清理怪物特有的事件
            OnEnrageStateChanged = null;
            OnSpecialAbilityUsed = null;
            
            // 清理目标
            ClearTarget();
            
            // 调用基类清理
            base.Cleanup();
        }
        
        #endregion
    }
    
    /// <summary>
    /// 怪物类型枚举
    /// </summary>
    public enum MonsterType
    {
        /// <summary>
        /// 野兽类
        /// </summary>
        Beast,
        
        /// <summary>
        /// 元素类
        /// </summary>
        Elemental,
        
        /// <summary>
        /// 亡灵类
        /// </summary>
        Undead,
        
        /// <summary>
        /// 龙类
        /// </summary>
        Dragon
    }
}