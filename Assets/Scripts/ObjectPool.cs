using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameObject对象池（直接管理Unity物体，避免泛型语法问题）
/// </summary>
public class ObjectPool<T> where T : MonoBehaviour
{
    // 待复用的对象队列
    private Queue<T> _poolQueue = new Queue<T>();
    // 预制体模板
    private GameObject _prefab;
    // 父物体（整理场景层级）
    private Transform _parent;
    private int _activeCount;

    // LRU+缓存上限+统计
    private Dictionary<T, float> _lastUseTimeDict = new Dictionary<T, float>(); // 记录每个对象最后使用时间（LRU）
    private int _maxCacheCount = 20; // 单预制体最大缓存数
    public int HitCount { get; private set; } = 0; // 复用命中数（面试量化用）
    public int MissCount { get; private set; } = 0; // 未命中数（面试量化用）

    /// <summary>
    /// 初始化对象池
    /// </summary>
    public ObjectPool(GameObject prefab, Transform parent, int maxCacheCount = 20)
    {
        _prefab = prefab;
        _parent = parent;
        _activeCount = 0;
        _maxCacheCount = maxCacheCount;
    }


    /// <summary>
    /// 获取物体（复用优先）
    /// </summary>
    public T Get()
    {
        T obj;
        if (_poolQueue.Count > 0)
        {
            obj = _poolQueue.Dequeue();
            _lastUseTimeDict.Remove(obj);
            obj.gameObject.SetActive(true);
            HitCount++;
        }
        else
        {
            GameObject go= Object.Instantiate(_prefab, _parent);
            obj = go.GetComponent<T>();
            MissCount++;
        }
        _activeCount++;
        return obj;
    }


    /// <summary>
    /// 回收物体（禁用并加入队列）
    /// </summary>
    public void Release(T obj)
    {
        if(obj==null)
        {
            Debug.LogWarning("尝试回收空组件，忽略");
            return;
        }
        obj.gameObject.SetActive(false);
        _activeCount--;
        _activeCount=Mathf.Max(0, _activeCount);

        // 缓存数超上限时，淘汰最久未使用的对象
        if (_poolQueue.Count >= _maxCacheCount)
        {
            T oldestObj = null;
            float oldestTime = float.MaxValue;
            // 遍历找到最久未使用的对象
            foreach (var kvp in _lastUseTimeDict)
            {
                if (kvp.Value < oldestTime)
                {
                    oldestTime = kvp.Value;
                    oldestObj = kvp.Key;
                }
            }
            // 移除并销毁最久未使用的对象
            if (oldestObj != null)
            {
                var tempList = new List<T>(_poolQueue);
                tempList.Remove(oldestObj);
                _poolQueue = new Queue<T>(tempList);
                _lastUseTimeDict.Remove(oldestObj);
                Object.Destroy(oldestObj.gameObject);
                Debug.Log($"[LRU淘汰] {_prefab.name}缓存达上限({_maxCacheCount})，销毁最久未使用对象");
            }
        }
        _poolQueue.Enqueue(obj);
        _lastUseTimeDict[obj] = Time.time; // 记录当前对象最后使用时间
    }

    public int GetActiveCount()
    {
        return _activeCount; 
    }

    /// <summary>
    /// 获取缓存的物体数量
    /// </summary>
    public int GetCacheCount()
    {
        return _poolQueue.Count;
    }
    public float GetHitRate()
    {
        int total = HitCount + MissCount;
        return total == 0 ? 0 : (float)HitCount / total * 100;
    }
}