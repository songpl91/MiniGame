using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.EnhanceUI.Config;

namespace Framework.EnhanceUI.Core
{
    /// <summary>
    /// UI加载队列管理器
    /// 负责管理UI的加载顺序，避免并发加载导致的问题
    /// </summary>
    public class UILoadQueue : MonoBehaviour
    {
        #region 字段和属性
        
        [Header("队列配置")]
        [SerializeField] private int maxConcurrentLoads = 3;
        [SerializeField] private float loadTimeoutSeconds = 30f;
        [SerializeField] private bool enablePriorityQueue = true;
        
        // 等待队列 - 按优先级排序的加载请求
        private List<UILoadRequest> waitingQueue = new List<UILoadRequest>();
        
        // 正在加载的请求
        private Dictionary<string, UILoadRequest> loadingRequests = new Dictionary<string, UILoadRequest>();
        
        // 已完成的请求缓存
        private Dictionary<string, UILoadRequest> completedRequests = new Dictionary<string, UILoadRequest>();
        
        // 加载器引用
        private IUILoader uiLoader;
        
        /// <summary>
        /// 等待队列中的请求数量
        /// </summary>
        public int WaitingCount => waitingQueue.Count;
        
        /// <summary>
        /// 正在加载的请求数量
        /// </summary>
        public int LoadingCount => loadingRequests.Count;
        
        /// <summary>
        /// 总请求数量
        /// </summary>
        public int TotalRequestCount => waitingQueue.Count + loadingRequests.Count;
        
        /// <summary>
        /// 是否正在处理请求
        /// </summary>
        public bool IsProcessing => TotalRequestCount > 0;
        
        #endregion
        
        #region 事件委托
        
        /// <summary>
        /// 请求添加到队列事件
        /// </summary>
        public event Action<UILoadRequest> OnRequestQueued;
        
        /// <summary>
        /// 请求开始加载事件
        /// </summary>
        public event Action<UILoadRequest> OnRequestStarted;
        
        /// <summary>
        /// 请求完成事件
        /// </summary>
        public event Action<UILoadRequest> OnRequestCompleted;
        
        /// <summary>
        /// 请求失败事件
        /// </summary>
        public event Action<UILoadRequest, string> OnRequestFailed;
        
        /// <summary>
        /// 请求取消事件
        /// </summary>
        public event Action<UILoadRequest> OnRequestCancelled;
        
        /// <summary>
        /// 队列清空事件
        /// </summary>
        public event Action OnQueueEmpty;
        
        #endregion
        
        #region 初始化方法
        
        /// <summary>
        /// 初始化加载队列
        /// </summary>
        /// <param name="loader">UI加载器</param>
        public void Initialize(IUILoader loader)
        {
            uiLoader = loader ?? throw new ArgumentNullException(nameof(loader));
            
            Debug.Log("[UILoadQueue] 加载队列初始化完成");
        }
        
        #endregion
        
        #region 队列管理方法
        
