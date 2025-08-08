using UnityEngine;
using Framework.CombatSystem.Entities;
using Framework.CombatSystem.Interfaces;

namespace Framework.CombatSystem.Presentation
{
    /// <summary>
    /// 玩家表现层类
    /// 体现：继承的使用 - PlayerView "Is-A" GameCharacterView
    /// 负责玩家特有的表现逻辑，如输入处理、UI显示等
    /// </summary>
    public class PlayerView : GameCharacterView
    {
        #region 字段
        
        /// <summary>
        /// 玩家逻辑引用（类型安全的引用）
        /// </summary>
        private PlayerLogic _playerLogic;
        
        /// <summary>
        /// 等级显示UI
        /// </summary>
        [SerializeField] private GameObject _levelDisplay;
        
        /// <summary>
        /// 经验值条
        /// </summary>
        [SerializeField] private UnityEngine.UI.Image _experienceBar;
        
        /// <summary>
        /// 技能冷却显示
        /// </summary>
        [SerializeField] private GameObject _skillCooldownDisplay;
        
        /// <summary>
        /// 输入控制器
        /// </summary>
        [SerializeField] private PlayerInputController _inputController;
        
        /// <summary>
        /// 玩家UI面板
        /// </summary>
        [SerializeField] private GameObject _playerUIPanel;
        
        /// <summary>
        /// 升级特效预制体
        /// </summary>
        [SerializeField] private GameObject _levelUpEffectPrefab;
        
        /// <summary>
        /// 技能释放特效预制体
        /// </summary>
        [SerializeField] private GameObject _skillEffectPrefab;
        
        /// <summary>
        /// 移动粒子系统
        /// </summary>
        [SerializeField] private ParticleSystem _movementParticles;
        
        /// <summary>
        /// 攻击特效挂载点
        /// </summary>
        [SerializeField] private Transform _attackEffectMount;
        
        /// <summary>
        /// 是否启用输入控制
        /// </summary>
        [SerializeField] private bool _enableInputControl = true;
        
        /// <summary>
        /// 上一帧的等级
        /// </summary>
        private int _lastLevel;
        
        /// <summary>
        /// 上一帧的经验值
        /// </summary>
        private int _lastExperience;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 玩家逻辑引用
        /// </summary>
        public PlayerLogic PlayerLogic => _playerLogic;
        
        /// <summary>
        /// 是否启用输入控制
        /// </summary>
        public bool EnableInputControl
        {
            get => _enableInputControl;
            set
            {
                _enableInputControl = value;
                if (_inputController != null)
                {
                    _inputController.enabled = value;
                }
            }
        }
        
        #endregion
        
        #region 重写基类方法
        
        /// <summary>
        /// 绑定角色逻辑
        /// 重写以提供类型安全的玩家逻辑绑定
        /// </summary>
        /// <param name="characterLogic">角色逻辑实例</param>
        public override void BindCharacterLogic(GameCharacterLogic characterLogic)
        {
            // 检查类型
            if (characterLogic is PlayerLogic playerLogic)
            {
                _playerLogic = playerLogic;
                base.BindCharacterLogic(characterLogic);
            }
            else
            {
                Debug.LogError($"[PlayerView] 尝试绑定非玩家逻辑到玩家视图: {gameObject.name}");
            }
        }
        
        /// <summary>
        /// 视图初始化完成回调
        /// </summary>
        protected override void OnViewInitialized()
        {
            base.OnViewInitialized();
            
            if (_playerLogic == null) return;
            
            // 初始化玩家特有的UI
            UpdateLevelDisplay(_playerLogic.Level);
            UpdateExperienceBar();
            
            // 设置输入控制器
            SetupInputController();
            
            // 记录初始状态
            _lastLevel = _playerLogic.Level;
            _lastExperience = _playerLogic.CurrentExperience;
            
            // 显示玩家UI
            if (_playerUIPanel != null)
            {
                _playerUIPanel.SetActive(true);
            }
        }
        
