using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace CatlikeCodings.Prototypes.Maze2
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 15:15:22
    [BurstCompile]
    public struct GenerateMazeJob : IJob
    {
        public Maze Maze;

        public int Seed;
        public float PickLastProbability, OpenDeadEndProbability, OpenArbitraryProbability;

        public void Execute()
        {
            var random = new Random((uint)Seed);
            var scratchpad = new NativeArray<(int, MazeFlags, MazeFlags)>(
                4, Allocator.Temp, NativeArrayOptions.UninitializedMemory
            );
            var activeIndices = new NativeArray<int>(
                Maze.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory
            );
            int firstActiveIndex = 0, lastActiveIndex = 0;
            activeIndices[firstActiveIndex] = random.NextInt(Maze.Length);
            while (firstActiveIndex <= lastActiveIndex)
            {
                var pickLast = random.NextFloat() < PickLastProbability;
                int randomActiveIndex, index;
                if (pickLast)
                {
                    randomActiveIndex = 0;
                    index = activeIndices[lastActiveIndex];
                }
                else
                {
                    randomActiveIndex = random.NextInt(firstActiveIndex, lastActiveIndex + 1);
                    index = activeIndices[randomActiveIndex];
                }

                var availablePassageCount = FindAvailablePassages(index, scratchpad);
                if (availablePassageCount <= 1)
                {
                    if (pickLast)
                    {
                        lastActiveIndex -= 1;
                    }
                    else
                    {
                        activeIndices[randomActiveIndex] = activeIndices[firstActiveIndex++];
                    }
                }

                if (availablePassageCount > 0)
                {
                    var passage = scratchpad[random.NextInt(0, availablePassageCount)];
                    Maze.Set(index, passage.Item2);
                    Maze[passage.Item1] = passage.Item3;
                    activeIndices[++lastActiveIndex] = passage.Item1;
                }
            }

            if (OpenDeadEndProbability > 0f)
            {
                random = OpenDeadEnds(random, scratchpad);
            }

            if (OpenArbitraryProbability > 0f)
            {
                random = OpenArbitraryPassages(random);
            }
        }

        private Random OpenDeadEnds(Random random, NativeArray<(int, MazeFlags, MazeFlags)> scratchpad)
        {
            for (var i = 0; i < Maze.Length; i++)
            {
                var cell = Maze[i];
                if (cell.HasExactlyOne() && random.NextFloat() < OpenDeadEndProbability)
                {
                    var availablePassageCount = FindClosedPassages(i, scratchpad, cell);
                    var passage = scratchpad[random.NextInt(0, availablePassageCount)];
                    Maze[i] = cell.With(passage.Item2);
                    Maze.Set(i + passage.Item1, passage.Item3);
                }
            }

            return random;
        }

        private Random OpenArbitraryPassages(Random random)
        {
            for (var i = 0; i < Maze.Length; i++)
            {
                var coordinates = Maze.IndexToCoordinates(i);
                if (coordinates.x > 0 && random.NextFloat() < OpenArbitraryProbability)
                {
                    Maze.Set(i, MazeFlags.PassageW);
                    Maze.Set(i + Maze.StepW, MazeFlags.PassageE);
                }

                if (coordinates.y > 0 && random.NextFloat() < OpenArbitraryProbability)
                {
                    Maze.Set(i, MazeFlags.PassageS);
                    Maze.Set(i + Maze.StepS, MazeFlags.PassageN);
                }
            }

            return random;
        }

        private int FindAvailablePassages(
            int index, NativeArray<(int, MazeFlags, MazeFlags)> scratchpad
        )
        {
            var coordinates = Maze.IndexToCoordinates(index);
            var count = 0;
            if (coordinates.x + 1 < Maze.SizeEW)
            {
                var i = index + Maze.StepE;
                if (Maze[i] == MazeFlags.Empty)
                {
                    scratchpad[count++] = (i, MazeFlags.PassageE, MazeFlags.PassageW);
                }
            }

            if (coordinates.x > 0)
            {
                var i = index + Maze.StepW;
                if (Maze[i] == MazeFlags.Empty)
                {
                    scratchpad[count++] = (i, MazeFlags.PassageW, MazeFlags.PassageE);
                }
            }

            if (coordinates.y + 1 < Maze.SizeNS)
            {
                var i = index + Maze.StepN;
                if (Maze[i] == MazeFlags.Empty)
                {
                    scratchpad[count++] = (i, MazeFlags.PassageN, MazeFlags.PassageS);
                }
            }

            if (coordinates.y > 0)
            {
                var i = index + Maze.StepS;
                if (Maze[i] == MazeFlags.Empty)
                {
                    scratchpad[count++] = (i, MazeFlags.PassageS, MazeFlags.PassageN);
                }
            }

            return count;
        }

        private int FindClosedPassages(int index,
            NativeArray<(int, MazeFlags, MazeFlags)> scratchpad, MazeFlags exclude)
        {
            var coordinates = Maze.IndexToCoordinates(index);
            var count = 0;
            if (exclude != MazeFlags.PassageE && coordinates.x + 1 < Maze.SizeEW)
            {
                scratchpad[count++] = (Maze.StepE, MazeFlags.PassageE, MazeFlags.PassageW);
            }

            if (exclude != MazeFlags.PassageW && coordinates.x > 0)
            {
                scratchpad[count++] = (Maze.StepW, MazeFlags.PassageW, MazeFlags.PassageE);
            }

            if (exclude != MazeFlags.PassageN && coordinates.y + 1 < Maze.SizeNS)
            {
                scratchpad[count++] = (Maze.StepN, MazeFlags.PassageN, MazeFlags.PassageS);
            }

            if (exclude != MazeFlags.PassageS && coordinates.y > 0)
            {
                scratchpad[count++] = (Maze.StepS, MazeFlags.PassageS, MazeFlags.PassageN);
            }

            return count;
        }
    }
}