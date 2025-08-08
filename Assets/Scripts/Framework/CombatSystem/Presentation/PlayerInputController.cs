using UnityEngine;
using Framework.CombatSystem.Entities;
using Framework.CombatSystem.Core;

namespace Framework.CombatSystem.Presentation
{
    /// <summary>
    /// 玩家输入控制器
    /// 体现：单一职责原则 - 专门负责处理玩家输入
    /// 体现：组合模式的使用 - PlayerView "Has-A" PlayerInputController
    /// 负责将Unity的输入转换为逻辑层可以理解的指令
    /// </summary>
    public class PlayerInputController : MonoBehaviour
    {
        #region 字段
        
        /// <summary>
        /// 玩家逻辑引用
        /// </summary>
        private PlayerLogic _playerLogic;
        
        /// <summary>
        /// 移动速度倍数
        /// </summary>
        [SerializeField] private float _moveSpeedMultiplier = 1f;
        
        /// <summary>
        /// 是否启用鼠标控制
        /// </summary>
        [SerializeField] private bool _enableMouseControl = true;
        
        /// <summary>
        /// 是否启用键盘控制
        /// </summary>
        [SerializeField] private bool _enableKeyboardControl = true;
        
        /// <summary>
        /// 攻击输入冷却时间
        /// </summary>
        [SerializeField] private float _attackInputCooldown = 0.1f;
        
        /// <summary>
        /// 技能输入冷却时间
        /// </summary>
        [SerializeField] private float _skillInputCooldown = 0.2f;
        
        /// <summary>
        /// 上次攻击输入时间
        /// </summary>
        private float _lastAttackInputTime;
        
        /// <summary>
        /// 上次技能输入时间
        /// </summary>
        private float _lastSkillInputTime;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _isInitialized;
        
        /// <summary>
        /// 主摄像机引用
        /// </summary>
        private Camera _mainCamera;
        
        /// <summary>
        /// 输入向量缓存
        /// </summary>
        private Vector3 _inputVector;
        
        /// <summary>
        /// 鼠标世界位置缓存
        /// </summary>
        private Vector3 _mouseWorldPosition;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// 移动速度倍数
        /// </summary>
        public float MoveSpeedMultiplier
        {
            get => _moveSpeedMultiplier;
            set => _moveSpeedMultiplier = Mathf.Max(0f, value);
        }
        
        /// <summary>
        /// 是否启用鼠标控制
        /// </summary>
        public bool EnableMouseControl
        {
            get => _enableMouseControl;
            set => _enableMouseControl = value;
        }
        
        /// <summary>
        /// 是否启用键盘控制
        /// </summary>
        public bool EnableKeyboardControl
        {
            get => _enableKeyboardControl;
            set => _enableKeyboardControl = value;
        }
        
