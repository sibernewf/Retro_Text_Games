using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace OrientExpress1923
{
    internal static class Program
    {
        static readonly Random Rng = new Random();

        // Core tables (1-based for easier mapping to original listing)
        static string[] City = new string[26];
        static string[] Country = new string[26];
        static int[] Meals = new int[26];     // 0 none, 1 breakfast, 2 lunch, 3 dinner, 4 sleeping
        static int[] Convos = new int[26];    // number of conversations in segment
        static int[] DayOffset = new int[26]; // printed day delta (added to Feb 13 + delay)
        static int[] Arr = new int[26];       // arrival time HHMM
        static int[] Dep = new int[26];       // departure time HHMM
        static int[] HazardTag = new int[26]; // (unused in original data; we leave at 0)

        static string[] People = new string[26];

        // Conversation pool (shuffled order in CS order)
        static int[] CS = new int[26];     // indices into CText/CPerson (shuffled)
        static int[] CPerson = new int[26];// fixed speaker index or 0 → random celeb
        static string[] CText = new string[26];

        // Menus
        static string[] Breakfast = new string[14];
        static string[] Dinner = new string[27];

        // Race/ride state
        static int seg;               // 1..24
        static int delayDays;         // HY
        static int totalDay;          // used to know if we’ve printed a date recently
        static int buzzerCount;       // CM
        static int minuteClock;       // T (HHMM-ish integer)
        static int hxBanditsOnce;     // HX (0=not yet)
        static int hwDerailOnce;      // HW (0=not yet)

        // Endgame identities
        static int defectorSlot;      // A3 in spirit
        static int killerSlot;        // A4 in spirit
        static int guessDefector;     // A1
        static int guessKiller;       // A2
        static int solvedBothFlag;    // A5

        static void Main()
        {
            Console.Clear();
            Center("The Orient Express, 1923");
            Center("(c) David H. Ahl, 1986");
            Console.WriteLine();
            Console.Write("Press any key to continue. ");
            Console.ReadKey(true);
            Console.Clear();

            Center("The Mysterious Arms Deal");
            Console.WriteLine();
            ScenarioIntro();

            InitJourneyData();
            InitStatements();
            InitPassengers();
            InitMenus();
            ShuffleConvoOrder();

            // choose who’s who (five arms dealers = 1..5)
            defectorSlot = Rng.Next(1, 6);
            do { killerSlot = Rng.Next(1, 6); } while (killerSlot == defectorSlot);

            Console.Write("Press any key to call a taxi…");
            Console.ReadKey(true);

            // Main journey: 24 segments
            for (seg = 1; seg <= 24; seg++)
            {
                Console.WriteLine();
                int printedDay = 13 + DayOffset[seg] + delayDays;
                Console.WriteLine($"February {printedDay}, 1923");

                int lateness = 18 - Rng.Next(0, 27); // -8 .. +18
                int scheduledArr = Arr[seg];         // HHMM-style
                int actualArr = scheduledArr + lateness; // still HHMM-ish; we’ll print with helper
                minuteClock = actualArr;

                if (seg == 1)
                {
                    Console.WriteLine("The taxi has dropped you at Victoria Station in London.");
                    Console.WriteLine("The Orient Express is standing majestically on Track 14.");
                }
                else
                {
                    WhistleMaybe();
                    Console.Write($"You have arrived at {City[seg]}, {Country[seg]} at ");
                    PrintHHMM(ref minuteClock);

                    if (lateness > 1) Console.WriteLine($"just {lateness} minutes late.");
                    else if (lateness < -1) Console.WriteLine($"almost {Math.Abs(lateness)} minutes early.");
                    else Console.WriteLine("— right on time!");
                }

                // ensure next departure not earlier than arrival
                int scheduledDep = Dep[seg];
                if (seg == 24)
                {
                    EndOfJourney();
                    return;
                }

                // compute actual departure time printed
                minuteClock = (actualArr > scheduledDep - 2) ? actualArr + 4 : scheduledDep;

                if (Meals[seg] >= 4) // sleeping through departure
                {
                    Console.WriteLine("Asleep in your compartment, you barely notice that the");
                    Console.Write("departure was right on time at ");
                    PrintHHMM(ref minuteClock);
                    Pause();
                    BoardSequence();
                }
                else
                {
                    if (seg == 23)
                        AskIdentify(); // before departing Uzunkopru

                    Console.Write("Departure is at ");
                    PrintHHMM(ref minuteClock);
                    Console.WriteLine();
                    if (YesNo("Would you like to get off and stretch your legs?"))
                        Console.WriteLine("Okay, but be sure not to miss the train.");
                    else
                        Console.WriteLine("Okay, you stay in your compartment.");

                    Console.WriteLine();
                    Console.Beep(500, 150);
                    Pause();
                    Console.Beep(500, 300);
                    Console.Write("All aboard…");
                    Pause();
                    Console.Write("train is leaving.");
                    Pause();
                }

                TrainNoises();

                // First leg: intro chat
                if (seg == 1)
                {
                    int x = 3 + Rng.Next(0, 20);
                    Console.WriteLine();
                    Console.WriteLine($"You speak to some of the passengers—{People[x]},");
                    Console.WriteLine($"{People[x + 1]}, {People[x + 2]} and others—and ask them to keep");
                    Console.WriteLine("their eyes and ears open and to pass any information—");
                    Console.WriteLine("no matter how trivial—to you in compartment 13. The Channel");
                    Console.WriteLine("crossing is pleasant and the first part of the trip uneventful.");
                }

                if (seg == 23) CheckIdentities();  // news after your guess at Uzunkopru

                // Meals
                if (Meals[seg] >= 1 && Meals[seg] <= 3)
                {
                    switch (Meals[seg])
                    {
                        case 1: ServeBreakfast(); break;
                        case 2: ServeLunch(); break;
                        case 3: ServeDinner(); break;
                    }
                }

                // Conversations for this segment
                HaveConversations();

                // Hazards (original had HZ(seg) gating; routines include their own % chances)
                SnowRoutine();
                BanditsRoutine();

                // Misc derailments (once)
                MiscHazards();
            }
        }

        // ========== Scenario text ==========
        static void ScenarioIntro()
        {
            Console.WriteLine(" It is February 1923. The following note is received at");
            Console.WriteLine("Whitehall: 'If you will furnish me with a new identity and a");
            Console.WriteLine("lifetime supply of Scotch, I will give up my life of arms dealing");
            Console.WriteLine("and will provide you with much valuable information. I will be");
            Console.WriteLine("on the Orient Express tonight. But you must contact me before");
            Console.WriteLine("the train reaches Uzunkopru or that swine dealer of Maxim machine");
            Console.WriteLine("guns will have me killed by bandits like he did to Baron Wunster");
            Console.WriteLine("last month.' The note is not signed.");
            Console.WriteLine(" You, a British agent, are assigned to take the train, rescue");
            Console.WriteLine("the defector, and arrest the killer.");
            Console.WriteLine(" You know there are five notorious arms dealers of different");
            Console.WriteLine("nationalities operating in Europe under an uneasy truce as each");
            Console.WriteLine("deals in a different kind of weapon. But it is obvious that the");
            Console.WriteLine("truce has ended.");
            Console.WriteLine();
        }

        // ========== Meals ==========
        static void ServeBreakfast()
        {
            Console.WriteLine();
            Console.WriteLine("Breakfast is now being served in the restaurant car.");
            Console.Write("Press any key when you're ready to have breakfast.");
            Console.ReadKey(true);
            Console.Clear();
            Center("BREAKFAST MENU");
            for (int i = 1; i <= 4; i++)
            {
                int idx = 3 * (i - 1) + 1 + Rng.Next(0, 3); // 1..3, 4..6, 7..9, 10..12
                Center(Breakfast[idx]);
            }
            Console.SetCursorPosition(0, Math.Min(Console.CursorTop + 4, Console.BufferHeight - 1));
            Center(Breakfast[13]);
            FinishEating();
        }

        static void ServeLunch()
        {
            Console.WriteLine();
            Console.Write("An enormous buffet luncheon has been laid out in the restaurant car. ");
            Console.Write("Press any key when you have finished. ");
            Console.ReadKey(true);
            Console.WriteLine("B-U-R-P !");
        }

        static void ServeDinner()
        {
            Console.WriteLine();
            Console.WriteLine("Dinner is now being served in the restaurant car.");
            Console.Write("Press any key when you're ready to have dinner.");
            Console.ReadKey(true);
            Console.Clear();
            Center("DINNER MENU");
            for (int i = 1; i <= 7; i++)
            {
                int idx = 3 * (i - 1) + 1 + Rng.Next(0, 3); // 1..3, 4..6, ..., 19..21
                Center(Dinner[idx]);
            }
            Console.WriteLine();
            Center(Dinner[22]);
            Center(Dinner[23]);
            Center(Dinner[24]);
            FinishEating();
        }

        static void FinishEating()
        {
            Console.SetCursorPosition(0, Math.Min(Console.CursorTop + 1, Console.BufferHeight - 1));
            Center("Press any key when you have finished eating");
            Console.ReadKey(true);
            Console.Clear();
        }

        // ========== Conversations ==========
        static void HaveConversations()
        {
            for (int k = 1; k <= Convos[seg]; k++)
            {
                RingBuzzer();
                buzzerCount++;
                int csIdx = CS[buzzerCount];

                int speaker = CPerson[csIdx] > 0 ? CPerson[csIdx] : (3 + Rng.Next(0, 23));
                Console.Write($"Standing there is {People[speaker]}, who tells you:\n");

                string msg = CText[csIdx];
                if (msg.Length <= 80)
                {
                    Console.WriteLine(msg);
                }
                else
                {
                    // wrap near 79 at last space
                    int cut = msg.LastIndexOf(' ', Math.Min(79, msg.Length - 1));
                    if (cut <= 0) cut = Math.Min(79, msg.Length - 1);
                    Console.WriteLine(msg.Substring(0, cut));
                    Console.WriteLine(msg.Substring(cut + 1));
                }
            }
        }

        // ========== Hazards ==========
        static void SnowRoutine()
        {
            double x = Rng.NextDouble();
            if (x > .65) return; // most of the time, nothing

            Console.WriteLine();
            Console.Write("It is snowing heavily ");
            if (x < .01)
            {
                Console.WriteLine("and the train is forced to slow down.");
                Console.WriteLine();
                Console.WriteLine("Oh no! The train is coming to a stop. Let's hope this is");
                Console.WriteLine("not a repeat of the trip of January 29, 1929 when the Orient");
                Console.WriteLine("Express was stuck in snowdrifts for five days.");
                Pause();
                Console.WriteLine("But it looks like it is!");
                Pause();
                Console.WriteLine("You are stranded for two days until a snowplow clears the track.");
                Console.WriteLine("The train is now exactly two days behind schedule.");
                delayDays += 2;
            }
            else
            {
                Console.WriteLine("but the tracks have been cleared and the train");
                Console.WriteLine("will not be delayed.");
            }
        }

        static void BanditsRoutine()
        {
            if (Rng.NextDouble() > .04) return; // 4% chance
            if (hxBanditsOnce == 1) return;
            hxBanditsOnce = 1;

            Console.WriteLine();
            Console.WriteLine("You are rudely awakened from a deep sleep by a loud noise");
            Console.WriteLine("as the train jerks to a halt.");
            RingBuzzer();
            Console.WriteLine("You are shocked to see a bandit waving a gun in your face.");
            Console.WriteLine("He demands that you give him your wallet, jewelry, and watch.");
            Console.WriteLine();
            Pause();
            Console.WriteLine("The bandits are off the train in a few moments with");
            Console.WriteLine("their loot. They disappear into the forest. No one");
            Console.WriteLine("was injured, and the train resumes its journey.");
        }

        static void MiscHazards()
        {
            if (Rng.NextDouble() > .02) return; // 2% chance
            if (hwDerailOnce == 1) return;
            hwDerailOnce = 1;

            Console.WriteLine();
            Console.WriteLine("You hear a loud screeching noise as the train comes to a");
            Console.WriteLine("crashing stop. The engine, tender, and first coach are");
            Console.WriteLine("leaning at a crazy angle. People are screaming.");
            Pause();
            Console.WriteLine();
            Console.WriteLine("While not as bad as the derailment at Vitry-le-Francois in");
            Console.WriteLine("November 1911, there is no question that the front of the");
            Console.WriteLine("train has left the track.");
            Pause();
            Console.WriteLine();
            Console.WriteLine("You are stranded for exactly one day while the track is");
            Console.WriteLine("repaired and a new locomotive obtained.");
            delayDays += 1;
        }

        // ========== Identify section ==========
        static void AskIdentify()
        {
            Console.WriteLine();
            Console.WriteLine("The Turkish police have boarded the train. They have been");
            Console.WriteLine("asked to assist you, but for them to do so you will have to");
            Console.WriteLine("identify the killer (the dealer in machine guns) and the defector");
            Console.WriteLine("(the Scotch drinker) to them. The arms dealers are lined");
            Console.WriteLine("up as follows:\n");
            Console.WriteLine(" (1) Austrian, (2) Turk, (3) Pole, (4) Greek, (5) Rumanian.\n");

            guessDefector = AskInt("Who is the defector (a number please)", 1, 5);
            guessKiller = AskInt("and who is the killer", 1, 5);
            Pause();

            Console.WriteLine();
            Console.WriteLine("The police take into custody the man you identified as the");
            Console.WriteLine("killer and provide a guard to ride on the train with the");
            Console.WriteLine("defector. You return to your compartment, praying that");
            Console.WriteLine("you made the correct deductions and identified the right men.");
            Console.WriteLine();
            Pause();
        }

        static void CheckIdentities()
        {
            if (guessDefector == defectorSlot || guessDefector == killerSlot)
            {
                // defector not killed
                Console.WriteLine();
                Console.WriteLine("You are suddenly awakened by what sounded like a gunshot.");
                Console.WriteLine("You rush to the defector's compartment, but he is okay.");
                Console.WriteLine("However, one of the other arms dealers has been shot.");
                Pause();
                Console.WriteLine();
                Console.WriteLine("You review the details of the case in your mind and realize");
                Console.WriteLine("that you came to the wrong conclusion and due to your mistake");
                Console.WriteLine("a man lies dead at the hands of bandits. You return to your");
                Console.WriteLine("compartment and are consoled by the thought that you correctly");
                Console.WriteLine("identified the killer and that he will hang for his crimes.");
                if (guessKiller == killerSlot) solvedBothFlag = 1;
                return;
            }

            // killer still aboard?
            if (guessKiller == killerSlot)
            {
                solvedBothFlag = 1;
                return;
            }

            // Killer visits…
            RingBuzzer();
            Console.WriteLine("A man is standing outside. He says, 'You made a");
            Console.WriteLine("mistake. A bad one. You see, I am the machine-gun dealer.");
            if (guessDefector == killerSlot)
            {
                Console.WriteLine("Moreover, you incorrectly identified the man who was cooperating");
                Console.WriteLine("with you as the killer. So the state will take care of him. Ha.");
            }
            Console.WriteLine();
            Pause();
            Console.WriteLine("He draws a gun. BANG. You are dead.");
            Console.WriteLine();
            Console.WriteLine("You never know that the train arrived at 12:30, right on");
            Console.WriteLine("time at Constantinople, Turkey.");
            Pause(); Pause();
            Console.WriteLine();
            Console.WriteLine();
            EndOfJourney(); // ends game
            Environment.Exit(0);
        }

        // ========== Ending ==========
        static void EndOfJourney()
        {
            Console.WriteLine();
            Console.WriteLine("Your journey has ended. Georges Nagelmackers and the");
            Console.WriteLine("management of Cie. Internationale des Wagons-Lits");
            Console.WriteLine("hope you enjoyed your trip on the Orient Express, the");
            Console.WriteLine("most famous train in the world.\n");

            if (solvedBothFlag == 1)
            {
                Console.Beep(); Console.Beep(); Console.Beep();
                Console.WriteLine("Whitehall telegraphs congratulations for identifying both");
                Console.WriteLine("the killer and defector correctly.");
                Pause(); Pause();

                for (int i = 0; i < 25; i++)
                {
                    Thread.Sleep(25);
                    Console.SetCursorPosition(30, Math.Min(Console.CursorTop + 1, Console.BufferHeight - 1));
                    Console.Write((i % 2 == 0) ? "CONGRATULATIONS !" : "                 ");
                    Console.Beep(880, 40);
                }
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
            }

            if (YesNo("Would you like to ride again?"))
            {
                Console.WriteLine("Okay. Good journey!");
                Pause();
                // reset state and reshuffle
                Array.Clear(HazardTag, 0, HazardTag.Length);
                Array.Clear(CS, 0, CS.Length);
                buzzerCount = 0; hxBanditsOnce = 0; hwDerailOnce = 0;
                solvedBothFlag = 0;
                ShuffleConvoOrder();
                defectorSlot = Rng.Next(1, 6);
                do { killerSlot = Rng.Next(1, 6); } while (killerSlot == defectorSlot);
                delayDays = 0;
                Console.Clear();
                Main();
            }
            else
            {
                Console.WriteLine("Okay. So long for now.");
                Pause();
                Console.Clear();
            }
        }

        // ========== Utilities ==========
        static void TrainNoises()
        {
            Console.WriteLine();
            Console.WriteLine("Clackety clack…clackety clack…clackety clack");
            if (Rng.NextDouble() > .5) return;
            for (int ka = 6; ka >= 1; ka--)
            {
                for (int i = 0; i < 4; i++) { Console.Beep(130, 50); Thread.Sleep(30); }
                if (ka == 4) WhistleMaybe();
                Thread.Sleep(50 + ka * 120);
            }
        }

        static void WhistleMaybe()
        {
            if (Rng.NextDouble() > .5) return;
            Console.Beep(500, 150);
            Thread.Sleep(140);
            Console.Beep(500, 60);
            Thread.Sleep(80);
            Console.Beep(500, 220);
        }

        static void BoardSequence() { /* timing already simulated above */ }

        static void RingBuzzer()
        {
            Console.WriteLine();
            Console.WriteLine("Your compartment buzzer rings…");
            Console.Beep();
            Thread.Sleep(120);
            Console.Beep();
            Console.Write("Press any key to open the door.");
            Console.ReadKey(true);
            Console.WriteLine();
        }

        static void PrintHHMM(ref int tHHMM)
        {
            // emulate BASIC’s trick: if minutes overflow, add 40
            int h = (tHHMM / 100) % 100;
            int m = tHHMM % 100;
            if (m > 59) { h += 1; m -= 60; }
            h = (h + 24) % 24;
            Console.Write($" {h:00}:{m:00} ");
        }

        static bool YesNo(string prompt)
        {
            Console.Write(prompt + " ");
            var s = Console.ReadLine() ?? "";
            if (s.Length == 0) return true;
            char c = char.ToUpperInvariant(s[0]);
            if (c == 'Y') return true;
            if (c == 'N') return false;
            Console.WriteLine("Please enter Y for 'yes' or N for 'no.'");
            return YesNo(prompt);
        }

        static int AskInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt + ": ");
                string s = Console.ReadLine() ?? "";
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v)
                    && v >= min && v <= max)
                    return v;
                Console.WriteLine("Please enter a valid number.");
            }
        }

        static void Center(string s)
        {
            int width = 70;
            int pad = Math.Max(0, (width - s.Length) / 2);
            Console.WriteLine(new string(' ', pad) + s);
        }

        static void Pause() => Thread.Sleep(400);

        // ========== Data loads (from BASIC DATA lines) ==========
        static void InitJourneyData()
        {
            // X, ME, CN, DA, TA, TD, City, Country
            (int X, int ME, int CN, int DA, int TA, int TD, string CA, string CB)[] rows = new[]
            {
                (1, 0, 0, 0, 1430, 0, "London", "England"),
                (2, 1, 2, 0, 1855, 1919, "Calais", "France"),
                (3, 0, 1, 0, 2233, 2253, "Paris (Nord)", "France"),
                (4, 4, 0, 0, 2316, 2350, "Paris (Lyon)", "France"),
                (5, 4, 0, 1,  600,  620, "Vallorbe", "Switzerland"),
                (6, 0, 1, 0,  700,  707, "Lausanne", "Switzerland"),
                (7, 3, 1, 1,  732,  734, "Montreux", "Switzerland"),
                (8, 0, 1, 1,  919,  927, "Brig", "Switzerland"),
                (9, 0, 3, 0, 1005, 1025, "Domodossola", "Italy"),
                (10,2, 2, 0, 1223, 1320, "Milan", "Italy"),
                (11,1, 2, 0, 1705, 1730, "Venice (S. Lucia)", "Italy"),
                (12,0, 1, 0, 1954, 2014, "Trieste", "(Free State)"),
                (13,0, 1, 0, 2044, 2110, "Opicina", "Italy"),
                (14,0, 2, 0, 2119, 2225, "Sezana", "Slovenia"),
                (15,4, 0, 0,   21,  107, "Ljubljana", "Slovenia"),
                (16,4, 0, 0,  310,  330, "Zagreb", "Croatia"),
                (17,3, 2, 0,  900,  956, "Belgrade", "Serbia"),
                (18,2, 1, 0, 1334, 1356, "Crveni Krst", "Serbia"),
                (19,0, 2, 0, 1555, 1634, "Caribrod", "Serbia"),
                (20,1, 2, 0, 1856, 1935, "Sofia", "Bulgaria"),
                (21,4, 0, 2,   45,  120, "Svilengrad", "Bulgaria"),
                (22,4, 0, 2,  406,  445, "Pithion", "Greece"),
                (23,3, 0, 3,  505,  545, "Uzunkopru", "Turkey"),
                (24,0, 0, 0, 1230,    0, "Constantinople", "Turkey")
            };
            for (int i = 1; i <= 24; i++)
            {
                Meals[i] = rows[i - 1].ME;
                Convos[i] = rows[i - 1].CN;
                DayOffset[i] = rows[i - 1].DA;
                Arr[i] = rows[i - 1].TA;
                Dep[i] = rows[i - 1].TD;
                City[i] = rows[i - 1].CA;
                Country[i] = rows[i - 1].CB;
            }
        }

        static void InitStatements()
        {
            (int cs, int cp, string line)[] rows = new[]
            {
                (1, 0, "I've heard they all have different color chalets on a north-south ridge in the Tyrol region."),
                (2, 0, "The Austrian said he likes the look of natural wood and would never paint his chalet."),
                (3, 0, "They gave the waiter a difficult time. The Turk ordered beer and the other four all ordered different drinks."),
                (4, 0, "The Greek told me he hunts deer, but he never hunts with any of the others because they all hunt different animals."),
                (5, 1, "My brother delivered a case of Kirsch to the green chalet. He remembers it being just south of the gaudy red chalet."),
                (6, 0, "The Pole asked me—can you imagine that?—if I wanted to buy any howitzers."),
                (7, 2, "One of them asked me to cook some pheasant that he shot. He said that I should come to the yellow chalet."),
                (8, 1, "One time my brother said he delivered a case of Cognac to the middle chalet."),
                (9, 0, "The Rumanian said he had the shortest distance to drive from his chalet to the railroad station at Munich."),
                (10,0, "One of them bragged that his military rifles were so accurate that he bagged a fox with one of them."),
                (11,0, "The man who hunts wild boar said that the pistol dealer who lives in the chalet next to his often gives loud parties."),
                (12,0, "The pheasant hunter complained that the arms dealer in the chalet next to his makes far too much noise testing his mortars."),
                (13,0, "The gin drinker bragged that he shot sixty warthogs on a single day last August."),
                (14,0, "The Rumanian said he looks out on a blue chalet."),
                (15,0, "The Cognac drinker bragged that he is the best hunter and can drink more than all of the rest of them combined."),
                (16,0, "The one carrying the pistol said he thinks the boar's head over his neighbor's doorway is revolting."),
                (17,0, "One of them said that one day he'd like to lob a mortar shell at the string of pheasants drying in his neighbor's yard."),
                (18,0, "The Kirsch drinker said he loved the roast chicken he had to eat last night."),
                (19,0, "The one carrying the pistol had a second helping of pie."),
                (20,0, "One commented that his beef dinner wasn't nearly as good as the boar that he shot last week."),
                (21,0, "The Pole asked for more soup."),
                (22,0, "The one eating all the cheese mumbled that it was the same color as his chalet."),
                (23,0, "The Rumanian and Austrian got completely drunk last night."),
                (24,0, "I'd like to visit the blue chalet. The owner is said to serve excellent lobster.")
            };
            for (int i = 1; i <= 24; i++)
            {
                CS[i] = rows[i - 1].cs;
                CPerson[i] = rows[i - 1].cp;
                CText[i] = rows[i - 1].line;
            }
        }

        static void InitPassengers()
        {
            string[] names = {
                "R. Brundt (a waiter)", "C. D'Arcy (a chef)",
                "Herbert Hoover", "Baron Rothschild", "Guido Famadotta", "Gustav Mahler",
                "Robert Baden-Powell", "Fritz Kreisler", "Dame Melba", "Gerald Murphy",
                "Calouste Gulbenkian", "Captain G.T. Ward", "Sir Ernest Cassel",
                "Major Custance", "F. Scott Fitzgerald", "Elsa Maxwell", "Mata Hari",
                "Clayton Pasha", "Arturo Toscanini", "Maharajah Behar", "Leon Wenger",
                "Sarah Bernhardt", "Arthur Vetter", "Isadora Duncan", "David K.E. Bruce"
            };
            for (int i = 1; i <= 24; i++) People[i] = names[i - 1];
        }

        static void InitMenus()
{
    // Breakfast (13 items in data)
    string[] mb =
    {
        "Variete Jus de Fruits", "Prunes Macerees dans le Vin",
        "Demi Pamplemouse", "Trois Oeufs sur le Plat", "Oeufs Poches",
        "Omelette aux Champignons", "Tranches de Pain Beurees et Confiturees",
        "Galettes", "Pommes-Frites", "Patisseries", "Croissants", "Yogurt",
        "Cafe, The, Lait, Vin, Eau Minerale"
    };

    // allocate 1-based storage
    Breakfast = new string[mb.Length + 1];
    for (int i = 1; i <= mb.Length; i++) Breakfast[i] = mb[i - 1];

    // Dinner (the BASIC lists 26; our text has 25—this code adapts to whatever is here)
    string[] md =
    {
        "Huitres de Beernham", "Cantaloup glace au Marsale",
        "Compote des Tomates Fraiches", "Potage Reine",
        "La Natte de Sole au Beurre", "Truite de riviere meuniere",
        "Poulet de grain grille a Diable", "Roti de Veau a l'Osille",
        "Truite Saumonee a la Chambord", "Chaud-froid de Caneton",
        "Chaudfroix des Langouste a la Parisienne",
        "Les Noisettes de Chevreuil Renaissance", "Becasses a la Monaco",
        "Pointes d'asperge a la creme", "Parfait de foies gras",
        "Salade Catalane", "Truffes au Champagne",
        "Tagliatelle de carottes et courgettes", "Souffle d'Anisette",
        "Creme de Caramel blond", "Sorbet aux Mures de Framboisier",
        "La selection du Maitre Fromager", "Corbeille de Fruits",
        "Les Mignardises", "Selection du vins et liquors"
    };

    Dinner = new string[md.Length + 1];
    for (int i = 1; i <= md.Length; i++) Dinner[i] = md[i - 1];
}

        static void ShuffleConvoOrder()
        {
            // Fisher-Yates on CS[1..24]
            for (int i = 1; i <= 23; i++)
            {
                int k = i + Rng.Next(0, 25 - i);
                (CS[i], CS[k]) = (CS[k], CS[i]);
            }
        }
    }
}
