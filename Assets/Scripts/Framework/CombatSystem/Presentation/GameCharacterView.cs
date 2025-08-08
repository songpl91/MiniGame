using UnityEngine;
using Framework.CombatSystem.Core;

namespace Framework.CombatSystem.Presentation
{
    /// <summary>
    /// 游戏角色表现层基类
    /// 体现：抽象类的使用 - 定义表现层的通用行为
    /// 体现：逻辑与表现分离 - 持有逻辑层引用，负责表现层更新
    /// 负责将逻辑层的数据同步到Unity表现层
    /// </summary>
    public abstract class GameCharacterView : MonoBehaviour
    {
        #region 字段
        
        /// <summary>
        /// 角色逻辑引用
        /// 体现：组合模式 - View "Has-A" Logic
        /// </summary>
        protected GameCharacterLogic _characterLogic;
        
        /// <summary>
        /// 角色名称显示
        /// </summary>
        [SerializeField] protected GameObject _nameDisplay;
        
        /// <summary>
        /// 生命值条
        /// </summary>
        [SerializeField] protected GameObject _healthBar;
        
        /// <summary>
        /// 生命值条填充
        /// </summary>
        [SerializeField] protected UnityEngine.UI.Image _healthBarFill;
        
        /// <summary>
        /// 角色模型
        /// </summary>
        [SerializeField] protected GameObject _characterModel;
        
        /// <summary>
        /// 动画控制器
        /// </summary>
        [SerializeField] protected Animator _animator;
        
        /// <summary>
        /// 音效播放器
        /// </summary>
        [SerializeField] protected AudioSource _audioSource;
        
        /// <summary>
        /// 特效挂载点
        /// </summary>
        [SerializeField] protected Transform _effectMount;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        protected bool _isInitialized;
        
        /// <summary>
        /// 上一帧的位置（用于检测移动）
        /// </summary>
        protected Vector3 _lastPosition;
        
        /// <summary>
        /// 上一帧的生命值（用于检测生命值变化）
        /// </summary>
        protected float _lastHealth;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 角色逻辑引用
        /// </summary>
        public GameCharacterLogic CharacterLogic => _characterLogic;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        #endregion
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Start方法
        /// </summary>
        protected virtual void Start()
        {
            // 如果还未绑定逻辑，尝试自动查找
            if (_characterLogic == null)
            {
                // 这里可以添加自动查找逻辑的代码
                // 例如：通过名称或ID在CombatManager中查找
            }
        }
        
        /// <summary>
        /// Unity Update方法
        /// </summary>
        protected virtual void Update()
        {
            if (!_isInitialized || _characterLogic == null) return;
            
            // 同步位置
            SyncPosition();
            
            // 同步生命值显示
            SyncHealthDisplay();
            
            // 同步动画状态
            SyncAnimationState();
            
            // 执行子类特定的更新逻辑
            UpdateView();
        }
        
        /// <summary>
        /// Unity OnDestroy方法
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 取消事件订阅
            UnsubscribeFromLogicEvents();
        }
        
        #endregion
        
        #region 初始化方法
        
        /// <summary>
        /// 绑定角色逻辑
        /// </summary>
        /// <param name="characterLogic">角色逻辑实例</param>
        public virtual void BindCharacterLogic(GameCharacterLogic characterLogic)
        {
            if (characterLogic == null)
            {
                Debug.LogError($"[GameCharacterView] 尝试绑定空的角色逻辑到 {gameObject.name}");
                return;
            }
            
            // 如果已经绑定了其他逻辑，先取消订阅
            if (_characterLogic != null)
            {
                UnsubscribeFromLogicEvents();
            }
            
            _characterLogic = characterLogic;
            
            // 订阅逻辑层事件
            SubscribeToLogicEvents();
            
            // 初始化表现层
            InitializeView();
            
            _isInitialized = true;
            
            Debug.Log($"[GameCharacterView] 成功绑定角色逻辑: {_characterLogic.CharacterName} 到视图 {gameObject.name}");
        }
        
