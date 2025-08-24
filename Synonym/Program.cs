using System;
using System.Collections.Generic;
using System.Linq;

namespace Synonm
{
    internal static class Program
    {
        // Praise messages from the listing (R$ array).
        static readonly string[] Praise =
        {
            "RIGHT", "CORRECT", "FINE", "GOOD!", "CHECK"
        };

        // DATA statements (500–600 in the BASIC source).
        // Each entry is a group: the first item is the “prompt word” candidate set.
        // We’ll pick one word from the group to ask about; the rest are valid answers.
        static readonly string[][] Groups =
        {
            new[] { "FIRST", "START", "BEGINNING", "ONSET", "INITIAL" },
            new[] { "SIMILAR", "ALIKE", "SAME", "LIKE", "RESEMBLING" },
            new[] { "MODEL", "PATTERN", "PROTOTYPE", "STANDARD", "CRITERION" },
            new[] { "SMALL", "INSIGNIFICANT", "LITTLE", "TINY", "MINUTE" },
            new[] { "STOP", "HALT", "STAY", "ARREST", "CHECK", "STANDSTILL" },
            new[] { "HOUSE", "DWELLING", "RESIDENCE", "DOMICILE", "LODGING", "HABITATION" },
            new[] { "PIT", "HOLE", "HOLLOW", "WELL", "GULF", "CHASM", "ABYSS" },
            new[] { "PUSH", "SHOVE", "THRUST", "PROD", "POKE", "BUTT", "PRESS" },
            new[] { "RED", "ROUGE", "SCARLET", "CRIMSON", "FLAME", "RUBY" },
            new[] { "PAIN", "SUFFERING", "HURT", "MISERY", "DISTRESS", "ACHE", "DISCOMFORT" }
        };

        static void Main()
        {
            Console.WriteLine("SYNONYMS\n");
            Console.WriteLine("A SYNONYM OF A WORD MEANS ANOTHER WORD IN THE ENGLISH");
            Console.WriteLine("LANGUAGE WHICH HAS THE SAME OR VERY NEARLY THE SAME");
            Console.WriteLine("MEANING.\n");
            Console.WriteLine("I CHOOSE A WORD -- YOU TYPE A SYNONYM.");
            Console.WriteLine("IF YOU CAN'T THINK OF A SYNONYM, TYPE THE WORD 'HELP'");
            Console.WriteLine("AND I WILL TELL YOU A SYNONYM.\n");

            var rnd = new Random();

            // Shuffle group order so each is asked once, like the BASIC program.
            var order = Enumerable.Range(0, Groups.Length).OrderBy(_ => rnd.Next()).ToList();

            foreach (var gi in order)
            {
                var group = Groups[gi];
                // Choose a prompt word from the group at random
                var promptIndex = rnd.Next(group.Length);
                var prompt = group[promptIndex];

                // For HELP, we’ll offer random *other* synonyms from this group
                var helpPool = Enumerable.Range(0, group.Length)
                                         .Where(i => i != promptIndex)
                                         .OrderBy(_ => rnd.Next())
                                         .ToList();

                AskUntilCorrect(prompt, group, promptIndex, helpPool, rnd);
            }

            Console.WriteLine();
            Console.WriteLine("SYNONYM DRILL COMPLETED.");
        }

        private static void AskUntilCorrect(
            string prompt,
            string[] group,
            int promptIndex,
            List<int> helpPool,
            Random rnd)
        {
            while (true)
            {
                Console.Write($"\nWHAT IS A SYNONYM OF {prompt}? ");
                var input = (Console.ReadLine() ?? string.Empty).Trim();

                if (input.Length == 0) continue;

                if (input.Equals("HELP", StringComparison.OrdinalIgnoreCase))
                {
                    // Provide a random synonym different from the prompt.
                    if (helpPool.Count == 0)
                    {
                        Console.WriteLine("**** NO MORE HINTS AVAILABLE FOR THIS WORD.");
                    }
                    else
                    {
                        var k = rnd.Next(helpPool.Count);
                        var idx = helpPool[k];
                        helpPool.RemoveAt(k);
                        Console.WriteLine($"**** A SYNONYM OF {prompt} IS {group[idx]}.");
                    }
                    continue;
                }

                // Check against any synonym in the group except the exact prompt word.
                var correct = group
                    .Where((w, i) => i != promptIndex)
                    .Any(w => w.Equals(input, StringComparison.OrdinalIgnoreCase));

                if (correct)
                {
                    Console.WriteLine(Praise[rnd.Next(Praise.Length)]);
                    return;
                }

                Console.WriteLine("TRY AGAIN.");
            }
        }
    }
}
