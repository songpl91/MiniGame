using UnityEngine;
using Framework.CombatSystem.Entities;
using Framework.CombatSystem.Core;

namespace Framework.CombatSystem.Presentation
{
    /// <summary>
    /// 怪物表现层类
    /// 体现：继承关系 - 继承自GameCharacterView
    /// 体现：多态性 - 重写基类的抽象方法和虚方法
    /// 体现：组合模式 - 包含多个表现组件
    /// 负责怪物特有的表现逻辑，如怪物类型显示、愤怒状态特效、特殊能力可视化等
    /// </summary>
    public class MonsterView : GameCharacterView
    {
        #region 字段
        
        /// <summary>
        /// 怪物逻辑引用（类型安全）
        /// </summary>
        private MonsterLogic _monsterLogic;
        
        /// <summary>
        /// 怪物类型显示UI
        /// </summary>
        [Header("怪物特有组件")]
        [SerializeField] private GameObject _monsterTypeIndicator;
        
        /// <summary>
        /// 愤怒状态特效
        /// </summary>
        [SerializeField] private ParticleSystem _rageEffect;
        
        /// <summary>
        /// 特殊能力充能特效
        /// </summary>
        [SerializeField] private ParticleSystem _specialAbilityChargeEffect;
        
        /// <summary>
        /// 特殊能力释放特效
        /// </summary>
        [SerializeField] private ParticleSystem _specialAbilityReleaseEffect;
        
        /// <summary>
        /// 领域范围指示器
        /// </summary>
        [SerializeField] private GameObject _territoryRangeIndicator;
        
        /// <summary>
        /// 怪物等级显示文本
        /// </summary>
        [SerializeField] private TMPro.TextMeshPro _levelText;
        
        /// <summary>
        /// 怪物类型显示文本
        /// </summary>
        [SerializeField] private TMPro.TextMeshPro _monsterTypeText;
        
        /// <summary>
        /// AI状态显示文本
        /// </summary>
        [SerializeField] private TMPro.TextMeshPro _aiStateText;
        
        /// <summary>
        /// 愤怒状态显示文本
        /// </summary>
        [SerializeField] private TMPro.TextMeshPro _rageStateText;
        
        /// <summary>
        /// 特殊能力冷却显示
        /// </summary>
        [SerializeField] private UnityEngine.UI.Image _specialAbilityCooldownBar;
        
        /// <summary>
        /// 怪物渲染器
        /// </summary>
        [SerializeField] private Renderer _monsterRenderer;
        
        /// <summary>
        /// 原始材质
        /// </summary>
        private Material _originalMaterial;
        
        /// <summary>
        /// 愤怒状态材质
        /// </summary>
        [SerializeField] private Material _rageMaterial;
        
        /// <summary>
        /// 特殊能力准备材质
        /// </summary>
        [SerializeField] private Material _specialAbilityReadyMaterial;
        
        /// <summary>
        /// 怪物类型颜色映射
        /// </summary>
        [Header("怪物类型配置")]
        [SerializeField] private MonsterTypeColorConfig[] _monsterTypeColors;
        
        /// <summary>
        /// 是否显示调试信息
        /// </summary>
        [Header("调试设置")]
        [SerializeField] private bool _showDebugInfo = false;
        
        /// <summary>
        /// 是否显示领域范围
        /// </summary>
        [SerializeField] private bool _showTerritoryRange = false;
        
        /// <summary>
        /// 是否显示特殊能力冷却
        /// </summary>
        [SerializeField] private bool _showSpecialAbilityCooldown = true;
        
        /// <summary>
        /// 当前AI状态缓存
        /// </summary>
        private AIState _currentAIState;
        
        /// <summary>
        /// 当前愤怒状态缓存
        /// </summary>
        private bool _currentRageState;
        
        /// <summary>
        /// 当前怪物类型缓存
        /// </summary>
        private MonsterType _currentMonsterType;
        
        /// <summary>
        /// 特殊能力冷却协程
        /// </summary>
        private Coroutine _specialAbilityCooldownCoroutine;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 怪物逻辑引用（只读）
        /// </summary>
        public MonsterLogic MonsterLogic => _monsterLogic;
        
        /// <summary>
        /// 是否显示调试信息
        /// </summary>
        public bool ShowDebugInfo
        {
            get => _showDebugInfo;
            set
            {
                _showDebugInfo = value;
                UpdateDebugDisplay();
            }
        }
        
