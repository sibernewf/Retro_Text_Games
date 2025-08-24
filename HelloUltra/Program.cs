using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HelloUltra;

internal static class Program
{
    static void Main()
    {
        Console.Title = "HelloUltra — Modular Chatbot";

        var engine = new BotEngine(Path.Combine(AppContext.BaseDirectory, "data"));
        if (!engine.LoadAll()) return;

        // Persona selection at startup
        if (engine.Personas.Count > 0)
        {
            var names = engine.Personas.Keys.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();

            if (names.Count == 1)
            {
                engine.SwitchPersona(names[0]);
            }
            else
            {
                Console.WriteLine("Choose someone to talk to:");
                for (int i = 0; i < names.Count; i++)
                    Console.WriteLine($"  {i + 1}. {names[i]}");
                Console.Write("Enter a number or name (Enter = default): ");

                string sel = (Console.ReadLine() ?? "").Trim();
                if (int.TryParse(sel, out int idx) && idx >= 1 && idx <= names.Count)
                {
                    engine.SwitchPersona(names[idx - 1]);
                }
                else if (!string.IsNullOrWhiteSpace(sel) && engine.Personas.ContainsKey(sel))
                {
                    engine.SwitchPersona(sel);
                }
                else
                {
                    engine.SwitchPersona(names[0]); // default to first
                }
            }
        }

        engine.LogSystem($"--- HelloUltra started. Persona={engine.ActivePersona.Name} ---");
        Console.WriteLine(engine.ActivePersona.Greeting.Render(engine.Memory));
        Console.WriteLine("Type 'help' for commands. Type 'q' to quit.\n");

        while (true)
        {
            Console.Write("> ");
            var input = (Console.ReadLine() ?? "").Trim();
            engine.LogUser(input);

            if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
            {
                engine.LogSystem("--- session end ---");
                break;
            }

            // meta commands
            if (string.Equals(input, "help", StringComparison.OrdinalIgnoreCase))
            {
                var msg = "Commands: persona <name>, reload, memory, help, q";
                Console.WriteLine(msg); engine.LogBot(msg); continue;
            }
            if (input.StartsWith("persona ", StringComparison.OrdinalIgnoreCase))
            {
                var name = input[8..].Trim();
                string msg = engine.SwitchPersona(name)
                    ? $"Persona set to {name}."
                    : "Unknown persona. Try: " + string.Join(", ", engine.Personas.Keys.OrderBy(n => n));
                Console.WriteLine(msg); engine.LogBot(msg); continue;
            }
            if (string.Equals(input, "reload", StringComparison.OrdinalIgnoreCase))
            {
                string msg = engine.LoadAll() ? "Data reloaded." : "Reload failed. Check JSON files.";
                Console.WriteLine(msg); engine.LogBot(msg); continue;
            }
            if (string.Equals(input, "memory", StringComparison.OrdinalIgnoreCase))
            {
                var msg = engine.Memory.ToDebugString();
                Console.WriteLine(msg); engine.LogBot(msg); continue;
            }

            var reply = engine.Reply(input);
            Console.WriteLine(reply);
            engine.LogBot(reply);
        }
    }
}

/* =================== Engine =================== */

internal sealed class BotEngine
{
    public readonly Memory Memory = new();

    readonly string dataDir;
    readonly string intentsDir;
    readonly string personasDir;
    readonly string responsesDir;
    readonly string transcriptPath;

