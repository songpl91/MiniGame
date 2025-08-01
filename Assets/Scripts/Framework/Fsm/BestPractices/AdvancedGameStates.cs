using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// 增强版游戏状态集合 - 展示状态机最佳实践
/// 使用了扩展功能、性能优化和完整的错误处理
/// </summary>
namespace AdvancedGameStates
{
    /// <summary>
    /// 加载状态 - 游戏启动时的资源加载
    /// </summary>
    public class LoadingState : EnhancedBaseStateNode
    {
        public override int Priority => 10; // 最高优先级，不能被打断
        
        private float loadingProgress = 0f;
        private bool isLoadingComplete = false;
        
        public override void OnEnter()
        {
            base.OnEnter();
            
            // 设置加载状态
            SetBlackboardValue("LoadingProgress", 0f);
            SetBlackboardValue("LoadingStatus", "正在加载游戏资源...");
            
            // 开始异步加载
            StartAsyncLoading();
        }
        
        public override void OnUpdate()
        {
            // 更新加载进度
            UpdateLoadingProgress();
            
            // 检查加载完成
            if (isLoadingComplete)
            {
                Debug.Log("[加载状态] 资源加载完成，切换到主菜单");
                ChangeState<AdvancedMainMenuState>();
            }
        }
        
        public override bool CanExit()
        {
            // 只有加载完成才能退出
            return isLoadingComplete;
        }
        
        public override string GetDebugInfo()
        {
            return base.GetDebugInfo() + 
                   $"\n加载进度: {loadingProgress:P}\n" +
                   $"加载完成: {isLoadingComplete}";
        }
        
        /// <summary>
        /// 开始异步加载
        /// </summary>
        private void StartAsyncLoading()
        {
            // 模拟异步加载过程
            (stateMachine.Owner as GameObject).GetComponent<MonoBehaviour>().StartCoroutine(LoadResourcesAsync());
        }
        
        /// <summary>
        /// 异步加载资源协程
        /// </summary>
        private IEnumerator LoadResourcesAsync()
        {
            string[] loadingSteps = {
                "加载音频资源...",
                "加载贴图资源...",
                "加载预制体...",
                "初始化游戏系统...",
                "加载完成！"
            };
            
            for (int i = 0; i < loadingSteps.Length; i++)
            {
                SetBlackboardValue("LoadingStatus", loadingSteps[i]);
                
                // 模拟加载时间
                float stepDuration = Random.Range(0.5f, 1.5f);
                float stepStartTime = Time.time;
                
                while (Time.time - stepStartTime < stepDuration)
                {
                    float stepProgress = (Time.time - stepStartTime) / stepDuration;
                    loadingProgress = (i + stepProgress) / loadingSteps.Length;
                    SetBlackboardValue("LoadingProgress", loadingProgress);
                    yield return null;
                }
            }
            
            isLoadingComplete = true;
            SetBlackboardValue("LoadingComplete", true);
        }
        
        /// <summary>
        /// 更新加载进度显示
        /// </summary>
        private void UpdateLoadingProgress()
        {
            // 这里可以添加进度条更新逻辑
            // UI系统会监听黑板数据变化来更新显示
        }
    }
    
    /// <summary>
    /// 增强版主菜单状态
    /// </summary>
    public class AdvancedMainMenuState : EnhancedBaseStateNode
    {
        public override int Priority => 1; // 低优先级
        
        private float idleTime = 0f;
        private const float AUTO_DEMO_TIME = 30f; // 30秒后自动演示
        
        public override void OnEnter()
        {
            base.OnEnter();
            
            // 设置菜单状态
            SetBlackboardValue("CurrentMenu", "MainMenu");
            SetBlackboardValue("MenuIdleTime", 0f);
            
            // 重置游戏数据
            ResetGameData();
            
            // 播放背景音乐
            PlayBackgroundMusic("MainMenuTheme");
        }
        
        public override void OnUpdate()
        {
            // 更新空闲时间
            idleTime += Time.deltaTime;
            SetBlackboardValue("MenuIdleTime", idleTime);
            
            // 处理用户输入
            HandleMenuInput();
            
            // 检查自动演示
            CheckAutoDemo();
        }
        
        public override void OnExit()
        {
            base.OnExit();
            StopBackgroundMusic();
        }
        
