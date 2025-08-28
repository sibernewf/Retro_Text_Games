using System;
using System.Threading;

namespace Seance
{
    class Program
    {
        static readonly Random Rng = new Random();

        static void Main()
        {
            Console.Title = "Seance";
            Console.Clear();

            int score = 0;
            Console.WriteLine("The séance begins... Spirits are sending messages.");
            Console.WriteLine("Watch carefully, then type the letters in order!");
            Console.WriteLine("Press ENTER after your attempt.\n");

            while (true)
            {
                // Generate a sequence
                int length = Rng.Next(4, 8); // between 4–7 letters
                string sequence = GenerateSequence(length);

                // Show it letter by letter with spooky delay
                Console.WriteLine("\n--- NEW MESSAGE ---");
                for (int i = 0; i < sequence.Length; i++)
                {
                    Console.Write(sequence[i]);
                    Thread.Sleep(400); // controls speed of letters
                }
                Console.WriteLine();
                Thread.Sleep(500);

                // Player input
                Console.Write("\nType the letters exactly: ");
                string input = (Console.ReadLine() ?? "").Trim().ToUpper();

                if (input == sequence)
                {
                    score++;
                    Console.WriteLine("The spirits are pleased... (Score = " + score + ")");
                }
                else
                {
                    Console.WriteLine("\nThe spirits are ANGRY!");
                    SpiritAnger();
                    Console.WriteLine("FINAL SCORE: " + score);
                    break;
                }
            }
        }

        static string GenerateSequence(int length)
        {
            string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            char[] seq = new char[length];
            for (int i = 0; i < length; i++)
                seq[i] = letters[Rng.Next(letters.Length)];
            return new string(seq);
        }

        static void SpiritAnger()
        {
            int choice = Rng.Next(3);
            Console.WriteLine();
            switch (choice)
            {
                case 0:
                    Console.WriteLine("The table begins to shake violently!");
                    break;
                case 1:
                    Console.WriteLine("The light bulb shatters with a POP!");
                    break;
                case 2:
                    Console.WriteLine("A pair of clammy hands grasps your neck!");
                    break;
            }
        }
    }
}