    public Dictionary<string, Persona> Personas { get; private set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<IntentDef> Intents { get; private set; } = new();
    public ResponseSet Responses { get; private set; } = new();
    public Persona ActivePersona { get; private set; } = new() { Name = "Default", Greeting = "HELLO. I'M {bot}. WHAT'S YOUR NAME?" };

    // sentiment nudge words
    readonly string[] pos = ["good","great","fine","ok","okay","happy","excited","awesome","better","fantastic","pretty good"];
    readonly string[] neg = ["bad","sad","tired","sick","angry","upset","anxious","stressed","terrible","awful","down"];

    public BotEngine(string dataDir)
    {
        this.dataDir = dataDir;
        intentsDir = Path.Combine(dataDir, "intents");
        personasDir = Path.Combine(dataDir, "personas");
        responsesDir = Path.Combine(dataDir, "responses");
        transcriptPath = Path.Combine(dataDir, "transcript.txt");

        Directory.CreateDirectory(dataDir);
        Directory.CreateDirectory(intentsDir);
        Directory.CreateDirectory(personasDir);
        Directory.CreateDirectory(responsesDir);
    }

    public bool LoadAll()
    {
        try
        {
            Personas = LoadAllPersonas();
            PickDefaultPersonaIfNeeded();

            Intents = LoadAllIntents()
                        .Select(i => i.Compile())
                        .OrderByDescending(i => i.Score)
                        .ToList();

            Responses = LoadAllResponses();

            Memory["bot"] = ActivePersona.Name;
            Memory.Ensure("user", "FRIEND");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to load data: " + ex.Message);
            return false;
        }
    }

    void PickDefaultPersonaIfNeeded()
    {
        if (Personas.Count == 0) return;

        // If current active doesn't exist, pick the first (alphabetical)
        if (!Personas.ContainsKey(ActivePersona.Name))
        {
            var first = Personas.Keys.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
            if (first != null) ActivePersona = Personas[first];
        }
    }

    /* -------- Transcript -------- */
    public void LogUser(string text)   => AppendTranscript($"[{Now()}] USER({Memory.Get("user", "?")}): {text}");
    public void LogBot(string text)    => AppendTranscript($"[{Now()}] BOT({ActivePersona.Name}): {Flatten(text)}");
    public void LogSystem(string text) => AppendTranscript($"[{Now()}] SYS: {text}");

    void AppendTranscript(string line)
    {
        try { File.AppendAllText(transcriptPath, line + Environment.NewLine); }
        catch { /* ignore logging failures */ }
    }
    static string Now() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    static string Flatten(string s) => s.Replace("\r", " ").Replace("\n", " ").Trim();

    /* -------- Chat flow -------- */
    public string Reply(string input)
    {
        // capture "my name is ..."
        var m = Regex.Match(input, @"\bmy\s+name\s+is\s+([A-Za-z][\w\-']*)", RegexOptions.IgnoreCase);
        if (m.Success) Memory["user"] = m.Groups[1].Value;

        if (!Memory.Has("introduced"))
        {
            Memory["introduced"] = "1";
            return $"NICE TO MEET YOU, {Memory["user"]}. HOW ARE YOU FEELING TODAY?";
        }

        var lower = input.ToLowerInvariant();
        if (pos.Any(lower.Contains)) Memory.Mood = Math.Clamp(Memory.Mood + 1, -3, 3);
        if (neg.Any(lower.Contains)) Memory.Mood = Math.Clamp(Memory.Mood - 1, -3, 3);

        var (intent, slots, score) = MatchIntent(input);
        if (score > 0)
        {
            foreach (var kv in slots) Memory[kv.Key] = kv.Value;
            return Respond(intent, input);
        }

        return Reflect(input) + " " + AskNext();
    }

    (string intent, Dictionary<string,string> slots, int score) MatchIntent(string input)
    {
        string bestIntent = "";
        var bestSlots = new Dictionary<string,string>();
        int bestScore = 0;

        foreach (var def in Intents)
        {
            foreach (var rx in def.Compiled)
            {
                var m = rx.Match(input);
                if (!m.Success) continue;

                int score = def.Score;
                var slots = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
                foreach (var name in def.SlotNames ?? Array.Empty<string>())
                    if (m.Groups[name].Success) slots[name] = m.Groups[name].Value;

                if (score > bestScore) { bestIntent = def.Name; bestSlots = slots; bestScore = score; }
            }
        }
        return (bestIntent, bestSlots, bestScore);
    }

    string Respond(string intent, string input)
    {
        var ctx = Memory;
        string say(params string[] pool) => pool.Length == 0 ? "" : pool[Random.Shared.Next(pool.Length)].Render(ctx);

        // built-ins
        switch (intent.ToLowerInvariant())
        {
            case "greet":  return say(ActivePersona.Responses.Greet);
            case "bye":    return say(ActivePersona.Responses.Bye);
            case "thanks": return say(ActivePersona.Responses.Thanks);
            case "setname":
                Memory["user"] = Memory.Get("name", Memory["user"]);
                return $"OK, I'LL CALL YOU {Memory["user"]}. HOW CAN I HELP?";
        }

        // data-driven
        if (Responses.Intents.TryGetValue(intent, out var resp))
        {
            if (Memory.Mood <= -1 && resp.Negative?.Length > 0) return say(resp.Negative);
            if (Memory.Mood >=  1 && resp.Positive?.Length > 0) return say(resp.Positive);
            if (resp.Neutral?.Length > 0) return say(resp.Neutral);
        }

        if (ActivePersona.Fallback?.Length > 0) return say(ActivePersona.Fallback);
        return Reflect(input);
    }

    // Tweaked to avoid parroting very short inputs (e.g., "good")
    static string Reflect(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.Length <= 4 && !trimmed.Contains(' '))
            return "TELL ME MORE ABOUT THAT.";

        string s = " " + trimmed + " ";
        s = Regex.Replace(s, @"\bI'm\b", "you're", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"\bI am\b", "you are", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"\bmy\b", "your", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"\bme\b", "you", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"\bI\b", "you", RegexOptions.IgnoreCase);
        return "TELL ME MORE ABOUT WHY " + s.Trim();
    }

