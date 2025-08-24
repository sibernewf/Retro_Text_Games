using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

class Program
{
    static void Main()
    {
        Console.Title = "SALVO I — Army Gun Battle";

        var rng = new Random();

        // --- Intro & reference grid ---
        Console.WriteLine("YOU ARE ON A BATTLEFIELD WITH 4 PLATOONS AND YOU");
        Console.WriteLine("HAVE 25 OUTPOSTS AVAILABLE WHERE THEY MAY BE PLACED.");
        Console.WriteLine("YOU CAN ONLY PLACE ONE PLATOON AT ANY ONE OUTPOST.");
        Console.WriteLine("THE COMPUTER DOES THE SAME WITH ITS FOUR PLATOONS.");
        Console.WriteLine();
        Console.WriteLine("THE OBJECT OF THE GAME IS TO FIRE MISSILES AT THE");
        Console.WriteLine("OUTPOSTS OF THE COMPUTER.  IT WILL DO THE SAME TO YOU.");
        Console.WriteLine("THE ONE WHO DESTROYS ALL FOUR OF THE ENEMY'S PLATOONS");
        Console.WriteLine("FIRST IS THE WINNER.");
        Console.WriteLine();
        Console.WriteLine("GOOD LUCK... AND TELL US WHERE YOU WANT THE BODIES SENT!");
        Console.WriteLine();
        PrintReferenceGrid();

        // --- Place player platoons ---
        var playerPlatoons = ReadPositions("WHAT ARE YOUR FOUR POSITIONS? ", 4);
        var playerAlive = new HashSet<int>(playerPlatoons);

        // --- Place computer platoons ---
        var allPositions = Enumerable.Range(1, 25).ToList();
        var compPlatoons = new HashSet<int>();
        while (compPlatoons.Count < 4)
        {
            int p = allPositions[rng.Next(allPositions.Count)];
            compPlatoons.Add(p);
        }
        var compAlive = new HashSet<int>(compPlatoons);

        // --- Shots history (to avoid repeats) ---
        var yourShots = new HashSet<int>();
        var compShots = new HashSet<int>();

        // --- Game loop (you start, like the sample run) ---
        while (true)
        {
            // YOUR TURN
            int shot = ReadShot("WHERE DO YOU WISH TO FIRE YOUR MISSILE? ", yourShots);
            if (compAlive.Contains(shot))
            {
                compAlive.Remove(shot);
                Console.WriteLine("I GOT YOU.  IT WON'T BE LONG NOW, POST** WAS HIT.".Replace("**", shot.ToString(CultureInfo.InvariantCulture)));
                int left = compAlive.Count;
                if (left == 3) Console.WriteLine("YOU HAVE ONLY THREE OUTPOSTS LEFT!");
                if (left == 2) Console.WriteLine("YOU HAVE ONLY TWO OUTPOSTS LEFT!");
                if (left == 1) Console.WriteLine("YOU HAVE ONLY ONE OUTPOST LEFT!");
                if (left == 0)
                {
                    Console.WriteLine("AFTER LONG DELAY, YOUR LAST OUTPOST WAS ATT**.  HA, HA, HA!!"
                        .Replace("**", "K"));
                    Console.WriteLine("I GOT THE LAST ONE NEXT TIME."); // playful nod to listing tone
                    Console.WriteLine("\nYOU HAVE WON");
                    break;
                }
            }
            else
            {
                Console.WriteLine("HA, HA YOU MISSED.  MY TURN NOW");
            }

            // COMPUTER TURN
            int cshot;
            do { cshot = rng.Next(1, 26); } while (!compShots.Add(cshot));

            if (playerAlive.Contains(cshot))
            {
                playerAlive.Remove(cshot);
                Console.WriteLine($"I MISSED YOU, YOU DIRTY RAT.  I PICKED {cshot}.  YOUR TURN.");
                Console.WriteLine("— WAIT —");
                Console.WriteLine("I GOT YOU!  YOUR OUTPOST WAS HIT.");
            }
            else
            {
                Console.WriteLine($"I MISSED YOU, YOU DIRTY RAT.  I PICKED {cshot}.  YOUR TURN.");
            }

            if (playerAlive.Count == 0)
            {
                Console.WriteLine("\nYOUR LAST OUTPOST IS GONE.  YOU HAVE LOST.");
                break;
            }
        }
    }

    // ----- Helpers -----

    static void PrintReferenceGrid()
    {
        Console.WriteLine("\nTEAR OFF THE MATRIX AND USE IT TO CHECK OFF THE NUMBERS.\n");
        for (int r = 0; r < 5; r++)
        {
            for (int c = 1; c <= 5; c++)
            {
                int v = r * 5 + c;
                Console.Write($"{v,3}");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    static HashSet<int> ReadPositions(string prompt, int count)
    {
        while (true)
        {
            Console.Write(prompt);
            var line = (Console.ReadLine() ?? "").Trim();
            var parts = line.Split(new[] { ',', ' ', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);

            var picks = new HashSet<int>();
            bool ok = true;

            foreach (var p in parts)
            {
                if (!int.TryParse(p, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) || n < 1 || n > 25)
                {
                    ok = false; break;
                }
                picks.Add(n);
            }

            if (ok && picks.Count == count) return picks;

            Console.WriteLine($"ENTER EXACTLY {count} DIFFERENT NUMBERS BETWEEN 1 AND 25 (e.g., 3 7 12 24).");
        }
    }

    static int ReadShot(string prompt, HashSet<int> alreadyTried)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim();
            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) && n >= 1 && n <= 25)
            {
                if (alreadyTried.Contains(n))
                {
                    Console.WriteLine("YOU ALREADY FIRED AT THAT OUTPOST. TRY AGAIN.");
                    continue;
                }
                alreadyTried.Add(n);
                return n;
            }
            Console.WriteLine("ENTER A NUMBER FROM 1 TO 25.");
        }
    }
}
