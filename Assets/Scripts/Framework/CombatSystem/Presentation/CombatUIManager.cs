using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Framework.CombatSystem.Core;
using Framework.CombatSystem.Entities;

namespace Framework.CombatSystem.Presentation
{
    /// <summary>
    /// 战斗UI管理器
    /// 体现：单一职责原则 - 专门负责战斗相关的UI管理
    /// 体现：组合模式 - 管理多个UI组件
    /// 体现：观察者模式 - 监听战斗事件并更新UI
    /// 负责管理所有战斗相关的UI显示，如生命值条、技能冷却、战斗状态等
    /// </summary>
    public class CombatUIManager : MonoBehaviour
    {
        #region 字段
        
        /// <summary>
        /// 战斗管理器引用
        /// </summary>
        private CombatManager _combatManager;
        
        /// <summary>
        /// 当前绑定的玩家逻辑
        /// </summary>
        private PlayerLogic _currentPlayerLogic;
        
        /// <summary>
        /// 玩家生命值条
        /// </summary>
        [Header("玩家UI组件")]
        [SerializeField] private Slider _playerHealthBar;
        
        /// <summary>
        /// 玩家生命值文本
        /// </summary>
        [SerializeField] private TextMeshProUGUI _playerHealthText;
        
        /// <summary>
        /// 玩家等级文本
        /// </summary>
        [SerializeField] private TextMeshProUGUI _playerLevelText;
        
        /// <summary>
        /// 玩家经验值条
        /// </summary>
        [SerializeField] private Slider _playerExpBar;
        
        /// <summary>
        /// 玩家经验值文本
        /// </summary>
        [SerializeField] private TextMeshProUGUI _playerExpText;
        
        /// <summary>
        /// 技能冷却显示容器
        /// </summary>
        [SerializeField] private Transform _skillCooldownContainer;
        
        /// <summary>
        /// 技能冷却UI预制体
        /// </summary>
        [SerializeField] private GameObject _skillCooldownPrefab;
        
        /// <summary>
        /// 目标信息面板
        /// </summary>
        [Header("目标信息UI")]
        [SerializeField] private GameObject _targetInfoPanel;
        
        /// <summary>
        /// 目标生命值条
        /// </summary>
        [SerializeField] private Slider _targetHealthBar;
        
        /// <summary>
        /// 目标名称文本
        /// </summary>
        [SerializeField] private TextMeshProUGUI _targetNameText;
        
        /// <summary>
        /// 目标等级文本
        /// </summary>
        [SerializeField] private TextMeshProUGUI _targetLevelText;
        
        /// <summary>
        /// 战斗状态面板
        /// </summary>
        [Header("战斗状态UI")]
        [SerializeField] private GameObject _combatStatusPanel;
        
        /// <summary>
        /// 战斗状态文本
        /// </summary>
        [SerializeField] private TextMeshProUGUI _combatStatusText;
        
        /// <summary>
        /// 敌人数量文本
        /// </summary>
        [SerializeField] private TextMeshProUGUI _enemyCountText;
        
        /// <summary>
        /// 怪物数量文本
        /// </summary>
        [SerializeField] private TextMeshProUGUI _monsterCountText;
        
        /// <summary>
        /// 消息显示面板
        /// </summary>
        [Header("消息显示UI")]
        [SerializeField] private GameObject _messagePanel;
        
        /// <summary>
        /// 消息文本
        /// </summary>
        [SerializeField] private TextMeshProUGUI _messageText;
        
        /// <summary>
        /// 消息显示时间
        /// </summary>
        [SerializeField] private float _messageDisplayTime = 3f;
        
        /// <summary>
        /// 调试信息面板
        /// </summary>
        [Header("调试UI")]
        [SerializeField] private GameObject _debugPanel;
        
        /// <summary>
        /// 调试信息文本
        /// </summary>
        [SerializeField] private TextMeshProUGUI _debugText;
        
        /// <summary>
        /// 是否显示调试信息
        /// </summary>
        [SerializeField] private bool _showDebugInfo = false;
        
        /// <summary>
        /// 技能冷却UI字典
        /// </summary>
        private System.Collections.Generic.Dictionary<string, SkillCooldownUI> _skillCooldownUIs;
        
        /// <summary>
        /// 当前目标角色
        /// </summary>
        private GameCharacterLogic _currentTarget;
        
