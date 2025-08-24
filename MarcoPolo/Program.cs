using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace MarcoPolo1271
{
    internal class Program
    {
        // --- RNG ---
        static readonly Random rng = new Random();

        // --- State ---
        static int[] EP = new int[21]; // event cumulative probabilities (1..14 used)
        static string[] MO = new string[7]; // 1..6
        static string[] Snd = { "", "SPLAT", "SPRONG", "TWACK", "ZUNK" };
        static string[] Fa = { "", "wild boar", "big stag", "black bear" };

        // supplies / stats
        static int JL = 300;     // jewels
        static int C = 2;        // clothes
        static int W = 30;       // arrows
        static int M = 5;        // balms/unguents
        static int FP = 5;       // previous meal size
        static int F = 0;        // sacks of food
        static int L = 0;        // skins of oil
        static int BSK = 99;     // index when a camel recovers
        static int B = 0;        // camels
        static int BL = 0;       // loadable camels (sick reduces)
        static int BA = 0;       // quality/speed modifier (higher better)
        static int A1, A2;       // range helper

        // travel/time
        static int J = 0;        // two-month segments
        static int D = 0;        // miles this segment
        static int DT = 0;       // miles total
        static int DZ = 0;       // are we in desert

        // status/sickness
        static double PSK = 0, PWD = 0;     // current sickness / wounds
        static double PSKT = 0, PWDT = 0;   // totals
        static double PFD = 0;              // poor-food accumulation
        static int FE = 0;                  // sacks eaten this turn
        static int FQ = 0;                  // sum of current + last meal
        static int HX = 2;                  // hunting skill “delay bias”
        static int SR = 0;                  // shooting response (smaller is better)
        static int CZ = 0;                  // clothing warning flag

        // helper
        static int EPT = 0;                 // total of event weights

        static void Main()
        {
            Console.Clear();
            Center("The Journey of Marco Polo, 1271");
            Console.WriteLine();
            Center("(c) David H. Ahl, 1986");
            ContinueKey();
            Console.Clear();

            InitScenario();
            ReadEventProbabilities();

            // initial trading + setup
            PurchaseInitialSupplies();
            InitHuntingSkill();

            Center("Press any key to begin your trek!");
            ContinueKey();
            Console.WriteLine();

            // main loop — 2-month segments
            while (true)
            {
                J++;
                PrintDate();

                // compute distance this segment (base + camel quality + randomness)
                D = 40 + BA * 20 + rng.Next(0, 100);
                DT += D;

                if (DT > 6000) { EndOfTrip(); return; }

                Console.WriteLine($"You have traveled {DT} miles.");
                Console.WriteLine("Here is what you now have:");
                PrintInventory();

                CheckNoJewelsOrClothes();
                DealWithSickness();

                // sick camel recovery?
                if (BSK == J) { BSK = 99; BL = B; BA = BA + 1; }

                if (J > 1 && JL > 1) Barter();
                if (C == 0) NoClothes();

                Eat();

                if (DZ == 0 && rng.NextDouble() < 0.18) HuntForFood();

                Console.WriteLine();
                DesertSections();

                if (DZ == 0) SpecialEvents();

                ClampNonNegatives();
            }
        }

        // ------------------- Scenario -------------------
        static void InitScenario()
        {
            Center("The Journey of Marco Polo - 1271");
            Console.WriteLine();
            Console.WriteLine(" Starting from Venice in 1271 you travel by sailing ship to the");
            Console.WriteLine("port of Armenia. Upon arrival, you prepare for a 6000-mile trek to");
            Console.WriteLine("the court of the Great Kublai Khan in Shang-tu, Cathay. Having set");
            Console.WriteLine($"aside {JL} precious jewels to finance your planned 3-year trip, you");
            Console.WriteLine("must barter for the following supplies in Armenia :");
            Console.WriteLine(" * Camels (Sturdier animals will cost more. You will probably");
            Console.WriteLine("   want 8 to 10 camels to carry your many supplies.)");
            Console.WriteLine(" * Food (You must barter for food as you travel along. However,");
            Console.WriteLine("   prices tend to be lower in port cities, so you should pack");
            Console.WriteLine("   in a good supply at the start.)");
            Console.WriteLine(" * Oil for lamps and cooking (Over much of the trip, you will be");
            Console.WriteLine("   able to use wood to build fires. However, in the Persian,");
            Console.WriteLine("   Lop, and Gobi deserts you will need oil.)");
            Console.WriteLine();
            Console.WriteLine(" From Venice you have also packed clothing, weapons (crossbows),");
            Console.WriteLine("and medicines (balms and unguents); however, your provisions will be");
            Console.WriteLine("depleted as you go along and you must replenish them. The selection");
            Console.WriteLine("and price of supplies is quite different in various regions, so you");
            Console.WriteLine("must barter wisely. As a merchant, you are not skilled in fishing");
            Console.WriteLine("or hunting, although occasionally you might be able to try to get");
            Console.WriteLine("some food in this way.");
            ContinueKey();
            Console.WriteLine();
        }

        // ------------------- Skill -------------------
        static void InitHuntingSkill()
        {
            Console.WriteLine();
            Console.WriteLine("Before you begin your journey, please rank your skill with");
            Console.WriteLine("the crossbow on the following scale:");
            Console.WriteLine(" (1) Can hit a charging boar at 300 paces");
            Console.WriteLine(" (2) Can hit a deer at 50 paces");
            Console.WriteLine(" (3) Can hit a sleeping woodchuck at 5 paces");
            Console.WriteLine(" (4) Occasionally hit own foot when loading");
            HX = AskInt("How do you rank yourself", 1, 4);
            Console.WriteLine();
        }

        // ------------------- Initial supplies -------------------
        static void PurchaseInitialSupplies()
        {
            Console.WriteLine(" After three months at sea, you have arrived at the seaport of");
            Console.WriteLine("Laiassus, Armenia. There are many merchants in the port city and");
            Console.WriteLine("you can easily get the supplies you need. Several traders offer you");

            int low = 17, high = 24;
            Console.WriteLine($"camels at prices between {low} and {high} jewels each.");
            int price = AskInt("How much do you want to pay for a camel", low, high);
            BA = price;

            Console.WriteLine("You will need at least 7 camels, but not more than 12.");
            int camels = AskInt("How many camels do you want to buy", 7, 12);
            B = camels;
            JL -= BA * B;
            A2 = 3 * B - 6;

            Console.WriteLine(" One large sack of food costs 2 jewels. You will need at least");
            Console.WriteLine($"8 sacks to get to Babylon (Baghdad); you can carry a maximum of {A2}");
            int sacks = AskInt("sacks. How many do you want", 8, 1000);
            F = sacks;
            JL -= sacks * 2;
            A2 = 3 * B - F;

            Console.WriteLine(" A skin of oil costs 2 jewels each. You should have at least 6");
            Console.WriteLine($"full skins for cooking in the desert. Your camels can carry {A2}");
            int skins = AskInt("skins. How many do you want", 5, 1000);
            BL = B;
            L = skins;
            JL -= 2 * L;
        }

        // ------------------- No jewels / clothes -------------------
        static void CheckNoJewelsOrClothes()
        {
            if (JL <= 15)
            {
                Console.WriteLine($"You have only {JL} jewels with which to barter.");
                if (B > 2 && AskYesNo("Would you like to sell a camel"))
                {
                    int rn = 8 + rng.Next(0, 9);
                    Console.WriteLine($"You get {rn} jewels for your best camel.");
                    JL += rn; B--; BL--;
                }
                else
                {
                    Console.WriteLine($"You push on with your {B} camels.");
                }
            }

            if (C <= 0)
            {
                Console.WriteLine("You should try to replace that tent you have been wearing as a");
                Console.WriteLine("robe. It is badly torn and the Tartars find it insulting.");
            }
        }

        // ------------------- Sickness -------------------
        static void DealWithSickness()
        {
            if (PSK > 0) { PSKT += PSK; PSK = 0; }
            if (PWD > 0) { PWDT += PWD; PWD = 0; }
            if (FE == 3) PFD += .4;

            if (PSKT + PWDT + PFD < 3) return;
            if (rng.NextDouble() > .7) return; // only sometimes

            Console.WriteLine("As a result of sickness, injuries, and poor eating, you must stop");
            Console.WriteLine("and regain your health. You trade a few jewels to stay in a hut.");
            int rn = 1 + (int)(3.2 * rng.NextDouble());
            if (rn > 3)
            {
                Pause();
                Console.WriteLine($"You stay for {rn} months but grow");
                Console.WriteLine("steadily weaker and finally pass away.");
                J += rn;
                GameOverOutOfLife();
                Environment.Exit(0);
            }
            Console.WriteLine($"You grow steadily stronger, but it is {rn * 2} months until you");
            Console.WriteLine("are again fit to travel.");
            PSKT = 0; PWDT = 0; PFD = 0; J += rn;
            M = Math.Max(3, M / 2);
            F = Math.Max(3, F / 2);
            JL = (JL > 20) ? JL - 10 : JL / 2;
            PrintDate();
        }

        // ------------------- Barter -------------------
        static void Barter()
        {
            Console.Write($"You have {JL} jewels. Do you want to barter here? ");
            if (!AskYesNo()) return;

            int camPrice = 17 + rng.Next(0, 8);
            Console.Write($"Camels cost {camPrice} jewels here. ");
            int max = JL / camPrice;
            int add = AskInt("How many do you want", 0, Math.Max(0, max));
            B += add; BL += add; BA -= add; JL -= add * camPrice;

            int foodP = 2 + rng.Next(0, 4);
            Console.Write($"Sacks of food cost {foodP} jewels. ");
            max = JL / foodP;
            int addF = AskInt("How many do you want", 0, Math.Max(0, max));
            F += addF;
            if (F + L > 3 * BL)
            {
                Console.WriteLine("Camels can't carry that much.");
                F -= addF;
            }
            else JL -= addF * foodP;

            int oilP = 2 + rng.Next(0, 4);
            Console.Write($"Skins of oil cost {oilP} jewels. ");
            max = JL / oilP;
            int addL = AskInt("How many do you want", 0, Math.Max(0, max));
            L += addL;
            if (F + L > 3 * BL)
            {
                Console.WriteLine("Camels can't carry that much.");
                L -= addL;
            }
            else JL -= addL * oilP;

            int clothesP = 8 + rng.Next(0, 8);
            Console.Write($"A set of clothes costs {clothesP} jewels. How many do you want ");
            int addC = AskInt("", 0, JL / clothesP);
            C += addC; JL -= addC * clothesP;

            Console.Write("You can get a bottle of balm for 2 jewels. ");
            int addM = AskInt("How many do you want", 0, JL / 2);
            JL -= addM * 2; M += addM;

            int arrowsPerJewel = 6 + rng.Next(0, 6);
            Console.WriteLine($"You can get {arrowsPerJewel} arrows for 1 jewel.");
            int spend = AskInt("How many jewels do you want to spend on arrows", 0, JL);
            JL -= spend; W += spend * arrowsPerJewel; if (C > 1) CZ = 0;

            Console.WriteLine();
            Console.WriteLine("Here is what you now have:");
            PrintInventory();
        }

        // ------------------- No clothes events -------------------
        static void NoClothes()
        {
            Console.WriteLine();
            Console.WriteLine("You were warned about getting more modest clothes.");
            Console.WriteLine("Furthermore, your sandals are in shreds.");
            if (CZ == 1)
            {
                Console.WriteLine("Word has been received about your disreputable appearance.");
                Console.Write("The people are not willing to deal with you and they ");
                StoneYou();
                return;
            }

            Console.Write("The Tartars chase you from town and ");
            if (rng.NextDouble() > .2)
            {
                Console.WriteLine("warn you not to return.");
                CZ = 1; return;
            }
            StoneYou();
        }

        static void StoneYou()
        {
            Console.WriteLine("stone you.");
            Console.WriteLine("You are badly wounded and vow to get new clothes as soon as possible.");
            PWD = 1.5; CZ = 1;
        }

        // ------------------- Eat -------------------
        static void Eat()
        {
            if (F < 3) { OutOfFood(); return; }

            Console.WriteLine("On the next stage of your journey, how do you want to eat :");
            Console.WriteLine(" (1) Reasonably well (can walk further; Less chance of sickness)");
            int a = AskInt(" (2) Adequately, or (3) Poorly", 1, 3);
            FE = 6 - a;

            if (FE > F)
            {
                Console.WriteLine("You don't have enough food to eat that well. Try again.");
                Eat(); return;
            }

            double FR = Math.Round(0.5 + 10 * (F - FE)) / 10.0;
            string xs = (FR == 1 ? "" : "s");
            Console.WriteLine($"Your food reserve will then be just {FR} sack{xs}");
            if (a != 3 && AskYesNo("Do you want to change your mind about how much you will eat")) { Eat(); return; }

            F -= FE;
            D -= (a - 1) * 50;
            FQ = FP + FE;
            FP = FE;
        }

        // ------------------- Out of food -------------------
        static void OutOfFood()
        {
            Console.WriteLine("You don't have enough food to go on.");
            if (JL >= 15)
            {
                Console.WriteLine("You should have bought food at the market. Now it will cost you");
                int rn = 5 + rng.Next(0, 4);
                Console.Write($"{rn} jewels per sack. ");
                int max = JL / rn;
                int buy = AskInt("How many sacks do you want", 1, Math.Max(1, max));
                F += buy; JL -= buy * rn;
                if (F >= 3) return;
                Console.WriteLine("You still don't have enough food and there is nothing to hunt.");
            }

            if (B >= 1)
            {
                if (AskYesNo("Do you want to eat a camel"))
                {
                    B--;
                    int rn = 3 + rng.Next(0, 3);
                    F += rn;
                    Console.WriteLine($"You manage to get about {rn} sacks of food out of it.");
                    return;
                }
                else
                {
                    EndGameOutOfFood();
                }
            }
            else
            {
                Console.WriteLine("You don't even have a camel left to eat.");
                EndGameOutOfFood();
            }
        }

        // ------------------- Desert sections -------------------
        static void DesertSections()
        {
            DZ = 0;
            if (DT < 2100 || DT > 5900) return;          // far ends no desert
            if (DT > 2600 && DT < 4100) return;          // Tigris valley
            if (DT > 4600 && DT < 5400) return;          // middle
            string des = (DT < 4100) ? "Dasht-e-Kavir (Persian)"
                      : (DT > 5399) ? "Gobi (Cathay)"
                      : "Taklimakan (Lop)";
            Console.WriteLine($"You are in the {des} desert.");

            if (L >= 3) { L -= 3; Console.WriteLine("Use 3 skins of oil for cooking."); }
            else
            {
                Console.WriteLine("You ran out of oil for cooking.");
                if (L > 1 && rng.NextDouble() > .5) L = 0;
                Console.WriteLine("You get horribly sick from eating raw and undercooked food.");
                L = 0; PSK = 1; D -= 80; M -= 1;
            }

            // random desert event
            int which = 1 + rng.Next(0, 7);
            switch (which)
            {
                case 1: EventSickCamel(); break;
                case 2: EventBadWater(); break;
                case 3: Console.WriteLine("You get lost trying to find an easier route."); D -= 100; break;
                case 4: Console.WriteLine("Heavy rains completely wash away the route."); D -= 90; break;
                case 5: Console.WriteLine("Some of your food rots in the humid weather."); F -= 1; break;
                case 6: Console.WriteLine("Marauding animals got into your food supply."); F -= 1; break;
                case 7: Console.WriteLine("You get through this stretch of desert without mishap!"); break;
            }

            DZ = 1;
            ClampNonNegatives();
        }

        // ------------------- Special events -------------------
        static void SpecialEvents()
        {
            int rn = (int)(EPT * rng.NextDouble());
            int idx;
            for (idx = 1; idx <= 14; idx++)
            {
                if (rn <= EP[idx]) break;
            }
            if (idx <= 10)
            {
                switch (idx)
                {
                    case 1: CamelLeg(); break;
                    case 2: EventSickCamel(); break;
                    case 3: EventBadWater(); break;
                    case 4: Console.WriteLine("You get lost trying to find an easier route."); D -= 100; break;
                    case 5: Console.WriteLine("Heavy rains completely wash away the route."); D -= 90; break;
                    case 6: Console.WriteLine("Some of your food rots in the humid weather."); F -= 1; break;
                    case 7: Console.WriteLine("Marauding animals got into your food supply."); F -= 1; break;
                    case 8: EventFire(); break;
                    case 9: Console.WriteLine("Two camels wander off. You finally find them after spending"); Console.WriteLine("several days searching for them."); D -= 20; break;
                    case 10: EventBurn(); break;
                }
            }
            else
            {
                switch (idx - 10)
                {
                    case 1: Console.WriteLine("A gash in your leg looks infected. It hurts like the blazes."); UseBalm(); D -= 50; PWD = .7; break;
                    case 2: Console.WriteLine("Jagged rocks tear your sandals and clothing. You'll have to get"); Console.WriteLine("replacements as soon as you can."); C -= 1; D -= 30; break;
                    case 3:
                        {
                            double r = rng.NextDouble() * FQ;
                            if (r < 2)
                            {
                                Console.WriteLine("All of you have horrible stomach cramps and intestinal disorders");
                                Console.WriteLine("and are laid up for over a month."); D -= 275;
                            }
                            else if (r < 3.5)
                            {
                                Console.WriteLine("You're running a high fever and your muscles feel like jelly.");
                                Console.WriteLine("Your party slows down for you."); PSK = .7; D -= 125;
                            }
                        }
                        break;
                    case 4: BanditAttack(); break;
                }
            }
            ClampNonNegatives();
        }

        static void CamelLeg()
        {
            Console.WriteLine("A camel injures its leg. Do you want to (1) Nurse it along or");
            Console.Write("(2) Abandon it, or (3) Sell it ");
            int a = AskInt("", 1, 3);
            if (a == 1) { BSK = J + 2; ExceedLoadCheck(); }
            else if (a == 2)
            {
                B -= 1; ExceedLoadCheck();
                int fc = 3 * BL - F - L;
                if (fc > 0)
                {
                    Console.WriteLine("You kill the camel for food.");
                    if (fc > 2) fc = 3;
                    F += fc;
                    Console.WriteLine($"You get the equivalent of {fc} sack{(fc == 1 ? "" : "s")} of food.");
                }
            }
            else
            {
                B -= 1; Console.WriteLine("It is a poor beast and you can get only 10 jewels for it.");
                JL += 10; ExceedLoadCheck();
            }
        }

        static void EventSickCamel()
        {
            Console.WriteLine("One of your camels is very sick and can't carry a full load.");
            Console.Write("Want to (1) Keep it with you, (2) Slaughter it, or (3) Sell it ");
            int a = AskInt("", 1, 3);
            if (a == 1) { BSK = J + 2; ExceedLoadCheck(); }
            else if (a == 2) CamelLeg(); // reuse slaughter path
            else { B -= 1; Console.WriteLine("It is a poor beast and you can get only 10 jewels for it."); JL += 10; ExceedLoadCheck(); }
        }

        static void EventBadWater()
        {
            Console.WriteLine("Long stretch with bad water. Costs time to find clean wells.");
            D -= 50;
        }

        static void EventFire()
        {
            Console.WriteLine("A fire flares up and destroys some of your food and clothes.");
            F -= 0; F -= 1; C -= 1; ClampNonNegatives();
            if (L >= 1) L -= 1;
        }

        static void EventBurn()
        {
            Console.WriteLine("You get a nasty burn from an oil fire.");
            PWD = .5; UseBalm();
        }

        static void BanditAttack()
        {
            Console.WriteLine("Blood-thirsty bandits are attacking your small caravan!");
            Console.Write("You grab your crossbow… ");
            ShootCrossbow();

            if (W <= 5)
            {
                Console.WriteLine("You try to drive them off, but you run out");
                Console.WriteLine("of arrows. They grab some jewels and food.");
                F -= 1;
                if (rng.NextDouble() <= .2)
                {
                    Console.WriteLine("They are savage, evil barbarians — they kill you and take");
                    Console.WriteLine("your remaining camels and jewels.");
                    JL = 0; B = 0; GameOverOutOfLife();
                }
                else
                {
                    Console.WriteLine("You caught a knife in the shoulder. That's going to take quite");
                    Console.WriteLine("a while to heal.");
                    UseBalm();
                    PWD = 1.5; JL -= 10; W -= 4 + 2 * SR; ClampNonNegatives();
                }
                return;
            }

            if (SR <= 1)
            {
                Console.WriteLine("Wow! Sensational shooting. You drove them off with no losses.");
                W -= 4;
            }
            else if (SR <= 3)
            {
                Console.WriteLine("With practice you could shoot the crossbow, but most of your shots");
                Console.WriteLine("missed. An iron mace got you in the chest. They took some jewels.");
                PWD = 1; JL -= 5; UseBalm(); W -= 3 + 2 * SR; ClampNonNegatives();
            }
            else
            {
                Console.WriteLine("Better stick to trading; your aim is terrible.");
                if (rng.NextDouble() <= .2)
                {
                    Console.WriteLine("They are savage, evil barbarians — they kill you and take");
                    Console.WriteLine("your remaining camels and jewels.");
                    JL = 0; B = 0; GameOverOutOfLife();
                }
                else
                {
                    Console.WriteLine("You caught a knife in the shoulder. That's going to take quite");
                    Console.WriteLine("a while to heal.");
                    UseBalm();
                    PWD = 1.5; JL -= 10; W -= 4 + 2 * SR; ClampNonNegatives();
                }
            }
        }

        // ------------------- Hunt for food (GW-BASIC 3020–3090) -------------------
static void HuntForFood()
{
    if (W < 15)
    {
        Console.WriteLine("You don't have enough arrows to hunt for food.");
        return;
    }

    // pick an animal name and shoot
    string animal = Fa[1 + rng.Next(1, 3)]; // wild boar / big stag / black bear (close enough)
    Console.Write($"There goes a {animal}… ");
    W -= 15;
    ShootCrossbow();

    if (SR <= 1)
    {
        Console.WriteLine("With shooting that good, the Khan will want you in his army.");
        int fa = 3;
        Console.WriteLine($"Your hunting yields {fa} sacks of food.");
        F += fa;
    }
    else if (SR <= 3)
    {
        Console.WriteLine("Not bad; you finally brought one down.");
        int fa = 2;
        Console.WriteLine($"Your hunting yields {fa} sacks of food.");
        F += fa;
    }
    else
    {
        Console.WriteLine("Were you too excited? All your shots went wild.");
    }
}

        // ------------------- Crossbow mini-game -------------------
        static void ShootCrossbow()
        {
            int rn = 1 + rng.Next(1, 4); // 1..4
            string word = Snd[rn];

            // start
            var sw = Stopwatch.StartNew();
            Console.Write($"Type : {word} ");
            string input = Console.ReadLine() ?? "";

            // permit exact or “shifted” uppercase tolerance like original
            if (string.Equals(input, word, StringComparison.OrdinalIgnoreCase))
            {
                sw.Stop();
                var secs = sw.Elapsed.TotalSeconds;
                // SR is smaller if fast; HX is handicap
                SR = (int)Math.Max(0, Math.Round(secs)) - HX;
                if (SR < 0) SR = 0;
                return;
            }
            Console.WriteLine("That's not it. Try again.");
            ShootCrossbow();
        }

        // ------------------- Use balm / unguent -------------------
        static void UseBalm()
        {
            int rn = 1 + rng.Next(0, 2);
            string xs = rn > 1 ? "s" : "";
            string xa = (rng.NextDouble() > .5) ? "balm" : "unguent";
            M -= rn;
            if (M >= 0)
            {
                Console.WriteLine($"You use {rn} bottle{xs} of {xa} treating your wound.");
                return;
            }
            M = 0;
            Console.WriteLine($"You need more {xa} to treat your wound.");
            if (JL < 8)
            {
                Console.Write("But, alas, you don't have enough jewels to buy any.");
                Console.WriteLine();
                Console.Write("Your wound is badly infected, ");
                if (rng.NextDouble() < .8)
                {
                    Console.WriteLine("but you push on for the next village.");
                    PWD = 3; return;
                }
                Console.WriteLine("but you keep going anyway.");
                Console.WriteLine("Unfortunately, the strain is too much for you and, after weeks of");
                Console.WriteLine("agony, you succumb to your wounds and die in the wilderness.");
                GameOverOutOfLife();
            }

            Console.WriteLine("Fortunately, you find some nomads who offer to sell you 2 bottles");
            Console.WriteLine($"of {xa} for the outrageous price of 4 jewels each.");
            if (AskYesNo("Do you want to buy it"))
            {
                Console.WriteLine("It works well and you're soon feeling better.");
                M = 0; JL -= 8;
            }
            else
            {
                Console.Write("Your wound is badly infected, ");
                if (rng.NextDouble() < .8)
                {
                    Console.WriteLine("but you push on for the next village.");
                    PWD = 3; return;
                }
                Console.WriteLine("but you keep going anyway.");
                Console.WriteLine("Unfortunately, the strain is too much for you and, after weeks of");
                Console.WriteLine("agony, you succumb to your wounds and die in the wilderness.");
                GameOverOutOfLife();
            }
        }

        // ------------------- Excess load -------------------
        static void ExceedLoadCheck()
        {
            BL = B;
            if (BSK <= J) { BL = B - 1; BA -= 1; }
            if (F + L <= 3 * BL) return;

            Console.WriteLine("You have too large a load for your camels.");
            int fc = (int)Math.Floor((double)(F + L - 3 * BL) + 0.9);
            Console.WriteLine($"You'll have to sell {fc} sack{(fc == 1 ? "" : "s")} of food or skin{(fc == 1 ? "" : "s")} of oil.");
            int fs = fc / 2; int ls = fc - fs;

            while (ls > L) { ls--; fs++; }
            while (fs > F) { fs--; ls++; }

            F -= fs; L -= ls; JL += fs + ls;
            Console.WriteLine($"You sell {fs} of food, {ls} of oil for which you get only {fs + ls} jewel{(fs + ls == 1 ? "" : "s")}.");
        }

        // ------------------- Inventory display -------------------
        static void PrintInventory()
        {
            ClampNonNegatives();
            Console.WriteLine("                      Sacks of  Skins of  Robes and  Balms and   Crossbow");
            Console.WriteLine("Jewels  Camels  Food     Oil     Sandals   Unguents    Arrows");
            Console.WriteLine(
                $"{JL,6}  {B,6}  {F,5}  {L,7}  {C,8}  {M,9}  {W,9}");
            Console.WriteLine();
        }

        // ------------------- Endings -------------------
        static void EndGameOutOfFood()
        {
            Console.WriteLine("You keep going as long as you can, trying to find berries and");
            Console.WriteLine("edible plants. But this is barren country and you fall ill and,");
            Console.WriteLine("after weeks of suffering, you collapse into eternal sleep.");
            J += 1; PrintDate();
            Console.WriteLine("You had the following left at the end :");
            PrintInventory();
            Console.WriteLine($"You traveled for {J * 2} months!");
            Console.WriteLine();
            Console.WriteLine("Sorry, you didn't make it to Shang-tu.");
            TryAgain();
        }

        static void GameOverOutOfLife()
        {
            J += 1; PrintDate();
            Console.WriteLine("You had the following left at the end :");
            PrintInventory();
            Console.WriteLine($"You traveled for {J * 2} months!");
            Console.WriteLine();
            Console.WriteLine("Sorry, you didn't make it to Shang-tu.");
            TryAgain();
        }

        static void EndOfTrip()
        {
            ClampNonNegatives();
            Console.Clear();
            for (int i = 0; i < 5; i++)
            {
                Center("CONGRATULATIONS !");
                System.Threading.Thread.Sleep(300);
                Console.Clear();
            }

            Console.WriteLine($"You have been traveling for {J * 2} months !");
            Console.WriteLine();
            Console.WriteLine("You are ushered into the court of the Great Kublai Khan.");
            Console.WriteLine("He surveys your meager remaining supplies :");
            PrintInventory();
            Console.WriteLine("… and marvels that you got here at all. He is disappointed");
            Console.WriteLine("that the Pope did not see fit to send the 100 men of learning");
            Console.WriteLine("that he requested and, as a result, keeps the three of you as");
            Console.WriteLine("his personal envoys for the next 21 years. Well done!");
            Console.WriteLine();
            TryAgain();
        }

        static void TryAgain()
        {
            Console.WriteLine();
            if (AskYesNo("Would you like to try again")) { Console.Clear(); ResetGame(); Main(); }
            else { Console.WriteLine("Bye for now."); Environment.Exit(0); }
        }

        static void ResetGame()
        {
            EP = new int[21];
            MO = new string[7];
            JL = 300; C = 2; W = 30; M = 5; FP = 5; BSK = 99;
            F = L = 0; B = 0; BL = 0; BA = 0;
            J = D = DT = DZ = 0;
            PSK = PWD = PSKT = PWDT = PFD = 0; FE = 0; FQ = 0; HX = 2; SR = 0; CZ = 0; EPT = 0;
        }

        // ------------------- Helpers -------------------
        static void PrintDate()
        {
            int mo = J;
            while (mo > 6) mo -= 6;
            int yr = 1271 + (J / 6);
            Console.WriteLine();
            Console.WriteLine($"Date : {MO[mo]} {yr}");
        }

        static void ReadEventProbabilities()
        {
            // cumulative probs for 14 events
            int[] data = { 6, 4, 4, 6, 6, 6, 6, 4, 4, 1, 6, 8, 18, 10 };
            for (int i = 1; i <= 14; i++) { EPT += data[i - 1]; EP[i] = EPT; }

            string[] mos = { "March", "May", "July", "September", "November", "January" };
            for (int i = 1; i <= 6; i++) MO[i] = mos[i - 1];
        }

        static void ClampNonNegatives()
        {
            JL = Math.Max(0, JL);
            F = Math.Max(0, F);
            L = Math.Max(0, L);
            C = Math.Max(0, C);
            M = Math.Max(0, M);
            W = Math.Max(0, W);
        }

        static void Center(string s)
        {
            int pad = Math.Max(0, (70 - s.Length) / 2);
            Console.WriteLine(new string(' ', pad) + s);
        }

        static void ContinueKey()
        {
            Center("Press any key to continue.");
            Console.ReadKey(true);
        }

        static void Pause() => System.Threading.Thread.Sleep(500);

        // ---- input helpers (BASIC-like) ----
        static int AskInt(string prompt, int min, int max)
        {
            while (true)
            {
                if (!string.IsNullOrEmpty(prompt)) Console.Write($"{prompt} ");
                string s = Console.ReadLine() ?? "";
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) && v >= min && v <= max) return v;

                Console.Write(min > 0 ? "That is too " : "That is ");
                Console.WriteLine(v < min ? "few." : "many.");
                Console.Write("Your answer please: ");
                // loop repeats
            }
        }

        static bool AskYesNo(string prompt = "")
        {
            if (!string.IsNullOrEmpty(prompt)) Console.Write(prompt + " ");
            while (true)
            {
                string s = Console.ReadLine() ?? "";
                if (string.IsNullOrWhiteSpace(s)) return true; // default Y like original
                char ch = char.ToUpperInvariant(s.Trim()[0]);
                if (ch == 'Y') return true;
                if (ch == 'N') return false;
                Console.Write("Don't understand answer. Enter 'Y' or 'N' please: ");
            }
        }
    }
}
