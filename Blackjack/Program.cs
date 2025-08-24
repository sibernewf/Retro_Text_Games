using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModernBlackjack
{
    enum Action { Hit, Stand, Double }
    enum RoundResult { PlayerWin, DealerWin, Push, Blackjack, DealerBlackjack }

    class Card
    {
        public string Rank; // "A", "2".. "10", "J","Q","K"
        public string Suit; // "♠","♥","♦","♣"
        public int Value => Rank switch
        {
            "A" => 11, "K" => 10, "Q" => 10, "J" => 10, _ => int.Parse(Rank)
        };
        public Card(string r, string s) { Rank = r; Suit = s; }
        public override string ToString() => $"{Rank} of {Suit}";
    }

    class Shoe
    {
        readonly Random rng = new Random();
        readonly List<Card> cards = new();
        readonly int numDecks;

        static readonly string[] Ranks = { "A", "2","3","4","5","6","7","8","9","10","J","Q","K" };
        static readonly string[] Suits = { "Spades", "Hearts", "Diamonds", "Clubs" };

        public Shoe(int decks = 4)
        {
            numDecks = Math.Max(1, decks);
            Refill();
        }

        void Refill()
        {
            cards.Clear();
            for (int d = 0; d < numDecks; d++)
                foreach (var s in Suits)
                    foreach (var r in Ranks)
                        cards.Add(new Card(r, s));
            Shuffle();
        }

        void Shuffle()
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
        }

        public Card Deal()
        {
            if (cards.Count < 15) Refill(); // auto-reshuffle when thin
            var c = cards[^1];
            cards.RemoveAt(cards.Count - 1);
            return c;
        }
    }

    class Hand
    {
        public List<Card> Cards = new();
        public int Total
        {
            get
            {
                int sum = Cards.Sum(c => c.Value);
                int aces = Cards.Count(c => c.Rank == "A");
                while (sum > 21 && aces > 0) { sum -= 10; aces--; }
                return sum;
            }
        }
        public bool IsSoft => Cards.Any(c => c.Rank == "A") && Total <= 21 && Total - 10 >= 1;
        public bool IsBlackjack => Cards.Count == 2 && Total == 21;
        public bool IsBusted => Total > 21;
        public void Add(Card c) => Cards.Add(c);
        public override string ToString() => string.Join(", ", Cards.Select(c => c.ToString()));
    }

    class Game
    {
        Shoe shoe = new Shoe(4);
        readonly List<string> log = new();
        decimal bankroll;
        const decimal MinBet = 10m;

        public void Run()
        {
            Intro();

            bankroll = AskMoney("HOW MANY DOLLARS ARE YOU STARTING WITH? ", min: 100m);

            while (true)
            {
                Console.Write($"\nWHAT IS YOUR WAGER THIS TIME? (min ${MinBet}, Q quits) ");
                var betOpt = ReadDecimalOrQuit();
                if (betOpt.quit) break;
                decimal bet = betOpt.value;

                if (bet < MinBet) { Console.WriteLine($"Minimum wager is ${MinBet}."); continue; }
                if (bet > bankroll) { Console.WriteLine("Your bet exceeds your remaining dollars."); continue; }

                var (result, delta) = PlayOneHand(bet);
                bankroll += delta;

                // Result banner
                switch (result)
                {
                    case RoundResult.Blackjack:
                        Console.WriteLine($"BLACKJACK! You win ${delta:+#;-#;0}. Bankroll: ${bankroll}");
                        break;
                    case RoundResult.PlayerWin:
                        Console.WriteLine($"You win ${delta:+#;-#;0}. Bankroll: ${bankroll}");
                        break;
                    case RoundResult.Push:
                        Console.WriteLine($"Push. Bankroll: ${bankroll}");
                        break;
                    case RoundResult.DealerBlackjack:
                    case RoundResult.DealerWin:
                        Console.WriteLine($"You lose ${delta:+#;-#;0}. Bankroll: ${bankroll}");
                        break;
                }

                if (bankroll < MinBet)
                {
                    Console.WriteLine("\nToo bad! You don't have the minimum bet left. Session over.");
                    break;
                }

                if (!AskYesNo("DO YOU WISH TO PLAY AGAIN? (Y/N) ")) break;
            }

            var path = "blackjack_log.txt";
            File.WriteAllLines(path, log);
            Console.WriteLine($"\nLog saved to: {Path.GetFullPath(path)}");
            Console.WriteLine("Hope you enjoyed yourself. Thanks for playing.");
        }

        (RoundResult result, decimal delta) PlayOneHand(decimal bet)
        {
            var player = new Hand();
            var dealer = new Hand();

            // initial deal
            player.Add(shoe.Deal());
            dealer.Add(shoe.Deal()); // dealer upcard
            player.Add(shoe.Deal());
            dealer.Add(shoe.Deal()); // dealer hole

            Console.WriteLine($"\nYOUR FIRST CARD IS {player.Cards[0]}");
            Console.WriteLine($"YOUR SECOND CARD IS {player.Cards[1]}");
            Console.WriteLine($"DEALER SHOWS {dealer.Cards[0].Rank} of {dealer.Cards[0].Suit}");
            Console.WriteLine($"YOU HAVE {player.Total} SHOWING.");

            log.Add($"Hand start: Bet {bet}, Player: {player}, Dealer up {dealer.Cards[0]}");

            // Insurance when dealer shows Ace
            decimal insuranceBet = 0m;
            if (dealer.Cards[0].Rank == "A")
            {
                if (AskYesNo("INSURANCE ANYONE? (Y/N) "))
                {
                    insuranceBet = Math.Min(bet / 2m, bankroll - bet >= 0 ? bet / 2m : 0m);
                    if (insuranceBet > 0m)
                    {
                        Console.WriteLine($"You placed insurance ${insuranceBet}.");
                        bankroll -= insuranceBet;
                    }
                    else Console.WriteLine("You didn't have enough to place insurance.");
                }
            }

            // Check for natural blackjacks
            bool dealerHasBJ = dealer.IsBlackjack; // we don't peek in advance, but computed here
            bool playerHasBJ = player.IsBlackjack;

            if (playerHasBJ || dealerHasBJ)
            {
                Console.WriteLine($"DEALER HOLE CARD WAS {dealer.Cards[1]}");
                Console.WriteLine($"DEALER HAS {dealer.Total}");
                if (dealerHasBJ && insuranceBet > 0m)
                {
                    Console.WriteLine("Insurance pays 2:1.");
                    bankroll += insuranceBet * 3m; // original stake back + 2x winnings already had stake subtracted
                    insuranceBet = 0m;
                }
                else if (insuranceBet > 0m)
                {
                    Console.WriteLine("Dealer does not have blackjack — insurance lost.");
                    // stake already deducted
                }

                if (playerHasBJ && dealerHasBJ) { log.Add("Both blackjack → push."); return (RoundResult.Push, 0m); }
                if (playerHasBJ)
                {
                    decimal win = bet * 3m / 2m; // 3:2
                    log.Add($"Player blackjack. +{win}");
                    return (RoundResult.Blackjack, win);
                }
                else
                {
                    log.Add("Dealer blackjack.");
                    return (RoundResult.DealerBlackjack, -bet);
                }
            }

            // Offer double down (first decision only)
            bool doubled = false;
            var action = AskAction("DO YOU WANT TO [H]IT, [S]TAND, or [D]OUBLE? ");
            if (action == Action.Double)
            {
                if (bankroll - bet < bet) { Console.WriteLine("Not enough bankroll to double. Defaulting to HIT."); action = Action.Hit; }
                else
                {
                    bankroll -= bet; // put the extra bet out
                    bet *= 2m;
                    doubled = true;
                    var card = shoe.Deal();
                    player.Add(card);
                    Console.WriteLine($"You doubled and drew {card}. YOU HAVE {player.Total}.");
                    log.Add($"Player doubled → drew {card}, total {player.Total}");
                    if (!player.IsBusted)
                        goto DealerPlay;
                    // else fall through to bust
                }
            }

            // If didn’t double, continue standard hit/stand loop
            if (!doubled)
            {
                while (true)
                {
                    if (action == Action.Stand) break;
                    var card = shoe.Deal();
                    player.Add(card);
                    Console.WriteLine($"YOU DREW {card}. YOU HAVE {player.Total}.");
                    log.Add($"Player hit → {card}, total {player.Total}");
                    if (player.IsBusted) break;

                    action = AskAction("DO YOU WANT A HIT? ([H]it/[S]tand) ");
                }
            }

        DealerPlay:
            if (player.IsBusted)
            {
                Console.WriteLine("YOU BUSTED.");
                log.Add("Player busted.");
                return (RoundResult.DealerWin, -bet);
            }

            // Reveal dealer and play to 17+
            Console.WriteLine($"DEALER HOLE CARD WAS {dealer.Cards[1]}");
            Console.WriteLine($"DEALER HAS {dealer.Total}");
            log.Add($"Dealer reveals → {dealer}, total {dealer.Total}");

            while (dealer.Total < 17) // dealer stands on ALL 17s
            {
                var d = shoe.Deal();
                dealer.Add(d);
                Console.WriteLine($"DEALER HITS {d.Rank}. DEALER HAS {dealer.Total}");
                log.Add($"Dealer hits → {d}, total {dealer.Total}");
            }

            if (dealer.IsBusted)
            {
                Console.WriteLine("DEALER BUSTED.");
                log.Add("Dealer busted.");
                return (RoundResult.PlayerWin, +bet);
            }

            // Compare totals
            Console.WriteLine($"YOU HAVE {player.Total}. DEALER HAS {dealer.Total}.");
            if (player.Total > dealer.Total) { log.Add("Player beats dealer."); return (RoundResult.PlayerWin, +bet); }
            if (player.Total < dealer.Total) { log.Add("Dealer beats player."); return (RoundResult.DealerWin, -bet); }
            log.Add("Push.");
            return (RoundResult.Push, 0m);
        }

        // -------- helpers --------
        void Intro()
        {
            Console.WriteLine("WELCOME TO DIGITAL EDUSYSTEM COMPUTER BLACKJACK!!");
            Console.WriteLine("Your dealer tonight is *Petey P. Eight*. Watch him closely — he deals from the bottom of the deck (kidding).");
            Console.WriteLine("Dealer stands on 17. Blackjack pays 3:2. You may double down on your first decision.");
            Console.WriteLine("Type Q at any prompt to quit.\n");
        }

        decimal AskMoney(string prompt, decimal min)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") Quit();
                if (decimal.TryParse(s, out var v) && v >= min) return v;
                Console.WriteLine($"Please enter a number ≥ {min}.");
            }
        }

        (decimal value, bool quit) ReadDecimalOrQuit()
        {
            var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            if (s == "Q") return (0m, true);
            if (!decimal.TryParse(s, out var v)) { Console.WriteLine("Please enter a number."); return ReadDecimalOrQuit(); }
            return (v, false);
        }

        static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") Quit();
                if (s.StartsWith("Y")) return true;
                if (s.StartsWith("N")) return false;
            }
        }

        static Action AskAction(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") Quit();
                if (s == "H") return Action.Hit;
                if (s == "S") return Action.Stand;
                if (s == "D") return Action.Double;
            }
        }

        static void Quit()
        {
            Console.WriteLine("Quitting…");
            Environment.Exit(0);
        }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var g = new Game();
            g.Run();
        }
    }
}
