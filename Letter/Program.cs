using System;

namespace LetterGuessingGame
{
    internal static class Program
    {
        private static readonly Random Rng = new Random();

        static void Main()
        {
            Console.Title = "LETTER — Letter Guessing Game";
            PrintBanner();

            do
            {
                PlayRound();
            } while (AskYesNo("\nLET'S PLAY AGAIN.....  (Y/N) "));
        }

        static void PrintBanner()
        {
            Console.WriteLine("LETTER GUESSING GAME");
            Console.WriteLine("I'LL THINK OF A LETTER OF THE ALPHABET, A TO Z.");
            Console.WriteLine("TRY TO GUESS MY LETTER AND I'LL GIVE YOU CLUES");
            Console.WriteLine("AS TO HOW CLOSE YOU'RE GETTING TO MY LETTER.");
        }

        static void PlayRound()
        {
            int target = Rng.Next(0, 26); // 0 = A, 25 = Z
            int guesses = 0;

            Console.WriteLine();
            Console.WriteLine("OK, I HAVE A LETTER.  START GUESSING.");

            while (true)
            {
                guesses++;
                char guessChar = ReadLetter($"\nWHAT IS YOUR GUESS? ");
                int g = char.ToUpperInvariant(guessChar) - 'A';

                if (g == target)
                {
                    Console.WriteLine($"\nYOU GOT IT IN {guesses} GUESSES!!");
                    if (guesses > 5)
                        Console.WriteLine("BUT IT SHOULDN'T TAKE MORE THAN 5 GUESSES!");
                    Console.WriteLine("GOOD JOB !!!");
                    break;
                }

                if (g < target)
                    Console.WriteLine("TOO LOW.  TRY A HIGHER LETTER.");
                else
                    Console.WriteLine("TOO HIGH. TRY A LOWER LETTER.");
            }
        }

        static char ReadLetter(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(s))
                {
                    Console.WriteLine("Please type a letter A–Z.");
                    continue;
                }

                // Find first alphabetic character in the input
                foreach (char c in s)
                {
                    if (char.IsLetter(c))
                        return c;
                }

                Console.WriteLine("Please type a letter A–Z.");
            }
        }

        static bool AskYesNo(string prompt)
        {
            Console.Write(prompt);
            while (true)
            {
                string? s = Console.ReadLine();
                if (s == null) return false;
                s = s.Trim().ToUpperInvariant();
                if (s == "Y" || s == "YES") return true;
                if (s == "N" || s == "NO") return false;
                Console.Write("Please answer Y or N: ");
            }
        }
    }
}
