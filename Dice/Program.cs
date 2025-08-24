using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DiceSim
{
    internal static class Program
    {
        private static readonly Random Rng = new();

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Multi-Sided Dice Simulator";

            PrintHeader();

            while (true)
            {
                int sides = AskSides();
                int rolls = AskRolls();
                if (rolls == 0) break;

                var combos = GetTheoreticalCombos(sides);
                var counts = Simulate(rolls, sides);
                var report = BuildReport(rolls, sides, counts, combos);

                Console.WriteLine(report);

                var file = $"dice_report_{sides}sides_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                File.WriteAllText(file, report);
                Console.WriteLine($"Saved report to {file}\n");

                if (!AskYesNo("Run another simulation? (y/n): ")) break;
                Console.WriteLine();
            }

            Console.WriteLine("READY");
        }

        static void PrintHeader()
        {
            Console.WriteLine("Multi-Sided Dice Simulator");
            Console.WriteLine("Rolls a pair of dice (any number of sides) N times and prints the frequency distribution.");
            Console.WriteLine("Shows observed %, theoretical %, difference, and chi-square.\n");
        }

        static int AskSides()
        {
            while (true)
            {
                Console.Write("Number of sides on each die (minimum 2): ");
                var s = Console.ReadLine();
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) && n >= 2)
                    return n;

                Console.WriteLine("Please enter a whole number of at least 2.");
            }
        }

        static int AskRolls()
        {
            while (true)
            {
                Console.Write("How many rolls? (0 to quit): ");
                var s = Console.ReadLine();
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) && n >= 0)
                    return n;

                Console.WriteLine("Please enter a non-negative whole number.");
            }
        }

        static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
                if (s is "y" or "yes") return true;
                if (s is "n" or "no") return false;
            }
        }

        static int[] GetTheoreticalCombos(int sides)
        {
            // Calculate number of ways to roll each sum
            int minSum = 2;
            int maxSum = sides * 2;
            var combos = new int[maxSum - minSum + 1];

            for (int d1 = 1; d1 <= sides; d1++)
            {
                for (int d2 = 1; d2 <= sides; d2++)
                {
                    int sum = d1 + d2;
                    combos[sum - minSum]++;
                }
            }

            return combos;
        }

        static int[] Simulate(int rolls, int sides)
        {
            int minSum = 2;
            int maxSum = sides * 2;
            var counts = new int[maxSum - minSum + 1];

            for (int i = 0; i < rolls; i++)
            {
                int d1 = 1 + Rng.Next(sides);
                int d2 = 1 + Rng.Next(sides);
                int sum = d1 + d2;
                counts[sum - minSum]++;
            }

            return counts;
        }

        static string BuildReport(int rolls, int sides, int[] counts, int[] combos)
        {
            int totalCombos = sides * sides;
            var sb = new StringBuilder();
            sb.AppendLine($"ROLLS: {rolls:N0} | SIDES: {sides}");
            sb.AppendLine();
            sb.AppendLine(" SUM   COUNT    OBSERVED%   THEOR.%   DIFF% ");
            sb.AppendLine("----  -------   ---------   -------   ------");

            double chi2 = 0.0;

            for (int i = 0; i < counts.Length; i++)
            {
                int sum = i + 2;
                int count = counts[i];

                double obsPct = rolls == 0 ? 0 : 100.0 * count / rolls;
                double expPct = 100.0 * combos[i] / totalCombos;
                double diff = obsPct - expPct;

                double expected = (double)rolls * combos[i] / totalCombos;
                if (expected > 0)
                {
                    double delta = count - expected;
                    chi2 += (delta * delta) / expected;
                }

                sb.AppendLine($"{sum,3} {count,9:N0} {obsPct,11:0.000}% {expPct,9:0.000}% {diff,8:+0.000;-0.000;0.000}%");
            }

            sb.AppendLine("----  -------   ---------   -------   ------");
            sb.AppendLine($"Chi-square (df={counts.Length - 1}): {chi2:0.###}");
            sb.AppendLine();

            sb.AppendLine("Histogram (each ▇ ≈ 0.5%):");
            for (int i = 0; i < counts.Length; i++)
            {
                int sum = i + 2;
                double pct = rolls == 0 ? 0 : 100.0 * counts[i] / rolls;
                int blocks = (int)Math.Round(pct / 0.5);
                sb.AppendLine($"{sum,3} | {new string('▇', Math.Min(blocks, 80))} {pct:0.00}%");
            }

            return sb.ToString();
        }
    }
}
