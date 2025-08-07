using Framework.SplUI.Core;
using UnityEngine;

namespace Framework.SplUI.Examples
{
    /// <summary>
    /// 示例面板 - 展示如何使用重构后的SplUIBase
    /// </summary>
    public class ExamplePanel : SplUIBase
    {
        [Header("示例面板配置")]
        [SerializeField] private UnityEngine.UI.Button closeButton;
        [SerializeField] private UnityEngine.UI.Text titleText;
        
        /// <summary>
        /// 子类初始化方法
        /// </summary>
        protected override void OnInitialize()
        {
            // 设置动画
            var animComponent = GetAnimationComponent();
            if (animComponent != null)
            {
                // 设置显示动画为淡入+缩放
                animComponent.ShowAnimation = SplUIAnimationType.FadeScale;
                animComponent.AnimationDuration = 0.3f;
                // 设置隐藏动画为淡出+缩放
                animComponent.HideAnimation = SplUIAnimationType.FadeScale;
            }
            
            // 绑定关闭按钮事件
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
        }
        
        /// <summary>
        /// 面板显示时调用
        /// </summary>
        /// <param name="data">传递的数据</param>
        protected override void OnShow(object data = null)
        {
            // 更新标题文本
            if (titleText != null && data is string title)
            {
                titleText.text = title;
            }
            else if (titleText != null)
            {
                titleText.text = "示例面板";
            }
        }
        
        /// <summary>
        /// 面板隐藏时调用
        /// </summary>
        protected override void OnHide()
        {
            // 清理逻辑
        }
        
        /// <summary>
        /// 关闭按钮点击事件
        /// </summary>
        private void OnCloseButtonClicked()
        {
            Hide();
        }
        
        /// <summary>
        /// 面板销毁时调用
        /// </summary>
        protected override void OnPanelDestroy()
        {
            // 清理事件绑定
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            }
        }
    }
}