        public override string GetDebugInfo()
        {
            return base.GetDebugInfo() + 
                   $"\n空闲时间: {idleTime:F1}秒\n" +
                   $"自动演示倒计时: {AUTO_DEMO_TIME - idleTime:F1}秒";
        }
        
        /// <summary>
        /// 处理菜单输入
        /// </summary>
        private void HandleMenuInput()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("[主菜单] 开始游戏");
                StartNewGame();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("[主菜单] 打开设置");
                ChangeState<SettingsState>();
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                Debug.Log("[主菜单] 查看排行榜");
                ChangeState<LeaderboardState>();
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                Debug.Log("[主菜单] 开始教程");
                ChangeState<TutorialState>();
            }
            
            // 任何输入都重置空闲时间
            if (Input.anyKeyDown)
            {
                idleTime = 0f;
            }
        }
        
        /// <summary>
        /// 检查自动演示
        /// </summary>
        private void CheckAutoDemo()
        {
            if (idleTime >= AUTO_DEMO_TIME)
            {
                Debug.Log("[主菜单] 触发自动演示");
                // 可以切换到演示状态或自动开始游戏
                StartDemoMode();
            }
        }
        
        /// <summary>
        /// 开始新游戏
        /// </summary>
        private void StartNewGame()
        {
            // 设置新游戏数据
            SetBlackboardValue("Score", 0);
            SetBlackboardValue("Lives", 3);
            SetBlackboardValue("Level", 1);
            SetBlackboardValue("GameMode", "Normal");
            
            ChangeState<AdvancedGamePlayState>();
        }
        
        /// <summary>
        /// 开始演示模式
        /// </summary>
        private void StartDemoMode()
        {
            SetBlackboardValue("GameMode", "Demo");
            ChangeState<AdvancedGamePlayState>();
        }
        
        // 辅助方法
        private void ResetGameData() { }
        private void PlayBackgroundMusic(string theme) { }
        private void StopBackgroundMusic() { }
    }
    
    /// <summary>
    /// 增强版游戏进行状态
    /// </summary>
    public class AdvancedGamePlayState : EnhancedBaseStateNode
    {
        public override int Priority => 5; // 中等优先级
        
        private float gameTime = 0f;
        private float lastScoreTime = 0f;
        private int comboCount = 0;
        private bool isGameActive = true;
        
        // 游戏配置
        private const float SCORE_INTERVAL = 1f; // 每秒得分
        private const int BASE_SCORE = 10;
        private const float COMBO_TIMEOUT = 3f;
        
        public override void OnEnter()
        {
            base.OnEnter();
            
            // 初始化游戏状态
            gameTime = 0f;
            lastScoreTime = 0f;
            comboCount = 0;
            isGameActive = true;
            
            // 设置游戏数据
            SetBlackboardValue("GameActive", true);
            SetBlackboardValue("GameTime", 0f);
            SetBlackboardValue("ComboCount", 0);
            
            // 开始游戏逻辑
            StartGameLogic();
            
            Debug.Log("[游戏进行] 游戏开始！");
        }
        
        public override void OnUpdate()
        {
            if (!isGameActive) return;
            
            // 更新游戏时间
            UpdateGameTime();
            
            // 处理游戏输入
            HandleGameInput();
            
            // 更新游戏逻辑
            UpdateGameLogic();
            
            // 检查游戏结束条件
            CheckGameEndConditions();
        }
        
        public override void OnPause()
        {
            base.OnPause();
            isGameActive = false;
            SetBlackboardValue("GameActive", false);
            Debug.Log("[游戏进行] 游戏已暂停");
        }
        
        public override void OnResume()
        {
            base.OnResume();
            isGameActive = true;
            SetBlackboardValue("GameActive", true);
            Debug.Log("[游戏进行] 游戏已恢复");
        }
        
        public override void OnExit()
        {
            base.OnExit();
            
            // 保存最终数据
            SaveFinalGameData();
            
            // 停止游戏逻辑
            StopGameLogic();
        }
        
        public override bool CanExit()
        {
            // 检查是否可以安全退出
            return !IsInCriticalGameMoment();
        }
        
        public override string GetDebugInfo()
        {
            return base.GetDebugInfo() + 
                   $"\n游戏时间: {gameTime:F1}秒\n" +
                   $"连击数: {comboCount}\n" +
                   $"游戏激活: {isGameActive}\n" +
                   $"当前分数: {GetBlackboardValue<int>("Score")}";
        }
        
        /// <summary>
        /// 更新游戏时间
        /// </summary>
        private void UpdateGameTime()
        {
            gameTime += Time.deltaTime;
            SetBlackboardValue("GameTime", gameTime);
            
            // 每秒自动得分
            if (Time.time - lastScoreTime >= SCORE_INTERVAL)
            {
                AddScore(BASE_SCORE);
                lastScoreTime = Time.time;
            }
        }
        
        /// <summary>
        /// 处理游戏输入
        /// </summary>
        private void HandleGameInput()
        {
            // 暂停游戏
            if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[游戏进行] 暂停游戏");
                stateMachine.PushState<AdvancedPauseState>();
                return;
            }
            
            // 游戏操作输入
            if (Input.GetKeyDown(KeyCode.Space))
            {
                HandlePlayerAction();
            }
            
            // 技能输入
            if (Input.GetKeyDown(KeyCode.Q))
            {
                UseSkill("Skill1");
            }
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                UseSkill("Skill2");
            }
        }
        
        /// <summary>
        /// 更新游戏逻辑
        /// </summary>
        private void UpdateGameLogic()
        {
            // 更新连击计时
            UpdateComboSystem();
            
            // 检查关卡进度
            CheckLevelProgression();
            
            // 更新游戏难度
            UpdateGameDifficulty();
        }
        
        /// <summary>
        /// 处理玩家动作
        /// </summary>
        private void HandlePlayerAction()
        {
            // 增加连击
            comboCount++;
            SetBlackboardValue("ComboCount", comboCount);
            
            // 连击奖励分数
            int comboBonus = comboCount * 5;
            AddScore(BASE_SCORE + comboBonus);
            
            Debug.Log($"[游戏进行] 玩家动作！连击: {comboCount}, 奖励: {comboBonus}");
        }
        
        /// <summary>
        /// 使用技能
        /// </summary>
        private void UseSkill(string skillName)
        {
            Debug.Log($"[游戏进行] 使用技能: {skillName}");
            
            // 技能效果
            switch (skillName)
            {
                case "Skill1":
                    AddScore(100);
                    break;
                case "Skill2":
                    // 增加生命
                    int currentLives = GetBlackboardValue<int>("Lives");
                    SetBlackboardValue("Lives", currentLives + 1);
                    break;
            }
        }
        
        /// <summary>
        /// 添加分数
        /// </summary>
        private void AddScore(int points)
        {
            int currentScore = GetBlackboardValue<int>("Score");
            int newScore = currentScore + points;
            SetBlackboardValue("Score", newScore);
            
            Debug.Log($"[游戏进行] 得分: +{points}, 总分: {newScore}");
        }
        
        /// <summary>
        /// 更新连击系统
        /// </summary>
        private void UpdateComboSystem()
        {
            // 连击超时检查
            if (comboCount > 0 && Time.time - lastScoreTime > COMBO_TIMEOUT)
            {
                Debug.Log($"[游戏进行] 连击中断，最高连击: {comboCount}");
                comboCount = 0;
                SetBlackboardValue("ComboCount", 0);
            }
        }
        
        /// <summary>
        /// 检查关卡进度
        /// </summary>
        private void CheckLevelProgression()
        {
            int currentScore = GetBlackboardValue<int>("Score");
            int currentLevel = GetBlackboardValue<int>("Level");
            
            // 每1000分升一级
            int targetLevel = (currentScore / 1000) + 1;
            
            if (targetLevel > currentLevel)
            {
                SetBlackboardValue("Level", targetLevel);
                Debug.Log($"[游戏进行] 升级到第 {targetLevel} 关！");
                TriggerLevelUp(targetLevel);
            }
        }
        
        /// <summary>
        /// 更新游戏难度
        /// </summary>
        private void UpdateGameDifficulty()
        {
            int currentLevel = GetBlackboardValue<int>("Level");
            float difficultyMultiplier = 1f + (currentLevel - 1) * 0.1f;
            SetBlackboardValue("DifficultyMultiplier", difficultyMultiplier);
        }
        
        /// <summary>
        /// 检查游戏结束条件
        /// </summary>
        private void CheckGameEndConditions()
        {
            int lives = GetBlackboardValue<int>("Lives");
            
            // 生命值耗尽
            if (lives <= 0)
            {
                Debug.Log("[游戏进行] 生命值耗尽，游戏结束");
                ChangeState<AdvancedGameOverState>();
                return;
            }
            
            // 时间限制（如果有）
            float timeLimit = GetBlackboardValue<float>("TimeLimit", 0f);
            if (timeLimit > 0 && gameTime >= timeLimit)
            {
                Debug.Log("[游戏进行] 时间到，游戏结束");
                ChangeState<AdvancedGameOverState>();
                return;
            }
        }
        
        /// <summary>
        /// 检查是否在关键游戏时刻
        /// </summary>
        private bool IsInCriticalGameMoment()
        {
            // 例如：正在执行重要动画、保存数据等
            return comboCount > 10; // 高连击时不允许退出
        }
        
        /// <summary>
        /// 触发升级事件
        /// </summary>
        private void TriggerLevelUp(int newLevel)
        {
            // 升级奖励
            AddScore(newLevel * 50);
            
            // 可以在这里添加升级特效、音效等
        }
        
        // 辅助方法
        private void StartGameLogic() { }
        private void StopGameLogic() { }
        private void SaveFinalGameData() { }
    }
    
    /// <summary>
    /// 增强版暂停状态
    /// </summary>
    public class AdvancedPauseState : EnhancedBaseStateNode
    {
        public override int Priority => 15; // 很高优先级
        
        private float pauseStartTime;
        private bool showPauseMenu = true;
        
        public override void OnEnter()
        {
            base.OnEnter();
            
            pauseStartTime = Time.time;
            
            // 暂停游戏时间
            Time.timeScale = 0f;
            
            // 设置暂停状态
            SetBlackboardValue("GamePaused", true);
            SetBlackboardValue("PauseStartTime", pauseStartTime);
            
            Debug.Log("[暂停状态] 游戏已暂停");
        }
        
        public override void OnUpdate()
        {
            // 使用unscaledDeltaTime因为timeScale为0
            HandlePauseInput();
            UpdatePauseUI();
        }
        
        public override void OnExit()
        {
            base.OnExit();
            
            // 恢复游戏时间
            Time.timeScale = 1f;
            
            // 记录暂停时长
            float pauseDuration = Time.unscaledTime - pauseStartTime;
            SetBlackboardValue("LastPauseDuration", pauseDuration);
            SetBlackboardValue("GamePaused", false);
            
            Debug.Log($"[暂停状态] 游戏恢复，暂停时长: {pauseDuration:F2}秒");
        }
        
        public override bool CanExit()
        {
            // 暂停状态可以被用户主动退出
            return true;
        }
        
        public override string GetDebugInfo()
        {
            float pauseDuration = Time.unscaledTime - pauseStartTime;
            return base.GetDebugInfo() + 
                   $"\n暂停时长: {pauseDuration:F1}秒\n" +
                   $"显示菜单: {showPauseMenu}";
        }
        
        /// <summary>
        /// 处理暂停输入
        /// </summary>
        private void HandlePauseInput()
        {
            // 恢复游戏
            if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("[暂停状态] 恢复游戏");
                stateMachine.PopState();
                return;
            }
            
            // 返回主菜单
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[暂停状态] 返回主菜单");
                stateMachine.ClearStateStack();
                ChangeState<AdvancedMainMenuState>();
                return;
            }
            
            // 重新开始
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("[暂停状态] 重新开始游戏");
                stateMachine.ClearStateStack();
                ChangeState<AdvancedGamePlayState>();
                return;
            }
            
            // 切换菜单显示
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                showPauseMenu = !showPauseMenu;
                SetBlackboardValue("ShowPauseMenu", showPauseMenu);
            }
        }
        
        /// <summary>
        /// 更新暂停UI
        /// </summary>
        private void UpdatePauseUI()
        {
            // 这里可以添加暂停菜单的动画更新等
            float pauseDuration = Time.unscaledTime - pauseStartTime;
            SetBlackboardValue("CurrentPauseDuration", pauseDuration);
        }
    }
    
    /// <summary>
    /// 增强版游戏结束状态
    /// </summary>
    public class AdvancedGameOverState : EnhancedBaseStateNode
    {
        public override int Priority => 8; // 较高优先级
        
        private bool isHighScore = false;
        private bool dataSaved = false;
        private float displayTime = 0f;
        
        public override void OnEnter()
        {
            base.OnEnter();
            
            // 获取游戏结果
            int finalScore = GetBlackboardValue<int>("Score");
            int finalLevel = GetBlackboardValue<int>("Level");
            float gameTime = GetBlackboardValue<float>("GameTime");
            
            // 检查是否是最高分
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            isHighScore = finalScore > highScore;
            
            if (isHighScore)
            {
                PlayerPrefs.SetInt("HighScore", finalScore);
                Debug.Log($"[游戏结束] 新纪录！分数: {finalScore}");
            }
            
            // 设置结束数据
            SetBlackboardValue("FinalScore", finalScore);
            SetBlackboardValue("FinalLevel", finalLevel);
            SetBlackboardValue("FinalGameTime", gameTime);
            SetBlackboardValue("IsHighScore", isHighScore);
            SetBlackboardValue("ShowGameOverUI", true);
            
            // 保存游戏数据
            SaveGameData();
            
            Debug.Log($"[游戏结束] 游戏结束 - 分数: {finalScore}, 关卡: {finalLevel}, 时间: {gameTime:F2}秒");
        }
        
        public override void OnUpdate()
        {
            displayTime += Time.deltaTime;
            
            // 处理结束界面输入
            HandleGameOverInput();
            
            // 更新结束界面动画
            UpdateGameOverUI();
        }
        
        public override void OnExit()
        {
            base.OnExit();
            
            // 隐藏游戏结束UI
            SetBlackboardValue("ShowGameOverUI", false);
            
            Debug.Log("[游戏结束] 离开游戏结束界面");
        }
        
        public override string GetDebugInfo()
        {
            int finalScore = GetBlackboardValue<int>("FinalScore");
            return base.GetDebugInfo() + 
                   $"\n最终分数: {finalScore}\n" +
                   $"是否新纪录: {isHighScore}\n" +
                   $"数据已保存: {dataSaved}\n" +
                   $"显示时间: {displayTime:F1}秒";
        }
        
        /// <summary>
        /// 处理游戏结束输入
        /// </summary>
        private void HandleGameOverInput()
        {
            // 重新开始
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("[游戏结束] 重新开始游戏");
                RestartGame();
                return;
            }
            
            // 返回主菜单
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("[游戏结束] 返回主菜单");
                ChangeState<AdvancedMainMenuState>();
                return;
            }
            
            // 查看排行榜
            if (Input.GetKeyDown(KeyCode.L))
            {
                Debug.Log("[游戏结束] 查看排行榜");
                ChangeState<LeaderboardState>();
                return;
            }
        }
        
        /// <summary>
        /// 重新开始游戏
        /// </summary>
        private void RestartGame()
        {
            // 重置游戏数据
            SetBlackboardValue("Score", 0);
            SetBlackboardValue("Lives", 3);
            SetBlackboardValue("Level", 1);
            SetBlackboardValue("GameTime", 0f);
            
            ChangeState<AdvancedGamePlayState>();
        }
        
        /// <summary>
        /// 更新游戏结束UI
        /// </summary>
        private void UpdateGameOverUI()
        {
            // 这里可以添加结束界面的动画效果
            // 例如分数滚动显示、新纪录闪烁等
        }
        
        /// <summary>
        /// 保存游戏数据
        /// </summary>
        private void SaveGameData()
        {
            if (dataSaved) return;
            
            try
            {
                // 保存游戏统计
                int gamesPlayed = PlayerPrefs.GetInt("GamesPlayed", 0);
                PlayerPrefs.SetInt("GamesPlayed", gamesPlayed + 1);
                
                float totalPlayTime = PlayerPrefs.GetFloat("TotalPlayTime", 0f);
                float gameTime = GetBlackboardValue<float>("FinalGameTime");
                PlayerPrefs.SetFloat("TotalPlayTime", totalPlayTime + gameTime);
                
                // 保存最后游戏数据
                int finalScore = GetBlackboardValue<int>("FinalScore");
                PlayerPrefs.SetInt("LastScore", finalScore);
                
                PlayerPrefs.Save();
                dataSaved = true;
                
                Debug.Log("[游戏结束] 游戏数据保存完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[游戏结束] 保存数据失败: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// 设置状态
    /// </summary>
    public class SettingsState : EnhancedBaseStateNode
    {
        public override int Priority => 6;
        
        public override void OnEnter()
        {
            base.OnEnter();
            SetBlackboardValue("CurrentMenu", "Settings");
            Debug.Log("[设置] 进入设置界面");
        }
        
        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ChangeState<AdvancedMainMenuState>();
            }
        }
    }
    
    /// <summary>
    /// 排行榜状态
    /// </summary>
    public class LeaderboardState : EnhancedBaseStateNode
    {
        public override int Priority => 6;
        
        public override void OnEnter()
        {
            base.OnEnter();
            SetBlackboardValue("CurrentMenu", "Leaderboard");
            Debug.Log("[排行榜] 进入排行榜界面");
        }
        
        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ChangeState<AdvancedMainMenuState>();
            }
        }
    }
    
    /// <summary>
    /// 教程状态
    /// </summary>
    public class TutorialState : EnhancedBaseStateNode
    {
        public override int Priority => 7;
        
        public override void OnEnter()
        {
            base.OnEnter();
            SetBlackboardValue("CurrentMenu", "Tutorial");
            Debug.Log("[教程] 开始教程");
        }
        
        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
            {
                ChangeState<AdvancedMainMenuState>();
            }
        }
    }
}

