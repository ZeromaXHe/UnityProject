using System;
using UnityEngine;

namespace CatlikeCodings.ProceduralMeshes
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class SimpleProceduralMesh : MonoBehaviour
    {
        private void OnEnable()
        {
            var mesh = new Mesh
            {
                name = "Procedural Mesh",
                vertices = new[]
                {
                    Vector3.zero, Vector3.right, Vector3.up, new Vector3(1f, 1f)
                },
                triangles = new[] { 0, 2, 1, 1, 2, 3 }, // Unity 顺时针是正面
                normals = new[]
                {
                    Vector3.back, Vector3.back, Vector3.back, Vector3.back
                },
                tangents = new[]
                {
                    new Vector4(1f, 0f, 0f, -1f),
                    new Vector4(1f, 0f, 0f, -1f),
                    new Vector4(1f, 0f, 0f, -1f),
                    new Vector4(1f, 0f, 0f, -1f)
                },
                uv = new[]
                {
                    Vector2.zero, Vector2.right, Vector2.up, Vector2.one
                }
            };
            GetComponent<MeshFilter>().mesh = mesh;
        }
    }
}