        /// <summary>
        /// 消息显示协程
        /// </summary>
        private Coroutine _messageCoroutine;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _isInitialized;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
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
        /// 当前绑定的玩家逻辑
        /// </summary>
        public PlayerLogic CurrentPlayerLogic => _currentPlayerLogic;
        
        #endregion
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法
        /// </summary>
        private void Awake()
        {
            // 初始化技能冷却UI字典
            _skillCooldownUIs = new System.Collections.Generic.Dictionary<string, SkillCooldownUI>();
            
            // 初始化UI状态
            InitializeUIState();
        }
        
        /// <summary>
        /// Unity Update方法
        /// </summary>
        private void Update()
        {
            if (!_isInitialized) return;
            
            // 更新玩家UI
            UpdatePlayerUI();
            
            // 更新目标UI
            UpdateTargetUI();
            
            // 更新战斗状态UI
            UpdateCombatStatusUI();
            
            // 更新调试信息
            if (_showDebugInfo)
            {
                UpdateDebugInfo();
            }
        }
        
        #endregion
        
        #region 初始化方法
        
        /// <summary>
        /// 初始化战斗UI管理器
        /// </summary>
        /// <param name="combatManager">战斗管理器</param>
        public void Initialize(CombatManager combatManager)
        {
            if (combatManager == null)
            {
                Debug.LogError("[CombatUIManager] 尝试用空的战斗管理器初始化UI管理器");
                return;
            }
            
            _combatManager = combatManager;
            
            // 订阅战斗管理器事件
            SubscribeToCombatManagerEvents();
            
            _isInitialized = true;
            
            Debug.Log("[CombatUIManager] 战斗UI管理器初始化完成");
        }
        
        /// <summary>
        /// 初始化UI状态
        /// </summary>
        private void InitializeUIState()
        {
            // 隐藏所有面板
            if (_targetInfoPanel != null) _targetInfoPanel.SetActive(false);
            if (_combatStatusPanel != null) _combatStatusPanel.SetActive(false);
            if (_messagePanel != null) _messagePanel.SetActive(false);
            if (_debugPanel != null) _debugPanel.SetActive(_showDebugInfo);
            
            // 重置UI值
            ResetPlayerUI();
            ResetTargetUI();
            ResetCombatStatusUI();
        }
        
        #endregion
        
        #region 玩家绑定方法
        
        /// <summary>
        /// 绑定玩家逻辑
        /// </summary>
        /// <param name="playerLogic">玩家逻辑</param>
        public void BindPlayerLogic(PlayerLogic playerLogic)
        {
            // 取消之前的绑定
            if (_currentPlayerLogic != null)
            {
                UnsubscribeFromPlayerEvents();
            }
            
            _currentPlayerLogic = playerLogic;
            
            if (_currentPlayerLogic != null)
            {
                // 订阅玩家事件
                SubscribeToPlayerEvents();
                
                // 初始化玩家UI
                InitializePlayerUI();
                
                Debug.Log($"[CombatUIManager] 绑定玩家逻辑: {_currentPlayerLogic.CharacterName}");
            }
        }
        
        #endregion
        
        #region 事件订阅
        
        /// <summary>
        /// 订阅战斗管理器事件
        /// </summary>
        private void SubscribeToCombatManagerEvents()
        {
            if (_combatManager == null) return;
            
            _combatManager.OnCombatStarted += OnCombatStarted;
            _combatManager.OnCombatEnded += OnCombatEnded;
            _combatManager.OnCharacterAdded += OnCharacterAdded;
            _combatManager.OnCharacterRemoved += OnCharacterRemoved;
        }
        
        /// <summary>
        /// 取消订阅战斗管理器事件
        /// </summary>
        private void UnsubscribeFromCombatManagerEvents()
        {
            if (_combatManager == null) return;
            
            _combatManager.OnCombatStarted -= OnCombatStarted;
            _combatManager.OnCombatEnded -= OnCombatEnded;
            _combatManager.OnCharacterAdded -= OnCharacterAdded;
            _combatManager.OnCharacterRemoved -= OnCharacterRemoved;
        }
        
