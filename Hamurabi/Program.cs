using System;

internal static class Program
{
    // Classic constants
    const int StartAcres = 1000;
    const int StartPeople = 100;
    const int StartGrain = 3000;
    const int YearsToRule = 10;

    const int FoodPerPerson = 20;      // bushels per person per year
    const int SeedPerAcre = 2;         // bushels per acre planted
    const int AcresPerPerson = 10;     // labor limit

    static readonly Random Rng = Random.Shared;

    static void Main()
    {
        Console.Title = "Hammurabi — Govern Ancient Sumeria";
        Intro();

        int year = 1;
        int acres = StartAcres;
        int people = StartPeople;
        int grain = StartGrain;

        // tracking for final report
        int totalStarved = 0;
        int immigrantsThisYear = 0;
        int starvedThisYear = 0;
        int harvestPerAcre = 3;
        int ratsAte = 0;

        while (year <= YearsToRule)
        {
            Console.WriteLine();
            Console.WriteLine($"HAMURABI:  I BEG TO REPORT TO YOU,");
            Console.WriteLine($"IN YEAR {year}, {starvedThisYear} PEOPLE STARVED, {immigrantsThisYear} CAME TO THE CITY.");
            Console.WriteLine($"POPULATION IS NOW {people}");
            Console.WriteLine($"THE CITY NOW OWNS {acres} ACRES.");
            Console.WriteLine($"YOU HARVESTED {harvestPerAcre} BUSHELS PER ACRE.");
            Console.WriteLine($"RATS ATE {ratsAte} BUSHELS.");
            Console.WriteLine($"YOU NOW HAVE {grain} BUSHELS IN STORE.");
            Console.WriteLine();

            // Current land price
            int price = Rng.Next(17, 27); // 17..26
            Console.WriteLine($"LAND IS TRADING AT {price} BUSHELS PER ACRE.");

            // Buy / sell land
            int deltaAcres = 0;
            while (true)
            {
                int? buy = AskInt("HOW MANY ACRES DO YOU WISH TO BUY? ");
                if (buy is null) return;
                if (buy < 0) { Console.WriteLine("TRY AGAIN."); continue; }

                int cost = buy.Value * price;
                if (cost > grain)
                {
                    Console.WriteLine($"HAMURABI:  THINK AGAIN. YOU HAVE ONLY {grain} BUSHELS OF GRAIN.  NOW THEN.");
                    continue;
                }

                if (buy.Value == 0)
                {
                    int? sell = AskInt("HOW MANY ACRES DO YOU WISH TO SELL? ");
                    if (sell is null) return;
                    if (sell < 0 || sell > acres)
                    {
                        Console.WriteLine("HAMURABI:  THINK AGAIN. YOU CANNOT SELL THAT MUCH.");
                        continue;
                    }
                    deltaAcres = -sell.Value;
                    grain += sell.Value * price;
                    acres -= sell.Value;
                    break;
                }
                else
                {
                    deltaAcres = buy.Value;
                    grain -= cost;
                    acres += buy.Value;
                    break;
                }
            }

            // Feed people
            int feed = 0;
            while (true)
            {
                int? f = AskInt("HOW MANY BUSHELS DO YOU WISH TO FEED YOUR PEOPLE? ");
                if (f is null) return;
                if (f < 0 || f > grain)
                {
                    Console.WriteLine($"HAMURABI:  THINK AGAIN. YOU HAVE ONLY {grain} BUSHELS OF GRAIN.  NOW THEN.");
                    continue;
                }
                feed = f.Value;
                grain -= feed;
                break;
            }

            // Plant seed
            int plant = 0;
            while (true)
            {
                int? p = AskInt("HOW MANY ACRES DO YOU WISH TO PLANT WITH SEED? ");
                if (p is null) return;
                if (p < 0) { Console.WriteLine("HAMURABI:  THINK AGAIN."); continue; }
                if (p > acres)
                {
                    Console.WriteLine($"HAMURABI:  THINK AGAIN. YOU CANNOT PLANT MORE ACRES THAN YOU OWN ({acres}).");
                    continue;
                }
                if (p > people * AcresPerPerson)
                {
                    Console.WriteLine($"HAMURABI:  THINK AGAIN. YOU HAVE ONLY {people} PEOPLE TO TEND THE FIELDS, NOW THEN.");
                    continue;
                }
                int neededSeed = p.Value * SeedPerAcre;
                if (neededSeed > grain)
                {
                    Console.WriteLine($"HAMURABI:  THINK AGAIN. YOU HAVE ONLY {grain} BUSHELS OF GRAIN.  NOW THEN.");
                    continue;
                }
                plant = p.Value;
                grain -= neededSeed;
                break;
            }

            // --- YEAR END EVENTS ---
            // Harvest
            harvestPerAcre = Rng.Next(1, 7); // 1..6
            int harvested = harvestPerAcre * plant;
            grain += harvested;

            // Rats
            ratsAte = 0;
            if (Rng.NextDouble() < 0.4) // some chance rats eat
            {
                // eat 10–30% of grain
                double percent = 0.1 + Rng.NextDouble() * 0.2;
                ratsAte = (int)Math.Round(grain * percent);
                grain -= ratsAte;
            }

            // Plague?
            bool plague = Rng.Next(1, 101) <= 15; // ~15% chance
            if (plague)
            {
                int died = people / 2;
                people -= died;
                Console.WriteLine("\nA HORRIBLE PLAGUE STRUCK!  HALF THE PEOPLE DIED.");
            }

            // Starvation & immigration
            int peopleCanFeed = feed / FoodPerPerson;
            if (peopleCanFeed >= people)
            {
                starvedThisYear = 0;
                // immigration (classic-ish formula)
                immigrantsThisYear = Rng.Next(0, Math.Max(1, (20 * acres + grain) / (100 * Math.Max(1, people)) + 1));
                people += immigrantsThisYear;
            }
            else
            {
                starvedThisYear = people - peopleCanFeed;
                totalStarved += starvedThisYear;
                people = peopleCanFeed;
                immigrantsThisYear = 0;

                // Immediate impeachment if > 45% starved in one year
                double frac = (double)starvedThisYear / Math.Max(1, people);
                if (frac > 0.45)
                {
                    Console.WriteLine($"\nDUE TO YOUR INCOMPETENCE, {starvedThisYear} PEOPLE HAVE STARVED IN ONE YEAR!!!");
                    Console.WriteLine("YOU HAVE BEEN IMPEACHED AND THROWN OUT OF OFFICE!");
                    FinalReport(year, totalStarved, StartPeople, acres, people);
                    return;
                }
            }

            year++;
        }

        // Finished full term
        FinalReport(YearsToRule, totalStarved, StartPeople, StartAcres, people, endAcres: null);
    }

