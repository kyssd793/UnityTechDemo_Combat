using System.Collections.Generic;
using UnityEngine;
using static NPCEmptyComp;

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
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, npcLayer);

        foreach (var collider in hitColliders)
        {
            GameObject npcObj = collider.gameObject;
            if (npcObj.CompareTag("NPC"))
            {
                NPCEmptyComp targetNpc = npcObj.GetComponent<NPCEmptyComp>();
                if (targetNpc != null)
                {
                    // 用字典传参（基础类型，不会被XLua干扰）
                    Dictionary<string, object> argsDict = new Dictionary<string, object>()
                    {
                        { "targetId", targetNpc.GetInstanceID() },
                        { "damageValue", damageValue },
                        { "attackTime", Time.time }
                    };
                    //EventManager.Instance.TriggerEvent("OnTakeDamage", argsDict);
                    if (NetSimulator.Instance != null)
                    {
                        NetSimulator.Instance.SendMessage(argsDict); // 发往模拟服务器
                    }
                    else
                    {
                        Debug.LogError("[攻击] NetSimulator未挂载！");
                        // 降级：直接触发事件（保证原有功能可用）
                        EventManager.Instance.TriggerEvent("OnTakeDamage", argsDict);
                    }
                }
            }           
        }
    }
}