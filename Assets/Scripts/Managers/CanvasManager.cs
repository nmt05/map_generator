using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    // Tạo Singleton Instance đúng kiểu dữ liệu CanvasManager
    public static CanvasManager Instance { get; private set; }
    [SerializeField] public Transform mainCanvasmanager;
    private string uiPrefabPath = "Prefabs/UI/";

    private void Awake()
    {
        // Khởi tạo Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public T LoadUIPrefabs<T>(string name) where T : MonoBehaviour
    {
        string fullPath = uiPrefabPath + name;
        GameObject prefab = Resources.Load<GameObject>(fullPath);
        
        if (prefab == null)
        {
            Debug.LogError($"[CanvasManager] Không tìm thấy Prefab tại: Resources/{fullPath}");
            return null;
        }

        GameObject spawnedGo = Instantiate(prefab);
        T uiInstance = spawnedGo.GetComponent<T>();
        
        if (uiInstance == null)
        {
            Debug.LogError($"[CanvasManager] Prefab '{name}' không chứa script component '{typeof(T).Name}'!");
            Destroy(spawnedGo);
            return null;
        }

        return uiInstance;
    }
    public void AddUI(MonoBehaviour ui){
        if(ui == null) return;

        ui.transform.SetParent(mainCanvasmanager, false);
        ui.gameObject.SetActive(true);
    }
    public void RemoveUI(MonoBehaviour ui){
        if(ui == null) return;

        ui.gameObject.SetActive(false);
    }
}
