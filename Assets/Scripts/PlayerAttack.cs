using UnityEngine;

/// <summary>
/// 极简Player攻击脚本（验证对象池回收）
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackRange = 2f; // 攻击范围
    [SerializeField] private LayerMask npcLayer; // 只检测NPC层

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
        // 1. 球形碰撞检测：获取范围内的所有NPC
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, npcLayer);

        foreach (var collider in hitColliders)
        {
            GameObject npcObj = collider.gameObject;
            if (npcObj.CompareTag("NPC"))
            {
                // 2. 找到NPC对应的预制体（假设NPC预制体在Resources/Prefabs下，或通过SpawnTest传参）
                // 简化：直接从SpawnTest的预制体引用获取（你可以把SpawnTest的_npcPrefab设为public）
                GameObject npcPrefab = FindObjectOfType<SpawnTest>()._npcPrefab;

                // 3. 调用对象池回收NPC（核心：验证对象池的Release逻辑）
                if (PoolManager.Instance != null && npcPrefab != null)
                {
                    PoolManager.Instance.Despawn(npcPrefab, npcObj);
                    Debug.Log($"回收NPC：{npcObj.name}，对象池缓存数+1");
                }
            }
        }
    }
}