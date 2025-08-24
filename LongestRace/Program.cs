using System;
using System.Globalization;
using System.Threading;

namespace LongestAutoRace1908
{
    internal static class Program
    {
        static readonly Random rng = new Random();

        // DATA ARRAYS (1-based indexing, allocate one extra)
        static string[] LA = new string[21];   // City
        static string[] LB = new string[21];   // State/Country
        static int[] TX = new int[21];         // Expected days to next location
        static int[] C = new int[21];          // Road condition index
        static int[] WX = new int[21];         // Expected weather index
        static int[] DX = new int[21];         // Miles to next location

        // breakdown tables
        static string[] FA = new string[21];   // malfunction name
        static string[] FB = new string[21];   // fix option 1 description
        static string[] FC = new string[21];   // fix option 2 description (may be empty)
        static int[,] FT = new int[3, 21];     // time to fix option1/2 (hours unless >=8-> days)
        static int[,] FL = new int[3, 21];     // cost to fix option1/2

        // vocab
        static string[] Roads = new string[7];
        static string[] Weather = new string[7];

        // STATE (float/double mirrors BASIC)
        static double Z = 1000;   // money
        static double GF = 0.25;  // “base” gas price (used to compute station price)
        static int J = 0;         // segment index (location index)
        static int TD = 0;        // total days since start
        static int TL = 0;        // elapsed for race leader
        static int TE = 0;        // expected days to next location (for leader)
        static int W = 0;         // weather code this segment
        static int D = 0;         // target miles this segment
        static double DC = 0;     // cumulative miles driven
        static double DA = 0;     // miles driven this segment
        static int JV = 0;        // ocean-voyage flag (in port/at sea)
        static int TT = -1;       // last printed date-day
        static double SP = 0;     // chosen speed
        static double HP = 0;     // driving hours per day
        static double PB = 0;     // breakdown probability factor
        static double PF = 0;     // fatigue probability factor
        static double PW = 1;     // weather/day speed factor
        static double GM = 0;     // gas used today
        static double GG = 0;     // gallons purchased for segment
        static double GP = 0;     // price per gallon this location (in dollars)
        static int TZ = 0;        // temporary “days to add”
        static int K = 0;         // ask-once flag for “pushing too hard”
        static double HC = 0;     // cumulative hours to detect pushing too hard
        static int FX = 0;        // unfixed breakdown id (0 = none)

        // telegraph money state
        static int ZB = 0;        // number of requests
        static int ZW = 0;        // amount wired (1000 then 500)

        // yes/no helper (BASIC A$ -> Ans)
        static int A = 0;         // 0 yes / 1 no
        static string Ans = "";

        static void Main()
        {
            Console.Clear();
            Center("The Longest Automobile Race, 1908");
            Console.WriteLine();
            Center("(c) David H. Ahl, 1986");
            Console.WriteLine();

            // init
            InitVocab();
            InitLocations();
            InitBreakdowns();
            ShowInstructions();

            // main loop per segment/location
            while (true)
            {
                J++;
                int T = 0; // (unused but kept for parity)
                PauseShort();
                BeepWarn();

                // set vars for this location
                DA = 0;
                W = WX[J];
                D = DX[J];
                TE = TX[J];

                PrintDate();
                Console.WriteLine($"You are at {LA[J]}, {LB[J]}.");
                Console.Write("You currently have ");
                Console.WriteLine(Z.ToString("$#0.00", CultureInfo.InvariantCulture));

                if (J > 1)
                {
                    if (FX != 0)
                    {
                        Console.WriteLine($"A sympathetic garage owner will fix the {FA[FX]} here.");
                        FX = 0;
                        TZ = 1 + rng.Next(1, 4);
                        Console.WriteLine($"It will take {TZ} day(s).");
                        TimeDelayAndHotelBills();
                    }
                }

                // ocean legs (pre-land)
                if (J > 7 && J < 11) OceanVoyage();

                if (JV == 1)
                {
                    JV = 0;
                    TL += TE;
                    J--; // re-run this leg after voyage finished
                    continue;
                }

                Console.WriteLine();
                Console.WriteLine($"You have driven {Math.Floor(DC)} miles in {TD} days.");

                if (J == 20) { FinishedParis(); return; }

                if (TD < TL) Console.WriteLine($"You are the race leader and are {TL - TD} day(s) ahead.");
                else if (TD == TL) Console.WriteLine("You and the Italian Zust are running even with each other.");
                else Console.WriteLine($"The race leader passed this point {TD - TL} day(s) ago.");

                TL += TE;

                // ocean voyage after land segment (J=7 or 12)
                if (J == 7 || J == 12) OceanVoyage();
                if (JV == 1)
                {
                    JV = 0;
                    J--; // re-run leg after voyage timing
                    continue;
                }

                Console.WriteLine($"Roads to the west of here are {Roads[C[J]]}.");
                Console.WriteLine($"The weather forecast is {Weather[W]}.");
                Console.WriteLine($"You set a goal of making {D} miles in the next {TE - 2} days.");

                FuelAndOil();
                AskSpeed();
                AskHours();

                // daily run within the segment until goal reached
                while (true)
                {
                    TZ = 1; TimeDelayAndHotelBills(); // a day passes
                    WeatherDay();
                    Breakdown();
                    AccidentOrFatigue();

                    double DD = SP * HP * PW; // miles today
                    DA += DD; DC += DD;

                    GM = 0.07 * DD * (0.8 + 0.4 * rng.NextDouble());
                    if (GM < GG)
                    {
                        GG -= GM;
                    }
                    else
                    {
                        // ran out of gas
                        Beep3();
                        PrintDate();
                        Console.WriteLine("You ran out of gas on the road.");
                        GF = 0.33; // emergency price anchor
                        FuelAndOil(); // buy
                        GG -= GM; // now subtract the consumed portion
                    }

                    if (DA >= D) break; // completed this travel segment
                }
            }
        }

