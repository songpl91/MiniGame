using UnityEngine;
using UnityEngine.UI;

namespace Framework.Tutorial.Intrusive.Steps
{
    /// <summary>
    /// 点击按钮引导步骤
    /// 引导用户点击指定的UI按钮
    /// </summary>
    public class ClickButtonStep : TutorialStepBase
    {
        private Button targetButton;
        private GameObject highlightEffect;
        private string buttonPath;
        private string hintText;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">步骤ID</param>
        /// <param name="name">步骤名称</param>
        /// <param name="buttonPath">按钮路径或名称</param>
        /// <param name="hint">提示文本</param>
        /// <param name="skipable">是否可跳过</param>
        public ClickButtonStep(string id, string name, string buttonPath, string hint = "", bool skipable = true) 
            : base(id, name, skipable)
        {
            this.buttonPath = buttonPath;
            this.hintText = hint;
        }
        
        /// <summary>
        /// 步骤开始时的具体逻辑
        /// </summary>
        protected override void OnStepStart()
        {
            // 查找目标按钮
            FindTargetButton();
            
            if (targetButton == null)
            {
                Debug.LogError($"[ClickButtonStep] 找不到按钮: {buttonPath}");
                CompleteStep();
                return;
            }
            
            // 添加点击监听
            targetButton.onClick.AddListener(OnButtonClicked);
            
            // 创建高亮效果
            CreateHighlightEffect();
            
            // 显示提示
            ShowHint();
            
            Debug.Log($"[ClickButtonStep] 开始引导: {stepName} - 请点击按钮: {buttonPath}");
        }
        
        /// <summary>
        /// 查找目标按钮
        /// </summary>
        private void FindTargetButton()
        {
            // 首先尝试通过路径查找
            GameObject buttonObj = GameObject.Find(buttonPath);
            if (buttonObj != null)
            {
                targetButton = buttonObj.GetComponent<Button>();
            }
            
            // 如果没找到，尝试通过名称查找
            if (targetButton == null)
            {
                Button[] allButtons = Object.FindObjectsOfType<Button>();
                foreach (var button in allButtons)
                {
                    if (button.name == buttonPath || button.gameObject.name == buttonPath)
                    {
                        targetButton = button;
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// 创建高亮效果
        /// </summary>
        private void CreateHighlightEffect()
        {
            if (targetButton == null) return;
            
            // 创建一个简单的高亮边框
            highlightEffect = new GameObject("TutorialHighlight");
            highlightEffect.transform.SetParent(targetButton.transform, false);
            
            // 添加Image组件作为高亮效果
            Image highlight = highlightEffect.AddComponent<Image>();
            highlight.color = new Color(1f, 1f, 0f, 0.3f); // 半透明黄色
            
            // 设置RectTransform使其覆盖按钮
            RectTransform highlightRect = highlightEffect.GetComponent<RectTransform>();
            highlightRect.anchorMin = Vector2.zero;
            highlightRect.anchorMax = Vector2.one;
            highlightRect.offsetMin = Vector2.zero;
            highlightRect.offsetMax = Vector2.zero;
            
            // 添加简单的闪烁动画
            StartBlinkAnimation();
        }
        
        /// <summary>
        /// 开始闪烁动画
        /// </summary>
        private void StartBlinkAnimation()
        {
            if (highlightEffect == null) return;
            
            // 简单的透明度动画
            var image = highlightEffect.GetComponent<Image>();
            if (image != null)
            {
                // 这里可以使用DOTween或其他动画库，现在用简单的协程
            TutorialCoroutineHelper.StartCoroutineStatic(BlinkCoroutine(image));
            }
        }
        
        /// <summary>
        /// 闪烁协程
        /// </summary>
        private System.Collections.IEnumerator BlinkCoroutine(Image image)
        {
            while (highlightEffect != null && !isCompleted)
            {
                // 淡入
                for (float t = 0; t < 1; t += Time.deltaTime * 2)
                {
                    if (image == null) yield break;
                    Color color = image.color;
                    color.a = Mathf.Lerp(0.1f, 0.5f, t);
                    image.color = color;
                    yield return null;
                }
                
                // 淡出
                for (float t = 0; t < 1; t += Time.deltaTime * 2)
                {
                    if (image == null) yield break;
                    Color color = image.color;
                    color.a = Mathf.Lerp(0.5f, 0.1f, t);
                    image.color = color;
                    yield return null;
                }
            }
        }
        
        /// <summary>
        /// 显示提示
        /// </summary>
        private void ShowHint()
        {
            if (!string.IsNullOrEmpty(hintText))
            {
                Debug.Log($"[引导提示] {hintText}");
                // 这里可以显示UI提示框
            }
        }
        
        /// <summary>
        /// 按钮点击回调
        /// </summary>
        private void OnButtonClicked()
        {
            Debug.Log($"[ClickButtonStep] 按钮被点击: {buttonPath}");
            CompleteStep();
        }
        
        /// <summary>
        /// 步骤完成时的处理
        /// </summary>
        protected override void OnStepComplete()
        {
            Debug.Log($"[ClickButtonStep] 步骤完成: {stepName}");
            RemoveHighlightEffect();
        }
        
        /// <summary>
        /// 步骤被跳过时的处理
        /// </summary>
        protected override void OnStepSkip()
        {
            Debug.Log($"[ClickButtonStep] 步骤被跳过: {stepName}");
            RemoveHighlightEffect();
        }
        
        /// <summary>
        /// 步骤清理时的处理
        /// </summary>
        protected override void OnStepCleanup()
        {
            // 移除按钮监听
            if (targetButton != null)
            {
                targetButton.onClick.RemoveListener(OnButtonClicked);
            }
            
            RemoveHighlightEffect();
        }
        
        /// <summary>
        /// 移除高亮效果
        /// </summary>
        private void RemoveHighlightEffect()
        {
            if (highlightEffect != null)
            {
                Object.Destroy(highlightEffect);
                highlightEffect = null;
            }
        }
    }
}