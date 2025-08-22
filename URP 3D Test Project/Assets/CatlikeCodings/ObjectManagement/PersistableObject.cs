using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 22:19:55
    [DisallowMultipleComponent]
    public class PersistableObject : MonoBehaviour
    {
        public virtual void Save(GameDataWriter writer)
        {
            writer.Write(transform.localPosition);
            writer.Write(transform.localRotation);
            writer.Write(transform.localScale);
        }

        public virtual void Load(GameDataReader reader)
        {
            transform.localPosition = reader.ReadVector3();
            transform.localRotation = reader.ReadQuaternion();
            transform.localScale = reader.ReadVector3();
        }
    }
}