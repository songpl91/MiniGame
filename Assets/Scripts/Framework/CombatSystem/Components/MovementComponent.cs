using System;
using Framework.CombatSystem.Core;
using Framework.CombatSystem.Interfaces;

namespace Framework.CombatSystem.Components
{
    /// <summary>
    /// 移动组件基类
    /// 体现：抽象类的使用 - 为不同类型的移动提供通用基础
    /// </summary>
    public abstract class MovementComponent : ICharacterComponent, IMovable
    {
        #region 字段
        
        /// <summary>
        /// 拥有此组件的角色
        /// </summary>
        protected GameCharacterLogic _owner;
        
        /// <summary>
        /// 移动速度
        /// </summary>
        protected float _moveSpeed;
        
        /// <summary>
        /// 是否正在移动
        /// </summary>
        protected bool _isMoving;
        
        /// <summary>
        /// 目标位置
        /// </summary>
        protected Vector3Logic _targetPosition;
        
        #endregion
        
        #region IMovable实现
        
        /// <summary>
        /// 移动速度
        /// </summary>
        public virtual float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = Math.Max(0f, value);
        }
        
        /// <summary>
        /// 是否正在移动
        /// </summary>
        public virtual bool IsMoving => _isMoving;
        
        /// <summary>
        /// 移动到指定位置 - 抽象方法，子类必须实现
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        public abstract void MoveTo(Vector3Logic targetPosition);
        
        /// <summary>
        /// 按方向移动 - 抽象方法，子类必须实现
        /// </summary>
        /// <param name="direction">移动方向</param>
        /// <param name="deltaTime">时间增量</param>
        public abstract void MoveInDirection(Vector3Logic direction, float deltaTime);
        
        /// <summary>
        /// 停止移动
        /// </summary>
        public virtual void StopMoving()
        {
            _isMoving = false;
            _targetPosition = _owner.Position;
        }
        
        /// <summary>
        /// 检查是否可以移动到指定位置 - 虚方法，子类可重写
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        /// <returns>是否可以移动</returns>
        public virtual bool CanMoveTo(Vector3Logic targetPosition)
        {
            // 默认实现：简单的距离检查
            return Vector3Logic.Distance(_owner.Position, targetPosition) > 0.01f;
        }
        
        #endregion
        
        #region ICharacterComponent实现
        
        /// <summary>
        /// 组件初始化
        /// </summary>
        /// <param name="owner">拥有此组件的角色</param>
        public virtual void Initialize(GameCharacterLogic owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _moveSpeed = 5f; // 默认移动速度
            _isMoving = false;
            _targetPosition = owner.Position;
        }
        
        /// <summary>
        /// 组件更新 - 抽象方法，子类必须实现
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public abstract void Update(float deltaTime);
        
        /// <summary>
        /// 组件清理
        /// </summary>
        public virtual void Cleanup()
        {
            _owner = null;
            _isMoving = false;
        }
        
        #endregion
        
        #region 受保护的辅助方法
        
        /// <summary>
        /// 更新角色位置
        /// </summary>
        /// <param name="newPosition">新位置</param>
        protected virtual void UpdatePosition(Vector3Logic newPosition)
        {
            if (_owner != null)
            {
                _owner.SetPosition(newPosition);
            }
        }
        
