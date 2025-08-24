using System;
using System.Globalization;

namespace KingIsland
{
    internal static class Program
    {
        static void Main()
        {
            Console.Title = "KING — Govern Your Own Island (Setats Detinu)";
            Game game = new Game();
            game.Run();
        }
    }

    #region Game Core

    internal sealed class Game
    {
        private readonly Random rng = new Random();

        // ---- World constants (easy to tweak) ----
        private const int StartYear = 1960;
        private const int TermYears = 8;

        private const int IslandWidthMiles = 30;
        private const int IslandHeightMiles = 70;
        private const int PeopleNeedPerYear = 100;     // rallods per citizen to survive
        private const int MaxPlantMilesPerCitizen = 2; // planting capacity constraint

        private const double BaseTouristRate = 45;     // rallods per unplanted (forest/natural) sq. mile, pre-pollution
        private const double PollutionToTourismPenalty = 0.005; // each pollution point cuts % of tourism

        private const double AirPollutionFromIndustryPerMile = 0.015;
        private const double WaterPollutionFromIndustryPerMile = 0.010;
        private const double PollutionReductionPerRallod = 0.0005; // how much each rallod of control reduces pollution

        private const double PollutionDeathRate = 0.0006; // yearly deaths per citizen * pollution index
        private const double StarvationHarshness = 0.9;   // fraction of shortfall converted into deaths

        private const double CropYieldJitter = 0.15;      // ±15% yearly yield swing
        private const int CropRevenuePerMileFactor = 2;   // revenue ~ cost*2 (tunable)

        private const double WorkerPollutionMultiplier = 0.5; // workers add less social burden but more pollution

        private readonly int TotalLand = IslandWidthMiles * IslandHeightMiles;

        // ---- Game state (mutates) ----
        private int year;
        private int citizens;              // countrymen
        private int foreignWorkers;        // supported by industry
        private int landOwned;             // sq miles owned by state (can farm or keep as forests)
        private int landSoldToIndustry;    // sq miles taken permanently by industry

        private double treasury;           // rallods
        private double airPollution;       // abstract 0..~infty
        private double waterPollution;

        // Revolt / stability
        private int consecutiveUnderfeedYears;

        public Game()
        {
            // Reasonable starting conditions inspired by the sample pages
            year = StartYear;
            citizens = 580;                // “~587 countrymen” in sample
            foreignWorkers = 150;          // small initial industry presence
            landSoldToIndustry = 0;
            landOwned = TotalLand - landSoldToIndustry; // everything not sold belongs to state
            treasury = 62000;              // sample shows ~62k after first round
            airPollution = 0.0;
            waterPollution = 0.0;
            consecutiveUnderfeedYears = 0;
        }

        public void Run()
        {
            PrintIntro();

            for (int y = 0; y < TermYears; y++)
            {
                if (!PlayOneYear()) return; // ended by overthrow, assassination, or user exit
                year++;
            }

            // Win
            Console.WriteLine();
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Console.WriteLine("CONGRATULATIONS! YOU'VE SUCCESSFULLY COMPLETED YOUR");
            Console.WriteLine($"{TermYears}-YEAR TERM OF OFFICE. LONG LIVE SETATS DETINU!");
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }

        private void PrintIntro()
        {
            Console.WriteLine("CONGRATULATIONS! YOU'VE BEEN ELECTED PREMIER OF SETATS DETINU,");
            Console.WriteLine("A SMALL COMMUNIST ISLAND 30 BY 70 MILES LONG.");
            Console.WriteLine("YOUR JOB IS TO SCHEDULE THE COUNTRY'S BUDGET AND DISTRIBUTE");
            Console.WriteLine("MONEY FROM THE COMMUNAL TREASURY.");
            Console.WriteLine();
            Console.WriteLine($"- EACH PERSON NEEDS {PeopleNeedPerYear} RALLODS/YEAR TO SURVIVE.");
            Console.WriteLine("- REVENUE COMES FROM TOURISTS AND FROM CROPS YOU PLANT.");
            Console.WriteLine("- YOU MAY SELL LAND TO FOREIGN INDUSTRY FOR STRIP MINING (POLLUTING).");
            Console.WriteLine($"- CROPS COST BETWEEN 10 AND 15 RALLODS PER SQ. MILE TO PLANT/HARVEST.");
            Console.WriteLine($"- EACH CITIZEN CAN PLANT UP TO {MaxPlantMilesPerCitizen} SQ. MILES PER YEAR.");
            Console.WriteLine($"- YOUR GOAL IS TO COMPLETE AN {TermYears}-YEAR TERM WITHOUT MAJOR MISHAP.");
            Console.WriteLine();
            Console.WriteLine("TYPE ENTER TO BEGIN (or 'Q' to quit).");
            var s = Console.ReadLine();
            if (s != null && s.Trim().ToUpperInvariant() == "Q")
                Environment.Exit(0);
        }

