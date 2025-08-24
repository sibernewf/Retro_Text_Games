using System;
using System.Collections.Generic;
using System.Linq;

namespace HiQ;

/// <summary>
/// Hi-Q (peg solitaire) on a 7x7 cross board (33 holes).
/// Classic start: all pegs except center empty.
/// Legal move: jump orthogonally over adjacent peg into empty hole two cells away.
/// Input: "from to" or "from,to" using hole numbers 1..33 (see numbering map).
/// Type 'q' to quit.
/// </summary>
internal static class Program
{
    private static readonly int[,] Mask =
    {
        { -1,-1, 1, 1, 1,-1,-1 },
        { -1,-1, 1, 1, 1,-1,-1 },
        {  1,  1, 1, 1, 1, 1, 1 },
        {  1,  1, 1, 0, 1, 1, 1 }, // center 0 (we'll fill with 1 later then empty)
        {  1,  1, 1, 1, 1, 1, 1 },
        { -1,-1, 1, 1, 1,-1,-1 },
        { -1,-1, 1, 1, 1,-1,-1 },
    };

    // Map holeId(1..33) <-> (r,c)
    private static readonly (int r, int c)[] IdToRC;
    private static readonly Dictionary<(int r,int c), int> RCToId;

    static Program()
    {
        var list = new List<(int,int)>(33);
        for (int r = 0; r < 7; r++)
            for (int c = 0; c < 7; c++)
                if (Mask[r, c] != -1) list.Add((r, c));
        IdToRC = new (int r, int c)[list.Count + 1];
        RCToId = new Dictionary<(int,int), int>(list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            IdToRC[i + 1] = list[i];
            RCToId[list[i]] = i + 1;
        }
    }

    static void Main()
    {
        Console.Title = "Hi-Q — Peg Solitaire";
        PrintIntro();
        PrintNumbering();

        do
        {
            var board = CreateStartBoard();
            Console.WriteLine("\nTO SAVE TYPING, A COMPRESSED NUMBERING IS USED. REFER TO THE MAP ABOVE.");
            Console.WriteLine("O.K. LET'S BEGIN.\n");

            while (true)
            {
                Draw(board);

                var legal = AllLegalMoves(board);
                if (legal.Count == 0)
                {
                    int remaining = CountPegs(board);
                    Console.WriteLine("GAME OVER.");
                    if (remaining == 1) Console.WriteLine("BRAVO! YOU MADE A PERFECT SCORE!!");
                    Console.WriteLine($"YOU HAVE {remaining} PIECE(S) REMAINING.");
                    break;
                }

                // ask for move
                if (!TryGetMove(out int from, out int to)) return;

                if (!IsHole(from) || !IsHole(to))
                {
                    Console.WriteLine("PLEASE USE NUMBERS BETWEEN 1 AND 33 THAT ARE VALID HOLES.");
                    continue;
                }

                var m = (from, to);
                if (!IsLegal(board, m))
                {
                    Console.WriteLine("ILLEGAL MOVE, TRY AGAIN...");
                    continue;
                }

                Apply(board, m);
            }

        } while (AskYesNo("\nPLAY AGAIN (YES OR NO)? "));

        Console.WriteLine("SO LONG.  HOPE YOU ENJOYED YOURSELF!");
        Console.WriteLine("READY");
    }

    /* ---------- UI ---------- */

    static void PrintIntro()
    {
        Console.WriteLine("THIS IS THE GAME OF HI-Q");
        Console.WriteLine("REMOVE PEGS BY JUMPING OVER AN ADJACENT PEG INTO AN EMPTY HOLE.");
        Console.WriteLine("THE JUMPED PEG IS REMOVED. THE GOAL IS TO LEAVE ONLY ONE PEG.");
        Console.WriteLine("TYPE 'q' ANYTIME TO QUIT.\n");
    }

