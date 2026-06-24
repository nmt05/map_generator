using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[RequireComponent(typeof(MeshCollider))]
public class GenerateMapCollider : MonoBehaviour
{
    [ContextMenu("Generate Collider")]
    void GenerateCollider()
    {
        MeshCollider mc = GetComponent<MeshCollider>();

        CombineInstance[] combine =
            GetComponentsInChildren<MeshFilter>()
            .Select(mf => new CombineInstance
            {
                mesh = mf.sharedMesh,
                transform = mf.transform.localToWorldMatrix
            })
            .ToArray();

        Mesh mesh = new Mesh();
        mesh.indexFormat =
            UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.CombineMeshes(combine);

        mc.sharedMesh = null;
        mc.sharedMesh = mesh;
    }
}