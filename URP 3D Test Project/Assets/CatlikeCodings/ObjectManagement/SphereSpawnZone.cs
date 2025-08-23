using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 12:11:08
    public class SphereSpawnZone : SpawnZone
    {
        [SerializeField] private bool surfaceOnly;

        public override Vector3 SpawnPoint =>
            transform.TransformPoint(surfaceOnly ? Random.onUnitSphere : Random.insideUnitSphere);

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, 1f);
        }
    }
}