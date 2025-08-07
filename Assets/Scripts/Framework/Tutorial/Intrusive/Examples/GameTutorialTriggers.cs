using UnityEngine;
using System.Collections.Generic;

namespace Framework.Tutorial.Intrusive.Examples
{
    /// <summary>
    /// 游戏引导触发器具体实现示例
    /// 展示如何为实际游戏场景创建引导触发器
    /// </summary>
    public static class GameTutorialTriggers
    {
        /// <summary>
        /// 新手引导 - 首次进入游戏
        /// </summary>
        public class NewPlayerTutorial : FirstTimeBasedTrigger
        {
            public NewPlayerTutorial() : base("NewPlayerTutorial", "新手引导", "首次进入游戏的基础引导")
            {
                Priority = 1000; // 最高优先级
            }
            
            public override List<TutorialStepBase> CreateTutorialSequence()
            {
                var steps = new List<TutorialStepBase>();
                
                // 欢迎消息
                steps.Add(new ShowMessageStep
                {
                    Message = "欢迎来到游戏世界！",
                    Duration = 3f,
                    ShowSkipButton = false
                });
                
                // 点击开始游戏按钮
                steps.Add(new ClickButtonStep
                {
                    TargetPath = "Canvas/MainMenu/StartButton",
                    HighlightType = HighlightType.Pulse,
                    HintMessage = "点击开始游戏",
                    AllowSkip = false
                });
                
                // 等待进入主城 - 使用延迟检测方案
                steps.Add(new DelayedDetectionTutorialStep(
                    "waitMainCity",
                    "等待进入主城",
                    "Canvas/MainUI", // 检测主城UI是否加载
                    "正在进入主城...",
                    3.0f // 3秒初始延迟
                )
                {
                    SetDetectionParams(3.0f, 1.0f, 10) // 3秒延迟，1秒重试间隔，最多10次重试
                });
                
                // 介绍主城界面
                steps.Add(new ShowMessageStep
                {
                    Message = "这里是主城，你可以在这里管理角色、装备和进行各种活动。",
                    Duration = 4f
                });
                
                return steps;
            }
        }
        
        /// <summary>
        /// 背包系统引导 - 等级达到5级时触发
        /// </summary>
        public class InventoryTutorial : LevelBasedTrigger
        {
            public InventoryTutorial() : base("InventoryTutorial", "背包系统引导", "介绍背包系统的使用", 5)
            {
                Priority = 800;
            }
            
            public override List<TutorialStepBase> CreateTutorialSequence()
            {
                var steps = new List<TutorialStepBase>();
                
                // 提示背包功能解锁
                steps.Add(new ShowMessageStep
                {
                    Message = "恭喜！背包系统已解锁，让我们来看看如何使用它。",
                    Duration = 3f
                });
                
                // 点击背包按钮
                steps.Add(new ClickButtonStep
                {
                    TargetPath = "Canvas/MainUI/BottomPanel/InventoryButton",
                    HighlightType = HighlightType.Glow,
                    HintMessage = "点击打开背包",
                    AllowSkip = true
                });
                
                // 介绍背包界面
                steps.Add(new ShowMessageStep
                {
                    Message = "在背包中，你可以查看和管理所有物品。",
                    Duration = 3f
                });
                
                // 点击某个物品
                steps.Add(new ClickButtonStep
                {
                    TargetPath = "Canvas/InventoryPanel/ItemGrid/Item_0",
                    HighlightType = HighlightType.Pulse,
                    HintMessage = "点击查看物品详情",
                    AllowSkip = true
                });
                
                return steps;
            }
        }
        
        /// <summary>
        /// 装备系统引导 - 等级达到10级且背包引导完成
        /// </summary>
        public class EquipmentTutorial : CompositeConditionTrigger
        {
            public EquipmentTutorial() : base("EquipmentTutorial", "装备系统引导", "介绍装备系统的使用")
            {
                Priority = 700;
                
                // 添加复合条件
                AddCondition(new LevelCondition(10));
                AddCondition(new TutorialCompletedCondition("InventoryTutorial"));
                AddCondition(new FunctionUnlockedCondition("Equipment"));
            }
            
