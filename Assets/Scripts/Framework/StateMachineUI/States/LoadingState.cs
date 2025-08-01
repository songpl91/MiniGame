// using UnityEngine;
// using UnityEngine.UI;
// using Framework.StateMachineUI.Core;
// using System.Collections;
// using System.Collections.Generic;
//
// namespace Framework.StateMachineUI.States
// {
//     /// <summary>
//     /// 加载状态
//     /// 管理游戏加载界面和资源加载过程
//     /// </summary>
//     public class LoadingState : UIStateBase
//     {
//         #region UI组件引用
//         
//         [Header("加载显示")]
//         [SerializeField] private Text loadingText;
//         [SerializeField] private Text progressText;
//         [SerializeField] private Text tipText;
//         [SerializeField] private Slider progressBar;
//         [SerializeField] private Image progressFill;
//         [SerializeField] private Image backgroundImage;
//         
//         [Header("动画组件")]
//         [SerializeField] private Animator loadingAnimator;
//         [SerializeField] private Image loadingIcon;
//         [SerializeField] private Transform loadingSpinner;
//         [SerializeField] private ParticleSystem loadingParticles;
//         
//         [Header("提示信息")]
//         [SerializeField] private Text[] tipTexts;
//         [SerializeField] private Image[] tipImages;
//         
//         #endregion
//         
//         #region 私有变量
//         
//         private bool isInitialized = false;
//         private float currentProgress = 0f;
//         private float targetProgress = 0f;
//         private string currentLoadingText = "加载中...";
//         private string targetStateName;
//         private object targetStateData;
//         
//         // 加载任务
//         private Queue<LoadingTask> loadingTasks = new Queue<LoadingTask>();
//         private LoadingTask currentTask;
//         private bool isLoading = false;
//         
//         // 提示信息
//         private string[] loadingTips = {
//             "游戏小贴士：合理利用道具可以帮助你更好地完成关卡",
//             "游戏小贴士：注意观察敌人的行动模式",
//             "游戏小贴士：收集金币可以购买更强大的装备",
//             "游戏小贴士：善用暂停功能来制定策略",
//             "游戏小贴士：不同的角色有不同的特殊能力"
//         };
//         
//         private int currentTipIndex = 0;
//         private float tipChangeInterval = 3f;
//         private float tipTimer = 0f;
//         
//         // 动画相关
//         private float spinnerSpeed = 360f;
//         private bool isSpinning = false;
//         
//         #endregion
//         
//         #region 构造函数
//         
//         public LoadingState()
//         {
//             StateName = "Loading";
//             StateType = UIStateType.Exclusive; // 加载状态是独占的
//             Priority = 20; // 最高优先级
//             CanBeInterrupted = false; // 加载状态不能被中断
//         }
//         
//         #endregion
//         
//         #region 状态生命周期
//         
//         public override void OnEnter(object data = null)
//         {
//             base.OnEnter(data);
//             
//             Debug.Log("[加载状态] 开始加载");
//             
//             // 处理传入的数据
//             if (data is LoadingData loadingData)
//             {
//                 targetStateName = loadingData.TargetStateName;
//                 targetStateData = loadingData.TargetStateData;
//                 
//                 if (loadingData.LoadingTasks != null && loadingData.LoadingTasks.Count > 0)
//                 {
//                     loadingTasks = new Queue<LoadingTask>(loadingData.LoadingTasks);
//                 }
//             }
//             
//             // 创建或获取UI
//             CreateUI();
//             
//             // 初始化UI
//             InitializeUI();
//             
//             // 开始加载过程
//             StartLoading();
//         }
//         
//         public override void OnUpdate(float deltaTime)
//         {
//             base.OnUpdate(deltaTime);
//             
//             // 更新进度条动画
//             UpdateProgressBar(deltaTime);
//             
//             // 更新加载图标旋转
//             UpdateLoadingSpinner(deltaTime);
//             
//             // 更新提示信息
//             UpdateTips(deltaTime);
//             
//             // 处理加载任务
//             ProcessLoadingTasks();
//         }
//         
//         public override void OnExit()
//         {
//             Debug.Log("[加载状态] 加载完成");
//             
//             // 停止所有动画
//             StopLoadingAnimations();
//             
//             // 清理加载任务
//             ClearLoadingTasks();
//             
//             // 清理UI
//             CleanupUI();
//             
//             base.OnExit();
//         }
//         
//         public override void OnPause()
//         {
//             base.OnPause();
//             Debug.Log("[加载状态] 加载状态被暂停");
//             
//             // 暂停加载动画
//             PauseLoadingAnimations();
//         }
//         
//         public override void OnResume()
//         {
//             base.OnResume();
//             Debug.Log("[加载状态] 加载状态恢复");
//             
//             // 恢复加载动画
//             ResumeLoadingAnimations();
//         }
//         
//         #endregion
//         
//         #region UI创建和初始化
//         
//         /// <summary>
//         /// 创建UI
//         /// </summary>
//         private void CreateUI()
//         {
//             if (uiGameObject != null)
//                 return;
//             
//             // 从UI管理器加载预制体
//             if (uiManager != null)
//             {
//                 Transform uiRoot = uiManager.GetUIRoot(StateType);
//                 uiGameObject = uiManager.InstantiateStateUI(StateName, uiRoot);
//                 
//                 if (uiGameObject != null)
//                 {
//                     GetUIComponents();
//                 }
//                 else
//                 {
//                     Debug.LogWarning($"[加载状态] 无法加载UI预制体: {StateName}");
//                     CreateDefaultUI();
//                 }
//             }
//             else
//             {
//                 CreateDefaultUI();
//             }
//         }
//         
//         /// <summary>
//         /// 获取UI组件引用
//         /// </summary>
//         private void GetUIComponents()
//         {
//             if (uiGameObject == null)
//                 return;
//             
//             // 获取加载显示组件
//             loadingText = FindUIComponent<Text>("LoadingText");
//             progressText = FindUIComponent<Text>("ProgressText");
//             tipText = FindUIComponent<Text>("TipText");
//             progressBar = FindUIComponent<Slider>("ProgressBar");
//             progressFill = FindUIComponent<Image>("ProgressFill");
//             backgroundImage = FindUIComponent<Image>("BackgroundImage");
//             
//             // 获取动画组件
//             loadingAnimator = uiGameObject.GetComponent<Animator>();
//             loadingIcon = FindUIComponent<Image>("LoadingIcon");
//             loadingSpinner = FindUIComponent<Transform>("LoadingSpinner");
//             loadingParticles = FindUIComponent<ParticleSystem>("LoadingParticles");
//             
//             // 获取提示信息组件
//             tipTexts = FindUIComponents<Text>("TipText");
//             tipImages = FindUIComponents<Image>("TipImage");
//         }
//         
//         /// <summary>
//         /// 创建默认UI
//         /// </summary>
//         private void CreateDefaultUI()
//         {
//             Debug.Log("[加载状态] 创建默认加载UI");
//             
//             // 创建根对象
//             uiGameObject = new GameObject("LoadingUI");
//             uiGameObject.transform.SetParent(uiManager?.GetUIRoot(StateType) ?? GameObject.Find("Canvas")?.transform);
//             
//             // 添加Canvas组件
//             Canvas canvas = uiGameObject.AddComponent<Canvas>();
//             canvas.overrideSorting = true;
//             canvas.sortingOrder = 200; // 最高层级
//             
//             // 创建背景
//             CreateBackground();
//             
//             // 创建加载内容
//             CreateLoadingContent();
//             
//             // 设置UI位置和大小
//             RectTransform rectTransform = uiGameObject.GetComponent<RectTransform>();
//             rectTransform.anchorMin = Vector2.zero;
//             rectTransform.anchorMax = Vector2.one;
//             rectTransform.offsetMin = Vector2.zero;
//             rectTransform.offsetMax = Vector2.zero;
//         }
//         
//         /// <summary>
//         /// 创建背景
//         /// </summary>
//         private void CreateBackground()
//         {
//             GameObject background = new GameObject("Background");
//             background.transform.SetParent(uiGameObject.transform);
//             
//             backgroundImage = background.AddComponent<Image>();
//             backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
//             
//             RectTransform backgroundRect = background.GetComponent<RectTransform>();
//             backgroundRect.anchorMin = Vector2.zero;
//             backgroundRect.anchorMax = Vector2.one;
//             backgroundRect.offsetMin = Vector2.zero;
//             backgroundRect.offsetMax = Vector2.zero;
//         }
//         
//         /// <summary>
//         /// 创建加载内容
//         /// </summary>
//         private void CreateLoadingContent()
//         {
//             // 创建加载文本
//             CreateLoadingText();
//             
//             // 创建进度条
//             CreateProgressBar();
//             
//             // 创建加载图标
//             CreateLoadingIcon();
//             
//             // 创建提示文本
//             CreateTipText();
//         }
//         
//         /// <summary>
//         /// 创建加载文本
//         /// </summary>
//         private void CreateLoadingText()
//         {
//             GameObject textObj = new GameObject("LoadingText");
//             textObj.transform.SetParent(uiGameObject.transform);
//             
//             loadingText = textObj.AddComponent<Text>();
//             loadingText.text = "加载中...";
//             loadingText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//             loadingText.fontSize = 24;
//             loadingText.color = Color.white;
//             loadingText.alignment = TextAnchor.MiddleCenter;
//             
//             RectTransform textRect = textObj.GetComponent<RectTransform>();
//             textRect.anchorMin = new Vector2(0, 0.6f);
//             textRect.anchorMax = new Vector2(1, 0.7f);
//             textRect.offsetMin = Vector2.zero;
//             textRect.offsetMax = Vector2.zero;
//         }
//         
//         /// <summary>
//         /// 创建进度条
//         /// </summary>
//         private void CreateProgressBar()
//         {
//             GameObject progressObj = new GameObject("ProgressBar");
//             progressObj.transform.SetParent(uiGameObject.transform);
//             
//             progressBar = progressObj.AddComponent<Slider>();
//             progressBar.minValue = 0f;
//             progressBar.maxValue = 1f;
//             progressBar.value = 0f;
//             
//             // 创建背景
//             GameObject backgroundObj = new GameObject("Background");
//             backgroundObj.transform.SetParent(progressObj.transform);
//             
//             Image backgroundImg = backgroundObj.AddComponent<Image>();
//             backgroundImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
//             
//             RectTransform backgroundRect = backgroundObj.GetComponent<RectTransform>();
//             backgroundRect.anchorMin = Vector2.zero;
//             backgroundRect.anchorMax = Vector2.one;
//             backgroundRect.offsetMin = Vector2.zero;
//             backgroundRect.offsetMax = Vector2.zero;
//             
//             // 创建填充区域
//             GameObject fillAreaObj = new GameObject("Fill Area");
//             fillAreaObj.transform.SetParent(progressObj.transform);
//             
//             RectTransform fillAreaRect = fillAreaObj.GetComponent<RectTransform>();
//             fillAreaRect.anchorMin = Vector2.zero;
//             fillAreaRect.anchorMax = Vector2.one;
//             fillAreaRect.offsetMin = Vector2.zero;
//             fillAreaRect.offsetMax = Vector2.zero;
//             
//             // 创建填充
//             GameObject fillObj = new GameObject("Fill");
//             fillObj.transform.SetParent(fillAreaObj.transform);
//             
//             progressFill = fillObj.AddComponent<Image>();
//             progressFill.color = new Color(0.2f, 0.8f, 0.2f, 1f);
//             progressFill.type = Image.Type.Filled;
//             progressFill.fillMethod = Image.FillMethod.Horizontal;
//             
//             RectTransform fillRect = fillObj.GetComponent<RectTransform>();
//             fillRect.anchorMin = Vector2.zero;
//             fillRect.anchorMax = Vector2.one;
//             fillRect.offsetMin = Vector2.zero;
//             fillRect.offsetMax = Vector2.zero;
//             
//             // 设置Slider组件引用
//             progressBar.fillRect = fillRect;
//             progressBar.targetGraphic = progressFill;
//             
//             // 设置进度条位置
//             RectTransform progressRect = progressObj.GetComponent<RectTransform>();
//             progressRect.anchorMin = new Vector2(0.2f, 0.45f);
//             progressRect.anchorMax = new Vector2(0.8f, 0.5f);
//             progressRect.offsetMin = Vector2.zero;
//             progressRect.offsetMax = Vector2.zero;
//             
//             // 创建进度文本
//             GameObject progressTextObj = new GameObject("ProgressText");
//             progressTextObj.transform.SetParent(uiGameObject.transform);
//             
//             progressText = progressTextObj.AddComponent<Text>();
//             progressText.text = "0%";
//             progressText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//             progressText.fontSize = 16;
//             progressText.color = Color.white;
//             progressText.alignment = TextAnchor.MiddleCenter;
//             
//             RectTransform progressTextRect = progressTextObj.GetComponent<RectTransform>();
//             progressTextRect.anchorMin = new Vector2(0, 0.4f);
//             progressTextRect.anchorMax = new Vector2(1, 0.45f);
//             progressTextRect.offsetMin = Vector2.zero;
//             progressTextRect.offsetMax = Vector2.zero;
//         }
//         
//         /// <summary>
//         /// 创建加载图标
//         /// </summary>
//         private void CreateLoadingIcon()
//         {
//             GameObject iconObj = new GameObject("LoadingIcon");
//             iconObj.transform.SetParent(uiGameObject.transform);
//             
//             loadingIcon = iconObj.AddComponent<Image>();
//             loadingIcon.color = Color.white;
//             
//             // 创建简单的圆形图标
//             Texture2D iconTexture = CreateCircleTexture(64, Color.white);
//             Sprite iconSprite = Sprite.Create(iconTexture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f);
//             loadingIcon.sprite = iconSprite;
//             
//             RectTransform iconRect = iconObj.GetComponent<RectTransform>();
//             iconRect.anchorMin = new Vector2(0.5f, 0.7f);
//             iconRect.anchorMax = new Vector2(0.5f, 0.7f);
//             iconRect.sizeDelta = new Vector2(64, 64);
//             iconRect.anchoredPosition = Vector2.zero;
//             
//             loadingSpinner = iconObj.transform;
//         }
//         
//         /// <summary>
//         /// 创建提示文本
//         /// </summary>
//         private void CreateTipText()
//         {
//             GameObject tipObj = new GameObject("TipText");
//             tipObj.transform.SetParent(uiGameObject.transform);
//             
//             tipText = tipObj.AddComponent<Text>();
//             tipText.text = loadingTips[0];
//             tipText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//             tipText.fontSize = 14;
//             tipText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
//             tipText.alignment = TextAnchor.MiddleCenter;
//             
//             RectTransform tipRect = tipObj.GetComponent<RectTransform>();
//             tipRect.anchorMin = new Vector2(0.1f, 0.1f);
//             tipRect.anchorMax = new Vector2(0.9f, 0.2f);
//             tipRect.offsetMin = Vector2.zero;
//             tipRect.offsetMax = Vector2.zero;
//         }
//         
//         /// <summary>
//         /// 创建圆形纹理
//         /// </summary>
//         private Texture2D CreateCircleTexture(int size, Color color)
//         {
//             Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
//             Color[] pixels = new Color[size * size];
//             
//             Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
//             float radius = size * 0.4f;
//             
//             for (int y = 0; y < size; y++)
//             {
//                 for (int x = 0; x < size; x++)
//                 {
//                     Vector2 pos = new Vector2(x, y);
//                     float distance = Vector2.Distance(pos, center);
//                     
//                     if (distance <= radius)
//                     {
//                         float alpha = 1f - (distance / radius);
//                         pixels[y * size + x] = new Color(color.r, color.g, color.b, alpha);
//                     }
//                     else
//                     {
//                         pixels[y * size + x] = Color.clear;
//                     }
//                 }
//             }
//             
//             texture.SetPixels(pixels);
//             texture.Apply();
//             
//             return texture;
//         }
//         
//         /// <summary>
//         /// 初始化UI
//         /// </summary>
//         private void InitializeUI()
//         {
//             if (isInitialized)
//                 return;
//             
//             // 设置初始状态
//             SetInitialUIState();
//             
//             // 开始动画
//             StartLoadingAnimations();
//             
//             isInitialized = true;
//         }
//         
//         /// <summary>
//         /// 设置初始UI状态
//         /// </summary>
//         private void SetInitialUIState()
//         {
//             if (progressBar != null)
//                 progressBar.value = 0f;
//             
//             if (progressText != null)
//                 progressText.text = "0%";
//             
//             if (loadingText != null)
//                 loadingText.text = currentLoadingText;
//             
//             if (tipText != null)
//                 tipText.text = loadingTips[0];
//         }
//         
//         #endregion
//         
//         #region 加载处理
//         
//         /// <summary>
//         /// 开始加载
//         /// </summary>
//         private void StartLoading()
//         {
//             isLoading = true;
//             
//             // 如果没有指定加载任务，创建默认任务
//             if (loadingTasks.Count == 0)
//             {
//                 CreateDefaultLoadingTasks();
//             }
//             
//             Debug.Log($"[加载状态] 开始加载，共 {loadingTasks.Count} 个任务");
//         }
//         
//         /// <summary>
//         /// 创建默认加载任务
//         /// </summary>
//         private void CreateDefaultLoadingTasks()
//         {
//             loadingTasks.Enqueue(new LoadingTask
//             {
//                 TaskName = "初始化系统",
//                 Duration = 1f,
//                 Action = () => Debug.Log("初始化系统完成")
//             });
//             
//             loadingTasks.Enqueue(new LoadingTask
//             {
//                 TaskName = "加载资源",
//                 Duration = 2f,
//                 Action = () => Debug.Log("加载资源完成")
//             });
//             
//             loadingTasks.Enqueue(new LoadingTask
//             {
//                 TaskName = "准备场景",
//                 Duration = 1.5f,
//                 Action = () => Debug.Log("准备场景完成")
//             });
//         }
//         
//         /// <summary>
//         /// 处理加载任务
//         /// </summary>
//         private void ProcessLoadingTasks()
//         {
//             if (!isLoading)
//                 return;
//             
//             // 如果当前没有任务，获取下一个任务
//             if (currentTask == null && loadingTasks.Count > 0)
//             {
//                 currentTask = loadingTasks.Dequeue();
//                 currentTask.StartTime = Time.time;
//                 
//                 Debug.Log($"[加载状态] 开始任务: {currentTask.TaskName}");
//                 UpdateLoadingText(currentTask.TaskName);
//             }
//             
//             // 处理当前任务
//             if (currentTask != null)
//             {
//                 float elapsed = Time.time - currentTask.StartTime;
//                 float progress = Mathf.Clamp01(elapsed / currentTask.Duration);
//                 
//                 // 更新任务进度
//                 UpdateTaskProgress(progress);
//                 
//                 // 任务完成
//                 if (progress >= 1f)
//                 {
//                     currentTask.Action?.Invoke();
//                     currentTask = null;
//                 }
//             }
//             
//             // 所有任务完成
//             if (currentTask == null && loadingTasks.Count == 0)
//             {
//                 CompleteLoading();
//             }
//         }
//         
//         /// <summary>
//         /// 更新任务进度
//         /// </summary>
//         private void UpdateTaskProgress(float taskProgress)
//         {
//             // 计算总进度
//             int totalTasks = loadingTasks.Count + (currentTask != null ? 1 : 0);
//             int completedTasks = GetCompletedTaskCount();
//             
//             float totalProgress = (completedTasks + taskProgress) / (totalTasks + completedTasks);
//             SetTargetProgress(totalProgress);
//         }
//         
//         /// <summary>
//         /// 获取已完成任务数量
//         /// </summary>
//         private int GetCompletedTaskCount()
//         {
//             // 这里可以根据实际情况计算
//             return 0; // 简化实现
//         }
//         
//         /// <summary>
//         /// 完成加载
//         /// </summary>
//         private void CompleteLoading()
//         {
//             isLoading = false;
//             SetTargetProgress(1f);
//             UpdateLoadingText("加载完成");
//             
//             Debug.Log("[加载状态] 所有加载任务完成");
//             
//             // 延迟一段时间后转换到目标状态
//             StartCoroutine(CompleteLoadingCoroutine());
//         }
//         
//         /// <summary>
//         /// 完成加载协程
//         /// </summary>
//         private IEnumerator CompleteLoadingCoroutine()
//         {
//             // 等待进度条动画完成
//             yield return new WaitForSeconds(0.5f);
//             
//             // 转换到目标状态
//             if (!string.IsNullOrEmpty(targetStateName))
//             {
//                 uiManager?.GetStateMachine()?.TransitionToState(targetStateName, targetStateData);
//             }
//             else
//             {
//                 // 默认转换到主菜单
//                 uiManager?.GetStateMachine()?.TransitionToState("MainMenu");
//             }
//         }
//         
//         #endregion
//         
//         #region UI更新
//         
//         /// <summary>
//         /// 更新进度条
//         /// </summary>
//         private void UpdateProgressBar(float deltaTime)
//         {
//             if (progressBar == null)
//                 return;
//             
//             // 平滑更新进度
//             if (Mathf.Abs(currentProgress - targetProgress) > 0.001f)
//             {
//                 currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, deltaTime * 2f);
//                 progressBar.value = currentProgress;
//                 
//                 // 更新进度文本
//                 if (progressText != null)
//                 {
//                     progressText.text = $"{Mathf.RoundToInt(currentProgress * 100)}%";
//                 }
//             }
//         }
//         
//         /// <summary>
//         /// 更新加载图标旋转
//         /// </summary>
//         private void UpdateLoadingSpinner(float deltaTime)
//         {
//             if (loadingSpinner != null && isSpinning)
//             {
//                 loadingSpinner.Rotate(0, 0, -spinnerSpeed * deltaTime);
//             }
//         }
//         
//         /// <summary>
//         /// 更新提示信息
//         /// </summary>
//         private void UpdateTips(float deltaTime)
//         {
//             tipTimer += deltaTime;
//             
//             if (tipTimer >= tipChangeInterval)
//             {
//                 tipTimer = 0f;
//                 currentTipIndex = (currentTipIndex + 1) % loadingTips.Length;
//                 
//                 if (tipText != null)
//                 {
//                     StartCoroutine(ChangeTipText(loadingTips[currentTipIndex]));
//                 }
//             }
//         }
//         
//         /// <summary>
//         /// 改变提示文本协程
//         /// </summary>
//         private IEnumerator ChangeTipText(string newTip)
//         {
//             if (tipText == null)
//                 yield break;
//             
//             // 淡出
//             float duration = 0.3f;
//             float elapsed = 0f;
//             Color originalColor = tipText.color;
//             
//             while (elapsed < duration)
//             {
//                 elapsed += Time.deltaTime;
//                 float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
//                 tipText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
//                 yield return null;
//             }
//             
//             // 更换文本
//             tipText.text = newTip;
//             
//             // 淡入
//             elapsed = 0f;
//             while (elapsed < duration)
//             {
//                 elapsed += Time.deltaTime;
//                 float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
//                 tipText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
//                 yield return null;
//             }
//             
//             tipText.color = originalColor;
//         }
//         
//         /// <summary>
//         /// 设置目标进度
//         /// </summary>
//         public void SetTargetProgress(float progress)
//         {
//             targetProgress = Mathf.Clamp01(progress);
//         }
//         
//         /// <summary>
//         /// 更新加载文本
//         /// </summary>
//         public void UpdateLoadingText(string text)
//         {
//             currentLoadingText = text;
//             if (loadingText != null)
//             {
//                 loadingText.text = text;
//             }
//         }
//         
//         #endregion
//         
//         #region 动画处理
//         
//         /// <summary>
//         /// 开始加载动画
//         /// </summary>
//         private void StartLoadingAnimations()
//         {
//             // 开始旋转动画
//             isSpinning = true;
//             
//             // 开始粒子效果
//             if (loadingParticles != null)
//             {
//                 loadingParticles.Play();
//             }
//             
//             // 播放动画器动画
//             if (loadingAnimator != null)
//             {
//                 loadingAnimator.SetBool("IsLoading", true);
//             }
//         }
//         
//         /// <summary>
//         /// 停止加载动画
//         /// </summary>
//         private void StopLoadingAnimations()
//         {
//             // 停止旋转动画
//             isSpinning = false;
//             
//             // 停止粒子效果
//             if (loadingParticles != null)
//             {
//                 loadingParticles.Stop();
//             }
//             
//             // 停止动画器动画
//             if (loadingAnimator != null)
//             {
//                 loadingAnimator.SetBool("IsLoading", false);
//             }
//         }
//         
//         /// <summary>
//         /// 暂停加载动画
//         /// </summary>
//         private void PauseLoadingAnimations()
//         {
//             isSpinning = false;
//             
//             if (loadingParticles != null)
//             {
//                 loadingParticles.Pause();
//             }
//             
//             if (loadingAnimator != null)
//             {
//                 loadingAnimator.speed = 0f;
//             }
//         }
//         
//         /// <summary>
//         /// 恢复加载动画
//         /// </summary>
//         private void ResumeLoadingAnimations()
//         {
//             isSpinning = true;
//             
//             if (loadingParticles != null)
//             {
//                 loadingParticles.Play();
//             }
//             
//             if (loadingAnimator != null)
//             {
//                 loadingAnimator.speed = 1f;
//             }
//         }
//         
//         #endregion
//         
//         #region 公共方法
//         
//         /// <summary>
//         /// 添加加载任务
//         /// </summary>
//         public void AddLoadingTask(LoadingTask task)
//         {
//             if (task != null)
//             {
//                 loadingTasks.Enqueue(task);
//             }
//         }
//         
//         /// <summary>
//         /// 添加加载任务
//         /// </summary>
//         public void AddLoadingTask(string taskName, float duration, System.Action action = null)
//         {
//             AddLoadingTask(new LoadingTask
//             {
//                 TaskName = taskName,
//                 Duration = duration,
//                 Action = action
//             });
//         }
//         
//         /// <summary>
//         /// 清理加载任务
//         /// </summary>
//         private void ClearLoadingTasks()
//         {
//             loadingTasks.Clear();
//             currentTask = null;
//             isLoading = false;
//         }
//         
//         #endregion
//         
//         #region 输入处理
//         
//         public override void HandleInput(UIInputEvent inputEvent)
//         {
//             // 加载状态通常不处理输入
//             // 可以在这里添加跳过加载的逻辑（如果需要）
//         }
//         
//         #endregion
//         
//         #region 辅助方法
//         
//         /// <summary>
//         /// 清理UI
//         /// </summary>
//         private void CleanupUI()
//         {
//             // 停止所有协程
//             StopAllCoroutines();
//             
//             // 清理动态创建的纹理
//             if (loadingIcon != null && loadingIcon.sprite != null && loadingIcon.sprite.texture != null)
//             {
//                 Object.Destroy(loadingIcon.sprite.texture);
//                 Object.Destroy(loadingIcon.sprite);
//             }
//         }
//         
//         #endregion
//         
//         #region 状态转换检查
//         
//         public override bool CanTransitionTo(string targetStateName)
//         {
//             // 加载状态只能在加载完成后转换
//             return !isLoading;
//         }
//         
//         #endregion
//         
//         #region 数据处理
//         
//         public override object GetStateData()
//         {
//             var baseData = base.GetStateData();
//             
//             var loadingStateData = new LoadingStateData
//             {
//                 BaseData = baseData,
//                 CurrentProgress = currentProgress,
//                 TargetProgress = targetProgress,
//                 CurrentLoadingText = currentLoadingText,
//                 TargetStateName = targetStateName,
//                 TargetStateData = targetStateData,
//                 IsLoading = isLoading,
//                 RemainingTasks = loadingTasks.Count
//             };
//             
//             return loadingStateData;
//         }
//         
//         #endregion
//     }
//     
//     /// <summary>
//     /// 加载任务
//     /// </summary>
//     [System.Serializable]
//     public class LoadingTask
//     {
//         public string TaskName;
//         public float Duration;
//         public System.Action Action;
//         public float StartTime;
//     }
//     
//     /// <summary>
//     /// 加载数据
//     /// </summary>
//     [System.Serializable]
//     public class LoadingData
//     {
//         public string TargetStateName;
//         public object TargetStateData;
//         public List<LoadingTask> LoadingTasks;
//     }
//     
//     /// <summary>
//     /// 加载状态完整数据
//     /// </summary>
//     [System.Serializable]
//     public class LoadingStateData
//     {
//         public object BaseData;
//         public float CurrentProgress;
//         public float TargetProgress;
//         public string CurrentLoadingText;
//         public string TargetStateName;
//         public object TargetStateData;
//         public bool IsLoading;
//         public int RemainingTasks;
//     }
// }