        /// <summary>
        /// 添加加载请求到队列
        /// </summary>
        /// <param name="request">加载请求</param>
        public void EnqueueRequest(UILoadRequest request)
        {
            if (request == null)
            {
                Debug.LogError("[UILoadQueue] 尝试添加空的加载请求");
                return;
            }
            
            // 检查是否已存在相同的请求
            if (IsRequestExists(request.UIName, request.Data))
            {
                Debug.LogWarning($"[UILoadQueue] 相同的加载请求已存在: {request.UIName}");
                request.InvokeFailure("相同的加载请求已存在");
                return;
            }
            
            try
            {
                // 添加到等待队列
                if (enablePriorityQueue)
                {
                    InsertByPriority(request);
                }
                else
                {
                    waitingQueue.Add(request);
                }
                
                // 触发队列事件
                OnRequestQueued?.Invoke(request);
                
                Debug.Log($"[UILoadQueue] 请求添加到队列: {request.UIName} (优先级: {request.Priority})");
                
                // 尝试处理队列
                ProcessQueue();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UILoadQueue] 添加请求到队列失败: {e.Message}");
                request.InvokeFailure($"添加到队列失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 取消加载请求
        /// </summary>
        /// <param name="requestId">请求ID</param>
        /// <returns>是否成功取消</returns>
        public bool CancelRequest(string requestId)
        {
            if (string.IsNullOrEmpty(requestId))
                return false;
            
            // 在等待队列中查找
            for (int i = 0; i < waitingQueue.Count; i++)
            {
                if (waitingQueue[i].RequestId == requestId)
                {
                    UILoadRequest request = waitingQueue[i];
                    waitingQueue.RemoveAt(i);
                    
                    request.InvokeCancel();
                    OnRequestCancelled?.Invoke(request);
                    
                    Debug.Log($"[UILoadQueue] 取消等待中的请求: {request.UIName}");
                    return true;
                }
            }
            
            // 在加载中的请求中查找
            if (loadingRequests.ContainsKey(requestId))
            {
                UILoadRequest request = loadingRequests[requestId];
                if (request.CanBeCancelled())
                {
                    loadingRequests.Remove(requestId);
                    
                    // 停止加载协程
                    StopCoroutine($"LoadUI_{requestId}");
                    
                    request.InvokeCancel();
                    OnRequestCancelled?.Invoke(request);
                    
                    Debug.Log($"[UILoadQueue] 取消加载中的请求: {request.UIName}");
                    
                    // 继续处理队列
                    ProcessQueue();
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 取消指定UI的所有请求
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <returns>取消的请求数量</returns>
        public int CancelRequestsByUIName(string uiName)
        {
            if (string.IsNullOrEmpty(uiName))
                return 0;
            
            int cancelledCount = 0;
            
            // 取消等待队列中的请求
            for (int i = waitingQueue.Count - 1; i >= 0; i--)
            {
                if (waitingQueue[i].UIName == uiName)
                {
                    UILoadRequest request = waitingQueue[i];
                    waitingQueue.RemoveAt(i);
                    
                    request.InvokeCancel();
                    OnRequestCancelled?.Invoke(request);
                    cancelledCount++;
                }
            }
            
            // 取消加载中的请求
            var requestsToCancel = new List<string>();
            foreach (var kvp in loadingRequests)
            {
                if (kvp.Value.UIName == uiName && kvp.Value.CanBeCancelled())
                {
                    requestsToCancel.Add(kvp.Key);
                }
            }
            
            foreach (string requestId in requestsToCancel)
            {
                if (CancelRequest(requestId))
                    cancelledCount++;
            }
            
            Debug.Log($"[UILoadQueue] 取消了 {cancelledCount} 个 {uiName} 的加载请求");
            return cancelledCount;
        }
        
        /// <summary>
        /// 清空所有请求
        /// </summary>
        public void ClearAllRequests()
        {
            // 取消所有等待中的请求
            foreach (var request in waitingQueue)
            {
                request.InvokeCancel();
                OnRequestCancelled?.Invoke(request);
            }
            waitingQueue.Clear();
            
            // 取消所有加载中的请求
            var loadingRequestIds = new List<string>(loadingRequests.Keys);
            foreach (string requestId in loadingRequestIds)
            {
                CancelRequest(requestId);
            }
            
            // 清空完成的请求缓存
            completedRequests.Clear();
            
            Debug.Log("[UILoadQueue] 所有加载请求已清空");
            OnQueueEmpty?.Invoke();
        }
        
        #endregion
        
        #region 队列处理方法
        
        /// <summary>
        /// 处理加载队列
        /// </summary>
        private void ProcessQueue()
        {
            // 检查是否可以开始新的加载
            while (loadingRequests.Count < maxConcurrentLoads && waitingQueue.Count > 0)
            {
                UILoadRequest request = waitingQueue[0];
                waitingQueue.RemoveAt(0);
                
                StartLoadRequest(request);
            }
            
            // 检查队列是否为空
            if (TotalRequestCount == 0)
            {
                OnQueueEmpty?.Invoke();
            }
        }
        
        /// <summary>
        /// 开始加载请求
        /// </summary>
        /// <param name="request">加载请求</param>
        private void StartLoadRequest(UILoadRequest request)
        {
            try
            {
                // 添加到加载中的请求
                loadingRequests[request.RequestId] = request;
                
                // 更新请求状态
                request.UpdateState(UILoadRequestState.Loading);
                
                // 触发开始加载事件
                OnRequestStarted?.Invoke(request);
                
                Debug.Log($"[UILoadQueue] 开始加载请求: {request.UIName}");
                
                // 根据加载模式选择加载方式
                if (request.LoadMode == UILoadMode.Sync)
                {
                    StartCoroutine(LoadUISyncCoroutine(request));
                }
                else
                {
                    StartCoroutine(LoadUIAsyncCoroutine(request));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[UILoadQueue] 开始加载请求失败: {request.UIName}, 错误: {e.Message}");
                CompleteRequest(request, false, e.Message);
            }
        }
        
        /// <summary>
        /// 同步加载UI协程
        /// </summary>
        private IEnumerator LoadUISyncCoroutine(UILoadRequest request)
        {
            float startTime = Time.time;
            
            try
            {
                // 同步加载UI
                EnhanceUIPanel panel = uiLoader.LoadUISync(request.UIName, request.Config);
                
                if (panel != null)
                {
                    // 初始化面板
                    panel.SetConfig(request.Config);
                    panel.Initialize(request.Data);
                    
                    CompleteRequest(request, true, null, panel);
                }
                else
                {
                    CompleteRequest(request, false, "同步加载返回空面板");
                }
            }
            catch (Exception e)
            {
                CompleteRequest(request, false, $"同步加载异常: {e.Message}");
            }
            
            yield return null;
        }
        
        /// <summary>
        /// 异步加载UI协程
        /// </summary>
        private IEnumerator LoadUIAsyncCoroutine(UILoadRequest request)
        {
            float startTime = Time.time;
            bool isCompleted = false;
            bool isSuccess = false;
            string errorMessage = null;
            EnhanceUIPanel resultPanel = null;
            
            try
            {
                // 异步加载UI
                uiLoader.LoadUIAsync(request.UIName, request.Config, 
                    (panel) => {
                        // 成功回调
                        resultPanel = panel;
                        isSuccess = true;
                        isCompleted = true;
                        
                        if (panel != null)
                        {
                            panel.SetConfig(request.Config);
                            panel.Initialize(request.Data);
                        }
                    },
                    (error) => {
                        // 失败回调
                        errorMessage = error;
                        isSuccess = false;
                        isCompleted = true;
                    },
                    (progress) => {
                        // 进度回调
                        request.UpdateProgress(progress);
                    });
                
                // 等待加载完成或超时
                while (!isCompleted)
                {
                    // 检查超时
                    if (Time.time - startTime > loadTimeoutSeconds)
                    {
                        errorMessage = $"异步加载超时 ({loadTimeoutSeconds}秒)";
                        isCompleted = true;
                        isSuccess = false;
                        break;
                    }
                    
                    // 检查请求是否被取消
                    if (!loadingRequests.ContainsKey(request.RequestId))
                    {
                        yield break; // 请求已被取消
                    }
                    
                   
                }
                
                // 完成请求
                if (isSuccess)
                {
                    CompleteRequest(request, true, null, resultPanel);
                }
                else
                {
                    CompleteRequest(request, false, errorMessage ?? "异步加载失败");
                }
            }
            catch (Exception e)
            {
                CompleteRequest(request, false, $"异步加载异常: {e.Message}");
            }
        }
        
        /// <summary>
        /// 完成加载请求
        /// </summary>
        /// <param name="request">加载请求</param>
        /// <param name="success">是否成功</param>
        /// <param name="errorMessage">错误信息</param>
        /// <param name="panel">加载的面板</param>
        private void CompleteRequest(UILoadRequest request, bool success, string errorMessage, EnhanceUIPanel panel = null)
        {
            try
            {
                // 从加载中的请求移除
                loadingRequests.Remove(request.RequestId);
                
                // 添加到完成的请求缓存
                completedRequests[request.RequestId] = request;
                
                if (success)
                {
                    // 成功完成
                    request.InvokeSuccess(panel);
                    OnRequestCompleted?.Invoke(request);
                    
                    Debug.Log($"[UILoadQueue] 请求加载成功: {request.UIName}");
                }
                else
                {
                    // 失败完成
                    request.InvokeFailure(errorMessage);
                    OnRequestFailed?.Invoke(request, errorMessage);
                    
                    Debug.LogError($"[UILoadQueue] 请求加载失败: {request.UIName}, 错误: {errorMessage}");
                }
                
                // 继续处理队列
                ProcessQueue();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UILoadQueue] 完成请求处理失败: {e.Message}");
            }
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 按优先级插入请求
        /// </summary>
        /// <param name="request">加载请求</param>
        private void InsertByPriority(UILoadRequest request)
        {
            int insertIndex = 0;
            
            // 找到合适的插入位置（优先级高的在前面）
            for (int i = 0; i < waitingQueue.Count; i++)
            {
                if (request.Priority > waitingQueue[i].Priority)
                {
                    insertIndex = i;
                    break;
                }
                insertIndex = i + 1;
            }
            
            waitingQueue.Insert(insertIndex, request);
        }
        
        /// <summary>
        /// 检查请求是否已存在
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="data">数据</param>
        /// <returns>是否存在</returns>
        private bool IsRequestExists(string uiName, object data)
        {
            // 检查等待队列
            foreach (var request in waitingQueue)
            {
                if (request.UIName == uiName && ReferenceEquals(request.Data, data))
                    return true;
            }
            
            // 检查加载中的请求
            foreach (var request in loadingRequests.Values)
            {
                if (request.UIName == uiName && ReferenceEquals(request.Data, data))
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取队列状态信息
        /// </summary>
        /// <returns>队列状态</returns>
        public QueueStatus GetQueueStatus()
        {
            return new QueueStatus
            {
                WaitingCount = waitingQueue.Count,
                LoadingCount = loadingRequests.Count,
                CompletedCount = completedRequests.Count,
                TotalCount = TotalRequestCount,
                IsProcessing = IsProcessing
            };
        }
        
        /// <summary>
        /// 获取等待队列的请求列表
        /// </summary>
        /// <returns>等待中的请求列表</returns>
        public List<UILoadRequest> GetWaitingRequests()
        {
            return new List<UILoadRequest>(waitingQueue);
        }
        
        /// <summary>
        /// 获取加载中的请求列表
        /// </summary>
        /// <returns>加载中的请求列表</returns>
        public List<UILoadRequest> GetLoadingRequests()
        {
            return new List<UILoadRequest>(loadingRequests.Values);
        }
        
        #endregion
        
        #region Unity生命周期
        
        private void Update()
        {
            // 清理过期的完成请求缓存
            CleanupCompletedRequests();
        }
        
        /// <summary>
        /// 清理过期的完成请求缓存
        /// </summary>
        private void CleanupCompletedRequests()
        {
            if (completedRequests.Count > 100) // 限制缓存大小
            {
                var requestsToRemove = new List<string>();
                foreach (var kvp in completedRequests)
                {
                    if (kvp.Value.GetWaitTime() > 300) // 5分钟后清理
                    {
                        requestsToRemove.Add(kvp.Key);
                    }
                }
                
                foreach (string requestId in requestsToRemove)
                {
                    completedRequests.Remove(requestId);
                }
            }
        }
        
        private void OnDestroy()
        {
            // 清理所有请求
            ClearAllRequests();
            
            Debug.Log("[UILoadQueue] 加载队列已销毁");
        }
        
        #endregion
    }
    
    /// <summary>
    /// 队列状态结构
    /// </summary>
    [Serializable]
    public struct QueueStatus
    {
        /// <summary>
        /// 等待中的请求数量
        /// </summary>
        public int WaitingCount;
        
        /// <summary>
        /// 加载中的请求数量
        /// </summary>
        public int LoadingCount;
        
        /// <summary>
        /// 已完成的请求数量
        /// </summary>
        public int CompletedCount;
        
        /// <summary>
        /// 总请求数量
        /// </summary>
        public int TotalCount;
        
        /// <summary>
        /// 是否正在处理
        /// </summary>
        public bool IsProcessing;
    }
    
    /// <summary>
    /// UI加载器接口
    /// </summary>
    public interface IUILoader
    {
        /// <summary>
        /// 同步加载UI
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="config">UI配置</param>
        /// <returns>UI面板</returns>
        EnhanceUIPanel LoadUISync(string uiName, UIConfigData config);
        
        /// <summary>
        /// 异步加载UI
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="config">UI配置</param>
        /// <param name="onSuccess">成功回调</param>
        /// <param name="onFailure">失败回调</param>
        /// <param name="onProgress">进度回调</param>
        void LoadUIAsync(string uiName, UIConfigData config, 
                        Action<EnhanceUIPanel> onSuccess, 
                        Action<string> onFailure, 
                        Action<float> onProgress);
    }
}