using UnityEngine;

namespace UniFramework.ObjectPool
{
    /// <summary>
    /// 对象池管理器驱动器
    /// 用于在 Unity 生命周期中更新对象池
    /// </summary>
    internal class PoolManagerDriver : MonoBehaviour
    {
        private float _lastUpdateTime;
        private const float UPDATE_INTERVAL = 1f; // 每秒更新一次

        void Update()
        {
            // 定期触发清理检查
            float currentTime = Time.realtimeSinceStartup;
            if (currentTime - _lastUpdateTime >= UPDATE_INTERVAL)
            {
                _lastUpdateTime = currentTime;
                // 这里可以添加定期维护逻辑
                // 例如：检查内存使用情况、执行自动清理等
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // 应用暂停时执行清理
                PoolManager.CleanupAll();
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // 应用失去焦点时执行清理
                PoolManager.CleanupAll();
            }
        }

        void OnDestroy()
        {
            // 驱动器销毁时清理所有对象池
            PoolManager.Destroy();
        }
    }
}