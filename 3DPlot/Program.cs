using System;

namespace Plot3D
{
    internal static class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Pick which function to use by uncommenting:
            Func<double, double> F = z => 30 * Math.Exp(-z * z / 100.0);    // Gaussian bell
            //Func<double, double> F = z => Math.Sqrt(900.01 - z * z) * 0.9 - 2; 
            //Func<double, double> F = z => 30 * Math.Pow(Math.Cos(z / 16.0), 2);
            //Func<double, double> F = z => 30 - 30 * Math.Sin(z / 18.0);
            //Func<double, double> F = z => 30 * Math.Exp(-Math.Cos(z / 16.0)) - 30; // Bessel-like
            //Func<double, double> F = z => 30 * Math.Sin(z / 10.0);

            for (double x = -30; x <= 30; x += 1.5)
            {
                for (double y = -30; y <= 30; y += 1.5)
                {
                    // Inside circle of radius 30
                    if (x * x + y * y > 900) continue;

                    // Compute projection
                    double z = (int)(F(Math.Sqrt(x * x + y * y)) / 5.0);
                    int col = (int)(42 + (x - y / 2));
                    int row = (int)(z + y / 2 + 20);

                    // Bounds check
                    if (row >= 0 && row < Console.WindowHeight &&
                        col >= 0 && col < Console.WindowWidth)
                    {
                        Console.SetCursorPosition(col, row);
                        Console.Write(".");
                    }
                }
            }

            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.WriteLine("\n3DPLOT COMPLETE. Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}
