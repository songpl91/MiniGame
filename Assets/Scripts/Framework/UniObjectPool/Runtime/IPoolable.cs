namespace UniFramework.ObjectPool
{
    /// <summary>
    /// 可池化对象接口
    /// 实现此接口的对象可以被对象池管理
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 对象从池中取出时调用
        /// 用于重置对象状态，准备重新使用
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// 对象归还到池中时调用
        /// 用于清理对象状态，准备回收
        /// </summary>
        void OnDespawn();
    }
}