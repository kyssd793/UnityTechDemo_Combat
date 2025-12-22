using UnityEngine;
using System.Collections.Generic;
using System.Linq; // 引入列表命名空间

public class SpawnTest : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _npcPrefab;

    [SerializeField] private float _maxX = 9f;
    [SerializeField] private float _maxZ = 9f;
    [SerializeField] private float _minY = 0.5f;

    // 用列表记录所有生成的物体和对应的预制体（键：物体，值：预制体）
    private Dictionary<GameObject, GameObject> _spawnedObjects = new Dictionary<GameObject, GameObject>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SpawnAndRecord(_playerPrefab);
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            SpawnAndRecord(_npcPrefab);
        }

        if (Input.GetKeyDown(KeyCode.F)) // 用F键回收
        {
            RecycleLastObject();
        }
    }

    /// <summary>
    /// 生成物体并记录到字典中
    /// </summary>
    private void SpawnAndRecord(GameObject prefab)
    {
        GameObject obj = SpawnObject(prefab);
        if (obj != null)
        {
            // 先移除旧的记录（如果物体已存在），再添加新记录
            if (_spawnedObjects.ContainsKey(obj))
            {
                _spawnedObjects.Remove(obj);
            }
            _spawnedObjects.Add(obj, prefab);
            Debug.Log($"已记录物体：{obj.name}，当前记录数量：{_spawnedObjects.Count}");
        }
    }

    private GameObject SpawnObject(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("预制体为空，无法生成物体！");
            return null;
        }

        GameObject obj = PoolManager.Instance.Spawn(prefab);
        if (obj == null)
        {
            Debug.LogError($"生成物体失败！预制体{prefab.name}返回null");
            return null;
        }

        Vector3 randomPos = new Vector3(
            Random.Range(-_maxX, _maxX),
            _minY,
            Random.Range(-_maxZ, _maxZ)
        );
        obj.transform.position = randomPos;
        return obj;
    }

    /// <summary>
    /// 回收最后一个生成的物体
    /// </summary>
    private void RecycleLastObject()
    {
        // 先清理字典中已被销毁的物体（避免空引用）
        CleanupDestroyedObjects();

        if (_spawnedObjects.Count == 0)
        {
            Debug.LogWarning("没有可回收的物体！");
            return;
        }

        // 取字典中最后一个物体（Linq需要引入：using System.Linq;）
        var lastEntry = _spawnedObjects.Last();
        GameObject lastObj = lastEntry.Key;
        GameObject lastPrefab = lastEntry.Value;

        // 执行回收
        if (lastObj != null)
        {
            PoolManager.Instance.Despawn(lastPrefab, lastObj);
            Debug.Log($"成功回收物体：{lastObj.name}");
            // 从字典中移除该物体
            _spawnedObjects.Remove(lastObj);
        }
        else
        {
            _spawnedObjects.Remove(lastObj);
            Debug.LogWarning("最后一个物体已销毁，自动移除记录");
        }
    }

    /// <summary>
    /// 清理字典中已被销毁的物体
    /// </summary>
    private void CleanupDestroyedObjects()
    {
        // 用临时列表存储要移除的物体
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var entry in _spawnedObjects)
        {
            if (entry.Key == null || !entry.Key.activeInHierarchy)
            {
                toRemove.Add(entry.Key);
            }
        }

        // 移除已销毁/禁用的物体
        foreach (var obj in toRemove)
        {
            _spawnedObjects.Remove(obj);
            Debug.Log($"清理已销毁/禁用的物体记录：{obj?.name}");
        }
    }
}