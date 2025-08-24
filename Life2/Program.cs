using System;
using System.Collections.Generic;
using System.Globalization;

namespace Life2
{
    enum Cell { Empty, P1, P2 }

    internal static class Program
    {
        const int N = 5;                 // 5x5 board
        const char P1Char = '*';
        const char P2Char = '#';
        const char EmptyChar = '.';

        static void Main()
        {
            Console.Title = "LIFE·2 — Two-Person Game of Life (5×5)";
            var board = new Cell[N, N];

            Console.WriteLine("U.B. LIFE GAME");
            Console.WriteLine("Player 1 uses '*', Player 2 uses '#'.");
            Console.WriteLine("Board coordinates are X,Y with 1..5 for both axes.\n");

            // ---- Initial placement (3 each), simultaneous resolution for clashes ----
            var p1Set = ReadManyCoords("PLAYER 1 3 LIVE PIECES", board, count: 3, allowExisting:false);
            var p2Set = ReadManyCoords("PLAYER 2 3 LIVE PIECES", board, count: 3, allowExisting:false);

            ApplyInitialPlacements(board, p1Set, p2Set);
            PrintBoard(board);

            // ---- Game loop ----
            int turn = 1;
            while (true)
            {
                if (Count(board, Cell.P1) == 0 || Count(board, Cell.P2) == 0)
                    break;

                Console.WriteLine($"\nTURN {turn}");

                var p1 = ReadOneCoord("PLAYER 1 X,Y", board, allowExisting:false);
                var p2 = ReadOneCoord("PLAYER 2 X,Y", board, allowExisting:false);

                if (p1 == p2)
                {
                    Console.WriteLine("SAME COORD. SET TO 0");
                }
                else
                {
                    if (IsEmpty(board, p1)) board[p1.y, p1.x] = Cell.P1;
                    if (IsEmpty(board, p2)) board[p2.y, p2.x] = Cell.P2;
                }

                // Advance one generation with LIFE·2 rules
                board = NextGeneration(board);

                PrintBoard(board);
                turn++;
            }

            // ---- Result ----
            int p1Left = Count(board, Cell.P1);
            int p2Left = Count(board, Cell.P2);
            Console.WriteLine();
            if (p1Left == 0 && p2Left == 0) Console.WriteLine("IT'S A DRAW.");
            else if (p2Left == 0) Console.WriteLine("PLAYER 1 IS THE WINNER!");
            else Console.WriteLine("PLAYER 2 IS THE WINNER!");
        }

        // ====== Game of Life logic with colored births ======

        static Cell[,] NextGeneration(Cell[,] b)
        {
            var next = new Cell[N, N];

            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                int alive = 0, nP1 = 0, nP2 = 0;
                for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int yy = y + dy, xx = x + dx;
                    if (yy < 0 || yy >= N || xx < 0 || xx >= N) continue;
                    var c = b[yy, xx];
                    if (c != Cell.Empty)
                    {
                        alive++;
                        if (c == Cell.P1) nP1++; else nP2++;
                    }
                }

                var cur = b[y, x];
                if (cur != Cell.Empty)
                {
                    // Survivals
                    next[y, x] = (alive == 2 || alive == 3) ? cur : Cell.Empty;
                }
                else
                {
                    // Births: exactly 3 neighbors; choose majority owner among the 3
                    if (alive == 3)
                        next[y, x] = (nP1 > nP2) ? Cell.P1 : Cell.P2; // 2 vs 1 decides; tie cannot occur with 3
                    else
                        next[y, x] = Cell.Empty;
                }
            }

            return next;
        }

        // ====== I/O helpers ======

        static List<(int x,int y)> ReadManyCoords(string header, Cell[,] board, int count, bool allowExisting)
        {
            Console.WriteLine();
            Console.WriteLine(header);
            var list = new List<(int x,int y)>();
            for (int i = 0; i < count; i++)
                list.Add(ReadOneCoord("X,Y", board, allowExisting));
            return list;
        }

        static (int x,int y) ReadOneCoord(string prompt, Cell[,] board, bool allowExisting)
        {
            while (true)
            {
                Console.Write($"{prompt} ");
                string? s = Console.ReadLine();
                if (TryParseXY(s, out int x, out int y))
                {
                    if (x < 0 || x >= N || y < 0 || y >= N)
                    {
                        Console.WriteLine("ILLEGAL COORDS. RETYPE");
                        continue;
                    }
                    if (!allowExisting && board[y, x] != Cell.Empty)
                    {
                        Console.WriteLine("ILLEGAL COORDS. RETYPE");
                        continue;
                    }
                    return (x, y);
                }

                Console.WriteLine("Please enter as X,Y with 1..5.");
            }
        }

        static bool TryParseXY(string? input, out int x, out int y)
        {
            x = y = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;
            var parts = input.Split(',', ';');
            if (parts.Length != 2) return false;
            if (!int.TryParse(parts[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int x1)) return false;
            if (!int.TryParse(parts[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int y1)) return false;
            x = x1 - 1; y = y1 - 1; // convert to 0-based
            return true;
        }

        static void ApplyInitialPlacements(Cell[,] board, List<(int x,int y)> p1, List<(int x,int y)> p2)
        {
            var setP1 = new HashSet<(int,int)>(p1);
            var setP2 = new HashSet<(int,int)>(p2);
            foreach (var pos in setP1)
                if (!setP2.Contains(pos)) board[pos.Item2, pos.Item1] = Cell.P1;
            foreach (var pos in setP2)
                if (!setP1.Contains(pos)) board[pos.Item2, pos.Item1] = Cell.P2;
            // if both chose same square, it remains empty
        }

        static bool IsEmpty(Cell[,] b, (int x,int y) p) => b[p.y, p.x] == Cell.Empty;

        static int Count(Cell[,] b, Cell who)
        {
            int c = 0;
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                    if (b[y, x] == who) c++;
            return c;
        }

        static void PrintBoard(Cell[,] b)
        {
            Console.WriteLine();
            // top axis
            Console.Write("   ");
            for (int x = 1; x <= N; x++) Console.Write($" {x}");
            Console.WriteLine();
            for (int y = 0; y < N; y++)
            {
                Console.Write($" {y + 1} ");
                for (int x = 0; x < N; x++)
                {
                    char ch = b[y, x] switch
                    {
                        Cell.P1 => P1Char,
                        Cell.P2 => P2Char,
                        _ => EmptyChar
                    };
                    Console.Write($" {ch}");
                }
                Console.Write($"  {y + 1}");
                Console.WriteLine();
            }
            // bottom axis
            Console.Write("   ");
            for (int x = 1; x <= N; x++) Console.Write($" {x}");
            Console.WriteLine();
        }
    }
}
