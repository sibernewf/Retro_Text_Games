using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LifeGame
{
    internal static class Program
    {
        // Display size to match the vintage listing
        private const int Rows = 24;
        private const int Cols = 70;

        private const char Live = '*';
        private const char Dead = ' ';

        static void Main()
        {
            Console.Title = "LIFE — John Conway's Game of Life (24×70)";
            PrintIntro();

            // Read initial pattern (free-form ASCII art)
            var seed = ReadPattern();
            var board = CenterOnBoard(seed, Rows, Cols);

            int generation = 0;
            int delayMs = 150; // simulation speed
            bool paused = false;

            // for simple still-life/oscillator detection
            var last = new bool[Rows, Cols];
            var prev = new bool[Rows, Cols];
            Copy(board, last);

            while (true)
            {
                // Draw
                Draw(board, generation, Population(board), paused, delayMs);

                // Controls (non-blocking)
                HandleKeys(ref paused, ref delayMs);

                if (!paused)
                {
                    // Evolve
                    var next = Step(board);

                    // Detect steady states (still life or period-2 oscillator)
                    bool sameAsNow = BoardsEqual(next, board);
                    bool sameAsLast = BoardsEqual(next, prev);

                    if (Population(next) == 0)
                    {
                        Draw(next, generation + 1, 0, paused, delayMs);
                        Console.WriteLine("\nPopulation died out. Press any key to exit.");
                        Console.ReadKey(true);
                        break;
                    }

                    if (sameAsNow)
                    {
                        Draw(next, generation + 1, Population(next), paused, delayMs);
                        Console.WriteLine("\nReached a stable pattern (still life). Press any key to exit.");
                        Console.ReadKey(true);
                        break;
                    }

                    if (sameAsLast)
                    {
                        Draw(next, generation + 1, Population(next), paused, delayMs);
                        Console.WriteLine("\nDetected a 2-step oscillator. Press any key to exit.");
                        Console.ReadKey(true);
                        break;
                    }

                    // rotate buffers
                    Copy(board, prev);
                    board = next;
                    generation++;
                }

                Thread.Sleep(delayMs);
            }
        }

        // --- Simulation core ---
        private static bool[,] Step(bool[,] board)
        {
            int rMax = board.GetLength(0);
            int cMax = board.GetLength(1);
            var next = new bool[rMax, cMax];

            for (int r = 0; r < rMax; r++)
            {
                for (int c = 0; c < cMax; c++)
                {
                    int n = Neighbors(board, r, c);
                    if (board[r, c])
                    {
                        // Survivals: 2 or 3 neighbors
                        next[r, c] = (n == 2 || n == 3);
                    }
                    else
                    {
                        // Births: exactly 3 neighbors
                        next[r, c] = (n == 3);
                    }
                }
            }

            return next;
        }

        private static int Neighbors(bool[,] board, int r, int c)
        {
            int rMax = board.GetLength(0);
            int cMax = board.GetLength(1);
            int count = 0;

            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    int rr = r + dr;
                    int cc = c + dc;
                    // no wrapping (edges are just edges)
                    if (rr >= 0 && rr < rMax && cc >= 0 && cc < cMax && board[rr, cc])
                        count++;
                }
            }
            return count;
        }

        private static int Population(bool[,] b)
        {
            int pop = 0;
            for (int r = 0; r < b.GetLength(0); r++)
                for (int c = 0; c < b.GetLength(1); c++)
                    if (b[r, c]) pop++;
            return pop;
        }

        // --- Input / layout ---
        private static void PrintIntro()
        {
            Console.WriteLine("LIFE — John Conway’s Game of Life");
            Console.WriteLine("Enter your starting pattern using '*' for live cells and spaces for empty.");
            Console.WriteLine("Finish with Ctrl+Z (Windows) or a single '.' on its own line.\n");
        }

        private static List<string> ReadPattern()
        {
            var lines = new List<string>();
            while (true)
            {
                string? line = Console.ReadLine();
                if (line == null) break;                 // Ctrl+Z (EOF)
                if (line.Length == 1 && line[0] == '.')  // '.' sentinel
                    break;
                lines.Add(line);
            }
            if (lines.Count == 0)
            {
                // default to a glider if no input provided
                lines.Add("  *");
                lines.Add("   *");
                lines.Add(" ***");
            }
            return lines;
        }

        private static bool[,] CenterOnBoard(List<string> pattern, int rows, int cols)
        {
            int ph = pattern.Count;
            int pw = pattern.Max(s => s.Length);

            var board = new bool[rows, cols];

            int top = Math.Max(0, (rows - ph) / 2);
            int left = Math.Max(0, (cols - pw) / 2);

            for (int r = 0; r < ph; r++)
            {
                var line = pattern[r];
                for (int c = 0; c < Math.Min(pw, line.Length); c++)
                {
                    int rr = top + r;
                    int cc = left + c;
                    if (rr >= rows || cc >= cols) continue;
                    board[rr, cc] = (line[c] == Live);
                }
            }

            return board;
        }

        // --- Rendering & controls ---
        private static void Draw(bool[,] board, int generation, int population, bool paused, int delayMs)
        {
            Console.SetCursorPosition(0, 0);
            var sb = new StringBuilder();

            sb.AppendLine($"GENERATION: {generation,5}    POPULATION: {population,5}    {(paused ? "[PAUSED]" : "         ")}    (+/- speed, P pause, Q quit)");
            sb.AppendLine();

            for (int r = 0; r < board.GetLength(0); r++)
            {
                for (int c = 0; c < board.GetLength(1); c++)
                    sb.Append(board[r, c] ? Live : Dead);
                sb.AppendLine();
            }

            // Pad/clear screen tail on first frame
            Console.Write(sb.ToString());
        }

        private static void HandleKeys(ref bool paused, ref int delayMs)
        {
            while (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.Q:
                        Environment.Exit(0);
                        break;
                    case ConsoleKey.P:
                        paused = !paused;
                        break;
                    case ConsoleKey.OemPlus:
                    case ConsoleKey.Add:
                        delayMs = Math.Max(10, delayMs - 20);
                        break;
                    case ConsoleKey.OemMinus:
                    case ConsoleKey.Subtract:
                        delayMs = Math.Min(1000, delayMs + 20);
                        break;
                }
            }
        }

        // --- helpers ---
        private static void Copy(bool[,] src, bool[,] dst)
        {
            for (int r = 0; r < src.GetLength(0); r++)
                for (int c = 0; c < src.GetLength(1); c++)
                    dst[r, c] = src[r, c];
        }

        private static bool BoardsEqual(bool[,] a, bool[,] b)
        {
            for (int r = 0; r < a.GetLength(0); r++)
                for (int c = 0; c < a.GetLength(1); c++)
                    if (a[r, c] != b[r, c]) return false;
            return true;
        }
    }
}
