using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModernBaseball
{
    enum Half { Top, Bottom }

    class Team
    {
        public string Name;
        public int Runs;
        public int[] Lineup = Enumerable.Range(0, 9).ToArray();
        public int BatterIdx;
        public Team(string name) { Name = name; }
        public int NextBatter()
        {
            int n = Lineup[BatterIdx];
            BatterIdx = (BatterIdx + 1) % 9;
            return n;
        }
    }

    class Bases
    {
        // first, second, third occupied?
        public bool[] B = new bool[3];

        public int Advance(int bases) // return runs scored
        {
            int runs = 0;
            for (int step = 0; step < bases; step++)
            {
                if (B[2]) { runs++; B[2] = false; }
                if (B[1]) { B[2] = true; B[1] = false; }
                if (B[0]) { B[1] = true; B[0] = false; }
            }
            return runs;
        }

        public int ForceAdvanceOnWalk()
        {
            int runs = 0;
            if (B[0] && B[1] && B[2]) { runs++; }
            if (B[1] && B[0]) { B[2] = true; }
            if (B[0]) { B[1] = true; }
            B[0] = true;
            return runs;
        }

        public int GroundBallDP(ref int outs) // simple 6-4-3
        {
            if (outs >= 2) return 0;
            if (B[0]) { B[0] = false; outs += 2; return 0; }
            outs += 1; return 0;
        }

        public override string ToString()
        {
            string s1 = B[0] ? "1" : ".";
            string s2 = B[1] ? "2" : ".";
            string s3 = B[2] ? "3" : ".";
            return $"[{s1}{s2}{s3}]";
        }
    }

    class Game
    {
        readonly Team Away, Home;
        readonly Random rng = new Random();
        readonly List<string> log = new List<string>();
        int inning = 1;
        Half half = Half.Top;
        int outs = 0, balls = 0, strikes = 0;
        Bases bases = new Bases();

        public Game(string away, string home)
        {
            Away = new Team(away);
            Home = new Team(home);
        }

        public void Play()
        {
            Log($"Welcome to {Home.Name} Stadium — {Away.Name} vs {Home.Name}\n");
            while (!IsGameOver())
            {
                BeginHalf();
                while (outs < 3)
                {
                    var offense = Offense();
                    var defense = Defense();

                    int batter = offense.NextBatter();
                    balls = 0; strikes = 0;
                    Log($"\n{SideLabel()} {offense.Name} — Batter #{batter + 1}  {bases}  Outs:{outs}");

                    if (IsUserTeam(offense))
                        AtBat_User(offense, defense);
                    else
                        AtBat_CPU(offense, defense);
                }
                EndHalf();
            }
            PrintFinal();
            File.WriteAllLines("bb_playbyplay.txt", log);
            Console.WriteLine($"\nPlay-by-play saved to {Path.GetFullPath("bb_playbyplay.txt")}");
        }

        bool IsUserTeam(Team t) => ReferenceEquals(t, Away); // you are the Away team

        Team Offense() => half == Half.Top ? Away : Home;
        Team Defense() => half == Half.Top ? Home : Away;

        void BeginHalf()
        {
            Log($"\n=== {HalfInningLabel()} — {ScoreLine()} ===");
            outs = 0; balls = 0; strikes = 0; bases = new Bases();
        }

        void EndHalf()
        {
            Log($"End of the {HalfInningLabel().ToLower()} — {ScoreLine()}");
            if (half == Half.Bottom) inning++;
            half = half == Half.Top ? Half.Bottom : Half.Top;
        }

        string HalfInningLabel() => half == Half.Top ? $"Top {inning}" : $"Bottom {inning}";
        string SideLabel() => half == Half.Top ? "↑" : "↓";
        string ScoreLine() => $"{Away.Name} {Away.Runs} — {Home.Name} {Home.Runs}";

        bool IsGameOver()
        {
            if (inning < 9) return false;
            if (half == Half.Top) return false;
            if (inning > 9 && Away.Runs == Home.Runs) return false;
            return (inning >= 9 && half == Half.Bottom && Away.Runs != Home.Runs && outs == 0);
        }

        // ---------- At-bats ----------
        void AtBat_User(Team O, Team D)
        {
            while (true)
            {
                Console.Write($"Count {balls}-{strikes}  Bases {bases}  Outs {outs} | Choose: [S]wing  [T]ake  [B]unt  [Q]uit: ");
                var key = (Console.ReadKey(true).KeyChar.ToString().ToUpper());
                if (key == "Q") QuitNow();
                if (key == "S" || key == "T" || key == "B")
                {
                    ResolvePlateAppearance(O, D, key switch
                    {
                        "S" => BatterChoice.Swing,
                        "T" => BatterChoice.Take,
                        "B" => BatterChoice.Bunt,
                        _ => BatterChoice.Swing
                    });
                    break;
                }
            }
        }

        void AtBat_CPU(Team O, Team D)
        {
            // simple CPU: with <2 outs & runner on first and no others, 20% bunt; else 70% swing, 30% take
            var runners = (bases.B[0] ? 1 : 0) + (bases.B[1] ? 1 : 0) + (bases.B[2] ? 1 : 0);
            BatterChoice c;
            if (outs < 2 && bases.B[0] && runners == 1 && rng.NextDouble() < 0.20) c = BatterChoice.Bunt;
            else c = rng.NextDouble() < 0.70 ? BatterChoice.Swing : BatterChoice.Take;
            ResolvePlateAppearance(O, D, c);
        }

        enum BatterChoice { Swing, Take, Bunt }

        void ResolvePlateAppearance(Team O, Team D, BatterChoice choice)
        {
            switch (choice)
            {
                case BatterChoice.Take:
                    TakePitch(O); break;
                case BatterChoice.Bunt:
                    AttemptBunt(O); break;
                default:
                    SwingBat(O); break;
            }
        }

        void TakePitch(Team O)
        {
            double pBall = 0.58;
            if (rng.NextDouble() < pBall)
            {
                balls++; Log("Taken for a ball.");
                if (balls >= 4)
                {
                    balls = strikes = 0;
                    int runs = bases.ForceAdvanceOnWalk();
                    if (runs > 0) { O.Runs += runs; Log($"Walk forces in a run! +{runs}"); }
                    else Log("Batter walks.");
                }
            }
            else
            {
                strikes++; Log("Taken for a strike.");
                if (strikes >= 3) { outs++; balls = strikes = 0; Log("Strike three looking — OUT."); }
            }
        }

        void AttemptBunt(Team O)
        {
            double pGoodSac = 0.65;
            double pFoulStrike = 0.20;
            double pBuntHit = 0.10;

            double r = rng.NextDouble();
            if (r < pBuntHit) { O.Runs += bases.Advance(1); bases.B[0] = true; Log("Perfect bunt — infield single."); }
            else if (r < pBuntHit + pGoodSac)
            {
                if (bases.B[2] || bases.B[1] || bases.B[0])
                {
                    O.Runs += bases.Advance(1);
                    outs++; Log("Good sacrifice — runner(s) advance, batter out.");
                }
                else
                {
                    outs++; Log("Bunt attempt with empty bases — easy out.");
                }
                balls = strikes = 0;
            }
            else if (r < pBuntHit + pGoodSac + pFoulStrike)
            {
                strikes++; Log("Bunt attempt goes foul — strike.");
                if (strikes >= 3) { outs++; balls = strikes = 0; Log("Strike three — OUT."); }
            }
            else
            {
                outs++; Log("Popped up the bunt — OUT.");
                balls = strikes = 0;
            }
        }

        void SwingBat(Team O)
        {
            double leverage = (balls - strikes) * 0.04;
            double pHR = Clamp(0.04 + leverage, 0.01, 0.08);
            double p3B = 0.01;
            double p2B = Clamp(0.07 + leverage, 0.03, 0.12);
            double p1B = Clamp(0.20 + leverage, 0.10, 0.32);
            double pBB = Clamp(0.04 + balls * 0.02, 0.02, 0.09);
            double pK = Clamp(0.20 - leverage, 0.12, 0.30);
            double pOut = 1 - (pHR + p3B + p2B + p1B + pBB + pK);

            double r = rng.NextDouble();
            if (r < pHR)
            {
                int runs = 1 + (bases.B[0] ? 1 : 0) + (bases.B[1] ? 1 : 0) + (bases.B[2] ? 1 : 0);
                bases = new Bases();
                O.Runs += runs; balls = strikes = 0;
                Log($"HOMER! {runs} run(s) score.");
                return;
            }
            r -= pHR;
            if (r < p3B)
            {
                int runs = bases.Advance(3);
                bases.B[0] = false; bases.B[1] = false; bases.B[2] = true;
                O.Runs += runs; balls = strikes = 0; Log("Triple off the wall!");
                return;
            }
            r -= p3B;
            if (r < p2B)
            {
                int runs = bases.Advance(2);
                bases.B[0] = false; bases.B[1] = true;
                O.Runs += runs; balls = strikes = 0; Log("Drilled to the gap — double.");
                return;
            }
            r -= p2B;
            if (r < p1B)
            {
                int runs = bases.Advance(1);
                bases.B[0] = true;
                O.Runs += runs; balls = strikes = 0; Log("Line drive — single.");
                return;
            }
            r -= p1B;
            if (r < pBB)
            {
                balls++;
                Log("Close pitch — called ball.");
                if (balls >= 4)
                {
                    balls = strikes = 0;
                    int runs = bases.ForceAdvanceOnWalk();
                    if (runs > 0) { O.Runs += runs; Log($"Walk forces in a run! +{runs}"); }
                    else Log("Batter walks.");
                }
                return;
            }
            r -= pBB;
            if (r < pK)
            {
                strikes++;
                if (strikes >= 3) { outs++; balls = strikes = 0; Log("Swing and a miss — strike three!"); }
                else Log("Swing and a miss — strike.");
                return;
            }
            // ball in play out — sometimes grounder DP with runner on first
            bool grounder = rng.NextDouble() < 0.55;
            if (grounder && bases.B[0] && outs <= 1 && rng.NextDouble() < 0.45)
            {
                bases.GroundBallDP(ref outs);
                Log("Hard grounder — double play!");
                balls = strikes = 0;
                return;
            }
            outs++; balls = strikes = 0;
            Log(grounder ? "Chopper to short — out at first." : "Fly ball — caught.");
        }

        // ---------- utilities ----------
        static double Clamp(double v, double lo, double hi) => Math.Min(hi, Math.Max(lo, v));
        void Log(string s) { Console.WriteLine(s); log.Add(s); }
        void PrintFinal()
        {
            Log($"\nFINAL — {ScoreLine()}");
            Console.WriteLine("\nThanks for playing!");
        }
        static void QuitNow()
        {
            Console.WriteLine("\nQuitting game…");
            Environment.Exit(0);
        }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("*** MODERN BASEBALL ***\n");
            Console.Write("Away team name (you) or 'Q' to quit: ");
            var awayIn = Console.ReadLine();                            // string?
            if (string.Equals(awayIn?.Trim(), "Q", StringComparison.OrdinalIgnoreCase)) { Console.WriteLine("Quitting…"); return; }
            string away = string.IsNullOrWhiteSpace(awayIn) ? "Panthers" : awayIn!.Trim();

            Console.Write("Home team name (CPU) or 'Q' to quit: ");
            var homeIn = Console.ReadLine();                            // string?
            if (string.Equals(homeIn?.Trim(), "Q", StringComparison.OrdinalIgnoreCase)) { Console.WriteLine("Quitting…"); return; }
            string home = string.IsNullOrWhiteSpace(homeIn) ? "Rockets" : homeIn!.Trim();

            Console.WriteLine("\nTip: During your at-bats choose S (swing), T (take), B (bunt), or Q (quit).");

            var g = new Game(away, home);
            g.Play();
        }
    }
}
