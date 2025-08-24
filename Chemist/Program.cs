using System;
using System.Globalization;
using System.IO;

class Program
{
    // Initialize to a no-op writer so nullability is happy; replaced in Main().
    static StreamWriter LogFile = new StreamWriter(Stream.Null);
    static string PlayerName = "PLAYER";

    static void Main()
    {
        Console.Write("Enter your name: ");
        PlayerName = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(PlayerName)) PlayerName = "PLAYER";

        // Practice mode: show ideal answer after each round?
        bool showAnswers = AskYesNo($"{PlayerName}, enable PRACTICE MODE (show ideal answer after each round)? (Y/N): ");

        string fileName = $"CHEMST_SampleRun_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        LogFile = new StreamWriter(fileName);

        try
        {
            // Intro & instructions
            Print("WELCOME TO CHEMST — DILUTE KRYPTOCYANIC ACID SAFELY!");
            Print("----------------------------------------------------");
            Print("You have been given a dangerous chemical, Kryptocyanic Acid.");
            Print("It must be diluted using EXACTLY 7 parts water to 3 parts acid.");
            Print("For example: If you have 30 liters of acid,");
            Print("Water needed = Acid × (7 ÷ 3)  →  30 × (7/3) = 70 liters.");
            Print("Any other ratio will cause an unstable reaction and an explosion!");
            Print("You have 9 lives. Being more than 5% off loses a life.");
            Print($"Practice mode is {(showAnswers ? "ON (ideal answer shown each round)" : "OFF")}.");
            Print($"PLAYER: {PlayerName}");
            Print("");

            var rng = new Random();
            int lives = 9;
            int round = 0;
            int streak = 0;
            int bestStreak = 0;

            while (lives > 0)
            {
                round++;

                // Random acid amount (10–100 L), similar feel to the book examples
                int acidLiters = rng.Next(10, 101);

                string prompt = $"{acidLiters} LITERS OF KRYPTOCYANIC ACID. HOW MUCH WATER? ";
                string raw = PromptAndRead(prompt);               // logs + shows the prompt once
                Print($"{PlayerName.ToUpper()} INPUT: {raw}");    // echo input to both

                if (!TryParseDouble(raw, out double waterGuess))
                {
                    Print("Please enter a number.");
                    Print("");
                    continue;
                }

                double idealWater = acidLiters * (7.0 / 3.0); // 7:3 water:acid
                double tolerance = idealWater * 0.05;         // ±5%
                double error = Math.Abs(waterGuess - idealWater);

                if (error <= tolerance)
                {
                    streak++;
                    if (streak > bestStreak) bestStreak = streak;

                    Print("GOOD JOB! YOU MAY BREATHE NOW, BUT DON'T INHALE THE FUMES!");
                    if (showAnswers)
                        Print($"[Practice] Ideal: {idealWater:F2} L   Your: {waterGuess:F2} L   Error: {error:F2} L");
                    Print($"Streak: {streak}   Best: {bestStreak}   Lives: {lives}");
                    Print("");
                }
                else
                {
                    lives--;
                    streak = 0;

                    Print("SIZZLE! YOU HAVE JUST BEEN DESALINATED INTO A BLOB");
                    Print("OF QUIVERING PROTOPLASM!");
                    if (showAnswers)
                        Print($"[Practice] Ideal: {idealWater:F2} L   Your: {waterGuess:F2} L   Error: {error:F2} L   Tol: ±{tolerance:F2} L");
                    Print($"Streak reset. Best: {bestStreak}   Lives left: {lives}");
                    if (lives > 0) Print("HOWEVER, YOU MAY TRY AGAIN WITH ANOTHER LIFE.");
                    Print("");
                }
            }

            Print("YOUR 9 LIVES ARE USED, BUT YOU WILL BE LONG REMEMBERED FOR");
            Print("YOUR CONTRIBUTIONS TO THE FIELD OF COMIC BOOK CHEMISTRY.");
            Print($"Best Streak: {bestStreak}");
            Print("");
            Print($"Sample run saved to: {Path.GetFullPath(fileName)}");
        }
        finally
        {
            LogFile.Flush();
            LogFile.Dispose();
        }
    }

    // ---------- Output & logging helpers ----------
    // Print = write once to console and to log file (keeps them in sync)
    static void Print(string s)
    {
        Console.WriteLine(s);
        LogFile.WriteLine(s);
        LogFile.Flush();
    }

    // Prompt shown once to console and recorded to log;
    // returns the trimmed console input (not logged here; caller can echo).
    static string PromptAndRead(string prompt)
    {
        Console.Write(prompt);
        LogFile.WriteLine(prompt);
        LogFile.Flush();
        return (Console.ReadLine() ?? "").Trim();
    }

    static bool AskYesNo(string question)
    {
        while (true)
        {
            Console.Write(question);
            string s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            if (s == "Y" || s == "YES") return true;
            if (s == "N" || s == "NO") return false;
            Console.WriteLine("Please answer Y or N.");
        }
    }

    static bool TryParseDouble(string s, out double value)
    {
        // Try current culture (supports , or .), then invariant
        return double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
            || double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}
