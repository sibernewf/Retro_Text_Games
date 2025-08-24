using System;
using System.Collections.Generic;
using System.Linq;

namespace Bullfight
{
    enum Move { Veronica = 0, Outside = 1, Swirl = 2, KillOverHorns = 4, KillInChest = 5 }
    enum Grade { Poor = 0, Fair = 1, Good = 2, Superb = 3 }

    sealed class Team
    {
        public Grade Picadores { get; }
        public Grade Toreadores { get; }
        public Team(Grade p, Grade t) { Picadores = p; Toreadores = t; }
        public double DefenseMultiplier
        {
            get
            {
                // Better helpers reduce gore risk slightly.
                // Poor=1.00  Fair=0.95  Good=0.90  Superb=0.85 (applied twice, once for each group)
                double m(Grade g) => g switch { Grade.Poor => 1.00, Grade.Fair => 0.95, Grade.Good => 0.90, _ => 0.85 };
                return m(Picadores) * m(Toreadores);
            }
        }
        public double KillBonus
        {
            get
            {
                // Helpers set up a cleaner kill. Small additive bonus.
                double b(Grade g) => g switch { Grade.Poor => 0.00, Grade.Fair => 0.03, Grade.Good => 0.06, _ => 0.10 };
                return b(Picadores) + b(Toreadores);
            }
        }
        public override string ToString() => $"Picadores: {Picadores}, Toreadores: {Toreadores}";
    }

    sealed class Bull
    {
        public Grade Quality { get; }
        public Bull(Grade q) { Quality = q; }
        public double AggressionMultiplier =>
            Quality switch
            {
                Grade.Poor => 0.70,
                Grade.Fair => 0.90,
                Grade.Good => 1.10,
                Grade.Superb => 1.30,
                _ => 1.0
            };
        public override string ToString() => $"{Quality} bull";
    }

    static class Program
    {
        static readonly Random Rng = new();

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("*** BULL – Bullfight Simulation ***\n");
            if (YesNo("DO YOU WANT INSTRUCTIONS? ")) PrintInstructions();

            // Setup random bull & helpers (this mirrors the “you have drawn a X bull” vibe)
            var bull = new Bull((Grade)Rng.Next(0, 4));
            var team = new Team((Grade)Rng.Next(0, 4), (Grade)Rng.Next(0, 4));

            Console.WriteLine($"\nYOU HAVE DRAWN A {bull.Quality.ToString().ToUpper()} BULL");
            Console.WriteLine(team.Picadores switch
            {
                Grade.Superb => "THE PICADORES DID A SUPERB JOB",
                Grade.Good   => "THE PICADORES DID A GOOD JOB",
                Grade.Fair   => "THE PICADORES DID A FAIR JOB",
                _            => "THE PICADORES DID A POOR JOB"
            });
            Console.WriteLine(team.Toreadores switch
            {
                Grade.Superb => "THE TOREADORES DID A SUPERB JOB",
                Grade.Good   => "THE TOREADORES DID A GOOD JOB",
                Grade.Fair   => "THE TOREADORES DID A FAIR JOB",
                _            => "THE TOREADORES DID A POOR JOB"
            });

            var history = new List<Move>();
            int pass = 0;
            bool alive = true;
            bool bullKilled = false;
            Move? last = null;

            while (alive && !bullKilled)
            {
                pass++;
                Console.WriteLine($"\nPASS NUMBER {pass}");
                Console.WriteLine("THE BULL IS CHARGING AT YOU! YOU ARE THE MATADOR—");

                // offer kill only if player wants
                bool tryKill = YesNo("DO YOU WANT TO KILL THE BULL? ");
                Move move;
                if (tryKill)
                {
                    move = AskKill(last);
                }
                else
                {
                    move = AskCapeMove(last);
                }
                last = move;
                history.Add(move);

                // Resolve outcome
                if (move is Move.KillOverHorns or Move.KillInChest)
                {
                    (bullKilled, alive) = ResolveKill(pass, bull, team, move, bravery: AverageBravery(history));
                }
                else
                {
                    alive = ResolveCape(pass, bull, team, move);
                }
            }