            public override List<TutorialStepBase> CreateTutorialSequence()
            {
                var steps = new List<TutorialStepBase>();
                
                // 提示装备功能解锁
                steps.Add(new ShowMessageStep
                {
                    Message = "装备系统已解锁！现在可以装备武器和防具来提升战斗力。",
                    Duration = 3f
                });
                
                // 点击装备按钮 - 使用延迟检测方案
                steps.Add(new DelayedDetectionTutorialStep(
                    "clickEquipment",
                    "点击装备按钮",
                    "Canvas/MainUI/BottomPanel/EquipmentButton",
                    "点击打开装备界面",
                    1.0f // 1秒初始延迟
                ));
                
                // 介绍装备界面
                steps.Add(new ShowMessageStep
                {
                    Message = "在装备界面，你可以穿戴和管理各种装备。",
                    Duration = 3f
                });
                
                // 点击装备槽
                steps.Add(new ClickButtonStep
                {
                    TargetPath = "Canvas/EquipmentPanel/EquipSlots/WeaponSlot",
                    HighlightType = HighlightType.Pulse,
                    HintMessage = "点击武器槽位装备武器",
                    AllowSkip = true
                });
                
                return steps;
            }
        }
        
        /// <summary>
        /// 战斗系统引导 - 完成第一关后触发
        /// </summary>
        public class BattleTutorial : StageBasedTrigger
        {
            public BattleTutorial() : base("BattleTutorial", "战斗系统引导", "介绍战斗系统的操作", 1)
            {
                Priority = 900;
            }
            
            public override List<TutorialStepBase> CreateTutorialSequence()
            {
                var steps = new List<TutorialStepBase>();
                
                // 战斗前准备
                steps.Add(new ShowMessageStep
                {
                    Message = "准备进入战斗！让我教你如何操作。",
                    Duration = 3f
                });
                
                // 点击挑战按钮
                steps.Add(new ClickButtonStep
                {
                    TargetPath = "Canvas/StagePanel/ChallengeButton",
                    HighlightType = HighlightType.Glow,
                    HintMessage = "点击挑战进入战斗",
                    AllowSkip = false
                });
                
                // 等待进入战斗场景 - 使用延迟检测方案
                steps.Add(new DelayedDetectionTutorialStep(
                    "waitBattle",
                    "等待进入战斗",
                    "Canvas/BattleUI", // 检测战斗UI是否加载
                    "正在进入战斗...",
                    2.0f // 2秒初始延迟
                ));
                
                // 战斗操作指导
                steps.Add(new ShowMessageStep
                {
                    Message = "使用技能按钮释放技能攻击敌人！",
                    Duration = 3f
                });
                
                steps.Add(new ClickButtonStep
                {
                    TargetPath = "Canvas/BattleUI/SkillPanel/Skill_1",
                    HighlightType = HighlightType.Pulse,
                    HintMessage = "点击释放技能",
                    AllowSkip = true
                });
                
                return steps;
            }
        }
        
        /// <summary>
        /// 商店系统引导 - 金币达到1000时触发
        /// </summary>
        public class ShopTutorial : TutorialTrigger
        {
            public ShopTutorial() : base("ShopTutorial", "商店系统引导", "介绍商店系统的使用")
            {
                Priority = 600;
            }
            
            public override bool CheckCondition(IGameDataProvider dataProvider)
            {
                // 金币达到1000且商店功能已解锁
                return dataProvider.GetPlayerGold() >= 1000 && 
                       dataProvider.IsFunctionUnlocked("Shop");
            }
            
            public override List<TutorialStepBase> CreateTutorialSequence()
            {
                var steps = new List<TutorialStepBase>();
                
                steps.Add(new ShowMessageStep
                {
                    Message = "你已经积累了足够的金币，可以去商店购买物品了！",
                    Duration = 3f
                });
                
                steps.Add(new ClickButtonStep
                {
                    TargetPath = "Canvas/MainUI/BottomPanel/ShopButton",
                    HighlightType = HighlightType.Glow,
                    HintMessage = "点击打开商店",
                    AllowSkip = true
                });
                
                steps.Add(new ShowMessageStep
                {
                    Message = "在商店中，你可以购买各种有用的物品和装备。",
                    Duration = 3f
                });
                
                return steps;
            }
        }
        
        /// <summary>
        /// 公会系统引导 - 等级达到20级且游戏时长超过1小时
        /// </summary>
        public class GuildTutorial : CompositeConditionTrigger
        {
            public GuildTutorial() : base("GuildTutorial", "公会系统引导", "介绍公会系统的功能")
            {
                Priority = 500;
                
                // 添加复合条件
                AddCondition(new LevelCondition(20));
                AddCondition(new PlayTimeCondition(3600f)); // 1小时
                AddCondition(new FunctionUnlockedCondition("Guild"));
                AddCondition(new NotInGuildCondition());
            }
            
