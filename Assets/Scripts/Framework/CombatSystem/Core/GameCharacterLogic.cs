using System;
using System.Collections.Generic;

namespace Framework.CombatSystem.Core
{
    /// <summary>
    /// 游戏角色逻辑抽象基类
    /// 体现：抽象类的使用 - 定义所有角色的核心本质（Is-A关系）
    /// 职责：包含所有角色共有的核心属性和基础行为
    /// </summary>
    public abstract class GameCharacterLogic
    {
        #region 核心属性 - 所有角色都必须具备的基本属性
        
        /// <summary>
        /// 角色唯一标识
        /// </summary>
        public string CharacterId { get; private set; }
        
        /// <summary>
        /// 角色名称
        /// </summary>
        public string CharacterName { get; private set; }
        
        /// <summary>
        /// 最大生命值
        /// </summary>
        public float MaxHealth { get; private set; }
        
        /// <summary>
        /// 当前生命值
        /// </summary>
        public float CurrentHealth { get; private set; }
        
        /// <summary>
        /// 是否存活
        /// </summary>
        public bool IsAlive => CurrentHealth > 0;
        
        /// <summary>
        /// 角色位置（逻辑坐标）
        /// </summary>
        public Vector3Logic Position { get; set; }
        
        /// <summary>
        /// 角色朝向
        /// </summary>
        public Vector3Logic Forward { get; set; }
        
        #endregion
        
        #region 组件系统 - 体现组合模式
        
        /// <summary>
        /// 组件容器 - 体现组合优于继承的设计思想
        /// </summary>
        private readonly Dictionary<Type, ICharacterComponent> _components;
        
        #endregion
        
        #region 事件系统 - 用于逻辑层与表现层的解耦通信
        
        /// <summary>
        /// 生命值变化事件
        /// </summary>
        public event Action<float, float> OnHealthChanged; // (oldHealth, newHealth)
        
        /// <summary>
        /// 角色死亡事件
        /// </summary>
        public event Action OnCharacterDied;
        
        /// <summary>
        /// 位置变化事件
        /// </summary>
        public event Action<Vector3Logic> OnPositionChanged;
        
        /// <summary>
        /// 朝向变化事件
        /// </summary>
        public event Action<Vector3Logic> OnForwardChanged;
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="characterId">角色ID</param>
        /// <param name="characterName">角色名称</param>
        /// <param name="maxHealth">最大生命值</param>
        protected GameCharacterLogic(string characterId, string characterName, float maxHealth)
        {
            CharacterId = characterId;
            CharacterName = characterName;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            Position = Vector3Logic.Zero;
            Forward = Vector3Logic.Forward;
            
            _components = new Dictionary<Type, ICharacterComponent>();
        }
        
        #endregion
        
        #region 抽象方法 - 子类必须实现的特定行为
        
        /// <summary>
        /// 角色类型 - 抽象属性，子类必须实现
        /// 体现：抽象类强制子类实现特定行为
        /// </summary>
        public abstract CharacterType GetCharacterType();
        
        /// <summary>
        /// 角色初始化 - 抽象方法，子类实现特定的初始化逻辑
        /// </summary>
        public abstract void Initialize();
        
        /// <summary>
        /// 角色更新逻辑 - 抽象方法，子类实现特定的更新逻辑
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public abstract void Update(float deltaTime);
        
        #endregion
        
        #region 虚方法 - 提供默认实现，子类可选择重写
        
        /// <summary>
        /// 受到伤害 - 虚方法，提供默认实现，子类可重写
        /// 体现：模板方法模式
        /// </summary>
        /// <param name="damage">伤害值</param>
        /// <param name="attacker">攻击者</param>
        /// <returns>实际受到的伤害</returns>
        public virtual float TakeDamage(float damage, GameCharacterLogic attacker)
        {
            if (!IsAlive) return 0f;
            
            // 调用受伤前的处理（子类可重写）
            damage = OnBeforeTakeDamage(damage, attacker);
            
            float oldHealth = CurrentHealth;
            CurrentHealth = Math.Max(0, CurrentHealth - damage);
            
            // 触发生命值变化事件
            OnHealthChanged?.Invoke(oldHealth, CurrentHealth);
            
            // 调用受伤后的处理（子类可重写）
            OnAfterTakeDamage(damage, attacker);
            
            // 检查是否死亡
            if (!IsAlive)
            {
                OnDeath();
            }
            
            return damage;
        }
        
