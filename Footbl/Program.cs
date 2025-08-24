using System;
using System.Collections.Generic;

namespace FootBlGame
{
    internal static class Program
    {
        static void Main()
        {
            Console.Title = "FOOTBL (DEC PDP-11 BASIC inspired)";
            new Footbl().Run();
        }
    }

    // ------------------ Game Engine ------------------
    internal sealed class Footbl
    {
        readonly Random rng = new();
        Team human = new("YOU");
        Team comp  = new("CPU");
        int ballYd = 25;   // yard line from receiving team’s goal (0..100)
        int toGo = 10;
        int down = 1;
        int quarter = 1;
        int secLeft = 15 * 60;   // seconds this quarter
        bool humanOnOffense = true;
        int humanTOLeft = 3, compTOLeft = 3;

        enum OffPlay { Run=1, Pass=11, Screen=12, LongPass=13, Draw=14, Punt=15, FieldGoal=16 }
        enum DefPlay { Normal=1, Hold=5, Blitz=7, Intercept=9, Block=13 }

        public void Run()
        {
            PrintIntro();
            Kickoff(starting: true);

            while (quarter <= 4)
            {
                if (secLeft <= 0) { NextQuarter(); continue; }
                if (ballYd <= 0) { TouchbackToOpponent(); continue; }
                if (ballYd >= 100) { Touchdown(); continue; }

                PrintSituation();

                var off = AskOffense();
                if (off == OffPlay.Punt) { DoPunt(); continue; }
                if (off == OffPlay.FieldGoal) { TryFieldGoal(); continue; }

                var def = CpuDefense(off);

                int playTime = rng.Next(18, 36);   // 18–35 sec
                int yards = ResolveYards(off, def);

                AdvanceClock(playTime);

                if (off == OffPlay.Pass || off == OffPlay.Screen || off == OffPlay.LongPass)
                {
                    if (yards == int.MinValue)
                    {
                        Console.WriteLine("PASS INCOMPLETE.");
                        NextDown(0);
                        continue;
                    }
                }

                Console.WriteLine(off == OffPlay.Run || off == OffPlay.Draw
                    ? $"GAIN OF {Math.Max(0,yards)}."
                    : $"PASS COMPLETE. GAIN OF {Math.Max(0,yards)}.");

                if (yards < 0) Console.WriteLine($"LOSS OF {-yards}.");

                ballYd += yards * (humanOnOffense ? 1 : -1);
                if (humanOnOffense) ballYd = Math.Clamp(ballYd, 0, 100);
                else                ballYd = Math.Clamp(ballYd, 0, 100);

                if (ballYd >= 100 || ballYd <= 0)
                {
                    if ((humanOnOffense && ballYd >= 100) || (!humanOnOffense && ballYd <= 0))
                    {
                        Touchdown();
                    }
                    else
                    {
                        TouchbackToOpponent();
                    }
                    continue;
                }

                NextDown(yards);
            }

            Console.WriteLine();
            Console.WriteLine("FINAL SCORE:");
            Console.WriteLine($"{human.Name} {human.Score} — {comp.Name} {comp.Score}");
        }

        // ---------- Menus ----------
        void PrintIntro()
        {
            Console.WriteLine("THIS IS A DEMONSTRATION OF PDP-11 BASIC (modern C# port).");
            Console.WriteLine("EIGHT OFFENSIVE PLAYS / FIVE DEFENSIVE LOOKS. NO PENALTIES.");
            Console.WriteLine("You vs. Computer. Four quarters, 15 min each.");
            Console.WriteLine();

            Console.WriteLine("OFFENSE PLAYS:");
            Console.WriteLine("  1) RUN        11) PASS       12) SCREEN PASS   13) LONG PASS");
            Console.WriteLine(" 14) DRAW       15) PUNT       16) FIELD GOAL");
            Console.WriteLine("DEFENSE (CPU chooses): NORMAL / HOLD / BLITZ / INTERCEPT / BLOCK");
            Console.WriteLine("Type T at any time to call a TIMEOUT (3 per half). Type Q to quit.");
            Console.WriteLine();
        }

