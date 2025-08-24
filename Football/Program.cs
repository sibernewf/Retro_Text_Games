using System;

namespace FootballGame
{
    internal static class Program
    {
        static void Main()
        {
            Console.Title = "FOOTBALL (with 2-pt conversions, onside kicks, 12-min Qtrs, OT)";
            new Football().Run();
        }
    }

    internal sealed class Football
    {
        readonly Random rng = new();

        Team home, away;
        bool homeOnOffense;

        // 0 = HOME goal line, 100 = AWAY goal line
        int ball;                 // absolute yard line
        int down = 1, toGo = 10;
        int quarter = 1;
        int secLeft;              // seconds remaining in current period
        const int REG_QTR_SEC = 12 * 60;  // 12-minute quarters
        const int OT_QTR_SEC  = 10 * 60;  // single OT sudden-death (10 min)
        int homeTO = 3, awayTO = 3;

        public Football()
        {
            home = new Team(Ask("Enter your team name (home): ", "HOME"));
            away = new Team("CPU");

            // coin flip & opening kickoff
            Console.WriteLine("\nTHE COIN IS FLIPPED...");
            bool receiveHome = rng.Next(2) == 0;
            Console.WriteLine(receiveHome ? $"{home.Name} RECEIVES." : $"{away.Name} RECEIVES.");
            StartHalfKickoff(kickingTeamIsHome: !receiveHome);
        }

        public void Run()
        {
            PrintIntro();

            while (true)
            {
                if (secLeft <= 0)
                {
                    if (!AdvancePeriod()) break; // game ended
                    continue;
                }

                if (ball <= 0) { Touchdown(isHome: true); continue; }
                if (ball >= 100) { Touchdown(isHome: false); continue; }

                PrintSituation();

                if (homeOnOffense)  // YOU on offense
                {
                    var off = AskOffense();
                    if (off == OffPlay.Timeout) { UseTimeout(true); continue; }
                    if (off == OffPlay.Quit) return;
                    if (off == OffPlay.Punt) { DoPunt(true); continue; }
                    if (off == OffPlay.FieldGoal) { TryFieldGoal(true); continue; }

                    var def = CpuDefenseCall(off);
                    ResolvePlay(off, def, true);
                }
                else                 // CPU on offense — YOU pick defense
                {
                    var def = AskDefense();
                    if (def == DefPlay.Timeout) { UseTimeout(false); continue; }
                    if (def == DefPlay.Quit) return;

                    var off = CpuOffenseCall();
                    ResolvePlay(off, def, false);
                }
            }

            Console.WriteLine("\nFINAL SCORE");
            Console.WriteLine($"{home.Name} {home.Score} — {away.Name} {away.Score}");
            PrintStats();
        }

        // ===== Menus & display =====
        void PrintIntro()
        {
            Console.WriteLine("\nFOOTBALL — merged edition + 2PT/Onside/OT. No penalties.");
            Console.WriteLine("You call OFFENSE with the ball and DEFENSE when CPU has it.");
            Console.WriteLine("Timeouts: 3 per half. T = timeout, Q = quit.\n");

            Console.WriteLine("OFFENSE:");
            Console.WriteLine(" 1) Inside Run  2) Outside Run  3) Draw  4) Option");
            Console.WriteLine(" 5) Screen Pass 6) Short Pass   7) Long Pass");
            Console.WriteLine("15) Punt        16) Field Goal   T) Timeout   Q) Quit\n");

            Console.WriteLine("DEFENSE:");
            Console.WriteLine(" 1) Normal   2) Run Commit   3) Pass Shell   4) Blitz   5) Intercept   6) Block Kick");
            Console.WriteLine(" T) Timeout  Q) Quit\n");
        }

        void PrintSituation()
        {
            Console.WriteLine();
            Console.WriteLine($"Q{quarter}  {secLeft/60:00}:{secLeft%60:00}   {(homeOnOffense?home.Name:away.Name)} BALL");
            Console.WriteLine($"DOWN {down} & {toGo}  ON {SpotString()}");
            Console.WriteLine($"SCORE  {home.Name} {home.Score} — {away.Name} {away.Score}");
        }

