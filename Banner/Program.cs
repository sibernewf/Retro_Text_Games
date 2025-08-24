using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Banner
{
    static readonly Dictionary<char, string[]> F = new Dictionary<char, string[]>
    {
        [' '] = new[]{
            "     ","     ","     ","     ","     ","     ","     "},
        ['A'] = new[]{
            "  #  "," # # ","#   #","#####","#   #","#   #","#   #"},
        ['B'] = new[]{
            "#### ","#   #","#   #","#### ","#   #","#   #","#### "},
        ['C'] = new[]{
            " ####","#    ","#    ","#    ","#    ","#    "," ####"},
        ['D'] = new[]{
            "#### ","#   #","#   #","#   #","#   #","#   #","#### "},
        ['E'] = new[]{
            "#####","#    ","#    ","#####","#    ","#    ","#####"},
        ['F'] = new[]{
            "#####","#    ","#    ","#####","#    ","#    ","#    "},
        ['G'] = new[]{
            " ####","#    ","#    ","#  ##","#   #","#   #"," ####"},
        ['H'] = new[]{
            "#   #","#   #","#   #","#####","#   #","#   #","#   #"},
        ['I'] = new[]{
            "#####","  #  ","  #  ","  #  ","  #  ","  #  ","#####"},
        ['J'] = new[]{
            "#####","    #","    #","    #","    #","#   #"," ### "},
        ['K'] = new[]{
            "#   #","#  # ","# #  ","##   ","# #  ","#  # ","#   #"},
        ['L'] = new[]{
            "#    ","#    ","#    ","#    ","#    ","#    ","#####"},
        ['M'] = new[]{
            "#   #","## ##","# # #","#   #","#   #","#   #","#   #"},
        ['N'] = new[]{
            "#   #","##  #","# # #","#  ##","#   #","#   #","#   #"},
        ['O'] = new[]{
            " ### ","#   #","#   #","#   #","#   #","#   #"," ### "},
        ['P'] = new[]{
            "#### ","#   #","#   #","#### ","#    ","#    ","#    "},
        ['Q'] = new[]{
            " ### ","#   #","#   #","#   #","# # #","#  # "," ## #"},
        ['R'] = new[]{
            "#### ","#   #","#   #","#### ","# #  ","#  # ","#   #"},
        ['S'] = new[]{
            " ####","#    ","#    "," ### ","    #","    #","#### "},
        ['T'] = new[]{
            "#####","  #  ","  #  ","  #  ","  #  ","  #  ","  #  "},
        ['U'] = new[]{
            "#   #","#   #","#   #","#   #","#   #","#   #"," ### "},
        ['V'] = new[]{
            "#   #","#   #","#   #","#   #"," # # "," # # ","  #  "},
        ['W'] = new[]{
            "#   #","#   #","#   #","# # #","# # #","## ##","#   #"},
        ['X'] = new[]{
            "#   #"," # # ","  #  ","  #  ","  #  "," # # ","#   #"},
        ['Y'] = new[]{
            "#   #"," # # ","  #  ","  #  ","  #  ","  #  ","  #  "},
        ['Z'] = new[]{
            "#####","    #","   # ","  #  "," #   ","#    ","#####"},
    };

    static void Main()
    {
        Console.Write("INPUT HEIGHT, WIDTH IN INCHES? ");
        var hw = (Console.ReadLine() ?? "").Split(new[]{',',' '}, StringSplitOptions.RemoveEmptyEntries);
        int hIn = hw.Length > 0 && int.TryParse(hw[0], out var H) ? Math.Max(1, H) : 4;
        int wIn = hw.Length > 1 && int.TryParse(hw[1], out var W) ? Math.Max(1, W) : 3;

        Console.Write("HOW FAR IN INCHES FROM THE LEFT HAND SIDE, DO YOU WANT TO PLACE THE LETTERS? ");
        int marginIn = int.TryParse(Console.ReadLine(), out var M) ? Math.Max(0, M) : 0;

        Console.Write("INPUT MESSAGE HERE\n> ");
        string message = (Console.ReadLine() ?? "").ToUpperInvariant();

        int scaleX = Math.Max(1, wIn * 2);
        int scaleY = Math.Max(1, hIn);
        int margin = Math.Max(0, marginIn * 4);

        string output = BuildBanner(message, scaleX, scaleY, margin);

        // Print to console
        Console.WriteLine();
        Console.WriteLine(output);

        // Save to text file
        string filePath = "banner_output.txt";
        File.WriteAllText(filePath, output);
        Console.WriteLine($"\nBanner saved to: {Path.GetFullPath(filePath)}");
    }

    static string BuildBanner(string msg, int sx, int sy, int margin)
    {
        var lines = new List<string>();

        for (int row = 0; row < 7; row++)
        {
            for (int v = 0; v < sy; v++)
            {
                string line = new string(' ', margin);

                foreach (char ch in msg)
                {
                    var glyph = F.ContainsKey(ch) ? F[ch] : F[' '];
                    string pattern = glyph[row];

                    foreach (char c in pattern)
                    {
                        char ink = (c == '#') ? (ch == ' ' ? ' ' : ch) : ' ';
                        line += new string(ink, sx);
                    }

                    line += new string(' ', sx);
                }
                lines.Add(line);
            }
        }

        return string.Join(Environment.NewLine, lines);
    }
}
