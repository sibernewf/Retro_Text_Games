using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HangGame
{
    internal static class Program
    {
        static void Main()
        {
            Console.Title = "HANG — Game of Hangman";
            new Hang().Run();
        }
    }

    internal sealed class Hang
    {
        readonly Random rng = new();
        string[] Words;

        // 10-part hangman (index = misses so far)
        readonly string[] Stages =
        {
@"
  +---+
  |   |
      |
      |
      |
      |
=======",
@"
  +---+
  |   |
  O   |
      |
      |
      |
=======",
@"
  +---+
  |   |
  O   |
  |   |
      |
      |
=======",
@"
  +---+
  |   |
  O   |
 \|   |
      |
      |
=======",
@"
  +---+
  |   |
  O   |
 \|/  |
      |
      |
=======",
@"
  +---+
  |   |
  O   |
 \|/  |
 /    |
      |
=======",
@"
  +---+
  |   |
  O   |
 \|/  |
 / \  |
      |
=======",
@"
  +---+
  |   |
 _O   |
 \|/  |
 / \  |
      |
=======",
@"
  +---+
  |   |
 _O_  |
 \|/  |
 / \  |
      |
=======",
@"
  +---+
  |   |
 _O_  |
 \|/  |
_/ \  |
      |
=======",
@"
  +---+
  |   |
 _O_  |
 \|/  |
_/ \_ |
      |
======="
        };

        public void Run()
        {
            LoadWords("words.txt");

            Console.WriteLine("HANG — Game of Hangman");
            Console.WriteLine("Guess letters (A–Z) or the whole word. You may miss up to 10 times.");
            Console.WriteLine("Type Q anytime to quit.\n");

            while (true)
            {
                string secret = Words[rng.Next(Words.Length)].ToUpperInvariant();
                HashSet<char> used = new();
                int misses = 0;
                var pattern = secret.Select(c => char.IsLetter(c) ? '_' : c).ToArray();

                while (true)
                {
                    PrintBoard(pattern, used, misses);

                    if (!pattern.Contains('_'))
                    {
                        Console.WriteLine("YOU GOT IT! NICE SHOOTING.");
                        break;
                    }
                    if (misses >= 10)
                    {
                        Console.WriteLine("SORRY, YOU LOSE. THE WORD WAS: " + secret);
                        break;
                    }

                    Console.Write("WHAT IS YOUR GUESS (letter or word)? ");
                    var raw = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                    if (raw == "Q") return;
                    if (string.IsNullOrWhiteSpace(raw)) continue;

                    // Whole-word guess
                    if (raw.Length > 1)
                    {
                        if (raw == secret)
                        {
                            for (int i = 0; i < secret.Length; i++) pattern[i] = secret[i];
                            continue;
                        }
                        Console.WriteLine("WRONG. TRY ANOTHER LETTER.");
                        misses++;
                        continue;
                    }

                    char g = raw[0];
                    if (g < 'A' || g > 'Z')
                    {
                        Console.WriteLine("PLEASE ENTER A LETTER A–Z, OR A FULL WORD.");
                        continue;
                    }
                    if (!used.Add(g))
                    {
                        Console.WriteLine("YOU’VE GUESSED THAT LETTER BEFORE — TRY AGAIN.");
                        continue;
                    }

                    bool hit = false;
                    for (int i = 0; i < secret.Length; i++)
                    {
                        if (secret[i] == g) { pattern[i] = g; hit = true; }
                    }
                    if (!hit)
                    {
                        Console.WriteLine("SORRY, THAT LETTER ISN’T IN THE WORD.");
                        misses++;
                    }
                }

                Console.Write("\nPLAY AGAIN (Y/N)? ");
                var again = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (again is not ("Y" or "YES")) break;
                Console.WriteLine();
            }
        }

        void LoadWords(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"ERROR: Word list file not found: {filePath}");
                Console.WriteLine("Please create the file with one word per line.");
                Environment.Exit(1);
            }

            Words = File.ReadAllLines(filePath)
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => line.Trim())
                        .Where(word => word.All(char.IsLetter))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();

            if (Words.Length == 0)
            {
                Console.WriteLine($"ERROR: No valid words found in {filePath}.");
                Environment.Exit(1);
            }
        }

        void PrintBoard(char[] pattern, HashSet<char> used, int misses)
        {
            Console.WriteLine();
            Console.WriteLine("HERE ARE THE LETTERS YOU USED:");
            Console.WriteLine(string.Join(' ', used.OrderBy(c => c)));
            Console.WriteLine(Stages[Math.Clamp(misses, 0, 10)]);
            Console.WriteLine(new string(pattern.Select(c => c == '_' ? '-' : c).ToArray()));
            Console.WriteLine();
        }
    }
}