    string AskNext()
    {
        string[] prompts =
        {
            "WOULD YOU LIKE ADVICE ON HEALTH, MONEY, JOB, OR RELATIONSHIPS?",
            "WHAT WOULD HELP RIGHT NOW — INFORMATION, A PLAN, OR JUST TO VENT?",
            "SHALL WE FOCUS ON ONE THING AT A TIME?"
        };
        return prompts[Random.Shared.Next(prompts.Length)];
    }

    /* -------- Loaders & Mergers -------- */

    public bool SwitchPersona(string name)
    {
        if (!Personas.TryGetValue(name, out var p)) return false;
        ActivePersona = p; Memory["bot"] = p.Name; return true;
    }

    Dictionary<string, Persona> LoadAllPersonas()
    {
        var into = new Dictionary<string, Persona>(StringComparer.OrdinalIgnoreCase);

        // optional bulk file
        MergePersonasFile(Path.Combine(dataDir, "personas.json"), into);
        // per-file folder
        foreach (var f in SafeEnum(personasDir, "*.json"))
            MergePersonasFile(f, into);

        return into;
    }

    List<IntentDef> LoadAllIntents()
    {
        var into = new List<IntentDef>();

        MergeIntentsFile(Path.Combine(dataDir, "intents.json"), into);
        foreach (var f in SafeEnum(intentsDir, "*.json"))
            MergeIntentsFile(f, into);

        return into;
    }

    ResponseSet LoadAllResponses()
    {
        var merged = new ResponseSet();

        // optional bulk
        var bulkPath = Path.Combine(dataDir, "responses.json");
        if (File.Exists(bulkPath))
        {
            var bulk = LoadJson<ResponseSet>(bulkPath);
            if (bulk?.Intents != null)
                foreach (var kv in bulk.Intents) merged.Intents[kv.Key] = kv.Value;
        }

        // per-intent root files: /responses/<intent>.json
        foreach (var f in SafeEnum(responsesDir, "*.json"))
            MergeResponseFile(f, merged);

        // nested folders: /responses/<intent>/*.json
        foreach (var dir in SafeEnumDirs(responsesDir))
        {
            var intentName = Path.GetFileName(dir);
            foreach (var f in SafeEnum(dir, "*.json"))
                MergeResponseFile(f, merged, forceIntent: intentName);
        }

        return merged;
    }

    void MergeResponseFile(string path, ResponseSet into, string? forceIntent = null)
    {
        try
        {
            var rv = LoadJson<ResponseVariants>(path);
            if (rv != null && (rv.Neutral != null || rv.Positive != null || rv.Negative != null))
            {
                var key = forceIntent ?? Path.GetFileNameWithoutExtension(path);
                into.Intents[key] = MergeVariants(into.Intents.GetValueOrDefault(key), rv);
                return;
            }
        }
        catch { /* try set form below */ }

        try
        {
            var set = LoadJson<ResponseSet>(path);
            if (set?.Intents != null)
            {
                foreach (var kv in set.Intents)
                    into.Intents[kv.Key] = MergeVariants(into.Intents.GetValueOrDefault(kv.Key), kv.Value);
            }
        }
        catch { /* skip malformed */ }
    }

