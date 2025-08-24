using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MNOPLY
{
    // ---------- Models ----------
    enum SpaceType { Go, Property, Railroad, Utility, Tax, Chance, CommunityChest, Jail, FreeParking, GoToJail }
    enum ColorGroup { None, Brown, LightBlue, Pink, Orange, Red, Yellow, Green, DarkBlue }

    abstract class Space
    {
        public int Index { get; }
        public string Name { get; }
        public SpaceType Type { get; }
        protected Space(int index, string name, SpaceType type) { Index = index; Name = name; Type = type; }
    }

    sealed class Property : Space
    {
        public ColorGroup Group { get; }
        public int Price { get; }
        public int HouseCost { get; }
        public int[] Rent { get; } // [0]=base, [1..4]=houses, [5]=hotel
        public Player Owner { get; set; }
        public int Houses { get; set; } // 0..4; 5=hotel
        public bool Mortgaged { get; set; }
        public Property(int index, string name, ColorGroup group, int price, int houseCost, int[] rent)
            : base(index, name, SpaceType.Property)
        { Group = group; Price = price; HouseCost = houseCost; Rent = rent; }
        public int CurrentRent(bool ownerHasMonopoly)
        {
            if (Mortgaged || Owner == null) return 0;
            if (Houses > 0) return Rent[Math.Min(Houses, 5)];
            return ownerHasMonopoly ? Rent[0] * 2 : Rent[0];
        }
    }

    sealed class Railroad : Space
    {
        public int Price { get; }
        public Player Owner { get; set; }
        public bool Mortgaged { get; set; }
        public Railroad(int index, string name, int price) : base(index, name, SpaceType.Railroad) { Price = price; }
        public int Rent(int ownedCount) => Mortgaged || Owner == null ? 0 : ownedCount switch { 1 => 25, 2 => 50, 3 => 100, _ => 200 };
    }

    sealed class Utility : Space
    {
        public int Price { get; }
        public Player Owner { get; set; }
        public bool Mortgaged { get; set; }
        public Utility(int index, string name, int price) : base(index, name, SpaceType.Utility) { Price = price; }
        public int Rent(int diceTotal, bool ownsBoth) => Mortgaged || Owner == null ? 0 : (ownsBoth ? 10 : 4) * diceTotal;
    }

    sealed class TaxSpace : Space
    {
        public int Amount { get; }
        public TaxSpace(int index, string name, int amount) : base(index, name, SpaceType.Tax) { Amount = amount; }
    }

    sealed class Player
    {
        public string Name { get; }
        public int Money { get; set; } = 1500;
        public int Position { get; set; } = 0;
        public bool InJail { get; set; }
        public int GetOutOfJailCards { get; set; }
        public Player(string name) { Name = name; }
        public override string ToString() => Name;
    }

    // ---------- Game ----------
    internal static class Program
    {
        private static readonly Random Rng = new Random();

        // Runtime board (loaded from JSON or fallback)
        private static List<Space> Board = new();

        // Decks
        private static Queue<Action<GameState>> ChanceDeck = new();
        private static Queue<Action<GameState>> ChestDeck = new();

        static void Main()
        {
            Console.Title = "MNOPLY — Monopoly for Two (Console)";
            Console.WriteLine("=== MNOPLY — THIS IS MONOPOLY FOR TWO ===\n");

            // ---- choose board file (robust search + seeding) ----
            var boardsDir = FindBoardsDir();
            Directory.CreateDirectory(boardsDir);
            SeedBoardsIfEmpty(boardsDir);

            var files = Directory.GetFiles(boardsDir, "*.json", SearchOption.TopDirectoryOnly)
                                 .OrderBy(f => Path.GetFileName(f)).ToList();

            Console.WriteLine($"Available gameboards (Boards/*.json) in:\n  {boardsDir}");
            if (files.Count == 0) Console.WriteLine("  (none found)");
            else for (int i = 0; i < files.Count; i++) Console.WriteLine($"  {i + 1}. {Path.GetFileName(files[i])}");
            Console.WriteLine("Press Enter for default: classic-us.json");
            Console.Write("Select by number or filename: ");

            string choice = Console.ReadLine();
            string selectedPath = Path.Combine(boardsDir, "classic-us.json");

            if (!string.IsNullOrWhiteSpace(choice))
            {
                choice = choice.Trim();
                if (int.TryParse(choice, out int idx) && idx >= 1 && idx <= files.Count)
                    selectedPath = files[idx - 1];
                else
                {
                    var typed = Path.Combine(boardsDir, choice);
                    if (File.Exists(typed)) selectedPath = typed;
                    else Console.WriteLine("Not found; using default classic-us.json.");
                }
            }

            if (!TryLoadBoardFromJson(selectedPath, out Board))
            {
                Console.WriteLine("Could not load board file; falling back to built-in classic board.");
                Board = BuildClassicBoard();
            }

            var p1 = new Player(ReadLineNonEmpty("WHO IS PLAYER #1? "));
            var p2 = new Player(ReadLineNonEmpty("WHO IS PLAYER #2? "));
            Console.WriteLine();

            InitDecks();
            var gs = new GameState(p1, p2);

            while (true)
            {
                var current = gs.Current;
                var other = gs.Other;

                // Jail handling
                if (current.InJail)
                {
                    Console.WriteLine($"\n** {current} IS IN JAIL **");
                    if (current.GetOutOfJailCards > 0)
                    {
                        current.GetOutOfJailCards--;
                        current.InJail = false;
                        Console.WriteLine($"{current} USED A 'GET OUT OF JAIL FREE' CARD.");
                    }
                    else
                    {
                        Console.WriteLine($"{current} PAYS $50 TO LEAVE JAIL.");
                        Charge(gs, current, 50, null);
                        current.InJail = false;
                    }
                }

                Console.WriteLine($"\n==> {current}'S TURN <==");
                // -------- Command loop until ROLL --------
                while (true)
                {
                    Console.Write("Type ROLL (or HELP) > ");
                    string cmd = Console.ReadLine();
                    if (cmd == null) continue;
                    cmd = cmd.Trim().ToUpperInvariant();

                    if (cmd == "ROLL") break;
                    else if (cmd == "HELP" || cmd == "?") { ShowHelp(); continue; }
                    else if (cmd == "CASH" || cmd == "MONEY") { Console.WriteLine($"{current} HAS ${current.Money}"); continue; }
                    else if (cmd == "OWN" || cmd == "PROPS") { ListOwned(current); continue; }
                    else if (cmd == "STATUS") { ShowStatus(gs, current); continue; }
                    else if (cmd == "WHERE") { Console.WriteLine($"YOU ARE ON {Board[current.Position].Name}"); continue; }
                    else if (cmd == "MORTGAGE") { VoluntaryMortgage(gs, current); continue; }

                    Console.WriteLine("Unknown command. Type HELP for a list.");
                }

                int d1 = Rng.Next(1, 7), d2 = Rng.Next(1, 7);
                int dice = d1 + d2;
                Console.WriteLine($"YOU ROLLED A {d1} AND A {d2}");

                int oldPos = current.Position;
                int newPos = (oldPos + dice) % Board.Count;
                if (oldPos + dice >= Board.Count)
                {
                    Console.WriteLine("YOU PASSED GO AND COLLECT $200");
                    current.Money += 200;
                }
                current.Position = newPos;

                gs.TurnStampSpaceIndex = null; // reset re-entry guard per landing
                var landed = Board[current.Position];
                Console.WriteLine($"YOU ARE ON {landed.Name.ToUpperInvariant()}");

                switch (landed.Type)
                {
                    case SpaceType.Go: break;
                    case SpaceType.FreeParking: Console.WriteLine("FREE PARKING. TAKE A BREAK."); break;
                    case SpaceType.Jail: Console.WriteLine("JUST VISITING JAIL."); break;
                    case SpaceType.GoToJail: Console.WriteLine("GO TO JAIL!"); SendToJail(current); break;
                    case SpaceType.Tax: var tax = (TaxSpace)landed; Console.WriteLine($"PAY TAX OF ${tax.Amount}"); Charge(gs, current, tax.Amount, null); break;
                    case SpaceType.Property: HandleProperty(gs, (Property)landed, dice); break;
                    case SpaceType.Railroad: HandleRailroad(gs, (Railroad)landed); break;
                    case SpaceType.Utility: HandleUtility(gs, (Utility)landed, dice); break;
                    case SpaceType.Chance: DrawChance(gs); break;
                    case SpaceType.CommunityChest: DrawChest(gs); break;
                }

                if (IsBankrupt(current)) { Console.WriteLine($"\n{current} IS BANKRUPT. {other} IS THE WINNER!"); break; }

                // Optional improvements
                if (AskYes("\nIF YOU WANT TO IMPROVE YOUR PROPERTY TYPE HOUSE"))
                    MaybeImproveProperties(gs, current);

                if (IsBankrupt(current)) { Console.WriteLine($"\n{current} IS BANKRUPT. {other} IS THE WINNER!"); break; }

                gs.NextTurn();
            }
        }

        // ---------- Turn handlers ----------
        private static void HandleProperty(GameState gs, Property p, int dice)
        {
            if (gs.TurnStampSpaceIndex == p.Index) return; gs.TurnStampSpaceIndex = p.Index;
            var current = gs.Current;

            if (p.Owner == null)
            {
                if (PromptBuy(p.Name, p.Price)) TryBuy(current, p.Price, () => p.Owner = current);
                return;
            }
            if (p.Owner == current) { Console.WriteLine("YOU OWN THIS PROPERTY."); return; }

            bool ownerHasMono = HasMonopoly(p.Owner, p.Group);
            int rent = p.CurrentRent(ownerHasMono);
            Console.WriteLine($"YOU OWE {p.Owner} ${rent} DOLLARS RENT");
            Charge(gs, current, rent, p.Owner);
        }

        private static void HandleRailroad(GameState gs, Railroad rr)
        {
            if (gs.TurnStampSpaceIndex == rr.Index) return; gs.TurnStampSpaceIndex = rr.Index;
            var current = gs.Current;

            if (rr.Owner == null)
            {
                if (PromptBuy(rr.Name, rr.Price)) TryBuy(current, rr.Price, () => rr.Owner = current);
                return;
            }
            if (rr.Owner == current) { Console.WriteLine("YOU OWN THIS RAILROAD."); return; }

            int count = Board.OfType<Railroad>().Count(r => r.Owner == rr.Owner);
            int rent = rr.Rent(count);
            Console.WriteLine($"YOU OWE {rr.Owner} ${rent} DOLLARS RENT");
            Charge(gs, current, rent, rr.Owner);
        }

        private static void HandleUtility(GameState gs, Utility ut, int dice)
        {
            if (gs.TurnStampSpaceIndex == ut.Index) return; gs.TurnStampSpaceIndex = ut.Index;
            var current = gs.Current;

            if (ut.Owner == null)
            {
                if (PromptBuy(ut.Name, ut.Price)) TryBuy(current, ut.Price, () => ut.Owner = current);
                return;
            }
            if (ut.Owner == current) { Console.WriteLine("YOU OWN THIS UTILITY."); return; }

            bool ownsBoth = Board.OfType<Utility>().All(u => u.Owner == ut.Owner);
            int rent = ut.Rent(dice, ownsBoth);
            Console.WriteLine($"YOU OWE {ut.Owner} ${rent} DOLLARS");
            Charge(gs, current, rent, ut.Owner);
        }

        // ---------- Chance / Chest ----------
        private static void DrawChance(GameState gs)
        {
            if (ChanceDeck.Count == 0) InitDecks();
            Console.WriteLine("CHANCE!");
            var action = ChanceDeck.Dequeue();
            action(gs);
            ChanceDeck.Enqueue(action);
        }

        private static void DrawChest(GameState gs)
        {
            if (ChestDeck.Count == 0) InitDecks();
            Console.WriteLine("COMMUNITY CHEST!");
            var action = ChestDeck.Dequeue();
            action(gs);
            ChestDeck.Enqueue(action);
        }

        // ---------- Improve ----------
        private static void MaybeImproveProperties(GameState gs, Player p)
        {
            var groups = Enum.GetValues(typeof(ColorGroup))
                             .Cast<ColorGroup>()
                             .Where(g => g != ColorGroup.None)
                             .ToList();

            var myGroups = groups.Where(g => HasMonopoly(p, g)).ToList();

            if (!myGroups.Any()) { Console.WriteLine("YOU DO NOT OWN ANY COMPLETE COLOR GROUPS."); return; }

            foreach (var g in myGroups)
            {
                var props = Board.OfType<Property>().Where(x => x.Group == g && x.Owner == p).OrderBy(x => x.Index).ToList();
                int cur = props.Min(x => x.Houses);
                int target = ReadInt($"HOW MANY HOUSES (0–5, 5=HOTEL) DO YOU WANT ON EACH {g.ToString().ToUpper()} LOT? CURRENT MIN {cur}: ", 0, 5);
                if (target == cur) continue;

                int diff = Math.Max(0, target - cur);
                int perLotCost = diff * props[0].HouseCost;
                int total = perLotCost * props.Count;

                if (p.Money < total) { Console.WriteLine($"INSUFFICIENT FUNDS. NEED ${total}, YOU HAVE ${p.Money}"); continue; }

                p.Money -= total;
                foreach (var pr in props) pr.Houses = target;
                Console.WriteLine($"BUILT UP TO {target} ON EACH {g} LOT FOR ${total}.");
            }
        }

        // ---------- Economy ----------
        private static void Charge(GameState gs, Player payer, int amount, Player payee)
        {
            if (amount <= 0) return;
            payer.Money -= amount;
            if (payee != null) payee.Money += amount;

            if (payer.Money < 0)
            {
                Console.WriteLine($"{payer} CANNOT AFFORD THE PAYMENT.");
                TryLiquidate(gs, payer);
            }
        }

        private static void TryLiquidate(GameState gs, Player p)
        {
            while (p.Money < 0)
            {
                Console.WriteLine($"\n{p} MUST RAISE CASH. MONEY = ${p.Money}.");
                MortgageMenu(gs, p);
                if (p.Money < 0)
                {
                    Console.WriteLine("STILL SHORT — MORTGAGE MORE OR YOU WILL GO BANKRUPT.");
                    if (!HasMortgageables(p)) break;
                }
            }
        }

        private static void VoluntaryMortgage(GameState gs, Player p)
        {
            Console.WriteLine("\n-- MORTGAGE MENU --");
            MortgageMenu(gs, p);
        }

        private static void MortgageMenu(GameState gs, Player p)
        {
            while (true)
            {
                var options = BuildMortgageOptions(p);
                if (options.Count == 0) { Console.WriteLine("NO UNMORTGAGED, UNIMPROVED PROPERTIES TO MORTGAGE."); return; }

                Console.WriteLine("TYPE A NUMBER TO MORTGAGE, OR 0 TO EXIT:");
                for (int i = 0; i < options.Count; i++)
                    Console.WriteLine($"{i + 1}. {options[i].label}");

                Console.Write("> ");
                int sel;
                if (!int.TryParse(Console.ReadLine(), out sel) || sel < 0 || sel > options.Count) { Console.WriteLine("INVALID."); continue; }
                if (sel == 0) return;
                options[sel - 1].act();
                Console.WriteLine($"NEW BALANCE: ${p.Money}");
            }
        }

        private static bool HasMortgageables(Player p) => BuildMortgageOptions(p).Count > 0;

        private static List<(string label, Action act)> BuildMortgageOptions(Player p)
        {
            var list = new List<(string, Action)>();

            foreach (var pr in Board.OfType<Property>().Where(x => x.Owner == p && !x.Mortgaged && x.Houses == 0))
            {
                var prop = pr;
                list.Add(($"MORTGAGE {prop.Name} FOR ${prop.Price / 2}", () => { prop.Mortgaged = true; p.Money += prop.Price / 2; }));
            }

            foreach (var rr in Board.OfType<Railroad>().Where(x => x.Owner == p && !x.Mortgaged))
            {
                var r = rr;
                list.Add(($"MORTGAGE {r.Name} FOR ${r.Price / 2}", () => { r.Mortgaged = true; p.Money += r.Price / 2; }));
            }

            foreach (var ut in Board.OfType<Utility>().Where(x => x.Owner == p && !x.Mortgaged))
            {
                var u = ut;
                list.Add(($"MORTGAGE {u.Name} FOR ${u.Price / 2}", () => { u.Mortgaged = true; p.Money += u.Price / 2; }));
            }

            return list;
        }

        private static bool IsBankrupt(Player p) => p.Money < 0;

        private static void TryBuy(Player p, int price, Action onSuccess)
        {
            if (p.Money < price) { Console.WriteLine("INSUFFICIENT FUNDS."); return; }
            p.Money -= price; onSuccess(); Console.WriteLine($"YOU NOW HAVE ${p.Money}");
        }

        private static bool HasMonopoly(Player p, ColorGroup group)
        {
            var props = Board.OfType<Property>().Where(x => x.Group == group).ToList();
            return props.Count > 0 && props.All(x => x.Owner == p) && props.All(x => !x.Mortgaged);
        }

        private static void SendToJail(Player p) { p.Position = 10; p.InJail = true; }

        // ---------- Decks ----------
        private static void InitDecks()
        {
            ChanceDeck = Shuffle(new[]
            {
                new Action<GameState>(gs => { Console.WriteLine("ADVANCE TO GO (COLLECT $200)"); MovePastGo(gs.Current, 0); }),
                gs => { Console.WriteLine("GO TO JAIL"); SendToJail(gs.Current); },
                gs => { Console.WriteLine("BANK PAYS YOU DIVIDEND OF $50"); gs.Current.Money += 50; },
                gs => { Console.WriteLine("GET OUT OF JAIL FREE"); gs.Current.GetOutOfJailCards++; },
                gs => { Console.WriteLine("GO BACK THREE SPACES"); MoveBy(gs.Current, -3); LandedAgain(gs, 0); },
                gs => { Console.WriteLine("YOUR BUILDING LOAN MATURES — COLLECT $150"); gs.Current.Money += 150; },
                gs => { Console.WriteLine("PAY POOR TAX OF $15"); Charge(gs, gs.Current, 15, null); },
                gs => AdvanceToNearestRailroad(gs),
                gs => AdvanceToNearestUtility(gs),
                gs => { Console.WriteLine("TAKE A WALK ON THE BOARDWALK"); MovePastGo(gs.Current, 39); LandedAgain(gs, 0); },
            });

            ChestDeck = Shuffle(new[]
            {
                new Action<GameState>(gs => { Console.WriteLine("ADVANCE TO GO (COLLECT $200)"); MovePastGo(gs.Current, 0); }),
                gs => { Console.WriteLine("BANK ERROR IN YOUR FAVOR — COLLECT $200"); gs.Current.Money += 200; },
                gs => { Console.WriteLine("DOCTOR'S FEE — PAY $50"); Charge(gs, gs.Current, 50, null); },
                gs => { Console.WriteLine("FROM SALE OF STOCK YOU GET $50"); gs.Current.Money += 50; },
                gs => { Console.WriteLine("GET OUT OF JAIL FREE"); gs.Current.GetOutOfJailCards++; },
                gs => { Console.WriteLine("GO TO JAIL"); SendToJail(gs.Current); },
                gs => { Console.WriteLine("INCOME TAX REFUND — COLLECT $20"); gs.Current.Money += 20; },
                gs => { Console.WriteLine("LIFE INSURANCE MATURES — COLLECT $100"); gs.Current.Money += 100; },
                gs => { Console.WriteLine("PAY HOSPITAL FEES OF $100"); Charge(gs, gs.Current, 100, null); },
                gs => { Console.WriteLine("PAY SCHOOL FEES OF $50"); Charge(gs, gs.Current, 50, null); },
                gs => { Console.WriteLine("RECEIVE $25 CONSULTANCY FEE"); gs.Current.Money += 25; },
                gs => { Console.WriteLine("YOU INHERIT $100"); gs.Current.Money += 100; },
            });
        }

        private static void AdvanceToNearestRailroad(GameState gs)
        {
            Console.WriteLine("ADVANCE TOKEN TO NEAREST RAILROAD");
            var rrIdx = Board.Where(s => s.Type == SpaceType.Railroad).Select(s => s.Index).OrderBy(i => i).ToArray();
            int pos = gs.Current.Position;
            int target = rrIdx.FirstOrDefault(i => i > pos);
            if (target == 0) target = rrIdx.First(); // wrap
            MovePastGo(gs.Current, target);
            LandedAgain(gs, 0);
        }

        private static void AdvanceToNearestUtility(GameState gs)
        {
            Console.WriteLine("ADVANCE TOKEN TO NEAREST UTILITY");
            var uIdx = Board.Where(s => s.Type == SpaceType.Utility).Select(s => s.Index).OrderBy(i => i).ToArray();
            int pos = gs.Current.Position;
            int target = uIdx.FirstOrDefault(i => i > pos);
            if (target == 0) target = uIdx.First(); // wrap
            MovePastGo(gs.Current, target);
            LandedAgain(gs, 0);
        }

        private static void LandedAgain(GameState gs, int lastDice)
        {
            var sp = Board[gs.Current.Position];
            switch (sp.Type)
            {
                case SpaceType.Property: HandleProperty(gs, (Property)sp, lastDice); break;
                case SpaceType.Railroad: HandleRailroad(gs, (Railroad)sp); break;
                case SpaceType.Utility: HandleUtility(gs, (Utility)sp, lastDice); break;
                case SpaceType.Tax: Charge(gs, gs.Current, ((TaxSpace)sp).Amount, null); break;
                case SpaceType.GoToJail: SendToJail(gs.Current); break;
            }
        }

        // ---------- Board: built-in classic fallback ----------
        private static List<Space> BuildClassicBoard()
        {
            var b = new List<Space>(40);
            void Add(Space s) => b.Add(s);

            Add(new GenericSpace(0, "GO", SpaceType.Go));
            Add(new Property(1, "MEDITERRANEAN AVE", ColorGroup.Brown, 60, 50, new[] { 2, 10, 30, 90, 160, 250 }));
            Add(new CommunityChest(2));
            Add(new Property(3, "BALTIC AVE", ColorGroup.Brown, 60, 50, new[] { 4, 20, 60, 180, 320, 450 }));
            Add(new TaxSpace(4, "INCOME TAX", 200));
            Add(new Railroad(5, "READING RAILROAD", 200));
            Add(new Property(6, "ORIENTAL AVE", ColorGroup.LightBlue, 100, 50, new[] { 6, 30, 90, 270, 400, 550 }));
            Add(new Chance(7));
            Add(new Property(8, "VERMONT AVE", ColorGroup.LightBlue, 100, 50, new[] { 6, 30, 90, 270, 400, 550 }));
            Add(new Property(9, "CONNECTICUT AVE", ColorGroup.LightBlue, 120, 50, new[] { 8, 40, 100, 300, 450, 600 }));

            Add(new GenericSpace(10, "JAIL / JUST VISITING", SpaceType.Jail));
            Add(new Property(11, "ST. CHARLES PLACE", ColorGroup.Pink, 140, 100, new[] { 10, 50, 150, 450, 625, 750 }));
            Add(new Utility(12, "ELECTRIC COMPANY", 150));
            Add(new Property(13, "STATES AVE", ColorGroup.Pink, 140, 100, new[] { 10, 50, 150, 450, 625, 750 }));
            Add(new Property(14, "VIRGINIA AVE", ColorGroup.Pink, 160, 100, new[] { 12, 60, 180, 500, 700, 900 }));
            Add(new Railroad(15, "PENNSYLVANIA RAILROAD", 200));
            Add(new Property(16, "ST. JAMES PLACE", ColorGroup.Orange, 180, 100, new[] { 14, 70, 200, 550, 750, 950 }));
            Add(new CommunityChest(17));
            Add(new Property(18, "TENNESSEE AVE", ColorGroup.Orange, 180, 100, new[] { 14, 70, 200, 550, 750, 950 }));
            Add(new Property(19, "NEW YORK AVE", ColorGroup.Orange, 200, 100, new[] { 16, 80, 220, 600, 800, 1000 }));

            Add(new GenericSpace(20, "FREE PARKING", SpaceType.FreeParking));
            Add(new Property(21, "KENTUCKY AVE", ColorGroup.Red, 220, 150, new[] { 18, 90, 250, 700, 875, 1050 }));
            Add(new Chance(22));
            Add(new Property(23, "INDIANA AVE", ColorGroup.Red, 220, 150, new[] { 18, 90, 250, 700, 875, 1050 }));
            Add(new Property(24, "ILLINOIS AVE", ColorGroup.Red, 240, 150, new[] { 20, 100, 300, 750, 925, 1100 }));
            Add(new Railroad(25, "B&O RAILROAD", 200));
            Add(new Property(26, "ATLANTIC AVE", ColorGroup.Yellow, 260, 150, new[] { 22, 110, 330, 800, 975, 1150 }));
            Add(new Property(27, "VENTNOR AVE", ColorGroup.Yellow, 260, 150, new[] { 22, 110, 330, 800, 975, 1150 }));
            Add(new Utility(28, "WATER WORKS", 150));
            Add(new Property(29, "MARVIN GARDENS", ColorGroup.Yellow, 280, 150, new[] { 24, 120, 360, 850, 1025, 1200 }));

            Add(new GenericSpace(30, "GO TO JAIL", SpaceType.GoToJail));
            Add(new Property(31, "PACIFIC AVE", ColorGroup.Green, 300, 200, new[] { 26, 130, 390, 900, 1100, 1275 }));
            Add(new Property(32, "NORTH CAROLINA AVE", ColorGroup.Green, 300, 200, new[] { 26, 130, 390, 900, 1100, 1275 }));
            Add(new CommunityChest(33));
            Add(new Property(34, "PENNSYLVANIA AVE", ColorGroup.Green, 320, 200, new[] { 28, 150, 450, 1000, 1200, 1400 }));
            Add(new Railroad(35, "SHORT LINE", 200));
            Add(new Chance(36));
            Add(new Property(37, "PARK PLACE", ColorGroup.DarkBlue, 350, 200, new[] { 35, 175, 500, 1100, 1300, 1500 }));
            Add(new TaxSpace(38, "LUXURY TAX", 100));
            Add(new Property(39, "BOARDWALK", ColorGroup.DarkBlue, 400, 200, new[] { 50, 200, 600, 1400, 1700, 2000 }));

            return b;
        }

        // ---------- JSON Loader ----------
        private static bool TryLoadBoardFromJson(string path, out List<Space> board)
        {
            board = new List<Space>();
            try
            {
                if (!File.Exists(path)) return false;
                var json = File.ReadAllText(path);
                var model = JsonSerializer.Deserialize<BoardFile>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });
                if (model == null || model.Spaces == null || model.Spaces.Count == 0) return false;

                foreach (var s in model.Spaces.OrderBy(x => x.Index))
                {
                    var type = (SpaceType)Enum.Parse(typeof(SpaceType), s.Type, true);
                    switch (type)
                    {
                        case SpaceType.Property:
                            if (s.Rent == null || s.Rent.Length < 6) throw new Exception("Property missing rent[6]: " + s.Name);
                            var group = (ColorGroup)Enum.Parse(typeof(ColorGroup), s.Group, true);
                            board.Add(new Property(s.Index, s.Name ?? "", group, s.Price ?? 0, s.HouseCost ?? 0, s.Rent));
                            break;
                        case SpaceType.Railroad:
                            board.Add(new Railroad(s.Index, s.Name ?? "RAILROAD", s.Price ?? 0));
                            break;
                        case SpaceType.Utility:
                            board.Add(new Utility(s.Index, s.Name ?? "UTILITY", s.Price ?? 0));
                            break;
                        case SpaceType.Tax:
                            board.Add(new TaxSpace(s.Index, s.Name ?? "TAX", s.Amount ?? 0));
                            break;
                        case SpaceType.Go:            board.Add(new GenericSpace(s.Index, s.Name ?? "GO", SpaceType.Go)); break;
                        case SpaceType.FreeParking:   board.Add(new GenericSpace(s.Index, s.Name ?? "FREE PARKING", SpaceType.FreeParking)); break;
                        case SpaceType.Jail:          board.Add(new GenericSpace(s.Index, s.Name ?? "JAIL / JUST VISITING", SpaceType.Jail)); break;
                        case SpaceType.GoToJail:      board.Add(new GenericSpace(s.Index, s.Name ?? "GO TO JAIL", SpaceType.GoToJail)); break;
                        case SpaceType.Chance:        board.Add(new Chance(s.Index)); break;
                        case SpaceType.CommunityChest:board.Add(new CommunityChest(s.Index)); break;
                        default: throw new Exception("Unsupported space type " + s.Type);
                    }
                }

                if (board.Count != 40) Console.WriteLine($"Warning: board has {board.Count} spaces (expected 40).");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Board load error: " + ex.Message);
                return false;
            }
        }

        // ---------- Small helpers / wrappers ----------
        sealed class GenericSpace : Space { public GenericSpace(int i, string name, SpaceType type) : base(i, name, type) { } }
        sealed class Chance : Space { public Chance(int i) : base(i, "CHANCE", SpaceType.Chance) { } }
        sealed class CommunityChest : Space { public CommunityChest(int i) : base(i, "COMMUNITY CHEST", SpaceType.CommunityChest) { } }

        private static void MoveBy(Player p, int delta)
        {
            int n = (p.Position + delta) % Board.Count;
            if (n < 0) n += Board.Count;
            p.Position = n;
        }

        private static void MovePastGo(Player p, int destIndex)
        {
            if (destIndex < p.Position)
            {
                Console.WriteLine("YOU PASSED GO AND COLLECT $200");
                p.Money += 200;
            }
            p.Position = destIndex;
            Console.WriteLine($"YOU ARE ON {Board[destIndex].Name.ToUpperInvariant()}");
        }

        private static Queue<T> Shuffle<T>(IEnumerable<T> items)
        {
            var list = items.ToList();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
            return new Queue<T>(list);
        }

        private static bool PromptBuy(string thing, int price)
        {
            while (true)
            {
                Console.Write($"BUY {thing} FOR ${price}?  Type BUY or PASS > ");
                string s = Console.ReadLine();
                if (s == null) continue;
                s = s.Trim().ToUpperInvariant();
                if (s == "BUY") return true;
                if (s == "PASS" || s == "") return false;
                if (s == "HELP") { Console.WriteLine("BUY to purchase, PASS to decline."); continue; }
                Console.WriteLine("Please type BUY or PASS.");
            }
        }

        private static bool AskYes(string prompt)
        {
            Console.Write($"{prompt} > ");
            var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            return s == "HOUSE";
        }

        private static string ReadLineNonEmpty(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
            }
        }

        private static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                int n;
                if (int.TryParse(Console.ReadLine(), NumberStyles.Integer, CultureInfo.InvariantCulture, out n) && n >= min && n <= max)
                    return n;
                Console.WriteLine($"ENTER A NUMBER FROM {min} TO {max}.");
            }
        }

        // ---------- JSON DTOs ----------
        class BoardFile
        {
            public string Name { get; set; }
            public List<BoardSpace> Spaces { get; set; }
        }
        class BoardSpace
        {
            public int Index { get; set; }
            public string Type { get; set; } = "";
            public string Name { get; set; }

            public string Group { get; set; }      // for properties
            public int? Price { get; set; }        // property/rail/utility
            public int? HouseCost { get; set; }    // property only
            public int[] Rent { get; set; }        // property only

            public int? Amount { get; set; }       // tax only
        }

        // ---------- UI / Command helpers ----------
        private static void ShowHelp()
        {
            Console.WriteLine(@"
Commands you can use before you ROLL:
  HELP          — show this list
  CASH / MONEY  — show your cash
  OWN / PROPS   — list properties you own
  STATUS        — cash, position, full portfolio
  WHERE         — show the space you're on
  MORTGAGE      — mortgage properties to raise cash
Then type ROLL to continue your turn.");
        }

        // full portfolio listing (used by OWN/PROPS and STATUS)
        private static void ListOwned(Player p)
        {
            var props = Board.OfType<Property>()
                .Where(x => x.Owner == p)
                .OrderBy(x => x.Group).ThenBy(x => x.Index)
                .Select(x =>
                    $"{x.Name} [{x.Group}]" +
                    (x.Mortgaged ? " [MORTGAGED]" :
                     (x.Houses == 0 ? "" : (x.Houses == 5 ? " [HOTEL]" : $" [{x.Houses} HOUSES]"))));

            var rrs = Board.OfType<Railroad>()
                .Where(x => x.Owner == p)
                .OrderBy(x => x.Index)
                .Select(x => $"{x.Name}" + (x.Mortgaged ? " [MORTGAGED]" : ""));

            var uts = Board.OfType<Utility>()
                .Where(x => x.Owner == p)
                .OrderBy(x => x.Index)
                .Select(x => $"{x.Name}" + (x.Mortgaged ? " [MORTGAGED]" : ""));

            Console.WriteLine($"-- {p} OWNS --");
            bool any = false;
            foreach (var s in props) { Console.WriteLine("  " + s); any = true; }
            foreach (var s in rrs)   { Console.WriteLine("  " + s); any = true; }
            foreach (var s in uts)   { Console.WriteLine("  " + s); any = true; }
            if (!any) Console.WriteLine("  (nothing)");
        }

        private static void ShowStatus(GameState gs, Player p)
        {
            Console.WriteLine($"\n-- STATUS: {p} --");
            Console.WriteLine($"CASH: ${p.Money}");
            Console.WriteLine($"POSITION: {Board[p.Position].Name}");
            Console.WriteLine($"GET OUT OF JAIL FREE CARDS: {p.GetOutOfJailCards}");

            var props = Board.OfType<Property>().Where(x => x.Owner == p).ToList();
            var rrs   = Board.OfType<Railroad>().Where(x => x.Owner == p).ToList();
            var utils = Board.OfType<Utility>().Where(x => x.Owner == p).ToList();

            Console.WriteLine($"ASSETS: {props.Count} properties, {rrs.Count} railroads, {utils.Count} utilities");

            if (props.Any())
            {
                Console.WriteLine("  Properties:");
                foreach (var pr in props.OrderBy(x => x.Group).ThenBy(x => x.Index))
                    Console.WriteLine($"    - {pr.Name} ({pr.Group})" + (pr.Mortgaged ? " [MORTGAGED]" :
                                    (pr.Houses == 0 ? "" : (pr.Houses == 5 ? " [HOTEL]" : $" [{pr.Houses} HOUSES]"))));
            }

            if (rrs.Any())
            {
                Console.WriteLine("  Railroads:");
                foreach (var rr in rrs.OrderBy(x => x.Index))
                    Console.WriteLine($"    - {rr.Name}" + (rr.Mortgaged ? " [MORTGAGED]" : ""));
            }

            if (utils.Any())
            {
                Console.WriteLine("  Utilities:");
                foreach (var ut in utils.OrderBy(x => x.Index))
                    Console.WriteLine($"    - {ut.Name}" + (ut.Mortgaged ? " [MORTGAGED]" : ""));
            }
        }

        // ---------- Boards directory helpers ----------
        private static string FindBoardsDir()
        {
            // 1) Next to the executable
            string exeDir = AppContext.BaseDirectory;
            string exeBoards = Path.Combine(exeDir, "Boards");
            if (Directory.Exists(exeBoards) && Directory.GetFiles(exeBoards, "*.json").Any())
                return exeBoards;

            // 2) Current working directory
            string cwdBoards = Path.Combine(Directory.GetCurrentDirectory(), "Boards");
            if (Directory.Exists(cwdBoards) && Directory.GetFiles(cwdBoards, "*.json").Any())
                return cwdBoards;

            // 3) Walk up to find /Boards
            string dir = exeDir;
            for (int i = 0; i < 5 && dir != null; i++)
            {
                string probe = Path.Combine(dir, "Boards");
                if (Directory.Exists(probe) && Directory.GetFiles(probe, "*.json").Any())
                    return probe;
                var parent = Directory.GetParent(dir);
                dir = parent == null ? null : parent.FullName;
            }

            // Fallback: next to the executable
            return exeBoards;
        }

        private static void SeedBoardsIfEmpty(string boardsDir)
        {
            if (Directory.GetFiles(boardsDir, "*.json").Any()) return;
            string path = Path.Combine(boardsDir, "classic-us.json");
            File.WriteAllText(path, ClassicUsJson);
            Console.WriteLine($"Seeded default board: {path}");
        }

        private static readonly string ClassicUsJson = @"
{
  ""name"": ""Classic US"",
  ""spaces"": [
    { ""index"": 0, ""type"": ""Go"", ""name"": ""GO"" },
    { ""index"": 1, ""type"": ""Property"", ""name"": ""MEDITERRANEAN AVE"", ""group"": ""Brown"", ""price"": 60, ""houseCost"": 50, ""rent"": [2,10,30,90,160,250] },
    { ""index"": 2, ""type"": ""CommunityChest"" },
    { ""index"": 3, ""type"": ""Property"", ""name"": ""BALTIC AVE"", ""group"": ""Brown"", ""price"": 60, ""houseCost"": 50, ""rent"": [4,20,60,180,320,450] },
    { ""index"": 4, ""type"": ""Tax"", ""name"": ""INCOME TAX"", ""amount"": 200 },
    { ""index"": 5, ""type"": ""Railroad"", ""name"": ""READING RAILROAD"", ""price"": 200 },
    { ""index"": 6, ""type"": ""Property"", ""name"": ""ORIENTAL AVE"", ""group"": ""LightBlue"", ""price"": 100, ""houseCost"": 50, ""rent"": [6,30,90,270,400,550] },
    { ""index"": 7, ""type"": ""Chance"" },
    { ""index"": 8, ""type"": ""Property"", ""name"": ""VERMONT AVE"", ""group"": ""LightBlue"", ""price"": 100, ""houseCost"": 50, ""rent"": [6,30,90,270,400,550] },
    { ""index"": 9, ""type"": ""Property"", ""name"": ""CONNECTICUT AVE"", ""group"": ""LightBlue"", ""price"": 120, ""houseCost"": 50, ""rent"": [8,40,100,300,450,600] },
    { ""index"": 10, ""type"": ""Jail"", ""name"": ""JAIL / JUST VISITING"" },
    { ""index"": 11, ""type"": ""Property"", ""name"": ""ST. CHARLES PLACE"", ""group"": ""Pink"", ""price"": 140, ""houseCost"": 100, ""rent"": [10,50,150,450,625,750] },
    { ""index"": 12, ""type"": ""Utility"", ""name"": ""ELECTRIC COMPANY"", ""price"": 150 },
    { ""index"": 13, ""type"": ""Property"", ""name"": ""STATES AVE"", ""group"": ""Pink"", ""price"": 140, ""houseCost"": 100, ""rent"": [10,50,150,450,625,750] },
    { ""index"": 14, ""type"": ""Property"", ""name"": ""VIRGINIA AVE"", ""group"": ""Pink"", ""price"": 160, ""houseCost"": 100, ""rent"": [12,60,180,500,700,900] },
    { ""index"": 15, ""type"": ""Railroad"", ""name"": ""PENNSYLVANIA RAILROAD"", ""price"": 200 },
    { ""index"": 16, ""type"": ""Property"", ""name"": ""ST. JAMES PLACE"", ""group"": ""Orange"", ""price"": 180, ""houseCost"": 100, ""rent"": [14,70,200,550,750,950] },
    { ""index"": 17, ""type"": ""CommunityChest"" },
    { ""index"": 18, ""type"": ""Property"", ""name"": ""TENNESSEE AVE"", ""group"": ""Orange"", ""price"": 180, ""houseCost"": 100, ""rent"": [14,70,200,550,750,950] },
    { ""index"": 19, ""type"": ""Property"", ""name"": ""NEW YORK AVE"", ""group"": ""Orange"", ""price"": 200, ""houseCost"": 100, ""rent"": [16,80,220,600,800,1000] },
    { ""index"": 20, ""type"": ""FreeParking"", ""name"": ""FREE PARKING"" },
    { ""index"": 21, ""type"": ""Property"", ""name"": ""KENTUCKY AVE"", ""group"": ""Red"", ""price"": 220, ""houseCost"": 150, ""rent"": [18,90,250,700,875,1050] },
    { ""index"": 22, ""type"": ""Chance"" },
    { ""index"": 23, ""type"": ""Property"", ""name"": ""INDIANA AVE"", ""group"": ""Red"", ""price"": 220, ""houseCost"": 150, ""rent"": [18,90,250,700,875,1050] },
    { ""index"": 24, ""type"": ""Property"", ""name"": ""ILLINOIS AVE"", ""group"": ""Red"", ""price"": 240, ""houseCost"": 150, ""rent"": [20,100,300,750,925,1100] },
    { ""index"": 25, ""type"": ""Railroad"", ""name"": ""B&O RAILROAD"", ""price"": 200 },
    { ""index"": 26, ""type"": ""Property"", ""name"": ""ATLANTIC AVE"", ""group"": ""Yellow"", ""price"": 260, ""houseCost"": 150, ""rent"": [22,110,330,800,975,1150] },
    { ""index"": 27, ""type"": ""Property"", ""name"": ""VENTNOR AVE"", ""group"": ""Yellow"", ""price"": 260, ""houseCost"": 150, ""rent"": [22,110,330,800,975,1150] },
    { ""index"": 28, ""type"": ""Utility"", ""name"": ""WATER WORKS"", ""price"": 150 },
    { ""index"": 29, ""type"": ""Property"", ""name"": ""MARVIN GARDENS"", ""group"": ""Yellow"", ""price"": 280, ""houseCost"": 150, ""rent"": [24,120,360,850,1025,1200] },
    { ""index"": 30, ""type"": ""GoToJail"", ""name"": ""GO TO JAIL"" },
    { ""index"": 31, ""type"": ""Property"", ""name"": ""PACIFIC AVE"", ""group"": ""Green"", ""price"": 300, ""houseCost"": 200, ""rent"": [26,130,390,900,1100,1275] },
    { ""index"": 32, ""type"": ""Property"", ""name"": ""NORTH CAROLINA AVE"", ""group"": ""Green"", ""price"": 300, ""houseCost"": 200, ""rent"": [26,130,390,900,1100,1275] },
    { ""index"": 33, ""type"": ""CommunityChest"" },
    { ""index"": 34, ""type"": ""Property"", ""name"": ""PENNSYLVANIA AVE"", ""group"": ""Green"", ""price"": 320, ""houseCost"": 200, ""rent"": [28,150,450,1000,1200,1400] },
    { ""index"": 35, ""type"": ""Railroad"", ""name"": ""SHORT LINE"", ""price"": 200 },
    { ""index"": 36, ""type"": ""Chance"" },
    { ""index"": 37, ""type"": ""Property"", ""name"": ""PARK PLACE"", ""group"": ""DarkBlue"", ""price"": 350, ""houseCost"": 200, ""rent"": [35,175,500,1100,1300,1500] },
    { ""index"": 38, ""type"": ""Tax"", ""name"": ""LUXURY TAX"", ""amount"": 100 },
    { ""index"": 39, ""type"": ""Property"", ""name"": ""BOARDWALK"", ""group"": ""DarkBlue"", ""price"": 400, ""houseCost"": 200, ""rent"": [50,200,600,1400,1700,2000] }
  ]
}
";

        // ---------- Game state ----------
        sealed class GameState
        {
            public Player P1 { get; }
            public Player P2 { get; }
            public Player Current { get; private set; }
            public Player Other => Current == P1 ? P2 : P1;
            public int? TurnStampSpaceIndex { get; set; } // prevents double-resolve per landing
            public GameState(Player p1, Player p2) { P1 = p1; P2 = p2; Current = P1; }
            public void NextTurn() { Current = Other; TurnStampSpaceIndex = null; }
        }
    }
}
