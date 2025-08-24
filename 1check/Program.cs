using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OneCheck
{
    internal static class Program
    {
        private const int N = 8;                // 8x8 checkerboard
        private static bool[,] B = new bool[N, N];  // true = checker present
        private static int jumpsMade = 0;
        private static string LogPath = "";

        static void Main()
        {
            Console.Title = "1CHECK — Solitaire Checker Game";

            // Start a fresh log
            LogPath = Path.Combine(AppContext.BaseDirectory,
                $"1check-log-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
            LogLine("=== 1CHECK LOG ===");
            LogLine($"Started: {DateTime.Now}");

            PrintIntro();
            InitBoard();
            PrintIndexBoard();

            while (true)
            {
                PrintBoard();
                var moves = AllLegalJumps();
                Console.WriteLine(moves.Count == 1
                    ? "POSSIBLE JUMP: 1"
                    : $"POSSIBLE JUMPS: {moves.Count}");

                if (moves.Count == 0)
                {
                    EndSummary();
                    break;
                }

                Console.WriteLine("Type: FROM TO   (square numbers 1..64)");
                Console.WriteLine("Or:   LIST   (show all legal jumps)");
                Console.WriteLine("      HELP   (rules / numbering)");
                Console.WriteLine("      QUIT   (finish and show summary)");
                Console.Write("> ");

                var line = (Console.ReadLine() ?? "").Trim();
                if (line.Length == 0) continue;

                if (line.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
                {
                    EndSummary();
                    break;
                }
                if (line.Equals("HELP", StringComparison.OrdinalIgnoreCase))
                {
                    PrintHelp();
                    continue;
                }
                if (line.Equals("LIST", StringComparison.OrdinalIgnoreCase))
                {
                    ListJumps(moves);
                    continue;
                }

                // Parse "from to"
                var parts = line.Split(new[] { ' ', ',', ';', '\t' },
                                       StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2 ||
                    !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int from) ||
                    !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int to) ||
                    from < 1 || from > 64 || to < 1 || to > 64)
                {
                    Console.WriteLine("Please enter two numbers 1..64 like: 12 26");
                    continue;
                }

                if (!TryApplyJump(from, to))
                {
                    Console.WriteLine("ILLEGAL MOVE. TRY AGAIN.");
                }
                else
                {
                    jumpsMade++;
                    LogLine($"MOVE {jumpsMade}: {from} -> {to}");
                    LogBoard();
                }
            }

            Console.WriteLine($"\nLog written to: {LogPath}");
        }

        // ---------- Setup & docs ----------
        private static void PrintIntro()
        {
            Console.WriteLine("1CHECK — SOLITAIRE CHECKER GAME\n");
            Console.WriteLine("48 CHECKERS ARE PLACED ON THE TWO OUTSIDE RINGS OF AN 8×8 BOARD.");
            Console.WriteLine("THE OBJECT IS TO REMOVE AS MANY CHECKERS AS POSSIBLE BY DIAGONAL JUMPS.");
            Console.WriteLine("A LEGAL MOVE: jump from a piece over an adjacent piece to the empty");
            Console.WriteLine("square two away diagonally; the jumped piece is removed.");
            Console.WriteLine("No multi-jump in one input; just keep jumping one at a time.");
            Console.WriteLine();
        }

        private static void PrintHelp()
        {
            Console.WriteLine("\n-- HELP --");
            PrintIndexBoard();
            Console.WriteLine("Enter moves by square number (FROM TO), e.g. 12 26.");
            Console.WriteLine("LIST shows all legal jumps. QUIT ends the game.\n");
        }

        private static void InitBoard()
        {
            // Two outer rings filled, inner 4×4 empty
            for (int r = 0; r < N; r++)
            for (int c = 0; c < N; c++)
            {
                bool outer = (r <= 1 || r >= 6 || c <= 1 || c >= 6);
                B[r, c] = outer;
            }
            // Inner 4x4 is already false by default; outer rings set true
            jumpsMade = 0;
            LogLine("Initial board:");
            LogBoard();
        }

        // ---------- Rendering ----------
        private static void PrintIndexBoard()
        {
            Console.WriteLine("HERE IS THE NUMERICAL BOARD:");
            int k = 1;
            for (int r = 0; r < N; r++)
            {
                for (int c = 0; c < N; c++, k++)
                {
                    Console.Write($"{k,3}");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private static void PrintBoard()
        {
            Console.WriteLine("CURRENT BOARD  (● = checker, · = empty):");
            for (int r = 0; r < N; r++)
            {
                for (int c = 0; c < N; c++)
                    Console.Write(B[r, c] ? " ●" : " ·");
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private static void LogBoard()
        {
            using var w = File.AppendText(LogPath);
            w.WriteLine();
            for (int r = 0; r < N; r++)
            {
                for (int c = 0; c < N; c++)
                    w.Write(B[r, c] ? "1 " : "0 ");
                w.WriteLine();
            }
        }

        // ---------- Moves ----------
        private static bool TryApplyJump(int fromSq, int toSq)
        {
            (int fr, int fc) = ToRC(fromSq);
            (int tr, int tc) = ToRC(toSq);

            // must move exactly 2 diagonally
            if (Math.Abs(fr - tr) != 2 || Math.Abs(fc - tc) != 2) return false;

            // from has piece, to empty, middle has piece
            if (!In(fr, fc) || !In(tr, tc)) return false;
            if (!B[fr, fc]) return false;
            if (B[tr, tc]) return false;

            int mr = (fr + tr) / 2;
            int mc = (fc + tc) / 2;
            if (!B[mr, mc]) return false;

            // apply
            B[fr, fc] = false;
            B[mr, mc] = false;   // captured
            B[tr, tc] = true;
            return true;
        }

        private static List<(int from, int to)> AllLegalJumps()
        {
            var res = new List<(int, int)>();
            for (int r = 0; r < N; r++)
            for (int c = 0; c < N; c++)
            {
                if (!B[r, c]) continue;
                // four diagonals
                TryAdd(r, c, r - 2, c - 2, res);
                TryAdd(r, c, r - 2, c + 2, res);
                TryAdd(r, c, r + 2, c - 2, res);
                TryAdd(r, c, r + 2, c + 2, res);
            }
            return res;

            void TryAdd(int fr, int fc, int tr, int tc, List<(int, int)> list)
            {
                if (!In(fr, fc) || !In(tr, tc)) return;
                int mr = (fr + tr) / 2, mc = (fc + tc) / 2;
                if (B[fr, fc] && !B[tr, tc] && In(mr, mc) && B[mr, mc])
                    list.Add((ToSq(fr, fc), ToSq(tr, tc)));
            }
        }

        private static void ListJumps(IEnumerable<(int from, int to)> moves)
        {
            Console.WriteLine("LEGAL JUMPS:");
            int i = 0;
            foreach (var m in moves)
            {
                Console.Write($"{m.from,2}->{m.to,2}   ");
                if (++i % 8 == 0) Console.WriteLine();
            }
            if (i % 8 != 0) Console.WriteLine();
            Console.WriteLine();
        }

        // ---------- Summary ----------
        private static void EndSummary()
        {
            int pieces = CountPieces();
            Console.WriteLine($"\nYOU MADE {jumpsMade} JUMPS AND HAVE {pieces} PIECES REMAINING.");
            LogLine($"\nFinal: jumps={jumpsMade}, remaining={pieces}");
        }

        // ---------- Utilities ----------
        private static (int r, int c) ToRC(int sq)
        {
            int k = sq - 1;
            return (k / N, k % N);
        }

        private static int ToSq(int r, int c) => r * N + c + 1;

        private static bool In(int r, int c) => r >= 0 && r < N && c >= 0 && c < N;

        private static int CountPieces()
        {
            int count = 0;
            for (int r = 0; r < N; r++)
                for (int c = 0; c < N; c++)
                    if (B[r, c]) count++;
            return count;
        }

        private static void LogLine(string s)
        {
            using var w = File.AppendText(LogPath);
            w.WriteLine(s);
        }
    }
}
