#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// 状态机调试器 - Unity编辑器工具，用于实时查看状态机状态
/// </summary>
[System.Serializable]
public class StateMachineDebugger : EditorWindow
{
    private GameManager gameManager;
    private Vector2 scrollPosition;
    private bool autoRefresh = true;
    private float refreshInterval = 0.5f;
    private double lastRefreshTime;
    
    [MenuItem("工具/状态机调试器")]
    public static void ShowWindow()
    {
        StateMachineDebugger window = GetWindow<StateMachineDebugger>("状态机调试器");
        window.minSize = new Vector2(400, 300);
    }
    
    void OnEnable()
    {
        // 查找场景中的GameManager
        FindGameManager();
    }
    
    void OnGUI()
    {
        DrawHeader();
        DrawGameManagerSection();
        
        if (gameManager != null && gameManager.GetComponent<GameManager>() != null)
        {
            DrawStateMachineInfo();
        }
        else
        {
            EditorGUILayout.HelpBox("未找到GameManager或状态机未初始化", MessageType.Warning);
        }
        
        // 自动刷新
        if (autoRefresh && EditorApplication.timeSinceStartup - lastRefreshTime > refreshInterval)
        {
            lastRefreshTime = EditorApplication.timeSinceStartup;
            Repaint();
        }
    }
    
    /// <summary>
    /// 绘制头部信息
    /// </summary>
    private void DrawHeader()
    {
        EditorGUILayout.BeginVertical("box");
        
        GUILayout.Label("状态机调试器", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        autoRefresh = EditorGUILayout.Toggle("自动刷新", autoRefresh);
        refreshInterval = EditorGUILayout.Slider("刷新间隔", refreshInterval, 0.1f, 2f);
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("手动刷新"))
        {
            FindGameManager();
            Repaint();
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }
    
    /// <summary>
    /// 绘制GameManager选择区域
    /// </summary>
    private void DrawGameManagerSection()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("GameManager", EditorStyles.boldLabel);
        
        GameManager newGameManager = (GameManager)EditorGUILayout.ObjectField(
            "目标对象", gameManager, typeof(GameManager), true);
        
        if (newGameManager != gameManager)
        {
            gameManager = newGameManager;
        }
        
        if (gameManager == null)
        {
            if (GUILayout.Button("自动查找GameManager"))
            {
                FindGameManager();
            }
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }
    
    /// <summary>
    /// 绘制状态机信息
    /// </summary>
    private void DrawStateMachineInfo()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // 获取状态机信息（这里需要在GameManager中添加公共访问方法）
        DrawCurrentStateInfo();
        DrawStateStackInfo();
        DrawBlackboardInfo();
        DrawControlButtons();
        
        EditorGUILayout.EndScrollView();
    }
    
    /// <summary>
    /// 绘制当前状态信息
    /// </summary>
    private void DrawCurrentStateInfo()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("当前状态信息", EditorStyles.boldLabel);
        
        // 注意：这里需要在GameManager中添加获取状态机信息的公共方法
        EditorGUILayout.LabelField("当前状态", "需要在GameManager中添加访问方法");
        EditorGUILayout.LabelField("上一个状态", "需要在GameManager中添加访问方法");
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }
    
    /// <summary>
    /// 绘制状态栈信息
    /// </summary>
    private void DrawStateStackInfo()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("状态栈信息", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField("栈深度", "需要在GameManager中添加访问方法");
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }
    
    /// <summary>
    /// 绘制黑板信息
    /// </summary>
    private void DrawBlackboardInfo()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("黑板数据", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField("数据项", "需要在GameManager中添加访问方法");
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }
    
    /// <summary>
    /// 绘制控制按钮
    /// </summary>
    private void DrawControlButtons()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("状态控制", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("切换到主菜单"))
        {
            // gameManager.ForceChangeState<MainMenuState>();
            Debug.Log("需要在GameManager中实现ForceChangeState方法");
        }
        
        if (GUILayout.Button("切换到游戏"))
        {
            // gameManager.ForceChangeState<GamePlayState>();
            Debug.Log("需要在GameManager中实现ForceChangeState方法");
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("暂停状态"))
        {
            // gameManager.PauseCurrentState<PauseState>();
            Debug.Log("需要在GameManager中实现PauseCurrentState方法");
        }
        
        if (GUILayout.Button("恢复状态"))
        {
            // gameManager.ResumeState();
            Debug.Log("需要在GameManager中实现ResumeState方法");
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    /// <summary>
    /// 查找场景中的GameManager
    /// </summary>
    private void FindGameManager()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }
}

/// <summary>
/// 状态机信息显示的自定义Inspector
/// </summary>
[CustomEditor(typeof(GameManager))]
public class GameManagerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("打开状态机调试器"))
        {
            StateMachineDebugger.ShowWindow();
        }
        
        GameManager gameManager = (GameManager)target;
        
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("运行时信息", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("状态机状态", "运行中");
            
            // 这里可以显示更多运行时信息
            
            EditorGUILayout.EndVertical();
        }
    }
}
#endif