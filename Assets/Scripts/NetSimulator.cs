using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单机模拟网络同步管理器（兼容字典参数，无序列化依赖）
/// </summary>
public class NetSimulator : MonoBehaviour
{
    public static NetSimulator Instance { get; private set; }

    [Header("模拟网络配置")]
    public int simulateDelay = 100; // 模拟延迟（毫秒）
    public bool isDebug = true;

    // 消息队列（存储字典参数+发送时间）
    private Queue<NetMessage> _messageQueue = new Queue<NetMessage>();

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

    private void Update()
    {
        // 处理延迟后的消息
        while (_messageQueue.Count > 0)
        {
            var msg = _messageQueue.Peek();
            if (Time.time * 1000 - msg.sendTime >= simulateDelay)
            {
                _messageQueue.Dequeue();
                // 分发字典参数（模拟服务器广播）
                DispatchMessage(msg.argsDict);
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// 发送字典参数（模拟客户端→服务器）
    /// </summary>
    public void SendMessage(Dictionary<string, object> argsDict)
    {
        if (argsDict == null || argsDict.Count == 0)
        {
            if (isDebug) Debug.LogWarning("[模拟网络] 空字典参数，忽略");
            return;
        }

        _messageQueue.Enqueue(new NetMessage
        {
            sendTime = Time.time * 1000,
            argsDict = argsDict
        });

        if (isDebug)
        {
            Debug.Log($"[模拟网络] 发送消息 | 延迟{simulateDelay}ms | 队列长度：{_messageQueue.Count}");
        }
    }

    /// <summary>
    /// 分发消息（模拟服务器→客户端）
    /// </summary>
    private void DispatchMessage(Dictionary<string, object> argsDict)
    {
        // 触发网络伤害事件，传字典参数
        EventManager.Instance.TriggerEvent("OnNetDamage", argsDict);

        if (isDebug)
        {
            Debug.Log($"[模拟网络] 广播消息 | 所有NPC接收");
        }
    }

    // 网络消息结构体
    private struct NetMessage
    {
        public float sendTime; // 发送时间（毫秒）
        public Dictionary<string, object> argsDict; // 字典参数
    }
}