        /// <summary>
        /// 初始化视图
        /// 子类可以重写此方法来实现特定的初始化逻辑
        /// </summary>
        protected virtual void InitializeView()
        {
            if (_characterLogic == null) return;
            
            // 设置初始位置
            SyncPosition();
            
            // 设置初始生命值显示
            SyncHealthDisplay();
            
            // 设置角色名称
            SetCharacterName(_characterLogic.CharacterName);
            
            // 记录初始状态
            _lastPosition = transform.position;
            _lastHealth = _characterLogic.CurrentHealth;
            
            // 执行子类特定的初始化
            OnViewInitialized();
        }
        
        /// <summary>
        /// 视图初始化完成回调
        /// 子类可以重写此方法来实现特定的初始化逻辑
        /// </summary>
        protected virtual void OnViewInitialized()
        {
            // 子类实现
        }
        
        #endregion
        
        #region 事件订阅
        
        /// <summary>
        /// 订阅逻辑层事件
        /// </summary>
        protected virtual void SubscribeToLogicEvents()
        {
            if (_characterLogic == null) return;
            
            _characterLogic.OnHealthChanged += OnHealthChanged;
            _characterLogic.OnDied += OnCharacterDied;
            _characterLogic.OnPositionChanged += OnPositionChanged;
        }
        
        /// <summary>
        /// 取消订阅逻辑层事件
        /// </summary>
        protected virtual void UnsubscribeFromLogicEvents()
        {
            if (_characterLogic == null) return;
            
            _characterLogic.OnHealthChanged -= OnHealthChanged;
            _characterLogic.OnDied -= OnCharacterDied;
            _characterLogic.OnPositionChanged -= OnPositionChanged;
        }
        
        #endregion
        
        #region 事件处理方法
        
        /// <summary>
        /// 生命值变化处理
        /// </summary>
        /// <param name="character">角色</param>
        /// <param name="oldHealth">旧生命值</param>
        /// <param name="newHealth">新生命值</param>
        protected virtual void OnHealthChanged(GameCharacterLogic character, float oldHealth, float newHealth)
        {
            // 播放受伤或治疗特效
            if (newHealth < oldHealth)
            {
                // 受伤
                PlayDamageEffect(oldHealth - newHealth);
                PlayDamageSound();
            }
            else if (newHealth > oldHealth)
            {
                // 治疗
                PlayHealEffect(newHealth - oldHealth);
                PlayHealSound();
            }
            
            // 更新生命值显示
            UpdateHealthBar(newHealth / character.MaxHealth);
        }
        
        /// <summary>
        /// 角色死亡处理
        /// </summary>
        /// <param name="character">死亡的角色</param>
        protected virtual void OnCharacterDied(GameCharacterLogic character)
        {
            // 播放死亡动画
            PlayDeathAnimation();
            
            // 播放死亡音效
            PlayDeathSound();
            
            // 播放死亡特效
            PlayDeathEffect();
            
            // 禁用某些组件
            DisableOnDeath();
        }
        
        /// <summary>
        /// 位置变化处理
        /// </summary>
        /// <param name="character">角色</param>
        /// <param name="oldPosition">旧位置</param>
        /// <param name="newPosition">新位置</param>
        protected virtual void OnPositionChanged(GameCharacterLogic character, Vector3Logic oldPosition, Vector3Logic newPosition)
        {
            // 位置同步在Update中处理，这里可以添加其他逻辑
            // 例如：播放移动音效、粒子效果等
        }
        
        #endregion
        
        #region 同步方法
        
