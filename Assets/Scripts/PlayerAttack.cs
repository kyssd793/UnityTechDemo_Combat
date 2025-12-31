using System.Collections.Generic;
using UnityEngine;
using static NPCEmptyComp;

/// <summary>
/// 极简Player攻击脚本（验证对象池回收）
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackRange = 2f; // 攻击范围
    [SerializeField] private LayerMask npcLayer; // 只检测NPC层
    [SerializeField] private int damageValue = 20; // 伤害值

    private void Update()
    {
        // 按空格键触发攻击
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AttackNPC();
        }
    }

    /// <summary>
    /// 范围攻击：回收范围内的NPC
    /// </summary>
    private void AttackNPC()
    {
        Debug.Log("[攻击] 开始检测NPC...");
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, npcLayer);
        Debug.Log($"[攻击] 检测到{hitColliders.Length}个碰撞体");

        foreach (var collider in hitColliders)
        {
            GameObject npcObj = collider.gameObject;
            Debug.Log($"[攻击] 检测到物体：{npcObj.name}，标签：{npcObj.tag}");
            if (npcObj.CompareTag("NPC"))
            {
                NPCEmptyComp targetNpc = npcObj.GetComponent<NPCEmptyComp>();
                if (targetNpc != null)
                {
                    Debug.Log($"[攻击] 触发受击事件：目标NPC={targetNpc.name}，ID={targetNpc.GetInstanceID()}");
                    // 用字典传参（基础类型，不会被XLua干扰）
                    Dictionary<string, object> argsDict = new Dictionary<string, object>()
                    {
                        { "targetId", targetNpc.GetInstanceID() },
                        { "damageValue", damageValue },
                        { "attackTime", Time.time }
                    };
                    EventManager.Instance.TriggerEvent("OnTakeDamage", argsDict);
                }
                else
                {
                    Debug.LogWarning($"[攻击] {npcObj.name} 没有NPCEmptyComp组件");
                }
            }           
            else
                {
                    Debug.LogWarning($"[攻击] {npcObj.name} 不是NPC标签"); 
                }
            }
    }
}