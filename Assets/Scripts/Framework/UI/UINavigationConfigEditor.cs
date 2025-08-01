#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Framework.UI;

namespace Framework.UI.Editor
{
    /// <summary>
    /// UI导航配置编辑器
    /// 提供可视化的界面跳转配置工具
    /// </summary>
    [CustomEditor(typeof(UINavigationSystem))]
    public class UINavigationConfigEditor : UnityEditor.Editor
    {
        private UINavigationSystem navigationSystem;
        private SerializedProperty navigationConfigProp;
        private bool showPages = true;
        private bool showRules = true;
        private bool showSettings = true;
        private bool showPreview = false;
        
        // 预览相关
        private Dictionary<string, Vector2> nodePositions = new Dictionary<string, Vector2>();
        private Vector2 graphScrollPos;
        private const float NODE_WIDTH = 120f;
        private const float NODE_HEIGHT = 60f;
        
        void OnEnable()
        {
            navigationSystem = (UINavigationSystem)target;
            navigationConfigProp = serializedObject.FindProperty("navigationConfig");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("UI导航系统配置", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 配置文件引用
            EditorGUILayout.PropertyField(navigationConfigProp, new GUIContent("导航配置"));
            
            if (navigationConfigProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("请创建或分配UI导航配置文件", MessageType.Warning);
                
                if (GUILayout.Button("创建新配置"))
                {
                    CreateNewNavigationConfig();
                }
                
                serializedObject.ApplyModifiedProperties();
                return;
            }
            
            EditorGUILayout.Space();
            
            // 运行时信息
            if (Application.isPlaying)
            {
                DrawRuntimeInfo();
                EditorGUILayout.Space();
            }
            
            // 配置编辑区域
            DrawConfigurationEditor();
            
            // 预览区域
            DrawPreviewSection();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        /// <summary>
        /// 绘制运行时信息
        /// </summary>
        private void DrawRuntimeInfo()
        {
            EditorGUILayout.LabelField("运行时状态", EditorStyles.boldLabel);
            
            using (new EditorGUI.DisabledScope(true))
            {
                if (navigationSystem != null)
                {
                    string info = navigationSystem.GetNavigationInfo();
                    EditorGUILayout.TextArea(info, GUILayout.Height(60));
                }
            }
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新状态"))
            {
                Repaint();
            }
            if (GUILayout.Button("清理所有页面"))
            {
                navigationSystem.ClearAllPages();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// 绘制配置编辑器
        /// </summary>
        private void DrawConfigurationEditor()
        {
            var config = navigationConfigProp.objectReferenceValue as ScriptableObject;
            if (config == null) return;
            
            var serializedConfig = new SerializedObject(config);
            
            // 基本设置
            showSettings = EditorGUILayout.Foldout(showSettings, "基本设置", true);
            if (showSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("configName"));
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("enableBackStack"));
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("maxStackDepth"));
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("enableTransitionAnimation"));
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("defaultTransitionDuration"));
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // 页面配置
            showPages = EditorGUILayout.Foldout(showPages, "页面配置", true);
            if (showPages)
            {
                DrawPagesEditor(serializedConfig);
            }
            
            EditorGUILayout.Space();
            
            // 跳转规则
            showRules = EditorGUILayout.Foldout(showRules, "跳转规则", true);
            if (showRules)
            {
                DrawRulesEditor(serializedConfig);
            }
            
            serializedConfig.ApplyModifiedProperties();
        }
        
        /// <summary>
        /// 绘制页面编辑器
        /// </summary>
        private void DrawPagesEditor(SerializedObject serializedConfig)
        {
            var pagesProperty = serializedConfig.FindProperty("pages");
            
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"页面列表 ({pagesProperty.arraySize})", EditorStyles.boldLabel);
            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                pagesProperty.InsertArrayElementAtIndex(pagesProperty.arraySize);
            }
            EditorGUILayout.EndHorizontal();
            
