using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class GreedyColliderGenerator : MonoBehaviour
{
    [Header("Block Settings")]
    public float blockSize = 2f;

    [Header("Collect Settings")]
    [Tooltip("Nếu bật, chỉ lấy object con trực tiếp của Map. Nên bật nếu mỗi block là con trực tiếp của Map.")]
    public bool onlyDirectChildren = true;

    private readonly HashSet<Vector3Int> blocks = new();

    private readonly List<Vector3> vertices = new();
    private readonly List<int> triangles = new();

    [ContextMenu("Generate Greedy Collider")]
    public void GenerateCollider()
    {
        blocks.Clear();
        vertices.Clear();
        triangles.Clear();

        CollectBlocks();

        if (blocks.Count == 0)
        {
            Debug.LogWarning("No blocks found.");
            return;
        }
        

        GenerateTopFaces();
        GenerateSideFaces();
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshCollider mc = GetComponent<MeshCollider>();
        mc.sharedMesh = null;
        mc.sharedMesh = mesh;

        Debug.Log($"Greedy Collider Generated | Blocks={blocks.Count} | Tris={triangles.Count / 3}");
    }

    void CollectBlocks()
    {
        foreach (MeshFilter mf in GetComponentsInChildren<MeshFilter>())
        {
            Transform t = mf.transform;

            // Bỏ qua item
            if (t.CompareTag("Item"))
                continue;

            Vector3 localPos = transform.InverseTransformPoint(t.position);

            int x = Mathf.RoundToInt(localPos.x / blockSize);
            int y = Mathf.RoundToInt(localPos.y / blockSize);
            int z = Mathf.RoundToInt(localPos.z / blockSize);

            blocks.Add(new Vector3Int(x, y, z));
        }
    }
    void AddBlockFromTransform(Transform blockTransform)
    {
        // Không dùng Renderer.bounds vì cỏ, táo hoặc chi tiết nhô lên sẽ làm bounds sai.
        // Chỉ lấy vị trí transform của block theo lưới 2x2.
        Vector3 localPos = transform.InverseTransformPoint(blockTransform.position);

        int x = Mathf.RoundToInt(localPos.x / blockSize);
        int y = Mathf.RoundToInt(localPos.y / blockSize);
        int z = Mathf.RoundToInt(localPos.z / blockSize);

        blocks.Add(new Vector3Int(x, y, z));
    }

    void GenerateTopFaces()
    {
        int minX = int.MaxValue;
        int maxX = int.MinValue;

        int minZ = int.MaxValue;
        int maxZ = int.MinValue;

        int minY = int.MaxValue;
        int maxY = int.MinValue;

        foreach (Vector3Int b in blocks)
        {
            minX = Mathf.Min(minX, b.x);
            maxX = Mathf.Max(maxX, b.x);

            minZ = Mathf.Min(minZ, b.z);
            maxZ = Mathf.Max(maxZ, b.z);

            minY = Mathf.Min(minY, b.y);
            maxY = Mathf.Max(maxY, b.y);
        }

        int width = maxX - minX + 1;
        int height = maxZ - minZ + 1;

        for (int y = minY; y <= maxY; y++)
        {
            bool[,] mask = new bool[width, height];

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    Vector3Int p = new Vector3Int(x, y, z);

                    bool exists = blocks.Contains(p);
                    bool topVisible = exists && !blocks.Contains(p + Vector3Int.up);

                    mask[x - minX, z - minZ] = topVisible;
                }
            }

            GreedyMask(mask, minX, minZ, y);
        }
    }


    void GreedyMask(bool[,] mask, int offsetX, int offsetZ, int y)
    {
        int sizeX = mask.GetLength(0);
        int sizeZ = mask.GetLength(1);

        for (int z = 0; z < sizeZ; z++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                if (!mask[x, z])
                    continue;

                int quadWidth = 1;

                while (x + quadWidth < sizeX && mask[x + quadWidth, z])
                {
                    quadWidth++;
                }

                int quadHeight = 1;
                bool canExpand = true;

                while (z + quadHeight < sizeZ && canExpand)
                {
                    for (int k = 0; k < quadWidth; k++)
                    {
                        if (!mask[x + k, z + quadHeight])
                        {
                            canExpand = false;
                            break;
                        }
                    }

                    if (canExpand)
                        quadHeight++;
                }

                AddTopQuad(
                    x + offsetX,
                    z + offsetZ,
                    y,
                    quadWidth,
                    quadHeight);

                for (int dz = 0; dz < quadHeight; dz++)
                {
                    for (int dx = 0; dx < quadWidth; dx++)
                    {
                        mask[x + dx, z + dz] = false;
                    }
                }
            }
        }
    }

    void AddTopQuad(int x, int z, int y, int width, int height)
    {
        float s = blockSize;
        float half = s * 0.5f;

        // Vì pivot block nằm ở tâm, block tại grid (0,0,0) chiếm:
        // X: -1 -> 1
        // Y: -1 -> 1
        // Z: -1 -> 1
        // Nên phải trừ half để collider không lệch +1 ở X/Z/Y.
        float topY = (y + 0.5f) * s + 1f;

        Vector3 v0 = new Vector3(
            x * s - half,
            topY,
            z * s - half);

        Vector3 v1 = new Vector3(
            (x + width) * s - half,
            topY,
            z * s - half);

        Vector3 v2 = new Vector3(
            (x + width) * s - half,
            topY,
            (z + height) * s - half);

        Vector3 v3 = new Vector3(
            x * s - half,
            topY,
            (z + height) * s - half);

        int start = vertices.Count;

        vertices.Add(v0);
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        triangles.Add(start + 0);
        triangles.Add(start + 2);
        triangles.Add(start + 1);

        triangles.Add(start + 0);
        triangles.Add(start + 3);
        triangles.Add(start + 2);
    }
    void GenerateSideFaces()
{
    GenerateXFace(Vector3Int.right, true);   // +X
    GenerateXFace(Vector3Int.left, false);   // -X
    GenerateZFace(Vector3Int.forward, true); // +Z
    GenerateZFace(Vector3Int.back, false);   // -Z
}