        // ------------------------------------------------------------
        // INSTRUCTIONS
        static void ShowInstructions()
        {
            Console.Clear();
            Center("The Longest Automobile Race, 1908");
            Console.WriteLine();
            Console.WriteLine(" In this program, you are the captain of the Thomas Flyer team.");
            Console.WriteLine("It is your job to get the car from New York to Paris—east to west—");
            Console.WriteLine("as quickly as possible. The race starts on February 12, 1908.");
            Console.WriteLine(" You must overcome many problems: bad weather, accidents,");
            Console.WriteLine("mechanical breakdowns, fatigue, and a lack of gas stations.");
            Console.WriteLine(" For each leg of the trip, buy as much gas as you need, but no");
            Console.WriteLine("more. Your car gets approximately 14 mpg (varies with conditions).");
            Console.WriteLine("You will carry what fuel you can and ship the rest ahead by rail");
            Console.WriteLine("to locations along your route to be held for you (‘spotting’).");
            Console.WriteLine(" Your car has a top speed of 54 mph. However, the probability");
            Console.WriteLine("of a breakdown increases substantially at speeds over 35 mph. Likewise,");
            Console.WriteLine("driving more than six hours per day increases your chance of");
            Console.WriteLine("having an accident. But don't forget, this IS a race.");
            Console.WriteLine(" If you get stuck, you can pay someone to pull you out (costs money)");
            Console.WriteLine("or try to get out on your own (costs time).");
            Console.WriteLine(" You can choose to repair a mechanical problem on the spot or");
            Console.WriteLine("wait until the next large town to get it fixed. Either choice has risks.");
            Console.WriteLine(" If/when you run out of money, you can wire Mr. Thomas for more—");
            Console.WriteLine("but your telegram must be POLITE and in ALL UPPER CASE letters.");
            Console.WriteLine();
            Console.Write("Press any key to continue…");
            Console.ReadKey(true);
            Console.Clear();
        }

        // ------------------------------------------------------------
        // DATE
        static void PrintDate()
        {
            if (TT == TD) return;

            string mo; int md;
            if (TD < 19) { mo = "February"; md = TD + 11; }
            else if (TD < 50) { mo = "March"; md = TD - 18; }
            else if (TD < 80) { mo = "April"; md = TD - 49; }
            else if (TD < 111) { mo = "May"; md = TD - 79; }
            else if (TD < 141) { mo = "June"; md = TD - 110; }
            else if (TD < 172) { mo = "July"; md = TD - 140; }
            else if (TD < 203) { mo = "August"; md = TD - 171; }
            else
            {
                BeepWarn();
                Console.WriteLine();
                Console.WriteLine("It's September 1 and the winning car crossed the finish");
                Console.WriteLine("line in Paris over a month ago. Your factory refuses to give");
                Console.WriteLine("you any more money to continue. Better luck next time.");
                EndSummary(false);
                return;
            }
            Console.WriteLine();
            Console.WriteLine($"Date: {mo} {md}, 1908");
            TT = TD;
        }