        private bool PlayOneYear()
        {
            // Draw yearly market & costs
            var costs = NewYearEconomy();

            // Overview
            PrintHeader(costs);

            // --- Ask the four big decisions ---
            int sellMiles = AskIntClamped($"HOW MANY SQ. MILES DO YOU WISH TO SELL TO INDUSTRY? ",
                                          0, landOwned, defaultValue: 0);

            // Proceeds from sale happen now, and land immediately leaves the state's control
            int sellProceeds = sellMiles * costs.IndustryPricePerMile;
            ApplyIndustrySale(sellMiles, costs);

            Console.WriteLine($"YOU MADE {sellProceeds} RALLODS FROM LAND SALES.");

            // Rations / distribution
            int distribute = AskIntClamped($"HOW MANY RALLODS DO YOU WISH TO DISTRIBUTE TO YOUR COUNTRYMEN? ",
                                           0, int.MaxValue, defaultValue: citizens * PeopleNeedPerYear);

            // Planting — bound by both land and labor capacity
            int maxPlantableByLand = landOwned; // any owned land can be farmed if you choose
            int maxPlantableByLabor = citizens * MaxPlantMilesPerCitizen;
            int maxPlantable = Math.Min(maxPlantableByLand, maxPlantableByLabor);
            int plantMiles = AskIntClamped($"HOW MANY SQ. MILES DO YOU WISH TO PLANT? ",
                                           0, maxPlantable,
                                           defaultValue: Math.Min(maxPlantable, (int)(0.4 * landOwned)));

            // Pollution control budget
            int pollutionSpend = AskIntClamped($"HOW MANY RALLODS DO YOU WISH TO SPEND ON POLLUTION CONTROL? ",
                                               0, int.MaxValue, defaultValue: (int)(0.05 * treasury));

            // --- Validate funds; if deficit, auto-sell emergency land ---
            double totalOut = distribute + plantMiles * costs.CropCostPerMile + pollutionSpend;
            if (totalOut > treasury)
            {
                double deficit = totalOut - treasury;
                int emergencyMiles = (int)Math.Ceiling(deficit / Math.Max(1, costs.IndustryPricePerMile));
                emergencyMiles = Math.Min(emergencyMiles, landOwned);
                if (emergencyMiles > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("INSUFFICIENT RESERVES TO COVER COST — LAND WAS SOLD.");
                    ApplyIndustrySale(emergencyMiles, costs);
                    Console.WriteLine($"YOU WERE FORCED TO SELL {emergencyMiles} SQ. MILES FOR {emergencyMiles * costs.IndustryPricePerMile} RALLODS.");
                }
            }

            // Spend now
            treasury -= distribute;
            treasury -= plantMiles * costs.CropCostPerMile;
            treasury -= pollutionSpend;

            // --- Resolve results of the year ---
            // 1) Crops
            var cropResult = ResolveCrops(plantMiles, costs);
            treasury += cropResult.Revenue;

            // 2) Tourism
            var tourism = ResolveTourism(costs, plantMiles);
            treasury += tourism.Income;

            // 3) Pollution dynamics
            ApplyPollution(costs, pollutionSpend, sellMiles);

            // 4) Population changes
            var pop = ResolvePopulation(distribute, cropResult, tourism);

            // 5) Report all outcomes
            ReportYear(costs, sellMiles, distribute, plantMiles, pollutionSpend, cropResult, tourism, pop);

            // 6) Check for catastrophes / overthrow
            if (CheckForGameOver(pop)) return false;

            return true;
        }

        #endregion

        #region Yearly Economy / Resolution

