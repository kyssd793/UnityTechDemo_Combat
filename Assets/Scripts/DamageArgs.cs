using System;
using UnityEngine; // 引入Unity命名空间

[Serializable] 
public class DamageArgs
{
    public int targetId; // 目标NPC的InstanceID
    public int damageValue; // 伤害值
    public float attackTime; // 攻击时间戳
}