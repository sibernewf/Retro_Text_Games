using System;
using System.Collections.Generic;
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
        readonly string[] WORDS =
        {
            // From the scan’s lists (plus a few fillers). Add more freely.
            "PUZZLE","THING","GREAT","FRIEND","BELOW","FAULT","DIRTY",
            "OBJECT","ELEVATOR","SILENT","QUAINT","WELCOME","ESCORT","PICKER",
            "EVERYTHING","LISTENING","RUNNING","KIDNEY","NECKLACE","REPLICA","SLEEPER",
            "TRIANGLE","STRONGHOLD","THOUGHT","SECRETION","SEQUENCE",
            "MOSQUITO","DANGEROUS","SCIENTIST","DIFFERENT","RICOCHET",
            "MISTRUSTS","FEROCIOUSLY","HOMESICKNESS","PLATFORM","PHOTOVOICE",
            "MATRIMONIAL","PARASYMPATHOMIMETIC","PHOTOTROPISM"
        };

        // 10-part hangman (index = misses so far)
        readonly string[] Stages =
        {
            // 0 - empty gallows
@"
  +---+
  |   |
      |
      |
      |
      |
=======",
            // 1 head
@"
  +---+
  |   |
  O   |
      |
      |
      |
=======",
            // 2 body
@"
  +---+
  |   |
  O   |
  |   |
      |
      |
=======",
            // 3 right arm
@"
  +---+
  |   |
  O   |
 \|   |
      |
      |
=======",
            // 4 left arm
@"
  +---+
  |   |
  O   |
 \|/  |
      |
      |
=======",
            // 5 right leg
@"
  +---+
  |   |
  O   |
 \|/  |
 /    |
      |
=======",
            // 6 left leg
@"
  +---+
  |   |
  O   |
 \|/  |
 / \  |
      |
=======",
            // 7 right hand
@"
  +---+
  |   |
 _O   |
 \|/  |
 / \  |
      |
=======",
            // 8 left hand
@"
  +---+
  |   |
 _O_  |
 \|/  |
 / \  |
      |
=======",
            // 9 right foot (close to complete)
@"
  +---+
  |   |
 _O_  |
 \|/  |
_/ \  |
      |
=======",
            // 10 left foot — you’re hanged
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
            Console.WriteLine("HANG — Game of Hangman");
            Console.WriteLine("Guess letters (A–Z) or the whole word. You may miss up to 10 times.");
            Console.WriteLine("Type Q anytime to quit.\n");

            while (true)
            {
                string secret = WORDS[rng.Next(WORDS.Length)].ToUpperInvariant();
                HashSet<char> used = new();
                HashSet<char> found = new();
                foreach (var ch in secret.Where(char.IsLetter)) found.Add(ch); // prefill set of letters (for win check)
                found.Clear(); // then clear; we’ll add as we find

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

        void PrintBoard(char[] pattern, HashSet<char> used, int misses)
        {
            Console.WriteLine();
            Console.WriteLine("HERE ARE THE LETTERS YOU USED:");
            Console.WriteLine(string.Join(' ', used.OrderBy(c => c)));
            Console.WriteLine(Stages[Math.Clamp(misses, 0, 10)]);
            Console.WriteLine(new string(pattern.Select(c => c == '_' ? '-' : c).ToArray())); // dashed look like the scan
            Console.WriteLine();
        }
    }
}
