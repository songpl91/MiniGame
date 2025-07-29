using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// using DG.Tweening;

/// <summary>
/// 通用的Toggle切换控制器，支持切换图片和文字
/// </summary>
public class ToggleSwitcher : MonoBehaviour
{
    [Header("目标Toggle")]
    [HideInInspector]public Toggle targetToggle;

    [Header("图片切换设置")]
    public bool switchImage = false;
    [HideInInspector]public Image targetImage;
    public Sprite onSprite;
    public Sprite offSprite;
    
    [Header("文字切换设置")]
    public bool switchText = false;
    public Text targetText;       // UGUI Text组件
    public TextMeshProUGUI targetTMP; // TextMeshPro组件
    public string onText = "ON";
    public string offText = "OFF";
    
    [Header("颜色切换设置")]
    public bool switchColor = false;
    public Color onColor = Color.white;
    public Color offColor = Color.white;

    [Header("动画设置")]
    public bool useAnimation = false;
    public float animationDuration = 0.2f;
    
    [Tooltip("Toggle状态变化时的回调函数")]
    public Action<bool> OnChangeState;

    /// <summary>
    /// 获取当前Toggle的状态
    /// </summary>
    public bool IsOn => targetToggle != null ? targetToggle.isOn : false;

    private bool _initialized = false;

    void Awake()
    {
        InitializeIfNeeded();
    }

    /// <summary>
    /// 初始化组件，检查必要的引用
    /// </summary>
    private void InitializeIfNeeded()
    {
        if (_initialized) return;
        
        // 检查必要组件
        if (targetToggle == null)
        {
            // 尝试自动查找Toggle组件
            targetToggle = GetComponent<Toggle>();
            if (targetToggle == null)
            {
                targetToggle = GetComponentInChildren<Toggle>();
                if (targetToggle == null)
                {
                    Debug.LogError($"[ToggleSwitcher] {gameObject.name}: targetToggle未分配，且无法自动查找到Toggle组件！");
                    enabled = false;
                    return;
                }
            }
        }
        
        // 检查图片切换相关组件
        if (switchImage)
        {
            if (targetImage == null)
            {
                // 尝试使用Toggle的图片组件
                targetImage = targetToggle.graphic as Image;
                if (targetImage == null)
                {
                    Debug.LogWarning($"[ToggleSwitcher] {gameObject.name}: 图片切换功能缺少图片组件！");
                    switchImage = false;
                }
                else
                {
                    targetToggle.graphic = null;
                }
            }
            
            if (onSprite == null || offSprite == null)
            {
                Debug.LogWarning($"[ToggleSwitcher] {gameObject.name}: 图片切换功能缺少精灵资源！");
                switchImage = false;
            }
        }
        
        // 检查文字切换相关组件
        if (switchText && targetText == null && targetTMP == null)
        {
            // 尝试自动查找文本组件
            targetText = GetComponentInChildren<Text>();
            targetTMP = GetComponentInChildren<TextMeshProUGUI>();
            
            if (targetText == null && targetTMP == null)
            {
                Debug.LogWarning($"[ToggleSwitcher] {gameObject.name}: 文字切换功能缺少文本组件！");
                switchText = false;
            }
        }
        
        _initialized = true;
    }

    /// <summary>
    /// 初始化Toggle的状态
    /// </summary>
    /// <param name="isOn">是否开启</param>
    public void Init(bool isOn)
    {
        InitializeIfNeeded();
        
        // 静默设置状态（不触发回调）
        if (targetToggle != null)
        {
            bool oldValue = targetToggle.isOn;
            targetToggle.onValueChanged.RemoveListener(ApplyToggleState);
            targetToggle.isOn = isOn;
            targetToggle.onValueChanged.AddListener(ApplyToggleState);
            
            // 手动应用状态
            ApplyToggleState(isOn);
            
            // 仅在状态确实改变时调用回调函数
            if (oldValue != isOn)
            {
                OnChangeState?.Invoke(isOn);
            }
        }
    }

    private void OnEnable()
    {
        InitializeIfNeeded();
        
        if (targetToggle != null)
        {
            targetToggle.onValueChanged.AddListener(ApplyToggleState);
            // 确保状态与UI显示一致
            ApplyToggleState(targetToggle.isOn);
        }
    }

    private void OnDisable()
    {
        if (targetToggle != null)
        {
            targetToggle.onValueChanged.RemoveListener(ApplyToggleState);
        }
    }
    
    /// <summary>
    /// 根据Toggle状态应用相应的图片和文字
    /// </summary>
    /// <param name="isOn">Toggle是否开启</param>
    void ApplyToggleState(bool isOn)
    {
        // 切换图片
        if (switchImage && targetImage != null)
        {
            targetImage.sprite = isOn ? onSprite : offSprite;
            targetImage.SetNativeSize();
            
            // 如果使用动画，添加过渡效果
            if (useAnimation)
            {
                targetImage.transform.localScale = Vector3.one * 0.8f;
                // targetImage.transform.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack);
            }
        }
        
        // 切换文字
        if (switchText)
        {
            string text = isOn ? onText : offText;
            
            if (targetText != null)
                targetText.text = text;
                
            if (targetTMP != null)
                targetTMP.text = text;
        }
        
        // 切换颜色
        if (switchColor)
        {
            Color color = isOn ? onColor : offColor;
            
            if (switchImage && targetImage != null)
            {
                if (useAnimation)
                {
                    // targetImage.DOColor(color, animationDuration);
                }
                else
                {
                    targetImage.color = color;
                }
            }
                
            if (switchText)
            {
                if (targetText != null)
                {
                    if (useAnimation)
                    {
                        // targetText.DOColor(color, animationDuration);
                    }
                    else
                    {
                        targetText.color = color;
                    }
                }
                    
                if (targetTMP != null)
                {
                    if (useAnimation)
                    {
                        // targetTMP.DOColor(color, animationDuration);
                    }
                    else
                    {
                        targetTMP.color = color;
                    }
                }
            }
        }
        
        OnChangeState?.Invoke(isOn);
    }
    
    /// <summary>
    /// 手动设置Toggle的状态
    /// </summary>
    /// <param name="value">是否开启</param>
    /// <param name="sendCallback">是否触发回调事件</param>
    public void SetToggleValue(bool value, bool sendCallback = true)
    {
        if (targetToggle != null)
        {
            if (!sendCallback)
            {
                // 暂时移除监听器
                targetToggle.onValueChanged.RemoveListener(ApplyToggleState);
                targetToggle.isOn = value;
                targetToggle.onValueChanged.AddListener(ApplyToggleState);
                
                // 手动应用状态
                ApplyToggleState(value);
            }
            else
            {
                targetToggle.isOn = value;
            }
        }
    }
    
    /// <summary>
    /// 切换Toggle状态（开/关切换）
    /// </summary>
    public void ToggleValue()
    {
        if (targetToggle != null)
        {
            targetToggle.isOn = !targetToggle.isOn;
        }
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// 在编辑器中预览Toggle状态
    /// </summary>
    [ContextMenu("预览ON状态")]
    private void PreviewOnState()
    {
        if (!Application.isPlaying)
        {
            InitializeIfNeeded();
            ApplyToggleState(true); 
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
    
    /// <summary>
    /// 在编辑器中预览Toggle状态
    /// </summary>
    [ContextMenu("预览OFF状态")]
    private void PreviewOffState()
    {
        if (!Application.isPlaying)
        {
            InitializeIfNeeded();
            ApplyToggleState(false);
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}