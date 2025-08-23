using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 13:52:27
    public class RotatingObject : PersistableObject
    {
        [SerializeField] private Vector3 angularVelocity;

        private void FixedUpdate()
        {
            transform.Rotate(angularVelocity * Time.deltaTime);
        }
    }
}