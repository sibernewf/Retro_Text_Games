using System;
using System.Collections.Generic;
using System.Linq;

namespace Hexapawn;

internal static class Program
{
    static void Main()
    {
        Console.Title = "HEX – Game of Hexapawn (learning)";
        var engine = new HexEngine();

        PrintHeader();
        if (AskYesNo("INSTRUCTIONS? (Y/N) ")) ShowInstructions();

        int humanWins = 0, computerWins = 0, games = 0;

        while (true)
        {
            Console.WriteLine();
            var result = engine.PlayOneGame();
            if (result == GameResult.HumanQuit) break;

            games++;
            if (result == GameResult.HumanWin) humanWins++; else computerWins++;

            Console.WriteLine();
            Console.WriteLine($"I HAVE WON {computerWins} AND YOU HAVE WON {humanWins} OF {games} GAMES.");
            if (!AskYesNo("ANOTHER GAME? (Y/N) ")) break;
        }

        Console.WriteLine("READY");
    }

    static void PrintHeader()
    {
        Console.WriteLine("HEX – GAME OF HEXAPAWN");
        Console.WriteLine("Type 'q' at any prompt to quit.");
        Console.WriteLine();
    }

    static void ShowInstructions()
    {
        Console.WriteLine();
        Console.WriteLine("THIS PROGRAM PLAYS THE GAME OF HEXAPAWN.");
        Console.WriteLine("HEXAPAWN IS PLAYED WITH CHESS PAWNS ON A 3x3 BOARD.");
        Console.WriteLine("THE PAWNS ARE MOVED AS IN CHESS – A PAWN MOVES FORWARD ONE SQUARE,");
        Console.WriteLine("OR DIAGONALLY TO CAPTURE AN OPPOSING MAN.");
        Console.WriteLine("ON THE BOARD, YOUR PAWNS ARE 'O', THE COMPUTER'S PAWNS ARE 'X'.");
        Console.WriteLine("BOARD NUMBERING (USE THESE WHEN ENTERING MOVES):");
        Console.WriteLine();
        HexEngine.PrintNumbering();
        Console.WriteLine();
        Console.WriteLine("YOU GO FIRST. ENTER MOVES AS 'FROM,TO' (E.G. 8,5).");
        Console.WriteLine("YOU WIN IF YOU REACH THE TOP RANK OR IF I HAVE NO LEGAL MOVE.");
        Console.WriteLine("I LEARN BY ELIMINATING BAD MOVES AFTER I LOSE.");
        Console.WriteLine();
    }

    static bool AskYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim();
            if (s.Equals("q", StringComparison.OrdinalIgnoreCase)) Environment.Exit(0);
            if (s.Length == 0) continue;
            char c = char.ToUpperInvariant(s[0]);
            if (c == 'Y') return true;
            if (c == 'N') return false;
        }
    }
}

internal enum GameResult { HumanWin, ComputerWin, HumanQuit }

internal sealed class HexEngine
{
    // State is a 9-char string, indexes 0..8 = cells 1..9
    // 'X' = computer (moves down), 'O' = human (moves up), '.' = empty
    private const string StartState = "XXX...OOO";

    // “Learning machine”: position -> list of legal computer moves
    private readonly Dictionary<string, List<Move>> book = new(StringComparer.Ordinal);

    // For punishment after a loss
    private readonly List<(string state, Move move)> history = new();

    // --- Top-level game driver
    public GameResult PlayOneGame()
    {
        string state = StartState;
        history.Clear();

        Console.WriteLine("SINCE I'M A GOOD SPORT, YOU'LL ALWAYS GO FIRST.");
        Console.WriteLine();
        PrintBoard(state);

        // Human (O) always first
        while (true)
        {
            // HUMAN TURN
            if (!HasAnyMove(state, 'O'))
            {
                Console.WriteLine("YOU CAN'T MOVE. I WIN.");
                RewardComputerWin();
                return GameResult.ComputerWin;
            }

            var hm = PromptHumanMove(state);
            if (hm == null) return GameResult.HumanQuit;
            state = ApplyMove(state, hm.Value);
            PrintBoard(state);

            if (HasReachedBackRank(state, 'O') || !HasAnyMove(state, 'X'))
            {
                Console.WriteLine("YOU WIN.");
                PunishComputerLoss(); // eliminate the last move(s) that led here
                return GameResult.HumanWin;
            }

            // COMPUTER TURN
            var cm = ChooseComputerMove(state);
            if (cm == null)
            {
                // No available move in book (all deleted) – resign:
                Console.WriteLine("I CAN'T MOVE. I RESIGN. YOU WIN.");
                // Punish: erase the move that led into this state
                PunishComputerLoss(resignAtCurrent: true);
                return GameResult.HumanWin;
            }

            Console.WriteLine($"I MOVE FROM {cm.Value.From} TO {cm.Value.To}");
            history.Add((state, cm.Value));
            state = ApplyMove(state, cm.Value);
            PrintBoard(state);

            if (HasReachedBackRank(state, 'X') || !HasAnyMove(state, 'O'))
            {
                Console.WriteLine("I WIN.");
                RewardComputerWin();
                return GameResult.ComputerWin;
            }
        }
    }

