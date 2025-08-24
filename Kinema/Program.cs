using System;

class Kinema
{
    static void Main()
    {
        Random rand = new Random();
        const double g = 9.8; // acceleration due to gravity (m/s²)

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine();

            // Random initial velocity (m/s) from 15 to 29
            double v = 15 + 3 * rand.Next(5); 
            Console.WriteLine($"A BALL IS THROWN UPWARDS AT {v} METERS PER SECOND");

            int score = 0;

            // Q1: Max height
            double h = (v * v) / (2 * g);
            Console.Write("\nHOW HIGH WILL IT GO (IN METERS)? ");
            if (CheckAnswer(h)) score++;

            // Q2: Time until it returns
            double tReturn = (2 * v) / g;
            Console.Write("\nHOW LONG UNTIL IT RETURNS (IN SECONDS)? ");
            if (CheckAnswer(tReturn)) score++;

            // Q3: Velocity after t seconds
            double t = Math.Round(rand.NextDouble() * 10, 1); // 0.0–10.0 seconds
            double velocityAfterT = v - g * t;
            Console.Write($"\nWHAT WILL ITS VELOCITY BE AFTER {t} SECONDS? ");
            if (CheckAnswer(velocityAfterT)) score++;

            // Round summary
            Console.WriteLine($"\n{score} RIGHT OUT OF 3. {(score >= 2 ? "NOT BAD." : "")}");

            Console.Write("\nPress ENTER for another problem, or type Q to quit: ");
            string? input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input) && input.Trim().ToUpper() == "Q")
                break;
        }
    }

    static bool CheckAnswer(double correct)
    {
        string? input = Console.ReadLine();
        if (!double.TryParse(input, out double guess))
        {
            Console.WriteLine($"Invalid input. Correct answer is {correct:F6}");
            return false;
        }

        double tolerance = 0.15 * Math.Abs(correct); // ±15%
        if (Math.Abs(guess - correct) <= tolerance)
        {
            Console.WriteLine(guess == correct ? "CORRECT! VERY GOOD!" : "CLOSE ENOUGH.");
            Console.WriteLine($"CORRECT ANSWER IS {correct:F6}");
            return true;
        }
        else
        {
            Console.WriteLine("NOT EVEN CLOSE...");
            Console.WriteLine($"CORRECT ANSWER IS {correct:F6}");
            return false;
        }
    }
}
