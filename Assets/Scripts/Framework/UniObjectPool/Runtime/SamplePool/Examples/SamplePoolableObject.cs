using UnityEngine;
using UniFramework.ObjectPool.SamplePool;

namespace UniFramework.ObjectPool.SamplePool.Examples
{
    /// <summary>
    /// 可池化对象示例
    /// 展示如何实现ISamplePoolable接口
    /// </summary>
    public class SamplePoolableObject : MonoBehaviour, ISamplePoolable
    {
        [Header("对象设置")]
        public float lifeTime = 3f;
        public Color spawnColor = Color.white;
        public Color despawnColor = Color.gray;
        
        private Renderer _renderer;
        private float _spawnTime;
        
        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }
        
        /// <summary>
        /// 对象从池中取出时调用
        /// </summary>
        public void OnSpawn()
        {
            _spawnTime = Time.time;
            
            // 设置外观
            if (_renderer != null)
            {
                _renderer.material.color = spawnColor;
            }
            
            // 启动生命周期计时
            if (lifeTime > 0)
            {
                Invoke(nameof(AutoDespawn), lifeTime);
            }
            
            Debug.Log($"{name} 从对象池中生成");
        }
        
        /// <summary>
        /// 对象归还到池中时调用
        /// </summary>
        public void OnDespawn()
        {
            // 取消所有延迟调用
            CancelInvoke();
            
            // 重置状态
            if (_renderer != null)
            {
                _renderer.material.color = despawnColor;
            }
            
            // 重置位置和旋转
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            
            float aliveTime = Time.time - _spawnTime;
            Debug.Log($"{name} 归还到对象池，存活时间: {aliveTime:F2}秒");
        }
        
        /// <summary>
        /// 自动回收
        /// </summary>
        private void AutoDespawn()
        {
            // 这里需要知道对象来自哪个池，实际使用时可以通过其他方式实现
            // 比如在生成时记录池的引用，或者通过管理器查找
            gameObject.SetActive(false);
            Debug.Log($"{name} 生命周期结束，自动回收");
        }
        
        /// <summary>
        /// 手动触发回收（用于测试）
        /// </summary>
        [ContextMenu("手动回收")]
        public void ManualDespawn()
        {
            AutoDespawn();
        }
    }
}