            for (int i = 0; i < pagesProperty.arraySize; i++)
            {
                var pageProperty = pagesProperty.GetArrayElementAtIndex(i);
                
                EditorGUILayout.BeginVertical("box");
                
                EditorGUILayout.BeginHorizontal();
                var pageIdProp = pageProperty.FindPropertyRelative("pageId");
                string pageId = pageIdProp.stringValue;
                if (string.IsNullOrEmpty(pageId))
                    pageId = $"页面 {i + 1}";
                
                EditorGUILayout.LabelField(pageId, EditorStyles.boldLabel);
                
                if (GUILayout.Button("×", GUILayout.Width(20)))
                {
                    pagesProperty.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
                
                // 页面基本信息
                EditorGUILayout.PropertyField(pageProperty.FindPropertyRelative("pageId"));
                EditorGUILayout.PropertyField(pageProperty.FindPropertyRelative("displayName"));
                EditorGUILayout.PropertyField(pageProperty.FindPropertyRelative("prefabPath"));
                
                // 页面属性
                EditorGUILayout.PropertyField(pageProperty.FindPropertyRelative("pageType"));
                EditorGUILayout.PropertyField(pageProperty.FindPropertyRelative("layerType"));
                EditorGUILayout.PropertyField(pageProperty.FindPropertyRelative("isModal"));
                EditorGUILayout.PropertyField(pageProperty.FindPropertyRelative("allowMultipleInstances"));
                
                // 生命周期
                EditorGUILayout.PropertyField(pageProperty.FindPropertyRelative("preload"));
                EditorGUILayout.PropertyField(pageProperty.FindPropertyRelative("cacheWhenClosed"));
                EditorGUILayout.PropertyField(pageProperty.FindPropertyRelative("autoCloseDelay"));
                
                // 动画设置
                EditorGUILayout.PropertyField(pageProperty.FindPropertyRelative("showAnimation"));
                EditorGUILayout.PropertyField(pageProperty.FindPropertyRelative("hideAnimation"));
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            
            EditorGUI.indentLevel--;
        }
        
        /// <summary>
        /// 绘制规则编辑器
        /// </summary>
        private void DrawRulesEditor(SerializedObject serializedConfig)
        {
            var rulesProperty = serializedConfig.FindProperty("transitionRules");
            var pagesProperty = serializedConfig.FindProperty("pages");
            
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"跳转规则 ({rulesProperty.arraySize})", EditorStyles.boldLabel);
            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                rulesProperty.InsertArrayElementAtIndex(rulesProperty.arraySize);
            }
            EditorGUILayout.EndHorizontal();
            
            // 收集页面ID列表
            List<string> pageIds = new List<string> { "任意页面" };
            for (int i = 0; i < pagesProperty.arraySize; i++)
            {
                var pageProperty = pagesProperty.GetArrayElementAtIndex(i);
                string pageId = pageProperty.FindPropertyRelative("pageId").stringValue;
                if (!string.IsNullOrEmpty(pageId))
                    pageIds.Add(pageId);
            }
            
            for (int i = 0; i < rulesProperty.arraySize; i++)
            {
                var ruleProperty = rulesProperty.GetArrayElementAtIndex(i);
                
                EditorGUILayout.BeginVertical("box");
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"规则 {i + 1}", EditorStyles.boldLabel);
                
