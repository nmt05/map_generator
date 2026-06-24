using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class VoxelPlacementRuntime : MonoBehaviour
{
    public enum BuildMode { Place, Eraser }

    [Header("Settings")]
    [SerializeField] private float cellSize = 2f;
    [SerializeField] private float maxBuildDistance = 20f;
    [SerializeField] private LayerMask mapLayer;
    [SerializeField] private string mapParentName = "Map";

    [Header("Current Selection")]
    public GameObject selectedPrefab; 
    public BuildMode currentMode = BuildMode.Place;

    [Header("Ghost Visual (Optional)")]
    [SerializeField] private GameObject ghostCubePrefab; 

    private Transform _mapParent;
    private GameObject _ghostInstance;
    private Camera _mainCamera;

    private readonly Dictionary<Vector3Int, GameObject> _runtimeBlocks = new();

    void Start()
    {
        _mainCamera = Camera.main;

        GameObject mapObj = GameObject.Find(mapParentName);
        if (mapObj == null) mapObj = new GameObject(mapParentName);
        _mapParent = mapObj.transform;

        RebuildInitialBlockData();

        if (ghostCubePrefab != null)
        {
            _ghostInstance = Instantiate(ghostCubePrefab);
            _ghostInstance.transform.position += new Vector3(0, 1f, 0);
            if (_ghostInstance.TryGetComponent<Collider>(out var col)) col.enabled = false;
        }

        // Chỉ LoadGame() bằng JSON khi thực sự đang chơi game
        if (Application.isPlaying)
        {
            // LoadGame();
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

#if UNITY_EDITOR
    private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
    {
        if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            string savePath = System.IO.Path.Combine(Application.persistentDataPath, saveFileName);
            if (!System.IO.File.Exists(savePath)) return;

            string json = System.IO.File.ReadAllText(savePath);
            SaveDataWrapper wrapper = JsonUtility.FromJson<SaveDataWrapper>(json);

            GameObject mapObj = GameObject.Find(mapParentName);
            
            // THAY ĐỔI QUAN TRỌNG: Xóa sạch map cũ trong Editor trước khi vẽ đè map mới để tránh trùng lặp, kẹt hình
            if (mapObj != null)
            {
                UnityEditor.Undo.DestroyObjectImmediate(mapObj);
            }
            
            mapObj = new GameObject(mapParentName);
            UnityEditor.Undo.RegisterCreatedObjectUndo(mapObj, "Create Map Parent");

            foreach (BlockSaveData blockData in wrapper.allBlocks)
            {
                Vector3Int cell = new Vector3Int(blockData.x, blockData.y, blockData.z);
                Vector3 worldPos = CellToWorld(cell);

                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Resources/Prefabs/NewAssets/GreenMapTiles/{blockData.prefabName}.prefab");
                if (prefab == null) prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Resources/Prefabs/NewAssets/Animals/{blockData.prefabName}.prefab");
                if (prefab == null) prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Resources/Prefabs/NewAssets/Items/{blockData.prefabName}.prefab");
                if (prefab == null) prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Resources/Prefabs/NewAssets/PinkMapTiles/{blockData.prefabName}.prefab");

                if (prefab != null)
                {
                    GameObject editorBlock = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
                    editorBlock.transform.SetParent(mapObj.transform);
                    editorBlock.transform.position = worldPos;
                    editorBlock.transform.rotation = Quaternion.identity;

                    // Đảm bảo gán lại đúng layer Map cho Editor
                    editorBlock.layer = Mathf.RoundToInt(Mathf.Log(mapLayer.value, 2));
                }
            }
            
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("<color=yellow>Đã đồng bộ thành công dữ liệu Player xây vào Scene Editor thật!</color>");
        }
    }
#endif

    private void OnApplicationQuit()
    {
        // SaveGame();
        Debug.Log("Game đang tắt... Đã kích hoạt Auto Save!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) SaveGame();
        if (Input.GetKeyDown(KeyCode.K)) LoadGame();
        
        HandleVoxelLogic();
    }

    private void HandleVoxelLogic()
    {
        if (_mainCamera == null) return;

        Ray ray = _mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0)); 

        if (Physics.Raycast(ray, out RaycastHit hit, maxBuildDistance, mapLayer))
        {
            Vector3 hitBlockCenter = hit.point - hit.normal * 0.01f;
            Vector3Int hoverCell = WorldToCell(hitBlockCenter);

            Vector3Int hitNormal = Vector3Int.RoundToInt(hit.normal);
            Vector3Int placeCell = hoverCell + hitNormal;

            if (_ghostInstance != null)
            {
                _ghostInstance.SetActive(true);
                Vector3Int targetCell = (currentMode == BuildMode.Place) ? placeCell : hoverCell;
                _ghostInstance.transform.position = CellToWorld(targetCell);
            }

            if (Input.GetMouseButtonDown(0)) 
            {
                if (currentMode == BuildMode.Place) ExecutePlace(placeCell);
                else if (currentMode == BuildMode.Eraser) ExecuteErase(hoverCell);
            }

            if (Input.GetMouseButtonDown(2)) 
            {
                ExecutePipette(hoverCell);
            }
        }
        else
        {
            if (_ghostInstance != null) _ghostInstance.SetActive(false);
        }
    }

    private void ExecutePlace(Vector3Int cell)
    {
        if (selectedPrefab == null) return;
        if (_runtimeBlocks.ContainsKey(cell)) return; 

        GameObject newBlock = Instantiate(selectedPrefab, CellToWorld(cell), Quaternion.identity, _mapParent);
        newBlock.layer = Mathf.RoundToInt(Mathf.Log(mapLayer.value, 2)); 
        _runtimeBlocks[cell] = newBlock;
    }

    private void ExecuteErase(Vector3Int cell)
    {
        if (_runtimeBlocks.TryGetValue(cell, out GameObject target))
        {
            Destroy(target);
            _runtimeBlocks.Remove(cell);
        }
    }

    private void ExecutePipette(Vector3Int cell)
    {
        if (_runtimeBlocks.TryGetValue(cell, out GameObject target))
        {
            selectedPrefab = target; 
            currentMode = BuildMode.Place;
            Debug.Log($"Đã sao chép khối: {target.name}");
        }
    }

    private void RebuildInitialBlockData()
    {
        _runtimeBlocks.Clear();
        if (_mapParent == null) return;
        
        foreach (Transform child in _mapParent)
        {
            Vector3Int cell = WorldToCell(child.position);
            if (!_runtimeBlocks.ContainsKey(cell))
            {
                _runtimeBlocks.Add(cell, child.gameObject);
            }
        }
    }

    private Vector3Int WorldToCell(Vector3 world)
    {
        return new Vector3Int(
            Mathf.FloorToInt(world.x / cellSize),
            Mathf.FloorToInt(world.y / cellSize),
            Mathf.FloorToInt(world.z / cellSize)
        );
    }

    private Vector3 CellToWorld(Vector3Int cell)
    {
        return new Vector3(
            cell.x * cellSize,
            cell.y * cellSize,
            cell.z * cellSize
        );
    }

    [Header("Save/Load Settings")]
    [SerializeField] private string saveFileName = "voxel_map_save.json";

    public void SaveGame()
    {
        SaveDataWrapper wrapper = new SaveDataWrapper();

        foreach (var pair in _runtimeBlocks)
        {
            if (pair.Value == null) continue;
            Vector3Int cell = pair.Key;
            GameObject blockObj = pair.Value;

            BlockSaveData blockData = new BlockSaveData
            {
                x = cell.x,
                y = cell.y,
                z = cell.z,
                prefabName = blockObj.name.Replace("(Clone)", "").Trim()
            };

            wrapper.allBlocks.Add(blockData);
        }

        string json = JsonUtility.ToJson(wrapper, true);
        string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        File.WriteAllText(savePath, json);

        Debug.Log($"<color=green>Đã lưu game thành công tại:</color> {savePath}");
    }

    public void LoadGame()
    {
        string savePath = Path.Combine(Application.persistentDataPath, saveFileName);

        if (!File.Exists(savePath)) return;

        foreach (var pair in _runtimeBlocks)
        {
            if (pair.Value != null) Destroy(pair.Value);
        }
        _runtimeBlocks.Clear();

        string json = File.ReadAllText(savePath);
        SaveDataWrapper wrapper = JsonUtility.FromJson<SaveDataWrapper>(json);

        foreach (BlockSaveData blockData in wrapper.allBlocks)
        {
            Vector3Int cell = new Vector3Int(blockData.x, blockData.y, blockData.z);
            
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/NewAssets/GreenMapTiles/{blockData.prefabName}");
            if (prefab == null) prefab = Resources.Load<GameObject>($"Prefabs/NewAssets/Animals/{blockData.prefabName}");
            if (prefab == null) prefab = Resources.Load<GameObject>($"Prefabs/NewAssets/Items/{blockData.prefabName}");
            if (prefab == null) prefab = Resources.Load<GameObject>($"Prefabs/NewAssets/PinkMapTiles/{blockData.prefabName}");

            if (prefab != null)
            {
                GameObject newBlock = Instantiate(prefab, CellToWorld(cell), Quaternion.identity, _mapParent);
                newBlock.layer = Mathf.RoundToInt(Mathf.Log(mapLayer.value, 2));
                _runtimeBlocks[cell] = newBlock;
            }
        }
        Debug.Log("<color=cyan>Đã tải dữ liệu map cũ lên thành công!</color>");
    }
}

// Sửa cấu trúc phân cấp dấu ngoặc nhọn chuẩn chỉnh ở ngoài Class chính
[System.Serializable]
public class BlockSaveData
{
    public int x, y, z;
    public string prefabName; 
}

[System.Serializable]
public class SaveDataWrapper
{
    public List<BlockSaveData> allBlocks = new List<BlockSaveData>();
}