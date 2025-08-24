using System;
using System.Globalization;

namespace Gomoko_V1
{
    internal static class Program
    {
        static void Main()
        {
            Console.Title = "GOMOKO — V1 (classic BASIC feel)";
            Console.WriteLine("WELCOME TO THE ORIENTAL GAME OF GOMOKO");
            Console.WriteLine("Board size 7..19. Your stones are X, computer is O. Empty is '.'");
            Console.WriteLine("Enter moves as row,col  (e.g., 7,5).  Type Q to quit.");
            Console.WriteLine("To end the game like the original, type -1,-1 for your move.");
            int n = AskInt("WHAT IS YOUR BOARD SIZE (MIN 7, MAX 19)? ", 7, 19);
            if (n == int.MinValue) return;

            int[,] b = new int[n, n]; // 0 empty, 1 human, 2 cpu
            var rng = new Random();

            while (true)
            {
                Print(b);
                // Player move
                (int r, int c) = AskMove("YOUR PLAY (row,col)? ", n);
                if (r == int.MinValue) return;           // Q
                if (r == -1 && c == -1) break;           // manual end
                if (!Place(b, r, c, 1))
                {
                    Console.WriteLine("SQUARE OCCUPIED, TRY AGAIN...");
                    continue;
                }

                Print(b);
                // Computer move (random empty)
                (int cr, int cc) = RandomEmpty(b, rng);
                if (cr < 0) break; // no empties
                Console.WriteLine("** COMPUTER MOVES **");
                b[cr, cc] = 2;
            }

            Console.WriteLine("THANKS FOR THE GAME!");
            if (AskYesNo("PLAY AGAIN (Y/N)? ")) Main();
        }

        static (int r, int c) RandomEmpty(int[,] b, Random rng)
        {
            int n = b.GetLength(0);
            for (int tries = 0; tries < n * n; tries++)
            {
                int r = rng.Next(n), c = rng.Next(n);
                if (b[r, c] == 0) return (r, c);
            }
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    if (b[i, j] == 0) return (i, j);
            return (-1, -1);
        }

        static bool Place(int[,] b, int r1, int c1, int who)
        {
            int n = b.GetLength(0);
            int r = r1 - 1, c = c1 - 1;
            if (r < 0 || r >= n || c < 0 || c >= n) return false;
            if (b[r, c] != 0) return false;
            b[r, c] = who;
            return true;
        }

        static void Print(int[,] b)
        {
            int n = b.GetLength(0);
            Console.WriteLine();
            for (int i = 0; i < n; i++)
            {
                var line = new char[n * 2 - 1];
                int k = 0;
                for (int j = 0; j < n; j++)
                {
                    line[k++] = b[i, j] switch { 1 => 'X', 2 => 'O', _ => '.' };
                    if (j < n - 1) line[k++] = ' ';
                }
                Console.WriteLine(line);
            }
            Console.WriteLine();
        }

        static (int r, int c) AskMove(string prompt, int n)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") return (int.MinValue, int.MinValue);
                if (s.Contains(","))
                {
                    var parts = s.Split(',');
                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int r) &&
                        int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int c))
                    {
                        if ((r == -1 && c == -1) || (r >= 1 && r <= n && c >= 1 && c <= n))
                            return (r, c);
                    }
                }
                Console.WriteLine("ENTER row,col within board (or -1,-1 to end; Q quits).");
            }
        }

        static int AskInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") return int.MinValue;
                if (int.TryParse(s, out var x) && x >= min && x <= max) return x;
                Console.WriteLine($"ENTER {min}..{max} (or Q to quit).");
            }
        }

        static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s is "Y" or "YES") return true;
                if (s is "N" or "NO") return false;
            }
        }
    }
}
