using System;
using System.Globalization;

class Program
{
    static void Main()
    {
        Console.WriteLine("I AM CHIEF NUMBERS FREEK, THE GREAT INDIAN MATH GOD.");
        Console.Write("ARE YOU READY TO TAKE THE TEST YOU CALLED ME OUT FOR? ");
        var ans = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

        if (ans != "YES")
        {
            Console.WriteLine("SHUT UP PALEFACE WITH WISE TONGUE.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("TAKE A NUMBER AND ADD 3.  DIVIDE THIS NUMBER BY 5 AND");
        Console.WriteLine("MULTIPLY BY 8.  DIVIDE BY 5 AND ADD THE SAME.  SUBTRACT 1.");
        Console.WriteLine("WHAT DO YOU HAVE?");

        if (!TryReadDouble(out double finalResult))
        {
            Console.WriteLine("I NEED A NUMBER. BE GONE!");
            return;
        }

        // ----------------------------------------------------------
        // We model the steps exactly as stated (our interpretation):
        // Let N be the original number.
        // S1 = N + 3
        // S2 = S1 / 5
        // S3 = S2 * 8
        // S4 = (S3 / 5) + (S3 / 5)   <-- “divide by 5 and add the same”
        // S5 = S4 - 1   (this is the number the player reports)
        //
        // So: S5 = ( ( (N+3)/5 )*8 /5 )*2 - 1
        //     S5 = (16/25)*(N+3) - 1  =  (16/25)N + (48/25) - 1
        //     S5 = 0.64*N + 0.92
        //
        // Therefore the Chief’s “mind reading” is:
        //     N = (S5 - 0.92) / 0.64
        // ----------------------------------------------------------
        double guessedN = (finalResult - 0.92) / 0.64;

        Console.WriteLine();
        Console.Write($"I BET YOUR NUMBER WAS {guessedN:G}  WAS I RIGHT? ");
        var yesNo = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

        if (yesNo == "YES")
        {
            Console.WriteLine("I KNEW IT! THE SPIRITS OF ARITHMETIC ARE PLEASED.");
            return;
        }

        // Show the working from the user's stated original number
        Console.Write("WHAT WAS YOUR ORIGINAL NUMBER? ");
        if (!TryReadDouble(out double original))
        {
            Console.WriteLine("WORDS! I ASKED FOR A NUMBER. BEGONE!");
            return;
        }

        Console.WriteLine();
        // Step-by-step demonstration
        double s1 = original + 3;
        double s2 = s1 / 5.0;
        double s3 = s2 * 8.0;
        double s4 = (s3 / 5.0) + (s3 / 5.0); // add “the same” quantity
        double s5 = s4 - 1.0;

        Console.WriteLine($"{original:G} PLUS 3 EQUALS {s1:G}");
        Console.WriteLine($"THIS DIVIDED BY 5 EQUALS {s2:G}");
        Console.WriteLine($"THIS TIMES 8 EQUALS {s3:G}");
        Console.WriteLine($"DIVIDE BY 5 AND ADD THE SAME → {s4:G}");
        Console.WriteLine($"MINUS 1 EQUALS {s5:G}");
        Console.WriteLine();

        Console.Write("NOW DO YOU BELIEVE ME? ");
        var believe = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
        if (believe == "YES")
        {
            Console.WriteLine("GOOD. RESPECT THE POWER OF NUMBERS.");
            return;
        }

        // Lightning bolt (ASCII, like the old listings)
        Console.WriteLine();
        Console.WriteLine("YOU HAVE MADE ME MAD!!!");
        Console.WriteLine("THERE MUST NOW BE A GREAT LIGHTNING BOLT!");
        DrawLightning();
        Console.WriteLine();
        Console.WriteLine("I HOPE YOU BELIEVE ME NOW, FOR YOUR SAKE!!");
        Console.WriteLine("BYE!");
    }

    static void DrawLightning()
    {
        string[] bolt =
        {
            "                X X",
            "               X   X",
            "              X X X",
            "             X   X",
            "            X X X",
            "           X   X",
            "          X X X",
            "         X   X",
            "        X X X",
            "       X   X",
            "      X X X",
            "     X   X",
            "    X X X",
            "   X   X",
            "  X X X",
            " X   X",
            "X X X",
            "  X",
            "  #############################",
        };
        foreach (var line in bolt) Console.WriteLine(line);
    }

    static bool TryReadDouble(out double value)
    {
        string? s = Console.ReadLine();
        return double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
            || double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}