        string SpotString()
        {
            bool offHome = homeOnOffense;
            int yardFromOff = offHome ? 100 - ball : ball;
            string side = yardFromOff >= 50 ? "OPP" : "OWN";
            int shown = yardFromOff >= 50 ? 100 - yardFromOff : yardFromOff;
            return $"{side} {shown}";
        }

        enum OffPlay { InsideRun=1, OutsideRun=2, Draw=3, Option=4, Screen=5, ShortPass=6, LongPass=7, Punt=15, FieldGoal=16, Timeout=99, Quit=98 }
        enum DefPlay { Normal=1, RunCommit=2, PassShell=3, Blitz=4, Intercept=5, BlockKick=6, Timeout=99, Quit=98 }

        OffPlay AskOffense()
        {
            while (true)
            {
                Console.Write("OFFENSE (1-7,15,16 / T timeout / Q quit): ");
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") return OffPlay.Quit;
                if (s == "T") return OffPlay.Timeout;
                if (int.TryParse(s, out int n) && (n is >=1 and <=7 or 15 or 16))
                    return (OffPlay)n;
                Console.WriteLine("Illegal entry—try again.");
            }
        }

        DefPlay AskDefense()
        {
            while (true)
            {
                Console.Write("DEFENSE (1-6 / T timeout / Q quit): ");
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") return DefPlay.Quit;
                if (s == "T") return DefPlay.Timeout;
                if (int.TryParse(s, out int n) && (n is >=1 and <=6))
                    return (DefPlay)n;
                Console.WriteLine("Illegal entry—try again.");
            }
        }

        // ===== CPU AI =====
        OffPlay CpuOffenseCall()
        {
            int yardsToTD = homeOnOffense ? ball : 100 - ball;
            int r = rng.Next(100);

            if (down == 3 && toGo >= 7) return r < 55 ? OffPlay.ShortPass : OffPlay.LongPass;
            if (down == 2 && toGo >= 8) return r < 50 ? OffPlay.ShortPass : OffPlay.Draw;

            if (down == 4)
            {
                int losFromOpp = homeOnOffense ? ball : 100 - ball;
                int fg = losFromOpp + 17;
                if (toGo <= 2 && losFromOpp <= 45 && r < 35) return OffPlay.InsideRun;
                if (fg <= 52 && r < 70) return OffPlay.FieldGoal;
                return OffPlay.Punt;
            }

            if (yardsToTD < 25)
                return r < 40 ? OffPlay.InsideRun : r < 65 ? OffPlay.ShortPass : OffPlay.Option;

            return r < 35 ? OffPlay.InsideRun :
                   r < 55 ? OffPlay.OutsideRun :
                   r < 70 ? OffPlay.Draw :
                   r < 87 ? OffPlay.ShortPass :
                            OffPlay.Screen;
        }

        DefPlay CpuDefenseCall(OffPlay offense)
        {
            int r = rng.Next(100);
            return offense switch
            {
                OffPlay.LongPass => r<50 ? DefPlay.PassShell : r<75 ? DefPlay.Intercept : DefPlay.Normal,
                OffPlay.ShortPass or OffPlay.Screen => r<40 ? DefPlay.PassShell : r<60 ? DefPlay.Blitz : DefPlay.Normal,
                OffPlay.InsideRun or OffPlay.OutsideRun or OffPlay.Draw or OffPlay.Option => r<45 ? DefPlay.RunCommit : r<65 ? DefPlay.Blitz : DefPlay.Normal,
                OffPlay.Punt or OffPlay.FieldGoal => r<55 ? DefPlay.BlockKick : DefPlay.Normal,
                _ => DefPlay.Normal
            };
        }

