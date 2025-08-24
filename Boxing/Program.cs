using System;
using System.Collections.Generic;
using System.Linq;

namespace BoxingModern
{
    enum Punch { FullSwing = 1, Hook = 2, Uppercut = 3, Jab = 4 }

    sealed class Boxer
    {
        public string Name { get; }
        public Punch Best { get; set; }
        public Punch VulnerableTo { get; set; }

        public Boxer(string name, Punch best, Punch vulnerable)
        {
            Name = name;
            Best = best;
            VulnerableTo = vulnerable;
        }
    }

    static class Program
    {
        static readonly Random Rng = new();

        // Base accuracy & damage by punch
        // These are tuned to “feel” right: slower punches hit harder but land less often.
        static readonly Dictionary<Punch, (double acc, int dmg)> PunchModel = new()
        {
            [Punch.FullSwing] = (0.45, 4),
            [Punch.Hook]      = (0.55, 3),
            [Punch.Uppercut]  = (0.50, 3),
            [Punch.Jab]       = (0.65, 2),
        };

        // Bonuses
        const double BestPunchAccBonus = 0.10;
        const int    BestPunchDmgBonus = 1;
        const double VulnerabilityAccBonus = 0.08;
        const double RandomDefenseBlockChance = 0.25; // simple “coach says defend this” feel
        const double BlockAccPenalty = 0.20;          // if they happen to block your chosen punch

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("OLYMPIC BOXING — 3 ROUNDS\n");
            Console.WriteLine("Press Q at any prompt to quit.\n");

            // Opponent & player names
            string oppName = AskNonEmpty("INPUT YOUR OPPONENT'S NAME: ");
            string youName = AskNonEmpty("INPUT YOUR NAME: ");

            // Choose best punch & vulnerability (player)
            var yourBest = AskPunch($"{youName}, WHAT IS YOUR BEST PUNCH? (1=FULL SWING, 2=HOOK, 3=UPPERCUT, 4=JAB): ");
            var yourVuln = AskPunch($"AND WHAT IS YOUR VULNERABILITY? (1=FULL SWING, 2=HOOK, 3=UPPERCUT, 4=JAB): ");

            // Computer gets its own (random) best/vulnerability
            var oppBest = (Punch)Rng.Next(1, 5);
            var oppVuln = (Punch)Rng.Next(1, 5);

            var you = new Boxer(youName, yourBest, yourVuln);
            var opp = new Boxer(oppName, oppBest, oppVuln);

            Console.WriteLine();
            Console.WriteLine($"{opp.Name}'s ADVANTAGE (best punch) IS SECRET.");
            Console.WriteLine($"{you.Name}'s BEST: {PunchName(you.Best)}   VULNERABILITY: {PunchName(you.VulnerableTo)}\n");

            int yourRounds = 0, oppRounds = 0;

            for (int round = 1; round <= 3 && yourRounds < 2 && oppRounds < 2; round++)
            {
                Console.WriteLine($"\nROUND {round} BEGINS…");
                var (youPts, oppPts) = FightRound(you, opp, exchanges: 7);

                Console.WriteLine($"\nRound {round} points — {you.Name}: {youPts}   {opp.Name}: {oppPts}");
                if (youPts > oppPts)
                {
                    yourRounds++;
                    Console.WriteLine($"{you.Name} WINS ROUND {round}!");
                }
                else if (oppPts > youPts)
                {
                    oppRounds++;
                    Console.WriteLine($"{opp.Name} WINS ROUND {round}!");
                }
                else
                {
                    // Tie-break: one sudden-death exchange
                    Console.WriteLine("TIE! ONE MORE EXCHANGE FOR THE ROUND!");
                    var (extraYou, extraOpp) = FightRound(you, opp, exchanges: 1, header: false);
                    if (extraYou >= extraOpp) { yourRounds++; Console.WriteLine($"{you.Name} EDGES THE ROUND!"); }
                    else { oppRounds++; Console.WriteLine($"{opp.Name} EDGES THE ROUND!"); }
                }
                Console.WriteLine($"Rounds: {you.Name} {yourRounds} — {opp.Name} {oppRounds}");
            }

            Console.WriteLine();
            if (yourRounds > oppRounds)
                Console.WriteLine($"{you.Name.ToUpper()} IS THE WINNER AND CHAMP!");
            else
                Console.WriteLine($"{opp.Name.ToUpper()} IS THE WINNER AND CHAMP!");

            Console.WriteLine("\nAND NOW GOODBYE FROM THE OLYMPIC ARENA.");
        }

