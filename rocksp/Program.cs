using System;
using System.Globalization;

class Program
{
    static void Main()
    {
        Console.Title = "ROCKSP — Rock, Scissors, Paper";

        Console.WriteLine("THIS PROGRAM ALLOWS YOU TO PLAY THE OLD GAME OF");
        Console.WriteLine("ROCKS, PAPER, AND SCISSORS AGAINST THE COMPUTER.\n");

        int total = AskGamesCount();

        int compWins = 0, youWins = 0, ties = 0;

        var rng = new Random();

        for (int game = 1; game <= total; game++)
        {
            Console.WriteLine($"\nGAME NUMBER {game}");
            int yourChoice = AskPlayerChoice();

            int compChoice = rng.Next(1, 4); // 1=Paper, 2=Scissors, 3=Rock
            Console.WriteLine("THIS IS MY CHOICE...");
            Console.WriteLine(".. " + NameOf(compChoice));

            int outcome = Winner(yourChoice, compChoice); // -1=comp, 0=tie, +1=you
            if (outcome == 0)
            {
                Console.WriteLine("TIE GAME, NO WINNER.");
                ties++;
            }
            else if (outcome > 0)
            {
                Console.WriteLine("YOU WIN!!!");
                youWins++;
            }
            else
            {
                Console.WriteLine("WOW! I WINN!");
                compWins++;
            }
        }

        // Final score (as in listing: “I have won … You have won … And … tie.”)
        Console.WriteLine("\nHERE IS THE FINAL SCORE:");
        Console.WriteLine($"I HAVE WON {compWins} GAME(S).");
        Console.WriteLine($"YOU HAVE WON {youWins} GAME(S).");
        Console.WriteLine($"AND {ties} GAME(S) ENDED IN A TIE.");
        Console.WriteLine("\nTHANKS FOR PLAYING!!");
    }

    // 1=Paper, 2=Scissors, 3=Rock (to match the BASIC’s menu)
    static string NameOf(int n) => n switch
    {
        1 => "PAPER",
        2 => "SCISSORS",
        3 => "ROCK",
        _ => "?"
    };

    // returns +1 if you win, -1 if computer wins, 0 if tie
    static int Winner(int you, int comp)
    {
        if (you == comp) return 0;
        bool youWin =
            (you == 1 && comp == 3) || // paper wraps rock
            (you == 2 && comp == 1) || // scissors cut paper
            (you == 3 && comp == 2);   // rock breaks scissors
        return youWin ? +1 : -1;
    }

    static int AskGamesCount()
    {
        while (true)
        {
            Console.Write("HOW MANY GAMES DO YOU WANT? ");
            if (int.TryParse(Console.ReadLine(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int n))
            {
                if (n <= 0) { Console.WriteLine("PLEASE ENTER A POSITIVE NUMBER."); continue; }
                if (n > 10) { Console.WriteLine("SORRY, BUT WE AREN'T ALLOWED TO PLAY THAT MANY."); continue; }
                return n;
            }
            Console.WriteLine("PLEASE ENTER A NUMBER (1–10).");
        }
    }

    static int AskPlayerChoice()
    {
        Console.WriteLine("3=ROCK..  2=SCISSORS..  1=PAPER");
        while (true)
        {
            Console.Write("1... 2... 3... WHAT'S YOUR CHOICE? ");
            string? s = Console.ReadLine();
            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int k) && k is >= 1 and <= 3)
            {
                Console.WriteLine("THIS IS MY CHOICE...");
                Console.WriteLine(".. " + NameOf(k)); // matches the BASIC echo
                return k;
            }
            Console.WriteLine("INVALID. TRY AGAIN.");
        }
    }
}
