using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitsOutguess
{
    internal static class Program
    {
        // We model the BASIC strategy: learn from the player’s stream by tracking
        // how often each 3-digit history (000..222) is followed by 0, 1, or 2.
        // Prediction = argmax over counts for the current history; ties → random.
        private const int Alphabet = 3;        // digits 0,1,2
        private const int HistoryLen = 3;      // length of context used for prediction
        private const int RoundInputs = 30;    // total numbers the player will type
        private const int AskBatch = 10;       // ask in lines of ten, like the BASIC

        private static readonly Random Rng = new Random();

        private static void Main()
        {
            Console.WriteLine("DIGITS — Computer Tries to Outguess Player");
            Console.WriteLine("Type '?' for brief instructions, or press Enter to begin.");
            var top = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(top) && top.Trim() == "?")
                PrintInstructions();

            do
            {
                PlayOneGame();
            } while (AskYesNo("\nDo you want to try again (1 for YES, 0 for NO)? "));
            
            Console.WriteLine("\nThanks for the game.");
        }

        private static void PlayOneGame()
        {
            // frequency[historyIndex, nextDigit] => how often nextDigit followed that history
            var frequency = new int[(int)Math.Pow(Alphabet, HistoryLen), Alphabet];
            var playerStream = new List<int>(RoundInputs);
            int correct = 0;

            Console.WriteLine();
            while (playerStream.Count < RoundInputs)
            {
                var need = Math.Min(AskBatch, RoundInputs - playerStream.Count);
                Console.WriteLine($"TEN NUMBERS PLEASE? (Enter {need} digits of 0, 1, or 2, separated by spaces/commas)");
                var nextChunk = ReadDigits(need);
                Console.WriteLine();
                Console.WriteLine("MY GUESS\tYOUR NO.\tRESULT\tNO. RIGHT");

                // Process one-by-one with “guess first, then reveal your next number”
                foreach (var actual in nextChunk)
                {
                    int guess = PredictNext(playerStream, frequency);
                    bool right = (guess == actual);
                    if (right) correct++;

                    Console.WriteLine($"{guess}\t\t{actual}\t\t{(right ? "RIGHT" : "WRONG")}\t{correct}");

                    // Update learning tables using newest symbol and the latest history
                    UpdateModel(playerStream, actual, frequency);
                    playerStream.Add(actual);
                }

                Console.WriteLine();
            }

            // Win/lose message (computer wins if it guesses ≥ 1/3 of 30 → 10 or more)
            if (correct > RoundInputs / 3)
            {
                Console.WriteLine("I GUESSED MORE THAN 1/3 OF YOUR NUMBERS.");
                Console.WriteLine("I WIN.");
            }
            else if (correct == RoundInputs / 3)
            {
                Console.WriteLine("I GUESSED EXACTLY 1/3 OF YOUR NUMBERS.");
                Console.WriteLine("I WIN.");
            }
            else
            {
                Console.WriteLine("I GUESSED LESS THAN 1/3 OF YOUR NUMBERS.");
                Console.WriteLine("YOU BEAT ME.  CONGRATULATIONS ****");
            }
        }

        private static void PrintInstructions()
        {
            Console.WriteLine();
            Console.WriteLine("This is a game of guessing.");
            Console.WriteLine("Please take a piece of paper and write down the digits 0, 1, or 2");
            Console.WriteLine("thirty times at random. Arrange them in three lines of ten digits.");
            Console.WriteLine("I will always guess first, then you type your next digit.");
            Console.WriteLine("By pure luck I ought to be right 10 times out of 30, but I hope");
            Console.WriteLine("to do better by finding patterns in what you type.");
            Console.WriteLine();
        }

        private static List<int> ReadDigits(int expectedCount)
        {
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine() ?? "";
                var tokens = line
                    .Replace(",", " ")
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length != expectedCount)
                {
                    Console.WriteLine($"Please enter exactly {expectedCount} digits (0, 1, or 2). Try again.");
                    continue;
                }
                var ok = new List<int>(expectedCount);
                bool bad = false;
                foreach (var t in tokens)
                {
                    if (t.Length != 1 || t[0] < '0' || t[0] > '2') { bad = true; break; }
                    ok.Add(t[0] - '0');
                }
                if (bad)
                {
                    Console.WriteLine("Use only digits 0, 1, or 2. Let's try again.");
                    continue;
                }
                return ok;
            }
        }

        private static int PredictNext(List<int> stream, int[,] freq)
        {
            // Not enough history? Guess random.
            if (stream.Count < HistoryLen)
                return Rng.Next(Alphabet);

            // Build history index from the last HistoryLen symbols (base-3 number)
            int idx = 0;
            for (int i = stream.Count - HistoryLen; i < stream.Count; i++)
                idx = idx * Alphabet + stream[i];

            // Choose the digit with the highest count; break ties randomly among maxima
            int max = freq[idx, 0];
            var best = new List<int> { 0 };
            for (int d = 1; d < Alphabet; d++)
            {
                if (freq[idx, d] > max)
                {
                    max = freq[idx, d];
                    best.Clear();
                    best.Add(d);
                }
                else if (freq[idx, d] == max)
                {
                    best.Add(d);
                }
            }
            return best[Rng.Next(best.Count)];
        }

        private static void UpdateModel(List<int> stream, int nextDigit, int[,] freq)
        {
            if (stream.Count < HistoryLen) return;

            int idx = 0;
            for (int i = stream.Count - HistoryLen; i < stream.Count; i++)
                idx = idx * Alphabet + stream[i];

            freq[idx, nextDigit]++;
        }

        private static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine()?.Trim() ?? "";
                if (s == "1") return true;
                if (s == "0") return false;
                Console.WriteLine("Please type 1 for YES or 0 for NO.");
            }
        }
    }
}
