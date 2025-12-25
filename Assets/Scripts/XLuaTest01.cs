using UnityEngine;
using XLua; // 此时命名空间应该能正常识别

public class XLuaTest01 : MonoBehaviour
{
    private void Start()
    {
        LuaEnv luaEnv = new LuaEnv();
        luaEnv.DoString("print('XLua导入成功！')");
        luaEnv.Dispose();
    }
}