        void PrintSituation()
        {
            Console.WriteLine();
            Console.WriteLine($"Q{quarter}  {secLeft/60:00}:{secLeft%60:00}  {(humanOnOffense ? "YOU" : "CPU")} BALL");
            Console.WriteLine($"DOWN {down} & {toGo}  ON {FieldSpot()}");
            Console.WriteLine($"SCORE  YOU {human.Score}  CPU {comp.Score}");
        }

        string FieldSpot()
        {
            int yard = humanOnOffense ? ballYd : 100 - ballYd;
            string side = humanOnOffense ? "OWN" : "OPP";
            if (yard >= 50) { side = humanOnOffense ? "OPP" : "OWN"; yard = 100 - yard; }
            return $"{side} {yard}";
        }

        OffPlay AskOffense()
        {
            while (true)
            {
                Console.Write("OFFENSIVE PLAY (1,11,12,13,14,15,16 / T timeout / Q quit): ");
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") Environment.Exit(0);
                if (s == "T")
                {
                    if (humanTOLeft > 0) { humanTOLeft--; AdvanceClock(-40); Console.WriteLine("TIMEOUT (YOU)."); }
                    else Console.WriteLine("NO TIMEOUTS LEFT.");
                    continue;
                }
                if (int.TryParse(s, out int n) && (n==1 || (n>=11 && n<=16)))
                    return (OffPlay)n;
                Console.WriteLine("ILLEGAL ENTRY—TRY AGAIN.");
            }
        }

        // ---------- Core logic ----------
        DefPlay CpuDefense(OffPlay offense)
        {
            // simple tendencies
            int r = rng.Next(100);
            return offense switch
            {
                OffPlay.LongPass => r<40 ? DefPlay.Intercept : r<70 ? DefPlay.Hold : DefPlay.Normal,
                OffPlay.Pass or OffPlay.Screen => r<35 ? DefPlay.Intercept : r<60 ? DefPlay.Blitz : DefPlay.Normal,
                OffPlay.Draw or OffPlay.Run => r<40 ? DefPlay.Hold : r<65 ? DefPlay.Blitz : DefPlay.Normal,
                OffPlay.Punt or OffPlay.FieldGoal => r<40 ? DefPlay.Block : DefPlay.Normal,
                _ => DefPlay.Normal
            };
        }

        int ResolveYards(OffPlay off, DefPlay def)
        {
            // Basic outcome tables. Positive = toward offense’s goal.
            // Incomplete pass: return int.MinValue.
            int bias = (def, off) switch
            {
                (DefPlay.Blitz, OffPlay.Pass) or (DefPlay.Blitz, OffPlay.LongPass) => -5,
                (DefPlay.Hold,  OffPlay.Run)  or (DefPlay.Hold,  OffPlay.Draw)     => -3,
                (DefPlay.Intercept, OffPlay.Pass) or (DefPlay.Intercept, OffPlay.LongPass) => -7,
                (DefPlay.Block, OffPlay.FieldGoal) or (DefPlay.Block, OffPlay.Punt) => -5,
                _ => 0
            };

            switch (off)
            {
                case OffPlay.Run:
                    return ClampYards(Normal(4 + bias, 12));
                case OffPlay.Draw:
                    return ClampYards(Normal(6 + bias, 14));
                case OffPlay.Screen:
                {
                    if (rng.Next(100) < (def==DefPlay.Intercept?15:10)) return int.MinValue;
                    return ClampYards(Normal(7 + bias, 18));
                }
                case OffPlay.Pass:
                {
                    if (rng.Next(100) < (def==DefPlay.Intercept?35:22)) return int.MinValue;
                    return ClampYards(Normal(10 + bias, 22));
                }
                case OffPlay.LongPass:
                {
                    if (rng.Next(100) < (def==DefPlay.Intercept?45:30)) return int.MinValue;
                    return ClampYards(Normal(18 + bias, 36));
                }
                default:
                    return 0;
            }
        }

        int Normal(int mean, int spread)
        {
            // Triangular-ish distribution
            int a = rng.Next(-spread, spread+1);
            int b = rng.Next(-spread, spread+1);
            return mean + (a+b)/2;
        }