            public override List<TutorialStepBase> CreateTutorialSequence()
            {
                var steps = new List<TutorialStepBase>();
                
                steps.Add(new ShowMessageStep
                {
                    Message = "公会系统已开放！加入公会可以获得更多资源和社交体验。",
                    Duration = 4f
                });
                
                steps.Add(new ClickButtonStep
                {
                    TargetPath = "Canvas/MainUI/TopPanel/GuildButton",
                    HighlightType = HighlightType.Glow,
                    HintMessage = "点击打开公会界面",
                    AllowSkip = true
                });
                
                steps.Add(new ShowMessageStep
                {
                    Message = "你可以创建自己的公会，或者加入其他玩家的公会。",
                    Duration = 3f
                });
                
                return steps;
            }
        }
        
        /// <summary>
        /// 每日任务引导 - 连续登录3天时触发
        /// </summary>
        public class DailyQuestTutorial : TimeBasedTrigger
        {
            public DailyQuestTutorial() : base("DailyQuestTutorial", "每日任务引导", "介绍每日任务系统")
            {
                Priority = 400;
                TriggerType = TimeBasedTrigger.TimeTriggerType.ConsecutiveLoginDays;
                RequiredValue = 3;
            }
            
            public override List<TutorialStepBase> CreateTutorialSequence()
            {
                var steps = new List<TutorialStepBase>();
                
                steps.Add(new ShowMessageStep
                {
                    Message = "你已经连续登录3天了！每日任务系统可以帮你获得更多奖励。",
                    Duration = 4f
                });
                
                steps.Add(new ClickButtonStep
                {
                    TargetPath = "Canvas/MainUI/TopPanel/QuestButton",
                    HighlightType = HighlightType.Glow,
                    HintMessage = "点击查看每日任务",
                    AllowSkip = true
                });
                
                steps.Add(new ShowMessageStep
                {
                    Message = "完成每日任务可以获得经验、金币和其他珍贵奖励。",
                    Duration = 3f
                });
                
                return steps;
            }
        }
    }
    
    #region 自定义条件类
    
    /// <summary>
    /// 等级条件
    /// </summary>
    public class LevelCondition : ITutorialCondition
    {
        private int requiredLevel;
        
        public LevelCondition(int level)
        {
            requiredLevel = level;
        }
        
        public bool CheckCondition(IGameDataProvider dataProvider)
        {
            return dataProvider.GetPlayerLevel() >= requiredLevel;
        }
        
        public string GetDescription()
        {
            return $"等级达到 {requiredLevel}";
        }
    }
    
    /// <summary>
    /// 引导完成条件
    /// </summary>
    public class TutorialCompletedCondition : ITutorialCondition
    {
        private string tutorialId;
        
        public TutorialCompletedCondition(string id)
        {
            tutorialId = id;
        }
        
        public bool CheckCondition(IGameDataProvider dataProvider)
        {
            return dataProvider.GetCustomBool($"Tutorial_{tutorialId}_Completed");
        }
        
        public string GetDescription()
        {
            return $"引导 {tutorialId} 已完成";
        }
    }
    
    /// <summary>
    /// 功能解锁条件
    /// </summary>
    public class FunctionUnlockedCondition : ITutorialCondition
    {
        private string functionName;
        
        public FunctionUnlockedCondition(string name)
        {
            functionName = name;
        }
        
        public bool CheckCondition(IGameDataProvider dataProvider)
        {
            return dataProvider.IsFunctionUnlocked(functionName);
        }
        
        public string GetDescription()
        {
            return $"功能 {functionName} 已解锁";
        }
    }
    
    /// <summary>
    /// 游戏时长条件
    /// </summary>
    public class PlayTimeCondition : ITutorialCondition
    {
        private float requiredTime;
        
        public PlayTimeCondition(float time)
        {
            requiredTime = time;
        }
        
        public bool CheckCondition(IGameDataProvider dataProvider)
        {
            return dataProvider.GetTotalPlayTime() >= requiredTime;
        }
        
        public string GetDescription()
        {
            return $"游戏时长达到 {requiredTime / 60f:F1} 分钟";
        }
    }
    
    /// <summary>
    /// 不在公会条件
    /// </summary>
    public class NotInGuildCondition : ITutorialCondition
    {
        public bool CheckCondition(IGameDataProvider dataProvider)
        {
            return !dataProvider.IsInGuild();
        }
        
        public string GetDescription()
        {
            return "未加入公会";
        }
    }
    
    #endregion
}