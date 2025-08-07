using UnityEngine;
using System.Collections.Generic;

namespace Framework.Tutorial.Intrusive.Examples
{
    /// <summary>
    /// 游戏数据提供者示例实现
    /// 在实际项目中，这个类应该连接到真实的游戏数据系统
    /// </summary>
    public class GameDataProvider : MonoBehaviour, IGameDataProvider
    {
        [Header("玩家数据模拟")]
        public int playerLevel = 1;
        public int playerExp = 0;
        public int playerGold = 100;
        public int playerDiamond = 10;
        public bool isFirstTimePlay = true;
        public float totalPlayTime = 0f;
        
        [Header("关卡数据模拟")]
        public int currentStageId = 1;
        public int maxPassedStageId = 0;
        public List<int> completedStages = new List<int>();
        
        [Header("功能解锁模拟")]
        public List<string> unlockedFunctions = new List<string>();
        public List<string> unlockedBuildings = new List<string>();
        public List<string> unlockedEquipments = new List<string>();
        public List<string> unlockedSkills = new List<string>();
        
        [Header("界面状态模拟")]
        public string currentSceneName = "MainScene";
        public string currentUIPanel = "";
        public List<string> openedUIs = new List<string>();
        public bool isInBattle = false;
        public bool isInMainCity = true;
        
        [Header("物品装备模拟")]
        public Dictionary<string, int> inventory = new Dictionary<string, int>();
        public List<string> ownedEquipments = new List<string>();
        public int equippedItemCount = 0;
        public bool isInventoryFull = false;
        
        [Header("时间数据模拟")]
        public int todayLoginCount = 1;
        public int consecutiveLoginDays = 1;
        
        [Header("社交数据模拟")]
        public bool isInGuild = false;
        public int friendCount = 0;
        public bool hasUnreadMessages = false;
        
        // 自定义数据存储
        private Dictionary<string, object> customData = new Dictionary<string, object>();
        
        // 上次操作时间记录
        private Dictionary<string, float> lastActionTimes = new Dictionary<string, float>();
        
        void Start()
        {
            InitializeTestData();
        }
        
        /// <summary>
        /// 初始化测试数据
        /// </summary>
        private void InitializeTestData()
        {
            // 初始化背包物品
            inventory["Sword"] = 1;
            inventory["Shield"] = 1;
            inventory["Potion"] = 5;
            
            // 初始化已拥有装备
            ownedEquipments.Add("BasicSword");
            ownedEquipments.Add("BasicShield");
            
            // 初始化解锁功能
            unlockedFunctions.Add("Inventory");
            unlockedFunctions.Add("Equipment");
            
            Debug.Log("[GameDataProvider] 测试数据初始化完成");
        }
        
        #region 玩家基础数据实现
        
        public int GetPlayerLevel() => playerLevel;
        public int GetPlayerExp() => playerExp;
        public int GetPlayerGold() => playerGold;
        public int GetPlayerDiamond() => playerDiamond;
        public bool IsFirstTimePlay() => isFirstTimePlay;
        public float GetTotalPlayTime() => totalPlayTime + Time.time;
        
        #endregion
        
        #region 关卡进度数据实现
        
        public int GetCurrentStageId() => currentStageId;
        public int GetMaxPassedStageId() => maxPassedStageId;
        public bool IsStageCompleted(int stageId) => completedStages.Contains(stageId);
        public int GetStageStars(int stageId) => IsStageCompleted(stageId) ? Random.Range(1, 4) : 0;
        
        #endregion
        
        #region 功能解锁状态实现
        
        public bool IsFunctionUnlocked(string functionName) => unlockedFunctions.Contains(functionName);
        public bool IsBuildingUnlocked(string buildingName) => unlockedBuildings.Contains(buildingName);
        public bool IsEquipmentUnlocked(string equipmentId) => unlockedEquipments.Contains(equipmentId);
        public bool IsSkillUnlocked(string skillId) => unlockedSkills.Contains(skillId);
        
        #endregion
        
        #region 界面状态实现
        
        public string GetCurrentSceneName() => currentSceneName;
        public string GetCurrentUIPanel() => currentUIPanel;
        public bool HasOpenedUI(string uiName) => openedUIs.Contains(uiName);
        public bool IsInBattle() => isInBattle;
        public bool IsInMainCity() => isInMainCity;
        
        #endregion
        
        #region 物品和装备实现
        
        public int GetItemCount(string itemId)
        {
            return inventory.ContainsKey(itemId) ? inventory[itemId] : 0;
        }
        
        public bool HasEquipment(string equipmentId) => ownedEquipments.Contains(equipmentId);
        public int GetEquippedItemCount() => equippedItemCount;
        public bool IsInventoryFull() => isInventoryFull;
        
        #endregion
        
        #region 成就和任务实现
        
        public bool IsAchievementCompleted(string achievementId)
        {
            return GetCustomBool($"Achievement_{achievementId}");
        }
        
        public bool IsQuestCompleted(string questId)
        {
            return GetCustomBool($"Quest_{questId}");
        }
        
