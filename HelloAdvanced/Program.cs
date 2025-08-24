using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace HelloSmart
{
    internal static class Program
    {
        static void Main()
        {
            Console.Title = "HELLO+ — Petey P. Eight (Smarter)";
            new Bot().Run();
        }
    }

    internal sealed class Bot
    {
        readonly Random rng = new();
        readonly Memory mem = new();

        // --- tiny lexicons ---
        readonly string[] posFeel = ["good","great","fine","ok","okay","happy","excited","better","fantastic","awesome"];
        readonly string[] negFeel = ["bad","sad","tired","sick","anxious","worried","depressed","angry","stressed","terrible","awful"];
        readonly string[] greet = ["hi","hello","hey","yo","howdy","greetings"];
        readonly string[] bye   = ["bye","goodbye","see ya","later","quit","exit"];

        // intent patterns (very small NLU)
        readonly (Intent intent, Regex rx)[] patterns = new (Intent, Regex)[]
        {
            (Intent.Yes,    new(@"^(y|yes|yeah|yup|sure|ok|okay)\b", RegexOptions.IgnoreCase)),
            (Intent.No,     new(@"^(n|no|nope|nah)\b", RegexOptions.IgnoreCase)),
            (Intent.Money,  new(@"\b(money|pay|bills?|debt|salary|raise|budget|rent|mortgage|broke)\b", RegexOptions.IgnoreCase)),
            (Intent.Job,    new(@"\b(job|work|boss|career|interview|resume|promotion|fired|quit)\b", RegexOptions.IgnoreCase)),
            (Intent.Health, new(@"\b(health|sick|ill|cold|flu|fever|pain|sleep|diet|exercise)\b", RegexOptions.IgnoreCase)),
            (Intent.Sex,    new(@"\b(sex|dating|partner|relationship|intimacy)\b", RegexOptions.IgnoreCase)),
            (Intent.Feelings,new(@"\b(feel|feeling|emotion|mood|stressed|anxious|happy|sad)\b", RegexOptions.IgnoreCase)),
            (Intent.Greeting,new(@"\b(hi|hello|hey|howdy|greetings)\b", RegexOptions.IgnoreCase)),
            (Intent.Thanks,  new(@"\b(thanks|thank you|ty)\b", RegexOptions.IgnoreCase)),
            (Intent.Help,    new(@"\b(help|advice|suggest|what should i do)\b", RegexOptions.IgnoreCase)),
        };

        // quick number/amount extractor
        readonly Regex moneyRx = new(@"\$?\s*(\d+(?:[.,]\d{1,2})?)", RegexOptions.Compiled);
        readonly Regex percentRx = new(@"\b(\d{1,3})\s*%", RegexOptions.Compiled);

        public void Run()
        {
            Say("HELLO. I'M AN EDUSYSTEM-25. CALL ME PETEY P. EIGHT.");
            mem.UserName = Ask("WHAT'S YOUR NAME? ", require: true);
            if (Quit(mem.UserName)) return;

            // small talk opener
            Say($"NICE TO MEET YOU, {mem.UserName}. HOW ARE YOU FEELING TODAY?");
            while (true)
            {
                var turn = GetTurn();
                if (turn.Quit) return;

                // high-level routing
                if (turn.Intent == Intent.Greeting) { Say(Greet()); continue; }
                if (turn.Intent == Intent.Thanks)   { Say("YOU'RE WELCOME."); continue; }
                if (bye.Any(w => turn.Raw.Contains(w, StringComparison.OrdinalIgnoreCase))) { Say("BYE! TAKE CARE."); return; }

                // update mood if present
                UpdateMood(turn.Raw);

                // topic handoff
                if (turn.Intent is Intent.Money or Intent.Job or Intent.Health or Intent.Sex)
                {
                    HandleTopic(turn.Intent, turn.Raw);
                    continue;
                }

                if (turn.Intent == Intent.Feelings)
                {
                    if (mem.Mood < 0) Say("SORRY YOU'RE NOT FEELING GREAT. WANT TO TALK HEALTH, JOB, MONEY, OR RELATIONSHIPS?");
                    else Say("GLAD TO HEAR IT! ANYTHING YOU WANT ADVICE ON — HEALTH, JOB, MONEY, OR RELATIONSHIPS?");
                    continue;
                }

                if (turn.Intent == Intent.Yes || turn.Intent == Intent.No)
                {
                    // context-aware follow-ups
                    if (mem.LastQuestion == Q.MoneyBudgetYesNo)
                    {
                        if (turn.Intent == Intent.Yes) Say("NICE! TRACKING SPENDING WEEKLY IS HALF THE BATTLE.");
                        else Say("OK — WANT A SIMPLE 50/30/20 BUDGET TEMPLATE?");
                        mem.LastQuestion = Q.None;
                        continue;
                    }
                }

                // fallback: reflective response
                Say(Reflect(turn.Raw));
                Say("WE CAN DIVE INTO HEALTH, JOB, MONEY, OR RELATIONSHIPS. WHICH?");
            }
        }

        // --- Topic handlers ---
        void HandleTopic(Intent topic, string raw)
        {
            switch (topic)
            {
                case Intent.Health:
                    HealthAdvice(raw);
                    break;
                case Intent.Job:
                    JobAdvice(raw);
                    break;
                case Intent.Money:
                    MoneyAdvice(raw);
                    break;
                case Intent.Sex:
                    SexAdvice(raw);
                    break;
            }
        }

        void HealthAdvice(string raw)
        {
            if (raw.Contains("cold", StringComparison.OrdinalIgnoreCase) || raw.Contains("flu", StringComparison.OrdinalIgnoreCase))
            {
                Say("SOUNDS LIKE A BUG: FLUIDS, REST, AND ACETAMINOPHEN/IBUPROFEN AS DIRECTED. IF FEVER > 3 DAYS, SEE A CLINIC.");
                return;
            }
            if (raw.Contains("sleep", StringComparison.OrdinalIgnoreCase) || raw.Contains("tired", StringComparison.OrdinalIgnoreCase))
            {
                Say("SLEEP HYGIENE: CONSISTENT BEDTIME, COOL DARK ROOM, NO CAFFEINE AFTER LUNCH, SCREENS OFF 1 HOUR BEFORE BED.");
                return;
            }
            Say("TELL ME A BIT MORE — SYMPTOMS, HOW LONG, AND WHAT YOU’VE TRIED?");
        }

        void JobAdvice(string raw)
        {
            if (raw.Contains("interview", StringComparison.OrdinalIgnoreCase))
            {
                Say("INTERVIEWS: PREP 3 STORIES (CHALLENGE–ACTION–RESULT), RESEARCH THE COMPANY, AND PREP 2 QUESTIONS TO ASK THEM.");
                return;
            }
            if (raw.Contains("boss", StringComparison.OrdinalIgnoreCase))
            {
                Say("TOUGH BOSSES: BOOK A 15-MIN 1:1, SHARE IMPACT NOT INTENT, ASK FOR ONE SPECIFIC CHANGE, AND FOLLOW UP IN EMAIL.");
                return;
            }
            Say("CAREER GROWTH: PICK A ROLE YOU ADMIRE, LIST 3 SKILLS THEY HAVE, PLAN 30-60-90 DAYS TO PRACTICE THEM.");
        }

        void MoneyAdvice(string raw)
        {
            // amounts or percents?
            var m = moneyRx.Match(raw);
            var p = percentRx.Match(raw);
            if (m.Success && decimal.TryParse(m.Groups[1].Value.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var amt))
            {
                Say($"OK, YOU MENTIONED ${amt:0.##}. HERE’S A QUICK SPLIT USING 50/30/20:");
                Say($"  • NEEDS: ${amt*0.50m:0.##}  • WANTS: ${amt*0.30m:0.##}  • SAVINGS/DEBT: ${amt*0.20m:0.##}");
                Say("DO YOU CURRENTLY TRACK SPENDING WEEKLY (YES/NO)?");
                mem.LastQuestion = Q.MoneyBudgetYesNo;
                return;
            }
            if (p.Success && int.TryParse(p.Groups[1].Value, out var pct))
            {
                Say($"PERCENTAGES ARE TRICKY. A RULE OF THUMB: KEEP HOUSING ≤ 30% OF TAKE-HOME; TOTAL DEBT ≤ 36%.");
                return;
            }
            Say("START SIMPLE: 50/30/20 BUDGET, AUTOMATIC TRANSFER ON PAYDAY, AND A $1000 EMERGENCY BUFFER.");
        }

        void SexAdvice(string raw)
        {
            if (raw.Contains("too much", StringComparison.OrdinalIgnoreCase))
            {
                Say("I SHOULD HAVE SUCH PROBLEMS! IF IT’S IMPACTING LIFE, SET BOUNDARIES AND FOCUS ON SHARED ACTIVITIES BEYOND ROMANCE.");
                return;
            }
            if (raw.Contains("too little", StringComparison.OrdinalIgnoreCase) || raw.Contains("not enough", StringComparison.OrdinalIgnoreCase))
            {
                Say("START WITH A CONVERSATION: FEELINGS, NOT BLAME. PLAN A LOW-PRESSURE DATE, AND CHECK IN WEEKLY.");
                return;
            }
            if (raw.Contains("dating", StringComparison.OrdinalIgnoreCase))
            {
                Say("DATING: WRITE A 2-SENTENCE BIO, ADD 3 PHOTOS WITH SMILE + ACTIVITY, AND OPEN WITH A QUESTION ABOUT THEIR PROFILE.");
                return;
            }
            Say("RELATIONSHIPS ARE COMPLEX — WANT COMMUNICATION TIPS, DATING IDEAS, OR BOUNDARY SETTING?");
        }

        // --- utilities ---
        Turn GetTurn()
        {
            Console.Write("> ");
            var raw = (Console.ReadLine() ?? "").Trim();
            if (Quit(raw)) return new(raw, Intent.None, quit: true);

            var intent = DetectIntent(raw);
            return new(raw, intent, quit: false);
        }

        Intent DetectIntent(string raw)
        {
            foreach (var (intent, rx) in patterns)
                if (rx.IsMatch(raw)) return intent;

            // strong lexical hints
            if (greet.Any(w => raw.Contains(w, StringComparison.OrdinalIgnoreCase))) return Intent.Greeting;
            if (bye.Any(w => raw.Contains(w, StringComparison.OrdinalIgnoreCase))) return Intent.Bye;

            return Intent.None;
        }

        void UpdateMood(string raw)
        {
            int score = 0;
            if (posFeel.Any(w => raw.Contains(w, StringComparison.OrdinalIgnoreCase))) score++;
            if (negFeel.Any(w => raw.Contains(w, StringComparison.OrdinalIgnoreCase))) score--;
            if (score != 0) mem.Mood = Math.Clamp(mem.Mood + score, -2, 2);
        }

        static bool Quit(string s) => s.Equals("Q", StringComparison.OrdinalIgnoreCase);

        string Greet() =>
            rng.Next(3) switch { 0 => "HI!", 1 => "HELLO THERE.", _ => "HEY — NICE TO SEE YOU." };

        string Reflect(string raw)
        {
            // ELIZA-style minimal reflection
            string s = raw;
            s = Regex.Replace(s, @"\bI am\b", "you are", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\bI'm\b", "you're", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\bmy\b", "your", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\bme\b", "you", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\bI\b", "you", RegexOptions.IgnoreCase);
            return $"TELL ME MORE ABOUT WHY {s.TrimEnd('.', '!', '?')}."; 
        }

        void Say(string text) => Console.WriteLine(text);
    }

    // --- small data structs ---
    enum Intent { None, Yes, No, Greeting, Bye, Thanks, Help, Feelings, Money, Job, Health, Sex }
    enum Q { None, MoneyBudgetYesNo }

    sealed class Memory
    {
        public string UserName { get; set; } = "FRIEND";
        public int Mood { get; set; } = 0; // -2..+2
        public Q LastQuestion { get; set; } = Q.None;
    }

    readonly record struct Turn(string Raw, Intent Intent, bool Quit);
}