        /// <summary>
        /// 是否显示领域范围
        /// </summary>
        public bool ShowTerritoryRange
        {
            get => _showTerritoryRange;
            set
            {
                _showTerritoryRange = value;
                UpdateTerritoryRangeDisplay();
            }
        }
        
        /// <summary>
        /// 是否显示特殊能力冷却
        /// </summary>
        public bool ShowSpecialAbilityCooldown
        {
            get => _showSpecialAbilityCooldown;
            set
            {
                _showSpecialAbilityCooldown = value;
                UpdateSpecialAbilityCooldownDisplay();
            }
        }
        
        #endregion
        
        #region 重写基类方法
        
        /// <summary>
        /// 绑定角色逻辑（重写基类方法，提供类型安全）
        /// </summary>
        /// <param name="characterLogic">角色逻辑</param>
        public override void BindCharacterLogic(GameCharacterLogic characterLogic)
        {
            // 调用基类方法
            base.BindCharacterLogic(characterLogic);
            
            // 类型安全转换
            _monsterLogic = characterLogic as MonsterLogic;
            if (_monsterLogic == null)
            {
                Debug.LogError($"[MonsterView] 尝试绑定非怪物逻辑到怪物视图: {characterLogic?.GetType().Name}");
                return;
            }
            
            // 订阅怪物特有事件
            SubscribeToMonsterEvents();
            
            // 初始化怪物特有显示
            InitializeMonsterDisplay();
            
            Debug.Log($"[MonsterView] 怪物视图绑定完成: {_monsterLogic.CharacterName}");
        }
        
        /// <summary>
        /// 更新视图显示（重写抽象方法）
        /// </summary>
        protected override void UpdateView()
        {
            if (_monsterLogic == null) return;
            
            // 调用基类更新
            base.UpdateView();
            
            // 更新怪物特有显示
            UpdateMonsterSpecificDisplay();
        }
        
        /// <summary>
        /// 获取视图配置（重写虚方法）
        /// </summary>
        /// <returns>视图配置信息</returns>
        public override string GetViewConfig()
        {
            string baseConfig = base.GetViewConfig();
            
            if (_monsterLogic == null) return baseConfig;
            
            return baseConfig + $"\n怪物特有配置:\n" +
                   $"等级: {_monsterLogic.Level}\n" +
                   $"怪物类型: {_monsterLogic.MonsterType}\n" +
                   $"AI状态: {_monsterLogic.CurrentAIState}\n" +
                   $"愤怒状态: {(_monsterLogic.IsInRage ? "是" : "否")}\n" +
                   $"视野范围: {_monsterLogic.VisionRange:F1}\n" +
                   $"追击范围: {_monsterLogic.ChaseRange:F1}\n" +
                   $"领域范围: {_monsterLogic.TerritoryRange:F1}\n" +
                   $"特殊能力冷却: {_monsterLogic.SpecialAbilityCooldownTime:F1}s\n" +
                   $"当前目标: {(_monsterLogic.CurrentTarget?.CharacterName ?? "无")}";
        }
        
        #endregion
        
        #region 事件订阅
        
        /// <summary>
        /// 订阅怪物特有事件
        /// </summary>
        private void SubscribeToMonsterEvents()
        {
            if (_monsterLogic == null) return;
            
            // 订阅AI状态变化事件
            _monsterLogic.OnAIStateChanged += OnAIStateChanged;
            
            // 订阅目标变化事件
            _monsterLogic.OnTargetChanged += OnTargetChanged;
            
            // 订阅等级变化事件
            _monsterLogic.OnLevelChanged += OnLevelChanged;
            
            // 订阅愤怒状态变化事件
            _monsterLogic.OnRageStateChanged += OnRageStateChanged;
            
            // 订阅特殊能力事件
            _monsterLogic.OnSpecialAbilityUsed += OnSpecialAbilityUsed;
            _monsterLogic.OnSpecialAbilityReady += OnSpecialAbilityReady;
        }
        
