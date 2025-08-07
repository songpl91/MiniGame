using UnityEngine;
using System.Collections.Generic;

namespace Framework.Tutorial.Intrusive
{
    /// <summary>
    /// 引导条件接口
    /// 用于复合条件触发器中的条件检查
    /// </summary>
    public interface ITutorialCondition
    {
        /// <summary>
        /// 检查条件是否满足
        /// </summary>
        /// <param name="dataProvider">游戏数据提供者</param>
        /// <returns>条件是否满足</returns>
        bool CheckCondition(IGameDataProvider dataProvider);
        
        /// <summary>
        /// 获取条件描述
        /// </summary>
        /// <returns>条件描述文本</returns>
        string GetDescription();
    }
    /// <summary>
    /// 引导触发器基类
    /// 定义引导的触发条件和创建逻辑
    /// </summary>
    public abstract class TutorialTrigger
    {
        /// <summary>
        /// 引导ID（唯一标识）
        /// </summary>
        public string TutorialId { get; protected set; }
        
        /// <summary>
        /// 引导名称
        /// </summary>
        public string TutorialName { get; protected set; }
        
        /// <summary>
        /// 引导描述
        /// </summary>
        public string Description { get; protected set; }
        
        /// <summary>
        /// 优先级（数值越小优先级越高）
        /// </summary>
        public int Priority { get; protected set; }
        
        /// <summary>
        /// 前置条件（需要完成的其他引导ID列表）
        /// </summary>
        public List<string> Prerequisites { get; protected set; }
        
        /// <summary>
        /// 是否只触发一次
        /// </summary>
        public bool TriggerOnce { get; protected set; }
        
        /// <summary>
        /// 触发延迟（秒）
        /// </summary>
        public float TriggerDelay { get; protected set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        protected TutorialTrigger(string tutorialId, string tutorialName, string description = "", int priority = 100)
        {
            TutorialId = tutorialId;
            TutorialName = tutorialName;
            Description = description;
            Priority = priority;
            Prerequisites = new List<string>();
            TriggerOnce = true;
            TriggerDelay = 0f;
        }
        
        /// <summary>
        /// 设置前置条件
        /// </summary>
        public TutorialTrigger SetPrerequisites(params string[] prerequisites)
        {
            Prerequisites.Clear();
            if (prerequisites != null)
            {
                Prerequisites.AddRange(prerequisites);
            }
            return this;
        }
        
        /// <summary>
        /// 设置是否只触发一次
        /// </summary>
        public TutorialTrigger SetTriggerOnce(bool triggerOnce)
        {
            TriggerOnce = triggerOnce;
            return this;
        }
        
        /// <summary>
        /// 设置触发延迟
        /// </summary>
        public TutorialTrigger SetTriggerDelay(float delay)
        {
            TriggerDelay = delay;
            return this;
        }
        
        /// <summary>
        /// 检查触发条件（抽象方法，由子类实现）
        /// </summary>
        public abstract bool CheckCondition(IGameDataProvider gameData);
        
        /// <summary>
        /// 创建引导序列（抽象方法，由子类实现）
        /// </summary>
        public abstract List<TutorialStepBase> CreateTutorialSequence();
        
        /// <summary>
        /// 获取触发器信息
        /// </summary>
        public virtual string GetTriggerInfo()
        {
            return $"ID: {TutorialId}\n" +
                   $"名称: {TutorialName}\n" +
                   $"描述: {Description}\n" +
                   $"优先级: {Priority}\n" +
                   $"前置条件: {string.Join(", ", Prerequisites)}\n" +
                   $"只触发一次: {TriggerOnce}\n" +
                   $"触发延迟: {TriggerDelay}秒";
        }
    }
    
    /// <summary>
    /// 基于等级的引导触发器
    /// </summary>
    public class LevelBasedTrigger : TutorialTrigger
    {
        private int targetLevel;
        private bool triggerOnReach; // true: 达到等级时触发, false: 超过等级时触发
        
        public LevelBasedTrigger(string tutorialId, string tutorialName, int targetLevel, bool triggerOnReach = true, int priority = 100)
            : base(tutorialId, tutorialName, $"等级{targetLevel}触发的引导", priority)
        {
            this.targetLevel = targetLevel;
            this.triggerOnReach = triggerOnReach;
        }
        
        public override bool CheckCondition(IGameDataProvider gameData)
        {
            int currentLevel = gameData.GetPlayerLevel();
            
            if (triggerOnReach)
            {
                return currentLevel == targetLevel;
            }
            else
            {
                return currentLevel >= targetLevel;
            }
        }
        
        public override List<TutorialStepBase> CreateTutorialSequence()
        {
            // 子类需要重写此方法来创建具体的引导序列
            Debug.LogWarning($"[LevelBasedTrigger] 需要重写 CreateTutorialSequence 方法: {TutorialId}");
            return new List<TutorialStepBase>();
        }
    }
    
    /// <summary>
    /// 基于关卡的引导触发器
    /// </summary>
    public class StageBasedTrigger : TutorialTrigger
    {
        private int targetStageId;
        private bool triggerOnComplete; // true: 完成关卡时触发, false: 进入关卡时触发
        
        public StageBasedTrigger(string tutorialId, string tutorialName, int targetStageId, bool triggerOnComplete = true, int priority = 100)
            : base(tutorialId, tutorialName, $"关卡{targetStageId}触发的引导", priority)
        {
            this.targetStageId = targetStageId;
            this.triggerOnComplete = triggerOnComplete;
        }
        
