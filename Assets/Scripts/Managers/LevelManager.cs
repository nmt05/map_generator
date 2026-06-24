using UnityEngine;
using System.Collections;
using System;
public class LevelManager : MonoBehaviour
{
    public static Action<int> LevelChanged;

    public int currentLevel = 1;

    private GameModel gameModel;
    private GameDatasetWrapper dataset;
    private int score;
    private GameData currentLevelData;
    private GameObject enemyPrefab;
    private GameObject playerPrefab;
    private GameObject player;
    private bool flag = false;
    private int count;
    private Coroutine spawnCoroutine;
    void Awake()
    {
        // DontDestroyOnLoad(gameObject);
        gameModel = new GameModel();
        dataset = gameModel.LoadData();

        Debug.Log("Loaded");
        Monster2Controller.DieEvent += ScorePlus;
        enemyPrefab = Resources.Load<GameObject>("Prefabs/Enemy/GullHolder");
        playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
        UISettings.PlayLoadClicked += save;

        if (UIMainMenu.PlayFromSave)
        {
            levelload();
        }
        else
        {
            GenerateLevel(currentLevel);
        }

    }
    private void save(){
        SaveGame();
    }
    private void levelload(){
        dataset = gameModel.LoadData();
        currentLevel = gameModel.LoadSavedLevel();
        Debug.Log("Loaded progress at Level: " + currentLevel);
        GenerateLevel(currentLevel);
    }
    void OnDestroy()
    {
        Monster2Controller.DieEvent -= ScorePlus;
        UISettings.PlayLoadClicked -= save;
    }

    void GenerateLevel(int levelId)
    {
        ClearCurrentLevel();

        GameData levelData = dataset.LevelsDataset.Find(data => data.Level == levelId);
        if (levelData == null) return;

        count = levelData.NumberOfEnemies;
        LevelChanged?.Invoke(levelId);



        if (!flag && playerPrefab != null)
        {
            player = Instantiate(playerPrefab, new Vector3(0, 6.5f, 0), Quaternion.identity);
            flag = true;
        }
        if (levelData != null && enemyPrefab != null)
        {
            // GỌI COROUTINE Ở ĐÂY
            spawnCoroutine = StartCoroutine(SpawnEnemiesWithDelay(levelData, enemyPrefab));
        }
    }

    IEnumerator SpawnEnemiesWithDelay(GameData levelData, GameObject enemyPrefab)
    {
        for (int i = 0; i < levelData.NumberOfEnemies; i++)
        {
            Debug.Log("summoned");
            Vector3 randomPosition = new Vector3(UnityEngine.Random.Range(-20f, 20f), 8f, UnityEngine.Random.Range(-20f, 20f));
            Instantiate(enemyPrefab, randomPosition, Quaternion.identity);

            // Chờ đúng 1 giây rồi mới chạy tiếp vòng lặp tiếp theo
            yield return new WaitForSeconds(1f); 
        }
    }
    private void ScorePlus(){
        score++;
        if (score >= count)
        {
            NextLevel();
        }
    }
    void NextLevel()
        {        
            currentLevel++; 
            if (currentLevel <= dataset.LevelsDataset.Count)
            {
                // SaveGame();
                GenerateLevel(currentLevel);
            }
            // else 
            // {
            //     ResetGameProgress();
            // }
        }
    public void SaveGame()
    {
        gameModel.SaveCurrentLevel(currentLevel);
        dataset = gameModel.LoadData();
    }
    public void ResetGameProgress()
    {
        currentLevel = 1;
        gameModel.SaveCurrentLevel(currentLevel);
        dataset = gameModel.LoadData();
        GenerateLevel(currentLevel);
    }
    private void ClearCurrentLevel()
    {
        // 1. Dừng đẻ quái
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        // 2. Xóa Player cũ
        if (player != null)
        {
            Destroy(player);
        }
        flag = false;

        // 3. Xóa toàn bộ quái vật đang có trên Map
        // LƯU Ý: Bạn cần vào Unity, chọn Prefab "GullHolder" và đổi Tag của nó thành "Enemy"
        GameObject[] enemiesInScene = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemiesInScene)
        {
            Destroy(enemy);
        }

        score = 0;
    }
}