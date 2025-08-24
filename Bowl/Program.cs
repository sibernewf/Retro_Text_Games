using System;
using System.Collections.Generic;
using System.Linq;

namespace BowlingModern
{
    class Player
    {
        public string Name { get; set; } = string.Empty; // init to satisfy nullable analysis
        public List<int> Rolls { get; } = new();         // flattened list of rolls (0..10)
        public Player(string name) { Name = name; }
    }

    class Program
    {
        static readonly Random Rng = new();

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("BOWL — Simulated Bowling Game");
            Console.WriteLine("Type ENTER (or 'ROLL') to bowl. No manual pin selection.");
            Console.WriteLine("After each roll we show the pin diagram (+ = standing, 0 = down) and an analysis:");
            Console.WriteLine("GUTTER (first ball 0) / STRIKE / SPARE / ERROR (second ball 0 with pins still up).");
            Console.WriteLine("Type Q at any prompt to quit.\n");

            int nPlayers = AskInt("How many players (2–4)? ", 2, 4);
            var players = new List<Player>();
            for (int i = 1; i <= nPlayers; i++)
            {
                Console.Write($"Player {i} name: ");
                string? raw = Console.ReadLine();
                string name = string.IsNullOrWhiteSpace(raw) ? $"Player {i}" : raw!;
                if (name.Equals("Q", StringComparison.OrdinalIgnoreCase)) return;
                players.Add(new Player(name));
            }

            // ---- game loop: 10 frames ----
            for (int frame = 1; frame <= 10; frame++)
            {
                Console.WriteLine($"\n===== FRAME {frame} =====");
                foreach (var p in players)
                {
                    Console.WriteLine($"\n{p.Name} — Frame {frame}");
                    FrameFor(p, frame);
                    ShowScoreboard(players, frame);
                }
            }

            // ---- final standings ----
            Console.WriteLine("\n===== FINAL =====");
            ShowScoreboard(players, 10);
            Console.WriteLine("\nThanks for bowling!");
        }

        // ----------------- frame logic -----------------
        static void FrameFor(Player p, int frame)
        {
            bool[] standing = NewRack();

            // first ball
            int first = BowlRollPrompt(p.Name, frame, ball: 1, standing, allowStrike: true);
            ApplyHitRandom(standing, first);
            DrawPinsPlusZero(standing);
            PrintAnalysisFirst(first);

            if (frame < 10)
            {
                if (first == 10) // strike
                {
                    p.Rolls.Add(10);
                    return;
                }

                // second ball
                int second = BowlRollPrompt(p.Name, frame, ball: 2, standing, allowStrike: false);
                int before = CountStanding(standing);
                ApplyHitRandom(standing, second);
                DrawPinsPlusZero(standing);
                PrintAnalysisSecond(before, second);

                p.Rolls.Add(first);
                if (before > 0 && second == before) p.Rolls.Add(10 - first); // spare stored as remainder
                else p.Rolls.Add(second);
            }
            else // 10th frame
            {
                p.Rolls.Add(first);

                int second = BowlRollPrompt(p.Name, frame, ball: 2, standing, allowStrike: true);
                int before2 = CountStanding(standing);
                ApplyHitRandom(standing, second);
                DrawPinsPlusZero(standing);
                // (Optional flavor line on 10th — we just keep the standard analysis for non-strike/spare cases)
                if (first < 10) PrintAnalysisSecond(before2, second);
                p.Rolls.Add(second);

                bool allowThird = first == 10 || (before2 > 0 && second == before2); // strike or spare
                if (allowThird)
                {
                    standing = NewRack();
                    int third = BowlRollPrompt(p.Name, frame, ball: 3, standing, allowStrike: true);
                    ApplyHitRandom(standing, third);
                    DrawPinsPlusZero(standing);
                    p.Rolls.Add(third);
                }
            }
        }

