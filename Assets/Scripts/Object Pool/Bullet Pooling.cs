using UnityEngine;
using UnityEngine.Pool; 
using System.Collections.Generic;
public class BulletPooling : MonoBehaviour
{
    public GameObject bulletPrefab;
    private Queue<GameObject> bulletPool = new Queue<GameObject>();
    public GameObject GetBullet()
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            // if (bullet == null) return GetBullet(); // In case the bullet was destroyed
            bullet.SetActive(true);
            return bullet;
        }
        else
        {
            return Instantiate(bulletPrefab);
        }
    }
    public void ReturnBullet(GameObject bullet)
   { // {if (bullet == null) return;
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }

    // public static BulletPooling Instance { get; private set; }
    // [SerializeField] private BulletController _bulletPrefab;
    // [SerializeField] private int _defaultCapacity = 2;
    // [SerializeField] private int _maxPoolSize = 2;
    // private IObjectPool<BulletController> _pool;
    // private void Awake()
    // {
    //     if (Instance != null && Instance != this)
    //     {
    //         Destroy(gameObject);
    //     }
    //     else
    //     {
    //         Instance = this;
    //         DontDestroyOnLoad(gameObject);
    //     }

    //     _pool = new ObjectPool<BulletController>(
    //         createFunc: CreateBullet,          // Hàm chạy khi pool thiếu đạn, cần tạo mới
    //         actionOnGet: OnGetBullet,          // Hàm chạy khi lấy đạn RA khỏi pool
    //         actionOnRelease: OnReleaseBullet,  // Hàm chạy khi TRẢ đạn VỀ pool
    //         actionOnDestroy: OnDestroyBullet,  // Hàm chạy nếu số lượng vượt quá maxPoolSize
    //         collectionCheck: true,             // Check lỗi nếu lỡ tay release 2 lần
    //         defaultCapacity: _defaultCapacity,
    //         maxSize: _maxPoolSize
    //     );
    // }
    // private BulletController CreateBullet(){
    //     BulletController bullet = Instantiate(_bulletPrefab);
    //     bullet.gameObject.SetActive(false);
    //     return bullet;
    // }
    // private void OnGetBullet(BulletController bullet)
    // {
    //     bullet.gameObject.SetActive(true);
    // }
    // private void OnReleaseBullet(BulletController bullet)
    // {
    //     bullet.gameObject.SetActive(false);
    // }
    // private void OnDestroyBullet(BulletController bullet)
    // {
    //     Destroy(bullet.gameObject);
    // }
    // // Start is called once before the first execution of Update after the MonoBehaviour is created
    // void Start()
    // {
        
    // }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }
}
