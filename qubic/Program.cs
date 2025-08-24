using System;
using System.Collections.Generic;
using System.Linq;

namespace Qubic4x4x4
{
    enum Cell { Empty = 0, Human = 1, CPU = 2 }

    internal static class Program
    {
        private static readonly Random Rng = new();

        // Board: [level, col, row]  — all 0..3
        private static Cell[,,] B = new Cell[4, 4, 4];

        // 76 winning lines; each line is 4 positions (l,c,r)
        private static readonly List<(int l, int c, int r)[]> Lines = new();

        static void Main()
        {
            BuildWinningLines();

            Console.Title = "QUBIC — 4×4×4 Tic-Tac-Toe";
            Console.WriteLine("QUBIC — 4×4×4 TIC-TAC-TOE (first to 4 in a line wins)\n");

            if (AskYes("DO YOU WANT INSTRUCTIONS?"))
                PrintInstructions();

            do
            {
                Array.Clear(B, 0, B.Length);
                bool humanFirst = AskYes("DO YOU WANT TO MOVE FIRST?");
                Play(humanFirst);
            } while (AskYes("\nDO YOU WANT TO TRY ANOTHER GAME?"));
        }

        private static void Play(bool humanFirst)
        {
            Cell turn = humanFirst ? Cell.Human : Cell.CPU;
            int moves = 0;

            while (true)
            {
                if (turn == Cell.Human)
                {
                    Console.Write("\nYOUR MOVE (LLC RR, e.g., 2,3,4 or 234, or SHOW): ");
                    string? s = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(s)) continue;
                    s = s.Trim().ToUpperInvariant();

                    if (s == "SHOW" || s == "BOARD")
                    {
                        PrintBoard();
                        continue;
                    }

                    if (!TryParseCoord(s, out var l, out var c, out var r))
                    {
                        Console.WriteLine("PLEASE TYPE THREE DIGITS BETWEEN 1–4 (LEVEL,COLUMN,ROW).");
                        continue;
                    }
                    if (B[l, c, r] != Cell.Empty)
                    {
                        Console.WriteLine("THAT SQUARE IS USED, TRY AGAIN.");
                        continue;
                    }

                    B[l, c, r] = Cell.Human;
                    moves++;
                    if (CheckWin(Cell.Human, out var winLine))
                    {
                        Console.WriteLine("YOU WIN, AND WINS AS FOLLOWS");
                        PrintLine(winLine);
                        return;
                    }

                    if (moves == 64)
                    {
                        Console.WriteLine("THE GAME IS A DRAW");
                        return;
                    }

                    turn = Cell.CPU;
                }
                else
                {
                    var move = CpuMove();
                    B[move.l, move.c, move.r] = Cell.CPU;
                    moves++;

                    Console.WriteLine($"MACHINE MOVES TO {ToHuman(move)}");

                    if (CheckWin(Cell.CPU, out var winLine))
                    {
                        Console.WriteLine("LET'S SEE YOU GET OUT OF THIS!  MACHINE MOVES TO:");
                        PrintLine(winLine);
                        Console.WriteLine("MACHINE WINS.");
                        return;
                    }

                    if (moves == 64)
                    {
                        Console.WriteLine("THE GAME IS A DRAW");
                        return;
                    }

                    turn = Cell.Human;
                }
            }
        }

        // ---------- Parsing / formatting ----------
        private static bool TryParseCoord(string s, out int l, out int c, out int r)
        {
            l = c = r = -1;

            // Allow "2,3,4" or "234"
            if (s.Contains(','))
            {
                var parts = s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length != 3) return false;
                if (!int.TryParse(parts[0], out var L) ||
                    !int.TryParse(parts[1], out var C) ||
                    !int.TryParse(parts[2], out var R)) return false;
                return From1Based(L, C, R, out l, out c, out r);
            }
            if (s.Length == 3 && s.All(char.IsDigit))
            {
                int L = s[0] - '0', C = s[1] - '0', R = s[2] - '0';
                return From1Based(L, C, R, out l, out c, out r);
            }
            return false;

