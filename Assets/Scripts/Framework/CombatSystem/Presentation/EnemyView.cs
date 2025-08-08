using UnityEngine;
using Framework.CombatSystem.Entities;
using Framework.CombatSystem.Core;

namespace Framework.CombatSystem.Presentation
{
    /// <summary>
    /// 敌人表现层类
    /// 体现：继承关系 - 继承自GameCharacterView
    /// 体现：多态性 - 重写基类的抽象方法和虚方法
    /// 体现：组合模式 - 包含多个表现组件
    /// 负责敌人特有的表现逻辑，如AI状态显示、巡逻路径可视化等
    /// </summary>
    public class EnemyView : GameCharacterView
    {
        #region 字段
        
        /// <summary>
        /// 敌人逻辑引用（类型安全）
        /// </summary>
        private EnemyLogic _enemyLogic;
        
        /// <summary>
        /// AI状态显示UI
        /// </summary>
        [Header("敌人特有组件")]
        [SerializeField] private GameObject _aiStateIndicator;
        
        /// <summary>
        /// 视野范围显示
        /// </summary>
        [SerializeField] private GameObject _visionRangeIndicator;
        
        /// <summary>
        /// 追击范围显示
        /// </summary>
        [SerializeField] private GameObject _chaseRangeIndicator;
        
        /// <summary>
        /// 巡逻路径线渲染器
        /// </summary>
        [SerializeField] private LineRenderer _patrolPathRenderer;
        
        /// <summary>
        /// 警戒状态特效
        /// </summary>
        [SerializeField] private ParticleSystem _alertEffect;
        
        /// <summary>
        /// 攻击状态特效
        /// </summary>
        [SerializeField] private ParticleSystem _attackEffect;
        
        /// <summary>
        /// 死亡特效
        /// </summary>
        [SerializeField] private ParticleSystem _deathEffect;
        
        /// <summary>
        /// 敌人等级显示文本
        /// </summary>
        [SerializeField] private TMPro.TextMeshPro _levelText;
        
        /// <summary>
        /// AI状态显示文本
        /// </summary>
        [SerializeField] private TMPro.TextMeshPro _aiStateText;
        
        /// <summary>
        /// 目标指示器
        /// </summary>
        [SerializeField] private GameObject _targetIndicator;
        
        /// <summary>
        /// 敌人材质
        /// </summary>
        [SerializeField] private Renderer _enemyRenderer;
        
        /// <summary>
        /// 原始材质
        /// </summary>
        private Material _originalMaterial;
        
        /// <summary>
        /// 警戒状态材质
        /// </summary>
        [SerializeField] private Material _alertMaterial;
        
        /// <summary>
        /// 攻击状态材质
        /// </summary>
        [SerializeField] private Material _attackMaterial;
        
        /// <summary>
        /// 是否显示调试信息
        /// </summary>
        [Header("调试设置")]
        [SerializeField] private bool _showDebugInfo = false;
        
        /// <summary>
        /// 是否显示巡逻路径
        /// </summary>
        [SerializeField] private bool _showPatrolPath = true;
        
        /// <summary>
        /// 是否显示视野范围
        /// </summary>
        [SerializeField] private bool _showVisionRange = false;
        
        /// <summary>
        /// 当前AI状态缓存
        /// </summary>
        private AIState _currentAIState;
        
        /// <summary>
        /// 当前目标缓存
        /// </summary>
        private GameCharacterLogic _currentTarget;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 敌人逻辑引用（只读）
        /// </summary>
        public EnemyLogic EnemyLogic => _enemyLogic;
        
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
        /// 是否显示巡逻路径
        /// </summary>
        public bool ShowPatrolPath
        {
            get => _showPatrolPath;
            set
            {
                _showPatrolPath = value;
                UpdatePatrolPathDisplay();
            }
        }
        
