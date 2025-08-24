using System;
using System.Globalization;

namespace Craps
{
    internal static class Program
    {
        private static readonly Random Rng = new Random();

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "CRAPS — Console Edition";

            PrintHeader();
            var bankroll = AskStartingBankroll(defaultBankroll: 95);

            Console.WriteLine();
            Console.WriteLine($"SPLENDID... You are given ${bankroll} to play with.");
            Console.WriteLine("You roll first. Type Q at any bet prompt to quit.");
            Console.WriteLine();

            while (bankroll > 0)
            {
                var bet = AskBet(bankroll);
                if (bet == null) // player quit
                    break;

                // Come-out roll
                var (d1, d2) = RollDice();
                var total = d1 + d2;

                Console.WriteLine($"YOU ROLL {d1} AND {d2}{Describe(total)}");

                if (total is 7 or 11)
                {
                    bankroll += bet.Value;
                    Console.WriteLine($"YOU WIN!   New bankroll: ${bankroll}");
                    Console.WriteLine();
                    continue;
                }
                if (total is 2 or 3 or 12)
                {
                    bankroll -= bet.Value;
                    Console.WriteLine($"CRAP OUT!  New bankroll: ${bankroll}");
                    Console.WriteLine();
                    continue;
                }

                // Point established
                var point = total;
                Console.WriteLine($"SO MY POINT IS {point}");
                Console.WriteLine("ROLL AGAIN...");

                // Point phase
                while (true)
                {
                    (d1, d2) = RollDice();
                    total = d1 + d2;
                    Console.WriteLine($"YOU ROLL {d1} AND {d2}{Describe(total)}");

                    if (total == point)
                    {
                        bankroll += bet.Value;
                        Console.WriteLine($"YOU MAKE YOUR POINT! YOU WIN.");
                        Console.WriteLine($"New bankroll: ${bankroll}");
                        Console.WriteLine();
                        break;
                    }
                    if (total == 7)
                    {
                        bankroll -= bet.Value;
                        Console.WriteLine("YOU ROLL A 7 AND LOSE...");
                        Console.WriteLine($"New bankroll: ${bankroll}");
                        Console.WriteLine();
                        break;
                    }

                    Console.WriteLine("ROLL AGAIN...");
                }
            }

            if (bankroll <= 0)
            {
                Console.WriteLine("YOU HAVE RUN OUT OF MONEY... SORRY ABOUT THAT.");
            }
            Console.WriteLine("THANKS FOR THE GAME. (Press any key to exit.)");
            Console.ReadKey(true);
        }

        // ─────────────────────────────────────────────────────────────

        private static void PrintHeader()
        {
            Console.WriteLine("THIS DEMONSTRATION SIMULATES A CRAPS GAME WITH THE COMPUTER.");
            Console.WriteLine("I’M YOUR OPPONENT. THE RULES ARE SIMPLE:");
            Console.WriteLine("  # 7 OR 11 ON THE FIRST ROLL WINS");
            Console.WriteLine("  # 2 OR 3 OR 12 ON THE FIRST ROLL LOSES");
            Console.WriteLine("  # ANY OTHER NUMBER BECOMES YOUR POINT; YOU KEEP ROLLING.");
            Console.WriteLine("    IF YOU MAKE YOUR POINT, YOU WIN. IF YOU ROLL A 7, YOU LOSE.");
            Console.WriteLine("NO COINS PERMITTED... JUST BILLS, PLEASE.\n");
        }

        private static int AskStartingBankroll(int defaultBankroll)
        {
            while (true)
            {
                Console.Write($"Enter starting bankroll (press Enter for ${defaultBankroll}): ");
                var s = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(s))
                    return defaultBankroll;

                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var bank) && bank > 0)
                    return bank;

                Console.WriteLine("Please enter a positive whole number.");
            }
        }

        /// <summary>
        /// Ask for a bet; returns null if player quits.
        /// </summary>
        private static int? AskBet(int bankroll)
        {
            while (true)
            {
                Console.Write($"HOW MUCH DO YOU BET? (Bankroll ${bankroll}) ");
                var s = Console.ReadLine()?.Trim();

                if (string.Equals(s, "Q", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s, "QUIT", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var bet))
                {
                    if (bet <= 0)
                    {
                        Console.WriteLine("Bet must be at least $1.");
                        continue;
                    }
                    if (bet > bankroll)
                    {
                        Console.WriteLine("Don’t try to bet more than you have, please.");
                        continue;
                    }
                    return bet;
                }

                Console.WriteLine("Enter a whole-dollar amount (or Q to quit).");
            }
        }

        private static (int d1, int d2) RollDice()
        {
            // classic 2d6
            int d1 = 1 + Rng.Next(6);
            int d2 = 1 + Rng.Next(6);
            return (d1, d2);
        }

        private static string Describe(int total) =>
            total switch
            {
                2 => " — SNAKE EYES... CRAP OUT.",
                3 => " — ACE-DEUCE... CRAP OUT.",
                7 => " — SEVEN!",
                11 => " — YO-LEVEN!",
                12 => " — BOXCARS... CRAP OUT.",
                _ => " ... ROLL AGAIN OR MAKE YOUR POINT"
            };
    }
}
