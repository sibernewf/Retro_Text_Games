using System;
using System.Collections.Generic;

namespace WarGame
{
    internal static class Program
    {
        private static readonly Random Rng = new Random();

        private static readonly string[] Suits = { "S", "H", "D", "C" }; // Spades, Hearts, Diamonds, Clubs
        private static readonly string[] Ranks =
        {
            "2","3","4","5","6","7","8","9","10","J","Q","K","A"
        };

        private static void Main()
        {
            Console.WriteLine("THIS IS THE CARD GAME OF WAR.");
            Console.WriteLine("Each card is given by SUIT-NUMBER (e.g., S-7 for 7 of Spades).");
            Console.WriteLine("The computer deals one to you and one to itself.");
            Console.WriteLine("The higher card (numerically) wins. Tie = no score.");
            Console.WriteLine("Game ends when deck is finished or you choose not to continue.\n");

            var deck = BuildDeck();
            Shuffle(deck);

            int youScore = 0;
            int compScore = 0;
            int round = 0;

            while (deck.Count >= 2)
            {
                round++;
                var yourCard = deck[0]; deck.RemoveAt(0);
                var compCard = deck[0]; deck.RemoveAt(0);

                Console.WriteLine($"YOU: {yourCard}   COMPUTER: {compCard}");

                int result = CompareCards(yourCard, compCard);
                if (result > 0)
                {
                    youScore++;
                    Console.WriteLine($"YOU WIN.  YOU HAVE {youScore} ; COMPUTER HAS {compScore}");
                }
                else if (result < 0)
                {
                    compScore++;
                    Console.WriteLine($"COMPUTER WINS.  YOU HAVE {youScore} ; COMPUTER HAS {compScore}");
                }
                else
                {
                    Console.WriteLine("TIE.  NO SCORE CHANGE.");
                }

                if (deck.Count < 2)
                {
                    Console.WriteLine("\nYOU HAVE RUN OUT OF CARDS.");
                    break;
                }

                if (!AskYesNo("DO YOU WANT TO CONTINUE ? (YES=1, NO=0) "))
                    break;

                Console.WriteLine();
            }

            Console.WriteLine($"\nFINAL SCORE: YOU = {youScore}, COMPUTER = {compScore}");
            Console.WriteLine("THANKS FOR PLAYING. IT WAS FUN.");
        }

        private static List<string> BuildDeck()
        {
            var deck = new List<string>();
            foreach (var suit in Suits)
            {
                foreach (var rank in Ranks)
                {
                    deck.Add($"{suit}-{rank}");
                }
            }
            return deck;
        }

        private static void Shuffle(List<string> deck)
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = Rng.Next(i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
        }

        private static int RankValue(string card)
        {
            string rank = card.Split('-')[1];
            for (int i = 0; i < Ranks.Length; i++)
                if (Ranks[i] == rank) return i;
            return -1;
        }

        private static int CompareCards(string a, string b)
        {
            int va = RankValue(a);
            int vb = RankValue(b);
            return va.CompareTo(vb);
        }

        private static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine()?.Trim().ToUpper();
                if (s == "1" || s == "YES" || s == "Y") return true;
                if (s == "0" || s == "NO" || s == "N") return false;
            }
        }
    }
}
