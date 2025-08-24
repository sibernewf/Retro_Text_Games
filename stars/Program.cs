using System;
using System.Globalization;

class Program
{
    static void Main()
    {
        Console.Title = "STARS — A Number Guessing Game";
        var rng = new Random();

        const int MAX = 100;   // you can change this if you like
        const int GUESSES = 7;

        while (true)
        {
            Console.Clear();
            PrintIntro(MAX);

            // Pick a secret number
            int secret = rng.Next(1, MAX + 1);

            Console.WriteLine("\nOK. I AM THINKING OF A NUMBER. START GUESSING.\n");

            bool won = false;
            for (int turn = 1; turn <= GUESSES; turn++)
            {
                int guess = AskInt($"YOUR GUESS? ", 1, MAX);

                // Feedback as stars (1 = far, 7 = very close)
                int stars = StarsByDistance(Math.Abs(guess - secret), MAX);
                Console.WriteLine(new string('*', stars) + "\n");

                if (guess == secret)
                {
                    Console.WriteLine($"************************************************************!!!");
                    Console.WriteLine($"YOU GOT IT IN {turn} GUESSES!!  LET'S PLAY AGAIN...");
                    won = true;
                    break;
                }
            }

            if (!won)
            {
                Console.WriteLine("SORRY, YOU DID NOT GUESS THE NUMBER IN 7 GUESSES.");
                Console.WriteLine($"THE NUMBER WAS {secret}.");
                Console.WriteLine("YOU GET 7 GUESSES NEXT GAME TOO.  LET'S PLAY AGAIN...");
            }

            // Play again? (Press Enter to continue, Q to quit)
            Console.Write("\nPLAY AGAIN? (Y/N) ");
            if (!YesNo()) break;
        }
    }

    static void PrintIntro(int max)
    {
        Console.WriteLine("STARS — A NUMBER GUESSING GAME");
        Console.WriteLine("--------------------------------");
        Console.WriteLine("I'M THINKING OF A WHOLE NUMBER FROM 1 TO " + max + ".");
        Console.WriteLine("TRY TO GUESS MY NUMBER. AFTER YOU GUESS, I'LL");
        Console.WriteLine("PRINT ONE OR MORE STARS (*). THE MORE STARS I TYPE,");
        Console.WriteLine("THE CLOSER YOU ARE TO MY NUMBER.");
        Console.WriteLine("ONE STAR (*) MEANS FAR AWAY; SEVEN STARS (*******) MEANS");
        Console.WriteLine("REALLY CLOSE! YOU GET 7 GUESSES.");
    }

    // Map distance -> 1..7 stars. The bins scale with the range so MAX can change.
    // Thresholds are halves of the range (50%, 25%, 12.5%, 6.25%, 3.125%, 1.5625%).
    // For MAX=100 this is roughly: >50 =>1, ≤50=>2, ≤25=>3, ≤12=>4, ≤6=>5, ≤3=>6, ≤1=>7
    static int StarsByDistance(int distance, int max)
    {
        if (distance <= Math.Max(1, max / 64)) return 7;
        if (distance <= Math.Max(1, max / 32)) return 6;
        if (distance <= Math.Max(1, max / 16)) return 5;
        if (distance <= Math.Max(1, max / 8 )) return 4;
        if (distance <= Math.Max(1, max / 4 )) return 3;
        if (distance <= Math.Max(1, max / 2 )) return 2;
        return 1;
    }

    static int AskInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim();
            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) && v >= min && v <= max)
                return v;
            Console.WriteLine($"PLEASE ENTER A WHOLE NUMBER BETWEEN {min} AND {max}.");
        }
    }

    static bool YesNo()
    {
        while (true)
        {
            var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            if (s.StartsWith("Y")) return true;
            if (s.StartsWith("N")) return false;
            Console.Write("PLEASE ANSWER Y OR N: ");
        }
    }
}
