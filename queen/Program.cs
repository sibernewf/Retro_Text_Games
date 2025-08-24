using System;
using System.Collections.Generic;
using System.Linq;

namespace QueenGame
{
    class Program
    {
        // Board is 8x8. Rows: 1 (top) -> 8 (bottom). Cols: 1 (left) -> 8 (right).
        // Goal square is row=8, col=1.
        const int Rows = 8;
        const int Cols = 8;

        // Numbering matches the book’s table exactly:
        // Row1: 81 71 61 51 41 31 21 11
        // Row2: 92 82 72 62 52 42 32 22
        // ...
        // Row8: 150 148 138 128 118 108 98 88   (note the special 150 at [8,1])
        static int SquareNumber(int r, int c)
        {
            int tens = r + 8 - c;     // 8..15 minus column, plus row
            int ones = r;             // ones = row number (except the 150 quirk)
            int n = tens * 10 + ones;
            if (r == 8 && c == 1) n = 150; // special case as printed in the book
            return n;
        }

        // Build a lookup from printed number -> (row,col)
        static readonly Dictionary<int, (int r, int c)> NumToPos = BuildNumToPos();
        static Dictionary<int, (int r, int c)> BuildNumToPos()
        {
            var d = new Dictionary<int, (int r, int c)>();
            for (int r = 1; r <= Rows; r++)
            {
                for (int c = 1; c <= Cols; c++)
                {
                    d[SquareNumber(r, c)] = (r, c);
                }
            }
            return d;
        }

        static void Main()
        {
            Console.Title = "QUEEN — One Chess Queen";

            while (true)
            {
                ShowIntro();
                var queen = AskHumanStart();  // (row, col) – must be top row or right column

                // Human has placed the first move. Now alternate: machine, human, ...
                bool humanTurn = false; // machine goes next

                while (true)
                {
                    PrintBoard(queen.r, queen.c);

                    if (IsGoal(queen))
                    {
                        Console.WriteLine("** The queen is on 150. {0} wins! **",
                            humanTurn ? "MACHINE" : "YOU");
                        break;
                    }

                    if (humanTurn)
                    {
                        if (!TryHumanMove(ref queen))
                        {
                            Console.WriteLine("You forfeit. MACHINE WINS.");
                            break;
                        }
                    }
                    else
                    {
                        MachineMove(ref queen);
                    }

                    humanTurn = !humanTurn;
                }

                Console.Write("\nPlay another game? (yes/no) ");
                var again = Console.ReadLine()?.Trim().ToUpperInvariant();
                if (again is not ("Y" or "YES")) break;
                Console.Clear();
            }
        }

        // ---- UI / I/O -------------------------------------------------------

        static void ShowIntro()
        {
            Console.WriteLine("=== QUEEN — One Chess Queen ===\n");
            Console.WriteLine("Place a single queen using these rules:");
            Console.WriteLine("- It may move only LEFT, DOWN, or DIAGONALLY DOWN-LEFT (any distance).");
            Console.WriteLine("- Goal: land exactly on square 150 (bottom-left). First to do so wins.\n");
            Console.WriteLine("Board numbering:");
            PrintBoard(-1, -1);
            Console.WriteLine("You move first by placing the queen on any square in the TOP ROW or RIGHT COLUMN.\n");
            Console.WriteLine("During the game, enter the destination square number (e.g., 126).");
            Console.WriteLine("Enter 0 to forfeit your move (you lose).\n");
        }

