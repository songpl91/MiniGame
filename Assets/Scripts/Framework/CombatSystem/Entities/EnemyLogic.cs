using System;
using System.Collections.Generic;
using Framework.CombatSystem.Core;
using Framework.CombatSystem.Interfaces;

namespace Framework.CombatSystem.Entities
{
    /// <summary>
    /// 敌人逻辑类
    /// 体现：继承的使用 - Enemy "Is-A" GameCharacterLogic
    /// 同时体现：组合的使用 - Enemy "Has-A" AI行为组件
    /// </summary>
    public class EnemyLogic : GameCharacterLogic, IAIBehavior
    {
        #region 字段
        
        /// <summary>
        /// 敌人等级
        /// </summary>
        private int _enemyLevel;
        
        /// <summary>
        /// 击败奖励经验值
        /// </summary>
        private int _rewardExperience;
        
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
        /// 初始位置（用于返回）
        /// </summary>
        private Vector3Logic _initialPosition;
        
        /// <summary>
        /// 巡逻点列表
        /// </summary>
        private List<Vector3Logic> _patrolPoints;
        
        /// <summary>
        /// 当前巡逻点索引
        /// </summary>
        private int _currentPatrolIndex;
        
        /// <summary>
        /// 状态计时器
        /// </summary>
        private float _stateTimer;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 敌人等级
        /// </summary>
        public int EnemyLevel
        {
            get => _enemyLevel;
            set => _enemyLevel = Math.Max(1, value);
        }
        
        /// <summary>
        /// 击败奖励经验值
        /// </summary>
        public int RewardExperience
        {
            get => _rewardExperience;
            set => _rewardExperience = Math.Max(0, value);
        }
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="characterId">角色ID</param>
        /// <param name="characterName">角色名称</param>
        /// <param name="maxHealth">最大生命值</param>
        /// <param name="enemyLevel">敌人等级</param>
        public EnemyLogic(string characterId, string characterName, float maxHealth = 50f, int enemyLevel = 1) 
            : base(characterId, characterName, maxHealth)
        {
            _enemyLevel = enemyLevel;
            _rewardExperience = enemyLevel * 10; // 根据等级计算奖励经验
            _currentState = AIState.Idle;
            _visionRange = 5f;
            _chaseRange = 8f;
            _patrolPoints = new List<Vector3Logic>();
            _currentPatrolIndex = 0;
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
            return CharacterType.Enemy;
        }
        
        /// <summary>
        /// 角色初始化
        /// </summary>
        public override void Initialize()
        {
            // 敌人特定的初始化逻辑
            _initialPosition = Position;
            _currentState = AIState.Idle;
            
            // 根据等级调整属性
            AdjustAttributesByLevel();
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
            
            // 执行AI逻辑
            ExecuteAI(deltaTime);
        }
        
        #endregion
        
        #region 重写虚方法 - 体现多态性
        
        /// <summary>
        /// 受伤前处理 - 敌人可能有特殊的防御机制
        /// </summary>
        /// <param name="damage">原始伤害</param>
        /// <param name="attacker">攻击者</param>
        /// <returns>处理后的伤害</returns>
        protected override float OnBeforeTakeDamage(float damage, GameCharacterLogic attacker)
        {
            // 敌人受到攻击时，如果没有目标，将攻击者设为目标
            if (_currentTarget == null && attacker != null)
            {
                SetTarget(attacker);
                ChangeState(AIState.Chase);
            }
            
            return damage;
        }
        
        /// <summary>
        /// 死亡处理 - 敌人死亡时的特殊逻辑
        /// </summary>
        protected override void OnDeath()
        {
            // 敌人死亡时给予玩家经验值
            if (_currentTarget is PlayerLogic player)
            {
                player.GainExperience(_rewardExperience);
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
                
                // 只攻击玩家
                if (target.GetCharacterType() != CharacterType.Player) continue;
                
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
            // 空闲状态下，定期切换到巡逻状态
            if (_stateTimer > 2f && _patrolPoints.Count > 0)
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
            if (_patrolPoints.Count == 0)
            {
                ChangeState(AIState.Idle);
                return;
            }
            
            var movementComponent = GetComponent<IMovable>();
            if (movementComponent == null) return;
            
            Vector3Logic targetPoint = _patrolPoints[_currentPatrolIndex];
            float distance = Vector3Logic.Distance(Position, targetPoint);
            
            if (distance <= 0.5f)
            {
                // 到达巡逻点，切换到下一个
                _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Count;
                targetPoint = _patrolPoints[_currentPatrolIndex];
            }
            
            // 移动到巡逻点
            Vector3Logic direction = (targetPoint - Position).Normalized;
            movementComponent.MoveInDirection(direction, deltaTime);
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
            
            // 检查是否超出追击范围
            if (distance > _chaseRange)
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
            
            // 执行攻击
            if (attackComponent.CanAttack)
            {
                attackComponent.Attack(_currentTarget);
            }
        }
        
        /// <summary>
        /// 执行返回状态
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        private void ExecuteReturnState(float deltaTime)
        {
            float distance = Vector3Logic.Distance(Position, _initialPosition);
            
            if (distance <= 0.5f)
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
        
        #region 敌人特有功能
        
        /// <summary>
        /// 根据等级调整属性
        /// </summary>
        private void AdjustAttributesByLevel()
        {
            // 根据等级调整各种属性
            // 这里可以根据游戏设计来调整公式
            
            // 调整攻击组件的攻击力
            var attackComponent = GetComponent<IAttacker>();
            if (attackComponent != null)
            {
                attackComponent.AttackPower = 5f + _enemyLevel * 2f;
            }
            
            // 调整移动组件的移动速度
            var movementComponent = GetComponent<IMovable>();
            if (movementComponent != null)
            {
                movementComponent.MoveSpeed = 3f + _enemyLevel * 0.5f;
            }
        }
        
        /// <summary>
        /// 设置巡逻路径
        /// </summary>
        /// <param name="patrolPoints">巡逻点列表</param>
        public void SetPatrolPath(List<Vector3Logic> patrolPoints)
        {
            _patrolPoints = patrolPoints ?? new List<Vector3Logic>();
            _currentPatrolIndex = 0;
        }
        
        /// <summary>
        /// 添加巡逻点
        /// </summary>
        /// <param name="point">巡逻点</param>
        public void AddPatrolPoint(Vector3Logic point)
        {
            _patrolPoints.Add(point);
        }
        
        /// <summary>
        /// 设置AI参数
        /// </summary>
        /// <param name="visionRange">视野范围</param>
        /// <param name="chaseRange">追击范围</param>
        public void SetAIParameters(float visionRange, float chaseRange)
        {
            _visionRange = visionRange;
            _chaseRange = chaseRange;
        }
        
        #endregion
        
        #region 重写清理方法
        
        /// <summary>
        /// 清理资源
        /// </summary>
        public override void Cleanup()
        {
            // 清理敌人特有的资源
            ClearTarget();
            _patrolPoints?.Clear();
            
            // 调用基类清理
            base.Cleanup();
        }
        
        #endregion
    }
}