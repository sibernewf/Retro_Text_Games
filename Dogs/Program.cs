using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DogsDogRace
{
    class Program
    {
        // --- Tunables to mirror the BASIC game feel ---
        const int MaxBettors = 19;
        const decimal MinBet = 2m;
        const decimal MaxBet = 500m;
        const string SaveFile = "dogs.json"; // persistent W/L

        static readonly string[] DogNames =
        {
            "Fastar","Zelda","Speedy","Ziffle","Killer",
            "Burbon","Suzy","Snoopy","Lassie","Winner"
        };

        static void Main()
        {
            Console.WriteLine("WELCOME TO ROOK-A-DAY RACE TRACK!");
            Console.Write("Type '?' for instructions, or press Enter to start: ");
            if ((Console.ReadLine() ?? "").Trim() == "?") PrintInstructions();

            var dogs = LoadOrInitKennel();

            while (true)
            {
                Console.WriteLine();
                PrintDogTable(dogs);

                // --- Collect bets ---
                var slips = GetBetsFromPlayers(dogs.Count);

                // No bets? bail
                if (slips.Count == 0)
                {
                    Console.WriteLine("No bets today. Track closing.");
                    break;
                }

                // --- Post odds from historical W/L (display only) ---
                Console.WriteLine();
                PrintPostedOdds(dogs);

                // --- Run the race ---
                Console.WriteLine("\n* 1 2 3 4 5 6 7 8 9 10   AND THEY'RE OFF!!!");
                RaceAnimation();
                int winner = PickWinner(dogs); // 0-based index
                Console.WriteLine("\n******************************");
                Console.WriteLine($"AND THE WINNER IS DOG NUMBER {winner + 1}   {dogs[winner].Name}");
                Console.WriteLine("******************************\n");

                // --- Payout (pari-mutuel) ---
                Payout(slips, winner);

                // --- Update records & save ---
                for (int i = 0; i < dogs.Count; i++)
                {
                    if (i == winner) dogs[i].Wins++;
                    else dogs[i].Losses++;
                }
                SaveKennel(dogs);

                // Play again?
                if (!AskYesNo("Do you want to run another race (1=YES, 0=NO)? ")) break;
            }

            Console.WriteLine("\nTrack closed. See you next time!");
        }

        // ===========================================================
        // Data
        // ===========================================================
        class Dog
        {
            public string Name { get; set; } = "";
            public int Wins { get; set; }
            public int Losses { get; set; }
        }

        class BetSlip
        {
            public string Bettor { get; set; } = "";
            public int DogIndex { get; set; } // 0-based
            public decimal Amount { get; set; }
        }

        static List<Dog> LoadOrInitKennel()
        {
            if (File.Exists(SaveFile))
            {
                try
                {
                    var loaded = JsonSerializer.Deserialize<List<Dog>>(File.ReadAllText(SaveFile));
                    if (loaded != null && loaded.Count == DogNames.Length)
                        return loaded;
                }
                catch { /* fall through to init */ }
            }
            // First day at the track: start everyone at 0–0
            return DogNames.Select(n => new Dog { Name = n, Wins = 0, Losses = 0 }).ToList();
        }

        static void SaveKennel(List<Dog> dogs)
        {
            File.WriteAllText(SaveFile, JsonSerializer.Serialize(dogs, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }

        // ===========================================================
        // UI & Input
        // ===========================================================
        static void PrintInstructions()
        {
            Console.WriteLine(@"
This is a DOG RACE game. There are 10 dogs. Their past WINS/LOSSES
are posted and persist across days. Up to 19 people may bet each race.
Min bet = $2.00, Max bet = $500.00. After bets are in, the track posts
odds (based on prior W/L), then the race runs. Payouts are pari-mutuel:
all winning bettors split the entire pool in proportion to their bet.");
        }

        static void PrintDogTable(List<Dog> dogs)
        {
            Console.WriteLine("DOG\tNUMBER\tWINS\tLOSSES");
            for (int i = 0; i < dogs.Count; i++)
                Console.WriteLine($"{dogs[i].Name,-7}\t{i + 1}\t{dogs[i].Wins}\t{dogs[i].Losses}");
        }

        static List<BetSlip> GetBetsFromPlayers(int dogCount)
        {
            var slips = new List<BetSlip>();
            int people = ReadInt($"\nHOW MANY WISH TO BET? (0–{MaxBettors}): ", 0, MaxBettors);
            for (int i = 0; i < people; i++)
            {
                Console.WriteLine();
                Console.Write($"BETTOR'S NAME? ");
                var name = (Console.ReadLine() ?? "").Trim();
                if (string.IsNullOrEmpty(name)) { i--; continue; }

                int dog = ReadInt("DOG'S NUMBER (1–10)? ", 1, 10) - 1;
                decimal amt = ReadMoney($"AND YOUR BET? (${MinBet}–${MaxBet}): ", MinBet, MaxBet);

                slips.Add(new BetSlip { Bettor = name, DogIndex = dog, Amount = amt });
            }
            return slips;
        }

        static void PrintPostedOdds(List<Dog> dogs)
        {
            Console.WriteLine("DOG\tNUMBER\tODDS");
            for (int i = 0; i < dogs.Count; i++)
            {
                var (num, den) = OddsFromWL(dogs[i].Wins, dogs[i].Losses);
                Console.WriteLine($"{dogs[i].Name,-7}\t{i + 1}\t{num}:{den}");
            }
        }

        static void RaceAnimation()
        {
            var rand = new Random();
            for (int lap = 0; lap < 3; lap++)
            {
                Console.WriteLine("XXXXXXXXXXSTARTXXXXXXXXXX");
                System.Threading.Thread.Sleep(350);
                Console.WriteLine("XXXXXXXXXXFINISHXXXXXXXXX");
                System.Threading.Thread.Sleep(250);
            }
        }

        static int PickWinner(List<Dog> dogs)
        {
            // Bias toward historically good dogs.
            // Weight = (wins+1) / (losses+1). (Always positive; larger => better chance.)
            double[] weights = dogs.Select(d => (double)(d.Wins + 1) / (d.Losses + 1)).ToArray();
            double total = weights.Sum();
            double r = new Random().NextDouble() * total;
            double acc = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                acc += weights[i];
                if (r <= acc) return i;
            }
            return weights.Length - 1;
        }

        static void Payout(List<BetSlip> slips, int winnerDogIndex)
        {
            decimal pool = slips.Sum(s => s.Amount);
            decimal totalOnWinner = slips.Where(s => s.DogIndex == winnerDogIndex).Sum(s => s.Amount);

            if (totalOnWinner <= 0)
            {
                Console.WriteLine("No one bet on the winner. The house keeps the pool.");
                Console.WriteLine($"Total pool was ${pool:0}.");
                return;
            }

            foreach (var s in slips.Where(s => s.DogIndex == winnerDogIndex))
            {
                decimal share = pool * (s.Amount / totalOnWinner);
                Console.WriteLine($"CONGRATULATIONS {s.Bettor.ToUpper()}! YOU HAVE WON ${Math.Round(share, 0)}");
            }
        }

        // ===========================================================
        // Helpers
        // ===========================================================
        static (int, int) OddsFromWL(int wins, int losses)
        {
            // Display odds ~ integer ratio of (losses+1):(wins+1), simplified to n:1 style.
            int a = Math.Max(1, losses + 1);
            int b = Math.Max(1, wins + 1);
            int g = Gcd(a, b);
            a /= g; b /= g;
            // We show as N:1 where N = ceil(a/b) when b>1; if b==1 it’s already N:1.
            int n = (int)Math.Ceiling(a / (double)b);
            return (n, 1);
        }

        static int Gcd(int a, int b) => b == 0 ? a : Gcd(b, a % b);

        static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim();
                if (s == "1") return true;
                if (s == "0") return false;
                Console.WriteLine("Please type 1 for YES or 0 for NO.");
            }
        }

        static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int v) && v >= min && v <= max) return v;
                Console.WriteLine($"Enter a whole number from {min} to {max}.");
            }
        }

        static decimal ReadMoney(string prompt, decimal min, decimal max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (decimal.TryParse(Console.ReadLine(), out decimal v) && v >= min && v <= max)
                    return Math.Round(v, 2);
                Console.WriteLine($"Enter an amount between ${min:0.00} and ${max:0.00}.");
            }
        }
    }
}