        public int GetActiveQuestCount()
        {
            return GetCustomInt("ActiveQuestCount", 0);
        }
        
        #endregion
        
        #region 时间相关实现
        
        public int GetTodayLoginCount() => todayLoginCount;
        public int GetConsecutiveLoginDays() => consecutiveLoginDays;
        
        public bool IsInTimeRange(int startHour, int endHour)
        {
            int currentHour = System.DateTime.Now.Hour;
            if (startHour <= endHour)
            {
                return currentHour >= startHour && currentHour <= endHour;
            }
            else
            {
                // 跨天的时间范围
                return currentHour >= startHour || currentHour <= endHour;
            }
        }
        
        public float GetTimeSinceLastAction(string actionName)
        {
            if (lastActionTimes.ContainsKey(actionName))
            {
                return Time.time - lastActionTimes[actionName];
            }
            return float.MaxValue; // 从未执行过该操作
        }
        
        #endregion
        
        #region 社交和公会实现
        
        public bool IsInGuild() => isInGuild;
        public int GetFriendCount() => friendCount;
        public bool HasUnreadMessages() => hasUnreadMessages;
        
        #endregion
        
        #region 自定义数据实现
        
        public int GetCustomInt(string key, int defaultValue = 0)
        {
            if (customData.ContainsKey(key) && customData[key] is int)
            {
                return (int)customData[key];
            }
            return defaultValue;
        }
        
        public float GetCustomFloat(string key, float defaultValue = 0f)
        {
            if (customData.ContainsKey(key) && customData[key] is float)
            {
                return (float)customData[key];
            }
            return defaultValue;
        }
        
        public string GetCustomString(string key, string defaultValue = "")
        {
            if (customData.ContainsKey(key) && customData[key] is string)
            {
                return (string)customData[key];
            }
            return defaultValue;
        }
        
        public bool GetCustomBool(string key, bool defaultValue = false)
        {
            if (customData.ContainsKey(key) && customData[key] is bool)
            {
                return (bool)customData[key];
            }
            return defaultValue;
        }
        
        public void SetCustomData(string key, object value)
        {
            customData[key] = value;
        }
        
        #endregion
        
        #region 测试用的数据修改方法
        
        /// <summary>
        /// 升级玩家等级（测试用）
        /// </summary>
        public void LevelUp()
        {
            playerLevel++;
            Debug.Log($"[GameDataProvider] 玩家升级到 {playerLevel} 级");
        }
        
        /// <summary>
        /// 完成关卡（测试用）
        /// </summary>
        public void CompleteStage(int stageId)
        {
            if (!completedStages.Contains(stageId))
            {
                completedStages.Add(stageId);
                maxPassedStageId = Mathf.Max(maxPassedStageId, stageId);
                Debug.Log($"[GameDataProvider] 完成关卡 {stageId}");
            }
        }
        
        /// <summary>
        /// 解锁功能（测试用）
        /// </summary>
        public void UnlockFunction(string functionName)
        {
            if (!unlockedFunctions.Contains(functionName))
            {
                unlockedFunctions.Add(functionName);
                Debug.Log($"[GameDataProvider] 解锁功能: {functionName}");
            }
        }
        
        /// <summary>
        /// 切换场景（测试用）
        /// </summary>
        public void ChangeScene(string sceneName)
        {
            currentSceneName = sceneName;
            Debug.Log($"[GameDataProvider] 切换到场景: {sceneName}");
        }
        
        /// <summary>
        /// 打开UI界面（测试用）
        /// </summary>
        public void OpenUI(string uiName)
        {
            currentUIPanel = uiName;
            if (!openedUIs.Contains(uiName))
            {
                openedUIs.Add(uiName);
            }
            Debug.Log($"[GameDataProvider] 打开UI: {uiName}");
        }
        
        /// <summary>
        /// 记录操作时间（测试用）
        /// </summary>
        public void RecordAction(string actionName)
        {
            lastActionTimes[actionName] = Time.time;
            Debug.Log($"[GameDataProvider] 记录操作: {actionName}");
        }
        
        /// <summary>
        /// 设置首次游戏状态（测试用）
        /// </summary>
        public void SetFirstTimePlay(bool isFirstTime)
        {
            isFirstTimePlay = isFirstTime;
            Debug.Log($"[GameDataProvider] 设置首次游戏: {isFirstTime}");
        }
        
        #endregion
        
        #region 调试信息
        
        /// <summary>
        /// 获取调试信息
        /// </summary>
        public string GetDebugInfo()
        {
            return $"玩家等级: {playerLevel}\n" +
                   $"当前关卡: {currentStageId}\n" +
                   $"最高关卡: {maxPassedStageId}\n" +
                   $"当前场景: {currentSceneName}\n" +
                   $"当前UI: {currentUIPanel}\n" +
                   $"解锁功能: {unlockedFunctions.Count}\n" +
                   $"首次游戏: {isFirstTimePlay}\n" +
                   $"自定义数据: {customData.Count}";
        }
        
        #endregion
    }
}