        private YearEconomy NewYearEconomy()
        {
            // Industry price fluctuates: 50–90 Ral/sqmi (close to sample 74 etc.)
            int price = rng.Next(50, 91);

            // Crop costs: 10–15 Ral/sqmi (as per manual)
            int cropCost = rng.Next(10, 16);

            // Immigration potential baseline
            int immigWorkers = rng.Next(150, 351); // wide swing
            int immigCitizens = rng.Next(200, 701);

            return new YearEconomy
            {
                IndustryPricePerMile = price,
                CropCostPerMile = cropCost,
                ImmigrantWorkersPotential = immigWorkers,
                ImmigrantCitizensPotential = immigCitizens
            };
        }

        private void PrintHeader(YearEconomy costs)
        {
            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine($"YEAR: {year} — ISLAND STATUS");
            Console.WriteLine($"YOU NOW HAVE {treasury:0} RALLODS IN THE TREASURY.");
            Console.WriteLine($"{citizens} COUNTRYMEN AND {foreignWorkers} FOREIGN WORKERS.");
            Console.WriteLine($"{landOwned} SQ. MILES OF STATE LAND (INDUSTRY HOLDS {landSoldToIndustry}).");
            Console.WriteLine($"THIS YEAR INDUSTRY WILL BUY LAND FOR {costs.IndustryPricePerMile} RALLODS PER SQ. MILE.");
            Console.WriteLine($"LAND CURRENTLY COSTS {costs.CropCostPerMile} RALLODS PER SQ. MILE TO PLANT.");
            Console.WriteLine("--------------------------------------------------------------");
        }

        private void ApplyIndustrySale(int sellMiles, YearEconomy costs)
        {
            sellMiles = Math.Max(0, Math.Min(sellMiles, landOwned));
            landOwned -= sellMiles;
            landSoldToIndustry += sellMiles;
            treasury += sellMiles * costs.IndustryPricePerMile;

            // Selling land immediately worsens pollution baseline (industry expands)
            airPollution += sellMiles * AirPollutionFromIndustryPerMile;
            waterPollution += sellMiles * WaterPollutionFromIndustryPerMile;

            // Industry brings/supports workers (they usually come with sold land)
            int newWorkers = (int)Math.Round(sellMiles * rng.NextDouble() * 3.0); // 0–3 workers per sold mile
            foreignWorkers += newWorkers;
        }

        private CropResult ResolveCrops(int plantMiles, YearEconomy costs)
        {
            if (plantMiles <= 0) return new CropResult(0, 0);

            // Weather yield: ±15%
            double jitter = 1.0 + rng.NextDouble() * 2 * CropYieldJitter - CropYieldJitter;

            // Pollution reduces yield a bit
            double pollutionFactor = Math.Max(0.6, 1.0 - 0.3 * (airPollution + waterPollution));
            double effective = plantMiles * jitter * pollutionFactor;

            int harvestedMiles = (int)Math.Round(effective);

            int revenuePerMile = costs.CropCostPerMile * CropRevenuePerMileFactor; // e.g., cost=12 → revenue 24
            int revenue = harvestedMiles * revenuePerMile;

            return new CropResult(harvestedMiles, revenue);
        }

        private TourismResult ResolveTourism(YearEconomy costs, int plantMiles)
        {
            // Forest/natural land = anything not farmed and not sold
            int natural = Math.Max(0, landOwned - plantMiles);

            // Pollution slashes attractiveness
            double pollutionIndex = airPollution + waterPollution; // simple aggregate
            double multiplier = Math.Max(0, 1.0 - pollutionIndex * PollutionToTourismPenalty);

            // Random seasonal swing ±20%
            double seasonal = 1.0 + (rng.NextDouble() * 0.40 - 0.20);

            double income = natural * BaseTouristRate * multiplier * seasonal;

            // Round to integer rallods
            return new TourismResult(natural, (int)Math.Round(income));
        }

        private void ApplyPollution(YearEconomy costs, int pollutionSpend, int industryExpansionMiles)
        {
            // Industry expansion already added pollution in ApplyIndustrySale

            // Workers add ongoing pollution pressure
            airPollution += foreignWorkers * WorkerPollutionMultiplier / 10000.0;
            waterPollution += foreignWorkers * WorkerPollutionMultiplier / 12000.0;

            // Spending reduces both
            double reduction = pollutionSpend * PollutionReductionPerRallod;
            airPollution = Math.Max(0, airPollution - reduction);
            waterPollution = Math.Max(0, waterPollution - reduction);

            // Clamp to sensible range
            airPollution = Math.Min(5.0, airPollution);
            waterPollution = Math.Min(5.0, waterPollution);
        }