            // Final outcome & crowd reaction
            Console.WriteLine();
            if (!alive)
            {
                Console.WriteLine("YOU ARE DEAD");
                CrowdAwards(posthumous: true, bravery: AverageBravery(history), killedBull: false, killStyle: null);
            }
            else if (bullKilled)
            {
                Console.WriteLine("YOU KILLED THE BULL!");
                var killMove = history.Last(m => m is Move.KillInChest or Move.KillOverHorns);
                CrowdAwards(posthumous: false, bravery: AverageBravery(history), killedBull: true, killStyle: killMove);
            }

            Console.WriteLine("\nADIOS");
        }

        // ===== Resolution models =====

        static bool ResolveCape(int pass, Bull bull, Team team, Move move)
        {
            // Base gore risks for cape moves (per pass)
            double baseRisk = move switch
            {
                Move.Veronica => 0.25,   // dangerous inside
                Move.Outside  => 0.12,   // less dangerous outside
                Move.Swirl    => 0.08,   // ordinary swirl
                _ => 0.10
            };

            // Later passes calm the bull slightly (but very little for Superb bulls)
            double passFactor = Math.Max(0.70, 1.0 - Math.Min(pass, 10) * 0.03);

            // Combine multipliers
            double goreProb = baseRisk * bull.AggressionMultiplier * team.DefenseMultiplier * passFactor;
            goreProb = Math.Clamp(goreProb, 0.02, 0.80);

            // Narrative
            Console.WriteLine(move switch
            {
                Move.Veronica => "YOU TRY A VERONICA — THE DANGEROUS INSIDE MOVE!",
                Move.Outside  => "YOU PLAY IT SAFER WITH AN OUTSIDE MOVE.",
                _             => "AN ORDINARY SWIRL OF THE CAPE.",
            });

            bool gored = Rng.NextDouble() < goreProb;
            if (gored)
            {
                Console.WriteLine("THE BULL HAS GORED YOU.");
                return false; // dead
            }

            // Some color about helpers
            if (Rng.NextDouble() < 0.10 * (1 + (int)bull.Quality))
            {
                var which = (Rng.Next(2) == 0) ? "PICADORES" : "TOREADORES";
                Console.WriteLine($"THE {which} COVER BEAUTIFULLY — OLE!");
            }

            return true;
        }

        static (bool killed, bool alive) ResolveKill(int pass, Bull bull, Team team, Move move, double bravery)
        {
            Console.WriteLine("IT IS THE MOMENT OF TRUTH. HOW DO YOU TRY TO KILL THE BULL?");
            Console.WriteLine(move == Move.KillOverHorns ? "— OVER THE HORNS!" : "— IN THE CHEST!");

            // Base success rates
            double baseSucc = move switch
            {
                Move.KillOverHorns => 0.45, // flashy but tricky
                Move.KillInChest   => 0.35, // traditional estocada
                _ => 0.40
            };

            // Later passes help (the booklet warns not to try early)
            if (pass >= 7) baseSucc += 0.20;
            else baseSucc -= 0.10 * (7 - Math.Min(pass, 7)); // penalize early tries slightly

            // Helpers & bull quality modify success
            baseSucc += team.KillBonus;
            baseSucc /= bull.AggressionMultiplier; // a superb bull is harder

            // Cap & floor
            baseSucc = Math.Clamp(baseSucc, 0.05, 0.85);

            bool success = Rng.NextDouble() < baseSucc;
            if (success) return (true, true);

            // Failure: risk of being gored on the kill attempt (higher than cape work)
            double failGore = 0.35 * bull.AggressionMultiplier * team.DefenseMultiplier;
            failGore = Math.Clamp(failGore, 0.05, 0.80);
            bool gored = Rng.NextDouble() < failGore;

            if (gored)
            {
                Console.WriteLine("THE BULL HAS GORED YOU.");
                return (false, false);
            }

            Console.WriteLine("YOU MISS THE KILL — THE BULL SURGES PAST!");
            return (false, true);
        }

        // ===== Crowd & awards =====

