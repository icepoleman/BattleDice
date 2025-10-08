using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用事件中心，支援多參數 (object[])，簡化 API
/// </summary>
public static class EventCenter
{
    private static readonly Dictionary<string, Action<object[]>> eventTable = new();

    /// <summary>
    /// 註冊事件
    /// </summary>
    public static void AddListener(string eventName, Action<object[]> callback)
    {
        if (string.IsNullOrEmpty(eventName) || callback == null)
        {
            Debug.LogWarning($"[EventCenter] AddListener: eventName 或 callback 為空");
            return;
        }
        if (!eventTable.ContainsKey(eventName))
            eventTable[eventName] = null;
        eventTable[eventName] += callback;
    }

    /// <summary>
    /// 移除事件
    /// </summary>
    public static void RemoveListener(string eventName, Action<object[]> callback)
    {
        if (string.IsNullOrEmpty(eventName) || callback == null)
            return;
        if (eventTable.TryGetValue(eventName, out var action))
        {
            eventTable[eventName] -= callback;
            if (eventTable[eventName] == null)
                eventTable.Remove(eventName);
        }
    }

    /// <summary>
    /// 移除整個事件
    /// </summary>
    public static void RemoveAll(string eventName)
    {
        if (string.IsNullOrEmpty(eventName)) return;
        eventTable.Remove(eventName);
    }

    /// <summary>
    /// 廣播事件
    /// </summary>
    public static void Dispatch(string eventName, params object[] args)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogWarning($"[EventCenter] Dispatch: eventName 為空");
            return;
        }
        if (eventTable.TryGetValue(eventName, out var action))
        {
            try
            {
                action?.Invoke(args);
            }
            catch (Exception ex)
            {
                //Debug.LogError($"[EventCenter] Dispatch: 事件 {eventName} 執行錯誤: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"[EventCenter] Dispatch: 未註冊事件 {eventName}");
        }
    }
}