        private PopulationResult ResolvePopulation(int distribute, CropResult crop, TourismResult tourism)
        {
            // --- Starvation ---
            int needed = citizens * PeopleNeedPerYear;
            double feedRatio = needed == 0 ? 1.0 : Math.Min(1.0, (double)distribute / needed);
            int starvationDeaths = 0;
            if (feedRatio < 1.0)
            {
                starvationDeaths = (int)Math.Round((1.0 - feedRatio) * citizens * StarvationHarshness);
            }

            // --- Pollution deaths ---
            double polIndex = airPollution + waterPollution;
            int pollutionDeaths = (int)Math.Round(citizens * polIndex * PollutionDeathRate);

            // --- Net citizen change (immigration/emigration) ---
            int immigrants = 0, emigrants = 0;

            // People like money + nature; they hate starvation & pollution
            if (feedRatio >= 1.0 && polIndex < 1.0)
            {
                immigrants = rng.Next(100, 500);
            }
            else if (feedRatio < 0.8 || polIndex > 2.0)
            {
                emigrants = rng.Next(100, 700);
            }
            else
            {
                int swing = rng.Next(-150, 151);
                if (swing >= 0) immigrants = swing; else emigrants = -swing;
            }

            // Clamp to existing population
            emigrants = Math.Min(emigrants, Math.Max(0, citizens - starvationDeaths - pollutionDeaths));

            // Change citizens
            int before = citizens;
            citizens = citizens - starvationDeaths - pollutionDeaths - emigrants + immigrants;
            if (citizens < 0) citizens = 0;

            // --- Foreign worker migration (industry health proxy) ---
            // More industry land/year → pull workers; heavy pollution or chaos → drive them away
            int workerDelta = 0;
            if (polIndex < 1.5 && landSoldToIndustry > 0)
                workerDelta = rng.Next(50, 201);
            else if (polIndex > 2.5 || feedRatio < 0.8)
                workerDelta = -rng.Next(20, 151);
            foreignWorkers = Math.Max(0, foreignWorkers + workerDelta);

            // Stability tracking
            consecutiveUnderfeedYears = feedRatio < 1.0 ? consecutiveUnderfeedYears + 1 : 0;

            return new PopulationResult
            {
                Starved = starvationDeaths,
                PollutionDeaths = pollutionDeaths,
                Immigrated = immigrants,
                Emigrated = emigrants,
                WorkerDelta = workerDelta,
                NewCitizens = citizens,
                FeedRatio = feedRatio
            };
        }

        private void ReportYear(
            YearEconomy costs, int sold, int distributed, int planted, int polSpend,
            CropResult crop, TourismResult tourism, PopulationResult pop)
        {
            Console.WriteLine();
            Console.WriteLine($"OF {planted} SQ. MILES PLANTED, YOU HARVESTED {crop.HarvestedMiles} SQ. MILES OF CROPS,");
            Console.WriteLine($"MAKING {crop.Revenue} RALLODS.");

            Console.WriteLine();
            Console.WriteLine($"YOU MADE {tourism.Income} RALLODS FROM TOURIST TRADE.");
            if (airPollution + waterPollution > 2.5)
                Console.WriteLine("DECREASE BECAUSE AIR/WATER POLLUTION IS HURTING WILDLIFE AND BEACHES.");

            if (pop.Starved > 0 || pop.PollutionDeaths > 0)
            {
                Console.WriteLine();
                if (pop.Starved > 0) Console.WriteLine($"{pop.Starved} COUNTRYMEN DIED OF STARVATION!");
                if (pop.PollutionDeaths > 0) Console.WriteLine($"{pop.PollutionDeaths} COUNTRYMEN DIED OF POLLUTION-RELATED ILLNESS!");
            }

            if (pop.WorkerDelta != 0)
            {
                Console.WriteLine();
                if (pop.WorkerDelta > 0) Console.WriteLine($"{pop.WorkerDelta} FOREIGN WORKERS CAME TO THE COUNTRY.");
                else Console.WriteLine($"{-pop.WorkerDelta} FOREIGN WORKERS LEFT THE COUNTRY.");
            }

            if (pop.Immigrated > 0 || pop.Emigrated > 0)
            {
                Console.WriteLine();
                if (pop.Immigrated > 0) Console.WriteLine($"{pop.Immigrated} COUNTRYMEN CAME TO THE ISLAND.");
                if (pop.Emigrated > 0) Console.WriteLine($"{pop.Emigrated} COUNTRYMEN LEFT THE ISLAND.");
            }

            Console.WriteLine();
            Console.WriteLine($"YOU NOW HAVE {treasury:0} RALLODS IN THE TREASURY.");
            Console.WriteLine($"{citizens} COUNTRYMEN, {foreignWorkers} FOREIGN WORKERS, AND {landOwned} SQ. MILES OF LAND.");
            Console.WriteLine($"LAND CURRENTLY COSTS {costs.CropCostPerMile} RALLODS PER SQ. MILE TO PLANT.");
        }