    static void Intro()
    {
        Console.WriteLine("HAMMURABI — GOVERN ANCIENT SUMERIA");
        Console.WriteLine("You rule for up to 10 years. Each year:");
        Console.WriteLine("  • Land price varies (17–26 bushels per acre).");
        Console.WriteLine("  • Buy/Sell land for grain.");
        Console.WriteLine("  • Feed your people (20 bushels per person).");
        Console.WriteLine("  • Plant acres (2 bushels seed per acre, max 10 acres per person).");
        Console.WriteLine("Random events: harvest per acre (1–6), rats, plague (15%).");
        Console.WriteLine("Starve too many people in one year and you’ll be impeached!");
        Console.WriteLine("Type 'q' whenever you want to quit.\n");
    }

    static int? AskInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim();
            if (s.Equals("q", StringComparison.OrdinalIgnoreCase)) return null;
            if (int.TryParse(s, out int v)) return v;
            Console.WriteLine("PLEASE ENTER A WHOLE NUMBER (OR 'q' TO QUIT).");
        }
    }

    static void FinalReport(int yearsRuled, int totalStarved, int startPop, int startAcres, int endPop, int? endAcres = null)
    {
        double pctStarved = 100.0 * totalStarved / Math.Max(1, startPop);
        int acresPerPerson = (endAcres ?? startAcres) / Math.Max(1, endPop);

        Console.WriteLine();
        Console.WriteLine("YOUR PERFORMANCE IN OFFICE:");
        Console.WriteLine($" • YEARS RULED: {yearsRuled}");
        Console.WriteLine($" • TOTAL STARVED: {totalStarved} ({pctStarved:0.0}% of initial population)");
        Console.WriteLine($" • ACRES PER PERSON AT END: {acresPerPerson}");

        // Very simple rating similar to classic end text
        if (pctStarved > 33 || acresPerPerson < 7)
        {
            Console.WriteLine("\nYour heavy-handed mismanagement has reduced Sumeria to ruin.");
            Console.WriteLine("The people (who are left) would have been happier under Nero!");
        }
        else if (pctStarved > 20 || acresPerPerson < 9)
        {
            Console.WriteLine("\nNot great, not terrible. You muddled through your reign.");
        }
        else if (pctStarved > 10 || acresPerPerson < 10)
        {
            Console.WriteLine("\nA respectable reign. The scribes will record your name with some praise.");
        }
        else
        {
            Console.WriteLine("\nMagnificent! Your wise administration will be sung by bards for ages.");
        }

        Console.WriteLine("\nSO LONG FOR NOW.");
        Console.WriteLine("READY");
    }
}
