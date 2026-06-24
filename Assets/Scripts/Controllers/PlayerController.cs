using UnityEngine;
using System;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private BasicInput _inputs;
    private GameObject mainCamera;
    private VoxelPlacementRuntime _voxelManager; // Tham chiếu đến bộ quản lý Voxel để ké hàm Raycast

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] public float jumpForce = 15f; 
    [SerializeField] public float fallMultiplier = 2.5f; 
    [SerializeField] public float lowJumpMultiplier = 2f; 
    private Vector2 _inputsMove;
    private bool _isGrounded;

    [Header("Camera and Rotation Settings")]
    [SerializeField] private float mouseSensitivity = 15f;
    private Vector2 _inputsLook;
    private float _xRotation = 0f;

    private Rigidbody _rb;

    private void Awake()
    {
        _inputs = new BasicInput();
        mainCamera = GameObject.Find("Main Camera");
        _rb = GetComponent<Rigidbody>();
        
        // Tìm kiếm component VoxelPlacementRuntime đang nằm trong Scene
        _voxelManager = FindFirstObjectByType<VoxelPlacementRuntime>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable() => _inputs.Enable();
    private void OnDisable() => _inputs.Disable();

    void Start()
    {
        transform.rotation = Quaternion.Euler(0, 180, 0);
        _inputs.Movement.Jump.performed += ctx => Jump();
        _inputs.Movement.Tele.performed += ctx => Tele();
        _inputs.Movement.Dash.performed += ctx => Dash();
        _inputs.Action.Esc.performed += ctx => Esc();
        _inputs.Movement.Home.performed += ctx => Home();
    }

    void Update()
    {   
        _inputsMove = _inputs.Movement.Move.ReadValue<Vector2>();
        _inputsLook = _inputs.Movement.Look.ReadValue<Vector2>();

        float mouseX = _inputsLook.x * mouseSensitivity * Time.deltaTime;
        float mouseY = _inputsLook.y * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        if (mainCamera != null)
        {
            mainCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        }

        Vector3 moveDir = transform.forward * _inputsMove.y + transform.right * _inputsMove.x;
        transform.Translate(moveDir * movementSpeed * Time.deltaTime, Space.World);

        // ĐO ĐẤT KHÔNG DÙNG COLLIDER:
        // Bắn 1 tia ngắn từ chân nhân vật xuống để check xem dưới chân có nằm trong Dictionary _runtimeBlocks không
        if (_voxelManager != null)
        {
            // Bắn một tia từ đáy nhân vật hướng thẳng xuống dưới
            Ray downRay = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
            
            // Mượn thuật toán DDA tầm ngắn (0.5m) để quét đất ngầm
            _isGrounded = _voxelManager.VoxelRaycastDDA(downRay, 0.5f, out _, out _);
        }
    }

    // void FixedUpdate()
    // {
    //     // Hệ thống tăng tốc độ rơi tự động (Gravity Modification) giúp nhảy đầm hơn
    //     if (_rb.linearVelocity.y < 0)
    //     {
    //         _rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
    //     }
    //     else if (_rb.linearVelocity.y > 0 && !_inputs.Movement.Jump.IsPressed())
    //     {
    //         _rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
    //     }
    // }
    public void Home()
    {
        transform.position = new Vector3(0,2,32);
        transform.eulerAngles = new Vector3(0f, 180f, 0f);

    }
    public void Jump()
    {
        transform.position += new Vector3(0, 1, 0);
    }

    public void Esc()
    {
        // Code xử lý nút Esc nếu cần
    }

    public void Dash()
    {
        // Code xử lý Dash của bạn
        transform.position += new Vector3(0, -1, 0);
    }

    public void Tele()
    {
        if (mainCamera == null) mainCamera = GameObject.Find("Main Camera");
        if (_voxelManager == null) _voxelManager = FindFirstObjectByType<VoxelPlacementRuntime>();

        if (_voxelManager == null)
        {
            Debug.LogError("Không tìm thấy VoxelPlacementRuntime trong Scene để thực hiện Tele!");
            return;
        }

        // 1. Tạo tia bắn từ tâm màn hình thông qua Camera
        Camera camComponent = mainCamera.GetComponent<Camera>();
        Ray ray = camComponent.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0)); 

        // 2. MƯỢN HÀM: Gọi thuật toán DDA từ bên file VoxelManager sang để dò ô
        if (_voxelManager.VoxelRaycastDDA(ray, 1000f, out Vector3Int hoverCell, out Vector3Int hitNormal))
        {
            Vector3Int targetCell = hoverCell + hitNormal;

            // 3. MƯỢN HÀM: Đổi tọa độ ô ra vị trí World
            Vector3 targetPosition = _voxelManager.CellToWorld(targetCell);

            // Cộng offset độ cao (cellSize / 2) kết hợp thêm chiều cao nhân vật để đứng ngay ngắn trên mặt block
            targetPosition += new Vector3(0, 1f, 0); 

            // Triệt tiêu quán tính cũ trước khi dịch chuyển tức thời
            _rb.linearVelocity = Vector3.zero;

            transform.position = targetPosition;
            
            Debug.Log($"<color=yellow>Teleport thành công đến ô:</color> {targetCell}");
        }
        else
        {
            Debug.LogWarning("Không tìm thấy khối block nào trong tầm ngắm để Teleport!");
        }
    }
}