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
            MeshJob<FlatHexagonGrid, SingleStream>.ScheduleParallel
        };

        private enum MeshType
        {
            SquareGrid,
            SharedSquareGrid,
            SharedTriangleGrid,
            PointyHexagonGrid,
            FlatHexagonGrid
        }

        [SerializeField] private MeshType meshType;
        [SerializeField, Range(1, 50)] private int resolution = 1;
        private Mesh _mesh;

        private void Awake()
        {
            _mesh = new Mesh
            {
                name = "Procedural Mesh"
            };
            GetComponent<MeshFilter>().mesh = _mesh;
        }

        private void OnValidate() => enabled = true;

        private void Update()
        {
            GenerateMesh();
            enabled = false;
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