        /// <summary>
        /// 订阅玩家事件
        /// </summary>
        private void SubscribeToPlayerEvents()
        {
            if (_currentPlayerLogic == null) return;
            
            _currentPlayerLogic.OnHealthChanged += OnPlayerHealthChanged;
            _currentPlayerLogic.OnLevelChanged += OnPlayerLevelChanged;
            _currentPlayerLogic.OnExperienceChanged += OnPlayerExperienceChanged;
            _currentPlayerLogic.OnSkillUsed += OnPlayerSkillUsed;
        }
        
        /// <summary>
        /// 取消订阅玩家事件
        /// </summary>
        private void UnsubscribeFromPlayerEvents()
        {
            if (_currentPlayerLogic == null) return;
            
            _currentPlayerLogic.OnHealthChanged -= OnPlayerHealthChanged;
            _currentPlayerLogic.OnLevelChanged -= OnPlayerLevelChanged;
            _currentPlayerLogic.OnExperienceChanged -= OnPlayerExperienceChanged;
            _currentPlayerLogic.OnSkillUsed -= OnPlayerSkillUsed;
        }
        
        #endregion
        
        #region 事件处理方法
        
        /// <summary>
        /// 处理战斗开始事件
        /// </summary>
        private void OnCombatStarted()
        {
            ShowMessage("战斗开始！");
            
            if (_combatStatusPanel != null)
            {
                _combatStatusPanel.SetActive(true);
            }
            
            Debug.Log("[CombatUIManager] 战斗开始UI更新");
        }
        
        /// <summary>
        /// 处理战斗结束事件
        /// </summary>
        /// <param name="isVictory">是否胜利</param>
        private void OnCombatEnded(bool isVictory)
        {
            string message = isVictory ? "战斗胜利！" : "战斗失败！";
            ShowMessage(message);
            
            // 隐藏目标信息
            if (_targetInfoPanel != null)
            {
                _targetInfoPanel.SetActive(false);
            }
            
            Debug.Log($"[CombatUIManager] 战斗结束UI更新: {message}");
        }
        
        /// <summary>
        /// 处理角色添加事件
        /// </summary>
        /// <param name="character">添加的角色</param>
        private void OnCharacterAdded(GameCharacterLogic character)
        {
            Debug.Log($"[CombatUIManager] 角色添加: {character.CharacterName}");
        }
        
        /// <summary>
        /// 处理角色移除事件
        /// </summary>
        /// <param name="character">移除的角色</param>
        private void OnCharacterRemoved(GameCharacterLogic character)
        {
            // 如果移除的是当前目标，清除目标UI
            if (_currentTarget == character)
            {
                SetTarget(null);
            }
            
            Debug.Log($"[CombatUIManager] 角色移除: {character.CharacterName}");
        }
        
        /// <summary>
        /// 处理玩家生命值变化事件
        /// </summary>
        /// <param name="oldHealth">旧生命值</param>
        /// <param name="newHealth">新生命值</param>
        private void OnPlayerHealthChanged(float oldHealth, float newHealth)
        {
            UpdatePlayerHealthUI();
        }
        
        /// <summary>
        /// 处理玩家等级变化事件
        /// </summary>
        /// <param name="oldLevel">旧等级</param>
        /// <param name="newLevel">新等级</param>
        private void OnPlayerLevelChanged(int oldLevel, int newLevel)
        {
            UpdatePlayerLevelUI();
            
            if (newLevel > oldLevel)
            {
                ShowMessage($"升级到 {newLevel} 级！");
            }
        }
        
        /// <summary>
        /// 处理玩家经验值变化事件
        /// </summary>
        /// <param name="oldExp">旧经验值</param>
        /// <param name="newExp">新经验值</param>
        private void OnPlayerExperienceChanged(int oldExp, int newExp)
        {
            UpdatePlayerExpUI();
        }
        
        /// <summary>
        /// 处理玩家技能使用事件
        /// </summary>
        /// <param name="skillName">技能名称</param>
        private void OnPlayerSkillUsed(string skillName)
        {
            // 显示技能冷却
            ShowSkillCooldown(skillName);
        }
        
        #endregion
        
        #region UI更新方法
        
        /// <summary>
        /// 更新玩家UI
        /// </summary>
        private void UpdatePlayerUI()
        {
            if (_currentPlayerLogic == null) return;
            
            UpdatePlayerHealthUI();
            UpdatePlayerLevelUI();
            UpdatePlayerExpUI();
        }
        
