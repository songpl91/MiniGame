using System;
using UnityEngine;
using Framework.EnhanceUI.Config;

namespace Framework.EnhanceUI.Core
{
    /// <summary>
    /// UI加载请求
    /// 封装UI加载的所有参数和回调
    /// </summary>
    public class UILoadRequest
    {
        /// <summary>
        /// 请求唯一ID
        /// </summary>
        public string RequestId { get; private set; }
        
        /// <summary>
        /// UI名称
        /// </summary>
        public string UIName { get; private set; }
        
        /// <summary>
        /// 加载模式
        /// </summary>
        public UILoadMode LoadMode { get; private set; }
        
        /// <summary>
        /// 传递给UI的数据
        /// </summary>
        public object Data { get; private set; }
        
        /// <summary>
        /// UI配置数据
        /// </summary>
        public UIConfigData Config { get; private set; }
        
        /// <summary>
        /// 请求创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }
        
        /// <summary>
        /// 请求优先级（数值越大优先级越高）
        /// </summary>
        public int Priority { get; private set; }
        
        /// <summary>
        /// 是否可以被取消
        /// </summary>
        public bool CanCancel { get; private set; }
        
        /// <summary>
        /// 请求状态
        /// </summary>
        public UILoadRequestState State { get; private set; }
        
        /// <summary>
        /// 加载进度（0-1）
        /// </summary>
        public float Progress { get; private set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; private set; }
        
        // 回调委托
        /// <summary>
        /// 加载成功回调
        /// </summary>
        public Action<EnhanceUIPanel> OnSuccess { get; private set; }
        
        /// <summary>
        /// 加载失败回调
        /// </summary>
        public Action<string> OnFailure { get; private set; }
        
        /// <summary>
        /// 加载进度回调
        /// </summary>
        public Action<float> OnProgress { get; private set; }
        
        /// <summary>
        /// 加载取消回调
        /// </summary>
        public Action OnCancel { get; private set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="loadMode">加载模式</param>
        /// <param name="data">传递的数据</param>
        /// <param name="config">UI配置</param>
        /// <param name="priority">优先级</param>
        /// <param name="canCancel">是否可取消</param>
        public UILoadRequest(string uiName, UILoadMode loadMode, object data, UIConfigData config, 
                           int priority = 0, bool canCancel = true)
        {
            RequestId = Guid.NewGuid().ToString();
            UIName = uiName;
            LoadMode = loadMode;
            Data = data;
            Config = config;
            Priority = priority;
            CanCancel = canCancel;
            CreateTime = DateTime.Now;
            State = UILoadRequestState.Pending;
            Progress = 0f;
        }
        
        /// <summary>
        /// 设置回调函数
        /// </summary>
        /// <param name="onSuccess">成功回调</param>
        /// <param name="onFailure">失败回调</param>
        /// <param name="onProgress">进度回调</param>
        /// <param name="onCancel">取消回调</param>
        /// <returns>返回自身以支持链式调用</returns>
        public UILoadRequest SetCallbacks(Action<EnhanceUIPanel> onSuccess = null, 
                                        Action<string> onFailure = null,
                                        Action<float> onProgress = null, 
                                        Action onCancel = null)
        {
            OnSuccess = onSuccess;
            OnFailure = onFailure;
            OnProgress = onProgress;
            OnCancel = onCancel;
            return this;
        }
        
        /// <summary>
        /// 更新请求状态
        /// </summary>
        /// <param name="state">新状态</param>
        internal void UpdateState(UILoadRequestState state)
        {
            State = state;
        }
        
        /// <summary>
        /// 更新加载进度
        /// </summary>
        /// <param name="progress">进度值（0-1）</param>
        internal void UpdateProgress(float progress)
        {
            Progress = Mathf.Clamp01(progress);
            OnProgress?.Invoke(Progress);
        }
        
        /// <summary>
        /// 设置错误信息
        /// </summary>
        /// <param name="errorMessage">错误信息</param>
        internal void SetError(string errorMessage)
        {
            ErrorMessage = errorMessage;
            State = UILoadRequestState.Failed;
        }
        
        /// <summary>
        /// 调用成功回调
        /// </summary>
        /// <param name="panel">加载成功的UI面板</param>
        internal void InvokeSuccess(EnhanceUIPanel panel)
        {
            State = UILoadRequestState.Completed;
            OnSuccess?.Invoke(panel);
        }
        
        /// <summary>
        /// 调用失败回调
        /// </summary>
        /// <param name="errorMessage">错误信息</param>
        internal void InvokeFailure(string errorMessage)
        {
            SetError(errorMessage);
            OnFailure?.Invoke(errorMessage);
        }
        
        /// <summary>
        /// 调用取消回调
        /// </summary>
        internal void InvokeCancel()
        {
            State = UILoadRequestState.Cancelled;
            OnCancel?.Invoke();
        }
        
        /// <summary>
        /// 判断请求是否已完成（成功、失败或取消）
        /// </summary>
        /// <returns>是否已完成</returns>
        public bool IsCompleted()
        {
            return State == UILoadRequestState.Completed || 
                   State == UILoadRequestState.Failed || 
                   State == UILoadRequestState.Cancelled;
        }
        
        /// <summary>
        /// 判断请求是否可以被取消
        /// </summary>
        /// <returns>是否可以取消</returns>
        public bool CanBeCancelled()
        {
            return CanCancel && !IsCompleted();
        }
        
        /// <summary>
        /// 获取请求的等待时间
        /// </summary>
        /// <returns>等待时间（秒）</returns>
        public double GetWaitTime()
        {
            return (DateTime.Now - CreateTime).TotalSeconds;
        }
        
        /// <summary>
        /// 重写ToString方法，便于调试
        /// </summary>
        /// <returns>请求的字符串表示</returns>
        public override string ToString()
        {
            return $"UILoadRequest[{RequestId}]: {UIName}, Mode={LoadMode}, State={State}, Priority={Priority}";
        }
    }
    
