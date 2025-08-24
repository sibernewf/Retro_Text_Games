using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

enum Team { Harvard, Cornell }
enum OutcomeKind { Goal, Post, Wide, Blocked, Saved }

record Weighted<T>(T Value, double Weight);
record ShotOutcome(
    [property: JsonConverter(typeof(JsonStringEnumConverter))] OutcomeKind Kind,
    double Weight,
    string[] Lines);

record ShotType(
    string Key,
    string Display,
    ShotOutcome[] Outcomes,
    double PowerPlayGoalBoost = 0.0,   // e.g. 0.05
    double PowerPlaySaveDrop = 0.0,    // e.g. 0.02
    double PowerPlayBlockDrop = 0.0    // e.g. 0.03
);

record Penalty(
    string Key,
    string Display,
    double Chance,                     // 0..1 per possession
    int Plays,                         // how many “turns” power play lasts
    string[] LinesWhenCalled
);

record PeriodConfig(int Plays);
record FaceoffText(string[] LinesHarvard, string[] LinesCornell);
record ScoreText(string Format); // e.g. "SCORE:  HARVARD {H}   CORNELL {C}"

record Strings(
    string Intro,
    string PromptShot,
    string PromptShotShort,
    string InvalidShot,
    string PeriodStartFmt,             // "***** START OF {0} PERIOD *****"
    string PeriodEndFmt,               // "***** END OF {0} PERIOD *****"
    string OvertimeStart,
    string OvertimeLoop,
    string GameEnd,
    ScoreText Score,
    FaceoffText Faceoff
);

record Config(
    string HomeTeam,                   // "Harvard"
    string AwayTeam,                   // "Cornell"
    PeriodConfig[] Periods,            // 3 periods (turn counts)
    bool SuddenDeathOvertime,
    ShotType[] Shots,
    Penalty[] Penalties,
    double IcingChance,                // 0..1 per possession
    string[] IcingLines,
    Strings Text
);

static class R
{
    static readonly Random rng = Random.Shared;

    public static T Pick<T>(IReadOnlyList<Weighted<T>> items)
    {
        var total = items.Sum(x => x.Weight);
        var r = rng.NextDouble() * total;
        foreach (var i in items)
        {
            r -= i.Weight;
            if (r <= 0) return i.Value;
        }
        return items[^1].Value;
    }

    public static T Pick<T>(IReadOnlyList<T> items) => items[rng.Next(items.Count)];
    public static bool Chance(double p) => rng.NextDouble() < p;
}

internal static class Program
{
    static Config cfg = null!;
    static int hScore, cScore;
    static int hPP, cPP; // power-play plays left

    static void Main()
    {
        Console.Title = "HOCKEY — Data-Driven";
        cfg = LoadConfig("data/config.json");

        Console.WriteLine(cfg.Text.Intro.Replace("{home}", cfg.HomeTeam).Replace("{away}", cfg.AwayTeam));
        Console.WriteLine();

        hScore = cScore = 0;
        hPP = cPP = 0;

        for (int p = 0; p < cfg.Periods.Length; p++)
        {
            var periodName = p switch { 0 => "FIRST", 1 => "SECOND", 2 => "THIRD", _ => $"P{p + 1}" };
            Console.WriteLine(string.Format(cfg.Text.PeriodStartFmt, periodName));
            PlaySegment(cfg.Periods[p].Plays, suddenDeath: false);
            Console.WriteLine(string.Format(cfg.Text.PeriodEndFmt, periodName));
            ShowScore();
            Console.WriteLine();
        }

        if (cfg.SuddenDeathOvertime && hScore == cScore)
        {
            Console.WriteLine(cfg.Text.OvertimeStart);
            while (hScore == cScore)
            {
                PlaySegment(999, suddenDeath: true);
                if (hScore == cScore) Console.WriteLine(cfg.Text.OvertimeLoop);
            }
        }

        Console.WriteLine(cfg.Text.GameEnd);
        ShowScore();
        Console.WriteLine(hScore > cScore ? $"{cfg.HomeTeam.ToUpper()} WINS!" : $"{cfg.AwayTeam.ToUpper()} WINS!");
        Console.WriteLine("READY");
    }