        // ===== Play Resolution =====
        void ResolvePlay(OffPlay off, DefPlay def, bool offenseIsHome)
        {
            int playTime = rng.Next(18, 36);
            int yards = 0;
            bool incomplete = false, intercepted = false, fumble = false, sack = false;

            int bias = 0;
            if (def == DefPlay.Blitz && (off == OffPlay.ShortPass || off == OffPlay.LongPass)) bias -= 6;
            if (def == DefPlay.RunCommit && (off == OffPlay.InsideRun || off == OffPlay.OutsideRun || off == OffPlay.Draw || off == OffPlay.Option)) bias -= 4;
            if (def == DefPlay.PassShell && (off == OffPlay.LongPass || off == OffPlay.ShortPass || off == OffPlay.Screen)) bias -= 3;
            if (def == DefPlay.Intercept && (off == OffPlay.LongPass || off == OffPlay.ShortPass)) bias -= 5;

            switch (off)
            {
                case OffPlay.InsideRun:
                    yards = Clamp(Normal(4 + bias, 10), -4, 18);
                    fumble = rng.Next(100) < 2;
                    home.AddRush(offenseIsHome, yards);
                    Console.WriteLine(yards >= 0 ? $"INSIDE RUN — GAIN OF {yards}." : $"INSIDE RUN — LOSS OF {-yards}.");
                    break;

                case OffPlay.OutsideRun:
                    yards = Clamp(Normal(5 + bias, 12), -6, 25);
                    fumble = rng.Next(100) < 2;
                    home.AddRush(offenseIsHome, yards);
                    Console.WriteLine(yards >= 0 ? $"OUTSIDE RUN — GAIN OF {yards}." : $"OUTSIDE RUN — LOSS OF {-yards}.");
                    break;

                case OffPlay.Draw:
                    yards = Clamp(Normal(6 + bias, 14), -6, 28);
                    fumble = rng.Next(100) < 2;
                    home.AddRush(offenseIsHome, yards);
                    Console.WriteLine(yards >= 0 ? $"DRAW — GAIN OF {yards}." : $"DRAW — LOSS OF {-yards}.");
                    break;

                case OffPlay.Option:
                    yards = Clamp(Normal(6 + bias, 14), -8, 30);
                    fumble = rng.Next(100) < 3;
                    home.AddRush(offenseIsHome, yards);
                    Console.WriteLine(yards >= 0 ? $"OPTION — GAIN OF {yards}." : $"OPTION — LOSS OF {-yards}.");
                    break;

                case OffPlay.Screen:
                    if (rng.Next(100) < (def == DefPlay.Intercept ? 20 : 12)) { incomplete = true; Console.WriteLine("SCREEN PASS INCOMPLETE."); playTime = rng.Next(6, 11); break; }
                    yards = Clamp(Normal(7 + bias, 16), -8, 35);
                    home.AddPass(offenseIsHome, yards, true);
                    Console.WriteLine($"SCREEN PASS COMPLETE — GAIN OF {Math.Max(0,yards)}.");
                    break;

                case OffPlay.ShortPass:
                    if (def == DefPlay.Blitz && rng.Next(100) < 18) { sack = true; yards = -Clamp(Normal(5, 7), 2, 15); playTime = rng.Next(8, 14); }
                    else if (rng.Next(100) < (def == DefPlay.PassShell ? 25 : 18)) { incomplete = true; playTime = rng.Next(6, 11); }
                    else yards = Clamp(Normal(10 + bias, 18), -8, 40);

                    if (sack) { Console.WriteLine($"QUARTERBACK SACKED — LOSS OF {-yards}."); home.AddPass(offenseIsHome, yards, false); break; }
                    if (incomplete) { Console.WriteLine("PASS INCOMPLETE."); home.AddPass(offenseIsHome, 0, false); break; }

                    intercepted = rng.Next(100) < (def == DefPlay.Intercept ? 9 : 3);
                    if (intercepted) { Console.WriteLine("PASS INTERCEPTED!"); TurnoverAtSpot(offenseIsHome); return; }

                    home.AddPass(offenseIsHome, yards, true);
                    Console.WriteLine($"PASS COMPLETE — GAIN OF {Math.Max(0,yards)}.");
                    break;

                case OffPlay.LongPass:
                    if (def == DefPlay.Blitz && rng.Next(100) < 15) { sack = true; yards = -Clamp(Normal(7, 9), 3, 18); playTime = rng.Next(9, 15); }
                    else if (rng.Next(100) < (def == DefPlay.PassShell ? 40 : 30)) { incomplete = true; playTime = rng.Next(6, 10); }
                    else yards = Clamp(Normal(20 + bias, 26), -10, 60);

                    if (sack) { Console.WriteLine($"QUARTERBACK SACKED — LOSS OF {-yards}."); home.AddPass(offenseIsHome, yards, false); break; }
                    if (incomplete) { Console.WriteLine("PASS INCOMPLETE."); home.AddPass(offenseIsHome, 0, false); break; }

                    intercepted = rng.Next(100) < (def == DefPlay.Intercept ? 18 : 7);
                    if (intercepted) { Console.WriteLine("DEEP PASS INTERCEPTED!"); TurnoverAtSpot(offenseIsHome); return; }

                    home.AddPass(offenseIsHome, yards, true);
                    Console.WriteLine($"LONG PASS COMPLETE — GAIN OF {Math.Max(0,yards)}.");
                    break;

                default: return;
            }

            AdvanceClock(playTime);

            if (fumble && rng.Next(100) < 45)
            {
                Console.WriteLine("FUMBLE! DEFENSE RECOVERS.");
                TurnoverAtSpot(offenseIsHome);
                return;
            }

            int dir = offenseIsHome ? 1 : -1;
            ball = Math.Clamp(ball + dir * yards, 0, 100);

            if ((offenseIsHome && ball >= 100) || (!offenseIsHome && ball <= 0))
            {
                Touchdown(offenseIsHome);
                return;
            }

            NextDown(yards);
        }

