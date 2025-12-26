using UnityEditor;
using UnityEngine;

// 让Unity把.lua文件识别为TextAsset
public class LuaImporter : AssetPostprocessor
{
    // 当资源导入时触发
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            // 匹配.lua后缀的文件
            if (path.EndsWith(".lua"))
            {
                // 强制将.lua文件的导入类型设为TextAsset
                AssetImporter importer = AssetImporter.GetAtPath(path);
                if (importer != null)
                {
                    importer.assetBundleName = "";
                    importer.userData = "";
                    // 关键：设置导入类型为TextAsset
                    importer.SetAssetBundleNameAndVariant("", "");
                    TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    if (textAsset != null)
                    {
                        Debug.Log($"成功将Lua文件识别为TextAsset：{path}");
                    }
                }
            }
        }
    }
}