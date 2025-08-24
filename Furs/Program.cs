using System;
using System.Globalization;

namespace FursTradingExpedition
{
    internal static class Program
    {
        static void Main()
        {
            Console.Title = "FURS — Fur Trading Expedition (BASIC conversion)";
            new Game().Run();
        }
    }

    internal sealed class Game
    {
        readonly Random rng = new();
        decimal savings = 50m;               // “YOU HAVE $50 SAVINGS.”
        bool dead = false;

        // Base market prices (approx. from sample run, in dollars per pelt)
        // Fort multipliers will modify these.
        const decimal BASE_MINK = 25.1m;
        const decimal BASE_BEAVER = 32.8m;   // ~32–33 in sample
        const decimal BASE_ERMINE = 42.6m;
        const decimal BASE_FOX = 46.0m;

        public void Run()
        {
            PrintIntro();

            while (!dead)
            {
                Console.WriteLine();
                if (!AskYesNo("DO YOU WISH TO TRADE FURS? ")) break;

                int totalFurs = 190; // “AND 190 FURS TO BEGIN THE EXPEDITION.”
                Console.WriteLine();
                Console.WriteLine("YOUR 190 FURS ARE DISTRIBUTED AMONG THE FOLLOWING");
                Console.WriteLine("KINDS OF PELTS: MINK, BEAVER, ERMINE AND FOX.");
                Console.WriteLine("TYPE HOW MANY YOU HAVE OF EACH (MUST TOTAL 190).  (Q to quit)");

                int mink = AskInt("HOW MANY MINK PELTS DO YOU HAVE? ", 0, 190, allowQuit:true); if (mink < 0) break;
                int beav = AskInt("HOW MANY BEAVER PELTS DO YOU HAVE? ", 0, 190, allowQuit:true); if (beav < 0) break;
                int ermi = AskInt("HOW MANY ERMINE PELTS DO YOU HAVE? ", 0, 190, allowQuit:true); if (ermi < 0) break;
                int fox  = AskInt("HOW MANY FOX PELTS DO YOU HAVE? ", 0, 190, allowQuit:true); if (fox  < 0) break;

                int sum = mink + beav + ermi + fox;
                if (sum != totalFurs)
                {
                    Console.WriteLine($"TOTAL MUST BE {totalFurs}. LET’S START THIS YEAR OVER.");
                    continue;
                }

                Console.WriteLine();
                Console.WriteLine("DO YOU WANT TO TRADE YOUR FURS AT FORT 1, FORT 2,");
                Console.WriteLine("OR FORT 3?  FORT 1 IS FORT HOCHELAGA (MONTREAL),");
                Console.WriteLine("AND IS UNDER THE PROTECTION OF THE FRENCH ARMY.");
                Console.WriteLine("FORT 2 IS FORT STADACONA (QUEBEC) AND IS UNDER THE");
                Console.WriteLine("PROTECTION OF THE FRENCH ARMY.  HOWEVER, YOU MUST");
                Console.WriteLine("MAKE A PORTAGE AND CROSS THE LACHINE RAPIDS.");
                Console.WriteLine("FORT 3 IS FORT NEW YORK AND IS UNDER DUTCH CONTROL.");
                Console.WriteLine("YOU MUST CROSS THROUGH IROQUOIS LAND.");
                Console.WriteLine("ANSWER 1, 2, OR 3.");
                int fort = AskInt("FORT? ", 1, 3, allowQuit:true); if (fort < 0) break;
                Console.WriteLine();

                // Narration similar to listing
                switch (fort)
                {
                    case 1:
                        Console.WriteLine("YOU HAVE CHOSEN THE EASIEST ROUTE.  HOWEVER, THE FORT");
                        Console.WriteLine("IS FAR FROM ANY SEAPORTS.  THE VALUE");
                        Console.WriteLine("YOU RECEIVE FOR YOUR FURS WILL BE LOWER AND THE COST");
                        Console.WriteLine("OF SUPPLIES HIGHER THAN AT FORTS STADACONA OR NEW YORK.");
                        break;
                    case 2:
                        Console.WriteLine("YOU HAVE CHOSEN A HARD ROUTE.  IT IS, IN COMPARISON,");
                        Console.WriteLine("HARDER THAN THE ROUTE TO HOCHELAGA BUT EASIER THAN");
                        Console.WriteLine("THE ROUTE TO NEW YORK.  YOU WILL RECEIVE AN AVERAGE VALUE");
                        Console.WriteLine("FOR YOUR FURS AND THE COST OF YOUR SUPPLIES WILL BE AVERAGE.");
                        break;
                    case 3:
                        Console.WriteLine("YOU HAVE CHOSEN THE MOST DIFFICULT ROUTE.  AT");
                        Console.WriteLine("FORT NEW YORK YOU WILL RECEIVE THE HIGHEST VALUE");
                        Console.WriteLine("FOR YOUR FURS; THE COST OF YOUR SUPPLIES");
                        Console.WriteLine("WILL BE LOWER THAN AT ALL THE OTHER FORTS.");
                        break;
                }
                Console.WriteLine();

                // Apply travel events (approximation of the BASIC’s flavor)
                // We tune odds by fort: 1 safest, 3 riskiest.
                var (minkAdj, beavAdj, ermiAdj, foxAdj, lostAll) = TravelEvents(fort, ref mink, ref beav, ref ermi, ref fox);
                if (dead) break;               // raiding party killed you
                if (lostAll)
                {
                    // still must pay supplies/travel (historically you'd still need some), but use $0 sales
                    decimal supplies = SuppliesCost(fort);
                    decimal travel   = TravelCost(fort);
                    Console.WriteLine();
                    Console.WriteLine($"SUPPLIES AT {FortName(fort)} COST ${supplies:0.0#}");
                    if (travel > 0) Console.WriteLine($"YOUR TRAVEL EXPENSES TO {FortName(fort)} WERE ${travel:0.0#}");
                    savings -= supplies + travel;
                    Console.WriteLine();
                    Console.WriteLine($"YOU NOW HAVE ${savings:0.0#} , INCLUDING YOUR PREVIOUS SAVINGS");
                    if (!AskYesNo("DO YOU WANT TO TRADE FURS NEXT YEAR? ")) break;
                    continue;
                }

                // Sell pelts — prices depend on fort + tiny randomness; some furs may be half price or unsellable.
                Console.WriteLine();
                decimal priceM = PricePerPelt(BASE_MINK, fort)  * minkAdj;
                decimal priceB = PricePerPelt(BASE_BEAVER, fort)* beavAdj;
                decimal priceE = PricePerPelt(BASE_ERMINE, fort)* ermiAdj;
                decimal priceF = PricePerPelt(BASE_FOX, fort)   * foxAdj;

                decimal incomeM = Round(mink * priceM);
                decimal incomeB = Round(beav * priceB);
                decimal incomeE = Round(ermi * priceE);
                decimal incomeF = Round(fox  * priceF);

                if (mink > 0)  Console.WriteLine($"YOUR MINK SOLD FOR ${priceM:0.0#} ; YOUR MINK TOTAL ${incomeM:0.0#}");
                if (beav > 0)  Console.WriteLine($"YOUR BEAVER SOLD FOR ${priceB:0.0#} ; YOUR BEAVER TOTAL ${incomeB:0.0#}");
                if (ermi > 0)  Console.WriteLine($"YOUR ERMINE SOLD FOR ${priceE:0.0#} ; YOUR ERMINE TOTAL ${incomeE:0.0#}");
                if (fox  > 0)  Console.WriteLine($"YOUR FOX SOLD FOR ${priceF:0.0#} ; YOUR FOX TOTAL ${incomeF:0.0#}");

                decimal gross = incomeM + incomeB + incomeE + incomeF;

                // Costs
                decimal suppliesCost = SuppliesCost(fort);
                decimal travelCost   = TravelCost(fort);

                Console.WriteLine();
                Console.WriteLine($"SUPPLIES AT {FortName(fort)} COST ${suppliesCost:0.0#}");
                if (travelCost > 0) Console.WriteLine($"YOUR TRAVEL EXPENSES TO {FortName(fort)} WERE ${travelCost:0.0#}");

                decimal net = gross - (suppliesCost + travelCost);
                savings += net;

                Console.WriteLine();
                Console.WriteLine($"YOU NOW HAVE ${savings:0.0#} , INCLUDING YOUR PREVIOUS SAVINGS");
                Console.WriteLine();

                if (!AskYesNo("DO YOU WANT TO TRADE FURS NEXT YEAR? ")) break;
            }

            if (dead)
            {
                Console.WriteLine();
                Console.WriteLine("YOUR PARTY WAS LOST. THIS ENDS THE GAME.");
            }
            Console.WriteLine();
            Console.WriteLine("HOPE YOU ENJOYED YOURSELF!");
        }