        #endregion
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Start方法
        /// </summary>
        private void Start()
        {
            // 获取主摄像机引用
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    _mainCamera = FindObjectOfType<Camera>();
                }
            }
        }
        
        /// <summary>
        /// Unity Update方法
        /// </summary>
        private void Update()
        {
            if (!_isInitialized || _playerLogic == null) return;
            
            // 处理移动输入
            HandleMovementInput();
            
            // 处理攻击输入
            HandleAttackInput();
            
            // 处理技能输入
            HandleSkillInput();
            
            // 处理交互输入
            HandleInteractionInput();
            
            // 处理其他输入
            HandleOtherInput();
        }
        
        #endregion
        
        #region 初始化方法
        
        /// <summary>
        /// 初始化输入控制器
        /// </summary>
        /// <param name="playerLogic">玩家逻辑实例</param>
        public void Initialize(PlayerLogic playerLogic)
        {
            if (playerLogic == null)
            {
                Debug.LogError("[PlayerInputController] 尝试用空的玩家逻辑初始化输入控制器");
                return;
            }
            
            _playerLogic = playerLogic;
            _isInitialized = true;
            
            // 重置输入状态
            _lastAttackInputTime = 0f;
            _lastSkillInputTime = 0f;
            
            Debug.Log($"[PlayerInputController] 输入控制器初始化完成，绑定玩家: {_playerLogic.CharacterName}");
        }
        
        #endregion
        
        #region 输入处理方法
        
        /// <summary>
        /// 处理移动输入
        /// </summary>
        private void HandleMovementInput()
        {
            if (!_playerLogic.CanMove) return;
            
            _inputVector = Vector3.zero;
            
            // 键盘输入
            if (_enableKeyboardControl)
            {
                HandleKeyboardMovement();
            }
            
            // 鼠标输入（点击移动）
            if (_enableMouseControl)
            {
                HandleMouseMovement();
            }
            
            // 应用移动输入
            if (_inputVector.magnitude > 0.1f)
            {
                // 归一化输入向量
                _inputVector = _inputVector.normalized * _moveSpeedMultiplier;
                
                // 转换为逻辑层的向量
                var logicDirection = new Vector3Logic(_inputVector.x, _inputVector.y, _inputVector.z);
                
                // 调用玩家逻辑的移动方法
                _playerLogic.HandleMovementInput(logicDirection);
            }
        }
        
        /// <summary>
        /// 处理键盘移动输入
        /// </summary>
        private void HandleKeyboardMovement()
        {
            // WASD或方向键移动
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
            {
                _inputVector = new Vector3(horizontal, 0f, vertical);
            }
        }
        
        /// <summary>
        /// 处理鼠标移动输入（点击移动）
        /// </summary>
        private void HandleMouseMovement()
        {
            // 鼠标右键点击移动
            if (Input.GetMouseButton(1)) // 鼠标右键
            {
                Vector3 mousePosition = Input.mousePosition;
                Vector3 worldPosition = GetMouseWorldPosition(mousePosition);
                
                if (worldPosition != Vector3.zero)
                {
                    // 计算移动方向
                    Vector3 direction = (worldPosition - transform.position).normalized;
                    direction.y = 0f; // 忽略Y轴
                    
                    if (direction.magnitude > 0.1f)
                    {
                        _inputVector = direction;
                    }
                }
            }
        }
        
        /// <summary>
        /// 处理攻击输入
        /// </summary>
        private void HandleAttackInput()
        {
            // 检查攻击输入冷却
            if (Time.time - _lastAttackInputTime < _attackInputCooldown) return;
            
            bool attackInput = false;
            
            // 鼠标左键攻击
            if (_enableMouseControl && Input.GetMouseButtonDown(0))
            {
                attackInput = true;
            }
            
            // 键盘攻击（Ctrl键或Fire1）
            if (_enableKeyboardControl && Input.GetButtonDown("Fire1"))
            {
                attackInput = true;
            }
            
            if (attackInput)
            {
                _playerLogic.HandleAttackInput();
                _lastAttackInputTime = Time.time;
            }
        }
        
        /// <summary>
        /// 处理技能输入
        /// </summary>
        private void HandleSkillInput()
        {
            if (!_enableKeyboardControl) return;
            
            // 检查技能输入冷却
            if (Time.time - _lastSkillInputTime < _skillInputCooldown) return;
            
            string skillName = null;
            
            // Q键 - 技能1
            if (Input.GetKeyDown(KeyCode.Q))
            {
                skillName = "Skill1";
            }
            // E键 - 技能2
            else if (Input.GetKeyDown(KeyCode.E))
            {
                skillName = "Skill2";
            }
            // R键 - 大招
            else if (Input.GetKeyDown(KeyCode.R))
            {
                skillName = "Ultimate";
            }
            // F键 - 特殊技能
            else if (Input.GetKeyDown(KeyCode.F))
            {
                skillName = "Special";
            }
            // 数字键1-4 - 快捷技能
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                skillName = "QuickSkill1";
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                skillName = "QuickSkill2";
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                skillName = "QuickSkill3";
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                skillName = "QuickSkill4";
            }
            
            if (!string.IsNullOrEmpty(skillName))
            {
                _playerLogic.HandleSkillInput(skillName);
                _lastSkillInputTime = Time.time;
            }
        }
        
        /// <summary>
        /// 处理交互输入
        /// </summary>
        private void HandleInteractionInput()
        {
            if (!_enableKeyboardControl) return;
            
            // 空格键 - 交互
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _playerLogic.HandleInteractionInput();
            }
            
            // G键 - 拾取
            if (Input.GetKeyDown(KeyCode.G))
            {
                _playerLogic.HandleInteractionInput("Pickup");
            }
        }
        
        /// <summary>
        /// 处理其他输入
        /// </summary>
        private void HandleOtherInput()
        {
            if (!_enableKeyboardControl) return;
            
            // Tab键 - 切换目标
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                HandleTargetSwitching();
            }
            
            // Escape键 - 取消操作
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleCancelInput();
            }
            
            // Shift键 - 冲刺
            if (Input.GetKey(KeyCode.LeftShift))
            {
                HandleSprintInput(true);
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                HandleSprintInput(false);
            }
        }
        
        #endregion
        
        #region 特殊输入处理
        
        /// <summary>
        /// 处理目标切换
        /// </summary>
        private void HandleTargetSwitching()
        {
            // 这里可以实现目标切换逻辑
            // 例如：在附近的敌人之间切换目标
            Debug.Log("[PlayerInputController] 处理目标切换输入");
        }
        
        /// <summary>
        /// 处理取消输入
        /// </summary>
        private void HandleCancelInput()
        {
            // 这里可以实现取消当前操作的逻辑
            // 例如：取消技能释放、取消移动等
            Debug.Log("[PlayerInputController] 处理取消输入");
        }
        
        /// <summary>
        /// 处理冲刺输入
        /// </summary>
        /// <param name="isSprinting">是否正在冲刺</param>
        private void HandleSprintInput(bool isSprinting)
        {
            // 这里可以实现冲刺逻辑
            // 例如：增加移动速度、消耗体力等
            if (isSprinting)
            {
                _moveSpeedMultiplier = 1.5f; // 冲刺时速度提升50%
            }
            else
            {
                _moveSpeedMultiplier = 1f; // 恢复正常速度
            }
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 获取鼠标在世界坐标中的位置
        /// </summary>
        /// <param name="mousePosition">鼠标屏幕位置</param>
        /// <returns>世界坐标位置</returns>
        private Vector3 GetMouseWorldPosition(Vector3 mousePosition)
        {
            if (_mainCamera == null) return Vector3.zero;
            
            // 从摄像机发射射线到鼠标位置
            Ray ray = _mainCamera.ScreenPointToRay(mousePosition);
            
            // 与地面平面相交（假设地面在Y=0）
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            
            if (groundPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }
            
            return Vector3.zero;
        }
        
        /// <summary>
        /// 检查输入是否被阻塞
        /// </summary>
        /// <returns>是否被阻塞</returns>
        private bool IsInputBlocked()
        {
            // 这里可以检查各种阻塞输入的条件
            // 例如：UI窗口打开、游戏暂停、角色死亡等
            
            if (_playerLogic == null || !_playerLogic.IsAlive)
            {
                return true;
            }
            
            // 检查是否有UI窗口阻塞输入
            // 这里可以添加具体的UI检查逻辑
            
            return false;
        }
        
        /// <summary>
        /// 获取输入强度（用于手柄支持）
        /// </summary>
        /// <returns>输入强度</returns>
        private float GetInputMagnitude()
        {
            return _inputVector.magnitude;
        }
        
        #endregion
        
        #region 配置方法
        
        /// <summary>
        /// 设置输入配置
        /// </summary>
        /// <param name="config">输入配置</param>
        public void SetInputConfig(PlayerInputConfig config)
        {
            if (config == null) return;
            
            _moveSpeedMultiplier = config.MoveSpeedMultiplier;
            _enableMouseControl = config.EnableMouseControl;
            _enableKeyboardControl = config.EnableKeyboardControl;
            _attackInputCooldown = config.AttackInputCooldown;
            _skillInputCooldown = config.SkillInputCooldown;
        }
        
        /// <summary>
        /// 获取当前输入配置
        /// </summary>
        /// <returns>输入配置</returns>
        public PlayerInputConfig GetInputConfig()
        {
            return new PlayerInputConfig
            {
                MoveSpeedMultiplier = _moveSpeedMultiplier,
                EnableMouseControl = _enableMouseControl,
                EnableKeyboardControl = _enableKeyboardControl,
                AttackInputCooldown = _attackInputCooldown,
                SkillInputCooldown = _skillInputCooldown
            };
        }
        
        #endregion
        
        #region 调试方法
        
        /// <summary>
        /// 获取当前输入状态信息
        /// </summary>
        /// <returns>输入状态信息</returns>
        public string GetInputStatusInfo()
        {
            if (!_isInitialized) return "输入控制器未初始化";
            
            return $"输入控制器状态:\n" +
                   $"绑定玩家: {(_playerLogic != null ? _playerLogic.CharacterName : "无")}\n" +
                   $"鼠标控制: {_enableMouseControl}\n" +
                   $"键盘控制: {_enableKeyboardControl}\n" +
                   $"移动速度倍数: {_moveSpeedMultiplier:F2}\n" +
                   $"当前输入向量: {_inputVector}\n" +
                   $"输入强度: {GetInputMagnitude():F2}";
        }
        
        #endregion
    }
    
    /// <summary>
    /// 玩家输入配置
    /// </summary>
    [System.Serializable]
    public class PlayerInputConfig
    {
        /// <summary>
        /// 移动速度倍数
        /// </summary>
        public float MoveSpeedMultiplier = 1f;
        
        /// <summary>
        /// 是否启用鼠标控制
        /// </summary>
        public bool EnableMouseControl = true;
        
        /// <summary>
        /// 是否启用键盘控制
        /// </summary>
        public bool EnableKeyboardControl = true;
        
        /// <summary>
        /// 攻击输入冷却时间
        /// </summary>
        public float AttackInputCooldown = 0.1f;
        
        /// <summary>
        /// 技能输入冷却时间
        /// </summary>
        public float SkillInputCooldown = 0.2f;
    }
}