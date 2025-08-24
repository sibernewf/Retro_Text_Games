using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;

internal static class Program
{
    static readonly string[] Names =
    {
        "MAN O' WAR", "CITATION", "WHIRLAWAY", "ASSAULT",
        "SEABISCUIT", "GALLANT FOX", "STYVIE", "COALTOWN"
    };

    enum BetType { Win = 1, Place = 2, Show = 3 }

    record Bet(int HorseIndex, BetType Type, decimal Amount);

    static void Main()
    {
        Console.Title = "HORSE RACE — Betting at a Horserace";
        Console.WriteLine("HORSE RACE");
        Console.WriteLine("EXAMPLE OF BET: 1,2,200.0  (Horse 1, Place, $200)");
        Console.WriteLine();
        Console.WriteLine("SEVENTH — 1 MILE, 3 YR. OLDS       POST 2:35\n");

        while (true)
        {
            var (strength, orderKeys) = GenerateField();
            PrintProgram();

            // --- Take bets ---
            var bets = new List<Bet>();
            Console.WriteLine("ENTER HORSE(1-8), TO WIN, PLACE, SHOW(1,2,3) AND THE WAGER,");
            Console.WriteLine("END 0 FOR NO MORE BETTING OR 1 OR MORE BETTING.\n");

            while (true)
            {
                Console.Write("BET NO. ");
                var ok = ParseBet(out var bet);
                if (!ok) { Console.WriteLine("  (ENDING BETTING.)"); break; }
                if (bet is null) continue; // invalid, re-ask
                bets.Add(bet);
            }

            // --- Run the race ---
            var timeline = SimulateRace(strength);
            PrintTimeline(timeline);

            var finish = timeline[^1]; // final snapshot
            var podium = finish.OrderBy(x => x.lengthsBehind).Select(x => x.index).Take(3).ToArray();
            // --- Compute payouts ---
            var payouts = ComputePayouts(strength, podium);

            // --- Print mutuels paid (Win horse shows all three; second shows place+show; third shows show) ---
            Console.WriteLine("MUTUELS PAID:");
            Console.WriteLine($"{"",2}{Names[podium[0]],-12}  STRAIGHT {payouts.win[podium[0]]:0.00}   PLACE {payouts.place[podium[0]]:0.00}   SHOW {payouts.show[podium[0]]:0.00}");
            Console.WriteLine($"{"",2}{Names[podium[1]],-12}  {"",8} {"",8}   PLACE {payouts.place[podium[1]]:0.00}   SHOW {payouts.show[podium[1]]:0.00}");
            Console.WriteLine($"{"",2}{Names[podium[2]],-12}  {"",8} {"",8}   {"",5} {"",7}   SHOW {payouts.show[podium[2]]:0.00}");
            Console.WriteLine();

            // --- Grade tickets ---
            decimal totalWin = 0m, totalLoss = 0m;
            int n = 0;
            foreach (var b in bets)
            {
                n++;
                var label = $"BET NO. {n}";
                decimal payoff = 0m;
                bool winner = false;

                if (b.Type == BetType.Win && b.HorseIndex == podium[0])
                {
                    payoff = payouts.win[b.HorseIndex];
                    winner = true;
                }
                else if (b.Type == BetType.Place && (b.HorseIndex == podium[0] || b.HorseIndex == podium[1]))
                {
                    payoff = payouts.place[b.HorseIndex];
                    winner = true;
                }
                else if (b.Type == BetType.Show && (b.HorseIndex == podium[0] || b.HorseIndex == podium[1] || b.HorseIndex == podium[2]))
                {
                    payoff = payouts.show[b.HorseIndex];
                    winner = true;
                }

                if (winner)
                {
                    var ret = payoff / 2m * b.Amount; // scale from $2 base
                    totalWin += ret;
                    Console.WriteLine($"{label}  YOU COLLECT {ret:0.00} ON {Names[b.HorseIndex]}");
                }
                else
                {
                    totalLoss += b.Amount;
                    Console.WriteLine($"{label}  TEAR UP YOUR TICKET ON {Names[b.HorseIndex]}");
                }
            }

            if (bets.Count == 0)
                Console.WriteLine("NO BETS WERE PLACED.");
            else
                Console.WriteLine($"\nYOUR TOTAL {(totalWin >= totalLoss ? "WINNINGS" : "LOSSES")} AMOUNT TO {(totalWin >= totalLoss ? totalWin : totalLoss):0.00}");

            Console.WriteLine();
            if (!AskYesNo("RUN ANOTHER RACE (YES OR NO)? ")) break;
            Console.WriteLine();
        }

        Console.WriteLine("READY");
    }

    // ----- Bets -----

    static bool ParseBet(out Bet? bet)
    {
        bet = null;
        var s = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(s) || s == "0") return false; // end

