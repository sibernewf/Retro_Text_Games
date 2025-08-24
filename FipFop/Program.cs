using System;
using System.Globalization;
using System.Linq;

namespace FipFop
{
    internal static class Program
    {
        private static readonly Random Rng = new Random();
        private static double q;                // per-puzzle seed
        private static char[] board = new char[10];
        private static int guesses;

        static void Main()
        {
            Console.Title = "FIPFOP — Flip-Flop Game (BASIC conversion)";
            NewPuzzle();                         // start with a fresh puzzle

            while (true)
            {
                PrintHeaderOnce();
                PrintBoard();

                int n = ReadMove();
                if (n == -1) return;             // quit on 'q' key
                if (n == 0) { ResetSamePuzzle(); continue; }
                if (n == 11) { NewPuzzle(); continue; }

                // Flip at chosen position
                Flip(n - 1);

                // Flip a second (deterministic) position depending on what we just did
                int second = (board[n - 1] == '0')
                    ? SecondIndexFromFirstFormula(n)
                    : SecondIndexFromSecondFormula(n);

                if (second != n) Flip(second - 1);

                guesses++;
                if (Solved())
                {
                    Console.WriteLine();
                    if (guesses <= 12)
                        Console.WriteLine($"VERY GOOD. YOU GUESSED IT IN ONLY {guesses} GUESSES!!!!");
                    else
                        Console.WriteLine($"TRY HARDER NEXT TIME. IT TOOK YOU {guesses} GUESSES");

                    if (!AskYesNo("DO YOU WANT TO DO ANOTHER PUZZLE? "))
                        return;

                    NewPuzzle();
                }
            }
        }

        // ---------------- gameplay helpers ----------------

        private static void NewPuzzle()
        {
            // “RANDOMIZE” + Q = RND(Y) analogue
            q = Rng.NextDouble() + 0.0001;          // keep nonzero to avoid div-by-zero in formulas
            for (int i = 0; i < 10; i++) board[i] = 'X';
            guesses = 0;

            Console.WriteLine();
            Console.WriteLine("HERE IS THE STARTING LINE OF X'S:");
        }

        private static void ResetSamePuzzle()
        {
            for (int i = 0; i < 10; i++) board[i] = 'X';
            guesses = 0;
            Console.WriteLine();
            Console.WriteLine("HERE IS THE STARTING LINE OF X'S:");
        }

        private static void PrintHeaderOnce()
        {
            if (_printedHeader) return;
            _printedHeader = true;
            Console.WriteLine("THE OBJECT OF THIS PUZZLE IS TO CHANGE THIS:");
            Console.WriteLine("X X X X X X X X X X");
            Console.WriteLine();
            Console.WriteLine("TO THIS:");
            Console.WriteLine("0 0 0 0 0 0 0 0 0 0");
            Console.WriteLine();
            Console.WriteLine("BY TYPING IN THE NUMBER CORRESPONDING TO THE POSITION OF THE LETTER.");
            Console.WriteLine("ON SOME NUMBERS, ONE POSITION WILL CHANGE; ON OTHERS, TWO WILL CHANGE.");
            Console.WriteLine("TO RESET THE LINE TO ALL X'S, TYPE 0 (ZERO). TO START A NEW PUZZLE");
            Console.WriteLine("IN THE MIDDLE OF A GAME, TYPE 11 (ELEVEN).");
            Console.WriteLine("PRESS 'Q' AT ANY TIME TO QUIT.");
            Console.WriteLine();
        }
        private static bool _printedHeader = false;

        private static void PrintBoard()
        {
            Console.WriteLine("1 2 3 4 5 6 7 8 9 10");
            Console.WriteLine(string.Join(' ', board));
            Console.WriteLine();
        }

        private static void Flip(int idx)
        {
            board[idx] = (board[idx] == 'X') ? '0' : 'X';
        }

        private static bool Solved() => board.All(c => c == '0');

        private static int ReadMove()
        {
            while (true)
            {
                Console.Write("INPUT THE NUMBER? ");
                var s = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(s)) continue;

                if (s.Equals("Q", StringComparison.OrdinalIgnoreCase))
                    return -1; // quit signal

                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                {
                    if (n == 0 || n == 11 || (n >= 1 && n <= 10))
                        return n;
                }
                Console.WriteLine("ILLEGAL ENTRY--TRY AGAIN");
            }
        }

        // ------------- “mystery” second-index formulas -------------
        private static int SecondIndexFromFirstFormula(int n)
        {
            double r;
            try
            {
                double term1 = Math.Tan(q * n / Math.Max(1e-6, (q - n)));
                double term2 = Math.Sin(q / Math.Max(1e-6, n));
                double term3 = 0.326 * Math.Sin(0.8 * n);
                r = Fractional(term1 - term2 + term3);
            }
            catch { r = Fractional(Rng.NextDouble()); }

            return 1 + (int)Math.Floor(Math.Clamp(r, 0.0, 0.999999) * 10.0);
        }

        private static int SecondIndexFromSecondFormula(int n)
        {
            double r;
            try
            {
                double sinA = Math.Sin(n * 2.0 + q);
                double cotB = Cot(q / Math.Max(1e-6, n) + q);
                double expr = 0.592 * cotB / Math.Max(1e-6, sinA) - Math.Cos(n);
                r = Fractional(expr);
            }
            catch { r = Fractional(Rng.NextDouble()); }

            return 1 + (int)Math.Floor(Math.Clamp(r, 0.0, 0.999999) * 10.0);
        }

        private static double Fractional(double x)
        {
            if (double.IsNaN(x) || double.IsInfinity(x)) return Rng.NextDouble();
            x -= Math.Floor(x);
            return x;
        }

        private static double Cot(double x)
        {
            double s = Math.Sin(x);
            double c = Math.Cos(x);
            if (Math.Abs(s) < 1e-9) s = Math.CopySign(1e-9, s == 0 ? 1 : s);
            return c / s;
        }

        private static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine()?.Trim().ToUpperInvariant();
                if (s is "Y" or "YES") return true;
                if (s is "N" or "NO") return false;
                if (s == "Q") return false; // allow quit here too
            }
        }
    }
}
