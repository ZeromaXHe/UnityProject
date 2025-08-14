using System;
using CatlikeCodings.ProceduralMeshes.Generators;
using CatlikeCodings.ProceduralMeshes.Streams;
using UnityEngine;

namespace CatlikeCodings.ProceduralMeshes
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-13 20:17:37
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ProceduralMesh : MonoBehaviour
    {
        private static readonly MeshJobScheduleDelegate[] Jobs =
        {
            MeshJob<SquareGrid, SingleStream>.ScheduleParallel,
            MeshJob<SharedSquareGrid, SingleStream>.ScheduleParallel,
            MeshJob<SharedTriangleGrid, SingleStream>.ScheduleParallel,
            MeshJob<PointyHexagonGrid, SingleStream>.ScheduleParallel,
            MeshJob<FlatHexagonGrid, SingleStream>.ScheduleParallel,
            MeshJob<CubeSphere, SingleStream>.ScheduleParallel,
            MeshJob<UvSphere, SingleStream>.ScheduleParallel
        };

        private enum MeshType
        {
            SquareGrid,
            SharedSquareGrid,
            SharedTriangleGrid,
            PointyHexagonGrid,
            FlatHexagonGrid,
            CubeSphere,
            UvSphere
        }

        [Flags]
        private enum GizmoMode
        {
            Nothing = 0,
            Vertices = 1,
            Normals = 0b10,
            Tangents = 0b100
        }

        private enum MaterialMode
        {
            Flat,
            Ripple,
            LatLonMap,
            CubeMap
        }

        [SerializeField] private MeshType meshType;
        [SerializeField, Range(1, 50)] private int resolution = 1;
        [SerializeField] private GizmoMode gizmos;
        [SerializeField] private MaterialMode material;
        [SerializeField] private Material[] materials;
        private Mesh _mesh;
        private Vector3[] _vertices, _normals;
        private Vector4[] _tangents;

        private void Awake()
        {
            _mesh = new Mesh
            {
                name = "Procedural Mesh"
            };
            GetComponent<MeshFilter>().mesh = _mesh;
        }

        private void OnValidate() => enabled = true;

        private void OnDrawGizmos()
        {
            if (gizmos == GizmoMode.Nothing || _mesh == null)
            {
                return;
            }

            var drawVertices = (gizmos & GizmoMode.Vertices) != 0;
            var drawNormals = (gizmos & GizmoMode.Normals) != 0;
            var drawTangents = (gizmos & GizmoMode.Tangents) != 0;
            if (_vertices == null)
            {
                _vertices = _mesh.vertices;
            }

            if (drawNormals && _normals == null)
            {
                _normals = _mesh.normals;
            }

            if (drawTangents && _tangents == null)
            {
                _tangents = _mesh.tangents;
            }

            var t = transform;
            for (var i = 0; i < _vertices.Length; i++)
            {
                var position = t.TransformPoint(_vertices[i]);
                if (drawVertices)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(position, 0.02f);
                }

                if (drawNormals)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(position, t.TransformDirection(_normals[i]) * 0.2f);
                }

                if (drawTangents)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(position, t.TransformDirection(_tangents[i]) * 0.2f);
                }
            }
        }

        private void Update()
        {
            GenerateMesh();
            enabled = false;
            _vertices = null;
            _normals = null;
            _tangents = null;
            GetComponent<MeshRenderer>().material = materials[(int)material];
        }

        private void GenerateMesh()
        {
            var meshDataArray = Mesh.AllocateWritableMeshData(1);
            var meshData = meshDataArray[0];
            Jobs[(int)meshType](_mesh, meshData, resolution, default).Complete();
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, _mesh);
        }
    }
}