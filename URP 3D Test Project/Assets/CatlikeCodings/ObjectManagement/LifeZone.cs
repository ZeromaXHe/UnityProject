using CatlikeCodings.ObjectManagement.Behaviors;
using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 20:04:39
    public class LifeZone : MonoBehaviour
    {
        [SerializeField] private float dyingDuration;

        private void OnTriggerExit(Collider other)
        {
            var shape = other.GetComponent<Shape>();
            if (shape)
            {
                if (dyingDuration <= 0f)
                {
                    shape.Die();
                }
                else if (!shape.IsMarkedAsDying)
                {
                    shape.AddBehavior<DyingShapeBehavior>().Initialize(
                        shape, dyingDuration
                    );
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            var c = GetComponent<Collider>();
            var b = c as BoxCollider;
            if (b != null)
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawWireCube(b.center, b.size);
                return;
            }

            var s = c as SphereCollider;
            if (s != null)
            {
                var scale = transform.lossyScale;
                scale = Vector3.one * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, scale);
                Gizmos.DrawWireSphere(s.center, s.radius);
                return;
            }
        }
    }
}