        /// <summary>
        /// 取消订阅怪物特有事件
        /// </summary>
        private void UnsubscribeFromMonsterEvents()
        {
            if (_monsterLogic == null) return;
            
            _monsterLogic.OnAIStateChanged -= OnAIStateChanged;
            _monsterLogic.OnTargetChanged -= OnTargetChanged;
            _monsterLogic.OnLevelChanged -= OnLevelChanged;
            _monsterLogic.OnRageStateChanged -= OnRageStateChanged;
            _monsterLogic.OnSpecialAbilityUsed -= OnSpecialAbilityUsed;
            _monsterLogic.OnSpecialAbilityReady -= OnSpecialAbilityReady;
        }
        
        #endregion
        
        #region 事件处理方法
        
        /// <summary>
        /// 处理AI状态变化事件
        /// </summary>
        /// <param name="oldState">旧状态</param>
        /// <param name="newState">新状态</param>
        private void OnAIStateChanged(AIState oldState, AIState newState)
        {
            _currentAIState = newState;
            
            // 更新AI状态显示
            UpdateAIStateDisplay(newState);
            
            // 播放状态切换特效
            PlayStateChangeEffect(oldState, newState);
            
            Debug.Log($"[MonsterView] AI状态变化: {oldState} -> {newState}");
        }
        
        /// <summary>
        /// 处理目标变化事件
        /// </summary>
        /// <param name="oldTarget">旧目标</param>
        /// <param name="newTarget">新目标</param>
        private void OnTargetChanged(GameCharacterLogic oldTarget, GameCharacterLogic newTarget)
        {
            Debug.Log($"[MonsterView] 目标变化: {oldTarget?.CharacterName ?? "无"} -> {newTarget?.CharacterName ?? "无"}");
        }
        
        /// <summary>
        /// 处理等级变化事件
        /// </summary>
        /// <param name="oldLevel">旧等级</param>
        /// <param name="newLevel">新等级</param>
        private void OnLevelChanged(int oldLevel, int newLevel)
        {
            // 更新等级显示
            UpdateLevelDisplay(newLevel);
            
            // 播放升级特效
            if (newLevel > oldLevel)
            {
                PlayLevelUpEffect();
            }
            
            Debug.Log($"[MonsterView] 等级变化: {oldLevel} -> {newLevel}");
        }
        
        /// <summary>
        /// 处理愤怒状态变化事件
        /// </summary>
        /// <param name="isInRage">是否愤怒</param>
        private void OnRageStateChanged(bool isInRage)
        {
            _currentRageState = isInRage;
            
            // 更新愤怒状态显示
            UpdateRageStateDisplay(isInRage);
            
            // 播放愤怒特效
            PlayRageEffect(isInRage);
            
            // 更新材质
            UpdateMaterialByRageState(isInRage);
            
            Debug.Log($"[MonsterView] 愤怒状态变化: {isInRage}");
        }
        
        /// <summary>
        /// 处理特殊能力使用事件
        /// </summary>
        /// <param name="abilityName">能力名称</param>
        private void OnSpecialAbilityUsed(string abilityName)
        {
            // 播放特殊能力释放特效
            PlaySpecialAbilityReleaseEffect();
            
            // 开始冷却显示
            StartSpecialAbilityCooldownDisplay();
            
            Debug.Log($"[MonsterView] 特殊能力使用: {abilityName}");
        }
        
        /// <summary>
        /// 处理特殊能力准备就绪事件
        /// </summary>
        private void OnSpecialAbilityReady()
        {
            // 播放准备就绪特效
            PlaySpecialAbilityReadyEffect();
            
            // 更新材质
            UpdateMaterialBySpecialAbilityState(true);
            
            Debug.Log("[MonsterView] 特殊能力准备就绪");
        }
        
        #endregion
        
        #region 初始化方法
        
        /// <summary>
        /// 初始化怪物显示
        /// </summary>
        private void InitializeMonsterDisplay()
        {
            if (_monsterLogic == null) return;
            
            // 保存原始材质
            if (_monsterRenderer != null)
            {
                _originalMaterial = _monsterRenderer.material;
            }
            
            // 初始化各种显示
            UpdateLevelDisplay(_monsterLogic.Level);
            UpdateMonsterTypeDisplay(_monsterLogic.MonsterType);
            UpdateAIStateDisplay(_monsterLogic.CurrentAIState);
            UpdateRageStateDisplay(_monsterLogic.IsInRage);
            UpdateTerritoryRangeDisplay();
            UpdateSpecialAbilityCooldownDisplay();
            UpdateDebugDisplay();
            
            // 设置初始状态
            _currentAIState = _monsterLogic.CurrentAIState;
            _currentRageState = _monsterLogic.IsInRage;
            _currentMonsterType = _monsterLogic.MonsterType;
        }
        
