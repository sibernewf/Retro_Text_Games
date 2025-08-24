using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PoetrySuite
{
    internal static class Program
    {
        private static readonly Random Rng = new();

        static void Main()
        {
            Console.Title = "Poetry Suite — Themes, Rhymes & Forms";
            ThemeRepo.LoadThemesOrDefaults();

            while (true)
            {
                Console.WriteLine("\n=== ADVANCED POETRY ===");
                Console.WriteLine($"Theme: {ThemeRepo.Current.Name}   (T=next theme)");
                Console.WriteLine($"Alliteration: {(Poet.Alliteration ? "ON" : "OFF")}   (A=toggle)");
                Console.WriteLine("Choose:");
                Console.WriteLine(" 1) Haiku (5-7-5)");
                Console.WriteLine(" 2) Couplet (AA)");
                Console.WriteLine(" 3) Quatrain ABAB");
                Console.WriteLine(" 4) Quatrain ABBA");
                Console.WriteLine(" 5) Tercet ABA");
                Console.WriteLine(" 6) Quintain AABBA");
                Console.WriteLine(" 7) Free verse");
                Console.WriteLine(" 0) Quit");
                Console.Write("> ");

                var cmd = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (cmd == "0") return;
                if (cmd == "T") { ThemeRepo.NextTheme(); continue; }
                if (cmd == "A") { Poet.Alliteration = !Poet.Alliteration; continue; }

                var outBuf = new StringBuilder();
                void Print(string s) { Console.WriteLine(s); outBuf.AppendLine(s); }

                Print("");
                switch (cmd)
                {
                    case "1": Poet.Haiku(Print); break;
                    case "2": Poet.Couplet(Print); break;
                    case "3": Poet.QuatrainABAB(Print); break;
                    case "4": Poet.QuatrainABBA(Print); break;
                    case "5": Poet.TercetABA(Print); break;
                    case "6": Poet.QuintainAABBA(Print); break;
                    case "7": Poet.FreeVerse(Print); break;
                    default: Print("Pick 1–7, T, A, or 0."); break;
                }
                Print("");
                Print("BY  A.  COM  PUTER.");
                Print("");

                // Offer to save
                Console.Write("Save to a .txt file? (y/N) > ");
                var save = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
                if (save is "y" or "yes")
                {
                    var dir = Path.Combine(AppContext.BaseDirectory, "Poems");
                    Directory.CreateDirectory(dir);
                    var name = $"poem-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
                    var path = Path.Combine(dir, name);
                    File.WriteAllText(path, outBuf.ToString());
                    Console.WriteLine($"Saved: {path}");
                }
            }
        }

        // ====== Core data structures ======

        enum Pos { Det, Adj, Noun, Verb, Prep, Adv, Gerund }

        sealed class Word
        {
            public string Text { get; }
            public int Syl { get; }
            public string Rhyme { get; }   // rhyme class
            public Pos Pos { get; }
            public char Initial => char.ToUpperInvariant(Text[0]);
            public Word(string text, int syl, Pos pos, string rhyme = "")
            { Text = text; Syl = syl; Pos = pos; Rhyme = rhyme; }
            public override string ToString() => Text;
        }

        sealed class Theme
        {
            public string Name { get; }
            public List<Word> Words { get; } = new();
            public List<Pos[]> Templates { get; } = new();
            public Theme(string name) { Name = name; }
        }

        static class ThemeRepo
        {
            public static List<Theme> Themes { get; } = new();
            private static int _idx = 0;
            public static Theme Current => Themes[_idx];

            public static void NextTheme() => _idx = ( _idx + 1 ) % Themes.Count;

            public static void LoadThemesOrDefaults()
            {
                Themes.Clear();
                var root = Path.Combine(AppContext.BaseDirectory, "Themes");
                Directory.CreateDirectory(root);

                var dirs = Directory.EnumerateDirectories(root).ToList();
                if (dirs.Count == 0)
                {
                    // Embedded defaults if no Themes on disk
                    Themes.Add(EmbeddedNature());
                    Themes.Add(EmbeddedPoe());
                    _idx = 0;
                    return;
                }

                foreach (var dir in dirs)
                {
                    var name = Path.GetFileName(dir);
                    var t = new Theme(name);

                    var lex = Path.Combine(dir, "lexicon.txt");
                    if (File.Exists(lex))
                    {
                        foreach (var raw in File.ReadLines(lex))
                        {
                            var line = raw.Split('#')[0].Trim();
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            var parts = line.Split('|');
                            if (parts.Length < 3) continue;

                            var wordTxt = parts[0].Trim();
                            if (!TryParsePos(parts[1], out var pos)) continue;
                            if (!int.TryParse(parts[2], out var syl)) syl = 1;
                            var rhyme = parts.Length >= 4 ? parts[3].Trim() : "";
                            var weight = (parts.Length >= 5 && int.TryParse(parts[4], out var w) && w > 0) ? w : 1;

                            for (int i = 0; i < weight; i++)
                                t.Words.Add(new Word(wordTxt, syl, pos, rhyme));
                        }
                    }

                    var tplPath = Path.Combine(dir, "templates.txt");
                    if (File.Exists(tplPath))
                    {
                        foreach (var raw in File.ReadLines(tplPath))
                        {
                            var line = raw.Split('#')[0].Trim();
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            var tags = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                            var seq = new List<Pos>();
                            foreach (var tag in tags) if (TryParsePos(tag, out var p)) seq.Add(p);
                            if (seq.Count > 0) t.Templates.Add(seq.ToArray());
                        }
                    }

                    if (t.Words.Count > 0)
                    {
                        if (t.Templates.Count == 0) t.Templates.AddRange(DefaultTemplates());
                        Themes.Add(t);
                    }
                }

                if (Themes.Count == 0) Themes.Add(EmbeddedNature());
                _idx = 0;
            }

            static bool TryParsePos(string s, out Pos pos)
            {
                s = s.Trim().ToLowerInvariant();
                pos = s switch
                {
                    "det" => Pos.Det, "adj" => Pos.Adj, "noun" => Pos.Noun, "verb" => Pos.Verb,
                    "prep" => Pos.Prep, "adv" => Pos.Adv, "gerund" => Pos.Gerund, _ => Pos.Det
                };
                return s is "det" or "adj" or "noun" or "verb" or "prep" or "adv" or "gerund";
            }

            static IEnumerable<Pos[]> DefaultTemplates() => new[]
            {
                new[]{ Pos.Det, Pos.Adj, Pos.Noun, Pos.Verb, Pos.Prep, Pos.Det, Pos.Noun },
                new[]{ Pos.Det, Pos.Noun, Pos.Verb, Pos.Adv },
                new[]{ Pos.Gerund, Pos.Prep, Pos.Det, Pos.Noun },
                new[]{ Pos.Noun, Pos.Verb, Pos.Prep, Pos.Det, Pos.Adj, Pos.Noun },
                new[]{ Pos.Det, Pos.Adj, Pos.Noun }
            };

            static Theme EmbeddedNature()
            {
                var t = new Theme("Nature");
                // small embedded default set (used only if Themes/ missing)
                t.Words.AddRange(new[]
                {
                    new Word("the",1,Pos.Det), new Word("a",1,Pos.Det),
                    new Word("soft",1,Pos.Adj,"OFT"), new Word("wild",1,Pos.Adj,"ILD"),
                    new Word("quiet",2,Pos.Adj), new Word("green",1,Pos.Adj,"EEN"),
                    new Word("golden",2,Pos.Adj,"OLD"),
                    new Word("dawn",1,Pos.Noun,"AWN"), new Word("pine",1,Pos.Noun,"INE"),
                    new Word("ocean",2,Pos.Noun,"OH"), new Word("breeze",1,Pos.Noun,"EEZ"),
                    new Word("stone",1,Pos.Noun,"ONE"), new Word("river",2,Pos.Noun,"IV"),
                    new Word("light",1,Pos.Noun,"ITE"), new Word("fern",1,Pos.Noun,"ERN"),
                    new Word("wide sky",2,Pos.Noun,"AI"),
                    new Word("sings",1,Pos.Verb,"INGS"), new Word("glitters",2,Pos.Verb,"IT"),
                    new Word("rises",2,Pos.Verb,"IZE"), new Word("wanders",2,Pos.Verb,"AN"),
                    new Word("rests",1,Pos.Verb,"ESTS"), new Word("whispers",2,Pos.Verb,"IZ"),
                    new Word("under",2,Pos.Prep), new Word("above",2,Pos.Prep), new Word("across",2,Pos.Prep),
                    new Word("forever",3,Pos.Adv,"EV"), new Word("slowly",2,Pos.Adv,"OH"),
                    new Word("falling",2,Pos.Gerund,"AWL"), new Word("shimmering",3,Pos.Gerund,"IM"),
                });
                t.Templates.AddRange(DefaultTemplates());
                return t;
            }

            static Theme EmbeddedPoe()
            {
                var t = new Theme("Poe");
                t.Words.AddRange(new[]
                {
                    new Word("the",1,Pos.Det), new Word("my",1,Pos.Det),
                    new Word("midnight",2,Pos.Adj), new Word("dreary",2,Pos.Adj,"EER"),
                    new Word("bleak",1,Pos.Adj,"EEK"), new Word("ghastly",2,Pos.Adj,"AST"),
                    new Word("dark",1,Pos.Adj,"ARK"), new Word("lonely",2,Pos.Adj,"OH"),
                    new Word("raven",2,Pos.Noun,"AVE"), new Word("shadow",2,Pos.Noun,"OW"),
                    new Word("door",1,Pos.Noun,"ORE"), new Word("chamber",2,Pos.Noun,"AIM"),
                    new Word("dream",1,Pos.Noun,"EEM"), new Word("silence",2,Pos.Noun,"ILE"),
                    new Word("terror",2,Pos.Noun,"AIR"),
                    new Word("whispers",2,Pos.Verb,"IZ"), new Word("tapping",2,Pos.Verb,"AP"),
                    new Word("creeps",1,Pos.Verb,"EEPS"), new Word("broods",1,Pos.Verb,"OODS"),
                    new Word("burns",1,Pos.Verb,"URNS"), new Word("lingers",2,Pos.Verb,"ING"),
                    new Word("upon",2,Pos.Prep), new Word("through",1,Pos.Prep,"OO"),
                    new Word("within",2,Pos.Prep), new Word("evermore",3,Pos.Adv,"ORE"),
                    new Word("nevermore",3,Pos.Adv,"ORE"),
                    new Word("beating",2,Pos.Gerund,"EET"), new Word("fluttering",3,Pos.Gerund,"UT"),
                });
                t.Templates.AddRange(DefaultTemplates());
                return t;
            }
        }

        // ====== Poet engine ======

        static class Poet
        {
            public static bool Alliteration = false;
            static Random Rng => Program.Rng;

            static IEnumerable<Word> Words => ThemeRepo.Current.Words;
            static IEnumerable<Word> Of(Pos p) => Words.Where(w => w.Pos == p);
            static IEnumerable<Word> Rhymes(string key) =>
                string.IsNullOrEmpty(key) ? Words : Words.Where(w => w.Rhyme == key);
            static List<Pos[]> Templates => ThemeRepo.Current.Templates;

            // UI wrappers (print action passed in so we can capture output)
            public static void Haiku(Action<string> print)
            {
                print("— HAIKU —");
                print("");
                print(BuildLine(5));
                print(BuildLine(7));
                print(BuildLine(5));
            }

            public static void Couplet(Action<string> print)
            {
                print("— COUPLET (AA) —");
                print("");
                var A = PickRhyme();
                print(BuildLine(Rng.Next(8, 11), A));
                print(BuildLine(Rng.Next(8, 11), A));
            }

            public static void QuatrainABAB(Action<string> print)
            {
                print("— QUATRAIN (ABAB) —");
                print("");
                var A = PickRhyme();
                var B = PickRhyme(exclude: A);
                print(BuildLine(Rng.Next(8, 11), A));
                print(BuildLine(Rng.Next(8, 11), B));
                print(BuildLine(Rng.Next(8, 11), A));
                print(BuildLine(Rng.Next(8, 11), B));
            }

            public static void QuatrainABBA(Action<string> print)
            {
                print("— QUATRAIN (ABBA) —");
                print("");
                var A = PickRhyme();
                var B = PickRhyme(exclude: A);
                print(BuildLine(Rng.Next(8, 11), A));
                print(BuildLine(Rng.Next(8, 11), B));
                print(BuildLine(Rng.Next(8, 11), B));
                print(BuildLine(Rng.Next(8, 11), A));
            }

            public static void TercetABA(Action<string> print)
            {
                print("— TERCET (ABA) —");
                print("");
                var A = PickRhyme();
                var B = PickRhyme(exclude: A);
                print(BuildLine(Rng.Next(7, 10), A));
                print(BuildLine(Rng.Next(7, 10), B));
                print(BuildLine(Rng.Next(7, 10), A));
            }

            public static void QuintainAABBA(Action<string> print)
            {
                print("— QUINTAIN (AABBA) —");
                print("");
                var A = PickRhyme();
                var B = PickRhyme(exclude: A);
                print(BuildLine(Rng.Next(7, 10), A));
                print(BuildLine(Rng.Next(7, 10), A));
                print(BuildLine(Rng.Next(7, 10), B));
                print(BuildLine(Rng.Next(7, 10), B));
                print(BuildLine(Rng.Next(7, 10), A));
            }

            public static void FreeVerse(Action<string> print)
            {
                print("— FREE VERSE —");
                print("");
                int lines = Rng.Next(3, 7);
                string? rhyme = Rng.NextDouble() < 0.5 ? PickRhyme() : null;
                for (int i = 0; i < lines; i++)
                {
                    int s = Rng.Next(6, 12);
                    string rk = rhyme != null && (i % 2 == 0) ? rhyme : "";
                    print(BuildLine(s, rk));
                    if (Rng.NextDouble() < 0.25) print("");
                }
            }

            // ---- line builder ----

            static string BuildLine(int targetSyl, string rhymeKey = "")
            {
                var tpl = Templates.Count > 0 ? Templates[Rng.Next(Templates.Count)] : new[] { Pos.Det, Pos.Adj, Pos.Noun };

                // chance to trim template for variety
                if (tpl.Length > 3 && Rng.NextDouble() < 0.25)
                    tpl = tpl.Take(tpl.Length - 1).ToArray();

                char? mustStart = null;
                if (Alliteration && Rng.NextDouble() < 0.6)
                {
                    var heads = Words.Where(w => w.Syl == 1).ToList();
                    if (heads.Count > 0) mustStart = heads[Rng.Next(heads.Count)].Initial;
                }

                var chosen = new List<Word>();
                int syl = 0;
                for (int i = 0; i < tpl.Length; i++)
                {
                    var pos = tpl[i];
                    Word? pick = null;

                    for (int t = 0; t < 12; t++)
                    {
                        var pool = Of(pos).ToList();
                        if (mustStart != null && (pos == Pos.Adj || pos == Pos.Noun || pos == Pos.Verb))
                            pool = pool.Where(w => w.Initial == mustStart).ToList();
                        if (pool.Count == 0) pool = Of(pos).ToList();

                        var cand = pool[Rng.Next(pool.Count)];
                        if (syl + cand.Syl <= targetSyl || i == tpl.Length - 1) { pick = cand; break; }
                    }
                    pick ??= Of(pos).OrderBy(w => w.Syl).First();
                    chosen.Add(pick);
                    syl += pick.Syl;
                }

                // enforce rhyme on last word if requested
                if (!string.IsNullOrEmpty(rhymeKey))
                {
                    var rhymes = Rhymes(rhymeKey).Where(w => w.Pos is Pos.Noun or Pos.Verb or Pos.Adv).ToList();
                    if (rhymes.Count > 0)
                    {
                        var last = chosen[^1];
                        var best = rhymes.OrderBy(w => Math.Abs((syl - last.Syl) - targetSyl)).First();
                        syl = syl - last.Syl + best.Syl;
                        chosen[^1] = best;
                    }
                }

                // pad if short
                while (syl < targetSyl && Rng.NextDouble() < 0.7)
                {
                    var pads = Of(Pos.Det).Concat(Of(Pos.Adv)).Where(w => w.Syl <= (targetSyl - syl)).ToList();
                    if (pads.Count == 0) break;
                    var add = pads[Rng.Next(pads.Count)];
                    chosen.Insert(Math.Max(1, chosen.Count - 1), add);
                    syl += add.Syl;
                }

                var sb = new StringBuilder();
                for (int i = 0; i < chosen.Count; i++)
                {
                    if (i > 0) sb.Append(' ');
                    sb.Append(chosen[i].Text);
                    if (i == chosen.Count - 2 && Rng.NextDouble() < 0.18) sb.Append(Rng.NextDouble() < 0.5 ? "," : " —");
                }

                string line = Capitalize(sb.ToString());
                string end = ".";
                if (Rng.NextDouble() < 0.15) end = "…";
                else if (Rng.NextDouble() < 0.20) end = ",";
                else if (!string.IsNullOrEmpty(rhymeKey) && Rng.NextDouble() < 0.2) end = ";";
                return line + end;
            }

            static string PickRhyme(string? exclude = null)
            {
                var groups = Words.Select(w => w.Rhyme).Where(k => !string.IsNullOrEmpty(k)).Distinct().ToList();
                if (exclude != null) groups = groups.Where(g => g != exclude).ToList();
                return groups.Count == 0 ? "" : groups[Rng.Next(groups.Count)];
            }

            static string Capitalize(string s) =>
                string.IsNullOrWhiteSpace(s) ? s : char.ToUpperInvariant(s[0]) + (s.Length > 1 ? s[1..] : "");
        }
    }
}
