using UnityEngine;
using System;
using System.Collections;
public class PlayerController : MonoBehaviour
{
    private BasicInput _inputs;
    GameObject mainCamera;

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 5f;
    private Vector2 _inputsMove;
    private bool _isGrounded;

    [Header("Camera and Rotation Settings")]
    [SerializeField] private float mouseSensitivity = 15f;
    private Vector2 _inputsLook;
    private float _xRotation = 0f;


    public GameObject testPrefab; // Tham chiếu đến prefab của bạn
    private AudioSource audioSource; 
    private AudioClip dashClip;
    private AudioClip jumpClip;
    private AudioClip shootClip;
    public BulletPooling bulletPool;
    public GameObject DashEffectPrefab; // Tham chiếu đến prefab của bạn
    public static Action EscTrigger;
    private void Awake()
    {
        _inputs = new BasicInput();
        mainCamera = GameObject.Find("Main Camera");

        audioSource = GameManager.instance.GetComponent<AudioSource>(); // Lấy AudioSource từ GameObject này
        dashClip = GameManager.instance.GetDashAudio();
        jumpClip = GameManager.instance.GetJumpAudio();
        shootClip = GameManager.instance.GetShootAudio();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable() => _inputs.Enable();
    private void OnDisable() => _inputs.Disable();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _inputs.Movement.Jump.performed += ctx => Jump();
        _inputs.Movement.Shoot.performed += ctx => Attack();
        _inputs.Movement.ChangeCam.performed += ctx =>  mainCamera.GetComponent<CameraControl>().change_camera_mode();
        _inputs.Movement.Dash.performed += ctx => Dash();
        _inputs.Action.Esc.performed += ctx => Esc();
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

        if (mainCamera != null){mainCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);}


        Vector3 moveDir = transform.forward * _inputsMove.y + transform.right * _inputsMove.x;
        transform.Translate(moveDir * movementSpeed * Time.deltaTime, Space.World);

        // 4. KIỂM TRA MẶT ĐẤT (RAYCAST)
        int layerMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Water"));
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.75f, layerMask);

        // transform.Translate(_inputsMove.x * Time.deltaTime * movementSpeed, 0, _inputsMove.y * Time.deltaTime * movementSpeed);
        // if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitt, 0.75f, 1 <<LayerMask.NameToLayer("Water")))
        // {
        //     // GameManager.instance.Victory();
        //     _isGrounded = true;
        // }
        // if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.75f, 1 <<LayerMask.NameToLayer("Default")))
        // {
        //     _isGrounded = true;
        // }
        // else
        // {
        //     _isGrounded = false;
        // }
    }

    public void Jump()
    {
        if (_isGrounded)
        {
            if(jumpClip != null)
            {
                audioSource.PlayOneShot(jumpClip);
            }
            GetComponent<Rigidbody>().AddForce(Vector3.up * 5, ForceMode.Impulse);
        }
        
    }
    public void Esc(){
        EscTrigger?.Invoke();
        Cursor.lockState = CursorLockMode.None; // Mở khóa chuột
        Cursor.visible = true;
    }
    public void Dash()
    {
        if (_isGrounded)
        {
            if(DashEffectPrefab != null)
            {
                GameObject dashEffect = Instantiate(DashEffectPrefab, transform.position, Quaternion.identity);
                Destroy(dashEffect, 0.5f); // Destroy the effect after 1 second
            }
            if(dashClip != null)
            {
                audioSource.PlayOneShot(dashClip);
            }
            transform.position += transform.forward * 10f;
        }
    }
    private void Attack()
        {
            if(shootClip != null)
            {
                audioSource.PlayOneShot(shootClip);
            }
            // Debug.Log("Shoot");
            GameObject bullet = bulletPool.GetBullet();
            // Debug.Log($"Bullet: {bullet}");
            bullet.transform.position = transform.position + transform.forward; // Spawn bullet in front of the player
            bullet.transform.rotation = transform.rotation; // Align bullet with player's facing direction
            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            // return bullet;
            StartCoroutine(DeactivateBullet(bullet, 2f)); // Deactivate bullet after 2 seconds
        }
    IEnumerator DeactivateBullet(GameObject bullet, float delay= 2f)
    {
        // if (bullet == null) yield break;
        yield return new WaitForSeconds(delay);
        bulletPool.ReturnBullet(bullet);
    }
    // Thêm đoạn này vào bất kỳ đâu bên trong class ExampleInput2 của bạn
    public System.Collections.IEnumerator ApplySpeedBoost(float boostAmount, float duration)
    {
        // 1. Cộng thêm tốc độ chạy
        movementSpeed += boostAmount;
        // Debug.Log($"Đã  buff. Tốc độ: {movementSpeed}");

        // 2. Chờ hết thời gian tác dụng của Buff
        yield return new WaitForSeconds(duration);

        // 3. Trả tốc độ chạy về ban đầu
        movementSpeed -= boostAmount;
        // Debug.Log($"Hết  buff. Tốc độ: {movementSpeed}");
    }
    public GameObject testpaticiate(){
        GameObject test = Instantiate(testPrefab, transform.position, Quaternion.identity);
        // gameManager.audioSource = gameManager.GetComponent<AudioSource>();
        // gameManager.victoryClip = gameManager.GetComponent<AudioSource>().clip;
        return test;
    }

}
