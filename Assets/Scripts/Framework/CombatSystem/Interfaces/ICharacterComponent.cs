using Framework.CombatSystem.Core;

namespace Framework.CombatSystem.Interfaces
{
    /// <summary>
    /// 角色组件基础接口
    /// 体现：接口的使用 - 定义组件的基本契约
    /// 所有角色组件都必须实现此接口
    /// </summary>
    public interface ICharacterComponent
    {
        /// <summary>
        /// 组件初始化
        /// </summary>
        /// <param name="owner">拥有此组件的角色</param>
        void Initialize(GameCharacterLogic owner);
        
        /// <summary>
        /// 组件更新
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        void Update(float deltaTime);
        
        /// <summary>
        /// 组件清理
        /// </summary>
        void Cleanup();
    }
}