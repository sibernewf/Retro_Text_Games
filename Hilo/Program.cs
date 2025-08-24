using System;

class Program
{
    static void Main()
    {
        Console.Title = "HI-LO — High/Low Jackpot";
        var rng = Random.Shared;
        int total = 0;

        PrintIntro();

        while (true)
        {
            int jackpot = rng.Next(1, 101); // 1..100
            bool won = false;

            for (int turn = 1; turn <= 6; turn++)
            {
                int? guess = AskGuess(turn);
                if (guess is null) { Quit(total); return; }

                if (guess == jackpot)
                {
                    Console.WriteLine($"GOT IT!!!!!!!   YOU WIN {jackpot} DOLLARS.");
                    total += jackpot;
                    won = true;
                    break;
                }

                Console.WriteLine(guess < jackpot
                    ? "YOUR GUESS IS TOO LOW"
                    : "YOUR GUESS IS TOO HIGH");
                Console.WriteLine();
            }

            if (!won)
                Console.WriteLine($"YOU BLEW IT....TOO BAD....THE NUMBER WAS {jackpot}");

            Console.WriteLine($"YOUR TOTAL WINNINGS ARE NOW {total} DOLLARS.");
            Console.WriteLine();

            if (!AskYesNo("PLAY AGAIN (YES OR NO)? ")) break;
            Console.WriteLine();
        }

        Console.WriteLine("SO LONG.  HOPE YOU ENJOYED YOURSELF!!");
        Console.WriteLine("READY");
    }

    static void PrintIntro()
    {
        Console.WriteLine("THIS IS THE GAME OF HI-LO");
        Console.WriteLine("YOU WILL HAVE 6 TRIES TO GUESS THE AMOUNT OF MONEY IN THE");
        Console.WriteLine("HI-LO JACKPOT, WHICH IS BETWEEN 1 AND 100 DOLLARS.  IF YOU");
        Console.WriteLine("GUESS THE AMOUNT, YOU WIN ALL THE MONEY IN THE JACKPOT; THEN");
        Console.WriteLine("YOU GET ANOTHER CHANCE TO WIN MORE MONEY.  HOWEVER, IF YOU DO");
        Console.WriteLine("NOT GUESS THE AMOUNT IN 6 TRIES, THE GAME TELLS YOU THE NUMBER.");
        Console.WriteLine("TYPE 'q' ANYTIME TO QUIT.\n");
    }

    static int? AskGuess(int turn)
    {
        while (true)
        {
            Console.Write($"YOUR GUESS? ");
            var s = (Console.ReadLine() ?? "").Trim();
            if (s.Equals("q", StringComparison.OrdinalIgnoreCase)) return null;

            if (int.TryParse(s, out int g) && g >= 1 && g <= 100)
                return g;

            Console.WriteLine("PLEASE ENTER A WHOLE NUMBER FROM 1 TO 100 (OR 'q' TO QUIT).");
        }
    }

    static bool AskYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim();
            if (s.Equals("q", StringComparison.OrdinalIgnoreCase)) return false;
            if (s.Length == 0) continue;
            char c = char.ToUpperInvariant(s[0]);
            if (c == 'Y') return true;
            if (c == 'N') return false;
        }
    }

    static void Quit(int total)
    {
        Console.WriteLine();
        Console.WriteLine($"YOU QUIT. YOUR TOTAL WINNINGS: {total} DOLLARS.");
        Console.WriteLine("READY");
    }
}