void GenerateXFace(Vector3Int dir, bool positive)
{
    GetBounds(out int minX, out int maxX, out int minY, out int maxY, out int minZ, out int maxZ);

    int sizeZ = maxZ - minZ + 1;
    int sizeY = maxY - minY + 1;

    for (int x = minX; x <= maxX; x++)
    {
        bool[,] mask = new bool[sizeZ, sizeY];

        for (int z = minZ; z <= maxZ; z++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector3Int p = new Vector3Int(x, y, z);
                mask[z - minZ, y - minY] = blocks.Contains(p) && !blocks.Contains(p + dir);
            }
        }

        GreedyMaskX(mask, x, minZ, minY, positive);
    }
}

void GenerateZFace(Vector3Int dir, bool positive)
{
    GetBounds(out int minX, out int maxX, out int minY, out int maxY, out int minZ, out int maxZ);

    int sizeX = maxX - minX + 1;
    int sizeY = maxY - minY + 1;

    for (int z = minZ; z <= maxZ; z++)
    {
        bool[,] mask = new bool[sizeX, sizeY];

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector3Int p = new Vector3Int(x, y, z);
                mask[x - minX, y - minY] = blocks.Contains(p) && !blocks.Contains(p + dir);
            }
        }

        GreedyMaskZ(mask, minX, z, minY, positive);
    }
}

void GreedyMaskX(bool[,] mask, int x, int offsetZ, int offsetY, bool positive)
{
    int sizeZ = mask.GetLength(0);
    int sizeY = mask.GetLength(1);

    for (int y = 0; y < sizeY; y++)
    {
        for (int z = 0; z < sizeZ; z++)
        {
            if (!mask[z, y]) continue;

            int width = 1;
            while (z + width < sizeZ && mask[z + width, y]) width++;

            int height = 1;
            bool canExpand = true;

            while (y + height < sizeY && canExpand)
            {
                for (int k = 0; k < width; k++)
                {
                    if (!mask[z + k, y + height])
                    {
                        canExpand = false;
                        break;
                    }
                }

                if (canExpand) height++;
            }

            AddXQuad(x, z + offsetZ, y + offsetY, width, height, positive);

            for (int dy = 0; dy < height; dy++)
                for (int dz = 0; dz < width; dz++)
                    mask[z + dz, y + dy] = false;
        }
    }
}

