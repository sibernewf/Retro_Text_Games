using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BuzzwordGenerator
{
    class Program
    {
        static readonly string[] First =
        {
            "TOTAL",
            "SYSTEMATIZED",
            "PARALLEL",
            "FUNCTIONAL",
            "RESPONSIVE",
            "OPTIMAL",
            "SYNCHRONIZED",
            "COMPATIBLE",
            "BALANCED",
            "INTEGRATED" // BASIC prints INTEGRATED later; fits naturally as a 10th here
        };

        static readonly string[] Second =
        {
            "MANAGEMENT",
            "ORGANIZATIONAL",
            "MONITORED",
            "RECIPROCAL",
            "DIGITAL",
            "LOGISTICAL",
            "TRANSITIONAL",
            "INCREMENTAL",
            "THIRD-GENERATION",
            "POLICY"
        };

        static readonly string[] Third =
        {
            "OPTIONS",
            "FLEXIBILITY",
            "CAPABILITY",
            "MOBILITY",
            "PROGRAMMING",
            "CONCEPT",
            "TIME-PHASE",
            "PROJECTION",
            "HARDWARE",
            "CONTINGENCY"
        };

        static readonly Random Rng = new();

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("BUZZWD — Buzzword Generator");
            Console.WriteLine("This invaluable program prepares speeches and briefings about computers and high technology.");
            Console.WriteLine("Give ANY THREE NUMBERS between 0 and 9 (separated by commas or spaces).");
            Console.WriteLine("Press ENTER for a random set; type Q to quit.");
            Console.WriteLine("Entering a number outside 0–9 ends the program.\n");

            var logPath = Path.GetFullPath("buzzwords.txt");
            using var log = new StreamWriter(new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8);

            while (true)
            {
                Console.Write("WHAT ARE YOUR THREE NUMBERS? ");
                var raw = Console.ReadLine();
                if (raw is null) break; // EOF
                raw = raw.Trim();

                if (raw.Equals("Q", StringComparison.OrdinalIgnoreCase))
                    break;

                int[] digits;

                if (string.IsNullOrEmpty(raw))
                {
                    // random phrase
                    digits = new[] { Rng.Next(0, 10), Rng.Next(0, 10), Rng.Next(0, 10) };
                }
                else
                {
                    var parts = raw.Split(new[] { ',', ' ', '\t', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 3 || !TryParse3Digits(parts, out digits))
                    {
                        Console.WriteLine("Please enter exactly three numbers (0–9), separated by commas or spaces.");
                        continue;
                    }
                }

                // If any is outside 0–9, we stop (matching the original’s “stop on invalid” behavior)
                if (digits.Any(d => d < 0 || d > 9))
                {
                    Console.WriteLine("GOODBYE FOR NOW!");
                    break;
                }

                string phrase = $"{First[digits[0]]} {Second[digits[1]]} {Third[digits[2]]}";
                Console.WriteLine(phrase);

                log.WriteLine(phrase);
                log.Flush();

                Console.WriteLine("\nTHREE MORE NUMBERS?");
            }

            Console.WriteLine($"\nSaved phrases to: {logPath}");
        }

        static bool TryParse3Digits(string[] parts, out int[] digits)
        {
            digits = new int[3];
            for (int i = 0; i < 3; i++)
            {
                if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                    return false;
                digits[i] = v;
            }
            return true;
        }
    }
}
