using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    // 字典：Key=预制体，Value=对应的对象池
    private Dictionary<GameObject, ObjectPool> _poolDict = new Dictionary<GameObject, ObjectPool>();

    [SerializeField] private Transform _poolParent;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_poolParent == null)
        {
            _poolParent = new GameObject("PoolParent").transform;
            _poolParent.parent = transform;
        }
    }


    /// <summary>
    /// 获取指定预制体的对象池
    /// </summary>
    public ObjectPool GetPool(GameObject prefab)
    {
        if (!_poolDict.ContainsKey(prefab))
        {
            _poolDict.Add(prefab, new ObjectPool(prefab, _poolParent));
        }
        return _poolDict[prefab];
    }


    /// <summary>
    /// 快捷生成物体
    /// </summary>
    public GameObject Spawn(GameObject prefab)
    {
        return GetPool(prefab).Get();
    }


    /// <summary>
    /// 快捷回收物体
    /// </summary>
    public void Despawn(GameObject prefab, GameObject obj)
    {
        if (_poolDict.ContainsKey(prefab))
        {
            _poolDict[prefab].Release(obj);
        }
        else
        {
            Debug.LogWarning($"未找到预制体{prefab.name}的对象池，直接销毁");
            Object.Destroy(obj);
        }
    }
}