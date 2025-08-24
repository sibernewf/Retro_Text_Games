using System;
using System.Collections.Generic;
using System.Linq;

class Bagels
{
    static void Main()
    {
        Console.WriteLine("RUNNN");
        Console.WriteLine("GAME OF BAGELS.");

        while (true)
        {
            if (AskYesNo("WOULD YOU LIKE THE RULES (YES OR NO)?"))
                ShowRules();

            PlayRound();

            if (!AskYesNo("PLAY AGAIN (YES OR NO)?"))
            {
                Console.WriteLine("\nHOPE YOU HAD FUN. BYE.");
                break;
            }
        }
    }

    static void ShowRules()
    {
        Console.WriteLine("\nI AM THINKING OF A THREE-DIGIT NUMBER. TRY TO GUESS.");
        Console.WriteLine("MY NUMBER AND I WILL GIVE YOU CLUES AS FOLLOWS:");
        Console.WriteLine("  PICO   - ONE DIGIT CORRECT BUT IN THE WRONG POSITION");
        Console.WriteLine("  FERMI  - ONE DIGIT CORRECT AND IN THE RIGHT POSITION");
        Console.WriteLine("  BAGELS - NO DIGITS CORRECT\n");
    }

    static void PlayRound()
    {
        var secret = GenerateSecret();
        // Uncomment for debugging: Console.WriteLine($"(Secret: {secret})");

        Console.WriteLine("O.K. I HAVE A NUMBER IN MIND.");

        int guessCount = 0;
        while (true)
        {
            string guess = PromptGuess();
            if (guess == "Q") { Console.WriteLine("QUITTING GAME..."); Environment.Exit(0); }

            guessCount++;

            if (guess == secret)
            {
                Console.WriteLine("YOU GOT IT!!!");
                break;
            }

            var clues = GetClues(secret, guess);
            Console.WriteLine(clues);
        }

        Console.WriteLine($"YOU GOT IT IN {guessCount} GUESSES!");
    }

    static string PromptGuess()
    {
        while (true)
        {
            Console.Write("GUESS #? ");
            string input = Console.ReadLine()?.Trim().ToUpper() ?? "";
            if (input == "Q") return "Q";

            if (input.Length == 3 && input.All(char.IsDigit))
                return input;

            Console.WriteLine("PLEASE ENTER A THREE-DIGIT NUMBER OR Q TO QUIT.");
        }
    }

    static string GenerateSecret()
    {
        var digits = Enumerable.Range(0, 10).OrderBy(_ => Guid.NewGuid()).ToList();
        return string.Concat(digits.Take(3));
    }

    static string GetClues(string secret, string guess)
    {
        if (secret == guess) return "YOU GOT IT!!!";

        List<string> clues = new List<string>();

        for (int i = 0; i < 3; i++)
        {
            if (guess[i] == secret[i])
                clues.Add("FERMI");
            else if (secret.Contains(guess[i]))
                clues.Add("PICO");
        }

        if (clues.Count == 0)
            return "BAGELS";

        return string.Join(" ", clues);
    }

    static bool AskYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt + " ");
            string input = Console.ReadLine()?.Trim().ToUpper() ?? "";
            if (input.StartsWith("Y")) return true;
            if (input.StartsWith("N")) return false;
        }
    }
}
