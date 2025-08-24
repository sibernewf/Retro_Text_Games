using System;
using System.Globalization;

class Program
{
    static void Main()
    {
        Console.Title = "RUSSIAN ROULETTE";

        var rng = new Random(); // BASIC's RANDOMIZE

        while (true)
        {
            Console.WriteLine("THIS IS A GAME OF >>>>>>>>>>>>RUSSIAN ROULETTE");
            Console.WriteLine();
            Console.WriteLine("HERE IS A REVOLVER");
            Console.WriteLine("HIT '1' TO SPIN CHAMBER AND PULL TRIGGER.");
            Console.WriteLine("(HIT '2' TO GIVE UP)");
            Console.WriteLine();

            int pulls = 0;
            bool dead = false;

            while (!dead)
            {
                int k = Ask12("GO? ");
                if (k == 2)
                {
                    Console.WriteLine("LET SOMEONE ELSE BLOW HIS BRAINS OUT.");
                    break; // give up
                }

                // k == 1 → spin & pull
                pulls++;
                // 1-in-6 chance (like IF RND >= .8333 THEN BANG)
                bool bang = rng.Next(6) == 0;

                if (bang)
                {
                    Console.WriteLine();
                    Console.WriteLine("BANG!!!!  YOU'RE DEAD!");
                    Console.WriteLine("CONDOLENCES WILL BE SENT TO YOUR RELATIVES.");
                    Console.WriteLine();
                    Console.WriteLine("...NEXT VICTIM...");
                    dead = true;
                    break;
                }
                else
                {
                    Console.WriteLine("- CLICK -");
                }

                if (pulls >= 10)
                {
                    Console.WriteLine();
                    Console.WriteLine("YOU WIN !!!");
                    break;
                }
            }

            // play again?
            Console.WriteLine();
            if (!AskYesNo("GO AGAIN (YES/NO)? ")) break;
            Console.WriteLine();
        }
    }

    static int Ask12(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim();
            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) && (v == 1 || v == 2))
                return v;
            Console.WriteLine("TYPE 1 TO PULL, OR 2 TO GIVE UP.");
        }
    }

    static bool AskYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            if (s.StartsWith("Y")) return true;
            if (s.StartsWith("N")) return false;
            Console.WriteLine("PLEASE ANSWER YES OR NO.");
        }
    }
}
