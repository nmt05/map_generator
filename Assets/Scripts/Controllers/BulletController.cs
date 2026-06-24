using UnityEngine;

public class BulletController : MonoBehaviour
{
    public GameObject speedBuffPrefab;
    public GameObject hitEffectPrefab;
    private AudioClip hitClip;
    private AudioSource audioSource;
    void Start()
    {
        GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
        audioSource = gameManager.GetComponent<AudioSource>(); // Lấy AudioSource từ GameObject này
        // hitClip = gameManager.GetHitAudio();
    }
    void Update()
    {
        transform.Translate(Vector3.forward * 10f * Time.deltaTime);

    }

    void OnTriggerEnter(Collider other)
    {
        MonsterController target = other.GetComponent<MonsterController>();
        if (target != null)
        {
            int damage = 50;
            target.TakeDamage(damage);
            // target.Die();
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            if (hitClip != null)
            {
                audioSource.PlayOneShot(hitClip);
            }
            HideBullet();

        }
        Monster2Controller target2 = other.GetComponent<Monster2Controller>();
        if (target2 != null)
        {
            int damage = 50;
            target2.TakeDamage(damage);
            // target2.Die();
            if (hitEffectPrefab != null)
            {
                GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(hitEffect, 0.5f);   
                bool rate = Random.Range(0f, 1f) <= 0.1f; // 10% chance
                if (rate)
                {
                    // Vector3 spawnPosition = new Vector3(transform.position.x, 0.65f, Random.Range(-10f, 10f));
                    Vector3 spawnPosition = new Vector3(transform.position.x, 0, transform.position.z);
                    Instantiate(speedBuffPrefab, spawnPosition, Quaternion.identity);
                }
            }
            if (hitClip != null)
            {
                audioSource.PlayOneShot(hitClip);
            }
            HideBullet();

        }
        float randomChance = Random.Range(0f, 1f);
    
        // if (randomChance <= 0.1f)
        // {
        //     Vector3 spawnPosition = new Vector3(Random.Range(-10f, 10f), transform.position.y, Random.Range(-10f, 10f));
        //     Instantiate(speedBuffPrefab, spawnPosition, Quaternion.identity);
        // }
    }
    private void HideBullet()
{

    {
        // Phòng hờ nếu không tìm thấy Pool thì tự ẩn đi
        gameObject.SetActive(false);
    }
}}
