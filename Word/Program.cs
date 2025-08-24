using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WordGame
{
    internal static class Program
    {
        // Original word list from the magazine DATA lines (5-letter words)
        private static readonly string[] WordList =
        {
            "DINKY","SMOKE","WATER","GRASS","TRAIN","NIGHT","FIRST",
            "CANDY","CHAMP","WOULD","CLUMP","DOPEY"
        };

        private static readonly Regex LettersOnly = new Regex("^[A-Z]{5}$", RegexOptions.Compiled);
        private static readonly Random Rng = new Random();

        static void Main()
        {
            Console.WriteLine("PROGRAM 'WORD'\n");
            Console.WriteLine("I AM THINKING OF A WORD — YOU GUESS IT. I WILL GIVE YOU");
            Console.WriteLine("CLUES TO HELP YOU GET IT.  GOOD LUCK!!\n");

            do
            {
                PlayOneRound();
            }
            while (AskYesNo("\nWANT TO PLAY AGAIN? "));

            Console.WriteLine("\nREADY");
        }

        private static void PlayOneRound()
        {
            string secret = WordList[Rng.Next(WordList.Length)].ToUpperInvariant();
            int guesses = 0;

            Console.WriteLine("YOU ARE STARTING A NEW GAME...\n");

            while (true)
            {
                string guess = AskGuess("GUESS A FIVE-LETTER WORD? ");
                if (guess == "?")
                {
                    Console.WriteLine($"IF YOU GIVE UP, TYPE '?' FOR YOUR NEXT GUESS");
                    // They typed '?', so reveal and end
                    Console.WriteLine($"THE SECRET WORD IS: {secret}");
                    return;
                }

                guesses++;

                if (guess.Length != 5 || !LettersOnly.IsMatch(guess))
                {
                    Console.WriteLine("YOU MUST GUESS A 5-LETTER WORD.  START AGAIN\n");
                    continue;
                }

                if (guess == secret)
                {
                    Console.WriteLine($"YOU HAVE GUESSED THE WORD.  IT TOOK {guesses} GUESSES!");
                    return;
                }

                // Common-letter clue (unique letters in common, alphabetical)
                var common = CommonLetters(secret, guess);
                Console.WriteLine($"THERE WERE {common.Length} MATCHES AND THE COMMON LETTERS WERE...  {common}");

                // Exact-match pattern
                string pattern = ExactPattern(secret, guess);
                Console.WriteLine($"FROM THE EXACT LETTER MATCHES, YOU KNOW............  {pattern}\n");
            }
        }

        private static string AskGuess(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "?" || s.Length == 5) return s;
                Console.WriteLine("ENTER 5 LETTERS (A–Z) OR '?' TO GIVE UP.");
            }
        }

        private static bool AskYesNo(string prompt)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            return s == "Y" || s == "YES" || s == "1";
        }

        private static string CommonLetters(string secret, string guess)
        {
            // Unique intersection, alphabetical (matches style shown in sample run)
            var secSet = new HashSet<char>(secret);
            var com = guess.Distinct().Where(c => secSet.Contains(c)).ToList();
            com.Sort();
            return com.Count == 0 ? "-----" : new string(com.ToArray());
        }

        private static string ExactPattern(string secret, string guess)
        {
            char[] outp = new char[5];
            for (int i = 0; i < 5; i++)
                outp[i] = (guess[i] == secret[i]) ? guess[i] : '-';
            return new string(outp);
        }
    }
}
