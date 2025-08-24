using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

enum Piece { Empty, X, XKing, O, OKing }

class Program
{
    static readonly int Size = 8;
    static Piece[,] B = new Piece[Size, Size];
    static Random rng = new Random();

    // Initialized so no nullability warnings
    static StreamWriter LogFile = new StreamWriter(Stream.Null);
    static string HumanName = "PLAYER";

    static void Main()
    {
        // Ask player name
        Console.Write("Enter your name: ");
        HumanName = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(HumanName)) HumanName = "PLAYER";

        // Create a timestamped log file
        string fileName = $"Checkers_SampleRun_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        LogFile = new StreamWriter(fileName);

        try
        {
            Log($"THIS PROGRAM WILL PLAY CHECKERS. THE COMPUTER IS X, YOU ARE 0.");
            Log($"THE COMPUTER WILL GO FIRST. NOTE: SQUARES ARE IN (X,Y) AND (1,1) IS BOTTOM LEFT.");
            Log($"ENTER MOVES AS: FROM (x y) TO (u v). MULTI-JUMPS ARE SUPPORTED.");
            Log($"HUMAN: {HumanName} (0 / Q as King)   COMPUTER: X / K as King");
            Log("");

            InitBoard();

            bool computerTurn = true;
            while (true)
            {
                LogBoard();

                var xMoves = GenerateAllMoves(true);   // computer legal moves
                var oMoves = GenerateAllMoves(false);  // human legal moves

                if (xMoves.Count == 0) { Log("VERY GOOD, YOU WIN!"); break; }
                if (oMoves.Count == 0) { Log($"{HumanName.ToUpper()} HAS NO LEGAL MOVES. I WIN."); break; }

                if (computerTurn)
                    ComputerMove(xMoves);
                else
                {
                    if (!PlayerMove(oMoves))
                        continue;
                }

                computerTurn = !computerTurn;
            }

            Log("");
            Log("-- CHECK OUT --");
            Log($"Sample run saved to: {Path.GetFullPath(fileName)}");
        }
        finally
        {
            LogFile.Flush();
            LogFile.Dispose();
        }
    }

    static void InitBoard()
    {
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                B[x, y] = Piece.Empty;
                bool dark = ((x + y) % 2 == 1);
                if (!dark) continue;

                if (y <= 2) B[x, y] = Piece.X;
                else if (y >= 5) B[x, y] = Piece.O;
            }
        }
    }

    static void Log(string s)
    {
        Console.WriteLine(s);
        LogFile.WriteLine(s);
    }

    static char DisplayChar(Piece p)
    {
        return p switch
        {
            Piece.Empty => '.',
            Piece.X => 'X',
            Piece.XKing => 'K',
            Piece.O => '0',
            Piece.OKing => 'Q',
            _ => '?'
        };
    }

    static void LogBoard()
    {
        var lines = new List<string>();
        lines.Add("BOARD:");

        // Column header aligned to board squares
        string colNumbers = "   " + "  "; 
        for (int x = 1; x <= Size; x++) colNumbers += x + " ";
        lines.Add(colNumbers.TrimEnd());

        int innerWidth = (Size * 2) - 1;
        string border = "   +" + new string('-', innerWidth + 2) + "+";
        lines.Add(border);

        for (int y = Size - 1; y >= 0; y--)
        {
            var chars = new char[Size];
            for (int x = 0; x < Size; x++) chars[x] = DisplayChar(B[x, y]);

            string inside = string.Join(' ', chars);
            string row = $"{(y + 1),2} | {inside} | {(y + 1),2}";
            lines.Add(row);
        }

        lines.Add(border);
        lines.Add(colNumbers.TrimEnd());
        lines.Add("");

        foreach (var s in lines)
        {
            Console.WriteLine(s);
            LogFile.WriteLine(s);
        }
    }

    class Move
    {
        public List<(int x, int y)> Path = new();
        public List<(int x, int y)> Captured = new();
        public bool IsCapture => Captured.Count > 0;
        public (int x, int y) From => Path[0];
        public (int x, int y) To => Path[^1];
    }

    static List<Move> GenerateAllMoves(bool forComputer)
    {
        var result = new List<Move>();
        bool anyCapture = false;

        for (int y = 0; y < Size; y++)
        for (int x = 0; x < Size; x++)
        {
            if (!Owns(forComputer, B[x, y])) continue;
            var pieceMoves = GenerateMovesForPiece(x, y, forComputer);
            if (pieceMoves.Count == 0) continue;

            if (pieceMoves.Any(m => m.IsCapture)) anyCapture = true;
            result.AddRange(pieceMoves);
        }

        if (anyCapture) result = result.Where(m => m.IsCapture).ToList();
        return result;
    }

    static bool Owns(bool forComputer, Piece p)
    {
        return forComputer ? (p == Piece.X || p == Piece.XKing)
                           : (p == Piece.O || p == Piece.OKing);
    }

    static bool IsKing(Piece p) => p == Piece.XKing || p == Piece.OKing;

    static List<Move> GenerateMovesForPiece(int sx, int sy, bool forComputer)
    {
        var res = new List<Move>();
        Piece p = B[sx, sy];
        bool king = IsKing(p);

        int forward = forComputer ? +1 : -1;

        var captures = new List<Move>();
        DFSBuildCaptures(sx, sy, sx, sy, p, king, forward, new bool[Size, Size],
                         new List<(int,int)>(), new List<(int,int)>(), captures);

        if (captures.Count > 0) return captures;

        foreach (var (dx, dy) in Diagonals(forward, king))
        {
            int nx = sx + dx, ny = sy + dy;
            if (!Inside(nx, ny)) continue;
            if (B[nx, ny] != Piece.Empty) continue;

            var m = new Move();
            m.Path.Add((sx, sy));
            m.Path.Add((nx, ny));
            res.Add(m);
        }

        return res;
    }

    static IEnumerable<(int dx, int dy)> Diagonals(int forward, bool king)
    {
        if (king)
        {
            yield return (1, 1); yield return (-1, 1);
            yield return (1, -1); yield return (-1, -1);
        }
        else
        {
            yield return (1, forward); yield return (-1, forward);
        }
    }

    static bool Inside(int x, int y) => x >= 0 && x < Size && y >= 0 && y < Size;

    static void DFSBuildCaptures(
        int sx, int sy, int cx, int cy, Piece piece, bool king, int forward,
        bool[,] visitedVictims,
        List<(int,int)> path, List<(int,int)> captured,
        List<Move> outMoves)
    {
        if (path.Count == 0) path.Add((sx, sy));

        foreach (var (dx, dy) in Diagonals(forward, king))
        {
            int mx = cx + dx, my = cy + dy;
            int nx = cx + 2 * dx, ny = cy + 2 * dy;
            if (!Inside(nx, ny) || !Inside(mx, my)) continue;
            if (B[nx, ny] != Piece.Empty) continue;
            if (B[mx, my] == Piece.Empty) continue;
            if (Owns(IsComputerPiece(piece), B[mx, my])) continue;
            if (visitedVictims[mx, my]) continue;

            bool promotes = Promotes(piece, ny);

            visitedVictims[mx, my] = true;
            path.Add((nx, ny));
            captured.Add((mx, my));

            if (!promotes)
            {
                DFSBuildCaptures(sx, sy, nx, ny, piece, king, forward, visitedVictims, path, captured, outMoves);
            }

            outMoves.Add(new Move { Path = new List<(int,int)>(path), Captured = new List<(int,int)>(captured) });

            captured.RemoveAt(captured.Count - 1);
            path.RemoveAt(path.Count - 1);
            visitedVictims[mx, my] = false;
        }
    }

    static bool IsComputerPiece(Piece p) => p == Piece.X || p == Piece.XKing;

    static bool Promotes(Piece piece, int y)
    {
        if (IsKing(piece)) return false;
        if (IsComputerPiece(piece)) return y == Size - 1;
        else return y == 0;
    }

    static void ApplyMove(Move m, bool forComputer)
    {
        var (sx, sy) = m.From;
        var (tx, ty) = m.To;
        var piece = B[sx, sy];
        B[sx, sy] = Piece.Empty;

        foreach (var c in m.Captured) B[c.Item1, c.Item2] = Piece.Empty;

        if (Promotes(piece, ty))
            B[tx, ty] = forComputer ? Piece.XKing : Piece.OKing;
        else
            B[tx, ty] = piece;
    }

    static void ComputerMove(List<Move> legal)
    {
        int bestCap = legal.Max(m => m.Captured.Count);
        var best = legal.Where(m => m.Captured.Count == bestCap)
                        .OrderByDescending(m => m.To.y)
                        .ThenBy(_ => rng.Next())
                        .ToList();

        var pick = best.First();
        Log($"COMPUTER (X) MOVE FROM ({pick.From.x + 1} {pick.From.y + 1}) TO ({pick.To.x + 1} {pick.To.y + 1})");
        ApplyMove(pick, true);
        LogBoard();
    }

    static bool PlayerMove(List<Move> legal)
    {
        bool captureOnly = legal.Any(m => m.IsCapture);

        while (true)
        {
            Console.Write($"{HumanName} — enter move FROM x y TO u v: ");
            var line = (Console.ReadLine() ?? "").Trim();

            Log($"{HumanName.ToUpper()} INPUT: {line}");

            var nums = ParseInts(line).ToList();
            if (nums.Count != 4)
            {
                Log("Please enter four integers like: 2 3 3 4");
                continue;
            }

            int fx = nums[0] - 1, fy = nums[1] - 1, tx = nums[2] - 1, ty = nums[3] - 1;

            var candidates = legal.Where(m => m.From == (fx, fy) && m.To == (tx, ty)).ToList();
            if (candidates.Count == 0)
            {
                if (captureOnly) Log("A capture is available — you must take it.");
                else Log("That is not a legal move.");
                continue;
            }

            var chosen = candidates.OrderByDescending(m => m.Captured.Count).First();
            Log($"{HumanName.ToUpper()} (0) MOVE FROM ({chosen.From.x + 1} {chosen.From.y + 1}) TO ({chosen.To.x + 1} {chosen.To.y + 1})");
            ApplyMove(chosen, false);
            LogBoard();
            return true;
        }
    }

    static IEnumerable<int> ParseInts(string s)
    {
        foreach (var tok in s.Split(new[] { ' ', ',', ';', '\t', '(', ')'}, StringSplitOptions.RemoveEmptyEntries))
            if (int.TryParse(tok, out int v)) yield return v;
    }
}
