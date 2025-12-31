using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 全局事件管理器（事件驱动核心）
/// </summary>
public class EventManager : MonoBehaviour
{
    // 单例模式
    public static EventManager Instance { get; private set; }

    // 事件字典：key=事件名，value=委托列表
    private Dictionary<string, UnityAction<object>> _eventDict = new Dictionary<string, UnityAction<object>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 注册事件
    public void AddListener(string eventName, UnityAction<object> callback)
    {
        if (!_eventDict.ContainsKey(eventName))
        {
            _eventDict[eventName] = callback;
        }
        else
        {
            _eventDict[eventName] += callback;
        }
    }

    // 移除事件
    public void RemoveListener(string eventName, UnityAction<object> callback)
    {
        if (_eventDict.ContainsKey(eventName))
        {
            _eventDict[eventName] -= callback;
            if (_eventDict[eventName] == null)
            {
                _eventDict.Remove(eventName);
            }
        }
    }

    // 触发事件
    public void TriggerEvent(string eventName, object args)
    {
        Debug.Log($"[事件] 触发事件：{eventName}，参数：{args}"); // 新增
        if (_eventDict.ContainsKey(eventName) && _eventDict[eventName] != null)
        {
            Debug.Log($"[事件] 事件{eventName}有{_eventDict[eventName].GetInvocationList().Length}个回调"); // 新增
            _eventDict[eventName]?.Invoke(args);
        }
        else
        {
            Debug.LogWarning($"[事件] 事件{eventName}无回调或未注册"); // 新增
        }
    }
}