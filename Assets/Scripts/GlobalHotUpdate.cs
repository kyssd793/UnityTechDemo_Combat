using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 全局热更新管理器（批量更新所有NPC）
/// </summary>
public class GlobalHotUpdate : MonoBehaviour
{
    // 单例模式（全局唯一）
    public static GlobalHotUpdate Instance { get; private set; }

    // 存储所有活跃的NPC
    private List<NPCEmptyComp> _allNPCs = new List<NPCEmptyComp>();

    // 全局热更提示文本（可选，显示整体热更状态）
    public TextMeshProUGUI globalHotUpdateTip;

    private void Awake()
    {
        // 单例初始化（避免重复创建）
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 注册NPC（NPC创建时调用）
    /// </summary>
    public void RegisterNPC(NPCEmptyComp npc)
    {
        if (!_allNPCs.Contains(npc))
        {
            _allNPCs.Add(npc);
        }
    }

    /// <summary>
    /// 注销NPC（NPC回收时调用）
    /// </summary>
    public void UnregisterNPC(NPCEmptyComp npc)
    {
        if (_allNPCs.Contains(npc))
        {
            _allNPCs.Remove(npc);
        }
    }

    /// <summary>
    /// 全局热更新所有NPC（绑定到UI按钮）
    /// </summary>
    [ContextMenu("GlobalHotUpdateAllNPCs")] // 编辑器右键可测试
    public void HotUpdateAllNPCs()
    {
        if (_allNPCs.Count == 0)
        {
            ShowGlobalTip("No NPCs to update!");
            Debug.Log("[全局热更] 无可用NPC");
            return;
        }

        int successCount = 0;
        foreach (var npc in _allNPCs)
        {
            try
            {
                npc.HotUpdateLua(); // 调用每个NPC的热更方法
                successCount++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[全局热更] NPC更新失败：{e.Message}");
            }
        }

        // 显示全局热更结果
        string tip = $"HotUpdate All NPCs: {successCount}/{_allNPCs.Count} success!";
        ShowGlobalTip(tip);
        Debug.Log($"[全局热更] 完成：成功{successCount}个，总计{_allNPCs.Count}个");
    }

    /// <summary>
    /// 显示全局热更提示
    /// </summary>
    private void ShowGlobalTip(string text)
    {
        if (globalHotUpdateTip != null)
        {
            globalHotUpdateTip.text = text;
        }
    }
}