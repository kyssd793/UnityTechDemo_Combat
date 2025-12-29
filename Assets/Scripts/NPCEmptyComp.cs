using System;
using UnityEngine;
using XLua;

public class NPCEmptyComp : MonoBehaviour
{
    private LuaEnv _luaEnv = new LuaEnv();
    private LuaFunction _luaCalcMoveDirFunc;
    private Rigidbody _rb;

    [Header("NPC移动配置")]
    public float moveSpeed = 3f;
    public float changeDirInterval = 5f; // 延长到x秒，减少变向
    public Vector2 boundaryX = new Vector2(-9, 9);
    public Vector2 boundaryZ = new Vector2(-9, 9);

    private float _currentDirX = 1;
    private float _currentDirZ = 0;
    private float _lastChangeTime;
    private float _lastDirUpdateTime; // 新增：记录上次更新方向的时间
    private float _dirUpdateInterval = 0.2f; // 每0.2秒才更新一次方向

    private void Awake()
    {
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

    private void FixedUpdate()
    {
        if (_luaCalcMoveDirFunc != null)
        {
            // 核心限制：每0.2秒才更新一次方向，避免高频抖动
            if (Time.time - _lastDirUpdateTime >= _dirUpdateInterval)
            {
                object[] luaResult = _luaCalcMoveDirFunc.Call(
                    _rb.position.x, _rb.position.z,
                    Time.time, _lastChangeTime,
                    _currentDirX, _currentDirZ
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

            // 移动逻辑（方向不再高频变化）
            Vector3 targetVelocity = new Vector3(_currentDirX, 0, _currentDirZ) * moveSpeed;
            _rb.velocity = targetVelocity;
        }
    }

    private void OnDestroy()
    {
        _luaCalcMoveDirFunc?.Dispose();
        _luaEnv.Dispose();
    }
}