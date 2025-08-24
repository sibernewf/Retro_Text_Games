using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoetrySuiteAdvanced
{
    internal static class Program
    {
        private static readonly Random Rng = new();

        static void Main()
        {
            Console.Title = "Poetry Suite — Advanced";

            while (true)
            {
                Console.WriteLine("\n=== ADVANCED POETRY ===");
                Console.WriteLine("1) Haiku (5-7-5)");
                Console.WriteLine("2) Couplet (AA)");
                Console.WriteLine("3) Quatrain (ABAB)");
                Console.WriteLine("4) Free verse (3–6 lines)");
                Console.WriteLine("T) Toggle theme (current: {0})", ThemeBank.CurrentThemeName);
                Console.WriteLine("A) Toggle alliteration (current: {0})", (Poet.Alliteration ? "ON" : "OFF"));
                Console.WriteLine("0) Quit");
                Console.Write("> ");
                var cmd = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

                if (cmd == "0") return;
                if (cmd == "T") { ThemeBank.ToggleTheme(); continue; }
                if (cmd == "A") { Poet.Alliteration = !Poet.Alliteration; continue; }

                Console.WriteLine();
                switch (cmd)
                {
                    case "1": Poet.Haiku(); break;
                    case "2": Poet.Couplet(); break;
                    case "3": Poet.Quatrain(); break;
                    case "4": Poet.FreeVerse(); break;
                    default:  Console.WriteLine("Pick 1–4, T, A, or 0."); break;
                }
                Console.WriteLine("\nBY  A.  COM  PUTER.\n");
            }
        }

        // ======== Core ========

        // Parts of speech; we keep it minimal
        enum Pos { Det, Adj, Noun, Verb, Prep, Adv, Gerund } // Gerund acts as noun-like

        sealed class Word
        {
            public string Text { get; }
            public int Syl { get; }
            public string Rhyme { get; }   // rhyme class for line endings
            public Pos Pos { get; }
            public char Initial => char.ToUpperInvariant(Text[0]);
            public Word(string text, int syl, Pos pos, string rhyme = "")
            { Text = text; Syl = syl; Pos = pos; Rhyme = rhyme; }
            public override string ToString() => Text;
        }

        static class ThemeBank
        {
            // ——— POE ———
            static readonly List<Word> Poe = new()
            {
                // determiners
                new("the",1,Pos.Det), new("a",1,Pos.Det), new("that",1,Pos.Det), new("this",1,Pos.Det), new("my",1,Pos.Det),
                // adjectives
                new("midnight",2,Pos.Adj), new("dreary",2,Pos.Adj,"EER"), new("bleak",1,Pos.Adj,"EEK"),
                new("ghastly",2,Pos.Adj,"AST"), new("ancient",2,Pos.Adj),
                new("dark",1,Pos.Adj,"ARK"), new("lonely",2,Pos.Adj,"OH"),
                // nouns
                new("raven",2,Pos.Noun,"AVE"), new("shadow",2,Pos.Noun,"OW"),
                new("door",1,Pos.Noun,"ORE"), new("chamber",2,Pos.Noun,"AIM"),
                new("dream",1,Pos.Noun,"EEM"), new("midnight",2,Pos.Noun),
                new("silence",2,Pos.Noun,"ILE"), new("terror",2,Pos.Noun,"AIR"),
                // verbs
                new("whispers",2,Pos.Verb,"IZ"), new("tapping",2,Pos.Verb,"AP"),
                new("creeps",1,Pos.Verb,"EEPS"), new("broods",1,Pos.Verb,"OODS"),
                new("burns",1,Pos.Verb,"URNS"), new("lingers",2,Pos.Verb,"ING"),
                // preps / adv / gerunds
                new("upon",2,Pos.Prep), new("through",1,Pos.Prep,"OO"),
                new("within",2,Pos.Prep), new("evermore",3,Pos.Adv,"ORE"),
                new("nevermore",3,Pos.Adv,"ORE"),
                // gerunds (noun-ish ends)
                new("beating",2,Pos.Gerund,"EET"), new("fluttering",3,Pos.Gerund,"UT"),
            };

            // ——— NATURE ———
            static readonly List<Word> Nature = new()
            {
                new("the",1,Pos.Det), new("a",1,Pos.Det), new("this",1,Pos.Det), new("soft",1,Pos.Adj,"OFT"), new("wild",1,Pos.Adj,"ILD"),
                new("quiet",2,Pos.Adj), new("green",1,Pos.Adj,"EEN"), new("golden",2,Pos.Adj,"OLD"),
                new("dawn",1,Pos.Noun,"AWN"), new("pine",1,Pos.Noun,"INE"), new("ocean",2,Pos.Noun,"OH"),
                new("breeze",1,Pos.Noun,"EEZ"), new("stone",1,Pos.Noun,"ONE"), new("fern",1,Pos.Noun,"ERN"),
                new("river",2,Pos.Noun,"IV"), new("light",1,Pos.Noun,"ITE"),
                new("sings",1,Pos.Verb,"INGS"), new("glitters",2,Pos.Verb,"IT"),
                new("rises",2,Pos.Verb,"IZE"), new("wanders",2,Pos.Verb,"AN"),
                new("rests",1,Pos.Verb,"ESTS"), new("whispers",2,Pos.Verb,"IZ"),
                new("under",2,Pos.Prep), new("above",2,Pos.Prep), new("across",2,Pos.Prep),
                new("forever",3,Pos.Adv,"EV"), new("slowly",2,Pos.Adv,"OH"),
                new("falling",2,Pos.Gerund,"AWL"), new("shimmering",3,Pos.Gerund,"IM"),
            };

            static bool _poe = false; // default to Nature for nicer demo
            public static string CurrentThemeName => _poe ? "Poe" : "Nature";
            public static IEnumerable<Word> Words => _poe ? Poe : Nature;
            public static void ToggleTheme() => _poe = !_poe;

            public static IEnumerable<Word> Of(Pos pos) => Words.Where(w => w.Pos == pos);
            public static IEnumerable<Word> Rhymes(string key) =>
                string.IsNullOrEmpty(key) ? Words : Words.Where(w => w.Rhyme == key);
        }

        static class Poet
        {
            public static bool Alliteration = false;

            // Templates: each is a small grammar pattern
            static readonly Pos[][] Templates =
            {
                new[]{ Pos.Det, Pos.Adj, Pos.Noun, Pos.Verb, Pos.Prep, Pos.Det, Pos.Noun }, // the dark door broods within the chamber
                new[]{ Pos.Det, Pos.Noun, Pos.Verb, Pos.Adv },                              // the river wanders slowly
                new[]{ Pos.Det, Pos.Adj, Pos.Noun },                                        // the golden light
                new[]{ Pos.Gerund, Pos.Prep, Pos.Det, Pos.Noun },                           // falling upon the stone
                new[]{ Pos.Noun, Pos.Verb, Pos.Prep, Pos.Det, Pos.Adj, Pos.Noun },          // breeze sings across the green pines
            };

            public static void Haiku()
            {
                Console.WriteLine("— HAIKU —\n");
                Console.WriteLine(BuildLine(5));
                Console.WriteLine(BuildLine(7));
                Console.WriteLine(BuildLine(5));
            }

            public static void Couplet()
            {
                Console.WriteLine("— COUPLET —\n");
                string rhymeA = PickRhyme();
                Console.WriteLine(BuildLine(Rng.Next(8, 11), rhymeA));
                Console.WriteLine(BuildLine(Rng.Next(8, 11), rhymeA));
            }

            public static void Quatrain()
            {
                Console.WriteLine("— QUATRAIN (ABAB) —\n");
                string rhymeA = PickRhyme();
                string rhymeB = PickRhyme(exclude: rhymeA);
                Console.WriteLine(BuildLine(Rng.Next(8, 11), rhymeA));
                Console.WriteLine(BuildLine(Rng.Next(8, 11), rhymeB));
                Console.WriteLine(BuildLine(Rng.Next(8, 11), rhymeA));
                Console.WriteLine(BuildLine(Rng.Next(8, 11), rhymeB));
            }

            public static void FreeVerse()
            {
                Console.WriteLine("— FREE VERSE —\n");
                int lines = Rng.Next(3, 7);
                string? rhyme = Rng.NextDouble() < 0.5 ? PickRhyme() : null;
                for (int i = 0; i < lines; i++)
                {
                    int s = Rng.Next(6, 12);
                    string rk = rhyme != null && (i % 2 == 0) ? rhyme : "";
                    Console.WriteLine(BuildLine(s, rk));
                    if (Rng.NextDouble() < 0.25) Console.WriteLine();
                }
            }

            // ---- Builders ----

            static string BuildLine(int targetSyl, string rhymeKey = "")
            {
                var tpl = Templates[Rng.Next(Templates.Length)];
                // small chance to shorten template a bit to hit target better
                if (tpl.Length > 3 && Rng.NextDouble() < 0.25)
                    tpl = tpl.Take(tpl.Length - 1).ToArray();

                // Alliteration (optional): choose a starting letter constraint
                char? mustStart = null;
                if (Alliteration && Rng.NextDouble() < 0.6)
                {
                    var headWords = ThemeBank.Words.Where(w => w.Syl == 1).ToList();
                    if (headWords.Count > 0) mustStart = headWords[Rng.Next(headWords.Count)].Initial;
                }

                var chosen = new List<Word>();
                int syl = 0;
                foreach (var pos in tpl)
                {
                    // try a few times to fit syllable budget
                    Word? pick = null;
                    for (int t = 0; t < 12; t++)
                    {
                        var pool = ThemeBank.Of(pos).ToList();
                        if (mustStart != null && (pos == Pos.Adj || pos == Pos.Noun || pos == Pos.Verb))
                            pool = pool.Where(w => w.Initial == mustStart).ToList();
                        if (pool.Count == 0) pool = ThemeBank.Of(pos).ToList();

                        var candidate = pool[Rng.Next(pool.Count)];
                        if (syl + candidate.Syl <= targetSyl || pos == tpl.Last())
                        { pick = candidate; break; }
                    }
                    pick ??= ThemeBank.Of(pos).OrderBy(w => w.Syl).First();
                    chosen.Add(pick);
                    syl += pick.Syl;
                }

                // Force rhyme on the last word if requested
                if (!string.IsNullOrEmpty(rhymeKey))
                {
                    var rhymes = ThemeBank.Rhymes(rhymeKey).Where(w => w.Pos is Pos.Noun or Pos.Verb or Pos.Adv).ToList();
                    if (rhymes.Count > 0)
                    {
                        // adjust syllables if needed by swapping the last word within a small margin
                        var last = chosen[^1];
                        var best = rhymes.OrderBy(w => Math.Abs((syl - last.Syl) - targetSyl)).First();
                        syl = syl - last.Syl + best.Syl;
                        chosen[^1] = best;
                    }
                }

                // If under target, pad with a tiny word (determiner/adv) when possible
                while (syl < targetSyl && Rng.NextDouble() < 0.7)
                {
                    var pads = ThemeBank.Of(Pos.Det).Concat(ThemeBank.Of(Pos.Adv)).Where(w => w.Syl <= (targetSyl - syl)).ToList();
                    if (pads.Count == 0) break;
                    var add = pads[Rng.Next(pads.Count)];
                    chosen.Insert(Math.Max(1, chosen.Count - 1), add);
                    syl += add.Syl;
                }

                // Compose text
                var sb = new StringBuilder();
                for (int i = 0; i < chosen.Count; i++)
                {
                    if (i > 0) sb.Append(' ');
                    sb.Append(chosen[i].Text);
                    // tasteful mid-line comma / em dash
                    if (i == chosen.Count - 2 && Rng.NextDouble() < 0.18) sb.Append(Rng.NextDouble() < 0.5 ? "," : " —");
                }

                // Capitalize first word
                string line = Capitalize(sb.ToString());

                // End punctuation
                string end = ".";
                if (Rng.NextDouble() < 0.15) end = "…";
                else if (Rng.NextDouble() < 0.20) end = ",";
                else if (!string.IsNullOrEmpty(rhymeKey) && Rng.NextDouble() < 0.2) end = ";";
                return line + end;
            }

            static string PickRhyme(string? exclude = null)
            {
                var groups = ThemeBank.Words.Select(w => w.Rhyme).Where(k => !string.IsNullOrEmpty(k)).Distinct().ToList();
                if (exclude != null) groups = groups.Where(g => g != exclude).ToList();
                return groups.Count == 0 ? "" : groups[Rng.Next(groups.Count)];
            }

            static string Capitalize(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return s;
                return char.ToUpperInvariant(s[0]) + (s.Length > 1 ? s[1..] : "");
            }
        }
    }
}
