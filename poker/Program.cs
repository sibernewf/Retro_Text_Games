using System;
using System.Collections.Generic;
using System.Linq;

namespace DrawPoker
{
    enum Suit { Clubs, Diamonds, Hearts, Spades }
    enum Rank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

    readonly struct Card
    {
        public Rank Rank { get; }
        public Suit Suit { get; }
        public Card(Rank r, Suit s) { Rank = r; Suit = s; }
        public override string ToString()
        {
            string r = Rank switch
            {
                Rank.Jack => "J",
                Rank.Queen => "Q",
                Rank.King => "K",
                Rank.Ace => "A",
                _ => ((int)Rank).ToString()
            };
            string s = Suit switch
            {
                Suit.Clubs => "♣",
                Suit.Diamonds => "♦",
                Suit.Hearts => "♥",
                _ => "♠"
            };
            return $"{r}{s}";
        }
    }

    sealed class Deck
    {
        private readonly List<Card> _cards = new(52);
        private readonly Random _rng;
        private int _i;

        public Deck(Random rng)
        {
            _rng = rng;
            foreach (Suit s in Enum.GetValues(typeof(Suit)))
                foreach (Rank r in Enum.GetValues(typeof(Rank)))
                    _cards.Add(new Card(r, s));
        }
        public void Shuffle()
        {
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }
            _i = 0;
        }
        public Card Deal() => _cards[_i++];
    }

    enum HandCategory
    {
        HighCard = 1,
        Pair = 2,
        TwoPair = 3,
        ThreeKind = 4,
        Straight = 5,
        Flush = 6,
        FullHouse = 7,
        FourKind = 8,
        StraightFlush = 9
    }

    readonly struct HandRank : IComparable<HandRank>
    {
        public HandCategory Category { get; }
        public int[] Tiebreak { get; } // high first, length varies
        public HandRank(HandCategory cat, IEnumerable<int> tie)
        {
            Category = cat;
            Tiebreak = tie.ToArray();
        }
        public int CompareTo(HandRank other)
        {
            int c = Category.CompareTo(other.Category);
            if (c != 0) return c;
            for (int i = 0; i < Math.Max(Tiebreak.Length, other.Tiebreak.Length); i++)
            {
                int a = i < Tiebreak.Length ? Tiebreak[i] : 0;
                int b = i < other.Tiebreak.Length ? other.Tiebreak[i] : 0;
                if (a != b) return a.CompareTo(b);
            }
            return 0;
        }
        public override string ToString() => Category.ToString().ToUpper().Replace("KIND", " OF A KIND");
    }

    static class HandEval
    {
        // Return a full ranking (category + kickers) with A-low straight handling
        public static HandRank Evaluate(IReadOnlyList<Card> hand)
        {
            // counts by rank
            var groups = hand.GroupBy(c => c.Rank)
                             .Select(g => (rank: (int)g.Key, cnt: g.Count()))
                             .OrderByDescending(t => t.cnt)
                             .ThenByDescending(t => t.rank)
                             .ToList();

            bool flush = hand.All(c => c.Suit == hand[0].Suit);
            var ranks = hand.Select(c => (int)c.Rank).OrderBy(x => x).ToArray();

            bool straight = IsStraight(ranks, out int straightHigh);

            if (straight && flush)
                return new HandRank(HandCategory.StraightFlush, new[] { straightHigh });

            if (groups[0].cnt == 4)
            {
                int four = groups[0].rank;
                int kicker = groups[1].rank;
                return new HandRank(HandCategory.FourKind, new[] { four, kicker });
            }

            if (groups[0].cnt == 3 && groups[1].cnt == 2)
                return new HandRank(HandCategory.FullHouse, new[] { groups[0].rank, groups[1].rank });

            if (flush)
                return new HandRank(HandCategory.Flush, ranks.Reverse());

            if (straight)
                return new HandRank(HandCategory.Straight, new[] { straightHigh });

            if (groups[0].cnt == 3)
            {
                var kickers = groups.Skip(1).Select(g => g.rank);
                return new HandRank(HandCategory.ThreeKind, (new[] { groups[0].rank }).Concat(kickers));
            }

            if (groups[0].cnt == 2 && groups[1].cnt == 2)
            {
                int hiPair = Math.Max(groups[0].rank, groups[1].rank);
                int loPair = Math.Min(groups[0].rank, groups[1].rank);
                int kicker = groups[2].rank;
                return new HandRank(HandCategory.TwoPair, new[] { hiPair, loPair, kicker });
            }

            if (groups[0].cnt == 2)
            {
                var kickers = groups.Skip(1).Select(g => g.rank);
                return new HandRank(HandCategory.Pair, (new[] { groups[0].rank }).Concat(kickers));
            }

            return new HandRank(HandCategory.HighCard, ranks.Reverse());

            static bool IsStraight(int[] sortedAsc, out int high)
            {
                // normal case
                bool seq = true;
                for (int i = 1; i < sortedAsc.Length; i++)
                    if (sortedAsc[i] != sortedAsc[0] + i) { seq = false; break; }
                if (seq) { high = sortedAsc[^1]; return true; }

                // A-low straight (A,2,3,4,5)
                if (sortedAsc.SequenceEqual(new[] { 2, 3, 4, 5, 14 }))
                {
                    high = 5;
                    return true;
                }

                high = 0;
                return false;
            }
        }

        public static string Describe(IReadOnlyList<Card> hand)
            => string.Join("  ", hand.Select(c => c.ToString()));
    }

    sealed class Player
    {
        public string Name { get; }
        public int Money { get; set; } = 200;
        public bool HasWatch { get; set; } = true;
        public bool HasTieTack { get; set; } = true;
        public List<Card> Cards { get; } = new(5);
        public Player(string name) { Name = name; }
        public override string ToString() => Name;
        public void ClearHand() => Cards.Clear();
    }

    internal static class Program
    {
        private static readonly Random Rng = new();

        private const int Ante = 5;

        static void Main()
        {
            Console.Title = "Draw Poker (Player vs Computer)";
            Console.WriteLine("WELCOME TO THE HALIDON CASINO.  WE EACH HAVE $200.");
            Console.WriteLine("THE ANTE IS $5.  I OPEN THE BETTING BEFORE THE DRAW; YOU OPEN AFTER.");
            Console.WriteLine("FOLD = 0,  CHECK (before-draw) = 5.  NUMBERS ARE DOLLARS.  GOOD LUCK!\n");

            var you = new Player(ReadLineNonEmpty("WHAT IS YOUR NAME? "));
            var cpu = new Player("DEALER");

            var deck = new Deck(Rng);

            while (you.Money > 0 && cpu.Money > 0)
            {
                // collect ante
                if (!EnsureFunds(you, Ante)) break;
                if (!EnsureFunds(cpu, Ante)) break;
                you.Money -= Ante; cpu.Money -= Ante;
                int pot = Ante * 2;

                deck.Shuffle();
                you.ClearHand(); cpu.ClearHand();
                for (int i = 0; i < 5; i++) { you.Cards.Add(deck.Deal()); cpu.Cards.Add(deck.Deal()); }

                Console.WriteLine($"\nTHE ANTE IS ${Ante}.  I WILL DEAL.");
                ShowYourHand(you);

                // ----- COMPUTER OPENS BEFORE DRAW -----
                int openBet = CpuOpenBet(cpu.Cards);
                int currentToCall = 0;

                if (openBet > 0)
                {
                    // place the computer's bet
                    openBet = Math.Min(openBet, cpu.Money);
                    cpu.Money -= openBet; pot += openBet;
                    currentToCall = openBet;
                    Console.WriteLine($"I'LL OPEN WITH ${openBet}.");
                }
                else
                {
                    Console.WriteLine("I CHECK.");
                    Console.WriteLine("IF YOU CAN'T SEE MY BET, THEN FOLD.");
                }

                // Player decision before draw
                int yourBet = GetBetFromPlayer($"WHAT IS YOUR BET? ", you.Money, allowZero: true);

                if (openBet == 0)
                {
                    // player must either check with 5 or start a bet
                    if (yourBet == 0)
                    {
                        Console.WriteLine("YOU FOLD.");
                        cpu.Money += pot;  // dealer wins pot
                        if (!AskCont()) break;
                        else continue;
                    }
                    if (yourBet == 5)
                    {
                        Console.WriteLine("I'LL SEE YOU.");
                        you.Money -= 5; pot += 5;
                        currentToCall = 0; // no more to call before draw
                    }
                    else
                    {
                        you.Money -= yourBet; pot += yourBet;
                        currentToCall = yourBet;
                        // dealer must decide to see or fold
                        if (!CpuCallOrFold(cpu, cpu.Cards, currentToCall))
                        {
                            Console.WriteLine("I FOLD.");
                            you.Money += pot;
                            if (!AskCont()) break; else continue;
                        }
                        Console.WriteLine("I'LL SEE YOU.");
                        cpu.Money -= currentToCall; pot += currentToCall;
                        currentToCall = 0;
                    }
                }
                else
                {
                    // computer has already bet: player must match or fold
                    if (yourBet == 0)
                    {
                        Console.WriteLine("YOU FOLD.");
                        cpu.Money += pot;
                        if (!AskCont()) break; else continue;
                    }
                    else
                    {
                        if (yourBet != openBet)
                        {
                            // treat as call if >=; as invalid if less than
                            if (yourBet < openBet) { yourBet = openBet; }
                        }
                        you.Money -= yourBet; pot += yourBet;
                        currentToCall = 0; // matched
                        Console.WriteLine("I'LL SEE YOU.");
                    }
                }

                // ----- DRAW PHASE -----
                YouDraw(you, deck);
                CpuDraw(cpu, deck);

                // ----- PLAYER OPENS AFTER DRAW -----
                ShowYourHand(you);

                int playerOpen = GetBetFromPlayer("WHAT IS YOUR BET? ", you.Money, allowZero: true);
                if (playerOpen == 0)
                {
                    Console.WriteLine("I'LL SEE YOU.");
                }
                else
                {
                    you.Money -= playerOpen; pot += playerOpen;
                    if (!CpuCallOrFoldAfter(cpu, cpu.Cards, playerOpen))
                    {
                        Console.WriteLine("I FOLD.");
                        you.Money += pot;
                        if (!AskCont()) break; else continue;
                    }
                    Console.WriteLine("I'LL SEE YOU, AND RAISE YOU...");
                    int raise = CpuRaiseAmount(cpu.Cards, playerOpen, cpu.Money);
                    raise = Math.Min(raise, cpu.Money);
                    if (raise > 0)
                    {
                        Console.WriteLine($"I RAISE ${raise}.");
                        cpu.Money -= raise; pot += raise;
                        int toCall = raise;
                        int call = GetBetFromPlayer("WHAT IS YOUR BET? ", you.Money, allowZero: true);
                        if (call == 0 || call < toCall)
                        {
                            Console.WriteLine("YOU FOLD.");
                            cpu.Money += pot;
                            if (!AskCont()) break; else continue;
                        }
                        you.Money -= toCall; pot += toCall;
                    }
                    else
                    {
                        Console.WriteLine("...SMALL CHANGE, PLEASE.");
                    }
                }

                // ----- SHOWDOWN -----
                Console.WriteLine("\nNOW WE COMPARE HANDS");
                var yourRank = HandEval.Evaluate(you.Cards);
                var cpuRank = HandEval.Evaluate(cpu.Cards);
                Console.WriteLine($"YOUR HAND:  {yourRank}   ({HandEval.Describe(you.Cards)})");
                Console.WriteLine($"MY HAND:    {cpuRank}   ({HandEval.Describe(cpu.Cards)})");

                int cmp = yourRank.CompareTo(cpuRank);
                if (cmp > 0)
                {
                    Console.WriteLine("YOU WIN!");
                    you.Money += pot;
                }
                else if (cmp < 0)
                {
                    Console.WriteLine("I WIN.");
                    cpu.Money += pot;
                }
                else
                {
                    // tie-breaker by total rank sum of cards (almost never hit after full rank compare)
                    int youSum = you.Cards.Sum(c => (int)c.Rank);
                    int cpuSum = cpu.Cards.Sum(c => (int)c.Rank);
                    if (youSum >= cpuSum) { Console.WriteLine("YOU WIN!"); you.Money += pot; }
                    else { Console.WriteLine("I WIN."); cpu.Money += pot; }
                }

                Console.WriteLine($"\nYOU HAVE ${you.Money} AND I HAVE ${cpu.Money}");
                if (!AskCont()) break;
            }

            Console.WriteLine("\nGAME OVER.  THANKS FOR PLAYING!");
        }

        // ----------------- I/O helpers -----------------

        static string ReadLineNonEmpty(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
            }
        }
        static bool AskYes(string prompt)
        {
            Console.Write($"{prompt} (YES/NO) > ");
            var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            return s is "Y" or "YES";
        }
        static int GetBetFromPlayer(string prompt, int max, bool allowZero)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim();
                if (int.TryParse(s, out int v))
                {
                    if (v == 0 && allowZero) return 0;
                    if (v > 0 && v <= max) return v;
                }
                Console.WriteLine("ENTER A WHOLE DOLLAR AMOUNT WITHIN YOUR CASH (OR 0 TO FOLD).");
            }
        }

        static void ShowYourHand(Player you)
        {
            Console.WriteLine("\nYOUR HAND:");
            for (int i = 0; i < you.Cards.Count; i++)
            {
                Console.WriteLine($"{i + 1,2} -- {you.Cards[i]}");
            }
            Console.WriteLine();
        }

        // ----------------- Money/sell-offs -----------------

        static bool EnsureFunds(Player p, int needed)
        {
            if (p.Money >= needed) return true;

            while (p.Money < needed)
            {
                if (p.HasWatch && AskYes("YOU'RE SHORT.  WOULD YOU LIKE TO SELL YOUR WATCH FOR $75?"))
                {
                    p.HasWatch = false; p.Money += 75;
                    Console.WriteLine("SOLD.");
                    continue;
                }
                if (p.HasTieTack && AskYes("HOW ABOUT THAT DIAMOND TIE TACK FOR $125?"))
                {
                    p.HasTieTack = false; p.Money += 125;
                    Console.WriteLine("SOLD.");
                    continue;
                }
                Console.WriteLine("YOU'RE BUSTED.");
                return false;
            }
            return true;
        }

        static bool AskCont() => AskYes("DO YOU WISH TO CONTINUE?");

        // ----------------- CPU logic -----------------

        static int CpuOpenBet(IReadOnlyList<Card> hand)
        {
            var r = HandEval.Evaluate(hand);
            // very rough sizing
            return r.Category switch
            {
                HandCategory.StraightFlush => 50,
                HandCategory.FourKind => 45,
                HandCategory.FullHouse => 35,
                HandCategory.Flush or HandCategory.Straight => 25,
                HandCategory.ThreeKind => 20,
                HandCategory.TwoPair => 12,
                HandCategory.Pair => (r.Tiebreak[0] >= (int)Rank.Jack ? 10 : 0),
                _ => 0
            };
        }

        static bool CpuCallOrFold(Player cpu, IReadOnlyList<Card> hand, int toCall)
        {
            var r = HandEval.Evaluate(hand);
            int willing = r.Category switch
            {
                HandCategory.StraightFlush or HandCategory.FourKind => 100,
                HandCategory.FullHouse => 60,
                HandCategory.Flush or HandCategory.Straight => 45,
                HandCategory.ThreeKind => 35,
                HandCategory.TwoPair => 25,
                HandCategory.Pair => (r.Tiebreak[0] >= (int)Rank.Ten ? 18 : 10),
                _ => 8
            };
            return toCall <= Math.Min(willing, cpu.Money);
        }

        static bool CpuCallOrFoldAfter(Player cpu, IReadOnlyList<Card> hand, int toCall)
        {
            // a bit stickier after improvement
            var r = HandEval.Evaluate(hand);
            int willing = r.Category switch
            {
                HandCategory.StraightFlush or HandCategory.FourKind => 999,
                HandCategory.FullHouse => 120,
                HandCategory.Flush or HandCategory.Straight => 80,
                HandCategory.ThreeKind => 60,
                HandCategory.TwoPair => 40,
                HandCategory.Pair => (r.Tiebreak[0] >= (int)Rank.Queen ? 28 : 18),
                _ => 10
            };
            return toCall <= Math.Min(willing, cpu.Money);
        }

        static int CpuRaiseAmount(IReadOnlyList<Card> hand, int yourOpen, int cpuMoney)
        {
            var r = HandEval.Evaluate(hand);
            int target = r.Category switch
            {
                HandCategory.StraightFlush => 60,
                HandCategory.FourKind => 50,
                HandCategory.FullHouse => 40,
                HandCategory.Flush or HandCategory.Straight => 24,
                HandCategory.ThreeKind => 18,
                HandCategory.TwoPair => 12,
                HandCategory.Pair => 8,
                _ => 0
            };
            int raise = Math.Max(0, target - yourOpen);
            return Math.Min(raise, cpuMoney);
        }

        // ------------- Draw strategies -------------

        static void YouDraw(Player you, Deck deck)
        {
            int n = AskDrawCount("NOW WE DRAW — HOW MANY CARDS DO YOU WANT? ");
            if (n == 0) { Console.WriteLine("YOU STAND PAT."); return; }
            var idxs = AskWhichCards(n, you.Cards.Count);
            foreach (int i in idxs.OrderByDescending(i => i))
                you.Cards.RemoveAt(i);
            for (int i = 0; i < n; i++) you.Cards.Add(deck.Deal());
        }

        static int AskDrawCount(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim();
                if (int.TryParse(s, out int n) && n >= 0 && n <= 3) // classic draw: max 3 to avoid abuse
                    return n;
                Console.WriteLine("YOU CAN'T DRAW MORE THAN THREE CARDS (0–3).");
            }
        }

        static List<int> AskWhichCards(int n, int handSize)
        {
            Console.Write("WHAT ARE THEIR NUMBERS (e.g., 1 3 5)? ");
            while (true)
            {
                var parts = ((Console.ReadLine() ?? "").Trim()).Split(new[] { ' ', ',', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                var nums = new List<int>();
                foreach (var p in parts)
                    if (int.TryParse(p, out int k) && k >= 1 && k <= handSize) nums.Add(k - 1);
                nums = nums.Distinct().ToList();
                if (nums.Count == n) return nums;
                Console.Write($"PLEASE ENTER EXACTLY {n} DISTINCT INDEX/INDICES BETWEEN 1 AND {handSize}: ");
            }
        }

        static void CpuDraw(Player cpu, Deck deck)
        {
            // simple rules: keep made hands; draw to 4-flush or 4-straight; else keep high cards
            var h = cpu.Cards;
            var rank = HandEval.Evaluate(h);

            List<int> toDiscard = new();

            if (rank.Category >= HandCategory.TwoPair)
            {
                // stand pat on two pair or better (can break two pair sometimes, but keep it simple)
            }
            else if (rank.Category == HandCategory.Pair)
            {
                int pRank = rank.Tiebreak[0];
                // keep pair, discard the 3 others
                var keep = h.Select((c, i) => (c, i))
                            .Where(t => (int)t.c.Rank == pRank)
                            .Select(t => t.i).ToHashSet();
                for (int i = 0; i < h.Count; i++) 
                    if (!keep.Contains(i)) toDiscard.Add(i);
            }
            else
            {
                // draw to 4-flush?
                var suitGroup = h.GroupBy(c => c.Suit).OrderByDescending(g => g.Count()).First();
                if (suitGroup.Count() == 4)
                {
                    for (int i = 0; i < h.Count; i++) if (h[i].Suit != suitGroup.Key) toDiscard.Add(i);
                }
                else
                {
                    // keep Ace/King and draw 3–4
                    var keepIdx = h.Select((c, i) => (c, i))
                                   .Where(t => t.c.Rank >= Rank.King)
                                   .Select(t => t.i).ToHashSet();
                    for (int i = 0; i < h.Count; i++) if (!keepIdx.Contains(i)) toDiscard.Add(i);
                    // cap to max 3 discards for our rules
                    while (toDiscard.Count > 3) toDiscard.RemoveAt(toDiscard.Count - 1);
                }
            }

            foreach (int i in toDiscard.OrderByDescending(i => i))
                h.RemoveAt(i);
            for (int i = 0; i < toDiscard.Count; i++) h.Add(deck.Deal());

            Console.WriteLine($"I AM TAKING {toDiscard.Count} CARD{(toDiscard.Count == 1 ? "" : "S")}");
        }
    }
}
