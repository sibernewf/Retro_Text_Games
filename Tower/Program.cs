using System;
using System.Collections.Generic;
using System.Linq;

namespace TowersOfHanoi
{
    internal static class Program
    {
        private static readonly int[] AllowedLabels = { 3, 5, 7, 9, 11, 13, 15 };

        static void Main()
        {
            Console.WriteLine("TOWERS OF HANOI PUZZLE");
            Console.WriteLine("You must transfer the disks from the LEFT needle (1) to the RIGHT needle (3).");
            Console.WriteLine("Move ONE disk at a time and NEVER place a larger disk on a smaller one.");
            Console.WriteLine("Disks are labeled by SIZE: 3 (smallest), then 5, 7, ... up to 15 (largest).");
            Console.WriteLine("Needles are numbered 1 to 3 (left to right). Good luck!\n");

            do
            {
                int n = AskInt("HOW MANY DISKS DO YOU WANT TO MOVE? (2–7): ", 2, 7);
                PlaySingleGame(n);
            }
            while (AskYesNo("\nDO YOU WANT TO PLAY ANOTHER GAME? (YES=1, NO=0) "));
            
            Console.WriteLine("\nTHANKS FOR THE GAME!");
        }

        private static void PlaySingleGame(int n)
        {
            // Choose the largest n labels from the allowed sequence: e.g., n=3 -> 11,13,15
            var labels = AllowedLabels.Skip(AllowedLabels.Length - n).ToArray();

            // Needles: 0=left,1=middle,2=right. Each is a stack: bottom -> top order in list.
            var rods = new List<int>[] { new(), new(), new() };
            foreach (var d in labels) rods[0].Add(d); // all on left, largest at bottom

            int moves = 0;
            PrintState(rods);

            while (rods[2].Count != n)
            {
                int disk = AskDisk("WHICH DISK WOULD YOU LIKE TO MOVE? ", labels);
                // disk must be on top of some rod
                int from = FindRodWithTop(rods, disk);
                if (from == -1)
                {
                    Console.WriteLine("THAT DISK IS BELOW ANOTHER ONE. MAKE ANOTHER CHOICE.");
                    continue;
                }

                int to = AskInt("PLACE DISK ON WHICH NEEDLE? (1–3): ", 1, 3) - 1;

                if (to == from)
                {
                    Console.WriteLine("ILLEGAL ENTRY...CHOOSE A DIFFERENT NEEDLE.");
                    continue;
                }

                // Can only place on empty rod or on a larger disk label (since labels grow with size)
                if (rods[to].Count > 0 && rods[to].Last() < disk)
                {
                    Console.WriteLine("YOU MAY NOT PLACE A LARGER DISK ON TOP OF A SMALLER ONE.");
                    continue;
                }

                // move
                rods[from].RemoveAt(rods[from].Count - 1);
                rods[to].Add(disk);
                moves++;

                PrintState(rods);
            }

            Console.WriteLine($"CONGRATULATIONS!! YOU HAVE PERFORMED THE TASK IN {moves} MOVES.");
            long optimal = (1L << n) - 1; // 2^n - 1
            if (moves == optimal)
                Console.WriteLine("PERFECT! THAT IS THE MINIMAL NUMBER OF MOVES.");
            else
                Console.WriteLine($"The minimal number of moves is {optimal}.");
        }

        private static int AskInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (int.TryParse(s, out int v) && v >= min && v <= max) return v;
                Console.WriteLine($"PLEASE ENTER A NUMBER FROM {min} TO {max}.");
            }
        }

        private static int AskDisk(string prompt, int[] validLabels)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (int.TryParse(s, out int v) && validLabels.Contains(v))
                    return v;
                Console.WriteLine($"ILLEGAL ENTRY...YOU MAY ONLY TYPE {string.Join(',', validLabels)}.");
            }
        }

        private static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim();
                if (s == "1" || s.Equals("YES", StringComparison.OrdinalIgnoreCase)) return true;
                if (s == "0" || s.Equals("NO", StringComparison.OrdinalIgnoreCase)) return false;
            }
        }

        private static int FindRodWithTop(List<int>[] rods, int disk)
        {
            for (int i = 0; i < 3; i++)
            {
                if (rods[i].Count > 0 && rods[i].Last() == disk)
                    return i;
            }
            return -1;
        }

        private static void PrintState(List<int>[] rods)
        {
            // Render rods vertically with simple ASCII columns, tallest at left/right
            // Determine the maximum height
            int maxH = Math.Max(rods[0].Count, Math.Max(rods[1].Count, rods[2].Count));

            Console.WriteLine();
            for (int row = maxH - 1; row >= 0; row--)
            {
                for (int r = 0; r < 3; r++)
                {
                    if (row < rods[r].Count)
                    {
                        int label = rods[r][row];
                        // width hint from label; smallest 3 -> small bar, largest 15 -> largest bar
                        int width = (label - 1) / 2; // 1..7
                        Console.Write(Bar(width).PadLeft(12));
                    }
                    else
                    {
                        Console.Write(" |".PadLeft(12)); // empty rod marker
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine(new string('=', 36));
            Console.WriteLine("   N1".PadLeft(12) + "N2".PadLeft(12) + "N3".PadLeft(12));
            Console.WriteLine();
        }

        private static string Bar(int w)
        {
            // centered bar like ***** with width proportional to disk
            return new string('*', w);
        }
    }
}
