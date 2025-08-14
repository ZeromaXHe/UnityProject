using System;
using CatlikeCodings.ProceduralMeshes;
using CatlikeCodings.ProceduralMeshes.Generators;
using CatlikeCodings.ProceduralMeshes.Streams;
using CatlikeCodings.PseudorandomNoise.Hashing;
using UnityEngine;
using UnityEngine.Rendering;
using static CatlikeCodings.PseudorandomNoise.Hashing.Noise;

namespace CatlikeCodings.PseudorandomSurfaces
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-13 20:17:37
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ProceduralSurface : MonoBehaviour
    {
        private static readonly AdvancedMeshJobScheduleDelegate[] MeshJobs =
        {
            MeshJob<SquareGrid, SingleStream>.ScheduleParallel,
            MeshJob<SharedSquareGrid, SingleStream>.ScheduleParallel,
            MeshJob<SharedTriangleGrid, SingleStream>.ScheduleParallel,
            MeshJob<PointyHexagonGrid, SingleStream>.ScheduleParallel,
            MeshJob<FlatHexagonGrid, SingleStream>.ScheduleParallel,
            MeshJob<CubeSphere, SingleStream>.ScheduleParallel,
            MeshJob<SharedCubeSphere, PositionStream>.ScheduleParallel,
            MeshJob<Icosphere, PositionStream>.ScheduleParallel,
            MeshJob<GeoIcosphere, PositionStream>.ScheduleParallel,
            MeshJob<Octasphere, SingleStream>.ScheduleParallel,
            MeshJob<GeoOctasphere, SingleStream>.ScheduleParallel,
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
            SharedCubeSphere,
            Icosphere,
            GeoIcosphere,
            Octasphere,
            GeoOctasphere,
            UvSphere
        }

        [Flags]
        private enum GizmoMode
        {
            Nothing = 0,
            Vertices = 1,
            Normals = 0b10,
            Tangents = 0b100,
            Triangles = 0b1000
        }

        private enum MaterialMode
        {
            Displacement,
            Flat,
            LatLonMap,
            CubeMap
        }

        [Flags]
        private enum MeshOptimizationMode
        {
            Nothing = 0,
            ReorderIndices = 1,
            ReorderVertices = 0b10
        }

        private static readonly SurfaceJobScheduleDelegate[,] SurfaceJobs =
        {
            {
                SurfaceJob<Simplex1D<Simplex>>.ScheduleParallel,
                SurfaceJob<Simplex2D<Simplex>>.ScheduleParallel,
                SurfaceJob<Simplex3D<Simplex>>.ScheduleParallel
            },
            {
                SurfaceJob<Simplex1D<Smoothstep<Turbulence<Simplex>>>>.ScheduleParallel,
                SurfaceJob<Simplex2D<Smoothstep<Turbulence<Simplex>>>>.ScheduleParallel,
                SurfaceJob<Simplex3D<Smoothstep<Turbulence<Simplex>>>>.ScheduleParallel
            },
            {
                SurfaceJob<Simplex1D<Value>>.ScheduleParallel,
                SurfaceJob<Simplex2D<Value>>.ScheduleParallel,
                SurfaceJob<Simplex3D<Value>>.ScheduleParallel
            }
        };

        private enum NoiseType
        {
            Simplex,
            SimplexSmoothTurbulence,
            SimplexValue
        }

        [SerializeField] private NoiseType noiseType;
        [SerializeField, Range(1, 3)] private int dimensions = 1;
        [SerializeField] private MeshOptimizationMode meshOptimization;
        [SerializeField] private MeshType meshType;
        [SerializeField] private bool recalculateNormals, recalculateTangents;
        [SerializeField, Range(1, 50)] private int resolution = 1;
        [SerializeField, Range(-1f, 1f)] private float displacement = 0.5f;
        [SerializeField] private Settings noiseSettings = Settings.Default;
        [SerializeField] private SpaceTRS domain = new() { scale = 1f };
        [SerializeField] private GizmoMode gizmos;
        [SerializeField] private MaterialMode material;
        [SerializeField] private Material[] materials;
        private Mesh _mesh;
        [NonSerialized] private Vector3[] _vertices, _normals;
        [NonSerialized] private Vector4[] _tangents;
        [NonSerialized] private int[] _triangles;

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
            var drawTriangles = (gizmos & GizmoMode.Triangles) != 0;
            if (_vertices == null)
            {
                _vertices = _mesh.vertices;
            }

            if (drawNormals && _normals == null)
            {
                drawNormals = _mesh.HasVertexAttribute(VertexAttribute.Normal);
                if (drawNormals)
                {
                    _normals = _mesh.normals;
                }
            }

            if (drawTangents && _tangents == null)
            {
                drawTangents = _mesh.HasVertexAttribute(VertexAttribute.Tangent);
                if (drawTangents)
                {
                    _tangents = _mesh.tangents;
                }
            }

            if (drawTriangles && _triangles == null)
            {
                _triangles = _mesh.triangles;
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

            if (drawTriangles)
            {
                var colorStep = 1f / (_triangles.Length - 3);
                for (var i = 0; i < _triangles.Length; i += 3)
                {
                    var c = i * colorStep;
                    Gizmos.color = new Color(c, 0f, c);
                    Gizmos.DrawSphere(
                        t.TransformPoint((
                            _vertices[_triangles[i]] +
                            _vertices[_triangles[i + 1]] +
                            _vertices[_triangles[i + 2]]
                        ) * (1f / 3f)),
                        0.02f
                    );
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
            _triangles = null;
            GetComponent<MeshRenderer>().material = materials[(int)material];
        }

        private void GenerateMesh()
        {
            var meshDataArray = Mesh.AllocateWritableMeshData(1);
            var meshData = meshDataArray[0];
            SurfaceJobs[(int)noiseType, dimensions - 1](meshData, resolution, noiseSettings, domain, displacement,
                    MeshJobs[(int)meshType](_mesh, meshData, resolution, default,
                        new Vector3(0f, Mathf.Abs(displacement)), true))
                .Complete();
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, _mesh);
            if (recalculateNormals)
            {
                _mesh.RecalculateNormals();
            }

            if (recalculateTangents)
            {
                _mesh.RecalculateTangents();
            }

            if (meshOptimization == MeshOptimizationMode.ReorderIndices)
            {
                _mesh.OptimizeIndexBuffers();
            }
            else if (meshOptimization == MeshOptimizationMode.ReorderVertices)
            {
                _mesh.OptimizeReorderVertexBuffer();
            }
            else if (meshOptimization != MeshOptimizationMode.Nothing)
            {
                _mesh.Optimize();
            }
        }
    }
}