        /// <summary>
        /// 是否显示视野范围
        /// </summary>
        public bool ShowVisionRange
        {
            get => _showVisionRange;
            set
            {
                _showVisionRange = value;
                UpdateVisionRangeDisplay();
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
            _enemyLogic = characterLogic as EnemyLogic;
            if (_enemyLogic == null)
            {
                Debug.LogError($"[EnemyView] 尝试绑定非敌人逻辑到敌人视图: {characterLogic?.GetType().Name}");
                return;
            }
            
            // 订阅敌人特有事件
            SubscribeToEnemyEvents();
            
            // 初始化敌人特有显示
            InitializeEnemyDisplay();
            
            Debug.Log($"[EnemyView] 敌人视图绑定完成: {_enemyLogic.CharacterName}");
        }
        
        /// <summary>
        /// 更新视图显示（重写抽象方法）
        /// </summary>
        protected override void UpdateView()
        {
            if (_enemyLogic == null) return;
            
            // 调用基类更新
            base.UpdateView();
            
            // 更新敌人特有显示
            UpdateEnemySpecificDisplay();
        }
        
        /// <summary>
        /// 获取视图配置（重写虚方法）
        /// </summary>
        /// <returns>视图配置信息</returns>
        public override string GetViewConfig()
        {
            string baseConfig = base.GetViewConfig();
            
            if (_enemyLogic == null) return baseConfig;
            
            return baseConfig + $"\n敌人特有配置:\n" +
                   $"等级: {_enemyLogic.Level}\n" +
                   $"AI状态: {_enemyLogic.CurrentAIState}\n" +
                   $"视野范围: {_enemyLogic.VisionRange:F1}\n" +
                   $"追击范围: {_enemyLogic.ChaseRange:F1}\n" +
                   $"当前目标: {(_enemyLogic.CurrentTarget?.CharacterName ?? "无")}\n" +
                   $"巡逻路径点数: {(_enemyLogic.PatrolPoints?.Count ?? 0)}";
        }
        
        #endregion
        
        #region 事件订阅
        
        /// <summary>
        /// 订阅敌人特有事件
        /// </summary>
        private void SubscribeToEnemyEvents()
        {
            if (_enemyLogic == null) return;
            
            // 订阅AI状态变化事件
            _enemyLogic.OnAIStateChanged += OnAIStateChanged;
            
            // 订阅目标变化事件
            _enemyLogic.OnTargetChanged += OnTargetChanged;
            
            // 订阅等级变化事件
            _enemyLogic.OnLevelChanged += OnLevelChanged;
            
            // 订阅巡逻路径变化事件
            _enemyLogic.OnPatrolPathChanged += OnPatrolPathChanged;
        }
        
        /// <summary>
        /// 取消订阅敌人特有事件
        /// </summary>
        private void UnsubscribeFromEnemyEvents()
        {
            if (_enemyLogic == null) return;
            
            _enemyLogic.OnAIStateChanged -= OnAIStateChanged;
            _enemyLogic.OnTargetChanged -= OnTargetChanged;
            _enemyLogic.OnLevelChanged -= OnLevelChanged;
            _enemyLogic.OnPatrolPathChanged -= OnPatrolPathChanged;
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
            
            // 更新材质
            UpdateMaterialByState(newState);
            
            Debug.Log($"[EnemyView] AI状态变化: {oldState} -> {newState}");
        }
        
        /// <summary>
        /// 处理目标变化事件
        /// </summary>
        /// <param name="oldTarget">旧目标</param>
        /// <param name="newTarget">新目标</param>
        private void OnTargetChanged(GameCharacterLogic oldTarget, GameCharacterLogic newTarget)
        {
            _currentTarget = newTarget;
            
            // 更新目标指示器
            UpdateTargetIndicator(newTarget);
            
            Debug.Log($"[EnemyView] 目标变化: {oldTarget?.CharacterName ?? "无"} -> {newTarget?.CharacterName ?? "无"}");
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
            
            Debug.Log($"[EnemyView] 等级变化: {oldLevel} -> {newLevel}");
        }
        
        /// <summary>
        /// 处理巡逻路径变化事件
        /// </summary>
        private void OnPatrolPathChanged()
        {
            // 更新巡逻路径显示
            UpdatePatrolPathDisplay();
            
            Debug.Log("[EnemyView] 巡逻路径已更新");
        }
        
        #endregion
        
        #region 初始化方法
        
        /// <summary>
        /// 初始化敌人显示
        /// </summary>
        private void InitializeEnemyDisplay()
        {
            if (_enemyLogic == null) return;
            
            // 保存原始材质
            if (_enemyRenderer != null)
            {
                _originalMaterial = _enemyRenderer.material;
            }
            
            // 初始化各种显示
            UpdateLevelDisplay(_enemyLogic.Level);
            UpdateAIStateDisplay(_enemyLogic.CurrentAIState);
            UpdateTargetIndicator(_enemyLogic.CurrentTarget);
            UpdatePatrolPathDisplay();
            UpdateVisionRangeDisplay();
            UpdateDebugDisplay();
            
            // 设置初始状态
            _currentAIState = _enemyLogic.CurrentAIState;
            _currentTarget = _enemyLogic.CurrentTarget;
        }
        
        #endregion
        
        #region 显示更新方法
        
        /// <summary>
        /// 更新敌人特有显示
        /// </summary>
        private void UpdateEnemySpecificDisplay()
        {
            // 更新AI状态指示器位置
            UpdateAIStateIndicatorPosition();
            
            // 更新目标指示器位置
            UpdateTargetIndicatorPosition();
            
            // 更新视野范围显示
            UpdateVisionRangePosition();
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
            
            if (_aiStateIndicator != null)
            {
                _aiStateIndicator.SetActive(_showDebugInfo);
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
        /// 更新目标指示器
        /// </summary>
        /// <param name="target">目标</param>
        private void UpdateTargetIndicator(GameCharacterLogic target)
        {
            if (_targetIndicator != null)
            {
                _targetIndicator.SetActive(target != null && _showDebugInfo);
            }
        }
        
        /// <summary>
        /// 更新巡逻路径显示
        /// </summary>
        private void UpdatePatrolPathDisplay()
        {
            if (_patrolPathRenderer == null || _enemyLogic == null) return;
            
            _patrolPathRenderer.enabled = _showPatrolPath && _enemyLogic.PatrolPoints != null && _enemyLogic.PatrolPoints.Count > 1;
            
            if (_patrolPathRenderer.enabled)
            {
                var patrolPoints = _enemyLogic.PatrolPoints;
                _patrolPathRenderer.positionCount = patrolPoints.Count + 1; // +1 for loop back
                
                for (int i = 0; i < patrolPoints.Count; i++)
                {
                    var logicPos = patrolPoints[i];
                    _patrolPathRenderer.SetPosition(i, new Vector3(logicPos.X, logicPos.Y, logicPos.Z));
                }
                
                // 闭合路径
                var firstPoint = patrolPoints[0];
                _patrolPathRenderer.SetPosition(patrolPoints.Count, new Vector3(firstPoint.X, firstPoint.Y, firstPoint.Z));
            }
        }
        
        /// <summary>
        /// 更新视野范围显示
        /// </summary>
        private void UpdateVisionRangeDisplay()
        {
            if (_visionRangeIndicator != null)
            {
                _visionRangeIndicator.SetActive(_showVisionRange);
                
                if (_showVisionRange && _enemyLogic != null)
                {
                    // 设置视野范围大小
                    float visionRange = _enemyLogic.VisionRange;
                    _visionRangeIndicator.transform.localScale = Vector3.one * visionRange * 2f;
                }
            }
            
            if (_chaseRangeIndicator != null)
            {
                _chaseRangeIndicator.SetActive(_showVisionRange);
                
                if (_showVisionRange && _enemyLogic != null)
                {
                    // 设置追击范围大小
                    float chaseRange = _enemyLogic.ChaseRange;
                    _chaseRangeIndicator.transform.localScale = Vector3.one * chaseRange * 2f;
                }
            }
        }
        
        /// <summary>
        /// 更新调试显示
        /// </summary>
        private void UpdateDebugDisplay()
        {
            if (_aiStateIndicator != null)
            {
                _aiStateIndicator.SetActive(_showDebugInfo);
            }
            
            if (_targetIndicator != null)
            {
                _targetIndicator.SetActive(_showDebugInfo && _currentTarget != null);
            }
        }
        
        #endregion
        
        #region 位置更新方法
        
        /// <summary>
        /// 更新AI状态指示器位置
        /// </summary>
        private void UpdateAIStateIndicatorPosition()
        {
            if (_aiStateIndicator != null && _aiStateIndicator.activeInHierarchy)
            {
                // 将指示器放在角色头顶
                Vector3 headPosition = transform.position + Vector3.up * 2f;
                _aiStateIndicator.transform.position = headPosition;
                
                // 让指示器始终面向摄像机
                if (Camera.main != null)
                {
                    _aiStateIndicator.transform.LookAt(Camera.main.transform);
                }
            }
        }
        
        /// <summary>
        /// 更新目标指示器位置
        /// </summary>
        private void UpdateTargetIndicatorPosition()
        {
            if (_targetIndicator != null && _targetIndicator.activeInHierarchy && _currentTarget != null)
            {
                // 指向目标的方向
                Vector3 targetPos = new Vector3(_currentTarget.Position.X, _currentTarget.Position.Y, _currentTarget.Position.Z);
                Vector3 direction = (targetPos - transform.position).normalized;
                
                // 将指示器放在角色前方
                Vector3 indicatorPosition = transform.position + direction * 1.5f + Vector3.up * 1f;
                _targetIndicator.transform.position = indicatorPosition;
                _targetIndicator.transform.LookAt(targetPos);
            }
        }
        
        /// <summary>
        /// 更新视野范围位置
        /// </summary>
        private void UpdateVisionRangePosition()
        {
            if (_visionRangeIndicator != null && _visionRangeIndicator.activeInHierarchy)
            {
                _visionRangeIndicator.transform.position = transform.position;
            }
            
            if (_chaseRangeIndicator != null && _chaseRangeIndicator.activeInHierarchy)
            {
                _chaseRangeIndicator.transform.position = transform.position;
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
                case AIState.Chase:
                case AIState.Attack:
                    if (_alertEffect != null && !_alertEffect.isPlaying)
                    {
                        _alertEffect.Play();
                    }
                    break;
                    
                case AIState.Dead:
                    if (_deathEffect != null)
                    {
                        _deathEffect.Play();
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 播放升级特效
        /// </summary>
        private void PlayLevelUpEffect()
        {
            // 这里可以播放升级特效
            Debug.Log("[EnemyView] 播放升级特效");
        }
        
        #endregion
        
        #region 材质更新方法
        
        /// <summary>
        /// 根据状态更新材质
        /// </summary>
        /// <param name="aiState">AI状态</param>
        private void UpdateMaterialByState(AIState aiState)
        {
            if (_enemyRenderer == null) return;
            
            Material targetMaterial = _originalMaterial;
            
            switch (aiState)
            {
                case AIState.Chase:
                    targetMaterial = _alertMaterial ?? _originalMaterial;
                    break;
                    
                case AIState.Attack:
                    targetMaterial = _attackMaterial ?? _originalMaterial;
                    break;
                    
                default:
                    targetMaterial = _originalMaterial;
                    break;
            }
            
            _enemyRenderer.material = targetMaterial;
        }
        
        #endregion
        
        #region 辅助方法
        
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
            // 取消订阅敌人特有事件
            UnsubscribeFromEnemyEvents();
            
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
            if (_enemyLogic == null || !_showDebugInfo) return;
            
            Vector3 position = transform.position;
            
            // 绘制视野范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, _enemyLogic.VisionRange);
            
            // 绘制追击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, _enemyLogic.ChaseRange);
            
            // 绘制巡逻路径
            if (_enemyLogic.PatrolPoints != null && _enemyLogic.PatrolPoints.Count > 1)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < _enemyLogic.PatrolPoints.Count; i++)
                {
                    var point = _enemyLogic.PatrolPoints[i];
                    Vector3 worldPoint = new Vector3(point.X, point.Y, point.Z);
                    
                    // 绘制巡逻点
                    Gizmos.DrawWireSphere(worldPoint, 0.5f);
                    
                    // 绘制连线
                    if (i < _enemyLogic.PatrolPoints.Count - 1)
                    {
                        var nextPoint = _enemyLogic.PatrolPoints[i + 1];
                        Vector3 nextWorldPoint = new Vector3(nextPoint.X, nextPoint.Y, nextPoint.Z);
                        Gizmos.DrawLine(worldPoint, nextWorldPoint);
                    }
                    else
                    {
                        // 连接最后一个点到第一个点
                        var firstPoint = _enemyLogic.PatrolPoints[0];
                        Vector3 firstWorldPoint = new Vector3(firstPoint.X, firstPoint.Y, firstPoint.Z);
                        Gizmos.DrawLine(worldPoint, firstWorldPoint);
                    }
                }
            }
            
            // 绘制目标连线
            if (_currentTarget != null)
            {
                Gizmos.color = Color.magenta;
                Vector3 targetPos = new Vector3(_currentTarget.Position.X, _currentTarget.Position.Y, _currentTarget.Position.Z);
                Gizmos.DrawLine(position, targetPos);
            }
        }
        
        #endregion
    }
}