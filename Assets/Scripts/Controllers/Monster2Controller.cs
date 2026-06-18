using System;
using UnityEngine;
using System.Collections;
public class Monster2Controller : MonoBehaviour
{
    [SerializeField] private BaseUnitConfig _config;
    // private BaseUnitConfig _config;
    public BaseUnitConfig Config => _config;
    private int _currentHealth;
    private int _currentDamage;
    private int _currentSpeed;

    private GameObject _unitObject;
    private BirdPooling birdPool;
    private GameObject bird;
    private GameManager gameManager;
    private AnimationHandle animationHandle;

    private AudioSource audioSource;
    private AudioClip killedClip;

    public static Action DieEvent;

    void Start()
    {

        gameManager = GameManager.instance;

        audioSource = gameManager.GetComponent<AudioSource>(); // Lấy AudioSource từ GameObject này
        killedClip = gameManager.GetKilledAudio();

        birdPool = GameObject.Find("BirdPool").GetComponent<BirdPooling>();
        animationHandle = GetComponent<AnimationHandle>();
        // bird = birdPool.Getbird(); 
        Initialize(_config);
        // Find the player ONCE at the very beginning of the game
        Target = GameObject.FindWithTag("Player");


    }
    public GameObject UnitObject
    {
        get => _unitObject;
        set => _unitObject = value;
    }
    public void Initialize(BaseUnitConfig config)
    {
        _config = config;

        _currentHealth = _config.BaseHealth;
        _currentDamage = _config.BaseDamage;
        _currentSpeed = _config.MoveSpeed;
        animationHandle.SetFlag();

    }
    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;

        // _unitObject.GetComponent<Renderer>().material.color = Color.Lerp(Color.black, _unitObject.GetComponent<Renderer>().material.color, (float)_currentHealth / _config.BaseHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    public void Die()
    {
        if (killedClip != null)
        {
            audioSource.PlayOneShot(killedClip);
        }

        StartCoroutine(DieCoroutine());
        DieEvent?.Invoke();
        Debug.Log("Die");
        // Destroy(gameObject);       // UnitPool.Instance.ReleaseHolder(this);
    }
    public GameObject Target;
    


    void Update()
    {
        
        // Only try to move if we actually found a target
        if (Target != null)
        {
            Move();
            RotateParentTowardsPlayer();
        }
    }

    void Move()
    {
        Vector3 currentPos = transform.parent != null ? transform.parent.position : transform.position;

        Vector3 direction = (Target.transform.position - currentPos).normalized;
        Vector3 direction_convert = new Vector3(direction.x, 0, direction.z);
        
        if (transform.parent != null)
        {
            transform.parent.position += direction_convert * _currentSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += direction_convert * _currentSpeed * Time.deltaTime;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem quái có chạm phải Object có Tag là "        Player" không
        if (other.CompareTag("Player"))
        {
            // Gọi hàm xử thua từ GameManager
            if (gameManager != null)
            {
                gameManager.Defeat();
            }
            // Debug.Log("Game Over! Player was hit by " + gameObject.name);
        }
    }
    IEnumerator DieCoroutine()
    {
        animationHandle.Die();
        // Play the death animation
        // Wait for the animation to finish (assuming it takes 2 seconds)
        yield return new WaitForSeconds(2f);
        // Animator anim = GetComponent<Animator>();
        // if (anim != null) anim.enabled = false;

        // Destroy the monster after the animation
        birdPool.Returnbird(gameObject);
    }
    void RotateTowardsPlayer()
    {
        // 1. Tính toán hướng từ quái đến Player
        Vector3 direction = Target.transform.position - transform.position;

        // 2. KHÓA TRỤC Y: Triệt tiêu cao độ (Y = 0) để quái không bị ngửa lên/chúi xuống
        direction.y = 0;

        // Kiểm tra điều kiện an toàn phòng trường hợp quái và Player trùng vị trí (Vector rỗng)
        if (direction != Vector3.zero)
        {
            // 3. Tạo rotation mục tiêu dựa trên hướng đã triệt tiêu Y
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 4. Xoay mượt mà (Slerp) từ góc quay hiện tại sang góc quay mục tiêu
            // Thay số 5f bằng tốc độ xoay bạn muốn (càng to xoay càng nhanh)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }
    void RotateParentTowardsPlayer()
    {
        // 1. Xác định Object cần xoay (Nếu có cha thì xoay cha, không thì tự xoay chính nó)
        Transform objectToRotate = transform.parent != null ? transform.parent : transform;


        Vector3 direction = Target.transform.position - objectToRotate.position;


        direction.y = 0;


        if (direction != Vector3.zero)
        {
            // 4. Tạo góc quay mục tiêu nhìn về hướng Player
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation *= Quaternion.Euler(0, 180f, 0);
            // 5. Xoay mượt mà lớp cha từ góc hiện tại sang góc mục tiêu
            // Tốc độ xoay là 5f (bạn có thể tăng lên 10f, 15f nếu muốn quái quay mặt nhanh hơn)
            objectToRotate.rotation = Quaternion.Slerp(objectToRotate.rotation, targetRotation, 5f * Time.deltaTime);
        }
}}