using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager
{
    private static TimerManager _instance;

    private Dictionary<Action, TimerListener> _times;

    private Dictionary<Action, TimerListener> _pendingDict;
    private List<TimerListener> _removeList;
    private Queue<TimerListener> _timerListenerPool;

    // 添加暂停控制变量
    private bool _isPaused = false;

    private const int MaxTimersNum = 100;

    public TimerManager()
    {
        _times = new Dictionary<Action, TimerListener>();
        _pendingDict = new Dictionary<Action, TimerListener>();
        _removeList = new List<TimerListener>();
        _timerListenerPool = new Queue<TimerListener>();
    }

    public static TimerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new TimerManager();
            }

            return _instance;
        }
    }

    public static void Clear()
    {
        _instance._times.Clear();
        _instance._pendingDict.Clear();
        _instance._removeList.Clear();
        _instance._timerListenerPool.Clear();

        _instance = null;
    }

    TimerListener GetTimerListener()
    {
        TimerListener tl = null;

        if (_timerListenerPool.Count > 0)
        {
            tl = _timerListenerPool.Dequeue();
            tl.Clear();
        }
        else
        {
            tl = new TimerListener();
        }

        return tl;
    }

    // repeat = 0 means repeat unlimited _times
    public Action Add(float interval, int repeat, Action fun)
    {
        var tl = GetTimerListener();

        if (tl != null)
        {
            tl.Interval = interval;
            tl.Repeat = repeat;
            tl.OnTime = fun;
            tl.IsDelete = false;
            _pendingDict[fun] = tl;
        }

        return fun;
    }


    public void Remove(Action fun)
    {
        if (fun == null)
        {
            return;
        }

        if (_pendingDict.ContainsKey(fun))
        {
            _pendingDict.Remove(fun);
        }

        if (_times.ContainsKey(fun))
        {
            _times[fun].IsDelete = true;
        }
    }

    /// <summary>
    /// 暂停所有计时器
    /// </summary>
    public void Pause()
    {
        _isPaused = true;
    }

    /// <summary>
    /// 恢复所有计时器
    /// </summary>
    public void Resume()
    {
        _isPaused = false;
    }

    /// <summary>
    /// 获取当前暂停状态
    /// </summary>
    public bool IsPaused()
    {
        return _isPaused;
    }

    /// <summary>
    /// 暂停单个计时器
    /// </summary>
    /// <param name="fun">计时器的回调函数</param>
    /// <returns>是否成功暂停该计时器</returns>
    public bool PauseTimer(Action fun)
    {
        if (fun == null)
        {
            return false;
        }

        if (_pendingDict.ContainsKey(fun))
        {
            _pendingDict[fun].IsPaused = true;
            return true;
        }

        if (_times.ContainsKey(fun))
        {
            _times[fun].IsPaused = true;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 恢复单个计时器
    /// </summary>
    /// <param name="fun">计时器的回调函数</param>
    /// <returns>是否成功恢复该计时器</returns>
    public bool ResumeTimer(Action fun)
    {
        if (fun == null)
        {
            return false;
        }

        if (_pendingDict.ContainsKey(fun))
        {
            _pendingDict[fun].IsPaused = false;
            return true;
        }

        if (_times.ContainsKey(fun))
        {
            _times[fun].IsPaused = false;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 获取单个计时器的暂停状态
    /// </summary>
    /// <param name="fun">计时器的回调函数</param>
    /// <returns>该计时器是否暂停</returns>
    public bool IsTimerPaused(Action fun)
    {
        if (fun == null)
        {
            return false;
        }

        if (_pendingDict.ContainsKey(fun))
        {
            return _pendingDict[fun].IsPaused;
        }

        if (_times.ContainsKey(fun))
        {
            return _times[fun].IsPaused;
        }

        return false;
    }

    public void Update(float unscaled_delta_time)
    {
        // 如果暂停中，则不更新计时器
        if (_isPaused)
        {
            return;
        }

        var start_update_time = Time.realtimeSinceStartup;

        if (unscaled_delta_time > 60)
        {
            Debug.Log($"[TimerManager]Update: Start update timer after a long pause");
        }

        if (_pendingDict.Count > 0)
        {
            var etor = _pendingDict.GetEnumerator();
            while (etor.MoveNext())
            {
                var tl = etor.Current.Value;
                _times[tl.OnTime] = tl;
            }

            _pendingDict.Clear();

            if (_times.Count > MaxTimersNum)
            {
                Debug.LogError(
                    $"[TimerManager]Update: Alert! Too much timers running! Current number = {_times.Count}");
            }
        }

        _removeList.Clear();

        if (_times.Count > 0)
        {
            var etor = _times.GetEnumerator();
            while (etor.MoveNext())
            {
                var tl1 = etor.Current.Value;

                if (tl1.Timer(unscaled_delta_time))
                {
                    _removeList.Add(tl1);
                }
            }
        }

        for (int i = 0; i < _removeList.Count; i++)
        {
            var item = _removeList[i];
            _times.Remove(item.OnTime);
            _timerListenerPool.Enqueue(item);
        }

        var update_time = Time.realtimeSinceStartup - start_update_time;

        if (update_time > 3)
        {
            Debug.LogWarning($"[TimerManager]Update: Long Pause({update_time} secs)!");
        }

        if (unscaled_delta_time > 60)
        {
            Debug.Log($"[TimerManager]Update: End update timer after a long pause");
        }
    }
}

public class TimerListener
{
    private float _elapsed = 0;
    public Action OnTime;
    public float Interval;
    public int Repeat;

    public bool IsDelete = false;

    // 添加单个计时器的暂停控制变量
    public bool IsPaused = false;

    public void Clear()
    {
        _elapsed = 0;
        OnTime = null;
        Interval = 0;
        Repeat = 0;
        IsDelete = false;
        IsPaused = false;
    }

    public bool Timer(float time)
    {
        if (IsDelete)
        {
            return IsDelete;
        }

        // 如果单个计时器被暂停，则不更新
        if (IsPaused)
        {
            return false;
        }

        _elapsed += time;

        if (_elapsed >= Interval)
        {
            this.OnTime();

            var count = (int)(_elapsed / Interval);
            _elapsed -= count * Interval;

            if (Repeat > 0)
            {
                Repeat -= count;

                if (Repeat <= 0)
                {
                    IsDelete = true;
                }
            }
        }

        return IsDelete;
    }

    public void Update()
    {
        // 如果计时器被暂停，则不执行回调
        if (IsPaused)
        {
            return;
        }

        try
        {
            this.OnTime();
        }
        catch (Exception e)
        {
            Debug.LogError($"[TimerListener]Update: Will delete this listener, Exception: {e.Message}");
            IsDelete = true;
        }
    }
}

// TimerManager.Instance.Add(0.2f, 1, () =>
// {
//     gameObject.SafeSetActive(false);
// });
//TimerManager.Instance.ResumeTimer(FrozenAction);
//TimerManager.Instance.PauseTimer(FrozenAction);
//TimerManager.Instance.Remove(_onCountDown);