        // ===== Specials =====
        void DoPunt(bool isHome)
        {
            AdvanceClock(rng.Next(8, 14));
            if (rng.Next(100) < 6)
            {
                Console.WriteLine("PUNT BLOCKED! BALL DEAD AT LINE.");
                ChangePossession();
                return;
            }

            int distance = Clamp(Normal(42, 18), 20, 65);
            int returnYds = Clamp(Normal(8, 12), 0, 40);
            Console.WriteLine($"PUNT {distance}, RETURN {returnYds}.");

            int dir = isHome ? 1 : -1;
            ball = Math.Clamp(ball + dir * (distance - returnYds), 0, 100);
            ChangePossession(true);
        }

        void TryFieldGoal(bool isHome)
        {
            int losFromOppGoal = isHome ? ball : 100 - ball;
            int kick = losFromOppGoal + 17;
            int basePct = kick <= 30 ? 92 : kick <= 40 ? 76 : kick <= 50 ? 48 : 12;
            bool good = rng.Next(100) < (basePct - 3);
            AdvanceClock(rng.Next(15, 25));

            if (good)
            {
                Console.WriteLine($"FIELD GOAL IS GOOD FROM {kick}!");
                (isHome ? home : away).Score += 3;
                AfterScoreKickChoice(scoringHome: isHome);
            }
            else
            {
                Console.WriteLine($"FIELD GOAL NO GOOD ({kick}).");
                ChangePossession();
            }
        }

        void Touchdown(bool isHome)
        {
            Console.WriteLine("TOUCHDOWN!");
            (isHome ? home : away).Score += 6;

            if (isHome) DoPostTDChoiceHuman();
            else        DoPostTDChoiceCpu();

            // kickoff (onside option handled inside AfterScoreKickChoice)
        }

        void DoPostTDChoiceHuman()
        {
            while (true)
            {
                Console.Write("AFTER TD: 1) Kick XP  2) Go for Two  -> ");
                var s = (Console.ReadLine() ?? "").Trim();
                if (s == "1")
                {
                    TryExtraPoint(true);
                    break;
                }
                if (s == "2")
                {
                    AttemptTwoPoint(isHome:true);
                    break;
                }
            }
            AfterScoreKickChoice(scoringHome: true);
        }

