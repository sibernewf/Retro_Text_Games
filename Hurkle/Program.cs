using System;
using System.Globalization;

namespace HurkleGame
{
    internal static class Program
    {
        private static readonly Random Rng = new Random();

        private const int GridSize = 10;      // 0..9 on each axis
        private const int MaxGuesses = 5;

        private static void Main()
        {
            Console.Title = "HURKLE — Find the Hurkle in Hiding";

            PrintIntro();

            do
            {
                PlayRound();
            } while (AskYesNo("\nLET'S PLAY AGAIN. HURKLE IS HIDING.  Play? (Y/N) "));
        }

        private static void PrintIntro()
        {
            Console.WriteLine("A HURKLE IS HIDING ON A 10 BY 10 GRID. HOMEBASE");
            Console.WriteLine("ON THE GRID IS POINT 0, 0 AND ANY GRIDPOINT IS A");
            Console.WriteLine("PAIR OF WHOLE NUMBERS SEPARATED BY A COMMA. TRY TO");
            Console.WriteLine("GUESS THE HURKLE'S GRIDPOINT. YOU GET 5 TRIES.");
            Console.WriteLine("AFTER EACH TRY, I WILL TELL YOU THE APPROXIMATE");
            Console.WriteLine("DIRECTION TO GO LOOK FOR THE HURKLE.");
            Console.WriteLine();
        }

        private static void PlayRound()
        {
            // Hurkle’s secret position
            int hurkleX = Rng.Next(GridSize); // 0..9
            int hurkleY = Rng.Next(GridSize);

            for (int guessNum = 1; guessNum <= MaxGuesses; )
            {
                var (ok, x, y) = ReadGuess(guessNum);
                if (!ok)
                {
                    // invalid input doesn't consume a guess
                    continue;
                }

                if (x == hurkleX && y == hurkleY)
                {
                    Console.WriteLine();
                    Console.WriteLine($"YOU FOUND HIM IN {guessNum} GUESSES!");
                    return;
                }

                Console.WriteLine(DirectionHint(x, y, hurkleX, hurkleY));
                guessNum++;
            }

            Console.WriteLine();
            Console.WriteLine("SORRY, THAT'S 5 GUESSES.");
            Console.WriteLine($"THE HURKLE IS AT {hurkleX}, {hurkleY}.");
        }

        private static (bool ok, int x, int y) ReadGuess(int guessNum)
        {
            Console.Write($"GUESS #{guessNum} ? ");
            string? line = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
            {
                Console.WriteLine("Please enter two numbers between 0 and 9, separated by a comma (e.g., 7,3).");
                return (false, 0, 0);
            }

            // Accept “x,y” or “x y” and ignore extra spaces
            line = line.Trim();
            line = line.Replace(" ", "");

            string[] parts = line.Split(',');
            if (parts.Length != 2)
            {
                Console.WriteLine("Format is x,y (example: 7,3). Try again.");
                return (false, 0, 0);
            }

            if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int x) ||
                !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int y))
            {
                Console.WriteLine("Both x and y must be whole numbers. Try again.");
                return (false, 0, 0);
            }

            if (x < 0 || x >= GridSize || y < 0 || y >= GridSize)
            {
                Console.WriteLine("Values must be from 0 to 9 inclusive. Try again.");
                return (false, 0, 0);
            }

            return (true, x, y);
        }

        private static string DirectionHint(int gx, int gy, int tx, int ty)
        {
            // gx/gy = guess; tx/ty = target
            string vertical = gy < ty ? "NORTH" : gy > ty ? "SOUTH" : string.Empty;
            string horizontal = gx < tx ? "EAST" : gx > tx ? "WEST" : string.Empty;

            string dir = (vertical + horizontal).Trim();
            if (vertical.Length > 0 && horizontal.Length > 0)
                dir = vertical + horizontal; // e.g., "NORTHEAST"

            if (dir.Length == 0) dir = "NOWHERE"; // shouldn't happen unless already correct

            return $"GO {dir}!";
        }

        private static bool AskYesNo(string prompt)
        {
            Console.Write(prompt);
            while (true)
            {
                string? s = Console.ReadLine();
                if (s == null) return false;
                s = s.Trim().ToUpperInvariant();
                if (s == "Y" || s == "YES") return true;
                if (s == "N" || s == "NO") return false;
                Console.Write("Please answer Y or N: ");
            }
        }
    }
}
