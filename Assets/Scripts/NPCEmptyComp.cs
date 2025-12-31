using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XLua;

public class NPCEmptyComp : MonoBehaviour
{
    // 血量配置
    [Header("NPC血量配置")]
    public int maxHp = 100;
    private int _currentHp;

    private LuaEnv _luaEnv = new LuaEnv();
    private LuaFunction _luaCalcMoveDirFunc;
    private Rigidbody _rb;

    [Header("NPC移动配置")]
    public float moveSpeed = 3f;
    public float changeDirInterval = 1f; // 延长到x秒，减少变向
    public Vector2 boundaryX = new Vector2(-9, 9);
    public Vector2 boundaryZ = new Vector2(-9, 9);

    private float _currentDirX = 1;
    private float _currentDirZ = 0;
    private float _lastChangeTime;
    private float _lastDirUpdateTime; // 新增：记录上次更新方向的时间
    private float _dirUpdateInterval = 3f; // 每0.2秒才更新一次方向

    private void Awake()
    {
        // 初始化血量
        _currentHp = maxHp;

        // 注册受击事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.AddListener("OnTakeDamage", OnTakeDamage);
        }

        // 自动找场景里的Text（名字叫"Text_for_XLua"）
        hotUpdateTipText = GameObject.Find("Text_for_XLua").GetComponent<TextMeshProUGUI>();

        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _rb.drag = 0.5f; // 直接在代码里设置阻尼，避免手动改预制体

        string luaPath = Application.dataPath + "/Resources/Lua/NPCAI.lua";
        if (System.IO.File.Exists(luaPath))
        {
            string luaContent = System.IO.File.ReadAllText(luaPath);
            _luaEnv.DoString(luaContent);
            _luaCalcMoveDirFunc = _luaEnv.Global.Get<LuaFunction>("CalcNPCMoveDir");
        }

