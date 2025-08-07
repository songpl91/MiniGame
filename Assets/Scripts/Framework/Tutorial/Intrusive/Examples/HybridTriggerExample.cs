using UnityEngine;

namespace Framework.Tutorial.Intrusive.Examples
{
    /// <summary>
    /// 混合触发机制使用示例
    /// 展示如何在实际游戏系统中集成事件驱动和定时检查的混合触发方案
    /// </summary>
    public class HybridTriggerExample : MonoBehaviour
    {
        [Header("示例设置")]
        [SerializeField] private bool showDebugLogs = true;
        
        // 引导触发系统引用
        private TutorialTriggerSystem triggerSystem;
        
        void Start()
        {
            // 获取引导触发系统实例
            triggerSystem = TutorialTriggerSystem.Instance;
            
            // 演示混合触发机制的配置
            DemonstrateHybridTriggerConfiguration();
        }
        
        /// <summary>
        /// 演示混合触发机制的配置
        /// </summary>
        private void DemonstrateHybridTriggerConfiguration()
        {
            LogDebug("=== 混合触发机制配置示例 ===");
            
            // 1. 启用事件驱动触发（推荐用于关键游戏事件）
            triggerSystem.SetEventDrivenTrigger(true);
            LogDebug("✓ 事件驱动触发已启用");
            
            // 2. 启用定时检查触发（作为兜底方案）
            triggerSystem.SetTimerBasedTrigger(true);
            LogDebug("✓ 定时检查触发已启用");
            
            // 3. 设置定时检查间隔（建议3-5秒，避免过于频繁）
            triggerSystem.SetTimerCheckInterval(3f);
            LogDebug("✓ 定时检查间隔设置为3秒");
            
            // 4. 设置事件触发延迟（避免数据更新延迟问题）
            triggerSystem.SetEventTriggerDelay(0.1f);
            LogDebug("✓ 事件触发延迟设置为0.1秒");
            
            // 5. 查看当前配置状态
            LogDebug($"当前配置: {triggerSystem.GetTriggerMechanismStatus()}");
        }
        
        #region 游戏系统集成示例
        
        /// <summary>
        /// 玩家系统集成示例
        /// 在玩家升级时触发引导检查
        /// </summary>
        public void OnPlayerLevelUpExample()
        {
            LogDebug("玩家升级事件发生");
            
            // 更新游戏数据
            // playerData.Level++;
            
            // 触发事件驱动检查（立即响应）
            triggerSystem.OnPlayerLevelUp();
            
            LogDebug("已触发等级相关引导检查");
        }
        
        /// <summary>
        /// 关卡系统集成示例
        /// 在完成关卡时触发引导检查
        /// </summary>
        public void OnStageCompletedExample(int stageId)
        {
            LogDebug($"关卡 {stageId} 完成事件发生");
            
            // 更新游戏数据
            // stageData.CompletedStages.Add(stageId);
            
            // 触发事件驱动检查（精确控制）
            triggerSystem.OnStageCompleted(stageId);
            
            LogDebug($"已触发关卡 {stageId} 相关引导检查");
        }
        
        /// <summary>
        /// 功能解锁系统集成示例
        /// 在解锁新功能时触发引导检查
        /// </summary>
        public void OnFunctionUnlockedExample(string functionName)
        {
            LogDebug($"功能 {functionName} 解锁事件发生");
            
            // 更新游戏数据
            // gameData.UnlockedFunctions.Add(functionName);
            
            // 触发事件驱动检查（即时反馈）
            triggerSystem.OnFunctionUnlocked(functionName);
            
            LogDebug($"已触发功能 {functionName} 相关引导检查");
        }
        
        /// <summary>
        /// UI系统集成示例
        /// 在首次进入某个界面时触发引导检查
        /// </summary>
        public void OnFirstTimeEnterUIExample(string uiName)
        {
            LogDebug($"首次进入 {uiName} 界面事件发生");
            
            // 记录首次进入状态
            // PlayerPrefs.SetInt($"FirstTime_{uiName}", 1);
            
            // 触发事件驱动检查（用户体验优化）
            triggerSystem.OnFirstTimeEnter(uiName);
            
            LogDebug($"已触发 {uiName} 界面相关引导检查");
        }
        
        #endregion
        
        #region 运行时控制示例
        
        /// <summary>
        /// 运行时切换到纯事件驱动模式
        /// 适用于性能敏感的场景
        /// </summary>
        [ContextMenu("切换到纯事件驱动模式")]
        public void SwitchToEventDrivenOnly()
        {
            triggerSystem.SetEventDrivenTrigger(true);
            triggerSystem.SetTimerBasedTrigger(false);
            LogDebug("已切换到纯事件驱动模式（高性能）");
        }
        
        /// <summary>
        /// 运行时切换到纯定时检查模式
        /// 适用于简单的游戏或测试场景
        /// </summary>
        [ContextMenu("切换到纯定时检查模式")]
        public void SwitchToTimerBasedOnly()
        {
            triggerSystem.SetEventDrivenTrigger(false);
            triggerSystem.SetTimerBasedTrigger(true);
            LogDebug("已切换到纯定时检查模式（简单可靠）");
        }
        
        /// <summary>
        /// 运行时启用混合模式
        /// 推荐的默认配置
        /// </summary>
        [ContextMenu("启用混合模式")]
        public void EnableHybridMode()
        {
            triggerSystem.SetEventDrivenTrigger(true);
            triggerSystem.SetTimerBasedTrigger(true);
            triggerSystem.SetTimerCheckInterval(3f);
            LogDebug("已启用混合模式（推荐配置）");
        }
        
        /// <summary>
        /// 临时禁用所有触发检查
        /// 适用于特殊场景（如过场动画、战斗等）
        /// </summary>
        [ContextMenu("临时禁用所有触发")]
        public void DisableAllTriggers()
        {
            triggerSystem.SetEventDrivenTrigger(false);
            triggerSystem.SetTimerBasedTrigger(false);
            LogDebug("已临时禁用所有触发检查");
        }
        
        #endregion
        
        #region 性能优化建议示例
        
        /// <summary>
        /// 战斗场景优化示例
        /// 在战斗中禁用定时检查，只保留关键事件触发
        /// </summary>
        public void OnEnterBattleScene()
        {
            LogDebug("进入战斗场景 - 优化触发机制");
            
            // 禁用定时检查以提升战斗性能
            triggerSystem.SetTimerBasedTrigger(false);
            
            // 保留事件驱动，但增加延迟避免战斗中断
            triggerSystem.SetEventDrivenTrigger(true);
            triggerSystem.SetEventTriggerDelay(0.5f);
            
            LogDebug("战斗场景触发优化完成");
        }
        
        /// <summary>
        /// 主城场景优化示例
        /// 在主城中启用完整的混合触发机制
        /// </summary>
        public void OnEnterMainCity()
        {
            LogDebug("进入主城场景 - 启用完整触发机制");
            
            // 启用完整的混合触发机制
            triggerSystem.SetEventDrivenTrigger(true);
            triggerSystem.SetTimerBasedTrigger(true);
            triggerSystem.SetTimerCheckInterval(2f);
            triggerSystem.SetEventTriggerDelay(0.1f);
            
            LogDebug("主城场景触发机制配置完成");
        }
        
        #endregion
        
        /// <summary>
        /// 调试日志输出
        /// </summary>
        private void LogDebug(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[HybridTriggerExample] {message}");
            }
        }
    }
}