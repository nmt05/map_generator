using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SceneTool
{
    enum ToolMode
    {
        Place,
        Eraser,
        Hold
    }

    const float CellSize = 2f;
    const float MaxRayDistance = 200f;
    const int WindowId = 9001;
    const string MapName = "Map";
    static bool collapseWindow;
    static readonly string BasePath =
        "Assets/Resources/Prefabs/NewAssets/GreenMapTiles/";

    static readonly string[] PrefabNames =
    {
        "grass1.vox.prefab",
        "grass2.vox.prefab"
    };

    class PrefabGroup
    {
        public string title;
        public string folder;
        public string[] names;

        public PrefabGroup(string title, string folder, params string[] names)
        {
            this.title = title;
            this.folder = folder;
            this.names = names;
        }
    }
    static readonly PrefabGroup[] PrefabGroups =
{
    new PrefabGroup("Animals",
        "Assets/Resources/Prefabs/NewAssets/Animals/",
        "axolotl.vox", "bear.vox", "bunny.vox", "cat.vox",
        "chicken.vox", "cow.vox", "crocodile.vox", "dog.vox",
        "elephant.vox", "fox.vox", "frog.vox", "mole.vox",
        "monkey.vox", "mouse.vox", "panda.vox", "parrot.vox",
        "penguin.vox", "piglet.vox", "turtle.vox", "unicorn.vox"
    ),

    new PrefabGroup("Green Map Tiles",
        "Assets/Resources/Prefabs/NewAssets/GreenMapTiles/",
        "box1.vox", "box2.vox", "grass1.vox", "grass2.vox",
        "grass3.vox", "grassflower1.vox", "grassflower2.vox",
        "grassmushroom.vox", "nograss.vox", "tree.vox",
        "tree2.vox", "walktile.vox"
    ),

    new PrefabGroup("Items",
        "Assets/Resources/Prefabs/NewAssets/Items/",
        "apple.vox", "bamboo.vox", "banana.vox", "candy.vox",
        "carrot.vox", "cheese.vox", "corn.vox", "fish.vox",
        "honey.vox", "melon.vox", "worm.vox"
    ),

    new PrefabGroup("Pink Map Tiles",
        "Assets/Resources/Prefabs/NewAssets/PinkMapTiles/",
        "box1.vox", "box2.vox", "grass1.vox", "grass2.vox",
        "grass3.vox", "grassflower1.vox", "grassmushroom.vox",
        "nograss.vox", "tree1.vox", "tree2.vox", "walktile.vox"
    )
};


    static Rect windowRect = new Rect(10, 10, 285, 520);
    static float scrollY;
    static Vector2 scroll;

    static GameObject selectedPrefab;
    static ToolMode currentMode = ToolMode.Place;

    static readonly Dictionary<Vector3Int, GameObject> blocks = new();

    static bool hasHover;
    static Vector3Int hoverCell;
    static Vector3Int hoverNormal;

    static SceneTool()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        HandleKeyboard();
        RebuildBlockData();

        UpdateHover();
        HandlePickPrefabByMouseButton4();
        DrawGhostBlock();

        if (currentMode == ToolMode.Eraser)
            HandleErase();
        else
            HandlePlacement();

        Handles.BeginGUI();
        windowRect = GUI.Window(WindowId, windowRect, DrawToolWindow, GUIContent.none);
        Handles.EndGUI();

        sceneView.Repaint();
    }
    static void HandlePickPrefabByMouseButton4()
{
    if (!hasHover)
        return;

    Event e = Event.current;

    if (e.type != EventType.MouseDown || e.button != 4)
        return;

    if (blocks.TryGetValue(hoverCell, out GameObject target))
    {
        GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(target);

        if (prefab != null)
        {
            selectedPrefab = prefab;
            currentMode = ToolMode.Place;
            Selection.activeGameObject = target;
            Debug.Log("Đã chọn prefab: " + prefab.name);
        }
    }

    e.Use();
}

    static void DrawToolWindow(int id)
    {
        HandleToolWindowScroll();

        GUILayout.Label("MAP TOOL", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Tạo Map", GUILayout.Height(32)))
            CreateMap();

        GUI.backgroundColor = currentMode == ToolMode.Eraser ? Color.red : Color.white;

        if (GUILayout.Button(currentMode == ToolMode.Eraser ? "Eraser ON" : "Eraser OFF", GUILayout.Height(32)))
            ToggleEraserMode();

        GUI.backgroundColor = Color.white;

        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        Rect clipRect = new Rect(0, 78, windowRect.width, windowRect.height - 82);
        GUI.BeginGroup(clipRect);

        GUILayout.BeginArea(new Rect(8, -scrollY, clipRect.width - 16, 5000));

        foreach (PrefabGroup group in PrefabGroups)
        {
            DrawPrefabGroup(group);
            GUILayout.Space(10);
        }

        GUILayout.EndArea();

        GUI.EndGroup();

        GUI.DragWindow(new Rect(0, 0, windowRect.width, 24));
    }


    static void HandleToolWindowScroll()
    {
        Event e = Event.current;

        if (e.type != EventType.ScrollWheel)
            return;

        Vector2 mouse = e.mousePosition;

        Rect localRect = new Rect(0, 0, windowRect.width, windowRect.height);

        if (!localRect.Contains(mouse))
            return;

        scrollY += e.delta.y * 18f;
        scrollY = Mathf.Clamp(scrollY, 0f, 3500f);

        e.Use();
    }

    static void DrawPrefabGroup(PrefabGroup group)
    {
        GUILayout.Label(group.title, EditorStyles.boldLabel);

        int columns = 4;
        int index = 0;

        while (index < group.names.Length)
        {
            GUILayout.BeginHorizontal();

            for (int i = 0; i < columns; i++)
            {
                if (index >= group.names.Length)
                    break;

                string path = group.folder + group.names[index];
                DrawPrefabButton(path);

                index++;
            }

            GUILayout.EndHorizontal();
        }
    }

    
    static void DrawPrefabButton(string path)
    {
        GameObject prefab = LoadPrefabFromPath(path);

        string buttonName = System.IO.Path.GetFileNameWithoutExtension(path);

        if (buttonName.EndsWith(".vox"))
            buttonName = buttonName.Replace(".vox", "");

        if (prefab == null)
        {
            GUI.backgroundColor = new Color(1f, 0.35f, 0.35f);
            GUILayout.Button("Missing\n" + buttonName, GUILayout.Width(90), GUILayout.Height(90));
            GUI.backgroundColor = Color.white;
            return;
        }

        bool isSelected = selectedPrefab == prefab;

        GUI.backgroundColor = isSelected ? new Color(0.35f, 1f, 0.35f) : Color.white;

        Texture preview = AssetPreview.GetAssetPreview(prefab);

        if (preview == null)
            preview = AssetPreview.GetMiniThumbnail(prefab);

        GUILayout.BeginVertical(GUILayout.Width(62), GUILayout.Height(90));

        if (GUILayout.Button(preview, GUILayout.Width(62), GUILayout.Height(62)))
        {
            selectedPrefab = prefab;
            currentMode = ToolMode.Place;
        }

        if (GUILayout.Button(buttonName, GUILayout.Width(62), GUILayout.Height(24)))
        {
            selectedPrefab = prefab;
            currentMode = ToolMode.Place;
        }

        GUILayout.EndVertical();

        GUI.backgroundColor = Color.white;
    }

static GameObject LoadPrefabFromPath(string path)
{
    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

    if (prefab != null)
        return prefab;

    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path + ".prefab");

    if (prefab != null)
        return prefab;

    if (path.EndsWith(".vox"))
    {
        string prefabPath = path + ".prefab";
        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
    }

    return prefab;
}

    static void HandleKeyboard()
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 3)
        {
            ToggleEraserMode();
            e.Use();
        }
    }

    static void ToggleEraserMode()
    {
        currentMode = currentMode == ToolMode.Eraser
            ? ToolMode.Place
            : ToolMode.Eraser;
    }

    static void CreateMap()
    {
        GameObject oldMap = GameObject.Find(MapName);

        if (oldMap != null)
        {
            Selection.activeGameObject = oldMap;
            Debug.Log("Map đã tồn tại");
            return;
        }

        GameObject map = new GameObject(MapName);
        Undo.RegisterCreatedObjectUndo(map, "Create Map");

        map.transform.position = Vector3.zero;
        map.transform.rotation = Quaternion.identity;
        map.transform.localScale = Vector3.one;

        GameObject prefab = LoadPrefab(0);

        if (prefab == null)
        {
            Debug.LogError("Không tìm thấy prefab đầu tiên");
            return;
        }

        int size = 5;
        int offset = size / 2; // =2

        for (int x = -offset; x <= offset; x++)
        {
            for (int z = -offset; z <= offset; z++)
            {
                GameObject block = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                Undo.RegisterCreatedObjectUndo(block, "Create Block");

                block.transform.SetParent(map.transform);
                block.transform.position = CellToWorld(new Vector3Int(x, 0, z));
                block.transform.rotation = Quaternion.identity;
                block.transform.localScale = Vector3.one;
            }
        }

        selectedPrefab = prefab;
        Selection.activeGameObject = map;
    }

    static GameObject LoadPrefab(int index)
    {
        if (index < 0 || index >= PrefabNames.Length)
            return null;

        return AssetDatabase.LoadAssetAtPath<GameObject>(
            BasePath + PrefabNames[index]
        );
    }

    static void RebuildBlockData()
    {
        blocks.Clear();

        GameObject map = GameObject.Find(MapName);

        if (map == null)
            return;

        foreach (Transform child in map.transform)
        {
            Vector3Int cell = WorldToCell(child.position);

            if (!blocks.ContainsKey(cell))
                blocks.Add(cell, child.gameObject);
        }
    }

    static void UpdateHover()
    {
        Event e = Event.current;

        if (windowRect.Contains(e.mousePosition))
        {
            hasHover = false;
            return;
        }

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        hasHover = RaycastVoxel(
            ray,
            MaxRayDistance,
            out hoverCell,
            out hoverNormal
        );
    }

    static bool RaycastVoxel(
        Ray ray,
        float maxDistance,
        out Vector3Int hitCell,
        out Vector3Int hitNormal)
    {
        Vector3 origin = ray.origin / CellSize;
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

        while (travelled <= maxDistance)
        {
            if (blocks.ContainsKey(current))
            {
                hitCell = current;
                return true;
            }

            if (tMax.x < tMax.y)
            {
                if (tMax.x < tMax.z)
                {
                    current.x += step.x;
                    travelled = tMax.x * CellSize;
                    tMax.x += tDelta.x;
                    hitNormal = new Vector3Int(-step.x, 0, 0);
                }
                else
                {
                    current.z += step.z;
                    travelled = tMax.z * CellSize;
                    tMax.z += tDelta.z;
                    hitNormal = new Vector3Int(0, 0, -step.z);
                }
            }
            else
            {
                if (tMax.y < tMax.z)
                {
                    current.y += step.y;
                    travelled = tMax.y * CellSize;
                    tMax.y += tDelta.y;
                    hitNormal = new Vector3Int(0, -step.y, 0);
                }
                else
                {
                    current.z += step.z;
                    travelled = tMax.z * CellSize;
                    tMax.z += tDelta.z;
                    hitNormal = new Vector3Int(0, 0, -step.z);
                }
            }
        }

        hitCell = Vector3Int.zero;
        hitNormal = Vector3Int.zero;
        return false;
    }

    static float IntBound(float s, float ds)
    {
        if (ds == 0)
            return float.MaxValue;

        if (ds < 0)
            return IntBound(-s, -ds);

        s -= Mathf.Floor(s);

        return (1f - s) / ds;
    }

    static void DrawHoverFace()
    {
        if (!hasHover)
            return;

        Vector3 center = CellToWorld(hoverCell);
        Vector3 normal = new Vector3(
            hoverNormal.x,
            hoverNormal.y,
            hoverNormal.z
        );

        Vector3 faceCenter = center + normal * (CellSize * 0.5f);

        GetFaceAxes(normal, out Vector3 right, out Vector3 up);

        Vector3[] verts =
        {
            faceCenter - right - up,
            faceCenter - right +  up,
            faceCenter + right +  up,
            faceCenter + right - up
        };

        Handles.DrawSolidRectangleWithOutline(
            verts,
            new Color(1f, 0f, 0f, 0.22f),
            Color.red
        );
    }

    static void HandlePlacement()
    {
        if (selectedPrefab == null || !hasHover)
            return;

        Event e = Event.current;

        if (e.type != EventType.MouseDown || e.button != 0)
            return;

        Vector3Int placeCell = hoverCell + hoverNormal;

        if (blocks.ContainsKey(placeCell))
        {
            Debug.Log("Ô này đã có block");
            e.Use();
            return;
        }

        PlacePrefab(placeCell);

        e.Use();
    }

    static void HandleErase()
    {
        if (!hasHover)
            return;

        Event e = Event.current;

        if (e.type != EventType.MouseDown || e.button != 0)
            return;

        if (blocks.TryGetValue(hoverCell, out GameObject target))
        {
            Undo.DestroyObjectImmediate(target);
            blocks.Remove(hoverCell);
        }

        e.Use();
    }

    static void PlacePrefab(Vector3Int cell)
    {
        GameObject map = GameObject.Find(MapName);

        if (map == null)
        {
            Debug.LogWarning("Chưa có Map. Bấm Tạo Map trước.");
            return;
        }

        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
        Undo.RegisterCreatedObjectUndo(obj, "Place Prefab");

        obj.transform.SetParent(map.transform);
        obj.transform.position = CellToWorld(cell);
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        blocks[cell] = obj;
        Selection.activeGameObject = obj;
    }

    static Vector3Int WorldToCell(Vector3 world)
    {
        return new Vector3Int(
            Mathf.FloorToInt(world.x / CellSize),
            Mathf.FloorToInt(world.y / CellSize),
            Mathf.FloorToInt(world.z / CellSize)
        );
    }

    static Vector3 CellToWorld(Vector3Int cell)
    {
        return new Vector3(
            cell.x * CellSize,
            cell.y * CellSize,
            cell.z * CellSize
        );
    }

    static void GetFaceAxes(
        Vector3 normal,
        out Vector3 right,
        out Vector3 up)
    {
        if (Mathf.Abs(normal.y) > 0.5f)
        {
            right = Vector3.right * CellSize * 0.5f;
            up = Vector3.forward * CellSize * 0.5f;
        }
        else if (Mathf.Abs(normal.x) > 0.5f)
        {
            right = Vector3.forward * CellSize * 0.5f;
            up = Vector3.up * CellSize * 0.5f;
        }
        else
        {
            right = Vector3.right * CellSize * 0.5f;
            up = Vector3.up * CellSize * 0.5f;
        }
    }
    static void DrawGhostBlock()
{
    if (!hasHover)
        return;

    Vector3Int placeCell;

    if (currentMode == ToolMode.Place)
        placeCell = hoverCell + hoverNormal;
    else
        placeCell = hoverCell;

    Vector3 center = CellToWorld(placeCell) + new Vector3(0,1,0);

    Vector3 size = Vector3.one * CellSize;

    Handles.color = new Color(1f, 0f, 0f, 0.15f);

    Handles.CubeHandleCap(
        0,
        center,
        Quaternion.identity,
        CellSize,
        EventType.Repaint
    );

    Handles.color = Color.red;

    Handles.DrawWireCube(
        center,
        size
    );
}
}