        _luaEnv.Global.Set("changeDirInterval", changeDirInterval);
        _luaEnv.Global.Set("boundaryXMin", boundaryX.x);
        _luaEnv.Global.Set("boundaryXMax", boundaryX.y);
        _luaEnv.Global.Set("boundaryZMin", boundaryZ.x);
        _luaEnv.Global.Set("boundaryZMax", boundaryZ.y);
    }
    private void OnEnable()
    {
        // NPC激活时（从对象池取出），注册到全局管理器
        if (GlobalHotUpdate.Instance != null)
        {
            GlobalHotUpdate.Instance.RegisterNPC(this);
        }
    }

    public TextMeshProUGUI hotUpdateTipText; // 对应场景里的TextMeshPro文本

    // 新增：热更新Lua脚本的核心方法（绑定到UI按钮）
    [ContextMenu("HotUpdateLua")] // 编辑器右键可测试，无需UI
    public void HotUpdateLua()
    {
        try
        {
            string luaPath = Application.dataPath + "/Resources/Lua/NPCAI.lua";
            if (System.IO.File.Exists(luaPath))
            {
                string luaContent = System.IO.File.ReadAllText(luaPath);
                _luaEnv.DoString("package.loaded['NPCAI'] = nil");
                _luaEnv.DoString(luaContent);
                _luaCalcMoveDirFunc = _luaEnv.Global.Get<LuaFunction>("CalcNPCMoveDir");

                float luaChangeDir = _luaEnv.Global.Get<float>("changeDirInterval");
                Debug.Log($"Lua changeDirInterval: {luaChangeDir}");

                if (hotUpdateTipText != null)
                {
                    hotUpdateTipText.text = $"HotUpdate Success!\nInterval: {luaChangeDir}s";
                }
            }
            else
            {
                if (hotUpdateTipText != null)
                {
                    hotUpdateTipText.text = "HotUpdate Failed: File not found";
                }
                Debug.LogError("[HotUpdate] File not found: " + luaPath);
            }
        }
        catch (Exception e)
        {
            if (hotUpdateTipText != null)
            {
                hotUpdateTipText.text = $"HotUpdate Error: {e.Message}";
            }
            Debug.LogError("[HotUpdate] Error: " + e);
        }
    }
    private void FixedUpdate()
    {
        if (_luaCalcMoveDirFunc != null)
        {
            // 新增：获取玩家位置（给玩家加"Player"标签）
            Vector3 playerPos = Vector3.zero;
            GameObject player = GameObject.FindWithTag("player");
            if (player != null)
            {
                playerPos = player.transform.position;
            }
            // 计算NPC与玩家的距离
            float distanceToPlayer = Vector3.Distance(_rb.position, playerPos);
            // 随机移动逻辑
            if (Time.time - _lastDirUpdateTime >= _dirUpdateInterval)
            {
                object[] luaResult = _luaCalcMoveDirFunc.Call(
                    _rb.position.x, _rb.position.z,
                    Time.time, _lastChangeTime,
                    _currentDirX, _currentDirZ,
                    playerPos.x, playerPos.z, // 玩家X/Z
                    distanceToPlayer          // 与玩家的距离
                );

                if (luaResult != null && luaResult.Length >= 2)
                {
                    _currentDirX = Convert.ToSingle(luaResult[0]);
                    _currentDirZ = Convert.ToSingle(luaResult[1]);

                    // 归一化+保底
                    float dirLen = Mathf.Sqrt(_currentDirX * _currentDirX + _currentDirZ * _currentDirZ);
                    if (dirLen < 0.1f)
                    {
                        _currentDirX = 1;
                        _currentDirZ = 0;
                    }
                    else
                    {
                        _currentDirX /= dirLen;
                        _currentDirZ /= dirLen;
                    }

                    if (luaResult.Length >= 3 && Convert.ToBoolean(luaResult[2]))
                    {
                        _lastChangeTime = Time.time;
                    }
                }

                _lastDirUpdateTime = Time.time; // 更新方向时间戳
            }

            //// 移动逻辑（方向不再高频变化）
            //Vector3 targetVelocity = new Vector3(_currentDirX, 0, _currentDirZ) * moveSpeed;
            //_rb.velocity = targetVelocity;
            // 新增：追击时加速
            float finalSpeed = moveSpeed;
            if (distanceToPlayer < 5)
            {
                finalSpeed *= 1.5f;
            }
            Vector3 targetVelocity = new Vector3(_currentDirX, 0, _currentDirZ) * finalSpeed;
            _rb.velocity = targetVelocity;
        }
    }

    /// <summary>
    /// 受击回调
    /// </summary>
    private void OnTakeDamage(object argsObj)
    {
        Debug.Log("[NPC] 收到受击事件");
        // 解析字典
        Dictionary<string, object> argsDict = argsObj as Dictionary<string, object>;
        if (argsDict == null)
        {
            Debug.LogError("[NPC] 受击参数不是字典");
            return;
        }

        // 获取参数
        if (!argsDict.TryGetValue("targetId", out object targetIdObj) ||
            !argsDict.TryGetValue("damageValue", out object damageValueObj))
        {
            Debug.LogError("[NPC] 受击参数缺失");
            return;
        }
        int targetId = Convert.ToInt32(targetIdObj);
        int damageValue = Convert.ToInt32(damageValueObj);

        Debug.Log($"[NPC] 受击参数：目标ID={targetId}，当前NPC ID={GetInstanceID()}");
        if (targetId != GetInstanceID())
        {
            Debug.Log($"[NPC] 不是当前NPC的事件");
            return;
        }

        _currentHp -= damageValue;
        Debug.Log($"[NPC受击] {gameObject.name} 血量：{_currentHp}/{maxHp}");

        if (_currentHp <= 0)
        {
            Debug.Log($"[NPC] 血量为0，准备回收");
            RecycleSelf();
        }
    }

    /// <summary>
    /// 回收自身到对象池
    /// </summary>
    private void RecycleSelf()
    {
        Debug.Log($"[NPC回收] 开始回收：{gameObject.name}");
        // 注销事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener("OnTakeDamage", OnTakeDamage);
        }
        if (GlobalHotUpdate.Instance != null)
        {
            GlobalHotUpdate.Instance.UnregisterNPC(this);
        }

        // 重置血量
        _currentHp = maxHp;

        // 回收到对象池（关键：传gameObject而非this）
        GameObject npcPrefab = FindObjectOfType<SpawnTest>()._npcPrefab;
        if (npcPrefab == null)
        {
            Debug.LogError("[NPC回收] 未找到NPC预制体");
            return;
        }
        if (PoolManager.Instance == null)
        {
            Debug.LogError("[NPC回收] PoolManager为空");
            return;
        }

        // 改为传gameObject（匹配非泛型Despawn方法）
        PoolManager.Instance.Despawn(npcPrefab, gameObject);
        Debug.Log($"[NPC回收] 成功回收到对象池：{gameObject.name}");
    }




    private void OnDisable()
    {
        // NPC回收时（放回对象池），注销从全局管理器
        if (GlobalHotUpdate.Instance != null)
        {
            GlobalHotUpdate.Instance.UnregisterNPC(this);
        }
    }
    private void OnDestroy()
    {
        _luaCalcMoveDirFunc?.Dispose();
        _luaEnv.Dispose();
    }
}


