using UnityEngine;

namespace Framework.SplUI.Core
{
    /// <summary>
    /// SplUI组件基类
    /// 为UI组件提供基础实现和生命周期管理
    /// </summary>
    public abstract class SplUIComponent : MonoBehaviour, ISplUIComponent
    {
        [Header("组件设置")]
        [SerializeField]
        [Tooltip("组件是否自动初始化")]
        protected bool autoInitialize = true;
        
        [SerializeField]
        [Tooltip("组件优先级（数值越小优先级越高）")]
        protected int priority = 0;
        
        /// <summary>
        /// 所属面板
        /// </summary>
        public SplUIBase OwnerPanel { get; private set; }
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// 组件优先级
        /// </summary>
        public int Priority => priority;
        
        /// <summary>
        /// 组件是否激活
        /// </summary>
        public bool IsActive => gameObject.activeInHierarchy;
        
        /// <summary>
        /// Unity生命周期：Awake
        /// </summary>
        protected virtual void Awake()
        {
            // 自动查找所属面板
            OwnerPanel = GetComponentInParent<SplUIBase>();
            
            if (OwnerPanel == null)
            {
                Debug.LogWarning($"[SplUIComponent] 组件 {name} 未找到所属面板", this);
            }
        }
        
        /// <summary>
        /// Unity生命周期：Start
        /// </summary>
        protected virtual void Start()
        {
            if (autoInitialize && !IsInitialized)
            {
                Initialize();
            }
        }
        
        /// <summary>
        /// Unity生命周期：OnDestroy
        /// </summary>
        protected virtual void OnDestroy()
        {
            OnComponentDestroy();
        }
        
        /// <summary>
        /// 初始化组件
        /// </summary>
        public virtual void Initialize()
        {
            if (IsInitialized)
            {
                Debug.LogWarning($"[SplUIComponent] 组件 {name} 已经初始化", this);
                return;
            }
            
            OnInitialize();
            IsInitialized = true;
            
            // 注册到所属面板
            if (OwnerPanel != null)
            {
                OwnerPanel.RegisterComponent(this);
            }
        }
        
        /// <summary>
        /// 初始化组件（带面板参数）
        /// 实现ISplUIComponent接口的Initialize(SplUIBase)方法
        /// </summary>
        /// <param name="ownerPanel">所属面板</param>
        public virtual void Initialize(SplUIBase ownerPanel)
        {
            // 设置所属面板
            SetOwnerPanel(ownerPanel);
            
            // 调用无参数的Initialize方法
            Initialize();
        }
        
        /// <summary>
        /// 更新组件
        /// </summary>
        public virtual void OnUpdate()
        {
            if (!IsInitialized || !IsActive)
                return;
            
            OnComponentUpdate();
        }
        
        /// <summary>
        /// 销毁组件（实现ISplUIComponent接口）
        /// 用于外部调用的组件销毁方法
        /// </summary>
        void ISplUIComponent.OnDestroy()
        {
            OnComponentDestroy();
        }
        
        /// <summary>
        /// 设置所属面板
        /// </summary>
        /// <param name="panel">面板实例</param>
        public void SetOwnerPanel(SplUIBase panel)
        {
            OwnerPanel = panel;
        }
        
        /// <summary>
        /// 激活组件
        /// </summary>
        public virtual void Activate()
        {
            gameObject.SetActive(true);
            OnActivate();
        }
        
        /// <summary>
        /// 停用组件
        /// </summary>
        public virtual void Deactivate()
        {
            OnDeactivate();
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 重置组件
        /// </summary>
        public virtual void Reset()
        {
            OnReset();
        }
        
        /// <summary>
        /// 子类重写：初始化逻辑
        /// </summary>
        protected abstract void OnInitialize();
        
        /// <summary>
        /// 子类重写：更新逻辑
        /// </summary>
        protected virtual void OnComponentUpdate() { }
        
        /// <summary>
        /// 子类重写：销毁逻辑
        /// </summary>
        protected virtual void OnComponentDestroy() { }
        
        /// <summary>
        /// 子类重写：激活逻辑
        /// </summary>
        protected virtual void OnActivate() { }
        
        /// <summary>
        /// 子类重写：停用逻辑
        /// </summary>
        protected virtual void OnDeactivate() { }
        
        /// <summary>
        /// 子类重写：重置逻辑
        /// </summary>
        protected virtual void OnReset() { }
        
        /// <summary>
        /// 获取组件类型名称
        /// </summary>
        /// <returns>类型名称</returns>
        public string GetComponentTypeName()
        {
            return GetType().Name;
        }
        
        /// <summary>
        /// 检查组件是否有效
        /// </summary>
        /// <returns>是否有效</returns>
        public virtual bool IsValid()
        {
            return this != null && gameObject != null;
        }
    }
}