        // ----------------- bowling prompts & RNG -----------------
        static int BowlRollPrompt(string name, int frame, int ball, bool[] standing, bool allowStrike)
        {
            int max = CountStanding(standing);
            while (true)
            {
                Console.Write($"{name} — Frame {frame}, Ball {ball}. Press ENTER or type ROLL (Q quits): ");
                string? raw = Console.ReadLine();
                string s = (raw ?? string.Empty).Trim().ToUpperInvariant();
                if (s == "Q") Environment.Exit(0);
                if (s == string.Empty || s == "ROLL")
                {
                    int knocked = RandomRoll(standing, firstBall: ball == 1, allowStrike: allowStrike);
                    knocked = Math.Clamp(knocked, 0, max);
                    return knocked;
                }
                Console.WriteLine("Please press ENTER or type ROLL (or Q to quit).");
            }
        }

        // Friendly RNG for pinfall:
        // - First ball (full rack): ~10% strike; mass around 4–8; small gutter chance.
        // - First ball (partial rack, 10th): tapered around half the standing pins.
        // - Second ball: fair spare odds; higher when only a few remain; otherwise partials with slight bias high.
        static int RandomRoll(bool[] standing, bool firstBall, bool allowStrike)
        {
            int remaining = CountStanding(standing);
            if (remaining <= 0) return 0;

            if (firstBall && allowStrike && remaining == 10)
            {
                var weights = new double[] { 0.04, 0.04, 0.05, 0.07, 0.11, 0.13, 0.15, 0.15, 0.12, 0.09, 0.10 }; // 0..10
                return SampleWeighted(weights);
            }

            if (!firstBall)
            {
                if (remaining <= 3)
                {
                    double pSpare = remaining switch { 1 => 0.85, 2 => 0.60, _ => 0.45 };
                    if (Rng.NextDouble() < pSpare) return remaining;
                }
                else
                {
                    double pSpare = 0.30 + 0.10 * Rng.NextDouble();
                    if (Rng.NextDouble() < pSpare) return remaining;
                }
                // partial cleanup
                return Math.Max(0, Math.Min(remaining - 1, SampleTapered(0, remaining - 1, skewHigh: true)));
            }
            else
            {
                // first ball with some pins down already (10th frame after strike/fill)
                return Math.Min(remaining, SampleTapered(0, remaining, skewHigh: false));
            }
        }

        static int SampleTapered(int lo, int hi, bool skewHigh)
        {
            if (lo >= hi) return lo;
            double w = skewHigh ? Math.Pow(Rng.NextDouble(), 0.7) : (Rng.NextDouble() + Rng.NextDouble()) / 2.0;
            int k = lo + (int)Math.Round(w * (hi - lo));
            return k;
        }

        static int SampleWeighted(double[] weightsInclusiveFor0toN)
        {
            double sum = 0;
            foreach (var w in weightsInclusiveFor0toN) sum += w;
            double r = Rng.NextDouble() * sum;
            double acc = 0;
            for (int i = 0; i < weightsInclusiveFor0toN.Length; i++)
            {
                acc += weightsInclusiveFor0toN[i];
                if (r <= acc) return i;
            }
            return weightsInclusiveFor0toN.Length - 1;
        }

        // ----------------- scoring -----------------
        static int ScoreUpToFrame(Player p, int frames)
        {
            int score = 0;
            int i = 0; // index into rolls
            for (int f = 1; f <= frames && f <= 10; f++)
            {
                if (i >= p.Rolls.Count) break;

                if (IsStrike(p.Rolls, i)) // 10 in first roll
                {
                    score += 10 + Bonus(p.Rolls, i + 1, 2);
                    i += 1;
                }
                else
                {
                    int first = Safe(p.Rolls, i);
                    int second = Safe(p.Rolls, i + 1);
                    int frameSum = first + second;

                    if (frameSum == 10) // spare
                        score += 10 + Bonus(p.Rolls, i + 2, 1);
                    else
                        score += frameSum;

                    i += 2;
                }

                if (f == 10)
                {
                    // 10th frame fill balls are accounted via Bonus and rolls progression
                }
            }
            return score;
        }

