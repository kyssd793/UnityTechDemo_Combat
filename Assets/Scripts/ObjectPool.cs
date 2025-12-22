using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameObject对象池（直接管理Unity物体，避免泛型语法问题）
/// </summary>
public class ObjectPool
{
    // 待复用的对象队列
    private Queue<GameObject> _poolQueue = new Queue<GameObject>();
    // 预制体模板
    private GameObject _prefab;
    // 父物体（整理场景层级）
    private Transform _parent;


    /// <summary>
    /// 初始化对象池
    /// </summary>
    public ObjectPool(GameObject prefab, Transform parent)
    {
        _prefab = prefab;
        _parent = parent;
    }


    /// <summary>
    /// 获取物体（复用优先）
    /// </summary>
    public GameObject Get()
    {
        GameObject obj;
        if (_poolQueue.Count > 0)
        {
            obj = _poolQueue.Dequeue();
        }
        else
        {
            obj = Object.Instantiate(_prefab, _parent);
        }
        obj.SetActive(true);
        return obj;
    }


    /// <summary>
    /// 回收物体（禁用并加入队列）
    /// </summary>
    public void Release(GameObject obj)
    {
        obj.SetActive(false);
        _poolQueue.Enqueue(obj);
    }
}