                if (GUILayout.Button("×", GUILayout.Width(20)))
                {
                    rulesProperty.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
                
                // 跳转条件
                EditorGUILayout.LabelField("跳转条件", EditorStyles.boldLabel);
                
                // 源页面选择
                var fromPageIdProp = ruleProperty.FindPropertyRelative("fromPageId");
                int fromIndex = pageIds.IndexOf(fromPageIdProp.stringValue);
                if (fromIndex < 0) fromIndex = 0;
                
                fromIndex = EditorGUILayout.Popup("从页面", fromIndex, pageIds.ToArray());
                fromPageIdProp.stringValue = fromIndex > 0 ? pageIds[fromIndex] : "";
                
                // 目标页面选择
                var toPageIdProp = ruleProperty.FindPropertyRelative("toPageId");
                int toIndex = pageIds.IndexOf(toPageIdProp.stringValue);
                if (toIndex < 0) toIndex = 0;
                
                toIndex = EditorGUILayout.Popup("到页面", toIndex, pageIds.ToArray());
                toPageIdProp.stringValue = toIndex > 0 ? pageIds[toIndex] : "";
                
                // 触发事件
                EditorGUILayout.PropertyField(ruleProperty.FindPropertyRelative("triggerEvent"));
                
                EditorGUILayout.Space();
                
                // 跳转行为
                EditorGUILayout.LabelField("跳转行为", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(ruleProperty.FindPropertyRelative("transitionType"));
                EditorGUILayout.PropertyField(ruleProperty.FindPropertyRelative("validateCondition"));
                
                var validateProp = ruleProperty.FindPropertyRelative("validateCondition");
                if (validateProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(ruleProperty.FindPropertyRelative("conditionMethod"));
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.Space();
                
                // 参数传递
                EditorGUILayout.LabelField("参数传递", EditorStyles.boldLabel);
                var parametersProp = ruleProperty.FindPropertyRelative("parameterKeys");
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"参数列表 ({parametersProp.arraySize})");
                if (GUILayout.Button("+", GUILayout.Width(30)))
                {
                    parametersProp.InsertArrayElementAtIndex(parametersProp.arraySize);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel++;
                for (int j = 0; j < parametersProp.arraySize; j++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(parametersProp.GetArrayElementAtIndex(j), GUIContent.none);
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        parametersProp.DeleteArrayElementAtIndex(j);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            
            EditorGUI.indentLevel--;
        }
        
        /// <summary>
        /// 绘制预览区域
        /// </summary>
        private void DrawPreviewSection()
        {
            EditorGUILayout.Space();
            showPreview = EditorGUILayout.Foldout(showPreview, "流程预览", true);
            
            if (showPreview)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("UI跳转流程图", EditorStyles.boldLabel);
                
                if (GUILayout.Button("生成流程图"))
                {
                    GenerateFlowChart();
                }
                
                // 绘制简化的流程图
                DrawSimpleFlowChart();
                
                EditorGUILayout.EndVertical();
            }
        }
        
        /// <summary>
        /// 绘制简化流程图
        /// </summary>
        private void DrawSimpleFlowChart()
        {
            var config = navigationConfigProp.objectReferenceValue as ScriptableObject;
            if (config == null) return;
            
            var serializedConfig = new SerializedObject(config);
            var pagesProperty = serializedConfig.FindProperty("pages");
            var rulesProperty = serializedConfig.FindProperty("transitionRules");
            
            EditorGUILayout.LabelField("页面流程:", EditorStyles.boldLabel);
            
            // 显示页面列表
            for (int i = 0; i < pagesProperty.arraySize; i++)
            {
                var pageProperty = pagesProperty.GetArrayElementAtIndex(i);
                string pageId = pageProperty.FindPropertyRelative("pageId").stringValue;
                string displayName = pageProperty.FindPropertyRelative("displayName").stringValue;
                
                if (!string.IsNullOrEmpty(pageId))
                {
                    EditorGUILayout.LabelField($"• {pageId} ({displayName})");
                    
                    // 显示从此页面出发的跳转规则
                    for (int j = 0; j < rulesProperty.arraySize; j++)
                    {
                        var ruleProperty = rulesProperty.GetArrayElementAtIndex(j);
                        string fromPageId = ruleProperty.FindPropertyRelative("fromPageId").stringValue;
                        string toPageId = ruleProperty.FindPropertyRelative("toPageId").stringValue;
                        string triggerEvent = ruleProperty.FindPropertyRelative("triggerEvent").stringValue;
                        
                        if (fromPageId == pageId)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.LabelField($"→ {toPageId} (触发: {triggerEvent})");
                            EditorGUI.indentLevel--;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 生成流程图
        /// </summary>
        private void GenerateFlowChart()
        {
            Debug.Log("[UI导航编辑器] 生成流程图功能待实现");
            // 这里可以实现更复杂的流程图生成逻辑
        }
        
        /// <summary>
        /// 创建新的导航配置
        /// </summary>
        private void CreateNewNavigationConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "创建UI导航配置",
                "UINavigationConfig",
                "asset",
                "选择保存位置");
            
            if (!string.IsNullOrEmpty(path))
            {
                var config = CreateInstance<UINavigationConfigAsset>();
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
                
                navigationConfigProp.objectReferenceValue = config;
                serializedObject.ApplyModifiedProperties();
                
                Debug.Log($"[UI导航编辑器] 创建配置文件: {path}");
            }
        }
    }
    
    /// <summary>
    /// UI导航配置资源文件
    /// </summary>
    [CreateAssetMenu(fileName = "UINavigationConfig", menuName = "UI/导航配置")]
    public class UINavigationConfigAsset : ScriptableObject
    {
        public UINavigationSystem.UINavigationConfig config = new UINavigationSystem.UINavigationConfig();
    }
}
#endif