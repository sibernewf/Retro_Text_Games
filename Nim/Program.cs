using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NimGame
{
    internal static class Program
    {
        private static readonly Random Rng = new Random();

        static void Main()
        {
            Console.Title = "NIM — Chinese Game of Nim";

            Console.WriteLine("THIS PROGRAM PLAYS NIM.");
            if (AskYesNo("DO YOU WANT INSTRUCTIONS? (Y/N) "))
                ShowInstructions();

            // main session loop
            List<int>? lastSetup = null;
            do
            {
                // Either reuse last arrangement or build a new one
                List<int> piles;
                if (lastSetup != null && AskYesNo("SAME ARRANGEMENT? (Y/N) "))
                    piles = new List<int>(lastSetup);
                else
                    piles = Setup();

                lastSetup = new List<int>(piles);

                bool humanTurn = AskYesNo("DO YOU WANT TO GO FIRST? (Y/N) ");

                // Play one game
                while (piles.Sum() > 0)
                {
                    PrintPiles(piles);

                    if (humanTurn)
                    {
                        HumanMove(piles);
                        if (piles.Sum() == 0)
                        {
                            Console.WriteLine("\nI WON. DO YOU WANT TO PLAY AGAIN? (Y/N) ");
                            break;
                        }
                    }
                    else
                    {
                        ComputerMove(piles);
                        if (piles.Sum() == 0)
                        {
                            Console.WriteLine("\nYOU WON. DO YOU WANT TO PLAY AGAIN? (Y/N) ");
                            break;
                        }
                    }

                    humanTurn = !humanTurn;
                }

            } while (AskYesNo(""));

            Console.WriteLine("GOODBYE.");
        }

        // ----- Setup & UI -----
        private static List<int> Setup()
        {
            int n = ReadInt("HOW MANY PILES? ", 1, 10);

            var piles = new List<int>(n);
            for (int i = 1; i <= n; i++)
            {
                int sticks = ReadInt($"HOW MANY STICKS IN PILE {i}? ", 1, 20);
                piles.Add(sticks);
            }
            Console.WriteLine();
            return piles;
        }

        private static void ShowInstructions()
        {
            Console.WriteLine(@"
NIM IS PLAYED BY TWO PEOPLE ALTERNATELY.
YOU SET UP ANY NUMBER OF PILES (ROWS) WITH ANY POSITIVE
NUMBER OF STICKS IN EACH (MAX 20 PER PILE IN THIS VERSION).
A MOVE CONSISTS OF REMOVING ANY POSITIVE NUMBER OF STICKS
FROM A SINGLE PILE. THE PLAYER WHO TAKES THE LAST STICK WINS.
");
        }

        private static void PrintPiles(IReadOnlyList<int> piles)
        {
            Console.WriteLine();
            Console.WriteLine("PILE NUMBER   STICKS LEFT");
            for (int i = 0; i < piles.Count; i++)
                Console.WriteLine($"{i + 1,5} {piles[i],12}");
            Console.WriteLine();
        }

        private static void HumanMove(List<int> piles)
        {
            while (true)
            {
                int pile = ReadInt("WHICH PILE DO YOU WANT STICKS FROM? ", 1, piles.Count);
                if (piles[pile - 1] == 0)
                {
                    Console.WriteLine("ILLEGAL PILE — THAT PILE HAS NO STICKS.");
                    continue;
                }

                int max = piles[pile - 1];
                int take = ReadInt("HOW MANY STICKS? ", 1, max);
                piles[pile - 1] -= take;

                Console.WriteLine($"\nI'LL TAKE {take} STICK{(take == 1 ? "" : "S")} FROM PILE {pile} .");
                return;
            }
        }

        // ----- Computer move (optimal) -----
        private static void ComputerMove(List<int> piles)
        {
            // Optimal Nim: Compute nim-sum (xor). If non-zero,
            // make a move that leaves nim-sum = 0.
            int nimSum = piles.Aggregate(0, (a, b) => a ^ b);

            int chosenPile = -1, take = 0;

            if (nimSum != 0)
            {
                for (int i = 0; i < piles.Count; i++)
                {
                    int target = piles[i] ^ nimSum;
                    if (target < piles[i])
                    {
                        chosenPile = i;
                        take = piles[i] - target;
                        break;
                    }
                }
            }

            // If already a losing position (nim-sum == 0), take one from a non-empty pile
            if (chosenPile == -1)
            {
                chosenPile = Enumerable.Range(0, piles.Count).First(i => piles[i] > 0);
                take = 1;
            }

            piles[chosenPile] -= take;

            Console.WriteLine($"-- MACHINE'S MOVE --");
            Console.WriteLine($"I TAKE {take} STICK{(take == 1 ? "" : "S")} FROM PILE {chosenPile + 1} .");
        }

        // ----- Helpers -----
        private static bool AskYesNo(string prompt)
        {
            if (!string.IsNullOrEmpty(prompt)) Console.Write(prompt);

            while (true)
            {
                string s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s.StartsWith("Y")) return true;
                if (s.StartsWith("N")) return false;
                Console.Write("TRY 'YES' OR 'NO': ");
            }
        }

        private static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v)
                    && v >= min && v <= max)
                    return v;

                Console.WriteLine($"ENTER AN INTEGER FROM {min} TO {max}.");
            }
        }
    }
}
