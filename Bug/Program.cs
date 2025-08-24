using System;
using System.Collections.Generic;

namespace BugGame
{
    enum Part
    {
        Body = 1,      // needs nothing
        Neck = 2,      // needs Body
        Head = 3,      // needs Neck
        Feelers = 4,   // needs Head (max 2)
        Tail = 5,      // needs Body (max 1)
        Legs = 6       // needs Body (max 6)
    }

    sealed class BugState
    {
        public bool Body;
        public bool Neck;
        public bool Head;
        public int Feelers; // 0..2
        public bool Tail;
        public int Legs;    // 0..6

        public bool IsComplete =>
            Body && Neck && Head && Tail && Feelers >= 2 && Legs >= 6;
    }

    static class Program
    {
        static readonly Random Rng = new();

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("BUG! — Draw a Bug vs. the Computer");
            Console.WriteLine("The object is to finish your bug before the computer finishes its.");
            Console.WriteLine("Roll the die; each number adds a part (if allowed by the rules).");
            Console.WriteLine("Press ENTER (or type ROLL) to roll. Type Q to quit.\n");

            var you = new BugState();
            var cpu = new BugState();

            // Intro table like the original
            PrintPartLegend();

            bool yourTurn = true;
            while (true)
            {
                if (yourTurn)
                {
                    if (!PromptRoll("YOUR roll")) QuitIfQ();

                    int roll = Die();
                    Console.WriteLine($"You rolled a {roll} — {PartName((Part)roll)}");
                    bool changed = Apply(you, (Part)roll, forPlayer: true);

                    if (changed)
                        MaybeShowPictures(you, cpu, label: "YOUR BUG");

                    if (you.IsComplete)
                    {
                        Console.WriteLine("\n*** YOUR BUG IS FINISHED! ***");
                        MaybeShowPictures(you, cpu, label: "YOUR BUG");
                        Console.WriteLine("\nYou win!");
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("\nCOMPUTER roll… (press Enter to continue)"); var _ = Console.ReadLine();
                    int roll = Die();
                    Console.WriteLine($"Computer rolled a {roll} — {PartName((Part)roll)}");
                    bool changed = Apply(cpu, (Part)roll, forPlayer: false);

                    if (changed)
                        MaybeShowPictures(you, cpu, label: "THE BUGS");

                    if (cpu.IsComplete)
                    {
                        Console.WriteLine("\n*** THE COMPUTER'S BUG IS FINISHED! ***");
                        MaybeShowPictures(you, cpu, label: "THE BUGS");
                        Console.WriteLine("\nThe computer wins. Better luck next time!");
                        break;
                    }
                }

                yourTurn = !yourTurn;
            }

            Console.WriteLine("\nI hope you enjoyed the game. Play again soon!");
        }

        // ===== Core rules =====
        static bool Apply(BugState b, Part p, bool forPlayer)
        {
            // returns true if a NEW part was added (used to offer pictures)
            switch (p)
            {
                case Part.Body:
                    if (b.Body) { Say(forPlayer, "YOU DO NOT NEED A BODY"); return false; }
                    b.Body = true;   Say(forPlayer, "YOU NOW HAVE A BODY"); return true;

                case Part.Neck:
                    if (!b.Body)     { Say(forPlayer, "YOU DO NOT HAVE A BODY"); return false; }
                    if (b.Neck)      { Say(forPlayer, "YOU DO NOT NEED A NECK"); return false; }
                    b.Neck = true;   Say(forPlayer, "YOU NOW HAVE A NECK"); return true;

                case Part.Head:
                    if (!b.Neck)     { Say(forPlayer, "YOU DO NOT HAVE A NECK"); return false; }
                    if (b.Head)      { Say(forPlayer, "YOU DO NOT NEED A HEAD"); return false; }
                    b.Head = true;   Say(forPlayer, "YOU NOW HAVE A HEAD"); return true;

                case Part.Feelers:
                    if (!b.Head)            { Say(forPlayer, "YOU DO NOT HAVE A HEAD"); return false; }
                    if (b.Feelers >= 2)     { Say(forPlayer, "YOU HAVE TWO FEELERS ALREADY"); return false; }
                    b.Feelers++;            Say(forPlayer, "I GIVE YOU A FEELER"); return true;

                case Part.Tail:
                    if (!b.Body)     { Say(forPlayer, "YOU DO NOT HAVE A BODY"); return false; }
                    if (b.Tail)      { Say(forPlayer, "YOU ALREADY HAVE A TAIL"); return false; }
                    b.Tail = true;   Say(forPlayer, "I NOW GIVE YOU A TAIL"); return true;

                case Part.Legs:
                    if (!b.Body)     { Say(forPlayer, "YOU DO NOT HAVE A BODY"); return false; }
                    if (b.Legs >= 6) { Say(forPlayer, "YOU HAVE 6 FEET ALREADY"); return false; }
                    b.Legs++;        Say(forPlayer, $"YOU NOW HAVE {b.Legs} LEG(S)"); return true;
            }
            return false;
        }