        /// <summary>
        /// 订阅逻辑层事件
        /// </summary>
        protected override void SubscribeToLogicEvents()
        {
            base.SubscribeToLogicEvents();
            
            if (_playerLogic != null)
            {
                _playerLogic.OnLevelUp += OnPlayerLevelUp;
                _playerLogic.OnExperienceGained += OnPlayerExperienceGained;
                _playerLogic.OnSkillUsed += OnPlayerSkillUsed;
            }
        }
        
        /// <summary>
        /// 取消订阅逻辑层事件
        /// </summary>
        protected override void UnsubscribeFromLogicEvents()
        {
            base.UnsubscribeFromLogicEvents();
            
            if (_playerLogic != null)
            {
                _playerLogic.OnLevelUp -= OnPlayerLevelUp;
                _playerLogic.OnExperienceGained -= OnPlayerExperienceGained;
                _playerLogic.OnSkillUsed -= OnPlayerSkillUsed;
            }
        }
        
        /// <summary>
        /// 更新视图 - 实现抽象方法
        /// </summary>
        protected override void UpdateView()
        {
            if (_playerLogic == null) return;
            
            // 更新等级显示
            if (_playerLogic.Level != _lastLevel)
            {
                UpdateLevelDisplay(_playerLogic.Level);
                _lastLevel = _playerLogic.Level;
            }
            
            // 更新经验值显示
            if (_playerLogic.CurrentExperience != _lastExperience)
            {
                UpdateExperienceBar();
                _lastExperience = _playerLogic.CurrentExperience;
            }
            
            // 更新技能冷却显示
            UpdateSkillCooldownDisplay();
            
            // 处理输入
            if (_enableInputControl)
            {
                HandleInput();
            }
        }
        
        /// <summary>
        /// 移动检测回调
        /// </summary>
        protected override void OnMovementDetected()
        {
            base.OnMovementDetected();
            
            // 播放移动粒子效果
            if (_movementParticles != null && !_movementParticles.isPlaying)
            {
                _movementParticles.Play();
            }
        }
        
        /// <summary>
        /// 获取玩家特定的视图配置
        /// </summary>
        /// <returns>视图配置</returns>
        protected override CharacterViewConfig GetViewConfig()
        {
            return new CharacterViewConfig
            {
                HealthBarOffset = Vector3.up * 2.2f,
                NameDisplayOffset = Vector3.up * 2.7f,
                EffectScale = 1.2f
            };
        }
        
        #endregion
        
        #region 玩家特有事件处理
        
        /// <summary>
        /// 玩家升级处理
        /// </summary>
        /// <param name="player">玩家逻辑</param>
        /// <param name="newLevel">新等级</param>
        private void OnPlayerLevelUp(PlayerLogic player, int newLevel)
        {
            // 播放升级特效
            PlayLevelUpEffect();
            
            // 播放升级音效
            PlayLevelUpSound();
            
            // 更新等级显示
            UpdateLevelDisplay(newLevel);
            
            // 显示升级提示
            ShowLevelUpNotification(newLevel);
            
            Debug.Log($"[PlayerView] 玩家 {player.CharacterName} 升级到 {newLevel} 级！");
        }
        
        /// <summary>
        /// 玩家获得经验处理
        /// </summary>
        /// <param name="player">玩家逻辑</param>
        /// <param name="experience">获得的经验值</param>
        private void OnPlayerExperienceGained(PlayerLogic player, int experience)
        {
            // 播放经验获得特效
            PlayExperienceGainEffect(experience);
            
            // 更新经验值条
            UpdateExperienceBar();
            
            // 显示经验值获得提示
            ShowExperienceGainNotification(experience);
        }
        
        /// <summary>
        /// 玩家使用技能处理
        /// </summary>
        /// <param name="player">玩家逻辑</param>
        /// <param name="skillName">技能名称</param>
        private void OnPlayerSkillUsed(PlayerLogic player, string skillName)
        {
            // 播放技能特效
            PlaySkillEffect(skillName);
            
            // 播放技能音效
            PlaySkillSound(skillName);
            
            // 更新技能冷却显示
            UpdateSkillCooldownDisplay();
            
            Debug.Log($"[PlayerView] 玩家 {player.CharacterName} 使用技能: {skillName}");
        }
        