        // ------------------------------------------------------------
        // FUEL & OIL
        static void FuelAndOil()
        {
            GP = GF * (0.7 + 0.6 * rng.NextDouble()); // price anchor +/- 30%
            Console.WriteLine($"Gas costs {Math.Floor(100 * GP)} cents per gallon here.");
            GG = AskDouble("How many gallons do you want for the segment ahead");
            GF = 0.25; // reset anchor

            double ZN = GG * GP;
            Console.Write("That will cost ");
            Console.WriteLine(ZN.ToString("$#0.00", CultureInfo.InvariantCulture));

            if (!PayBills(ZN))
            {
                if (Z < 2)
                {
                    Console.WriteLine("Your car won't run on fumes. It's all over.");
                    EndSummary(false);
                }
                GG = (int)(Z / GP);
                Console.WriteLine($"Sorry, you could only get {GG} gallons.");
                Z -= GG * GP;
            }
        }

        // ------------------------------------------------------------
        // SPEED
        static void AskSpeed()
        {
            while (true)
            {
                SP = AskDouble("How fast (mph) do you want to drive");
                if (SP > 54) { Console.WriteLine("Top speed of your car is only 54 mph."); continue; }
                if (SP < 8) { Console.WriteLine("At that rate, you'll never get there."); continue; }

                if (W < 3 && SP > 30)
                {
                    Console.WriteLine("That's too fast for these weather and road conditions.");
                    continue;
                }
                PB = (SP * SP) / 7000.0; // breakdown chance factor
                return;
            }
        }

        // ------------------------------------------------------------
        // HOURS
        static void AskHours()
        {
            K = 0;
            while (true)
            {
                HP = AskDouble("How many hours do you want to drive each day");
                if (K == 0)
                {
                    if (HP > 8) { Console.WriteLine("That's too much for both you and your car."); continue; }
                    if (HP < 2) { Console.WriteLine("No one is that lazy!"); continue; }
                    HC += HP;
                    if (J > 2 && (HC / J) > 7.55)
                    {
                        Console.WriteLine("You've been pushing yourself and your crew pretty hard.");
                        Console.WriteLine("You should probably back off a bit.");
                        K = 1;
                        continue; // asks again, then we allow
                    }
                }
                PF = (HP * HP * HP) / 1000.0 - 0.15;
                if (PF < 0.01) PF = 0.01;
                return;
            }
        }

        // ------------------------------------------------------------
        // WEATHER (per-day)
        static void WeatherDay()
        {
            switch (W)
            {
                case 1: // blizzard
                {
                    double rn = rng.NextDouble();
                    if (rn < .33)
                    {
                        PrintDate();
                        PW = .03 + .08 * rng.NextDouble();
                        Console.WriteLine("Blizzard conditions. Tough going today.");
                        PauseShort();
                    }
                    else if (rn > .83)
                    {
                        PrintDate();
                        PW = .05 + .1 * rng.NextDouble();
                        Console.WriteLine("You're stuck in a huge snow drift.");
                        PullYouOutOfDitch();
                    }
                    else
                    {
                        PW = .14 + .17 * rng.NextDouble();
                    }
                    break;
                }
                case 2: // snow & sleet
                {
                    if (rng.NextDouble() < .1)
                    {
                        PW = .15 + .1 * rng.NextDouble();
                        PrintDate();
                        Console.WriteLine("You have skidded into a ditch.");
                        PullYouOutOfDitch();
                    }
                    else
                    {
                        PW = .3 + .4 * rng.NextDouble();
                    }
                    break;
                }
                case 3: // rain
                {
                    if (rng.NextDouble() < .2)
                    {
                        PrintDate();
                        PW = .02 + .04 * rng.NextDouble();
                        Console.WriteLine("You are totally bogged down in the mud.");
                        PullYouOutOfDitch();
                    }
                    else
                    {
                        PW = .35 + .4 * rng.NextDouble();
                    }
                    break;
                }
                case 4: // cloudy chance of rain
                case 5: // mixed
                {
                    double rn = rng.NextDouble();
                    if (rn > .08)
                    {
                        PW = .4 + .4 * rng.NextDouble();
                    }
                    else
                    {
                        PrintDate();
                        if (rn < .01)
                        {
                            Console.WriteLine("An unexpected downpour!");
                            PW = .02 + .04 * rng.NextDouble();
                            Console.WriteLine("You are totally bogged down in the mud.");
                            PullYouOutOfDitch();
                        }
                        else
                        {
                            RiverNoBridge();
                        }
                    }
                    break;
                }
                case 6: // sunny
                {
                    if (rng.NextDouble() < .025)
                    {
                        PrintDate();
                        RiverNoBridge();
                    }
                    else
                    {
                        PW = .45 + .5 * rng.NextDouble();
                    }
                    break;
                }
            }
        }

