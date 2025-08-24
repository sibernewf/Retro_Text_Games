using System;
using System.Collections.Generic;

class Program
{
    static readonly Random rng = new Random();

    static void Main()
    {
        int bankroll = 100;
        Console.WriteLine("ACEY DEUCY (Console Edition)");
        Console.WriteLine("You start with $100. Bet that the next card falls between the first two.");
        Console.WriteLine("Hit ENTER to deal. Type Q at any bet prompt to quit.\n");

        while (bankroll > 0)
        {
            Console.WriteLine($"Bankroll: ${bankroll}");
            Console.Write("Deal? (ENTER) "); 
            var key = Console.ReadKey();
            Console.WriteLine();
            if (key.Key == ConsoleKey.Q) break;

            // Deal two cards and sort them low..high
            int a = DealCard(), b = DealCard();
            if (a > b) (a, b) = (b, a);

            Console.WriteLine($"First two: [{CardName(a)}]  [{CardName(b)}]");

            // Get bet
            int bet = PromptBet(bankroll);
            if (bet == -1) break;        // user typed Q
            if (bet == 0) { Console.WriteLine("Chicken! No bet this hand.\n"); continue; }

            // Draw third
            int c = DealCard();
            Console.WriteLine($"Third card: [{CardName(c)}]");

            if (c > a && c < b)
            {
                bankroll += bet;
                Console.WriteLine($"Between! You WIN ${bet}.\n");
            }
            else if (c == a || c == b)
            {
                int loss = Math.Min(bet * 2, bankroll);
                bankroll -= loss;
                Console.WriteLine($"Matched an end—double loss. You LOSE ${loss}.\n");
            }
            else
            {
                bankroll -= bet;
                Console.WriteLine($"Not between. You LOSE ${bet}.\n");
            }
        }

        Console.WriteLine(bankroll <= 0
            ? "Sorry, friend—you're busted."
            : $"You walk away with ${bankroll}. Thanks for playing!");
    }

    static int DealCard() => rng.Next(2, 15); // 2..14 (Ace=14)

    static string CardName(int v)
    {
        var faces = new Dictionary<int,string> {
            [11]="JACK", [12]="QUEEN", [13]="KING", [14]="ACE"
        };
        return faces.ContainsKey(v) ? faces[v] : v.ToString();
    }

    static int PromptBet(int bankroll)
    {
        while (true)
        {
            Console.Write($"Bet amount (0..{bankroll}) or Q to quit: ");
            string? s = Console.ReadLine()?.Trim();
            if (string.Equals(s, "q", StringComparison.OrdinalIgnoreCase)) return -1;

            if (int.TryParse(s, out int bet) && bet >= 0 && bet <= bankroll)
                return bet;

            Console.WriteLine("Invalid bet. Try again.");
        }
    }
}
