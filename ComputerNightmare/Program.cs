using System;
using System.Diagnostics;
using System.Threading;

namespace ComputerNightmare
{
    class Program
    {
        // Tune this to make it easier/harder (BASIC used a tight FOR/NEXT delay).
        // 500 ms feels close to the original “blink and you miss it”.
        private const int ReactionWindowMs = 500;

        static readonly string[] Taunts =
        {
            "** MICROS RULE! **",
            "** PEOPLE ARE STUPID **",
            "** A ROBOT FOR PRESIDENT! **",
            "** COMPUTERS ARE GREAT! **",
            "** I'M BETTER THAN YOU! **"
        };

        static void Main()
        {
            Console.Title = "Computer Nightmare";
            Console.Clear();

            int score = 300;
            var rng = new Random();

            while (true)
            {
                // 90 LET N=INT(RND*9)+1
                int n = rng.Next(1, 10);

                // crude layout similar to PRINT TAB(5);N : TAB(15);S
                Console.WriteLine();
                Console.Write("     " + n);
                Console.WriteLine("          " + score);

                // 120 IF RND>0.5 THEN GOTO 150
                if (rng.NextDouble() <= 0.5)
                {
                    // 130 PRINT : 140 PRINT C$(INT(S/100)+1)
                    Console.WriteLine();
                    int idx = Math.Clamp(score / 100, 0, Taunts.Length - 1);
                    Console.WriteLine(Taunts[idx]);
                }

                // 150 IF S<60 THEN PRINT "<THERE'S NO HOPE>"
                if (score < 60)
                    Console.WriteLine("<THERE'S NO HOPE>");

                // 160 IF S>440 THEN PRINT "URK! HELP!!"
                if (score > 440)
                    Console.WriteLine("URK! HELP!!");

                // 170–200: small timed loop that samples a key if one is pressed
                string f = ""; // F$
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < ReactionWindowMs)
                {
                    if (Console.KeyAvailable)
                    {
                        var k = Console.ReadKey(intercept: true);
                        f = k.KeyChar.ToString();
                        // emulate BASIC behaviour: remember the last thing you pressed during the window
                    }
                    Thread.SpinWait(10_000); // tiny busy-wait to keep it snappy
                }

                // 210 LET S=S-10
                score -= 10;

                // 220 IF VAL(F$)<>N THEN GOTO 240
                int parsed = 0;
                bool ok = f.Length == 1 && char.IsDigit(f[0]) && int.TryParse(f, out parsed) && parsed == n;

                if (ok)
                {
                    // 230 LET S=S+10+N*2
                    score += 10 + n * 2;
                }

                // 240 IF S<0 THEN GOTO 270
                if (score < 0)
                {
                    // 270 PRINT "YOU'RE NOW MY SLAVE"
                    Console.WriteLine();
                    Console.WriteLine("YOU'RE NOW MY SLAVE");
                    break;
                }

                // 250 IF S>500 THEN GOTO 290
                if (score > 500)
                {
                    // 290 PRINT "OK. YOU WIN (THIS TIME)"
                    Console.WriteLine();
                    Console.WriteLine("OK. YOU WIN (THIS TIME)");
                    break;
                }

                // 260 GOTO 80  (loop again)
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}
