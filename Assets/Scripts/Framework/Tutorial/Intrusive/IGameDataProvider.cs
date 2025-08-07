using UnityEngine;

namespace Framework.Tutorial.Intrusive
{
    /// <summary>
    /// 游戏数据提供者接口
    /// 为引导系统提供游戏状态和数据访问
    /// </summary>
    public interface IGameDataProvider
    {
        #region 玩家基础数据
        
        /// <summary>
        /// 获取玩家等级
        /// </summary>
        int GetPlayerLevel();
        
        /// <summary>
        /// 获取玩家经验值
        /// </summary>
        int GetPlayerExp();
        
        /// <summary>
        /// 获取玩家金币
        /// </summary>
        int GetPlayerGold();
        
        /// <summary>
        /// 获取玩家钻石
        /// </summary>
        int GetPlayerDiamond();
        
        /// <summary>
        /// 是否是首次进入游戏
        /// </summary>
        bool IsFirstTimePlay();
        
        /// <summary>
        /// 获取游戏总时长（秒）
        /// </summary>
        float GetTotalPlayTime();
        
        #endregion
        
        #region 关卡进度数据
        
        /// <summary>
        /// 获取当前关卡ID
        /// </summary>
        int GetCurrentStageId();
        
        /// <summary>
        /// 获取已通过的最高关卡ID
        /// </summary>
        int GetMaxPassedStageId();
        
        /// <summary>
        /// 检查指定关卡是否已通过
        /// </summary>
        bool IsStageCompleted(int stageId);
        
        /// <summary>
        /// 获取关卡星级评价
        /// </summary>
        int GetStageStars(int stageId);
        
        #endregion
        
        #region 功能解锁状态
        
        /// <summary>
        /// 检查功能是否已解锁
        /// </summary>
        bool IsFunctionUnlocked(string functionName);
        
        /// <summary>
        /// 检查建筑是否已解锁
        /// </summary>
        bool IsBuildingUnlocked(string buildingName);
        
        /// <summary>
        /// 检查装备是否已解锁
        /// </summary>
        bool IsEquipmentUnlocked(string equipmentId);
        
        /// <summary>
        /// 检查技能是否已解锁
        /// </summary>
        bool IsSkillUnlocked(string skillId);
        
        #endregion
        
        #region 界面状态
        
        /// <summary>
        /// 获取当前场景名称
        /// </summary>
        string GetCurrentSceneName();
        
        /// <summary>
        /// 获取当前打开的UI界面
        /// </summary>
        string GetCurrentUIPanel();
        
        /// <summary>
        /// 检查指定UI是否已打开过
        /// </summary>
        bool HasOpenedUI(string uiName);
        
        /// <summary>
        /// 检查是否在战斗中
        /// </summary>
        bool IsInBattle();
        
        /// <summary>
        /// 检查是否在主城
        /// </summary>
        bool IsInMainCity();
        
        #endregion
        
        #region 物品和装备
        
        /// <summary>
        /// 获取背包中指定物品数量
        /// </summary>
        int GetItemCount(string itemId);
        
        /// <summary>
        /// 检查是否拥有指定装备
        /// </summary>
        bool HasEquipment(string equipmentId);
        
        /// <summary>
        /// 获取已装备的装备数量
        /// </summary>
        int GetEquippedItemCount();
        
        /// <summary>
        /// 检查背包是否已满
        /// </summary>
        bool IsInventoryFull();
        
        #endregion
        
        #region 成就和任务
        
        /// <summary>
        /// 检查成就是否已完成
        /// </summary>
        bool IsAchievementCompleted(string achievementId);
        
        /// <summary>
        /// 检查任务是否已完成
        /// </summary>
        bool IsQuestCompleted(string questId);
        
        /// <summary>
        /// 获取当前活跃任务数量
        /// </summary>
        int GetActiveQuestCount();
        
        #endregion
        
        #region 时间相关
        
        /// <summary>
        /// 获取今日登录次数
        /// </summary>
        int GetTodayLoginCount();
        
        /// <summary>
        /// 获取连续登录天数
        /// </summary>
        int GetConsecutiveLoginDays();
        
        /// <summary>
        /// 检查是否是特定时间段
        /// </summary>
        bool IsInTimeRange(int startHour, int endHour);
        
        /// <summary>
        /// 获取距离上次操作的时间（秒）
        /// </summary>
        float GetTimeSinceLastAction(string actionName);
        
        #endregion
        
        #region 社交和公会
        
        /// <summary>
        /// 检查是否已加入公会
        /// </summary>
        bool IsInGuild();
        
        /// <summary>
        /// 获取好友数量
        /// </summary>
        int GetFriendCount();
        
        /// <summary>
        /// 检查是否有未读消息
        /// </summary>
        bool HasUnreadMessages();
        
        #endregion
        
        #region 自定义数据
        
        /// <summary>
        /// 获取自定义整数数据
        /// </summary>
        int GetCustomInt(string key, int defaultValue = 0);
        
        /// <summary>
        /// 获取自定义浮点数据
        /// </summary>
        float GetCustomFloat(string key, float defaultValue = 0f);
        
        /// <summary>
        /// 获取自定义字符串数据
        /// </summary>
        string GetCustomString(string key, string defaultValue = "");
        
        /// <summary>
        /// 获取自定义布尔数据
        /// </summary>
        bool GetCustomBool(string key, bool defaultValue = false);
        
        /// <summary>
        /// 设置自定义数据（用于引导过程中的状态记录）
        /// </summary>
        void SetCustomData(string key, object value);
        
        #endregion
    }
}