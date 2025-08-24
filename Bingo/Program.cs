using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BingoModern
{
    class BingoCard
    {
        public int?[,] Numbers = new int?[5, 5]; // null = FREE center
        public bool[,] Marked = new bool[5, 5];

        public static BingoCard Create(Random rng)
        {
            var card = new BingoCard();

            // Columns B,I,N,G,O → ranges: 1–15, 16–30, 31–45, 46–60, 61–75
            for (int col = 0; col < 5; col++)
            {
                int start = col * 15 + 1;
                var pool = Enumerable.Range(start, 15).OrderBy(_ => rng.Next()).Take(5).ToArray();
                for (int row = 0; row < 5; row++)
                    card.Numbers[row, col] = pool[row];
            }

            // FREE center
            card.Numbers[2, 2] = null;
            card.Marked[2, 2] = true;

            return card;
        }

        public bool Mark(int n)
        {
            for (int r = 0; r < 5; r++)
                for (int c = 0; c < 5; c++)
                    if (Numbers[r, c].HasValue && Numbers[r, c]!.Value == n)
                    {
                        Marked[r, c] = true;
                        return true;
                    }
            return false;
        }

        public bool HasBingo()
        {
            // rows / cols
            for (int r = 0; r < 5; r++)
                if (Enumerable.Range(0, 5).All(c => Marked[r, c])) return true;

            for (int c = 0; c < 5; c++)
                if (Enumerable.Range(0, 5).All(r => Marked[r, c])) return true;

            // diagonals
            if (Enumerable.Range(0, 5).All(i => Marked[i, i])) return true;
            if (Enumerable.Range(0, 5).All(i => Marked[i, 4 - i])) return true;

            return false;
        }

        public string Render()
        {
            var sw = new System.Text.StringBuilder();
            void Line() => sw.AppendLine("+----+----+----+----+----+");

            Line();
            sw.AppendLine("| B  | I  | N  | G  | O  |");
            Line();
            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    string cell = Numbers[r, c]?.ToString() ?? "FREE";
                    if (cell.Length > 4) cell = cell[..4];
                    sw.Append($"|{cell,4}");
                }
                sw.AppendLine("|");

                for (int c = 0; c < 5; c++)
                {
                    string mark = Marked[r, c] ? "xxxx" : "    ";
                    sw.Append($"|{mark}");
                }
                sw.AppendLine("|");
                Line();
            }
            return sw.ToString();
        }

        public void Print() => Console.Write(Render());
    }

    class BingoGame
    {
        readonly Random rng = new Random();
        readonly List<string> log = new List<string>();

        public void Run()
        {
            Console.WriteLine("=== BINGO ===  (Enter=draw, N=new card, P=print card to file, Q=quit)");
            while (true)
            {
                var card = BingoCard.Create(rng);
                log.Add("NEW CARD");
                Console.WriteLine("\nYour card:");
                card.Print();
                SaveCardToFile(card, announce:true);   // auto-save new card

                var deck = Enumerable.Range(1, 75).OrderBy(_ => rng.Next()).ToList();

                while (deck.Count > 0)
                {
                    Console.Write("Press ENTER to draw (N new / P print card / Q quit): ");
                    var input = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                    if (input == "Q") { SaveLog(); return; }
                    if (input == "N") break; // start new card
                    if (input == "P") { SaveCardToFile(card, announce:true); continue; }

                    int n = deck[0]; deck.RemoveAt(0);

                    Console.WriteLine($"\nCALL: {n}");
                    log.Add($"CALL {n}");

                    bool onCard = card.Mark(n);
                    Console.WriteLine(onCard ? "Marked on your card!" : "Not on your card.");
                    Console.WriteLine("\nCard:");
                    card.Print();

                    if (card.HasBingo())
                    {
                        Console.WriteLine("***** B I N G O ! *****");
                        log.Add("BINGO");
                        // Save the winning card snapshot too
                        SaveCardToFile(card, fileName:"bingo_card_winner.txt", announce:true);
                        break;
                    }
                }

                Console.Write("\nPlay again with a new card? (Y/N): ");
                var again = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (!again.StartsWith("Y")) { SaveLog(); return; }
            }
        }

        void SaveCardToFile(BingoCard card, bool announce = false, string fileName = "bingo_card.txt")
        {
            File.WriteAllText(fileName, card.Render());
            if (announce)
                Console.WriteLine($"Card saved to: {Path.GetFullPath(fileName)}");
        }

        void SaveLog()
        {
            var path = "bingo_log.txt";
            File.WriteAllLines(path, log);
            Console.WriteLine($"\nSession log saved to: {Path.GetFullPath(path)}");
        }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("Q at any prompt to quit.\n");
            var g = new BingoGame();
            g.Run();
        }
    }
}
