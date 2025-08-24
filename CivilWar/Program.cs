using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CivilWarRemake
{
    enum Stance { Defensive, Offensive }

    sealed class Battle
    {
        public string Code = "";
        public string Title = "";
        public string DateLine = "";
        public string Flavor = "";
        public int MenCSA, MenUSA;
        public int MoneyCSA, MoneyUSA;
        public int InflCSA, InflUSA;     // percent (0–100)
        public int HistCasCSA, HistCasUSA;
        public int HistDesCSA, HistDesUSA;
        public string HistOutcome = "";  // narrative only
    }

    sealed class Result
    {
        public int CasCSA, CasUSA, DesCSA, DesUSA;
        public bool CSAWins;
    }

    class Program
    {
        static readonly CultureInfo IC = CultureInfo.InvariantCulture;
        static StreamWriter Log = new StreamWriter(Stream.Null);
        static string PlayerName = "GENERAL";

        static void Main()
        {
            Console.Write("Enter your name (or leave blank): ");
            PlayerName = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(PlayerName)) PlayerName = "GENERAL";

            bool showRules = AskYesNo("Do you want descriptions (0=No, 1=Yes)? ", zeroOne: true);

            string logPath = $"CIVILWAR_SampleRun_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            Log = new StreamWriter(logPath);

            try
            {
                if (showRules) ShowRules();

                Print("");
                Print("YOU ARE THE CONFEDERACY.  GOOD LUCK!");
                Print("");

                var battles = LoadBattles(); // 14 battles
                int wins = 0, losses = 0;

                foreach (var b in battles)
                {
                    // Strategy selections (kept simple & readable)
                    var def = AskStance("DEFENSIVE");
                    var off = AskStance("OFFENSIVE");

                    Print("");
                    Print($"THIS IS THE BATTLE OF {b.Title}");
                    Print(b.DateLine);
                    if (!string.IsNullOrWhiteSpace(b.Flavor)) Print(b.Flavor);
                    Print("");
                    Print("               CONFEDERACY                UNION");
                    Print($"MEN            {b.MenCSA,10:N0}           {b.MenUSA,10:N0}");
                    Print($"MONEY          {b.MoneyCSA,10:N0}           {b.MoneyUSA,10:N0}");
                    Print($"INFLATION            {b.InflCSA,2}%                  {b.InflUSA,2}%");
                    Print("");

                    // CSA budgets only (Union uses an AI heuristic)
                    int food = AskInt($"HOW MUCH DO YOU WISH TO SPEND FOR FOOD? ", 0, b.MoneyCSA);
                    int salaries = AskInt($"HOW MUCH DO YOU WISH TO SPEND FOR SALARIES? ", 0, b.MoneyCSA - food);
                    int ammo = AskInt($"HOW MUCH DO YOU WISH TO SPEND FOR AMMUNITION? ", 0, b.MoneyCSA - food - salaries);

                    string morale = (food >= b.MoneyCSA * 0.12 && salaries >= b.MoneyCSA * 0.12) ? "FAIR"
                                 : (food < b.MoneyCSA * 0.08 || salaries < b.MoneyCSA * 0.08) ? "POOR" : "HIGH";
                    Print("");
                    Print($"MORALE IS {morale}");
                    Print("");

                    // Union budgets (simple scripted AI)
                    (int uFood, int uSal, int uAmmo) = UnionBudget(b);

                    var r = Resolve(b, def, off, food, salaries, ammo, uFood, uSal, uAmmo);

                    Print("               CONFEDERACY                UNION");
                    Print($"CASUALTIES     {r.CasCSA,10:N0}           {r.CasUSA,10:N0}");
                    Print($"DESERTIONS     {r.DesCSA,10:N0}           {r.DesUSA,10:N0}");
                    Print("");

                    CompareToHistorical(r.CasCSA, b.HistCasCSA, b.Title);

                    if (r.CSAWins) { wins++; Print($"YOU WIN {b.Title.ToUpper()}"); }
                    else { losses++; Print($"YOU LOSE {b.Title.ToUpper()}"); }

                    Print(new string('-', 56));
                    Print("");
                }

                Print($"YOU HAVE WON {wins} BATTLE(S) AND LOST {losses}.");
                Print(wins >= 8 ? "THE CONFEDERACY HAS WON THE WAR"
                                : "THE UNION HAS WON THE WAR");
                Print("");
                Print($"Sample run saved to: {Path.GetFullPath(logPath)}");
            }
            finally
            {
                Log.Flush();
                Log.Dispose();
            }
        }

        // ---------------- Mechanics ----------------

        static Result Resolve(
            Battle b, Stance csaDef, Stance csaOff, int cFood, int cSal, int cAmmo,
            int uFood, int uSal, int uAmmo)
        {
            // Normalize budgets as shares of total money (avoid div-by-zero)
            double cMoney = Math.Max(1, b.MoneyCSA);
            double uMoney = Math.Max(1, b.MoneyUSA);

            double cFoodR = cFood / cMoney, cSalR = cSal / cMoney, cAmmoR = cAmmo / cMoney;
            double uFoodR = uFood / uMoney, uSalR = uSal / uMoney, uAmmoR = uAmmo / uMoney;

            // Discipline & firepower proxies (bounded)
            double Disc(double foodR, double salR) =>
                Clamp(0.4 + 0.7 * salR + 0.5 * foodR, 0.35, 1.6);
            double Fire(double ammoR) =>
                Clamp(0.45 + 1.0 * ammoR, 0.40, 1.6);

            double cDisc = Disc(cFoodR, cSalR);
            double uDisc = Disc(uFoodR, uSalR);
            double cFire = Fire(cAmmoR);
            double uFire = Fire(uAmmoR);

            // Stance multipliers (offense tends to bleed a bit more; defense reduces desertions)
            (double casMul, double desMul) Mul(Stance s)
                => s == Stance.Offensive ? (1.10, 1.05) : (0.95, 0.90);

            var (cOffCas, cDefDes) = (Mul(csaOff).casMul, Mul(csaDef).desMul);
            var (uOffCas, uDefDes) = (Mul(Stance.Offensive).casMul, Mul(Stance.Defensive).desMul); // Union fights mixed

            // Inflation penalty
            double cInfl = 1.0 + 0.15 * (b.InflCSA / 100.0);
            double uInfl = 1.0 + 0.12 * (b.InflUSA / 100.0);

            // Anchor from historical (fallback to rates if missing)
            int cBaseCas = b.HistCasCSA > 0 ? b.HistCasCSA : (int)(0.08 * b.MenCSA);
            int uBaseCas = b.HistCasUSA > 0 ? b.HistCasUSA : (int)(0.07 * b.MenUSA);
            int cBaseDes = b.HistDesCSA > 0 ? b.HistDesCSA : (int)(0.012 * b.MenCSA);
            int uBaseDes = b.HistDesUSA > 0 ? b.HistDesUSA : (int)(0.010 * b.MenUSA);

            // Casualty/Desertion computation (symmetrical-ish)
            double cCas = cBaseCas * cInfl * cOffCas * (uFire / (0.8 + cFire));
            double uCas = uBaseCas * uInfl * uOffCas * (cFire / (0.8 + uFire));
            double cDes = cBaseDes * cInfl * cDefDes * (1.0 / (0.7 + cDisc));
            double uDes = uBaseDes * uInfl * uDefDes * (1.0 / (0.7 + uDisc));

            int casC = Math.Max(0, (int)Math.Round(cCas));
            int casU = Math.Max(0, (int)Math.Round(uCas));
            int desC = Math.Max(0, (int)Math.Round(cDes));
            int desU = Math.Max(0, (int)Math.Round(uDes));

            // Victory heuristic: compare proportional losses with small bias to side w/ lower manpower
            double cLossRate = (casC + desC) / Math.Max(1.0, b.MenCSA);
            double uLossRate = (casU + desU) / Math.Max(1.0, b.MenUSA);
            bool csaWins = (uLossRate - cLossRate) > -0.005; // tiny bias toward CSA to allow narrow wins

            return new Result { CasCSA = casC, CasUSA = casU, DesCSA = desC, DesUSA = desU, CSAWins = csaWins };
        }

        static (int food, int sal, int ammo) UnionBudget(Battle b)
        {
            // A light-touch heuristic: decent food/salary, rest to ammo.
            int food = (int)(b.MoneyUSA * 0.18);
            int sal  = (int)(b.MoneyUSA * 0.22);
            int ammo = Math.Max(0, b.MoneyUSA - food - sal);
            return (food, sal, ammo);
        }

        static double Clamp(double v, double lo, double hi) => v < lo ? lo : (v > hi ? hi : v);

        // ---------------- I/O helpers ----------------

        static void Print(string s)
        {
            Console.WriteLine(s);
            Log.WriteLine(s);
            Log.Flush();
        }

        static bool AskYesNo(string prompt, bool zeroOne = false)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (zeroOne)
                {
                    if (s == "0") return false;
                    if (s == "1") return true;
                    Console.WriteLine("Please answer 0 or 1.");
                }
                else
                {
                    if (s is "Y" or "YES") return true;
                    if (s is "N" or "NO") return false;
                    Console.WriteLine("Please answer Y or N.");
                }
            }
        }

        static int AskInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim();
                if (int.TryParse(s, NumberStyles.Integer, IC, out int v) && v >= min && v <= max) return v;
                Console.WriteLine($"Please enter an integer from {min} to {max}.");
            }
        }

        static Stance AskStance(string phase)
        {
            Print($"YOUR CHOICES FOR {phase} STRATEGY ARE:");
            Print("  (1) ARTILLERY ATTACK");
            Print("  (2) FRONTAL ATTACK");
            Print("  (3) FLANKING MANEUVERS");
            Print("  (4) ENCIRCLEMENT");
            int x = AskInt("YOUR STRATEGY? ", 1, 4);
            return x <= 2 ? Stance.Offensive : Stance.Defensive;
        }

        static void CompareToHistorical(int yourCas, int histCas, string title)
        {
            if (histCas <= 0) { Print("HISTORICAL COMPARISON NOT AVAILABLE."); Print(""); return; }
            double pct = 100.0 * (yourCas - histCas) / histCas;
            string msg = yourCas == histCas ? "THE SAME AS"
                       : yourCas > histCas ? $"{Math.Abs(pct):0}% MORE THAN"
                                           : $"{Math.Abs(pct):0}% LESS THAN";
            Print($"YOUR CASUALTIES WERE {msg} THE ACTUAL CASUALTIES AT {title.ToUpper()}");
            Print("");
        }

        static void ShowRules()
        {
            Print("THIS IS A CIVIL WAR SIMULATION.");
            Print("TO PLAY, TYPE A RESPONSE WHEN THE COMPUTER ASKS.");
            Print("REMEMBER THAT ALL FACTORS ARE INTERRELATED AND THAT YOUR CHOICES");
            Print("COULD CHANGE HISTORY. MOST BATTLES TEND TO RESULT AS THEY DID,");
            Print("BUT IT ALL DEPENDS ON YOU!");
            Print("");
            Print("OBJECTIVE: WIN AS MANY BATTLES AS POSSIBLE.");
            Print("FOR EACH BATTLE:");
            Print("  • CHOOSE A DEFENSIVE STRATEGY AND AN OFFENSIVE STRATEGY.");
            Print("  • ALLOCATE YOUR BUDGET (FOOD, SALARIES, AMMUNITION).");
            Print("  • RESULTS SHOW CASUALTIES/DESERTIONS AND A WIN/LOSS.");
            Print("  • YOUR CASUALTIES ARE COMPARED TO THE HISTORICAL RECORD.");
            Print("");
        }

        // ---------------- Data Pack (14 battles) ----------------
        // Numbers below are reasonable, self-consistent anchors (not strict historical records);
        // they exist to give the game texture and difficulty across the campaign.

        static List<Battle> LoadBattles() => new()
        {
            new Battle {
                Code="BULL_RUN_1", Title="BULL RUN (FIRST MANASSAS)",
                DateLine="JULY 21, 1861",
                Flavor="PREMATURE UNION ASSAULT; JACKSON'S STAND EARNS 'STONEWALL'.",
                MenCSA=33000, MenUSA=68000, MoneyCSA=180000, MoneyUSA=120000, InflCSA=25, InflUSA=8,
                HistCasCSA=2000, HistCasUSA=1900, HistDesCSA=60, HistDesUSA=55, HistOutcome="CONFEDERACY"
            },
            new Battle {
                Code="SHILOH", Title="SHILOH",
                DateLine="APRIL 6–7, 1862",
                Flavor="CONFEDERATE SURPRISE BLUNTS; UNION HOLDS FIELD ON DAY TWO.",
                MenCSA=40000, MenUSA=48000, MoneyCSA=170000, MoneyUSA=220000, InflCSA=27, InflUSA=8,
                HistCasCSA=10500, HistCasUSA=11800, HistDesCSA=30, HistDesUSA=18, HistOutcome="UNION"
            },
            new Battle {
                Code="SEVEN_DAYS", Title="SEVEN DAYS",
                DateLine="JUNE 25–JULY 1, 1862",
                Flavor="LEE FORCES McCLELLAN AWAY FROM RICHMOND.",
                MenCSA=95000, MenUSA=115000, MoneyCSA=360000, MoneyUSA=300000, InflCSA=25, InflUSA=8,
                HistCasCSA=20000, HistCasUSA=16000, HistDesCSA=90, HistDesUSA=70, HistOutcome="CONFEDERACY"
            },
            new Battle {
                Code="BULL_RUN_2", Title="SECOND BULL RUN",
                DateLine="AUG 28–30, 1862",
                Flavor="LEE & JACKSON OUTMANEUVER POPE; UNION RETREATS.",
                MenCSA=50000, MenUSA=62000, MoneyCSA=200000, MoneyUSA=240000, InflCSA=25, InflUSA=8,
                HistCasCSA=8300, HistCasUSA=14000, HistDesCSA=70, HistDesUSA=120, HistOutcome="CONFEDERACY"
            },
            new Battle {
                Code="ANTIETAM", Title="ANTIETAM",
                DateLine="SEPT 17, 1862",
                Flavor="BLOODIEST SINGLE DAY; LEE WITHDRAWS, UNION CLAIMS EDGE.",
                MenCSA=38000, MenUSA=75000, MoneyCSA=160000, MoneyUSA=260000, InflCSA=26, InflUSA=9,
                HistCasCSA=10300, HistCasUSA=12400, HistDesCSA=40, HistDesUSA=35, HistOutcome="UNION"
            },
            new Battle {
                Code="FREDERICKSBURG", Title="FREDERICKSBURG",
                DateLine="DEC 11–15, 1862",
                Flavor="FRONTAL UNION ASSAULTS FAIL AGAINST STRONG CSA POSITION.",
                MenCSA=78000, MenUSA=122000, MoneyCSA=240000, MoneyUSA=320000, InflCSA=27, InflUSA=10,
                HistCasCSA=5300, HistCasUSA=12600, HistDesCSA=50, HistDesUSA=60, HistOutcome="CONFEDERACY"
            },
            new Battle {
                Code="CHANCELLORSVILLE", Title="CHANCELLORSVILLE",
                DateLine="APR 30–MAY 6, 1863",
                Flavor="LEE'S DARING DIVIDE ROUTS UNION; JACKSON MORTALLY WOUNDED.",
                MenCSA=60000, MenUSA=133000, MoneyCSA=260000, MoneyUSA=340000, InflCSA=27, InflUSA=9,
                HistCasCSA=13000, HistCasUSA=17500, HistDesCSA=70, HistDesUSA=90, HistOutcome="CONFEDERACY"
            },
            new Battle {
                Code="GETTYSBURG", Title="GETTYSBURG",
                DateLine="JULY 1–3, 1863",
                Flavor="PIVOTAL NORTHERN VICTORY; PICKETT'S CHARGE REPULSED.",
                MenCSA=71000, MenUSA=94000, MoneyCSA=270000, MoneyUSA=360000, InflCSA=28, InflUSA=10,
                HistCasCSA=23000, HistCasUSA=23000, HistDesCSA=120, HistDesUSA=150, HistOutcome="UNION"
            },
            new Battle {
                Code="CHICKAMAUGA", Title="CHICKAMAUGA",
                DateLine="SEPT 19–20, 1863",
                Flavor="CONFEDERATES FORCE UNION BACK; HEAVY LOSSES.",
                MenCSA=66000, MenUSA=58000, MoneyCSA=250000, MoneyUSA=260000, InflCSA=29, InflUSA=10,
                HistCasCSA=18000, HistCasUSA=16300, HistDesCSA=80, HistDesUSA=100, HistOutcome="CONFEDERACY"
            },
            new Battle {
                Code="CHATTANOOGA", Title="CHATTANOOGA",
                DateLine="NOV 23–25, 1863",
                Flavor="GRANT BREAKS THE SIEGE; UNION OPENS THE GATE SOUTH.",
                MenCSA=45000, MenUSA=56000, MoneyCSA=220000, MoneyUSA=300000, InflCSA=30, InflUSA=11,
                HistCasCSA=6700, HistCasUSA=5800, HistDesCSA=60, HistDesUSA=40, HistOutcome="UNION"
            },
            new Battle {
                Code="WILDERNESS", Title="THE WILDERNESS",
                DateLine="MAY 5–7, 1864",
                Flavor="FEROCIOUS FIGHTING IN DENSE WOODS; GRANT PRESSES ON.",
                MenCSA=61000, MenUSA=101000, MoneyCSA=260000, MoneyUSA=380000, InflCSA=32, InflUSA=12,
                HistCasCSA=11000, HistCasUSA=17800, HistDesCSA=70, HistDesUSA=90, HistOutcome="INCONCLUSIVE"
            },
            new Battle {
                Code="SPOTSYLVANIA", Title="SPOTSYLVANIA",
                DateLine="MAY 8–21, 1864",
                Flavor="GRANT'S RELENTLESS OFFENSIVE BLEEDS BOTH ARMIES.",
                MenCSA=62000, MenUSA=100000, MoneyCSA=260000, MoneyUSA=390000, InflCSA=33, InflUSA=12,
                HistCasCSA=12000, HistCasUSA=18000, HistDesCSA=75, HistDesUSA=95, HistOutcome="INCONCLUSIVE"
            },
            new Battle {
                Code="VICKSBURG", Title="VICKSBURG",
                DateLine="MAY 18–JULY 4, 1863",
                Flavor="SIEGE GIVES UNION CONTROL OF THE MISSISSIPPI.",
                MenCSA=34000, MenUSA=77000, MoneyCSA=200000, MoneyUSA=310000, InflCSA=30, InflUSA=10,
                HistCasCSA=9700, HistCasUSA=10000, HistDesCSA=50, HistDesUSA=40, HistOutcome="UNION"
            },
            new Battle {
                Code="ATLANTA", Title="ATLANTA",
                DateLine="JULY–SEPT 1864",
                Flavor="SHERMAN TAKES THE CITY; A MAJOR BLOW TO CSA INDUSTRY.",
                MenCSA=55000, MenUSA=100000, MoneyCSA=240000, MoneyUSA=400000, InflCSA=35, InflUSA=12,
                HistCasCSA=8000, HistCasUSA=3700, HistDesCSA=85, HistDesUSA=30, HistOutcome="UNION"
            },
            new Battle {
                Code="PETERSBURG", Title="PETERSBURG (SIEGE)",
                DateLine="JUNE 1864–APRIL 1865",
                Flavor="TRENCH WARFARE; UNION CUTS SUPPLY LINES TO RICHMOND.",
                MenCSA=60000, MenUSA=125000, MoneyCSA=260000, MoneyUSA=420000, InflCSA=40, InflUSA=13,
                HistCasCSA=28000, HistCasUSA=42000, HistDesCSA=140, HistDesUSA=120, HistOutcome="UNION"
            },
        };
    }
}
