using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Amelia1937
{
    internal static class Program
    {
        // ========= Random =========
        static readonly Random Rng = new Random();

        // ========= State (1-based like the BASIC) =========
        static string[] LA = new string[33];   // City name
        static string[] LB = new string[33];   // Region/Country
        static int[] FX = new int[33];         // Repair facilities code 1..4
        static int[] CC = new int[33];         // Runway construction code 1..3
        static int[] RR = new int[33];         // Runway length code 1..3
        static int[] WX = new int[33];         // Weather expectation 1..6
        static int[] DX = new int[33];         // Miles to next airport

        // Text tables
        static string[] FText = new string[5];   // repair facilities text (1..4)
        static string[] CText = new string[4];   // runway construction (1..3)
        static string[] RText = new string[4];   // runway length (1..3)
        static string[] WText = new string[7];   // weather (1..6)
        static string[] MText = new string[12];  // malfunction names (1..11)
        static string[] CrowdText = new string[7]; // crowd adjectives (1..6)

        // Game variables
        static int J = 0;         // current segment index (1..32)
        static int JA = 0;        // next destination index
        static int DY = 0;        // day counter (starting May 20 as day 1)
        static int TG = 0;        // ground-time accumulator this day
        static int F;             // facility At current
        static int W;             // expected weather code at current
        static int D;             // distance to next (copy of DX(J))
        static double TM;         // hours since last major overhaul
        static double TC;         // total hours flown
        static int DM;            // miles since last maintenance “chunk” (for failures)
        static int DC;            // total miles flown

        // Crew condition
        static int ND;            // navigator delay flag (1 = wait a day)
        static double NC;         // navigator condition 0..100

        // Maintenance/repair
        static int M;             // malfunction code (0 = none; 1..11 defined)
        static int TP;            // days for major overhaul
        static int TQ;            // days for repair
        static int AB;            // abort flight flag (takeoff failure / turn back)
        static int MJ;            // Java forced return flag
        static int RF;            // Radio retry flag

        // Scratch
        static string MO = "May"; // month name for display
        static int DA;            // day of month for display

        // Entry
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Intro();
            InitText();
            InitAirports();

            Console.Write(new string(' ', 16));
            PressAny("Press any key when you're ready to go");
            Console.Clear();

            // Main loop
            while (true)
            {
                // New day and “alarm clock”
                J++;
                DY++;
                TG = 0;
                Pause(); AlarmClock();

                // Out-of-range guard
                if (J < 1 || J > 32) J = Math.Clamp(J, 1, 32);

                // Set current segment data
                F = FX[J];
                W = WX[J];
                D = DX[J];

                PrintDate();
                Console.WriteLine($"You are at {LA[J]}, {LB[J]}. Repair facilities are {FText[F]}.");
                Console.WriteLine($"Runway is made of {CText[CC[J]]} and is {RText[RR[J]]} for your plane.");

                if (DC > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine($"You have flown {DC} miles in total and you have flown");
                    Console.WriteLine($"{(int)(TM + 0.5)} hours since your last major overhaul.");
                    AircraftRepairs();     // may set TQ and M
                    MajorOverhaul();       // may set TP
                    if (TP + TQ > 0)
                    {
                        TG += TP + TQ;
                        DY += TP + TQ;
                        TP = 0; TQ = 0;
                        PrintDate();
                    }
                }

                PilotCondition();
                NavigatorCondition();
                if (ND == 1)
                {
                    // chose to fly with drunk navigator: we delay below in flow only when asked
                }

                NextDestination();

                // Fuel input
                double FU = AskDouble("How many gallons of fuel do you want in the plane", min: 0, max: 1150, rangeHint: "Maximum capacity is 1150 gallons.");

                // Actual weather: W +/- (roughly)
                int WA = W + (int)Math.Round(1.6 * Rng.NextDouble() - 0.3);
                WA = Math.Clamp(WA, 1, 6);
                Console.WriteLine($"Current weather : {WText[WA]}");
                if (WA >= 3)
                {
                    if (!AskYesNo("Do you want to wait a day for better weather (Y or N)"))
                    {
                        // wait
                        AB = 0;
                        TG += 1; DY += 1; PrintDate();
                        // loop to top of day again without advancing J
                        J--; // counteract impending J++ at top of while
                        continue;
                    }
                }

                // Take-off
                TakeOff(ref WA, ref AB);
                if (AB == 1)
                {
                    // scrub day
                    AB = 0; TG += 1; DY += 1; PrintDate();
                    J--; // retry this segment tomorrow
                    continue;
                }

                // In-flight
                bool madeProgress = InFlight(JA, ref FU, ref WA);
                // If madeProgress false, game ended.
                if (!madeProgress) return;

                // Arrived / landed processing handles PrintDate inside its flow
                Pause(); // short breather
            }
        }

        // ======= Intro / Scenario =======
        static void Intro()
        {
            Console.Clear();
            Center(10, "Around the World Flight of Amelia Earhart, 1937");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(new string(' ', 29) + "(c) David H. Ahl, 1986");
            Console.SetCursorPosition(27, 23);
            PressAny("Press any key to continue.");
            PrintScenario();
        }

        static void PrintScenario()
        {
            Console.Clear();
            Center(1, "Around the World Flight Attempt");
            Console.WriteLine();
            Console.WriteLine(" In this simulation, you take the role of Amelia Earhart in her");
            Console.WriteLine(" attempt to fly around the world in a twin-engine Lockheed Electra.");
            Console.WriteLine(" Prior to each flight leg, you are given information about your");
            Console.WriteLine("  physical condition and that of your navigator, the distance to your");
            Console.WriteLine(" next destination, and the current weather. As pilot, you must make");
            Console.WriteLine(" many decisions before taking off, while aloft, and prior to landing.");
            Console.WriteLine(" Under ideal conditions, at 150 mph, your plane can fly 2.3");
            Console.WriteLine(" miles on one gallon of fuel, but conditions are seldom ideal.");
            Console.WriteLine(" The Electra can hold up to 1150 gallons of fuel.");
            Console.WriteLine(" Your engine and mechanical components will last longer if they");
            Console.WriteLine(" are maintained regularly; on the Electra, the recommended interval");
            Console.WriteLine(" for a major overhaul is 40 hours. But remember, not all airports");
            Console.WriteLine(" are equipped to service your aircraft.");
            Console.WriteLine(" If you have malfunctions along the way, you may want to have");
            Console.WriteLine(" them fixed at a secondary aerodrome. Of course, this costs time.");
            Console.WriteLine(" Your navigator has a serious alcohol problem. As long as your");
            Console.WriteLine(" ground time is minimal, he won't have much chance to get lost in");
            Console.WriteLine(" the bottle, but if you get trapped on the ground by a series of");
            Console.WriteLine(" tropical storms and he gets drunk, you may find you have to rely");
            Console.WriteLine(" on dead reckoning and landmarks when you get back in the air.");
            Console.WriteLine();
        }

        // ======= Subroutines / Helpers =======

        static void PrintDate()
        {
            // Day index -> month/day
            // BASIC mapping:
            // May 20 is “day 1”; if DY < 12 -> May, DA = DY + 20
            // else if DY > 56 -> July failure
            // else if DY > 41 -> July, DA = DY - 41
            // else June, DA = DY - 11
            if (DY < 12)
            {
                MO = "May";
                DA = DY + 20;
            }
            else if (DY > 56)
            {
                Console.WriteLine();
                Console.WriteLine("It's July 16 and your money has completely run out.");
                Console.WriteLine("Sorry, you were unsuccessful. Perhaps you and George Putnam");
                Console.WriteLine("can raise enough money for a try again next year.");
                EndSummary(failed: true);
            }
            else if (DY > 41)
            {
                MO = "July";
                DA = DY - 41;
            }
            else
            {
                MO = "June";
                DA = DY - 11;
            }

            Console.WriteLine();
            Console.WriteLine($"Date: {MO}{(DA < 10 ? " " : " ")}{DA}, 1937");
        }

        static void AircraftRepairs()
        {
            // TQ set to 0 unless repair chosen; M==0 means no malfunction
            TQ = 0;
            if (M == 0) return;

            Console.WriteLine($"Your {MText[M]} has been giving you problems.");
            if (!AskYesNo("Do you want to have it repaired here (Y or N)")) return;

            M = 0;
            int hours = 2 * F + 1;
            Console.Write($"That will take {hours} hours");
            // if long runway next OR good facilities, day lost
            if (((DX[J + 1] > 600 && F > 1) || F > 3))
            {
                Console.WriteLine(" and will prevent you from leaving today.");
                TQ = 1;
            }
            else
            {
                Console.WriteLine(".");
            }
        }

        static void MajorOverhaul()
        {
            TP = 0;
            if (TM < 28)
            {
                // Check convenience stops if > 12 hours when at certain Js
                if ((J == 5 || J == 9 || J == 19 || J == 25) && TM > 12)
                {
                    SuggestMajorOverhaul();
                }
                return;
            }
            else if (TM < 39)
            {
                Console.Write("You should probably have a major overhaul");
                Console.WriteLine(" sometime soon.");
                if (F > 2) return;
                SuggestMajorOverhaul();
                return;
            }
            else
            {
                Console.Write("You should probably have a major overhaul");
                Console.WriteLine(" as soon as possible.");
                if (F > 2) return;
                SuggestMajorOverhaul();
                return;
            }

            void SuggestMajorOverhaul()
            {
                if (!AskYesNo("Do you want a major overhaul here (Y or N)")) return;

                // 30% chance extra day (TP = F+1), else TP = F
                TP = (Rng.NextDouble() > 0.7) ? (F + 1) : F;
                DM = 0;
                TM = 0;
                Console.WriteLine($"That will take {TP} day(s).");
            }
        }

        static void PilotCondition()
        {
            double x = 10 * Rng.NextDouble();
            Console.WriteLine();
            Console.Write("You are feeling ");
            if (x < 5) Console.WriteLine("as if you could use some more sleep.");
            else if (x < 8) Console.WriteLine("pretty good, all things considered.");
            else Console.WriteLine("fit as a fiddle and ready to go.");
        }

        static void NavigatorCondition()
        {
            ND = 0;
            NC = .002 * DC + 15 * TG;
            Console.Write("Your navigator is ");
            if (NC > 80) { Console.WriteLine("drunk and barely able to walk.");  AskHold(); }
            else if (NC > 50) { Console.WriteLine("droopy and has a bad hangover."); AskHold(); }
            else if (NC > 25) { Console.WriteLine("a bit under the weather from drinking last night."); }
            else Console.WriteLine("well rested and ready to go.");

            void AskHold()
            {
                Console.WriteLine("Do you want to wait until tomorrow and hope he will be in");
                if (!AskYesNo(" better shape (Y or N)")) { ND = 1; }
            }
        }

        static void NextDestination()
        {
            JA = (J == 10 || J == 21) ? J + 2 : J + 1;
            Console.WriteLine($"Your next destination is {LA[JA]}, {LB[JA]}, {DX[JA]} miles away.");
        }

        static void TakeOff(ref int WA, ref int abortFlag)
        {
            Console.WriteLine("Revving up engines…everything seems okay…rolling…");
            Console.Write("…picking up speed and…"); Pause();

            double x = Rng.NextDouble();
            // 2% chance of catastrophic problem on the roll
            if (x > .99)
            {
                Console.WriteLine("the landing gear strut broke!");
                CrashOnRunway();
                return;
            }
            if (x > .98)
            {
                Console.WriteLine("engines aren't synchronized…plane is turning!");
                Pause(); Beep(3); Console.WriteLine();
                CrashOnRunway();
                return;
            }

            // muddy/short/wet?
            int y = CC[J] + RR[J] + WA;
            if ((y > 9 && x > .85) || (y > 8 && x > .6))
            {
                Console.WriteLine("the wheels just won't lift out of the mud!");
                Console.WriteLine("Reluctantly you concede there is no chance of a takeoff today.");
                abortFlag = 1;
                return;
            }

            Console.WriteLine("you're finally off!");
            Console.WriteLine();

            void CrashOnRunway()
            {
                Console.WriteLine("Disaster! The Electra is lying helpless on the runway with");
                Console.WriteLine("a broken wing, smashed engine, and structural damage just");
                Console.WriteLine("as in the ill-fated March 20 takeoff from Honolulu. So sorry.");
                EndSummary(failed: true);
            }
        }

        static bool InFlight(int nextIndex, ref double fuelGallons, ref int WA)
        {
            // Ask for speed
            double S = AskDouble("At what speed do you wish to fly", 120, 170,
                "Minimum cruising speed is 120 mph; maximum is 170 mph.");

            // Weather aloft (perturb again)
            WA = Math.Clamp(WA + (int)Math.Round(1.6 * Rng.NextDouble() - 0.3), 1, 6);
            Console.WriteLine($"Current weather aloft is : {WText[WA]}");

            int SW = 0;
            if (J == 6 || J == 7) { SW = 30; Console.WriteLine("Strong 30+ mph headwind."); }
            if (J == 10) { SW = 15; Console.WriteLine("Mixed weather…doldrums…headwinds."); }
            if (J >= 20 && J <= 22)
            {
                SW = 20;
                Console.Write("The plane is being buffetted about, ");
                if (Rng.NextDouble() <= .4)
                {
                    Console.WriteLine("and you'll have to turn back.");
                    TurnBack(); return true;
                }
                Console.WriteLine("but you decide to push on.");
            }

            double SA = S - SW;               // actual speed
            int DJ = DX[nextIndex];           // distance this leg
            double TE = DJ / Math.Max(1.0, SA);     // expected time

            DC += DJ;  TC += TE; DM += DJ; TM += TE;

            // Major-engine failure?
            double PC = DM * (double)DM / (900000.0 * Math.Max(0.1, TM)); // plane condition
            double MP = Math.Atan(14 * PC - 17) / Math.PI + 0.5;
            if (Rng.NextDouble() <= MP)
            {
                M = 11; Warn();
                Console.WriteLine("Right engine gauges are going crazy…major engine failure!");
                if (AskYesNo("Want to try to limp along on one engine (Y or N)"))
                {
                    // try to limp
                    double x = Rng.NextDouble();
                    if (x < .333) { Console.WriteLine($"No chance of making {LA[nextIndex]}. You'll have to turn back."); TurnBack(); return true; }
                    if (x > .667) { ForcedLanding(); return false; }
                    Pause(); Console.WriteLine("Whew! It looks as if you can nurse it along.");
                    // Skip minor/fuel checks and go land sequence
                    return LandSequence(nextIndex);
                }
                else
                {
                    // Declined limping; roll for outcome
                    double x = Rng.NextDouble();
                    if (x < .333) { Console.WriteLine($"No chance of making {LA[nextIndex]}. You'll have to turn back."); TurnBack(); return true; }
                    if (x > .667) { ForcedLanding(); return false; }
                    Pause(); Console.WriteLine("Whew! It looks as if you can nurse it along.");
                    return LandSequence(nextIndex);
                }
            }

            // Minor malfunction 30% chance
            if (Rng.NextDouble() <= .3)
            {
                int prev = (M > 0 ? M : 0);
                M = 1 + (int)(10 * Rng.NextDouble());
                Warn();
                Console.WriteLine();
                Console.WriteLine($"Malfunction in the {MText[M]}");
                if (prev != 0)
                {
                    Console.WriteLine($"This combined with the previous malfunction of the {MText[prev]} will");
                    Console.WriteLine("create very serious problems for you.");
                    if (!AfterMalfunctionChoice(DJ, out var keepGoing))
                    {
                        ForcedLanding(); return false;
                    }
                    if (!keepGoing) { TurnBack(); return true; }
                    // else continue / fall through
                }
                else
                {
                    int MDLocal = (int)(DJ * Rng.NextDouble()); // miles already flown in leg
                    Console.WriteLine($"You have flown {MDLocal} miles of this flight leg. Do you want to");
                    bool push = AskYesNo("push on (Y or N)");
                    int ME = push ? DJ - MDLocal : MDLocal;
                    if (Rng.NextDouble() < .05 * ME / DJ)
                    {
                        Warn();
                        Console.WriteLine("Uh oh. Fuel-feed system has malfunctioned also.");
                        Console.WriteLine("Things look pretty bad.");
                        Pause();
                        ForcedLanding(); return false;
                    }
                    if (!push)
                    {
                        TurnBack(); return true;
                    }
                }
            }

            // Fuel consumption check
            // Baseline: TF = FU * (5.6 / S - .02), if TF*.8 > TE it's enough fuel
            double TF = fuelGallons * (5.6 / S - .02);
            if (TF * .8 <= TE)
            {
                if (S < 121) Console.WriteLine("Fuel consumption seems very high…");
                else
                {
                    Console.WriteLine("You're going to be tight on fuel. Perhaps you should throttle");
                    double SQ = AskDouble("back. What speed would you like", 120, 300);
                    if ((J > 28 && fuelGallons > 1100) || (S - SQ >= 9))
                    {
                        TF = fuelGallons * (5.6 / SQ - .02);
                        if (TF * .96 <= TE)
                        {
                            Console.WriteLine("Uh oh…the right engine is sputtering…");
                            Pause();
                            Console.WriteLine("And now the left engine too. You're out of fuel.");
                            ForcedLanding(); return false;
                        }
                    }
                    else
                    {
                        if (TF * .96 <= TE)
                        {
                            Console.WriteLine("Uh oh…the right engine is sputtering…");
                            Pause();
                            Console.WriteLine("And now the left engine too. You're out of fuel.");
                            ForcedLanding(); return false;
                        }
                    }
                }
            }

            // Navigation
            if (NC >= 51)
            {
                Console.WriteLine("Your navigator isn't going to be of much use to you today.");
                Console.WriteLine("You'll have to rely upon dead reckoning and landmarks.");
                if (M == 5 || M == 7) Console.WriteLine($"Moreover, your {MText[M]} is on the fritz.");
            }

            double TR = Math.Max(1.2, (int)TE);
            Console.WriteLine();
            Console.WriteLine($"You have been flying for over {TR} hours but there is");
            Console.Write("no sign of ");

            // Special situations and land sequence
            return SpecialSituations(nextIndex);
        }

        static bool SpecialSituations(int nextIndex)
        {
            // Atlantic crossing (J=10): Paris->Natal (Brazil)
            if (J == 10)
            {
                Console.Write("land. Pushing onwards. ");
                Pause();
                Console.WriteLine("Wow! Land! Look!");
                Console.Write("Approaching coast of Africa; ahead of you is ");
                if (Rng.NextDouble() > .95)
                {
                    Console.WriteLine("Dakar! Nice flying!");
                    J = 11;
                    return LandSequence(11);
                }
                Console.WriteLine("nothing but jungle. Turning north.");
                Pause();
                Console.WriteLine("A half hour later you sight St. Louis, Senegal, and decide to land.");
                JA = 11; DX[12] = 163; // St. Louis to Dakar distance
                return LandSequence(11);
            }

            // Monsoons out of Akyab (J=21)
            if (J == 21)
            {
                Console.WriteLine("anything except the deluge of water. You'll have to put down at");
                Console.WriteLine("Rangoon…if you can find it.");
                Pause();
                Console.WriteLine("Look! There!");
                JA = 22;
                return LandSequence(22);
            }

            // Java instruments problem (J=25 => forced back to Bandoeng 24)
            if (J == 25)
            {
                if (MJ == 1) return LandSequence(JA);
                Console.WriteLine("civilization. Moreover, several of your instruments");
                Console.WriteLine("are behaving quite badly. Reluctantly, you turn back to");
                Console.WriteLine("Bandoeng because you know that facilities at Saurabaya are minimal.");
                DC -= 300; DM -= 300; TC -= 2; TM -= 2;
                J = 24; JA = 24; MJ = 1;
                return LandSequence(24);
            }

            // Lae to Howland (J=29)
            if (J == 29)
            {
                Console.WriteLine("land. You spotted the arc lights at Nauru 8 hours ago.");
                Console.Write("Calling Coast Guard cutter Itasca…"); Radio(); Pause();
                if (RF == 1) Console.WriteLine("Nothing.");
                else { RF = 1; Console.WriteLine("Nothing."); Console.WriteLine("Switch radio frequency…try again…"); Console.Write("Calling Coast Guard cutter Itasca…"); Radio(); Pause(); Console.WriteLine("Still nothing."); }
                Console.WriteLine("You're very low on fuel!");
                Console.WriteLine("You can search for Howland or turn back to the Gilbert Islands.");
                if (!AskYesNo("Want to search (Y or N)"))
                {
                    for (int k = 0; k < 4; k++) { Console.WriteLine("Searching…"); Pause(); }
                    Pause();
                    double rn = (NC < 30 ? .3 : .02);
                    if (Rng.NextDouble() > rn)
                    {
                        ForcedLanding(); return false;
                    }
                    Console.WriteLine("My gosh! There it is! A tiny speck of land. WOW!");
                    return LandSequence(JA);
                }
                else
                {
                    Console.WriteLine("Tuvalu, the only island in the Gilberts with a landing strip,");
                    Console.WriteLine("is almost 4 hours distant on a course almost due west.");
                    for (int k = 0; k < 4; k++) { Console.WriteLine("Flying…"); Pause(); }
                    Pause();
                    Console.WriteLine();
                    Console.WriteLine("Look! Coral reefs. A small island.");
                    Pause();
                    Console.WriteLine("Virtually no fuel left…both engines sputtering…try to put");
                    Console.WriteLine("it down in that flat area along the beach.");
                    Pause();
                    Console.WriteLine("You made it down…a wing tore off the plane…navigator injured.");
                    Pause(); Console.WriteLine("Men in uniform are coming over the sand dunes.");
                    Pause(); Pause(); Console.WriteLine();
                    if (Rng.NextDouble() > .985)
                    {
                        // British
                        Console.WriteLine("They're British. You're safe. In three days the USS Itasca");
                        Console.WriteLine("picks you up and deposits you in Honolulu a week later.");
                        EndSummary(failed: true);
                        return false;
                    }
                    else
                    {
                        // Japanese capture
                        Console.WriteLine("They're Japanese. An English-speaking native tells you that");
                        Console.WriteLine("this is Mili Atoll in the Marshall Islands. You are put on a");
                        Console.WriteLine("warship bound for Majuro. Days later you are put on another");
                        Console.WriteLine("Japanese warship bound for Saipan. The Japanese accuse you");
                        Console.WriteLine("of being a spy, torture you, and put you in a tiny prison cell.");
                        Pause(); Console.WriteLine(); Pause();
                        Console.WriteLine("After months in a tiny, damp prison cell you contract dysentery.");
                        Console.WriteLine("Your navigator is executed and in August 1938 you die of disease");
                        Console.WriteLine("and thus become the first U.S. casualities of World War II.");
                        EndSummary(failed: true);
                        return false;
                    }
                }
            }

            // Howland to Honolulu (J=30)
            if (J == 30)
            {
                Console.WriteLine("the Hawaiian Islands. But you're buoyed by the thought");
                Console.WriteLine("that you found Howland Island in the middle of the Pacific.");
                for (int k = 0; k < 4; k++) { Console.Write("Flying…"); Pause(); }
                Pause();
                Console.WriteLine();
                Console.Write("Calling Honolulu…come in please "); Radio(); Pause();
                Console.WriteLine("Honolulu to Electra: You're right on course. Weather is");
                Console.WriteLine("excellent. You should sight Diamond Head very soon.");
                Pause();
                Console.WriteLine();
                Console.WriteLine("Yes…there it is. What a welcome sight!");
                return LandSequence(JA);
            }

            // Honolulu to Oakland (J=31)
            if (J == 31)
            {
                Console.WriteLine("the mainland. But you're confident you'll make it.");
                Pause();
                Console.WriteLine();
                Console.WriteLine("You've been flying nearly 20 hours and you're");
                Console.WriteLine("bone tired. You wish your navigator could relieve you.");
                Pause();
                Radio(); Console.WriteLine("Oakland calling Electra…Oakland calling Electra…");
                bool ok = AskYesNo("Are you okay…please respond…are you okay");
                Console.WriteLine(ok ? "Oakland : Glad to hear it." : "Oakland: Sorry to hear that. Keep going. Just a short way now.");
                Pause(); Console.WriteLine(" Oh yes, G.P. sends greetings."); Pause(); Console.WriteLine();
                Console.WriteLine("And there it is; the Pacific coast and the Golden Gate Bridge.");
                Console.WriteLine("What a beautiful sight! Coming into Oakland…steady…steady.");
                Console.WriteLine("Touchdown…slowing down…HUGE crowds all around…stopping.");
                for (int k = 0; k < 4; k++) Pause();
                Console.Clear();
                int blink = 0;
                for (int i = 0; i < 30; i++)
                {
                    for (int k = 0; k < 100; k++) { /* tiny loop */ }
                    Console.SetCursorPosition(30, 10);
                    Console.Write(blink == 0 ? "CONGRATULATIONS !" : "               ");
                    Beep();
                    blink = 1 - blink;
                }
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                EndSummary(failed: false);
                return false;
            }

            // Normal arrival path
            return LandSequence(nextIndex);
        }

        static bool LandSequence(int targetIndex)
        {
            // Standard landing sequence with tower & conditions
            Console.WriteLine($"{LA[targetIndex]}. Flying on…");
            Pause();
            Console.WriteLine("Look to the right!");
            Console.WriteLine("It looks like…yes it is…an aerodrome. What a welcome sight.");
            Console.WriteLine($"Field looks {RText[RR[targetIndex]]} for the plane.");

            if (M == 7)
            {
                Console.WriteLine("Radio broken. You'll have to try to land");
                Console.WriteLine("without establishing contact.");
                Console.WriteLine();
                if (!AskYesNo("Can't establish contact. Do you want to land"))
                {
                    Console.WriteLine("Circling…circling…trying radio again.");
                    // fall back to tower loop
                }
                else
                {
                    TouchdownOrCrash(targetIndex);
                    return true;
                }
            }

            Console.Write("Electra calling control tower…"); Radio();
            if (Rng.NextDouble() < .1)
            {
                if (!AskYesNo("Can't establish contact. Do you want to land"))
                {
                    Console.WriteLine("Circling…circling…trying radio again.");
                }
                else
                {
                    TouchdownOrCrash(targetIndex);
                    return true;
                }
            }

            Console.WriteLine("Tower to Electra…tower to Electra…");
            if (WX[targetIndex] <= 3)
            {
                if (AskYesNo("Condition of field is fine. Do you want clearance to land"))
                {
                    TouchdownOrCrash(targetIndex); return true;
                }
                else
                {
                    Console.Write("Repeat: ");
                    // loop back into tower; but we’ll just permit land anyway
                    TouchdownOrCrash(targetIndex); return true;
                }
            }
            else
            {
                if (AskYesNo("Field is a bit soggy. Do you want clearance to land"))
                {
                    TouchdownOrCrash(targetIndex); return true;
                }
                else
                {
                    if (AskYesNo("Do you want to turn back"))
                    {
                        TurnBack(); return true;
                    }
                    else
                    {
                        Console.Write("Repeat : ");
                        TouchdownOrCrash(targetIndex); return true;
                    }
                }
            }
        }

        static void TouchdownOrCrash(int targetIndex)
        {
            Console.Write("Coming in…steady…steady…"); Pause(); Console.WriteLine("touchdown.");
            if (CC[targetIndex] + RR[targetIndex] + WX[targetIndex] > 9 && Rng.NextDouble() < .15)
            {
                Console.WriteLine("Field is soggy…one wheel caught in mud…plane is tipping.");
                // Crash like takeoff catastrophic
                Console.WriteLine("Disaster! The Electra is lying helpless on the field.");
                EndSummary(failed: true);
                return;
            }

            Console.WriteLine("Slowing down…turning…bring it to a stop…shut down engines.");
            int k = (J == 11 || J == 2) ? 6 : 1 + (int)(4.9 * Rng.NextDouble());
            Console.WriteLine($"A {CrowdText[k]} crowd is waiting for you. Nice job.");

            // Darwin delay
            if (J == 28)
            {
                Console.WriteLine("Australian authorities claim that your medical papers are");
                Console.WriteLine("not in order and hold you on the plane for 10 hours. That");
                Console.WriteLine("costs you an extra day.");
                DY += 1; TG += 1;
            }

            // advance to next leg
            J = targetIndex;
        }

        static void TurnBack()
        {
            AB = 1;
            J = J - 1;
            JA = J;
        }

        static void ForcedLanding()
        {
            // Over water?
            if (J == 4 || J == 10 || J == 27 || J > 28)
            {
                Console.WriteLine("Going down…nothing but water..looking for a reef or anything…");
                if (Rng.NextDouble() <= .2)
                {
                    Pause();
                    Console.WriteLine(" C R A S H ! No survivors.");
                    EndSummary(failed: true);
                    return;
                }
                Console.Write("maybe that small clearing…"); Pause(); Console.WriteLine("you made it!");
                Console.WriteLine("The plane is a wreck but at least you're alive.");
                EndSummary(failed: true);
                return;
            }
            else
            {
                Console.WriteLine("Going down…looking for a suitable place to land…nothing…");
                if (Rng.NextDouble() <= .2)
                {
                    Pause();
                    Console.WriteLine(" C R A S H ! No survivors.");
                    EndSummary(failed: true);
                    return;
                }
                Console.Write("maybe that small clearing…"); Pause(); Console.WriteLine("you made it!");
                Console.WriteLine("The plane is a wreck but at least you're alive.");
                EndSummary(failed: true);
                return;
            }
        }

        static bool AfterMalfunctionChoice(int DJ, out bool pushOn)
        {
            // combined malfunction => decide try/turn/forced landing
            int MDLocal = (int)(DJ * Rng.NextDouble());
            Console.WriteLine($"You have flown {MDLocal} miles of this flight leg. Do you want to");
            pushOn = AskYesNo("push on (Y or N)");
            int ME = pushOn ? DJ - MDLocal : MDLocal;
            if (Rng.NextDouble() < .05 * ME / DJ)
            {
                Warn();
                Console.WriteLine("Uh oh. Fuel-feed system has malfunctioned also.");
                Console.WriteLine("Things look pretty bad.");
                Pause();
                ForcedLanding();
                return false;
            }
            return true;
        }

        static void EndSummary(bool failed)
        {
            Pause(); Console.WriteLine();
            Console.WriteLine();
            if (failed)
            {
                Console.WriteLine("Sorry your flight was unsuccessful.");
            }
            Console.WriteLine();
            Console.Write($"You flew {DC} miles and were aloft for ");
            int hours = (int)TC;
            int mins = (int)(60 * (TC - hours));
            Console.WriteLine($"{hours} hours and {mins} minutes.");
            Console.WriteLine($"Your flight started on May 20 and ended on {MO} {DA}, 1937.");
            Console.WriteLine();
            Console.WriteLine("Amelia Earhart flew approximately 27,000 miles between");
            Console.WriteLine("May 20 and July 2, 1937 before going down at Mili Atoll");
            Console.WriteLine("in the Japanese-held Marshall Islands.");
            Pause(); Console.WriteLine();

            bool again = AskYesNo("Would you like to try again (Y or N)");
            if (again)
            {
                Console.WriteLine("Okay. Good luck!");
                Pause();
                // reset all state and restart
                ResetAll();
                Console.Clear();
                Main();
            }
            else
            {
                Console.WriteLine("Okay. So long for now.");
                Pause();
                Environment.Exit(0);
            }
        }

        static void SafeBeep(int freq = 800, int durMs = 60)
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    Console.Beep(freq, durMs);
                }
                else
                {
                    // Fallback: ASCII bell; harmless on Windows, works on many terminals
                    Console.Write("\a");
                }
            }
            catch
            {
                // Ignore if console can't beep
            }
        }

        static void ResetAll()
        {
            J = 0; JA = 0; DY = 0; TG = 0;
            F = 0; W = 0; D = 0;
            TM = 0; TC = 0; DM = 0; DC = 0;
            ND = 0; NC = 0;
            M = 0; TP = 0; TQ = 0; AB = 0; MJ = 0; RF = 0;
            MO = "May"; DA = 20;
        }

        // ======= Utilities =======
        static void Pause(int ms = 300) => Thread.Sleep(ms);

        static void AlarmClock()
        {
            Console.WriteLine();
            for (int i = 0; i < 7; i++) SafeBeep();
            Console.WriteLine($"There goes the alarm. It's { (int)(3 + 3.7 * Rng.NextDouble()) } a.m. Y - A - W - N");
            Thread.Sleep(250);
            for (int i = 0; i < 7; i++) SafeBeep();
        }

        static void Beep(int times = 1)
        {
            for (int i = 0; i < times; i++) SafeBeep(800, 60);
        }

        static void Warn()
        {
            Console.WriteLine();
            for (int i = 0; i < 3; i++)
            {
                SafeBeep(800, 60);
                SafeBeep(800, 60);
                Pause(100);
            }
        }

        static void Radio()
        {
            for (int i = 0; i < 4; i++)
            {
                int x = 1 + (int)(3 * Rng.NextDouble());
                for (int k = 0; k < x; k++) SafeBeep(1200, 40);
                Pause(60);
            }
        }


        static void Center(int row, string text)
        {
            if (row >= 0) Console.SetCursorPosition(0, row);
            int col = Math.Max(0, (Console.WindowWidth - text.Length) / 2);
            Console.SetCursorPosition(col, row);
            Console.WriteLine(text);
        }

        static void PressAny(string prompt)
        {
            Console.Write(prompt);
            Console.ReadKey(true);
            Console.WriteLine();
        }

        static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} ");
                string? s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s)) return true; // default yes like BASIC sometimes did
                s = s.Trim();
                if (s.Equals("Y", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("Yes", StringComparison.OrdinalIgnoreCase)) return true;
                if (s.Equals("N", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("No", StringComparison.OrdinalIgnoreCase)) return false;
                Console.WriteLine($"Don't understand your answer of {s}.");
            }
        }

        static double AskDouble(string prompt, double min, double max, string? rangeHint = null)
        {
            while (true)
            {
                Console.Write($"{prompt} ");
                string? s = Console.ReadLine();
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                {
                    if (v < min || v > max)
                    {
                        if (!string.IsNullOrEmpty(rangeHint))
                            Console.WriteLine(rangeHint);
                        continue;
                    }
                    return v;
                }
                Console.WriteLine("Please enter a number.");
            }
        }

        // ======= Data Init =======
        static void InitText()
        {
            // F$ (1..4)
            FText[1] = "excellent";
            FText[2] = "good";
            FText[3] = "fair";
            FText[4] = "poor";

            // C$ (1..3)
            CText[1] = "concrete";
            CText[2] = "packed gravel";
            CText[3] = "grass and dirt";

            // R$ (1..3)
            RText[1] = "plenty long";
            RText[2] = "of adequate length";
            RText[3] = "barely long enough";

            // W$ (1..6)
            WText[1] = "clear";
            WText[2] = "scattered clouds";
            WText[3] = "overcast";
            WText[4] = "light rain";
            WText[5] = "wind and heavy rain";
            WText[6] = "high wind and monsoon rains";

            // M$ (1..11)
            MText[1] = "thermocouple";
            MText[2] = "turn & bank indicator";
            MText[3] = "fuel gauge";
            MText[4] = "altimeter";
            MText[5] = "Bendix radio direction finder";
            MText[6] = "Sperry Gyro Pilot";
            MText[7] = "radio";
            MText[8] = "mixture control lever";
            MText[9] = "hydraulic system";
            MText[10] = "electrical system";
            MText[11] = "engine";

            // Z$ crowd (1..6)
            CrowdText[1] = "small";
            CrowdText[2] = "large";
            CrowdText[3] = "noisy";
            CrowdText[4] = "clamorous";
            CrowdText[5] = "restless";
            CrowdText[6] = "tiny";
        }

        static void InitAirports()
        {
            // Indices 1..32
            Set(1,  "Oakland",        "California",             1, 1, 1, 1,    0);
            Set(2,  "Burbank",        "California",             2, 1, 1, 1,  332);
            Set(3,  "Tucson",         "Arizona",                2, 1, 1, 1,  456);
            Set(4,  "New Orleans",    "Louisiana",              1, 1, 1, 1, 1287);
            Set(5,  "Miami",          "Florida",                1, 1, 1, 1,  688);
            Set(6,  "San Juan",       "Puerto Rico",            2, 2, 2, 1, 1053);
            Set(7,  "Caripito",       "Colombia",               1, 1, 1, 3,  624);
            Set(8,  "Paramaribo",     "Dutch Guiana",           3, 3, 1, 2,  610);
            Set(9,  "Fortaleza",      "Brazil",                 1, 3, 2, 5, 1332);
            Set(10, "Natal",          "Brazil",                 3, 3, 2, 4,  270);
            Set(11, "St. Louis",      "Senegal",                4, 3, 2, 2, 1992);
            Set(12, "Dakar",          "French West Africa",     2, 2, 2, 1, 1974);
            Set(13, "Gao",            "French Sudan",           3, 3, 2, 2, 1150);
            Set(14, "Fort Lamy",      "Chad",                   4, 3, 3, 1, 1027);
            Set(15, "El Fasher",      "Fr. Equatorial Africa",  4, 3, 3, 1,  679);
            Set(16, "Khartoum",       "Anglo Egyptian Sudan",   4, 2, 3, 1,  494);
            Set(17, "Massawa",        "Abyssinia",              3, 3, 2, 1,  442);
            Set(18, "Assab",          "Eritrea",                3, 2, 2, 1,  340);
            Set(19, "Karachi",        "India",                  1, 1, 1, 3, 1920);
            Set(20, "Calcutta",       "India",                  2, 2, 2, 6, 1390);
            Set(21, "Akyab",          "Burma",                  3, 2, 2, 6,  338);
            Set(22, "Rangoon",        "Burma",                  3, 2, 2, 6,  330);
            Set(23, "Bangkok",        "Siam",                   2, 2, 2, 5,  365);
            Set(24, "Singapore",      "Asia",                   2, 2, 2, 2,  895);
            Set(25, "Bandoeng",       "Java",                   1, 3, 1, 2,  635);
            Set(26, "Saurabaya",      "Java",                   4, 3, 3, 2,  365);
            Set(27, "Koepang",        "Timor",                  4, 3, 3, 2, 1148);
            Set(28, "Port Darwin",    "Australia",              2, 1, 1, 1,  517);
            Set(29, "Lae",            "New Guinea",             3, 3, 2, 2, 1196);
            Set(30, "Howland Island", "Pacific",                4, 2, 2, 2, 2556);
            Set(31, "Honolulu",       "Hawaii",                 1, 1, 1, 1, 1818);
            Set(32, "Oakland",        "California",             1, 1, 1, 1, 2420);

            void Set(int i, string a, string b, int fx, int c, int r, int w, int dx)
            {
                LA[i] = a; LB[i] = b; FX[i] = fx; CC[i] = c; RR[i] = r; WX[i] = w; DX[i] = dx;
            }
        }
    }
    
}
