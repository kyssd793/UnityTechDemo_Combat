using UnityEngine;

public class SpawnTest : MonoBehaviour
{
    // 在Inspector中拖入Player预制体
    [SerializeField] private GameObject _playerPrefab;
    // 在Inspector中拖入NPC预制体
    [SerializeField] private GameObject _npcPrefab;

    // 箱庭边界（匹配标准化场景：X/Z ∈ [-10,10]，留1单位余量）
    [SerializeField] private float _maxX = 9f;
    [SerializeField] private float _maxZ = 9f;
    [SerializeField] private float _minY = 0.5f;

    // 存储生成的物体（用于测试回收）
    private GameObject _lastSpawnedObj;


    private void Update()
    {
        // 按P键生成Player（限制在箱庭内）
        if (Input.GetKeyDown(KeyCode.P))
        {
            _lastSpawnedObj = SpawnObject(_playerPrefab);
        }

        // 按N键生成NPC（限制在箱庭内）
        if (Input.GetKeyDown(KeyCode.N))
        {
            _lastSpawnedObj = SpawnObject(_npcPrefab);
        }

        // 按R键回收最后生成的物体
        if (Input.GetKeyDown(KeyCode.R) && _lastSpawnedObj != null)
        {
            RecycleObject(_lastSpawnedObj);
            _lastSpawnedObj = null;
        }
    }


    /// <summary>
    /// 封装生成物体的逻辑（限制位置在箱庭内）
    /// </summary>
    private GameObject SpawnObject(GameObject prefab)
    {
        GameObject obj = PoolManager.Instance.Spawn(prefab);
        // 生成位置：X∈[-_maxX, _maxX]，Y=_minY，Z∈[-_maxZ, _maxZ]
        Vector3 randomPos = new Vector3(
            Random.Range(-_maxX, _maxX),
            _minY,
            Random.Range(-_maxZ, _maxZ)
        );
        obj.transform.position = randomPos;
        return obj;
    }


    /// <summary>
    /// 封装回收物体的逻辑（根据Tag匹配预制体）
    /// </summary>
    private void RecycleObject(GameObject obj)
    {
        if (obj.CompareTag("Player"))
        {
            PoolManager.Instance.Despawn(_playerPrefab, obj);
        }
        else if (obj.CompareTag("NPC"))
        {
            PoolManager.Instance.Despawn(_npcPrefab, obj);
        }
        else
        {
            Debug.LogWarning($"未知Tag的物体：{obj.name}，直接销毁");
            Object.Destroy(obj);
        }
    }
}