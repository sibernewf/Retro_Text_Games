using System;
using System.Text;

namespace MathDi
{
    internal static class Program
    {
        private static readonly Random Rng = new Random();

        static void Main()
        {
            Console.Title = "MATHDI — Pictorial Addition Practice";
            PrintHeader();

            while (true)
            {
                int d1 = Rng.Next(1, 7);
                int d2 = Rng.Next(1, 7);
                int sum = d1 + d2;

                DrawDice(d1, d2);
                Console.WriteLine("=");
                
                // First try
                if (AskNumber("? ") == sum)
                {
                    Console.WriteLine("RIGHT!");
                    RollAgain();
                    continue;
                }

                // Second chance with hint
                Console.WriteLine("NO, COUNT THE SPOTS AND GIVE ANOTHER ANSWER.");
                if (AskNumber("? ") == sum)
                {
                    Console.WriteLine("RIGHT!");
                }
                else
                {
                    Console.WriteLine($"NO. THE ANSWER IS {sum}");
                }

                RollAgain();
            }
        }

        private static void PrintHeader()
        {
            Console.WriteLine("THIS PROGRAM GENERATES SUCCESSIVE PICTURES OF TWO DICE.");
            Console.WriteLine("WHEN TWO DICE AND AN EQUAL SIGN FOLLOWED BY A QUESTION");
            Console.WriteLine("MARK HAVE BEEN PRINTED, TYPE YOUR ANSWER AND PRESS ENTER.");
            Console.WriteLine("TYPE Q TO QUIT.\n");
        }

        private static int AskNumber(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();
                if (s == null) continue;
                s = s.Trim();
                if (s.Equals("Q", StringComparison.OrdinalIgnoreCase)) Environment.Exit(0);
                if (int.TryParse(s, out int n)) return n;
                Console.WriteLine("Please type a number (or Q to quit).");
            }
        }

        private static void RollAgain()
        {
            Console.WriteLine("\nTHE DICE ROLL AGAIN.....\n");
        }

        // ---- ASCII dice ----
        private static void DrawDice(int a, int b)
        {
            string[] da = RenderDie(a);
            string[] db = RenderDie(b);
            for (int i = 0; i < da.Length; i++)
                Console.WriteLine($"{da[i]}  {db[i]}");
        }

        private static string[] RenderDie(int n)
        {
            // 5x5 die with a dotted border style like vintage listings
            char[,] grid = new char[5, 5];
            for (int r = 0; r < 5; r++)
            for (int c = 0; c < 5; c++)
                grid[r, c] = ' ';

            // border
            for (int c = 0; c < 5; c++) { grid[0, c] = '.'; grid[4, c] = '.'; }
            for (int r = 0; r < 5; r++) { grid[r, 0] = '.'; grid[r, 4] = '.'; }

            // pip coordinates (inside area 1..3)
            void Pip(int r, int c) => grid[r, c] = '*';

            switch (n)
            {
                case 1: Pip(2, 2); break;
                case 2: Pip(1, 1); Pip(3, 3); break;
                case 3: Pip(1, 1); Pip(2, 2); Pip(3, 3); break;
                case 4: Pip(1, 1); Pip(1, 3); Pip(3, 1); Pip(3, 3); break;
                case 5: Pip(1, 1); Pip(1, 3); Pip(2, 2); Pip(3, 1); Pip(3, 3); break;
                case 6: Pip(1, 1); Pip(1, 3); Pip(2, 1); Pip(2, 3); Pip(3, 1); Pip(3, 3); break;
            }

            var lines = new string[5];
            for (int r = 0; r < 5; r++)
            {
                var sb = new StringBuilder(5);
                for (int c = 0; c < 5; c++) sb.Append(grid[r, c]);
                lines[r] = sb.ToString();
            }
            return lines;
        }
    }
}