        // ------------------------------------------------------------
        // River without bridge (boat or detour)
        static void RiverNoBridge()
        {
            Console.WriteLine("River ahead with no bridge. Some locals tell you there is a bridge");
            Console.WriteLine("'some distance' north. They also offer to take you across by boat");
            int cost = 3 + 2 * rng.Next(0, 3);
            Console.Write($"for ${cost}. Want to go by boat ");
            Ans = Console.ReadLine() ?? "";
            ParseYesNo();
            if (A == 1) // no
            {
                TZ = 1 + rng.Next(1, 3);
                Console.WriteLine($"It took {TZ} day(s) for you to drive north and find the bridge.");
                TimeDelayAndHotelBills();
                return;
            }
            if (!PayBills(cost))
            {
                TZ = 1 + rng.Next(1, 3);
                Console.WriteLine($"It took {TZ} day(s) for you to drive north and find the bridge.");
                TimeDelayAndHotelBills();
                return;
            }
            int hours = 2 + rng.Next(0, 3);
            Console.WriteLine($"They got you across in {hours} hours.");
            PW = .3; // rest of day slow
        }

        // ------------------------------------------------------------
        // Pull out of ditch/mud
        static void PullYouOutOfDitch()
        {
            int ZN = 5 * rng.Next(1, 5); // 5,10,15,20
            Console.WriteLine($"A farmer offers to pull you out for ${ZN}");
            Console.Write("Do you want to pay him to pull you out ");
            Ans = Console.ReadLine() ?? "";
            ParseYesNo();
            if (A == 1) // no
            {
                TZ = 1 + (int)Math.Round(1.3 * rng.NextDouble());
                Console.WriteLine($"It took {TZ} day(s) for you and your mechanic");
                Console.WriteLine("to pull the car out by yourselves.");
                TimeDelayAndHotelBills();
                PW = PW * 1.5;
                return;
            }
            if (!PayBills(ZN))
            {
                TZ = 1 + (int)Math.Round(1.3 * rng.NextDouble());
                Console.WriteLine($"It took {TZ} day(s) for you and your mechanic");
                Console.WriteLine("to pull the car out by yourselves.");
                TimeDelayAndHotelBills();
                PW = PW * 1.5;
                return;
            }
            int RQ = 1 + rng.Next(1, 6); // 2–6 hours approx
            Console.WriteLine($"It took {RQ} hours for him to pull you out.");
            if (RQ >= 5)
            {
                TZ = 1; TimeDelayAndHotelBills();
                PW *= 1.5;
            }
        }

        // ------------------------------------------------------------
        // Breakdown handling
        static void Breakdown()
        {
            if (rng.NextDouble() > PB) return;

            int F = 1 + rng.Next(1, 16);
            if (F > 13) F = 14 + rng.Next(0, 5);

            Beep3();
            PrintDate();
            Console.WriteLine($"Uh oh. You have a problem. It's a {FA[F]}.");
            Console.WriteLine("Here's what you can do about the problem:");
            Console.WriteLine("       (1) Try to keep going with it");
            Console.WriteLine($"       (2) {FB[F]}, cost ${FL[1, F]}");
            if (!string.IsNullOrWhiteSpace(FC[F]))
                Console.WriteLine($"       (3) {FC[F]}, cost ${FL[2, F]}");

            int maxOpt = string.IsNullOrWhiteSpace(FC[F]) ? 2 : 3;
            int choice = AskInt("Which would you like to do", 1, maxOpt);

            if (choice == 1)
            {
                Console.WriteLine("You try to nurse the car along to the next major city.");
                if (FX != 0)
                {
                    Console.WriteLine("But with the other problem you just can't make it and");
                    Console.WriteLine("reluctantly you admit defeat.");
                    EndSummary(false);
                    return;
                }

                if (rng.NextDouble() > .4)
                {
                    Console.WriteLine("It looks like you'll make but at a drastically reduced speed.");
                    PW *= .5; FX = F;
                    return;
                }
                PauseShort();
                Console.WriteLine();
                Console.WriteLine("Unfortunately, it just won't make it and");
                Console.WriteLine("reluctantly you admit defeat.");
                EndSummary(false);
                return;
            }

            int opt = choice - 1;
            int time = FT[opt, F];
            string unit = "hours";
            if (time >= 8) { unit = "day"; if (time / 8 != 1) unit = "days"; TZ = time / 8; }
            else
            {
                if (time >= 5) { TZ = 1; }
                PW *= 1.5;
            }
            if (TZ > 0)
            {
                TimeDelayAndHotelBills();
            }

            double cost = FL[opt, F];
            Console.WriteLine($"Repairs will take {(TZ > 0 ? TZ : time)} {(TZ > 0 ? "day" + (TZ == 1 ? "" : "s") : unit)} and will cost ${cost}");
            PayBills(cost); // if this fails later, travel still continues (as in BASIC flow)
        }

