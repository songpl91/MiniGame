using System;
using System.Collections.Generic;
using Framework.CombatSystem.Entities;
using Framework.CombatSystem.Interfaces;

namespace Framework.CombatSystem.Core
{
    /// <summary>
    /// 战斗管理器 - 核心逻辑层
    /// 体现：组合模式的使用 - CombatManager "Has-A" 多个角色和系统
    /// 负责管理所有战斗相关的逻辑，包括角色管理、AI更新、战斗状态等
    /// 完全独立于Unity，可以在任何C#环境中运行
    /// </summary>
    public class CombatManager
    {
        #region 字段
        
        /// <summary>
        /// 所有角色逻辑实例
        /// 体现：组合模式 - 管理器包含多个角色
        /// </summary>
        private readonly List<GameCharacterLogic> _allCharacters;
        
        /// <summary>
        /// 玩家角色列表
        /// </summary>
        private readonly List<PlayerLogic> _players;
        
        /// <summary>
        /// 敌人角色列表
        /// </summary>
        private readonly List<EnemyLogic> _enemies;
        
        /// <summary>
        /// 怪物角色列表
        /// </summary>
        private readonly List<MonsterLogic> _monsters;
        
        /// <summary>
        /// 战斗是否激活
        /// </summary>
        private bool _isCombatActive;
        
        /// <summary>
        /// 战斗回合计数
        /// </summary>
        private int _combatRound;
        
        /// <summary>
        /// 战斗开始时间
        /// </summary>
        private float _combatStartTime;
        
        /// <summary>
        /// 当前战斗时间
        /// </summary>
        private float _currentCombatTime;
        
        /// <summary>
        /// 角色ID计数器
        /// </summary>
        private int _characterIdCounter;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 所有角色数量
        /// </summary>
        public int TotalCharacterCount => _allCharacters.Count;
        
        /// <summary>
        /// 存活角色数量
        /// </summary>
        public int AliveCharacterCount
        {
            get
            {
                int count = 0;
                foreach (var character in _allCharacters)
                {
                    if (character.IsAlive) count++;
                }
                return count;
            }
        }
        
        /// <summary>
        /// 玩家数量
        /// </summary>
        public int PlayerCount => _players.Count;
        
        /// <summary>
        /// 敌人数量
        /// </summary>
        public int EnemyCount => _enemies.Count;
        
        /// <summary>
        /// 怪物数量
        /// </summary>
        public int MonsterCount => _monsters.Count;
        
        /// <summary>
        /// 战斗是否激活
        /// </summary>
        public bool IsCombatActive => _isCombatActive;
        
        /// <summary>
        /// 当前战斗回合
        /// </summary>
        public int CurrentCombatRound => _combatRound;
        
        /// <summary>
        /// 当前战斗时间
        /// </summary>
        public float CurrentCombatTime => _currentCombatTime;
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 战斗开始事件
        /// </summary>
        public event Action OnCombatStarted;
        
        /// <summary>
        /// 战斗结束事件
        /// </summary>
        public event Action<CombatResult> OnCombatEnded;
        
        /// <summary>
        /// 角色添加事件
        /// </summary>
        public event Action<GameCharacterLogic> OnCharacterAdded;
        
        /// <summary>
        /// 角色移除事件
        /// </summary>
        public event Action<GameCharacterLogic> OnCharacterRemoved;
        
        /// <summary>
        /// 回合变化事件
        /// </summary>
        public event Action<int> OnRoundChanged;
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public CombatManager()
        {
            _allCharacters = new List<GameCharacterLogic>();
            _players = new List<PlayerLogic>();
            _enemies = new List<EnemyLogic>();
            _monsters = new List<MonsterLogic>();
            _isCombatActive = false;
            _combatRound = 0;
            _combatStartTime = 0f;
            _currentCombatTime = 0f;
            _characterIdCounter = 1;
        }
        
        #endregion
        
        #region 角色管理
        
