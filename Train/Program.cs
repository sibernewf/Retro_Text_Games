using System;

namespace TrainQuiz
{
    internal static class Program
    {
        private static readonly Random Rnd = new Random();

        static void Main()
        {
            Console.WriteLine("TIME - SPEED - DISTANCE EXERCISE\n");

            bool another;
            do
            {
                RunProblem();
                another = AskYesNo("\nANOTHER PROBLEM (YES OR NO)? ");
            }
            while (another);
        }

        private static void RunProblem()
        {
            // Randomize values similar to BASIC
            int carSpeed = Rnd.Next(25) + 40;   // 40–64 mph
            int diffHours = Rnd.Next(15) + 5;   // 5–19 hours less
            int trainSpeed = Rnd.Next(19) + 20; // 20–38 mph

            Console.WriteLine($"A CAR TRAVELING {carSpeed} MPH CAN MAKE A CERTAIN TRIP IN");
            Console.WriteLine($"{diffHours} HOURS LESS THAN A TRAIN TRAVELING AT {trainSpeed} MPH.");
            Console.Write("HOW LONG DOES THE TRIP TAKE BY CAR? ");

            // correct answer:
            // Let H = time car takes. Distance = carSpeed*H.
            // Train takes H + diffHours at trainSpeed.
            // So carSpeed*H = trainSpeed*(H + diffHours).
            // Solve for H.
            double H = (double)(trainSpeed * diffHours) / (carSpeed - trainSpeed);

            string? input = Console.ReadLine();
            if (!double.TryParse(input, out double guess))
            {
                Console.WriteLine("INVALID ENTRY.");
                return;
            }

            // percent error
            double percentError = Math.Abs((guess - H) / H * 100.0);
            if (percentError <= 5.0) // within 5% is "good"
            {
                Console.WriteLine($"GOOD! ANSWER WITHIN {percentError:F1} PERCENT.");
            }
            else
            {
                Console.WriteLine($"SORRY. YOU WERE OFF BY {percentError:F1} PERCENT.");
            }

            Console.WriteLine($"CORRECT ANSWER IS {H:F1} HOURS.");
        }

        private static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();
                if (string.IsNullOrEmpty(s)) continue;
                s = s.Trim().ToUpperInvariant();
                if (s == "YES" || s == "Y" || s == "1") return true;
                if (s == "NO" || s == "N" || s == "0") return false;
            }
        }
    }
}
