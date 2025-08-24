using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Diamnd
{
    internal static class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("FOR A PRETTY DIAMOND PATTERN.");
            Console.Write("TYPE IN AN ODD NUMBER BETWEEN 5 AND 31: ");

            int n = ReadOddBetween(5, 31);

            // Build one diamond shape
            var tile = BuildDiamond(n, outlineChar: 'D');

            const int targetWidth = 120;
            const int targetLines = 66;

            int hGap = Math.Max(1, n <= 9 ? 2 : 1);
            int tileWidth = n + hGap;
            int across = Math.Max(1, targetWidth / tileWidth);

            int vGap = 0;
            int tileHeight = n + vGap;
            int rows = Math.Max(1, targetLines / tileHeight);

            StringBuilder output = new StringBuilder();
            output.AppendLine();

            // Build and output to both console & StringBuilder
            for (int r = 0; r < rows; r++)
            {
                for (int line = 0; line < n; line++)
                {
                    var sb = new StringBuilder(targetWidth + 8);
                    for (int c = 0; c < across; c++)
                    {
                        sb.Append(tile[line]);
                        if (c != across - 1) sb.Append(' ', hGap);
                    }

                    string lineStr = sb.ToString().TrimEnd();
                    Console.WriteLine(lineStr);
                    output.AppendLine(lineStr);
                }
                for (int g = 0; g < vGap; g++)
                {
                    Console.WriteLine();
                    output.AppendLine();
                }
            }

            Console.WriteLine();
            Console.WriteLine("READY");
            output.AppendLine();
            output.AppendLine("READY");

            string filePath = "diamond_output.txt";
            File.WriteAllText(filePath, output.ToString());

            Console.WriteLine($"\nDiamond pattern also saved to {filePath}");
        }

        private static int ReadOddBetween(int min, int max)
        {
            while (true)
            {
                string? s = Console.ReadLine();
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n)
                    && n >= min && n <= max && (n % 2 == 1))
                {
                    return n;
                }
                Console.Write($"Please enter an ODD number between {min} and {max}: ");
            }
        }

        private static string[] BuildDiamond(int n, char outlineChar = 'D')
        {
            int c = (n - 1) / 2;
            var lines = new string[n];

            for (int i = 0; i < n; i++)
            {
                var row = new char[n];
                for (int j = 0; j < n; j++)
                {
                    bool onOutline = Math.Abs(i - c) + Math.Abs(j - c) == c;
                    row[j] = onOutline ? outlineChar : ' ';
                }
                lines[i] = new string(row);
            }
            return lines;
        }
    }
}
