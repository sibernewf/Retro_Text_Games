using System;
using System.Collections.Generic;

namespace GhostMaze
{
    class Program
    {
        // Maze (12x12). # = wall, . = corridor, X = exit marker (we'll render ✚)
        static readonly string[] Raw =
        {
            "############",
            "#...#......#",
            "#.#.#.####.#",
            "#.#...#..#.#",
            "#.###.#..#.#",
            "#.....#..#.#",
            "###.#####.#X",
            "#...#.....##",
            "#.#.###.#..#",
            "#.#...#.#..#",
            "#...#.....##",
            "############",
        };

        static char[,] map;
        static int H, W;

        enum Dir { Up=0, Right=1, Down=2, Left=3 }

        static (int r,int c) player;
        static Dir facing;
        static (int r,int c) ghost;

        static readonly Random Rng = new Random();
        static int moves = 0;

        static void Main()
        {
            Console.Title = "Ghost Maze";
            LoadMap();

            // Random valid start that isn't the exit
            player = RandomEmpty();
            while (map[player.r, player.c] == 'X') player = RandomEmpty();
            facing = (Dir)Rng.Next(4);

            // Ghost starts somewhere else
            ghost = RandomEmpty();
            while (ghost == player || map[ghost.r, ghost.c] == 'X') ghost = RandomEmpty();

            while (true)
            {
                DrawFrame();

                // Input
                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Q) return;
                if (key == ConsoleKey.N) facing = (Dir)(((int)facing + 3) & 3);
                else if (key == ConsoleKey.M) facing = (Dir)(((int)facing + 1) & 3);
                else if (key == ConsoleKey.X)
                {
                    var ahead = Ahead(player, facing);
                    if (map[ahead.r, ahead.c] != '#') player = ahead;
                    moves++;
                }
                else continue;

                // Check win
                if (map[player.r, player.c] == 'X')
                {
                    Console.Clear();
                    Console.WriteLine("YOU HAVE ESCAPED!");
                    Console.WriteLine($"IN {moves} MOVES.");
                    return;
                }

                // Ghost moves every 5 *player* moves
                if (moves > 0 && moves % 5 == 0)
                {
                    ghost = StepGhost(ghost);
                }

                // If ghost is adjacent after your step, teleport you
                if (IsAdjacent(player, ghost))
                {
                    TeleportPlayer();
                    // and relocate ghost too (fresh scare)
                    ghost = RandomEmpty();
                }
            }
        }

        static void LoadMap()
        {
            H = Raw.Length; W = Raw[0].Length;
            map = new char[H, W];
            for (int r = 0; r < H; r++)
                for (int c = 0; c < W; c++)
                    map[r, c] = Raw[r][c];
        }

        static (int r,int c) RandomEmpty()
        {
            int r, c;
            do { r = Rng.Next(H); c = Rng.Next(W); }
            while (map[r, c] == '#');
            return (r, c);
        }

        static (int r,int c) Ahead((int r,int c) p, Dir d)
        {
            return d switch
            {
                Dir.Up => (p.r - 1, p.c),
                Dir.Right => (p.r, p.c + 1),
                Dir.Down => (p.r + 1, p.c),
                Dir.Left => (p.r, p.c - 1),
                _ => p
            };
        }

        static bool IsAdjacent((int r,int c) a, (int r,int c) b)
            => Math.Abs(a.r - b.r) + Math.Abs(a.c - b.c) == 1;

        static (int r,int c) StepGhost((int r,int c) g)
        {
            // Try shuffled 4-neighbour step; prefer corridors, avoid walls
            var dirs = new List<Dir> { Dir.Up, Dir.Right, Dir.Down, Dir.Left };
            for (int i = 0; i < dirs.Count; i++)
            {
                int j = Rng.Next(i, dirs.Count);
                (dirs[i], dirs[j]) = (dirs[j], dirs[i]);
            }
            foreach (var d in dirs)
            {
                var n = Ahead(g, d);
                if (map[n.r, n.c] != '#') return n;
            }
            return g;
        }

        static void TeleportPlayer()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("A GHOST LOOMS BESIDE YOU!  *WHOOSH*  You are swept elsewhere!\n");
            player = RandomEmpty();
            facing = (Dir)Rng.Next(4);
        }

        static void DrawFrame()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("GHOST MAZE — X=forward, N=turn left, M=turn right, Q=quit");
            Console.WriteLine($"Moves: {moves}");
            Console.WriteLine();

            // Render a 3×5 window in front of the player (like the booklet’s limited view).
            // Convert relative offsets to absolute cells based on facing.
            int depth = 5, width = 3; // 3 columns wide, 5 deep
            for (int d = 0; d < depth; d++)
            {
                for (int w = -1; w <= 1; w++)
                {
                    var cell = OffsetRelative(player, facing, d, w);
                    char ch = '#';
                    if (InBounds(cell))
                    {
                        ch = map[cell.r, cell.c] switch
                        {
                            '#' => '#',
                            '.' => '·',
                            'X' => '✚',
                            _ => '·'
                        };
                        if (cell == ghost) ch = 'G';
                        if (cell == player) ch = 'Y';
                    }
                    Console.Write(ch);
                }
                Console.WriteLine();
            }

            // Also show a tiny compass arrow for orientation.
            Console.WriteLine();
            Console.WriteLine("Facing: " + facing.ToString().ToUpper());
            Console.WriteLine();
        }

        static (int r,int c) OffsetRelative((int r,int c) p, Dir d, int forward, int side)
        {
            // forward >=0; side in {-1,0,1}
            return d switch
            {
                Dir.Up    => (p.r - forward, p.c + side),
                Dir.Right => (p.r + side, p.c + forward),
                Dir.Down  => (p.r + forward, p.c - side),
                Dir.Left  => (p.r - side, p.c - forward),
                _ => p
            };
        }

        static bool InBounds((int r,int c) q)
            => q.r >= 0 && q.c >= 0 && q.r < H && q.c < W;
    }
}
