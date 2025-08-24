using System;
using System.IO;
using System.Text;

class Program
{
    // Canvas size (characters). You can tweak these.
    const int DefaultWidth = 110;   // columns
    const int DefaultHeight = 60;   // rows

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("BUNNY — ASCII silhouette filled with B U N N Y");
        Console.WriteLine("Press Enter to accept defaults or type custom sizes.");
        int width = AskInt($"Width (chars) [{DefaultWidth}]: ", DefaultWidth, 40, 200);
        int height = AskInt($"Height (rows) [{DefaultHeight}]: ", DefaultHeight, 20, 120);
        Console.WriteLine();

        string art = MakeBunny(width, height);

        Console.WriteLine(art);

        var path = Path.GetFullPath("bunny.txt");
        File.WriteAllText(path, art, Encoding.UTF8);
        Console.WriteLine($"\nSaved to: {path}");
    }

    static int AskInt(string prompt, int def, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(s)) return def;
            if (int.TryParse(s, out int v) && v >= min && v <= max) return v;
        }
    }

    static string MakeBunny(int width, int height)
    {
        var sb = new StringBuilder(height * (width + 2));
        string letters = "BUNNY";
        int li = 0;

        // Normalize coordinates to [0,1] across the canvas; sample at cell centers.
        for (int y = 0; y < height; y++)
        {
            double ny = (y + 0.5) / height;
            for (int x = 0; x < width; x++)
            {
                double nx = (x + 0.5) / width;

                bool inside = InBunny(nx, ny);

                if (inside)
                {
                    sb.Append(letters[li]);
                    li = (li + 1) % letters.Length;
                }
                else
                {
                    sb.Append(' ');
                }
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    // --------- Bunny silhouette ---------
    // Constructed from overlapping/rotated ellipses:
    //   - two tall, narrow ears (slightly tilted)
    //   - head ellipse
    //   - body ellipse
    // Then subtract a few cutouts for ear notches/neck and an eye.
    static bool InBunny(double x, double y)
    {
        // Main pieces (union)
        bool inEarL = InRotEllipse(x, y, cx: 0.40, cy: 0.22, rx: 0.07, ry: 0.23, deg: -18);
        bool inEarR = InRotEllipse(x, y, cx: 0.60, cy: 0.22, rx: 0.07, ry: 0.23, deg: +18);
        bool inHead = InEllipse(x, y, cx: 0.50, cy: 0.42, rx: 0.18, ry: 0.14);
        bool inBody = InEllipse(x, y, cx: 0.50, cy: 0.66, rx: 0.27, ry: 0.22);

        bool union = inEarL || inEarR || inHead || inBody;

        if (!union) return false;

        // Subtractions to refine shape
        bool cutEarL = InRotEllipse(x, y, cx: 0.43, cy: 0.15, rx: 0.03, ry: 0.09, deg: -18);
        bool cutEarR = InRotEllipse(x, y, cx: 0.57, cy: 0.15, rx: 0.03, ry: 0.09, deg: +18);
        bool cutNeck = InEllipse(x, y, cx: 0.50, cy: 0.52, rx: 0.12, ry: 0.05);
        bool cutWaist = InEllipse(x, y, cx: 0.33, cy: 0.64, rx: 0.10, ry: 0.09) ||
                        InEllipse(x, y, cx: 0.67, cy: 0.64, rx: 0.10, ry: 0.09);

        // A small eye cutout
        bool cutEye = InEllipse(x, y, cx: 0.57, cy: 0.41, rx: 0.02, ry: 0.015);

        if (cutEarL || cutEarR || cutNeck || cutWaist || cutEye)
            return false;

        return true;
    }

    // Axis-aligned ellipse: ((x-cx)/rx)^2 + ((y-cy)/ry)^2 <= 1
    static bool InEllipse(double x, double y, double cx, double cy, double rx, double ry)
    {
        double dx = (x - cx) / rx;
        double dy = (y - cy) / ry;
        return dx * dx + dy * dy <= 1.0;
    }

    // Rotated ellipse by angle (degrees)
    static bool InRotEllipse(double x, double y, double cx, double cy, double rx, double ry, double deg)
    {
        double a = deg * Math.PI / 180.0;
        double cos = Math.Cos(a), sin = Math.Sin(a);
        double dx = x - cx, dy = y - cy;
        // rotate point into ellipse's local axes
        double u =  dx * cos + dy * sin;
        double v = -dx * sin + dy * cos;
        double du = u / rx, dv = v / ry;
        return du * du + dv * dv <= 1.0;
    }
}
