using System;

namespace Target3D
{
    internal static class Program
    {
        static readonly Random Rnd = new Random();

        static void Main()
        {
            Console.WriteLine("YOU ARE THE WEAPONS OFFICER ON THE STAR SHIP ENTERPRISE");
            Console.WriteLine("AND THIS IS A TEST TO SEE HOW ACCURATE A SHOT YOU'LL");
            Console.WriteLine("BE ABLE TO MAKE IN 3-DIMENSIONAL SPACE.");
            Console.WriteLine("THE TARGET SHIP IS SOMEWHERE IN SPACE.  YOU WILL BE TOLD:");
            Console.WriteLine("  • THE RANDOM OFFSET OF THE TARGET IN X, Y, Z COORDINATES");
            Console.WriteLine("  • THE APPROX. NUMBER OF DEGREES FROM THE X OR Z AXIS");
            Console.WriteLine("  • THE APPROX. DISTANCE TO THE TARGET");
            Console.WriteLine("YOU WILL THEN PROCEED TO SHOOT AT THE TARGET UNTIL IT IS DESTROYED!");
            Console.WriteLine("\nGOOD LUCK!\n");

            // Pick random target coordinates
            double tx = (Rnd.NextDouble() - 0.5) * 20000; // ~±10,000
            double ty = (Rnd.NextDouble() - 0.5) * 20000;
            double tz = (Rnd.NextDouble() - 0.5) * 20000;

            bool destroyed = false;
            int shots = 0;

            while (!destroyed)
            {
                shots++;

                // Give approximate bearings
                double dist = Distance(tx, ty, tz);
                double radX = Math.Atan2(ty, tx);
                double radZ = Math.Atan2(ty, tz);

                Console.WriteLine($"READINGS FROM X AXIS = {radX:F6} RADIANS");
                Console.WriteLine($"READINGS FROM Z AXIS = {radZ:F6} RADIANS");
                Console.WriteLine($"APPROX DEGREES FROM X AXIS = {radX * 180 / Math.PI:F2}");
                Console.WriteLine($"APPROX DEGREES FROM Z AXIS = {radZ * 180 / Math.PI:F2}");
                Console.WriteLine($"TARGET SIGHTED! APPROX POSITION = X:{tx:F2}, Y:{ty:F2}, Z:{tz:F2}");
                Console.WriteLine($"ESTIMATED DISTANCE = {dist:F2}\n");

                // Get user shot input
                double angleFromX = AskDouble("INPUT ANGLE DEVIATION FROM X (degrees): ");
                double angleFromZ = AskDouble("INPUT ANGLE DEVIATION FROM Z (degrees): ");
                double shotDist = AskDouble("INPUT SHOT DISTANCE (km): ");

                // Convert to radians
                double ax = angleFromX * Math.PI / 180.0;
                double az = angleFromZ * Math.PI / 180.0;

                // Compute shot coordinates
                double sx = Math.Cos(ax) * shotDist;
                double sy = Math.Sin(ax) * shotDist;
                double sz = Math.Cos(az) * shotDist;

                double shotError = Distance(sx - tx, sy - ty, sz - tz);

                Console.WriteLine($"\nSHOT POSITION: X:{sx:F2}, Y:{sy:F2}, Z:{sz:F2}");
                Console.WriteLine($"APPROX POSITION OF EXPLOSION: X:{sx:F2}, Y:{sy:F2}, Z:{sz:F2}");
                Console.WriteLine($"DISTANCE FROM TARGET = {shotError:F2}\n");

                if (shotError <= 20)
                {
                    Console.WriteLine("DIRECT HIT — TARGET DESTROYED!");
                    Console.WriteLine($"MISSION ACCOMPLISHED IN {shots} SHOTS.");
                    destroyed = true;
                }
                else
                {
                    Console.WriteLine("MISS — TARGET STILL ACTIVE.");
                    Console.WriteLine("TRY AGAIN.\n");
                }
            }

            Console.WriteLine("\nEND OF SIMULATION.");
        }

        private static double AskDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (double.TryParse(Console.ReadLine(), out double val))
                    return val;
                Console.WriteLine("PLEASE ENTER A NUMBER.");
            }
        }

        private static double Distance(double x, double y, double z)
            => Math.Sqrt(x * x + y * y + z * z);
    }
}
