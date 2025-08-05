namespace UniFramework.ObjectPool.Enhanced
{
    /// <summary>
    /// 增强版可池化对象接口
    /// 在极简版基础上增加更多生命周期回调，便于复杂对象的状态管理
    /// 同时保持向后兼容性，可以无缝升级到完整版IPoolable接口
    /// </summary>
    public interface IEnhancedPoolable
    {
        #region 基础生命周期（与极简版兼容）

        /// <summary>
        /// 对象从池中取出时调用
        /// 用于初始化对象状态，准备使用
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// 对象归还到池中时调用
        /// 用于重置对象状态，准备下次使用
        /// </summary>
        void OnDespawn();

        #endregion

        #region 增强生命周期回调

        /// <summary>
        /// 对象首次创建时调用（仅调用一次）
        /// 用于执行一次性的初始化操作
        /// </summary>
        void OnCreate();

        /// <summary>
        /// 对象即将被销毁时调用
        /// 用于清理资源，释放引用等
        /// </summary>
        void OnDestroy();

        #endregion

        #region 验证接口

        /// <summary>
        /// 验证对象是否可以安全归还到池中
        /// </summary>
        /// <returns>如果对象状态正常可以归还则返回true，否则返回false</returns>
        bool CanReturn();

        #endregion

        #region 调试接口

        /// <summary>
        /// 获取对象的调试信息
        /// 用于调试时查看对象状态
        /// </summary>
        /// <returns>对象的调试信息字符串</returns>
        string GetDebugInfo();

        #endregion
    }

    /// <summary>
    /// 简化版增强可池化接口
    /// 只包含基础的生命周期方法，便于快速实现
    /// </summary>
    public interface ISimpleEnhancedPoolable
    {
        /// <summary>
        /// 对象从池中取出时调用
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// 对象归还到池中时调用
        /// </summary>
        void OnDespawn();

        /// <summary>
        /// 验证对象是否可以安全归还
        /// </summary>
        /// <returns>是否可以归还</returns>
        bool CanReturn();
    }

    /// <summary>
    /// 增强可池化对象的抽象基类
    /// 提供默认实现，减少重复代码
    /// </summary>
    public abstract class EnhancedPoolableBase : IEnhancedPoolable
    {
        #region 状态跟踪

        /// <summary>
        /// 对象是否已被创建
        /// </summary>
        protected bool IsCreated { get; private set; }

        /// <summary>
        /// 对象是否处于活跃状态（已从池中取出）
        /// </summary>
        protected bool IsActive { get; private set; }

        /// <summary>
        /// 对象的创建时间
        /// </summary>
        protected System.DateTime CreationTime { get; private set; }

        /// <summary>
        /// 对象被使用的次数
        /// </summary>
        protected int UseCount { get; private set; }

        #endregion

        #region IEnhancedPoolable实现

        /// <summary>
        /// 对象从池中取出时调用
        /// </summary>
        public virtual void OnSpawn()
        {
            IsActive = true;
            UseCount++;
            OnSpawnInternal();
        }

        /// <summary>
        /// 对象归还到池中时调用
        /// </summary>
        public virtual void OnDespawn()
        {
            IsActive = false;
            OnDespawnInternal();
        }

        /// <summary>
        /// 对象首次创建时调用
        /// </summary>
        public virtual void OnCreate()
        {
            if (IsCreated)
                return;

            IsCreated = true;
            CreationTime = System.DateTime.Now;
            UseCount = 0;
            OnCreateInternal();
        }

        /// <summary>
        /// 对象即将被销毁时调用
        /// </summary>
        public virtual void OnDestroy()
        {
            OnDestroyInternal();
        }

        /// <summary>
        /// 验证对象是否可以安全归还到池中
        /// </summary>
        /// <returns>默认返回true，子类可以重写</returns>
        public virtual bool CanReturn()
        {
            return IsActive && IsCreated;
        }

        /// <summary>
        /// 获取对象的调试信息
        /// </summary>
        /// <returns>调试信息</returns>
        public virtual string GetDebugInfo()
        {
            return $"{GetType().Name}[Created:{IsCreated}, Active:{IsActive}, " +
                   $"UseCount:{UseCount}, CreationTime:{CreationTime:HH:mm:ss}]";
        }

        #endregion

        #region 受保护的虚方法（供子类重写）

        /// <summary>
        /// 子类重写此方法实现具体的Spawn逻辑
        /// </summary>
        protected virtual void OnSpawnInternal() { }

        /// <summary>
        /// 子类重写此方法实现具体的Despawn逻辑
        /// </summary>
        protected virtual void OnDespawnInternal() { }

        /// <summary>
        /// 子类重写此方法实现具体的Create逻辑
        /// </summary>
        protected virtual void OnCreateInternal() { }

        /// <summary>
        /// 子类重写此方法实现具体的Destroy逻辑
        /// </summary>
        protected virtual void OnDestroyInternal() { }

        #endregion
    }
}