        // ------------------------------------------------------------
        // Accident or fatigue
        static void AccidentOrFatigue()
        {
            if (rng.NextDouble() <= PF)
            {
                Beep4();
                PrintDate();
                Console.Write("You dozed off and your car has run ");
                int type = 1 + rng.Next(1, 5);
                int tz; int zn;
                switch (type)
                {
                    case 1: Console.WriteLine("into a tree."); tz = 2; zn = 24; break;
                    case 2: Console.WriteLine("off the road."); tz = 1; zn = 12; break;
                    case 3: Console.WriteLine("into a gaping hole."); tz = 1; zn = 18; break;
                    default: Console.WriteLine("into a farmer's wagon."); tz = 2; zn = 25; break;
                }
                Console.WriteLine("You can try to fix it or get a tow to the next village for $15.");
                Console.Write("Want to try to bang out the damage on the spot ");
                Ans = Console.ReadLine() ?? "";
                ParseYesNo();
                if (A == 0)
                {
                    Console.WriteLine($"You finally manage to do it but it takes {tz} day(s).");
                    PW *= 1.5;
                    TZ = tz; TimeDelayAndHotelBills();
                }
                else
                {
                    Console.WriteLine($"The tow costs $15 and the repairs cost ${zn}");
                    if (!PayBills(15 + zn))
                    {
                        Console.WriteLine("The locals impound your car for your unpaid debt.");
                        EndSummary(false);
                        return;
                    }
                }
            }

            // railroad ties
            if (J == 2 || J == 5 || J == 13 || J == 14)
            {
                if (rng.NextDouble() <= .4)
                {
                    PrintDate();
                    Console.WriteLine("In this area of terrible roads, you can save some time by driving");
                    Console.WriteLine("on the railraod tracks. However, it is murder on your wheels,");
                    Console.WriteLine("tires, and whole car. ");
                    Console.Write("Want to drive on the tracks ");
                    Ans = Console.ReadLine() ?? "";
                    ParseYesNo();
                    if (A == 0)
                    {
                        PW *= 1.7; PB *= 1.25;
                    }
                }
            }

            // no grease routine (central Russia J = 15 or 16)
            if (J == 15 || J == 16)
            {
                if (rng.NextDouble() <= .2)
                {
                    Beep3();
                    PrintDate();
                    Console.WriteLine("Your differential is dry and there is");
                    Console.WriteLine("no grease available here. However, you can get Vaseline.");
                    Console.Write("Want to use it in place of grease ");
                    Ans = Console.ReadLine() ?? "";
                    ParseYesNo();
                    if (A == 0)
                    {
                        Console.WriteLine("Okay, you buy 20 jars for $4.");
                        Z -= 4;
                    }
                    else
                    {
                        Console.WriteLine("The gears sound horrible. You'll have to cut your speed in half.");
                        PW *= .5;
                    }
                }
            }
        }

        // ------------------------------------------------------------
        // Ocean voyage segments
        static void OceanVoyage()
        {
            JV = 1;
            TZ = 1 + (int)Math.Round(3.5 * rng.NextDouble()); // days stuck in port
            if (J == 12)
            {
                Console.WriteLine("The freighter across the Pacific takes a leisurely 21 days making");
                Console.WriteLine("stops at Hawaii, Guam, and the Philippines. Also the Chinese");
                Console.WriteLine("crewmen made sandals out of your leather fenders and mud flaps.");
                Console.WriteLine("You can't replace them in Japan, but you can at Vladivostock,");
                Console.WriteLine("Russia. There you'll have to spend several days arranging for");
                Console.WriteLine("fuel also. But hurry now. A steamer to Russia leaves tonight.");
                ReadyToGo();
                TD += 7; // overnight leave
                return;
            }
            else if (J == 10)
            {
                Console.WriteLine("It took 7 days to get back to Seattle. Now you have a " + TZ + " day");
                Console.WriteLine("wait before you can get a freighter for Japan.");
                TimeDelayAndHotelBills();
                ReadyToGo();
                TD += 21; // 3 weeks at sea to Japan
                return;
            }
            else if (J == 9)
            {
                Console.WriteLine("The steamer made many stops up the coast and it took 7 days.");
                Console.WriteLine("It is apparent that the race organizers have never been in Alaska");
                Console.WriteLine("and have no idea that it is impossible to drive on the snow and");
                Console.WriteLine("ice at all, much less across the Bering Strait to Russia. You'll");
                Console.WriteLine("have to return to Seattle. Next steamer goes in " + TZ + " days.");
                TimeDelayAndHotelBills();
                ReadyToGo();
                TD += 7; // 7 back
                return;
            }
            else if (J == 8)
            {
                Console.WriteLine("It took 3 days on the steamer. The next steamer for Valdez");
                Console.WriteLine("Leaves in " + TZ + " days. Nothing to do but wait.");
                TimeDelayAndHotelBills();
                ReadyToGo();
                TD += 7;
                return;
            }
            else
            {
                Console.WriteLine($"You're stuck in port for {TZ + 1} days before you can get a steamer");
                Console.WriteLine("for Seattle. You use the time to get new countershaft");
                Console.WriteLine("housings, springs, wheels, drive chains, and tires.");
                if (Z > 300)
                {
                    Console.WriteLine("The cost of these items is $164.");
                    Z -= 164;
                }
                else
                {
                    Console.WriteLine("These were all furnished by the local Thomas Flyer dealer.");
                }
                TZ = TZ + 1; TD += 3; TimeDelayAndHotelBills(); ReadyToGo();
                return;
            }
        }