        static void CrowdAwards(bool posthumous, double bravery, bool killedBull, Move? killStyle)
        {
            // bravery: 0..1 (based on mix of dangerous moves & early kill attempts)
            string cheer(double x) =>
                x switch
                {
                    < 0.30 => "THE CROWD BOOS.",
                    < 0.55 => "THE CROWD IS POLITE.",
                    < 0.80 => "THE CROWD CHEERS!",
                    _      => "THE CROWD ROARS ¡OLE! ¡OLE!"
                };

            Console.WriteLine(cheer(bravery));

            int ears = 0;
            if (killedBull)
            {
                // base one ear; with high bravery and over-the-horns or late chest → two
                ears = 1;
                if (bravery >= 0.60 && (killStyle == Move.KillOverHorns || killStyle == Move.KillInChest))
                    ears = 2;
            }
            else if (posthumous && bravery >= 0.55)
            {
                // “posthumously if necessary”
                ears = 1;
            }

            if (ears == 0) Console.WriteLine("NO EARS AWARDED.");
            else if (ears == 1) Console.WriteLine("THE CROWD AWARDS YOU ONE EAR OF THE BULL.");
            else Console.WriteLine("THE CROWD AWARDS YOU TWO EARS OF THE BULL.");
        }

        static double AverageBravery(List<Move> history)
        {
            if (history.Count == 0) return 0.0;
            // Assign bravery weights: inside=1.0, outside=0.6, swirl=0.4, kills=0.9 (earlier kills braver)
            double Sum = 0;
            for (int i = 0; i < history.Count; i++)
            {
                var m = history[i];
                double w = m switch
                {
                    Move.Veronica => 1.00,
                    Move.Outside  => 0.60,
                    Move.Swirl    => 0.40,
                    Move.KillOverHorns or Move.KillInChest => 0.90 * Math.Clamp(1.2 - 0.05 * i, 0.6, 1.0), // earlier = braver
                    _ => 0.5
                };
                Sum += w;
            }
            return Math.Clamp(Sum / history.Count, 0.0, 1.0);
        }

        // ===== Input helpers =====

        static bool YesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") Environment.Exit(0);
                if (s is "Y" or "YES") return true;
                if (s is "N" or "NO" or "") return false;
            }
        }

        static Move AskCapeMove(Move? last)
        {
            while (true)
            {
                Console.Write($"WHAT MOVE DO YOU MAKE WITH THE CAPE? (0=VERONICA,1=OUTSIDE,2=SWIRL){(last is Move.Veronica or Move.Outside or Move.Swirl ? ", ENTER repeats" : "")}: ");
                string s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") Environment.Exit(0);
                if (s == "" && last is Move.Veronica or Move.Outside or Move.Swirl) return last!.Value;
                if (int.TryParse(s, out int n) && (n == 0 || n == 1 || n == 2)) return (Move)n;
            }
        }

        static Move AskKill(Move? last)
        {
            while (true)
            {
                Console.Write($"CHOOSE KILL: 4=OVER THE HORNS, 5=IN THE CHEST{(last is Move.KillOverHorns or Move.KillInChest ? ", ENTER repeats" : "")}: ");
                string s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") Environment.Exit(0);
                if (s == "" && last is Move.KillOverHorns or Move.KillInChest) return last!.Value;
                if (int.TryParse(s, out int n) && (n == 4 || n == 5)) return (Move)n;
            }
        }

        static void PrintInstructions()
        {
            Console.WriteLine(@"
HELLO, ALL YOU BLOODLOVERS AND AFICIONADOS!
HERE IS YOUR BIG CHANCE TO KILL A BULL.

ON EACH PASS OF THE BULL, YOU MAY TRY:
 0 - VERONICA (DANGEROUS INSIDE MOVE OF THE CAPE)
 1 - LESS DANGEROUS OUTSIDE MOVE OF THE CAPE
 2 - ORDINARY SWIRL OF THE CAPE

INSTEAD OF THE ABOVE, YOU MAY TRY TO KILL THE BULL
ON ANY TURN: 4 (OVER THE HORNS), 5 (IN THE CHEST).
BUT IF I WERE YOU, I WOULDN'T TRY IT BEFORE THE SEVENTH PASS.

THE CROWD WILL DETERMINE WHAT AWARD YOU DESERVE—
POSTHUMOUSLY IF NECESSARY. THE BRAVER YOU ARE,
THE BETTER THE AWARD YOU RECEIVE. THE BETTER THE JOB THE
PICADORES AND TOREADORES DO, THE BETTER YOUR CHANCES.
");
        }
    }
}