        public override bool CheckCondition(IGameDataProvider gameData)
        {
            if (triggerOnComplete)
            {
                return gameData.IsStageCompleted(targetStageId);
            }
            else
            {
                return gameData.GetCurrentStageId() == targetStageId;
            }
        }
        
        public override List<TutorialStepBase> CreateTutorialSequence()
        {
            Debug.LogWarning($"[StageBasedTrigger] 需要重写 CreateTutorialSequence 方法: {TutorialId}");
            return new List<TutorialStepBase>();
        }
    }
    
    /// <summary>
    /// 基于首次进入的引导触发器
    /// </summary>
    public class FirstTimeBasedTrigger : TutorialTrigger
    {
        private string targetScene;
        private string targetUI;
        
        public FirstTimeBasedTrigger(string tutorialId, string tutorialName, string targetScene = "", string targetUI = "", int priority = 100)
            : base(tutorialId, tutorialName, "首次进入触发的引导", priority)
        {
            this.targetScene = targetScene;
            this.targetUI = targetUI;
        }
        
        public override bool CheckCondition(IGameDataProvider gameData)
        {
            // 检查是否是首次进入游戏
            if (gameData.IsFirstTimePlay())
                return true;
            
            // 检查是否是首次进入指定场景
            if (!string.IsNullOrEmpty(targetScene))
            {
                return gameData.GetCurrentSceneName() == targetScene && !gameData.GetCustomBool($"FirstEnter_{targetScene}");
            }
            
            // 检查是否是首次打开指定UI
            if (!string.IsNullOrEmpty(targetUI))
            {
                return gameData.GetCurrentUIPanel() == targetUI && !gameData.HasOpenedUI(targetUI);
            }
            
            return false;
        }
        
        public override List<TutorialStepBase> CreateTutorialSequence()
        {
            Debug.LogWarning($"[FirstTimeBasedTrigger] 需要重写 CreateTutorialSequence 方法: {TutorialId}");
            return new List<TutorialStepBase>();
        }
    }
    
    /// <summary>
    /// 基于功能解锁的引导触发器
    /// </summary>
    public class FunctionUnlockTrigger : TutorialTrigger
    {
        private string functionName;
        
        public FunctionUnlockTrigger(string tutorialId, string tutorialName, string functionName, int priority = 100)
            : base(tutorialId, tutorialName, $"功能{functionName}解锁触发的引导", priority)
        {
            this.functionName = functionName;
        }
        
        public override bool CheckCondition(IGameDataProvider gameData)
        {
            // 检查功能是否刚解锁（需要配合自定义数据记录）
            bool isUnlocked = gameData.IsFunctionUnlocked(functionName);
            bool hasTriggered = gameData.GetCustomBool($"FunctionUnlock_{functionName}_Triggered");
            
            if (isUnlocked && !hasTriggered)
            {
                // 标记为已触发
                gameData.SetCustomData($"FunctionUnlock_{functionName}_Triggered", true);
                return true;
            }
            
            return false;
        }
        
        public override List<TutorialStepBase> CreateTutorialSequence()
        {
            Debug.LogWarning($"[FunctionUnlockTrigger] 需要重写 CreateTutorialSequence 方法: {TutorialId}");
            return new List<TutorialStepBase>();
        }
    }
    
    /// <summary>
    /// 基于时间的引导触发器
    /// </summary>
    public class TimeBasedTrigger : TutorialTrigger
    {
        private float targetPlayTime; // 目标游戏时长（秒）
        private int targetLoginDays; // 目标连续登录天数
        
        public TimeBasedTrigger(string tutorialId, string tutorialName, float targetPlayTime = 0f, int targetLoginDays = 0, int priority = 100)
            : base(tutorialId, tutorialName, "基于时间触发的引导", priority)
        {
            this.targetPlayTime = targetPlayTime;
            this.targetLoginDays = targetLoginDays;
        }
        
        public override bool CheckCondition(IGameDataProvider gameData)
        {
            // 检查游戏时长
            if (targetPlayTime > 0 && gameData.GetTotalPlayTime() >= targetPlayTime)
            {
                return true;
            }
            
            // 检查连续登录天数
            if (targetLoginDays > 0 && gameData.GetConsecutiveLoginDays() >= targetLoginDays)
            {
                return true;
            }
            
            return false;
        }
        
        public override List<TutorialStepBase> CreateTutorialSequence()
        {
            Debug.LogWarning($"[TimeBasedTrigger] 需要重写 CreateTutorialSequence 方法: {TutorialId}");
            return new List<TutorialStepBase>();
        }
    }
    
    /// <summary>
    /// 复合条件引导触发器
    /// </summary>
    public class CompositeConditionTrigger : TutorialTrigger
    {
        private System.Func<IGameDataProvider, bool> conditionChecker;
        private System.Func<List<TutorialStepBase>> sequenceCreator;
        
        public CompositeConditionTrigger(string tutorialId, string tutorialName, 
            System.Func<IGameDataProvider, bool> conditionChecker,
            System.Func<List<TutorialStepBase>> sequenceCreator,
            int priority = 100)
            : base(tutorialId, tutorialName, "复合条件触发的引导", priority)
        {
            this.conditionChecker = conditionChecker;
            this.sequenceCreator = sequenceCreator;
        }
        
        public override bool CheckCondition(IGameDataProvider gameData)
        {
            return conditionChecker?.Invoke(gameData) ?? false;
        }
        
        public override List<TutorialStepBase> CreateTutorialSequence()
        {
            return sequenceCreator?.Invoke() ?? new List<TutorialStepBase>();
        }
    }
}