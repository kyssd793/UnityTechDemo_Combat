using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Text;
using System.Linq;

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

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Object Pool Monitor:");

        // 关键：先将字典转为列表，避免遍历中字典被修改
        var poolDict = PoolManager.Instance.GetPoolDict().ToList();
        foreach (var kvp in poolDict)
        {
            GameObject prefab = kvp.Key;
            if (prefab == null) continue; // 过滤空预制体

            int active = PoolManager.Instance.GetActiveCount(prefab);
            int cache = PoolManager.Instance.GetCacheCount(prefab);
            sb.AppendLine($"{prefab.name}: Active {active} | Cached {cache}");
        }
        sb.AppendLine($"\nGC Triggers: {_gcCount}");
        _monitorText.text = sb.ToString();
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