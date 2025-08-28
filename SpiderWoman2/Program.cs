using System;

namespace Spiderwoman
{
    class Program
    {
        static void Main()
        {
            Console.Title = "Spiderwoman (Bonus Rule)";
            var rng = new Random();

            int goes = 0;                          // G
            char target = (char)(rng.Next(26) + 'A'); // T$: random A–Z

            Console.Clear();
            Console.WriteLine("SPIDERWOMAN HAS CHOSEN");
            Console.WriteLine("TRY A WORD");

            while (true)
            {
                Console.Write("> ");
                string word = Console.ReadLine()?.Trim().ToUpper() ?? "";
                goes++;

                if (word.Length < 4 || word.Length > 8)
                {
                    Console.WriteLine("WORD TOO SHORT OR LONG, TRY AGAIN");
                    continue;
                }

                bool contains = false;
                foreach (char c in word)
                    if (c == target) { contains = true; break; }

                if (!contains)
                {
                    Console.WriteLine($"'{target}' IS NOT IN THAT WORD");
                }
                else
                {
                    Console.WriteLine("YES – IT'S ONE OF THOSE");
                    Console.Write("DO YOU WANT TO GUESS? (Y/N) ");
                    string yn = (Console.ReadLine() ?? "").Trim().ToUpper();

                    if (yn == "Y")
                    {
                        bool won = false;
                        for (int attempt = 1; attempt <= 2; attempt++)
                        {
                            Console.Write($"WHAT IS YOUR GUESS ({attempt}/2)? ");
                            string guess = Console.ReadLine()?.Trim().ToUpper() ?? "";
                            if (guess == target.ToString())
                            {
                                won = true;
                                break;
                            }
                        }

                        if (won)
                        {
                            Console.WriteLine("OK – YOU CAN GO (THIS TIME)");
                            return;
                        }
                        else
                        {
                            Console.WriteLine("WRONG! YOU FORFEIT FIVE GOES.");
                            goes += 5; // bonus rule penalty
                        }
                    }
                }

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