        static void Say(bool forPlayer, string msg)
        {
            Console.WriteLine(forPlayer ? msg : msg.Replace("YOU ", "CPU ").Replace("I ", "CPU "));
        }

        // ===== Pictures =====
        static void MaybeShowPictures(BugState you, BugState cpu, string label)
        {
            Console.Write("DO YOU WANT THE PICTURES? (Y/N) ");
            var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            if (s.StartsWith("Y"))
            {
                Console.WriteLine($"\n**** {label} ****");
                Console.WriteLine();
                Console.WriteLine(AsciiBug(you));
                Console.WriteLine();
                Console.WriteLine(AsciiBug(cpu, title: "CPU BUG"));
                Console.WriteLine();
            }
        }

        static string AsciiBug(BugState b, string title = "YOUR BUG")
        {
            // Simple, readable figure modeled after the pamphlet’s feel.
            // We show partials based on what the player has earned.
            var lines = new List<string>();
            lines.Add($"*** {title} ***");
            // Feelers (up to 2) above the head
            if (b.Feelers >= 1) lines.Add("  A   A");
            if (b.Feelers >= 2) lines.Add("  A   A");
            // Head
            if (b.Head) lines.Add(" HHHHHH ");
            else lines.Add("   ..   ");
            // Neck
            if (b.Neck) lines.Add("   ||   ");
            else lines.Add("        ");
            // Body (wide) + legs/tail base
            if (b.Body) lines.Add("BBBBBBBB");
            else lines.Add("  ( )   ");
            // Side gaps
            lines.Add("B      B");
            // Tail (drawn as a spike below body)
            if (b.Tail) lines.Add("TTTTTTT ");
            else lines.Add("        ");

            // Legs: show up to 6 across two rows
            int legs = b.Legs;
            string LegRow(int count)
            {
                var row = new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };
                for (int i = 0; i < Math.Min(count, 4); i++) row[i * 2] = 'L';
                return new string(row);
            }
            if (legs > 0) lines.Add(LegRow(Math.Min(4, legs)));
            if (legs > 4) lines.Add(LegRow(legs - 4));

            return string.Join(Environment.NewLine, lines);
        }

        // ===== Utilities =====
        static bool PromptRoll(string caption)
        {
            Console.Write($"{caption} — press ENTER or type ROLL (Q quits): ");
            var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            if (s == "Q") Environment.Exit(0);
            return true;
        }

        static void QuitIfQ()
        {
            // just a helper to keep symmetry with prompts
        }

        static int Die() => Rng.Next(1, 7);
        static string PartName(Part p) => p switch
        {
            Part.Body    => "BODY",
            Part.Neck    => "NECK",
            Part.Head    => "HEAD",
            Part.Feelers => "FEELERS",
            Part.Tail    => "TAIL",
            Part.Legs    => "LEGS",
            _ => "PART"
        };

        static void PrintPartLegend()
        {
            Console.WriteLine("NUMBER  PART       NUMBER OF PART NEEDED");
            Console.WriteLine("1       BODY       1");
            Console.WriteLine("2       NECK       1");
            Console.WriteLine("3       HEAD       1");
            Console.WriteLine("4       FEELERS    2");
            Console.WriteLine("5       TAIL       1");
            Console.WriteLine("6       LEGS       6");
            Console.WriteLine();
        }
    }
}
