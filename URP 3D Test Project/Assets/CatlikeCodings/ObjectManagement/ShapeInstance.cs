namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 18:15:23
    [System.Serializable]
    public struct ShapeInstance
    {
        public Shape Shape { get; private set; }
        private int _instanceIdOrSaveIndex;

        public bool IsValid => Shape && _instanceIdOrSaveIndex == Shape.InstanceId;

        public ShapeInstance(Shape shape)
        {
            Shape = shape;
            _instanceIdOrSaveIndex = shape.InstanceId;
        }

        public ShapeInstance(int saveIndex)
        {
            Shape = null;
            _instanceIdOrSaveIndex = saveIndex;
        }

        public static implicit operator ShapeInstance(Shape shape)
        {
            return new ShapeInstance(shape);
        }

        public void Resolve()
        {
            if (_instanceIdOrSaveIndex >= 0)
            {
                Shape = Game.Instance.GetShape(_instanceIdOrSaveIndex);
                _instanceIdOrSaveIndex = Shape.InstanceId;
            }
        }
    }
}