    static ResponseVariants MergeVariants(ResponseVariants? oldV, ResponseVariants add)
    {
        oldV ??= new ResponseVariants();
        return new ResponseVariants
        {
            Neutral  = MergeArrays(oldV.Neutral,  add.Neutral),
            Positive = MergeArrays(oldV.Positive, add.Positive),
            Negative = MergeArrays(oldV.Negative, add.Negative)
        };
    }

    static string[]? MergeArrays(string[]? a, string[]? b)
    {
        if ((a == null || a.Length == 0) && (b == null || b.Length == 0)) return a ?? b;
        var list = new List<string>();
        if (a != null) list.AddRange(a);
        if (b != null) list.AddRange(b);
        return list.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    void MergeIntentsFile(string path, List<IntentDef> into)
    {
        if (!File.Exists(path)) return;

        // array form
        try
        {
            var arr = LoadJson<List<IntentDef>>(path);
            if (arr != null && arr.Count > 0) { into.AddRange(arr); return; }
        } catch { }

        // single intent form
        try
        {
            var one = LoadJson<IntentDef>(path);
            if (one != null && !string.IsNullOrWhiteSpace(one.Name)) { into.Add(one); return; }
        } catch { }
    }

    void MergePersonasFile(string path, Dictionary<string, Persona> into)
    {
        if (!File.Exists(path)) return;

        // dict form
        try
        {
            var dict = LoadJson<Dictionary<string, Persona>>(path);
            if (dict != null && dict.Count > 0)
            {
                foreach (var kv in dict) into[kv.Key] = kv.Value;
                return;
            }
        } catch { }

        // single persona form
        try
        {
            var one = LoadJson<Persona>(path);
            if (one != null && !string.IsNullOrWhiteSpace(one.Name)) { into[one.Name] = one; return; }
        } catch { }
    }

    IEnumerable<string> SafeEnum(string dir, string pattern)
    {
        try { return Directory.EnumerateFiles(dir, pattern); }
        catch { return Array.Empty<string>(); }
    }
    IEnumerable<string> SafeEnumDirs(string dir)
    {
        try { return Directory.EnumerateDirectories(dir); }
        catch { return Array.Empty<string>(); }
    }

    T? LoadJson<T>(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}

/* =================== Models =================== */

internal sealed class Persona
{
    public string Name { get; set; } = string.Empty; // Will be set from JSON
    public string Greeting { get; set; } = string.Empty; // Set from JSON
    public PersonaResponses Responses { get; set; } = new();
    public string[] Fallback { get; set; } = Array.Empty<string>();
}

internal sealed class PersonaResponses
{
    public string[] Greet { get; set; } = Array.Empty<string>();
    public string[] Bye { get; set; } = Array.Empty<string>();
    public string[] Thanks { get; set; } = Array.Empty<string>();
}

internal sealed class IntentDef
{
    public string Name { get; set; } = "";
    public int Score { get; set; } = 10;
    public string[] Patterns { get; set; } = Array.Empty<string>();
    public string[]? SlotNames { get; set; }
    [System.Text.Json.Serialization.JsonIgnore] public List<Regex> Compiled { get; private set; } = new();
    public IntentDef Compile() { Compiled = Patterns.Select(p => new Regex(p, RegexOptions.IgnoreCase | RegexOptions.Compiled)).ToList(); return this; }
}

internal sealed class ResponseSet
{
    public Dictionary<string, ResponseVariants> Intents { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
internal sealed class ResponseVariants
{
    public string[]? Neutral { get; set; }
    public string[]? Positive { get; set; }
    public string[]? Negative { get; set; }
}

/* =================== Memory & templating =================== */

internal sealed class Memory : Dictionary<string,string>
{
    public int Mood { get; set; } = 0;
    public string Get(string key, string def="") => TryGetValue(key, out var v) ? v : def;
    public bool Has(string key) => ContainsKey(key);
    public void Ensure(string key, string value) { if (!ContainsKey(key)) this[key] = value; }
    public string ToDebugString() => $"user={Get("user","(none)")}, bot={Get("bot")}, mood={Mood}";
}
internal static class Template
{
    public static string Render(this string s, Memory mem)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return Regex.Replace(s, "{(.*?)}", m => mem.Get(m.Groups[1].Value.Trim(), m.Value));
    }
}
