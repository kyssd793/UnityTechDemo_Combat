using UnityEngine;
using System.Collections.Generic;
using System.Linq; // 引入列表命名空间

public class SpawnTest : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] public GameObject _npcPrefab;

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
            if (_spawnedObjects.ContainsKey(obj))
            {
                _spawnedObjects.Remove(obj);
            }
            _spawnedObjects.Add(obj, prefab);

            // 如果是Player预制体，让摄像机跟随
            if (prefab == _playerPrefab)
            {
                PlayerMovement.SetCurrentPlayer(obj);
                // 通知摄像机跟随新Player
                if (CameraFollow.Instance != null)
                {
                    CameraFollow.Instance.SetTargetPlayer(obj.transform);
                }
            }
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
        //// 不用对象池，直接新建
        //return Object.Instantiate(prefab);
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
    /// 清理字典中已被销毁的物体（彻底修复无效条目）
    /// </summary>
    private void CleanupDestroyedObjects()
    {
        // 用临时列表存储要移除的物体（包含null键）
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var entry in _spawnedObjects)
        {
            // 同时检查“键是否为null”和“物体是否禁用/销毁”
            if (entry.Key == null || !entry.Key.activeInHierarchy)
            {
                toRemove.Add(entry.Key);
            }
        }

        // 移除已销毁/禁用的物体
        foreach (var obj in toRemove)
        {
            if (_spawnedObjects.ContainsKey(obj))
            {
                _spawnedObjects.Remove(obj);
                Debug.Log($"清理已销毁/禁用的物体记录：{obj?.name}");
            }
        }
    }

    /// <summary>
    /// 回收最后一个生成的物体（增加空值判断）
    /// </summary>
    private void RecycleLastObject()
    {
        CleanupDestroyedObjects();

        if (_spawnedObjects.Count == 0)
        {
            Debug.LogWarning("没有可回收的物体！");
            return;
        }

        var validEntries = _spawnedObjects.Where(entry => entry.Key != null).ToList();
        if (validEntries.Count == 0)
        {
            Debug.LogWarning("无有效物体可回收！");
            return;
        }

        var lastEntry = validEntries.Last();
        GameObject lastObj = lastEntry.Key;
        GameObject lastPrefab = lastEntry.Value;

        if (lastObj != null && lastPrefab != null)
        {
            // ========== 新增：回收Player时的切换逻辑 ==========
            if (lastPrefab == _playerPrefab)
            {
                // 1. 移除当前回收的Player后，查找剩余的Player
                List<GameObject> remainingPlayers = new List<GameObject>();
                foreach (var entry in _spawnedObjects)
                {
                    if (entry.Key != null && entry.Value == _playerPrefab && entry.Key != lastObj)
                    {
                        remainingPlayers.Add(entry.Key);
                    }
                }

                // 2. 有剩余Player → 切换视角和控制权到最后一个剩余的Player
                if (remainingPlayers.Count > 0)
                {
                    GameObject newActivePlayer = remainingPlayers.Last();
                    // 切换摄像机跟随
                    if (CameraFollow.Instance != null)
                    {
                        CameraFollow.Instance.SetTargetPlayer(newActivePlayer.transform);
                    }
                    // 切换操控权
                    PlayerMovement.SetCurrentPlayer(newActivePlayer);
                    Debug.Log($"回收Player后，切换控制权到：{newActivePlayer.name}");
                }
                // 3. 无剩余Player → 清空摄像机目标
                else if (CameraFollow.Instance != null)
                {
                    CameraFollow.Instance.ClearTargetPlayer();
                    CameraFollow.Instance.ResetToInitialView();
                }
            }
            // ========== 原有回收逻辑 ==========
            PoolManager.Instance.Despawn(lastPrefab, lastObj);
            _spawnedObjects.Remove(lastObj);
        }
    }
}