        #endregion
        
        #region 显示更新方法
        
        /// <summary>
        /// 更新怪物特有显示
        /// </summary>
        private void UpdateMonsterSpecificDisplay()
        {
            // 更新各种指示器位置
            UpdateIndicatorPositions();
            
            // 更新特殊能力冷却进度
            UpdateSpecialAbilityCooldownProgress();
        }
        
        /// <summary>
        /// 更新怪物类型显示
        /// </summary>
        /// <param name="monsterType">怪物类型</param>
        private void UpdateMonsterTypeDisplay(MonsterType monsterType)
        {
            if (_monsterTypeText != null)
            {
                _monsterTypeText.text = GetMonsterTypeDisplayText(monsterType);
                _monsterTypeText.color = GetMonsterTypeColor(monsterType);
            }
            
            if (_monsterTypeIndicator != null)
            {
                _monsterTypeIndicator.SetActive(_showDebugInfo);
            }
        }
        
        /// <summary>
        /// 更新AI状态显示
        /// </summary>
        /// <param name="aiState">AI状态</param>
        private void UpdateAIStateDisplay(AIState aiState)
        {
            if (_aiStateText != null)
            {
                _aiStateText.text = GetAIStateDisplayText(aiState);
                _aiStateText.color = GetAIStateColor(aiState);
            }
        }
        
        /// <summary>
        /// 更新等级显示
        /// </summary>
        /// <param name="level">等级</param>
        private void UpdateLevelDisplay(int level)
        {
            if (_levelText != null)
            {
                _levelText.text = $"Lv.{level}";
            }
        }
        
        /// <summary>
        /// 更新愤怒状态显示
        /// </summary>
        /// <param name="isInRage">是否愤怒</param>
        private void UpdateRageStateDisplay(bool isInRage)
        {
            if (_rageStateText != null)
            {
                _rageStateText.text = isInRage ? "愤怒" : "";
                _rageStateText.color = Color.red;
                _rageStateText.gameObject.SetActive(isInRage && _showDebugInfo);
            }
        }
        
        /// <summary>
        /// 更新领域范围显示
        /// </summary>
        private void UpdateTerritoryRangeDisplay()
        {
            if (_territoryRangeIndicator != null)
            {
                _territoryRangeIndicator.SetActive(_showTerritoryRange);
                
                if (_showTerritoryRange && _monsterLogic != null)
                {
                    // 设置领域范围大小
                    float territoryRange = _monsterLogic.TerritoryRange;
                    _territoryRangeIndicator.transform.localScale = Vector3.one * territoryRange * 2f;
                }
            }
        }
        
        /// <summary>
        /// 更新特殊能力冷却显示
        /// </summary>
        private void UpdateSpecialAbilityCooldownDisplay()
        {
            if (_specialAbilityCooldownBar != null)
            {
                _specialAbilityCooldownBar.gameObject.SetActive(_showSpecialAbilityCooldown);
            }
        }
        
        /// <summary>
        /// 更新调试显示
        /// </summary>
        private void UpdateDebugDisplay()
        {
            if (_monsterTypeIndicator != null)
            {
                _monsterTypeIndicator.SetActive(_showDebugInfo);
            }
            
            if (_rageStateText != null)
            {
                _rageStateText.gameObject.SetActive(_showDebugInfo && _currentRageState);
            }
        }
        
        #endregion
        
        #region 位置更新方法
        
        /// <summary>
        /// 更新指示器位置
        /// </summary>
        private void UpdateIndicatorPositions()
        {
            // 更新怪物类型指示器位置
            if (_monsterTypeIndicator != null && _monsterTypeIndicator.activeInHierarchy)
            {
                Vector3 headPosition = transform.position + Vector3.up * 2.5f;
                _monsterTypeIndicator.transform.position = headPosition;
                
                if (Camera.main != null)
                {
                    _monsterTypeIndicator.transform.LookAt(Camera.main.transform);
                }
            }
            
            // 更新领域范围指示器位置
            if (_territoryRangeIndicator != null && _territoryRangeIndicator.activeInHierarchy)
            {
                _territoryRangeIndicator.transform.position = transform.position;
            }
        }
        