        static (int youPts, int oppPts) FightRound(Boxer you, Boxer opp, int exchanges, bool header = true)
        {
            if (header) Console.WriteLine("(Up to seven major punches this round.)");

            int yourPoints = 0, oppPoints = 0;
            Punch? lastYou = null;

            for (int i = 1; i <= exchanges; i++)
            {
                Console.WriteLine($"\nEXCHANGE {i}:");

                // Player chooses a punch
                var yourPunch = AskPunch($"YOUR PUNCH (1=FULL SWING, 2=HOOK, 3=UPPERCUT, 4=JAB{(lastYou is null ? "" : $", ENTER repeats {PunchName(lastYou.Value)}")}): ", lastYou);
                lastYou = yourPunch;

                // Opponent picks a punch (use its best more often)
                var oppPunch = OpponentPickPunch(opp);

                // Each side picks a “defend focus” (what they’re trying to watch for this exchange)
                var youDefend   = RandomDefendFocus();
                var oppDefend   = RandomDefendFocus();

                // Resolve your attack on them
                int youScoreThis = ResolveExchange(attacker: you, defender: opp, attackPunch: yourPunch, defenderFocus: oppDefend, attackerLabel: you.Name, defenderLabel: opp.Name);
                yourPoints += youScoreThis;

                // Resolve their attack on you
                int oppScoreThis = ResolveExchange(attacker: opp, defender: you, attackPunch: oppPunch, defenderFocus: youDefend, attackerLabel: opp.Name, defenderLabel: you.Name);
                oppPoints += oppScoreThis;
            }

            return (yourPoints, oppPoints);
        }

        static int ResolveExchange(Boxer attacker, Boxer defender, Punch attackPunch, Punch defenderFocus, string attackerLabel, string defenderLabel)
        {
            // Base model
            var (acc, dmg) = PunchModel[attackPunch];

            // Best punch bonus
            if (attackPunch == attacker.Best) { acc += BestPunchAccBonus; dmg += BestPunchDmgBonus; }

            // Defender vulnerability
            if (attackPunch == defender.VulnerableTo) acc += VulnerabilityAccBonus;

            // Did defender happen to focus on the right punch this exchange?
            if (attackPunch == defenderFocus) acc -= BlockAccPenalty;

            acc = Math.Clamp(acc, 0.05, 0.95);

            bool hit = Rng.NextDouble() < acc;
            if (hit)
            {
                int points = dmg;
                var crit = Rng.NextDouble() < 0.07; // little spice: occasional extra pop
                if (crit) points += 1;

                Console.WriteLine($"{attackerLabel.ToUpper()} THROWS A {PunchName(attackPunch).ToUpper()}… CONNECTS{(crit ? " HARD!" : "!")}");
                return points;
            }
            else
            {
                Console.WriteLine($"{attackerLabel} swings a {PunchName(attackPunch).ToLower()} and misses.");
                return 0;
            }
        }

        static Punch OpponentPickPunch(Boxer opp)
        {
            // Weighted to use best punch more often, but still mixes it up.
            int r = Rng.Next(100);
            if (r < 45) return opp.Best; // lean on best
            return (Punch)Rng.Next(1, 5);
        }

        static Punch RandomDefendFocus()
        {
            // Randomly “watching” for one type; creates occasional blocks
            return (Punch)Rng.Next(1, 5);
        }

        // ---------- input helpers ----------
        static string AskNonEmpty(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim();
                if (s.Equals("Q", StringComparison.OrdinalIgnoreCase)) Environment.Exit(0);
                if (!string.IsNullOrWhiteSpace(s)) return s;
            }
        }

        static Punch AskPunch(string prompt, Punch? repeat = null)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") Environment.Exit(0);
                if (s == "" && repeat is Punch r) return r; // Enter repeats last punch
                if (int.TryParse(s, out int n) && n >= 1 && n <= 4) return (Punch)n;

                // allow typing names
                if (s is "FULL" or "FULLSWING" or "SWING") return Punch.FullSwing;
                if (s is "HOOK") return Punch.Hook;
                if (s is "UPPERCUT") return Punch.Uppercut;
                if (s is "JAB") return Punch.Jab;

                Console.WriteLine("Enter 1=Full Swing, 2=Hook, 3=Uppercut, 4=Jab (or press Enter to repeat).");
            }
        }

        static string PunchName(Punch p) => p switch
        {
            Punch.FullSwing => "Full Swing",
            Punch.Hook      => "Hook",
            Punch.Uppercut  => "Uppercut",
            Punch.Jab       => "Jab",
            _ => "Punch"
        };
    }
}