        /// <summary>
        /// 更新玩家生命值UI
        /// </summary>
        private void UpdatePlayerHealthUI()
        {
            if (_currentPlayerLogic == null) return;
            
            float healthPercent = _currentPlayerLogic.CurrentHealth / _currentPlayerLogic.MaxHealth;
            
            if (_playerHealthBar != null)
            {
                _playerHealthBar.value = healthPercent;
            }
            
            if (_playerHealthText != null)
            {
                _playerHealthText.text = $"{_currentPlayerLogic.CurrentHealth:F0}/{_currentPlayerLogic.MaxHealth:F0}";
            }
        }
        
        /// <summary>
        /// 更新玩家等级UI
        /// </summary>
        private void UpdatePlayerLevelUI()
        {
            if (_currentPlayerLogic == null) return;
            
            if (_playerLevelText != null)
            {
                _playerLevelText.text = $"Lv.{_currentPlayerLogic.Level}";
            }
        }
        
        /// <summary>
        /// 更新玩家经验值UI
        /// </summary>
        private void UpdatePlayerExpUI()
        {
            if (_currentPlayerLogic == null) return;
            
            float expPercent = (float)_currentPlayerLogic.CurrentExperience / _currentPlayerLogic.ExperienceToNextLevel;
            
            if (_playerExpBar != null)
            {
                _playerExpBar.value = expPercent;
            }
            
            if (_playerExpText != null)
            {
                _playerExpText.text = $"{_currentPlayerLogic.CurrentExperience}/{_currentPlayerLogic.ExperienceToNextLevel}";
            }
        }
        
        /// <summary>
        /// 更新目标UI
        /// </summary>
        private void UpdateTargetUI()
        {
            if (_currentTarget == null || !_currentTarget.IsAlive)
            {
                if (_targetInfoPanel != null && _targetInfoPanel.activeInHierarchy)
                {
                    _targetInfoPanel.SetActive(false);
                }
                return;
            }
            
            if (_targetInfoPanel != null && !_targetInfoPanel.activeInHierarchy)
            {
                _targetInfoPanel.SetActive(true);
            }
            
            // 更新目标生命值
            float healthPercent = _currentTarget.CurrentHealth / _currentTarget.MaxHealth;
            if (_targetHealthBar != null)
            {
                _targetHealthBar.value = healthPercent;
            }
            
            // 更新目标名称
            if (_targetNameText != null)
            {
                _targetNameText.text = _currentTarget.CharacterName;
            }
            
            // 更新目标等级（如果有的话）
            if (_targetLevelText != null)
            {
                string levelText = "";
                if (_currentTarget is EnemyLogic enemyLogic)
                {
                    levelText = $"Lv.{enemyLogic.Level}";
                }
                else if (_currentTarget is MonsterLogic monsterLogic)
                {
                    levelText = $"Lv.{monsterLogic.Level} ({GetMonsterTypeDisplayText(monsterLogic.MonsterType)})";
                }
                _targetLevelText.text = levelText;
            }
        }
        
        /// <summary>
        /// 更新战斗状态UI
        /// </summary>
        private void UpdateCombatStatusUI()
        {
            if (_combatManager == null) return;
            
            if (_combatStatusText != null)
            {
                string statusText = _combatManager.IsInCombat ? "战斗中" : "非战斗状态";
                _combatStatusText.text = statusText;
            }
            
            if (_enemyCountText != null)
            {
                int enemyCount = _combatManager.GetAliveEnemyCount();
                _enemyCountText.text = $"敌人: {enemyCount}";
            }
            
            if (_monsterCountText != null)
            {
                int monsterCount = _combatManager.GetAliveMonsterCount();
                _monsterCountText.text = $"怪物: {monsterCount}";
            }
        }
        
