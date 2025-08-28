using System;

namespace Spiderwoman
{
    class Program
    {
        static void Main()
        {
            Console.Title = "Spiderwoman";
            var rng = new Random();

            int goes = 0;  // G in BASIC
            // Pick a letter T$ (A–Z range as per INT(RND*26)+65)
            char target = (char)(rng.Next(0, 26) + 'A');

            Console.Clear();
            Console.WriteLine("SPIDERWOMAN HAS CHOSEN");
            Console.WriteLine("TRY A WORD");

            while (true)
            {
                Console.Write("> ");
                string word = Console.ReadLine()?.Trim().ToUpper() ?? "";

                goes++;

                // must be between 4 and 8 chars
                if (word.Length < 4 || word.Length > 8)
                {
                    Console.WriteLine("WORD TOO SHORT OR LONG, TRY AGAIN");
                    continue;
                }

                // check if target letter is in the word
                bool found = false;
                foreach (char c in word)
                {
                    if (c == target) { found = true; break; }
                }

                if (!found)
                {
                    Console.WriteLine($"'{target}' IS NOT IN THAT WORD");
                }
                else
                {
                    Console.WriteLine("YES - IT'S ONE OF THOSE");
                    Console.Write("DO YOU WANT TO GUESS? (Y/N) ");
                    string ans = Console.ReadLine()?.Trim().ToUpper() ?? "";
                    if (ans == "Y")
                    {
                        Console.Write("WHAT IS YOUR GUESS THEN? ");
                        string guess = Console.ReadLine()?.Trim().ToUpper() ?? "";
                        if (guess == target.ToString())
                        {
                            Console.WriteLine("OK - YOU CAN GO (THIS TIME)");
                            return;
                        }
                        else
                        {
                            Console.WriteLine("YOU ARE TOO LATE");
                            Console.WriteLine("YOU ARE NOW A FLY");
                            return;
                        }
                    }
                }

                // after 15 goes you lose automatically
                if (goes > 15)
                {
                    Console.WriteLine("YOU ARE TOO LATE");
                    Console.WriteLine("YOU ARE NOW A FLY");
                    return;
                }
            }
        }
    }
}
