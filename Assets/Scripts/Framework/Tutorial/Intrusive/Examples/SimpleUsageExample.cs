using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Tutorial.Intrusive.Examples
{
    /// <summary>
    /// 简单使用示例
    /// 展示如何快速集成新的自动触发引导系统
    /// </summary>
    public class SimpleUsageExample : MonoBehaviour
    {
        [Header("UI引用")]
        [SerializeField] private Button shopButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button battleButton;
        
        [Header("系统配置")]
        [SerializeField] private bool autoSetup = true;
        
        void Start()
        {
            if (autoSetup)
            {
                SetupTutorialSystem();
            }
        }
        
        /// <summary>
        /// 设置引导系统
        /// </summary>
        public void SetupTutorialSystem()
        {
            // 1. 获取或创建触发系统
            var triggerSystem = TutorialTriggerSystem.Instance;
            
            // 2. 设置游戏数据提供者
            var gameDataProvider = new SimpleGameDataProvider();
            triggerSystem.SetGameDataProvider(gameDataProvider);
            
            // 3. 注册引导触发器
            RegisterTutorials(triggerSystem);
            
            // 4. 启用系统
            triggerSystem.SetEnabled(true);
            
            Debug.Log("引导系统设置完成！");
        }
        
        /// <summary>
        /// 注册引导触发器
        /// </summary>
        private void RegisterTutorials(TutorialTriggerSystem triggerSystem)
        {
            // 新手引导 - 首次进入游戏
            triggerSystem.RegisterTrigger(new WelcomeTutorial());
            
            // 商店引导 - 等级达到3级
            triggerSystem.RegisterTrigger(new ShopTutorial(shopButton));
            
            // 背包引导 - 等级达到5级
            triggerSystem.RegisterTrigger(new InventoryTutorial(inventoryButton));
            
            // 战斗引导 - 等级达到10级
            triggerSystem.RegisterTrigger(new BattleTutorial(battleButton));
        }
        
        #region 测试方法
        
        /// <summary>
        /// 模拟玩家升级
        /// </summary>
        [ContextMenu("模拟升级")]
        public void SimulatePlayerLevelUp()
        {
            var dataProvider = TutorialTriggerSystem.Instance.GameDataProvider as SimpleGameDataProvider;
            if (dataProvider != null)
            {
                dataProvider.SimulatePlayerLevelUp();
                Debug.Log($"玩家升级到 {dataProvider.GetPlayerLevel()} 级");
            }
        }
        
        /// <summary>
        /// 重置引导数据
        /// </summary>
        [ContextMenu("重置引导数据")]
        public void ResetTutorialData()
        {
            TutorialTriggerSystem.Instance.ResetAllTutorialProgress();
            Debug.Log("引导数据已重置");
        }
        
        #endregion
    }
    
    #region 简单的游戏数据提供者
    
    /// <summary>
    /// 简单的游戏数据提供者
    /// 用于演示目的
    /// </summary>
    public class SimpleGameDataProvider : IGameDataProvider
    {
        private int playerLevel = 1;
        private bool hasEnteredGame = false;
        
        public int GetPlayerLevel() => playerLevel;
        public int GetCurrentStage() => 1;
        public bool HasCompletedTutorial(string tutorialId) => false;
        public bool IsFirstTimeEntering(string sceneOrFeature) => !hasEnteredGame;
        public bool IsFunctionUnlocked(string functionName) => true;
        public float GetTotalPlayTime() => Time.time;
        public int GetConsecutiveLoginDays() => 1;
        public bool IsInGuild() => false;
        public int GetPlayerGold() => 1000;
        public int GetPlayerExp() => playerLevel * 100;
        public bool HasItem(string itemId) => false;
        public bool IsItemEquipped(string itemId) => false;
        public bool IsUIOpen(string uiName) => false;
        public bool HasCompletedAchievement(string achievementId) => false;
        public bool HasCompletedQuest(string questId) => false;
        public int GetFriendCount() => 0;
        
        /// <summary>
        /// 模拟玩家升级
        /// </summary>
        public void SimulatePlayerLevelUp()
        {
            playerLevel++;
            hasEnteredGame = true;
        }
    }
    
    #endregion
    
    #region 简单的引导触发器示例
    
    /// <summary>
    /// 欢迎引导 - 首次进入游戏
    /// </summary>
    public class WelcomeTutorial : FirstTimeBasedTrigger
    {
        public WelcomeTutorial() : base("welcome_tutorial", "欢迎引导", "game") { }
        
        public override List<TutorialStepBase> CreateTutorialSequence()
        {
            return new List<TutorialStepBase>
            {
                new ShowMessageStep("欢迎来到游戏世界！"),
                new ShowMessageStep("让我们开始你的冒险之旅吧！"),
                new WaitTimeStep(2f, "等待2秒")
            };
        }
    }
    
    /// <summary>
    /// 商店引导 - 等级达到3级
    /// </summary>
    public class ShopTutorial : LevelBasedTrigger
    {
        private Button shopButton;
        
        public ShopTutorial(Button button) : base("shop_tutorial", "商店引导", 3)
        {
            shopButton = button;
        }
        
        public override List<TutorialStepBase> CreateTutorialSequence()
        {
            return new List<TutorialStepBase>
            {
                new ShowMessageStep("恭喜达到3级！商店功能已解锁！"),
                new ClickButtonStep(shopButton?.transform, "点击商店按钮查看商品")
            };
        }
    }
    
    /// <summary>
    /// 背包引导 - 等级达到5级
    /// </summary>
    public class InventoryTutorial : LevelBasedTrigger
    {
        private Button inventoryButton;
        
        public InventoryTutorial(Button button) : base("inventory_tutorial", "背包引导", 5)
        {
            inventoryButton = button;
        }
        
        public override List<TutorialStepBase> CreateTutorialSequence()
        {
            return new List<TutorialStepBase>
            {
                new ShowMessageStep("背包功能已解锁！"),
                new ShowMessageStep("你可以在这里管理你的物品"),
                new ClickButtonStep(inventoryButton?.transform, "点击背包按钮")
            };
        }
    }
    
    /// <summary>
    /// 战斗引导 - 等级达到10级
    /// </summary>
    public class BattleTutorial : LevelBasedTrigger
    {
        private Button battleButton;
        
        public BattleTutorial(Button button) : base("battle_tutorial", "战斗引导", 10)
        {
            battleButton = button;
        }
        
        public override List<TutorialStepBase> CreateTutorialSequence()
        {
            return new List<TutorialStepBase>
            {
                new ShowMessageStep("你已经足够强大了！"),
                new ShowMessageStep("是时候开始真正的战斗了！"),
                new ClickButtonStep(battleButton?.transform, "点击战斗按钮开始冒险")
            };
        }
    }
    
    #endregion
}