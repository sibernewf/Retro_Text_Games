using System;
using System.Collections.Generic;

namespace FotBalGame
{
    internal static class Program
    {
        static void Main()
        {
            Console.Title = "FOTBAL (NFU HS BASIC inspired)";
            new Fotbal().Run();
        }
    }

    // A more “play-chart” style with numbered plays (offense & defense).
    internal sealed class Fotbal
    {
        readonly Random rng = new();
        Team t1 = new("TEAM 1");
        Team t2 = new("TEAM 2");

        int quarter = 1;
        int secLeft = 12 * 60;        // shorter quarters like the sample
        int down = 1, toGo = 10;
        int ball = 20;                 // yards from Team 2’s goal; we’ll treat Team 1 on offense first
        bool t1Offense = true;
        int t1TO = 3, t2TO = 3;

        // offense play chart (a subset)
        readonly Dictionary<int, string> oChart = new()
        {
            { 1, "PITCHOUT" }, { 2, "TRIPLE REVERSE" }, { 3, "DIVE" }, { 5, "QB SNEAK" },
            { 8, "END AROUND" }, { 10, "COUNTER REVERSE" }, { 12, "LEFT SWEEP" },
            { 14, "OFF TACKLE" }, { 15, "WISHBONE OPTION" }, { 16, "SLIP SCREEN" },
            { 18, "SCREEN PASS" }, { 19, "SIDELINE PASS" }, { 20, "BOMB!!!" }
        };

        // defense options 1..10: vanilla names
        readonly Dictionary<int, string> dChart = new()
        {
            {1,"4–3 BASE"}, {2,"5–2"}, {3,"NICKEL"}, {4,"DIME"},
            {5,"BLITZ"}, {6,"LINE SLANT"}, {7,"PRESS"}, {8,"TWO-DEEP ZONE"},
            {9,"COVER-3"}, {10,"QUARTERS"}
        };

        public void Run()
        {
            PrintIntro();
            CoinFlipAndKickoff();

            while (quarter <= 4)
            {
                if (secLeft <= 0) { NextQuarter(); continue; }
                if (ball <= 0) { Touchdown(); continue; }
                if (ball >= 100) { Touchback(); continue; }

                PrintSituation();

                int o = AskOffense();
                if (o == -1) return;
                if (o == 98) { CallTimeout(); continue; }
                if (o == 97) { DoPunt(); continue; }
                if (o == 96) { TryFieldGoal(); continue; }

                int d = CpuDefenseChoice();

                var (yards, time, text) = Resolve(o, d);
                Console.WriteLine(text);
                Advance(time);

                ball += (t1Offense ? yards : -yards);
                ball = Math.Clamp(ball, 0, 100);

                if (ball <= 0) { Touchdown(); continue; }
                if (ball >= 100) { Touchback(); continue; }

                NextDown(yards);
            }

            Console.WriteLine();
            Console.WriteLine($"FINAL — {t1.Name} {t1.Score} , {t2.Name} {t2.Score}");
        }

        void PrintIntro()
        {
            Console.WriteLine("NFU FOOTBALL (condensed). No penalties. Computer plays defense.");
            Console.WriteLine("Numbered offense plays, defense chooses a look. Try to manage clock & field.");
            Console.WriteLine();
            PrintCharts();
            Console.WriteLine("Special: 97=PUNT, 96=FIELD GOAL, 98=TIMEOUT, Q=Quit");
            Console.WriteLine();
        }

        void PrintCharts()
        {
            Console.WriteLine("OFFENSE PLAY CHART:");
            foreach (var kv in oChart)
                Console.WriteLine($"{kv.Key,2}  {kv.Value}");
            Console.WriteLine();
            Console.WriteLine("DEFENSE (CPU uses):");
            foreach (var kv in dChart)
                Console.WriteLine($"{kv.Key,2}  {kv.Value}");
            Console.WriteLine();
        }

        void PrintSituation()
        {
            Console.WriteLine();
            Console.WriteLine($"Q{quarter}  {secLeft/60:00}:{secLeft%60:00}   {(t1Offense?t1.Name:t2.Name)} BALL");
            Console.WriteLine($"DOWN {down} & {toGo} ON {FieldSpot()}");
            Console.WriteLine($"SCORE  {t1.Name} {t1.Score} — {t2.Name} {t2.Score}");
        }

        string FieldSpot()
        {
            int yard = t1Offense ? ball : 100 - ball;
            string side = yard < 50 ? "OWN" : "OPP";
            int shown = yard < 50 ? yard : 100 - yard;
            return $"{side} {shown}";
        }

        int AskOffense()
        {
            while (true)
            {
                Console.Write("OFFENSE PLAY (chart # / 97 punt / 96 FG / 98 T.O. / Q quit): ");
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s=="Q") return -1;
                if (int.TryParse(s, out int n))
                {
                    if (oChart.ContainsKey(n) || n is 97 or 96 or 98) return n;
                }
                Console.WriteLine("Try a valid number.");
            }
        }

        int CpuDefenseChoice()
        {
            // favor pass shells if long yardage
            int prefer = toGo >= 8 ? rng.Next(6, 11) : rng.Next(1, 7);
            return Math.Clamp(prefer, 1, 10);
        }