        /// <summary>
        /// 更新角色朝向
        /// </summary>
        /// <param name="direction">朝向</param>
        protected virtual void UpdateForward(Vector3Logic direction)
        {
            if (_owner != null && direction.SqrMagnitude > 0.01f)
            {
                _owner.SetForward(direction.Normalized);
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 地面移动组件
    /// 体现：继承的使用 - 实现具体的地面移动逻辑
    /// </summary>
    public class GroundMovementComponent : MovementComponent
    {
        #region 字段
        
        /// <summary>
        /// 到达目标的距离阈值
        /// </summary>
        private const float ARRIVAL_THRESHOLD = 0.1f;
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="owner">拥有此组件的角色</param>
        /// <param name="moveSpeed">移动速度</param>
        public GroundMovementComponent(GameCharacterLogic owner, float moveSpeed = 5f)
        {
            Initialize(owner);
            MoveSpeed = moveSpeed;
        }
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="owner">拥有此组件的角色</param>
        /// <param name="moveSpeed">移动速度</param>
        /// <param name="flyingHeight">飞行高度</param>
        public FlyingMovementComponent(GameCharacterLogic owner, float moveSpeed = 5f, float flyingHeight = 2f)
        {
            Initialize(owner);
            MoveSpeed = moveSpeed;
            FlyingHeight = flyingHeight;
        }
        
        #endregion
        
        #region 重写方法
        
        /// <summary>
        /// 移动到指定位置
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        public override void MoveTo(Vector3Logic targetPosition)
        {
            if (!CanMoveTo(targetPosition)) return;
            
            _targetPosition = targetPosition;
            _isMoving = true;
        }
        
        /// <summary>
        /// 按方向移动
        /// </summary>
        /// <param name="direction">移动方向</param>
        /// <param name="deltaTime">时间增量</param>
        public override void MoveInDirection(Vector3Logic direction, float deltaTime)
        {
            if (direction.SqrMagnitude < 0.01f)
            {
                StopMoving();
                return;
            }
            
            Vector3Logic normalizedDirection = direction.Normalized;
            Vector3Logic movement = normalizedDirection * _moveSpeed * deltaTime;
            Vector3Logic newPosition = _owner.Position + movement;
            
            // 更新位置和朝向
            UpdatePosition(newPosition);
            UpdateForward(normalizedDirection);
            
            _isMoving = true;
            _targetPosition = newPosition;
        }
        
        /// <summary>
        /// 组件更新
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public override void Update(float deltaTime)
        {
            if (!_isMoving) return;
            
            Vector3Logic currentPosition = _owner.Position;
            Vector3Logic direction = _targetPosition - currentPosition;
            float distance = direction.Magnitude;
            
            // 检查是否到达目标
            if (distance <= ARRIVAL_THRESHOLD)
            {
                UpdatePosition(_targetPosition);
                StopMoving();
                return;
            }
            
            // 继续移动
            Vector3Logic normalizedDirection = direction.Normalized;
            Vector3Logic movement = normalizedDirection * _moveSpeed * deltaTime;
            
            // 防止超过目标位置
            if (movement.Magnitude >= distance)
            {
                UpdatePosition(_targetPosition);
                StopMoving();
            }
            else
            {
                Vector3Logic newPosition = currentPosition + movement;
                UpdatePosition(newPosition);
                UpdateForward(normalizedDirection);
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 飞行移动组件
    /// 体现：继承的使用 - 实现具体的飞行移动逻辑
    /// </summary>
    public class FlyingMovementComponent : MovementComponent
    {
        #region 字段
        
        /// <summary>
        /// 飞行高度
        /// </summary>
        private float _flyingHeight = 2f;
        
        /// <summary>
        /// 到达目标的距离阈值
        /// </summary>
        private const float ARRIVAL_THRESHOLD = 0.1f;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 飞行高度
        /// </summary>
        public float FlyingHeight
        {
            get => _flyingHeight;
            set => _flyingHeight = Math.Max(0f, value);
        }
        
        #endregion
        
        #region 重写方法
        
        /// <summary>
        /// 移动到指定位置
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        public override void MoveTo(Vector3Logic targetPosition)
        {
            if (!CanMoveTo(targetPosition)) return;
            
            // 飞行单位保持一定高度
            _targetPosition = new Vector3Logic(targetPosition.X, _flyingHeight, targetPosition.Z);
            _isMoving = true;
        }
        
        /// <summary>
        /// 按方向移动
        /// </summary>
        /// <param name="direction">移动方向</param>
        /// <param name="deltaTime">时间增量</param>
        public override void MoveInDirection(Vector3Logic direction, float deltaTime)
        {
            if (direction.SqrMagnitude < 0.01f)
            {
                StopMoving();
                return;
            }
            
            Vector3Logic normalizedDirection = direction.Normalized;
            Vector3Logic movement = normalizedDirection * _moveSpeed * deltaTime;
            Vector3Logic newPosition = _owner.Position + movement;
            
            // 保持飞行高度
            newPosition.Y = _flyingHeight;
            
            // 更新位置和朝向
            UpdatePosition(newPosition);
            UpdateForward(normalizedDirection);
            
            _isMoving = true;
            _targetPosition = newPosition;
        }
        
        /// <summary>
        /// 组件更新
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public override void Update(float deltaTime)
        {
            if (!_isMoving) return;
            
            Vector3Logic currentPosition = _owner.Position;
            Vector3Logic direction = _targetPosition - currentPosition;
            float distance = direction.Magnitude;
            
            // 检查是否到达目标
            if (distance <= ARRIVAL_THRESHOLD)
            {
                UpdatePosition(_targetPosition);
                StopMoving();
                return;
            }
            
            // 继续移动
            Vector3Logic normalizedDirection = direction.Normalized;
            Vector3Logic movement = normalizedDirection * _moveSpeed * deltaTime;
            
            // 防止超过目标位置
            if (movement.Magnitude >= distance)
            {
                UpdatePosition(_targetPosition);
                StopMoving();
            }
            else
            {
                Vector3Logic newPosition = currentPosition + movement;
                newPosition.Y = _flyingHeight; // 保持飞行高度
                UpdatePosition(newPosition);
                UpdateForward(normalizedDirection);
            }
        }
        
        #endregion
    }
}