using System;
using System.Globalization;

namespace Roulette
{
    class Program
    {
        static void Main()
        {
            Console.Title = "ROULET — European Roulette (Single Zero)";
            var rng = new Random();

            PrintWelcome();

            long net = 0; // cumulative winnings (can be negative)

            do
            {
                // --- Collect bets for this round (all optional) ---
                Bet? betOddEven = null;
                if (AskYesNo("DO YOU WANT TO BET AN ODD OR EVEN NUMBER? "))
                {
                    int side = AskChoice("TYPE ODD OR EVEN: ", new[] { "ODD", "EVEN" });
                    long amount = AskBetAmount();
                    betOddEven = new Bet
                    {
                        Kind = BetKind.OddEven,
                        Param = side == 0 ? 1 : 0, // 1=ODD, 0=EVEN
                        Amount = amount
                    };
                }

                Bet? betRedBlack = null;
                if (AskYesNo("DO YOU WANT TO BET A RED OR BLACK NUMBER? "))
                {
                    int side = AskChoice("TYPE RED OR BLACK: ", new[] { "RED", "BLACK" });
                    long amount = AskBetAmount();
                    betRedBlack = new Bet
                    {
                        Kind = BetKind.RedBlack,
                        Param = side == 0 ? 1 : 0, // 1=RED, 0=BLACK
                        Amount = amount
                    };
                }

                Bet? betColumn = null;
                if (AskYesNo("DO YOU WANT TO BET A COLUMN OF NUMBERS? "))
                {
                    int col = AskInt("ENTER COLUMN 1, 2 OR 3: ", 1, 3);
                    long amount = AskBetAmount();
                    betColumn = new Bet
                    {
                        Kind = BetKind.Column,
                        Param = col, // 1..3
                        Amount = amount
                    };
                }

                Bet? betNumber = null;
                if (AskYesNo("DO YOU WANT TO BET A NUMBER? "))
                {
                    int n = AskInt("WHAT IS YOUR NUMBER (0–36)? ", 0, 36);
                    long amount = AskBetAmount();
                    betNumber = new Bet
                    {
                        Kind = BetKind.SingleNumber,
                        Param = n, // 0..36
                        Amount = amount
                    };
                }

                Console.WriteLine();

                // --- Spin the wheel ---
                int result = rng.Next(0, 37); // 0..36
                string color = ColorOf(result); // RED / BLACK / "0"
                int column = ColumnOf(result);  // 0 (for 0) or 1..3
                Console.WriteLine($"THE NUMBER IS {result}  {color}{(column == 0 ? "" : $", COLUMN {column}")}");

                // --- Resolve all bets ---
                long roundNet = 0;

                if (betOddEven is not null)
                {
                    long delta = SettleOddEven(betOddEven, result);
                    roundNet += delta;
                    PrintPayout(delta, "ODD-EVEN BET");
                }

                if (betRedBlack is not null)
                {
                    long delta = SettleRedBlack(betRedBlack, result);
                    roundNet += delta;
                    PrintPayout(delta, "RED-BLACK BET");
                }

                if (betColumn is not null)
                {
                    long delta = SettleColumn(betColumn, result);
                    roundNet += delta;
                    PrintPayout(delta, "COLUMN BET");
                }

                if (betNumber is not null)
                {
                    long delta = SettleNumber(betNumber, result);
                    roundNet += delta;
                    PrintPayout(delta, "NUMBER BET");
                }

                if (roundNet == 0)
                    Console.WriteLine("YOU BROKE EVEN THIS TIME.");
                else if (roundNet > 0)
                    Console.WriteLine($"YOU WIN ${roundNet} ON THIS ROUND.");
                else
                    Console.WriteLine($"YOU LOSE ${-roundNet} ON THIS ROUND.");

                net += roundNet;

                Console.WriteLine();
                Console.WriteLine($"YOU HAVE WON A TOTAL OF ${net} THUS FAR.");
            }
            while (AskYesNo("\nDO YOU WANT TO PLAY AGAIN? "));

            Console.WriteLine("\nTHANKS FOR PLAYING.");
        }

        // =================== Bets & Settlement ===================

        enum BetKind { OddEven, RedBlack, Column, SingleNumber }

        sealed class Bet
        {
            public BetKind Kind;
            public int Param;      // OddEven: 1=ODD,0=EVEN | RedBlack:1=RED,0=BLACK | Column:1..3 | Number:0..36
            public long Amount;    // dollars (whole)
        }

        static long SettleOddEven(Bet b, int n)
        {
            if (n == 0) return -b.Amount; // 0 loses
            bool isOdd = (n % 2) != 0;
            bool win = (b.Param == 1 && isOdd) || (b.Param == 0 && !isOdd);
            return win ? b.Amount : -b.Amount; // 1:1
        }

