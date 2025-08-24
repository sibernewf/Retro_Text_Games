using System;
using System.Linq;

class Program
{
    const int MaxSize = 9;

    static void Main()
    {
        Console.WriteLine("*** THE GAME OF CHOMP ***");
        Console.WriteLine("(adapted from Scientific American, Jan. 1973)\n");

        // Show rules?
        if (AskYesNo("Want the rules (Y/N)? "))
        {
            ShowRules();
            Console.WriteLine();
        }

        int players = AskIntInRange("HOW MANY PLAYERS (2–9)? ", 2, 9);

        // We’ll keep a running win streak across games
        int[] streak = Enumerable.Repeat(0, players).ToArray();

        bool keepPlaying = true;
        while (keepPlaying)
        {
            int rows = AskIntInRange($"HOW MANY ROWS (1–{MaxSize})? ", 1, MaxSize);
            int cols = AskIntInRange($"HOW MANY COLUMNS (1–{MaxSize})? ", 1, MaxSize);

            // present[r,c] means the square still exists (r,c are 0-based, poison at 0,0)
            bool[,] present = new bool[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    present[r, c] = true;

            int current = 0; // player index 0..players-1

            Console.WriteLine();
            Console.WriteLine("HERE WE GO...");
            Console.WriteLine();

            while (true)
            {
                PrintBoard(present);
                Console.WriteLine($"PLAYER {current + 1}");

                // Get a valid chomp coordinate (row, col), both 1-based for the player
                int r = AskInt($"COORDINATES OF CHOMP (ROW,COLUMN)? ", allowCommaPair: true, out int c);
                // r and c are 1-based now

                // Validate inside original dimensions
                if (r < 1 || r > rows || c < 1 || c > cols)
                {
                    Console.WriteLine("NO FAIR. THAT'S OUTSIDE THE ORIGINAL DIMENSIONS OF THE COOKIE.");
                    Console.WriteLine();
                    continue; // same player tries again
                }

                // Validate square still present
                if (!present[r - 1, c - 1])
                {
                    Console.WriteLine("NO FAIR. YOU'RE TRYING TO CHOMP ON EMPTY SPACE!");
                    Console.WriteLine();
                    continue; // same player tries again
                }

                // If they chose the poison square, they lose immediately
                if (r == 1 && c == 1)
                {
                    Console.WriteLine();
                    Console.WriteLine("END OF GAME DETECTED!");
                    Console.WriteLine($"YOU LOSE, PLAYER {current + 1}");
                    Console.WriteLine();

                    // Winner is the previous player (wrapping around)
                    int winner = (current - 1 + players) % players;
                    streak[winner] += 1;
                    streak[current] = 0;

                    // Show streak table
                    Console.WriteLine("STREAKS:");
                    for (int p = 0; p < players; p++)
                        Console.WriteLine($"  PLAYER {p + 1}: {streak[p]}");
                    Console.WriteLine();

                    break; // end the game
                }

                // Apply chomp: remove all squares at or below r, and to the right of c
                for (int rr = r - 1; rr < rows; rr++)
                    for (int cc = c - 1; cc < cols; cc++)
                        present[rr, cc] = false;

                // Next player's turn
                current = (current + 1) % players;
                Console.WriteLine();
            }

            keepPlaying = AskYesNo("AGAIN (Y/N)? ");
            Console.WriteLine();
        }

        Console.WriteLine("READY");
    }

    // ---------- Helpers ----------

    static void ShowRules()
    {
        Console.WriteLine("THIS IS THE GAME OF CHOMP.");
        Console.WriteLine("THE BOARD IS A BIG COOKIE — R ROWS HIGH AND C COLUMNS WIDE.");
        Console.WriteLine("YOU INPUT R AND C AT THE START. IN THE UPPER LEFT CORNER OF");
        Console.WriteLine("THE COOKIE IS A POISON SQUARE 'P'. THE ONE WHO CHOMPS THE POISON");
        Console.WriteLine("SQUARE LOSES. TO TAKE A CHOMP, TYPE THE ROW AND COLUMN OF ONE");
        Console.WriteLine("OF THE SQUARES ON THE COOKIE.");
        Console.WriteLine("ALL OF THE SQUARES BELOW AND TO THE RIGHT OF THAT SQUARE,");
        Console.WriteLine("INCLUDING THAT SQUARE, DISAPPEAR (ARE EATEN).");
        Console.WriteLine("NO FAIR CHOMPING SQUARES THAT HAVE ALREADY BEEN CHOMPED,");
        Console.WriteLine("OR THAT ARE OUTSIDE THE ORIGINAL DIMENSIONS OF THE COOKIE.");
        Console.WriteLine("ANY NUMBER OF PEOPLE CAN PLAY — THE COMPUTER IS JUST THE MODERATOR.");
    }

    static void PrintBoard(bool[,] present)
{
    int rows = present.GetLength(0);
    int cols = present.GetLength(1);

    // How many chars the row label takes (1 for 1–9, but compute generically)
    int rowLabelWidth = rows.ToString().Length;

    // Prefix so column numbers align with the first cell (label + space + '|')
    string prefix = new string(' ', rowLabelWidth + 2);

    // Header numbers and top border
    Console.WriteLine(prefix + string.Join(" ", Enumerable.Range(1, cols)));
    Console.WriteLine(prefix + "+" + new string('-', cols * 2 - 1) + "+");

    // Rows
    for (int r = 0; r < rows; r++)
    {
        Console.Write($"{(r + 1).ToString().PadLeft(rowLabelWidth)} |");
        for (int c = 0; c < cols; c++)
        {
            char ch = !present[r, c] ? ' ' : (r == 0 && c == 0 ? 'P' : '*');
            Console.Write(ch);
            if (c < cols - 1) Console.Write(' ');
        }
        Console.WriteLine("|");
    }

    // Bottom border and numbers
    Console.WriteLine(prefix + "+" + new string('-', cols * 2 - 1) + "+");
    Console.WriteLine(prefix + string.Join(" ", Enumerable.Range(1, cols)));
    Console.WriteLine();
}


    static bool AskYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            if (s == "Y" || s == "YES") return true;
            if (s == "N" || s == "NO") return false;
            Console.WriteLine("Please answer Y or N.");
        }
    }

    static int AskIntInRange(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim();
            if (int.TryParse(s, out int v) && v >= min && v <= max)
                return v;
            Console.WriteLine($"Please enter an integer from {min} to {max}.");
        }
    }

    // Returns row; if allowCommaPair==true, also parses a comma/space separated column into 'col'
    static int AskInt(string prompt, bool allowCommaPair, out int col)
    {
        col = 0;
        while (true)
        {
            Console.Write(prompt);
            string s = (Console.ReadLine() ?? "").Trim();

            if (allowCommaPair)
            {
                // Accept formats: "r,c" or "r c"
                var parts = s.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && int.TryParse(parts[0], out int r) && int.TryParse(parts[1], out int c))
                {
                    col = c;
                    return r;
                }
            }

            if (int.TryParse(s, out int onlyRow))
                return onlyRow;

            Console.WriteLine("Please enter row and column like '3,5' or '3 5'.");
        }
    }
}