        /// <summary>
        /// 更新调试信息
        /// </summary>
        private void UpdateDebugInfo()
        {
            if (_debugText == null || _combatManager == null) return;
            
            string debugInfo = "=== 战斗调试信息 ===\n";
            debugInfo += $"战斗状态: {(_combatManager.IsInCombat ? "进行中" : "未开始")}\n";
            debugInfo += $"总角色数: {_combatManager.GetAllCharacterCount()}\n";
            debugInfo += $"玩家数: {_combatManager.GetPlayerCount()}\n";
            debugInfo += $"敌人数: {_combatManager.GetAliveEnemyCount()}\n";
            debugInfo += $"怪物数: {_combatManager.GetAliveMonsterCount()}\n";
            
            if (_currentPlayerLogic != null)
            {
                debugInfo += $"\n=== 玩家信息 ===\n";
                debugInfo += $"名称: {_currentPlayerLogic.CharacterName}\n";
                debugInfo += $"等级: {_currentPlayerLogic.Level}\n";
                debugInfo += $"生命值: {_currentPlayerLogic.CurrentHealth:F1}/{_currentPlayerLogic.MaxHealth:F1}\n";
                debugInfo += $"经验值: {_currentPlayerLogic.CurrentExperience}/{_currentPlayerLogic.ExperienceToNextLevel}\n";
                debugInfo += $"位置: {_currentPlayerLogic.Position}\n";
            }
            
            if (_currentTarget != null)
            {
                debugInfo += $"\n=== 目标信息 ===\n";
                debugInfo += $"名称: {_currentTarget.CharacterName}\n";
                debugInfo += $"类型: {_currentTarget.CharacterType}\n";
                debugInfo += $"生命值: {_currentTarget.CurrentHealth:F1}/{_currentTarget.MaxHealth:F1}\n";
                debugInfo += $"位置: {_currentTarget.Position}\n";
            }
            
            _debugText.text = debugInfo;
        }
        
        #endregion
        
        #region UI重置方法
        
        /// <summary>
        /// 重置玩家UI
        /// </summary>
        private void ResetPlayerUI()
        {
            if (_playerHealthBar != null) _playerHealthBar.value = 1f;
            if (_playerHealthText != null) _playerHealthText.text = "0/0";
            if (_playerLevelText != null) _playerLevelText.text = "Lv.1";
            if (_playerExpBar != null) _playerExpBar.value = 0f;
            if (_playerExpText != null) _playerExpText.text = "0/0";
        }
        
        /// <summary>
        /// 重置目标UI
        /// </summary>
        private void ResetTargetUI()
        {
            if (_targetHealthBar != null) _targetHealthBar.value = 1f;
            if (_targetNameText != null) _targetNameText.text = "";
            if (_targetLevelText != null) _targetLevelText.text = "";
        }
        
        /// <summary>
        /// 重置战斗状态UI
        /// </summary>
        private void ResetCombatStatusUI()
        {
            if (_combatStatusText != null) _combatStatusText.text = "非战斗状态";
            if (_enemyCountText != null) _enemyCountText.text = "敌人: 0";
            if (_monsterCountText != null) _monsterCountText.text = "怪物: 0";
        }
        
        #endregion
        
        #region 初始化UI方法
        
        /// <summary>
        /// 初始化玩家UI
        /// </summary>
        private void InitializePlayerUI()
        {
            if (_currentPlayerLogic == null) return;
            
            UpdatePlayerHealthUI();
            UpdatePlayerLevelUI();
            UpdatePlayerExpUI();
        }
        
        #endregion
        
        #region 目标设置方法
        
        /// <summary>
        /// 设置目标
        /// </summary>
        /// <param name="target">目标角色</param>
        public void SetTarget(GameCharacterLogic target)
        {
            _currentTarget = target;
            
            if (_currentTarget == null)
            {
                if (_targetInfoPanel != null)
                {
                    _targetInfoPanel.SetActive(false);
                }
            }
            else
            {
                if (_targetInfoPanel != null)
                {
                    _targetInfoPanel.SetActive(true);
                }
                UpdateTargetUI();
            }
        }
        
        #endregion
        
        #region 技能冷却UI方法
        
        /// <summary>
        /// 显示技能冷却
        /// </summary>
        /// <param name="skillName">技能名称</param>
        private void ShowSkillCooldown(string skillName)
        {
            if (_skillCooldownContainer == null || _skillCooldownPrefab == null) return;
            
            // 如果已经存在该技能的冷却UI，更新它
            if (_skillCooldownUIs.ContainsKey(skillName))
            {
                _skillCooldownUIs[skillName].StartCooldown();
                return;
            }
            
            // 创建新的技能冷却UI
            GameObject cooldownObj = Instantiate(_skillCooldownPrefab, _skillCooldownContainer);
            SkillCooldownUI cooldownUI = cooldownObj.GetComponent<SkillCooldownUI>();
            
            if (cooldownUI != null)
            {
                cooldownUI.Initialize(skillName, 5f); // 假设冷却时间为5秒
                _skillCooldownUIs[skillName] = cooldownUI;
            }
        }
        