        (int yards, int sec, string text) Resolve(int o, int d)
        {
            int sec = rng.Next(20, 36);
            string oName = oChart[o];
            string dName = dChart[d];

            int baseY = o switch
            {
                1 => Tri(3, 10), 2 => Tri(0, 15), 3 => Tri(2, 6), 5 => Tri(1, 3),
                8 => Tri(0, 12), 10 => Tri(0, 12), 12 => Tri(2, 9),
                14 => Tri(2, 8), 15 => Tri(3, 10), 16 => (rng.Next(100)<20?int.MinValue:Tri(5,14)),
                18 => (rng.Next(100)<25?int.MinValue:Tri(6, 12)),
                19 => (rng.Next(100)<30?int.MinValue:Tri(8, 20)),
                20 => (rng.Next(100)<40?int.MinValue:Tri(15, 35)),
                _ => 0
            };

            // defense effect
            int mod = d switch
            {
                5 => -3,        // blitz
                8 or 9 or 10 => (o>=16? -2 : -1), // deep zones vs passes
                2 => -1,        // 5–2 stout vs run
                _ => 0
            };
            if (baseY == int.MinValue)
                return (0, sec, "PASS INCOMPLETE.");

            int y = Math.Clamp(baseY + mod, -8, 60);
            string result = (o >=16 || o==18 || o==19 || o==20)
                ? $"PASS COMPLETE FOR {Math.Max(0,y)}"
                : (y>=0 ? $"GAIN OF {y}" : $"LOSS OF {-y}");

            return (y, sec, $"{oName} vs {dName}: {result}.");
        }

        int Tri(int mean, int spread)
        {
            int a = rng.Next(-spread, spread+1);
            int b = rng.Next(-spread, spread+1);
            return mean + (a+b)/2;
        }

        void NextDown(int gained)
        {
            toGo -= gained;
            if (toGo <= 0)
            {
                Console.WriteLine("FIRST DOWN.");
                down = 1; toGo = 10;
            }
            else
            {
                down++;
                if (down == 5)
                {
                    Console.WriteLine("TURNOVER ON DOWNS.");
                    ChangeSides();
                }
            }
        }

        void DoPunt()
        {
            Advance(rng.Next(12, 20));
            int dist = Math.Clamp(Tri(42, 16), 20, 65);
            Console.WriteLine($"PUNT {dist}.");
            ball += t1Offense ? dist : -dist;
            ball = Math.Clamp(ball, 0, 100);
            ChangeSides(reset:true);
        }

        void TryFieldGoal()
        {
            int distFromOpp = t1Offense ? 100 - ball : ball;
            int kick = distFromOpp + 17;
            int pct = kick<=30?90:kick<=40?70:kick<=50?45:10;
            Advance(25);
            if (rng.Next(100) < pct)
            {
                Console.WriteLine($"FIELD GOAL GOOD ({kick}).");
                (t1Offense?t1:t2).Score += 3;
                Kickoff(scoringTeamIsOffense:true);
            }
            else
            {
                Console.WriteLine("FIELD GOAL NO GOOD.");
                ChangeSides();
            }
        }

        void Touchdown()
        {
            Console.WriteLine("TOUCHDOWN!");
            (t1Offense?t1:t2).Score += 6;
            if (rng.Next(100) < 94) { Console.WriteLine("PAT GOOD."); (t1Offense?t1:t2).Score++; }
            else Console.WriteLine("PAT MISSED.");
            Kickoff(scoringTeamIsOffense:true);
        }

        void Touchback()
        {
            Console.WriteLine("TOUCHBACK.");
            ball = 25;
            ChangeSides(reset:true);
        }

        void Kickoff(bool scoringTeamIsOffense)
        {
            // scoring team kicks
            int dist = Math.Clamp(Tri(62, 10), 50, 75);
            t1Offense = scoringTeamIsOffense ? !t1Offense : t1Offense;
            ball = t1Offense ? dist : 100 - dist;
            down = 1; toGo = 10;
            Console.WriteLine($"KICKOFF RETURNED TO {FieldSpot()}.");
        }

        void CallTimeout()
        {
            ref int tos = ref (t1Offense ? ref t1TO : ref t2TO);
            if (tos > 0)
            {
                tos--; Advance(-40); Console.WriteLine("TIMEOUT.");
            }
            else Console.WriteLine("NO TIMEOUTS LEFT.");
        }

        void ChangeSides(bool reset=false)
        {
            t1Offense = !t1Offense;
            if (reset) { down=1; toGo=10; }
        }

        void Advance(int s)
        {
            if (s>0) secLeft -= s;
            if (secLeft < 0) secLeft = 0;
        }

        void CoinFlipAndKickoff()
        {
            Console.WriteLine("THE COIN IS FLIPPED...");
            bool team1Receives = rng.Next(2)==0;
            t1Offense = team1Receives;
            ball = team1Receives ? Tri(30,8) : 100 - Tri(30,8);
            Console.WriteLine(team1Receives ? "TEAM 1 RECEIVES KICKOFF." : "TEAM 2 RECEIVES KICKOFF.");
            down=1; toGo=10;
        }

        void NextQuarter()
        {
            quarter++;
            if (quarter == 3) { t1TO = t2TO = 3; }
            if (quarter<=4) secLeft = 12*60;
        }
    }

    internal sealed class Team
    {
        public string Name { get; }
        public int Score { get; set; }
        public Team(string n){ Name = n; }
    }
}
