using System;
using System.Linq;

namespace NumberWizard
{
    class Program
    {
        static void Main()
        {
            Console.Title = "The Number Wizard";
            var rng = new Random();

            // A[i] == true means number i is still available.
            // BASIC used A(1)..A(10) for 0..9; A(1) (i.e., 0) never gets disabled.
            bool[] A = Enumerable.Repeat(true, 10).ToArray();
            int T = 8;                 // goes allowed
            A[0] = true;               // zero may be used as often as you like

            while (true)
            {
                Console.Clear();

                // Show which numbers 1..9 are still available
                // and count how many have been used so far (V).
                int V = 0;
                for (int i = 1; i <= 9; i++)
                {
                    if (A[i]) Console.Write(i + " ");
                    else { Console.Write("  "); V++; }
                }
                Console.WriteLine();

                // If 9 numbers are gone (all 1..9), you win.
                if (V == 9)
                {
                    Console.WriteLine("YOU WON");
                    break;
                }

                Console.WriteLine($"YOU'VE {T} TURNS LEFT");

                // Throw two dice: C and B (1..6)
                int C = rng.Next(1, 7);
                int B = rng.Next(1, 7);
                Console.WriteLine($"THE DICE THROW IS {C}, {B}");

                // Double gives you an extra go: +2 then we subtract 1 below -> net +1
                if (B == C) T += 2;

                // Each cycle costs one turn.
                T -= 1;
                if (T <= 0)
                {
                    Console.WriteLine("THE WIZARD WON");
                    break;
                }

                // Read two numbers from the player
                int need = B + C;
                int n = ReadInt("Enter first number (0–9): ");
                int m = ReadInt("Enter second number (0–9): ");

                // Validate like the BASIC code
                if (m > 9 || n > 9)
                {
                    Console.WriteLine("TOO BIG - TRY AGAIN");
                    continue; // back to redraw (you already spent the turn)
                }
                if (m < 0 || n < 0) continue;

                // Must add up to the dice total and both must be available.
                if (m + n != need || !A[m] || !A[n])
                    continue;

                // Mark both numbers as used (0 never disappears because we never flip A[0] off)
                A[m] = false;
                A[n] = false;
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();
                if (int.TryParse(s, out int v)) return v;
                Console.WriteLine("Please enter a whole number.");
            }
        }
    }
}