        private bool CheckForGameOver(PopulationResult pop)
        {
            // Revolt if severe underfeeding or mass deaths
            double deathRate = (pop.Starved + pop.PollutionDeaths) / Math.Max(1.0, pop.NewCitizens + pop.Starved + pop.PollutionDeaths);
            bool massDeaths = deathRate > 0.25; // 25% is catastrophic

            if (consecutiveUnderfeedYears >= 2 || massDeaths)
            {
                Console.WriteLine();
                Console.WriteLine("THE PEOPLE ARE ENRAGED. RIOTS BREAK OUT ACROSS THE ISLAND...");
                if (massDeaths) Console.WriteLine("DUE TO MASSIVE DEATHS, YOU HAVE BEEN OVERTHROWN!");
                else Console.WriteLine("DUE TO MISMANAGEMENT, YOU HAVE BEEN IMPEACHED AND REMOVED!");
                return true;
            }

            // Bankruptcy spiral
            if (treasury < -1000)
            {
                Console.WriteLine();
                Console.WriteLine("THE TREASURY IS BANKRUPT. CREDITORS SEIZE STATE ASSETS.");
                Console.WriteLine("YOU ARE FORCED OUT OF OFFICE IN DISGRACE.");
                return true;
            }

            // Player voluntary exit
            Console.WriteLine();
            Console.Write("Press ENTER for next year, or type Q to resign: ");
            string? s = Console.ReadLine();
            if (!string.IsNullOrEmpty(s) && s.Trim().ToUpperInvariant() == "Q")
            {
                Console.WriteLine("YOU RESIGNED BEFORE THE END OF YOUR TERM.");
                return true;
            }

            return false;
        }

        #endregion

        #region Utilities & DTOs

        private int AskIntClamped(string prompt, int min, int max, int? defaultValue = null)
        {
            while (true)
            {
                Console.Write(prompt);
                string? line = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(line) && defaultValue.HasValue)
                    return defaultValue.Value;

                if (int.TryParse(line, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                {
                    if (v < min)
                    {
                        Console.WriteLine($"Minimum is {min}. Using {min}.");
                        return min;
                    }
                    if (v > max)
                    {
                        Console.WriteLine($"Maximum is {max}. Using {max}.");
                        return max;
                    }
                    return v;
                }

                Console.WriteLine("Please enter a whole number.");
            }
        }

        private sealed class YearEconomy
        {
            public int IndustryPricePerMile { get; set; }
            public int CropCostPerMile { get; set; }
            public int ImmigrantWorkersPotential { get; set; }
            public int ImmigrantCitizensPotential { get; set; }
        }

        private readonly struct CropResult
        {
            public int HarvestedMiles { get; }
            public int Revenue { get; }
            public CropResult(int harvestedMiles, int revenue) { HarvestedMiles = harvestedMiles; Revenue = revenue; }
        }

        private readonly struct TourismResult
        {
            public int NaturalMiles { get; }
            public int Income { get; }
            public TourismResult(int naturalMiles, int income) { NaturalMiles = naturalMiles; Income = income; }
        }

        private sealed class PopulationResult
        {
            public int Starved { get; set; }
            public int PollutionDeaths { get; set; }
            public int Immigrated { get; set; }
            public int Emigrated { get; set; }
            public int WorkerDelta { get; set; }
            public int NewCitizens { get; set; }
            public double FeedRatio { get; set; }
        }

        #endregion
    }
}