void GreedyMaskZ(bool[,] mask, int offsetX, int z, int offsetY, bool positive)
{
    int sizeX = mask.GetLength(0);
    int sizeY = mask.GetLength(1);

    for (int y = 0; y < sizeY; y++)
    {
        for (int x = 0; x < sizeX; x++)
        {
            if (!mask[x, y]) continue;

            int width = 1;
            while (x + width < sizeX && mask[x + width, y]) width++;

            int height = 1;
            bool canExpand = true;

            while (y + height < sizeY && canExpand)
            {
                for (int k = 0; k < width; k++)
                {
                    if (!mask[x + k, y + height])
                    {
                        canExpand = false;
                        break;
                    }
                }

                if (canExpand) height++;
            }

            AddZQuad(x + offsetX, z, y + offsetY, width, height, positive);

            for (int dy = 0; dy < height; dy++)
                for (int dx = 0; dx < width; dx++)
                    mask[x + dx, y + dy] = false;
        }
    }
}

void AddXQuad(int x, int z, int y, int width, int height, bool positive)
{
    float s = blockSize;
    float half = s * 0.5f;

    float px = (x * s) + (positive ? half : -half);
    float y0 = y * s - half + 1f;
    float y1 = (y + height) * s - half + 1f;
    float z0 = z * s - half;
    float z1 = (z + width) * s - half;

    Vector3 v0 = new Vector3(px, y0, z0);
    Vector3 v1 = new Vector3(px, y1, z0);
    Vector3 v2 = new Vector3(px, y1, z1);
    Vector3 v3 = new Vector3(px, y0, z1);

    AddQuad(v0, v1, v2, v3, positive);
}

void AddZQuad(int x, int z, int y, int width, int height, bool positive)
{
    float s = blockSize;
    float half = s * 0.5f;

    float pz = (z * s) + (positive ? half : -half);
    float y0 = y * s - half + 1f;
    float y1 = (y + height) * s - half + 1f;
    float x0 = x * s - half;
    float x1 = (x + width) * s - half;

    Vector3 v0 = new Vector3(x0, y0, pz);
    Vector3 v1 = new Vector3(x1, y0, pz);
    Vector3 v2 = new Vector3(x1, y1, pz);
    Vector3 v3 = new Vector3(x0, y1, pz);

    AddQuad(v0, v1, v2, v3, !positive);
}

void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, bool flip)
{
    int start = vertices.Count;

    vertices.Add(v0);
    vertices.Add(v1);
    vertices.Add(v2);
    vertices.Add(v3);

    if (!flip)
    {
        triangles.Add(start + 0);
        triangles.Add(start + 1);
        triangles.Add(start + 2);

        triangles.Add(start + 0);
        triangles.Add(start + 2);
        triangles.Add(start + 3);
    }
    else
    {
        triangles.Add(start + 0);
        triangles.Add(start + 2);
        triangles.Add(start + 1);

        triangles.Add(start + 0);
        triangles.Add(start + 3);
        triangles.Add(start + 2);
    }
}

void GetBounds(
    out int minX, out int maxX,
    out int minY, out int maxY,
    out int minZ, out int maxZ)
{
    minX = minY = minZ = int.MaxValue;
    maxX = maxY = maxZ = int.MinValue;

    foreach (Vector3Int b in blocks)
    {
        minX = Mathf.Min(minX, b.x);
        maxX = Mathf.Max(maxX, b.x);

        minY = Mathf.Min(minY, b.y);
        maxY = Mathf.Max(maxY, b.y);

        minZ = Mathf.Min(minZ, b.z);
        maxZ = Mathf.Max(maxZ, b.z);
    }
}
}
