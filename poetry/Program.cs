using System;
using System.Collections.Generic;
using System.Linq;

namespace PoetrySuite
{
    internal static class Program
    {
        private static readonly Random Rng = new();

        static void Main()
        {
            Console.Title = "POETRY / POET – Random Verse";

            while (true)
            {
                Console.WriteLine("\n=== RANDOM POETRY ===");
                Console.WriteLine("1) POETRY  (classic singlet/couplet/quatrain)");
                Console.WriteLine("2) POET    (haiku-style with punctuation/indent/paragraphs)");
                Console.WriteLine("0) Quit");
                Console.Write("> ");
                var choice = (Console.ReadLine() ?? "").Trim();

                if (choice == "0") return;
                if (choice == "1") { PoetryClassic.Run(); continue; }
                if (choice == "2") { PoetHaiku.Run(); continue; }
            }
        }

        // ------------------------------------------------------------
        // POETRY — classic (23 preset lines; 1–4 lines output)
        // ------------------------------------------------------------
        private static class PoetryClassic
        {
            // 23 short lines (public-domain vibes; feel free to edit)
            private static readonly string[] Lines =
            {
                "TIME IS TWISTED TO LAP UPON",     //  1
                "ITSELF FOREVER",                  //  2
                "NOBODY LOSES ALL THE TIME",       //  3
                "AND IT IS DAWN",                  //  4
                "THE WORLD",                       //  5
                "GOES FORTH TO MURDER DREAMS",     //  6
                "HIS MOST RARE MUSIC STOLE",       //  7
                "NOTHING FROM DEATH",              //  8
                "AND IT IS DUSK ON EARTH",         //  9
                "THE PEOPLE ARE IN THEIR HOUSES",  // 10
                "SHE SLEEPS WITH DEATH UPON HER MOUTH", // 11
                "HER HOURS SONG IN HER EYES",      // 12
                "THE HOURS DESCENDED PUTTING ON STARS", // 13
                "IN THE MIRROR I SEE A MAN, AND HE",    // 14
                "SCREAMS",                         // 15
                "FOR HE IS ME",                    // 16
                "AND I HE",                        // 17
                "NIGHT IS A CANDLE IS LIGHTED",    // 18
                "AND IT IS DARK",                  // 19
                "A SONG UPON HER MOUTH",           // 20
                "EVENING ON EARTH",                // 21
                "HAVING DEATH IN HER EYES",        // 22
                "THE CITY WAKES"                   // 23
            };

            public static void Run()
            {
                Console.WriteLine("\nRANDOM POETRY IN FOUR PART HARMONY.\n");

                // 1=singlet, 2=couplet, 4=quatrain (roughly as in the BASIC)
                int[] stanzaSizes = { 1, 2, 4 };
                int size = stanzaSizes[Rng.Next(stanzaSizes.Length)];

                // Pick unique random lines
                var chosen = Lines.OrderBy(_ => Rng.Next()).Take(size).ToList();

                // A little random vertical spacing like the original
                void MaybeBlank() { if (Rng.NextDouble() < 0.35) Console.WriteLine(); }

                foreach (var line in chosen)
                {
                    MaybeBlank();
                    Console.WriteLine(line.ToUpperInvariant());
                }
                MaybeBlank();

                Console.WriteLine("\nBY A. COM PUTER.\n");
            }
        }

        // ------------------------------------------------------------
        // POET — haiku-like with commas/indent/paragraphs
        // ------------------------------------------------------------
        private static class PoetHaiku
        {
            // Two themes. Each theme has 4 groups × 5 phrases.
            private static readonly string[][] PoeTheme =
            {
                new [] { "MIDNIGHT DREARY", "THING OF EVIL", "DARKNESS THERE", "SLOWLY CREEPING", "STILL SITTING" },
                new [] { "PROPHET", "FIREY EYES", "NEVERMORE", "NOTHING MORE", "SIGN OF PARTING" },
                new [] { "BIRD OR FIEND", "BURNED", "THRILLED ME", "YET AGAIN", "MY SOUL EVERMORE" },
                new [] { "QUOTH THE RAVEN", "SHALL BE LIFTED", "FROM THIS DOOR", "SHADOW DREAMING", "THE RAVEN EVERMORE" }
            };

            private static readonly string[][] NatureTheme =
            {
                new [] { "CARPET OF FERNS", "MORNING DEW", "TANG OF DAWN", "SWAYING PINES", "MIGHTY OAKS" },
                new [] { "RUSTLING LEAVES", "SHADES OF GREEN", "GRACE AND BEAUTY", "SILENTLY SINGING", "NATURE SPEAKING" },
                new [] { "SOOTHING ME", "TRANQUILITY", "RADIATES CALM", "ENTRANCES ME", "SO PEACEFUL" },
                new [] { "UNTOUCHED UNSPOILED", "WHISPERING BREEZE", "SUN-WARM STONE", "FALLING WATER", "WIDE OPEN SKY" }
            };

            public static void Run()
            {
                Console.WriteLine("\nPOET — RANDOM HAIKU-STYLE VERSE");
                Console.WriteLine("Choose theme: 1) Poe  2) Nature  (Enter for 1)");
                Console.Write("> ");
                var t = (Console.ReadLine() ?? "").Trim();
                var groups = t == "2" ? NatureTheme : PoeTheme;

                // probabilities (from description)
                const double pComma = 0.19;
                const double pIndent = 0.22;
                const double pNewParagraph = 0.18;
                const int minParagraphBreak = 20;

                int sinceBreak = 0;
                int linesToGenerate = 80; // stop after a while (original ran long)

                Console.WriteLine();
                for (int i = 0; i < linesToGenerate; i++)
                {
                    // cycle groups 0..3 mostly in order (with a tiny chance to jump)
                    int g = i % 4;
                    if (Rng.NextDouble() < 0.05) g = Rng.Next(4); // small randomness like the BASIC versions

                    string phrase = Pick(groups[g]);

                    // punctuation
                    if (Rng.NextDouble() < pComma) phrase += ",";

                    // indentation
                    string indent = Rng.NextDouble() < pIndent ? "    " : "";

                    // print line
                    Console.WriteLine(indent + phrase);

                    sinceBreak++;
                    bool breakNow = Rng.NextDouble() < pNewParagraph || sinceBreak >= minParagraphBreak;
                    if (breakNow)
                    {
                        Console.WriteLine();
                        sinceBreak = 0;
                    }
                }

                Console.WriteLine("BY  A.  COM  PUTER.\n");
            }

            private static string Pick(string[] arr) => arr[Rng.Next(arr.Length)];
        }
    }
}
