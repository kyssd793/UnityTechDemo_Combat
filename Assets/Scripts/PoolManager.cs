using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    // 单例
    public static PoolManager Instance { get; private set; }

    // 字典：Key=预制体，Value=对应的对象池
    private Dictionary<GameObject, object> _poolDict = new Dictionary<GameObject, object>();

    [SerializeField] private Transform _poolParent;
    [SerializeField] private GameObject _pool;

    // 内存管理+监控配置
    [Header("对象池性能配置")]
    public int GlobalMaxCacheCount = 100; // 全局最大缓存总数
    public int DefaultSingleMaxCache = 20; // 单预制体默认最大缓存数

    [Header("运行时监控")]
    public int TotalActiveCount; // 所有预制体活跃总数
    public int TotalCacheCount; // 所有预制体缓存总数
    public long EstimatedMemoryUsage; // 预估内存占用


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
    // 每帧更新监控数据
    private void Update()
    {
        UpdatePoolStats();
    }

    /// <summary>
    /// 实时更新所有对象池的统计数据
    /// </summary>
    private void UpdatePoolStats()
    {
        TotalActiveCount = 0;
        TotalCacheCount = 0;

        foreach (var kvp in _poolDict)
        {
            // 适配泛型ObjectPool<T>
            if (kvp.Value is ObjectPool<MonoBehaviour> pool)
            {
                TotalActiveCount += pool.GetActiveCount();
                TotalCacheCount += pool.GetCacheCount();
            }
        }

        // 每个缓存对象按1KB估算,精准计算需遍历组件
        EstimatedMemoryUsage = TotalCacheCount * 1024;

        // 全局缓存上限预警
        if (TotalCacheCount > GlobalMaxCacheCount)
        {
            Debug.LogWarning($"[对象池预警] 全局缓存超上限({GlobalMaxCacheCount})，当前缓存数：{TotalCacheCount}");
        }
    }

    /// <summary>
    /// 获取指定预制体的对象池
    /// </summary>
    /// <typeparam name="T">物体挂载的核心组件（如NPCAI、PlayerMovement）</typeparam>
    public ObjectPool<T> GetPool<T> (GameObject prefab) where T : MonoBehaviour
    {
        if (!_poolDict.ContainsKey(prefab))
        {
            _poolDict.Add(prefab, new ObjectPool<T>(prefab, _poolParent));
        }
        // 安全转换为对应泛型池
        var pool = _poolDict[prefab] as ObjectPool<T>;
        if (pool == null)
        {
            Debug.LogError($"预制体{prefab.name}的对象池类型不匹配，已重新创建");
            pool = new ObjectPool<T>(prefab, _poolParent);
            _poolDict[prefab] = pool;
        }
        return pool;
    }

    /// <summary>
    /// 快捷生成物体（泛型版，返回带组件的对象）
    /// </summary>
    public T Spawn<T>(GameObject prefab) where T : MonoBehaviour
    {
        return GetPool<T>(prefab).Get();
    }
    // 替换非泛型Spawn方法
    public GameObject Spawn(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("预制体为空，无法生成");
            return null;
        }

        // 优先找预制体上的第一个MonoBehaviour组件
        var comp = prefab.GetComponent<MonoBehaviour>();
        if (comp != null)
        {
            // 有组件则用对应类型的池
            var pool = GetPool<MonoBehaviour>(prefab);
            var compInstance = pool.Get();
            return compInstance?.gameObject;
        }
        else
        {
            // 无组件则创建临时空组件
            Debug.LogWarning($"{prefab.name}无MonoBehaviour组件，自动添加临时组件");
            var tempComp = prefab.AddComponent<MonoBehaviour>();
            var pool = GetPool<MonoBehaviour>(prefab);
            var compInstance = pool.Get();
            return compInstance?.gameObject;
        }
    }


    /// <summary>
    /// 快捷回收物体，泛型版
    /// </summary>
    public void Despawn<T>(GameObject prefab, T obj) where T : MonoBehaviour
    {
        if (_poolDict.ContainsKey(prefab))
        {
            var pool = _poolDict[prefab] as ObjectPool<T>;
            pool?.Release(obj);
        }
        else
        {
            Debug.LogWarning($"未找到预制体{prefab.name}的对象池，直接销毁");
            Object.Destroy(obj.gameObject);
        }
    }
    /// <summary>
    /// 快捷回收物体（非泛型版）
    /// </summary>
    public void Despawn(GameObject prefab, GameObject obj)
    {
        if (prefab == null || obj == null)
        {
            Debug.LogError("预制体或物体为空，无法回收");
            return;
        }

        // 尝试获取物体上的组件（和生成时的组件类型匹配）
        var comp = obj.GetComponent<MonoBehaviour>();
        if (comp == null)
        {
            Debug.LogError($"物体{obj.name}无MonoBehaviour组件，无法回收至对象池");
            Object.Destroy(obj);
            return;
        }

        // 找到对应的泛型池并回收
        if (_poolDict.ContainsKey(prefab))
        {
            var pool = _poolDict[prefab] as ObjectPool<MonoBehaviour>;
            if (pool != null)
            {
                pool.Release(comp);
            }
            else
            {
                Debug.LogError($"预制体{prefab.name}的对象池类型不匹配，无法回收");
                Object.Destroy(obj);
            }
        }
        else
        {
            Debug.LogWarning($"未找到预制体{prefab.name}的对象池，直接销毁");
            Object.Destroy(obj);
        }
    }
    public Dictionary<GameObject, object> GetPoolDict()
    {
        return _poolDict;
    }

    /// <summary>
    /// 获取当前活跃的物体数量
    /// </summary>
    public int GetActiveCount(GameObject prefab)
    {
        if (_poolDict.TryGetValue(prefab, out var poolObj))
        {
            var pool = poolObj as ObjectPool<MonoBehaviour>;
            return pool?.GetActiveCount() ?? 0;
        }
        return 0;
    }
    public int GetCacheCount(GameObject prefab)
    {
        if (_poolDict.TryGetValue(prefab, out var poolObj))
        {
            var pool = poolObj as ObjectPool<MonoBehaviour>;
            return pool?.GetCacheCount() ?? 0;
        }
        return 0;
    }

    // 获取指定预制体的复用命中率
    public float GetHitRate(GameObject prefab)
    {
        if (_poolDict.TryGetValue(prefab, out var poolObj))
        {
            var pool = poolObj as ObjectPool<MonoBehaviour>;
            return pool?.GetHitRate() ?? 0;
        }
        return 0;
    }
}