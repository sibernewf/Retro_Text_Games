using System;

enum Team { Harvard, Cornell }

internal static class Program
{
    static readonly Random Rng = Random.Shared;

    static void Main()
    {
        Console.Title = "HOCKEY — vs. Cornell";
        Intro();

        int hScore = 0, cScore = 0;

        for (int period = 1; period <= 3; period++)
        {
            (hScore, cScore) = PlayPeriod(period, hScore, cScore);
            Console.WriteLine($"***** END OF {(period == 1 ? "FIRST" : period == 2 ? "SECOND" : "THIRD")} PERIOD *****");
            Console.WriteLine($"SCORE:  HARVARD {hScore}   CORNELL {cScore}\n");
        }

        // Sudden death if tied
        if (hScore == cScore)
        {
            Console.WriteLine("***** START OF SUDDEN-DEATH OVERTIME *****\n");
            while (hScore == cScore)
            {
                (hScore, cScore) = PlayPeriod(0, hScore, cScore, playsThisPeriod: 999); // play until someone scores
                if (hScore == cScore) Console.WriteLine("...STILL TIED — WE PLAY ON!\n");
            }
        }

        Console.WriteLine("***** END OF GAME *****");
        Console.WriteLine($"FINAL SCORE:  HARVARD {hScore}   CORNELL {cScore}");
        Console.WriteLine(hScore > cScore ? "HARVARD WINS!" : "CORNELL WINS!");
        Console.WriteLine("READY");
    }

    static void Intro()
    {
        Console.WriteLine("N.B. THIS PROGRAM IS DESIGNED FOR THOSE WHO KNOW NOTHING");
        Console.WriteLine("ABOUT HOCKEY LIKE MYSELF.  — C. BUTTREY");
        Console.WriteLine("THIS IS CORNELL v. HOCKEY.");
        Console.WriteLine("I AM CORNELL. WHO ARE YOU?  HARVARD");
        Console.WriteLine("YOU HAVE THREE SHOTS:");
        Console.WriteLine("  1. SLAP SHOT");
        Console.WriteLine("  2. FLICK SHOT");
        Console.WriteLine("  3. WRIST SHOT\n");
    }

    static (int hScore, int cScore) PlayPeriod(int period, int hScore, int cScore, int playsThisPeriod = 32)
    {
        int hPP = 0, cPP = 0; // "plays" of power play remaining
        Team poss = Faceoff();

        int plays = 0;
        while (playsThisPeriod == 999 || plays < playsThisPeriod)
        {
            plays++;

            // decrement power plays each possession
            if (hPP > 0) hPP--;
            if (cPP > 0) cPP--;

            // Penalties / icing chance before a shot
            if (TryIcing(poss))
            {
                Console.WriteLine("ICING PENALTY...  OPPONENT REGAINS THE PUCK.\n");
                poss = Opp(poss);
                continue;
            }
            if (TryHighStick(poss, out int ppPlays))
            {
                Console.WriteLine("PENALTY FOR HIGH STICKING!  MAN PUT IN PENALTY BOX FOR 2 MINUTES (SIMULATED).");
                if (poss == Team.Harvard) cPP += ppPlays; else hPP += ppPlays;
                poss = Opp(poss);
                continue;
            }

            if (poss == Team.Harvard)
            {
                // Ask for user's shot
                int shot = AskShot();
                if (shot == -1) Environment.Exit(0);

                var outcome = ResolveShot(Team.Harvard, shot, hPP > 0);
                NarrateOutcome(outcome, Team.Harvard);

                switch (outcome.Kind)
                {
                    case ShotResultKind.Goal:
                        hScore++;
                        poss = Faceoff();
                        if (playsThisPeriod == 999 && hScore != cScore) return (hScore, cScore); // sudden death
                        break;
                    case ShotResultKind.Blocked:
                    case ShotResultKind.Saved:
                        poss = Team.Cornell; break;
                    case ShotResultKind.Post:
                    case ShotResultKind.Wide:
                        poss = Rng.Next(2) == 0 ? Team.Harvard : Team.Cornell; break;
                }
            }
            else
            {
                // Cornell possession — choose shot
                int shot = CornellSelectShot(hPP > 0, cPP > 0);
                string shotName = shot switch { 1 => "SLAP SHOT.", 2 => "FLICK SHOT.", _ => "WRIST SHOT." };
                Console.WriteLine($"CORNELL SHOT — {shotName}");

                var outcome = ResolveShot(Team.Cornell, shot, cPP > 0);
                NarrateOutcome(outcome, Team.Cornell);

                switch (outcome.Kind)
                {
                    case ShotResultKind.Goal:
                        cScore++;
                        poss = Faceoff();
                        if (playsThisPeriod == 999 && hScore != cScore) return (hScore, cScore);
                        break;
                    case ShotResultKind.Blocked:
                    case ShotResultKind.Saved:
                        poss = Team.Harvard; break;
                    case ShotResultKind.Post:
                    case ShotResultKind.Wide:
                        poss = Rng.Next(2) == 0 ? Team.Harvard : Team.Cornell; break;
                }
            }

            // Period ends only in regulation mode
            if (playsThisPeriod != 999 && plays >= playsThisPeriod) break;
        }

        return (hScore, cScore);
    }

