using UnityEngine;
using UnityEngine.Pool;
using System.Collections;
using System.Collections.Generic;
public class BirdPooling : MonoBehaviour
{
    public GameObject GULLPrefab;
    private Queue<GameObject> birdPool = new Queue<GameObject>();
    // public GameObject Getbird()
    // {
    //     // if (birdPool.Count > 0)
    //     // {
    //     //     GameObject bird = birdPool.Dequeue();
    //     //     // if (bird == null) return Getbird(); // In case the bird was destroyed
    //     //     bird.SetActive(true);
    //     //     return bird;
    //     // }
    //     // else
    //     // {
    //     //     return Instantiate(GULLPrefab);
    //     // }
    // }


    public void Returnbird(GameObject bird)
    {
        // bird.SetActive(false);
        // birdPool.Enqueue(bird);
    }
    // void Start()
    // {
    //     // Pre-instantiate a few birds to populate the pool
    //     for (int i = 0; i < 5; i++)
    //     {
    //         GameObject bird = Instantiate(GULLPrefab);
    //         bird.SetActive(false);
    //         birdPool.Enqueue(bird);
    //     }
    // }
    // void Start()
    // {
    //     // 1. Khởi tạo sẵn một vài bird trong pool
    //     for (int i = 0; i < 5; i++)
    //     {
    //         GameObject bird = Instantiate(GULLPrefab);
    //         bird.SetActive(false);
    //         birdPool.Enqueue(bird);
    //     }

    //     // 2. CHỈ GỌI COROUTINE MỘT LẦN DUY NHẤT Ở ĐÂY
    //     StartCoroutine(SpawnbirdsRoutine());
    // }
    // IEnumerator SpawnbirdsRoutine()
    // {
    //     // while (true)
    //     // {
    //     //     yield return new WaitForSeconds(2f); // Spawn a bird every 2 seconds
    //     //     GameObject bird = Getbird();
    //     //     bird.transform.position = new Vector3(Random.Range(-0.5f, 0.5f), 2, Random.Range(-0.5f, 0.5f)); // Random spawn position
    //     // }
    // }
}