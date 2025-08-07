using UnityEngine;
using UnityEngine.UI;

namespace Framework.Tutorial.Intrusive.Steps
{
    /// <summary>
    /// 显示消息引导步骤
    /// 显示引导消息，等待用户确认
    /// </summary>
    public class ShowMessageStep : TutorialStepBase
    {
        private string message;
        private float autoCompleteTime;
        private GameObject messageUI;
        private bool waitingForConfirm;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">步骤ID</param>
        /// <param name="name">步骤名称</param>
        /// <param name="message">要显示的消息</param>
        /// <param name="autoCompleteTime">自动完成时间（0表示需要手动确认）</param>
        /// <param name="skipable">是否可跳过</param>
        public ShowMessageStep(string id, string name, string message, float autoCompleteTime = 0f, bool skipable = true) 
            : base(id, name, skipable)
        {
            this.message = message;
            this.autoCompleteTime = autoCompleteTime;
        }
        
        /// <summary>
        /// 步骤开始时的具体逻辑
        /// </summary>
        protected override void OnStepStart()
        {
            Debug.Log($"[ShowMessageStep] 显示消息: {stepName} - {message}");
            
            // 创建消息UI
            CreateMessageUI();
            
            // 如果设置了自动完成时间，启动自动完成
            if (autoCompleteTime > 0)
            {
                TutorialCoroutineHelper.StartCoroutineStatic(AutoCompleteCoroutine());
            }
            else
            {
                waitingForConfirm = true;
            }
        }
        
        /// <summary>
        /// 创建消息UI
        /// </summary>
        private void CreateMessageUI()
        {
            // 创建Canvas
            GameObject canvasObj = new GameObject("TutorialMessageCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // 确保在最上层
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // 创建背景
            GameObject backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(canvasObj.transform, false);
            
            Image background = backgroundObj.AddComponent<Image>();
            background.color = new Color(0, 0, 0, 0.5f); // 半透明黑色背景
            
            RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            // 创建消息面板
            GameObject panelObj = new GameObject("MessagePanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            
            Image panel = panelObj.AddComponent<Image>();
            panel.color = new Color(0.2f, 0.2f, 0.2f, 0.9f); // 深灰色面板
            
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(600, 200);
            
            // 创建消息文本
            GameObject textObj = new GameObject("MessageText");
            textObj.transform.SetParent(panelObj.transform, false);
            
            Text messageText = textObj.AddComponent<Text>();
            messageText.text = message;
            messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            messageText.fontSize = 24;
            messageText.color = Color.white;
            messageText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20, 60);
            textRect.offsetMax = new Vector2(-20, -20);
            
            // 如果需要手动确认，创建确认按钮
            if (autoCompleteTime <= 0)
            {
                CreateConfirmButton(panelObj);
            }
            
            messageUI = canvasObj;
        }
        
        /// <summary>
        /// 创建确认按钮
        /// </summary>
        private void CreateConfirmButton(GameObject parent)
        {
            GameObject buttonObj = new GameObject("ConfirmButton");
            buttonObj.transform.SetParent(parent.transform, false);
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.6f, 1f, 1f); // 蓝色按钮
            
            Button button = buttonObj.AddComponent<Button>();
            button.onClick.AddListener(OnConfirmClicked);
            
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0f);
            buttonRect.anchoredPosition = new Vector2(0, 10);
            buttonRect.sizeDelta = new Vector2(120, 40);
            
            // 按钮文本
            GameObject buttonTextObj = new GameObject("ButtonText");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            
            Text buttonText = buttonTextObj.AddComponent<Text>();
            buttonText.text = "确认";
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 18;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
        }
        
        /// <summary>
        /// 确认按钮点击回调
        /// </summary>
        private void OnConfirmClicked()
        {
            Debug.Log($"[ShowMessageStep] 用户确认消息: {stepName}");
            CompleteStep();
        }
        
        /// <summary>
        /// 自动完成协程
        /// </summary>
        private System.Collections.IEnumerator AutoCompleteCoroutine()
        {
            yield return new WaitForSeconds(autoCompleteTime);
            
            if (!isCompleted)
            {
                Debug.Log($"[ShowMessageStep] 消息自动完成: {stepName}");
                CompleteStep();
            }
        }
        
        /// <summary>
        /// 步骤完成时的处理
        /// </summary>
        protected override void OnStepComplete()
        {
            waitingForConfirm = false;
            DestroyMessageUI();
            Debug.Log($"[ShowMessageStep] 消息步骤完成: {stepName}");
        }
        
        /// <summary>
        /// 步骤被跳过时的处理
        /// </summary>
        protected override void OnStepSkip()
        {
            waitingForConfirm = false;
            DestroyMessageUI();
            Debug.Log($"[ShowMessageStep] 消息步骤被跳过: {stepName}");
        }
        
        /// <summary>
        /// 步骤清理时的处理
        /// </summary>
        protected override void OnStepCleanup()
        {
            waitingForConfirm = false;
            DestroyMessageUI();
        }
        
        /// <summary>
        /// 销毁消息UI
        /// </summary>
        private void DestroyMessageUI()
        {
            if (messageUI != null)
            {
                Object.Destroy(messageUI);
                messageUI = null;
            }
        }
    }
}