    static Team Faceoff()
    {
        Console.WriteLine("HERE IS THE FACE-OFF:");
        Team winner = Rng.Next(2) == 0 ? Team.Harvard : Team.Cornell;
        Console.WriteLine(winner == Team.Harvard ? "HARVARD WINS THE FACE-OFF.\n" : "CORNELL WINS THE FACE-OFF.\n");
        return winner;
    }

    static bool TryIcing(Team t)
    {
        // ~6% chance icing on the team with the puck
        if (Rng.NextDouble() < 0.06)
        {
            Console.WriteLine("ICING PENALTY.");
            return true;
        }
        return false;
    }

    static bool TryHighStick(Team t, out int ppPlays)
    {
        // ~4% chance high sticking → 6 plays power play
        if (Rng.NextDouble() < 0.04)
        {
            ppPlays = 6;
            return true;
        }
        ppPlays = 0; return false;
    }

    static int AskShot()
    {
        while (true)
        {
            Console.Write("YOUR SHOT? (1=SLAP, 2=FLICK, 3=WRIST, q=quit): ");
            var s = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
            if (s == "q") return -1;
            if (s == "1" || s == "2" || s == "3") return int.Parse(s);
            Console.WriteLine("PLEASE INPUT EITHER '1', '2', OR '3'.");
        }
    }

    // Cornell prefers safer wrist unless on power play or trailing
    static int CornellSelectShot(bool harvardPP, bool cornellPP)
    {
        double r = Rng.NextDouble();
        if (cornellPP)
        {
            if (r < 0.5) return 1;  // slap
            if (r < 0.8) return 3;  // wrist
            return 2;               // flick
        }
        else
        {
            if (r < 0.20) return 1;
            if (r < 0.35) return 2;
            return 3;
        }
    }

    // Shot model: returns probabilistic outcome.
    static ShotResult ResolveShot(Team shooter, int shotType, bool powerPlay)
    {
        // Base probabilities for (goal, post, wide, blocked, saved)
        // Sums to ~1.0
        (double g, double post, double wide, double block, double save) p = shotType switch
        {
            1 => (0.15, 0.08, 0.38, 0.18, 0.21), // Slap: strong, more wide/post
            2 => (0.10, 0.03, 0.42, 0.27, 0.18), // Flick: quick, often blocked or wide
            _ => (0.12, 0.04, 0.35, 0.25, 0.24), // Wrist: accurate, more saves/blocks
        };

        // Power play mild boost to goals, reduce saves/blocks
        if (powerPlay)
        {
            p.g += 0.05; p.block -= 0.03; p.save -= 0.02;
            p = Normalize(p);
        }

        double r = Rng.NextDouble();
        if ((r -= p.g) <= 0)    return new(ShotResultKind.Goal);
        if ((r -= p.post) <= 0) return new(ShotResultKind.Post);
        if ((r -= p.wide) <= 0) return new(ShotResultKind.Wide);
        if ((r -= p.block) <= 0) return new(ShotResultKind.Blocked);
        return new(ShotResultKind.Saved);

        static (double,double,double,double,double) Normalize((double g,double post,double wide,double block,double save) p)
        {
            double s = p.g + p.post + p.wide + p.block + p.save;
            return (p.g/s, p.post/s, p.wide/s, p.block/s, p.save/s);
        }
    }

    static void NarrateOutcome(ShotResult res, Team shooter)
    {
        switch (res.Kind)
        {
            case ShotResultKind.Goal:
                Console.WriteLine("***** GOAL! *****\n");
                break;
            case ShotResultKind.Post:
                Console.WriteLine("SHOT HITS THE POST.\n");
                break;
            case ShotResultKind.Wide:
                Console.WriteLine(Rng.Next(2)==0 ? "SHOT IS WIDE." : "SHOT IS BARELY WIDE...\n");
                break;
            case ShotResultKind.Blocked:
                Console.WriteLine("SHOT IS BLOCKED BY THE GOALIE.\n");
                break;
            case ShotResultKind.Saved:
                Console.WriteLine("SHOT IS TAKEN BY THE GOALIE.\n");
                break;
        }
    }

    static Team Opp(Team t) => t == Team.Harvard ? Team.Cornell : Team.Harvard;
}

readonly record struct ShotResult(ShotResultKind Kind);
enum ShotResultKind { Goal, Post, Wide, Blocked, Saved }
