using System;
using System.Collections.Generic;
using System.Globalization;

namespace AppalachianTrailGame
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var game = new Game();
            game.Run();
        }
    }

    #region Data Models
    record ItemOption(string Name, int WeightOz, int PriceUSD, int VolumeCuIn);

    record Section(string Title, ItemOption[] Options);
    #endregion

    class Game
    {
        // Core RNG (QBASIC RANDOMIZE RN equivalent)
        private readonly Random _rng;

        // Player & hike state (direct translations of BASIC variables)
        string A = "";                 // Generic input string holder (A$)
        string Sex = "";               // A$
        double RN = 0;                 // RN
        double RT = 1.0;               // Off‑trail rate multiplier (RT)
        double RM = 1.0;               // Rate multiplier after mishaps (RM)
        double RW = 0;                 // Walking rate (mph)
        double TW = 0;                 // Walking hours per day
        int PC = 0;                    // Physical condition 1..4
        int IVY = 0;                   // Poison ivy sensitivity 1..4
        int RAIN = 0;                  // Attitude towards rain 1..4
        int FOOD = 0;                  // Food supply method (1..3)
        int U = 0;                     // Underwear changes
        int STK = 0;                   // Walking stick (1/0)

        // Gear chosen
        readonly string[] ITEM = new string[8]; // 1..7
        readonly int[] WT = new int[8];         // oz, 1..7
        readonly int[] COST = new int[8];       // $, 1..7
        readonly int[] VOL = new int[8];        // cu in, 1..7

        // Running totals / derived
        int WPoz = 0;                  // Total worn+carried weight (oz) before food/water
        int CST = 0;                   // Total optional gear cost
        double WB = 0;                 // Body weight (lb)
        int STV = 0;                   // Segments where user changed variables

        // Food / calories
        int[] FD = new int[6];         // 1..5 percentages
        int CAL = 0;                   // calories eaten per day
        int CD = 0;                    // calories burned per day
        int FWT = 0;                   // food weight per day (oz)
        int DY = 0;                    // average days of food carried
        int WADD = 0;                  // food+water add (oz)
        double CADD = 0;               // added calories due to carrying extra weight

        // Hike loop variables
        double WP = 0;                 // Pack weight (lb) during hike (WPoz/16 + food)
        double DINPT = 0;              // Desired miles per 3‑day segment
        double DMAX = 0;               // Max miles based on condition
        double D = 0;                  // Distance walked (mi)
        double T = 0;                  // Time (days)
        int TD = 0;                    // Printed days rounded
        int TT = -1;                   // Last printed day

        // Mishap controls (simplified scaffolding)
        readonly int[] HZ = new int[26];
        readonly double[] R = new double[11];
        readonly int[] TSLOW = new int[11];
        int SNO = 0;                   // Snow counter
        int KEN = 0;                   // Kennebec crossed?
        int DOC = 0;                   // Doctor case selector
        int DB = 300;                  // Boot mileage offset (as in BASIC)

        // Weight loss tracking
        double TL = 0;                 // last segment time stamp
        double WTLOST = 0;             // total lb lost

        // Locations (mileposts)
        readonly int[] DLC = new int[26];
        readonly string[] LC = new string[26];

        // Sections / options (from DATA statements)
        readonly Section[] Sections;

        public Game()
        {
            // RANDOMIZE using time seed; BASIC code uses keystroke loops to seed RN
            _rng = new Random();

            // Gear sections
            Sections = new[]
            {
                new Section("Tent", new []
                {
                    new ItemOption("Sierra Designs Divine Light, 20 sq ft, max height 34 in.", 35, 135, 214),
                    new ItemOption("Eureka! Crescent Bike, 22 sq ft, height 43 in.", 48, 125, 353),
                    new ItemOption("Moss Starlet, 29 sq ft; with vestibule, 37; height 39 in.", 78, 250, 334),
                    new ItemOption("None. Use trail shelters and sleep in open.", 0, 0, 0)
                }),
                new Section("Pack", new []
                {
                    new ItemOption("Kelty Mountaineer external - frame with Seneca pack", 69, 139, 3975),
                    new ItemOption("Jansport D2 external - frame pack", 99, 169, 5520),
                    new ItemOption("Camp Trails Grey Wolf internal - frame pack, large", 82, 119, 5975),
                    new ItemOption("Coleman Peak 1 model 680 internal - frame pack", 58, 115, 4013)
                }),
                new Section("Sleeping bag", new []
                {
                    new ItemOption("North Face Blue Kazoo, mummy, goose down, rated 25 deg", 34, 140, 452),
                    new ItemOption("Slumberjack Bike Lite, mummy, Quallofil, rated 30 deg", 50, 65, 808),
                    new ItemOption("REI 747 Wide Body, semi-rect, Quallofil, rated 30 deg", 54, 90, 1884),
                    new ItemOption("L.L. Bean Ultra - Lite, rectangular, Quallofil, rated 35 deg", 58, 80, 804),
                }),
                new Section("Sleeping pad/mattress", new []
                {
                    new ItemOption("Sevylor Superlight air mattress", 32, 20, 360),
                    new ItemOption("Therm-A-Rest self - inflating ultra-lite pad", 28, 48, 325),
                    new ItemOption("Ensolite 1/2 in. pad", 24, 23, 300),
                    new ItemOption("None.", 0, 0, 0),
                }),
                new Section("Stove", new []
                {
                    new ItemOption("MSR Whisperlite, uses white gas (available along trail)", 18, 37, 120),
                    new ItemOption("Gaz Bleuet, fuel: butane cartridge (light and reliable)", 16, 20, 106),
                    new ItemOption("Primus Grasshopper, fuel: propane cylinder (long life)", 22, 19, 90),
                    new ItemOption("None (incidentally, wood fires are PROHIBITED on the trail)", 0, 0, 0),
                }),
                new Section("Boots", new []
                {
                    new ItemOption("Asolo Trail II S, mid-ankle leather boots", 49, 140, 0),
                    new ItemOption("Danner Featherlight Trail, mid-ankle leather & Gore-Tex boot", 52, 95, 0),
                    new ItemOption("Timberland Lightweight Hiker, mid-ankle fabric and Gore-Tex", 46, 50, 0),
                    new ItemOption("Raichle Montagnas, full-height leather boots", 80, 125, 0),
                }),
                new Section("Raingear", new []
                {
                    new ItemOption("Early Winters Ultralight Gore-Tex rain parka", 13, 145, 25),
                    new ItemOption("Patagonia featherweight Gore-Tex shell", 7, 58, 15),
                    new ItemOption("Campmor nylon poncho", 16, 25, 22),
                    new ItemOption("None.", 0, 0, 0),
                }),
            };

            // Mileposts / locations (from DATA statements)
            var dlcData = new (int mile, string name)[]
            {
                (79, "Bly Gap, GA"), (165, "Doe Knob, NC"), (302, "Big Bald Mt., NC"),
                (384, "Wilbur Lake, TN"), (483, "Big Walker Lookout, VA"), (602, "Tinker Mt., VA"),
                (698, "Salt Log Gap, VA"), (800, "Fishers Gap, VA"), (889, "Potomac River, WV"),
                (966, "Piney Mt., PA"), (1099, "Baer Rocks, PA"), (1190, "High Point, NC"),
                (1272, "Shenandoah Mt., NY"), (1361, "Sages Ravine, CT"), (1435, "Mt. Greylock, MA"),
                (1554, "Killington, VT"), (1687, "Mt. Washington, NH"), (1776, "Bemis Pond, ME"),
                (1855, "Kennebec River, ME"), (1922, "Chairback Mt., ME"), (1992, "Penobscot West Branch, ME")
            };
            for (int i = 0; i < dlcData.Length; i++)
            {
                DLC[i + 1] = dlcData[i].mile;
                LC[i + 1] = dlcData[i].name;
            }
        }

        public void Run()
        {
            Console.Clear();
            CenterPrint("Appalachian Trail");
            Console.SetCursorPosition(0, Console.CursorTop + 2);
            CenterPrint("(c) by David H. Ahl, 1986");
            Console.WriteLine(); Console.WriteLine();
            CenterPrint("Press any key to continue.");
            WaitForKey();

            // Instructions
            Console.Clear();
            Instructions();

            // Input
            GatherPlayerData();

            // Gear selection (7 sections)
            SelectGear();

            // Food strategy & diet
            ChooseFoodStrategy();
            ChooseDiet();

            // Calorie usage and pack weight adjustments
            CalculateCaloriesAndWeights();

            // Begin hike
            BeginHike();
        }

        #region Flow Sections
        void Instructions()
        {
            CenterPrint("Appalachian Trail");
            Console.WriteLine();
            Console.WriteLine();
            PrintLines(
                " You are a hiker whose goal is to walk the entire 2007 miles of",
                "the Appalachian Trail from Springer Mt., GA, to Mt. Katahdin, Maine.",
                "You set out in April as soon as the Smokies are clear of snow, and",
                "you must reach the northern terminus before it is blocked by snow.",
                " Your hike is divided into three-day segments. Along the way,",
                "you encounter natural hazards, difficulties with your equipment,",
                "and physical problems.",
                " Careful planning for your hike is very important. In deciding",
                "what to pack, you have to make trade-offs—generally between weight",
                "and comfort. Of course, everything must fit in your pack.",
                " You must decide how you will obtain food along the route, how",
                "much to eat in each food group, and how many calories to replenish.",
                " You must decide at what pace you will walk, and how long to",
                "hike each day. Of course, a faster pace will cover mileage more",
                "quickly than a slower one, but it is much harder on your body.",
                " You don't have many choices when dealing with mishaps. It is",
                "assumed that you are a sensible hiker, make repairs when necessary,",
                "replace things that wear out, and see a doctor if you get sick.",
                "Nevertheless, mishaps cost you time, of which you have little to",
                "spare as you take another of the five million steps towards Maine.");
            Console.WriteLine();
            CenterPrint("Press any key when you're ready to go.");
            WaitForKey();
        }

        void GatherPlayerData()
        {
            Console.Clear();
            Console.WriteLine("First we need some data about you.\n");

            Sex = Prompt("Your sex (male or female)", normalizeFirstLetter: true);
            if (Sex != "M" && Sex != "F")
            {
                Console.WriteLine("Answer 'M' or 'F' please.");
                GatherPlayerData();
                return;
            }

            WB = PromptNumberDouble("Your weight in pounds", min: 80, max: 400, retryMsg: "Surely you jest. Let's try that one again.");

            PC = (int)PromptNumberDouble("What is your physical condition (1 = excellent, 2 = good, 3 = fair, 4 = poor)", 1, 4,
                retryMsg: "Answer 1, 2, 3, or 4 please.");

            Console.WriteLine();
            PrintLines(
                "Walking pace: You may change your pace as the hike progresses.",
                "Remember, a faster pace covers the distance more quickly but",
                "burns more calories and has a higher risk of injury.",
                "Slow and deliberate......1.7 mph",
                "Moderate and vigorous......3 mph",
                "Fast and very difficult....4 mph");
            RW = PromptNumberDouble("At what rate in mph do you wish to walk (number & decimal okay)", 1.0, 4.2,
                outOfRangeMsg: v => $"A rate of {v.ToString(CultureInfo.InvariantCulture)} mph is silly.");

            Console.WriteLine();
            Console.WriteLine("Walking hours per day: You may change this as time goes on.");
            TW = PromptNumberDouble("To start, how many hours do you wish to walk per day", max: 14,
                retryMsg: "That's just too ambitious.");
            while (TW * RW < 7)
            {
                Console.WriteLine("You won't even reach NJ by Christmas.");
                TW = PromptNumberDouble("To start, how many hours do you wish to walk per day", max: 14,
                    retryMsg: "That's just too ambitious.");
            }

            Console.WriteLine();
            Console.WriteLine("Your sensitivity to poison ivy:");
            Console.WriteLine(" (1) Highly sensitive\n (2) Moderately sensitive\n (3) Immune\n (4) Had series of desensitization shots");
            IVY = (int)PromptNumberDouble("Which number describes you", 1, 4,
                retryMsg: "What's that? Let's try again.");

            Console.WriteLine();
            PrintLines(
                "People who have hiked the Trail have different feelings about rain:",
                " (1) Let it pour, I love it.",
                " (2) No problem as long as the sun comes out every few days.",
                " (3) Five solid days of rain really gets me down.",
                " (4) If I foresee a long stretch of rain, I'll hole up in a",
                "     shelter or motel and wait it out.");
            RAIN = (int)PromptNumberDouble("Which number most closely describes your feeling", 1, 4,
                retryMsg: "Not possible. Again please.");
        }

        void SelectGear()
        {
            Console.WriteLine();
            Console.WriteLine("You must make some decisions about what to pack.");
            for (int i = 0; i < Sections.Length; i++)
            {
                var s = Sections[i];
                Console.WriteLine();
                Console.WriteLine($"{s.Title}:");
                for (int j = 0; j < s.Options.Length; j++)
                {
                    var o = s.Options[j];
                    Console.Write($"  {j + 1}.. {o.Name}  ");
                    Console.Write(" ");
                    PrintWeightOz(o.WeightOz);
                    if (o.VolumeCuIn > 0) Console.Write($", {o.VolumeCuIn} cu in");
                    if (o.PriceUSD > 0) Console.Write($", price: ${o.PriceUSD}");
                    Console.WriteLine();
                }

                int choice;
                while (true)
                {
                    choice = (int)PromptNumberDouble("Which one do you want (number)", 1, s.Options.Length,
                        retryMsg: "Come on now; answer a valid option number.");
                    if (choice >= 1 && choice <= s.Options.Length) break;
                }
                var pick = s.Options[choice - 1];
                ITEM[i + 1] = pick.Name;
                WT[i + 1] = pick.WeightOz;
                COST[i + 1] = pick.PriceUSD;
                VOL[i + 1] = pick.VolumeCuIn;
            }

            // Volume checks (translated from BASIC logic)
            int VOL1 = VOL[1] + VOL[5] + VOL[7];
            int VOL2 = VOL[3] + VOL[4];

            // If COST(2) > 135 then internal pack? Keep the original conditional intent
            bool ok = true;
            if (COST[2] > 135)
            {
                if (VOL[2] <= 3000 + VOL1) ok = true; else ok = false;
            }
            else
            {
                if (VOL[2] > 3000 + VOL1 + VOL2) ok = false;
            }

            if (!ok)
            {
                Console.WriteLine("Your pack is too small to hold all those things plus clothes and");
                Console.WriteLine("food. You'll have to take a larger pack or some smaller items.");
                Console.WriteLine();
                Console.WriteLine("Let's try again…\n");
                // Re-prompt the entire gear selection (as BASIC does via RESTORE/GOTO 570)
                Array.Clear(ITEM, 0, ITEM.Length);
                Array.Clear(WT, 0, WT.Length);
                Array.Clear(COST, 0, COST.Length);
                Array.Clear(VOL, 0, VOL.Length);
                SelectGear();
                return;
            }

            U = (int)PromptNumberDouble("How many changes of underwear do you want to take", max: 6,
                retryMsg: "This is not a picnic. Take fewer.");

            A = Prompt("Do you want to take a walking stick (Y or N)", normalizeFirstLetter: true);
            STK = A == "Y" ? 1 : 0;

            Console.WriteLine();
            Console.WriteLine("To summarize, here is what you have chosen:");
            for (int i = 0; i < Sections.Length; i++)
                Console.WriteLine($"{Sections[i].Title} : {ITEM[i + 1]}");
            Console.WriteLine($"Changes of underwear: {U}");
            if (STK == 1) Console.WriteLine("Walking stick.");

            PrintLines(
                " In addition, you must carry (or wear) a hat, short-sleeve shirt,",
                "chamois shirt, light jacket, long underwear, hiking shorts, long",
                "pants, 3 pairs socks, eating gear, water bottle, soap, toilet tissue,",
                "toilet supplies, towel, first-aid kit, snakebite kit, flashlight,",
                "100' nylon cord, watch, compass, lighter, bandanna, sewing kit, insect",
                "repellent, Swiss Army knife, water-purifier tablets, notebook, maps,",
                "guidebook, stuff sacks, moleskin, camera, and money.\n");

            // Summarize weights & costs
            WPoz = 0; CST = 0;
            for (int i = 1; i <= 7; i++) { WPoz += WT[i]; CST += COST[i]; }
            WPoz += 190 + U * 4; // clothing & required items
            if (STK == 1) WPoz += 24;

            Console.WriteLine($"If you bought everything new, the total cost would be ${225 + CST}");
            Console.Write("The total weight of what you are wearing and carrying is ");
            PrintWeightOz(WPoz); Console.WriteLine("\n.... not including food or water.");
        }

        void ChooseFoodStrategy()
        {
            Pause("Press any key to continue");
            Console.WriteLine();
            Console.WriteLine("Common systems of food supply include:");
            PrintLines(
                " (1) Caches buried along the trail. Pros: no wasted time leaving",
                "     the Trail for food, heavy items can be buried.",
                " (2) Food sent to post offices along the way. Pros: more flexible",
                "     than caches. Cons: P.O.s closed nights, Sat pm and Sun.",
                " (3) Grocery stores and restaurants. Pros: good variety, cheap.",
                "     Cons: wasted time leaving Trail, limited opening hours.");

            FOOD = (int)PromptNumberDouble("Which will be your major method of food supply", 1, 3,
                retryMsg: "Sorry, try again.");
            RT = (FOOD == 1) ? 1.0 : 0.95; // off‑trail excursions reduce rate
            RM = RT; // initial
            ShortPause();
            Console.Clear();
        }

        void ChooseDiet()
        {
            Console.WriteLine("Obviously, you will carry your food in the most efficient form:");
            Console.WriteLine("dried, dehydrated, concentrated, etc. However, you must specify");
            Console.WriteLine("the percentage of your diet accounted for by each of the following");
            Console.WriteLine("food groups (remember, all five must add up to 100).");
            Console.WriteLine(" (1) Dairy foods, cheese, yogurt");
            Console.WriteLine(" (2) Fruits and vegetables");
            Console.WriteLine(" (3) Meat, poultry, fish, eggs");
            Console.WriteLine(" (4) Bread, cereal, seeds, nuts,");
            Console.WriteLine(" (5) Margarine, lard, oils, fats");

            while (true)
            {
                int ct = 0;
                for (int i = 1; i <= 5; i++)
                {
                    FD[i] = (int)PromptNumberDouble($"Percent for group {i}", 0, 100);
                    ct += FD[i];
                }
                Console.WriteLine($"Total: {ct}%\n");
                if (ct == 100)
                {
                    Console.WriteLine("Very good.\n");
                    break;
                }
                Console.WriteLine($"Sorry, but your percentages add up to {ct} rather than to 100%.");
                Pause("Press any key when you're ready to try again.");
                Console.Clear();
            }
        }

        void CalculateCaloriesAndWeights()
        {
            // Desired distance (cap 30 in BASIC per day value; we will keep the equation as-is)
            double DM = RW * TW;
            if (DM > 30) DM = 30;

            // Calories = metabolism + walking + climbing + camp activities (translated)
            CD = (int)(WB * 11.5 + WB * DM * .3 + (WB + (double)WPoz / 16.0) * DM * .21 + WB * (15 - TW) * .22);
            Console.WriteLine();
            Console.WriteLine("Given your weight and that of your supplies, your walking");
            Console.WriteLine("speed, and your walking time per day, you can expect");
            Console.WriteLine($"to burn at least {CD} calories per day.");

            // Ask calories to eat
            while (true)
            {
                CAL = (int)PromptNumberDouble("How many calories worth of food do you want to eat", min: 0);
                if (CAL < 0.6 * CD)
                {
                    Console.WriteLine("Your body will rebel against burning that much body fat.");
                    Console.WriteLine("Better eat a bit more…");
                    continue;
                }
                if (CAL > 1.5 * CD)
                {
                    Console.WriteLine("No blimps allowed on the trail.");
                    continue;
                }
                break;
            }

            // Food weight per day (FWT), printed as lbs/oz
            FWT = (int)(CAL * 3.2 / (4 * FD[1] + 3 * FD[2] + 4 * FD[3] + 4 * FD[4] + 9 * FD[5]));
            Console.Write("That means eating an approx food weight per day of ");
            PrintWeightOz(FWT); Console.WriteLine();

            // Avg days of food carried
            DY = (FOOD == 3) ? 2 : 3;
            WADD = DY * FWT + 17;            // food + water in oz
            CADD = WADD * DM * .21;          // extra calories to carry it
            WPoz += WADD;                    // add to carried weight (oz)
            CD += (int)CADD;                 // increase daily burn

            Console.WriteLine($"Food and water add {WADD} oz. to your trail weight bringing your");
            Console.Write("total weight (worn and carried) to ");
            PrintWeightOz(WPoz); Console.WriteLine();

            Console.WriteLine();
            CenterPrint("Preparations are finally complete!");
            ShortPause();
        }

        void BeginHike()
        {
            WP = WPoz / 16.0; // convert to lb for formulas
            EstablishTrueHikingPace();

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(" It is April 1 and you briskly step out on the approach trail");
            Console.WriteLine("at Amicalola Falls, Georgia. You hike the 6.9 miles to the peak");
            Console.WriteLine("of Springer Mountain and sign the trail log, the first of many");
            Console.WriteLine("that you intend to sign. Your hike will take you through 14");
            Console.WriteLine("states as the Trail wanders 2007 miles along the Appalachian");
            Console.WriteLine("Mountains to Baxter Peak on Mt. Katahdin in Maine. It is a");
            Console.WriteLine("challenging trail with an average climb of 200 feet each mile.");
            Console.WriteLine("Fewer than 2000 people have walked its entire length. Good luck!");
            Pause();

            // Main loop — translated in spirit; many mishaps omitted for brevity.
            while (true)
            {
                T += 3;                 // each loop covers ~3 days
                TD = (int)Math.Floor(T + 0.5);
                Console.WriteLine();
                PrintDateIfNeeded();

                // Distance this segment (scaled by RM and DINPT)
                double DDAYS = 3 * RM * DINPT;
                D += DDAYS;
                if (D > 1999)
                {
                    ReachEndOfTrail();
                    return;
                }

                Console.Write($"You have walked {(int)D} miles. ");
                bool nearSpot = false;
                for (int i = 1; i <= 21; i++)
                {
                    if (D > DLC[i] - 17 && D < DLC[i] + 17)
                    {
                        Console.WriteLine($"You are near {LC[i]}");
                        nearSpot = true; break;
                    }
                }
                if (!nearSpot) Console.WriteLine();

                if (D > 1466) { R[10] = .85; TSLOW[10] = 2007; } // slow in mountains (placeholder)

                // Mishaps & weather (scaffold)
                HandleMishaps();
                if (T > 12) LongRainStretch();
                if (D > 1545 && T > 166) SnowInNewEngland();
                if (D > 1845 && KEN == 0) KennebecRiver();

                if (D > (STV + 1) * 400) MaybeResetInputs();

                Pause();
            }
        }
        #endregion

        #region Subroutines (translated)
        void EstablishTrueHikingPace()
        {
            DINPT = RW * TW;                 // desired distance per day
            if (D <= 600)
            {
                // Under 600 miles, physical condition limits mileage (PC 1..4)
                DMAX = 6 + 6 * (5 - PC);
                if (DINPT > DMAX) DINPT = DMAX;
            }
            if (WB / (WP > 0 ? WP : 1) <= 6)
            {
                // Heavy pack reduces speed
                DINPT = (.49 + .086 * WB / (WP > 0 ? WP : 1)) * DINPT;
            }
        }

        void RecomputeRateFromMishaps()
        {
            RM = RT;
            for (int i = 1; i <= 10; i++)
            {
                if (TSLOW[i] > T && R[i] > 0) RM *= R[i];
            }
        }

        void HandleMishaps()
        {
            // This is a condensed version of BASIC's large mishap table (1710..3340).
            // It randomly applies a few slowdowns to show the porting structure.
            int rn = _rng.Next(1, 41); // 1..40
            int tmTenthDays = 0;       // time lost (days)

            switch (rn)
            {
                case <= 4:
                    // Blisters (2570)
                    if (HZ[8] <= 3)
                    {
                        HZ[8]++;
                        R[1] = .9; TSLOW[1] = (int)T + 14;
                        Console.WriteLine("You have some nasty blisters that will slow your pace.");
                    }
                    break;
                case <= 8:
                    // Twisted ankle (2670)
                    if (HZ[11] <= 1)
                    {
                        HZ[11]++;
                        R[3] = .75; TSLOW[3] = (int)T + 6;
                        Console.WriteLine("You twisted your ankle crossing a stream. That will slow your pace for a few days.");
                    }
                    break;
                case <= 12:
                    // Lost (2390)
                    Console.WriteLine("Trail is poorly marked and you get temporarily lost.");
                    tmTenthDays += 3; // 0.3 day ~ 7 hours
                    break;
                case <= 16:
                    // Bad zipper (2410)
                    Console.WriteLine("Broken zipper on your pack. Lose time drying stuff.");
                    tmTenthDays += 2; // 0.2 day
                    break;
                case <= 20:
                    // Rain gear rip if chosen (3100)
                    if (WT[7] > 0 && HZ[20] == 0 && D >= 500)
                    {
                        HZ[20] = 1; tmTenthDays += 4;
                        Console.WriteLine("Bad rip in raingear. Must get a replacement.");
                    }
                    break;
                default:
                    // Walking… (3340)
                    Console.WriteLine("Walking…walking…walking…walking.");
                    break;
            }

            if (tmTenthDays > 0)
            {
                T += tmTenthDays / 10.0;
            }
            RecomputeRateFromMishaps();
        }

        void LongRainStretch()
        {
            // 6% chance of heavy rain sequence (4030..4250 condensed)
            if (_rng.NextDouble() < 0.06)
            {
                Console.WriteLine("It has been raining steadily for the past week and you are thoroughly soaked…");
                int stage = Math.Min(++HZ[25], 4);
                double tm = RAIN * 1.7; // days lost (scaled as per BASIC intent)
                T += tm;
                if (stage >= 4 && (RAIN == 2 || RAIN == 3) && D < 1900)
                {
                    Console.WriteLine("That's it. You can't take any more. Maybe you'll try again next year.");
                    EndGameSummary(aborted: true);
                }
            }
        }

        void SnowInNewEngland()
        {
            // Simplified version of 4260..4370
            bool snow = (T > 200 && _rng.NextDouble() > .5) || _rng.NextDouble() <= .2;
            if (!snow) return;
            SNO++;
            Console.WriteLine("Oh oh, New England is getting some snow…");
            if (SNO >= 3)
            {
                Console.WriteLine("You made a gallant attempt to get through, but the Park Rangers won't let you go on. Too bad.");
                EndGameSummary(aborted: true);
            }
            else if (SNO == 2)
            {
                Console.WriteLine("You pushed through the last flurries but this looks more serious. On you go…");
            }
        }

        void KennebecRiver()
        {
            KEN = 1;
            Console.WriteLine();
            Console.WriteLine("You have arrived at the Kennebec River.");
            var ans = Prompt("Did you make prior arrangements to get across (Y/N)", normalizeFirstLetter: true);
            if (ans == "Y")
            {
                if (_rng.NextDouble() > 0.5)
                {
                    Console.WriteLine("Fortunately the person you called showed up to meet you with a canoe. You get across in jig time.");
                    T += 0.5;
                }
                else
                {
                    Console.WriteLine("Too bad; the guy you called didn't show up.");
                    ForceWaitForCrossing();
                }
            }
            else
            {
                Console.WriteLine("That wasn't very sensible. What will you do now?");
                ForceWaitForCrossing();
            }
        }

        void ForceWaitForCrossing()
        {
            if (_rng.NextDouble() > 0.7)
            {
                Console.WriteLine("Fortunately the river isn't running too high and you can probably wade across at the ford. Lucky!");
                T += 0.6;
            }
            else
            {
                int tm = 2 + _rng.Next(0, 3); // 2..4 days
                Console.WriteLine($"The river is running high; you wait around for help. Finally—you're across, but it cost you {tm} days.");
                T += tm;
            }
        }

        void MaybeResetInputs()
        {
            STV++;
            var ans = Prompt("Want to change walking pace or hours of walking (Y/N)", normalizeFirstLetter: true);
            if (ans == "Y")
            {
                RW = PromptNumberDouble("New walking pace (mph)", 1.0, 4.5,
                        outOfRangeMsg: v => $"A rate of {v.ToString(CultureInfo.InvariantCulture)} mph is silly.");
                TW = PromptNumberDouble("New hours per day on the trail", max: 14,
                        retryMsg: "Come now; that's just too ambitious.");

                if (STK == 0)
                {
                    var s = Prompt("Want to change your mind and carry a walking stick (Y/N)", normalizeFirstLetter: true);
                    if (s == "Y") STK = 1;
                }
            }
            Console.WriteLine();
            EstablishTrueHikingPace();
        }

        void ReachEndOfTrail()
        {
            Console.WriteLine("You reached the end of the trail at Baxter Peak on Mt. Katahdin!");
            for (int j = 0; j < 3; j++) ShortPause();
            Console.Clear();
            for (int j = 0; j < 3; j++)
            {
                CenterPrint("CONGRATULATIONS!");
                System.Threading.Thread.Sleep(400);
                Console.Clear();
            }
            D = 2007;
            EndGameSummary();
        }

        void EndGameSummary(bool aborted = false)
        {
            TD = (int)Math.Floor(T + 0.5);
            D = Math.Floor(D);
            double X = Math.Round(10 * D / TD) / 10.0;
            Console.WriteLine();
            Console.Write("It is now ");
            PrintDateIfNeeded(force: true);
            Console.WriteLine(" and you have been on the");
            Console.WriteLine($"trail for {TD} days. You have covered {D} miles. Your average");
            Console.WriteLine($"speed, considering all the delays, was {X:0.0} miles per day.");
            double WBend = Math.Round(WB, MidpointRounding.AwayFromZero);
            double WL = Math.Round(Math.Abs(WTLOST), MidpointRounding.AwayFromZero);
            string moreLess = WTLOST > 0 ? "less" : "more";
            Console.WriteLine($"You weighed {WBend} pounds at the end, {WL} {moreLess} than at the start.");
            Console.WriteLine(aborted ? "Tough break this year. Better luck next time!" : "Nice going!");

            var again = Prompt("Would you like to try again (Y or N)", normalizeFirstLetter: true);
            if (again == "Y")
            {
                // Restart cleanly
                new Game().Run();
            }
            else
            {
                Console.Clear();
                Environment.Exit(0);
            }
        }
        #endregion

        #region Utilities
        static void CenterPrint(string s)
        {
            if (string.IsNullOrEmpty(s)) { Console.WriteLine(); return; }
            int width = 70;
            int pad = Math.Max(0, (width - s.Length) / 2);
            Console.WriteLine(new string(' ', pad) + s);
        }

        static void WaitForKey()
        {
            Console.ReadKey(true);
        }

        static void Pause(string message = "Press any key to continue")
        {
            Console.WriteLine();
            CenterPrint(message);
            WaitForKey();
            Console.WriteLine();
        }

        static void ShortPause()
        {
            // Rough stand‑in for FOR-loop delay
            System.Threading.Thread.Sleep(350);
        }

        static void PrintLines(params string[] lines)
        {
            foreach (var l in lines) Console.WriteLine(l);
        }

        static void PrintWeightOz(int weightOz)
        {
            int lb = weightOz / 16;
            int oz = weightOz % 16;

            if (lb > 1) Console.Write($"{lb} pounds");
            else if (lb == 1) Console.Write(" 1 pound");

            if (oz > 1) Console.Write($" {oz} ounces");
            else if (oz == 1) Console.Write(" 1 ounce");
        }

        string Prompt(string label, bool normalizeFirstLetter = false)
        {
            Console.Write($"{label}: ");
            var s = Console.ReadLine() ?? string.Empty;
            if (normalizeFirstLetter)
            {
                if (string.IsNullOrWhiteSpace(s)) s = "Y"; // BASIC default
                s = s.Trim();
                if (s.Length > 0) s = s.Substring(0, 1).ToUpperInvariant();
            }
            return s;
        }

        double PromptNumberDouble(string label, double min = double.NegativeInfinity, double max = double.PositiveInfinity, string? retryMsg = null, Func<double, string>? outOfRangeMsg = null)
        {
            while (true)
            {
                Console.Write($"{label}: ");
                var s = Console.ReadLine();
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                {
                    if (v < min || v > max)
                    {
                        if (outOfRangeMsg != null) Console.WriteLine(outOfRangeMsg(v));
                        else if (!string.IsNullOrEmpty(retryMsg)) Console.WriteLine(retryMsg);
                        else Console.WriteLine("Out of range. Try again.");
                        continue;
                    }
                    return v;
                }
                Console.WriteLine("Please enter a valid number.");
                if (!string.IsNullOrEmpty(retryMsg)) Console.WriteLine(retryMsg);
            }
        }

        void PrintDateIfNeeded(bool force = false)
        {
            if (!force && TT == TD) return;

            string mo;
            int md;
            if (TD < 31) { mo = "April"; md = TD; }
            else if (TD < 62) { mo = "May"; md = TD - 30; }
            else if (TD < 90) { mo = "June"; md = TD - 61; }
            else if (TD < 121) { mo = "July"; md = TD - 89; }
            else if (TD < 152) { mo = "August"; md = TD - 120; }
            else if (TD < 182) { mo = "September"; md = TD - 151; }
            else if (TD < 213) { mo = "October"; md = TD - 181; }
            else if (TD < 225) { mo = "November"; md = TD - 212; }
            else
            {
                Console.WriteLine();
                Console.WriteLine("It's November 12 and all the New England states are covered with snow.");
                Console.WriteLine("You have no chance of finishing the trail. Better luck next year.");
                TD = (int)Math.Floor(T + .5);
                D = Math.Floor(D);
                double X = Math.Round(10 * D / TD) / 10.0;
                Console.WriteLine($"You have been out on the trail {TD} days and covered {D} miles. Avg {X:0.0} mi/day.");
                EndGameSummary(aborted: true);
                return;
            }
            Console.WriteLine($"{mo} {md}");
            TT = TD;
        }
        #endregion
    }
}