        /// <summary>
        /// 更新特殊能力冷却进度
        /// </summary>
        private void UpdateSpecialAbilityCooldownProgress()
        {
            if (_specialAbilityCooldownBar != null && _specialAbilityCooldownBar.gameObject.activeInHierarchy && _monsterLogic != null)
            {
                float cooldownProgress = _monsterLogic.GetSpecialAbilityCooldownProgress();
                _specialAbilityCooldownBar.fillAmount = 1f - cooldownProgress; // 反向显示，满血表示冷却完成
            }
        }
        
        #endregion
        
        #region 特效播放方法
        
        /// <summary>
        /// 播放状态切换特效
        /// </summary>
        /// <param name="oldState">旧状态</param>
        /// <param name="newState">新状态</param>
        private void PlayStateChangeEffect(AIState oldState, AIState newState)
        {
            switch (newState)
            {
                case AIState.Attack:
                    // 攻击状态可以播放一些特效
                    break;
                    
                case AIState.Dead:
                    // 死亡状态播放死亡特效
                    PlayDeathEffect();
                    break;
            }
        }
        
        /// <summary>
        /// 播放愤怒特效
        /// </summary>
        /// <param name="isInRage">是否愤怒</param>
        private void PlayRageEffect(bool isInRage)
        {
            if (_rageEffect != null)
            {
                if (isInRage && !_rageEffect.isPlaying)
                {
                    _rageEffect.Play();
                }
                else if (!isInRage && _rageEffect.isPlaying)
                {
                    _rageEffect.Stop();
                }
            }
        }
        
        /// <summary>
        /// 播放特殊能力释放特效
        /// </summary>
        private void PlaySpecialAbilityReleaseEffect()
        {
            if (_specialAbilityReleaseEffect != null)
            {
                _specialAbilityReleaseEffect.Play();
            }
        }
        
        /// <summary>
        /// 播放特殊能力准备就绪特效
        /// </summary>
        private void PlaySpecialAbilityReadyEffect()
        {
            if (_specialAbilityChargeEffect != null)
            {
                _specialAbilityChargeEffect.Play();
            }
        }
        
        /// <summary>
        /// 播放升级特效
        /// </summary>
        private void PlayLevelUpEffect()
        {
            Debug.Log("[MonsterView] 播放升级特效");
        }
        
        /// <summary>
        /// 播放死亡特效
        /// </summary>
        private void PlayDeathEffect()
        {
            Debug.Log("[MonsterView] 播放死亡特效");
        }
        
        #endregion
        
        #region 材质更新方法
        
        /// <summary>
        /// 根据愤怒状态更新材质
        /// </summary>
        /// <param name="isInRage">是否愤怒</param>
        private void UpdateMaterialByRageState(bool isInRage)
        {
            if (_monsterRenderer == null) return;
            
            Material targetMaterial = isInRage ? (_rageMaterial ?? _originalMaterial) : _originalMaterial;
            _monsterRenderer.material = targetMaterial;
        }
        
        /// <summary>
        /// 根据特殊能力状态更新材质
        /// </summary>
        /// <param name="isReady">特殊能力是否准备就绪</param>
        private void UpdateMaterialBySpecialAbilityState(bool isReady)
        {
            if (_monsterRenderer == null || _currentRageState) return; // 愤怒状态优先级更高
            
            Material targetMaterial = isReady ? (_specialAbilityReadyMaterial ?? _originalMaterial) : _originalMaterial;
            _monsterRenderer.material = targetMaterial;
        }
        
        #endregion
        
        #region 冷却显示方法
        
        /// <summary>
        /// 开始特殊能力冷却显示
        /// </summary>
        private void StartSpecialAbilityCooldownDisplay()
        {
            if (_specialAbilityCooldownCoroutine != null)
            {
                StopCoroutine(_specialAbilityCooldownCoroutine);
            }
            
            _specialAbilityCooldownCoroutine = StartCoroutine(SpecialAbilityCooldownCoroutine());
        }
        
