using System;
using System.Threading;

namespace Gravedigger
{
    class Program
    {
        const int Size = 20;      // grid size
        static int[,] grid = new int[Size, Size]; // 0=empty, 1=dug tunnel, 2=stone
        static int px = Size / 2, py = Size / 2;  // player position
        static Random rng = new Random();

        static void Main()
        {
            Console.Title = "Gravedigger";
            Console.CursorVisible = false;
            InitGrid();

            while (true)
            {
                DrawGrid();

                ConsoleKey key = Console.ReadKey(true).Key;
                int dx = 0, dy = 0;
                if (key == ConsoleKey.LeftArrow) dx = -1;
                if (key == ConsoleKey.RightArrow) dx = 1;
                if (key == ConsoleKey.UpArrow) dy = -1;
                if (key == ConsoleKey.DownArrow) dy = 1;
                if (dx == 0 && dy == 0) continue;

                int nx = px + dx, ny = py + dy;

                // Bounds check
                if (nx < 0 || nx >= Size || ny < 0 || ny >= Size) continue;

                // If already dug, you lose
                if (grid[ny, nx] == 1)
                {
                    Console.Clear();
                    Console.WriteLine("YOU FELL INTO YOUR OWN GRAVE!");
                    break;
                }
                // If stone, blocked
                if (grid[ny, nx] == 2) continue;

                // Move player, dig tunnel
                grid[py, px] = 1; // leave tunnel
                px = nx; py = ny;

                // Occasionally drop a stone
                if (rng.NextDouble() < 0.1)
                {
                    int sx = rng.Next(Size);
                    int sy = rng.Next(Size);
                    if (grid[sy, sx] == 0) grid[sy, sx] = 2;
                }

                // Escape condition: reach border
                if (px == 0 || px == Size - 1 || py == 0 || py == Size - 1)
                {
                    Console.Clear();
                    Console.WriteLine("YOU ESCAPED THE GRAVEYARD!");
                    break;
                }
            }

            Console.CursorVisible = true;
        }

        static void InitGrid()
        {
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    grid[y, x] = 0;
        }

        static void DrawGrid()
        {
            Console.SetCursorPosition(0, 0);
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    if (x == px && y == py) Console.Write("☺");       // player
                    else if (grid[y, x] == 0) Console.Write(".");     // empty
                    else if (grid[y, x] == 1) Console.Write(" ");     // tunnel
                    else if (grid[y, x] == 2) Console.Write("#");     // stone
                }
                Console.WriteLine();
            }
            Console.WriteLine("Use arrow keys. Escape by reaching the border!");
        }
    }
}
