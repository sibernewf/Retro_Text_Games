using System;
using System.Threading;

namespace GhostGuzzler
{
    class Program
    {
        // Tweak to change game speed. Smaller = faster ghosts.
        const int TickMs = 120;       // close to the booklet’s suggested pacing
        const int BarrierCol = 18;    // BASIC used column 18 as ":" barrier

        static void Main()
        {
            Console.Title = "Ghost Guzzler";
            var rng = new Random();

            int score = 0;    // S
            int you = 0;      // Y (your number 0..9)
            int lives = 3;    // L ("/" printed)
            while (true)
            {
                // New ghost
                int ghost = rng.Next(0, 10); // N
                int pos = 1;                 // I (starts at left and advances)

                while (true)
                {
                    // ---- draw frame (roughly mirrors lines 70..120) ----
                    Console.Clear();
                    // lives as slashes on the first row
                    Console.WriteLine(new string('/', lives));
                    // second row: ghost at pos, barrier at col 18, your number after it
                    // build a simple line with spaces
                    var line = new char[BarrierCol + 3];
                    for (int i = 0; i < line.Length; i++) line[i] = ' ';
                    // ghost number (single char)
                    var gch = (char)('0' + ghost);
                    int gcol = Math.Clamp(pos - 1, 0, line.Length - 1);
                    line[gcol] = gch;
                    line[BarrierCol - 1] = ':';              // barrier
                    line[Math.Min(BarrierCol + 1, line.Length - 1)] = (char)('0' + you); // your number
                    Console.WriteLine(new string(line));
                    Console.WriteLine($"Score: {score}   Lives: {lives}   Controls: [M]=change number  [X]=guzzle");

                    // ---- input handling (lines 130–160) ----
                    var until = DateTime.UtcNow.AddMilliseconds(TickMs);
                    bool triedGuzzle = false;
                    while (DateTime.UtcNow < until)
                    {
                        if (!Console.KeyAvailable) { Thread.Sleep(1); continue; }
                        var key = Console.ReadKey(intercept: true).Key;
                        if (key == ConsoleKey.M)
                        {
                            you = (you + 1) % 10;             // IF B$="M" THEN LET Y=Y+1 ; IF Y=10 THEN LET Y=0
                        }
                        else if (key == ConsoleKey.X)
                        {
                            triedGuzzle = true;                // IF B$="X" THEN GOTO 220
                            break;
                        }
                    }

                    // ghost proceeds (lines 170–180)
                    pos++;

                    // guzzle check (lines 220–260)
                    if (triedGuzzle)
                    {
                        if (you == ghost)
                        {
                            // PRINT "GOT IT" ; S = S + (18 - I) ; new ghost
                            score += Math.Max(0, BarrierCol - pos);
                            break;
                        }
                        // wrong number -> carry on (program simply continues)
                    }

                    // barrier reached? (lines 190–210)
                    if (pos >= BarrierCol)
                    {
                        lives--;
                        if (lives <= 0)
                        {
                            // end screen (lines 270–360 condensed)
                            Console.Clear();
                            Console.WriteLine("YOUR GHOST GUZZLING");
                            Console.WriteLine($"SCORE IS {score}");
                            Console.Write("\nANOTHER GO? (Y/N): ");
                            var again = Console.ReadKey().Key;
                            if (again == ConsoleKey.Y)
                            {
                                // reset whole game
                                score = 0;
                                you = 0;
                                lives = 3;
                                Console.Clear();
                                Console.WriteLine("GHOST GUZZLER");
                                Thread.Sleep(600);
                                break; // break out of ghost loop, continue outer while (new ghost)
                            }
                            return; // quit
                        }
                        // still have lives -> spawn next ghost
                        break;
                    }
                } // inner while (one ghost)
            }     // outer while (game loop)
        }
    }
}
