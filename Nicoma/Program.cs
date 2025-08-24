using System;
using System.Globalization;
using System.Threading;

namespace NICOMA
{
    internal static class Program
    {
        // Chinese Remainder Theorem base = 3 * 5 * 7 = 105
        private const int ModBase = 105;

        static void Main()
        {
            Console.Title = "NICOMA — Computer Guesses Your Number";

            Console.WriteLine("BOOMERANG PUZZLE FROM *ARITHMETICA* OF NICOMACHUS (A.D. ~120)\n");

            while (true)
            {
                Console.WriteLine("PLEASE THINK OF A WHOLE NUMBER BETWEEN 1 AND 100.\n");

                int r3  = ReadRemainder(3);
                int r5  = ReadRemainder(5);
                int r7  = ReadRemainder(7);

                Console.WriteLine("\nLET ME THINK A MOMENT...");
                Thread.Sleep(600);

                // Reconstruct using CRT:
                // 70 ≡ 1 (mod 3), 21 ≡ 1 (mod 5), 15 ≡ 1 (mod 7)
                int x = (r3 * 70) + (r5 * 21) + (r7 * 15);
                int guess = x % ModBase;
                if (guess == 0) guess = ModBase; // 0 corresponds to 105

                if (guess < 1 || guess > 100)
                {
                    Console.WriteLine("\nI FEAR YOUR ARITHMETIC IS IN ERROR.");
                    if (!AskYesNo("\nLET'S TRY ANOTHER? (Y/N) ")) break;
                    Console.WriteLine();
                    continue;
                }

                Console.Write($"\nYOUR NUMBER WAS {guess}, RIGHT? ");
                bool right = AskYesNo(""); // use same line
                if (!right)
                {
                    Console.WriteLine("EH? I DON'T UNDERSTAND 'NO'?  (Kidding—double-check your remainders!)");
                }
                else
                {
                    Console.WriteLine("HOW ABOUT THAT!");
                }

                if (!AskYesNo("\nLET'S TRY ANOTHER? (Y/N) ")) break;
                Console.WriteLine();
            }

            Console.WriteLine("\nGOODBYE.");
        }

        private static int ReadRemainder(int divisor)
        {
            while (true)
            {
                Console.Write($"YOUR NUMBER DIVIDED BY {divisor} HAS A REMAINDER OF? ");
                string s = (Console.ReadLine() ?? "").Trim();

                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int r)
                    && r >= 0 && r < divisor)
                {
                    return r;
                }

                Console.WriteLine($"PLEASE ENTER AN INTEGER BETWEEN 0 AND {divisor - 1}.");
            }
        }

        private static bool AskYesNo(string prompt)
        {
            if (!string.IsNullOrEmpty(prompt))
                Console.Write(prompt);

            string s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            while (s.Length == 0 || (s[0] != 'Y' && s[0] != 'N'))
            {
                Console.Write("TRY 'YES' OR 'NO': ");
                s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            }
            return s[0] == 'Y';
        }
    }
}