        #endregion
        
        #region UI更新方法
        
        /// <summary>
        /// 更新等级显示
        /// </summary>
        /// <param name="level">等级</param>
        private void UpdateLevelDisplay(int level)
        {
            if (_levelDisplay != null)
            {
                var textComponent = _levelDisplay.GetComponent<UnityEngine.UI.Text>();
                if (textComponent != null)
                {
                    textComponent.text = $"Lv.{level}";
                }
                
                var tmpComponent = _levelDisplay.GetComponent<TMPro.TextMeshProUGUI>();
                if (tmpComponent != null)
                {
                    tmpComponent.text = $"Lv.{level}";
                }
            }
        }
        
        /// <summary>
        /// 更新经验值条
        /// </summary>
        private void UpdateExperienceBar()
        {
            if (_experienceBar != null && _playerLogic != null)
            {
                float expPercent = (float)_playerLogic.CurrentExperience / _playerLogic.ExperienceToNextLevel;
                _experienceBar.fillAmount = Mathf.Clamp01(expPercent);
            }
        }
        
        /// <summary>
        /// 更新技能冷却显示
        /// </summary>
        private void UpdateSkillCooldownDisplay()
        {
            if (_skillCooldownDisplay != null && _playerLogic != null)
            {
                // 这里可以根据玩家的技能冷却状态更新UI
                // 例如：显示技能图标的冷却遮罩、冷却时间文本等
            }
        }
        
        #endregion
        
        #region 输入处理
        
        /// <summary>
        /// 设置输入控制器
        /// </summary>
        private void SetupInputController()
        {
            if (_inputController == null)
            {
                _inputController = GetComponent<PlayerInputController>();
            }
            
            if (_inputController != null)
            {
                _inputController.Initialize(_playerLogic);
                _inputController.enabled = _enableInputControl;
            }
        }
        
        /// <summary>
        /// 处理输入
        /// </summary>
        private void HandleInput()
        {
            if (_playerLogic == null) return;
            
            // 移动输入
            HandleMovementInput();
            
            // 攻击输入
            HandleAttackInput();
            
            // 技能输入
            HandleSkillInput();
            
            // 其他输入
            HandleOtherInput();
        }
        
        /// <summary>
        /// 处理移动输入
        /// </summary>
        private void HandleMovementInput()
        {
            if (!_playerLogic.CanMove) return;
            
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
            {
                Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
                
                // 将Unity的Vector3转换为逻辑层的Vector3Logic
                var logicDirection = new Vector3Logic(moveDirection.x, moveDirection.y, moveDirection.z);
                
                // 调用玩家逻辑的移动方法
                _playerLogic.HandleMovementInput(logicDirection);
            }
        }
        
        /// <summary>
        /// 处理攻击输入
        /// </summary>
        private void HandleAttackInput()
        {
            if (Input.GetButtonDown("Fire1")) // 鼠标左键或Ctrl键
            {
                _playerLogic.HandleAttackInput();
            }
        }
        
        /// <summary>
        /// 处理技能输入
        /// </summary>
        private void HandleSkillInput()
        {
            // Q键 - 技能1
            if (Input.GetKeyDown(KeyCode.Q))
            {
                _playerLogic.HandleSkillInput("Skill1");
            }
            
            // E键 - 技能2
            if (Input.GetKeyDown(KeyCode.E))
            {
                _playerLogic.HandleSkillInput("Skill2");
            }
            
            // R键 - 大招
            if (Input.GetKeyDown(KeyCode.R))
            {
                _playerLogic.HandleSkillInput("Ultimate");
            }
        }
        
        /// <summary>
        /// 处理其他输入
        /// </summary>
        private void HandleOtherInput()
        {
            // 空格键 - 交互
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _playerLogic.HandleInteractionInput();
            }
        }
        
        #endregion
        
        #region 特效和音效方法
        
