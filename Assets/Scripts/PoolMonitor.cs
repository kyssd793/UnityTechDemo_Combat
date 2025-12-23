using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 对象池性能监控面板（技术导向）
/// </summary>
public class PoolMonitor : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _monitorText; // 显示监控信息的UI文本
    private float _updateInterval = 1f; // 1秒更新一次
    private float _timer;
    private int _gcCount = 0; // GC触发次数
    private long _lastGcMemory; // 上一次GC内存

    private void Start()
    {
        _lastGcMemory = System.GC.GetTotalMemory(false);
    }

    private void Update()
    {
        // 定时更新监控信息
        _timer += Time.deltaTime;
        if (_timer >= _updateInterval)
        {
            UpdateMonitorInfo();
            CheckGC();
            _timer = 0;
        }
    }

    /// <summary>
    /// 更新监控信息
    /// </summary>
    private void UpdateMonitorInfo()
    {
        if (_monitorText == null || PoolManager.Instance == null) return;

        // 替换：对象池监控 → Object Pool Monitor
        string info = "Object Pool Monitor:\n";
        // 获取PoolManager的对象池字典（需要给PoolManager的_poolDict加public访问权限）
        Dictionary<GameObject, ObjectPool> poolDict = PoolManager.Instance.GetPoolDict();

        foreach (var kvp in poolDict)
        {
            GameObject prefab = kvp.Key;
            ObjectPool pool = kvp.Value;
            // 替换：活跃 → Active，缓存 → Cached
            info += $"{prefab.name}: Active {pool.GetActiveCount()} | Cached {pool.GetCacheCount()}\n";
        }

        // 替换：GC触发次数 → GC Triggers
        info += $"\nGC Triggers: {_gcCount}";
        _monitorText.text = info;
    }

    /// <summary>
    /// 检测GC是否触发
    /// </summary>
    private void CheckGC()
    {
        long currentMemory = System.GC.GetTotalMemory(false);
        if (currentMemory < _lastGcMemory)
        {
            _gcCount++;
        }
        _lastGcMemory = currentMemory;
    }
}