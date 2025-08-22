using Unity.Collections;

namespace CatlikeCodings.Prototypes.Maze2
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 20:49:22
    public partial struct OcclusionJob
    {
        struct ScanStack
        {
            private NativeArray<Scan> _stack;
            private int _stackSize;

            public ScanStack(int capacity, Scan firstScan)
            {
                _stack = new NativeArray<Scan>(capacity, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                _stack[0] = firstScan;
                _stackSize = 1;
            }

            public void Push(Scan scan) => _stack[_stackSize++] = scan;

            public bool TryPop(out Scan scan)
            {
                if (_stackSize > 0)
                {
                    scan = _stack[--_stackSize];
                    return true;
                }

                scan = default;
                return false;
            }
        }
    }
}