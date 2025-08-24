using System;
using System.Collections.Generic;
using System.Linq;

namespace Yahtze
{
    enum Cat
    {
        Aces, Twos, Threes, Fours, Fives, Sixes,
        ThreeKind, FourKind, FullHouse, SmallStraight, LargeStraight, Yahtzee, Chance
    }

    class Player
    {
        public string Name = "";
        public Dictionary<Cat, int?> Scores = Enum.GetValues(typeof(Cat))
                                               .Cast<Cat>()
                                               .ToDictionary(c => c, _ => (int?)null);

        public int UpperSubtotal =>
            (Scores[Cat.Aces] ?? 0) + (Scores[Cat.Twos] ?? 0) + (Scores[Cat.Threes] ?? 0) +
            (Scores[Cat.Fours] ?? 0) + (Scores[Cat.Fives] ?? 0) + (Scores[Cat.Sixes] ?? 0);

        public int UpperBonus => UpperSubtotal >= 63 ? 35 : 0;

        public int GrandTotal => Scores.Values.Where(v => v.HasValue).Sum(v => v!.Value) + UpperBonus;

        public bool CategoryAvailable(Cat c) => Scores[c] is null;

        public IEnumerable<Cat> AvailableCategories() => Scores.Where(kv => kv.Value is null).Select(kv => kv.Key);
    }

    internal static class Program
    {
        private static readonly Random Rng = new Random();

        static void Main()
        {
            Console.WriteLine("YAHTZEE — up to 15 players\n");

            int nPlayers = AskInt("HOW MANY PEOPLE WISH TO PLAY? ", 1, 15);
            var players = new List<Player>();
            for (int i = 1; i <= nPlayers; i++)
            {
                Console.Write($"NAME {i}? ");
                players.Add(new Player { Name = (Console.ReadLine() ?? $"P{i}").Trim() });
            }

            // 13 rounds
            for (int round = 1; round <= 13; round++)
            {
                Console.WriteLine($"\nROUND {round}\n");
                foreach (var p in players)
                {
                    TakeTurn(p);
                    Console.WriteLine(ScoresLine(players));
                }
            }

            // Final scoreboard
            Console.WriteLine("\nTHE GAME IS OVER.\n");
            foreach (var p in players.OrderByDescending(p => p.GrandTotal))
            {
                Console.WriteLine($"{p.Name.ToUpper(),-15}  TOTAL: {p.GrandTotal,4}  (Upper {p.UpperSubtotal}, Bonus {p.UpperBonus})");
            }
            Console.WriteLine($"\nTHE WINNER IS {players.OrderByDescending(p => p.GrandTotal).First().Name.ToUpper()}!");
        }

        private static void TakeTurn(Player p)
        {
            Console.WriteLine($"{p.Name.ToUpper()}'S TURN");
            var dice = Roll5();
            int rolls = 1;
            while (rolls <= 3)
            {
                Console.WriteLine($"YOU HAVE {DiceToString(dice)}");
                if (rolls == 3) break;

                int change = AskInt("HOW MANY DO YOU WANT TO CHANGE ? ", 0, 5);
                if (change == 0) break;

                var which = AskIndices($"WHICH ? (enter {change} die indices 1–5, space-separated): ", change);
                foreach (var idx in which) dice[idx - 1] = Rng.Next(1, 7);
                rolls++;
                Console.WriteLine();
            }

            // Choose category
            while (true)
            {
                var entry = AskString("HOW DO YOU WANT THIS ROUND SCORED ? (type category, SUMMARY, or ZERO) ")
                            .ToUpperInvariant();

                if (entry == "SUMMARY")
                {
                    PrintSummary(p);
                    continue;
                }
                if (entry == "ZERO")
                {
                    Cat c = AskCategory("WHAT DO YOU WANT TO ZERO ? ", p, mustBeAvailable: true);
                    p.Scores[c] = 0;
                    Console.WriteLine($"{p.Name} YOU GET A SCORE OF 0 FOR THIS ROUND");
                    break;
                }

                if (!TryParseCategory(entry, out var cat))
                {
                    Console.WriteLine("UNKNOWN CATEGORY. Try ACES,TWOS,THREES,FOURS,FIVES,SIXES,THREE OF A KIND,FOUR OF A KIND,FULL HOUSE,SM. STRAIGHT,LG. STRAIGHT,YAHTZEE,CHANCE.");
                    continue;
                }
                if (!p.CategoryAvailable(cat))
                {
                    Console.WriteLine("YOU HAVE ALREADY USED THAT CATEGORY.");
                    continue;
                }

                int score = Score(cat, dice);
                p.Scores[cat] = score;
                Console.WriteLine($"{p.Name} YOU GET A SCORE OF {score} FOR THIS ROUND");
                break;
            }
        }