        static bool IsStrike(List<int> rolls, int i) => Safe(rolls, i) == 10;
        static int Bonus(List<int> rolls, int start, int count)
        {
            int sum = 0;
            for (int k = 0; k < count; k++) sum += Safe(rolls, start + k);
            return sum;
        }
        static int Safe(List<int> rolls, int i) => (i >= 0 && i < rolls.Count) ? rolls[i] : 0;

        // ----------------- scoreboard -----------------
        static void ShowScoreboard(List<Player> players, int upToFrame)
        {
            Console.WriteLine();
            foreach (var p in players)
            {
                int total = ScoreUpToFrame(p, upToFrame);
                Console.WriteLine($"{p.Name,-12}  Score: {total,3}   Rolls: {RollString(p.Rolls)}");
            }
        }

        static string RollString(List<int> rolls)
        {
            var parts = new List<string>();
            int i = 0; int frame = 1;
            while (i < rolls.Count && frame <= 10)
            {
                if (i < rolls.Count && rolls[i] == 10 && frame < 10)
                { parts.Add("X"); i += 1; frame++; }
                else
                {
                    int a = Safe(rolls, i);
                    int b = Safe(rolls, i + 1);
                    if (a == 10) { parts.Add("X"); i += 2; frame++; } // 10th-frame oddity
                    else if (a + b == 10) { parts.Add($"{Glyph(a)} /"); i += 2; frame++; }
                    else { parts.Add($"{Glyph(a)} {Glyph(b)}"); i += 2; frame++; }
                }
                if (frame == 11 && i < rolls.Count)
                {
                    var tail = string.Join(" ", rolls.Skip(i).Take(2).Select(Glyph));
                    if (!string.IsNullOrEmpty(tail)) parts.Add(tail);
                }
            }
            return string.Join(" | ", parts);
        }

        static string Glyph(int pins) => pins switch
        {
            10 => "X",
            0 => "–",
            _ => pins.ToString()
        };

        // ----------------- pins / diagram -----------------
        // pin order (1..10): 7 8 9 10 / 4 5 6 / 2 3 / 1
        static bool[] NewRack() => new bool[] { false, true, true, true, true, true, true, true, true, true, true };
        static int CountStanding(bool[] standing) => standing.Count(x => x);

        static void ApplyHitRandom(bool[] standing, int knocked)
        {
            // remove 'knocked' random standing pins (more natural than left-to-right)
            var idx = new List<int>();
            for (int i = 1; i <= 10; i++) if (standing[i]) idx.Add(i);
            Shuffle(idx);
            foreach (var pin in idx)
            {
                if (knocked <= 0) break;
                standing[pin] = false;
                knocked--;
            }
        }

        static void DrawPinsPlusZero(bool[] s)
        {
            // '+' for standing and '0' for down, per the original description
            Console.WriteLine();
            PrintRow(s[7], s[8], s[9], s[10]);
            PrintRow(s[4], s[5], s[6]);
            PrintRow(s[2], s[3]);
            PrintRow(s[1]);
            Console.WriteLine();
            static void PrintRow(params bool[] pins)
            {
                int pad = (4 - pins.Length);
                Console.WriteLine(new string(' ', pad * 2) + string.Join(" ", pins.Select(p => p ? "+" : "0")));
            }
        }

        static void PrintAnalysisFirst(int first)
        {
            if (first == 10) Console.WriteLine("STRIKE!!!");
            else if (first == 0) Console.WriteLine("GUTTER!!!");
        }

        static void PrintAnalysisSecond(int pinsBefore, int second)
        {
            if (pinsBefore > 0 && second == pinsBefore) Console.WriteLine("SPARE!!!");
            else if (pinsBefore > 0 && second == 0) Console.WriteLine("ERROR!!!");
        }

        // utils
        static void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        static int AskInt(string prompt, int lo, int hi)
        {
            while (true)
            {
                Console.Write(prompt);
                string? raw = Console.ReadLine();
                if ((raw ?? string.Empty).Trim().Equals("Q", StringComparison.OrdinalIgnoreCase)) Environment.Exit(0);
                if (int.TryParse(raw, out int v) && v >= lo && v <= hi) return v;
            }
        }
    }
}
