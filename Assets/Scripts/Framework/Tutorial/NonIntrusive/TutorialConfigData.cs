using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Tutorial.NonIntrusive
{
    /// <summary>
    /// 非侵入式新手引导配置数据
    /// 极简版本，通过配置文件定义引导流程
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialConfig", menuName = "Framework/Tutorial/Tutorial Config")]
    [Serializable]
    public class TutorialConfigData : ScriptableObject
    {
        [Header("引导配置信息")]
        public string configId;
        public string configName;
        public string description;
        public bool enabled = true;
        
        [Header("引导序列")]
        public List<TutorialSequenceConfigData> sequences = new List<TutorialSequenceConfigData>();
    }
    
    /// <summary>
    /// 引导序列配置数据
    /// </summary>
    [Serializable]
    public class TutorialSequenceConfigData
    {
        [Header("序列信息")]
        public string sequenceId;
        public string sequenceName;
        public bool enabled = true;
        
        [Header("引导步骤")]
        public List<TutorialStepConfigData> steps = new List<TutorialStepConfigData>();
        
        [Header("序列设置")]
        public float stepDelay = 0.5f;
    }
    
    /// <summary>
    /// 引导步骤配置数据
    /// </summary>
    [Serializable]
    public class TutorialStepConfigData
    {
        [Header("步骤信息")]
        public string stepId;
        public string stepName;
        public bool enabled = true;
        
        [Header("步骤类型")]
        public TutorialStepType stepType;
        
        [Header("目标路径")]
        public string targetPath;
        
        [Header("消息内容")]
        public string message;
        
        [Header("显示时间")]
        public float displayTime = 2f;
        
        [Header("等待时间")]
        public float waitTime = 1f;
        
        [Header("步骤延迟")]
        public float stepDelay = 0.5f;
    }
    
    /// <summary>
    /// 引导步骤类型
    /// </summary>
    public enum TutorialStepType
    {
        Message,    // 显示消息
        Highlight,  // 高亮元素
        Wait,       // 等待
        Click       // 等待点击
    }
}