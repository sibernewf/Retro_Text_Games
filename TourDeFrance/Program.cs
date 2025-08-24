using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TourDeFrance1986
{
    internal static class Program
    {
        // --- RNG ---
        static readonly Random Rng = new Random();

        // --- Data arrays (mirroring BASIC) ---
        static string[] PLACE = new string[23 + 1];     // 1..22 used (0 is Reims)
        static int[] TYPE = new int[23 + 1];
        static int[] DIST = new int[23 + 1];
        static string[] TYPESTR = new string[5 + 1];    // 1..5
        static int[] PR = new int[10 + 1];              // road hazards cumulative prob
        static int PRT = 0;
        static int[] PM = new int[8 + 1];               // mechanical cumulative
        static int PMT = 0;
        static int[] PP = new int[14 + 1];              // physical cumulative
        static int PPT = 0;

        // --- Race state ---
        static int DAY = 0;              // July day counter (1..22)
        static int TDEP;                 // departure minutes past 9:00 (100..159 meaning 9:xx)
        static double PT = 1800;         // timing loop baseline (we derive if user doesn't have one)
        static int PFRQ = 10;            // how often player pedals (1..10)
        static int PD = 0;               // # of human pedaling sessions used
        static double FIT = 0.57;        // fitness factor
        static int WK = 8;               // training weeks
        static string LF = "A";          // left pedal key str
        static string RT = "L";          // right pedal key str
        static double PTM = 1.0;              // pedaling scale for sprint vs normal
        static int RPS = 130;            // baseline RPM seed for computer pedaling
        static double RPM = 80;          // result after pedaling
        static double GR = 52.0 / 17.0;  // base gear ratio
        static double GDGR = 1.0;        // gear/day factor penalty
        static string LastKey = "";      // to track alternation

        // hazards
        static double TDL = 0;           // time delay (hours)
        static int CR = 0;               // crash occurred flag
        static int PPX = 0;              // temporary PP entry push

        // Stage/overall timing
        static double[] TTM = new double[6 + 1]; // stage times 1..6
        static double[] TTR = new double[6 + 1]; // cumulative times
        static int[] WSG = new int[6 + 1];       // stage wins

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Tour de France Bicycle Race (1986) – C# Port";

            // Title / intro
            Center("Tour de France Bicycle Race");
            Center("(c) David H. Ahl, 1986");
            Center("Press any key to continue.");
            Console.ReadKey(true);

            Console.Clear();
            ShowScenario();

            // Init arrays / data
            PLACE[0] = "Reims";
            ReadEventProbabilities();
            ReadRoutes();
            ReadTypeWords();

            Center("Press any key to continue.");
            Console.ReadKey(true);
            Console.Clear();

            // --- Pre-race data ---
            string hasPT = AskYesNo("Do you have the timing loop value from a previous run (Y/N)? ");
            if (hasPT == "Y")
            {
                PT = AskDouble("Please enter the value", 1, 100000);
                if (!(PT > 500 && PT < 5000))
                {
                    PT = AskDouble("That doesn't sound right. Please enter it again", 1, 100000);
                }
            }
            else
            {
                CalculateTimingLoop();
            }

            Console.WriteLine();
            Console.WriteLine("About your physical fitness:");
            Console.WriteLine("  (1) in fantastic health");
            Console.WriteLine("  (2) excellent shape");
            Console.WriteLine("  (3) quite good");
            Console.WriteLine("  (4) okay");
            Console.WriteLine("  (5) poor");
            int fitLevel = AskInt("Please enter a number between 1 and 5", 1, 5);
            FIT = 0.57 - 0.04 * fitLevel;

            Console.WriteLine("How many weeks do you intend to take off to practice and prepare?");
            WK = AskInt("Weeks (max 12)", 0, 12);
            if (WK <= 5)
            {
                Console.WriteLine("You must be joking. You'll need at least six weeks if you want to be a real contender.");
            }
            FIT = FIT - (12 - WK) * 0.05;

            // Assign pedal keys
            Console.WriteLine();
            Console.WriteLine("To pedal your computer bike, you'll strike two keys alternately.");
            Console.Write("Which key do you want for your left pedal? ");
            LF = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrEmpty(LF)) LF = "A";
            RT = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrEmpty(RT)) RT = "L";

            Console.WriteLine();
            Console.WriteLine("Thank you. Let's go out for a practice run.");
            PFRQ = 10; PTM = 1.0;
            Pedal(); PD = 0; Console.WriteLine();

            Console.WriteLine($"In an upper-middle gear ratio (52/17), {RPM} rpm ≈ {Math.Round(0.3956 * RPM)} kph.");
            if (RPM <= 60)
            {
                Console.WriteLine("That speed is barely competitive.");
                Console.WriteLine($"Next time you play, try a timing-loop value of {Math.Round(80 * PT / Math.Max(1, RPM))},");
                Console.WriteLine("but for now you may want to have the computer do most of the pedaling.");
            }

            Console.WriteLine();
            Console.WriteLine("How much do you want to pedal your bike (on the keyboard)?");
            Console.WriteLine("1 = hardly at all, 3 = occasionally, 7 = frequently, 10 = every opportunity");
            PFRQ = AskInt("Please enter a number between 1 and 10", 1, 10);

            // --- Race loop (22 days) ---
            int dayMax = 22;
            while (true)
            {
                DAY++;
                Console.WriteLine();
                Console.WriteLine($"Date: July {DAY}  You are at {PLACE[DAY - 1]}.");
                if (TYPE[DAY] >= 5)
                {
                    Console.WriteLine("Today, thank goodness, is a rest and recuperation day.");
                    if (DAY < dayMax) { continue; } else break;
                }

                Console.WriteLine($"Your destination is {PLACE[DAY]}, {DIST[DAY]} km from here.");
                Console.WriteLine($"Type of racing this stage: {TYPESTR[TYPE[DAY]]}");

                // gear change?
                if (TYPE[DAY] != TYPE[DAY - 1])
                {
                    // force ask
                    ChooseGear();
                }
                else
                {
                    var diff = AskYesNo("Do you want a different basic gear range than yesterday (Y/N)? ");
                    if (diff == "Y") ChooseGear();
                }

                // Start of stage
                TDEP = 100 + Rng.Next(0, 60); // 9:xx
                Console.Write("Your departure time is scheduled at 9:");
                Console.WriteLine($"{(TDEP % 100):00}");
                ShortPause();
                PTM = 1.0; RPS = 130;
                Pedal();
                double KPH = RPM * 0.1292706 * GR * GDGR;
                Console.WriteLine($"{KPH:0.0} kph.\n");

                TDL = 0;
                RoadHazards();

                CR = 0;
                Console.WriteLine();
                MechanicalBreakdowns();

                // Penalty for too high gear in mountains
                if (TYPE[DAY] == 3 && GR > 2.7)
                {
                    PP[1] += 10; PPT += 10; PPX = 1;
                }

                Console.WriteLine();
                PhysicalProblems();

                if (PPX == 1)
                {
                    PPX = 0; PP[1] -= 10; PPT -= 10;
                }

                Console.WriteLine();
                Console.Write("Time for a quick breather. You have about ");
                Console.WriteLine($"{(int)(20 + 20 * Rng.NextDouble())} km to go.");
                Console.WriteLine("Press any key when you're ready to go.");
                Console.ReadKey(true);
                Console.WriteLine("Okay, on the road again…");

                // Sprint section
                ShortPause();
                Console.WriteLine();
                Console.WriteLine("You're coming up on 10 km from the end.");
                Console.WriteLine("During the countdown (in 0.1-km increments) you can press any key to start your sprint.");

                ShortPause();
                double DSP = 10.0;
                bool startedSprint = false;
                for (double d = 10.0; d >= 0.0; d -= 0.1)
                {
                    Console.Write($"\r{d:00.0} km remaining      ");
                    if (Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                        DSP = d;
                        startedSprint = true;
                        break;
                    }
                    System.Threading.Thread.Sleep(35);
                }
                Console.WriteLine();
                double KSR;
                if (!startedSprint)
                {
                    // auto
                    KSR = 1; // i.e., no special sprint speed
                }
                else
                {
                    PTM = Math.Max(0.2, DSP / 2.0);
                    RPS = 140;
                    Pedal();
                    KSR = RPM * 0.396;
                    Console.WriteLine($"{KSR:0.0} kph.\n");
                }

                double TMSD = (DSP > 0 && startedSprint) ? DSP / Math.Max(1, KSR) : 0;
                // ride the remainder at KPH
                double TMRD = (DIST[DAY] - (startedSprint ? DSP : 0)) / Math.Max(1, KPH);
                TTM[1] = TMSD + TMRD + TDL; // hours

                if (DSP > 3 && startedSprint)
                    Console.WriteLine("Puff…puff…puff. That was a L-O-N-G sprint!");

                // Calculate top six riders
                Console.WriteLine();
                Console.WriteLine("Race summary (total times in hours):");
                Console.WriteLine(" Rider  1 (you)    2       3       4       5       6");
                Console.Write("Stage   ");
                Console.Write($"{TTM[1],8:0.00}");

                double TTS = TTM[1];
                int WS = 1;
                double GQ = (TYPE[DAY] == 3) ? 0.3 : 0.4;

                for (int i = 2; i <= 6; i++)
                {
                    RPM = 70 + 20 * Rng.NextDouble();
                    TTM[i] = DIST[DAY] / (GQ * RPM) + 1.4 * Rng.NextDouble();
                    Console.Write($"{TTM[i],8:0.00}");
                    if (TTM[i] < TTS) { TTS = TTM[i]; WS = i; }
                }
                Console.WriteLine();

                Console.Write("Total  ");
                double TTT = 1000; int WT = 0;
                for (int i = 1; i <= 6; i++)
                {
                    TTR[i] += TTM[i];
                    Console.Write($"{TTR[i],8:0.00}");
                    if (TTR[i] < TTT) { TTT = TTR[i]; WT = i; }
                }
                Console.WriteLine("\n");

                Console.WriteLine($" Stage winner : Rider {WS}   Overall leader : Rider {WT}");
                WSG[WS]++;

                if (DAY >= dayMax) break;
            }

            // End-of-race summary
            Console.WriteLine();
            Console.WriteLine("The Tour de France has ended!\n");
            int most = 0; int mostIdx = 1;
            for (int i = 1; i <= 6; i++)
            {
                if (WSG[i] > most) { most = WSG[i]; mostIdx = i; }
            }
            Console.Write($"Winner of the most stages ({most}) was Rider {mostIdx}");
            Console.WriteLine(mostIdx == 1 ? "  That's YOU!" : "");

            double best = 1000; int WToverall = 0;
            for (int i = 1; i <= 6; i++)
            {
                if (TTR[i] < best) { best = TTR[i]; WToverall = i; }
            }
            Console.Write($"Overall winner by elapsed time was Rider {WToverall}");
            Console.WriteLine(WToverall == 1 ? "  That's YOU!" : "");

            best = 1000; WToverall = 0;
            for (int i = 1; i <= 6; i++)
            {
                var pts = TTR[i] - 2 * WSG[i];
                if (pts < best) { best = pts; WToverall = i; }
            }
            Console.Write($"Overall points winner (time and stages) was Rider {WToverall}");
            Console.WriteLine(WToverall == 1 ? "  That's YOU!" : "");

            Console.WriteLine();
            var again = AskYesNo("Would you like to ride again (Y/N)? ");
            if (again == "Y")
            {
                // Reset minimal persistent state
                Array.Clear(TTR, 0, TTR.Length);
                Array.Clear(WSG, 0, WSG.Length);
                DAY = 0;
                Console.Clear();
                Main();
                return;
            }
            Console.WriteLine("Bye for now.");
        }

        // --- Subroutines ---

        static void ShowScenario()
        {
            Center("Tour de France Bicycle Race");
            Console.WriteLine();
            Console.WriteLine("You are a bicycle racer in the 22-day Tour de France.");
            Console.WriteLine("Win by having the lowest overall time and try to win as many stages as possible.");
            Console.WriteLine("You’ll pedal by alternating two keys (or let the computer assist).");
            Console.WriteLine("Hazards include weather, mechanical issues, road conditions, and physical problems.");
            Console.WriteLine("At the end of each stage, you may sprint to the finish; timing is critical.");
            Console.WriteLine();
        }

        static void ReadEventProbabilities()
        {
            // Road hazards (10 items), cumulative
            int[] road = { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 };
            PRT = 0;
            for (int i = 1; i <= 10; i++)
            {
                PRT += road[i - 1];
                PR[i] = PRT;
            }

            // Mechanical (8 items), cumulative
            int[] mech = { 5, 5, 5, 5, 5, 5, 10, 5 };
            PMT = 0;
            for (int i = 1; i <= 8; i++)
            {
                PMT += mech[i - 1];
                PM[i] = PMT;
            }

            // Physical (14 items), cumulative
            int[] phys = { 8, 5, 5, 5, 5, 5, 5, 5, 5, 8, 5, 5, 5, 3 };
            PPT = 0;
            for (int i = 1; i <= 14; i++)
            {
                PPT += phys[i - 1];
                PP[i] = PPT;
            }
        }

        static void ReadRoutes()
        {
            // 0 is Reims
            PLACE[0] = "Reims";

            // 1..22 stages
            var rows = new (string Place, int Type, int Dist)[]
            {
                ("Lille", 1, 213),
                ("Arras", 4, 52),
                ("Caen", 1, 308),
                ("Le Mans", 1, 172),
                ("Nantes", 1, 192),
                ("Bordeaux", 1, 338),
                ("Biarritz/Bayonne", 1, 184),
                ("Lourdes", 3, 168),
                ("Lourdes", 5, 0),
                ("Toulouse", 2, 172),
                ("Rodez", 2, 176),
                ("Avignon", 2, 294),
                ("Grenoble", 2, 228),
                ("l'Alpe-d'Huez", 4, 39),
                ("Lansleburg", 3, 225),
                ("Martigny, Switzerland", 3, 230),
                ("Annecy", 3, 218),
                ("Annecy", 5, 0),
                ("Lyon", 2, 182),
                ("Dijon", 1, 212),
                ("Fountainbleau", 1, 259),
                ("Paris", 1, 210),
            };

            for (int i = 1; i <= 22; i++)
            {
                PLACE[i] = rows[i - 1].Place;
                TYPE[i] = rows[i - 1].Type;
                DIST[i] = rows[i - 1].Dist;
            }
        }

        static void ReadTypeWords()
        {
            TYPESTR[1] = "Mostly flat with small hills.";
            TYPESTR[2] = "Hills, gorges, steep slopes.";
            TYPESTR[3] = "Mountains.";
            TYPESTR[4] = "Time trial against the clock.";
            TYPESTR[5] = "Rest.";
        }

        static void ChooseGear()
        {
            Console.WriteLine("Naturally you will shift during the day, but pick a basic gear range.");
            int ring;
            while (true)
            {
                ring = AskInt("First the ring (40 or 52)", 40, 52);
                if (ring == 40 || ring == 52) break;
                Console.WriteLine("You don't have that ring.");
            }
            int cog;
            while (true)
            {
                cog = AskInt("Which cog (13,15,17,19,21,23,25)?", 13, 25);
                if (new[] { 13, 15, 17, 19, 21, 23, 25 }.Contains(cog)) break;
                Console.WriteLine("Sorry, you don't have that cog.");
            }

            // disallow extreme skew
            if ((cog == 13 && ring == 40) || (cog == 25 && ring == 52))
            {
                Console.WriteLine("The chain line will be badly skewed with that combination. Try again.");
                ChooseGear();
                return;
            }

            GR = (double)ring / cog;

            if (TYPE[DAY] != 4) // not time-trial
            {
                string x = null;
                if (GR > 3.2) x = "high";
                else if (GR < 1.8) x = "low";

                if (x != null)
                {
                    var change = AskYesNo($"That ratio sounds very {x}. Do you want to change it (Y/N)? ");
                    if (change == "Y")
                    {
                        ChooseGear();
                        return;
                    }
                }

                if (TYPE[DAY] == 3 && GR > 2.3)
                {
                    var stick = AskYesNo("For mountainous terrain, that's rather high. Do you want to stick with it (Y/N)? ");
                    if (stick == "N")
                    {
                        ChooseGear();
                        return;
                    }
                    GDGR = 1.3 - 0.19 * GR; // penalty
                }
                else
                {
                    if (GR > 3) GDGR = 1.35 - 0.14 * GR; else GDGR = 1.0;
                }
            }
        }

        static void RoadHazards()
        {
            int RN = Rng.Next(PRT); // 0..PRT-1
            int idx = 1;
            for (; idx <= 10; idx++)
            {
                if (RN <= PR[idx]) break;
            }
            switch (idx)
            {
                case 1:
                    Console.WriteLine("Mostly gravel roads this stage. You'll have to slow down.");
                    TDL += 0.8; break;
                case 2:
                    Console.WriteLine("Very bumpy roads will slow you down.");
                    TDL += 0.5; break;
                case 3:
                    Console.WriteLine("Hot weather caused oily slippery roads.");
                    TDL += 0.3; break;
                case 4:
                    Console.WriteLine("The wind is at your back — fast ride!");
                    TDL += -0.3; break;
                case 5:
                    Console.WriteLine("Heading straight into the wind. Tough going.");
                    TDL += 0.5; break;
                case 6:
                    Console.WriteLine("Gusty sidewind creating balance problems.");
                    TDL += 0.3; break;
                case 7:
                    Console.WriteLine("Dreary day: drizzle, fog, clammy chill.");
                    TDL += 0.2; break;
                case 8:
                    Console.WriteLine("Horrible icy rain. Shoes soaked. Few spectators.");
                    TDL += 0.5; break;
                case 9:
                    Console.WriteLine("Mud and puddles — sliding and skidding all over.");
                    TDL += 0.4; break;
                default:
                    Console.WriteLine("Crisp, clear day in the French countryside.");
                    break;
            }
        }

        static void MechanicalBreakdowns()
        {
            int RN = Rng.Next(PMT);
            int idx = 1;
            for (; idx <= 8; idx++)
            {
                if (RN <= PM[idx]) break;
            }

            switch (idx)
            {
                case 1:
                {
                    var y = AskYesNo("You have a broken spoke. Want to fix it now (Y/N)? ");
                    TDL += (y == "Y") ? 0.1 : 0.15;
                    break;
                }
                case 2:
                    Console.WriteLine("You got a flat tire. You’ll have to change it now.");
                    TDL += 0.1; break;
                case 3:
                {
                    Console.WriteLine("Brakes tend to lock on hard application.");
                    var y = AskYesNo("Nurse along or fix now? Fix now (Y/N)? ");
                    TDL += (y == "Y") ? 0.2 : 0.4;
                    break;
                }
                case 4:
                {
                    Console.WriteLine("Missing shifts to your 19 cog; teeth may be worn.");
                    var y = AskYesNo("Fix it now (Y/N)? ");
                    TDL += (y == "Y") ? 0.2 : 0.4;
                    break;
                }
                case 5:
                {
                    Console.WriteLine("Toe clip bent on a boulder.");
                    var y = AskYesNo("Bend it out now (Y/N)? ");
                    TDL += (y == "Y") ? 0.1 : 0.2;
                    break;
                }
                case 6:
                    Console.WriteLine("Uh oh! Chain broke. You’ve no choice but to fix it now.");
                    TDL += 0.15; break;
                case 7:
                {
                    Console.WriteLine("WHOOPS! Corner too fast, lost traction — CRASHED!");
                    ShortPause();
                    CR = 1;
                    double r = Rng.NextDouble();
                    if (r < 0.03)
                    {
                        Console.WriteLine("Blood everywhere; ambulance called; rushed to hospital.");
                        ShortPause();
                        Console.WriteLine("Bad news! You dislocated your shoulder and you're out of the race.");
                        EndRaceEarly();
                    }
                    else if (r < 0.5)
                    {
                        Console.WriteLine("Twisted ankle; very painful. No way you’ll drop out; you continue.");
                        TDL += 0.8;
                    }
                    else
                    {
                        Console.WriteLine("Scratched and beaten up but no serious damage. You continue.");
                        TDL += 0.3;
                    }
                    break;
                }
                default:
                    Console.WriteLine("Bicycle ran like a charm today. No problems at all!");
                    break;
            }
        }

        static void PhysicalProblems()
        {
            int RN = Rng.Next(PPT);
            int idx = 1;
            for (; idx <= 14; idx++)
            {
                if (RN <= PP[idx]) break;
            }

            switch (idx)
            {
                case 1:
                {
                    int X = DIST[DAY] / 50;
                    if (X < 2) X = 2;
                    Console.WriteLine($"You're pushing to the absolute limit; after {X} hours you totally collapse.");
                    Console.WriteLine("Medics give you oxygen and warn you against resuming.");
                    // first collapse 80% chance to continue
                    if (Rng.NextDouble() > 0.8)
                    {
                        ShortPause();
                        Console.WriteLine("You heard of another rider dying last year; you withdraw.");
                        EndRaceEarly();
                    }
                    else
                    {
                        ShortPause();
                        Console.WriteLine("Nothing can defeat your competitive spirit; you press on.");
                        TDL += 1.0;
                    }
                    break;
                }
                case 2:
                    Console.WriteLine("Terrible abdominal pain… something you ate? You slow down.");
                    TDL += 0.4; break;
                case 3:
                case 4:
                    Console.WriteLine("Difficulty breathing / lightheaded — you wisely slow your pace.");
                    TDL += (idx == 3 ? 0.3 : 0.3); break;
                case 5:
                    Console.WriteLine("Vision a bit hazy — also slow your pace.");
                    TDL += 0.3; break;
                case 6:
                    Console.WriteLine("Calf muscle like jelly; slow down a bit.");
                    TDL += 0.3; break;
                case 7:
                    Console.WriteLine("Sharp lower back pain — maybe tension. Slow slightly.");
                    TDL += 0.2; break;
                case 8:
                    Console.WriteLine("Shin splints from gearing; back off your pace.");
                    TDL += 0.3; break;
                case 9:
                    Console.WriteLine("Terrible pain in balls of feet; back off a bit.");
                    TDL += 0.3; break;
                case 10:
                    Console.WriteLine("Salt/water imbalance; drink more water & take salt pills.");
                    break;
                case 11:
                    if (TYPE[DAY] == 3)
                    {
                        Console.WriteLine("Altitude in the mountains affecting you; slow slightly.");
                        TDL += 0.3;
                    }
                    else
                    {
                        Console.WriteLine("Feeling good today.");
                    }
                    break;
                case 12:
                    Console.WriteLine("Saddle trouble; cyst risk; add padding & slow a tad.");
                    TDL += 0.15; break;
                case 13:
                    Console.WriteLine("Knees suffering from blistering pace; slow down a bit.");
                    TDL += 0.2; break;
                case 14:
                    Console.WriteLine("Bad leg cramp; take it a bit easier.");
                    TDL += 0.15; break;
                default:
                    if (CR == 1)
                    {
                        PhysicalProblems(); // recursion to avoid "feeling great" right after crash
                    }
                    else
                    {
                        Console.WriteLine("You're feeling fit as a fiddle. No physical problems today.");
                    }
                    break;
            }
        }

        // --- Pedaling emulation ---
        static void Pedal()
        {
            // decide human vs computer pedaling
            bool human =
                (DAY == 8 && PD == 0) ||
                (DAY == 17 && PD == 1) ||
                (PFRQ >= 9.92 * Rng.NextDouble());

            if (!human)
            {
                RPM = (int)((RPS + 40 * Rng.NextDouble()) * FIT);
                Console.WriteLine("Computer is pedaling your bicycle.");
                ShortPause();
                Console.WriteLine("It pedaled…");
            }
            else
            {
                PD++;
                Console.WriteLine("Start pedaling… NOW!");
                // measure alternations for ~2.5 seconds for normal, scaled by PTM for sprint
                double seconds = Math.Max(1.5, 2.5 * (PTM > 0 ? PTM : 1.0));
                var sw = Stopwatch.StartNew();
                int k = 0;
                LastKey = "";
                while (sw.Elapsed.TotalSeconds < seconds)
                {
                    if (Console.KeyAvailable)
                    {
                        var ki = Console.ReadKey(true).KeyChar.ToString();
                        if (!string.IsNullOrEmpty(ki) &&
                            (ki.Equals(LF, StringComparison.OrdinalIgnoreCase) ||
                             ki.Equals(RT, StringComparison.OrdinalIgnoreCase)))
                        {
                            if (!ki.Equals(LastKey, StringComparison.OrdinalIgnoreCase))
                            {
                                k++;
                                LastKey = ki;
                            }
                        }
                    }
                }
                sw.Stop();
                Console.WriteLine("Okay, stop pedaling.");
                // turns per second to RPM (each alternation ~ half rev; scale a bit)
                RPM = Math.Max(20, 0.9 * FIT * (k / Math.Max(0.5, seconds)) * 2.0);
                if (RPM > 95) RPM = 84 + 10 * Rng.NextDouble();
                Console.WriteLine($"You pedaled at a rate of {RPM:0} rpm. Calculating speed…");
                BusyWait(300);
            }
        }

        // --- Helpers ---
        static void EndRaceEarly()
        {
            Console.WriteLine();
            Console.WriteLine("Too bad. That's it for this year, but there's always next year…");
            var again = AskYesNo("Would you like to ride again (Y/N)? ");
            if (again == "Y")
            {
                // reset
                Array.Clear(TTR, 0, TTR.Length);
                Array.Clear(WSG, 0, WSG.Length);
                DAY = 0;
                Console.Clear();
                Main();
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Bye for now.");
                Environment.Exit(0);
            }
        }

        static void CalculateTimingLoop()
        {
            Console.WriteLine("Calculating timing-loop value takes ~5 seconds. Please be patient.");
            var sw = Stopwatch.StartNew();
            long loop = 0;
            while (sw.Elapsed.TotalSeconds < 5.0)
            {
                // nothing heavy — just spin
                loop++;
            }
            sw.Stop();
            // Normalize to a reasonable scale ~ BASIC PT*2
            PT = Math.Max(800, Math.Min(4000, loop / 150000.0));
            Console.WriteLine($"The timing-loop value for your computer is {PT:0}.");
            Console.WriteLine("Please write it down for playing this game in the future.");
        }

        static void Center(string s)
        {
            int w = Math.Max(Console.WindowWidth, 80);
            int pad = Math.Max(0, (w - s.Length) / 2);
            Console.WriteLine(new string(' ', pad) + s);
        }

        static void ShortPause(int ticks = 350)
        {
            // small sleep
            System.Threading.Thread.Sleep(Math.Max(0, ticks));
        }
        static void BusyWait(int loops = 800)
        {
            for (int i = 0; i < loops; i++) { _ = i * i; }
        }

        // --- Input helpers ---
        static string AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim();
                if (string.IsNullOrEmpty(s)) return "Y";
                s = s.Substring(0, 1).ToUpperInvariant();
                if (s == "Y" || s == "N") return s;
                Console.WriteLine("Please enter Y or N.");
            }
        }
        static int AskInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt + ": ");
                var s = Console.ReadLine();
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                {
                    if (v >= min && v <= max) return v;
                }
                Console.WriteLine($"Enter an integer between {min} and {max}.");
            }
        }
        static double AskDouble(string prompt, double min, double max)
        {
            while (true)
            {
                Console.Write(prompt + ": ");
                var s = Console.ReadLine();
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                {
                    if (v >= min && v <= max) return v;
                }
                Console.WriteLine($"Enter a number between {min} and {max}.");
            }
        }
    }
}
