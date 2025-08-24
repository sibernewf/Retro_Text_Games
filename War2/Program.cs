using System;
using System.Globalization;

namespace War2TroopTactics
{
    internal static class Program
    {
        private const int TotalTroops = 72000;
        private static readonly Random Rng = new Random();

        static void Main()
        {
            Console.WriteLine("WAR·2 — TROOP TACTICS IN WAR\n");
            Console.WriteLine("I AM AT WAR WITH YOU.");
            Console.WriteLine($"WE HAVE {TotalTroops} SOLDIERS A PIECE.");

            // Distribution
            int compArmy = TotalTroops / 3, compNavy = TotalTroops / 3, compAir = TotalTroops / 3;
            (int youArmy, int youNavy, int youAir) = GetDistribution();

            Console.WriteLine("\nDISTRIBUTE YOUR FORCES.");
            PrintForces("YOU", youArmy, youNavy, youAir);
            PrintForces("ME", compArmy, compNavy, compAir);

            // Two battles
            for (int battle = 1; battle <= 2; battle++)
            {
                Console.WriteLine($"\nBATTLE {battle}: YOU ATTACK FIRST.");
                (youArmy, youNavy, youAir, compArmy, compNavy, compAir) =
                    DoBattle(youArmy, youNavy, youAir, compArmy, compNavy, compAir);
            }

            // Outcome
            Console.WriteLine("\nFROM THE RESULTS OF BOTH OF YOUR ATTACKS:");
            int youScore = youArmy + youNavy + youAir;
            int compScore = compArmy + compNavy + compAir;

            if (youScore > compScore * 1.1)
                Console.WriteLine("YOU WON, OH! SHUCKS!!!!!");
            else if (compScore > youScore * 1.1)
                Console.WriteLine("YOU LOST — I CONQUERED YOUR COUNTRY. IT SERVES YOU RIGHT FOR PLAYING THIS STUPID GAME!!!");
            else
                Console.WriteLine("THE TREATY OF PARIS CONCLUDED THAT WE TAKE OUR RESPECTIVE COUNTRIES, AND LIVE IN PEACE.");
        }

        private static (int, int, int) GetDistribution()
        {
            int army = 0, navy = 0, air = 0;
            while (true)
            {
                Console.Write("HOW MANY TROOPS FOR ARMY? ");
                army = AskInt();
                Console.Write("HOW MANY TROOPS FOR NAVY? ");
                navy = AskInt();
                Console.Write("HOW MANY TROOPS FOR AIR FORCE? ");
                air = AskInt();

                if (army + navy + air <= TotalTroops) break;
                Console.WriteLine($"TOTAL MUST NOT EXCEED {TotalTroops}. TRY AGAIN.");
            }
            return (army, navy, air);
        }

        private static (int, int, int, int, int, int) DoBattle(
            int youArmy, int youNavy, int youAir,
            int compArmy, int compNavy, int compAir)
        {
            int branch = AskBranch();
            int men = AskMen();

            switch (branch)
            {
                case 1: // Army attack
                    compArmy -= men;
                    if (compArmy < 0) compArmy = 0;
                    Console.WriteLine($"YOU USED {men} MEN FROM YOUR ARMY.");
                    break;
                case 2: // Navy
                    compNavy -= men / 2;
                    Console.WriteLine($"YOU SUNK {men / 2} OF MY PATROL BOATS.");
                    break;
                case 3: // Air Force
                    compAir -= men / 3;
                    Console.WriteLine($"YOUR AIR FORCE ATTACK WIPED OUT {men / 3} OF MY PLANES.");
                    break;
            }

            PrintForces("YOU", youArmy, youNavy, youAir);
            PrintForces("ME", compArmy, compNavy, compAir);

            return (youArmy, youNavy, youAir, compArmy, compNavy, compAir);
        }

        private static int AskBranch()
        {
            while (true)
            {
                Console.Write("WHAT IS YOUR NEXT MOVE? ARMY=1, NAVY=2, AIR FORCE=3: ");
                string? s = Console.ReadLine();
                if (s == "1" || s == "2" || s == "3") return int.Parse(s);
            }
        }

        private static int AskMen()
        {
            Console.Write("HOW MANY MEN? ");
            return AskInt();
        }

        private static int AskInt()
        {
            while (true)
            {
                string? s = Console.ReadLine();
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) && v >= 0)
                    return v;
            }
        }

        private static void PrintForces(string who, int a, int n, int af)
        {
            Console.WriteLine($"{who} — ARMY: {a}   NAVY: {n}   A.F.: {af}");
        }
    }
}
