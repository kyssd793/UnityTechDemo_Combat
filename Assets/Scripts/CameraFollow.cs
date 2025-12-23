using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // 单例：全局唯一的摄像机脚本
    public static CameraFollow Instance;

    // 初始俯视位置（游戏开始时的位置）
    [SerializeField] private Vector3 _initialPosition = new Vector3(0, 12, -18);
    [SerializeField] private Vector3 _initialRotation = new Vector3(35, 0, 0);

    // 跟随Player的偏移量
    [SerializeField] private Vector3 _followOffset = new Vector3(0, 6, -8);
    [SerializeField] private float _followSmoothSpeed = 0.1f;

    private Transform _targetPlayer;
    private Vector3 _smoothVelocity;

    private void Awake()
    {
        Instance = this; // 初始化单例
    }

    private void Start()
    {
        // 游戏开始时，摄像机回到初始位置
        ResetToInitialView();
    }

    private void LateUpdate()
    {
        if (_targetPlayer != null)
        {
            FollowTarget(); // 有Player时跟随
        }
        else
        {
            ResetToInitialView(); // 无Player时保持初始位置
        }
    }

    // 跟随Player的逻辑
    private void FollowTarget()
    {
        Vector3 targetPosition = _targetPlayer.position + _followOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _smoothVelocity, _followSmoothSpeed);
        transform.LookAt(new Vector3(_targetPlayer.position.x, 0, _targetPlayer.position.z));
    }

    // 重置到初始俯视位置
    public void ResetToInitialView()
    {
        transform.position = _initialPosition;
        transform.rotation = Quaternion.Euler(_initialRotation);
    }

    // 外部调用：设置跟随的Player
    public void SetTargetPlayer(Transform playerTransform)
    {
        _targetPlayer = playerTransform;
    }

    // 外部调用：清除跟随目标
    public void ClearTargetPlayer()
    {
        _targetPlayer = null;
    }
}