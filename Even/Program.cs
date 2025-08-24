using System;
using System.Collections.Generic;
using System.Linq;

namespace EvenPickUpGame
{
    internal static class Program
    {
        static void Main()
        {
            Console.Title = "EVEN / EVEN 1 (BASIC conversion)";
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("EVEN — PICK UP AN EVEN NUMBER OF OBJECTS");
                Console.WriteLine("1) EVEN  (deterministic, perfect play)");
                Console.WriteLine("2) EVEN 1 (cybernetic learner)");
                Console.WriteLine("Q) Quit");
                Console.Write("Choose: ");
                var c = Console.ReadLine()?.Trim().ToUpperInvariant();
                if (string.IsNullOrEmpty(c)) continue;
                if (c == "Q") return;

                switch (c)
                {
                    case "1":
                        EvenDeterministic.Run();
                        break;
                    case "2":
                        EvenLearner.Run();
                        break;
                    default:
                        Console.WriteLine("Please choose 1, 2, or Q.");
                        break;
                }
            }
        }

        // -------------------- Helpers --------------------
        internal static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (int.TryParse(s, out var n) && n >= min && n <= max)
                    return n;
                Console.WriteLine($"Enter a number from {min} to {max}.");
            }
        }

        internal static bool ReadYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s is "Y" or "YES") return true;
                if (s is "N" or "NO") return false;
                Console.WriteLine("Please answer YES or NO.");
            }
        }
    }

    // =========================================================
    // EVEN (deterministic). Computer uses search to force a win.
    // State = (remaining, myParity, oppParity). We only need myParity.
    // =========================================================
    internal static class EvenDeterministic
    {
        private static Dictionary<(int rem, int myParity), bool> _winMemo = null!;
        private static Dictionary<(int rem, int myParity), int> _bestMove = null!;

        public static void Run()
        {
            Console.WriteLine();
            Console.WriteLine("THIS IS A TWO-PERSON GAME CALLED 'EVEN'.");
            Console.WriteLine("AN ODD NUMBER OF OBJECTS ARE ON THE TABLE.");
            Console.WriteLine("YOU MAY REMOVE 1 TO 4 EACH TURN. NO SKIPS.");
            Console.WriteLine("THE WINNER IS THE PLAYER WHO ENDS WITH AN EVEN TOTAL PICKED UP.");
            Console.WriteLine();

            // Let the user pick the initial odd total (classic listing often used 27)
            int total = Program.ReadInt("Pick an odd starting total (9–51 recommended): ", 3, 199);
            if (total % 2 == 0) total++; // ensure odd

            bool youFirst = Program.ReadYesNo("Do you want to go first? (YES/NO): ");

            int remaining = total;
            int yourTotal = 0, compTotal = 0;

            // prepare memo tables
            _winMemo = new();
            _bestMove = new();

            // loop
            while (remaining > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"TOTAL = {remaining}");

                if (youFirst)
                {
                    int maxTake = Math.Min(4, remaining);
                    int you = Program.ReadInt("WHAT IS YOUR MOVE? ", 1, maxTake);
                    yourTotal += you;
                    remaining -= you;
                    if (remaining == 0) break;
                }

                // Computer move (perfect)
                int comp = ChooseWinningMove(remaining, compTotal % 2); // myParity is computer's parity
                if (comp == 0) comp = Math.Min(1, remaining); // fallback, never happens
                comp = Math.Min(comp, remaining);
                Console.WriteLine($"I PICK UP {comp}.");
                compTotal += comp;
                remaining -= comp;
                Console.WriteLine($"MY TOTAL IS {compTotal}.");
                if (remaining == 0) break;

                // now it's the user's turn next iteration
                youFirst = true;
            }

            Console.WriteLine();
            Console.WriteLine("THAT IS ALL OF THE OBJECTS.");
            Console.WriteLine($"MY TOTAL IS {compTotal}   YOUR TOTAL IS {yourTotal}");
            if (compTotal % 2 == 0)
                Console.WriteLine("I WIN!");
            else
                Console.WriteLine("YOU WIN!");
            Console.WriteLine();
        }

        // Return a move 1..4 that leads to a forced win if possible; otherwise 1.
        private static int ChooseWinningMove(int remaining, int myParity)
        {
            // try each legal move; if any forces a win, pick it
            int best = 1;
            for (int take = 1; take <= 4 && take <= remaining; take++)
            {
                bool win = CanForceWin(remaining - take, myParity ^ (take % 2));
                if (win) return take;
            }
            return best;
        }

        // true if current player to move (the computer in our decision point)
        // can force a win from state (remaining, myParityAtThisPoint)
        private static bool CanForceWin(int remaining, int myParity)
        {
            // base case: no objects left; did I end with even?
            if (remaining == 0)
                return myParity == 0; // even parity wins

            var key = (remaining, myParity);
            if (_winMemo.TryGetValue(key, out var memo)) return memo;

            // Opponent moves next. If for ALL their legal moves I can still force a win
            // on my following turn, then current state is winning; otherwise losing.
            // Equivalently: if there exists an opponent move that makes me lose, then I cannot force win.
            for (int oppTake = 1; oppTake <= 4 && oppTake <= remaining; oppTake++)
            {
                int rem2 = remaining - oppTake;
                int myParityNext = myParity; // opponent parity change doesn't affect my parity
                // After opponent, it's me again: can I force a win from that state?
                bool iWinFromNext = false;
                for (int myTake = 1; myTake <= 4 && myTake <= rem2; myTake++)
                {
                    if (CanForceWin(rem2 - myTake, myParityNext ^ (myTake % 2)))
                    {
                        iWinFromNext = true;
                        break;
                    }
                }
                if (!iWinFromNext)
                {
                    _winMemo[key] = false;
                    return false; // opponent can spoil
                }
            }
            _winMemo[key] = true;
            return true;
        }
    }

    // =========================================================
    // EVEN 1 (learning). Weighted “beads” per remaining count.
    // Reinforce on win, penalize on loss. Type 0 to quit this mode.
    // =========================================================
    internal static class EvenLearner
    {
        private static readonly Random Rng = new();
        // weights[remaining][move] -> weight (move in 1..4)
        private static readonly Dictionary<int, Dictionary<int, int>> Weights = new();

        public static void Run()
        {
            Console.WriteLine();
            if (Program.ReadYesNo("DO YOU WANT INSTRUCTIONS (YES OR NO)? "))
            {
                Console.WriteLine();
                Console.WriteLine("THE GAME IS PLAYED AS FOLLOWS:");
                Console.WriteLine("AT THE BEGINNING OF A GAME, A RANDOM ODD NUMBER OF CHIPS ARE");
                Console.WriteLine("PLACED ON THE BOARD. ON EACH TURN, A PLAYER MUST TAKE ONE,");
                Console.WriteLine("TWO, THREE, OR FOUR CHIPS. THE WINNER IS THE PLAYER WHO");
                Console.WriteLine("FINISHES WITH A TOTAL NUMBER OF CHIPS EQUAL TO AN EVEN NUMBER.");
                Console.WriteLine("THE COMPUTER STARTS OUT KNOWING ONLY THE RULES OF THE GAME.");
                Console.WriteLine("IT GRADUALLY LEARNS TO PLAY WELL. TRY TO BEAT IT!");
                Console.WriteLine("TO QUIT AT ANY TIME, TYPE '0' AS YOUR MOVE.");
                Console.WriteLine();
            }

            Console.WriteLine("Starting NEW learning session. (Tip: play several rounds!)");
            Console.WriteLine();

            while (true)
            {
                int total = RandomOdd(9, 25);
                int remaining = total;
                int yourTotal = 0, compTotal = 0;

                var trace = new List<(int remainingBefore, int moveTaken)>();

                Console.WriteLine($"THERE ARE {remaining} CHIPS ON THE BOARD.");

                while (remaining > 0)
                {
                    // Computer moves first/second? The original variant usually lets the computer start randomly.
                    // We'll let the HUMAN move first for variety.
                    // ---- Human move ----
                    int human = ReadHumanMove(remaining);
                    if (human == 0) // quit the learning mode entirely
                    {
                        Console.WriteLine("READY");
                        return;
                    }
                    yourTotal += human;
                    remaining -= human;
                    if (remaining == 0) break;

                    // ---- Computer move ----
                    int comp = ChooseWeightedMove(remaining);
                    trace.Add((remaining, comp));
                    remaining -= comp;
                    compTotal += comp;
                    Console.WriteLine($"COMPUTER TAKES {comp} CHIPS LEAVING {remaining}.");
                    if (remaining == 0) break;
                }

                // Decide winner and train
                bool compWon = (compTotal % 2 == 0);
                Console.WriteLine(compWon ? "GAME OVER ... I WIN!!" : "GAME OVER ... YOU WIN!");
                Train(trace, compWon);

                Console.WriteLine();
            }
        }

        private static int ReadHumanMove(int remaining)
        {
            while (true)
            {
                Console.Write($"YOUR MOVE (1–{Math.Min(4, remaining)}; 0 to quit): ");
                var s = Console.ReadLine();
                if (int.TryParse(s, out int n))
                {
                    if (n == 0) return 0;
                    if (n >= 1 && n <= Math.Min(4, remaining)) return n;
                }
                Console.WriteLine("Please enter a legal move.");
            }
        }

        private static int RandomOdd(int min, int max)
        {
            int v = Rng.Next(min, max + 1);
            return v % 2 == 1 ? v : v + 1 == max + 1 ? v - 1 : v + 1;
        }

        private static void EnsureWeights(int remaining)
        {
            if (!Weights.TryGetValue(remaining, out var row))
            {
                row = new Dictionary<int, int>();
                for (int m = 1; m <= 4; m++) row[m] = 1; // equal start
                Weights[remaining] = row;
            }
        }

        private static int ChooseWeightedMove(int remaining)
        {
            EnsureWeights(remaining);
            var row = Weights[remaining];

            // limit to legal moves
            var legal = row.Where(kv => kv.Key <= remaining).ToArray();
            int sum = legal.Sum(kv => Math.Max(1, kv.Value));
            int pick = Rng.Next(1, sum + 1);
            int cum = 0;
            foreach (var (move, weight) in legal)
            {
                cum += Math.Max(1, weight);
                if (pick <= cum) return move;
            }
            return Math.Min(1, remaining);
        }

        private static void Train(List<(int remainingBefore, int moveTaken)> trace, bool compWon)
        {
            // Simple reinforcement:
            // If won: +3 to each used move at the encountered remaining.
            // If lost: -1 (but keep at least 1).
            int delta = compWon ? 3 : -1;

            foreach (var (rem, mv) in trace)
            {
                EnsureWeights(rem);
                int cur = Weights[rem][mv];
                cur = Math.Max(1, cur + delta);
                Weights[rem][mv] = cur;
            }
        }
    }
}
