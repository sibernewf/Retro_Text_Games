using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace WestwardHo1847
{
    internal static class Program
    {
        // --- RNG ---
        static readonly Random rng = new Random();

        // --- DATA ARRAYS ---
        static string[] DA = new string[21];      // dates (1..20)
        static int[] EP = new int[21];            // event thresholds (1..15)
        static int[] MP = new int[16];            // mileage posts (1..15)
        static string[] PL = new string[16];      // place strings (1..15)
        static string[] ShotWords = new string[9]; // 1..8

        // --- STATE ---
        // Money & resources (most as double to match BASIC arithmetic)
        static double T = 0;      // cash left
        static double F = 0;      // food ($ worth)
        static int B = 0;         // ammo (bullets)
        static double C = 0;      // clothing ($ worth)
        static double R = 0;      // medicine & parts ($ worth)

        // Oxen cost & current “miles”
        static double A = 0;      // $ spent on oxen (affects speed)
        static double M = 0;      // total miles so far (also adjusted +/- by events)
        static double MA = 0;     // M before this segment (for computing fraction at finish)

        // Trip control
        static int J = 0;         // 2-week segments (1..)
        static int E = 2;         // eating level (1..3) from last menu
        static int DR = 3;        // shooting rank (1..5), lower = better
        static int BR = 0;        // shooting response result (smaller = faster)
        static int KQ = 0;        // set to 1 when “not enough ammo to hunt”
        static int KS = 0;        // illness flag
        static int KH = 0;        // injury flag

        // Mountain flags
        static int KP = 0;        // South Pass seen/cleared flag
        static int KM = 0;        // Blue Mountains flag
        static int KB = 0;        // blizzard happened flag

        // misc flags
        static int C1 = 0;        // cold check in mountains
        static int MPflag = 0;    // prints the “950 miles” when set
        static double ML = 0;     // fraction of last segment when arriving

        static void Main()
        {
            Console.Clear();
            Center("Westward Ho! 1847");
            Console.WriteLine();
            Center("(c) David H. Ahl, 1986");
            Console.WriteLine();

            // init data
            ReadWordsAndDates();
            ReadPlaces();
            ReadEventThresholds();

            Console.WriteLine("Press any key when you're ready to go");
            Console.ReadKey(true);
            Console.Clear();

            PrintScenario();
            InitialPurchases();
            InitShooting();

            Console.WriteLine();
            Console.WriteLine(" Your trip is about to begin…");
            Console.WriteLine();
            Fanfare();

            // main loop: each iteration is 2 weeks
            while (true)
            {
                if (M > 2039) { MadeIt(); return; }
                J++;
                if (J > 20) { Died_OxenWornOut(); return; }

                Console.WriteLine();
                Console.Write($"Monday, {DA[J]}, 1847. You are ");
                // “You are {place}” based on distance M
                bool said = false;
                for (int i = 1; i <= 15; i++)
                {
                    if (M <= MP[i]) { Console.WriteLine(PL[i]); said = true; break; }
                }
                if (!said) Console.WriteLine("somewhere on the trail.");

                if (F < 6) Console.WriteLine("You're low on food. Better buy some or go hunting soon.");

                // any sickness or injuries?
                if (KS != 1 && KH != 1) goto SkipDoctor;
                T -= 10;
                if (T < 0) { CantAffordDoctor(); } else
                {
                    Console.Write("Doctor charged $10 for his services to treat your ");
                    Console.WriteLine(KS == 1 ? "illness." : "injuries.");
                    KS = 0; KH = 0;
                }
            SkipDoctor:

                M = Math.Floor(M); MA = M; // update cumulative milestone for arrival math

                if (MPflag == 1)
                {
                    Console.WriteLine("Total mileage to date is 950.");
                    MPflag = 0;
                }
                else
                {
                    Console.WriteLine($"Total mileage to date is {Math.Floor(M + 0.5)}");
                }

                // distance this segment
                M += 200 + (A - 110) / 2.5 + 10 * rng.NextDouble();

                Console.WriteLine("Here's what you now have (no. of bullets, $ worth of other items) :");
                PrintInventory();

                StopHuntOrPush();         // forts/hunting
                Eat();                    // consumption
                Riders();                 // riders — attack?
                HazardsAndEvents();       // hazards & events
                Mountains();              // mountains
            }
        }

        // ----------------------------------------------------------
        // Scenario
        static void PrintScenario()
        {
            Center("Westward Ho! 1847");
            Console.WriteLine();
            Console.WriteLine(" Your journey over the Oregon Trail takes place in 1847. Start.");
            Console.WriteLine("ing in Independence, Missouri, you plan to take your family of");
            Console.WriteLine("five over 2040 tough miles to Oregon City.");
            Console.WriteLine(" Having saved $420 for the trip, you bought a wagon for $70 and");
            Console.WriteLine("now have to purchase the following items :");
            Console.WriteLine();
            Console.WriteLine(" * Oxen (spending more will buy you a larger and better team which");
            Console.WriteLine("   will be faster so you'll be on the trail for less time)");
            Console.WriteLine(" * Food (you'll need ample food to keep up your strength and health)");
            Console.WriteLine(" * Ammunition ($1 buys a belt of 50 bullets. You'll need ammo for");
            Console.WriteLine("   hunting and for fighting off attacks by bandits and animals)");
            Console.WriteLine(" * Clothing (you'll need warm clothes, especially when you hit the");
            Console.WriteLine("   snow and freezing weather in the mountains)");
            Console.WriteLine(" * Other supplies (includes medicine, first-aid supplies, tools, and");
            Console.WriteLine("   wagon parts for unexpected emergencies)");
            Console.WriteLine();
            Console.WriteLine(" You can spend all your money at the start or save some to spend");
            Console.WriteLine("at forts along the way. However, items cost more at the forts. You");
            Console.WriteLine("can also hunt for food if you run low.");
            Console.WriteLine();
        }

        // ----------------------------------------------------------
        // Initial purchases
        static void InitialPurchases()
        {
            Console.WriteLine();
            A = AskInt("How much do you want to pay for a team of oxen", 100, int.MaxValue);
            if (A > 150)
            {
                Console.WriteLine($"You choose an honest dealer who tells you that ${A} is too much for");
                Console.WriteLine("a team of oxen. He charges you $150 and gives you $" + (A - 150) + " change.");
                A = 150;
            }

            F = AskInt("How much do you want to spend on food", 14, int.MaxValue);
            while (A + F > 300)
            {
                Console.WriteLine("You won't have any for ammo and clothes.");
                F = AskInt("How much do you want to spend on food", 14, int.MaxValue);
            }

            int ammoDollars = AskInt("How much do you want to spend on ammunition", 2, int.MaxValue);
            while (A + F + ammoDollars > 320)
            {
                Console.WriteLine("That won't leave any money for clothes.");
                ammoDollars = AskInt("How much do you want to spend on ammunition", 2, int.MaxValue);
            }

            C = AskInt("How much do you want to spend on clothes", 25, int.MaxValue);
            while (A + F + ammoDollars + C > 345)
            {
                Console.WriteLine("That leaves nothing for medicine.");
                C = AskInt("How much do you want to spend on clothes", 25, int.MaxValue);
            }

            R = AskInt("How much for medicine, bandages, repair parts, etc.", 5, int.MaxValue);
            while (A + F + ammoDollars + C + R > 350)
            {
                Console.WriteLine("You don't have that much money.");
                R = AskInt("How much for medicine, bandages, repair parts, etc.", 5, int.MaxValue);
            }

            T = 350 - A - F - ammoDollars - C - R;
            Console.WriteLine();
            Console.WriteLine($"You now have ${T} left.");

            // Convert ammo dollars -> bullets (50 per $ at start)
            B = 50 * ammoDollars;
        }

        // ----------------------------------------------------------
        // Shooting rank
        static void InitShooting()
        {
            Console.WriteLine();
            Console.WriteLine("Please rank your shooting (typing) ability as follows :");
            Console.WriteLine(" (1) Ace marksman (2) Good shot (3) Fair to middlin'");
            Console.WriteLine(" (4) Need more practice (5) Shaky knees");
            DR = AskInt("How do you rank yourself", 1, 5);
        }

        // ----------------------------------------------------------
        // Fort / Hunt / Push
        static void StopHuntOrPush()
        {
            if (J % 2 == 0)
            {
                int x = AskInt("Want to (1) stop at next fort, (2) hunt, or (3) push on", 1, 3);
                if (x == 3) return;
                if (x == 1) Fort(); else Hunt();
                if (KQ == 1) StopHuntOrPush(); // if "not enough ammo to hunt", reprompt
                return;
            }
            else
            {
                int x = AskInt("Would you like to (1) hunt or (2) continue on", 1, 2);
                if (x == 2) return;
                Hunt();
            }
        }

        static void Fort()
        {
            if (T <= 0)
            {
                Console.WriteLine("You sing with the folks there and get a good");
                Console.WriteLine("night's sleep, but you have no money to buy anything.");
                return;
            }

            Console.WriteLine("What would you like to spend on each of the following;");
            double p1 = AskDouble("Food");          // dollars of fort food
            double p2 = AskDouble("Ammunition");    // dollars of fort ammo
            double p3 = AskDouble("Clothing");
            double p4 = AskDouble("Medicine and supplies");
            double P = p1 + p2 + p3 + p4;

            // convert at “fort prices”: items are pricier than at start
            double addFood = 0.67 * p1;     // fewer food-$ per $ spent
            int addBullets = (int)(33 * p2); // 33 bullets per $ at forts
            double addClothes = 0.67 * p3;
            double addRepair = 0.67 * p4;

            Console.WriteLine($"The storekeeper tallies up your bill. It comes to ${P}");
            if (T >= P)
            {
                T -= P;
                F += addFood; B += addBullets; C += addClothes; R += addRepair;
                return;
            }
            Console.WriteLine("Uh, oh. That's more than you have. Better start over.");
            Fort();
        }

        static void Hunt()
        {
            KQ = 0;
            if (B <= 39)
            {
                Console.WriteLine("Tough luck. You don't have enough ammo to hunt.");
                KQ = 1; return;
            }

            M -= 45;
            ShootGun();

            if (BR <= 1)
            {
                Console.WriteLine("Right between the eyes…you got a big one!");
                F += 26 + 3 * rng.NextDouble();
                Console.WriteLine("Full bellies tonight!");
                B -= (int)(10 + 4 * rng.NextDouble());
                return;
            }

            // chance to miss entirely
            if (100 * rng.NextDouble() < 13 * BR)
            {
                Console.WriteLine("You missed completely…and your dinner got away.");
                return;
            }

            Console.WriteLine("Nice shot…right on target…good eatin' tonight!");
            F += 24 - 2 * BR;
            B -= 10 + 3 * BR;
        }

        // ----------------------------------------------------------
        // Eat
        static void Eat()
        {
            if (F < 5) { Died_Starvation(); return; }

            E = AskInt("Do you want to eat (1) poorly (2) moderately or (3) well", 1, 3);
            double need = 4 + 2.5 * E;
            F -= need;
            if (F >= 0) return;

            if (E == 1) return; // can’t be less than poorly; just allow negative check
            F += need; // revert
            Console.WriteLine("You don't have enough to eat that well.");
            Eat();
        }

        // ----------------------------------------------------------
        // Riders routine
        static void Riders()
        {
            // IF RND * 10 > ((M/100 - 4)^2 + 72)/((M/100 - 4)^2 + 12) - 1 THEN RETURN
            double top = Math.Pow(M / 100.0 - 4.0, 2.0) + 72.0;
            double bot = Math.Pow(M / 100.0 - 4.0, 2.0) + 12.0;
            double thresh = (top / bot) - 1.0;
            if (rng.NextDouble() * 10.0 > thresh) return;

            string notWord = ""; int GH = 0;
            if (rng.NextDouble() > 0.2) { notWord = "don't "; GH = 1; }

            Console.WriteLine();
            Console.WriteLine($"Riders ahead! They {notWord}look hostile.");
            Console.WriteLine("You can (1) run, (2) attack, (3) ignore them, or (4) circle wagons.");
            int GT = AskInt("What do you want to do", 1, 4);

            if (rng.NextDouble() < .2) GH = 1 - GH; // maybe flip hostility

            if (GH == 1) // friendly
            {
                // “Cost if friendly”
                if (GT == 1) { M += 15; A -= 5; }
                else if (GT == 2) { M -= 5; B -= 100; }
                else if (GT == 4) { M -= 20; }
                Console.WriteLine("Riders were friendly, but check for possible losses.");
                return;
            }

            // hostile: do based on tactic
            switch (GT)
            {
                case 1: // run
                    M += 20; R -= 7; B -= 150; A -= 20; break;
                case 2: // attack
                    ShootGun();
                    B -= BR * 40 + 80;
                    if (BR <= 1)
                    {
                        Console.WriteLine("Nice shooting — you drove them off.");
                        goto RidersEnd;
                    }
                    if (BR <= 4)
                    {
                        Console.WriteLine("Kind of slow with your Colt .45.");
                        goto RidersEnd;
                    }
                    Console.WriteLine("Pretty slow on the draw, partner. You got a nasty flesh wound.");
                    KH = 1; Console.WriteLine("You'll have to see the doc soon as you can.");
                    break;

                case 3: // ignore
                    if (rng.NextDouble() > .8)
                    {
                        Console.WriteLine("They did not attack. Whew!");
                        return;
                    }
                    B -= 150; R -= 7;
                    break;

                case 4: // circle wagons
                    ShootGun();
                    B -= BR * 30 + 80; M -= 25;
                    if (BR <= 1)
                    {
                        Console.WriteLine("Nice shooting — you drove them off.");
                        goto RidersEnd;
                    }
                    if (BR <= 4)
                    {
                        Console.WriteLine("Kind of slow with your Colt .45.");
                        goto RidersEnd;
                    }
                    Console.WriteLine("Pretty slow on the draw, partner. You got a nasty flesh wound.");
                    KH = 1; Console.WriteLine("You'll have to see the doc soon as you can.");
                    break;
            }

        RidersEnd:
            Console.WriteLine("Riders were hostile. Better check for losses!");
            if (B >= 0) return;

            Console.WriteLine();
            ShortPause();
            Console.Write("Oh, my gosh! ");
            Console.WriteLine("They're coming back and you're out of ammo! Your dreams turn to");
            Console.WriteLine("dust as you and your family are massacred on the prairie.");
            Died_BodiesFound();
        }

        // ----------------------------------------------------------
        // Hazards & events
        static void HazardsAndEvents()
        {
            double RN = 100 * rng.NextDouble();
            int i;
            for (i = 1; i <= 15; i++)
            {
                if (RN <= EP[i]) break;
            }

            if (i <= 8)
            {
                switch (i)
                {
                    case 1:
                        Console.WriteLine("Your wagon breaks down. It costs you time and supplies to fix it.");
                        M -= 15 + 5 * rng.NextDouble(); R -= 4; break;
                    case 2:
                        Console.WriteLine("An ox gores your leg. That slows you down for the rest of the trip.");
                        M -= 25; A -= 10; break;
                    case 3:
                        Console.WriteLine("Bad luck…your daughter breaks her arm. You must stop and");
                        Console.WriteLine("make a splint and sling with some of your medical supplies.");
                        M -= 5 + 4 * rng.NextDouble(); R -= 1 + 2 * rng.NextDouble(); break;
                    case 4:
                        Console.WriteLine("An ox wanders off and you have to spend time looking for it.");
                        M -= 17; break;
                    case 5:
                        Console.WriteLine("Your son gets lost and you spend half a day searching for him.");
                        M -= 10; break;
                    case 6:
                        Console.WriteLine("Nothing but contaminated and stagnant water near the trail.");
                        Console.WriteLine("You lose time looking for a clean spring or creek.");
                        M -= 2 + 10 * rng.NextDouble(); break;
                    case 7:
                        if (M > 950)
                        {
                            Console.Write("Cold weather…Brrrrrrr!…You ");
                            C1 = 0;
                            if (C < 11 + 2 * rng.NextDouble()) { Console.Write("don't "); C1 = 1; }
                            Console.WriteLine("have enough clothing to keep warm.");
                            if (C1 != 0) { Illness(); }
                        }
                        else
                        {
                            Console.WriteLine("Heavy rains. Traveling is slow in the mud and you break your spare");
                            Console.WriteLine("ox yoke using it to pry your wagon out of the mud. Worse yet, some");
                            Console.WriteLine("of your ammo is damaged by the water.");
                            M -= 5 + 10 * rng.NextDouble(); R -= 7; B -= 400; F -= 5;
                        }
                        break;
                    case 8:
                        Console.WriteLine("Bandits attacking!");
                        ShootGun();
                        B -= 20 * BR;
                        if (B <= 0)
                        {
                            T /= 3;
                            Console.WriteLine("You try to drive them off but you run out of bullets.");
                            Console.WriteLine("They grab as much cash as they can find.");
                            Console.Write("You get shot in the leg — ");
                            ShortPause(); KH = 1;
                            Console.WriteLine("and they grab one of your oxen.");
                            A -= 10; R -= 2;
                            Console.WriteLine("Better have a doc look at your leg…and soon!");
                        }
                        else
                        {
                            if (BR <= 1)
                            {
                                Console.WriteLine("That was the quickest draw outside of Dodge City.");
                                Console.WriteLine("You got at least one and drove 'em off.");
                            }
                            else
                            {
                                Console.Write("You get shot in the leg — ");
                                ShortPause(); KH = 1;
                                Console.WriteLine("and they grab one of your oxen.");
                                A -= 10; R -= 2;
                                Console.WriteLine("Better have a doc look at your leg…and soon!");
                            }
                        }
                        break;
                }
                return;
            }

            switch (i - 8)
            {
                case 1:
                    Console.WriteLine("You have a fire in your wagon. Food and supplies are damaged.");
                    M -= 15; F -= 20; B -= 400; R -= 12 * rng.NextDouble(); break;

                case 2:
                    Console.WriteLine("You lose your way in heavy fog. Time lost regaining the trail.");
                    M -= 10 + 5 * rng.NextDouble(); break;

                case 3:
                    Console.WriteLine("You come upon a rattlesnake and before you are able to get your gun");
                    Console.WriteLine("out, it bites you.");
                    B -= 10; R -= 2;
                    if (R < 0)
                    {
                        Console.WriteLine("You have no medical supplies left, and you die of poison.");
                        Died_Poison();
                    }
                    else
                    {
                        Console.WriteLine("Fortunately, you acted quickly, sucked out the poison, and");
                        Console.WriteLine("treated the wound. It is painful, but you'll survive.");
                    }
                    break;

                case 4:
                    Console.WriteLine("Your wagon gets swamped fording a river; you lose food and clothes.");
                    M -= 20 + 20 * rng.NextDouble(); F -= 15; C -= 10; break;

                case 5:
                    Console.WriteLine("You're sound asleep and you hear a noise…get up to investigate.");
                    ShortPause();
                    Console.WriteLine("It's wild animals! They attack you!");
                    ShootGun();
                    if (B <= 39)
                    {
                        Console.WriteLine("You're almost out of ammo; can't reach more.");
                        Console.WriteLine("The wolves come at you biting and clawing.");
                        KH = 1; GoPneumoniaOrInjury();
                    }
                    else
                    {
                        if (BR <= 2)
                        {
                            Console.WriteLine("Nice shooting, pardner…They didn't get much.");
                        }
                        else
                        {
                            Console.WriteLine("Kind of slow on the draw. The wolves got at your food and clothes.");
                            B -= 20 * BR; C -= 2 * BR; F -= 4 * BR;
                        }
                    }
                    break;

                case 6:
                    Console.WriteLine("You're caught in a fierce hailstorm; ammo and supplies are damaged.");
                    M -= 5 + 10 * rng.NextDouble(); B -= 150; R -= 2 + 2 * rng.NextDouble(); break;

                case 7:
                    // not eating well enough?
                    if (E == 1) { Illness(); return; }
                    if (E == 2 && rng.NextDouble() > .25) { Illness(); return; }
                    if (E == 3 && rng.NextDouble() > .5) { Illness(); return; }
                    break;

                case 8:
                    Console.WriteLine("Helpful Indians show you where to find more food.");
                    F += 7; break;
            }
        }

        // ----------------------------------------------------------
        // Mountains
        static void Mountains()
        {
            if (M <= 975) return;

            // IF 10*RND > 9 - ((M/100 - 15)^2 + 72)/((M/100 - 15)^2 + 12) THEN 2750
            double ratio = (Math.Pow(M / 100.0 - 15, 2) + 72) / (Math.Pow(M / 100.0 - 15, 2) + 12);
            if (10 * rng.NextDouble() <= 9 - ratio)
            {
                Console.WriteLine("You're in rugged mountain country.");
                if (rng.NextDouble() <= .1)
                {
                    Console.WriteLine("You get lost and lose valuable time trying to find the trail.");
                    M -= 60;
                }
                else if (rng.NextDouble() <= .11)
                {
                    Console.WriteLine("Trail cave in damages your wagon. You lose time and supplies.");
                    M -= 20 + 30 * rng.NextDouble(); B -= 200; R -= 3;
                }
                else
                {
                    Console.WriteLine("The going is really slow; oxen are very tired.");
                    M -= 45 + 50 * rng.NextDouble();
                }
            }

            // South Pass
            if (KP == 1) goto AfterSouthPass;
            KP = 1;
            if (rng.NextDouble() >= .8) // 20% chance of no blizzard -> safe
            {
                Console.WriteLine("You made it safely through the South Pass....no snow!");
            }
            else
            {
                BlizzardInPass();
            }

        AfterSouthPass:
            if (M < 1700) return;
            if (KM == 1) return;
            KM = 1;
            if (rng.NextDouble() >= .7) // 30% mishap -> blizzard
            {
                // get through without mishap
                return;
            }
            BlizzardInPass();
        }

        static void BlizzardInPass()
        {
            Console.WriteLine("Blizzard in the mountain pass. Going is slow; supplies are lost.");
            KB = 1;
            M -= 30 + 40 * rng.NextDouble(); F -= 12; B -= 200; R -= 5;
            if (C < 18 + 2 * rng.NextDouble()) { Illness(); } else { }
        }

        // ----------------------------------------------------------
        // Illness routine
        static void Illness()
        {
            if (100 * rng.NextDouble() < 10 + 35 * (E - 1))
            {
                Console.WriteLine("Mild illness. Your own medicine will cure it.");
                M -= 5; R -= 1;
            }
            else if (100 * rng.NextDouble() < 100 - (40 / Math.Pow(4, (E - 1))))
            {
                Console.WriteLine("Serious illness in the family. You'll have to stop and see a doctor");
                Console.WriteLine("soon. For now, your medicine will work.");
                R -= 5; KS = 1;
            }
            else
            {
                Console.WriteLine("The whole family is sick. Your medicine will probably work okay.");
                M -= 5; R -= 2.5;
            }

            if (R > 0) return;

            Console.Write(" …if only you had enough.");
            Console.WriteLine();
            OutOfMedicalSupplies();
        }

        // ----------------------------------------------------------
        // Gun mini-game (typing speed)
        static void ShootGun()
        {
            int rn = 1 + rng.Next(1, 4); // 1..4
            string word = ShotWords[rn];

            var sw = Stopwatch.StartNew();
            Console.Write($"Type {word} ");
            string x = Console.ReadLine() ?? "";

            // the BASIC accepted exact word or lowercase variant in slots 5..8
            bool ok = x == ShotWords[rn] || x == ShotWords[rn + 4];
            while (!ok)
            {
                Console.Write("Nope. Try again. ");
                x = Console.ReadLine() ?? "";
                ok = x == ShotWords[rn] || x == ShotWords[rn + 4];
            }
            sw.Stop();

            // seconds elapsed (rounded), then minus difficulty DR, minus 1
            int secs = (int)Math.Round(sw.Elapsed.TotalSeconds);
            BR = secs - DR - 1;
            if (BR < 0) BR = 0;
        }

        // ----------------------------------------------------------
        // Inventory
        static void PrintInventory()
        {
            Console.WriteLine("Cash\tFood\tAmmo\tClothes\tMedicine, parts, etc.");
            F = (F < 0) ? 0 : Math.Floor(F);
            B = (B < 0) ? 0 : B;
            C = (C < 0) ? 0 : Math.Floor(C);
            R = (R < 0) ? 0 : Math.Floor(R);
            Console.WriteLine($"{(int)Math.Floor(T)}\t{(int)F}\t{B}\t{(int)C}\t{(int)R}");
            Console.WriteLine();
        }

        // ----------------------------------------------------------
        // Death & end screens
        static void Died_Starvation()
        {
            Console.WriteLine("You run out of food and starve to death.");
            Died_BodiesFound();
        }

        static void CantAffordDoctor()
        {
            T = 0;
            Console.WriteLine("You need a doctor badly, but can't afford one.");
            GoPneumoniaOrInjury();
        }

        static void OutOfMedicalSupplies()
        {
            Console.WriteLine("You have run out of all medical supplies.");
            GoPneumoniaOrInjury();
        }

        static void GoPneumoniaOrInjury()
        {
            Console.WriteLine();
            Console.Write("The wilderness is unforgiving and you die of ");
            if (KH == 1) { Console.WriteLine("your injuries."); }
            else { Console.WriteLine("pneumonia."); }
            FamilyAftermath();
        }

        static void Died_Poison()
        {
            Console.WriteLine("Your family tries to push on, but finds the going too rough without you.");
            FamilyAftermath(true);
        }

        static void Died_OxenWornOut()
        {
            Console.WriteLine("Your oxen are worn out and can't go another step. You try pushing");
            Console.WriteLine("ahead on foot, but it is snowing heavily and everyone is exhausted.");
            Console.WriteLine();
            ShortPause();
            Console.WriteLine("You stumble and can't get up....");
            Died_BodiesFound();
        }

        static void Died_BodiesFound()
        {
            Console.WriteLine();
            ShortPause();
            Console.WriteLine("Some travelers find the bodies of you and your");
            Console.WriteLine("family the following spring. They give you a decent");
            Console.WriteLine("burial and notify your next of kin.");
            Console.WriteLine();

            int Dd = (int)Math.Floor(14 * (J + ML));
            int DM = (int)Math.Floor(Dd / 30.5);
            int DD = (int)Math.Floor(Dd - 30.5 * DM);

            Console.WriteLine("At the time of your unfortunate demise, you had been on the trail");
            Console.WriteLine($"for {DM} months and {DD} days and had covered {Math.Floor(M + 70)} miles.");
            Console.WriteLine(" You had a few supplies left :");
            PrintInventory();
            Console.WriteLine();
            PlayAgain();
        }

        static void FamilyAftermath(bool alreadyPrinted = false)
        {
            if (!alreadyPrinted)
            {
                Console.WriteLine("Your family tries to push on, but finds the going too rough");
                Console.WriteLine(" without you.");
            }
            Died_BodiesFound();
        }

        static void MadeIt()
        {
            // interpolate final partial segment
            ML = (2040 - MA) / (M - MA);
            F += (1 - ML) * (8 + 5 * E);
            Fanfare();

            Console.WriteLine("You finally arrived at Oregon City after 2040 long miles.");
            Console.WriteLine("You're exhausted and haggard, but you made it! A real pioneer!");

            int Dd = (int)Math.Floor(14 * (J + ML));
            int DM = (int)Math.Floor(Dd / 30.5);
            int DD = (int)Math.Floor(Dd - 30.5 * DM);

            Console.WriteLine($"You've been on the trail for {DM} months and {DD} days.");
            Console.WriteLine("You have few supplies remaining :");
            PrintInventory();
            Console.WriteLine();
            Console.WriteLine("President James A. Polk sends you his heartiest");
            Console.WriteLine("congratulations and wishes you a prosperous life in your new home.");
            Console.WriteLine();

            PlayAgain();
        }

        static void PlayAgain()
        {
            Console.WriteLine();
            Console.Write("Would you like to play again? ");
            string a = Console.ReadLine() ?? "";
            int Aflag = ParseYesNo(a);
            if (Aflag == 0)
            {
                Console.WriteLine("Okay, good luck!");
                ShortPause();
                // Restart from scratch
                ResetAll();
                Console.Clear();
                Main();
            }
            else
            {
                Console.WriteLine("Okay. So long for now.");
                ShortPause();
                Environment.Exit(0);
            }
        }

        static void ResetAll()
        {
            // reset to start conditions (like RUN)
            T = F = C = R = 0;
            B = 0; A = 0; M = 0; MA = 0;
            J = 0; E = 2; DR = 3; BR = 0; KQ = 0; KS = 0; KH = 0;
            KP = 0; KM = 0; KB = 0; C1 = 0; MPflag = 0; ML = 0;
            // data arrays remain loaded
        }

        // ----------------------------------------------------------
        // Utilities
        static int AskInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write($"{prompt} ");
                string s = Console.ReadLine() ?? "";
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) && v >= min && v <= max)
                    return v;

                Console.WriteLine("Please enter a valid number" + (min != int.MinValue ? $" between {min} and {max}" : "") + ".");
            }
        }

        static double AskDouble(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} ");
                string s = Console.ReadLine() ?? "";
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v >= 0)
                    return Math.Floor(v); // BASIC used INT for most reads here
                Console.WriteLine("Please enter a non-negative number.");
            }
        }

        static int ParseYesNo(string a)
        {
            if (string.IsNullOrWhiteSpace(a)) return 0; // default YES like original inputs
            char x = a.Trim()[0];
            if (x == 'Y' || x == 'y') return 0; // A=0 means play again in the BASIC
            if (x == 'N' || x == 'n') return 1;
            Console.WriteLine($"Don't understand your answer of {a}.");
            Console.Write("Please enter Y for 'yes' or N for 'no.' Which is it ");
            return ParseYesNo(Console.ReadLine() ?? "");
        }

        static void Fanfare()
        {
            // BASIC used SOUND with musical ratios; use simple beeps.
            Beep(440, 180); Beep(586, 180); Beep(697, 180);
            Beep(830, 300); Beep(697, 120); Beep(830, 400);
        }

        static void Beep(int freq, int ms)
        {
            try { Console.Beep(freq, ms); } catch { Thread.Sleep(ms); }
        }

        static void ShortPause() => Thread.Sleep(400);

        static void Center(string s)
        {
            int width = 70;
            int pad = Math.Max(0, (width - s.Length) / 2);
            Console.WriteLine(new string(' ', pad) + s);
        }

        // ----------------------------------------------------------
        // DATA LOAD
        static void ReadWordsAndDates()
        {
            // S$ (1..8)
            string[] w = { "POW", "BANG", "BLAM", "WHOP", "pow", "bang", "blam", "whop" };
            for (int i = 1; i <= 8; i++) ShotWords[i] = w[i - 1];

            string[] d = {
                "March 29","April 12","April 26","May 10","May 24","June 7","June 21",
                "July 5","July 19","August 2","August 16","August 31","September 13",
                "September 27","October 11","October 25","November 8","November 22",
                "December 6","December 20"
            };
            for (int i = 1; i <= 20; i++) DA[i] = d[i - 1];
        }

        static void ReadPlaces()
        {
            var pairs = new (int miles, string place)[]
            {
                (5, "on the high prairie."),
                (200, "near Independence Crossing on the Big Blue River."),
                (350, "following the Platte River."),
                (450, "near Fort Kearney."),
                (600, "following the North Platte River."),
                (750, "within sight of Chimney Rock."),
                (850, "near Fort Laramie."),
                (1000, "close upon Independence Rock."),
                (1050, "in the Big Horn Mountains."),
                (1150, "following the Green River."),
                (1250, "not too far from Fort Hall."),
                (1400, "following the Snake River."),
                (1550, "not far from Fort Boise."),
                (1850, "in the Blue Mountains."),
                (2040, "following the Columbia River.")
            };
            for (int i = 1; i <= 15; i++) { MP[i] = pairs[i - 1].miles; PL[i] = pairs[i - 1].place; }
        }

        static void ReadEventThresholds()
        {
            int[] v = { 6, 11, 13, 15, 17, 22, 32, 35, 37, 42, 44, 54, 64, 69, 95 };
            for (int i = 1; i <= 15; i++) EP[i] = v[i - 1];
        }
    }
}
