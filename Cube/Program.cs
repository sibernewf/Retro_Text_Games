using System;

class Program
{
    static void Main()
    {
        int X = 0, Y = 0, Z = 0;
        int[] M1 = new int[3];
        int[] M2 = new int[3];
        int[] M3 = new int[3];
        int[] M4 = new int[3];
        int[] M5 = new int[3];
        int Money = 500;
        Random rand = new Random();

        Console.Write("DO YOU WANT TO SEE THE INSTRUCTIONS? (YES=1, NO=0)? ");
        string? answer = Console.ReadLine();
        if (answer == "1")
        {
            Console.WriteLine("THIS IS A GAME IN WHICH YOU WILL BE PLAYING AGAINST THE");
            Console.WriteLine("RANDOM DECISION OF THE COMPUTER. THE FIELD OF PLAY IS A");
            Console.WriteLine("CUBE OF SIDE 3. ANY OF THE 27 LOCATIONS CAN BE DESIGNATED");
            Console.WriteLine("BY PRINTING THREE NUMBERS SUCH AS 2,3,1. AT THE START,");
            Console.WriteLine("YOU ARE AUTOMATICALLY AT LOCATION 1,1,1. THE OBJECT OF");
            Console.WriteLine("THE GAME IS TO GET TO LOCATION 3,3,3, ONE NUMBER AT A TIME.");
            Console.WriteLine("THE COMPUTER WILL PICK AT RANDOM 5 LOCATIONS AT WHICH");
            Console.WriteLine("IT WILL PLANT LAND MINES. IF YOU HIT ONE OF THESE LOCATIONS,");
            Console.WriteLine("YOU LOSE. ONE OTHER DETAIL, YOU MAY MOVE ONLY ONE SPACE");
            Console.WriteLine("IN ONE DIRECTION EACH MOVE. FOR EXAMPLE, FROM 1,2,2 YOU");
            Console.WriteLine("MAY MOVE TO 2,2,2 OR 1,3,2. YOU MAY NOT CHANGE 2");
            Console.WriteLine("NUMBERS ON THE SAME MOVE. IF YOU MAKE AN ILLEGAL");
            Console.WriteLine("MOVE, YOU LOSE AND THE COMPUTER TAKES THE MONEY YOU MAY");
            Console.WriteLine("HAVE BET ON THAT ROUND.");
        }

        while (true)
        {
            Console.WriteLine();
            Console.Write("WANT TO MAKE A WAGER? ");
            string? wagerAnswer = Console.ReadLine();
            if (wagerAnswer != "1")
                break;

            Console.Write("HOW MUCH? ");
            string? wagerInput = Console.ReadLine();
            if (!int.TryParse(wagerInput, out int wager) || wager <= 0)
                continue;

            if (wager > Money)
                wager = Money;

            // Player starts at 1,1,1
            X = 1; Y = 1; Z = 1;

            // Place mines
            M1[0] = rand.Next(1, 4);
            M1[1] = rand.Next(1, 4);
            M1[2] = rand.Next(1, 4);

            M2[0] = rand.Next(1, 4);
            M2[1] = rand.Next(1, 4);
            M2[2] = rand.Next(1, 4);

            M3[0] = rand.Next(1, 4);
            M3[1] = rand.Next(1, 4);
            M3[2] = rand.Next(1, 4);

            M4[0] = rand.Next(1, 4);
            M4[1] = rand.Next(1, 4);
            M4[2] = rand.Next(1, 4);

            M5[0] = rand.Next(1, 4);
            M5[1] = rand.Next(1, 4);
            M5[2] = rand.Next(1, 4);

            bool playing = true;
            while (playing)
            {
                Console.WriteLine();
                Console.WriteLine("IT'S YOUR MOVE");
                Console.Write("X = ");
                string? sx = Console.ReadLine();
                Console.Write("Y = ");
                string? sy = Console.ReadLine();
                Console.Write("Z = ");
                string? sz = Console.ReadLine();

                if (!int.TryParse(sx, out int NX) ||
                    !int.TryParse(sy, out int NY) ||
                    !int.TryParse(sz, out int NZ))
                {
                    Console.WriteLine("INVALID INPUT");
                    continue;
                }

                // Illegal move check (must only change 1 coordinate)
                int changeCount = 0;
                if (NX != X) changeCount++;
                if (NY != Y) changeCount++;
                if (NZ != Z) changeCount++;

                if (changeCount != 1)
                {
                    Console.WriteLine("ILLEGAL MOVE, YOU LOSE");
                    Money -= wager;
                    playing = false;
                    continue;
                }

                X = NX;
                Y = NY;
                Z = NZ;

                // Check for mine
                if ((X == M1[0] && Y == M1[1] && Z == M1[2]) ||
                    (X == M2[0] && Y == M2[1] && Z == M2[2]) ||
                    (X == M3[0] && Y == M3[1] && Z == M3[2]) ||
                    (X == M4[0] && Y == M4[1] && Z == M4[2]) ||
                    (X == M5[0] && Y == M5[1] && Z == M5[2]))
                {
                    Console.WriteLine("BANG **** YOU LOSE");
                    Money -= wager;
                    playing = false;
                    continue;
                }

                // Check for win
                if (X == 3 && Y == 3 && Z == 3)
                {
                    Console.WriteLine("CONGRATULATIONS");
                    Money += wager;
                    playing = false;
                }
            }

            Console.WriteLine($"YOU NOW HAVE {Money} DOLLARS");
            Console.Write("DO YOU WANT TO TRY AGAIN? ");
            string? again = Console.ReadLine();
            if (again != "1")
                break;
        }

        Console.WriteLine("GOODBYE");
    }
}
