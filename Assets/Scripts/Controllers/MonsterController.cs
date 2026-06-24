using UnityEngine;
using System.Collections;
public class MonsterController : MonoBehaviour
{
    [SerializeField] private BaseUnitConfig _config;
    // private BaseUnitConfig _config;
    public BaseUnitConfig Config => _config;
    private int _currentHealth;
    private int _currentDamage;
    private int _currentSpeed;

    private GameObject _unitObject;
    private SpiderPooling spiderPool;
    private GameObject spider;

    private GameManager gameManager;
    // public GameObject speedBuffPrefab;
    private AnimationHandle animationHandle;
        public GameObject Target; 

    private AudioSource audioSource;
    private AudioClip killedClip;
    void Start()
    {
        gameManager = GameManager.instance;
        audioSource = gameManager.GetComponent<AudioSource>(); // Lấy AudioSource từ GameObject này
        // killedClip = gameManager.GetKilledAudio();

        spiderPool = GameObject.Find("SpiderPool").GetComponent<SpiderPooling>();
        animationHandle = GetComponent<AnimationHandle>();
        // spider = spiderPool.GetSpider(); 
        Initialize(_config);
        // Find the player ONCE at the very beginning of the game
        Target = GameObject.FindWithTag("Player");

        // Safety check in case you forgot to tag your player
        if (Target == null)
        {
            Debug.LogError($"Player target not found on {gameObject.name}! Make sure your Player object is tagged 'Player'.");
        }

    }
    public GameObject UnitObject
    {
        get => _unitObject;
        set => _unitObject = value;
    }


    public void Initialize(BaseUnitConfig config)
    {
        _config = config;
        Debug.Log($"Initializing {gameObject.name} with config: Health={_config.BaseHealth}, Damage={_config.BaseDamage}, Speed={_config.MoveSpeed}");
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
                    if (killedClip != null)
        {
            audioSource.PlayOneShot(killedClip);
        }
            Die();
        }
    }
    public void Die()
    {
        // UnitPool.Instance.ReleaseHolder(this);
        StartCoroutine(DieCoroutine());
    }

    


    void Update()
    {
        // Only try to move if we actually found a target
        if (Target != null)
        {
            RotateParentTowardsPlayer();
            Move();
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
        // Kiểm tra xem quái có chạm phải Object có Tag là "Player" không
        if (other.CompareTag("Player"))
        {
            // Gọi hàm xử thua từ GameManager
            if (gameManager != null)
            {
                gameManager.Defeat();
            }
            Debug.Log("Game Over! Player was hit by " + gameObject.name);
        }
    }
    IEnumerator DieCoroutine()
    {
        // float randomChance = Random.Range(0f, 1f);
    
        // if (randomChance <= 0.5f)
        // {
        //     Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        //     Instantiate(speedBuffPrefab, spawnPosition, Quaternion.identity);
        // }
        // Destroy(gameObject); 
        animationHandle.Die();
        yield return new WaitForSeconds(0.5f); // Wait for 1 second before destroying the object
        spiderPool.ReturnSpider(gameObject);
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