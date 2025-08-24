using System;
using System.Collections.Generic;
using System.Linq;

namespace Bulcow
{
    class Program
    {
        static readonly Random Rng = new();

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("BULCOW — Bulls & Cows (5-digit, all digits unique)");
            Console.WriteLine("Type Q anytime to quit.\n");

            // Player secret (hidden scoring)
            string playerSecret = PromptSecret("Pick YOUR secret 5-digit number (no repeats, first digit 1-9): ");
            // Computer secret
            string compSecret = RandomSecret();

            // Candidate set for computer’s solver
            var pool = GenerateAllCandidates();

            int turn = 1;
            string? lastCompGuess = null;

            while (true)
            {
                Console.WriteLine($"\n— TURN {turn} —");

                // ===== Player guesses first =====
                string guess = PromptGuess("Your guess: ");
                var (pb, pc) = Score(compSecret, guess);
                Console.WriteLine($"You score: {pb} BULL{Plural(pb)} and {pc} COW{Plural(pc)}.");
                if (pb == 5)
                {
                    Console.WriteLine("\nYou cracked the computer’s number — YOU WIN!");
                    Console.WriteLine($"Computer's secret was: {compSecret}");
                    break;
                }

                // ===== Computer guesses =====
                string compGuess = NextGuess(pool, lastCompGuess);
                lastCompGuess = compGuess;

                var (cb, cc) = Score(playerSecret, compGuess);
                Console.WriteLine($"\nComputer guesses: {compGuess}");
                Console.WriteLine($"Computer scores: {cb} BULL{Plural(cb)} and {cc} COW{Plural(cc)}.");
                if (cb == 5)
                {
                    Console.WriteLine("\nThe computer found your number — COMPUTER WINS.");
                    Console.WriteLine($"Your secret was: {playerSecret}");
                    break;
                }

                // Narrow the pool to all candidates consistent with (cb,cc) vs compGuess
                pool = pool.Where(s =>
                {
                    var (b, c) = Score(s, compGuess);
                    return b == cb && c == cc;
                }).ToList();

                Console.WriteLine($"Candidates remaining for the computer: {pool.Count}");
                turn++;
            }

            Console.WriteLine("\nThanks for playing!");
        }

        // ===== Secrets & guesses =====

        static string PromptSecret(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var raw = (Console.ReadLine() ?? "").Trim();
                if (raw.Equals("Q", StringComparison.OrdinalIgnoreCase)) Environment.Exit(0);
                if (IsValidSecret(raw))
                {
                    return raw;
                }
                Console.WriteLine("Invalid: must be 5 digits, all unique, first digit 1–9.");
            }
        }

        static string PromptGuess(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var raw = (Console.ReadLine() ?? "").Trim();
                if (raw.Equals("Q", StringComparison.OrdinalIgnoreCase)) Environment.Exit(0);
                if (IsValidSecret(raw))
                    return raw;

                Console.WriteLine("Guess must be 5 digits, all unique, first digit 1–9.");
            }
        }

        static bool IsValidSecret(string s)
        {
            if (s.Length != 5 || !s.All(char.IsDigit)) return false;
            if (s[0] == '0') return false;
            return s.Distinct().Count() == 5;
        }

        static string RandomSecret()
        {
            var digits = Enumerable.Range(0, 10).Select(i => (char)('0' + i)).ToList();
            // first digit 1–9
            digits.Remove('0');
            char first = digits[Rng.Next(digits.Count)];
            var restPool = Enumerable.Range(0, 10).Select(i => (char)('0' + i)).ToList();
            restPool.Remove(first);
            // remove one more so we still can include 0 possibly
            var remaining = new List<char>();
            while (remaining.Count < 4)
            {
                var pickIndex = Rng.Next(restPool.Count);
                remaining.Add(restPool[pickIndex]);
                restPool.RemoveAt(pickIndex);
            }
            return first + string.Concat(remaining);
        }

        // ===== Scoring =====

        static (int bulls, int cows) Score(string secret, string guess)
        {
            int bulls = 0, cows = 0;
            for (int i = 0; i < 5; i++)
                if (guess[i] == secret[i]) bulls++;

            // cows = shared digits minus bulls
            int shared = guess.Count(ch => secret.Contains(ch));
            cows = shared - bulls;
            return (bulls, cows);
        }

        // ===== Computer solver =====

        static List<string> GenerateAllCandidates()
        {
            var list = new List<string>(9 * 9 * 8 * 7 * 6);
            for (char a = '1'; a <= '9'; a++)
            {
                for (char b = '0'; b <= '9'; b++)
                {
                    if (b == a) continue;
                    for (char c = '0'; c <= '9'; c++)
                    {
                        if (c == a || c == b) continue;
                        for (char d = '0'; d <= '9'; d++)
                        {
                            if (d == a || d == b || d == c) continue;
                            for (char e = '0'; e <= '9'; e++)
                            {
                                if (e == a || e == b || e == c || e == d) continue;
                                list.Add(new string(new[] { a, b, c, d, e }));
                            }
                        }
                    }
                }
            }
            return list;
        }

        static string NextGuess(List<string> pool, string? last)
        {
            // Simple strategy: take the first remaining candidate,
            // but shuffle a bit so it doesn’t look mechanical.
            if (pool.Count == 0) return "12345"; // should never happen with correct scoring
            // Pick a middle element to avoid repeating the same path every game.
            int index = pool.Count > 7 ? pool.Count / 2 : 0;
            return pool[index];
        }

        static string Plural(int n) => n == 1 ? "" : "S";
    }
}