        static void PrintBoard(int qr, int qc)
        {
            Console.WriteLine();
            for (int r = 1; r <= Rows; r++)
            {
                for (int c = 1; c <= Cols; c++)
                {
                    int n = SquareNumber(r, c);
                    string cell = n.ToString().PadLeft(3);
                    if (r == qr && c == qc)
                        cell = "[" + cell.Trim().PadLeft(3) + "]"; // mark queen
                    else
                        cell = " " + cell + " ";

                    Console.Write(cell);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            if (qr >= 1 && qc >= 1)
                Console.WriteLine($"Current square: {SquareNumber(qr, qc)}   (row {qr}, col {qc})\n");
        }

        static (int r, int c) AskHumanStart()
        {
            while (true)
            {
                Console.Write("Where would you like to START? (top row / right column) ");
                var s = Console.ReadLine()?.Trim();
                if (!int.TryParse(s, out int n) || !NumToPos.TryGetValue(n, out var pos))
                {
                    Console.WriteLine("Please type a valid square number shown on the board.");
                    continue;
                }

                // Legal start: top row OR rightmost column
                if (pos.r == 1 || pos.c == Cols)
                {
                    Console.WriteLine($"You place the queen on {n}.");
                    return pos;
                }

                Console.WriteLine("Illegal start. You must choose from the TOP ROW or RIGHT COLUMN.");
            }
        }

        static bool TryHumanMove(ref (int r, int c) queen)
        {
            while (true)
            {
                Console.Write("YOUR MOVE (square #, or 0 to forfeit): ");
                var s = Console.ReadLine()?.Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(s)) continue;

                if (s == "0")
                    return false;

                if (!int.TryParse(s, out int n) || !NumToPos.TryGetValue(n, out var to))
                {
                    Console.WriteLine("Please type a valid square number on the board.");
                    continue;
                }

                if (!IsLegalMove(queen, to))
                {
                    Console.WriteLine("Illegal move. You may move only LEFT, DOWN, or DIAGONALLY DOWN-LEFT.");
                    continue;
                }

                queen = to;
                return true;
            }
        }

        // ---- Rules & helpers -------------------------------------------------

        static bool IsGoal((int r, int c) q) => (q.r == 8 && q.c == 1);

        static bool IsLegalMove((int r, int c) from, (int r, int c) to)
        {
            // Must stay inside board
            if (to.r < 1 || to.r > Rows || to.c < 1 || to.c > Cols) return false;

            int dr = to.r - from.r;
            int dc = to.c - from.c;

            // Only down (dr>0, dc=0), left (dr=0, dc<0), or down-left diag (dr>0, dc<0, |dr|==|dc|)
            if (dr == 0 && dc < 0) return true;                       // left
            if (dc == 0 && dr > 0) return true;                       // down
            if (dr > 0 && dc < 0 && dr == -dc) return true;           // diag down-left

            return false;
        }

        // ---- Machine (AI) using Wythoff cold positions ----------------------
        //
        // Model position by distance-to-go from goal (8,1):
        //   x = (c - 1)  : steps remaining to the left edge
        //   y = (8 - r)  : steps remaining to the bottom
        // Legal moves reduce x, or reduce y, or reduce both equally => Wythoff’s Game.
        static void MachineMove(ref (int r, int c) queen)
        {
            var (r, c) = queen;
            int x = c - 1;       // 0..7
            int y = 8 - r;       // 0..7

            // Precompute cold (P-) positions up to 7 using Wythoff Beatty sequences.
            var cold = ColdPositionsUpTo(7); // HashSet<(int,int)>

            // If already cold, any move is fine; we’ll prefer a short safe push.
            // Otherwise move to a reachable cold position.
            (int rx, int ry) target = (-1, -1);

            if (!cold.Contains((x, y)))
            {
                // Find a cold target reachable by one legal move.
                foreach (var (a, b) in cold)
                {
                    if (a > x || b > y) continue; // must decrease or equal
                    bool sameCol = (a == x) && b < y;                // pure down
                    bool sameRow = (b == y) && a < x;                // pure left
                    bool diag    = (x - a == y - b) && a < x && b < y; // down-left

                    if (sameCol || sameRow || diag)
                    {
                        target = (a, b);
                        break;
                    }
                }
            }

            int nx, ny;
            if (target.rx >= 0)
            {
                (nx, ny) = target;
            }
            else
            {
                // Already cold; push one step toward (0,0) without handing over 150 immediately.
                if (x > y && x > 0) { nx = x - 1; ny = y; }
                else if (y > x && y > 0) { nx = x; ny = y - 1; }
                else if (x > 0) { nx = x - 1; ny = y - 1; }
                else { nx = x; ny = y; } // (0,0) shouldn’t occur here unless already at goal
            }

            // Convert (nx,ny) distances back to (row,col)
            int newC = nx + 1;
            int newR = 8 - ny;
            var to = (newR, newC);

            Console.WriteLine($"MACHINE moves to square {SquareNumber(to.newR, to.newC)}");
            queen = to;
        }

        static HashSet<(int,int)> ColdPositionsUpTo(int max)
        {
            // Wythoff pairs: (a_k, b_k) = (floor(k*phi), a_k + k) for k>=1 plus (0,0)
            var set = new HashSet<(int,int)>();
            set.Add((0, 0));

            double phi = (1 + Math.Sqrt(5)) / 2.0;
            for (int k = 1; k <= 30; k++)
            {
                int a = (int)Math.Floor(k * phi);
                int b = a + k;
                if (a <= max && b <= max) set.Add((a, b));
                if (b <= max && a <= max) set.Add((b, a)); // store both orders (we’ll test both)
            }
            return set;
            // (Board radius is only 7, but 30 is harmless.)
        }
    }
}