    static void PlaySegment(int plays, bool suddenDeath)
    {
        var poss = Faceoff();
        int turns = 0;

        while (turns < plays || suddenDeath)
        {
            turns++;

            if (hPP > 0) hPP--;
            if (cPP > 0) cPP--;

            // Icing?
            if (R.Chance(cfg.IcingChance))
            {
                Console.WriteLine(R.Pick(cfg.IcingLines));
                poss = Opp(poss);
                continue;
            }

            // Penalties
            bool penaltyOccurred = false;
            foreach (var pen in cfg.Penalties)
            {
                if (!R.Chance(pen.Chance)) continue;
                Console.WriteLine(R.Pick(pen.LinesWhenCalled));
                if (poss == Team.Harvard) cPP += pen.Plays; else hPP += pen.Plays;
                poss = Opp(poss);
                penaltyOccurred = true;
                break;
            }
            if (penaltyOccurred) { if (!suddenDeath && turns >= plays) break; else continue; }

            // Shot
            if (poss == Team.Harvard)
            {
                int pp = hPP > 0 ? 1 : 0;
                var shot = AskShot();
                if (shot is null) Environment.Exit(0);

                poss = Resolve(Team.Harvard, shot!, pp);   // capture next possession
                if (suddenDeath && hScore != cScore) return;
            }
            else
            {
                int pp = cPP > 0 ? 1 : 0;
                // Cornell logic: slightly prefer wrist unless on PP
                var s = (pp == 1)
                  ? cfg.Shots.OrderByDescending(x => x.PowerPlayGoalBoost).First()
                  : cfg.Shots.First(x => x.Key == "wrist");
                Console.WriteLine($"CORNELL SHOT — {s.Display.ToUpper()}");

                poss = Resolve(Team.Cornell, s, pp);       // capture next possession
                if (suddenDeath && hScore != cScore) return;
            }

            if (!suddenDeath && turns >= plays) break;
        }
    }

    static Team Faceoff()
    {
        Console.WriteLine("HERE IS THE FACE-OFF:");
        var who = R.Chance(0.5) ? Team.Harvard : Team.Cornell;
        Console.WriteLine(who == Team.Harvard ? R.Pick(cfg.Text.Faceoff.LinesHarvard)
                                              : R.Pick(cfg.Text.Faceoff.LinesCornell));
        Console.WriteLine();
        return who;
    }

    static Team Resolve(Team who, ShotType shot, int powerPlay)
    {
        var weighted = new List<Weighted<ShotOutcome>>(shot.Outcomes.Length);
        foreach (var o in shot.Outcomes)
        {
            double w = o.Weight;
            if (powerPlay == 1)
            {
                if (o.Kind == OutcomeKind.Goal)   w += shot.PowerPlayGoalBoost;
                if (o.Kind == OutcomeKind.Saved)  w = Math.Max(0, w - shot.PowerPlaySaveDrop);
                if (o.Kind == OutcomeKind.Blocked)w = Math.Max(0, w - shot.PowerPlayBlockDrop);
            }
            weighted.Add(new Weighted<ShotOutcome>(o, w));
        }

        var outcome = R.Pick(weighted);
        Console.WriteLine(R.Pick(outcome.Lines));

        switch (outcome.Kind)
        {
            case OutcomeKind.Goal:
                if (who == Team.Harvard) hScore++; else cScore++;
                // New faceoff decides next possession:
                var next = Faceoff();
                return next;

            case OutcomeKind.Blocked:
            case OutcomeKind.Saved:
                // Turnover to the other team:
                Console.WriteLine();
                return Opp(who);

            case OutcomeKind.Post:
            case OutcomeKind.Wide:
                // 50/50 scrum:
                bool homeKeeps = R.Chance(0.5);
                Console.WriteLine(homeKeeps ? $"{cfg.HomeTeam.ToUpper()} REGAINS THE PUCK."
                                             : $"{cfg.AwayTeam.ToUpper()} REGAINS THE PUCK.");
                Console.WriteLine();
                return homeKeeps ? Team.Harvard : Team.Cornell;

            default:
                return Opp(who);
        }
    }

    static ShotType? AskShot()
    {
        Console.Write($"{cfg.Text.PromptShot} {ListShots()} {cfg.Text.PromptShotShort}: ");
        var s = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
        if (s == "q") return null;

        var map = cfg.Shots.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);
        if (map.TryGetValue(s, out var shot)) return shot;

        Console.WriteLine(cfg.Text.InvalidShot);
        return AskShot();
    }

    static string ListShots() => string.Join(", ", cfg.Shots.Select(s => $"{s.Display} [{s.Key}]"));

    static void ShowScore()
        => Console.WriteLine(cfg.Text.Score.Format.Replace("{H}", hScore.ToString()).Replace("{C}", cScore.ToString()));

    static Team Opp(Team t) => t == Team.Harvard ? Team.Cornell : Team.Harvard;

    static Config LoadConfig(string path)
    {
        var json = File.ReadAllText(path);
        var cfg = JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        }) ?? throw new Exception("config.json missing/invalid.");
        return cfg;
    }
}
