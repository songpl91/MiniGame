using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace UniFramework.ObjectPool.Examples
{
    /// <summary>
    /// 示例：敌人AI脚本的对象池处理
    /// 展示如何处理状态重置、事件清理、协程管理等
    /// </summary>
    public class PoolableEnemyAI : MonoBehaviour, IGameObjectPoolable
    {
        [Header("敌人属性")]
        public float maxHealth = 100f;
        public float moveSpeed = 5f;
        public float attackDamage = 10f;
        
        [Header("状态信息")]
        [SerializeField] private float _currentHealth;
        [SerializeField] private bool _isAttacking;
        [SerializeField] private bool _isDead;
        
        // 运行时状态
        private Transform _target;
        private Coroutine _attackCoroutine;
        private Coroutine _moveCoroutine;
        private UnityAction<float> _onHealthChanged;
        
        // 组件引用
        private Rigidbody _rigidbody;
        private Animator _animator;
        
        private void Awake()
        {
            // 缓存组件引用
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// 从对象池取出时的初始化
        /// </summary>
        public void OnSpawnFromPool()
        {
            // 重置基础状态
            _currentHealth = maxHealth;
            _isAttacking = false;
            _isDead = false;
            _target = null;
            
            // 重置物理状态
            if (_rigidbody != null)
            {
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
                _rigidbody.isKinematic = false;
            }
            
            // 重置动画状态
            if (_animator != null)
            {
                _animator.SetBool("IsAttacking", false);
                _animator.SetBool("IsDead", false);
                _animator.SetFloat("Speed", 0f);
            }
            
            // 注册事件（示例）
            _onHealthChanged = OnHealthChangedHandler;
            // GameEventManager.Instance.OnPlayerHealthChanged += _onHealthChanged;
            
            // 启动AI逻辑
            StartAI();
            
            Debug.Log($"[PoolableEnemyAI] {gameObject.name} 已从对象池激活");
        }

        /// <summary>
        /// 归还到对象池时的清理
        /// </summary>
        public void OnDespawnToPool()
        {
            // 停止所有协程
            StopAllCoroutines();
            _attackCoroutine = null;
            _moveCoroutine = null;
            
            // 注销事件
            if (_onHealthChanged != null)
            {
                // GameEventManager.Instance.OnPlayerHealthChanged -= _onHealthChanged;
                _onHealthChanged = null;
            }
            
            // 清理引用
            _target = null;
            
            // 重置物理状态
            if (_rigidbody != null)
            {
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }
            
            // 重置动画状态
            if (_animator != null)
            {
                _animator.SetBool("IsAttacking", false);
                _animator.SetBool("IsDead", false);
                _animator.SetFloat("Speed", 0f);
            }
            
            Debug.Log($"[PoolableEnemyAI] {gameObject.name} 已归还到对象池");
        }

        /// <summary>
        /// 启动AI逻辑
        /// </summary>
        private void StartAI()
        {
            if (_moveCoroutine == null)
            {
                _moveCoroutine = StartCoroutine(MoveCoroutine());
            }
        }

        /// <summary>
        /// 移动协程示例
        /// </summary>
        private IEnumerator MoveCoroutine()
        {
            while (!_isDead)
            {
                // AI移动逻辑
                yield return new WaitForSeconds(0.1f);
            }
        }

        /// <summary>
        /// 攻击协程示例
        /// </summary>
        private IEnumerator AttackCoroutine()
        {
            _isAttacking = true;
            
            // 攻击逻辑
            yield return new WaitForSeconds(1f);
            
            _isAttacking = false;
            _attackCoroutine = null;
        }

        /// <summary>
        /// 健康值变化事件处理
        /// </summary>
        private void OnHealthChangedHandler(float newHealth)
        {
            // 处理健康值变化
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (_isDead) return;
            
            _currentHealth -= damage;
            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 死亡处理
        /// </summary>
        private void Die()
        {
            _isDead = true;
            
            // 播放死亡动画
            if (_animator != null)
            {
                _animator.SetBool("IsDead", true);
            }
            
            // 延迟归还到对象池
            StartCoroutine(DelayedReturnToPool());
        }

        /// <summary>
        /// 延迟归还到对象池
        /// </summary>
        private IEnumerator DelayedReturnToPool()
        {
            yield return new WaitForSeconds(2f); // 等待死亡动画播放完成
            
            // 归还到对象池
            PoolManager.Return(gameObject);
        }
    }

    /// <summary>
    /// 示例：UI元素的对象池处理
    /// 展示如何处理UI组件的状态重置
    /// </summary>
    public class PoolableUIElement : MonoBehaviour, IGameObjectPoolable
    {
        [Header("UI组件")]
        public Text titleText;
        public Image iconImage;
        public Button actionButton;
        public Slider progressSlider;
        
        // 原始状态备份
        private string _originalTitle;
        private Sprite _originalIcon;
        private float _originalProgress;
        
        private void Awake()
        {
            // 备份原始状态
            if (titleText != null) _originalTitle = titleText.text;
            if (iconImage != null) _originalIcon = iconImage.sprite;
            if (progressSlider != null) _originalProgress = progressSlider.value;
        }

        /// <summary>
        /// 从对象池取出时的初始化
        /// </summary>
        public void OnSpawnFromPool()
        {
            // 重置UI状态到原始状态
            if (titleText != null) titleText.text = _originalTitle;
            if (iconImage != null) iconImage.sprite = _originalIcon;
            if (progressSlider != null) progressSlider.value = _originalProgress;
            
            // 重置按钮状态
            if (actionButton != null)
            {
                actionButton.interactable = true;
                // 清理之前的监听器
                actionButton.onClick.RemoveAllListeners();
            }
            
            // 重置透明度和缩放
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            
            transform.localScale = Vector3.one;
            
            Debug.Log($"[PoolableUIElement] {gameObject.name} UI元素已从对象池激活");
        }

        /// <summary>
        /// 归还到对象池时的清理
        /// </summary>
        public void OnDespawnToPool()
        {
            // 停止所有动画和协程
            StopAllCoroutines();
            
            // 清理按钮事件
            if (actionButton != null)
            {
                actionButton.onClick.RemoveAllListeners();
            }
            
            // 重置到原始状态
            if (titleText != null) titleText.text = _originalTitle;
            if (iconImage != null) iconImage.sprite = _originalIcon;
            if (progressSlider != null) progressSlider.value = _originalProgress;
            
            Debug.Log($"[PoolableUIElement] {gameObject.name} UI元素已归还到对象池");
        }

        /// <summary>
        /// 设置UI内容（使用示例）
        /// </summary>
        public void SetContent(string title, Sprite icon, float progress)
        {
            if (titleText != null) titleText.text = title;
            if (iconImage != null) iconImage.sprite = icon;
            if (progressSlider != null) progressSlider.value = progress;
        }
    }

    /// <summary>
    /// 示例：粒子效果的对象池处理
    /// 展示如何处理粒子系统的重置和清理
    /// </summary>
    public class PoolableParticleEffect : MonoBehaviour, IGameObjectPoolable
    {
        [Header("粒子设置")]
        public float autoReturnDelay = 3f;
        public bool autoReturnWhenStopped = true;
        
        private ParticleSystem[] _particleSystems;
        private AudioSource _audioSource;
        private Coroutine _autoReturnCoroutine;

        private void Awake()
        {
            // 缓存所有粒子系统
            _particleSystems = GetComponentsInChildren<ParticleSystem>();
            _audioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// 从对象池取出时的初始化
        /// </summary>
        public void OnSpawnFromPool()
        {
            // 重置并播放所有粒子系统
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                if (_particleSystems[i] != null)
                {
                    _particleSystems[i].Clear();
                    _particleSystems[i].Play();
                }
            }
            
            // 播放音效
            if (_audioSource != null && _audioSource.clip != null)
            {
                _audioSource.Play();
            }
            
            // 启动自动归还计时器
            if (autoReturnDelay > 0)
            {
                _autoReturnCoroutine = StartCoroutine(AutoReturnCoroutine());
            }
            
            Debug.Log($"[PoolableParticleEffect] {gameObject.name} 粒子效果已从对象池激活");
        }

        /// <summary>
        /// 归还到对象池时的清理
        /// </summary>
        public void OnDespawnToPool()
        {
            // 停止自动归还协程
            if (_autoReturnCoroutine != null)
            {
                StopCoroutine(_autoReturnCoroutine);
                _autoReturnCoroutine = null;
            }
            
            // 停止所有粒子系统
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                if (_particleSystems[i] != null)
                {
                    _particleSystems[i].Stop();
                    _particleSystems[i].Clear();
                }
            }
            
            // 停止音效
            if (_audioSource != null)
            {
                _audioSource.Stop();
            }
            
            Debug.Log($"[PoolableParticleEffect] {gameObject.name} 粒子效果已归还到对象池");
        }

        /// <summary>
        /// 自动归还协程
        /// </summary>
        private IEnumerator AutoReturnCoroutine()
        {
            yield return new WaitForSeconds(autoReturnDelay);
            
            // 检查是否需要等待粒子播放完成
            if (autoReturnWhenStopped)
            {
                bool anyPlaying = true;
                while (anyPlaying)
                {
                    anyPlaying = false;
                    for (int i = 0; i < _particleSystems.Length; i++)
                    {
                        if (_particleSystems[i] != null && _particleSystems[i].isPlaying)
                        {
                            anyPlaying = true;
                            break;
                        }
                    }
                    
                    if (anyPlaying)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
            
            // 归还到对象池
            PoolManager.Return(gameObject);
        }

        /// <summary>
        /// 手动归还到对象池
        /// </summary>
        public void ReturnToPool()
        {
            PoolManager.Return(gameObject);
        }
    }
}