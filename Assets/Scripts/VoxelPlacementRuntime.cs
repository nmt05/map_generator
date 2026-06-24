using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class VoxelPlacementRuntime : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] public float cellSize = 2f;
    [SerializeField] public float maxBuildDistance = 20f;
    [SerializeField] public string mapParentName = "Map";

    [Header("Current Selection")]
    public GameObject selectedPrefab; 

    [Header("Ghost Visual (Optional)")]
    [SerializeField] public GameObject ghostCubePrefab; 

    public Transform _mapParent;
    public GameObject _ghostInstance;
    public Camera _mainCamera;

    // Quản lý thế giới hoàn toàn bằng dữ liệu tọa độ ngầm
    public readonly Dictionary<Vector3Int, GameObject> _runtimeBlocks = new();

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
            if (_ghostInstance.TryGetComponent<Collider>(out var col)) col.enabled = false;
        }

        if (Application.isPlaying)
        {
            // LoadGame();
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

#if UNITY_EDITOR
    public void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
    {
        if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            string savePath = System.IO.Path.Combine(Application.persistentDataPath, saveFileName);
            if (!System.IO.File.Exists(savePath)) return;

            string json = System.IO.File.ReadAllText(savePath);
            SaveDataWrapper wrapper = JsonUtility.FromJson<SaveDataWrapper>(json);

            GameObject mapObj = GameObject.Find(mapParentName);
            if (mapObj != null) UnityEditor.Undo.DestroyObjectImmediate(mapObj);
            
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
                }
            }
            
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
#endif

    // public void OnApplicationQuit() => SaveGame();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) SaveGame();
        if (Input.GetKeyDown(KeyCode.K)) LoadGame();
        
        HandleVoxelLogicWithoutPhysics();
    }

    // ================= TOÁN HỌC THUẦN TÚY: DÒ Ô KHÔNG DÙNG COLLIDER =================
    public void HandleVoxelLogicWithoutPhysics()
    {
        if (_mainCamera == null) return;

        // Tạo tia từ tâm màn hình
        Ray ray = _mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0)); 

        // Gọi hàm thuật toán DDA kiểm tra xem tia có cắt qua block nào trong Dictionary ko
        if (VoxelRaycastDDA(ray, maxBuildDistance, out Vector3Int hoverCell, out Vector3Int hitNormal))
        {
            Vector3Int placeCell = hoverCell + hitNormal;

            // Xử lý Ghost Block hiển thị
            if (_ghostInstance != null)
            {
                if (!_runtimeBlocks.ContainsKey(placeCell))
                {
                    _ghostInstance.SetActive(true);
                    _ghostInstance.transform.position = CellToWorld(placeCell) + new Vector3(0,1,0);
                }
                else if (_runtimeBlocks.ContainsKey(hoverCell))
                {
                    _ghostInstance.SetActive(true);
                    _ghostInstance.transform.position = CellToWorld(hoverCell) + new Vector3(0,1,0) ;
                }
                else
                {
                    _ghostInstance.SetActive(false);
                }
            }

            // Click chuột phải = Đặt khối
            if (Input.GetMouseButtonDown(1)) ExecutePlace(placeCell);

            // Click chuột trái = Phá khối
            if (Input.GetMouseButtonDown(0)) ExecuteErase(hoverCell);

            // Chuột giữa = Copy khối
            if (Input.GetMouseButtonDown(2)) ExecutePipette(hoverCell);
        }
        else
        {
            if (_ghostInstance != null) _ghostInstance.SetActive(false);
        }
    }

    // Thuật toán DDA (Digital Differential Analysis) quét lưới 3D tương tự file Editor cũ của bạn
    public bool VoxelRaycastDDA(Ray ray, float maxDistance, out Vector3Int hitCell, out Vector3Int hitNormal)
    {
        Vector3 origin = ray.origin / cellSize;
        Vector3 direction = ray.direction.normalized;

        Vector3Int current = new Vector3Int(
            Mathf.FloorToInt(origin.x),
            Mathf.FloorToInt(origin.y),
            Mathf.FloorToInt(origin.z)
        );

        Vector3Int step = new Vector3Int(
            direction.x >= 0 ? 1 : -1,
            direction.y >= 0 ? 1 : -1,
            direction.z >= 0 ? 1 : -1
        );

        Vector3 tDelta = new Vector3(
            direction.x == 0 ? float.MaxValue : Mathf.Abs(1f / direction.x),
            direction.y == 0 ? float.MaxValue : Mathf.Abs(1f / direction.y),
            direction.z == 0 ? float.MaxValue : Mathf.Abs(1f / direction.z)
        );

        Vector3 tMax = new Vector3(
            IntBound(origin.x, direction.x),
            IntBound(origin.y, direction.y),
            IntBound(origin.z, direction.z)
        );

        hitNormal = Vector3Int.zero;
        float travelled = 0f;

        while (travelled <= (maxDistance / cellSize))
        {
            // Kiểm tra trong Dictionary dữ liệu xem ô hiện tại "current" có block nào không
            if (_runtimeBlocks.ContainsKey(current) && _runtimeBlocks[current] != null)
            {
                hitCell = current;
                return true;
            }

            if (tMax.x < tMax.y)
            {
                if (tMax.x < tMax.z)
                {
                    current.x += step.x;
                    travelled = tMax.x;
                    tMax.x += tDelta.x;
                    hitNormal = new Vector3Int(-step.x, 0, 0);
                }
                else
                {
                    current.z += step.z;
                    travelled = tMax.z;
                    tMax.z += tDelta.z;
                    hitNormal = new Vector3Int(0, 0, -step.z);
                }
            }
            else
            {
                if (tMax.y < tMax.z)
                {
                    current.y += step.y;
                    travelled = tMax.y;
                    tMax.y += tDelta.y;
                    hitNormal = new Vector3Int(0, -step.y, 0);
                }
                else
                {
                    current.z += step.z;
                    travelled = tMax.z;
                    tMax.z += tDelta.z;
                    hitNormal = new Vector3Int(0, 0, -step.z);
                }
            }
        }

        hitCell = Vector3Int.zero;
        hitNormal = Vector3Int.zero;
        return false;
    }

    public float IntBound(float s, float ds)
    {
        if (ds == 0) return float.MaxValue;
        if (ds < 0) return IntBound(-s, -ds);
        s -= Mathf.Floor(s);
        return (1f - s) / ds;
    }
    // =================================================================================

    public void ExecutePlace(Vector3Int cell)
    {
        if (selectedPrefab == null || _runtimeBlocks.ContainsKey(cell)) return; 

        GameObject newBlock = Instantiate(selectedPrefab, CellToWorld(cell), Quaternion.identity, _mapParent);
        _runtimeBlocks[cell] = newBlock;
    }

    public void ExecuteErase(Vector3Int cell)
    {
        if (_runtimeBlocks.TryGetValue(cell, out GameObject target))
        {
            Destroy(target);
            _runtimeBlocks.Remove(cell);
        }
    }