    /// <summary>
    /// UI加载请求状态枚举
    /// </summary>
    public enum UILoadRequestState
    {
        /// <summary>
        /// 等待处理
        /// </summary>
        Pending,
        
        /// <summary>
        /// 正在加载
        /// </summary>
        Loading,
        
        /// <summary>
        /// 加载完成
        /// </summary>
        Completed,
        
        /// <summary>
        /// 加载失败
        /// </summary>
        Failed,
        
        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled
    }
    
    /// <summary>
    /// UI加载选项
    /// 提供更灵活的UI加载配置
    /// </summary>
    public class UILoadOptions
    {
        /// <summary>
        /// 加载模式
        /// </summary>
        public UILoadMode LoadMode { get; set; } = UILoadMode.Sync;
        
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; } = 0;
        
        /// <summary>
        /// 是否可以取消
        /// </summary>
        public bool CanCancel { get; set; } = true;
        
        /// <summary>
        /// 是否强制重新加载
        /// </summary>
        public bool ForceReload { get; set; } = false;
        
        /// <summary>
        /// 是否跳过动画
        /// </summary>
        public bool SkipAnimation { get; set; } = false;
        
        /// <summary>
        /// 自定义层级（如果设置，会覆盖配置中的层级）
        /// </summary>
        public UILayerType? CustomLayer { get; set; } = null;
        
        /// <summary>
        /// 超时时间（秒，0表示使用默认值）
        /// </summary>
        public float Timeout { get; set; } = 0f;
        
        /// <summary>
        /// 创建默认选项
        /// </summary>
        /// <returns>默认加载选项</returns>
        public static UILoadOptions Default()
        {
            return new UILoadOptions();
        }
        
        /// <summary>
        /// 创建异步加载选项
        /// </summary>
        /// <param name="priority">优先级</param>
        /// <returns>异步加载选项</returns>
        public static UILoadOptions Async(int priority = 0)
        {
            return new UILoadOptions
            {
                LoadMode = UILoadMode.Async,
                Priority = priority
            };
        }
        
        /// <summary>
        /// 创建同步加载选项
        /// </summary>
        /// <returns>同步加载选项</returns>
        public static UILoadOptions Sync()
        {
            return new UILoadOptions
            {
                LoadMode = UILoadMode.Sync
            };
        }
    }
}