        void DoPostTDChoiceCpu()
        {
            // CPU logic: when trailing by 2 (late) or to reach even deficit, etc.
            int diff = (away.Score - home.Score);
            bool goForTwo =
                (quarter == 4 && secLeft < 3 * 60 && diff == -2) ||
                (quarter == 4 && secLeft < 2 * 60 && Math.Abs(diff) == 1) ||
                rng.Next(100) < 12; // occasional surprise

            if (goForTwo) AttemptTwoPoint(isHome:false);
            else          TryExtraPoint(false);

            AfterScoreKickChoice(scoringHome: false);
        }

        void TryExtraPoint(bool isHome)
        {
            bool good = rng.Next(100) < 94;
            if (good) { Console.WriteLine("EXTRA POINT GOOD."); (isHome ? home : away).Score += 1; }
            else      { Console.WriteLine("EXTRA POINT MISSED."); }
        }

        void AttemptTwoPoint(bool isHome)
        {
            if (isHome)
            {
                Console.Write("TWO-POINT: choose R)un or P)ass: ");
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                bool run = s != "P";
                bool good = RunTwoPoint(run, isCpu:false);
                Console.WriteLine(good ? "TWO-POINT CONVERSION IS GOOD!" : "TWO-POINT TRY FAILED.");
                if (good) (isHome ? home : away).Score += 2;
            }
            else
            {
                bool run = rng.Next(100) < 55; // CPU slight run bias at 2
                bool good = RunTwoPoint(run, isCpu:true);
                Console.WriteLine(good ? "CPU 2-POINT IS GOOD." : "CPU 2-POINT FAILED.");
                if (good) (isHome ? home : away).Score += 2;
            }
        }

        bool RunTwoPoint(bool run, bool isCpu)
        {
            // ball at the 2-yard line. Success rates ~45–50%, nudged by random & “defense”.
            int basePct = run ? 48 : 45;
            // tiny situational wobble
            basePct += rng.Next(-6, 7);
            return rng.Next(100) < basePct;
        }

        void AfterScoreKickChoice(bool scoringHome)
        {
            bool onside = false;
            if (scoringHome)
            {
                Console.Write("ONSIDE KICK? (Y/N): ");
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                onside = s == "Y" || s == "YES";
            }
            else
            {
                // CPU may onside late when trailing by one score
                int def = home.Score - away.Score;
                onside = (quarter == 4 && secLeft < 3 * 60 && def > 0 && def <= 8 && rng.Next(100) < 60);
            }

            if (onside) DoOnsideKick(scoringHome);
            else        StartKickoff(kickingTeamIsHome: scoringHome);
        }

        void DoOnsideKick(bool kickingHome)
        {
            Console.WriteLine("ONSIDE KICK ATTEMPT!");
            bool recoveredByKicker = rng.Next(100) < 12; // success rate
            if (recoveredByKicker)
            {
                Console.WriteLine("ONSIDE KICK RECOVERED BY KICKING TEAM!");
                // Possession stays with kicker near midfield
                ball = kickingHome ? 44 : 56;
                homeOnOffense = kickingHome;
                down = 1; toGo = 10;
            }
            else
            {
                Console.WriteLine("ONSIDE KICK FAILED.");
                // Receiving team takes over already in + territory
                ball = kickingHome ? 56 : 44;
                homeOnOffense = !kickingHome;
                down = 1; toGo = 10;
            }
        }

        void StartKickoff(bool kickingTeamIsHome)
        {
            int kickDist = Clamp(Normal(62, 10), 50, 75);
            int returnYds = Clamp(Normal(20, 12), 0, 45);
            Console.WriteLine($"KICKOFF: {kickDist} YARDS, RETURN {returnYds}.");

            if (kickingTeamIsHome)
            {
                ball = Clamp(kickDist - returnYds, 0, 100);
                homeOnOffense = false;
            }
            else
            {
                ball = Clamp(100 - (kickDist - returnYds), 0, 100);
                homeOnOffense = true;
            }
            down = 1; toGo = 10;
        }

