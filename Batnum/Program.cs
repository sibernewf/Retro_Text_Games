using System;
using System.Linq;

namespace BattleOfNumbers
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("THIS PROGRAM IS A 'BATTLE OF NUMBERS'");
            Console.WriteLine("GAME, WHERE THE COMPUTER IS YOUR OPPONENT.\n");

            while (true)
            {
                int pile = AskInt("ENTER PILE SIZE: ", min: 1);
                int winOption = AskChoice("ENTER WIN OPTION - 1 TO TAKE LAST, 2 TO AVOID LAST: ", new[] { 1, 2 });
                int minTake = AskInt("ENTER MIN (>=1): ", min: 1);
                int maxTake = AskInt($"ENTER MAX (>= {minTake}): ", min: minTake);

                int starter = AskChoice("ENTER START OPTION: 1 COMPUTER FIRST, 2 YOU FIRST: ", new[] { 1, 2 });

                bool computerTurn = (starter == 1);

                Console.WriteLine();

                while (pile > 0)
                {
                    if (computerTurn)
                    {
                        int take = ComputerMove(pile, minTake, maxTake, winOption);
                        // Guard if remaining is smaller than minTake: you can take what's left
                        take = Math.Min(take, pile);
                        if (pile < minTake) take = pile; // forced
                        Console.WriteLine($"COMPUTER TAKES {take} AND LEAVES {pile - take}");
                        pile -= take;

                        if (pile == 0)
                        {
                            if (winOption == 1) Console.WriteLine("COMPUTER TAKES LAST AND WINS.");
                            else Console.WriteLine("COMPUTER TAKES LAST AND LOSES.");
                            break;
                        }
                        computerTurn = false;
                    }
                    else
                    {
                        int take = AskPlayerMove(pile, minTake, maxTake);
                        pile -= take;
                        Console.WriteLine($"YOUR MOVE: {take}, PILE NOW {pile}");

                        if (pile == 0)
                        {
                            if (winOption == 1) Console.WriteLine("CONGRATULATIONS, YOU WIN.");
                            else Console.WriteLine("TOUGH LUCK, YOU LOSE.");
                            break;
                        }
                        computerTurn = true;
                    }
                }

                Console.WriteLine();
                if (!AskYesNo("PLAY AGAIN (Y/N)? ")) break;
                Console.WriteLine();
            }
        }

        // ===== Player I/O =====

        static int AskInt(string prompt, int min)
        {
            while (true)
            {
                Console.Write(prompt + "(or Q to quit) ");
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") Quit();

                if (int.TryParse(s, out int v) && v >= min)
                    return v;

                Console.WriteLine($"Please enter an integer >= {min}.");
            }
        }

        static int AskChoice(string prompt, int[] allowed)
        {
            while (true)
            {
                Console.Write(prompt + "(or Q to quit) ");
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") Quit();

                if (int.TryParse(s, out int v) && allowed.Contains(v))
                    return v;

                Console.WriteLine("Invalid choice.");
            }
        }

        static int AskPlayerMove(int pile, int minTake, int maxTake)
        {
            while (true)
            {
                Console.Write($"YOUR MOVE (take {minTake}..{maxTake}, <= pile; Q to quit): ");
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") Quit();

                if (!int.TryParse(s, out int v))
                {
                    Console.WriteLine("Please enter a number.");
                    continue;
                }

                int allowedMin = Math.Min(minTake, pile); // if pile < minTake, you may take what's left
                int allowedMax = Math.Min(maxTake, pile);

                if (v >= allowedMin && v <= allowedMax)
                    return v;

                Console.WriteLine($"Illegal move. You must take between {allowedMin} and {allowedMax}.");
            }
        }

        static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") Quit();
                if (s.StartsWith("Y")) return true;
                if (s.StartsWith("N")) return false;
            }
        }

        static void Quit()
        {
            Console.WriteLine("Quitting…");
            Environment.Exit(0);
        }

        // ===== Computer Strategy =====
        // Optimal play for the subtraction game with a contiguous move set [a..b].
        // Normal play (take-last WINS):
        //   P-positions are numbers n with n mod (a+b) in {0,1,...,a-1}.
        // Misère play (take-last LOSES):
        //   P-positions are numbers n with n mod (a+b) in {1,2,...,a}.
        // Strategy: if possible, move to a P-position for the opponent; otherwise take the minimum.

        static int ComputerMove(int pile, int a, int b, int winOption)
        {
            // If the pile is smaller than a, the only legal move is to take all.
            if (pile <= a) return pile;

            int m = a + b;
            // Target residues that are P-positions for the OPPONENT after our move.
            bool IsPResidue(int r) =>
                winOption == 1
                    ? r >= 0 && r <= a - 1
                    : r >= 1 && r <= a;

            // Try to find a move x in [a..b] that leaves a P-position.
            for (int x = a; x <= b; x++)
            {
                if (x > pile) break;
                int r = (pile - x) % m;
                if (r < 0) r += m;
                if (IsPResidue(r)) return x;
            }

            // If no such move exists (or pile is awkward), take the minimum legal.
            return Math.Min(a, pile);
        }
    }
}