/// <summary>
/// 高级状态转换管理器 - 实现复杂的状态转换规则
/// </summary>
public class AdvancedGameStateTransition : DefaultStateTransition
{
    public override bool CanTransition(System.Type fromState, System.Type toState)
    {
        // 基础验证
        if (fromState == null || toState == null)
            return false;
        
        // 相同状态不允许转换
        if (fromState == toState)
            return false;
        
        // 定义状态转换矩阵
        return CheckTransitionMatrix(fromState, toState);
    }
    
    public override void OnTransition(System.Type fromState, System.Type toState)
    {
        Debug.Log($"[状态转换] {fromState?.Name ?? "None"} -> {toState.Name}");
        
        // 记录状态转换日志
        LogStateTransition(fromState, toState);
        
        // 执行转换特定逻辑
        ExecuteTransitionLogic(fromState, toState);
    }
    
    /// <summary>
    /// 检查状态转换矩阵
    /// </summary>
    private bool CheckTransitionMatrix(System.Type fromState, System.Type toState)
    {
        // 使用状态名称进行匹配（简化处理）
        string from = fromState.Name;
        string to = toState.Name;
        
        // 定义允许的转换规则
        var allowedTransitions = new Dictionary<string, string[]>
        {
            ["LoadingState"] = new[] { "AdvancedMainMenuState" },
            ["AdvancedMainMenuState"] = new[] { "AdvancedGamePlayState", "SettingsState", "LeaderboardState", "TutorialState" },
            ["AdvancedGamePlayState"] = new[] { "AdvancedPauseState", "AdvancedGameOverState" },
            ["AdvancedPauseState"] = new[] { "AdvancedMainMenuState", "AdvancedGamePlayState" },
            ["AdvancedGameOverState"] = new[] { "AdvancedMainMenuState", "AdvancedGamePlayState", "LeaderboardState" },
            ["SettingsState"] = new[] { "AdvancedMainMenuState" },
            ["LeaderboardState"] = new[] { "AdvancedMainMenuState" },
            ["TutorialState"] = new[] { "AdvancedMainMenuState", "AdvancedGamePlayState" }
        };
        
        // 检查是否允许转换
        if (allowedTransitions.TryGetValue(from, out string[] allowedTargets))
        {
            for (int i = 0; i < allowedTargets.Length; i++)
            {
                if (allowedTargets[i] == to)
                    return true;
            }
        }
        
        Debug.LogWarning($"[状态转换] 不允许的转换: {from} -> {to}");
        return false;
    }
    
    /// <summary>
    /// 记录状态转换日志
    /// </summary>
    private void LogStateTransition(System.Type fromState, System.Type toState)
    {
        // 这里可以添加详细的日志记录
        // 例如：写入文件、发送到分析服务器等
    }
    
    /// <summary>
    /// 执行转换特定逻辑
    /// </summary>
    private void ExecuteTransitionLogic(System.Type fromState, System.Type toState)
    {
        // 根据具体的状态转换执行特定逻辑
        // 例如：播放转换动画、预加载资源等
    }
}