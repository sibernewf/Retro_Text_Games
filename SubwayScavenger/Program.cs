
using System;
using System.Collections.Generic;

namespace SubwayScavenger
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            new Game().Run();
        }
    }

    class Package
    {
        public int Id;
        public string Destination;
        public int StationCount;
        public int[] StationIds;
        public int[] StationBlocks;
        public Package(int id, string dest, int count, int[] ids, int[] blocks)
        { Id = id; Destination = dest; StationCount = count; StationIds = ids; StationBlocks = blocks; }
    }

    class Station
    {
        public int Id;
        public string Name;
        public int TrainCount;
        public int[] TrainIds;
        public Station(int id, string name, int count, int[] trains)
        { Id = id; Name = name; TrainCount = count; TrainIds = trains; }
    }

    class Train
    {
        public int Id;
        public string Name;
        public int StopCount;
        public int[] StationIds;
        public string Terminus1 = "";
        public string Terminus2 = "";
        public Train(int id, string name, int stopCount, int[] stationIds)
        { Id = id; Name = name; StopCount = stopCount; StationIds = stationIds; }
    }

    class Game
    {
        // RNG and time/state
        readonly Random _rng = new Random();
        int MIN = 0;
        int TOKEN = 0;
        int TKMAX = 20;
        int TK = 0;
        int LUN = 0;
        int PERS = 0; // 0=on foot, 1=on train
        int DELTOT = 0;
        int STA = 21; // start at 42 St/8 Av
        int TR = 0;
        int TRSTX = 0;
        int TRX = 0;
        int DES = 0;
        int TM = 480; // 5pm
        string tmLabel = "5:00";

        // Data
        List<Package> Packages = new List<Package>();
        List<Station> Stations = new List<Station>();
        List<Train> Trains = new List<Train>();

        // Logbook status: 0=done, 1=delivery, 2=pickup, 3=unused
        int[] DORP = new int[21]; // 1..20
        int LGMAX = 10;

        public void Run()
        {
            Splash();
            InitData();
            ShuffleInitialPackages();

            Console.WriteLine();
            CenterLine("(Initializing data - please be patient)");
            CenterLine("Press any key to continue.");
            WaitKey();
            Console.WriteLine();

            Console.Write("Do you want to be able to deliver after 5:00 pm (easier) (Y/N): ");
            string ans = ReadYesNo();
            if (ans == "Y") { TM = 540; tmLabel = "6:00"; }

            int i;
            for (i = 1; i <= 5; i++) DORP[i] = 1;
            for (i = 6; i <= 10; i++) DORP[i] = 2;

            Console.Clear();
            Console.WriteLine("You may want to print or copy this screen for later reference.");
            Console.WriteLine();
            PrintLog();

            for (;;)
            {
                ArriveAtStation();
            }
        }

        void Splash()
        {
            Console.Clear();
            CenterAt(10, "Subway Scavenger");
            CenterAt(13, "(c) by David H. Ahl, 1986");
            Console.SetCursorPosition(0, 23);
            CenterLine("Press any key to continue.");
            WaitKey();
            Instructions();
        }

        void Instructions()
        {
            Console.Clear();
            CenterLine("Subway Scavenger");
            Console.WriteLine();
            Console.WriteLine(" You have a job with a messenger/courier service located in");
            Console.WriteLine("mid-town Manhattan. Today, you have five packages to deliver and");
            Console.WriteLine("five packages to pick up for delivery to other locations in the");
            Console.WriteLine("city. So, in total you must visit 15 different locations.");
            Console.WriteLine();
            Console.WriteLine(" You can use 264 stations of the New York Subway System which");
            Console.WriteLine("are serviced by the following 11 trains: A, B, CG, D, E, F, N, 1,");
            Console.WriteLine("2, 4, and 7.");
            Console.WriteLine();
            Console.WriteLine(" You must complete all your deliveries and pickups by 5:00 pm.");
            Console.WriteLine("Your boss has given you $20 for tokens (which will allow for a few");
            Console.WriteLine("wrong trains). Any money that you don't use on tokens is yours to");
            Console.WriteLine("keep. Good luck! (You'll need it.)");
        }

        void ArriveAtStation()
        {
            CheckForLunchOrClosing();
            Station st = Stations[STA - 1];
            Console.WriteLine("You have arrived at " + st.Name + " station.");
            Console.WriteLine("Trains that stop at this station:");
            for (int i = 0; i < st.TrainCount; i++)
            {
                Train trn = Trains[st.TrainIds[i] - 1];
                Console.WriteLine("  " + trn.Name);
            }

            if (PERS == 0)
            {
                BuyToken();
                TrainsComing();
                return;
            }
            else
            {
                Train cur = Trains[TR - 1];
                if (STA == cur.StationIds[0] || STA == cur.StationIds[cur.StopCount - 1])
                {
                    Console.WriteLine("End of the line. You'll have to get off.");
                }
                else
                {
                    Console.Write("Do you want to get off (Y/N): ");
                    string a = ReadYesNo();
                    if (a == "N")
                    {
                        TrainsComing();
                        return;
                    }
                }
                PERS = 0;
            }

            Console.WriteLine("Do you want to:");
            Console.WriteLine("  Make a pickup (P)");
            Console.WriteLine("  Make a delivery (D)");
            Console.WriteLine("  Check your logbook (C)");
            Console.WriteLine("  Get another train (T)");

            for (;;)
            {
                Console.Write("Your choice please (P, D, C, or T): ");
                string c = ReadFirstLetter();
                if (c == "P" || c == "D")
                {
                    HandlePickupDelivery(c);
                    return;
                }
                if (c == "T")
                {
                    TrainsComing();
                    return;
                }
                if (c == "C")
                {
                    PrintLog();
                }
                else
                {
                    Console.WriteLine("Not a valid choice.");
                }
            }
        }

        void TrainsComing()
        {
            PrintTime();
            Station st = Stations[STA - 1];
            int idx = _rng.Next(0, st.TrainCount);
            TR = st.TrainIds[idx];
            Train tr = Trains[TR - 1];

            if (STA == tr.StationIds[0]) DES = 2;
            else if (STA == tr.StationIds[tr.StopCount - 1]) DES = 1;
            else DES = _rng.Next(0, 2) == 0 ? 1 : 2;

            Console.WriteLine("Here comes the " + tr.Name + " train to " + (DES == 1 ? tr.Terminus1 : tr.Terminus2));
            MIN += 1;
            Console.Write("Do you want to get on (Y/N): ");
            string a = ReadYesNo();
            if (a == "N") { TrainsComing(); return; }

            // find index
            TRSTX = 0;
            for (int i = 0; i < tr.StopCount; i++) if (tr.StationIds[i] == STA) { TRSTX = i; break; }
            TRX = (DES == 1) ? -1 : 1;

            RideTrain();
        }

        void RideTrain()
        {
            PERS = 1;
            Train tr = Trains[TR - 1];
            Console.WriteLine("You are on the " + tr.Name + " train to " + (DES == 1 ? tr.Terminus1 : tr.Terminus2));
            TripHazards();
            TRSTX += TRX;
            if (TRSTX < 0) TRSTX = 0;
            if (TRSTX > tr.StopCount - 1) TRSTX = tr.StopCount - 1;
            STA = tr.StationIds[TRSTX];
            MIN += 2 + (int)Math.Floor(1.3 * _rng.NextDouble());
            ArriveAtStation();
        }

        void HandlePickupDelivery(string kind)
        {
            Console.Write("Which " + (kind == "P" ? "pickup" : "delivery") + " do you want to make (by Logbook number): ");
            int a;
            if (!int.TryParse(Console.ReadLine(), out a) || a < 1 || a > 20)
            {
                Console.WriteLine("That number seems to be in error.");
                AskShowLog();
                return;
            }
            if (DORP[a] == 0 || DORP[a] == 3)
            {
                Console.WriteLine("That number seems to be in error.");
                AskShowLog();
                return;
            }

            Package pkg = Packages[a - 1];
            Console.WriteLine("That " + (kind == "P" ? "pickup" : "delivery") + " is at " + pkg.Destination);
            int whereIndex = -1;
            for (int i = 0; i < pkg.StationCount; i++)
            {
                if (pkg.StationIds[i] == STA) { whereIndex = i; break; }
            }
            if (whereIndex < 0)
            {
                Console.WriteLine("which is too far to walk from this station. Perhaps try something else.");
                return;
            }

            int blocks = pkg.StationBlocks[whereIndex];
            Console.WriteLine("which is " + blocks + " block" + (blocks == 1 ? "" : "s") + " from here. Off you go.");

            MIN += 2 * blocks + 6;
            DELTOT += 1;
            if (DORP[a] == 2)
            {
                LGMAX += 1;
                Console.WriteLine("You pick up a package and log it in as no. " + LGMAX);
                Console.WriteLine("The address on it is " + pkg.Destination);
                DORP[a] = 0;
                DORP[LGMAX] = 1;
                Packages[LGMAX - 1] = pkg;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("You find someone to sign for the package.");
                DORP[a] = 0;
            }

            if (DELTOT == 15) { EndGame(true); return; }

            Console.WriteLine();
            Console.WriteLine("From here you can walk to the following subway station(s): ");
            if (pkg.StationCount == 1)
            {
                Console.WriteLine("  " + Stations[pkg.StationIds[0] - 1].Name);
                ArriveAtStation();
                return;
            }
            for (int i = 0; i < pkg.StationCount; i++)
            {
                Console.WriteLine("  " + (i + 1) + " — " + Stations[pkg.StationIds[i] - 1].Name);
            }
            int choice = ReadIntInRange("Which station do you want to go to (by number)", 1, pkg.StationCount);
            STA = pkg.StationIds[choice - 1];
            MIN += 3 + pkg.StationBlocks[choice - 1];
            ArriveAtStation();
        }

        void AskShowLog()
        {
            Console.Write("Want to check your logbook (Y/N): ");
            if (ReadYesNo() == "Y") PrintLog();
        }

        void BuyToken()
        {
            TOKEN += 1;
            if (TOKEN <= TKMAX) return;
            Console.WriteLine();
            Console.WriteLine("You have spent the entire $20 your boss gave you on tokens.");
            if (TK == 1)
            {
                Console.WriteLine("Moreover, you have used up your own money as well.");
                EndGame(false);
                return;
            }
            TK = 1;
            Console.Write("Do you want to buy tokens with your own money (Y/N): ");
            if (ReadYesNo() == "N")
            {
                Console.WriteLine("Okay, that's it then.");
                EndGame(false);
                return;
            }
            double RN = Math.Floor((300 + 600 * _rng.NextDouble())) / 100.0;
            int more = (int)RN;
            Console.WriteLine("You have exactly $" + RN.ToString("0.00") + " so you can buy " + more + " more tokens.");
            TKMAX += more;
        }

        void TripHazards()
        {
            // 5% sticky door
            if (_rng.NextDouble() <= 0.05)
            {
                Console.WriteLine("One of the car doors refuses to close and the train can't move.");
                int rn = 1 + (int)Math.Floor(2.5 * _rng.NextDouble());
                MIN += rn;
                Console.WriteLine("You're stuck here for " + rn + " minute" + (rn == 1 ? "" : "s") + ".");
            }

            // 35% branch (mugging/fire)
            if (_rng.NextDouble() <= 0.35)
            {
                // 5% tough characters
                if (_rng.NextDouble() <= 0.05)
                {
                    Console.WriteLine("Some real unsavory types are whooping it up in the car across from");
                    Console.Write("your seat. Do you want to move to another car (Y/N): ");
                    string a = ReadYesNo();
                    if (a == "Y")
                    {
                        if (_rng.NextDouble() > 0.05)
                        {
                            Console.WriteLine("They jeer at you but let you pass. All is okay...for now.");
                            return;
                        }
                        else
                        {
                            Console.WriteLine("Uh oh. Two of them get up and block your way.");
                        }
                    }
                    else
                    {
                        if (_rng.NextDouble() > 0.05)
                            Console.WriteLine("They look at you and try to bait you, but you avoid them.");
                        else
                            Console.WriteLine("Oh my, oh my. They're all moving to surround you.");
                    }
                    Console.WriteLine("They pull knives and demand your money.");
                    ShortPause();
                    Console.WriteLine("You, deciding that discretion is the better part of valor, give");
                    Console.WriteLine("them all your money and call it quits for the day.");
                    EndGame(false);
                    return;
                }
                else
                {
                    // 0.8% fire
                    if (_rng.NextDouble() <= 0.008)
                    {
                        Console.WriteLine("Uh oh. The train is slowing down and seems to be stopping.");
                        ShortPause();
                        Console.WriteLine("You're stuck here in the tunnel.");
                        ShortPause();
                        Console.WriteLine("A trainman finally comes through and announces, 'It's just a");
                        int rn = 10 + (int)Math.Floor(35 * _rng.NextDouble());
                        MIN += rn;
                        Console.WriteLine("fire on the tracks folks. We'll be under way in a few minutes.'");
                        Console.WriteLine("In fact, the delay is more like " + rn + " minutes!");
                    }
                }
            }
        }

        void CheckForLunchOrClosing()
        {
            if (MIN > TM)
            {
                Console.WriteLine();
                Console.WriteLine("So sorry, it is after " + tmLabel + "pm and the places to which");
                Console.WriteLine("you want to go will be closed.");
                EndGame(false);
                return;
            }

            PrintTime();
            if (LUN == 1) return;
            if (MIN < 180) return;
            if (PERS == 1) return;

            Console.WriteLine();
            Console.WriteLine("Time for a lunch break. Chili dog and cola. Burp!");
            Console.WriteLine();
            MIN += 24 + (int)Math.Floor(20 * _rng.NextDouble());
            LUN = 1;
        }

        void EndGame(bool win)
        {
            if (!win)
            {
                if (DELTOT != 15)
                {
                    Console.WriteLine();
                    Console.WriteLine("You made it to " + DELTOT + " locations, but");
                    Console.WriteLine("your log still shows the following items:");
                    PrintLog();
                    PrintTime();
                    Console.WriteLine("Perhaps you'll be able to do better tomorrow.");
                }
            }
            else
            {
                PrintTime();
                Console.WriteLine();
                Console.WriteLine("           CONGRATULATIONS !");
                Console.WriteLine();
                Console.WriteLine("You made all your deliveries and pick-ups successfully in the");
                Console.WriteLine("Largest city in the world. Very good!");
            }

            Console.WriteLine("You used $" + TOKEN + " for tokens.");
            Console.WriteLine();
            Console.Write("Would you like to try again (Y/N): ");
            string again = ReadYesNo();
            if (again == "Y")
            {
                Reset();
                Run();
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Bye for now.");
                Environment.Exit(0);
            }
        }

        // Helpers
        void Reset()
        {
            MIN = 0; TOKEN = 0; TKMAX = 20; TK = 0; LUN = 0; PERS = 0; DELTOT = 0; STA = 21; TR = 0; TRSTX = 0; TRX = 0; DES = 0;
            TM = (tmLabel == "6:00") ? 540 : 480;
            LGMAX = 10;
            for (int i = 0; i < DORP.Length; i++) DORP[i] = 0;
            for (int i = 1; i <= 5; i++) DORP[i] = 1;
            for (int i = 6; i <= 10; i++) DORP[i] = 2;
        }

        static void CenterAt(int row, string s)
        {
            try { Console.SetCursorPosition(0, row); } catch { }
            CenterLine(s);
        }
        static void CenterLine(string s)
        {
            int width = 70;
            int pad = Math.Max(0, (width - s.Length) / 2);
            Console.WriteLine(new string(' ', pad) + s);
        }
        static void WaitKey() { Console.ReadKey(true); }
        static void ShortPause() { System.Threading.Thread.Sleep(350); }

        string ReadYesNo()
        {
            string s = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrEmpty(s)) return "Y";
            s = s.Substring(0, 1).ToUpperInvariant();
            while (s != "Y" && s != "N")
            {
                Console.Write("Don't understand your answer. Enter 'Y' or 'N' please: ");
                s = (Console.ReadLine() ?? "").Trim();
                if (string.IsNullOrEmpty(s)) s = "Y"; else s = s.Substring(0, 1).ToUpperInvariant();
            }
            return s;
        }
        string ReadFirstLetter()
        {
            string s = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Substring(0, 1).ToUpperInvariant();
            return s;
        }
        int ReadIntInRange(string prompt, int min, int max)
        {
            for (;;)
            {
                Console.Write(prompt + ": ");
                int v;
                if (int.TryParse(Console.ReadLine(), out v) && v >= min && v <= max) return v;
                Console.WriteLine("Not a valid response.");
            }
        }

        void PrintTime()
        {
            int HR = MIN / 60;
            int MN = MIN - 60 * HR;
            int HP = (HR < 4 ? 9 + HR : HR - 3);
            int hhmm = 100 * HP + MN + 10000;
            string H = hhmm.ToString();
            string outStr = H.Substring(1, 2) + ":" + H.Substring(3, 2);
            Console.WriteLine();
            Console.WriteLine("Time " + outStr);
        }

        void PrintLog()
        {
            Console.WriteLine();
            Console.WriteLine("                    Delivery - Pick-up Log");
            Console.WriteLine();
            for (int i = 1; i <= 15; i++)
            {
                if (DORP[i] == 0 || DORP[i] == 3) continue;
                string kind = (DORP[i] == 1 ? "Delivery" : "Pick-up");
                Console.WriteLine(string.Format("{0,2}  {1,-8}  {2}", i, kind, Packages[i - 1].Destination));
            }
            Console.WriteLine();
            CenterLine("Press any key to continue.");
            WaitKey();
            Console.WriteLine();
        }

        // Data
        void InitData()
        {
            // Packages (20)
            Packages = new List<Package>(new Package[] {
                new Package(1,  "Curator, Museum of Natural History", 1, new int[]{17}, new int[]{1}),
                new Package(2,  "George Washington Bridge Bus Terminal", 1, new int[]{5}, new int[]{1}),
                new Package(3,  "West Side Tennis Club, Forest Hills", 1, new int[]{75}, new int[]{4}),
                new Package(4,  "Nathan's at Coney Island Amusement Park", 1, new int[]{95}, new int[]{2}),
                new Package(5,  "Big Al's Discount Mart, Rockaway Blvd, Woodhaven", 3, new int[]{50,49,51}, new int[]{1,9,9}),
                new Package(6,  "Apollo Theater, 125th St, Harlem", 2, new int[]{11,12}, new int[]{1,9}),
                new Package(7,  "Met's Dugout, Shea Stadium", 1, new int[]{260}, new int[]{3}),
                new Package(8,  "Press Room, Yankee Stadium", 1, new int[]{246}, new int[]{3}),
                new Package(9,  "Lion Keeper, Bronx Zoo", 2, new int[]{204,205}, new int[]{5,8}),
                new Package(10, "Borough Hall, Brooklyn", 2, new int[]{32,221}, new int[]{1,2}),
                new Package(11, "Brooklyn Academy of Music", 1, new int[]{67}, new int[]{2}),
                new Package(12, "Registrar, Brooklyn College, Flatbush", 1, new int[]{234}, new int[]{1}),
                new Package(13, "Computer Science Dept, NYU, Washington Sq", 1, new int[]{25}, new int[]{3}),
                new Package(14, "NY Botanical Gardens", 1, new int[]{55}, new int[]{4}),
                new Package(15, "Windows on the World, World Trade Center", 3, new int[]{29,28,191}, new int[]{1,5,1}),
                new Package(16, "Metropolitan Museum of Art", 1, new int[]{249}, new int[]{1}),
                new Package(17, "Computer Education Dept, Columbia Univ.", 2, new int[]{174,175}, new int[]{2,8}),
                new Package(18, "Alice Tully Hall, Lincoln Center", 1, new int[]{181}, new int[]{2}),
                new Package(19, "New York Stock Exchange", 2, new int[]{219,252}, new int[]{2,2}),
                new Package(20, "Lin Chows, Mott St, Chinatown", 2, new int[]{65,146}, new int[]{4,4}),
            });

            // Stations (264)
            Stations = new List<Station>(new Station[] {
                new Station(1, "207 St/Bdwy/Wash Hts (Manhattan)", 1, new int[]{1}),
                new Station(2, "Dyckman St/Bdwy", 1, new int[]{1}),
                new Station(3, "190 St/Ft Wash Av", 1, new int[]{1}),
                new Station(4, "181 St/Ft Wash Av", 1, new int[]{1}),
                new Station(5, "175 St/GW Bridge", 1, new int[]{1}),
                new Station(6, "168 St/Bdwy (Manhattan)", 3, new int[]{1,3,6}),
                new Station(7, "163 St/Amsterdam Av", 1, new int[]{3}),
                new Station(8, "155 St/St Nicholas Av", 1, new int[]{3}),
                new Station(9, "145 St/St Nicholas Av", 3, new int[]{1,3,4}),
                new Station(10,"135 St/St Nicholas Av", 1, new int[]{3}),
                new Station(11,"125 St/St Nicholas Av", 3, new int[]{1,3,4}),
                new Station(12,"116 St/8 Av", 1, new int[]{3}),
                new Station(13,"110 St/Cathedral Pkwy", 1, new int[]{3}),
                new Station(14,"103 St/Central Pk W", 1, new int[]{3}),
                new Station(15,"96 St/Central Pk W", 1, new int[]{3}),
                new Station(16,"86 St/Central Pk W", 1, new int[]{3}),
                new Station(17,"81 St/Museum Natl History", 1, new int[]{3}),
                new Station(18,"72 St/Central Pk W", 1, new int[]{3}),
                new Station(19,"59 St/Columbus Circle", 4, new int[]{1,3,4,6}),
                new Station(20,"50 St/8 Av", 1, new int[]{2}),
                new Station(21,"42 St/8 Av", 2, new int[]{1,2}),
                new Station(22,"34 St/Penn Station", 2, new int[]{1,2}),
                new Station(23,"23 St/8 Av", 1, new int[]{2}),
                new Station(24,"14 St/8 Av", 2, new int[]{1,2}),
                new Station(25,"W 4 St/Washington Sq", 5, new int[]{1,2,3,4,5}),
                new Station(26,"Spring St/6 Av", 1, new int[]{2}),
                new Station(27,"Canal St/6 Av", 2, new int[]{1,2}),
                new Station(28,"Chambers St/Church St", 3, new int[]{1,2,7}),
                new Station(29,"World Trade Center", 1, new int[]{2}),
                new Station(30,"Bdwy/Nassau St/Fulton St (Manhattan)", 3, new int[]{1,7,8}),
                new Station(31,"High St/Brooklyn Br (Bklyn)", 1, new int[]{1}),
                new Station(32,"Jay St/Borough Hall", 2, new int[]{1,5}),
                new Station(33,"Hoyt St", 2, new int[]{1,11}),
                new Station(34,"Lafayette Av", 1, new int[]{11}),
                new Station(35,"Clinton Av", 1, new int[]{11}),
                new Station(36,"Franklin Av", 1, new int[]{11}),
                new Station(37,"Nostrand Av", 1, new int[]{1}),
                new Station(38,"Kingston Av", 1, new int[]{11}),
                new Station(39,"Utica Av", 1, new int[]{1}),
                new Station(40,"Ralph Av", 1, new int[]{11}),
                new Station(41,"Rockaway Av", 1, new int[]{11}),
                new Station(42,"Bdwy, E NY", 1, new int[]{11}),
                new Station(43,"Liberty Av", 1, new int[]{11}),
                new Station(44,"Van Sicien Av", 1, new int[]{11}),
                new Station(45,"Shepherd Av", 1, new int[]{11}),
                new Station(46,"Euclid Av", 1, new int[]{1}),
                new Station(47,"Grant Av (Brooklyn)", 1, new int[]{1}),
                new Station(48,"80 St/Liberty Av (Queens)", 1, new int[]{1}),
                new Station(49,"88 St/Liberty Av", 1, new int[]{1}),
                new Station(50,"Rockaway Blvd", 1, new int[]{1}),
                new Station(51,"104 St/Liberty Av", 1, new int[]{1}),
                new Station(52,"111 St/Liberty Av", 1, new int[]{1}),
                new Station(53,"Lefferts Blvd (Queens)", 1, new int[]{1}),
                new Station(54,"205 St/Bainbridge Av (Bronx)", 1, new int[]{4}),
                new Station(55,"Bedford Pk Blvd (NY Botanical Garden)", 1, new int[]{4}),
                new Station(56,"Kingsbridge Rd", 1, new int[]{4}),
                new Station(57,"Fordham Rd", 1, new int[]{4}),
                new Station(58,"Tremont Av", 1, new int[]{4}),
                new Station(59,"47-50 St/Rockefeller Center", 3, new int[]{3,4,5}),
                new Station(60,"42 St/Av Americas", 4, new int[]{3,4,5,9}),
                new Station(61,"34 St/Herald Sq", 4, new int[]{3,4,5,10}),
                new Station(62,"23 St/Av Americas", 1, new int[]{5}),
                new Station(63,"14 St/Av Americas", 3, new int[]{5,6,7}),
                new Station(64,"Bdwy/Lafayette St", 4, new int[]{3,4,5,8}),
                new Station(65,"Grand St (Manhattan)", 2, new int[]{3,4}),
                new Station(66,"DeKalb Av/Flatbush Av (Bklyn)", 2, new int[]{4,10}),
                new Station(67,"Atlantic Av/Pacific St/BAM", 5, new int[]{3,4,7,8,10}),
                new Station(68,"179 St/Hillside Av (Queens)", 2, new int[]{2,5}),
                new Station(69,"169 St", 1, new int[]{2}),
                new Station(70,"Parsons Blvd", 2, new int[]{2,5}),
                new Station(71,"Sutphin Av", 1, new int[]{2}),
                new Station(72,"Van Wyck Blvd", 1, new int[]{2}),
                new Station(73,"Union Tpk", 2, new int[]{2,5}),
                new Station(74,"75 Av", 1, new int[]{2}),
                new Station(75,"71 Av/Continental Av/Forest Hills", 4, new int[]{2,5,10,11}),
                new Station(76,"Roosevelt Av", 5, new int[]{2,5,9,10,11}),
                new Station(77,"Queens Plaza", 4, new int[]{2,5,10,11}),
                new Station(78,"23 St/Ely Av (Queens)", 2, new int[]{2,5}),
                new Station(79,"Lexington Av (Manhattan)", 2, new int[]{2,5}),
                new Station(80,"5th Av/53 St", 2, new int[]{2,5}),
                new Station(81,"7th Av/53 St", 3, new int[]{2,3,4}),
                new Station(82,"36 St/4 Av", 2, new int[]{3,10}),
                new Station(83,"9 Av/39 St", 1, new int[]{3}),
                new Station(84,"Ft Hamilton Pky", 1, new int[]{3}),
                new Station(85,"50 St/New Utrecht Av", 1, new int[]{3}),
                new Station(86,"55 St/New Utrecht Av", 1, new int[]{3}),
                new Station(87,"62 St/New Utrecht Av", 2, new int[]{3,10}),
                new Station(88,"71 St/New Utrecht Av", 1, new int[]{3}),
                new Station(89,"79 St/New Utrecht Av", 1, new int[]{3}),
                new Station(90,"18 Av/New Utrecht Av", 1, new int[]{3}),
                new Station(91,"20 Av/86 St", 1, new int[]{3}),
                new Station(92,"Bay Pky/86 St", 1, new int[]{3}),
                new Station(93,"25 Av/86 St", 1, new int[]{3}),
                new Station(94,"Bay 50 St", 1, new int[]{3}),
                new Station(95,"Coney Island/Surf Av (Bklyn)", 4, new int[]{3,4,5,10}),
                new Station(96,"67 Av/Queens Blvd", 2, new int[]{10,11}),
                new Station(97,"63 Dr/Queens Blvd", 2, new int[]{10,11}),
                new Station(98,"Woodhaven Blvd", 2, new int[]{10,11}),
                new Station(99,"Grand Av/Queens Blvd", 2, new int[]{10,11}),
                new Station(100,"Elmhurst Av", 2, new int[]{10,11}),
                new Station(101,"65 St/Bdwy", 2, new int[]{10,11}),
                new Station(102,"Northern Blvd", 2, new int[]{10,11}),
                new Station(103,"46 St/Bdwy", 2, new int[]{10,11}),
                new Station(104,"Steinway St", 2, new int[]{10,11}),
                new Station(105,"2 Av/Houston St", 1, new int[]{5}),
                new Station(106,"Delancey St", 1, new int[]{5}),
                new Station(107,"East Bdwy (Manhattan)", 1, new int[]{5}),
                new Station(108,"York St/Jay St (Brooklyn)", 1, new int[]{5}),
                new Station(109,"Bergen St", 2, new int[]{5,11}),
                new Station(110,"Carroll St", 2, new int[]{5,11}),
                new Station(111,"Smith St", 2, new int[]{5,11}),
                new Station(112,"4 Av/9 St", 2, new int[]{5,10}),
                new Station(113,"7 Av/9 St", 1, new int[]{5}),
                new Station(114,"15 St/Prospect Park", 1, new int[]{5}),
                new Station(115,"Ft Hamilton Pwy", 1, new int[]{5}),
                new Station(116,"Church Av", 1, new int[]{5}),
                new Station(117,"Ditmas Av", 1, new int[]{5}),
                new Station(118,"18 Av/McDonald Av", 1, new int[]{5}),
                new Station(119,"Kings Hwy", 1, new int[]{5}),
                new Station(120,"Avenue U", 1, new int[]{5}),
                new Station(121,"Avenue X", 1, new int[]{5}),
                new Station(122,"Neptune Av", 1, new int[]{5}),
                new Station(123,"W 8th/NY Aquarium", 1, new int[]{5}),
                new Station(124,"7 Av/Flatbush Av", 1, new int[]{4}),
                new Station(125,"Prospect Park", 1, new int[]{4}),
                new Station(126,"Church Av/E 18 St", 1, new int[]{4}),
                new Station(127,"Newkirk Av", 1, new int[]{4}),
                new Station(128,"Kings Hwy/E 16 St", 1, new int[]{4}),
                new Station(129,"Sheepshead Bay", 1, new int[]{4}),
                new Station(130,"Brighton Beach", 1, new int[]{4}),
                new Station(131,"Court Square", 1, new int[]{11}),
                new Station(132,"21 St/Jackson Av (Queens)", 1, new int[]{11}),
                new Station(133,"Greenpoint Av (Bklyn)", 1, new int[]{11}),
                new Station(134,"Nassau Av", 1, new int[]{11}),
                new Station(135,"Metropolitan Av", 1, new int[]{11}),
                new Station(136,"Broadway/Union Av", 1, new int[]{11}),
                new Station(137,"Flushing-Marcy Avs", 1, new int[]{11}),
                new Station(138,"Myrtle-Willoughby Avs", 1, new int[]{11}),
                new Station(139,"Bedford-Nostrand Avs", 1, new int[]{11}),
                new Station(140,"36 St/Northern Blvd", 2, new int[]{10,11}),
                new Station(141,"Lexington Av/59-60 Sts (Manhattan)", 2, new int[]{8,10}),
                new Station(142,"5th Av/59-60 Sts", 1, new int[]{10}),
                new Station(143,"57 St/7 Av", 1, new int[]{10}),
                new Station(144,"Times Sq/42 St/Bdwy", 4, new int[]{6,7,9,10}),
                new Station(145,"Union Sq/14 St", 2, new int[]{8,10}),
                new Station(146,"Canal St/Bdwy (Manhattan)", 1, new int[]{10}),
                new Station(147,"Union St/4 Av", 1, new int[]{10}),
                new Station(148,"Prospect Av", 1, new int[]{10}),
                new Station(149,"25 St/4 Av", 1, new int[]{10}),
                new Station(150,"45 St/4 Av", 1, new int[]{10}),
                new Station(151,"53 St/4 Av", 1, new int[]{10}),
                new Station(152,"59 St/4 Av", 1, new int[]{10}),
                new Station(153,"8 Av/62 St", 1, new int[]{10}),
                new Station(154,"Ft Hamilton Pwy", 1, new int[]{10}),
                new Station(155,"18 Av/64 St", 1, new int[]{10}),
                new Station(156,"20 Av/64 St", 1, new int[]{10}),
                new Station(157,"Bay Pwy/Av O", 1, new int[]{10}),
                new Station(158,"Kings Hwy/W 7 St", 1, new int[]{10}),
                new Station(159,"Avenue U/W 7 St", 1, new int[]{10}),
                new Station(160,"86 St/W 7 St", 1, new int[]{10}),
                new Station(161,"242 St/Van Cortlandt Park", 1, new int[]{6}),
                new Station(162,"238 St/Bdwy", 1, new int[]{6}),
                new Station(163,"231 St/Bdwy (Bronx)", 1, new int[]{6}),
                new Station(164,"225 St/Bdwy (Manhattan)", 1, new int[]{6}),
                new Station(165,"215 St/10 Av", 1, new int[]{6}),
                new Station(166,"207 St/10 Av", 1, new int[]{6}),
                new Station(167,"Dyckman Av", 1, new int[]{6}),
                new Station(168,"191 St/St Nicholas Av", 1, new int[]{6}),
                new Station(169,"181 St/St Nicholas Av", 1, new int[]{6}),
                new Station(170,"157 St/Bdwy", 1, new int[]{6}),
                new Station(171,"145 St/Bdwy", 1, new int[]{6}),
                new Station(172,"137 St/Bdwy", 1, new int[]{6}),
                new Station(173,"125 St/Bdwy", 1, new int[]{6}),
                new Station(174,"116 St/Bdwy/Columbia Univ", 1, new int[]{6}),
                new Station(175,"110 St/Cathedral Pkwy", 1, new int[]{6}),
                new Station(176,"103 St/Bdwy", 1, new int[]{6}),
                new Station(177,"96 St/Bdwy", 2, new int[]{6,7}),
                new Station(178,"86 St/Bdwy", 1, new int[]{6}),
                new Station(179,"79 St/Bdwy", 1, new int[]{6}),
                new Station(180,"72 St/Bdwy", 2, new int[]{6,7}),
                new Station(181,"66 St/Bdwy/Lincoln Center", 1, new int[]{6}),
                new Station(182,"50 St/Bdwy", 1, new int[]{6}),
                new Station(183,"Penn Station/34 St/7 Av", 2, new int[]{6,7}),
                new Station(184,"28 St/7 Av", 1, new int[]{6}),
                new Station(185,"23 St/7 Av", 1, new int[]{6}),
                new Station(186,"18 St/7 Av", 1, new int[]{6}),
                new Station(187,"Christopher St", 1, new int[]{6}),
                new Station(188,"Houston St", 1, new int[]{6}),
                new Station(189,"Canal & Varick Sts", 1, new int[]{6}),
                new Station(190,"Franklin St", 1, new int[]{6}),
                new Station(191,"Chambers St/W Bdwy", 2, new int[]{6,7}),
                new Station(192,"Cortlandt St/World Trade Center", 1, new int[]{6}),
                new Station(193,"Rector St/Greenwich St", 1, new int[]{6}),
                new Station(194,"South Ferry/Battery Park", 1, new int[]{6}),
                new Station(195,"241 St/White Plains Rd (Bronx)", 1, new int[]{7}),
                new Station(196,"238 St/White Plains Rd", 1, new int[]{7}),
                new Station(197,"233 St/White Plains Rd", 1, new int[]{7}),
                new Station(198,"225 St/White Plains Rd", 1, new int[]{7}),
                new Station(199,"219 St/White Plains Rd", 1, new int[]{7}),
                new Station(200,"Gun Hill Rd/White Plains Rd", 1, new int[]{7}),
                new Station(201,"Burke Av/White Plains Rd", 1, new int[]{7}),
                new Station(202,"Allerton Av/White Plains Rd", 1, new int[]{7}),
                new Station(203,"Pelham Pkwy/White Plains Rd", 1, new int[]{7}),
                new Station(204,"Bronx Pk E/White Plains Rd", 1, new int[]{7}),
                new Station(205,"E 180 St/Bronx Zoo", 1, new int[]{7}),
                new Station(206,"E Tremont Av/Boston Rd", 1, new int[]{7}),
                new Station(207,"174 St/Southern Blvd", 1, new int[]{7}),
                new Station(208,"Freeman St", 1, new int[]{7}),
                new Station(209,"Simpson St", 1, new int[]{7}),
                new Station(210,"Intervale Av", 1, new int[]{7}),
                new Station(211,"Prospect Av", 1, new int[]{7}),
                new Station(212,"Jackson Av", 1, new int[]{7}),
                new Station(213,"3 Av/149 St", 1, new int[]{7}),
                new Station(214,"149 St/Grand Concourse (Bronx)", 2, new int[]{7,8}),
                new Station(215,"135 St/Lenox Av (Manhattan)", 1, new int[]{7}),
                new Station(216,"125 St/Lenox Av", 1, new int[]{7}),
                new Station(217,"116 St/Lenox Av", 1, new int[]{7}),
                new Station(218,"110 St/Lenox Av", 1, new int[]{7}),
                new Station(219,"Wall St (Manhattan)", 1, new int[]{7}),
                new Station(220,"Clark St (Brooklyn)", 1, new int[]{7}),
                new Station(221,"Borough Hall/Court St (Bklyn)", 2, new int[]{7,8}),
                new Station(222,"Hoyt St/Fulton St", 1, new int[]{7}),
                new Station(223,"Nevins St", 2, new int[]{7,8}),
                new Station(224,"Bergen St", 1, new int[]{7}),
                new Station(225,"Grand Army Plaza, Prospect Park", 1, new int[]{7}),
                new Station(226,"Eastern Pkwy/Brooklyn Museum", 1, new int[]{7}),
                new Station(227,"Franklin Av/Eastern Pkwy", 1, new int[]{7}),
                new Station(228,"President St", 1, new int[]{7}),
                new Station(229,"Sterling St/Nostrand Av", 1, new int[]{7}),
                new Station(230,"Winthrop St/Nostrand Av", 1, new int[]{7}),
                new Station(231,"Church Av/Nostrand Av", 1, new int[]{7}),
                new Station(232,"Beverley Rd/Nostrand Av", 1, new int[]{7}),
                new Station(233,"Newkirk Av/Nostrand Av", 1, new int[]{7}),
                new Station(234,"Flatbush Av/Bklyn College", 1, new int[]{7}),
                new Station(235,"Woodlawn/Jerome Av (Bronx)", 1, new int[]{8}),
                new Station(236,"Mosholu Pkwy", 1, new int[]{8}),
                new Station(237,"Bedford Park Blvd", 1, new int[]{8}),
                new Station(238,"Kingsbridge Rd", 1, new int[]{8}),
                new Station(239,"Fordham Rd/Jerome Av", 1, new int[]{8}),
                new Station(240,"183 St/Jerome Av", 1, new int[]{8}),
                new Station(241,"Burnside Av/Jerome Av", 1, new int[]{8}),
                new Station(242,"176 St/Jerome Av", 1, new int[]{8}),
                new Station(243,"Mt Eden Av/Jerome Av", 1, new int[]{8}),
                new Station(244,"170 St/Jerome Av", 1, new int[]{8}),
                new Station(245,"167 St/River Av", 1, new int[]{8}),
                new Station(246,"161 St/Yankee Stadium (Bronx)", 2, new int[]{4,8}),
                new Station(247,"", 1, new int[]{11}),
                new Station(248,"125 St/Lexington Av (Manhattan)", 1, new int[]{8}),
                new Station(249,"86 St/Lexington Av/Metropolitan Museum", 1, new int[]{8}),
                new Station(250,"42 St/Grand Central Sta", 2, new int[]{8,9}),
                new Station(251,"Bklyn Bridge/Worth St", 1, new int[]{8}),
                new Station(252,"Wall St/Bdwy", 1, new int[]{8}),
                new Station(253,"Bowling Green (Manhattan)", 1, new int[]{8}),
                new Station(254,"Nostrand Av/Eastern Pkwy", 1, new int[]{8}),
                new Station(255,"Rockaway Av/Livonia Av", 1, new int[]{8}),
                new Station(256,"New Lots Av (Brooklyn)", 1, new int[]{8}),
                new Station(257,"Queensboro Plaza (Queens)", 1, new int[]{9}),
                new Station(258,"61 St/Roosevelt Av", 1, new int[]{9}),
                new Station(259,"Junction Blvd", 1, new int[]{9}),
                new Station(260,"Willets Point/Shea Stadium", 1, new int[]{9}),
                new Station(261,"Main St/Flushing (Queens)", 1, new int[]{9}),
                new Station(262,"Classon Av", 1, new int[]{11}),
                new Station(263,"Clinton-Washington Avs", 1, new int[]{11}),
                new Station(264,"Fulton St/Lafayette Av", 1, new int[]{11}),
            });

            // Trains (11)
            Trains = new List<Train>(new Train[] {
                new Train(1, "A - 8 Av Express", 29, new int[]{1,2,3,4,5,6,9,11,19,21,22,24,25,27,28,30,31,32,33,37,39,46,47,48,49,50,51,52,53}),
                new Train(2, "E - 8 Av Local", 24, new int[]{68,69,70,71,72,73,74,75,76,77,78,79,80,81,20,21,22,23,24,25,26,27,28,29}),
                new Train(3, "B - Av Americas Express", 36, new int[]{6,7,8,9,10,11,12,13,14,15,16,17,18,19,81,59,60,61,25,64,65,67,82,83,84,85,86,87,88,89,90,91,92,93,94,95}),
                new Train(4, "D - Av Americas Express", 26, new int[]{54,55,56,57,58,246,9,11,19,81,59,60,61,25,64,65,66,67,124,125,126,127,128,129,130,95}),
                new Train(5, "F - Av Americas Local", 37, new int[]{68,70,73,75,76,77,78,79,80,59,60,61,62,63,25,64,105,106,107,108,32,109,110,111,112,113,114,115,116,117,118,119,120,121,122,123,95}),
                new Train(6, "1 - Bdwy-7th Av Local", 38, new int[]{161,162,163,164,165,166,167,168,169,6,170,171,172,173,174,175,176,177,178,179,180,181,19,182,144,183,184,185,186,63,187,188,189,190,191,192,193,194}),
                new Train(7, "2 - 7th Av Express", 49, new int[]{195,196,197,198,199,200,201,202,203,204,205,206,207,208,209,210,211,212,213,214,215,216,217,218,177,180,144,183,63,191,28,30,219,220,221,222,223,67,224,225,226,227,228,229,230,231,232,233,234}),
                new Train(8, "4 - Lexington Av Express", 29, new int[]{235,236,237,238,239,240,241,242,243,244,245,246,214,248,249,141,250,145,64,251,30,252,253,221,223,67,254,255,256}),
                new Train(9, "7 - Flushing Express", 9, new int[]{144,60,250,257,258,76,259,260,261}),
                new Train(10,"N - Broadway Express", 40, new int[]{75,96,97,98,99,100,76,101,102,103,104,140,77,141,142,143,144,61,145,146,66,67,147,112,148,149,82,150,151,152,153,154,87,155,156,157,158,159,160,95}),
                new Train(11,"CG - Bklyn/Queens Crosstown", 29, new int[]{75,96,97,98,99,100,76,101,102,103,104,140,77,131,132,133,134,135,136,137,138,139,262,263,264,33,109,110,111}),
            });

            // Set termini
            for (int i = 0; i < Trains.Count; i++)
            {
                Train t = Trains[i];
                Station s1 = Stations[t.StationIds[0] - 1];
                Station s2 = Stations[t.StationIds[t.StopCount - 1] - 1];
                t.Terminus1 = s1.Name;
                t.Terminus2 = s2.Name;
            }
        }

        void ShuffleInitialPackages()
        {
            for (int i = 0; i < Packages.Count - 1; i++)
            {
                int k = i + _rng.Next(Packages.Count - i);
                Package tmp = Packages[i]; Packages[i] = Packages[k]; Packages[k] = tmp;
            }
            for (int i = 11; i <= 20; i++) DORP[i] = 3;
        }
    }
}
