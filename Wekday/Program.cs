using System;
using System.Globalization;

namespace Wekday
{
    internal static class Program
    {
        // Fractions of life for each activity (to match the PDP-era listing feel):
        // Sleep 35%, Eat 17%, Work/Study (or Play for young) 23%, Relax 25%
        const double SleepFrac = 0.35;
        const double EatFrac   = 0.17;
        const double WorkFrac  = 0.23; // label changes by age; fraction stays the same
        const double RelaxFrac = 0.25;

        // PDP listing used 365-day year and 30-day month arithmetic for the breakdowns
        const int DaysPerYear  = 365;
        const int DaysPerMonth = 30;

        static void Main()
        {
            Console.WriteLine("THIS PROGRAM DEMONSTRATES PDP-11 BASIC (IN SPIRIT) AND ALSO GIVES");
            Console.WriteLine("FACTS ABOUT A DATE OF INTEREST TO YOU\n");

            // 1) Get TODAY (month,day,year)
            var today = AskDate("ENTER TODAY'S DATE IN THIS FORM: MONTH,DAY, YEAR? ");
            if (today.Year < 1582)
            {
                Console.WriteLine("NOT PREPARED TO GIVE DAY OR WEEK PRIOR TO 1582 (PRE-GREGORIAN).");
                return;
            }

            // Fun easter egg like the listing
            if (today.Day == 13 && today.DayOfWeek == DayOfWeek.Friday)
                Console.WriteLine("FRIDAY THE THIRTEENTH——BEWARE!\n");

            // Show "HEKDAY" style (time + date)
            Console.WriteLine($"HEKDAY: {DateTime.Now:hh:mm tt}");
            Console.WriteLine($"{today:dd-MMM-yy}\n".ToUpperInvariant());

            // 2) Get BIRTH date (or any date of interest)
            var dob = AskDate("ENTER DATE OF BIRTH IN THIS FORM: MO,DAY,YEAR? ");
            if (dob.Year < 1582)
            {
                Console.WriteLine("THE CURRENT CALENDAR DID NOT EXIST BEFORE THAT YEAR.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"{dob:M / d / yyyy} WAS A {dob.DayOfWeek.ToString().ToUpper()}");
            Console.WriteLine();

            // 3) Compute age in years, months, days (PDP-ish arithmetic)
            var ageYMD = AgeBreakdown(today, dob);
            Console.WriteLine("YOUR AGE".PadRight(14) + "YEARS".PadLeft(8) + "MONTHS".PadLeft(10) + "DAYS".PadLeft(8));
            Console.WriteLine("".PadRight(14) +
                              ageYMD.y.ToString().PadLeft(8) +
                              ageYMD.m.ToString().PadLeft(10) +
                              ageYMD.d.ToString().PadLeft(8));

            // Total lived days (approx model to match outputs)
            int totalDays = (ageYMD.y * DaysPerYear) + (ageYMD.m * DaysPerMonth) + ageYMD.d;

            // 4) Breakdowns
            PrintBreakdown("YOU HAVE SLEPT  ", totalDays, SleepFrac);
            PrintBreakdown("YOU HAVE EATEN  ", totalDays, EatFrac);

            string workLabel = ageYMD.y <= 3 ? "YOU HAVE PLAYED          "
                               : ageYMD.y <= 9 ? "YOU HAVE PLAYED/STUDIED "
                               : "YOU HAVE WORKED/STUDIED ";
            PrintBreakdown(workLabel, totalDays, WorkFrac);
            PrintBreakdown("YOU HAVE RELAXED ", totalDays, RelaxFrac);

            // 5) Retirement year (age 65)
            int retireYear = dob.Year + 65;
            Console.WriteLine($"\n**YOU MAY RETIRE IN {retireYear} **\n");

            Console.WriteLine("CALCULATED BY THE BEST MINICOMPUTER TODAY — THE PDP-11 (WINK)");
        }

        // ---- helpers ----

        private static DateTime AskDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim();
                // Accept forms like "6,12,73" or "06,12,1973"
                var parts = s.Split(',');
                if (parts.Length == 3 &&
                    int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int mo) &&
                    int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int da) &&
                    int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int yr))
                {
                    if (yr < 100) yr += (yr >= 50 ? 1900 : 2000); // match old examples
                    try { return new DateTime(yr, mo, da); }
                    catch { /* fallthrough */ }
                }
                Console.WriteLine("PLEASE ENTER AS M,D,Y (e.g., 6,12,1973).");
            }
        }

        private static (int y, int m, int d) AgeBreakdown(DateTime today, DateTime dob)
        {
            if (today < dob) (today, dob) = (dob, today); // swap just in case

            // exact civil difference first, then convert to PDP-ish Y/M/D for print
            int years = today.Year - dob.Year;
            int months = today.Month - dob.Month;
            int days = today.Day - dob.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(today.AddMonths(-1).Year, today.AddMonths(-1).Month);
            }
            if (months < 0)
            {
                years--;
                months += 12;
            }

            // Convert to the 365/30 display style (as in the listing)
            // Take the exact difference and re-express “approximately”.
            int approxDays = (int)Math.Round((today - dob).TotalDays);
            int y = approxDays / DaysPerYear;
            int rem = approxDays % DaysPerYear;
            int m = rem / DaysPerMonth;
            int d = rem % DaysPerMonth;

            // Prefer the “approx” PDP-style output (matches sample)
            return (y, m, d);
        }

        private static void PrintBreakdown(string label, int totalDays, double fraction)
        {
            int days = (int)Math.Round(totalDays * fraction);

            int y = days / DaysPerYear;
            int rem = days % DaysPerYear;
            int m = rem / DaysPerMonth;
            int d = rem % DaysPerMonth;

            Console.WriteLine(label +
                              y.ToString().PadLeft(6) +
                              m.ToString().PadLeft(10) +
                              d.ToString().PadLeft(8));
        }
    }
}