        // ---------- Travel events ----------
        // Returns multipliers for each pelt (1.0 normal, 0.5 damaged, 0 spoiled=0),
        // and a flag if all cargo was lost.
        (decimal minkMul, decimal beavMul, decimal ermiMul, decimal foxMul, bool lostAll)
            TravelEvents(int fort, ref int mink, ref int beav, ref int ermi, ref int fox)
        {
            decimal minkMul = 1m, beavMul = 1m, ermiMul = 1m, foxMul = 1m;
            bool lostAll = false;

            int risk = fort switch { 1 => 5, 2 => 12, 3 => 22 }; // crude overall risk index

            // Rare catastrophic events
            if (rng.Next(100) < (risk - 3)) // deadly raid (very rare)
            {
                if (fort == 3 && rng.Next(100) < 3)
                {
                    Console.WriteLine("YOU WERE ATTACKED BY A PARTY OF IROQUOIS.");
                    Console.WriteLine("ALL PEOPLE IN YOUR TRADING GROUP WERE KILLED.");
                    dead = true;
                    return (0,0,0,0,false);
                }
            }

            if (fort == 2 && rng.Next(100) < 6)
            {
                Console.WriteLine("YOUR CANOE UPSET IN THE LACHINE RAPIDS, YOU");
                Console.WriteLine("LOST ALL YOUR FURS!");
                mink = beav = ermi = fox = 0;
                lostAll = true;
                return (0,0,0,0,true);
            }

            // Theft while portaging (mostly fort 2)
            if (fort == 2 && rng.Next(100) < 8)
            {
                Console.WriteLine("YOUR BEAVER WERE TOO HEAVY TO CARRY ACROSS");
                Console.WriteLine("THE PORTAGE. YOU HAD TO LEAVE THE PELTS BUT FOUND");
                Console.WriteLine("THEM STOLEN WHEN YOU RETURNED.");
                beav = 0;
            }

            // Damage events (mostly fort 3)
            if (fort == 3 && rng.Next(100) < 30)
            {
                Console.WriteLine("YOUR MINK AND BEAVER WERE DAMAGED ON YOUR TRIP.");
                Console.WriteLine("YOU RECEIVE ONLY HALF THE CURRENT PRICE FOR THESE FURS.");
                minkMul = 0.5m; beavMul = 0.5m;
            }

            // Spoilage events
            if (rng.Next(100) < (fort==1?4:fort==2?6:8))
            {
                Console.WriteLine("YOUR FOX PELTS WERE NOT CURED PROPERLY.");
                Console.WriteLine("NO ONE WILL BUY THEM.");
                fox = 0; foxMul = 0m;
            }

            // “Narrowly escaped” flavor text for fort 3
            if (fort == 3 && rng.Next(100) < 25)
            {
                Console.WriteLine("YOU NARROWLY ESCAPED AN IROQUOIS RAIDING PARTY.");
            }

            return (minkMul, beavMul, ermiMul, foxMul, lostAll);
        }

