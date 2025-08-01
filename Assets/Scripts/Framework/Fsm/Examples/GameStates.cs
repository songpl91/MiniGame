using UnityEngine;

/// <summary>
/// 游戏状态示例 - 展示如何使用增强后的状态机框架
/// </summary>
namespace GameStateExamples
{
    /// <summary>
    /// 游戏主菜单状态
    /// </summary>
    public class MainMenuState : BaseStateNode
    {
        public override int Priority => 1; // 低优先级
        
        public override void OnEnter()
        {
            base.OnEnter();
            // 显示主菜单UI
            SetBlackboardValue("CurrentMenu", "MainMenu");
            Debug.Log("[主菜单] 显示主菜单界面");
        }
        
        public override void OnUpdate()
        {
            // 检测用户输入
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("[主菜单] 开始游戏");
                ChangeState<GamePlayState>();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[主菜单] 退出游戏");
                Application.Quit();
            }
        }
        
        public override void OnExit()
        {
            base.OnExit();
            // 隐藏主菜单UI
            Debug.Log("[主菜单] 隐藏主菜单界面");
        }
    }
    
    /// <summary>
    /// 游戏进行状态
    /// </summary>
    public class GamePlayState : BaseStateNode
    {
        public override int Priority => 5; // 中等优先级
        
        private float gameTime;
        
        public override void OnEnter()
        {
            base.OnEnter();
            gameTime = 0f;
            SetBlackboardValue("GameStartTime", Time.time);
            Debug.Log("[游戏进行] 游戏开始");
        }
        
        public override void OnUpdate()
        {
            gameTime += Time.deltaTime;
            SetBlackboardValue("GameTime", gameTime);
            
            // 检测暂停输入
            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log("[游戏进行] 暂停游戏");
                stateMachine.PushState<PauseState>();
            }
            
            // 检测游戏结束条件
            if (gameTime > 60f) // 60秒后游戏结束
            {
                Debug.Log("[游戏进行] 游戏时间到，结束游戏");
                ChangeState<GameOverState>();
            }
        }
        
        public override void OnExit()
        {
            base.OnExit();
            float finalTime = GetBlackboardValue<float>("GameTime");
            SetBlackboardValue("FinalScore", Mathf.RoundToInt(finalTime * 10));
            Debug.Log($"[游戏进行] 游戏结束，最终时间: {finalTime:F2}秒");
        }
    }
    
    /// <summary>
    /// 暂停状态
    /// </summary>
    public class PauseState : BaseStateNode
    {
        public override int Priority => 10; // 高优先级，不能被其他状态打断
        
        public override bool CanExit()
        {
            // 只有用户主动恢复才能退出暂停状态
            return Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape);
        }
        
        public override void OnEnter()
        {
            base.OnEnter();
            Time.timeScale = 0f; // 暂停游戏时间
            Debug.Log("[暂停] 游戏已暂停");
        }
        
        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log("[暂停] 恢复游戏");
                stateMachine.PopState(); // 恢复到之前的状态
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[暂停] 返回主菜单");
                stateMachine.ClearStateStack(); // 清空状态栈
                ChangeState<MainMenuState>();
            }
        }
        
        public override void OnExit()
        {
            base.OnExit();
            Time.timeScale = 1f; // 恢复游戏时间
            Debug.Log("[暂停] 游戏恢复");
        }
    }
    
    /// <summary>
    /// 游戏结束状态
    /// </summary>
    public class GameOverState : BaseStateNode
    {
        public override int Priority => 3; // 中等优先级
        
        public override void OnEnter()
        {
            base.OnEnter();
            int finalScore = GetBlackboardValue<int>("FinalScore");
            Debug.Log($"[游戏结束] 最终得分: {finalScore}");
            
            // 显示游戏结束UI
            SetBlackboardValue("ShowGameOverUI", true);
        }
        
        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("[游戏结束] 重新开始游戏");
                ChangeState<GamePlayState>();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[游戏结束] 返回主菜单");
                ChangeState<MainMenuState>();
            }
        }
        
        public override void OnExit()
        {
            base.OnExit();
            // 隐藏游戏结束UI
            SetBlackboardValue("ShowGameOverUI", false);
            Debug.Log("[游戏结束] 离开游戏结束界面");
        }
    }
    
    /// <summary>
    /// 自定义状态转换管理器示例
    /// </summary>
    public class GameStateTransition : DefaultStateTransition
    {
        public override bool CanTransition(System.Type fromState, System.Type toState)
        {
            // 定义状态转换规则
            
            // 从主菜单只能进入游戏
            if (fromState == typeof(MainMenuState))
            {
                return toState == typeof(GamePlayState);
            }
            
            // 从游戏进行状态可以进入暂停或游戏结束
            if (fromState == typeof(GamePlayState))
            {
                return toState == typeof(PauseState) || toState == typeof(GameOverState);
            }
            
            // 从暂停状态可以返回主菜单
            if (fromState == typeof(PauseState))
            {
                return toState == typeof(MainMenuState);
            }
            
            // 从游戏结束可以重新开始或返回主菜单
            if (fromState == typeof(GameOverState))
            {
                return toState == typeof(GamePlayState) || toState == typeof(MainMenuState);
            }
            
            return base.CanTransition(fromState, toState);
        }
        
        public override void OnTransition(System.Type fromState, System.Type toState)
        {
            Debug.Log($"[状态转换] {fromState.Name} -> {toState.Name}");
            
            // 可以在这里添加状态转换时的特殊逻辑
            // 比如播放转换动画、保存数据等
        }
    }
}