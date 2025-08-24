using System;
using System.Globalization;

class Program
{
    static void Main()
    {
        Console.Title = "SLOTS — Slot Machine";
        var rng = new Random();

        // Reels and symbols (even distribution; easy to tweak)
        string[] symbols = { "BELL", "BAR", "CHERRY", "APPLE", "LEMON", "$" };

        // Running total (like coin-in/out tracker)
        int bank = 0;

        PrintIntro();
        PrintPayoutTable();

        while (AskYesNo("\nAGAIN? (Y/N) "))
        {
            // One “pull”
            var a = symbols[rng.Next(symbols.Length)];
            var b = symbols[rng.Next(symbols.Length)];
            var c = symbols[rng.Next(symbols.Length)];

            Console.WriteLine();
            Console.WriteLine($"{a,-7} {b,-7} {c,-7}");

            int win = Payout(a, b, c, out string msg);

            bank += win switch
            {
                > 0 => win,    // win adds to total
                _   => 0       // loss prints only
            };

            if (win > 0)
                Console.WriteLine($"{msg}  TOTAL=${bank}");
            else
                Console.WriteLine($"YOU HAVE LOST $1  ---  TOTAL=${bank}");
        }

        Console.WriteLine("\nIT'S BEEN NICE OPERATING FOR YOU. COME BACK SOON!");
    }

    // ---------------- Payout rules ----------------
    // You can tweak these numbers to match different “house edges”.
    // - JACKPOT: APPLE APPLE APPLE -> $20
    // - KENO: any two '$' (in any order) and the third is different -> $5
    // - Any other triple (three of a kind) -> $5
    // - Any pair (exactly two of a kind) -> $1
    // - Otherwise -> lose $1 (handled in caller)
    static int Payout(string a, string b, string c, out string message)
    {
        // JACKPOT
        if (a == "APPLE" && b == "APPLE" && c == "APPLE")
        {
            message = "JACKPOT... $20.";
            return 20;
        }

        // KENO (two $’s anywhere; third different)
        int dollars = (a == "$" ? 1 : 0) + (b == "$" ? 1 : 0) + (c == "$" ? 1 : 0);
        if (dollars == 2) // exactly two
        {
            message = "CHERRY KENO.. YOU WIN $5.";
            return 5;
        }

        // Other triples
        if (a == b && b == c)
        {
            message = "YOU HAVE WON $5.";
            return 5;
        }

        // Any pair
        if (a == b || a == c || b == c)
        {
            message = "YOU HAVE WON $1 ---";
            return 1;
        }

        // No win
        message = "";
        return 0;
    }

    // --------------- UI helpers ---------------
    static void PrintIntro()
    {
        Console.WriteLine("THIS IS A SIMULATION OF A SLOT MACHINE USING A COMPUTER");
        Console.WriteLine("EACH TIME YOU 'PULL' I WILL ASK YOU IF YOU WISH TO PLAY AGAIN.");
        Console.WriteLine("JUST ANSWER WITH A 'Y' FOR YES OR AN 'N' FOR NO.");
        Console.WriteLine("PLEASE PLACE 4 QUARTERS ON MY CPU FOR EACH PLAY. ;)\n");
    }

    static void PrintPayoutTable()
    {
        Console.WriteLine("PAYOUT TABLE");
        Console.WriteLine("------------");
        Console.WriteLine("APPLE  APPLE  APPLE        ->  JACKPOT           $20");
        Console.WriteLine("$      $      (not $)       ->  CHERRY KENO       $5");
        Console.WriteLine("Any other three of a kind   ->  $5");
        Console.WriteLine("Any pair (two of a kind)    ->  $1");
        Console.WriteLine("No match                     ->  Lose $1");
    }

    static bool AskYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            if (s.StartsWith("Y")) return true;
            if (s.StartsWith("N")) return false;
            Console.WriteLine("PLEASE ANSWER Y OR N.");
        }
    }
}
