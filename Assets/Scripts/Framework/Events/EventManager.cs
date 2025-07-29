using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    private static Dictionary<string, Delegate> m_EventTable = new Dictionary<string, Delegate>();

        // 添加时进行判断的所有方法
        private static bool OnListenerAdding(string eventType, Delegate e)
        {
            if (!m_EventTable.ContainsKey(eventType))
            {
                m_EventTable.Add(eventType, null);
            }

            Delegate d = m_EventTable[eventType];
            if (d != null && d.GetType() != e.GetType())
            {
                Debug.LogError(string.Format($"{eventType} 所要添加的类型不一致,无法进行添加!"));
                return false;
            }

            return true;
        }

        // 移除事件时进行判断的所有方法
        private static bool OnListenerRemoving(string eventType, Delegate e)
        {
            if (m_EventTable.ContainsKey(eventType))
            {
                Delegate d = m_EventTable[eventType];
                if (d == null)
                {
                    Debug.LogWarning($"移除的事件类型为空 {eventType}");
                    return false;
                }
                else if (d.GetType() != e.GetType())
                {
                    Debug.LogError($"{eventType} 移除的两个事件类型不一致,无法移除");
                    return false;
                }
            }
            else
            {
                // Debug.LogWarning($"移除时不存在当前的类型 {eventType}");
                return false;
            }

            return true;
        }

        // 事件是空的需要进行移除操作
        private static void OnListenerRemoved(string eventType)
        {
            if (m_EventTable[eventType] == null)
            {
                m_EventTable.Remove(eventType);
            }
        }

        // 添加事件
        public static void AddListener(string eventType, Action act)
        {
            if (!OnListenerAdding(eventType, act))
            {
                return;
            }

            m_EventTable[eventType] = (Action)m_EventTable[eventType] + act;
        }

        public static void AddListener<T>(string eventType, Action<T> act)
        {
            if (!OnListenerAdding(eventType, act))
            {
                return;
            }

            m_EventTable[eventType] = (Action<T>)m_EventTable[eventType] + act;
        }

        public static void AddListener<T, X>(string eventType, Action<T, X> act)
        {
            if (!OnListenerAdding(eventType, act))
            {
                return;
            }

            m_EventTable[eventType] = (Action<T, X>)m_EventTable[eventType] + act;
        }

        public static void AddListener<T, X, Y>(string eventType, Action<T, X, Y> act)
        {
            if (!OnListenerAdding(eventType, act))
            {
                return;
            }

            m_EventTable[eventType] = (Action<T, X, Y>)m_EventTable[eventType] + act;
        }

        public static void AddListener<T, X, Y, Z>(string eventType, Action<T, X, Y, Z> act)
        {
            if (!OnListenerAdding(eventType, act))
            {
                return;
            }

            m_EventTable[eventType] = (Action<T, X, Y, Z>)m_EventTable[eventType] + act;
        }

        public static void AddListener<T, X, Y, Z, W>(string eventType, Action<T, X, Y, Z, W> act)
        {
            if (!OnListenerAdding(eventType, act))
            {
                return;
            }

            m_EventTable[eventType] = (Action<T, X, Y, Z, W>)m_EventTable[eventType] + act;
        }

        // 移除事件
        public static void RemoveListener(string eventType, Action act)
        {
            if (!OnListenerRemoving(eventType, act))
            {
                return;
            }

            m_EventTable[eventType] = (Action)m_EventTable[eventType] - act;
            OnListenerRemoved(eventType);
        }

        public static void RemoveListener<T>(string eventType, Action<T> act)
        {
            if (!OnListenerRemoving(eventType, act))
            {
                return;
            }

            m_EventTable[eventType] = (Action<T>)m_EventTable[eventType] - act;
            OnListenerRemoved(eventType);
        }

        public static void RemoveListener<T, X>(string eventType, Action<T, X> act)
        {
            if (!OnListenerRemoving(eventType, act))
            {
                return;
            }

            m_EventTable[eventType] = (Action<T, X>)m_EventTable[eventType] - act;
            OnListenerRemoved(eventType);
        }

        public static void RemoveListener<T, X, Y>(string eventType, Action<T, X, Y> act)
        {
            if (!OnListenerRemoving(eventType, act))
            {
                return;
            }

            m_EventTable[eventType] = (Action<T, X, Y>)m_EventTable[eventType] - act;
            OnListenerRemoved(eventType);
        }

        public static void RemoveListener<T, X, Y, Z>(string eventType, Action<T, X, Y, Z> act)
        {
            if (!OnListenerRemoving(eventType, act))
            {
                return;
            }

            m_EventTable[eventType] = (Action<T, X, Y, Z>)m_EventTable[eventType] - act;
            OnListenerRemoved(eventType);
        }

        public static void RemoveListener<T, X, Y, Z, W>(string eventType, Action<T, X, Y, Z, W> act)
        {
            if (!OnListenerRemoving(eventType, act))
            {
                return;
            }

            m_EventTable[eventType] = (Action<T, X, Y, Z, W>)m_EventTable[eventType] - act;
            OnListenerRemoved(eventType);
        }

        public static void RemoveAll()
        {
            foreach (var item in m_EventTable)
            {
                Delegate del = item.Value;
                del = null;
            }

            m_EventTable.Clear();
        }

        // 广播
        public static void Trigger(string eventType)
        {
            if (m_EventTable.TryGetValue(eventType, out Delegate d))
            {
                Action act = d as Action;
                act?.Invoke();
            }
            else
            {
                // Debug.LogWarning($"广播事件为空 {eventType}");
            }
        }

        public static void Trigger<T>(string eventType, T arg1)
        {
            if (m_EventTable.TryGetValue(eventType, out Delegate d))
            {
                Action<T> act = d as Action<T>;
                act?.Invoke(arg1);
            }
            else
            {
                // Debug.LogWarning($"广播事件为空 {eventType}");
            }
        }

        public static void Trigger<T, X>(string eventType, T arg1, X arg2)
        {
            if (m_EventTable.TryGetValue(eventType, out Delegate d))
            {
                Action<T, X> act = d as Action<T, X>;
                act?.Invoke(arg1, arg2);
            }
            else
            {
                // Debug.LogWarning($"广播事件为空 {eventType}");
            }
        }

        public static void Trigger<T, X, Y>(string eventType, T arg1, X arg2, Y arg3)
        {
            if (m_EventTable.TryGetValue(eventType, out Delegate d))
            {
                Action<T, X, Y> act = d as Action<T, X, Y>;
                act?.Invoke(arg1, arg2, arg3);
            }
            else
            {
                // Debug.LogWarning($"广播事件为空 {eventType}");
            }
        }

        public static void Trigger<T, X, Y, Z>(string eventType, T arg1, X arg2, Y arg3, Z arg4)
        {
            if (m_EventTable.TryGetValue(eventType, out Delegate d))
            {
                Action<T, X, Y, Z> act = d as Action<T, X, Y, Z>;
                act?.Invoke(arg1, arg2, arg3, arg4);
            }
            else
            {
                // Debug.LogWarning($"广播事件为空 {eventType}");
            }
        }

        public static void Trigger<T, X, Y, Z, W>(string eventType, T arg1, X arg2, Y arg3, Z arg4, W arg5)
        {
            if (m_EventTable.TryGetValue(eventType, out Delegate d))
            {
                Action<T, X, Y, Z, W> act = d as Action<T, X, Y, Z, W>;
                act?.Invoke(arg1, arg2, arg3, arg4, arg5);
            }
            else
            {
                // Debug.LogWarning($"广播事件为空 {eventType}");
            }
        }
}