        /// <summary>
        /// 同步位置
        /// </summary>
        protected virtual void SyncPosition()
        {
            if (_characterLogic == null) return;
            
            // 将逻辑层的位置同步到Unity Transform
            Vector3 targetPosition = new Vector3(
                _characterLogic.Position.X,
                _characterLogic.Position.Y,
                _characterLogic.Position.Z
            );
            
            // 使用插值平滑移动
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
            
            // 检测移动状态变化
            bool isMoving = Vector3.Distance(transform.position, _lastPosition) > 0.01f;
            if (isMoving)
            {
                OnMovementDetected();
            }
            
            _lastPosition = transform.position;
        }
        
        /// <summary>
        /// 同步生命值显示
        /// </summary>
        protected virtual void SyncHealthDisplay()
        {
            if (_characterLogic == null) return;
            
            float currentHealth = _characterLogic.CurrentHealth;
            
            // 检测生命值变化
            if (Mathf.Abs(currentHealth - _lastHealth) > 0.01f)
            {
                UpdateHealthBar(currentHealth / _characterLogic.MaxHealth);
                _lastHealth = currentHealth;
            }
        }
        
        /// <summary>
        /// 同步动画状态
        /// </summary>
        protected virtual void SyncAnimationState()
        {
            if (_animator == null || _characterLogic == null) return;
            
            // 设置基础动画参数
            _animator.SetBool("IsAlive", _characterLogic.IsAlive);
            _animator.SetFloat("HealthPercent", _characterLogic.CurrentHealth / _characterLogic.MaxHealth);
            
            // 检测移动状态
            var movableComponent = _characterLogic.GetComponent<IMovable>();
            if (movableComponent != null)
            {
                _animator.SetBool("IsMoving", movableComponent.IsMoving);
                _animator.SetFloat("MoveSpeed", movableComponent.MoveSpeed);
            }
            
            // 检测攻击状态
            var attackComponent = _characterLogic.GetComponent<IAttacker>();
            if (attackComponent != null)
            {
                _animator.SetBool("CanAttack", attackComponent.CanAttack);
            }
        }
        
        #endregion
        
        #region 表现层效果方法
        
        /// <summary>
        /// 设置角色名称显示
        /// </summary>
        /// <param name="characterName">角色名称</param>
        protected virtual void SetCharacterName(string characterName)
        {
            if (_nameDisplay != null)
            {
                var textComponent = _nameDisplay.GetComponent<UnityEngine.UI.Text>();
                if (textComponent != null)
                {
                    textComponent.text = characterName;
                }
                
                var tmpComponent = _nameDisplay.GetComponent<TMPro.TextMeshProUGUI>();
                if (tmpComponent != null)
                {
                    tmpComponent.text = characterName;
                }
            }
        }
        
        /// <summary>
        /// 更新生命值条
        /// </summary>
        /// <param name="healthPercent">生命值百分比</param>
        protected virtual void UpdateHealthBar(float healthPercent)
        {
            if (_healthBarFill != null)
            {
                _healthBarFill.fillAmount = Mathf.Clamp01(healthPercent);
                
                // 根据生命值百分比改变颜色
                if (healthPercent > 0.6f)
                {
                    _healthBarFill.color = Color.green;
                }
                else if (healthPercent > 0.3f)
                {
                    _healthBarFill.color = Color.yellow;
                }
                else
                {
                    _healthBarFill.color = Color.red;
                }
            }
        }
        
        /// <summary>
        /// 播放受伤特效
        /// </summary>
        /// <param name="damage">伤害值</param>
        protected virtual void PlayDamageEffect(float damage)
        {
            // 这里可以实例化受伤特效预制体
            // 例如：血液飞溅、伤害数字等
            Debug.Log($"[{gameObject.name}] 播放受伤特效，伤害: {damage}");
        }
        
        /// <summary>
        /// 播放治疗特效
        /// </summary>
        /// <param name="healAmount">治疗量</param>
        protected virtual void PlayHealEffect(float healAmount)
        {
            // 这里可以实例化治疗特效预制体
            // 例如：绿色光芒、治疗数字等
            Debug.Log($"[{gameObject.name}] 播放治疗特效，治疗量: {healAmount}");
        }
        