        /// <summary>
        /// 添加角色
        /// 体现：多态性的使用 - 可以添加任何继承自GameCharacterLogic的角色
        /// </summary>
        /// <param name="character">角色逻辑</param>
        public void AddCharacter(GameCharacterLogic character)
        {
            if (character == null) return;
            
            // 检查是否已存在
            if (_allCharacters.Contains(character)) return;
            
            // 添加到总列表
            _allCharacters.Add(character);
            
            // 根据类型添加到对应列表
            // 体现：多态性 - 运行时类型判断
            switch (character.GetCharacterType())
            {
                case CharacterType.Player:
                    if (character is PlayerLogic player)
                    {
                        _players.Add(player);
                        // 订阅玩家特有事件
                        player.OnLevelUp += OnPlayerLevelUp;
                        player.OnExperienceGained += OnPlayerExperienceGained;
                    }
                    break;
                    
                case CharacterType.Enemy:
                    if (character is EnemyLogic enemy)
                    {
                        _enemies.Add(enemy);
                        // 订阅敌人特有事件
                        enemy.OnStateChanged += OnEnemyStateChanged;
                    }
                    break;
                    
                case CharacterType.Monster:
                    if (character is MonsterLogic monster)
                    {
                        _monsters.Add(monster);
                        // 订阅怪物特有事件
                        monster.OnEnrageStateChanged += OnMonsterEnrageStateChanged;
                        monster.OnSpecialAbilityUsed += OnMonsterSpecialAbilityUsed;
                    }
                    break;
            }
            
            // 订阅通用事件
            character.OnHealthChanged += OnCharacterHealthChanged;
            character.OnDied += OnCharacterDied;
            character.OnPositionChanged += OnCharacterPositionChanged;
            
            // 初始化角色
            character.Initialize();
            
            // 触发事件
            OnCharacterAdded?.Invoke(character);
        }
        
        /// <summary>
        /// 移除角色
        /// </summary>
        /// <param name="character">角色逻辑</param>
        public void RemoveCharacter(GameCharacterLogic character)
        {
            if (character == null) return;
            
            // 从总列表移除
            if (!_allCharacters.Remove(character)) return;
            
            // 从对应类型列表移除
            switch (character.GetCharacterType())
            {
                case CharacterType.Player:
                    if (character is PlayerLogic player)
                    {
                        _players.Remove(player);
                        // 取消订阅玩家事件
                        player.OnLevelUp -= OnPlayerLevelUp;
                        player.OnExperienceGained -= OnPlayerExperienceGained;
                    }
                    break;
                    
                case CharacterType.Enemy:
                    if (character is EnemyLogic enemy)
                    {
                        _enemies.Remove(enemy);
                        // 取消订阅敌人事件
                        enemy.OnStateChanged -= OnEnemyStateChanged;
                    }
                    break;
                    
                case CharacterType.Monster:
                    if (character is MonsterLogic monster)
                    {
                        _monsters.Remove(monster);
                        // 取消订阅怪物事件
                        monster.OnEnrageStateChanged -= OnMonsterEnrageStateChanged;
                        monster.OnSpecialAbilityUsed -= OnMonsterSpecialAbilityUsed;
                    }
                    break;
            }
            
            // 取消订阅通用事件
            character.OnHealthChanged -= OnCharacterHealthChanged;
            character.OnDied -= OnCharacterDied;
            character.OnPositionChanged -= OnCharacterPositionChanged;
            
            // 清理角色
            character.Cleanup();
            
            // 触发事件
            OnCharacterRemoved?.Invoke(character);
        }
        
        /// <summary>
        /// 创建玩家
        /// </summary>
        /// <param name="playerName">玩家名称</param>
        /// <param name="maxHealth">最大生命值</param>
        /// <returns>玩家逻辑实例</returns>
        public PlayerLogic CreatePlayer(string playerName, float maxHealth = 100f)
        {
            string playerId = $"Player_{_characterIdCounter++}";
            var player = new PlayerLogic(playerId, playerName, maxHealth);
            
            // 为玩家添加基础组件
            AddBasicComponentsToPlayer(player);
            
            AddCharacter(player);
            return player;
        }
        