        // ------------------------------------------------------------
        // PAY / MONEY
        static bool PayBills(double amount)
        {
            if (Z >= amount) { Z -= amount; A = 0; return true; }

            NeedMoreMoney(amount);
            if (Z >= amount) { Z -= amount; A = 0; return true; }

            A = 1; return false;
        }

        static void NeedMoreMoney(double amount)
        {
            ZB++;
            ZW = ZB < 3 ? 1000 : 500;
            Console.WriteLine();
            Console.WriteLine("You don't have enough money to continue. Your only hope is");
            Console.WriteLine("to send a telegram back to Mr. Thomas at the factory and ask");
            Console.WriteLine("for more money. Remember, telegrams in 1908 used all capital");
            Console.WriteLine("letters, had no commas, and were short.");
            Console.Write("What is your message ");
            string msg = Console.ReadLine() ?? "";
            Console.WriteLine("Sending telegram now…");
            Telegraph();

            if (ZB > 3)
            {
                Console.WriteLine("Mr. Thomas wires back: I AM FED UP WITH THIS ADVENTURE STOP");
                Console.WriteLine("YOU WILL GET NO MORE MONEY FROM ME STOP");
                return;
            }

            bool polite = false, urgent = false;
            string M = msg.ToUpperInvariant();
            if (M.Length >= 12)
            {
                for (int i = 0; i <= M.Length - 3; i++)
                {
                    string trio = M.Substring(i, 3);
                    if (trio == "PLE" || trio == "BEG" || trio == "SOR" || trio == "IMP") polite = true;
                    if (trio == "SOO" || trio == "QUI" || trio == "EAR" || trio == "FAS" || trio == "HUR") urgent = true;
                    if (trio == "IMM" || trio == "ONC" || trio == "URG") urgent = true;
                }
            }

            if (polite && urgent)
            {
                Console.WriteLine($"Mr. Thomas wired back $ {ZW} and said 'GOOD LUCK!'");
                Z += ZW; return;
            }
            if (polite && !urgent)
            {
                Console.WriteLine("Mr. Thomas didn't know you needed the money right away and waited");
                Console.WriteLine($"3 days before wiring back ${ZW}");
                Z += ZW; TZ = 3; TimeDelayAndHotelBills(); return;
            }
            if (!polite && urgent)
            {
                Console.WriteLine("Mr. Thomas wired back, 'YOU COULD AT LEAST BE POLITE,' but did");
                ZW /= 2;
                Console.WriteLine($"include a draft for ${ZW}");
                Z += ZW; return;
            }

            if (M.Length < 12)
            {
                Console.WriteLine("Your message was short all right. Too short. Mr. Thomas didn't");
                Console.WriteLine("send any money. Sorry.");
                return;
            }
            Console.WriteLine("Mr. Thomas was offended by your telegram and refused to");
            Console.WriteLine("send any money. Sorry.");
        }

        // ------------------------------------------------------------
        // TIME DELAY / HOTEL BILLS
        static void TimeDelayAndHotelBills()
        {
            int days = TZ;
            if (days <= 0) return;
            double bill = 10 * days;
            TD += days;
            TZ = 0;
            if (!PayBills(bill))
            {
                Console.WriteLine();
                Console.WriteLine("You don't even have enough money to pay for meals.");
                Console.WriteLine("That's the end of the road for you.");
                EndSummary(false);
            }
        }