        /// <summary>
        /// 播放死亡特效
        /// </summary>
        protected virtual void PlayDeathEffect()
        {
            // 这里可以实例化死亡特效预制体
            // 例如：爆炸效果、消散效果等
            Debug.Log($"[{gameObject.name}] 播放死亡特效");
        }
        
        /// <summary>
        /// 播放死亡动画
        /// </summary>
        protected virtual void PlayDeathAnimation()
        {
            if (_animator != null)
            {
                _animator.SetTrigger("Die");
            }
        }
        
        /// <summary>
        /// 播放受伤音效
        /// </summary>
        protected virtual void PlayDamageSound()
        {
            if (_audioSource != null)
            {
                // 这里可以播放受伤音效
                // _audioSource.PlayOneShot(damageSound);
            }
        }
        
        /// <summary>
        /// 播放治疗音效
        /// </summary>
        protected virtual void PlayHealSound()
        {
            if (_audioSource != null)
            {
                // 这里可以播放治疗音效
                // _audioSource.PlayOneShot(healSound);
            }
        }
        
        /// <summary>
        /// 播放死亡音效
        /// </summary>
        protected virtual void PlayDeathSound()
        {
            if (_audioSource != null)
            {
                // 这里可以播放死亡音效
                // _audioSource.PlayOneShot(deathSound);
            }
        }
        
        /// <summary>
        /// 移动检测回调
        /// </summary>
        protected virtual void OnMovementDetected()
        {
            // 子类可以重写此方法来处理移动相关的表现
            // 例如：播放脚步声、粒子效果等
        }
        
        /// <summary>
        /// 死亡时禁用组件
        /// </summary>
        protected virtual void DisableOnDeath()
        {
            // 禁用碰撞器
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            
            // 可以添加其他需要禁用的组件
        }
        
        #endregion
        
        #region 抽象方法
        
        /// <summary>
        /// 更新视图 - 子类特定的更新逻辑
        /// 体现：抽象方法的使用 - 强制子类实现特定行为
        /// </summary>
        protected abstract void UpdateView();
        
        #endregion
        
        #region 虚方法
        
        /// <summary>
        /// 获取角色类型特定的表现配置
        /// 体现：虚方法的使用 - 提供默认实现，子类可以重写
        /// </summary>
        /// <returns>表现配置</returns>
        protected virtual CharacterViewConfig GetViewConfig()
        {
            return new CharacterViewConfig
            {
                HealthBarOffset = Vector3.up * 2f,
                NameDisplayOffset = Vector3.up * 2.5f,
                EffectScale = 1f
            };
        }
        
        #endregion
        
        #region 公共接口方法
        
        /// <summary>
        /// 手动触发位置同步
        /// </summary>
        public void ForcePositionSync()
        {
            SyncPosition();
        }
        
        /// <summary>
        /// 手动触发生命值同步
        /// </summary>
        public void ForceHealthSync()
        {
            SyncHealthDisplay();
        }
        
        /// <summary>
        /// 设置视图可见性
        /// </summary>
        /// <param name="visible">是否可见</param>
        public virtual void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
        
        /// <summary>
        /// 获取角色在屏幕上的位置
        /// </summary>
        /// <returns>屏幕位置</returns>
        public Vector3 GetScreenPosition()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                return mainCamera.WorldToScreenPoint(transform.position);
            }
            return Vector3.zero;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 角色视图配置
    /// </summary>
    [System.Serializable]
    public class CharacterViewConfig
    {
        /// <summary>
        /// 生命值条偏移
        /// </summary>
        public Vector3 HealthBarOffset;
        
        /// <summary>
        /// 名称显示偏移
        /// </summary>
        public Vector3 NameDisplayOffset;
        
        /// <summary>
        /// 特效缩放
        /// </summary>
        public float EffectScale;
    }
}