    static void PrintNumbering()
    {
        Console.WriteLine("HERE IS THE BOARD NUMBERING (HOLES 1..33):\n");
        int idx = 1;
        for (int r = 0; r < 7; r++)
        {
            for (int c = 0; c < 7; c++)
            {
                if (Mask[r, c] == -1) { Console.Write("   "); continue; }
                Console.Write($"{idx,2} ");
                idx++;
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    static void Draw(int[,] b)
    {
        Console.WriteLine();
        for (int r = 0; r < 7; r++)
        {
            for (int c = 0; c < 7; c++)
            {
                if (Mask[r, c] == -1) { Console.Write("   "); continue; }
                Console.Write(b[r, c] == 1 ? " o " : " . ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    static bool TryGetMove(out int from, out int to)
    {
        from = to = -1;
        Console.Write("MOVE WHICH PIECE? ");
        string s1 = (Console.ReadLine() ?? "").Trim();
        if (s1.Equals("q", StringComparison.OrdinalIgnoreCase)) return false;

        Console.Write("TO WHERE? ");
        string s2 = (Console.ReadLine() ?? "").Trim();
        if (s2.Equals("q", StringComparison.OrdinalIgnoreCase)) return false;

        if (!int.TryParse(s1.Replace(",", " "), out from)
            || !int.TryParse(s2.Replace(",", " "), out to))
        {
            Console.WriteLine("PLEASE ENTER NUMBERS (1..33).");
            return TryGetMove(out from, out to); // re-ask
        }
        return true;
    }

    static bool AskYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim();
            if (s.Equals("q", StringComparison.OrdinalIgnoreCase)) return false;
            if (s.Length == 0) continue;
            char c = char.ToUpperInvariant(s[0]);
            if (c == 'Y') return true;
            if (c == 'N') return false;
        }
    }

    /* ---------- Game core ---------- */

    static int[,] CreateStartBoard()
    {
        var b = new int[7, 7];
        for (int r = 0; r < 7; r++)
            for (int c = 0; c < 7; c++)
                if (Mask[r, c] != -1) b[r, c] = 1;
        // empty center
        b[3, 3] = 0;
        return b;
    }

    static int CountPegs(int[,] b)
    {
        int n = 0;
        for (int r = 0; r < 7; r++)
            for (int c = 0; c < 7; c++)
                if (Mask[r, c] != -1 && b[r, c] == 1) n++;
        return n;
    }

    static bool IsHole(int id) => id >= 1 && id < IdToRC.Length;

    static bool IsLegal(int[,] b, (int from, int to) m)
    {
        var (fr, fc) = IdToRC[m.from];
        var (tr, tc) = IdToRC[m.to];

        // must be straight line move by 2 cells
        int dr = tr - fr, dc = tc - fc;
        bool straight = (dr == 0 && Math.Abs(dc) == 2) || (dc == 0 && Math.Abs(dr) == 2);
        if (!straight) return false;

        // from must have a peg; to must be empty
        if (b[fr, fc] != 1 || b[tr, tc] != 0) return false;

        // jumped cell must exist and contain a peg
        int jr = (fr + tr) / 2;
        int jc = (fc + tc) / 2;
        if (Mask[jr, jc] == -1) return false;
        if (b[jr, jc] != 1) return false;

        return true;
    }

    static void Apply(int[,] b, (int from, int to) m)
    {
        var (fr, fc) = IdToRC[m.from];
        var (tr, tc) = IdToRC[m.to];
        int jr = (fr + tr) / 2, jc = (fc + tc) / 2;

        b[fr, fc] = 0; // from becomes empty
        b[jr, jc] = 0; // jumped peg removed
        b[tr, tc] = 1; // to becomes peg
    }

    static List<(int from, int to)> AllLegalMoves(int[,] b)
    {
        var list = new List<(int from, int to)>(12);
        for (int id = 1; id < IdToRC.Length; id++)
        {
            var (r, c) = IdToRC[id];
            if (b[r, c] != 1) continue;

            // four directions: up/down/left/right by 2
            var targets = new (int tr, int tc)[] { (r - 2, c), (r + 2, c), (r, c - 2), (r, c + 2) };
            foreach (var (tr, tc) in targets)
            {
                if (tr < 0 || tr >= 7 || tc < 0 || tc >= 7) continue;
                if (Mask[tr, tc] == -1) continue;

                var toId = RCToId[(tr, tc)];
                if (IsLegal(b, (id, toId))) list.Add((id, toId));
            }
        }
        return list;
    }
}
