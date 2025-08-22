namespace CatlikeCodings.Prototypes.Maze2
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 21:08:22
    public partial struct OcclusionJob
    {
        readonly struct Quadrant
        {
            public readonly MazeFlags North, East, South, Northwest, Northeast, Southeast;
            public readonly bool FlipNS, FlipEW;

            public Quadrant(bool flipNS, bool flipEW)
            {
                FlipNS = flipNS;
                FlipEW = flipEW;

                North = flipNS ? MazeFlags.PassageS : MazeFlags.PassageN;
                South = flipNS ? MazeFlags.PassageN : MazeFlags.PassageS;
                East = flipEW ? MazeFlags.PassageW : MazeFlags.PassageE;

                if (flipEW)
                {
                    if (flipNS)
                    {
                        Northwest = MazeFlags.PassageSE;
                        Northeast = MazeFlags.PassageSW;
                        Southeast = MazeFlags.PassageNW;
                    }
                    else
                    {
                        Northwest = MazeFlags.PassageNE;
                        Northeast = MazeFlags.PassageNW;
                        Southeast = MazeFlags.PassageSW;
                    }
                }
                else if (flipNS)
                {
                    Northwest = MazeFlags.PassageSW;
                    Northeast = MazeFlags.PassageSE;
                    Southeast = MazeFlags.PassageNE;
                }
                else
                {
                    Northwest = MazeFlags.PassageNW;
                    Northeast = MazeFlags.PassageNE;
                    Southeast = MazeFlags.PassageSE;
                }
            }
        }

        private static readonly Quadrant[] Quadrants =
        {
            new(false, false), // NE
            new(true, false), // SE
            new(true, true), // SW
            new(false, true), // NW
        };
    }
}