        // ------------------------------------------------------------
        // FINISH / END
        static void FinishedParis()
        {
            for (int k = 0; k < 3; k++)
            {
                PauseShort(); Beep3();
            }
            Console.Clear();
            string x = "";
            for (int i = 0; i < 30; i++)
            {
                for (int k = 0; k < 100; k++) { }
                Console.SetCursorPosition(10, 10);
                Console.Write(x);
                Console.Beep(880, 60);
                x = (i % 2 == 0) ? "CONGRATULATIONS!" : new string(' ', "CONGRATULATIONS!".Length);
            }
            Console.WriteLine();
            Console.WriteLine();

            if (TD < TL)
            {
                Console.WriteLine("You reached Paris first! The next car is " + (TL - TD) + " days behind.");
            }
            else if (TD == TL)
            {
                Console.WriteLine("You reached Paris in a dead tie with the French Motobloc!");
            }
            else
            {
                Console.WriteLine("You made it to Paris! The German Protos beat you by");
                Console.WriteLine($"{TD - TL} days but just to finish is a great honor!");
            }
            Console.WriteLine();
            Console.WriteLine($"You reached Paris in {TD} days. In 1908, the Thomas Flyer");
            Console.WriteLine("won the race reaching Paris on July 30 after 169 days.");
            PlayAgain();
        }

        static void EndSummary(bool won)
        {
            Console.WriteLine();
            BeepWarn();
            Console.WriteLine();
            if (!won)
            {
                Console.WriteLine("Sorry you were unsuccessful. Only three of the");
                Console.WriteLine("cars in the 1908 race ever finished.");
                Console.WriteLine();
                Console.WriteLine($"In the {TD} days since the start of the race on February 12, 1908,");
                Console.WriteLine($"you covered {Math.Floor(DC)} miles. You almost made it to {LA[J + 1]}, {LB[J + 1]}. ");
                Console.WriteLine("Not bad, but you can do better.");
            }
            PlayAgain();
        }

        static void PlayAgain()
        {
            Console.WriteLine();
            Console.Write("Would you like to try again (Y or N) ");
            Ans = Console.ReadLine() ?? "";
            ParseYesNo();
            if (A == 0)
            {
                Console.WriteLine("Okay. Good Luck!");
                PauseShort();
                ResetAll();
                Console.Clear();
                Main();
            }
            else
            {
                Console.WriteLine("Okay. So long for now.");
                PauseShort();
                Environment.Exit(0);
            }
        }

        static void ResetAll()
        {
            Z = 1000; GF = 0.25;
            J = 0; TD = 0; TL = 0; TE = 0; W = 0; D = 0;
            DC = 0; DA = 0; JV = 0; TT = -1; SP = 0; HP = 0; PB = 0; PF = 0; PW = 1; GM = 0; GG = 0; GP = 0; TZ = 0; K = 0; HC = 0; FX = 0; ZB = 0; ZW = 0; A = 0; Ans = "";
        }

        // ------------------------------------------------------------
        // UTILS
        static void ParseYesNo()
        {
            if (string.IsNullOrWhiteSpace(Ans) || Ans == "Y" || Ans == "y") { A = 0; return; }
            if (Ans == "N" || Ans == "n") { A = 1; return; }
            Console.WriteLine($"Don't understand your answer of {Ans}.");
            Console.Write("Please enter Y for 'yes' or N for 'no.' ");
            Ans = Console.ReadLine() ?? "";
            ParseYesNo();
        }

