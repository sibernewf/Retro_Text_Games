using System;
using System.Globalization;

namespace Ugly
{
    internal static class Program
    {
        // "Dimensions" – you can think of these roughly as: 
        // A: head height scale, B: face length scale, C: nose/lips/chin emphasis.
        // Ranges are picked to be playful and generate varied silhouettes.
        private static readonly Random Rng = new Random();
        private const int MinA = 18, MaxA = 28;  // head/height-ish
        private const int MinB = 24, MaxB = 52;  // face length-ish
        private const int MinC = 12, MaxC = 30;  // feature emphasis

        static void Main()
        {
            Console.WriteLine("PROGRAM 'UGLY'\n");
            var mode = AskInt("DO YOU WANT CHANCE(1), OR SPECIAL(2)? ", 1, 2);

            int A, B, C;
            if (mode == 1)
            {
                (A, B, C) = Roll();
                Console.WriteLine($"A={A}   B={B}   C={C}");
                var keep = AskInt("DO YOU STILL WANT CHANCE -- 1 FOR YES, 2 FOR NO? ", 1, 2);
                if (keep == 2)
                {
                    (A, B, C) = AskABC();
                }
            }
            else
            {
                (A, B, C) = AskABC();
            }

            Console.WriteLine();
            DrawProfile(A, B, C);
            Console.WriteLine();

            // “Verdict” (playful, not serious): very out-of-range ratios earn a loud UGLY;
            // otherwise we leave it open like the original did sometimes.
            var verdict = Score(A, B, C);
            if (verdict >= 2)
            {
                for (int i = 0; i < 8; i++)
                    Console.WriteLine("UGLY! UGLY! UGLY! UGLY! UGLY! UGLY! UGLY! UGLY!");
            }
            else if (verdict == 1)
            {
                Console.WriteLine("…hmm. YOUR CALL.");
            }
            else
            {
                Console.WriteLine("LOOKIN' GOOD!");
            }
        }

        private static (int A, int B, int C) Roll()
            => (Rng.Next(MinA, MaxA + 1), Rng.Next(MinB, MaxB + 1), Rng.Next(MinC, MaxC + 1));

        private static (int A, int B, int C) AskABC()
        {
            Console.WriteLine("WHAT ARE YOUR VALUES FOR A, B, AND C?");
            int A = AskAnyInt("  A = ");
            int B = AskAnyInt("  B = ");
            int C = AskAnyInt("  C = ");
            return (A, B, C);
        }

        private static int AskInt(string prompt, int lo, int hi)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int v) && v >= lo && v <= hi)
                    return v;
            }
        }

        private static int AskAnyInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), NumberStyles.Integer,
                                  CultureInfo.InvariantCulture, out int v))
                    return v;
            }
        }

        // ---------------- RENDERING ----------------

        private static void DrawProfile(int A, int B, int C)
        {
            // Layout constants (tuned for modern consoles)
            int left = 10;                          // left margin / TAB
            int headH = Clamp(A, 12, 34);           // rows for hair/head
            int faceL = Clamp(B / 2, 10, 26);       // how far face protrudes
            int feat  = Clamp(C / 3, 4, 14);        // feature emphasis (nose/lips/chin)
            int neckH = Math.Max(3, A / 6);
            int torsoH = Math.Max(4, A / 3);

            // Hair / crown (rounded dome)
            for (int r = 0; r < headH / 3; r++)
            {
                int inset = headH / 3 - r;
                PrintRow(left + inset, "xxx");
            }

            // Forehead / upper face (slight slope)
            for (int r = 0; r < headH / 3; r++)
            {
                int x = left + r / 2;
                PrintRow(x, "xxxxx");
            }

            // Eye line & cheek; add eye mark
            PrintRow(left + headH / 6, "x   x");
            for (int r = 0; r < headH / 6; r++)
            {
                int x = left + r / 3;
                PrintRow(x, new string('x', 7));
            }

            // Nose (uses feat)
            for (int r = 0; r < feat / 2; r++)
            {
                int x = left + r + faceL / 3;
                PrintRow(x, "x");
            }
            // Nose tip
            PrintRow(left + feat / 2 + faceL / 3 + 1, "xx");

            // Lips (two short protrusions)
            PrintRow(left + feat / 2 + faceL / 3, " xx");
            PrintRow(left + feat / 2 + faceL / 3 - 1, "xxx");

            // Chin (curves back)
            for (int r = 0; r < Math.Max(2, feat / 2); r++)
            {
                int x = left + Math.Max(0, feat / 2 - r - 1);
                PrintRow(x, "xx");
            }

            // Neck
            for (int r = 0; r < neckH; r++)
            {
                PrintRow(left - 2, "xx");
            }

            // Torso / shoulder swoop
            for (int r = 0; r < torsoH; r++)
            {
                int x = left - 4 - r / 2;
                PrintRow(x, new string('x', 6 + r / 3));
            }
        }

        private static void PrintRow(int col, string s)
        {
            if (col < 0) col = 0;
            Console.WriteLine(new string(' ', col) + s.Replace('x', 'X'));
        }

        private static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);

        // --------------- VERDICT -------------------

        private static int Score(int A, int B, int C)
        {
            // Simple proportion checks; higher score => more “UGLY!” spam.
            int score = 0;

            double faceToHead = (double)B / Math.Max(1, A);   // ~1.2–2.2 feels OK
            if (faceToHead < 1.1 || faceToHead > 2.4) score++;

            // C influences nose/lips/chin; 4–12 after scaling felt OK in the renderer
            double feat = C / 3.0;
            if (feat < 4 || feat > 12) score++;

            // Extremely tiny or huge A looks odd in our ASCII
            if (A < MinA - 2 || A > MaxA + 2) score++;

            return score;
        }
    }
}
