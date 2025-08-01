#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Framework.UI.Editor
{
    /// <summary>
    /// 非线性导航可视化工具
    /// 在Unity编辑器中可视化显示导航栈的变化过程
    /// </summary>
    public class NonLinearNavigationVisualizer : EditorWindow
    {
        #region 字段和属性
        
        private AdvancedUINavigationSystem targetSystem;
        private List<NavigationStep> simulationSteps = new List<NavigationStep>();
        private int currentStepIndex = 0;
        private Vector2 scrollPosition;
        
        // 可视化设置
        private bool showStackVisualization = true;
        private bool showStepByStep = true;
        private bool autoRefresh = true;
        private float refreshInterval = 0.5f;
        private double lastRefreshTime;
        
        // 颜色配置
        private Color currentPageColor = Color.green;
        private Color normalPageColor = Color.white;
        private Color removedPageColor = Color.red;
        private Color addedPageColor = Color.cyan;
        
        #endregion
        
        #region 数据结构
        
        /// <summary>
        /// 导航步骤
        /// </summary>
        [System.Serializable]
        public class NavigationStep
        {
            public string stepName;
            public List<string> stackBefore;
            public List<string> stackAfter;
            public string operation;
            public string description;
            public float timestamp;
            
            public NavigationStep(string name, List<string> before, List<string> after, string op, string desc)
            {
                stepName = name;
                stackBefore = new List<string>(before);
                stackAfter = new List<string>(after);
                operation = op;
                description = desc;
                timestamp = Time.time;
            }
        }
        
        #endregion
        
        #region Unity编辑器方法
        
        [MenuItem("Window/UI Navigation/Non-Linear Visualizer")]
        public static void ShowWindow()
        {
            var window = GetWindow<NonLinearNavigationVisualizer>("非线性导航可视化");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }
        
        void OnEnable()
        {
            // 查找场景中的导航系统
            FindNavigationSystem();
            
            // 设置自动刷新
            EditorApplication.update += OnEditorUpdate;
        }
        
        void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }
        
        void OnGUI()
        {
            DrawHeader();
            DrawSystemSelection();
            
            if (targetSystem != null)
            {
                DrawVisualizationOptions();
                DrawCurrentStack();
                DrawSimulationControls();
                DrawStepByStepVisualization();
            }
            else
            {
                DrawNoSystemMessage();
            }
        }
        
        #endregion
        
        #region GUI绘制方法
        
        /// <summary>
        /// 绘制标题
        /// </summary>
        private void DrawHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("非线性UI导航可视化工具", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("可视化显示复杂导航栈的变化过程", EditorStyles.helpBox);
            EditorGUILayout.Space();
        }
        
        /// <summary>
        /// 绘制系统选择
        /// </summary>
        private void DrawSystemSelection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("目标导航系统:", GUILayout.Width(100));
            
            var newTarget = EditorGUILayout.ObjectField(targetSystem, typeof(AdvancedUINavigationSystem), true) as AdvancedUINavigationSystem;
            if (newTarget != targetSystem)
            {
                targetSystem = newTarget;
                ClearSimulation();
            }
            
            if (GUILayout.Button("查找", GUILayout.Width(50)))
            {
                FindNavigationSystem();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// 绘制可视化选项
        /// </summary>
        private void DrawVisualizationOptions()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("可视化选项", EditorStyles.boldLabel);
            
            showStackVisualization = EditorGUILayout.Toggle("显示栈可视化", showStackVisualization);
            showStepByStep = EditorGUILayout.Toggle("显示步骤详情", showStepByStep);
            autoRefresh = EditorGUILayout.Toggle("自动刷新", autoRefresh);
            
            if (autoRefresh)
            {
                refreshInterval = EditorGUILayout.Slider("刷新间隔", refreshInterval, 0.1f, 2f);
            }
            
            EditorGUILayout.Space();
            
            // 颜色配置
            EditorGUILayout.LabelField("颜色配置", EditorStyles.boldLabel);
            currentPageColor = EditorGUILayout.ColorField("当前页面", currentPageColor);
            normalPageColor = EditorGUILayout.ColorField("普通页面", normalPageColor);
            addedPageColor = EditorGUILayout.ColorField("新增页面", addedPageColor);
            removedPageColor = EditorGUILayout.ColorField("移除页面", removedPageColor);
        }
        
        /// <summary>
        /// 绘制当前栈状态
        /// </summary>
        private void DrawCurrentStack()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("当前导航栈", EditorStyles.boldLabel);
            
            if (showStackVisualization)
            {
                DrawStackVisualization();
            }
            else
            {
                string stackInfo = targetSystem.GetNavigationStackInfo();
                EditorGUILayout.TextArea(stackInfo, GUILayout.Height(100));
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"栈大小: {targetSystem.GetStackSize()}");
            EditorGUILayout.LabelField($"当前页面: {targetSystem.GetCurrentPageId() ?? "无"}");
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// 绘制栈可视化
        /// </summary>
        private void DrawStackVisualization()
        {
            var stackInfo = targetSystem.GetNavigationStackInfo();
            if (stackInfo.Contains("导航栈为空"))
            {
                EditorGUILayout.HelpBox("导航栈为空", MessageType.Info);
                return;
            }
            
            // 解析栈信息
            var pages = ParseStackInfo(stackInfo);
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("栈可视化 (从底到顶):", EditorStyles.boldLabel);
            
            for (int i = 0; i < pages.Count; i++)
            {
                bool isTop = (i == pages.Count - 1);
                Color pageColor = isTop ? currentPageColor : normalPageColor;
                
                var originalColor = GUI.backgroundColor;
                GUI.backgroundColor = pageColor;
                
                EditorGUILayout.BeginHorizontal("button");
                EditorGUILayout.LabelField($"{i + 1}.", GUILayout.Width(30));
                EditorGUILayout.LabelField(pages[i]);
                if (isTop)
                {
                    EditorGUILayout.LabelField("← 当前", EditorStyles.boldLabel, GUILayout.Width(50));
                }
                EditorGUILayout.EndHorizontal();
                
                GUI.backgroundColor = originalColor;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// 绘制模拟控制
        /// </summary>
        private void DrawSimulationControls()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("模拟控制", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("记录当前状态"))
            {
                RecordCurrentState("手动记录");
            }
            
            if (GUILayout.Button("清空记录"))
            {
                ClearSimulation();
            }
            
            if (GUILayout.Button("导出记录"))
            {
                ExportSimulation();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 快速测试按钮
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("快速测试", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("建立A-B-C-D场景"))
            {
                SimulateABCDScenario();
            }
            
            if (GUILayout.Button("演示非线性跳转"))
            {
                SimulateNonLinearJump();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// 绘制步骤可视化
        /// </summary>
        private void DrawStepByStepVisualization()
        {
            if (!showStepByStep || simulationSteps.Count == 0)
                return;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("步骤记录", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            for (int i = 0; i < simulationSteps.Count; i++)
            {
                var step = simulationSteps[i];
                bool isSelected = (i == currentStepIndex);
                
                var originalColor = GUI.backgroundColor;
                if (isSelected)
                    GUI.backgroundColor = Color.yellow;
                
                EditorGUILayout.BeginVertical("box");
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"步骤 {i + 1}: {step.stepName}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"时间: {step.timestamp:F1}s", GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.LabelField($"操作: {step.operation}");
                EditorGUILayout.LabelField($"描述: {step.description}");
                
                // 显示栈变化
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("变化前:", GUILayout.Width(60));
                EditorGUILayout.LabelField(string.Join("-", step.stackBefore));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("变化后:", GUILayout.Width(60));
                EditorGUILayout.LabelField(string.Join("-", step.stackAfter));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                
                GUI.backgroundColor = originalColor;
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        /// <summary>
        /// 绘制无系统消息
        /// </summary>
        private void DrawNoSystemMessage()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("未找到高级UI导航系统。请确保场景中存在AdvancedUINavigationSystem组件。", MessageType.Warning);
            
            if (GUILayout.Button("创建导航系统"))
            {
                CreateNavigationSystem();
            }
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 查找导航系统
        /// </summary>
        private void FindNavigationSystem()
        {
            targetSystem = FindObjectOfType<AdvancedUINavigationSystem>();
            if (targetSystem == null)
            {
                Debug.LogWarning("[导航可视化] 未找到高级UI导航系统");
            }
        }
        
        /// <summary>
        /// 创建导航系统
        /// </summary>
        private void CreateNavigationSystem()
        {
            var go = new GameObject("AdvancedUINavigationSystem");
            targetSystem = go.AddComponent<AdvancedUINavigationSystem>();
            Selection.activeGameObject = go;
            
            Debug.Log("[导航可视化] 已创建高级UI导航系统");
        }
        
        /// <summary>
        /// 解析栈信息
        /// </summary>
        private List<string> ParseStackInfo(string stackInfo)
        {
            var pages = new List<string>();
            var lines = stackInfo.Split('\n');
            
            foreach (var line in lines)
            {
                if (line.Contains("Page"))
                {
                    var parts = line.Split('.');
                    if (parts.Length > 1)
                    {
                        var pageName = parts[1].Split(' ')[1];
                        pages.Add(pageName);
                    }
                }
            }
            
            return pages;
        }
        
        /// <summary>
        /// 记录当前状态
        /// </summary>
        private void RecordCurrentState(string stepName)
        {
            if (targetSystem == null) return;
            
            var currentStack = ParseStackInfo(targetSystem.GetNavigationStackInfo());
            var lastStack = simulationSteps.Count > 0 ? simulationSteps.Last().stackAfter : new List<string>();
            
            var step = new NavigationStep(
                stepName,
                lastStack,
                currentStack,
                "记录状态",
                $"记录导航栈状态: {string.Join("-", currentStack)}"
            );
            
            simulationSteps.Add(step);
            currentStepIndex = simulationSteps.Count - 1;
            
            Repaint();
        }
        
        /// <summary>
        /// 清空模拟
        /// </summary>
        private void ClearSimulation()
        {
            simulationSteps.Clear();
            currentStepIndex = 0;
            Repaint();
        }
        
        /// <summary>
        /// 导出模拟记录
        /// </summary>
        private void ExportSimulation()
        {
            if (simulationSteps.Count == 0)
            {
                EditorUtility.DisplayDialog("导出失败", "没有可导出的记录", "确定");
                return;
            }
            
            string path = EditorUtility.SaveFilePanel("导出导航记录", "", "navigation_log", "txt");
            if (!string.IsNullOrEmpty(path))
            {
                var content = "非线性UI导航记录\n";
                content += "===================\n\n";
                
                foreach (var step in simulationSteps)
                {
                    content += $"步骤: {step.stepName}\n";
                    content += $"时间: {step.timestamp:F1}s\n";
                    content += $"操作: {step.operation}\n";
                    content += $"描述: {step.description}\n";
                    content += $"变化前: {string.Join("-", step.stackBefore)}\n";
                    content += $"变化后: {string.Join("-", step.stackAfter)}\n";
                    content += "-------------------\n\n";
                }
                
                System.IO.File.WriteAllText(path, content);
                EditorUtility.DisplayDialog("导出成功", $"记录已导出到: {path}", "确定");
            }
        }
        
        /// <summary>
        /// 模拟A-B-C-D场景
        /// </summary>
        private void SimulateABCDScenario()
        {
            if (targetSystem == null) return;
            
            ClearSimulation();
            
            // 记录初始状态
            RecordCurrentState("初始状态");
            
            // 模拟打开A-B-C-D
            targetSystem.ClearNavigationStack();
            RecordCurrentState("清空栈");
            
            targetSystem.NavigateToPage("PageA");
            RecordCurrentState("打开页面A");
            
            targetSystem.NavigateToPage("PageB");
            RecordCurrentState("打开页面B");
            
            targetSystem.NavigateToPage("PageC");
            RecordCurrentState("打开页面C");
            
            targetSystem.NavigateToPage("PageD");
            RecordCurrentState("打开页面D");
            
            Debug.Log("[导航可视化] A-B-C-D场景模拟完成");
        }
        
        /// <summary>
        /// 模拟非线性跳转
        /// </summary>
        private void SimulateNonLinearJump()
        {
            if (targetSystem == null) return;
            
            // 确保有A-B-C-D场景
            if (targetSystem.GetStackSize() < 4)
            {
                SimulateABCDScenario();
            }
            
            // 执行非线性跳转：从D跳转到B
            targetSystem.NavigateToPage("PageB", AdvancedUINavigationSystem.JumpOperation.BringToTop);
            RecordCurrentState("非线性跳转到B");
            
            Debug.Log("[导航可视化] 非线性跳转模拟完成");
        }
        
        /// <summary>
        /// 编辑器更新
        /// </summary>
        private void OnEditorUpdate()
        {
            if (!autoRefresh || targetSystem == null)
                return;
            
            if (EditorApplication.timeSinceStartup - lastRefreshTime > refreshInterval)
            {
                lastRefreshTime = EditorApplication.timeSinceStartup;
                Repaint();
            }
        }
        
        #endregion
    }
}
#endif