        static int AskInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write($"{prompt} ");
                string s = Console.ReadLine() ?? "";
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) && v >= min && v <= max)
                    return v;
                Console.WriteLine("Please enter a valid number.");
            }
        }

        static double AskDouble(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} ");
                string s = Console.ReadLine() ?? "";
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v >= 0)
                    return Math.Floor(v);
                Console.WriteLine("Please enter a non-negative number.");
            }
        }

        static void Telegraph()
        {
            for (int i = 0; i < 4; i++)
            {
                int x = 1 + rng.Next(1, 4);
                for (int k = 0; k < x; k++) Console.Beep(900, 40);
                Thread.Sleep(180);
            }
        }

        static void PauseShort() => Thread.Sleep(250);
        static void BeepWarn()
        {
            Console.WriteLine();
            for (int i = 0; i < 3; i++) { Console.Beep(700, 60); Console.Beep(900, 60); Thread.Sleep(60); }
        }
        static void Beep3() { Console.Beep(700, 80); Console.Beep(700, 80); Console.Beep(700, 80); }
        static void Beep4() { Console.Beep(600, 80); Console.Beep(750, 80); Console.Beep(600, 80); Console.Beep(750, 80); }
        static void Center(string s)
        {
            int width = 70;
            int pad = Math.Max(0, (width - s.Length) / 2);
            Console.WriteLine(new string(' ', pad) + s);
        }

        static void ReadyToGo()
        {
            Console.WriteLine();
            Console.Write("Press any key when you're ready to go aboard.");
            Console.ReadKey(true);
        }

        // ------------------------------------------------------------
        // DATA LOAD
        static void InitVocab()
        {
            Roads[1] = "hard packed gravel";
            Roads[2] = "muddy ruts";
            Roads[3] = "slightly improved wagon tracks";
            Roads[4] = "built for narrow carts";
            Roads[5] = "practically non-existent";
            Roads[6] = "horrible";

            Weather[1] = "blizzard conditions";
            Weather[2] = "snow and sleet";
            Weather[3] = "rain";
            Weather[4] = "cloudy with a chance of rain";
            Weather[5] = "mixed";
            Weather[6] = "sunny and dry";
        }

        static void InitLocations()
        {
            (string city, string region, int wx, int cx, int tx, int dx)[] loc = new[]
            {
                ("New York","New York", 2,1, 8, 897),
                ("Kendallville","Indiana", 1,1, 6, 166),
                ("Chicago","Illinois", 3,2, 7, 634),
                ("Omaha","Nebraska", 6,3, 4, 482),
                ("Laramie","Wyoming", 2,3, 7, 467),
                ("Ogden","Utah", 6,1, 8, 1237),
                ("San Francisco","California", 5,7, 8, 0),
                ("Seattle","Washington", 5,7, 8, 0),
                ("Valdez","Alaska", 5,7, 8, 0),
                ("Seattle","Washington", 5,7, 25, 0),
                ("Kobe","Japan", 4,4, 4, 350),
                ("Tsuruga","Japan", 4,7, 7, 0),
                ("Vladivostock","Russia", 3,5, 15, 558),
                ("Tsitsihar","Manchuria", 5,6, 10, 659),
                ("Chita","Russia", 3,3, 8, 1116),
                ("Kansk","Russia", 4,3, 6, 1075), // fixed missing comma in listing
                ("Omsk","Russia", 5,1, 7, 820),
                ("Perm","Russia", 3,2, 14, 1090),
                ("St. Petersburg","Russia", 3,1, 8, 1575),
                ("Paris","France", 0,0, 0, 0)
            };
            for (int i = 1; i <= 20; i++)
            {
                LA[i] = loc[i - 1].city;
                LB[i] = loc[i - 1].region;
                WX[i] = loc[i - 1].wx;
                C[i] = loc[i - 1].cx;
                TX[i] = loc[i - 1].tx;
                DX[i] = loc[i - 1].dx;
            }
        }

        static void InitBreakdowns()
        {
            (string name, string fix1, string fix2, int ft1, int fl1, int ft2, int fl2)[] rows = new[]
            {
                ("tire blowout","Patch the hole","Replace the tire", 2, 1, 2, 7),
                ("skipping cylinder","New spark plugs","Grind cylinder", 1, 2, 8, 2),
                ("rough running engine","Do a tune up","", 4, 5, 0, 0),
                ("binding axle bearing","Regrind bearing","Get a new one", 8, 2, 4, 8),
                ("cracked spring","New spring","Weld angle iron to it", 8, 26, 8, 4),
                ("cracked wheel","New wheel","Weld brace on it", 2, 42, 8, 4),
                ("slipping clutch","Adjust clutch","New clutch plate", 4, 4, 8, 54),
                ("stripped gear","Weld teeth back on","New gear", 16, 6, 8, 24),
                ("radiator leak","Weld a patch on it","", 4, 2, 0, 0),
                ("brakes failure","Replace the linings","", 8, 7, 0, 0),
                ("crack in the countershaft housing","A new housing","", 24, 40, 0, 0),
                ("broken drive pinion","Weld teeth back on","New pinion", 16, 6, 8, 18),
                ("broken rear axle","Get a new axle","", 16, 68, 0, 0),
                ("cracked transmission housing","New one from factory","", 24, 60, 0, 0),
                ("broken motor support","Make a new one of scrap iron","", 16, 16, 0, 0),
                ("worn down clutch shaft","A new clutch shaft","", 8, 28, 0, 0),
                ("cracked frame","Weld on braces of angle iron","", 24, 26, 0, 0),
                ("total transmission failure","A new one from factory","", 40, 225, 0, 0),
            };
            for (int i = 1; i <= 18; i++)
            {
                var r = rows[i - 1];
                FA[i] = r.name;
                FB[i] = r.fix1;
                FC[i] = r.fix2;
                FT[1, i] = r.ft1; FL[1, i] = r.fl1;
                FT[2, i] = r.ft2; FL[2, i] = r.fl2;
            }
        }
    }
}