        #endregion
        
        #region 消息显示方法
        
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="message">消息内容</param>
        public void ShowMessage(string message)
        {
            if (_messagePanel == null || _messageText == null) return;
            
            _messageText.text = message;
            _messagePanel.SetActive(true);
            
            // 停止之前的协程
            if (_messageCoroutine != null)
            {
                StopCoroutine(_messageCoroutine);
            }
            
            // 开始新的消息显示协程
            _messageCoroutine = StartCoroutine(HideMessageAfterDelay());
        }
        
        /// <summary>
        /// 延迟隐藏消息协程
        /// </summary>
        private System.Collections.IEnumerator HideMessageAfterDelay()
        {
            yield return new WaitForSeconds(_messageDisplayTime);
            
            if (_messagePanel != null)
            {
                _messagePanel.SetActive(false);
            }
        }
        
        #endregion
        
        #region 调试显示方法
        
        /// <summary>
        /// 更新调试显示
        /// </summary>
        private void UpdateDebugDisplay()
        {
            if (_debugPanel != null)
            {
                _debugPanel.SetActive(_showDebugInfo);
            }
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
        
        #endregion
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity OnDestroy方法
        /// </summary>
        private void OnDestroy()
        {
            // 停止协程
            if (_messageCoroutine != null)
            {
                StopCoroutine(_messageCoroutine);
            }
            
            // 取消事件订阅
            UnsubscribeFromCombatManagerEvents();
            UnsubscribeFromPlayerEvents();
        }
        
        #endregion
    }
    
    /// <summary>
    /// 技能冷却UI组件
    /// </summary>
    public class SkillCooldownUI : MonoBehaviour
    {
        /// <summary>
        /// 技能图标
        /// </summary>
        [SerializeField] private Image _skillIcon;
        
        /// <summary>
        /// 冷却遮罩
        /// </summary>
        [SerializeField] private Image _cooldownMask;
        
        /// <summary>
        /// 冷却时间文本
        /// </summary>
        [SerializeField] private TextMeshProUGUI _cooldownText;
        
        /// <summary>
        /// 技能名称
        /// </summary>
        private string _skillName;
        
        /// <summary>
        /// 冷却时间
        /// </summary>
        private float _cooldownTime;
        
        /// <summary>
        /// 剩余冷却时间
        /// </summary>
        private float _remainingTime;
        
        /// <summary>
        /// 是否在冷却中
        /// </summary>
        private bool _isCoolingDown;
        
        /// <summary>
        /// 初始化技能冷却UI
        /// </summary>
        /// <param name="skillName">技能名称</param>
        /// <param name="cooldownTime">冷却时间</param>
        public void Initialize(string skillName, float cooldownTime)
        {
            _skillName = skillName;
            _cooldownTime = cooldownTime;
            
            // 设置初始状态
            if (_cooldownMask != null) _cooldownMask.fillAmount = 0f;
            if (_cooldownText != null) _cooldownText.text = "";
        }
        
        /// <summary>
        /// 开始冷却
        /// </summary>
        public void StartCooldown()
        {
            _remainingTime = _cooldownTime;
            _isCoolingDown = true;
        }
        
        /// <summary>
        /// Unity Update方法
        /// </summary>
        private void Update()
        {
            if (!_isCoolingDown) return;
            
            _remainingTime -= Time.deltaTime;
            
            if (_remainingTime <= 0f)
            {
                // 冷却完成
                _isCoolingDown = false;
                _remainingTime = 0f;
                
                if (_cooldownMask != null) _cooldownMask.fillAmount = 0f;
                if (_cooldownText != null) _cooldownText.text = "";
            }
            else
            {
                // 更新冷却显示
                float progress = _remainingTime / _cooldownTime;
                if (_cooldownMask != null) _cooldownMask.fillAmount = progress;
                if (_cooldownText != null) _cooldownText.text = _remainingTime.ToString("F1");
            }
        }
    }
}