    // --- Human I/O

    private Move? PromptHumanMove(string state)
    {
        while (true)
        {
            Console.Write("WHAT IS YOUR MOVE? (e.g. 8,5) ");
            var s = (Console.ReadLine() ?? "").Trim();
            if (s.Equals("q", StringComparison.OrdinalIgnoreCase)) return null;

            if (TryParseMove(s, out var move) && IsLegalMove(state, move, 'O'))
                return move;

            Console.WriteLine("ILLEGAL MOVE — TRY AGAIN.");
        }
    }

    // --- Computer policy

    private Move? ChooseComputerMove(string state)
    {
        // Ensure book entry exists
        if (!book.TryGetValue(state, out var moves))
        {
            moves = GenerateMoves(state, 'X');
            book[state] = moves;
        }

        if (moves.Count == 0)
        {
            // Learned to avoid this state entirely (i.e., all moves deleted)
            return null;
        }

        // Pick a random remaining move from the “matchbox”
        int idx = Random.Shared.Next(moves.Count);
        return moves[idx];
    }

    private void RewardComputerWin()
    {
        // This minimal learner only eliminates losing moves.
        // (Optional reinforcement could bias toward winning moves; skipped here.)
    }

    private void PunishComputerLoss(bool resignAtCurrent = false)
    {
        // If we resigned because the *current* position had 0 moves,
        // delete the *previous* move that led here.
        int i = resignAtCurrent ? history.Count - 1 : history.Count - 1;

        while (i >= 0)
        {
            var (pos, mv) = history[i];
            if (book.TryGetValue(pos, out var list))
            {
                list.RemoveAll(m => m.From == mv.From && m.To == mv.To);
                if (list.Count > 0) break; // we still have alternatives from this pos; stop backtracking
            }
            i--;
        }
    }

    // --- Moves and rules

    private static bool TryParseMove(string s, out Move move)
    {
        move = default;
        var t = s.Replace(" ", "").Split(',', ';');
        if (t.Length != 2) return false;
        if (!int.TryParse(t[0], out int from)) return false;
        if (!int.TryParse(t[1], out int to)) return false;
        if (from < 1 || from > 9 || to < 1 || to > 9) return false;
        move = new Move(from, to);
        return true;
    }

    private static bool IsLegalMove(string state, Move m, char who)
    {
        var moves = GenerateMoves(state, who);
        return moves.Any(x => x.From == m.From && x.To == m.To);
    }

    private static bool HasAnyMove(string state, char who)
        => GenerateMoves(state, who).Count > 0;

    private static bool HasReachedBackRank(string state, char who)
    {
        if (who == 'O') // human goes up; top rank is cells 1..3
            return state[0] == 'O' || state[1] == 'O' || state[2] == 'O';
        else            // computer goes down; bottom rank is cells 7..9
            return state[6] == 'X' || state[7] == 'X' || state[8] == 'X';
    }

    private static List<Move> GenerateMoves(string state, char who)
    {
        var res = new List<Move>(8);
        int dir = who == 'O' ? -1 : +1; // O moves up (row decreases), X moves down (row increases)

        for (int idx = 0; idx < 9; idx++)
        {
            if (state[idx] != who) continue;

            int r = idx / 3, c = idx % 3;

            // forward one
            int fr = r + dir, fc = c;
            if (In(fr, fc))
            {
                int toIdx = fr * 3 + fc;
                if (state[toIdx] == '.')
                    res.Add(new Move(idx + 1, toIdx + 1));
            }

            // capture diagonals
            foreach (int dc in new[] { -1, +1 })
            {
                fr = r + dir; fc = c + dc;
                if (!In(fr, fc)) continue;
                int toIdx = fr * 3 + fc;
                char enemy = who == 'O' ? 'X' : 'O';
                if (state[toIdx] == enemy)
                    res.Add(new Move(idx + 1, toIdx + 1));
            }
        }
        return res;

        static bool In(int rr, int cc) => rr >= 0 && rr < 3 && cc >= 0 && cc < 3;
    }

    private static string ApplyMove(string state, Move m)
    {
        int fromIdx = m.From - 1;
        int toIdx = m.To - 1;
        char piece = state[fromIdx];

        var chars = state.ToCharArray();
        chars[fromIdx] = '.';
        chars[toIdx] = piece;
        return new string(chars);
    }

    // --- Display

    public static void PrintNumbering()
    {
        Console.WriteLine("NUMBERING:");
        Console.WriteLine(" 1 2 3");
        Console.WriteLine(" 4 5 6");
        Console.WriteLine(" 7 8 9");
    }

    private static void PrintBoard(string state)
    {
        Console.WriteLine();
        Console.WriteLine("BOARD:");
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                char ch = state[r * 3 + c];
                Console.Write(ch == '.' ? '-' : ch);
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }
}

internal readonly record struct Move(int From, int To);