        // ---------- Pricing & costs ----------
        static string FortName(int fort) => fort switch
        {
            1 => "FORT HOCHELAGA",
            2 => "FORT STADACONA",
            _ => "NEW YORK"
        };

        decimal PricePerPelt(decimal basePrice, int fort)
        {
            // Fort multipliers roughly match the text:
            // 1 (Hochelaga) = lowest, 2 (Stadacona) = average, 3 (New York) = highest.
            decimal mult = fort switch { 1 => 0.80m, 2 => 0.92m, 3 => 1.00m };
            // Small year-to-year market wobble
            decimal wiggle = (decimal)(rng.NextDouble() * 0.08 - 0.04); // ±4%
            decimal price = basePrice * (mult + wiggle);
            if (price < 0) price = basePrice * mult * 0.8m;
            return Math.Round(price, 2);
        }

        decimal SuppliesCost(int fort)
        {
            // Pulled from sample texts:
            // Stadacona $125; Hochelaga $150; New York $88
            return fort switch { 1 => 150m, 2 => 125m, 3 => 88m, _ => 120m };
        }

        decimal TravelCost(int fort)
        {
            // From sample text lines: Stadacona travel ~$15, Hochelaga ~$10, New York ~0
            return fort switch { 1 => 10m, 2 => 15m, 3 => 0m, _ => 0m };
        }

        // ---------- I/O helpers ----------
        bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();
                if (s == null) continue;
                s = s.Trim().ToUpperInvariant();
                if (s == "Q") return false;
                if (s is "Y" or "YES") return true;
                if (s is "N" or "NO") return false;
                Console.WriteLine("PLEASE ANSWER YES OR NO (or Q to quit).");
            }
        }

        int AskInt(string prompt, int min, int max, bool allowQuit=false)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();
                if (s == null) continue;
                s = s.Trim().ToUpperInvariant();
                if (allowQuit && s == "Q") return -1;
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) && n >= min && n <= max)
                    return n;
                Console.WriteLine($"ENTER A NUMBER FROM {min} TO {max}{(allowQuit ? " (or Q to quit)" : "")}.");
            }
        }

        static decimal Round(decimal d) => Math.Round(d, 2);
        
        void PrintIntro()
        {
            Console.WriteLine("FURS — FUR TRADING EXPEDITION");
            Console.WriteLine("You are the leader of a French fur trading expedition in 1776.");
            Console.WriteLine("You’ll sell furs and buy supplies at one of three forts each year.");
            Console.WriteLine("Prices and risks depend on the fort. Type Q at any prompt to quit.");
            Console.WriteLine();
            Console.WriteLine($"YOU HAVE ${savings:0.0#} SAVINGS,");
            Console.WriteLine("AND 190 FURS TO BEGIN THE EXPEDITION.");
            Console.WriteLine();
        }
    }
}