        // ---------- prompts ----------
        private static int AskInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int v) && v >= min && v <= max) return v;
            }
        }

        private static string AskString(string prompt)
        {
            Console.Write(prompt);
            return (Console.ReadLine() ?? "").Trim();
        }

        private static int[] AskIndices(string prompt, int count)
        {
            while (true)
            {
                Console.Write(prompt);
                var parts = (Console.ReadLine() ?? "").Split(new[] { ' ', ',', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != count) { Console.WriteLine($"Please enter exactly {count} indices."); continue; }
                var idx = new List<int>();
                bool ok = true;
                foreach (var p in parts)
                {
                    if (!int.TryParse(p, out int v) || v < 1 || v > 5) { ok = false; break; }
                    idx.Add(v);
                }
                if (ok) return idx.ToArray();
            }
        }

        private static void PrintSummary(Player p)
        {
            Console.WriteLine("\nSUMMARY OF USED CATEGORIES:");
            foreach (Cat c in Enum.GetValues(typeof(Cat)))
            {
                string used = p.Scores[c] is null ? "(unused)" : p.Scores[c]!.Value.ToString();
                Console.WriteLine($"{CatLabel(c),-15}: {used}");
            }
            Console.WriteLine($"Upper subtotal: {p.UpperSubtotal}   Bonus: {p.UpperBonus}   Grand total: {p.GrandTotal}\n");
        }

        private static string ScoresLine(List<Player> players)
            => string.Join("   ", players.Select(p => $"{p.Name.ToUpper()} HAS {p.GrandTotal} POINTS"));

        // ---------- dice & scoring ----------
        private static int[] Roll5() => Enumerable.Range(0, 5).Select(_ => Rng.Next(1, 7)).ToArray();

        private static string DiceToString(int[] d)
            => $"{d[0]} {d[1]} {d[2]} {d[3]} {d[4]}";

        private static bool TryParseCategory(string s, out Cat cat)
        {
            s = s.Replace(" ", "").Replace(".", "").ToUpperInvariant();
            switch (s)
            {
                case "ACES": cat = Cat.Aces; return true;
                case "TWOS": cat = Cat.Twos; return true;
                case "THREES": cat = Cat.Threes; return true;
                case "FOURS": cat = Cat.Fours; return true;
                case "FIVES": cat = Cat.Fives; return true;
                case "SIXES": cat = Cat.Sixes; return true;
                case "THREEOFAKIND": case "3KIND": case "THREEKIND": cat = Cat.ThreeKind; return true;
                case "FOUROFAKIND": case "4KIND": case "FOURKIND": cat = Cat.FourKind; return true;
                case "FULLHOUSE": case "FH": cat = Cat.FullHouse; return true;
                case "SMALLSTRAIGHT": case "SMSTRAIGHT": case "SM.STRAIGHT": case "SMALL": cat = Cat.SmallStraight; return true;
                case "LARGESTRAIGHT": case "LGSTRAIGHT": case "LG.STRAIGHT": case "LARGE": cat = Cat.LargeStraight; return true;
                case "YAHTZEE": case "YAHTZE": cat = Cat.Yahtzee; return true;
                case "CHANCE": cat = Cat.Chance; return true;
                default: cat = Cat.Aces; return false;
            }
        }

        private static Cat AskCategory(string prompt, Player p, bool mustBeAvailable)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim();
                if (TryParseCategory(s, out var c))
                {
                    if (!mustBeAvailable || p.CategoryAvailable(c)) return c;
                    Console.WriteLine("YOU HAVE ALREADY USED THAT CATEGORY.");
                }
                else
                {
                    Console.WriteLine("UNKNOWN CATEGORY.");
                }
            }
        }

        private static int Score(Cat c, int[] d)
        {
            var counts = new int[7];
            foreach (var x in d) counts[x]++;

            int Sum() => d.Sum();

            switch (c)
            {
                case Cat.Aces:   return counts[1] * 1;
                case Cat.Twos:   return counts[2] * 2;
                case Cat.Threes: return counts[3] * 3;
                case Cat.Fours:  return counts[4] * 4;
                case Cat.Fives:  return counts[5] * 5;
                case Cat.Sixes:  return counts[6] * 6;

                case Cat.ThreeKind:
                    return counts.Any(n => n >= 3) ? Sum() : 0;

                case Cat.FourKind:
                    return counts.Any(n => n >= 4) ? Sum() : 0;

                case Cat.FullHouse:
                    return (counts.Any(n => n == 3) && counts.Any(n => n == 2)) ? 25 : 0;

                case Cat.SmallStraight:
                    // any 4-length run among 1-2-3-4-5-6
                    var set = new HashSet<int>(d);
                    bool ss = (set.Contains(1) && set.Contains(2) && set.Contains(3) && set.Contains(4)) ||
                              (set.Contains(2) && set.Contains(3) && set.Contains(4) && set.Contains(5)) ||
                              (set.Contains(3) && set.Contains(4) && set.Contains(5) && set.Contains(6));
                    return ss ? 30 : 0;

                case Cat.LargeStraight:
                    bool ls = d.Distinct().Count() == 5 &&
                              ((d.Contains(1) && d.Contains(2) && d.Contains(3) && d.Contains(4) && d.Contains(5)) ||
                               (d.Contains(2) && d.Contains(3) && d.Contains(4) && d.Contains(5) && d.Contains(6)));
                    return ls ? 40 : 0;

                case Cat.Yahtzee:
                    return counts.Any(n => n == 5) ? 50 : 0;

                case Cat.Chance:
                    return Sum();

                default: return 0;
            }
        }

        private static string CatLabel(Cat c) => c switch
        {
            Cat.Aces => "Aces",
            Cat.Twos => "Twos",
            Cat.Threes => "Threes",
            Cat.Fours => "Fours",
            Cat.Fives => "Fives",
            Cat.Sixes => "Sixes",
            Cat.ThreeKind => "Three of a kind",
            Cat.FourKind => "Four of a kind",
            Cat.FullHouse => "Full house",
            Cat.SmallStraight => "Sm. straight",
            Cat.LargeStraight => "Lg. straight",
            Cat.Yahtzee => "Yahtzee",
            Cat.Chance => "Chance",
            _ => c.ToString()
        };
    }
}