        static long SettleRedBlack(Bet b, int n)
        {
            if (n == 0) return -b.Amount;
            string color = ColorOf(n);
            bool isRed = color == "RED";
            bool win = (b.Param == 1 && isRed) || (b.Param == 0 && !isRed);
            return win ? b.Amount : -b.Amount; // 1:1
        }

        static long SettleColumn(Bet b, int n)
        {
            if (n == 0) return -b.Amount;
            int col = ColumnOf(n); // 1..3
            bool win = col == b.Param;
            return win ? 2 * b.Amount : -b.Amount; // 2:1
        }

        static long SettleNumber(Bet b, int n)
        {
            bool win = n == b.Param;
            return win ? 35 * b.Amount : -b.Amount; // 35:1
        }

        // =================== Wheel Helpers ===================

        // European single-zero wheel color set (standard layout colours)
        // RED numbers:
        static readonly int[] Reds = new[]
        {
            1,3,5,7,9,12,14,16,18,19,21,23,25,27,30,32,34,36
        };

        static string ColorOf(int n)
        {
            if (n == 0) return "0";
            // simple membership test in Reds set
            foreach (var r in Reds) if (r == n) return "RED";
            return "BLACK";
        }

        // Column 1: 1,4,7,...34   Column 2: 2,5,8,...35   Column 3: 3,6,9,...36
        static int ColumnOf(int n) => n == 0 ? 0 : ((n - 1) % 3) + 1;

        // =================== UI Helpers ===================

        static void PrintWelcome()
        {
            Console.WriteLine("WELCOME TO MONTE CARLO AND OUR EUROPEAN ROULETTE TABLE.");
            Console.WriteLine("I WISH YOU THE BEST OF LUCK.\n");
            Console.WriteLine("THIS IS A GAME OF ROULETTE.  YOU ARE ALLOWED TO BET:");
            Console.WriteLine("  AN ODD OR EVEN NUMBER AND/OR A BLACK OR RED NUMBER AND/OR");
            Console.WriteLine("  A COLUMN OF NUMBERS AND/OR A NUMBER ITSELF.  NUMBERS RANGE");
            Console.WriteLine("  FROM 0 TO 36.  IF 0 APPEARS, THE BANK COLLECTS ALL BETS");
            Console.WriteLine("  EXCEPT THOSE BET ON THE NUMBER 0.  THE PAYOFFS ARE AS FOLLOWS:");
            Console.WriteLine("    ODD OR EVEN   :  1 TO 1");
            Console.WriteLine("    RED OR BLACK  :  1 TO 1");
            Console.WriteLine("    A COLUMN      :  2 TO 1");
            Console.WriteLine("    A NUMBER      : 35 TO 1");
            Console.WriteLine("YOU ARE ALLOWED TO BET FROM $1 TO $10,000, BUT THE TABLE WILL ONLY");
            Console.WriteLine("ACCEPT BETS OF WHOLE DOLLARS (NO CENTS).\n");
        }

        static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s.StartsWith("Y")) return true;
                if (s.StartsWith("N")) return false;
                Console.WriteLine("PLEASE TYPE YES OR NO.");
            }
        }

        static int AskChoice(string prompt, string[] optionsUpper)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                for (int i = 0; i < optionsUpper.Length; i++)
                {
                    if (s == optionsUpper[i]) return i;
                }
                Console.WriteLine($"PLEASE TYPE {string.Join(" OR ", optionsUpper)}.");
            }
        }

        static int AskInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) && v >= min && v <= max)
                    return v;
                Console.WriteLine($"ENTER AN INTEGER BETWEEN {min} AND {max}.");
            }
        }

        static long AskBetAmount()
        {
            while (true)
            {
                Console.Write("HOW MUCH DO YOU WANT TO BET? $");
                var s = Console.ReadLine();
                if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out long v) && v >= 1 && v <= 10_000)
                    return v;
                Console.WriteLine("SORRY, THE TABLE ACCEPTS WHOLE-DOLLAR BETS FROM $1 TO $10,000.");
            }
        }

        static void PrintPayout(long delta, string label)
        {
            if (delta > 0) Console.WriteLine($"YOU WIN ${delta} FOR YOUR {label.ToUpper()}.");
            else if (delta < 0) Console.WriteLine($"YOU LOSE ${-delta} FOR YOUR {label.ToUpper()}.");
            else Console.WriteLine($"YOU BREAK EVEN ON YOUR {label.ToUpper()}.");
        }
    }
}