        var parts = s.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3 ||
            !int.TryParse(parts[0], out int horse) ||
            !int.TryParse(parts[1], out int pos) ||
            !decimal.TryParse(parts[2], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amt))
        {
            Console.WriteLine("  PLEASE ENTER: horse(1-8), position(1=win,2=place,3=show), wager");
            return ParseBet(out bet);
        }

        if (horse < 1 || horse > 8 || pos < 1 || pos > 3 || amt < 2m || amt > 300m)
        {
            Console.WriteLine("  ILLEGAL BET. HORSE 1..8, POS 1..3, WAGER $2..$300.");
            return ParseBet(out bet);
        }

        bet = new Bet(horse - 1, (BetType)pos, Math.Round(amt, 2));
        return true;
    }

    // ----- Race model -----

    // Returns: strengths (probabilities) and an internal rank key order
    static (double[] strength, int[] key) GenerateField()
    {
        var strengths = new double[8];
        var rnd = Random.Shared;

        // Give each horse a base strength (log-normal-ish)
        for (int i = 0; i < 8; i++)
        {
            // Center around 1.0 with variance; a couple of longshots, a few contenders.
            var u = rnd.NextDouble() * 2 - 1; // -1..1
            strengths[i] = Math.Exp(u * 0.9);
        }
        // Normalize to probabilities
        var sum = strengths.Sum();
        for (int i = 0; i < strengths.Length; i++) strengths[i] /= sum;

        var key = Enumerable.Range(0, 8).OrderByDescending(i => strengths[i]).ToArray();
        return (strengths, key);
    }

    record Snap(int index, double lengthsBehind);

    static List<List<Snap>> SimulateRace(double[] strength)
    {
        var rnd = Random.Shared;
        // We simulate fractional distances — lengthsBehind is (leader distance - horse distance) in lengths.
        // 5 checkpoints (gate, 1/4, 1/2, turn (3/4), stretch, finish)
        var checkpoints = new[] { "THEY'RE OFF AND RUNNING -",
                                  "THE 1/4 MILE POLE -",
                                  "NEARING THE HALFWAY MARK -",
                                  "ROUNDING THE TURN -",
                                  "COMING DOWN THE STRETCH -",
                                  "FINISH" };

        // Each segment contributes distance = base on strength + randomness
        double[] dist = new double[8];
        var timeline = new List<List<Snap>>();

        for (int seg = 0; seg < checkpoints.Length; seg++)
        {
            for (int i = 0; i < 8; i++)
            {
                var noise = (rnd.NextDouble() * 2 - 1) * 0.25; // small segment variance
                var burst = (seg == 4 && rnd.NextDouble() < 0.25) ? rnd.NextDouble() * 0.5 : 0.0; // late burst possibility
                dist[i] += strength[i] * 6.0 + noise + burst;
            }

            // Convert distances to "lengths behind" the leader
            double lead = dist.Max();
            var snap = new List<Snap>(8);
            for (int i = 0; i < 8; i++)
            {
                var lb = (lead - dist[i]) * 3.0; // scale to lengths
                snap.Add(new Snap(i, Math.Max(0, lb)));
            }

            // Order by current position
            snap = snap.OrderBy(s => s.lengthsBehind).ToList();
            timeline.Add(snap);
        }

        return timeline;
    }

    static void PrintProgram()
    {
        Console.WriteLine("POS.  HORSE");
        for (int i = 0; i < Names.Length; i++)
            Console.WriteLine($"{i + 1,2}.  {Names[i]}");
        Console.WriteLine();
    }

    static void PrintTimeline(List<List<Snap>> timeline)
    {
        string[] headers =
        {
            "THEY'RE OFF AND RUNNING -",
            "THE 1/4 MILE POLE -",
            "NEARING THE HALFWAY MARK -",
            "ROUNDING THE TURN -",
            "COMING DOWN THE STRETCH -",
            "FINISH"
        };

        for (int s = 0; s < timeline.Count; s++)
        {
            Console.WriteLine(headers[s]);
            Console.WriteLine("POS.  HORSE            LENGTHS BEHIND");
            int pos = 1;
            foreach (var sn in timeline[s])
                Console.WriteLine($"{pos++,2}   {Names[sn.index],-14} {sn.lengthsBehind:0.0}");
            Console.WriteLine();
        }
    }

    // Payouts based on implied probability model.
    static (Dictionary<int, decimal> win,
            Dictionary<int, decimal> place,
            Dictionary<int, decimal> show)
        ComputePayouts(double[] strength, int[] podium)
    {
        var win = new Dictionary<int, decimal>();
        var place = new Dictionary<int, decimal>();
        var show = new Dictionary<int, decimal>();

        for (int i = 0; i < 8; i++)
        {
            double p = Math.Clamp(strength[i], 0.01, 0.80);
            double decOdds = (1.0 - p) / p;            // decimal odds (excluding stake)
            double basePay = 2.0 + 2.0 * decOdds;      // $2 win ticket (naive mutuel)
            var winPay = (decimal)Math.Round(basePay, 2);

            // Place & show scaled; always >= $2.10 typical floor, cap to reasonable ranges
            decimal placePay = (decimal)Math.Round(Math.Max(2.1, basePay * 0.45), 2);
            decimal showPay  = (decimal)Math.Round(Math.Max(2.1, basePay * 0.33), 2);

            win[i] = winPay;
            place[i] = placePay;
            show[i] = showPay;
        }

        // Only podium horses actually pay; others are printed only if needed per ticket.
        // (We still keep values for grading bets.)
        return (win, place, show);
    }

    static bool AskYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim();
            if (s.Length == 0) continue;
            char c = char.ToUpperInvariant(s[0]);
            if (c == 'Y') return true;
            if (c == 'N') return false;
        }
    }
}