        /// <summary>
        /// 播放升级特效
        /// </summary>
        private void PlayLevelUpEffect()
        {
            if (_levelUpEffectPrefab != null && _effectMount != null)
            {
                GameObject effect = Instantiate(_levelUpEffectPrefab, _effectMount.position, _effectMount.rotation);
                Destroy(effect, 3f); // 3秒后销毁特效
            }
        }
        
        /// <summary>
        /// 播放技能特效
        /// </summary>
        /// <param name="skillName">技能名称</param>
        private void PlaySkillEffect(string skillName)
        {
            if (_skillEffectPrefab != null && _attackEffectMount != null)
            {
                GameObject effect = Instantiate(_skillEffectPrefab, _attackEffectMount.position, _attackEffectMount.rotation);
                Destroy(effect, 2f); // 2秒后销毁特效
            }
        }
        
        /// <summary>
        /// 播放经验获得特效
        /// </summary>
        /// <param name="experience">获得的经验值</param>
        private void PlayExperienceGainEffect(int experience)
        {
            // 这里可以根据经验值大小播放不同的特效
            Debug.Log($"[PlayerView] 播放经验获得特效: +{experience} EXP");
        }
        
        /// <summary>
        /// 播放升级音效
        /// </summary>
        private void PlayLevelUpSound()
        {
            if (_audioSource != null)
            {
                // 这里可以播放升级音效
                // _audioSource.PlayOneShot(levelUpSound);
            }
        }
        
        /// <summary>
        /// 播放技能音效
        /// </summary>
        /// <param name="skillName">技能名称</param>
        private void PlaySkillSound(string skillName)
        {
            if (_audioSource != null)
            {
                // 这里可以根据技能名称播放不同的音效
                // _audioSource.PlayOneShot(GetSkillSound(skillName));
            }
        }
        
        #endregion
        
        #region 通知方法
        
        /// <summary>
        /// 显示升级通知
        /// </summary>
        /// <param name="newLevel">新等级</param>
        private void ShowLevelUpNotification(int newLevel)
        {
            // 这里可以显示升级通知UI
            // 例如：弹出升级提示窗口、显示升级文本等
            Debug.Log($"[PlayerView] 显示升级通知: 达到 {newLevel} 级！");
        }
        
        /// <summary>
        /// 显示经验获得通知
        /// </summary>
        /// <param name="experience">获得的经验值</param>
        private void ShowExperienceGainNotification(int experience)
        {
            // 这里可以显示经验获得通知
            // 例如：飘字效果、经验值增加动画等
            Debug.Log($"[PlayerView] 显示经验获得通知: +{experience} EXP");
        }
        
        #endregion
        
        #region 公共接口方法
        
        /// <summary>
        /// 设置玩家UI可见性
        /// </summary>
        /// <param name="visible">是否可见</param>
        public void SetPlayerUIVisible(bool visible)
        {
            if (_playerUIPanel != null)
            {
                _playerUIPanel.SetActive(visible);
            }
        }
        
        /// <summary>
        /// 强制更新所有UI
        /// </summary>
        public void ForceUpdateAllUI()
        {
            if (_playerLogic == null) return;
            
            UpdateLevelDisplay(_playerLogic.Level);
            UpdateExperienceBar();
            UpdateSkillCooldownDisplay();
            ForceHealthSync();
        }
        
        /// <summary>
        /// 获取玩家当前状态信息
        /// </summary>
        /// <returns>状态信息字符串</returns>
        public string GetPlayerStatusInfo()
        {
            if (_playerLogic == null) return "未绑定玩家逻辑";
            
            return $"玩家: {_playerLogic.CharacterName}\n" +
                   $"等级: {_playerLogic.Level}\n" +
                   $"生命值: {_playerLogic.CurrentHealth:F1}/{_playerLogic.MaxHealth:F1}\n" +
                   $"经验值: {_playerLogic.CurrentExperience}/{_playerLogic.ExperienceToNextLevel}\n" +
                   $"位置: {_playerLogic.Position}";
        }
        
        #endregion
    }
}