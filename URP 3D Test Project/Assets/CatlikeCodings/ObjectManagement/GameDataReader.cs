using System.IO;
using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 22:18:47
    public class GameDataReader
    {
        public int Version { get; }
        private readonly BinaryReader _reader;

        public GameDataReader(BinaryReader reader, int version)
        {
            _reader = reader;
            Version = version;
        }

        public float ReadFloat()
        {
            return _reader.ReadSingle();
        }

        public int ReadInt()
        {
            return _reader.ReadInt32();
        }

        public Quaternion ReadQuaternion()
        {
            Quaternion value;
            value.x = _reader.ReadSingle();
            value.y = _reader.ReadSingle();
            value.z = _reader.ReadSingle();
            value.w = _reader.ReadSingle();
            return value;
        }

        public Vector3 ReadVector3()
        {
            Vector3 value;
            value.x = _reader.ReadSingle();
            value.y = _reader.ReadSingle();
            value.z = _reader.ReadSingle();
            return value;
        }

        public Color ReadColor()
        {
            Color value;
            value.r = _reader.ReadSingle();
            value.g = _reader.ReadSingle();
            value.b = _reader.ReadSingle();
            value.a = _reader.ReadSingle();
            return value;
        }

        public Random.State ReadRandomState()
        {
            return JsonUtility.FromJson<Random.State>(_reader.ReadString());
        }
    }
}