        /// <summary>
        /// 治疗 - 虚方法
        /// </summary>
        /// <param name="healAmount">治疗量</param>
        /// <returns>实际治疗量</returns>
        public virtual float Heal(float healAmount)
        {
            if (!IsAlive) return 0f;
            
            float oldHealth = CurrentHealth;
            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + healAmount);
            float actualHeal = CurrentHealth - oldHealth;
            
            if (actualHeal > 0)
            {
                OnHealthChanged?.Invoke(oldHealth, CurrentHealth);
            }
            
            return actualHeal;
        }
        
        /// <summary>
        /// 受伤前处理 - 虚方法，子类可重写实现特殊逻辑（如护甲减伤）
        /// </summary>
        /// <param name="damage">原始伤害</param>
        /// <param name="attacker">攻击者</param>
        /// <returns>处理后的伤害</returns>
        protected virtual float OnBeforeTakeDamage(float damage, GameCharacterLogic attacker)
        {
            return damage;
        }
        
        /// <summary>
        /// 受伤后处理 - 虚方法，子类可重写实现特殊逻辑（如反击）
        /// </summary>
        /// <param name="damage">实际伤害</param>
        /// <param name="attacker">攻击者</param>
        protected virtual void OnAfterTakeDamage(float damage, GameCharacterLogic attacker)
        {
            // 默认空实现
        }
        
        /// <summary>
        /// 死亡处理 - 虚方法，子类可重写
        /// </summary>
        protected virtual void OnDeath()
        {
            OnCharacterDied?.Invoke();
        }
        
        #endregion
        
        #region 组件管理 - 体现组合模式
        
        /// <summary>
        /// 添加组件 - 体现组合模式
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="component">组件实例</param>
        public void AddComponent<T>(T component) where T : class, ICharacterComponent
        {
            Type componentType = typeof(T);
            if (_components.ContainsKey(componentType))
            {
                throw new InvalidOperationException($"组件 {componentType.Name} 已存在");
            }
            
            _components[componentType] = component;
            component.Initialize(this);
        }
        
        /// <summary>
        /// 获取组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>组件实例，如果不存在返回null</returns>
        public T GetComponent<T>() where T : class, ICharacterComponent
        {
            Type componentType = typeof(T);
            return _components.ContainsKey(componentType) ? _components[componentType] as T : null;
        }
        
        /// <summary>
        /// 检查是否拥有组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>是否拥有该组件</returns>
        public bool HasComponent<T>() where T : class, ICharacterComponent
        {
            return _components.ContainsKey(typeof(T));
        }
        
        /// <summary>
        /// 移除组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>是否成功移除</returns>
        public bool RemoveComponent<T>() where T : class, ICharacterComponent
        {
            Type componentType = typeof(T);
            if (_components.ContainsKey(componentType))
            {
                _components[componentType].Cleanup();
                _components.Remove(componentType);
                return true;
            }
            return false;
        }
        
        #endregion
        
        #region 位置和朝向管理
        
        /// <summary>
        /// 设置位置
        /// </summary>
        /// <param name="newPosition">新位置</param>
        public void SetPosition(Vector3Logic newPosition)
        {
            if (Position != newPosition)
            {
                Position = newPosition;
                OnPositionChanged?.Invoke(newPosition);
            }
        }
        
        /// <summary>
        /// 设置朝向
        /// </summary>
        /// <param name="newForward">新朝向</param>
        public void SetForward(Vector3Logic newForward)
        {
            if (Forward != newForward)
            {
                Forward = newForward;
                OnForwardChanged?.Invoke(newForward);
            }
        }
        
        #endregion
        
        #region 清理资源
        
        /// <summary>
        /// 清理资源 - 虚方法，子类可重写
        /// </summary>
        public virtual void Cleanup()
        {
            // 清理所有组件
            foreach (var component in _components.Values)
            {
                component.Cleanup();
            }
            _components.Clear();
            
            // 清理事件订阅
            OnHealthChanged = null;
            OnCharacterDied = null;
            OnPositionChanged = null;
            OnForwardChanged = null;
        }
        
        #endregion
    }
}