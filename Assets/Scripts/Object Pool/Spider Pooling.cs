using UnityEngine;
using UnityEngine.Pool;
using System.Collections;
using System.Collections.Generic;
public class SpiderPooling : MonoBehaviour
{
    public GameObject spiderPrefab;
    private Queue<GameObject> spiderPool = new Queue<GameObject>();
    public GameObject GetSpider()
    {
        if (spiderPool.Count > 0)
        {
            GameObject spider = spiderPool.Dequeue();
            // if (spider == null) return GetSpider(); // In case the spider was destroyed
            spider.SetActive(true);
            return spider;
        }
        else
        {
            return Instantiate(spiderPrefab);
        }
    }


    public void ReturnSpider(GameObject spider)
    {
        spider.SetActive(false);
        spiderPool.Enqueue(spider);
    }
    // void Start()
    // {
    //     // Pre-instantiate a few spiders to populate the pool
    //     for (int i = 0; i < 5; i++)
    //     {
    //         GameObject spider = Instantiate(spiderPrefab);
    //         spider.SetActive(false);
    //         spiderPool.Enqueue(spider);
    //     }
    // }
    void Start()
    {
        // 1. Khởi tạo sẵn một vài spider trong pool
        for (int i = 0; i < 5; i++)
        {
            GameObject spider = Instantiate(spiderPrefab);
            spider.SetActive(false);
            spiderPool.Enqueue(spider);
        }

        // 2. CHỈ GỌI COROUTINE MỘT LẦN DUY NHẤT Ở ĐÂY
        StartCoroutine(SpawnSpidersRoutine());
    }
    IEnumerator SpawnSpidersRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f); // Spawn a spider every 3 seconds
            GameObject spider = GetSpider();
            spider.transform.position = new Vector3(Random.Range(-3, 3), 0.5f, Random.Range(-3, 3)); // Random spawn position
        }
    }
}