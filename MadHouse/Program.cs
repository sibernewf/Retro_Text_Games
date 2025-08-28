using System;
using System.Threading;

namespace MadHouse
{
    class Program
    {
        // Visual grid: doorways move among these rows.
        const int Rows = 6;                 // 6 vertical positions feels good in console
        const int TickMs = 140;             // game speed
        const int StartFootsteps = 300;     // like CT in the listing (counts down)
        const int FootstepStep = 10;

        static readonly Random Rng = new Random();

        static void Main()
        {
            Console.Title = "Mad House";
            Console.CursorVisible = false;

            // Doorway positions 0..Rows-1 (top..bottom)
            int[] p = { Rng.Next(Rows), Rng.Next(Rows), Rng.Next(Rows) };
            // Velocities -1 or +1
            int[] v = { Rng.Next(2) == 0 ? -1 : 1, Rng.Next(2) == 0 ? -1 : 1, Rng.Next(2) == 0 ? -1 : 1 };

            int footsteps = StartFootsteps;
            bool won = false;

            while (true)
            {
                // ----- draw frame -----
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("MAD HOUSE  — align all three doors, then press J to run!");
                Console.WriteLine("Controls: A/X reverse NEAR door | N/M reverse FAR door | J dash when aligned");
                Console.WriteLine($"Footsteps: {footsteps,3}   (you lose at 0)\n");

                for (int r = 0; r < Rows; r++)
                {
                    // Each room column is drawn as a wall with a single space opening where the door is.
                    DrawRoom(r == p[0]);
                    Console.Write("   ");
                    DrawRoom(r == p[1]);
                    Console.Write("   ");
                    DrawRoom(r == p[2]);
                    Console.WriteLine();
                }
                Console.WriteLine();

                bool aligned = p[0] == p[1] && p[1] == p[2];
                if (aligned) Console.WriteLine(">>> DOORWAYS ALIGNED! Press J **now** to dash! <<<");
                else         Console.WriteLine("Doorways misaligned. Reverse the near/far doors to sync them.");

                // ----- input window for this tick -----
                bool dash = false;
                var until = DateTime.UtcNow.AddMilliseconds(TickMs);
                while (DateTime.UtcNow < until)
                {
                    if (!Console.KeyAvailable) { Thread.Sleep(1); continue; }
                    var key = Console.ReadKey(intercept: true).Key;
                    if (key == ConsoleKey.A || key == ConsoleKey.X) v[0] = -v[0];
                    else if (key == ConsoleKey.N || key == ConsoleKey.M) v[2] = -v[2];
                    else if (key == ConsoleKey.J) dash = true;
                }

                // Resolve dash
                if (dash && aligned)
                {
                    won = true;
                    break;
                }

                // Move doors (middle door always moves)
                for (int i = 0; i < 3; i++)
                {
                    p[i] += v[i];
                    if (p[i] < 0) { p[i] = 0; v[i] = +1; }
                    if (p[i] >= Rows) { p[i] = Rows - 1; v[i] = -1; }
                }

                // Tick footsteps
                footsteps -= FootstepStep;
                if (footsteps <= 0) break;
            }

            Console.Clear();
            Console.CursorVisible = true;
            if (won)
            {
                Console.WriteLine("YOU'RE OUT! YOU'RE FREE!!");
            }
            else
            {
                Console.WriteLine("TOO LATE... THE FOOTSTEPS HAVE STOPPED.");
                Console.WriteLine("A cold hand grips your shoulder...");
            }
            Console.WriteLine("\nPress any key to quit.");
            Console.ReadKey(true);
        }

        static void DrawRoom(bool hasDoorHere)
        {
            // Render like a wall segment: ███, with a gap for the door
            // [## ##] with a single space in the middle for the door.
            if (hasDoorHere) Console.Write("[## ##]");
            else             Console.Write("[####]");
        }
    }
}
