using Framework.CombatSystem.Core;

namespace Framework.CombatSystem.Interfaces
{
    /// <summary>
    /// 可移动接口
    /// 体现：接口隔离原则 - 定义移动能力的契约
    /// 实现此接口的角色具备移动能力
    /// </summary>
    public interface IMovable
    {
        /// <summary>
        /// 移动速度
        /// </summary>
        float MoveSpeed { get; set; }
        
        /// <summary>
        /// 是否正在移动
        /// </summary>
        bool IsMoving { get; }
        
        /// <summary>
        /// 移动到指定位置
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        void MoveTo(Vector3Logic targetPosition);
        
        /// <summary>
        /// 按方向移动
        /// </summary>
        /// <param name="direction">移动方向（归一化向量）</param>
        /// <param name="deltaTime">时间增量</param>
        void MoveInDirection(Vector3Logic direction, float deltaTime);
        
        /// <summary>
        /// 停止移动
        /// </summary>
        void StopMoving();
        
        /// <summary>
        /// 检查是否可以移动到指定位置
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        /// <returns>是否可以移动</returns>
        bool CanMoveTo(Vector3Logic targetPosition);
    }
}