        int ClampYards(int y) => Math.Clamp(y, -10, 60);

        void NextDown(int gained)
        {
            toGo -= gained;
            if (toGo <= 0)
            {
                Console.WriteLine("FIRST DOWN!");
                down = 1;
                toGo = 10;
            }
            else
            {
                down++;
                if (down == 5)
                {
                    Console.WriteLine("TURNOVER ON DOWNS.");
                    ChangePossession();
                }
            }
        }

        void DoPunt()
        {
            int rush = rng.Next(3, 10);
            AdvanceClock(rush);
            int blockChance = 10;
            if (rng.Next(100) < blockChance)
            {
                Console.WriteLine("PUNT BLOCKED! BALL DEAD AT LINE OF SCRIMMAGE.");
                ChangePossession();
                return;
            }
            int distance = Math.Clamp(Normal(38, 18), 20, 65);
            Console.WriteLine($"THE PUNT IS {distance} YARDS.");
            ballYd += (humanOnOffense ? distance : -distance);
            ballYd = Math.Clamp(ballYd, 0, 100);
            ChangePossession(postKick: true);
        }

        void TryFieldGoal()
        {
            int spotFromOppGoal = humanOnOffense ? 100 - ballYd : ballYd;
            int kickDist = spotFromOppGoal + 17; // +17 for end zone+hold
            int basePct = kickDist <= 30 ? 90 : kickDist <= 40 ? 70 : kickDist <= 50 ? 45 : 10;
            if (rng.Next(100) < basePct)
            {
                Console.WriteLine($"FIELD GOAL IS GOOD FROM {kickDist}!");
                (humanOnOffense?human:comp).Score += 3;
                Kickoff(starting:false);
            }
            else
            {
                Console.WriteLine($"THE KICK IS NO GOOD (FROM {kickDist}).");
                ChangePossession();
            }
        }

        void Touchdown()
        {
            Console.WriteLine("TOUCHDOWN!");
            (humanOnOffense ? human : comp).Score += 6;

            // simple PAT
            if (rng.Next(100) < 94)
            {
                Console.WriteLine("EXTRA POINT IS GOOD.");
                (humanOnOffense ? human : comp).Score += 1;
            }
            else Console.WriteLine("EXTRA POINT NO GOOD.");

            Kickoff(starting:false);
        }

        void TouchbackToOpponent()
        {
            Console.WriteLine("TOUCHBACK.");
            ballYd = 25;
            ChangePossession(resetDownDist: true);
        }

        void Kickoff(bool starting)
        {
            ballYd = 35; // kicking team spot from own goal
            bool humanKicks = starting ? rng.Next(2)==0 : humanOnOffense; // team that just scored kicks
            humanOnOffense = !humanKicks;

            int dist = Math.Clamp(Normal(62, 10), 50, 75);
            ballYd = humanKicks ? dist : 100 - dist;
            Console.WriteLine($"KICKOFF: RETURNED TO {FieldSpot()}.");
            down = 1; toGo = 10;
            if (starting){ quarter = 1; secLeft = 15*60; humanTOLeft = compTOLeft = 3; }
        }

        void ChangePossession(bool resetDownDist=true, bool postKick=false)
        {
            humanOnOffense = !humanOnOffense;
            if (resetDownDist) { down = 1; toGo = 10; }
            if (postKick) { down = 1; toGo = 10; }
        }

        void AdvanceClock(int delta)
        {
            secLeft -= Math.Max(0, delta);
            if (secLeft < 0) secLeft = 0;
        }

        void NextQuarter()
        {
            quarter++;
            if (quarter == 3)
            {
                Console.WriteLine();
                Console.WriteLine("— HALFTIME —");
                humanTOLeft = compTOLeft = 3;
                // simple halftime kickoff
                Kickoff(starting:false);
            }
            if (quarter<=4) secLeft = 15*60;
        }
    }

    internal sealed class Team
    {
        public string Name { get; }
        public int Score { get; set; }
        public Team(string n) { Name = n; }
    }
}
