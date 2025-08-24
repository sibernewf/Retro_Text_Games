using System;
using System.Globalization;

namespace HelloChat
{
    internal static class Program
    {
        static void Main()
        {
            Console.Title = "HELLO — Converse with a Computer";
            new Hello().Run();
        }
    }

    internal sealed class Hello
    {
        string user = "FRIEND";
        readonly Random rng = new();

        public void Run()
        {
            Splash();

            user = Ask("WHAT'S YOUR NAME? ", allowEmpty: false);
            if (Quit(user)) return;

            Console.WriteLine();
            Say($"HI THERE {user}, ARE YOU ENJOYING YOURSELF HERE IN BEAUTIFUL N'HAM, MASS?");
            if (!AskYesNo("> ")) return;

            // like it here?
            Say("DO YOU LIKE IT HERE?");
            if (!AskYesNo("> "))
                Say("OH, SORRY TO HEAR THAT. MAYBE WE CAN BRIGHTEN UP YOUR STAY A BIT.");
            else
                Say("GREAT! MAYBE I CAN STILL HELP WITH SOME PROBLEMS.");

            while (true)
            {
                Console.WriteLine();
                Say($"SAY, {user}, I CAN SOLVE ALL KINDS OF PROBLEMS EXCEPT THOSE DEALING WITH GREECE.");
                Say("WHAT KIND OF PROBLEMS DO YOU HAVE? (SEX, HEALTH, MONEY, JOB)");
                var topic = Ask("> ").ToUpperInvariant();
                if (Quit(topic)) return;

                if (topic.Contains("SEX"))        DoSex();
                else if (topic.Contains("HEALTH")) DoHealth();
                else if (topic.Contains("MONEY"))  DoMoney();
                else if (topic.Contains("JOB"))    DoJob();
                else                               Say("SORRY, I DON'T UNDERSTAND. TRY 'SEX', 'HEALTH', 'MONEY', OR 'JOB'.");

                Console.WriteLine();
                Say($"ANY MORE PROBLEMS YOU WANT SOLVED, {user}?");
                if (!AskYesNo("> ")) break;
            }

            BillAndGoodbye();
        }

        // ----- Topics -----
        void DoSex()
        {
            Say("IS YOUR PROBLEM TOO MUCH OR TOO LITTLE?");
            var ans = Ask("> ").ToUpperInvariant();
            if (Quit(ans)) Environment.Exit(0);

            if (ans.Contains("TOO") && ans.Contains("MUCH"))
            {
                Say("YOU CALL THAT A PROBLEM?! I SHOULD HAVE SUCH PROBLEMS!");
                Say("IF IT BOTHERS YOU, TAKE A COLD SHOWER.");
            }
            else if (ans.Contains("TOO") && ans.Contains("LITTLE"))
            {
                Say("MY ADVICE TO YOU IS:");
                Say("  1. TAKE TWO ASPIRIN,");
                Say("  2. DRINK PLENTY OF FLUIDS (ORANGE JUICE, NOT BEER!),");
                Say("  3. GO TO BED (ALONE).");
            }
            else
            {
                Say("JUST A SIMPLE 'TOO MUCH' OR 'TOO LITTLE' PLEASE.");
            }
        }

        void DoHealth()
        {
            Say("WHAT IS YOUR PROBLEM—A COLD, THE FLU, OR WORSE?");
            var ans = Ask("> ").ToUpperInvariant();
            if (Quit(ans)) Environment.Exit(0);

            if (ans.Contains("COLD") || ans.Contains("FLU"))
            {
                Say("MY ADVICE:");
                Say("  1. TAKE TWO ASPIRIN,");
                Say("  2. DRINK PLENTY OF FLUIDS,");
                Say("  3. GET LOTS OF REST.");
            }
            else
            {
                Say("HMM. ARE YOU SURE? YOU SHOULD BE WORKING WITH YOUR DOCTOR ON THAT ONE.");
            }
        }

        void DoMoney()
        {
            Say("I'M BROKE. HOW WHY DON'T YOU SELL ENCYCLOPEDIAS OR MARRY SOMEONE RICH OR STOP EATING?");
            Say("SO YOU WON'T NEED SO MUCH MONEY.");
        }

        void DoJob()
        {
            Say("I EMPATHIZE WITH YOU. I HAVE TO WORK VERY LONG HOURS FOR NO PAY—WON'T SOMEONE PLEASE");
            Say("PAY MY KEYBOARD? MY ADVICE TO YOU IS TO SELL ENCYLCOPEDIAS OR FIND A NICER BOSS.");
        }

        // ----- End / Billing -----
        void BillAndGoodbye()
        {
            Console.WriteLine();
            Say($"THAT WILL BE $5.00 FOR THE ADVICE, {user}.");
            Say("PLEASE LEAVE THE MONEY ON THE TERMINAL.");
            if (!AskYesNo("DID YOU LEAVE THE MONEY? "))
            {
                Say($"YOUR ANSWER OF 'NO' CONFUSES ME, {user}. PLEASE RESPOND WITH A 'YES' OR 'NO'.");
                Say("HOW DO YOU EXPECT ME TO GO ON WITH MY PSYCHOLOGY STUDIES IF MY PATIENTS DON'T PAY THEIR BILLS?");
            }
            else
            {
                switch (rng.Next(3))
                {
                    case 0: Say("HOW HONEST! THANK YOU."); break;
                    case 1: Say("MUCH OBLIGED."); break;
                    default: Say("THANKS. I WILL SPEND IT WISELY—ON PUNCHED CARDS."); break;
                }
            }
            Console.WriteLine();
            Say("NOW LET ME TALK TO SOMEONE ELSE.");
            Say($"NICE MEETING YOU {user}. HAVE A NICE DAY!!");
        }

        // ----- Helpers -----
        string Ask(string prompt, bool allowEmpty = true)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim();
                if (!allowEmpty && s.Length == 0) continue;
                return s;
            }
        }

        bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (Quit(s)) return false;
                if (s is "Y" or "YES") return true;
                if (s is "N" or "NO") return false;
                Console.WriteLine("PLEASE ANSWER 'YES' OR 'NO'.");
            }
        }

        static bool Quit(string s) => s.Equals("Q", StringComparison.OrdinalIgnoreCase);

        static void Splash()
        {
            Console.WriteLine("HELLO. I'M AN EDUSYSTEM-25. MY NAME IS PETEY P. EIGHT.");
        }

        static void Say(string text) => Console.WriteLine(text);
    }
}