        /// <summary>
        /// 特殊能力冷却显示协程
        /// </summary>
        private System.Collections.IEnumerator SpecialAbilityCooldownCoroutine()
        {
            while (_monsterLogic != null && _monsterLogic.GetSpecialAbilityCooldownProgress() > 0f)
            {
                UpdateSpecialAbilityCooldownProgress();
                yield return null;
            }
            
            // 冷却完成，更新材质
            UpdateMaterialBySpecialAbilityState(true);
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 获取怪物类型显示文本
        /// </summary>
        /// <param name="monsterType">怪物类型</param>
        /// <returns>显示文本</returns>
        private string GetMonsterTypeDisplayText(MonsterType monsterType)
        {
            switch (monsterType)
            {
                case MonsterType.Normal: return "普通";
                case MonsterType.Elite: return "精英";
                case MonsterType.Boss: return "首领";
                case MonsterType.MiniBoss: return "小首领";
                case MonsterType.Rare: return "稀有";
                case MonsterType.Legendary: return "传说";
                default: return "未知";
            }
        }
        
        /// <summary>
        /// 获取怪物类型颜色
        /// </summary>
        /// <param name="monsterType">怪物类型</param>
        /// <returns>类型颜色</returns>
        private Color GetMonsterTypeColor(MonsterType monsterType)
        {
            if (_monsterTypeColors != null)
            {
                foreach (var config in _monsterTypeColors)
                {
                    if (config.MonsterType == monsterType)
                    {
                        return config.Color;
                    }
                }
            }
            
            // 默认颜色
            switch (monsterType)
            {
                case MonsterType.Normal: return Color.white;
                case MonsterType.Elite: return Color.blue;
                case MonsterType.Boss: return Color.red;
                case MonsterType.MiniBoss: return Color.yellow;
                case MonsterType.Rare: return Color.green;
                case MonsterType.Legendary: return Color.magenta;
                default: return Color.gray;
            }
        }
        
        /// <summary>
        /// 获取AI状态显示文本
        /// </summary>
        /// <param name="aiState">AI状态</param>
        /// <returns>显示文本</returns>
        private string GetAIStateDisplayText(AIState aiState)
        {
            switch (aiState)
            {
                case AIState.Idle: return "待机";
                case AIState.Patrol: return "巡逻";
                case AIState.Chase: return "追击";
                case AIState.Attack: return "攻击";
                case AIState.Return: return "返回";
                case AIState.Dead: return "死亡";
                default: return "未知";
            }
        }
        
        /// <summary>
        /// 获取AI状态颜色
        /// </summary>
        /// <param name="aiState">AI状态</param>
        /// <returns>状态颜色</returns>
        private Color GetAIStateColor(AIState aiState)
        {
            switch (aiState)
            {
                case AIState.Idle: return Color.white;
                case AIState.Patrol: return Color.green;
                case AIState.Chase: return Color.yellow;
                case AIState.Attack: return Color.red;
                case AIState.Return: return Color.blue;
                case AIState.Dead: return Color.gray;
                default: return Color.white;
            }
        }
        
        #endregion
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity OnDestroy方法
        /// </summary>
        protected override void OnDestroy()
        {
            // 停止协程
            if (_specialAbilityCooldownCoroutine != null)
            {
                StopCoroutine(_specialAbilityCooldownCoroutine);
            }
            
            // 取消订阅怪物特有事件
            UnsubscribeFromMonsterEvents();
            
            // 调用基类清理
            base.OnDestroy();
        }
        
        #endregion
        
        #region 调试方法
        
        /// <summary>
        /// 在Scene视图中绘制调试信息
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (_monsterLogic == null || !_showDebugInfo) return;
            
            Vector3 position = transform.position;
            
            // 绘制视野范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, _monsterLogic.VisionRange);
            
            // 绘制追击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, _monsterLogic.ChaseRange);
            
            // 绘制领域范围
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(position, _monsterLogic.TerritoryRange);
            
            // 绘制目标连线
            if (_monsterLogic.CurrentTarget != null)
            {
                Gizmos.color = Color.magenta;
                Vector3 targetPos = new Vector3(_monsterLogic.CurrentTarget.Position.X, 
                                              _monsterLogic.CurrentTarget.Position.Y, 
                                              _monsterLogic.CurrentTarget.Position.Z);
                Gizmos.DrawLine(position, targetPos);
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 怪物类型颜色配置
    /// </summary>
    [System.Serializable]
    public class MonsterTypeColorConfig
    {
        /// <summary>
        /// 怪物类型
        /// </summary>
        public MonsterType MonsterType;
        
        /// <summary>
        /// 对应颜色
        /// </summary>
        public Color Color = Color.white;
    }
}