        /// <summary>
        /// 创建敌人
        /// </summary>
        /// <param name="enemyName">敌人名称</param>
        /// <param name="enemyLevel">敌人等级</param>
        /// <param name="maxHealth">最大生命值</param>
        /// <returns>敌人逻辑实例</returns>
        public EnemyLogic CreateEnemy(string enemyName, int enemyLevel = 1, float maxHealth = 80f)
        {
            string enemyId = $"Enemy_{_characterIdCounter++}";
            var enemy = new EnemyLogic(enemyId, enemyName, maxHealth, enemyLevel);
            
            // 为敌人添加基础组件
            AddBasicComponentsToEnemy(enemy);
            
            AddCharacter(enemy);
            return enemy;
        }
        
        /// <summary>
        /// 创建怪物
        /// </summary>
        /// <param name="monsterName">怪物名称</param>
        /// <param name="monsterLevel">怪物等级</param>
        /// <param name="monsterType">怪物类型</param>
        /// <param name="maxHealth">最大生命值</param>
        /// <returns>怪物逻辑实例</returns>
        public MonsterLogic CreateMonster(string monsterName, int monsterLevel = 5, 
                                         MonsterType monsterType = MonsterType.Beast, float maxHealth = 150f)
        {
            string monsterId = $"Monster_{_characterIdCounter++}";
            var monster = new MonsterLogic(monsterId, monsterName, maxHealth, monsterLevel, monsterType);
            
            // 为怪物添加基础组件
            AddBasicComponentsToMonster(monster);
            
            AddCharacter(monster);
            return monster;
        }
        
        #endregion
        
        #region 查询方法
        
        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns>所有角色列表的只读副本</returns>
        public List<GameCharacterLogic> GetAllCharacters()
        {
            return new List<GameCharacterLogic>(_allCharacters);
        }
        
        /// <summary>
        /// 获取所有存活角色
        /// </summary>
        /// <returns>存活角色列表</returns>
        public List<GameCharacterLogic> GetAliveCharacters()
        {
            var aliveCharacters = new List<GameCharacterLogic>();
            foreach (var character in _allCharacters)
            {
                if (character.IsAlive)
                {
                    aliveCharacters.Add(character);
                }
            }
            return aliveCharacters;
        }
        
        /// <summary>
        /// 获取指定类型的角色
        /// </summary>
        /// <param name="characterType">角色类型</param>
        /// <returns>指定类型的角色列表</returns>
        public List<GameCharacterLogic> GetCharactersByType(CharacterType characterType)
        {
            var characters = new List<GameCharacterLogic>();
            foreach (var character in _allCharacters)
            {
                if (character.GetCharacterType() == characterType)
                {
                    characters.Add(character);
                }
            }
            return characters;
        }
        
