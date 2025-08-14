using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace CatlikeCodings.ProceduralMeshes
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-13 20:13:13
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct MeshJob<TG, TS> : IJobFor
        where TG : struct, IMeshGenerator
        where TS : struct, IMeshStreams
    {
        private TG _generator;
        [WriteOnly] private TS _streams;

        public void Execute(int i) => _generator.Execute(i, _streams);

        public static JobHandle ScheduleParallel(
            Mesh mesh, Mesh.MeshData meshData, int resolution, JobHandle dependency
        ) => ScheduleParallel(mesh, meshData, resolution, dependency, Vector3.zero, false);

        public static JobHandle ScheduleParallel(Mesh mesh, Mesh.MeshData meshData,
            int resolution, JobHandle dependency, Vector3 extraBoundsExtents, bool supportVectorization)
        {
            var job = new MeshJob<TG, TS>();
            job._generator.Resolution = resolution;
            var vertexCount = job._generator.VertexCount;
            if (supportVectorization && (vertexCount & 0b11) != 0)
            {
                vertexCount += 4 - (vertexCount & 0b11);
            }
            Bounds bounds = job._generator.Bounds;
            bounds.extents += extraBoundsExtents;
            job._streams.Setup(
                meshData,
                mesh.bounds = bounds,
                vertexCount,
                job._generator.IndexCount
            );
            return job.ScheduleParallel(job._generator.JobLength, 1, dependency);
        }
    }

    public delegate JobHandle MeshJobScheduleDelegate(Mesh mesh, Mesh.MeshData meshData,
        int resolution, JobHandle dependency);

    public delegate JobHandle AdvancedMeshJobScheduleDelegate(Mesh mesh, Mesh.MeshData meshData,
        int resolution, JobHandle dependency, Vector3 extraBoundsExtents, bool supportVectorization);
}