            static bool From1Based(int L, int C, int R, out int l, out int c, out int r)
            {
                l = L - 1; c = C - 1; r = R - 1;
                return l is >= 0 and < 4 && c is >= 0 and < 4 && r is >= 0 and < 4;
            }
        }

        private static string ToHuman((int l, int c, int r) p) => $"{p.l + 1}{p.c + 1}{p.r + 1}";

        private static void PrintLine((int l, int c, int r)[] line)
        {
            Console.Write(" ");
            foreach (var p in line)
                Console.Write($" {ToHuman(p)}");
            Console.WriteLine();
        }

        // ---------- Board rendering ----------
        private static void PrintBoard()
        {
            // Show each level (1..4). Row 1 at top for readability.
            for (int l = 0; l < 4; l++)
            {
                Console.WriteLine($"\nLEVEL {l + 1}");
                Console.WriteLine("  C1 C2 C3 C4");
                for (int r = 0; r < 4; r++)
                {
                    Console.Write($"R{r + 1} ");
                    for (int c = 0; c < 4; c++)
                    {
                        char ch = B[l, c, r] switch
                        {
                            Cell.Human => 'X',
                            Cell.CPU => 'O',
                            _ => '.'
                        };
                        Console.Write($" {ch} ");
                    }
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }

        // ---------- CPU ----------
        private static (int l, int c, int r) CpuMove()
        {
            // 1) Immediate win
            foreach (var p in EmptyCells())
            {
                B[p.l, p.c, p.r] = Cell.CPU;
                bool win = CheckWin(Cell.CPU, out _);
                B[p.l, p.c, p.r] = Cell.Empty;
                if (win) return p;
            }

            // 2) Block human winning threats (any 3-in-line with 1 empty)
            foreach (var p in EmptyCells())
            {
                B[p.l, p.c, p.r] = Cell.Human;
                bool humanWins = CheckWin(Cell.Human, out _);
                B[p.l, p.c, p.r] = Cell.Empty;
                if (humanWins) return p;
            }

            // 3) Heuristic: prefer cells in many promising lines; prefer central-ish squares
            (int l, int c, int r) best = (-1, -1, -1);
            double bestScore = double.NegativeInfinity;

            foreach (var p in EmptyCells())
            {
                double s = ScoreCell(p);
                if (s > bestScore + 1e-9 || (Math.Abs(s - bestScore) < 1e-9 && Rng.Next(2) == 0))
                {
                    bestScore = s; best = p;
                }
            }

            return best;

            IEnumerable<(int l, int c, int r)> EmptyCells()
            {
                for (int l = 0; l < 4; l++)
                    for (int c = 0; c < 4; c++)
                        for (int r = 0; r < 4; r++)
                            if (B[l, c, r] == Cell.Empty) yield return (l, c, r);
            }

            double ScoreCell((int l, int c, int r) p)
            {
                double score = 0;

                foreach (var line in Lines)
                {
                    if (!line.Any(q => q == p)) continue;

                    int cpu = 0, human = 0, empty = 0;
                    foreach (var q in line)
                        switch (B[q.l, q.c, q.r])
                        {
                            case Cell.Empty: empty++; break;
                            case Cell.CPU: cpu++; break;
                            case Cell.Human: human++; break;
                        }

                    // If line has both sides, it’s poisoned; else grade exponentially
                    if (cpu > 0 && human > 0) continue;

                    if (human == 0) score += Math.Pow(3, cpu); // build own lines
                    if (cpu == 0) score += Math.Pow(2, human); // pressure blocks

                    // Small center bonus
                    score += CenterBonus(p);
                }

                return score;
            }

            static double CenterBonus((int l, int c, int r) p)
            {
                // Manhattan distance from center (1.5,1.5,1.5) → smaller is better
                double dl = Math.Abs(p.l - 1.5);
                double dc = Math.Abs(p.c - 1.5);
                double dr = Math.Abs(p.r - 1.5);
                return 2.5 - (dl + dc + dr); // roughly in [-2, +2.5]
            }
        }

        // ---------- Rules ----------
        private static bool CheckWin(Cell who, out (int l, int c, int r)[] win)
        {
            foreach (var line in Lines)
            {
                bool ok = true;
                foreach (var p in line)
                    if (B[p.l, p.c, p.r] != who) { ok = false; break; }
                if (ok) { win = line; return true; }
            }
            win = Array.Empty<(int, int, int)>();
            return false;
        }

        private static void BuildWinningLines()
        {
            if (Lines.Count > 0) return;

            // Axes in each dimension (rows, columns, pillars)
            for (int l = 0; l < 4; l++)            // rows across columns
                for (int r = 0; r < 4; r++)
                    Lines.Add(Enumerable.Range(0, 4).Select(c => (l, c, r)).ToArray());

            for (int l = 0; l < 4; l++)            // columns down rows
                for (int c = 0; c < 4; c++)
                    Lines.Add(Enumerable.Range(0, 4).Select(r => (l, c, r)).ToArray());

            for (int c = 0; c < 4; c++)            // vertical pillars through levels
                for (int r = 0; r < 4; r++)
                    Lines.Add(Enumerable.Range(0, 4).Select(l => (l, c, r)).ToArray());

            // 2D diagonals on each level (l fixed)
            for (int l = 0; l < 4; l++)
            {
                Lines.Add(Enumerable.Range(0, 4).Select(i => (l, i, i)).ToArray());
                Lines.Add(Enumerable.Range(0, 4).Select(i => (l, i, 3 - i)).ToArray());
            }

            // 2D diagonals on each column-slice (c fixed)
            for (int c = 0; c < 4; c++)
            {
                Lines.Add(Enumerable.Range(0, 4).Select(i => (i, c, i)).ToArray());
                Lines.Add(Enumerable.Range(0, 4).Select(i => (i, c, 3 - i)).ToArray());
            }

            // 2D diagonals on each row-slice (r fixed)
            for (int r = 0; r < 4; r++)
            {
                Lines.Add(Enumerable.Range(0, 4).Select(i => (i, i, r)).ToArray());
                Lines.Add(Enumerable.Range(0, 4).Select(i => (i, 3 - i, r)).ToArray());
            }

            // 4 space diagonals through the cube
            Lines.Add(Enumerable.Range(0, 4).Select(i => (i, i, i)).ToArray());
            Lines.Add(Enumerable.Range(0, 4).Select(i => (i, i, 3 - i)).ToArray());
            Lines.Add(Enumerable.Range(0, 4).Select(i => (i, 3 - i, i)).ToArray());
            Lines.Add(Enumerable.Range(0, 4).Select(i => (i, 3 - i, 3 - i)).ToArray());
        }

        // ---------- UI helpers ----------
        private static bool AskYes(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} (YES/NO) ");
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s is "Y" or "YES") return true;
                if (s is "N" or "NO") return false;
            }
        }

        private static void PrintInstructions()
        {
            Console.WriteLine(@"
THE GAME IS TIC-TAC-TOE IN A 4×4×4 CUBE.
YOU AND THE MACHINE ALTERNATE MOVES. FIRST TO GET 4 IN A STRAIGHT LINE WINS.
A MOVE IS A 3-DIGIT LOCATION (LEVEL, COLUMN, ROW), EACH FROM 1 TO 4.
EXAMPLES:  233  OR  2,3,3

THE MACHINE DOES NOT PRINT THE WHOLE CUBE EACH TURN; TYPE 'SHOW' ANYTIME
TO SEE ALL FOUR LEVELS.

GOOD LUCK!
");
        }
    }
}