        /// <summary>
        /// 根据ID查找角色
        /// </summary>
        /// <param name="characterId">角色ID</param>
        /// <returns>找到的角色，未找到返回null</returns>
        public GameCharacterLogic FindCharacterById(string characterId)
        {
            foreach (var character in _allCharacters)
            {
                if (character.CharacterId == characterId)
                {
                    return character;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 获取指定位置范围内的角色
        /// </summary>
        /// <param name="center">中心位置</param>
        /// <param name="radius">半径</param>
        /// <returns>范围内的角色列表</returns>
        public List<GameCharacterLogic> GetCharactersInRange(Vector3Logic center, float radius)
        {
            var charactersInRange = new List<GameCharacterLogic>();
            foreach (var character in _allCharacters)
            {
                if (character.IsAlive && Vector3Logic.Distance(character.Position, center) <= radius)
                {
                    charactersInRange.Add(character);
                }
            }
            return charactersInRange;
        }
        
        #endregion
        
        #region 战斗管理
        
        /// <summary>
        /// 开始战斗
        /// </summary>
        public void StartCombat()
        {
            if (_isCombatActive) return;
            
            _isCombatActive = true;
            _combatRound = 1;
            _combatStartTime = GetCurrentTime();
            _currentCombatTime = 0f;
            
            // 通知所有角色战斗开始
            foreach (var character in _allCharacters)
            {
                if (character.IsAlive)
                {
                    // 这里可以添加战斗开始时的特殊逻辑
                    // 例如：重置某些状态、应用战斗buff等
                }
            }
            
            OnCombatStarted?.Invoke();
        }
        
        /// <summary>
        /// 结束战斗
        /// </summary>
        /// <param name="result">战斗结果</param>
        public void EndCombat(CombatResult result)
        {
            if (!_isCombatActive) return;
            
            _isCombatActive = false;
            
            // 通知所有角色战斗结束
            foreach (var character in _allCharacters)
            {
                // 这里可以添加战斗结束时的特殊逻辑
                // 例如：清除战斗buff、重置状态等
            }
            
            OnCombatEnded?.Invoke(result);
        }
        
        /// <summary>
        /// 检查战斗是否应该结束
        /// </summary>
        /// <returns>战斗结果，如果战斗未结束返回null</returns>
        public CombatResult CheckCombatEnd()
        {
            if (!_isCombatActive) return null;
            
            // 检查玩家是否全部死亡
            bool anyPlayerAlive = false;
            foreach (var player in _players)
            {
                if (player.IsAlive)
                {
                    anyPlayerAlive = true;
                    break;
                }
            }
            
            if (!anyPlayerAlive)
            {
                return new CombatResult
                {
                    IsVictory = false,
                    CombatDuration = _currentCombatTime,
                    TotalRounds = _combatRound,
                    SurvivingPlayers = 0,
                    DefeatedEnemies = GetDefeatedEnemyCount(),
                    DefeatedMonsters = GetDefeatedMonsterCount()
                };
            }
            
            // 检查敌人和怪物是否全部死亡
            bool anyEnemyAlive = false;
            foreach (var enemy in _enemies)
            {
                if (enemy.IsAlive)
                {
                    anyEnemyAlive = true;
                    break;
                }
            }
            
            bool anyMonsterAlive = false;
            foreach (var monster in _monsters)
            {
                if (monster.IsAlive)
                {
                    anyMonsterAlive = true;
                    break;
                }
            }
            
            if (!anyEnemyAlive && !anyMonsterAlive)
            {
                return new CombatResult
                {
                    IsVictory = true,
                    CombatDuration = _currentCombatTime,
                    TotalRounds = _combatRound,
                    SurvivingPlayers = GetAlivePlayers().Count,
                    DefeatedEnemies = _enemies.Count,
                    DefeatedMonsters = _monsters.Count
                };
            }
            
            return null; // 战斗继续
        }
        
        #endregion
        
        #region 更新逻辑
        
        /// <summary>
        /// 更新战斗管理器
        /// 这是核心的更新方法，需要在游戏主循环中调用
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public void Update(float deltaTime)
        {
            if (_isCombatActive)
            {
                _currentCombatTime += deltaTime;
            }
            
            // 更新所有角色
            UpdateAllCharacters(deltaTime);
            
            // 更新AI行为
            UpdateAIBehaviors(deltaTime);
            
            // 检查战斗是否结束
            if (_isCombatActive)
            {
                var combatResult = CheckCombatEnd();
                if (combatResult != null)
                {
                    EndCombat(combatResult);
                }
            }
        }
        
        /// <summary>
        /// 更新所有角色
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        private void UpdateAllCharacters(float deltaTime)
        {
            // 使用倒序遍历，防止在遍历过程中移除角色导致的问题
            for (int i = _allCharacters.Count - 1; i >= 0; i--)
            {
                var character = _allCharacters[i];
                if (character != null)
                {
                    character.Update(deltaTime);
                }
            }
        }
        
        /// <summary>
        /// 更新AI行为
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        private void UpdateAIBehaviors(float deltaTime)
        {
            var allAliveCharacters = GetAliveCharacters();
            
            // 更新敌人AI
            foreach (var enemy in _enemies)
            {
                if (enemy.IsAlive)
                {
                    // 为AI提供潜在目标列表
                    var potentialTargets = GetPotentialTargetsForAI(enemy, allAliveCharacters);
                    
                    // 如果没有目标，尝试寻找新目标
                    if (enemy.CurrentTarget == null || !enemy.CurrentTarget.IsAlive)
                    {
                        var newTarget = enemy.FindTarget(potentialTargets);
                        if (newTarget != null)
                        {
                            enemy.SetTarget(newTarget);
                        }
                    }
                    
                    enemy.ExecuteAI(deltaTime);
                }
            }
            
            // 更新怪物AI
            foreach (var monster in _monsters)
            {
                if (monster.IsAlive)
                {
                    // 为AI提供潜在目标列表
                    var potentialTargets = GetPotentialTargetsForAI(monster, allAliveCharacters);
                    
                    // 如果没有目标，尝试寻找新目标
                    if (monster.CurrentTarget == null || !monster.CurrentTarget.IsAlive)
                    {
                        var newTarget = monster.FindTarget(potentialTargets);
                        if (newTarget != null)
                        {
                            monster.SetTarget(newTarget);
                        }
                    }
                    
                    monster.ExecuteAI(deltaTime);
                }
            }
        }
        
        /// <summary>
        /// 获取AI的潜在目标列表
        /// </summary>
        /// <param name="aiCharacter">AI角色</param>
        /// <param name="allAliveCharacters">所有存活角色</param>
        /// <returns>潜在目标列表</returns>
        private List<GameCharacterLogic> GetPotentialTargetsForAI(GameCharacterLogic aiCharacter, 
                                                                  List<GameCharacterLogic> allAliveCharacters)
        {
            var potentialTargets = new List<GameCharacterLogic>();
            
            foreach (var character in allAliveCharacters)
            {
                if (character == aiCharacter) continue; // 不能攻击自己
                
                // 根据AI角色类型确定可攻击的目标
                var aiType = aiCharacter.GetCharacterType();
                var targetType = character.GetCharacterType();
                
                bool canAttack = false;
                
                if (aiType == CharacterType.Enemy || aiType == CharacterType.Monster)
                {
                    // 敌人和怪物可以攻击玩家
                    if (targetType == CharacterType.Player)
                    {
                        canAttack = true;
                    }
                    // 在某些情况下，敌人和怪物也可能互相攻击
                    // 这里可以根据具体游戏规则进行调整
                }
                
                if (canAttack)
                {
                    potentialTargets.Add(character);
                }
            }
            
            return potentialTargets;
        }
        
        #endregion
        
        #region 组件管理辅助方法
        
        /// <summary>
        /// 为玩家添加基础组件
        /// 体现：组合模式的使用 - 通过组件组合实现功能
        /// </summary>
        /// <param name="player">玩家逻辑</param>
        private void AddBasicComponentsToPlayer(PlayerLogic player)
        {
            // 添加地面移动组件
            var movementComponent = new GroundMovementComponent(player, 5f);
            player.AddComponent(movementComponent);
            
            // 添加近战攻击组件
            var attackComponent = new MeleeAttackComponent(player, 20f, 2f, 1.5f);
            player.AddComponent(attackComponent);
        }
        
        /// <summary>
        /// 为敌人添加基础组件
        /// </summary>
        /// <param name="enemy">敌人逻辑</param>
        private void AddBasicComponentsToEnemy(EnemyLogic enemy)
        {
            // 添加地面移动组件
            var movementComponent = new GroundMovementComponent(enemy, 3f);
            enemy.AddComponent(movementComponent);
            
            // 根据敌人等级决定攻击类型
            if (enemy.EnemyLevel <= 3)
            {
                // 低级敌人使用近战攻击
                var attackComponent = new MeleeAttackComponent(enemy, 15f, 1.8f, 2f);
                enemy.AddComponent(attackComponent);
            }
            else
            {
                // 高级敌人使用远程攻击
                var attackComponent = new RangedAttackComponent(enemy, 18f, 5f, 2.5f, 8f);
                enemy.AddComponent(attackComponent);
            }
        }
        
        /// <summary>
        /// 为怪物添加基础组件
        /// </summary>
        /// <param name="monster">怪物逻辑</param>
        private void AddBasicComponentsToMonster(MonsterLogic monster)
        {
            // 根据怪物类型添加不同的组件
            switch (monster.MonsterType)
            {
                case MonsterType.Beast:
                    // 野兽：快速移动 + 强力近战
                    var beastMovement = new GroundMovementComponent(monster, 6f);
                    monster.AddComponent(beastMovement);
                    
                    var beastAttack = new MeleeAttackComponent(monster, 25f, 2.2f, 1.8f);
                    monster.AddComponent(beastAttack);
                    break;
                    
                case MonsterType.Elemental:
                    // 元素：中等移动 + 魔法攻击
                    var elementalMovement = new GroundMovementComponent(monster, 4f);
                    monster.AddComponent(elementalMovement);
                    
                    var elementalAttack = new MagicAttackComponent(monster, 30f, 6f, 3f, 50f);
                    monster.AddComponent(elementalAttack);
                    break;
                    
                case MonsterType.Undead:
                    // 亡灵：慢速移动 + 中等近战
                    var undeadMovement = new GroundMovementComponent(monster, 2.5f);
                    monster.AddComponent(undeadMovement);
                    
                    var undeadAttack = new MeleeAttackComponent(monster, 22f, 2.5f, 2.2f);
                    monster.AddComponent(undeadAttack);
                    break;
                    
                case MonsterType.Dragon:
                    // 龙：飞行移动 + 强力远程攻击
                    var dragonMovement = new FlyingMovementComponent(monster, 7f, 10f);
                    monster.AddComponent(dragonMovement);
                    
                    var dragonAttack = new RangedAttackComponent(monster, 40f, 8f, 4f, 12f);
                    monster.AddComponent(dragonAttack);
                    break;
            }
        }
        
        #endregion
        
        #region 事件处理方法
        
        /// <summary>
        /// 角色生命值变化处理
        /// </summary>
        /// <param name="character">角色</param>
        /// <param name="oldHealth">旧生命值</param>
        /// <param name="newHealth">新生命值</param>
        private void OnCharacterHealthChanged(GameCharacterLogic character, float oldHealth, float newHealth)
        {
            // 这里可以添加生命值变化时的逻辑
            // 例如：触发治疗效果、检查低血量状态等
        }
        
        /// <summary>
        /// 角色死亡处理
        /// </summary>
        /// <param name="character">死亡的角色</param>
        private void OnCharacterDied(GameCharacterLogic character)
        {
            // 清除其他角色对该角色的目标引用
            ClearTargetReferences(character);
        }
        
        /// <summary>
        /// 角色位置变化处理
        /// </summary>
        /// <param name="character">角色</param>
        /// <param name="oldPosition">旧位置</param>
        /// <param name="newPosition">新位置</param>
        private void OnCharacterPositionChanged(GameCharacterLogic character, Vector3Logic oldPosition, Vector3Logic newPosition)
        {
            // 这里可以添加位置变化时的逻辑
            // 例如：检查区域触发器、更新空间索引等
        }
        
        /// <summary>
        /// 玩家升级处理
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="newLevel">新等级</param>
        private void OnPlayerLevelUp(PlayerLogic player, int newLevel)
        {
            // 玩家升级时的处理逻辑
            // 例如：恢复生命值、增加属性等
        }
        
        /// <summary>
        /// 玩家获得经验处理
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="experience">获得的经验值</param>
        private void OnPlayerExperienceGained(PlayerLogic player, int experience)
        {
            // 玩家获得经验时的处理逻辑
        }
        
        /// <summary>
        /// 敌人状态变化处理
        /// </summary>
        /// <param name="enemy">敌人</param>
        /// <param name="newState">新状态</param>
        private void OnEnemyStateChanged(EnemyLogic enemy, AIState newState)
        {
            // 敌人状态变化时的处理逻辑
        }
        
        /// <summary>
        /// 怪物愤怒状态变化处理
        /// </summary>
        /// <param name="monster">怪物</param>
        /// <param name="isEnraged">是否愤怒</param>
        private void OnMonsterEnrageStateChanged(MonsterLogic monster, bool isEnraged)
        {
            // 怪物愤怒状态变化时的处理逻辑
        }
        
        /// <summary>
        /// 怪物使用特殊能力处理
        /// </summary>
        /// <param name="monster">怪物</param>
        /// <param name="abilityName">能力名称</param>
        private void OnMonsterSpecialAbilityUsed(MonsterLogic monster, string abilityName)
        {
            // 怪物使用特殊能力时的处理逻辑
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 清除对指定角色的目标引用
        /// </summary>
        /// <param name="deadCharacter">死亡的角色</param>
        private void ClearTargetReferences(GameCharacterLogic deadCharacter)
        {
            // 清除敌人的目标引用
            foreach (var enemy in _enemies)
            {
                if (enemy.CurrentTarget == deadCharacter)
                {
                    enemy.ClearTarget();
                }
            }
            
            // 清除怪物的目标引用
            foreach (var monster in _monsters)
            {
                if (monster.CurrentTarget == deadCharacter)
                {
                    monster.ClearTarget();
                }
            }
        }
        
        /// <summary>
        /// 获取存活的玩家列表
        /// </summary>
        /// <returns>存活的玩家列表</returns>
        private List<PlayerLogic> GetAlivePlayers()
        {
            var alivePlayers = new List<PlayerLogic>();
            foreach (var player in _players)
            {
                if (player.IsAlive)
                {
                    alivePlayers.Add(player);
                }
            }
            return alivePlayers;
        }
        
        /// <summary>
        /// 获取被击败的敌人数量
        /// </summary>
        /// <returns>被击败的敌人数量</returns>
        private int GetDefeatedEnemyCount()
        {
            int count = 0;
            foreach (var enemy in _enemies)
            {
                if (!enemy.IsAlive) count++;
            }
            return count;
        }
        
        /// <summary>
        /// 获取被击败的怪物数量
        /// </summary>
        /// <returns>被击败的怪物数量</returns>
        private int GetDefeatedMonsterCount()
        {
            int count = 0;
            foreach (var monster in _monsters)
            {
                if (!monster.IsAlive) count++;
            }
            return count;
        }
        
        /// <summary>
        /// 获取当前时间（这里简化处理，实际项目中可能需要更精确的时间管理）
        /// </summary>
        /// <returns>当前时间</returns>
        private float GetCurrentTime()
        {
            // 在实际项目中，这里应该返回游戏时间或系统时间
            // 这里简化为返回0，实际使用时需要替换为真实的时间获取方法
            return 0f;
        }
        
        #endregion
        
        #region 清理方法
        
        /// <summary>
        /// 清理所有资源
        /// </summary>
        public void Cleanup()
        {
            // 清理所有角色
            for (int i = _allCharacters.Count - 1; i >= 0; i--)
            {
                RemoveCharacter(_allCharacters[i]);
            }
            
            // 清理事件
            OnCombatStarted = null;
            OnCombatEnded = null;
            OnCharacterAdded = null;
            OnCharacterRemoved = null;
            OnRoundChanged = null;
            
            // 重置状态
            _isCombatActive = false;
            _combatRound = 0;
            _combatStartTime = 0f;
            _currentCombatTime = 0f;
            _characterIdCounter = 1;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 战斗结果
    /// </summary>
    public class CombatResult
    {
        /// <summary>
        /// 是否胜利
        /// </summary>
        public bool IsVictory { get; set; }
        
        /// <summary>
        /// 战斗持续时间
        /// </summary>
        public float CombatDuration { get; set; }
        
        /// <summary>
        /// 总回合数
        /// </summary>
        public int TotalRounds { get; set; }
        
        /// <summary>
        /// 存活的玩家数量
        /// </summary>
        public int SurvivingPlayers { get; set; }
        
        /// <summary>
        /// 击败的敌人数量
        /// </summary>
        public int DefeatedEnemies { get; set; }
        
        /// <summary>
        /// 击败的怪物数量
        /// </summary>
        public int DefeatedMonsters { get; set; }
    }
}