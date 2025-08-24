using System;
using System.Collections.Generic;

public static class Amazin
{
    // Directions: N,E,S,W
    readonly struct Dir
    {
        public readonly int dx, dy, bit, opposite;
        public Dir(int dx, int dy, int bit, int opposite)
        { this.dx = dx; this.dy = dy; this.bit = bit; this.opposite = opposite; }
    }

    static readonly Dir[] dirs = new[]
    {
        new Dir(0,-1, 1, 4),   // N
        new Dir(1, 0, 2, 8),   // E
        new Dir(0, 1, 4, 1),   // S
        new Dir(-1,0, 8, 2)    // W
    };

    // Each cell stores which walls are OPEN using bit flags NESW (1,2,4,8)
    static int[,] carve;
    static int W, H;
    static Random rng = new Random();

    public static void Main()
    {
        Console.WriteLine("AMAZIN (C#) — Perfect Maze Generator\n");

        W = ReadBoundedInt("What are your WIDTH and LENGTH? (e.g. 39 7): ", 2, 200, out H);

        Generate();
        Print();
    }

    static int ReadBoundedInt(string prompt, int min, int max, out int second)
    {
        while (true)
        {
            Console.Write(prompt);
            var line = Console.ReadLine();
            if (line == null) continue;

            var parts = line.Split(new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out var a) &&
                int.TryParse(parts[1], out var b) &&
                a >= min && a <= max && b >= min && b <= max)
            {
                second = b;
                return a;
            }

            Console.WriteLine($"Please enter two integers between {min} and {max} (inclusive).");
        }
    }

    static void Generate()
    {
        carve = new int[H, W];
        var visited = new bool[H, W];

        int x = rng.Next(W);
        int y = rng.Next(H);

        var stack = new Stack<(int x, int y)>();
        visited[y, x] = true;
        stack.Push((x, y));

        while (stack.Count > 0)
        {
            (x, y) = stack.Peek();

            // gather unvisited neighbors
            var options = new List<Dir>();
            foreach (var d in dirs)
            {
                int nx = x + d.dx, ny = y + d.dy;
                if (nx >= 0 && nx < W && ny >= 0 && ny < H && !visited[ny, nx])
                    options.Add(d);
            }

            if (options.Count == 0)
            {
                stack.Pop(); // backtrack
                continue;
            }

            var dir = options[rng.Next(options.Count)];
            int tx = x + dir.dx, ty = y + dir.dy;

            carve[y, x] |= dir.bit;          // open wall from current to next
            carve[ty, tx] |= dir.opposite;    // open opposite wall from next to current

            visited[ty, tx] = true;
            stack.Push((tx, ty));
        }
    }

    static void Print()
    {
        // Top border
        Console.Write("+");
        for (int x = 0; x < W; x++) Console.Write("---+");
        Console.WriteLine();

        for (int y = 0; y < H; y++)
        {
            // Row of vertical walls
            Console.Write("|");
            for (int x = 0; x < W; x++)
            {
                Console.Write("   "); // cell interior
                bool rightOpen = (carve[y, x] & 2) != 0;
                Console.Write(rightOpen ? " " : "|");
            }
            Console.WriteLine();

            // Row of horizontal walls
            Console.Write("+");
            for (int x = 0; x < W; x++)
            {
                bool bottomOpen = (carve[y, x] & 4) != 0;
                Console.Write(bottomOpen ? "   +" : "---+");
            }
            Console.WriteLine();
        }
    }
}