public void ExecutePipette(Vector3Int cell)
    {
        if (_runtimeBlocks.TryGetValue(cell, out GameObject target))
        {
            if (target == null) return;

            // Lấy tên sạch của block (bỏ chữ (Clone) đi)
            string cleanName = target.name.Replace("(Clone)", "").Trim();

            // Tìm nạp lại Prefab gốc từ thư mục Resources giống như lúc Load game
            GameObject originalPrefab = Resources.Load<GameObject>($"Prefabs/NewAssets/GreenMapTiles/{cleanName}");
            if (originalPrefab == null) originalPrefab = Resources.Load<GameObject>($"Prefabs/NewAssets/Animals/{cleanName}");
            if (originalPrefab == null) originalPrefab = Resources.Load<GameObject>($"Prefabs/NewAssets/Items/{cleanName}");
            if (originalPrefab == null) originalPrefab = Resources.Load<GameObject>($"Prefabs/NewAssets/PinkMapTiles/{cleanName}");

            if (originalPrefab != null)
            {
                selectedPrefab = originalPrefab; // Gán Prefab gốc làm block hiện tại để xây
                Debug.Log($"<color=cyan>Pipette:</color> Đã hút thành công khối gốc [{cleanName}]");
            }
            else
            {
                Debug.LogError($"Không thể hút khối! Không tìm thấy Prefab gốc tên '{cleanName}' trong thư mục Resources.");
            }
        }
    }
    public void RebuildInitialBlockData()
    {
        _runtimeBlocks.Clear();
        if (_mapParent == null) return;
        
        foreach (Transform child in _mapParent)
        {
            Vector3Int cell = WorldToCell(child.position);
            if (!_runtimeBlocks.ContainsKey(cell)) _runtimeBlocks.Add(cell, child.gameObject);
        }
    }

    public Vector3Int WorldToCell(Vector3 world)
    {
        return new Vector3Int(
            Mathf.FloorToInt(world.x / cellSize),
            Mathf.FloorToInt(world.y / cellSize),
            Mathf.FloorToInt(world.z / cellSize)
        );
    }

    public Vector3 CellToWorld(Vector3Int cell)
    {
        return new Vector3(cell.x * cellSize, cell.y * cellSize, cell.z * cellSize);
    }

    [Header("Save/Load Settings")]
    [SerializeField] public string saveFileName = "voxel_map_save.json";

    public void SaveGame()
    {
        SaveDataWrapper wrapper = new SaveDataWrapper();
        foreach (var pair in _runtimeBlocks)
        {
            if (pair.Value == null) continue;
            Vector3Int cell = pair.Key;
            wrapper.allBlocks.Add(new BlockSaveData { x = cell.x, y = cell.y, z = cell.z, prefabName = pair.Value.name.Replace("(Clone)", "").Trim() });
        }
        File.WriteAllText(Path.Combine(Application.persistentDataPath, saveFileName), JsonUtility.ToJson(wrapper, true));
    }

    public void LoadGame()
    {
        string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        if (!File.Exists(savePath)) return;

        foreach (var pair in _runtimeBlocks) if (pair.Value != null) Destroy(pair.Value);
        _runtimeBlocks.Clear();

        SaveDataWrapper wrapper = JsonUtility.FromJson<SaveDataWrapper>(File.ReadAllText(savePath));
        foreach (BlockSaveData blockData in wrapper.allBlocks)
        {
            Vector3Int cell = new Vector3Int(blockData.x, blockData.y, blockData.z);
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/NewAssets/GreenMapTiles/{blockData.prefabName}");
            if (prefab == null) prefab = Resources.Load<GameObject>($"Prefabs/NewAssets/Animals/{blockData.prefabName}");
            if (prefab == null) prefab = Resources.Load<GameObject>($"Prefabs/NewAssets/Items/{blockData.prefabName}");
            if (prefab == null) prefab = Resources.Load<GameObject>($"Prefabs/NewAssets/PinkMapTiles/{blockData.prefabName}");

            if (prefab != null) _runtimeBlocks[cell] = Instantiate(prefab, CellToWorld(cell), Quaternion.identity, _mapParent);
        }
    }
}

[System.Serializable]
public class BlockSaveData { public int x, y, z; public string prefabName; }

[System.Serializable]
public class SaveDataWrapper { public List<BlockSaveData> allBlocks = new List<BlockSaveData>(); }