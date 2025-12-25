using UnityEngine;

/// <summary>
/// Player的WASD移动脚本（单例控制+带速度控制+碰撞检测）
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    private Rigidbody _rb;
    // 单例：记录当前可操控的Player
    private static PlayerMovement _currentControllablePlayer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        // 初始时禁用移动（等待成为当前可操控Player）
        enabled = false;
    }

    private void OnEnable()
    {
        // 当脚本启用时，设置为当前可操控Player，并禁用之前的Player
        if (_currentControllablePlayer != null && _currentControllablePlayer != this)
        {
            _currentControllablePlayer.enabled = false;
        }
        _currentControllablePlayer = this;
    }

    // 补充：外部获取当前可操控Player（可选）
    public static PlayerMovement GetCurrentPlayer()
    {
        return _currentControllablePlayer;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDir = new Vector3(horizontal, 0, vertical).normalized;
        if (Camera.main != null)
        {
            moveDir = Camera.main.transform.TransformDirection(moveDir);
            moveDir.y = 0;
        }

        _rb.velocity = moveDir * _moveSpeed;
    }

    // 提供外部方法：启用当前Player的操控
    public static void SetCurrentPlayer(GameObject playerObj)
    {
        PlayerMovement movement = playerObj.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = true;
        }
    }
}