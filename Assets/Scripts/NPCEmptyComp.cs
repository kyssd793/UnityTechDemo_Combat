using UnityEngine;
using XLua;

public class NPCEmptyComp : MonoBehaviour
{
    private LuaEnv _luaEnv;
    private LuaFunction _luaMoveFunc;
    private Rigidbody _rb;

    private void Awake()
    {
        _luaEnv = new LuaEnv();
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true; // 和Player一致，防止翻滚

        string luaPath = Application.dataPath + "/Resources/Lua/NPCAI.lua";
        if (System.IO.File.Exists(luaPath))
        {
            string luaContent = System.IO.File.ReadAllText(luaPath);
            _luaEnv.DoString(luaContent);
            _luaMoveFunc = _luaEnv.Global.Get<LuaFunction>("UpdateNPCMovement");
        }
    }

    // 和Player一致，用FixedUpdate处理物理移动（防止卡顿）
    private void FixedUpdate()
    {
        if (_luaMoveFunc != null)
        {
            Vector3 moveDir = new Vector3(1, 0, 0).normalized;
            float moveSpeed = 3f;
            _luaMoveFunc.Call(_rb, moveDir, moveSpeed);
        }
    }

    private void OnDestroy()
    {
        _luaMoveFunc?.Dispose();
        _luaEnv?.Dispose();
    }
}