using System;
using System.Collections.Generic;

namespace Zoop
{
    internal static class Program
    {
        // First-two-letter -> response (roughly from the magazine listing)
        private static readonly Dictionary<string, string> Replies =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // “System” commands it loves to mangle
                ["CA"] = "TRY MONTGOMERY WARD'S",                // CAT / CATALOG
                ["SC"] = "GOT AN ITCH?",                         // SCRATCH
                ["PR"] = "THIS IS NO NEWSPAPER",                 // PRINT
                ["RU"] = "I DON'T FEEL LIKE IT",                 // RUN
                ["NE"] = "YES I'M QUITE NEW",                    // NEW
                ["OL"] = "I'M NOT THAT OLD, BUT I'M OLD ENOUGH!!!", // OLD
                ["NA"] = "WHY? I LIKE MY NAME",                  // NAME
                ["BU"] = "GET A CAN OF RAID",                    // BUG
                ["ST"] = "THE FUN IS JUST STARTING",             // STOP
                ["SA"] = "SAVE O.K., THE WHOLE DEC TAPE",        // SAVE
                ["DE"] = "I DON'T LIKE BANKS",                   // (DE)POSIT / DEVICE
                ["OP"] = "NO FILE, YOU BOOB",                    // OPEN / OLD — cheeky!
            };

        static void Main()
        {
            Console.WriteLine("READY");
            Console.WriteLine("(Type commands like CAT, SCRATCH, PRINT… only the first two letters matter.)");
            Console.WriteLine("(Type BYE or press Enter to quit.)\n");

            while (true)
            {
                Console.Write("> ");
                var line = (Console.ReadLine() ?? "").Trim();

                if (string.IsNullOrEmpty(line) || line.Equals("BYE", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("WIPED OUT COMPLETELY!!!");
                    break;
                }

                var key = line.Length >= 2 ? line.Substring(0, 2).ToUpperInvariant()
                                           : line.ToUpperInvariant();

                if (Replies.TryGetValue(key, out var reply))
                {
                    Console.WriteLine(reply);
                }
                else
                {
                    Console.WriteLine("WHAT??");
                }
            }
        }
    }
}
