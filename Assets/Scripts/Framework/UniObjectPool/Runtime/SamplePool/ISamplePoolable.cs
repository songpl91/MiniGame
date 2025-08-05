namespace UniFramework.ObjectPool.SamplePool
{
    /// <summary>
    /// 简化版可池化对象接口
    /// 为Demo提供最基础的池化功能，后期可扩展
    /// </summary>
    public interface ISamplePoolable
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