using UnityEngine;

public class SpeedBuff : MonoBehaviour
{
    [SerializeField] private float speedBoostAmount = 3f; // Lượng tốc độ cộng thêm
    [SerializeField] private float duration = 5f;         // Thời gian tác dụng (giây)

    void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem có phải người chơi nhặt được không
        // if (other.CompareTag("Player"))
        // {
        //     // Tìm script di chuyển trên người chơi (trong code của bạn là ExampleInput2)
        //     PlayerController playerMovement = other.GetComponent<PlayerController>();
            
        //     if (playerMovement != null)
        //     {
        //         // Gọi một Coroutine trên Player để tăng tốc rồi tự giảm sau vài giây
        //         playerMovement.StartCoroutine(playerMovement.ApplySpeedBoost(speedBoostAmount, duration));
        //     }

        //     // Hủy vật phẩm buff này đi sau khi ăn
        //     Destroy(gameObject);

        // }
    }
}