        void StartHalfKickoff(bool kickingTeamIsHome)
        {
            StartKickoff(kickingTeamIsHome);
            quarter = 1; secLeft = REG_QTR_SEC; homeTO = awayTO = 3;
        }

        // ===== Game state helpers =====
        void NextDown(int gained)
        {
            toGo -= gained;
            if (toGo <= 0)
            {
                Console.WriteLine("FIRST DOWN!");
                down = 1; toGo = 10;
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

        void TurnoverAtSpot(bool offenseWasHome)
        {
            ChangePossession(true);
        }

        void ChangePossession(bool resetDownDist = true)
        {
            homeOnOffense = !homeOnOffense;
            if (resetDownDist) { down = 1; toGo = 10; }
        }

        void UseTimeout(bool isHome)
        {
            ref int tos = ref (isHome ? ref homeTO : ref awayTO);
            if (tos > 0)
            {
                tos--;
                secLeft = Math.Min(secLeft + 40, (quarter <= 4 ? REG_QTR_SEC : OT_QTR_SEC));
                Console.WriteLine("TIMEOUT.");
            }
            else Console.WriteLine("NO TIMEOUTS LEFT.");
        }

        bool AdvancePeriod()
        {
            quarter++;
            if (quarter == 3)
            {
                Console.WriteLine("\n— HALFTIME —\n");
                homeTO = awayTO = 3;
                // Halftime kickoff: flip who kicked first
                StartKickoff(kickingTeamIsHome: homeOnOffense); // reverse possession
            }

            if (quarter <= 4) { secLeft = REG_QTR_SEC; return true; }

            // End of regulation — overtime if tied
            if (home.Score == away.Score)
            {
                Console.WriteLine("\n— OVERTIME — Sudden Death (10:00) —");
                secLeft = OT_QTR_SEC;
                homeTO = awayTO = 2;
                Console.WriteLine("OVERTIME COIN TOSS...");
                bool homeReceives = rng.Next(2) == 0;
                StartKickoff(kickingTeamIsHome: !homeReceives);
                quarter = 5; // mark OT
                // sudden death handled by main loop: first score ends game
                return true;
            }

            return false; // game ends
        }

        void AdvanceClock(int seconds)
        {
            secLeft -= Math.Max(0, seconds);
            if (secLeft < 0) secLeft = 0;
        }

        // ===== Utils / Stats =====
        int Normal(int mean, int spread)
        {
            int a = rng.Next(-spread, spread + 1);
            int b = rng.Next(-spread, spread + 1);
            return mean + (a + b) / 2;
        }
        static int Clamp(int v, int min, int max) => Math.Min(max, Math.Max(min, v));

        string Ask(string prompt, string fallback)
        {
            Console.Write(prompt);
            var s = Console.ReadLine();
            return string.IsNullOrWhiteSpace(s) ? fallback : s.Trim();
        }

        void PrintStats()
        {
            Console.WriteLine("\nSTATS (light):");
            Console.WriteLine($"{home.Name} — RushYds {home.RushYds}  PassYds {home.PassYds}  Plays {home.Plays}");
            Console.WriteLine($"{away.Name} — RushYds {away.RushYds}  PassYds {away.PassYds}  Plays {away.Plays}");
        }
    }

    internal sealed class Team
    {
        public string Name { get; }
        public int Score { get; set; }
        public int RushYds { get; private set; }
        public int PassYds { get; private set; }
        public int Plays { get; private set; }
        public Team(string n) { Name = n; }
        public void AddRush(bool isHomeOffense, int yds) { RushYds += yds; Plays++; }
        public void AddPass(bool isHomeOffense, int yds, bool complete) { PassYds += Math.Max(0, yds); Plays++; }
    }
}
